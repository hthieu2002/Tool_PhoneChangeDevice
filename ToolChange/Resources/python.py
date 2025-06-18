import xml.etree.ElementTree as ET
import argparse

# Hàm đọc danh sách các tên đã lưu trước đó
def read_existing_names(file_name):
    try:
        with open(file_name, "r", encoding="utf-8") as file:
            return [line.strip() for line in file.readlines()]
    except FileNotFoundError:
        return []

# Hàm xử lý file XML và trích xuất tên game
def extract_game_names(xml_file, resource_ids):
    tree = ET.parse(xml_file)
    root = tree.getroot()

    game_names = set()
    for node in root.iter("node"):
        text = node.attrib.get("text")  # Lấy thuộc tính 'text'
        resource_id = node.attrib.get("resource-id")  # Lấy thuộc tính 'resource-id'

        # Kiểm tra nếu resource_id khớp với các giá trị được chỉ định
        if resource_id in resource_ids:
            if text and text.strip():  # Đảm bảo text không rỗng
                short_name = text.strip()[:10]  # Giới hạn tên game tối đa 10 ký tự
                game_names.add(short_name)

    return game_names

# Tập hợp các resource-id liên quan đến tên game
resource_ids_set = {
    "com.play.lucky.real.earn.money.free.fun.games.play.reward.income:id/ongoing_adv_name",
    "com.play.lucky.real.earn.money.free.fun.games.play.reward.income:id/title"
}

def main():
    # Sử dụng argparse để xử lý các cờ
    parser = argparse.ArgumentParser(description="Trích xuất tên game từ file XML.")
    parser.add_argument("-c", "--file", required=True, help="Tên file đầu vào (XML).")
    parser.add_argument("device", help="Tên thiết bị, được sử dụng làm tên file đầu ra.")

    # Phân tích các cờ từ câu lệnh
    args = parser.parse_args()

    xml_file = args.file  # Lấy tên file XML đầu vào
    output_file = f"{args.device}_game_names.txt"  # Tên file đầu ra dựa trên tên thiết bị

    # Đọc các tên đã lưu trước đó
    existing_names = read_existing_names(output_file)

    # Xử lý file XML để lấy danh sách tên game mới
    new_game_names = extract_game_names(xml_file, resource_ids_set)

    # Lọc chỉ những tên chưa có trong file
    unique_new_names = [name for name in new_game_names if name not in existing_names]

    # Giới hạn số dòng tối đa là 11
    final_names_to_save = (existing_names + unique_new_names)[:11]

    # Ghi danh sách cuối cùng vào file
    with open(output_file, "w", encoding="utf-8") as file:
        for name in final_names_to_save:
            file.write(name + "\n")

    # Tính số lượng tên thực sự được lưu mới
    new_saved_count = len(final_names_to_save) - len(existing_names)

    print(f"Đã lưu thành công {new_saved_count} tên game mới vào {output_file}.")

if __name__ == "__main__":
    main()
