# [Asset, UI] [Tank War] In-game GUI Design

THÔNG TIN THIẾT KẾ GUI TRONG TRẬN ĐẤU

Version: v1.4   
Người viết: phucth12 (phucth12)   
Ngày tạo: 28 - 07 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mo ta</td><td rowspan=1 colspan=1>Nguoiviet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>30-07-2025</td><td rowspan=1 colspan=1>Hoan thanh file</td><td rowspan=1 colspan=1>O phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>05-08-2025</td><td rowspan=1 colspan=1>Overhaul lai phän thanh phän vä tuongtac theo tien d ban tháng 8-9</td><td rowspan=1 colspan=1>O phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.2</td><td rowspan=1 colspan=1>06-08-2025</td><td rowspan=1 colspan=1>Cäp nhät Stats &amp; Shop</td><td rowspan=1 colspan=1>® phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.3</td><td rowspan=1 colspan=1>12-09-2025</td><td rowspan=1 colspan=1> Chinh sua thanh phän mäu cua trän däu</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.4</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format lai file</td><td rowspan=1 colspan=1>O phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mục đích thiết kế

Phân định rõ ràng các thông tin cần trình bày và các cách tương tác với nó trong màn hình chính của trận đấu.

Đảm bảo thiết kế có những đặc điểm sau:

Liệt kê đầy đủ các yếu tố cần có mặt trong màn hình chính của trận đấu, mục đích và cách thức tương tác với các yếu tố đó.   
• Những thông số nếu có phải định dạng rõ ràng.

# 2. Mục tiêu tài liệu

Tài liệu được dùng để giúp đội Art & Dev thiết kế thông tin hiển thị trong trận đấu phù hợp.

# 3. Tổng quan tài liệu

4. Thành phần   
5. Tương tác a. Tương tác với các thành phần b. Tương tác trong các trường hợp khác

6. Notes

# 4. Thành phần

<table><tr><td rowspan=1 colspan=1> Thanh phän</td><td rowspan=1 colspan=1> Muc dich</td><td rowspan=1 colspan=1>Note</td></tr><tr><td rowspan=1 colspan=1>Move joystick</td><td rowspan=1 colspan=1>Dieu khien tank</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Shoot joystick</td><td rowspan=1 colspan=1>Dieu khien huong bän cua tank</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1> Skill button</td><td rowspan=1 colspan=1>Cho phép player su dung skill</td><td rowspan=1 colspan=1> Néu cän tien dé kich hoatitem,cän hien thi so tienyéu cau</td></tr><tr><td rowspan=1 colspan=1>Game clock</td><td rowspan=1 colspan=1>Cung cäp thong tin thαi gian choi va tong sö killcua 2 phia</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Player list</td><td rowspan=1 colspan=1>Liet ké thong tin vé dong doi, bao gom:·Username·Avatar·K/D</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Mission bar</td><td rowspan=1 colspan=1>Cung cap thong tin vé muc tieu chinh cua gamemode</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr></table>

Cung cấp thông tin về HP của tất cả mọi vật có HP

• Tham khảo HP của tank: [Tank War]_Tank System Detail   
• Tham khảo HP của outpost: [Tank War] Outpost Design - Base Capture Mode   
Tham khảo HP của các vật thể khác nhau trên map: [Tank War] Map Design Document   
Riêng từng tank sẽ có UI   
gồm:   
• Số lvl   
• Thanh HP   
• Ngôi sao truy nã

UI nên nằm trên vật thể

<table><tr><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1">NOTE: Hien thi mau cua thanh mau thay doiphu thu@c vao tung player:· Mau xanh duonghién thi máu playerMau xanh lahien thi mäu dong di (TeamDEF cüng sé thäy máu outpost mau xanh la)：Mau dohien thi mäu team dich va raocan (neu có ap dung)</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Damage dealt</td><td colspan="1" rowspan="1">Cung cap thong tin vé dps ngui choi gay ra</td><td colspan="1" rowspan="1"> UI nén nhódé khóng bivu&amp;ng tam nhin nguoi choi</td></tr><tr><td colspan="1" rowspan="1">Stats icon</td><td colspan="1" rowspan="1"> Hien thi thanh mua stats</td><td colspan="1" rowspan="1"> Vi tri du tinh: Nam 8 dui, giua man hinh</td></tr><tr><td colspan="1" rowspan="1">Stats shop</td><td colspan="1" rowspan="1">Cho phép player mua stats,c6 3 muc rieng biet:·HP·Damage·Fire RateSPD4 muc nay deu hien thi gia tien cua nó, nämtrong file *DIEN FILE BALANCING VAO DAY*</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Combat log</td><td colspan="1" rowspan="1">Bao cao khi có player bi giet va bi giét böi ai</td><td colspan="1" rowspan="1">Chi chua:*Ten ngudi giet* *icongiet* * Ten nguoi bi giet*</td></tr><tr><td colspan="1" rowspan="1">Buff bar</td><td colspan="1" rowspan="1">Danh sach bufngu8i chdi nhän dugc</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1"> Shop icon</td><td colspan="1" rowspan="1">Dé m&amp; giao dién shop mua skill</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1"> Shop side bar</td><td colspan="1" rowspan="1"> Noi nguαi choi mua skillva attributes</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1"> Minimap</td><td colspan="1" rowspan="1"> Cung cap thóng tin cua map</td><td colspan="1" rowspan="1"></td></tr><tr><td></td><td>*Ban vé cách trinh bay map &amp; noi dung trong map*</td><td></td></tr><tr><td>Option button</td><td>M&amp; giao dién cho nguoi choithay doi am luong, thoat game</td><td></td></tr></table>

