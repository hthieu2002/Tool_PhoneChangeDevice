Param(
    [Parameter(Mandatory=$true)]
    [string]$DeviceId,

    [Parameter(Mandatory=$false)]
    [string]$AdbPath = "adb",

    # Thresholds
    [int]$SdcardPassMB = 20,        # /sdcard < 20MB => very clean
    [int]$SdcardWarnMB = 200,       # /sdcard >= 200MB => Red

    [int]$SysAppPerPkgWarnMB = 20,  # System app data > 20MB => warn
    [int]$UserDataPerPkgRedMB = 50, # User/orphan >= 50MB => Red

    [int]$MaxUserPkgsWarn = 5,      # >5 user/orphan => Red
    [int]$MaxOrphanWarn   = 3,      # >3 orphan => Red

    # Output mode (default: compact)
    [object]$CompactOutput = $true,

    # Treat "signed-in accounts" as not pristine (affects verdict)
    [object]$TreatSignedInAsWarn = $true,

    # Luôn liệt kê benign orphan (trichrome)
    [object]$AlwaysListBenign = $true,

    # === NEW: Deep check controls ===
    [object]$DeepDirScan = $true,       # Bật kiểm tra sâu thư mục
    [int]$MaxFileSamples = 5            # Số file mẫu hiển thị mỗi thư mục
)

# -------- Normalize booleans (fix khi gọi qua -File) --------
function To-Bool($v) {
    if ($v -is [bool]) { return $v }
    if ($null -eq $v) { return $false }
    $s = "$v".Trim().ToLower()
    switch -regex ($s) {
        '^(true|1|\$true)$'  { return $true }
        '^(false|0|\$false)$' { return $false }
        default { return [System.Convert]::ToBoolean($v) }
    }
}
$CompactOutput      = To-Bool $CompactOutput
$TreatSignedInAsWarn= To-Bool $TreatSignedInAsWarn
$AlwaysListBenign   = To-Bool $AlwaysListBenign
$DeepDirScan        = To-Bool $DeepDirScan
# ------------------------------------------------------------

