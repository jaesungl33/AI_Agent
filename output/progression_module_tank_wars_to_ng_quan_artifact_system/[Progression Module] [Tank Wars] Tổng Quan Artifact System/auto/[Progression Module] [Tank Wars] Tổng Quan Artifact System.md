# [Progression Module] [Tank Wars] Tổng Quan Artifact System

# TANK WAR ‒ ARTIFACT SYSTEM

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mota</td><td rowspan=1 colspan=1>Ngudi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét?</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>30-10-2025</td><td rowspan=1 colspan=1>Taofile</td><td rowspan=1 colspan=1> Kent</td><td rowspan=1 colspan=1>.Tomato</td><td rowspan=1 colspan=1>☑</td></tr><tr><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr></table>

# 1. Mục tiêu thiết kế (Design Philosophy)

• Dễ tiếp cận, khó hoàn thiện giữ chân player lâu dài.   
• Random có kiểm soát may mắn có giá trị nhưng không phá cân bằng.   
• Không liên kết class tự do build, tạo meta bất ngờ.   
• Artifact giữ “giá trị tồn tại lâu dài” ‒ không obsolete qua patch.   
• Tạo tính bất ngờ trong game & tăng khả năng chiến thuật

# 2. Tổng quan hệ thống (Overview)

Artifact System là một tầng progression cao cấp, cho phép người chơi tinh chỉnh sức mạnh xe tăng thông qua việc lắp đặt 5 linh kiện công nghệ (Tech Parts).   
Hệ thống được thiết kế để:   
• Mang đến trải nghiệm phát triển lâu dài và “endgame grind”. Khuyến khích sự sáng tạo build tank độc lập, không phụ thuộc vào Class hay Skill.   
• Giữ vai trò “ $2 0 \%$ total power” trong chỉ số tổng thể ‒ không phá vỡ cân bằng core gameplay.

# 3. Cấu trúc trang bị (Artifact Slots)

Mỗi Tank có 5 slot Artifact cố định, tương ứng với 5 bộ phận cơ học chính:

<table><tr><td rowspan=1 colspan=1>Slot</td><td rowspan=1 colspan=1> Mo ta chuc nang</td><td rowspan=1 colspan=1>Chi so chinh có thé xuat hi@</td></tr><tr><td rowspan=1 colspan=1>Engine</td><td rowspan=1 colspan=1>Anh hu8ng tóc d di chuyén,phan üng,khä näng né tránh SPD/CRIT DMG</td><td rowspan=1 colspan=1>SPD/CRITDMG</td></tr><tr><td rowspan=1 colspan=1>Weapon</td><td rowspan=1 colspan=1>Lien quan den sat thuong va toc do bän</td><td rowspan=1 colspan=1>ATK /FIRE RATE</td></tr><tr><td rowspan=1 colspan=1>Armor</td><td rowspan=1 colspan=1>Phong thu vät ly va khäng hieu üng</td><td rowspan=1 colspan=1>SHIELD /HP</td></tr><tr><td rowspan=1 colspan=1>CPU</td><td rowspan=1 colspan=1>Toi uu hóa khä näng xu ly chien däu</td><td rowspan=1 colspan=1>CRIT Rate / Skill Power</td></tr><tr><td rowspan=1 colspan=1>Energy Core</td><td rowspan=1 colspan=1>Nguon nang luong chinh,anh huong toan cuc</td><td rowspan=1 colspan=1>HP Regen /SHIELD Regen / Skill Cooldow</td></tr></table>

# 4. Bộ Artifact (Set System)

Artifact chia thành các bộ (Set) — mỗi bộ có tên, chủ đề, và hiệu ứng đặc biệt.

