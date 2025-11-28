# [Tank War] User Profile Design

HỆ THỐNG THIẾT KẾ THÔNG TIN USER

Phiên bản: v1.2 Người tạo file: phucth12 (phucth12) Ngày tạo file: 01 - 07 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mota</td><td rowspan=1 colspan=1>Nguoiviet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét?</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>18-07-2025</td><td rowspan=1 colspan=1>Hoan thanh file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>22- 07-2025</td><td rowspan=1 colspan=1>Cäp nhät các thanh phän trong userproflie (RollID, PlayerName) + Thém dvitien te Gold/Diamond + Xoa phanpassword</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.2</td><td rowspan=1 colspan=1>19-09-2025</td><td rowspan=1 colspan=1>Format lai file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mục đích thiết kế

Liệt kê rõ ràng các thông tin của user cần có.

Đảm bảo thiết kế có những đặc điểm sau:

• Những thông số sau đều có thể được tham khảo bởi user chính.   
• Những thông số phải có định dạng rõ ràng.   
• Có sự tương tác giữa các người chơi khác nhau để xây dụng tính cộng đồng.

# 2. Mục tiêu tài liệu

Document này được lập để giúp đội Dev có danh sách hoàn chỉnh liệt kê các thành phần cần có trong 1 user profile.

# 3. Tổng quan tài liệu

4. Thành phần của user profile   
5. Cách tương tác với user profile   
6. Rủi ro

# 4 Thành phần của user profile

<table><tr><td colspan="1" rowspan="1">Thanhphan</td><td colspan="1" rowspan="1">Dinhdang</td><td colspan="1" rowspan="1"> Muc dich</td><td colspan="1" rowspan="1">Giadinh</td><td colspan="1" rowspan="1"> Limit value</td><td colspan="1" rowspan="1">Note</td></tr><tr><td colspan="1" rowspan="1">RoleID</td><td colspan="1" rowspan="1">Chuoiso</td><td colspan="1" rowspan="1">Dinh danh cua playerCho phép ket ban voi các user khác qua thanh phän nay</td><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1">7kitu</td><td colspan="1" rowspan="1">RolelD cua nguoichoi khong thetrung nhau</td></tr><tr><td colspan="1" rowspan="1">PlayerName</td><td colspan="1" rowspan="1">Chuoi kitu</td><td colspan="1" rowspan="1"> Nickname cua playerDung de hien thi ten cä nhäncua minh cho cac nguoi choicon lai trong game</td><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1">12 kituSu dung cac ki tutrong bang chucaitieng Anh &amp;cac chu so 0-9</td><td colspan="1" rowspan="1">PlayerName cuangudi choi c6thé trung nhau</td></tr><tr><td colspan="1" rowspan="1">Avatar</td><td colspan="1" rowspan="1">Hinh anh</td><td colspan="1" rowspan="1"> Hinh dai dien cua playerDung de hien thi hinh cä nhäncho cac nguoi chdi con lai trong game</td><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1">200Kb300x300 pixels</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Rank(ELO)</td><td colspan="1" rowspan="1">s6 vabieutuong</td><td colspan="1" rowspan="1">Cäp hang cua 1 playerThé hien trinh d choi cua player dó,trinh bay dung bieu tuong va so hang chinh xacbén canh</td><td colspan="1" rowspan="1">1000</td><td colspan="1" rowspan="1">500-0</td><td colspan="1" rowspan="1">Tham khao:目[TANK WAR]-ELO&amp; RANK SYSTEM DESIGN</td></tr><tr><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1"></td><td></td><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="4">Gotd</td><td colspan="1" rowspan="4">so</td><td></td><td></td><td></td><td></td></tr><tr><td colspan="1" rowspan="3"> Don vi tienté thong dung cuangudichoi.Dung de trao doi vat lieu vaphan thuong:</td><td></td><td></td><td colspan="1"></td></tr><tr><td colspan="1" rowspan="2">母</td><td colspan="1" rowspan="2"></td><td colspan="1"></td></tr><tr><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Diamond</td><td colspan="1" rowspan="1">so</td><td colspan="1" rowspan="1">Don vi tien te cao cap cuangudi choi. Dung de trao doi vat lieu vaphan thuong:</td><td colspan="1" rowspan="1">+</td><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Trophy Road</td><td colspan="1" rowspan="1">Chui kitu va model</td><td colspan="1" rowspan="1"> Kinh nghiem player nhän dudcCung cap phän thu8ng chocac players mi khi ho datdugc cäp bäc nhat dinh</td><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1">Tham khao: &lt;Thém filedesign cua TRvao&gt;</td></tr></table>

# 5. Cách tương tác với user profile

<table><tr><td>Thanh phan</td><td colspan="2"> Cach tuong tac</td><td>C6 the hien thi cho player khac khong?</td><td>Notes</td></tr><tr><td>PlayerNa me</td><td>ain Menu 0 PleayrNae bXx D 自 P ← Khong Main Menu 0</td><td>uerPrefle Main Menu 0 ↑ t mone </td><td>√</td><td></td></tr><tr><td>Avatar</td><td colspan="2">Changing Username</td><td>√</td><td></td></tr><tr><td rowspan="2"></td><td colspan="2">0 Main Menu UserPrflee Avatar scren Mai Menu</td><td rowspan="2">1 Khong</td><td rowspan="2"></td></tr><tr><td colspan="2">0 broner A Khong C ↓ Min enu 0</td></tr><tr><td>Rank (ELO）</td><td colspan="2">Changing Avatar</td><td colspan="1">Rank hien thi duoi dang √ bieu tuong&amp; giau chi so</td></tr><tr><td>Trophy</td><td colspan="2"></td><td colspan="1">chinh xac voi tat ca user Ngudichoi khac chi thäy dugc so Ivl user nhan</td></tr><tr><td>Road Gotd/Dia mond</td><td colspan="2"></td><td colspan="1">dudc</td></tr></table>

# 6. Rủi ro

Có 1 vài trường hợp cần cân nhắc khi thiết lập các hệ thông như:

• Player sử dụng từ ngữ tục tĩu khi điền vào phần PlayerName.