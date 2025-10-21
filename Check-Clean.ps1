# Check-Clean.ps1  —  Deep Clean/Usage Checker for Android via ADB
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

    # Output mode
    [object]$CompactOutput = $true,

    # Treat "signed-in accounts" as not pristine (affects verdict)
    [object]$TreatSignedInAsWarn = $true,

    # Always list benign orphan (trichrome)
    [object]$AlwaysListBenign = $true,

    # Deep checks
    [object]$DeepDirScan = $true,   # Count real files under dirs
    [int]$MaxFileSamples = 5,       # Sample files per dir

    # Progress & Parallel
    [object]$ShowProgress = $true,  # Show progress bars
    [object]$UseParallel  = $true,  # Use -Parallel if PS 7+
    [int]$Parallelism     = 6,      # Max threads

    # Policy: warn if /data/adb exists but empty?
    [object]$WarnEmptyAdbDir = $false
)

# -------- Normalize booleans (fix when invoked via -File) --------
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
$CompactOutput       = To-Bool $CompactOutput
$TreatSignedInAsWarn = To-Bool $TreatSignedInAsWarn
$AlwaysListBenign    = To-Bool $AlwaysListBenign
$DeepDirScan         = To-Bool $DeepDirScan
$ShowProgress        = To-Bool $ShowProgress
$UseParallel         = To-Bool $UseParallel
$WarnEmptyAdbDir     = To-Bool $WarnEmptyAdbDir
# ------------------------------------------------------------

# ---- Helpers ----
function Coalesce($v, $fallback) {
    if ($null -eq $v) { return $fallback }
    $s = "$v"
    if ($s -eq "") { return $fallback }
    return $v
}
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

# Progress helpers (PS 5.1 safe)
function Start-ProgressBlock([string]$Activity, [string]$Status=""){
    if (-not $ShowProgress) { return }
    $st = Coalesce $Status ""
    Write-Progress -Activity $Activity -Status $st -PercentComplete 0
}
function Update-ProgressBlock([string]$Activity, [int]$Index, [int]$Total){
    if (-not $ShowProgress) { return }
    if ($Total -le 0) { Write-Progress -Activity $Activity -Status "Working..." -PercentComplete 0; return }
    $pct = [int](($Index / [math]::Max($Total,1)) * 100)
    Write-Progress -Activity $Activity -Status ("{0}/{1}" -f $Index, $Total) -PercentComplete $pct
}
function Stop-ProgressBlock([string]$Activity){
    if (-not $ShowProgress) { return }
    Write-Progress -Activity $Activity -Completed
}

# Whitelist orphan "benign"
$BenignOrphanPatterns = @('com.google.android.trichromelibrary_*')
function Is-BenignOrphan([string]$pkgName) {
    foreach ($pat in $BenignOrphanPatterns) { if ($pkgName -like $pat) { return $true } }
    return $false
}

# Deep dir inspector
function Get-DirFileInfo([string]$dir, [int]$maxSamples) {
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
        FileCount   = $fileCount
        SampleFiles = $samples
    }
}

# ===== Robust account detectors =====
function Get-AndroidUserIds {
    $out = Shell 'pm list users'
    $ids = @()
    if ($out) {
        foreach ($ln in ($out -split "`r?`n")) {
            if ($ln -match 'UserInfo\{(\d+):') { $ids += [int]$matches[1] }
        }
    }
    if ($ids.Count -eq 0) { $ids = @(0) }
    return $ids
}
function Get-AccountsByUser([int]$userId) {
    $accs = @()
    $out = Shell ("cmd account list --user {0}" -f $userId)
    if ($out) {
        foreach ($ln in ($out -split "`r?`n")) {
            if ($ln -match 'name=([^,]+),\s*type=([^\}]+)') {
                $name = $matches[1].Trim()
                $type = $matches[2].Trim()
                $accs += [pscustomobject]@{ UserId=$userId; Name=$name; Type=$type }
            }
        }
    }
    if ($accs.Count -eq 0) {
        $dump = Shell 'dumpsys account'
        $current = $null
        foreach ($ln in ($dump -split "`r?`n")) {
            if ($ln -match 'Account\s*\{') { $current = @{Name=$null;Type=$null} }
            if ($ln -match '\bname=([^\s,}]+)') { if ($current) { $current.Name = $matches[1] } }
            if ($ln -match '\btype=([^\s,}]+)') { if ($current) { $current.Type = $matches[1] } }
            if ($ln -match '\}') {
                if ($current -and $current.Name) {
                    $accs += [pscustomobject]@{ UserId=$userId; Name=$current.Name; Type=$current.Type }
                }
                $current = $null
            }
        }
    }
    return $accs
}
function Get-AllAccounts {
    $all = @()
    $users = Get-AndroidUserIds
    foreach ($uid in $users) { $all += Get-AccountsByUser $uid }
    return $all
}

