# [Multiplayer Module] [Tank War] Match Profile Design

THÔNG TIN TRẬN ĐẤU

Version: v1.3   
Người viết: phucth12 (phucth12)   
Ngày tạo: 03 - 07 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mota</td><td rowspan=1 colspan=1>Nguoi viet</td><td rowspan=1 colspan=1>Ngudireview</td><td rowspan=1 colspan=1>Duyet？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>04-07-2025</td><td rowspan=1 colspan=1>Hoan thanh file</td><td rowspan=1 colspan=1>®phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>15-07-2025</td><td rowspan=1 colspan=1> Update limit cac value</td><td rowspan=1 colspan=1> phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.2</td><td rowspan=1 colspan=1>25-07-2025</td><td rowspan=1 colspan=1>Update thong só</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.3</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format laifile+Chuän bi phänDeathmatch</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mục đích thiết kế

Phân định rõ ràng các thông tin nhận được từ bên ngoài trận vào và trong quá trình diễn ra trận đấu.

Đảm bảo thiết kế có những đặc điểm sau:

• Giữ các thông số đồng nhất ở các loại mode game khác nhau • Những thông số phải có định dạng rõ ràng

# 2. Mục tiêu tài liệu

Tài liệu được dùng để giúp đội Dev xây dựng thông tin trận đấu phù hợp cho các loại gamemodes khác nhau.

# 3. Tổng quan tài liệu

4. Các yếu tố trong 1 match

a. Thông số của user b. Thông số của trận đấu

5. Các giá trị theo từng gamemodes

a. Base capture b. Deathmatch

# 4 Các yếu tố trong 1 match

a. Thông số của user

<table><tr><td colspan="1" rowspan="1">Thanhphan</td><td colspan="1" rowspan="1">Dinhdang</td><td colspan="1" rowspan="1"> Muc dich</td><td colspan="1" rowspan="1">Note</td></tr><tr><td colspan="1" rowspan="1">TeamID</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1">Trän däu dugc chia thanh nhiéu team khácnhau Nhän dang qua ten va mau säc</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Playerlndex</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> Xac dinh vi tri cua cac nhan vat trong team</td><td colspan="1" rowspan="1">Moi player c6 1index rieng</td></tr><tr><td colspan="1" rowspan="1">PlayerName</td><td colspan="1" rowspan="1">Chuoi kitu</td><td colspan="1" rowspan="1"> Nhän dang cac players qua qua ten dugc hiénthi trong tran dau</td><td colspan="1" rowspan="1">Tham khao UserProfile Design:[Tank War] UserProfile Design</td></tr><tr><td colspan="1" rowspan="1">AvatarID</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> Mä so cua avatar nguoi choi</td><td colspan="1" rowspan="1">Tham khao ItemList Design: 目</td></tr><tr><td colspan="1" rowspan="1">SkillIDs</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> Ma cua cac skill</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">TankID</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> Thé hien mä so class tank Mi player có 1tank gän lien voi ho Nhän dang qua model tank va ten tank</td><td colspan="1" rowspan="1">Ngudi chdichiduoc phép thay d6iTank 8 dau trandau,trong pickscreen</td></tr><tr><td colspan="1" rowspan="1">Gold</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> S6 tien tung player s8 huu trong quä trinhchoi 1 match</td><td colspan="1" rowspan="4">Thong s6 ca nhan theo tung player</td></tr><tr><td colspan="1" rowspan="1">Kill</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> S6 tank dich dä phä huy</td></tr><tr><td colspan="1" rowspan="1"> KillStreak</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> S6lugng gietlien tiep cua tuing player</td></tr><tr><td colspan="1" rowspan="1">Death</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> Chi so sδ lan chet cua player</td></tr><tr><td colspan="1" rowspan="1">HitPoints(HP)</td><td colspan="1" rowspan="1">S</td><td colspan="1" rowspan="1"> Mäu hien taicua tank</td><td colspan="1" rowspan="10">Thóng so ca nhantheo tung tank</td></tr><tr><td colspan="1" rowspan="1">MaxHitPoints</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> Mau toi da cua tank</td></tr><tr><td colspan="1" rowspan="1">MovementSpeed</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1">Toc do cua tank</td></tr><tr><td colspan="1" rowspan="1">Damage</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> Tan cong cua tank</td></tr><tr><td colspan="1" rowspan="1">ReloadTime</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1">Thoi gian nap dan cua tank</td></tr><tr><td colspan="1" rowspan="1">FireRate</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1">Toc do ban cua tank</td></tr><tr><td colspan="1" rowspan="1">ProjectileSpeed</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> Toc do dan bay cua tank</td></tr><tr><td colspan="1" rowspan="1">Range</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1">Pham vi ban cua tank</td></tr><tr><td colspan="1" rowspan="1">MagazineSize</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1"> Kich co cua tank</td></tr><tr><td colspan="1" rowspan="1">ProjectileCount</td><td colspan="1" rowspan="1">S6</td><td colspan="1" rowspan="1">S6 dan ban ra trong 1lan bän</td></tr><tr><td colspan="1" rowspan="1">Fime ptayed</td><td colspan="1" rowspan="1">sithapphan</td><td colspan="1" rowspan="1"> Th8igian choi</td><td colspan="1" rowspan="1">Duge bieu dierr trong game qua s6tunhien,don vitaiy</td></tr></table>

