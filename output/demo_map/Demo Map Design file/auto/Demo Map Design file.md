# Demo Map Design file

Demo Map Design file

Project TOG - Map Design Guideline: Desert Environment

Mục tiêu thiết kế

Test mecharicnic game.

Test lợi thế của 2 bên theo giai đoạn của trận đấu

# 1. Theme Overview

• Tên bản đồ: Crimson Dunes / Rust Frontier / Khamsin Outpost

• Bối cảnh: Sa mạc khô cằn hậu tận thế, từng là căn cứ quân sự bỏ hoang nay trở thành điểm tranh chấp chiến lược giữa các phe tank.

• Tông màu: Cam đất, vàng cháy, đỏ gỉ sét, điểm xanh bạc từ thiết bị công nghệ cũ.

• Phong cách kiến trúc: Hầm quân sự, trạm radar, container rỉ sét, tháp canh gãy đổ, tàn tích bê tông.

2. Kết cấu bản đồ 3v3

# 2.1 Kích thước đề xuất

• Tổng diện tích: $\mathord { \sim } 5 0$ unit x 50unit • Lane chính: 3 lane (trung tâm $^ { + 2 }$ cánh) • Khoảng cách giữa các Outpost: 20‒25unit

# 2.2 Khu vực chính

Click the image to view the sheet.

3. Gợi ý Terrain & Chi tiết môi trường

Click the image to view the sheet.

4. Tỷ lệ bố trí

Click the image to view the sheet.

5. Lighting & FX

• Ánh sáng chiều đỏ cam ‒ ánh sáng chéo tạo bóng đậm để giúp phân biệt silhouette các tank.   
• Hạt bụi bay nhẹ $^ +$ hiệu ứng nhiệt mờ xa.   
• Âm thanh nền: gió sa mạc, tiếng kim loại va chạm nhẹ.

# 6. Gameplay Notes

• Flank route giúp Scout tạo áp lực bất ngờ.   
• Cát lún gần Outpost ép team phải kiểm soát map trước.   
• Dốc cao có thể đặt turret hoặc sniping drone.   
• Map lý tưởng để thử cơ chế buff từ terrain (vị trí cao tăng tầm nhìn).

7. Ghi chú kỹ thuật (Unity Setup)

• Đơn vị đo chuẩn: 1 Unity Unit $= 1 \mathsf { m }$ • NavMesh: Dùng NavMesh Modifier cho cát lún, ramp dốc • Prefab khối chính: Container (2x4m), Bunker (6x6m), Wall (2x1m) • Lighting Bake: Static terrain, dynamic tank shadow

8. Mở rộng sau này

• Storm Event: Gió cát che tầm nhìn 5s/lần • Map Night Variant: Đèn pha xe tăng $^ +$ vùng ánh sáng động • Secret Tunnel: Tạo điểm dịch chuyển giữa 2 flank lane trong 10s cooldown

# Chỉ số & Tỷ lệ quan trọng trong thiết kế bản đồ PvP ‒ Project Tank War

1. Tỷ lệ giao tranh dự kiến

Click the image to view the sheet.

Thiết kế map cần 2‒3 "choke points" để định hướng giao tranh tự nhiên.

2. Tỷ lệ phân bổ khu vực bản đồ

Click the image to view the sheet.

3. Tốc độ di chuyển & khoảng cách giao tranh Click the image to view the sheet.

Đảm bảo tất cả tank có thể tiếp cận outpost đầu tiên trong ${ \tt { \tt { \tt { \tt { \tt { \tt { 7 5 } } } } } } }$ , tránh cảm giác bị bỏ lại quá xa.

4. Phân bố Outpost & Combat Zone

• Tổng Outpost: 3 điểm chính (A ‒ B ‒ C)

• Khoảng cách Outpost - Base DEF: 10‒20 unit

• Khoảng cách giữa các Outpost: 25‒35 unit để tạo không gian chia lane & mở combat đa hướng

• Bán kính chiếm outpost: $\sim 5 \mathrm { - } 6$ unit

• Combat zone buffer: mỗi Outpost nên có ${ \sim } 1 2 { - } 1 5$ unit vùng xung quanh có cover, flank lối và vật cản

• Ưu thế phe Def: Cần phải define rõ ưu thế của phe Def theo thứ tự của từng trụ. Ví dụ trụ A là vị trí dễ công khó thủ nhất, xếp sau là trụ B, và cuối cùng là trụ C nơi phe Def có nhiều lợi thế nhất để đảm bảo yếu tố bất ngờ có thể xảy ra.

# 5. Tỷ lệ giao tranh 1v1 / 2v2 / tổng lực (teamfight) Click the image to view the sheet.

Gợi ý: bản đồ 3v3 nên thiết kế thiên về chia lane $^ +$ đụng độ nhỏ, còn 5v5 nên có 1 khu vực “teamfight” chính để tận dụng DPS của Heavy tank.

# 6. Nhịp chiến thuật & thời lượng vòng lặp Click the image to view the sheet.

Tổng vòng lặp 1 combat ${ \sim } 2 5 { \mathrm { - } } 3 0 5 $ Trong 1 trận 10 phút có thể có 20‒25 lượt giao tranh nhỏ hoặc 8‒10 combat lớn.

7. Tổng Kill dự kiến

a. Tổng 30 kill cho 2 phe.