# ===== Suspicious traces (packages & folders) =====
function Scan-SuspiciousTraces($allPkgs) {
    $flags = @()
    $pkgStr = ($allPkgs -join ' ')
    $patterns = @(
        'magisk', 'zygisk', 'riru', 'lsposed', 'edxposed', 'xposed'
    )
    foreach ($p in $patterns) {
        if ($pkgStr -match $p) { $flags += ("pkg:"+$p) }
    }
    $paths = @(
        '/data/adb', '/data/adb/modules', '/data/adb/riru', '/data/adb/magisk',
        '/data/adb/lsposed', '/data/adb/modules_update',
        '/system/xbin/su', '/system/bin/su', '/sbin/su'
    )
    foreach ($pp in $paths) {
        $ls = Shell ("ls -la {0} 2>/dev/null" -f $pp)
        if ($ls -and $ls -notmatch 'No such file') { $flags += ("path:"+$pp) }
    }
    return $flags
}

# ===== Generic path checker for general/suspicious dirs =====
function Check-CorePath([string]$title, [string]$globPath, [int]$maxSamples) {
    Write-Section $title
    Start-ProgressBlock ("[CORE] Scanning {0}" -f $globPath) "ls/find"
    $list = Shell ("ls -d {0} 2>/dev/null" -f $globPath)
    if (-not $list) {
        Stop-ProgressBlock ("[CORE] Scanning {0}" -f $globPath)
        Write-Host ("[CORE] {0} -> OK files=0 dirs=0 size_kb=0 (INACCESSIBLE_OR_NOT_EXISTS)" -f $globPath) -ForegroundColor Green
        return @{ files=0; dirs=0; sizeKB=0; items=@(); exists=$false }
    }
    $entries = @()
    foreach ($ln in ($list -split "`r?`n")) { if ($ln.Trim()) { $entries += $ln.Trim() } }
    $files=0; $dirs=$entries.Count; $sizeKB=0
    $items = @()
    $i=0; $tot=$entries.Count
    foreach ($d in $entries) {
        $i++; Update-ProgressBlock ("[CORE] Scanning {0}" -f $globPath) $i $tot
        $cntStr = Shell ("sh -c 'find ""$d"" -type f 2>/dev/null | wc -l'")
        $cnt = 0; if ($cntStr) { try { $cnt = [int]($cntStr.Trim()) } catch {} }
        $files += $cnt
        $du = Shell ("du -s ""$d"" 2>/dev/null")
        $kb = 0; if ($du -match '^\s*(\d+)\s+') { $kb = [int]$matches[1] }
        $sizeKB += $kb
        $samples = @()
        if ($cnt -gt 0 -and $maxSamples -gt 0) {
            $sp = Shell ("sh -c 'find ""$d"" -type f 2>/dev/null | head -n {0}'" -f $maxSamples)
            if ($sp) { foreach ($l in ($sp -split "`r?`n")) { if ($l.Trim()) { $samples += $l.Trim() } } }
        }
        $items += [pscustomobject]@{ Path=$d; FileCount=$cnt; SizeKB=$kb; Samples=$samples }
    }
    Stop-ProgressBlock ("[CORE] Scanning {0}" -f $globPath)
    $status = if ($files -eq 0) { "OK (empty trees)" } else { "WARN (has files)" }
    $color = if ($files -eq 0) { "Green" } else { "Yellow" }
    $msg = ("[CORE] {0} -> {1} files={2} dirs={3} size_kb={4}" -f $globPath, $status, $files, $dirs, $sizeKB)
    if ($color -eq "Green") { Write-Host $msg -ForegroundColor Green } else { Write-Host $msg -ForegroundColor Yellow }
    if (-not $CompactOutput) {
        foreach ($it in $items) {
            Write-Host (" - {0}  (files={1}, size_kb={2})" -f $it.Path, $it.FileCount, $it.SizeKB)
            if ($it.Samples -and $it.Samples.Count -gt 0) {
                foreach ($s in $it.Samples) { Write-Host ("     · {0}" -f $s) }
            }
        }
    }
    return @{ files=$files; dirs=$dirs; sizeKB=$sizeKB; items=$items; exists=$true }
}

