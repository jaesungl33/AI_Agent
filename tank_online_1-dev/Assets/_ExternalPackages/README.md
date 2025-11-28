## The External Packages

### Mục đích

Quản lý các packages/plugins được lấy từ bên ngoài:

- Miễn phí 
- Có phí bản quyền

Hướng đến:

- Dễ dàng quản lý và mở rộng.
- Có khả năng hoạt động độc lập (modular).
- Dễ tái sử dụng trong các dự án khác.

---

### Cấu trúc thư mục hệ thống logic

Mỗi hệ thống logic được đặt trong:  
`Assets/_ExternalPackages/ABCPackage/`

Với cấu trúc thư mục đề xuất:

ABCPackage/
├── Scripts/ # Chứa mã nguồn chính 
├── Prefabs/ # Các prefab liên quan
├── Scenes|Demo/ # Sử dụng để người dùng có thể kiểm tra package hoạt động như thế nào
├── Images|Textures|Sprites/ # Tài nguyên hình ảnh (UI, icon, v.v.) 
├── Animation/ # (Tuỳ chọn) Animation clip và controller
├── Materials/ # (Tuỳ chọn) Các material chỉ sử dụng cho package này

> ⚠️ Lưu ý: Đảm bảo giấy phép nếu cần thiết, luôn đảm bảo có lưu trữ thông tin để sau này khi chính thức phát hành mà cần giấy phép sẽ biết phải mua ở đâu.