# [Progression Module] [Tank Wars] Artifact Enhancement

# Artifact Enhancement System

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>M6ta</td><td rowspan=1 colspan=1>Ngudi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>30-10-2025</td><td rowspan=1 colspan=1>Taofile</td><td rowspan=1 colspan=1> Kent</td><td rowspan=1 colspan=1>0Tomato</td><td rowspan=1 colspan=1>☑</td></tr><tr><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr></table>

# 1. Mục tiêu hệ thống (Purpose)

• Hệ thống Enhancement (Cường hoá) cho phép người chơi tăng sức mạnh của Artifact thông qua việc nâng cấp từ $\mathsf { L } \mathsf { v } . 1 \to \mathsf { L } \mathsf { v } . 2 0$ .

• Mục tiêu:

◦ Tạo progression loop ngắn ‒ trung hạn (dễ grind hơn Fusion).   
◦ Duy trì cảm giác RNG nhưng có kiểm soát — mỗi lần nâng cấp đều hồi hộp.   
◦ Giữ tính giá trị cho Artifact hiếm: chỉ số và tiềm năng tăng theo độ hiếm.

# 2. Core Mechanic

• Mỗi Artifact có thể nâng cấp tối đa Lv.20.

• Tỉ lệ thành công: $1 0 0 \% $ không fail.

• Tuy nhiên, chỉ số tăng là RNG ‒ random sub-stat tăng, hoặc tăng giá trị random cho sub-stat đã có.

• Mỗi 5 cấp (Lv.5, 10, 15, 20) sẽ có một “Enhancement Jump” (sub-stat roll).

◦ Nếu Artifact $\mathsf { c } \acute { 0 } < 2$ sub-stat thêm chỉ số mới.   
◦ Nếu đủ 2 sub-stat random 1 chỉ số tăng giá trị.

# 3. Loại chỉ số có thể tăng (Stat Pool)

# Main Stat:

• Không thay đổi loại thuộc tính khi nâng cấp - chỉ tăng giá trị cố định theo cấp.

# Sub Stat Pool:

• ATK (%) / SHIELD (Points) / HP (%) / CRIT Rate (%) / CRIT DMG (%) / MOVE SPD (%) / HP Regen (point/ s) / SHIELD Regen (points/ s)

# 4. Cấu trúc cấp độ & chỉ số (Level Structure)

<table><tr><td rowspan=1 colspan=1>Cap do</td><td rowspan=1 colspan=1>Tac dong</td><td rowspan=1 colspan=1>Ghi chu</td></tr><tr><td rowspan=1 colspan=1>Lv.1-2</td><td rowspan=1 colspan=1>Base stats</td><td rowspan=1 colspan=1>Khóng tang sub-stat</td></tr><tr><td rowspan=1 colspan=1>Lv.5</td><td rowspan=1 colspan=1>+1 random sub-stat hoäc täng l sub-stat có sän</td><td rowspan=1 colspan=1>Jump 1</td></tr><tr><td rowspan=1 colspan=1>Lv.10</td><td rowspan=1 colspan=1>+1 hoäc tang sub-stat</td><td rowspan=1 colspan=1>Jump 2</td></tr><tr><td rowspan=1 colspan=1>Lv.15</td><td rowspan=1 colspan=1> Tang 1 sub-stat</td><td rowspan=1 colspan=1>Jump 3</td></tr><tr><td rowspan=1 colspan=1>Lv.20</td><td rowspan=1 colspan=1>Täng 1 sub-stat</td><td rowspan=1 colspan=1>Jump 4</td></tr></table>

# Link: Artifact Enhancement Table

# 5. Chi phí nâng cấp (Upgrade Cost)

• Dùng Coin và Core Dust (currency grind từ PvE / PvP / Daily Mission). • Chi phí Core Dust tăng theo cấp độ (theo dạng exponential curve). • Chi phí Coin giữ nguyên cho mỗi lần Enhancement là 1,000 Coin

<table><tr><td rowspan=1 colspan=1>cap do</td><td rowspan=1 colspan=1>Chi phi (Core Dust)</td><td rowspan=1 colspan=1>Chi phi (Coin)</td></tr><tr><td rowspan=1 colspan=1>1-5</td><td rowspan=1 colspan=1>1,000 Core Dust / lv</td><td rowspan=1 colspan=1>4000</td></tr><tr><td rowspan=1 colspan=1>6-10</td><td rowspan=1 colspan=1>2,000 Core Dust /lv</td><td rowspan=1 colspan=1>5000</td></tr><tr><td rowspan=1 colspan=1>11-15</td><td rowspan=1 colspan=1>3,000 Core Dust / lv</td><td rowspan=1 colspan=1>5000</td></tr><tr><td rowspan=1 colspan=1>16-20</td><td rowspan=1 colspan=1>5,000 Core Dust / lv</td><td rowspan=1 colspan=1>5000</td></tr></table>

