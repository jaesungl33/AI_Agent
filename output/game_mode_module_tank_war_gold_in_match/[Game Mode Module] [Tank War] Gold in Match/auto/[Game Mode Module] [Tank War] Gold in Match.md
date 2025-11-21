# [Game Mode Module] [Tank War] Gold in Match

THÔNG TIN VÀNG TRONG TRẬN ĐẤU

Ngày khởi tạo: 21 - 08 - 2025

Người soạn: Q Kent

Phiên bản mới nhất: v1.1

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mo ta</td><td rowspan=1 colspan=1>Ngudi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>21-08-2025</td><td rowspan=1 colspan=1>Hoan thanh file</td><td rowspan=1 colspan=1> Kent</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format laifile</td><td rowspan=1 colspan=1>O phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

1. Mục tiêu thiết kế

• Trung tính ATK/DEF: Không để ATK hưởng lợi thế vàng tuyệt đối so với DEF.

Chiến thuật rõ rệt: Người chơi không đủ vàng để max tất cả 40 cấp, buộc phải chọn build.

• Nhịp độ hấp dẫn: Vàng tăng dần đều tới cuối trận, tạo cảm giác cao trào.

• Không snowball gắt: Rubber-band nhẹ dựa trên Kill/Death để đội thua còn cơ hội.

• Điều kiện thắng đặc biệt: Bên nào đạt 20 kills trước thắng ngay.

# 2. Nguồn Gold trong trận

2.1. Gold per Second (GpS) ‒ cho cả 2 phe

$$
G p S _ { p l a y e r } ( t ) = G _ { b a s e } \times M _ { \mathrm { r a m p } } ( t ) \times M _ { \mathrm { r b } } ^ { t e a m } ( t )
$$

• G_base $= 2$ vàng/giây.

• Ramp multiplier (t giây):

$$
M _ { \mathrm { r a m p } } ( t ) = 1 + \rho \cdot \frac { t } { 2 4 0 } , \quad \rho = 0 . 3 5
$$

• Early ${ \sim } 1 . 0 \to \mathsf { M i d } \sim 1 . 1 8 \to \mathsf { E n d } \sim 1 . 3 5$ .

• Rubber-band multiplier dựa trên Kill/Death:

$\Delta K D = K D _ { t e a m } - K D _ { e n e m y } .$ Chuan hoa: $x = { \mathrm { c l a m p } } ( { \Delta K D } / { 6 } , - 1 , 1 ) ,$ He s6:

$$
M _ { \mathrm { r b } } = \left\{ \begin{array} { l l } { 1 - 0 . 1 2 \cdot x , } & { x > 0 ( \mathrm { d a n g d \tilde { a } n } ) } \\ { 1 + 0 . 1 2 \cdot | x | , } & { x < 0 ( \mathrm { d a n g t h u a } ) } \\ { 1 , } & { x = 0 } \end{array} \right.
$$

Hieu luc 15s khi $| \Delta K D | \ge 6 ,$ sau d6 danh gia lai.

Ky vong (khong rubber-band, van 240s):

$$
T \mathring { \circ } n g \ G p S / n g { \mathrm { u } \ o { \dot { \sigma } i } } \approx 5 6 4 \ v \dot { a } n g
$$

Kỳ vọng (không rubber-band, ván 240s):

Tổng GpS/người ≈ 564 Gold

# 2.2. Gold hạ gục xe địch (Kill Gold)

• Người Kill nhận: 80 vàng.   
• Người chết: không mất vàng (tránh snowball).   
• Win condition: Bên nào đạt 20 mạng thắng ngay (dừng phát vàng tức thời).   
Kỳ vọng team thắng (20 kills): $\sim 1 6 0 0$ vàng tổng ${ \langle \approx } 3 2 0 ,$ /người, phân bố theo last-hit).

# 2.3. Gold phá trụ (Objective Gold)

• Phe ATK khi phá trụ: mỗi người $\mathbf { \Gamma } + \mathbf { 4 0 }$ vàng (tổng team = 200).   
• Phe DEF khi mất trụ: nhận $+ 1 0 \%$ GpS trong 15s (không cộng dồn, chỉ refresh).

# 3. Chi phí nâng cấp (Upgrade Cost)

# 3.1. Rule nâng cấp

• Mỗi stat có 10 cấp (Level 1‒10).   
• Tổng cộng 40 cấp cho 4 stat (HP, Speed, FireRate, Damage).   
• Người chơi chỉ có thể đạt 12‒18 cấp/trận (không all-max).

# 3.2. Công thức

$$
\begin{array} { r l } { \cdot } & { { } { \mathsf { H P } } { : } C = 5 0 , g = 1 . 1 2 } \\ { \cdot } & { { } { \mathsf { S p e e d } } { : } C = 4 0 , g = 1 . 1 2 } \\ { \cdot } & { { } { \mathsf { F i r e R a t e } } { : } C = 6 0 , g = 1 . 1 5 } \\ { \cdot } & { { } { \mathsf { D a m a g e } } { : } C = 7 0 , g = 1 . 1 5 } \end{array}
$$

$$
C o s t _ { s } ( k ) = \lfloor C _ { s } \cdot g _ { s } ^ { ( k - 1 ) } \rfloor
$$