<table><tr><td rowspan=1 colspan=1>Ten B</td><td rowspan=1 colspan=1> S6 mon de kich hoat</td><td rowspan=1 colspan=1>Hieu ung</td></tr><tr><td rowspan=1 colspan=1>Overdrive Set</td><td rowspan=1 colspan=1>2 món →+10% ATK5 món →Sau khitieu diet 1 ké dich,täng toc bän 10% trong 4s</td><td rowspan=1 colspan=1>Offensive</td></tr><tr><td rowspan=1 colspan=1>Guardian Set</td><td rowspan=1 colspan=1>2 món → +8% DEF5 món → Khi HP&lt;30%,tao khién häp thu 15% Max HP (CD 10s)</td><td rowspan=1 colspan=1>Defensive</td></tr><tr><td rowspan=1 colspan=1>Pulse Set</td><td rowspan=1 colspan=1>2 món → +6% Speed5 món → Sau khi dung Skill, täng CRlT Rate 15% trong 6s</td><td rowspan=1 colspan=1>Hybrid</td></tr><tr><td rowspan=1 colspan=1>Specter Set</td><td rowspan=1 colspan=1>2 món → +10% Shield Regen5 món → 20% chance hoi skill CD 2s khi crit</td><td rowspan=1 colspan=1>Tactical</td></tr><tr><td rowspan=1 colspan=1>Nova Set</td><td rowspan=1 colspan=1>2 món → +8% Damage to Shielded Target5 món → Khi pha khien doi phuong, gay thém 10% true damage</td><td rowspan=1 colspan=1>Anti-Tank</td></tr><tr><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>Skill Countdown</td><td rowspan=1 colspan=1></td></tr></table>

Người chơi được tự do phối hợp set, ví dụ: 2 Overdrive $^ { + 2 }$ Guardian $^ { + 1 }$ Nova để tạo build tùy biến.

# 5. Thuộc tính & chỉ số (Stats Structure)

Mỗi Artifact có:

• 1 chỉ số chính (Main Stat) . 3‒4 chỉ số phụ (Sub Stats)

Main Stat cố định theo slot:

<table><tr><td rowspan=1 colspan=1>Slot</td><td rowspan=1 colspan=1> Main Stat</td></tr><tr><td rowspan=1 colspan=1>Engine</td><td rowspan=1 colspan=1>MOV SPD (%) / CRIT DMG (%)</td></tr><tr><td rowspan=1 colspan=1>Weapon</td><td rowspan=1 colspan=1>ATK (%)/FIRE RATE (%)</td></tr><tr><td rowspan=1 colspan=1>Armor</td><td rowspan=1 colspan=1>SHIELD (Points)/ HP (%)</td></tr><tr><td rowspan=1 colspan=1>CPU</td><td rowspan=1 colspan=1>CRIT Rate (%) /Skill Power (%)</td></tr><tr><td rowspan=1 colspan=1>Energy Core</td><td rowspan=1 colspan=1>HP Regen (point/ s) /SHIELD Regen (point/s)/SkillCooldown Reduction (%)</td></tr></table>

Sub Stats sẽ được random từ pool chỉ số sau:

<table><tr><td></td><td></td><td></td><td>ATK（%) / SHIELD （Points）/ HP （%)/ CRIT Rate （%) / CRIT DMG（%）/</td><td></td><td></td><td></td><td></td><td></td></tr><tr><td></td><td></td><td></td><td>MOVE SPD (%) / HP Regen (point/ s) / SHIELD Regen (points/ s)</td><td></td><td></td><td></td><td></td><td></td></tr></table>

Link: Artifact Stat Table

# 6. Cường hóa (Enhancement System)

• Mỗi Artifact có thể cường hóa từ $\mathsf { L } \mathsf { v } . 1 \to \mathsf { L } \mathsf { v } . 2 0$ .

Cường hóa $100 \%$ thành công, nhưng chỉ số tăng là ngẫu nhiên như trong Epic Seven.

Mỗi 2 cấp (Lv.10/20) sẽ random tăng 1 sub-stat hoặc thêm sub-stat mới nếu chưa đủ 2

• Tại Lv.20, mở khóa “Overclock Bonus” ‒ bonus ẩn theo độ hiếm Artifact (Legendary trở lên).

# • Ví dụ:

Artifact Weapon “Plasma Barrel (Epic)”

