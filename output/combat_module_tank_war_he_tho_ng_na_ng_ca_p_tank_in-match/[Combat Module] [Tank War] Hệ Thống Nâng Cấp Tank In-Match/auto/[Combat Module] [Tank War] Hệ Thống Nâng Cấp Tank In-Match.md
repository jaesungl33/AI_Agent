# [Combat Module] [Tank War] Hệ Thống Nâng Cấp Tank In-Match

HỆ THỐNG NÂNG CẤP XE TANK IN-MATCH

# Phiên bản: v1.2

Người tạo file: Kent

Ngày cập nhật: 22 - 09 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mota</td><td rowspan=1 colspan=1>Nguoi viet</td><td rowspan=1 colspan=1>Nguireview</td><td rowspan=1 colspan=1>Duyét</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>29-06-2025</td><td rowspan=1 colspan=1>Tao file</td><td rowspan=1 colspan=1> Kent</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Forrmat lai file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.2</td><td rowspan=1 colspan=1>22-09-2025</td><td rowspan=1 colspan=1> Dieu chinh lai chi so va so lan nang cap</td><td rowspan=1 colspan=1> Kent</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Tổng quan hệ thống

• Hệ thống nâng cấp xe Tank trong trận giúp người chơi tối ưu hóa chiến thuật theo diễn biến trận đấu.

• Mỗi trận có thời lượng tối đa: 4 phút.

• Mỗi xe Tank được phép nâng cấp tối đa 12 lần cho 4 Stats - Mỗi Stat sẽ có 3 level trong màn chơi.

• Sau khi trận kết thúc, toàn bộ chỉ số trở về mặc định ban đầu.

• Hệ thống nâng cấp trong trận tạo ra chiều sâu chiến thuật mỗi trận, giúp người chơi thích nghi với nhiều tình huống và phong cách Tank khác nhau.

# 2. Mục lục

3. Các thuộc tính được phép nâng cấp   
4. Nguyên tắc thiết kế   
5. Bảng tăng chỉ số theo lần nâng 5.1 Cấu trúc tăng mỗi lần nâng cấp (Unit $= \%$ so với base) Scout (cơ động ‒ dồn nhịp bắn) Assault (toàn diện ‒ nhịp giao tranh chính) Heavy (chống chịu ‒ giữ tuyến)

6. Quy tắc stack Ví dụ:

7. Ghi chú thiết kế

# 3. Các thuộc tính được phép nâng cấp

Chỉ có 4 chỉ số được phép nâng cấp trong trận:

• HP $=$ Health Points (Tăng lượng máu tối đa) • SPD $=$ Speed (Tăng tốc độ di chuyển) • DMG $=$ Damage (Tăng sát thương cơ bản) • FR $=$ Fire Rate (Tăng tốc độ bắn)

# 4. Nguyên tắc thiết kế

• Mỗi loại Tank sẽ nhận được lợi ích khác nhau khi nâng cấp từng thuộc tính.   
• Trọng số mỗi lần nâng sẽ không đồng đều, nhưng phải đảm bảo WI (Weighted Index) sau nâng đứng trong khung cho phép.   
• Hiệu quả mỗi lần nâng sẽ tăng tuyến tính, nhưng theo quy tắc “lợi ích giảm dần” (diminishing returns).

# 5. Bảng tăng chỉ số theo lần nâng

# 5.1 Cấu trúc tăng mỗi lần nâng cấp (Unit $= \%$ so với base)

Scout (cơ động ‒ dồn nhịp bắn)

<table><tr><td rowspan=1 colspan=1> Stat</td><td rowspan=1 colspan=1>L1 (%)</td><td rowspan=1 colspan=1>L2 (%)</td><td rowspan=1 colspan=1>L3 (%)</td><td rowspan=1 colspan=1>Tong cong</td></tr><tr><td rowspan=1 colspan=1>HP</td><td rowspan=1 colspan=1>5</td><td rowspan=1 colspan=1>4</td><td rowspan=1 colspan=1>3</td><td rowspan=1 colspan=1>12.50%</td></tr><tr><td rowspan=1 colspan=1>SPD</td><td rowspan=1 colspan=1>9</td><td rowspan=1 colspan=1>6</td><td rowspan=1 colspan=1>4</td><td rowspan=1 colspan=1>20.20%</td></tr><tr><td rowspan=1 colspan=1>DMG</td><td rowspan=1 colspan=1>7</td><td rowspan=1 colspan=1>6</td><td rowspan=1 colspan=1>5</td><td rowspan=1 colspan=1>19.10%</td></tr><tr><td rowspan=1 colspan=1>FR</td><td rowspan=1 colspan=1>10</td><td rowspan=1 colspan=1>7</td><td rowspan=1 colspan=1>5</td><td rowspan=1 colspan=1>23.60%</td></tr></table>

• DPS multiplier $( \mathsf { D M G } \times \mathsf { F R } )$ khi max: $\times 1 . 4 7 2 $ đóng vai trò như xạ thủ ◦ Ví dụ cụ thể (DPS):

Final Stat Multiplier $= ( 1 + P 1 ) \times ( 1 + P 2 ) \times ( 1 + P 3 )$   
▪ Với Scout ‒ DMG: $( 1 + 0 . 0 7 ) \times ( 1 + 0 . 0 6 ) \times ( 1 + 0 . 0 5 ) { = } 1 . 1 9 1 ( k h o 3 n g 1 9 . 1 \% )$ Với Scout ‒ FR: $( 1 + 0 . 1 0 ) \times ( 1 + 0 . 0 7 ) \times ( 1 + 0 . 0 5 ) { = } 1 . 2 3 6 ( k h o 3 n g 2 3 . 6 ^ { \circ } \theta )$   
▪ Nhân hai multiplier lại để ra DPS multiplier • DPS Multiplier $=$ DMG Multiplier $\times F R$ Multiplier • Với Scout: $\scriptstyle 1 . 1 9 1 \times 1 . 2 3 6 = 1 . 4 7 2 1$ (1.472 nghĩa là DPS khi max upgrade sẽ ca hon ${ \sim } 4 7 . 2 \%$ so voi base)