• HP: $\mathsf C = 5 0 , \mathsf g = 1 . 1 2 \mathsf C = 5 0 , \mathsf g = 1 . 1 2 \mathsf C = 5 0 , \mathsf g = 1 . 1 2$ • Speed: $\mathsf { C } = 4 0 , \mathsf { g } = 1 . 1 2 \mathsf { C } = 4 0$ , g=1.12C=40,g=1.12 • FireRate: $\mathtt { C } { = } 6 0$ ,g=1.15C $\mathtt { - 6 0 }$ , g=1.15C=60,g=1.15 • Damage: $\mathsf { C } = 7 0 , \mathsf { g } = 1 . 1 5 \mathsf { C } = 7 0$ , g=1.15C=70,g=1.15

3.3. Bảng mẫu (vàng / cấp)

<table><tr><td rowspan=1 colspan=1>cap k</td><td rowspan=1 colspan=1>HP</td><td rowspan=1 colspan=1>Speed</td><td rowspan=1 colspan=1>FireRate</td><td rowspan=1 colspan=1>Damage</td></tr><tr><td rowspan=1 colspan=1>1</td><td rowspan=1 colspan=1>50</td><td rowspan=1 colspan=1>40</td><td rowspan=1 colspan=1>60</td><td rowspan=1 colspan=1>70</td></tr><tr><td rowspan=1 colspan=1>2</td><td rowspan=1 colspan=1>56</td><td rowspan=1 colspan=1>45</td><td rowspan=1 colspan=1>69</td><td rowspan=1 colspan=1>80</td></tr><tr><td rowspan=1 colspan=1>3</td><td rowspan=1 colspan=1>63</td><td rowspan=1 colspan=1>50</td><td rowspan=1 colspan=1>79</td><td rowspan=1 colspan=1>92</td></tr><tr><td rowspan=1 colspan=1>4</td><td rowspan=1 colspan=1>71</td><td rowspan=1 colspan=1>56</td><td rowspan=1 colspan=1>91</td><td rowspan=1 colspan=1>106</td></tr><tr><td rowspan=1 colspan=1>5</td><td rowspan=1 colspan=1>79</td><td rowspan=1 colspan=1>63</td><td rowspan=1 colspan=1>105</td><td rowspan=1 colspan=1>122</td></tr><tr><td rowspan=1 colspan=1>6</td><td rowspan=1 colspan=1>89</td><td rowspan=1 colspan=1>71</td><td rowspan=1 colspan=1>121</td><td rowspan=1 colspan=1>140</td></tr><tr><td rowspan=1 colspan=1>7</td><td rowspan=1 colspan=1>100</td><td rowspan=1 colspan=1>80</td><td rowspan=1 colspan=1>139</td><td rowspan=1 colspan=1>161</td></tr><tr><td rowspan=1 colspan=1>8</td><td rowspan=1 colspan=1>112</td><td rowspan=1 colspan=1>90</td><td rowspan=1 colspan=1>160</td><td rowspan=1 colspan=1>185</td></tr><tr><td rowspan=1 colspan=1>9</td><td rowspan=1 colspan=1>126</td><td rowspan=1 colspan=1>101</td><td rowspan=1 colspan=1>184</td><td rowspan=1 colspan=1>212</td></tr><tr><td rowspan=1 colspan=1>10</td><td rowspan=1 colspan=1>141</td><td rowspan=1 colspan=1>113</td><td rowspan=1 colspan=1>212</td><td rowspan=1 colspan=1>243</td></tr></table>

Max 1 stat: HP $\sim 8 8 0$ vàng, Damage ${ \sim } 1 . 4 \mathbf { k }$ vàng $\Rightarrow$ không thể all-max.

# 4. Sanity Check (1 trận 4’)

• $\mathsf { G p S } \approx 5 6 4 / \mathsf { n g u } \mathsf { \dot { \alpha } i }$ .   
• Kill Gold (trung bình team thắng): \~320/người.   
• Tower Gold (ATK, 2 trụ): $+ 8 0$ /người. Tổng kỳ vọng/người: \~880‒960 vàng.

Đủ để mua 12‒18 cấp, build đa dạng nhưng không full 40 cấp.

# 5. Quy ước triển khai (dev-friendly) GpS mỗi giây:

GpS = G_base \* (1 + rho\*(t/240)) \* M_rb

# Kill Event:

OnKill(killer): Gold[killer] += 80 If TeamKills >= 20: EndMatch()

# Tower Event (ATK only):

OnTowerDown:

For each player in ATK: Gold += 40

DEF: GpS \*= 1.10 for 15s

# Upgrade Cost (stat s, level k):

Cost = FLOOR(C_s \* (g_s^(k-1)), 1)

# 6. Nút tuning nhanh khi test

• rho (độ dốc ramp): $0 . 3 0  0 . 4 0$ • KillerGold : $7 0  9 0$ • TowerGold_per_player : $3 5  4 5$ • kappa (KD gap): 5 ↔ 7 • rb_strength : 0.10 ↔ 0.15 • g_s (growth factor cost): $1 . 1 2  1 . 1 8$

# 7. Kết luận

Với bản này, người chơi sẽ:

• Luôn có vàng để bấm nâng liên tục (12‒18 cấp/trận).   
• Phải chọn đường nâng, không all-max.   
• Nhịp độ trận tăng đều tới cuối (GpS ramp).   
ATK/DEF cân bằng, có Rubber-band để chống snowball.