◦ Main Stat: $\mathsf { A T K } ^ { 0 } / 0 + 2 2 \%$ ◦ Sub Stat: $\mathsf { S P D } + 5 / \mathsf { C R } | \mathsf { T } \mathsf { R a t e } + 7 \% / \mathsf { D E F } ^ { \circ } \rangle _ { 0 } + 4 \% / \mathsf { H P } + 6 \%$ ◦ Sau cường hóa đến $\mathtt { - v } . 2 0 \to \mathsf { S P D } + 8 /$ CRIT Rate +13% / DEF% +4% / HP +9%

• Link: [Progression Module] [Tank Wars] Artifact Enhancement

# 7. Hệ thống ghép (Fusion System)

• 3 Artifact cùng độ hiếm, cùng loại Ghép thành 1 Artifact cùng loại độ hiếm cao hơn và random bộ.

• Có thể chọn định hướng bộ khi ghép với vật phẩm đặc biệt.

• Tỉ lệ thành công giảm theo cấp hiếm: ◦ Common Rare: $5 0 \%$ ◦ Rare Epic: $2 0 \%$

◦ Epic Legendary: $5 \%$ ◦ Legendary Mythic: $2 \%$

• Nếu thất bại, sẽ trả lại 1 Artifact ngẫu nhiên cùng độ hiếm, giúp hạn chế mất trắng.

• Có thể bảo hiểm nếu thất bại không mất Artifact nguyên liệu nào với vật phẩm đặc biệt.

• Link: [Progression Module] [Tank Wars] Fusion Artifact

# 8. Độ hiếm (Rarity) khi nhận được Artifact từ các tính năng ingame (không đến từ Hệ thống ghép)

<table><tr><td rowspan=1 colspan=1>Rarity</td><td rowspan=1 colspan=1>Mau</td><td rowspan=1 colspan=1> S6 sub-stat</td><td rowspan=1 colspan=1>Drop rate</td><td rowspan=1 colspan=1>Overclock Bonus (Level 20)</td></tr><tr><td rowspan=1 colspan=1>Common</td><td rowspan=1 colspan=1>xam</td><td rowspan=1 colspan=1>1</td><td rowspan=1 colspan=1>85%</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Rare</td><td rowspan=1 colspan=1>Xanh</td><td rowspan=1 colspan=1>2</td><td rowspan=1 colspan=1>14%</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Epic</td><td rowspan=1 colspan=1>Tim</td><td rowspan=1 colspan=1>3</td><td rowspan=1 colspan=1>1%</td><td rowspan=1 colspan=1> +5% hieu ung set</td></tr><tr><td rowspan=1 colspan=1>Legendary</td><td rowspan=1 colspan=1>vang</td><td rowspan=1 colspan=1>4</td><td rowspan=1 colspan=1>0%</td><td rowspan=1 colspan=1> +10% hiéu ung set</td></tr><tr><td rowspan=1 colspan=1>Mythical</td><td rowspan=1 colspan=1>cam</td><td rowspan=1 colspan=1>4</td><td rowspan=1 colspan=1>0%</td><td rowspan=1 colspan=1> +15% hiéu ung set + (Passive Unique)</td></tr></table>

# 9. Trải nghiệm người chơi (Player Loop & Feeling)

• Daily Goal: Thu thập Artifact (PvE / PvP mode gameplay reward)

• Weekly Goal: Ghép Artifact, test build, chia sẻ combo với cộng đồng.

• Long-term Goal: Tối ưu “perfect stat roll” hoặc “Mythic Overclock”.

• Trải nghiệm cảm xúc:

◦ Mỗi lần cường hóa là một “gacha moment” ‒ biết chắc thành công, nhưng không biết stat nào tăng.

◦ Cảm giác “may mắn” khi sub-stat nhảy đúng thứ mình cần (Crit Rate, SPD...).

◦ Cảm giác “phấn khích $^ +$ tiếc nuối” khi roll ra Artifact gần như hoàn hảo nhưng không hoàn hảo.