Note:

Những thành phần được tô xám có thể sẽ được áp dụng sau này • Những thành phần được tô xanh chưa được áp dụng nhưng sẽ tốt khi có thể áp dụng được

# 5. Tương tác

# a. Tương tác với các thành phần

<table><tr><td colspan="1" rowspan="1">Thanh phän</td><td colspan="1" rowspan="1">Cach tuong täc</td><td colspan="1" rowspan="1">Note</td></tr><tr><td colspan="1" rowspan="1"> Move joystick</td><td colspan="1" rowspan="1">Joystick dung dé di chuyén,kéo huóng nao thi tank sétuong üng di chuyén huóng d6Note: Chua kéo fulljoystick (chua tói vi tri xa nhat sovoi vung trung tam), tank vän sé di chuyén full töc dó.</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Shoot joystick</td><td colspan="1" rowspan="1">Khi kéo joystick vé 1 huong, nong sung tank sé chiasang huöng tuong tu va bän dan:。 Dan bän lién tiep khi nguoi choi tiep tuc kéo joystick0 Dan dugc tiep tuc bän khi nguoichoi kéo joystick sang hu&amp;ng khácDan sé ngung bän khi nguoichoi thä joystickNote: Chua kéo full joystick (chua tói vi tri xa nhät sovoi vung trung tam),tank vän bän nhu binh thuong.</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1"> Skill buttons</td><td colspan="1" rowspan="1">Tham khäo: 目[Combat Module][Tank War] Mobile SkillControl System</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Stats icon</td><td colspan="1" rowspan="1">Nhän vao icon sé hién ra stats shop</td><td colspan="1" rowspan="1">Néu duαc, player cóthé quét lén tu gócduoi de hién statsshop</td></tr><tr><td colspan="1" rowspan="1"> Stats shop</td><td colspan="1" rowspan="1">Player có thé nhan vao 1 trong 3 muc dé mua stats tuongung</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1">Player nhän vao nut kéo xuöng hoäc nhän bät ki noi naobén ngoai de ha shop</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Minimap</td><td colspan="1" rowspan="1">Quan sat thöng tin Nhäp vao minimap de hien thi giao dien map lón</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Shop icon</td><td colspan="1" rowspan="1"> Nhap vao icon dé hien thi giao dien shop</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Shop side bar</td><td colspan="1" rowspan="1">：   Git icon cua skill dé xem thong tin·   Nhäp vao s6 tien &amp; du&amp;i icon dé mua no·   Nhan vao vung ngoai giao dien tat giao dien·   C6 tabs dung dé chuyen tu giao dien skillsang giaodien attribute hoac nguoc lai</td><td colspan="1" rowspan="1">Cän phai xem ki xemviec mua do = nhap vao s6 tien c6 thät st tam giam kha nang nguoi choi mua nhämskitt?</td></tr><tr><td colspan="1" rowspan="1">Option button</td><td colspan="1" rowspan="1"> Nhäp vao button de hien thi giao dien</td><td colspan="1" rowspan="1"></td></tr></table>

# b. Tương tác trong các trường hợp khác

<table><tr><td rowspan=1 colspan=1>Case</td><td rowspan=1 colspan=1>Behaviour/Solution</td></tr><tr><td rowspan=1 colspan=1> Khi player thoat app</td><td rowspan=1 colspan=1>Néu thαi gian chua hét, vän cho phép player δ trong trän khi quay lai</td></tr><tr><td rowspan=1 colspan=1>Khi player xai skill cän aim</td><td rowspan=1 colspan=1> Shoot joystick tr&amp; thänh Skilljoystick,cho phép aim skill, chia ra 3 truonghop:：  Nhäp skill button 1 län nua dé cancel su dung skill.Nhäm joystick va thä dé su dung skill.·Nhäp vao 1 skill button khäc sé override sang su dung skill dó.</td></tr><tr><td rowspan=1 colspan=1> Khi thoat tran (Trong mucoptions)</td><td rowspan=1 colspan=1>Nguoichoi sé khóng thé quay lai vao trän,va dugc tinh la thua(?)</td></tr></table>

# 6. Notes