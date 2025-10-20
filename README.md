⚙️ Cách chạy script Check-Clean.ps1<br>
1️⃣ Mở PowerShell (Admin)<br>
Ví dụ:<br>
Nhấn Start → gõ PowerShell → Run as Administrator<br>
2️⃣ Di chuyển đến thư mục chứa file<br>
cd "C:\Users\Hoang Trung Hieu\OneDrive\Desktop"<br>
3️⃣ Chạy script với quyền Bypass<br>
Ví dụ kiểm tra thiết bị:<br>
powershell -ExecutionPolicy Bypass -File .\Check-Clean.ps1 -DeviceId 98JAY15771<br>
4️⃣ Một số tùy chọn hữu ích<br>
Tham số	Ý nghĩa	Mặc định<br>
-DeviceId	ID thiết bị ADB (bắt buộc)	—<br>
-AdbPath	Đường dẫn adb.exe	adb<br>
-TreatSignedInAsWarn	Nếu có tài khoản Google thì coi là cảnh báo	true<br>
-DeepDirScan	Quét sâu thư mục (đếm file thật)	true<br>
-MaxFileSamples	Số file mẫu hiển thị mỗi thư mục	5<br>
-CompactOutput	Rút gọn log (ẩn chi tiết file nhỏ)	true<br>
Ví dụ chạy đầy đủ:<br>
powershell -ExecutionPolicy Bypass -File .\Check-Clean.ps1  -DeviceId ...  -TreatSignedInAsWarn 0 <br>

✅ Kết quả:<br>
Nếu thư mục chỉ còn cây rỗng ⇒ [OK] (empty)<br>
Nếu còn file thật ⇒ hiển thị số lượng file & vài file mẫu.<br>
Phần cuối cùng in ra verdict:<br>
PASS (fully clean)<br>
NOT FULLY CLEAN<br>

NOT CLEAN

Bạn chỉ cần commit file mới này, sau đó chạy lệnh trên — script sẽ tự động kiểm tra sâu hệ thống Android qua ADB.
