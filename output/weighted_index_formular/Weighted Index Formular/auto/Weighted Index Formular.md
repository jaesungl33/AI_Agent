# Weighted Index Formular

Weighted Index Formular

# Công thức tính Weighted Index cho Tank

1. Định nghĩa

Weighted Index là chỉ số đại diện cho tổng ảnh hưởng thực tế của các thuộc tính Tank đến gameplay, có tính đến mức độ quan trọng của từng thuộc tính.

2. Công thức tổng quát

Weighted Index $= \pmb { \Sigma }$ (Multiplieri $\boldsymbol { x }$ Design Weighti)

• Multiplierᵢ: hệ số chỉ số của loại Tank đó (so với Tank chuẩn ‒ thường là Scout).   
• Design Weight $\dot { \bf 1 }$ : mức độ ảnh hưởng thực tế của chỉ số thứ i đến gameplay (do game designer xác định).   
• n: tổng số thuộc tính (thường là 7: HP, Speed, Damage, Reload, Fire Rate, Projectile Speed, Range)

# 3. Bảng ví dụ trọng số thiết kế (Design Weights)

<table><tr><td colspan="1" rowspan="1">Thu@c tinh</td><td colspan="1" rowspan="1">Ten ky thuat</td><td colspan="1" rowspan="1">Design Weight</td><td colspan="1" rowspan="1"> Ghi chu anh huong gameplay</td></tr><tr><td colspan="1" rowspan="1">HP</td><td colspan="1" rowspan="1">Health</td><td colspan="1" rowspan="1">1.2</td><td colspan="1" rowspan="1"> Söng lau hon = ton tai lau hon trong giaotranh</td></tr><tr><td colspan="1" rowspan="1">Speed</td><td colspan="1" rowspan="1">MovementSpeed</td><td colspan="1" rowspan="1">1.5</td><td colspan="1" rowspan="1">Co dong,né tránh,dinh vi- cuc ky quantrong</td></tr><tr><td colspan="1" rowspan="1">Damage</td><td colspan="1" rowspan="1">Base Damage</td><td colspan="1" rowspan="1">1.0</td><td colspan="1" rowspan="1">Gay sat thuong truc tiep</td></tr><tr><td colspan="1" rowspan="1">Reload Time</td><td colspan="1" rowspan="1">Reload Speed</td><td colspan="1" rowspan="1">0.8</td><td colspan="1" rowspan="1">Tac dong gian tiep den DPS</td></tr><tr><td colspan="1" rowspan="1">Fire Rate</td><td colspan="1" rowspan="1">Shots per second</td><td colspan="1" rowspan="1">1.3</td><td colspan="1" rowspan="1">Tac dong lón den DPS va áp luc giao tranh</td></tr><tr><td colspan="1" rowspan="1">Projectile Speed</td><td colspan="1" rowspan="1">Bullet Velocity</td><td colspan="1" rowspan="1">0.5</td><td colspan="1" rowspan="1">Dé né tränh néu dan chäm</td></tr><tr><td></td><td></td><td></td><td></td></tr><tr><td>Range</td><td>Attack Range</td><td>0.7</td><td>Tot nhung khong phai yéu tδ then chöt</td></tr></table>

4. Ví dụ tính Weighted Index (Assault Tank)

Giả sử Assault Tank có multiplier như sau:

<table><tr><td rowspan=1 colspan=1>Thuc tinh</td><td rowspan=1 colspan=1>Multiplier</td><td rowspan=1 colspan=1>Weight</td><td rowspan=1 colspan=1> Multiplier X Weight</td></tr><tr><td rowspan=1 colspan=1>HP</td><td rowspan=1 colspan=1>1.4</td><td rowspan=1 colspan=1>1.2</td><td rowspan=1 colspan=1>1.68</td></tr><tr><td rowspan=1 colspan=1>Speed</td><td rowspan=1 colspan=1>0.85</td><td rowspan=1 colspan=1>1.5</td><td rowspan=1 colspan=1>1.275</td></tr><tr><td rowspan=1 colspan=1>Damage</td><td rowspan=1 colspan=1>1.3</td><td rowspan=1 colspan=1>1.0</td><td rowspan=1 colspan=1>1.3</td></tr><tr><td rowspan=1 colspan=1>Reload Time</td><td rowspan=1 colspan=1>1.3</td><td rowspan=1 colspan=1>0.8</td><td rowspan=1 colspan=1>1.04</td></tr><tr><td rowspan=1 colspan=1>Fire Rate</td><td rowspan=1 colspan=1>0.9</td><td rowspan=1 colspan=1>1.3</td><td rowspan=1 colspan=1>1.17</td></tr><tr><td rowspan=1 colspan=1>Projectile Speed</td><td rowspan=1 colspan=1>0.9</td><td rowspan=1 colspan=1>0.5</td><td rowspan=1 colspan=1>0.45</td></tr><tr><td rowspan=1 colspan=1>Range</td><td rowspan=1 colspan=1>0.75</td><td rowspan=1 colspan=1>0.7</td><td rowspan=1 colspan=1>0.525</td></tr><tr><td rowspan=1 colspan=1>Weighted Index</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>7.44</td></tr></table>

5. Ứng dụng

• Dùng để so sánh sức mạnh tương đối của các loại Tank.

• Tránh hiểu lầm “Tank có tổng chỉ số cao là mạnh”, vì chỉ số quan trọng khác nhau.

• Giúp Game Designer cân bằng class dễ hơn khi thêm kỹ năng, chỉ số, hoặc thiết kế mới.