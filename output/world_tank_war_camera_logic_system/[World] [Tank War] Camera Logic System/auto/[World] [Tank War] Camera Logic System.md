# [World] [Tank War] Camera Logic System

HỆ THỐNG CAMERA LOGIC

Phiên bản: v1.1 Người tạo file: Kent Ngày cập nhật: 22 - 07 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>M6ta</td><td rowspan=1 colspan=1>Nguoi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyet？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>22-07-2025</td><td rowspan=1 colspan=1> Hoan thanh file</td><td rowspan=1 colspan=1>QuocTA</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format lai file</td><td rowspan=1 colspan=1>® phucth12</td><td rowspan=1 colspan=1> Kent</td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mục tiêu thiết kế

• Tái hiện phong cách camera tương tự \*\*Liên Quân Mobile (MOBA view)\*\*.

• Giữ góc nhìn nghiêng chiến thuật $( \sim 4 5 ^ { \circ } )$ , vừa bao quát vừa gần nhân vật.

• Camera tự động \*\*xoay theo hướng chiến đấu của phe\*\*.

• Hỗ trợ chế độ \*\*Spectator sau khi chết\*\*, chuyển qua view đồng đội còn sống.

# 2. Mục lục

3. Góc nhìn chính trong trận đấu   
4. Hành vi camera trong trận   
5. Spectator Mode (Chế Độ Quan Sát)   
6. Chức năng làm mờ vật cản chắn tầm nhìn camera - Fade Camera Occluder   
7. Gợi Ý Triển Khai Cho Dev   
8. Mở rộng

# 3. Góc nhìn chính trong trận đấu

<table><tr><td rowspan=1 colspan=1>Yeu tó</td><td rowspan=1 colspan=1>Mo ta</td></tr><tr><td rowspan=1 colspan=1>Góc nhin tong thé</td><td rowspan=1 colspan=1>Top-down nghieng xuong (45°)</td></tr><tr><td rowspan=1 colspan=1>Chieu cao camera</td><td rowspan=1 colspan=1>15-20 units (tuy theo scale bändo/tank)</td></tr><tr><td rowspan=1 colspan=1>Khoäng cách dén tank</td><td rowspan=1 colspan=1>15 units</td></tr><tr><td rowspan=1 colspan=1>Field of View (FOV)</td><td rowspan=1 colspan=1>60</td></tr><tr><td rowspan=1 colspan=1>Projection</td><td rowspan=1 colspan=1>Perspective</td></tr><tr><td rowspan=1 colspan=1> Huóng camera</td><td rowspan=1 colspan=1>Tuy theo phe, luon huóng véphia doi thu</td></tr></table>

Định hướng camera theo phe:

• Phe A (Xanh) Camera từ dưới lên.   
• Phe B $( { \bf \delta \vec { \bf { \delta } } } ( { \bf \vec { \bf { \delta } } } ( { \bf \vec { \bf { \delta } } } |  0 )$ Camera từ trên xuống.

# 4. Hành vi camera trong trận

<table><tr><td>Trang thai</td><td>Hänh vi camera</td></tr><tr><td></td><td></td></tr><tr><td colspan="1" rowspan="1">Khi con song</td><td colspan="1" rowspan="1">Camera lock theo tank, giugóc nghiéng&amp; chieu cao</td></tr><tr><td colspan="1" rowspan="1">Khi di chuyén</td><td colspan="1" rowspan="1">Camera truot muot theotank,khóng lac, khóng xoay theo huóng</td></tr><tr><td colspan="1" rowspan="1">Khong edge pan</td><td colspan="1" rowspan="1">Khóng cho thao tac kéocamera bäng cam ung</td></tr><tr><td colspan="1" rowspan="1">Khóng auto xoay</td><td colspan="1" rowspan="1">Camera khong xoay theohu&amp;ng di chuyén cua tank</td></tr></table>

# 5. Spectator Mode (Chế Độ Quan Sát)

<table><tr><td rowspan=1 colspan=1>Tinh näng</td><td rowspan=1 colspan=1>Mo ta</td></tr><tr><td rowspan=1 colspan=1>Kich hoat</td><td rowspan=1 colspan=1>Ngay khi nguoi choi bi haguc</td></tr><tr><td rowspan=1 colspan=1>Chuyen dong doi</td><td rowspan=1 colspan=1>Có thé nhän Next/Prev détheo doi nguoi khac</td></tr><tr><td rowspan=1 colspan=1>Góc nhin</td><td rowspan=1 colspan=1>Giu nguyén thong socamera nhu luc diéu khien</td></tr><tr><td rowspan=1 colspan=1>Thoat ché do</td><td rowspan=1 colspan=1>Tu dong khi player hoi sinh</td></tr><tr><td rowspan=1 colspan=1>Free cam (tuy chon)</td><td rowspan=1 colspan=1>Có thé bö sungα phase sau</td></tr></table>

# 6. Chức năng làm mờ vật cản chắn tầm nhìn camera - Fade Camera Occluder

• Mô tả: Khi tầm nhìn người chơi tới tank bị hạn chế bởi các vật thể, các vật thể đó sẽ được làm mờ đi.

• Công dụng: Hiện rõ vị trí tank, hạn chế làm nhiễu thông tin người chơi.

• Điều kiện kích hoạt: Khi model của tank bị che $> 5 0 \%$

# 7. Gợi Ý Triển Khai Cho Dev

<table><tr><td rowspan=1 colspan=1>Yéu cäu</td><td rowspan=1 colspan=1>Goi y trién khai</td></tr><tr><td rowspan=1 colspan=1>Góc nghieng co dinh</td><td rowspan=1 colspan=1>Virtual Camera nghiéng 45°</td></tr><tr><td rowspan=1 colspan=1>Xoay theo huóng cua phe</td><td rowspan=1 colspan=1>Y-axis O° hoäc 180° theo team</td></tr><tr><td rowspan=1 colspan=1>Mugt khi bäm tank</td><td rowspan=1 colspan=1>Dung damping (0.2-0.3s)</td></tr><tr><td rowspan=1 colspan=1>Spectator Mode</td><td rowspan=1 colspan=1>Chuyén camera follow sangdong doi con song</td></tr></table>

# 8. Mở rộng

• Áp dụng cho các mode 5v5 (Solo / Team / Custom) • Không có zoom / freecam tùy chỉnh trong giai đoạn đầu • Spectator chỉ dạng "lock-on", chưa hỗ trợ camera bay tự do