# [World] [Tank War] Grass Logic Design

HỆ THỐNG LOGIC CỦA CỎ XANH

Version: 1.1   
Người viết: phucth12 (phucth12)   
Ngày tạo: 11 - 09 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mo ta</td><td rowspan=1 colspan=1>Ngudi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>11-09-2025</td><td rowspan=1 colspan=1>Hoan thanh file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format lai file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1> Kent</td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mục đích thiết kế

Tài liệu thiết kế chức năng cỏ trong map.

Cung cấp chiều sâu về mặt chiến thuật cho game.

# 2. Mục tiêu tài liệu

Tài liệu được dùng để giúp đội Art & Dev thiết kế cỏ đúng chức năng.

3. Tổng quan tài liệu

3. Tổng quan tài liệu

4. Ảnh hưởng tới a. Ảnh hưởng tới đối tượng khác b. Trường hợp ảnh hưởng ngoại lệ

5. Tác động bởi

6. Notes

# 4 Ảnh hưởng tới

a. Ảnh hưởng tới đối tượng khác

<table><tr><td rowspan=1 colspan=1>Doi tuong</td><td rowspan=1 colspan=1> Két qua mong muon</td><td rowspan=1 colspan=1>Note</td></tr><tr><td rowspan=1 colspan=1>Tank</td><td rowspan=1 colspan=1>Tank sé tang hinh</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Dan</td><td rowspan=1 colspan=1>Dan se tang hinh</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>VFX</td><td rowspan=1 colspan=1>：  Sat thuong c6 hien thiEffect ban dan khong hien thi：：  Khói xe khong hien thiEffect Bear Trap c6 hien thi·： Effect Kamikaze có hien thi： Effect Hook</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Moitruong</td><td rowspan=1 colspan=1>Có có shadow</td><td rowspan=1 colspan=1></td></tr></table>

# Skills:

<table><tr><td rowspan=1 colspan=1>Doi tuong</td><td rowspan=1 colspan=1>Ket qua mong muon</td><td rowspan=1 colspan=1>Note</td></tr><tr><td rowspan=1 colspan=1>Bear Trap</td><td rowspan=1 colspan=1>Bay sé tang hinh</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Hook</td><td rowspan=1 colspan=1>Hook khong tang hinh</td><td rowspan=1 colspan=1></td></tr></table>

# b. Trường hợp ảnh hưởng ngoại lệ

<table><tr><td rowspan=1 colspan=1>Truong hop</td><td rowspan=1 colspan=1>Dieu kien phu</td><td rowspan=1 colspan=1>Ket qua mong muon</td><td rowspan=1 colspan=1>Note</td></tr><tr><td rowspan=3 colspan=1>Tank dongdoidivaotham co</td><td rowspan=1 colspan=1> Player dang dung 8ngoai</td><td rowspan=3 colspan=1>Tank dong doi khong tang hinhTank dong di hien thi dan + VFX.day du</td><td rowspan=3 colspan=1></td></tr><tr><td rowspan=1 colspan=1> Player trong cungtham co</td></tr><tr><td rowspan=1 colspan=1> Player trong thamco khac</td></tr><tr><td rowspan=4 colspan=1>Tank dich divao tham co</td><td rowspan=1 colspan=1>Player dang dung &amp;ngoai</td><td rowspan=2 colspan=1>Tank dich tang hinh</td><td rowspan=4 colspan=1></td></tr><tr><td rowspan=1 colspan=1> Player trong thamco khac</td></tr><tr><td rowspan=1 colspan=1> Player trong cungtham co</td><td rowspan=1 colspan=1>·Tank dich khóng tang hinh·Tank dich hien thi dan + VFX däy du</td></tr><tr><td rowspan=1 colspan=1>Tank dich bi bäntrung</td><td rowspan=1 colspan=1> Hien thichi sö sat thuong</td></tr><tr><td rowspan=2 colspan=1>Player8trong tham co</td><td rowspan=1 colspan=1>Tank dich trongtham co khac</td><td rowspan=1 colspan=1>Tank dich tang hinh</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1> Tank dich bän trungminh</td><td rowspan=1 colspan=1> Hien thi chi so sat thuong</td><td rowspan=1 colspan=1>Player van thäy VFXdan</td></tr><tr><td rowspan=1 colspan=1>Outpost bänvao tham co</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1> Hien thi VFX outpost day du</td><td rowspan=1 colspan=1></td></tr></table>

# 5. Tác động bởi

<table><tr><td rowspan=1 colspan=1>Doi tuong</td><td rowspan=1 colspan=1>Két quä mong muon</td><td rowspan=1 colspan=1>Note</td></tr><tr><td rowspan=1 colspan=1>VFX</td><td rowspan=1 colspan=1>Gi6 lam co dung dua</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Tank</td><td rowspan=1 colspan=1>· Tao SFX có khi tank di chuyén trong thäm co</td><td rowspan=1 colspan=1></td></tr></table>

Note: Sẽ được cập nhật khi có trường hợp mới

# 6. Notes

• Kích thước đối tượng được đo bằng đơn vị mặc định trong Unity (Unit): ◦ 1 mét $\mathbf { \lambda } = \mathbf { 1 }$ Unit trong Unity   
• Trường hợp liệt kê áp dụng cho cả 2 đội.