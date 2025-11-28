## Features Of The Game

### Mục đích

Thư mục này dùng để quản lý **các tính năng (features)** của trò chơi. Mỗi tính năng được tổ chức theo một cấu trúc riêng biệt để đảm bảo:

- Dễ dàng quản lý và mở rộng.
- Có khả năng hoạt động độc lập (modular).
- Dễ tái sử dụng trong các dự án khác.

---

### Cấu trúc thư mục tính năng

Mỗi module tính năng sẽ được đặt trong:  
`Assets/_GameModules/ModuleA/`  
Với cấu trúc đề xuất như sau:

ModuleA/
├── Scripts/ # Chứa mã nguồn chính của module
├── Prefabs/ # Các prefab liên quan
├── Testing/ # Unit tests hoặc test script cho module
├── Demo_Scene/ # Cảnh demo riêng của tính năng
├── Images|Textures|Sprites/ # Tài nguyên hình ảnh (UI, icon, v.v.)
├── Animation/ # (Tuỳ chọn) Animation clip và controller
├── Materials/ # (Tuỳ chọn) Các material sử dụng riêng cho module


> ⚠️ Lưu ý: Mỗi tính năng nên được phát triển và kiểm thử độc lập, sau đó mới tích hợp vào hệ thống chung.