$\vartriangle { \underline { { \mathbf { \delta \pi } } } } > \mathbf { \tilde { \pi } }$ Tổng chi phí là 54,000 Core Dust và 19,000 Coin cho 1 Artifact từ $\mathsf { L v } . 1 \to \mathsf { L v } . 2 0$ .

(Game Economy sẽ fine-tune sau theo player curve)

# 6. Overclock Bonus (Lv.20) (hold lại)

Khi đạt Lv.20 cho cả 5 Artifact cùng bộ và cùng hiệu ứng Overclock Bonus, Artifact sẽ kích hoạt bonus đặc biệt theo độ hiếm:

<table><tr><td rowspan=1 colspan=1>D hiem</td><td rowspan=1 colspan=1>Overclock Bonus</td><td rowspan=1 colspan=1>Mota</td></tr><tr><td rowspan=1 colspan=1>Epic</td><td rowspan=1 colspan=1>+5% hieu ung bó</td><td rowspan=1 colspan=1>Tang hieu qua thuoc tinh bó</td></tr><tr><td rowspan=1 colspan=1>Legendary</td><td rowspan=1 colspan=1>+10% hiéu üng bó hoäc +5% main stat cho xe Tank dang su dung</td><td rowspan=1 colspan=1>Random 1 trong 2</td></tr><tr><td rowspan=1 colspan=1>Mythic</td><td rowspan=1 colspan=1>+15% hieu ung bó hoäc 8% main stat cho xe Tank dang su dung</td><td rowspan=1 colspan=1>Random 1trong 2</td></tr></table>

# 7. Trải nghiệm người chơi (Player Experience)

Cảm xúc khi nâng cấp: "Roll hay hên xui?" — người chơi hồi hộp chờ xem sub-stat nào nhảy.

Cảm giác “tích lũy được tiến trình” vì $1 0 0 \%$ thành công, nhưng mỗi jump vẫn tạo bất ngờ.

Với Artifact hiếm, mỗi lần $+ 3$ cấp là một lần adrenaline hit dopamine loop cực mạnh.

• Người chơi có thể “đánh cược” xem roll ra SPD hay CRIT Rate để chase build hoàn hảo.

# 8. Định hướng cân bằng (Balance Direction)

• RNG có kiểm soát: $100 \%$ thành công không frustrate, nhưng vẫn ngẫu nhiên sub-stat để duy trì replay value.

Không pay-to-win: nguyên liệu có thể kiếm qua gameplay, không bị khoá premium.

Khuyến khích build đa dạng: sub-stat ngẫu nhiên tạo build bất ngờ meta thay đổi liên tục.

• Tăng giá trị cho Legendary/Mythic: mỗi độ hiếm đều có cảm giác tăng trưởng rõ ràng.

# 9. Hành trình progression (Loop)

• Nhận Artifact (thông qua drop, craft hoặc fusion).   
• Dùng Coin $^ +$ Core Dust để nâng cấp.   
• Mỗi 3 cấp roll sub-stat (RNG).   
• Tới $\mathsf { L v } . 2 0 \to \mathsf { m } \overset { \circ } { \partial }$ Overclock Bonus.   
• Tiếp tục Fusion hoặc mix Artifact khác để hoàn thiện build nếu chưa nhận được Artifact có các thuộc tính sau Enhancement ưng ý.

# 10. UX / UI Note

• Hiển thị cấp độ 1‒20 ở góc phải Artifact.   
• Hiệu ứng “stat roll” riêng cho từng mốc (animation highlight sub-stat tăng).   
• Gợi ý nhỏ trên UI: “Enhance x3” ‒ thao tác nhanh qua từng milestone.   
• Tooltip preview: cho phép xem pool sub-stat có thể tăng trước khi nâng cấp.

# 11. Key Design Intent

• Đảm bảo player luôn cảm thấy tiến bộ nhưng vẫn có “độ hồi hộp dễ chịu”.   
• Tạo nền cho Artifact Meta Build ‒ nơi mỗi người chơi có thể sở hữu Artifact ngẫu nhiên nhưng độc nhất.   
• Giữ thành công luôn $1 0 0 \%$ , nhưng may mắn mới quyết định đỉnh cao.   
• Là loop daily‒midgame progression cân bằng với Fusion (endgame).