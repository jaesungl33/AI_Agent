## The Game Logic

### Mục đích

Quản lý các thành phần **logic cốt lõi** của trò chơi bao gồm:

- Luồng xử lý chính.
- Quá trình khởi động (bootstrap).
- Tải và nạp dữ liệu (data loading).

Hướng đến:

- Dễ dàng quản lý và mở rộng.
- Có khả năng hoạt động độc lập (modular).
- Dễ tái sử dụng trong các dự án khác.

---

### Cấu trúc thư mục hệ thống logic

Mỗi hệ thống logic được đặt trong:  
`Assets/_Game/ABCSystem/`

Với cấu trúc thư mục đề xuất:

ABCSystem/
├── *.cs # Chứa toàn bộ các file mã nguồn C# dành riêng cho hệ thống đó, nhằm xử lý các logic liên quan.

> ⚠️ Lưu ý: Mỗi hệ thống nên được thiết kế độc lập và có khả năng hoạt động riêng biệt. Điều này giúp dễ dàng kiểm thử v