• SPD cao để bảo toàn “hit-and-run”, HP chỉ đủ chống sốc sát thương.

# Assault (toàn diện ‒ nhịp giao tranh chính)

<table><tr><td rowspan=1 colspan=1>Stat</td><td rowspan=1 colspan=1>L1(%)</td><td rowspan=1 colspan=1>L2 (%)</td><td rowspan=1 colspan=1>L3 (%)</td><td rowspan=1 colspan=1>Tong cong</td></tr><tr><td rowspan=1 colspan=1>HP</td><td rowspan=1 colspan=1>8</td><td rowspan=1 colspan=1>7</td><td rowspan=1 colspan=1>5</td><td rowspan=1 colspan=1>21.30%</td></tr><tr><td rowspan=1 colspan=1>SPD</td><td rowspan=1 colspan=1>6</td><td rowspan=1 colspan=1>4</td><td rowspan=1 colspan=1>3</td><td rowspan=1 colspan=1>13.50%</td></tr><tr><td rowspan=1 colspan=1>DMG</td><td rowspan=1 colspan=1>8</td><td rowspan=1 colspan=1>7</td><td rowspan=1 colspan=1>6</td><td rowspan=1 colspan=1>22.50%</td></tr><tr><td rowspan=1 colspan=1>FR</td><td rowspan=1 colspan=1>7</td><td rowspan=1 colspan=1>6</td><td rowspan=1 colspan=1>5</td><td rowspan=1 colspan=1>19.10%</td></tr></table>

• DPS multiplier khi max: ×1.459 — bám sát Scout nhưng bền hơn (HP cao hơn) để giữ vai “carry mid-game”

# Heavy (chống chịu ‒ giữ tuyến)

<table><tr><td rowspan=1 colspan=1>Stat</td><td rowspan=1 colspan=1>L1 (%)</td><td rowspan=1 colspan=1>L2 (%)</td><td rowspan=1 colspan=1>L3 (%)</td><td rowspan=1 colspan=1>Tong cong</td></tr><tr><td rowspan=1 colspan=1>HP</td><td rowspan=1 colspan=1>12</td><td rowspan=1 colspan=1>9</td><td rowspan=1 colspan=1>6</td><td rowspan=1 colspan=1>29.40%</td></tr><tr><td rowspan=1 colspan=1>SPD</td><td rowspan=1 colspan=1>4</td><td rowspan=1 colspan=1>3</td><td rowspan=1 colspan=1>2</td><td rowspan=1 colspan=1>9.30%</td></tr><tr><td rowspan=1 colspan=1>DMG</td><td rowspan=1 colspan=1>7</td><td rowspan=1 colspan=1>6</td><td rowspan=1 colspan=1>5</td><td rowspan=1 colspan=1>19.10%</td></tr><tr><td rowspan=1 colspan=1>FR</td><td rowspan=1 colspan=1>6</td><td rowspan=1 colspan=1>5</td><td rowspan=1 colspan=1>4</td><td rowspan=1 colspan=1>15.80%</td></tr></table>

• DPS multiplier khi max: $\times 1 . 3 7 9$ — thấp hơn 2 class kia để không lấn tuyến, bù lại EHP cao nhất $\left( \mathsf { H P } + 2 9 . 4 \% \right)$ .

# 6. Quy tắc stack

• Mỗi lần nâng cấp sẽ cộng dồn (stack) với lần trước đó.

• Công thức tính tổng hiệu ứng cộng dồn:

Trong đó:

• P1, P2, ..., Pn: phần trăm tăng của từng lần nâng cấp. • n: số lần nâng (tối đa 10)

Kết quả là tỷ lệ phần trăm tổng cộng được cộng dồn theo cấp số nhân, phản ánh đúng đặc điểm giảm dần lợi ích.

# Ví dụ:

• Giả sử Scout Tank nâng Fire Rate theo các mốc:

◦ Lần 1: $+ 1 0 \%$ ◦ Lần 2: +7% ◦ Lần 3: +5%

• Tính:

◦ Total Bonus $( \% ) = ( 1 + 0 . 1 ) \times ( 1 + 0 . 0 7 ) \times ( 1 + 0 . 0 5 ) \cdot 1$ $= 1 . 1 \times 1 . 0 7 \times 1 . 0 5 - 1$ $\approx 1 . 2 3 5 8 5 - 1 = 0 . 2 3 5 = 2 3 . 5 8 5 \% \sim 2 3 . 6 \%$

◦ Sau 3 lần nâng, Fire Rate tăng tổng cộng khoảng $2 3 . 6 \%$ , chứ không phải $1 0 \% + 7 \% + 5 \%$ $= 2 2 \%$ như cộng tuyến tính.

# 7. Ghi chú thiết kế

• Bảng nâng cấp sẽ được áp dụng linh hoạt trong Mode PvE/PvP.

• Tối ưu hóa trải nghiệm của người chơi bằng việc chọn đồng bộ với vai trò Class Tank.

• Phần thưởng (vàng, XP trong trận) sẽ dùng để mở khóa các lần nâng.

• Hệ thống có thể mở rộng sau này (Skill, Aura, Passive )