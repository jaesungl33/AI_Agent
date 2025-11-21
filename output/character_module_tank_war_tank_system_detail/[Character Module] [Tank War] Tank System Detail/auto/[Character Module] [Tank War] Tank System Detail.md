# [Character Module] [Tank War] Tank System Detail

# HỆ THỐNG XE TANK

Phiên bản: v1.5 Người tạo: QuocTA Ngày tạo file: 29 - 06 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mo ta</td><td rowspan=1 colspan=1>Nguoiviet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét?</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>29-06-2025</td><td rowspan=1 colspan=1>Tao file</td><td rowspan=1 colspan=1>QuocTA</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>30-06-2025</td><td rowspan=1 colspan=1> Dieu chinh Damage fix thanh Damagerange (min-max)</td><td rowspan=1 colspan=1>QuocTA</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.2</td><td rowspan=1 colspan=1>09-07-2025</td><td rowspan=1 colspan=1>Diéu chinh lai Balance, thém 2 chi so coban (chua dung den)</td><td rowspan=1 colspan=1>QuocTA</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.3</td><td rowspan=1 colspan=1>04-08-2025</td><td rowspan=1 colspan=1>Thém phän dieu khien tank</td><td rowspan=1 colspan=1>phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.4</td><td rowspan=1 colspan=1>20-08-2025</td><td rowspan=1 colspan=1>Dieu chinh thong tin skill va thém thong tin passives</td><td rowspan=1 colspan=1>phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.5</td><td rowspan=1 colspan=1>18-09-2025</td><td rowspan=1 colspan=1>Format file + Tam xoä thong tin noi tai</td><td rowspan=1 colspan=1>phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Tổng quan chức năng

Hệ thống xe Tank trong Tank War bao gồm ba lớp chiến xa chính: Scout Tank, Assault Tank và Heavy Tank. Mỗi loại xe mang đặc điểm riêng về chỉ số, vai trò chiến thuật và cách sử dụng trong trận chiến.

# 2. Mục đích thiết kế

Tạo ra sự đa dạng về lối chơi, cho phép người chơi lựa chọn phong cách điều khiển Tank phù hợp nhất với chiến thuật cá nhân hoặc nhóm.

# 3. Mục đích thiết kế

4. Mục tiêu thiết kế   
5. Chỉ số chung   
6. Công thức tính chỉ số cơ bản   
7. Phân loại Class chi tiết 7.1 Tank hạng nhẹ (Scout Tank) 7.2 Tank hạng trung (Assault Tank)

# 4. Mục tiêu thiết kế

• Phân hạng rõ ràng 3 Class Tank: nhanh - cân bằng - sát thương chủ lực.

• Dễ dàng cân bằng theo Mode chơi.

• Dễ mở rộng cho skin, nâng cấp, tính năng trong tương lai.

• Tất cả chỉ số và đơn vị đều được quy đổi theo chuẩn Unity:

◦ Speed: units/second   
◦ Range: units (1 unit Unity $\approx 1$ mét)   
◦ Time: seconds   
◦ Fire Rate: shots/second

5. Chỉ số chung

<table><tr><td rowspan=1 colspan=1>Chi so</td><td rowspan=1 colspan=1>Tén</td><td rowspan=1 colspan=1>Design Weight</td></tr><tr><td rowspan=1 colspan=1>HP</td><td rowspan=1 colspan=1>Health</td><td rowspan=1 colspan=1>1.2</td></tr><tr><td rowspan=1 colspan=1>Speed</td><td rowspan=1 colspan=1>Movement Speed</td><td rowspan=1 colspan=1>1.5</td></tr><tr><td rowspan=1 colspan=1>Damage</td><td rowspan=1 colspan=1>Base Damage</td><td rowspan=1 colspan=1>1</td></tr><tr><td rowspan=1 colspan=1>Reload Time</td><td rowspan=1 colspan=1>Reload Speed</td><td rowspan=1 colspan=1>0.8</td></tr><tr><td rowspan=1 colspan=1>Fire Rate</td><td rowspan=1 colspan=1>Shots per second</td><td rowspan=1 colspan=1>1.5</td></tr><tr><td rowspan=1 colspan=1>Projectile Speed Bullet Velocity</td><td rowspan=1 colspan=1>Projectile Speed Bullet Velocity</td><td rowspan=1 colspan=1>0.5</td></tr><tr><td rowspan=1 colspan=1>Range</td><td rowspan=1 colspan=1>Attack Range</td><td rowspan=1 colspan=1>0.7</td></tr><tr><td rowspan=1 colspan=1>Magazine SizeA</td><td rowspan=1 colspan=1>Amo Capacity</td><td rowspan=1 colspan=1>0.6</td></tr><tr><td rowspan=1 colspan=1>Projectile Count Bullets/Fire</td><td rowspan=1 colspan=1>Projectile Count Bullets/Fire</td><td rowspan=1 colspan=1>1.1</td></tr></table>

