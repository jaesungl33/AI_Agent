# [Combat Module] [Tank War] Outpost Design - Base Capture Mode

THÔNG TIN THIẾT KẾ OUTPOST

# Version: v1.5

Người viết: phucth12

Ngày cập nhật: 17 - 09 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mota</td><td rowspan=1 colspan=1>Nguoiviet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyet？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>29-07-2025</td><td rowspan=1 colspan=1>Hoan thanh file</td><td rowspan=1 colspan=1>P phucth:</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>31-07-2025</td><td rowspan=1 colspan=1>Cäp nhät thong sδ theo dung cong thuc </td><td rowspan=1 colspan=1>P phucth:</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.2</td><td rowspan=1 colspan=1>04-08-2025</td><td rowspan=1 colspan=1>Balance lai outpost</td><td rowspan=1 colspan=1> phucth:</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.3</td><td rowspan=1 colspan=1>06-08-2025</td><td rowspan=1 colspan=1>G@p sheet bäng tinh</td><td rowspan=1 colspan=1>Ophucth:</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.4</td><td rowspan=1 colspan=1>04-09-2025</td><td rowspan=1 colspan=1> Cäp nhät logic tim muc tieu cua outpost (Giuatarget va vat can)</td><td rowspan=1 colspan=1>P phucth:</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.5</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format laifile</td><td rowspan=1 colspan=1> phucth:</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mục đích thiết kế

Tạo outpost làm mục tiêu chơi chính của gamemode Base Capture và thiết kế sao cho outposts mang cảm giác nguy hiểm nhưng cũng phù hợp với khung thời gian chơi đã đặt ra.

# 2. Mục tiêu thiết kế

• Dễ dàng cân bằng theo Mode chơi.   
• Dễ mở rộng cho skin, nâng cấp, tính năng trong tương lai.   
• Tất cả chỉ số và đơn vị đều được quy đổi theo chuẩn Unity: ◦ Fire Rate: shots/second ◦ Fire Range: units (1 unit Unity $\approx 1$ mét)

Note: Täm bän cua outpost lä hinh tron,pham vi bän tinh theo bän kinh

# 3. Mục tiêu & tổng quan tài liệu

Tài liệu được dùng để giúp đội Art & Dev hình dung rõ ràng hơn về design của các Outposts.

4. Chỉ số chung   
5. Công thức tính chỉ số   
6. Phân loại cấp của Outpost a. Outpost 1 b. Outpost 2 c. Outpost 3

# 4. Chỉ số chung

<table><tr><td colspan="1" rowspan="1">Chi so</td><td colspan="1" rowspan="1">Ten</td></tr><tr><td colspan="1" rowspan="1">HP</td><td colspan="1" rowspan="1">Health</td></tr><tr><td colspan="1" rowspan="1">Damage</td><td colspan="1" rowspan="1">Base Damage</td></tr><tr><td colspan="1" rowspan="1">Fire Rate</td><td colspan="1" rowspan="1">Shots/Second</td></tr><tr><td colspan="1" rowspan="1">Projectile Speed</td><td colspan="1" rowspan="1">Bullet Velocity</td></tr><tr><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Range</td><td colspan="1" rowspan="1">Attack Range</td></tr><tr><td colspan="1" rowspan="1">Projectile Count</td><td colspan="1" rowspan="1">Bullets/Fire</td></tr><tr><td colspan="1" rowspan="1">Tier</td><td colspan="1" rowspan="1">Type Of Outpost</td></tr></table>

# 5. Công thức tính chỉ số

(Các công thức sau sẽ áp dụng cho hệ thống Tank PvP tiêu chuẩn và được scale cấp Outpost)

• HP $=$ BaseTankHP \* OutpostMultiplier • Damage $=$ BaseTankDamage \* OutpostMultiplier • Fire Rate $=$ BaseFireRate \* OutpostMultiplier • Projectile Speed $=$ BaseTankFireRate \* OutpostMultiplier • Range $=$ BaseTankRange \* OutpostMultiplier

# 6. Phân loại cấp của Outpost

Note: Déhieu vi sao lai chon cac thöng so nay, tham khao file balancing ＆ phän 8 Note 2: Range trong nay dugc hiéu la ban kinh

# a. Outpost 1

• Vai trò: Trọng điểm đầu tiên, không quá khó để chiếm và khó để thủ.

• Đặc điểm: Đánh chậm, dam thấp.

• Chỉ số cơ bản:

◦ HP: 2140   
◦ Damage: 112 - 160 (HP/hit)   
◦ Fire Rate: 1 (shots/s)   
◦ Projectile Speed: 45 (units/s)   
◦ Range: 20 (units)

# b. Outpost 2

Vai trò: Trọng điểm thứ hai, tương đối dễ để chiếm và tương đối dễ để thủ.

