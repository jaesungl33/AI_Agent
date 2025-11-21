# [Progression Module] [Tank War] ELO & RANK System - OLD (not use)

TANK WAR - ELO & RANK SYSTEM DESIGN

Phiên bản: 1.0 Ngày cập nhật: 09.07.2025 Người phụ trách: QuocTA

<table><tr><td rowspan=1 colspan=1>Phien ban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>M6ta</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>09.07.2025Tao file</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>QuocTA</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>v1.2</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr></table>

# 1. Mục đích thiết kế

Thiết lập hệ thống điểm Elo kèm hệ thống Rank dựa trên phong cách MOBA, nhằm tăng tính cạnh tranh, háo hứng khi leo hạng và duy trì độ hút trong game Tank War.

2. Hệ thống Elo

# 2.1 Cách tính điểm

• Elo mặc định khi bắt đầu: 1000   
• Chiến thắng: $+ 2 0$   
• Thua trận: -15   
• Disconnect: $^ { - 1 5 + }$ thay bằng bot   
• $\mathbb { A } \mathbb { F } \mathbb { K } > \mathbb { X }$ phút đầu trận: xem như Disconnect

# 2.2 Tối đa/ Tối thiểu

• Elo tối thiểu: 500 • Elo tối đa: không giới hạn

# 2.3 Có độ điều chỉnh

• Có thể dựa trên chênh lệch Elo giữa hai đội:

◦ Thắng trước đội cao hơn: $+ 2 5$ , thua -10   
◦ Thắng trước đội thấp hơn: $+ 1 5$ , thua -20

• Có thể lựa chọn sử dụng trong v1.2 trở đi

3. Hệ thống Rank

# 3.1 Cấu trúc Rank

<table><tr><td rowspan=1 colspan=1>Rank</td><td rowspan=1 colspan=1> Elo Range</td><td rowspan=1 colspan=1>Sub-division</td><td rowspan=1 colspan=1>Bieu tuong</td></tr><tr><td rowspan=1 colspan=1>Bronze</td><td rowspan=1 colspan=1>500-799</td><td rowspan=1 colspan=1>三 =, </td><td rowspan=1 colspan=1>←</td></tr><tr><td rowspan=1 colspan=1>Silver</td><td rowspan=1 colspan=1>800-999</td><td rowspan=1 colspan=1>三 =,</td><td rowspan=1 colspan=1>→</td></tr><tr><td rowspan=1 colspan=1>Gold</td><td rowspan=1 colspan=1>1000 - 1199</td><td rowspan=1 colspan=1>, 1</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Platinum</td><td rowspan=1 colspan=1>1200 - 1399</td><td rowspan=1 colspan=1>三 ,1</td><td rowspan=1 colspan=1>★</td></tr><tr><td rowspan=1 colspan=1>Diamond</td><td rowspan=1 colspan=1>1400 - 1599</td><td rowspan=1 colspan=1>三, , I</td><td rowspan=1 colspan=1>★★</td></tr><tr><td rowspan=1 colspan=1>Master</td><td rowspan=1 colspan=1>1600 - 1799</td><td rowspan=1 colspan=1>,</td><td rowspan=1 colspan=1>★★★</td></tr><tr><td rowspan=1 colspan=1>Grandmaster</td><td rowspan=1 colspan=1>1800+</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>☆</td></tr></table>

# 3.2 Quy tắc thăng hạng

• Tại mỗi mốc sub-rank ( $\lvert \lvert 1 \rvert  \lvert 1 \rvert  \lvert \lvert \xrightarrow { } \lvert \lvert$ rank tiếp theo): ◦ Cần đạt ngưỡng Elo quy định ◦ Thắng 3/5 trận cuối để được thăng (hoặc quy tắc linh hoạt)

# 3.3 Quy tắc giữ hạng / rơi hạng

• Tụt Rank khi Elo thấp hơn mức Rank hiện tại $> 5 0$ Elo trong 5 trận liên tiếp • Không tụt Rank trong Gold trở xuống Silver (giữ Rank) • Master trở xuống Diamond được bật điều kiện tụt Rank.

# 3.4 Season Reset

• Có thể reset Rank theo mùa (dự kiến 3 tháng / mùa).   
• Elo sẽ bị soft reset về mốc trung bình tống hợp.

4. Hiển thị & UI

• Hiển thị Rank hiện tại ngay trong lobby   
• Sau mỗi trận, thông báo thắng/thua $^ +$ cập nhật điểm Elo và Rank   
• Biểu tượng Rank dễ nhận biết, tăng hứng thú

5. Dự phòng về Smurf / đều hạng không đúng

• Có thể dùng thuật toán để điều chỉnh nhanh Elo của người chơi có winrate cao bất thường trong 10 trận đầu

• Đặt giới hạn vào lobby theo Rank (v1.2)