Lưu ý: Các chỉ số này có thể thay đổi theo Mode chơi. Việc cân đối chi tiết sẽ được quy định trong Mecharnics từng Mode.

# 6. Công thức tính chỉ số cơ bản

(Các công thức sau sẽ áp dụng cho hệ thống Tank PvP tiêu chuẩn và được scale theo cấp hoặc Mode chơi)

1. $\mathsf { H P } = \mathsf { B a s e H P } \times \mathsf { H P \_ M u l t i p l i e r } ( \mathsf { L e v e l / M o d e } )$

2. Damage $=$ BaseDamage $\times$ WeaponModifier $\times$ CritMultiplier (nếu bắn chí mạng)

3. Reload Time $=$ BaseReload $\div$ ReloadUpgrade $\times$ ModeModifier

4. Fire Rate $=$ BaseFireRate $\times$ FireRateBoost

5. Projectile Speed $=$ BaseSpeed $\times$ UpgradeMultiplier

6. Range $=$ BaseRange $+$ RangeBonus (từ item/kỹ năng)

# 7. Phân loại Class chi tiết

# 7.1 Tank hạng nhẹ (Scout Tank)

Vai trò: Trinh sát, tập kích, chiếm mục tiêu nhanh. Chỉ số cơ bản:

• HP: 1000   
• Speed: 8 (units/s)   
• Damage (min-max): 70-100 (HP/hit)   
• Fire Rate: 2 (shots/s)   
• Projectile Speed: 20 (units/s)   
• Range: 15 (units)

▪ Chiến thuật: Đánh du kích, vòng ra sau, tạo nhiễu, lấy thông tin.

# 7.2 Tank hạng trung (Assault Tank)

▪ Vai trò: Tấn công đa năng, trung tâm đội hình.   
▪ Chỉ số cơ bản: • HP: 1200 • Speed: 6.4 (units/s) • Damage (min-max): 100-143 (HP/hit) • Fire Rate: 1.4 (shots/s) • Projectile Speed: 20 (units/s) • Range: 18 (units)

▪ Chiến thuật: Công thủ toàn diện, kiểm soát khu vực.

# 7.3 Tank hạng nặng (Heavy Tank)

Vai trò: Tuyến trước, hủy diệt, chống chịu. ▪ Chỉ số cơ bản: • HP: 1450 • Speed: 4.8 (units/s) • Damage (min-max): 203-290 (HP/hit) • Fire Rate: 1 (shots/s) • Projectile Speed: 20 (units/s) • Range: 22.5 (units)

▪ Chiến thuật: Dẫn đầu giao tranh, phòng thủ cứ điểm, chống lại Tank địch mạnh.

# 8. Tank Skills

[Combat Module] [Tank War] Skill Design Document

# Tank Passives

# 9.1 Scout Tank Passives

Note: b hưa implement

▪ Against All Odds: Khi đồng đội bị giết trong phạm vi xung quanh người chơi (10 units), nâng sức tấn công 5% với mỗi đồng đội (max 30%), resets khi chết.

▪ Like A Bat Out Of Hell: Tăng tốc độ 20% khi ở xung quanh heavy tanks.

# 9.2 Assault Tank Passives

▪ Bloodlust: Với mỗi tank địch bị phá huỷ bởi class này, $\nsucc$ 7% ATK (max 30%), resets khi chết. ▪ Domination: Nhận được ATK + 5% nguyên trận với mỗi 4 tank địch bị phá huỷ.

# 9.3 Heavy Tank Passives

▪ Intimidation: Làm tank địch xung quanh 7 units chậm 20%.   
▪ Squid: Tạo đám khói phạm vi 5 units khi dưới 30% máu, khiến team mình tàng hình, kéo dài trong 5s.

# 10. Balance và bảng tính