# b. Thông số của trận đấu

<table><tr><td rowspan=1 colspan=1> Thanhphan</td><td rowspan=1 colspan=1>Dinhdang</td><td rowspan=1 colspan=1> Muc dich</td><td rowspan=1 colspan=1>Gia trimacdinh</td><td rowspan=1 colspan=1>Limit</td><td rowspan=1 colspan=1>Note</td></tr><tr><td rowspan=1 colspan=1>MapID</td><td rowspan=1 colspan=1>S6</td><td rowspan=1 colspan=1> Mä so map users sé chditren</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>ModelD</td><td rowspan=1 colspan=1>S6</td><td rowspan=1 colspan=1> Mä so gamemode cuatran dau</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1> Match Type</td><td rowspan=1 colspan=1>Hang s6</td><td rowspan=1 colspan=1>Chi dinh tran däu la däu rank hay thuong</td><td rowspan=1 colspan=1>Thuong:0Rank: 1</td><td rowspan=1 colspan=1>1</td><td rowspan=1 colspan=1>Chi khi däu rankmoi can quantam toi gia triRank W/L</td></tr><tr><td rowspan=1 colspan=1> Max players</td><td rowspan=1 colspan=1>S6</td><td rowspan=1 colspan=1> Xac dinh so nguoi cänthiet dé vao 1 man chdi</td><td rowspan=1 colspan=1>10</td><td rowspan=1 colspan=1>10</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Maximumtime</td><td rowspan=1 colspan=1>Hang s6</td><td rowspan=1 colspan=1>Thαi gian chdi toi da Khi thoi gian user thäytrong game dat toinguong nay thi game séket thuc</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>Tinh theo 1f= 1s</td></tr></table>

Khi định dạng là Hằng số, giá trị không lệch đi khỏi giá trị mặc định

# 5. Các giá trị theo từng gamemodes

# a. Base capture

<table><tr><td colspan="1" rowspan="1">Thänh phän</td><td colspan="1" rowspan="1"></td><td colspan="1" rowspan="1">Gia tri</td></tr><tr><td colspan="1" rowspan="2">Team ID</td><td colspan="1" rowspan="1">Doicong:</td><td colspan="1" rowspan="1">0</td></tr><tr><td colspan="1" rowspan="1">Doi thu:</td><td colspan="1" rowspan="1">1</td></tr><tr><td colspan="1" rowspan="1"></td><td></td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Map ID</td><td colspan="1" rowspan="1">Test_Map (Tén sé dugc thay thé sau khi hoan tätcäc yéu tó choi khác)</td><td colspan="1" rowspan="1">0</td></tr><tr><td colspan="1" rowspan="1">Maximum time</td><td colspan="1" rowspan="1">Thoi gian choi toi da</td><td colspan="1" rowspan="1">300 giay= 5 phut</td></tr></table>

# b. Deathmatch

<table><tr><td rowspan=1 colspan=1>Thänh phän</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>Gia tri</td></tr><tr><td rowspan=2 colspan=1>Team ID</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>0</td></tr><tr><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>1</td></tr><tr><td rowspan=1 colspan=1>Map ID</td><td rowspan=1 colspan=1>Test_Map (Tén sé dugc thay thé sau khi hoan tatcac yéu tä choi khac)</td><td rowspan=1 colspan=1>0</td></tr><tr><td rowspan=1 colspan=1>Maximum time</td><td rowspan=1 colspan=1>Thoi gian choi toi da</td><td rowspan=1 colspan=1>300 giay= 5 phut</td></tr></table>

# 6. Notes

Deathmatch profile sẽ được cập nhật sau