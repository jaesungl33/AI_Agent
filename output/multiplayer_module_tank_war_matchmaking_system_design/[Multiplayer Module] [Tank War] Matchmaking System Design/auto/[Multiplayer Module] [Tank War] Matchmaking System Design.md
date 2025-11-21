# [Multiplayer Module] [Tank War] Matchmaking System Design

# HỆ THỐNG GHÉP TRẬN

Phiên bản: v1.2 Người tạo file: QuocTA Ngày tạo: 30 - 06 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mota</td><td rowspan=1 colspan=1>Ngudi viet</td><td rowspan=1 colspan=1>Ngudireview</td><td rowspan=1 colspan=1>Duyét?</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>30-06-2025</td><td rowspan=1 colspan=1>Taofile</td><td rowspan=1 colspan=1>QuocTA</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>09-07-2025</td><td rowspan=1 colspan=1> Dieu chinh lai co ché ghép trän bang Elopoints</td><td rowspan=1 colspan=1>QuocTA</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.2</td><td rowspan=1 colspan=1>18-09-2025</td><td rowspan=1 colspan=1>Format laifile</td><td rowspan=1 colspan=1>? phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mục đích thiết kế

Thiết lập hệ thống ghép trận cho Tank War, đảm bảo:

◦ Tính công bằng trong lựa chọn đối thủ.   
◦ Tính nhanh chóng trong quá trình ghép.   
◦ Trải nghiệm người chơi được đặt lên hàng đầu.

# 2. Mục lục

3. Hình thức ghép trận hỗ trợ

4. Quy tắc ghép trận cốt lõi

4.1 Thông tin ghép trận   
4.2 Pool ghép trận   
4.3 Mô hình phòng chơi   
4.4 Hệ thống Elo points đơn giản (chỉ dùng cho Version hiện tại)   
5. Quy trình ghép trận   
6. Quy tắc đặc biệt trong quá trình Ghép trận   
7. Quy tắc mở rộng   
8. Xử lý đặc biệt   
9. Mở rộng   
10. Ghi chú

# 3. Hình thức ghép trận hỗ trợ

◦ Solo Match (5v5): Người chơi vào hàng ghép trận một mình, hệ thống sẽ ghép 10 người chia làm 2 đội.

# 4. Quy tắc ghép trận cốt lõi

# 4.1 Thông tin ghép trận

Elo points hiện tại Thời gian vào hàng

# 4.2 Pool ghép trận

▪ Chia theo chế độ Solo. Nạp pool trong X giây hoặc khi đủ Y nhóm.   
▪ Ghép theo thứ tự thời gian $+$ giá trị độ chênh lệch Elo points.

# 4.3 Mô hình phòng chơi

Cân bằng: Elo point các thành viên trong đội không chênh lệch > 10% ▪ Chênh lệch: Cho phép tối đa Elo point chênh lệch $1 0 - 3 0 \%$ (tự động điều chỉnh theo thời gian) - trong trường hợp chưa kiếm được người chơi phù hợp.

# $4 . 4 H { \hat { \boldsymbol { \ e } } }$ thống Elo points đơn giản (chỉ dùng cho Version hiện tại)

Điểm Elo mặc định của tất cả người chơi là 1000.   
▪ Thắng được $+ 2 0$ .   
▪ Thua bị - 15.   
▪ Disconnect khỏi trận đấu khi chưa có kết quả mặc định bị xử thua và hệ thống sẽ - 15 Elo ngay lập tức.

# 5. Quy trình ghép trận

◦ Tạo nhóm ghép (1 người)   
◦ Nạp pool (X giây / đủ Y người)   
◦ Tạo mô hình phòng chơi (cân bằng hoặc chênh lệch) ◦ Fill phòng: ghép dựa theo Elo points   
◦ Thông báo Match Success   
◦ Người chơi tự động được đưa vào màn hình Chọn Tank.

▪ Chọn Tank (thời gian CD là 15s) Hết time chọn nếu người chơi chưa chọn Tank sẽ Random trong Pool xe Tank họ có.

◦ Loading Screen ◦ Game Screen

# 6. Quy tắc đặc biệt trong quá trình Ghép trận

◦ Người chơi trong quá trình chọn Tank nếu bị Disconnect sẽ không thể Reconnect lại.

◦ Hệ thống sẽ tự dùng Bot điều khiển xe tăng của dưới tên người chơi đã bị Disconnect.   
◦ Người chơi bị Disconnect sẽ bị trừ điểm Elo tương đương với thua trận.   
◦ Hủy ghép giữa chừng: Cho phép tại UI.

# 7. Quy tắc mở rộng

◦ Tăng dần phạm vi chênh lệch cho Elo points theo thời gian chờ.   
◦ Sau 60s, Elo points chênh lệch có thể lên đến $2 0 \%$ .   
◦ Sau 90s, Elo points chênh lệch có thể lên đến $3 0 \%$ .   
◦ Sau 120s, ghép với bot nếu không tìm thấy đối thủ tương đương.

# 8. Xử lý đặc biệt

# 9. Mở rộng

◦ Team Match (5v5): Nhóm 5 người lập sẵn, ghép với nhóm 5 người khác.   
◦ Custom Room: Tự tạo phòng, chọn mode chơi, mời bạn bè và tự do thiết lập quy tắc (không cần ghép).   
◦ Tránh ghép với người mình muốn tránh (block / report)   
◦ Ghép theo Tank Class khác nhau: Để tăng chiến thuật đội hình

◦ Thêm 2 thông số ghép trận:

▪ Cấp độ (level) (not for this version) Power Rating (tính theo tổng số sao nâng cấp và trang bị) (not for this version)

# 10. Ghi chú

◦ Bản thiết kế chỉ áp dụng cho các chế độ PvP matchmaking. PvE sẽ có logic riêng.

◦ Y ê u  c ầ u  c l i e n t - s i d e  t riể n  k h a i c ơ  c h ế  C a n c e l ,  A u t o - c o n f ir m kh i t h ô n g  b á o  g h é p  t h à n h c ô n g .