function ADB([string]$ArgLine) {
    $cmdLine = if ($AdbPath -match '\s') { "`"$AdbPath`" $ArgLine" } else { "$AdbPath $ArgLine" }
    $out = & cmd.exe /c $cmdLine 2>&1
    if ($null -eq $out) { return "" }
    return ($out | Out-String).Trim()
}
function Shell([string]$Cmd) {
    $escaped = $Cmd -replace '"','\"'
    return ADB "-s $DeviceId shell `"$escaped`""
}
function Parse-DuList([string]$duText, [string]$prefixFilter = "") {
    $items = @()
    if (-not $duText) { return $items }
    foreach ($ln in ($duText -split "`r?`n")) {
        if ($ln -match '^\s*(\d+)\s+(.+)\s*$') {
            $mb = [int]$matches[1]
            $path = $matches[2]
            if ($prefixFilter -and $path -notlike "$prefixFilter*") { continue }
            $name = Split-Path $path -Leaf
            $items += [pscustomobject]@{ SizeMB = $mb; Path = $path; Name = $name }
        }
    }
    return $items
}
function Write-Section($title) {
    if (-not $CompactOutput) {
        Write-Host ""
        Write-Host ("=== {0} ===" -f $title) -ForegroundColor Cyan
    }
}

# Whitelist orphan "benign"
$BenignOrphanPatterns = @('com.google.android.trichromelibrary_*')
function Is-BenignOrphan([string]$pkgName) {
    foreach ($pat in $BenignOrphanPatterns) { if ($pkgName -like $pat) { return $true } }
    return $false
}

# === NEW: Deep dir inspector ===
function Get-DirFileInfo([string]$dir, [int]$maxSamples) {
    # Đếm file thật sự (type f). Nếu không có file => coi như cây rỗng.
    # Dùng sh + find + wc -l để đảm bảo portable trên toybox.
    $countStr = Shell "sh -c 'find \"${dir}\" -type f 2>/dev/null | wc -l'"
    $fileCount = 0
    if ($countStr) {
        $countStr = $countStr.Trim()
        try { $fileCount = [int]$countStr } catch { $fileCount = 0 }
    }
    $samples = @()
    if ($fileCount -gt 0 -and $maxSamples -gt 0) {
        $sampleOut = Shell "sh -c 'find \"${dir}\" -type f 2>/dev/null | head -n $maxSamples'"
        if ($sampleOut) {
            foreach ($ln in ($sampleOut -split "`r?`n")) {
                if ($ln.Trim()) { $samples += $ln.Trim() }
            }
        }
    }
    return [pscustomobject]@{
        FileCount  = $fileCount
        SampleFiles= $samples
    }
}

# PASS/FAIL per base path (có deep-check)
function Summarize-PathPassFail([string]$basePath, [int]$perItemRedMB, [string]$title) {
    Write-Section $title
    $duRaw = Shell "toybox du -sm $basePath/* 2>/dev/null"
    $items = Parse-DuList $duRaw $basePath
    if (-not $items -or $items.Count -eq 0) {
        Write-Host ("[OK] {0}: empty or not present." -f $basePath) -ForegroundColor Green
        return @{ totalMB = 0; totalCount = 0; fail = @(); passCount = 0; allPass = $true; emptyDirs=@(); nonEmpty=@() }
    }

    # Deep check: loại những mục CHỈ có cây thư mục rỗng
    $emptyDirs = @()
    $nonEmptyItems = @()
    if ($DeepDirScan) {
        foreach ($it in $items) {
            $info = Get-DirFileInfo $it.Path $MaxFileSamples
            if ($info.FileCount -eq 0) {
                $emptyDirs += $it
            } else {
                # đính kèm info vào object để dùng khi in
                $obj = [pscustomobject]@{
                    SizeMB = $it.SizeMB
                    Path   = $it.Path
                    Name   = $it.Name
                    FileCount = $info.FileCount
                    SampleFiles = $info.SampleFiles
                }
                $nonEmptyItems += $obj
            }
        }
    } else {
        $nonEmptyItems = $items
    }

    if ($emptyDirs.Count -gt 0) {
        Write-Host ("[OK] {0}: {1} entries are EMPTY (directory trees only, no files)." -f $basePath, $emptyDirs.Count) -ForegroundColor Green
        if (-not $CompactOutput) {
            $emptyDirs | ForEach-Object { Write-Host (" - (empty) {0}" -f $_.Name) }
        }
    }

    $totalMB = ($items | Measure-Object SizeMB -Sum).Sum
    $maxMB   = ($items | Measure-Object SizeMB -Maximum).Maximum
    $fail    = @($nonEmptyItems | Where-Object { $_.SizeMB -ge $perItemRedMB })
    $passCnt = $nonEmptyItems.Count - $fail.Count

    if ($nonEmptyItems.Count -eq 0) {
        Write-Host ("[OK] {0} - PASS (all {1} entries are empty dirs)" -f $basePath, $items.Count) -ForegroundColor Green
        return @{ totalMB = 0; totalCount = $items.Count; fail = @(); passCount = $items.Count; allPass = $true; emptyDirs=$emptyDirs; nonEmpty=@() }
    }

    if ($fail.Count -eq 0) {
        Write-Host ("[OK] {0} - PASS (non-empty items={1}, total={2} MB, max={3} MB < {4} MB)" -f $basePath, $nonEmptyItems.Count, $totalMB, $maxMB, $perItemRedMB) -ForegroundColor Green
    } else {
        Write-Host ("[WARN] {0} - NOT PASS (failing {1}/{2} non-empty, threshold per item >={3} MB)" -f $basePath, $fail.Count, $nonEmptyItems.Count, $perItemRedMB) -ForegroundColor Yellow
        $fail | Sort-Object SizeMB -Descending | ForEach-Object {
            Write-Host (" - {0,-6} MB  {1} (files={2})" -f $_.SizeMB, $_.Name, $_.FileCount)
            if (-not $CompactOutput -and $_.SampleFiles -and $_.SampleFiles.Count -gt 0) {
                foreach ($f in $_.SampleFiles) { Write-Host ("     · {0}" -f $f) }
            }
        }
    }

    return @{
        totalMB    = $totalMB
        totalCount = $items.Count
        fail       = $fail
        passCount  = $passCnt
        allPass    = ($fail.Count -eq 0)
        emptyDirs  = $emptyDirs
        nonEmpty   = $nonEmptyItems
    }
}

Write-Host ("=== Clean Check (deep) for device {0} ===" -f $DeviceId) -ForegroundColor Cyan
[void](ADB 'start-server')

# Devices ready?
$devicesOut = ADB 'devices'
if ($devicesOut -notmatch 'List of devices attached') { Write-Host "[ERR] 'adb devices' did not return expected header." -ForegroundColor Red; Write-Host $devicesOut; exit 1 }
$found = $false
foreach ($line in ($devicesOut -split "`r?`n")) { if ($line -match "^\s*$([regex]::Escape($DeviceId))\s+device\s*$") { $found = $true; break } }
if (-not $found) { Write-Host "[ERR] Device not found in 'device' state." -ForegroundColor Red; Write-Host $devicesOut; exit 1 }

# 1) Basic Info
$model = (Shell 'getprop ro.product.model').Trim(); if (-not $model) { $model = "(unknown)" }
Write-Host ("[INFO] Model: {0}" -f $model)

$rawAll  = Shell 'pm list packages'
$rawSys  = Shell 'pm list packages -s'
$rawUser = Shell 'pm list packages -3'
$allPkgs  = if ($rawAll)  { $rawAll  -split "`r?`n" | ForEach-Object { $_ -replace '^package:','' } | Where-Object { $_ } } else { @() }
$sysPkgs  = if ($rawSys)  { $rawSys  -split "`r?`n" | ForEach-Object { $_ -replace '^package:','' } | Where-Object { $_ } } else { @() }
$userPkgs = if ($rawUser) { $rawUser -split "`r?`n" | ForEach-Object { $_ -replace '^package:','' } | Where-Object { $_ } } else { @() }
Write-Host ("[INFO] System packages: {0} | User packages: {1}" -f $sysPkgs.Count, $userPkgs.Count)
if ($userPkgs.Count -eq 0) { Write-Host "[OK] No user apps." -ForegroundColor Green } else { Write-Host ("[WARN] Found {0} user apps:" -f $userPkgs.Count) -ForegroundColor Yellow; if (-not $CompactOutput) { $userPkgs | ForEach-Object { Write-Host (" - {0}" -f $_) } } }

# 1.1) Accounts
$accDump = Shell 'dumpsys account'
$accCount = 0; $googleAcc = $false
foreach ($ln in ($accDump -split "`r?`n")) {
    if ($ln -match 'Account\s*\{' -or $ln -match '\bname=' -or $ln -match '\btype=') { $accCount++ }
    if ($ln -match 'type=com\.google') { $googleAcc = $true }
}
$hasAccounts = ($accCount -gt 0)
if ($hasAccounts) {
    $accSuffix = ""
    if ($googleAcc) { $accSuffix = " (includes Google)" }
    Write-Host ("[INFO] Accounts present: {0}{1}" -f $accCount, $accSuffix)
} else { Write-Host "[OK] No accounts found." }

# 2) Security (su/Magisk)
$adbDir   = Shell 'ls -la /data/adb 2>/dev/null'
$suPaths  = Shell 'ls -la /system/xbin/su /system/bin/su 2>/dev/null'
$magiskGp = Shell 'getprop | grep -i magisk'
$hasAdbDir = ($adbDir -and $adbDir -notmatch 'No such file'); $adbEmpty = $true
if ($hasAdbDir) { $adbGlob = Shell 'ls -la /data/adb/* 2>/dev/null'; if ($adbGlob -and $adbGlob -notmatch 'No such file') { $adbEmpty = $false } }
$hasSU = ($suPaths -and $suPaths -notmatch 'No such file'); $hasMagiskProp = ($magiskGp -and $magiskGp.Trim().Length -gt 0)
if (-not $hasSU -and -not $hasMagiskProp -and (-not $hasAdbDir -or $adbEmpty)) {
    $adbNote = "absent"; if ($hasAdbDir) { if ($adbEmpty) { $adbNote = "empty" } else { $adbNote = "has content" } }
    Write-Host ("[OK] No Magisk/SU ( /data/adb: {0} )." -f $adbNote) -ForegroundColor Green
} else {
    Write-Host "[WARN] Possible root traces:" -ForegroundColor Yellow
    if ($hasSU)         { Write-Host " - su binary present in /system/*/su" }
    if ($hasMagiskProp) { Write-Host " - getprop contains Magisk entries" }
    if ($hasAdbDir)     { if ($adbEmpty) { Write-Host " - /data/adb exists (empty)" } else { Write-Host " - /data/adb exists (has content)" } }
}

$vb  = (Shell 'getprop ro.boot.verifiedbootstate').Trim()
$lk  = (Shell 'getprop ro.boot.flash.locked').Trim()
$tag = (Shell 'getprop ro.build.tags').Trim()
Write-Host ("[INFO] VerifiedBoot: {0} | Bootloader locked: {1} | Build tags: {2}" -f $vb, $lk, $tag)
$stockLike = ($vb -eq 'green' -and $lk -eq '1' -and $tag -eq 'release-keys')
if ($stockLike) { Write-Host "[OK] Stock-like security state." -ForegroundColor Green } else { Write-Host "[WARN] Not fully stock-like. Please review." -ForegroundColor Yellow }

# 2.1) WebView: cập nhật whitelist trichrome theo dumpsys
$wvDump = Shell 'dumpsys webviewupdate'
try {
    $mCurrent = [regex]::Matches($wvDump, 'versionCode:\s*(\d+)', 'IgnoreCase')
    $mMin     = [regex]::Matches($wvDump, 'Minimum WebView version code:\s*(\d+)', 'IgnoreCase')
    if ($mCurrent.Count -gt 0) {
        $codes = @()
        foreach ($m in $mCurrent) { $codes += $m.Groups[1].Value }
        $codes = ($codes | Select-Object -Unique)
        foreach ($vc in $codes) { $BenignOrphanPatterns += "com.google.android.trichromelibrary_$vc" }
    }
    if ($mMin.Count -gt 0) {
        $minVC = $mMin[0].Groups[1].Value
        if ($minVC) { $BenignOrphanPatterns += "com.google.android.trichromelibrary_$minVC" }
    }
} catch { }

# 3) Deep scan /data/data
if (-not $CompactOutput) { Write-Section "/data/data deep scan" }
$dataDataDu = Shell 'toybox du -sm /data/data/* 2>/dev/null'; $dataItems = Parse-DuList $dataDataDu "/data/data/"
$setAll  = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$setSys  = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$setUser = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$allPkgs  | ForEach-Object { [void]$setAll.Add($_) }; $sysPkgs | ForEach-Object { [void]$setSys.Add($_) }; $userPkgs | ForEach-Object { [void]$setUser.Add($_) }

$userData = @(); $orphanDataStrict = @(); $orphanDataBenign = @(); $sysHeavy = @()
$emptyTrees = @()  # NEW: thu gom thư mục rỗng để log

foreach ($it in $dataItems) {
    $pkg = $it.Name; $size = $it.SizeMB
    $pkgPath = "/data/data/$pkg"
    $isEmptyTree = $false
    if ($DeepDirScan) {
        $info = Get-DirFileInfo $pkgPath $MaxFileSamples
        if ($info.FileCount -eq 0) {
            $isEmptyTree = $true
            $emptyTrees += $it
        } else {
            # đính kèm info để in khi cảnh báo
            Add-Member -InputObject $it -NotePropertyName FileCount -NotePropertyValue $info.FileCount -Force
            Add-Member -InputObject $it -NotePropertyName SampleFiles -NotePropertyValue $info.SampleFiles -Force
        }
    }
    if ($isEmptyTree) { continue }

    if     ($setUser.Contains($pkg)) { $userData += $it }
    elseif (-not $setAll.Contains($pkg)) {
        if (Is-BenignOrphan $pkg) { $orphanDataBenign += $it } else { $orphanDataStrict += $it }
    }
    else   {
        if ($size -ge $SysAppPerPkgWarnMB -and $setSys.Contains($pkg)) { $sysHeavy += $it }
    }
}

if ($emptyTrees.Count -gt 0) {
    Write-Host ("[OK] Empty app data trees (no files) in /data/data: {0}" -f $emptyTrees.Count) -ForegroundColor Green
    if (-not $CompactOutput) {
        $emptyTrees | Sort-Object Name | ForEach-Object { Write-Host (" - (empty) {0}" -f $_.Name) }
    }
}

if ($userData.Count -gt 0) {
    Write-Host ("[WARN] User app data present ({0}):" -f $userData.Count) -ForegroundColor Yellow
    $userData | Sort-Object SizeMB -Descending | ForEach-Object {
        if ($_.PSObject.Properties.Match('FileCount').Count -gt 0) {
            Write-Host (" - {0,-8} MB  {1} (files={2})" -f $_.SizeMB, $_.Name, $_.FileCount)
            if (-not $CompactOutput -and $_.SampleFiles) { foreach ($f in $_.SampleFiles) { Write-Host ("     · {0}" -f $f) } }
        } else {
            Write-Host (" - {0,-8} MB  {1}" -f $_.SizeMB, $_.Name)
        }
    }
}
if ($orphanDataStrict.Count -gt 0) {
    Write-Host ("[WARN] Orphan data (not in pm list) - counted for verdict ({0}):" -f $orphanDataStrict.Count) -ForegroundColor Yellow
    $orphanDataStrict | Sort-Object SizeMB -Descending | ForEach-Object {
        if ($_.PSObject.Properties.Match('FileCount').Count -gt 0) {
            Write-Host (" - {0,-8} MB  {1} (files={2})" -f $_.SizeMB, $_.Name, $_.FileCount)
            if (-not $CompactOutput -and $_.SampleFiles) { foreach ($f in $_.SampleFiles) { Write-Host ("     · {0}" -f $f) } }
        } else {
            Write-Host (" - {0,-8} MB  {1}" -f $_.SizeMB, $_.Name)
        }
    }
}
# Benign: luôn liệt kê (gọn) 1 lần
if ($orphanDataBenign.Count -gt 0) {
    Write-Host ("[WARN] Orphan data (benign, ignored for verdict) ({0}):" -f $orphanDataBenign.Count) -ForegroundColor Yellow
    $orphanDataBenign | Sort-Object SizeMB -Descending | ForEach-Object { Write-Host (" - {0,-8} MB  {1}" -f $_.SizeMB, $_.Name) }
}
if ($sysHeavy.Count -gt 0) {
    Write-Host ("[WARN] System app data large (>={0}MB) ({1}):" -f $SysAppPerPkgWarnMB, $sysHeavy.Count) -ForegroundColor Yellow
    $sysHeavy | Sort-Object SizeMB -Descending | ForEach-Object {
        if ($_.PSObject.Properties.Match('FileCount').Count -gt 0) {
            Write-Host (" - {0,-8} MB  {1} (files={2})" -f $_.SizeMB, $_.Name, $_.FileCount)
            if (-not $CompactOutput -and $_.SampleFiles) { foreach ($f in $_.SampleFiles) { Write-Host ("     · {0}" -f $f) } }
        } else {
            Write-Host (" - {0,-8} MB  {1}" -f $_.SizeMB, $_.Name)
        }
    }
}

# 4) /data/app, /data/local/tmp, /data/user(_de)/0
if (-not $CompactOutput) { Write-Section "/data/app, /data/local/tmp, /data/user(_de)/0" }
# /data/app: chỉ cảnh báo khi có leftover
$dataAppLs = Shell 'ls -l /data/app 2>/dev/null'
if ($dataAppLs -and $dataAppLs -notmatch 'No such file') {
    if ($userPkgs.Count -eq 0 -and $dataAppLs -match '^[dl-]') {
        Write-Host "[WARN] /data/app contains entries but pm -3 is empty (possible leftovers)." -ForegroundColor Yellow
        if (-not $CompactOutput) { Write-Host "[INFO] /data/app:"; Write-Host $dataAppLs }
    } else { if (-not $CompactOutput) { Write-Host "[INFO] /data/app:"; Write-Host $dataAppLs } }
} else { if (-not $CompactOutput) { Write-Host "[INFO] /data/app: (not present)" } }

# /data/local/tmp (deep)
$tmpFiles = 0; $tmpSamples = @()
if ($DeepDirScan) {
    $tmpInfo = Get-DirFileInfo "/data/local/tmp" $MaxFileSamples
    $tmpFiles = $tmpInfo.FileCount; $tmpSamples = $tmpInfo.SampleFiles
}
else {
    $tmpList = Shell 'ls -A /data/local/tmp 2>/dev/null'
    if ($tmpList) { $tmpFiles = 1 } else { $tmpFiles = 0 }
}
if ($tmpFiles -eq 0) {
    Write-Host "[OK] /data/local/tmp is empty." -ForegroundColor Green
} else {
    Write-Host "[WARN] /data/local/tmp has files." -ForegroundColor Yellow
    if (-not $CompactOutput -and $tmpSamples -and $tmpSamples.Count -gt 0) {
        foreach ($f in $tmpSamples) { Write-Host (" - {0}" -f $f) }
    }
}

# Summaries (có deep-check)
$summaryUser0  = Summarize-PathPassFail "/data/user/0"    $UserDataPerPkgRedMB "/data/user/0"
$summaryUserde = Summarize-PathPassFail "/data/user_de/0" $UserDataPerPkgRedMB "/data/user_de/0"

# 5) /sdcard (không bắt buộc deep vì rất lớn; giữ cách cân bằng hiệu năng)
if (-not $CompactOutput) { Write-Section "/sdcard (emulated data)" }
$sdUsageRaw = Shell 'du -s /sdcard/* 2>/dev/null | sort -n'
$sdItemsKB = @(); if ($sdUsageRaw) { foreach ($ln in ($sdUsageRaw -split "`r?`n")) { if ($ln -match '^\s*(\d+)\s+(.+)\s*$') { $sdItemsKB += [int]$matches[1] } } }
$sdTotalKB = ($sdItemsKB | Measure-Object -Sum).Sum; if (-not $sdTotalKB) { $sdTotalKB = 0 }
$sdTotalMB = [math]::Round(($sdTotalKB / 1024.0), 1)
if ($CompactOutput -and $sdTotalMB -lt $SdcardPassMB) {
    Write-Host ("[OK] /sdcard total: {0} MB (< {1} MB Pass)" -f $sdTotalMB, $SdcardPassMB) -ForegroundColor Green
} else {
    $sdList = Shell 'ls -la /sdcard/ 2>/dev/null'
    Write-Host "[INFO] /sdcard/:"; Write-Host $sdList
    if ($sdUsageRaw) { Write-Host "[INFO] /sdcard/* usage (KB):"; Write-Host $sdUsageRaw }
    Write-Host ("[INFO] /sdcard total: {0} MB" -f $sdTotalMB)
}

# 6) Setup flags & Top
$setupDone = (Shell 'settings get secure user_setup_complete').Trim()
$provision = (Shell 'settings get global device_provisioned').Trim()
Write-Host ("[INFO] Setup flags: user_setup_complete={0}, device_provisioned={1}" -f $setupDone, $provision)

$topData = Shell 'toybox du -sm /data/data/* 2>/dev/null | sort -nr | head -n 20'
if (-not $CompactOutput) { Write-Host "[INFO] Top /data/data (MB):"; Write-Host $topData }
elseif ($sysHeavy.Count -gt 0 -or $userData.Count -gt 0 -or $orphanDataStrict.Count -gt 0) {
    Write-Host "[INFO] Top /data/data (MB):"; Write-Host $topData
}

# 7) Verdict
$hasUserApps          = ($userPkgs.Count -gt 0)
$hasUserDataPresent   = ($userData.Count -gt 0)
$hasOrphanDataStrict  = ($orphanDataStrict.Count -gt 0)
$hasSysHeavy          = ($sysHeavy.Count -gt 0)

$tooManyUserOrphan = (($userData.Count + $orphanDataStrict.Count) -gt $MaxUserPkgsWarn)
$tooManyOrphans    = ($orphanDataStrict.Count -gt $MaxOrphanWarn)
$sdTooBig          = ($sdTotalMB -ge $SdcardWarnMB)
$anyUserRedBig     = (
    (($userData | Where-Object { $_.SizeMB -ge $UserDataPerPkgRedMB }).Count -gt 0) -or
    (($orphanDataStrict | Where-Object { $_.SizeMB -ge $UserDataPerPkgRedMB }).Count -gt 0) -or
    ($summaryUser0.fail.Count -gt 0) -or
    ($summaryUserde.fail.Count -gt 0)
)
$securityClean = (-not $hasSU -and -not $hasMagiskProp -and (-not $hasAdbDir -or $adbEmpty) -and $stockLike)

$finalColor = "Green"
if (-not $securityClean -or $sdTooBig -or $tooManyUserOrphan -or $tooManyOrphans -or $anyUserRedBig) {
    $finalColor = "Red"
} elseif ($hasUserApps -or $hasUserDataPresent -or $hasOrphanDataStrict -or $hasSysHeavy -or ($sdTotalMB -ge $SdcardPassMB) -or ($TreatSignedInAsWarn -and $hasAccounts)) {
    $finalColor = "Yellow"
}

$usageSummary = "PRISTINE"
if ($sdTotalMB -ge $SdcardWarnMB -or $tooManyUserOrphan -or $anyUserRedBig) {
    $usageSummary = "HEAVY USAGE"
} elseif ($sdTotalMB -ge $SdcardPassMB -or $hasUserDataPresent -or $hasOrphanDataStrict -or $hasSysHeavy -or ($summaryUser0.fail.Count -gt 0) -or ($summaryUserde.fail.Count -gt 0) -or ($TreatSignedInAsWarn -and $hasAccounts)) {
    $usageSummary = "LIGHT/MODERATE USAGE"
}

# Nếu muốn account là ĐỎ mạnh, bật block dưới:
if ($hasAccounts -and $TreatSignedInAsWarn) {
    # Chuyển từ vàng -> đỏ nếu đang xanh/vàng
    if ($finalColor -ne "Red") { $finalColor = "Red"; if ($usageSummary -eq "PRISTINE") { $usageSummary = "LIGHT/MODERATE USAGE" } }
}

$finalStatus = if ($finalColor -eq "Green") { "PASS (fully clean)" } elseif ($finalColor -eq "Yellow") { "NOT FULLY CLEAN" } else { "NOT CLEAN" }
$finalLine = ("=> Device {0}: {1} | Usage: {2}" -f $DeviceId, $finalStatus, $usageSummary)
switch ($finalColor) {
    "Green"  { Write-Host ("`n{0}" -f $finalLine) -ForegroundColor Green }
    "Yellow" { Write-Host ("`n{0}" -f $finalLine) -ForegroundColor Yellow }
    "Red"    { Write-Host ("`n{0}" -f $finalLine) -ForegroundColor Red }
}

# Reasons
if ($finalColor -ne "Green") {
    Write-Host ""
    Write-Host "[REASONS]" -ForegroundColor Cyan
    if ($hasAccounts) {
        if ($TreatSignedInAsWarn) {
            Write-Host " - Signed-in accounts detected." -ForegroundColor Yellow
        } else {
            Write-Host " - Accounts present (policy: ignored for verdict)." -ForegroundColor Yellow
        }
    }
    if ($hasUserDataPresent) { Write-Host " - User app data present." -ForegroundColor Yellow }
    if ($hasOrphanDataStrict) { Write-Host " - Orphan (non-whitelisted) app data present." -ForegroundColor Yellow }
    if ($hasSysHeavy) { Write-Host " - Large system app caches (>= threshold)." -ForegroundColor Yellow }
    if ($sdTooBig) { Write-Host (" - /sdcard too big (>= {0} MB)" -f $SdcardWarnMB) -ForegroundColor Yellow }
}

# END