◦ Đặc điểm: Đánh nhanh, dam thấp.

◦ Chỉ số cơ bản:

HP: 2570 ▪ Damage: 120 - 171 (HP/hit) ▪ Fire Rate: 1.5 (shots/s) Projectile Speed: 45 (units/s) ▪ Range: 20 (units)

# c. Outpost 3

• Vai trò: Trọng điểm cuối, khó để chiếm và dễ để thủ.

• Đặc điểm: Đánh nhanh, dam cao.

• Chỉ số cơ bản:

◦ HP: 3080   
◦ Damage: 134 - 192 (HP/hit)   
◦ Fire Rate: 2 (shots/s)   
◦ Projectile Speed: 45 (units/s)   
◦ Range: 20 (units)

# 7. Logic

# a. Outpost target logic

<table><tr><td colspan="1" rowspan="1">Case</td><td colspan="1" rowspan="1">Behaviour</td><td colspan="1" rowspan="1">Note</td></tr><tr><td colspan="1" rowspan="1">Khi có ké dich (pheATK) vao vung bäncua outpost</td><td colspan="1" rowspan="1">·  Chua c6 target:0  Tank khöng bi khuat sau vät can: Läy tankdó lam target0  Tank bi khuat sau vät can: Khong thay doitarget</td><td colspan="1" rowspan="1">Thdi gian scantarget tuongduong voi base firerate(Scan target time =0.5s)</td></tr><tr><td></td><td>Dä có target: Khong thay doi target ·</td><td></td></tr><tr><td>Khi target thoat vung bän cua outpost</td><td>C6 ké dich khäc trong vung bän: Lay tank gän ： nhät lam target moi Khong c6 ké dich khác trong vung ban: Khong · lam gi</td><td></td></tr><tr><td> Khi bän target</td><td>Nong sung duoi theo target</td><td></td></tr><tr><td>Khi target nam 8 dang sau cac vat can</td><td> Huy target, tim target khác</td><td></td></tr></table>

# b. Bullet logic

Đạn của outpost sẽ có những tính chất sau:

◦ Bắn từ đỉnh tháp xuống.   
◦ Đạn sẽ chọn vị trí gần đây nhất của target để di chuyển tới, không có homing (dẫn đường) tới target.   
◦ Đạn chỉ tác động tới 1 tank, không AOE.

# 8. Balance và bảng tính

<table><tr><td rowspan=1 colspan=1>Chi so</td><td rowspan=1 colspan=1>Ten</td><td rowspan=1 colspan=1>Design Weight</td><td rowspan=1 colspan=1>Giai thich</td></tr><tr><td rowspan=1 colspan=1>HP</td><td rowspan=1 colspan=1>Health</td><td rowspan=1 colspan=1>1.2</td><td rowspan=1 colspan=1>Song lau tang thoi gian gay DPS</td></tr><tr><td rowspan=1 colspan=1>Damage</td><td rowspan=1 colspan=1>Base Damage</td><td rowspan=1 colspan=1>1</td><td rowspan=1 colspan=1>Gia tri sat thuong co ban</td></tr><tr><td rowspan=1 colspan=1>Fire Rate</td><td rowspan=1 colspan=1>Shots/Second</td><td rowspan=1 colspan=1>0.5</td><td rowspan=1 colspan=1>Quyet dinh tong DPS thuc te</td></tr><tr><td rowspan=1 colspan=1>Projectile Speed</td><td rowspan=1 colspan=1>Bullet Velocity</td><td rowspan=1 colspan=1>0.5</td><td rowspan=1 colspan=1>Tac dong nho, phan ung/né tránh</td></tr><tr><td rowspan=1 colspan=1>Range</td><td rowspan=1 colspan=1>Attack Range</td><td rowspan=1 colspan=1>1</td><td rowspan=1 colspan=1>Quyet dinh tong DPS thuc te</td></tr><tr><td rowspan=1 colspan=1>Projectile Count</td><td rowspan=1 colspan=1>Bullets/Fire</td><td rowspan=1 colspan=1>1.6</td><td rowspan=1 colspan=1>Tac dong toi DPS va so luong tank bitan cong</td></tr><tr><td rowspan=1 colspan=1>Tier</td><td rowspan=1 colspan=1>Type Of Outpost</td><td rowspan=1 colspan=1>1.2-1.4</td><td rowspan=1 colspan=1>Truc tiep anh huong toi cac thong sö trén</td></tr></table>

# [Tank War] Stats & Balancing

( \* F i l e  n à y  s ẽ  ti ế p  t ụ c  h o à n  t h iệ n  s a u  k h i n h ậ n  t h ê m t h ô n g  t i n  v ề  m o d e  c h ơ i m ớ i v à  s k i n c u s t o m i z a t i o n . )