# ===== PASS/FAIL per base path (with deep-check + progress) =====
function Summarize-PathPassFail([string]$basePath, [int]$perItemRedMB, [string]$title) {
    Write-Section $title

    Start-ProgressBlock ("[SCAN] Checking {0}/*" -f $basePath) "du -sm ..."
    $duRaw = Shell "toybox du -sm $basePath/* 2>/dev/null"
    Stop-ProgressBlock ("[SCAN] Checking {0}/*" -f $basePath)

    $items = Parse-DuList $duRaw $basePath
    if (-not $items -or $items.Count -eq 0) {
        Write-Host ("[OK] {0}: empty or not present." -f $basePath) -ForegroundColor Green
        return @{ totalMB = 0; totalCount = 0; fail = @(); passCount = 0; allPass = $true; emptyDirs=@(); nonEmpty=@() }
    }

    $emptyDirs = @()
    $nonEmptyItems = @()

    if ($DeepDirScan) {
        $total = $items.Count; $i=0
        Start-ProgressBlock ("[SCAN] Deep-check {0}" -f $basePath) "find -type f ..."
        foreach ($it in $items) {
            $i++; Update-ProgressBlock ("[SCAN] Deep-check {0}" -f $basePath) $i $total
            $info = Get-DirFileInfo $it.Path $MaxFileSamples
            if ($info.FileCount -eq 0) {
                $emptyDirs += $it
            } else {
                $obj = [pscustomobject]@{
                    SizeMB      = $it.SizeMB
                    Path        = $it.Path
                    Name        = $it.Name
                    FileCount   = $info.FileCount
                    SampleFiles = $info.SampleFiles
                }
                $nonEmptyItems += $obj
            }
        }
        Stop-ProgressBlock ("[SCAN] Deep-check {0}" -f $basePath)
    } else {
        $nonEmptyItems = $items
    }

    if ($emptyDirs.Count -gt 0) {
        Write-Host ("[OK] {0}: {1} entries are EMPTY (directory trees only, no files)." -f $basePath, $emptyDirs.Count) -ForegroundColor Green
        if (-not $CompactOutput) { $emptyDirs | ForEach-Object { Write-Host (" - (empty) {0}" -f $_.Name) } }
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

# =========================
# MAIN
# =========================
Write-Host ("=== Clean Check (deep) for device {0} ===" -f $DeviceId) -ForegroundColor Cyan
[void](ADB 'start-server')

# Devices ready?
$devicesOut = ADB 'devices'
if ($devicesOut -notmatch 'List of devices attached') { Write-Host "[ERR] 'adb devices' did not return expected header." -ForegroundColor Red; Write-Host $devicesOut; exit 1 }
$found = $false
foreach ($line in ($devicesOut -split "`r?`n")) { if ($line -match "^\s*$([regex]::Escape($DeviceId))\s+device\s*$") { $found = $true; break } }
if (-not $found) { Write-Host "[ERR] Device not found in 'device' state." -ForegroundColor Red; Write-Host $devicesOut; exit 1 }

# ---- CONNECT / DEVICE INFO ----
Write-Host "[CONNECT / DEVICE INFO]" -ForegroundColor Cyan
$brand  = (Shell 'getprop ro.product.brand').Trim()
$model  = (Shell 'getprop ro.product.model').Trim()
$rel    = (Shell 'getprop ro.build.version.release').Trim()
$sdk    = (Shell 'getprop ro.build.version.sdk').Trim()
$build  = (Shell 'getprop ro.build.display.id').Trim()
if (-not $model) { $model="(unknown)" }
Write-Host ("Serial: {0}" -f $DeviceId)
Write-Host ("Brand: {0}" -f (Coalesce $brand "(unknown)"))
Write-Host ("Model: {0}" -f $model)
Write-Host ("Android: {0} (API {1})" -f (Coalesce $rel "?"), (Coalesce $sdk "?"))
Write-Host ("Build: {0}" -f (Coalesce $build "?"))
Write-Host ""

# ---- PACKAGES ----
$rawAll  = Shell 'pm list packages'
$rawSys  = Shell 'pm list packages -s'
$rawUser = Shell 'pm list packages -3'
$allPkgs  = if ($rawAll)  { $rawAll  -split "`r?`n" | ForEach-Object { $_ -replace '^package:','' } | Where-Object { $_ } } else { @() }
$sysPkgs  = if ($rawSys)  { $rawSys  -split "`r?`n" | ForEach-Object { $_ -replace '^package:','' } | Where-Object { $_ } } else { @() }
$userPkgs = if ($rawUser) { $rawUser -split "`r?`n" | ForEach-Object { $_ -replace '^package:','' } | Where-Object { $_ } } else { @() }

Write-Host ("[INFO] System packages: {0} | User packages: {1}" -f $sysPkgs.Count, $userPkgs.Count)
if ($userPkgs.Count -eq 0) { Write-Host "[OK] No user apps." -ForegroundColor Green } else { Write-Host ("[WARN] Found {0} user apps:" -f $userPkgs.Count) -ForegroundColor Yellow; if (-not $CompactOutput) { $userPkgs | ForEach-Object { Write-Host (" - {0}" -f $_) } } }

# ---- ACCOUNTS (robust) ----
$allAccs = Get-AllAccounts
$accCount = $allAccs.Count
$googleAcc = ($allAccs | Where-Object { $_.Type -eq 'com.google' }).Count -gt 0
$hasAccounts = ($accCount -gt 0)

if ($hasAccounts) {
    $accSuffix = ""; if ($googleAcc) { $accSuffix = " (includes Google)" }
    if ($TreatSignedInAsWarn) {
        Write-Host ("[WARN] Accounts present: {0}{1}" -f $accCount, $accSuffix) -ForegroundColor Yellow
    } else {
        Write-Host ("[INFO] Accounts present: {0}{1} (policy: ignored for verdict)" -f $accCount, $accSuffix)
    }
    if (-not $CompactOutput) {
        foreach ($a in $allAccs) { Write-Host (" - [user {0}] {1} ({2})" -f $a.UserId, $a.Name, $a.Type) }
    }
} else {
    Write-Host "[OK] No accounts found." -ForegroundColor Green
}

# ---- SECURITY ----
$adbDir   = Shell 'ls -la /data/adb 2>/dev/null'
$suPaths  = Shell 'ls -la /system/xbin/su /system/bin/su 2>/dev/null'
$magiskGp = Shell 'getprop | grep -i magisk'
$hasAdbDir = ($adbDir -and $adbDir -notmatch 'No such file'); $adbEmpty = $true
if ($hasAdbDir) { $adbGlob = Shell 'ls -la /data/adb/* 2>/dev/null'; if ($adbGlob -and $adbGlob -notmatch 'No such file') { $adbEmpty = $false } }
$hasSU = ($suPaths -and $suPaths -notmatch 'No such file')
$hasMagiskProp = ($magiskGp -and $magiskGp.Trim().Length -gt 0)

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

# ---- WEBVIEW TRICHROME WHITELIST ----
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

# ---- /data/data deep scan + optional parallel ----
if (-not $CompactOutput) { Write-Section "/data/data deep scan" }
$dataDataDu = Shell 'toybox du -sm /data/data/* 2>/dev/null'; $dataItems = Parse-DuList $dataDataDu "/data/data/"

$setAll  = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$setSys  = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$setUser = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$allPkgs  | ForEach-Object { [void]$setAll.Add($_) }
$sysPkgs  | ForEach-Object { [void]$setSys.Add($_) }
$userPkgs | ForEach-Object { [void]$setUser.Add($_) }

$userData = @(); $orphanDataStrict = @(); $orphanDataBenign = @(); $sysHeavy = @()
$emptyTrees = @()

$act = "[SCAN] Checking /data/data/*"
Start-ProgressBlock $act "Preparing..."
$canParallel = $DeepDirScan -and $UseParallel -and ($PSVersionTable.PSVersion.Major -ge 7) -and ($dataItems.Count -ge 20)

if ($canParallel) {
    $total = $dataItems.Count
    $results = $dataItems | ForEach-Object -Parallel {
        using:MaxFileSamples > $null
        using:BenignOrphanPatterns > $null
        using:AdbPath > $null
        using:DeviceId > $null
        using:SysAppPerPkgWarnMB > $null
        function Is-BenignOrphan([string]$pkgName){
            foreach ($pat in $using:BenignOrphanPatterns) { if ($pkgName -like $pat) { return $true } }
            return $false
        }
        param($it)
        $pkg = $it.Name
        $path = "/data/data/$pkg"
        $countStr = & cmd.exe /c "$($using:AdbPath) -s $($using:DeviceId) shell `"sh -c 'find \"$path\" -type f 2>/dev/null | wc -l'`""
        $fileCount = 0
        if ($countStr) { try { $fileCount = [int]($countStr.Trim()) } catch {} }
        if ($fileCount -eq 0) { return [pscustomobject]@{Kind='empty'; Item=$it} }
        $sampleOut = & cmd.exe /c "$($using:AdbPath) -s $($using:DeviceId) shell `"sh -c 'find \"$path\" -type f 2>/dev/null | head -n $($using:MaxFileSamples)'`""
        $samples = @()
        if ($sampleOut) { $sampleOut -split "`r?`n" | ForEach-Object { if ($_.Trim()) { $samples += $_.Trim() } } }
        $it | Add-Member FileCount $fileCount -Force
        $it | Add-Member SampleFiles $samples -Force
        return [pscustomobject]@{Kind='nonempty'; Item=$it}
    } -ThrottleLimit $Parallelism

    $i=0
    foreach ($r in $results) {
        $i++; Update-ProgressBlock $act $i $total
        $it = $r.Item; $pkg = $it.Name; $size = $it.SizeMB
        if ($r.Kind -eq 'empty') { $emptyTrees += $it; continue }
        if     ($setUser.Contains($pkg)) { $userData += $it }
        elseif (-not $setAll.Contains($pkg)) { if (Is-BenignOrphan $pkg) { $orphanDataBenign += $it } else { $orphanDataStrict += $it } }
        elseif ($size -ge $SysAppPerPkgWarnMB -and $setSys.Contains($pkg)) { $sysHeavy += $it }
    }
    Stop-ProgressBlock $act
} else {
    $total = $dataItems.Count; $i=0
    Start-ProgressBlock $act "Scanning..."
    foreach ($it in $dataItems) {
        $i++; Update-ProgressBlock $act $i $total
        $pkg = $it.Name; $size = $it.SizeMB; $pkgPath = "/data/data/$pkg"
        $isEmptyTree = $false
        if ($DeepDirScan) {
            $info = Get-DirFileInfo $pkgPath $MaxFileSamples
            if ($info.FileCount -eq 0) { $isEmptyTree = $true; $emptyTrees += $it }
            else {
                Add-Member -InputObject $it -NotePropertyName FileCount -NotePropertyValue $info.FileCount -Force
                Add-Member -InputObject $it -NotePropertyName SampleFiles -NotePropertyValue $info.SampleFiles -Force
            }
        }
        if ($isEmptyTree) { continue }
        if     ($setUser.Contains($pkg)) { $userData += $it }
        elseif (-not $setAll.Contains($pkg)) { if (Is-BenignOrphan $pkg) { $orphanDataBenign += $it } else { $orphanDataStrict += $it } }
        else   { if ($size -ge $SysAppPerPkgWarnMB -and $setSys.Contains($pkg)) { $sysHeavy += $it } }
    }
    Stop-ProgressBlock $act
}
if ($emptyTrees.Count -gt 0) {
    Write-Host ("[OK] Empty app data trees (no files) in /data/data: {0}" -f $emptyTrees.Count) -ForegroundColor Green
    if (-not $CompactOutput) { $emptyTrees | Sort-Object Name | ForEach-Object { Write-Host (" - (empty) {0}" -f $_.Name) } }
}
if ($userData.Count -gt 0) {
    Write-Host ("[WARN] User app data present ({0}):" -f $userData.Count) -ForegroundColor Yellow
    $userData | Sort-Object SizeMB -Descending | ForEach-Object {
        if ($_.PSObject.Properties.Match('FileCount').Count -gt 0) {
            Write-Host (" - {0,-8} MB  {1} (files={2})" -f $_.SizeMB, $_.Name, $_.FileCount)
            if (-not $CompactOutput -and $_.SampleFiles) { foreach ($f in $_.SampleFiles) { Write-Host ("     · {0}" -f $f) } }
        } else { Write-Host (" - {0,-8} MB  {1}" -f $_.SizeMB, $_.Name) }
    }
}
if ($orphanDataStrict.Count -gt 0) {
    Write-Host ("[WARN] Orphan data (not in pm list) - counted for verdict ({0}):" -f $orphanDataStrict.Count) -ForegroundColor Yellow
    $orphanDataStrict | Sort-Object SizeMB -Descending | ForEach-Object {
        if ($_.PSObject.Properties.Match('FileCount').Count -gt 0) {
            Write-Host (" - {0,-8} MB  {1} (files={2})" -f $_.SizeMB, $_.Name, $_.FileCount)
            if (-not $CompactOutput -and $_.SampleFiles) { foreach ($f in $_.SampleFiles) { Write-Host ("     · {0}" -f $f) } }
        } else { Write-Host (" - {0,-8} MB  {1}" -f $_.SizeMB, $_.Name) }
    }
}
if ($orphanDataBenign.Count -gt 0 -and $AlwaysListBenign) {
    Write-Host ("[WARN] Orphan data (benign, ignored for verdict) ({0}):" -f $orphanDataBenign.Count) -ForegroundColor Yellow
    $orphanDataBenign | Sort-Object SizeMB -Descending | ForEach-Object { Write-Host (" - {0,-8} MB  {1}" -f $_.SizeMB, $_.Name) }
}
if ($sysHeavy.Count -gt 0) {
    Write-Host ("[WARN] System app data large (>={0}MB) ({1}):" -f $SysAppPerPkgWarnMB, $sysHeavy.Count) -ForegroundColor Yellow
    $sysHeavy | Sort-Object SizeMB -Descending | ForEach-Object {
        if ($_.PSObject.Properties.Match('FileCount').Count -gt 0) {
            Write-Host (" - {0,-8} MB  {1} (files={2})" -f $_.SizeMB, $_.Name, $_.FileCount)
            if (-not $CompactOutput -and $_.SampleFiles) { foreach ($f in $_.SampleFiles) { Write-Host ("     · {0}" -f $f) } }
        } else { Write-Host (" - {0,-8} MB  {1}" -f $_.SizeMB, $_.Name) }
    }
}

# ---- OTHER CORE PATHS (leftovers & tmp) ----
if (-not $CompactOutput) { Write-Section "/data/app, /data/local/tmp, /data/user(_de)/*" }

# /data/app leftovers
$dataAppLs = Shell 'ls -l /data/app 2>/dev/null'
if ($dataAppLs -and $dataAppLs -notmatch 'No such file') {
    if ($userPkgs.Count -eq 0 -and $dataAppLs -match '^[dl-]') {
        Write-Host "[WARN] /data/app contains entries but pm -3 is empty (possible leftovers)." -ForegroundColor Yellow
        if (-not $CompactOutput) { Write-Host "[INFO] /data/app:"; Write-Host $dataAppLs }
    } else { if (-not $CompactOutput) { Write-Host "[INFO] /data/app:"; Write-Host $dataAppLs } }
} else { if (-not $CompactOutput) { Write-Host "[INFO] /data/app: (not present)" } }

# /data/local/tmp deep
$tmpInfo = Get-DirFileInfo "/data/local/tmp" $MaxFileSamples
if ($tmpInfo.FileCount -eq 0) { Write-Host "[OK] /data/local/tmp is empty." -ForegroundColor Green }
else {
    Write-Host "[WARN] /data/local/tmp has files." -ForegroundColor Yellow
    if (-not $CompactOutput) { foreach ($f in $tmpInfo.SampleFiles) { Write-Host (" - {0}" -f $f) } }
}

# ---- Summaries for /data/user/* & /data/user_de/* (all users) ----
$users = Get-AndroidUserIds
foreach ($u in $users) {
    $p = "/data/user/$u";    [void](Summarize-PathPassFail $p $UserDataPerPkgRedMB $p)
    $pd= "/data/user_de/$u"; [void](Summarize-PathPassFail $pd $UserDataPerPkgRedMB $pd)
}

# ---- Suspicious/general directories ----
$cores = @(
    @{title="/data/system";        glob="/data/system";        sample=$MaxFileSamples},
    @{title="/data/system_ce/0";   glob="/data/system_ce/0";   sample=$MaxFileSamples},
    @{title="/data/system_de/0";   glob="/data/system_de/0";   sample=$MaxFileSamples},
    @{title="/data/misc";          glob="/data/misc/*";        sample=$MaxFileSamples},
    @{title="/data/misc_ce/0";     glob="/data/misc_ce/0";     sample=$MaxFileSamples},
    @{title="/data/misc_de/0";     glob="/data/misc_de/0";     sample=$MaxFileSamples},
    @{title="/data/property";      glob="/data/property";      sample=$MaxFileSamples},
    @{title="/data/vendor";        glob="/data/vendor/*";      sample=$MaxFileSamples},
    @{title="/data/adb";           glob="/data/adb/*";         sample=$MaxFileSamples},
    @{title="/cache";              glob="/cache/*";            sample=$MaxFileSamples},
    @{title="/metadata";           glob="/metadata/*";         sample=$MaxFileSamples},
    @{title="/persist";            glob="/persist/*";          sample=$MaxFileSamples}
)
foreach ($c in $cores) { [void](Check-CorePath $c.title $c.glob $c.sample) }

# /sdcard (emulated) — quick summary only
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

# Setup flags & Top table
$setupDone = (Shell 'settings get secure user_setup_complete').Trim()
$provision = (Shell 'settings get global device_provisioned').Trim()
Write-Host ("[INFO] Setup flags: user_setup_complete={0}, device_provisioned={1}" -f $setupDone, $provision)

$topData = Shell 'toybox du -sm /data/data/* 2>/dev/null | sort -nr | head -n 20'
if (-not $CompactOutput) { Write-Host "[INFO] Top /data/data (MB):"; Write-Host $topData }
elseif ($sysHeavy.Count -gt 0 -or $userData.Count -gt 0 -or $orphanDataStrict.Count -gt 0) {
    Write-Host "[INFO] Top /data/data (MB):"; Write-Host $topData
}

# ---- SUSPECT SCAN ----
$susFlags = Scan-SuspiciousTraces $allPkgs
# Filter out /data/adb if it's empty (treat as INFO or optional WARN via flag)
if ($adbEmpty) { $susFlags = @($susFlags | Where-Object { $_ -ne 'path:/data/adb' }) }

# --------------------
# VERDICT AGGREGATION
# --------------------
# Buckets:
$severeReasons   = @()  # 🔴 nghiêm trọng
$moderateReasons = @()  # 🟡 trung bình
$minorReasons    = @()  # 🟢 thông tin

# 1) Security / root
if ($hasSU)               { $severeReasons   += "su binary present" }
if ($hasMagiskProp)       { $severeReasons   += "magisk-related properties present" }
if ($hasAdbDir -and -not $adbEmpty) { $severeReasons += "/data/adb exists with content" }
if ($hasAdbDir -and  $adbEmpty) {
    if ($WarnEmptyAdbDir) { $moderateReasons += "/data/adb exists (empty)" }
    else                  { $minorReasons    += "/data/adb exists (empty)" }
}
if (-not $stockLike)      { $moderateReasons += "not fully stock-like security state" }
foreach ($f in $susFlags) { $moderateReasons += ("suspicious: "+$f) }

# 2) Usage / data
if ($userPkgs.Count -gt 0)                   { $moderateReasons += ("user apps present: "+$userPkgs.Count) }
if ($userData.Count -gt 0)                   { $moderateReasons += ("user app data present: "+$userData.Count) }
if ($orphanDataStrict.Count -gt 0)           { $moderateReasons += ("orphan data present: "+$orphanDataStrict.Count) }
if ($sysHeavy.Count -gt 0)                   { $moderateReasons += ("large system caches: "+$sysHeavy.Count) }
if ($emptyTrees.Count -gt 0)                 { $minorReasons    += ("many empty app data trees: "+$emptyTrees.Count) }

# /sdcard usage
if ($sdTotalMB -ge $SdcardWarnMB)            { $severeReasons   += ("/sdcard too big (>= "+$SdcardWarnMB+" MB)") }
elseif ($sdTotalMB -ge $SdcardPassMB)        { $moderateReasons += ("/sdcard not empty (~"+$sdTotalMB+" MB)") }
else                                         { $minorReasons    += ("/sdcard very small (~"+$sdTotalMB+" MB)") }

# Accounts policy
if ($hasAccounts) {
    if ($TreatSignedInAsWarn) { $moderateReasons += "signed-in accounts detected" }
    else                      { $minorReasons    += "accounts present (ignored by policy)" }
}

# Final decision:
$finalColor = "Green"      # 🟢 minor only
if ($severeReasons.Count -gt 0) {
    $finalColor = "Red"    # 🔴 if any severe
} elseif ($moderateReasons.Count -gt 0) {
    $finalColor = "Yellow" # 🟡 if any moderate but no severe
}

# Usage summary (lightweight heuristic)
$usageSummary = "PRISTINE"
if ($sdTotalMB -ge $SdcardWarnMB -or $userData.Count -ge 3 -or $orphanDataStrict.Count -ge 3) {
    $usageSummary = "HEAVY USAGE"
} elseif ($sdTotalMB -ge $SdcardPassMB -or $userData.Count -gt 0 -or $orphanDataStrict.Count -gt 0 -or $sysHeavy.Count -gt 0 -or $hasAccounts) {
    $usageSummary = "LIGHT/MODERATE USAGE"
}

$finalStatus = if ($finalColor -eq "Green") { "PASS (fully clean)" } elseif ($finalColor -eq "Yellow") { "NOT FULLY CLEAN" } else { "NOT CLEAN" }
$finalLine = ("=> Device {0}: {1} | Usage: {2}" -f $DeviceId, $finalStatus, $usageSummary)
switch ($finalColor) {
    "Green"  { Write-Host ("`n{0}" -f $finalLine) -ForegroundColor Green }
    "Yellow" { Write-Host ("`n{0}" -f $finalLine) -ForegroundColor Yellow }
    "Red"    { Write-Host ("`n{0}" -f $finalLine) -ForegroundColor Red }
}

# Reasons (grouped by severity)
if ($finalColor -ne "Green" -or $minorReasons.Count -gt 0) {
    Write-Host ""
    Write-Host "[REASONS]" -ForegroundColor Cyan
    if ($severeReasons.Count -gt 0) {
        Write-Host "  [ERROR]" -ForegroundColor Red
        foreach ($r in $severeReasons)   { Write-Host ("   - {0}" -f $r) }
    }
    if ($moderateReasons.Count -gt 0) {
        Write-Host "  [WARN]" -ForegroundColor Yellow
        foreach ($r in $moderateReasons) { Write-Host ("   - {0}" -f $r) }
    }
    if ($minorReasons.Count -gt 0) {
        Write-Host "  [INFO]"
        foreach ($r in $minorReasons)    { Write-Host ("   - {0}" -f $r) }
    }
}

# END
