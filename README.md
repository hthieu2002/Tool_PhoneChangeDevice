⚙️ Cách chạy script Check-Clean.ps1
1️⃣ Mở PowerShell (Admin)
Ví dụ:
Nhấn Start → gõ PowerShell → Run as Administrator
2️⃣ Di chuyển đến thư mục chứa file
cd "C:\Users\Hoang Trung Hieu\OneDrive\Desktop"
3️⃣ Chạy script với quyền Bypass
Ví dụ kiểm tra thiết bị:
powershell -ExecutionPolicy Bypass -File .\Check-Clean.ps1 -DeviceId 98JAY15771
4️⃣ Một số tùy chọn hữu ích
Tham số	Ý nghĩa	Mặc định
-DeviceId	ID thiết bị ADB (bắt buộc)	—
-AdbPath	Đường dẫn adb.exe	adb
-TreatSignedInAsWarn	Nếu có tài khoản Google thì coi là cảnh báo	true
-DeepDirScan	Quét sâu thư mục (đếm file thật)	true
-MaxFileSamples	Số file mẫu hiển thị mỗi thư mục	5
-CompactOutput	Rút gọn log (ẩn chi tiết file nhỏ)	true
Ví dụ chạy đầy đủ:
powershell -ExecutionPolicy Bypass -File .\Check-Clean.ps1  -DeviceId ...  -TreatSignedInAsWarn 0 

✅ Kết quả:
Nếu thư mục chỉ còn cây rỗng ⇒ [OK] (empty)
Nếu còn file thật ⇒ hiển thị số lượng file & vài file mẫu.
Phần cuối cùng in ra verdict:
PASS (fully clean)
NOT FULLY CLEAN

NOT CLEAN

Bạn chỉ cần commit file mới này, sau đó chạy lệnh trên — script sẽ tự động kiểm tra sâu hệ thống Android qua ADB.