<table><tr><td rowspan=1 colspan=1>Chi s6</td><td rowspan=1 colspan=1>Name</td><td rowspan=1 colspan=1>Trong so thiet ké (Design Weight)</td><td rowspan=1 colspan=1>Giai thich</td></tr><tr><td rowspan=1 colspan=1>HP</td><td rowspan=1 colspan=1>Health</td><td rowspan=1 colspan=1>1.2</td><td rowspan=1 colspan=1>S6ng lau tang thoi gian gay DF</td></tr><tr><td rowspan=1 colspan=1>Speed</td><td rowspan=1 colspan=1>Movement Speed</td><td rowspan=1 colspan=1>1.5</td><td rowspan=1 colspan=1>Anh huong né tranh, kiem soa</td></tr><tr><td rowspan=1 colspan=1>Damage</td><td rowspan=1 colspan=1>Base Damage</td><td rowspan=1 colspan=1>1</td><td rowspan=1 colspan=1>Gia tri sat thuong co ban</td></tr><tr><td rowspan=1 colspan=1>Reload Time</td><td rowspan=1 colspan=1>Reload Speed</td><td rowspan=1 colspan=1>0.8</td><td rowspan=1 colspan=1>Anh huong nhung khong truc </td></tr><tr><td rowspan=1 colspan=1>Fire Rate</td><td rowspan=1 colspan=1>Shots per second</td><td rowspan=1 colspan=1>1.5</td><td rowspan=1 colspan=1>Quyet dinh tong DPS thuc te</td></tr><tr><td rowspan=1 colspan=1>Projectile Speed Velocity</td><td rowspan=1 colspan=1>Velocity</td><td rowspan=1 colspan=1>0.5</td><td rowspan=1 colspan=1>Tac dong nho, phan ung/né tr:</td></tr><tr><td rowspan=1 colspan=1>Range</td><td rowspan=1 colspan=1>Attack Range</td><td rowspan=1 colspan=1>0.7</td><td rowspan=1 colspan=1>C6 loi nhung chi 8 vai tinh huo</td></tr><tr><td rowspan=1 colspan=1>Magazine Size</td><td rowspan=1 colspan=1>Amo Capacity</td><td rowspan=1 colspan=1>0.6</td><td rowspan=1 colspan=1>Hopdan</td></tr><tr><td rowspan=1 colspan=1>Projectile Count Projectile Count</td><td rowspan=1 colspan=1>Projectile Count Projectile Count</td><td rowspan=1 colspan=1>1.1</td><td rowspan=1 colspan=1>S6 dan ban ra 1lan</td></tr></table>

# Link file tính

# 11. Điều khiển

Tank được điều khiển dùng 2 joysticks.

Joystick bắn đạn: Khi kéo joystick về 1 hướng, nòng súng tank sẽ chĩa sang hướng tương tự và bắn đạn:

◦ Đạn bắn liên tiếp khi người chơi tiếp tục kéo joystick ◦ Đạn được tiếp tục bắn khi người chơi kéo joystick sang hướng khác.

Đạn sẽ ngưng bắn khi người chơi thả joystick.

Note: Chưa kéo full joystick (chưa tới vị trí xa nhất so với vùng trung tâm), tank vẫn bắn như bình thường.

Joystick di chuyển: Joystick dùng để di chuyển, kéo hướng nào thì tank sẽ tương ứng di chuyển hướng đó.

Note: Chưa kéo full joystick (chưa tới vị trí xa nhất so với vùng trung tâm), tank vẫn sẽ di chuyển full tốc độ.

# 12. Hitbox

Hitbox của tank có các đặc điểm sau:

• Giống model tank ở chiều dài và chiều rộng (X, Z trong Unity)   
• Chiều cao (Y) không cần giống như model tank, đều chia sẻ chung cùng 1 số và có giới hạn từ 2.5-3 units, để đảm bảo đạn các class đánh trúng tank.

Để tham khảo độ lớn của tank, check file [World] [Tank War] Map Design Document

# 13. Notes

Subclasses có thể áp dụng sau này:

# Scout Tank Skills

• Overdrive: Spd boost for n seconds.   
Smoke Dash: Create a smoke trail.

Assault Tank Skills

• Armor Shift: Gain bonus health for n seconds.   
• Ricochet Round: Bullets bounce of walls.

# Heavy Tank Skills

• EMP Blast: Roots all units and yourself in an AOE.

(\*File này sẽ tiếp tục hoàn thiện sau khi nhận thêm thông tin về hệ thống vũ khí, nâng cấp item, mode PvE, AI enemy và Skin Customization.)