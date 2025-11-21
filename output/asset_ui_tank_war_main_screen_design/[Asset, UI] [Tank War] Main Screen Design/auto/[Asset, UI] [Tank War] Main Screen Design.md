# [Asset, UI] [Tank War] Main Screen Design

THÔNG TIN MÀN HÌNH CHÍNH

Version: v1.1   
Người viết: phucth12 (phucth12)   
Ngày tạo: 05 - 08 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mo ta</td><td rowspan=1 colspan=1>Ngudi viet</td><td rowspan=1 colspan=1>Ngudireview</td><td rowspan=1 colspan=1>Duyét？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>05-08-2025</td><td rowspan=1 colspan=1>Taofile</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format lai file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mục đích thiết kế

Xác định những gì người chơi có thể tương tác được trong màn hình chính của game.

Đảm bảo thiết kế có những đặc điểm sau:

Người chơi thấy thông tin các chức năng có trên màn hình chính được truyền tải rõ ràng.   
• Người chơi không bị quá choáng ngợp thông tin.

# 2. Mục tiêu tài liệu

Tài liệu được dùng để giúp đội Art & Dev hình dung rõ ràng hơn về các yếu tố trong màn hình chính.

3. Tổng quan tài liệu

4. Thành phần   
5. Tương tác   
6. Notes

# 4. Thành phần

<table><tr><td colspan="1" rowspan="1">Ten thanh phän</td><td colspan="1" rowspan="1"> Muc dich</td><td colspan="1" rowspan="1"> Design Note</td></tr><tr><td colspan="1" rowspan="1">Player info</td><td colspan="1" rowspan="1"> Hien thi thong tin chung cua nguoi choi, baogom:·Username·Avatar·ELO icon·ELO bar</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Find Match (Bät däutran)</td><td colspan="1" rowspan="1">Cho phép nguoi choi tim vä bät däu trän däu</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Tank showcase (Modeltank chinh)</td><td colspan="1" rowspan="1"> Showcase model tank 3D nguoi choi chonde showcase</td><td colspan="1" rowspan="1">Cän xäc dinh la cho phép doitank &amp; däu? Khöng thi tank có thay doi dua vao so lan sudung ko？</td></tr><tr><td colspan="1" rowspan="1"> Mode+ Ten mode</td><td colspan="1" rowspan="1"> Hien thi giao dien chon mode choi</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Garage (Chinh sua tank)</td><td colspan="1" rowspan="1">Chuyén nguoi choi den giao dién garage (Sélien ket voifile Garage Document sau)</td><td colspan="1" rowspan="1"> Hien thi nhung chua có noidung</td></tr><tr><td colspan="1" rowspan="1">Currency (Danh sáchtien te)</td><td colspan="1" rowspan="1">Liet ké các don vitien té quan trong cuagame</td><td colspan="1" rowspan="1"> Hién thi nhung chua äp dung</td></tr><tr><td colspan="1" rowspan="1">Settings</td><td colspan="1" rowspan="1"> Hien giao dien setting</td><td colspan="1" rowspan="1"> Hien thi nhung chua có noidung</td></tr><tr><td colspan="1" rowspan="1">Daily (Nhiem vu hangngay）</td><td colspan="1" rowspan="1"> Hien giao dien nhiem vu hang ngay</td><td colspan="1" rowspan="1"> Hien thi nhung chua có noidung</td></tr><tr><td colspan="1" rowspan="1">Giao dien Garage</td><td colspan="1" rowspan="1">Cho phép nguoichoi thay doi dien mao cuatank</td><td colspan="1" rowspan="1">Thong tin vé Garage nämtrong file: *dien link file vaoday</td></tr><tr><td colspan="1" rowspan="1">Giao dién User Profile</td><td colspan="1" rowspan="1">Cho phép nguoi choi thay doi avatar, username va thanh tich dat du@c</td><td colspan="1" rowspan="1">Chua c6 n@i dung</td></tr><tr><td colspan="1" rowspan="1">Giao dién Settings</td><td colspan="1" rowspan="1"> Cho phép nguoi choi thay döi thöng sö cua game, bao gom:·  (Danh sach sé dudc cäp nhät sau nay)</td><td colspan="1" rowspan="1">Chua c6 n@i dung</td></tr><tr><td colspan="1" rowspan="1"> Add player (Thém nguoichoi)</td><td colspan="1" rowspan="1"> Cho phép them nguoi choi khac vao party eua minh</td><td colspan="1" rowspan="1"></td></tr></table>

Note: Những thành phần tô xám sẽ được thêm sau

# 5. Tương tác

<table><tr><td rowspan=1 colspan=1>Tén thanh phän</td><td rowspan=1 colspan=1>Cach tuong tac</td><td rowspan=1 colspan=1>Notes</td></tr><tr><td rowspan=1 colspan=1>Player info</td><td rowspan=1 colspan=1> Nhän vao vung Player Info de hien ra giaodién User Profile</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Find Match (Bät däu trändau）</td><td rowspan=1 colspan=1>Khi nhän sé chuyén sang &quot;Finding&quot; vä hién thi so nguoi tham gia dugc vao phong. Saukhi có du thibät däu trän dau</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Garage (Chinh sua tank)</td><td rowspan=1 colspan=1> Nhän vao nut Garage dé hien ra giao dién</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1> Tank showcase (Model tankchinh)</td><td rowspan=1 colspan=1>Hién tai chi cho phép quan sat</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Mode</td><td rowspan=1 colspan=1> Nhän vao nut mode dé́ hién giao dién mode</td><td rowspan=1 colspan=1>√[Asset, UI] [Tank War] Mode Selection Design</td></tr><tr><td rowspan=1 colspan=1> Settings</td><td rowspan=1 colspan=1> Nhän vao nut Settings de hién ra giao dién Settings</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Daily (Nhiém vu häng ngay)</td><td rowspan=1 colspan=1> Hién giao dien nhiem vu häng ngay khi nhän vao nut</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Add player (Thém nguδichoi)</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr></table>

Note: Những thành phần tô xám sẽ được thêm sau

# 6. Notes

\*\*Nhung functions khac nhuShop,Events,Mode, Mission,v.v.,sédugc thém vao va hoan thién sau