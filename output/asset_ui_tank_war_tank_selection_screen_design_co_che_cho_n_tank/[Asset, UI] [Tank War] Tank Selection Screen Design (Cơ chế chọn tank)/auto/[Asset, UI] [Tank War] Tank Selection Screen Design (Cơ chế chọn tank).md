# [Asset, UI] [Tank War] Tank Selection Screen Design (Cơ chế chọn tank)

THÔNG TIN CƠ CHẾ CHỌN TANK

Version: v1.1   
Người viết: phucth12 (phucth12)   
Ngày tạo: 28 - 07 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mo ta</td><td rowspan=1 colspan=1>Ngudi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>04-08-2025</td><td rowspan=1 colspan=1>Taofile</td><td rowspan=1 colspan=1>®phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format lai file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mục đích thiết kế

Xác định với người chơi những gì họ có thể làm trong giai đoạn chuẩn bị chiến đấu.

Đảm bảo thiết kế có những đặc điểm sau:

• Giữ công bằng giữa 2 đội chơi khi chọn tank. Người chơi cảm thấy giai đoạn chọn tank diễn ra phù hợp và không lâu.

# 2. Mục tiêu tài liệu

Tài liệu được dùng để giúp đội Art & Dev hình dung rõ ràng hơn về các yếu tố trong màn hình chọn tank.

3. Tổng quan tài liệu

4. Thành phần   
5. Tương tác a. Tương tác với các thành phần b. Các cách tương tác khác

6. Notes

# 4. Thành phần

<table><tr><td rowspan=1 colspan=1>Tén thanh phän</td><td rowspan=1 colspan=1>Muc dich</td><td rowspan=1 colspan=1>Design Note</td></tr><tr><td rowspan=1 colspan=1>Thdi gian</td><td rowspan=1 colspan=1> Hién thi thdi gian cho phép 8trong manhinh chon tank va thoi gian chuan bidévao tran Khi hét thαi gian chon, players va nhüng tanks ho chon dugc dua vao trong map：  Thoi gian chon tank: 15sGET READY!: 5s.</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Danh sách tank</td><td rowspan=1 colspan=1>Liét ké cac loai tank</td><td rowspan=1 colspan=1>Tank duαc säp xép theo class Sé dugc än di sau khi het thoi gian chon tank (15s)</td></tr><tr><td rowspan=1 colspan=1>Danh sách 2 doi choi</td><td rowspan=1 colspan=1>Cac nguoi chdi dugc chia thanh 2 doi.Danh sách nay cho thäy nguoichoi voiminh sé la aiva sé chon tank gi</td><td rowspan=1 colspan=1>Vi tri 2 team sé dugc säp sän la:·ATK: TraiDEF: Phai Hién thi ten,avatar cua player, tén tank va model 3D tank hochon</td></tr><tr><td rowspan=1 colspan=1>Nhiem vu</td><td rowspan=1 colspan=1>Giup nguoi choi (mói) hiéu muc tiéu chdicua team minhDugc hien ra sau khi moi nguoi hoanthanh giai doan chon tank</td><td rowspan=1 colspan=1>Thoi gian hien thi nhiém vu la5s</td></tr><tr><td rowspan=1 colspan=1> Nut khoa tank</td><td rowspan=1 colspan=1> Chän ngudi chdi d6itank sau khi dudebdit</td><td rowspan=1 colspan=1> Nam trong hogc gän danh sach tank</td></tr></table>

# 5. Tương tác

# a. Tương tác với các thành phần

<table><tr><td rowspan=1 colspan=1>Tén thänh phän</td><td rowspan=1 colspan=1>Cach tuong tác</td></tr><tr><td rowspan=1 colspan=1>Thoi gian chon tank</td><td rowspan=1 colspan=1>Quan sat</td></tr><tr><td rowspan=1 colspan=1>Danh säch tank</td><td rowspan=1 colspan=1>Nhän vao các ó tank dé́ chon loai tank muönchoi, tank khong chon du@c sé bi xám/den di.Sé än di sau khi hét thoi gian chon tank</td></tr><tr><td rowspan=1 colspan=1>2 hang doi choi</td><td rowspan=1 colspan=1>Quan sät xem các tank dä dugc chon la gi décó thé lén chien thuät</td></tr><tr><td rowspan=1 colspan=1>Nhiem vu</td><td rowspan=1 colspan=1>Doc nhiem vuThoi gian: 5s</td></tr><tr><td rowspan=1 colspan=1> Nut khoa tank</td><td rowspan=1 colspan=1> Nam trong hoac gän danh sach tank, nhan vao icon de khoa lua chon tank ctia minh</td></tr></table>

# b. Các cách tương tác khác

<table><tr><td rowspan=1 colspan=1>Case</td><td rowspan=1 colspan=1>Behaviour/Solution</td></tr><tr><td rowspan=1 colspan=1> Khi player thoat app</td><td rowspan=1 colspan=1>Néu thαi gian chua hét, vän cho phép player ＆ trong trän khi quay lai</td></tr><tr><td rowspan=1 colspan=1>Khi player chua chon tank</td><td rowspan=1 colspan=1>Mäc dinh möi player chon tank δ däu danh sách tank khi bätdäu screenchon</td></tr></table>

# 6. Notes