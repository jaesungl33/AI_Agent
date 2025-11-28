# [Progression Module] [Tank Wars] Fusion Artifact

# Fusion Artifact

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>M6ta</td><td rowspan=1 colspan=1>Ngudi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét?</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>30-10-2025</td><td rowspan=1 colspan=1>Taofile</td><td rowspan=1 colspan=1> Kent</td><td rowspan=1 colspan=1> Tomato</td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr></table>

1. Mục tiêu hệ thống (Purpose)

• Là cơ chế nâng cấp và tiến hóa Artifact từ độ hiếm thấ $)  { \mathsf { c a o } }$ .

Tạo progression loop dài hạn cho người chơi mà không cần phụ thuộc class hoặc skill.

• Giữ giá trị cho các Artifact cũ, đồng thời tạo “động lực và cảm xúc RNG” trong hành trình build.

• Là một phần quan trọng của Artifact System, giúp người chơi đạt tới các độ hiếm Epic, Legendary, Mythic.

# 2. Core Mechanic

• Người chơi sử dụng 3 Artifact cùng độ hiếm để tiến hành Fusion ra Artifact có độ hiếm cao hơn.

• Tỉ lệ thành công giảm dần theo cấp độ hiếm, và kết quả phụ thuộc vào RNG.

• Khi Fusion, người chơi có thể chọn:

◦ Slot mục tiêu: Engine / Weapon / Armor / CPU / Energy Core - Nếu dùng đúng loại Artifact làm nguyên liệu.

◦ Bộ Artifact: Nếu dùng đúng loại và bộ Artifact làm nguyên liệu.

• Fusion thất bại sẽ không mất trắng hoàn toàn ‒ người chơi sẽ nhận lại 1 Artifact cùng cấp.

# 3. Cấu trúc cơ bản (System Structure)

<table><tr><td rowspan=1 colspan=1>Turcap</td><td rowspan=1 colspan=1>Den cap</td><td rowspan=1 colspan=1>Ti le thanh cong (base)</td><td rowspan=1 colspan=1>Gqi y tai nguyén yéu cäu</td></tr><tr><td rowspan=1 colspan=1>Common → Rare</td><td rowspan=1 colspan=1>50%</td><td rowspan=1 colspan=1>3 Common Artifact + Coin (1,000)</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Rare→ Epic</td><td rowspan=1 colspan=1>30%</td><td rowspan=1 colspan=1>3 Rare Artifact + Coin (5,000)</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Epic→ Legendary</td><td rowspan=1 colspan=1>10%</td><td rowspan=1 colspan=1>3 Epic Artifact + Coin (10,000)</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Legendary → Mythic</td><td rowspan=1 colspan=1>3%</td><td rowspan=1 colspan=1>3 Legendary Artifact + Coin (20,000)</td><td rowspan=1 colspan=1></td></tr></table>

• Có thể dùng vật phẩm hỗ trợ Fusion:

Fusion Data Core : tăng tỉ lệ thành công $+ 5 \%$ mỗi item (stack tối đa $+ 2 0 \%$ ).

◦ Stabilizer Module : bảo hiểm ‒ khi thất bại vẫn hoàn lại 3 Artifact nguyên liệu.

# 4. Công thức nếu dùng Fusion Data Core

FinalRate $=$ BaseRate $^ +$ (Fusion Data Core × 5%)

Vidu: Epic Legendary (base 10%),dung3 Fusion Core $( + 1 5 \% ) = 2 5 \%$ success rate

# 5. Trải nghiệm người chơi (Player Experience)

• Cảm xúc chủ đạo: “hy vọng ‒ hồi hộp ‒ bùng nổ”.

• Mỗi lần Fusion là một canh bạc chiến lược:

◦ Có thể đầu tư thêm vật phẩm để tăng tỉ lệ thành công, hoặc liều lĩnh để tiết kiệm tài nguyên.

• Cảm giác thỏa mãn khi roll ra Artifact hiếm với sub-stat mong muốn ‒ mang đặc trưng RNG “Epic Seven style”.

• Fusion trở thành nội dung endgame ‒ duy trì động lực grind và thử build mới.

# 6. Thiết kế cân bằng (Balance Direction)

. Không ép buộc: Fusion là tùy chọn, không bắt buộc.

• Thời gian ‒ rủi ro ‒ phần thưởng được cân bằng bằng tỉ lệ thành công.

• Fusion Data Core và Stabilizer là công cụ (hiếm) tạo lựa chọn an toàn, tránh RNG thuần túy.

• Giới hạn “cảm xúc pay-to-win”: người chơi F2P vẫn có thể đạt Artifact cao bằng thời gian và kiên nhẫn.

# 7. Hành trình progression (Loop)

• Thu thập Artifact (PvE / PvP reward).   
• Chọn nguyên liệu Fusion (3 cùng rarity).   
• Chọn slot & hướng build.   
• Dùng vật phẩm phụ (nếu có).   
• Xác nhận Fusion (RNG kết quả).   
• Cường hóa Artifact mới tiếp tục loop.

# 8. UX / UI Note

• Giao diện “Fusion Lab” đơn giản, hiển thị:

◦ Slot chọn Artifact Preview (trước & sau Fusion). ◦ Tỉ lệ thành công hiện tại $^ +$ chi phí. ◦ Animation riêng khi thành công (ánh sáng màu rarity). ◦ Sound cue khác biệt cho từng rarity (Epic, Legendary, Mythic)

# 9. Key Design Intent

• Tạo độ sâu progression nhưng không gây mệt mỏi.   
• Cho phép người chơi cảm thấy “mình vẫn kiểm soát được may rủi” thông qua vật phẩm phụ trợ.   
• Biến Fusion trở thành “mini-gacha mechanic” trong Tank War ‒ vui, hồi hộp, không áp lực.