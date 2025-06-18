import argparse

def has_reached_max_games(file_name, max_games=11):
    try:
        # Mở và đọc số dòng trong file
        with open(file_name, "r", encoding="utf-8") as file:
            lines = file.readlines()
            # Kiểm tra số dòng có đạt tối đa hay không
            return len(lines) >= max_games
    except FileNotFoundError:
        # Nếu file không tồn tại, trả về False
        return False

def main():
    # Sử dụng argparse để lấy tên file từ dòng lệnh
    parser = argparse.ArgumentParser(description="Kiểm tra xem file có đủ 11 game hay chưa.")
    parser.add_argument("-c", "--file", required=True, help="Tên file TXT cần kiểm tra.")
    
    args = parser.parse_args()  # Phân tích các tham số dòng lệnh
    file_name = args.file       # Tên file TXT được truyền vào

    # Kiểm tra file và in kết quả
    if has_reached_max_games(file_name):
        print("True")
    else:
        print("False")

if __name__ == "__main__":
    main()
