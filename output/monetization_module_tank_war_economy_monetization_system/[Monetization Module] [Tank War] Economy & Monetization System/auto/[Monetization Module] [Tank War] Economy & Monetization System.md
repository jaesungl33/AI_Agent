# [Monetization Module] [Tank War] Economy & Monetization System

HỆ THỐNG KINH TẾ VÀ PHƯƠNG THỨC KINH DOANH

Phiên bản: v1.1 Người tạo file: Kent (QuocTA) Ngày tạo: 07 - 09 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mo ta</td><td rowspan=1 colspan=1>Nguoi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duy@t?</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>07-09-2025</td><td rowspan=1 colspan=1>Taofile</td><td rowspan=1 colspan=1>QuocTA</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format lai file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Currency Flow (Nguồn & Sink)

1.1 Diamond (Hard Currency)

<table><tr><td>Nh6m</td><td>Nguon sinh</td><td>Sink (Tieu thu)</td></tr><tr><td rowspan="6">Nap</td><td rowspan="5">Thanh toan IAP (góilé, gói nap dinh ky)</td><td>Buff Premium (thé tháng, Battle Pass Premium)</td></tr><tr><td>Ticket Gen-Al</td></tr><tr><td>Warp Premium (gacha cao cäp) Decal Premium (Seasonal, Limited Edition)</td></tr><tr><td></td></tr><tr><td>Al Companion</td></tr><tr><td rowspan="2"></td><td>Effect Premium (trails,aura,animation) Tank Skin (LE, Seasonal)</td></tr><tr><td></td></tr></table>

# 1.2 Coin (Soft Currency)

<table><tr><td rowspan=1 colspan=1>Nh6m</td><td rowspan=1 colspan=1>Nguon sinh</td><td rowspan=1 colspan=1> Sink (Tieu thu)</td></tr><tr><td rowspan=1 colspan=1>Gameplay</td><td rowspan=1 colspan=1>Thuong trän (theo KDA, Win/Lose, Mode)</td><td rowspan=1 colspan=1> Mua xe Tank</td></tr><tr><td rowspan=1 colspan=1>Gameplay</td><td rowspan=1 colspan=1>LiveOps Event Reward</td><td rowspan=1 colspan=1>Mua Warp (gacha thuong)</td></tr><tr><td rowspan=1 colspan=1>Gameplay</td><td rowspan=1 colspan=1>Daily Dungeon (PvE)</td><td rowspan=1 colspan=1>Mua Decal thuong</td></tr><tr><td rowspan=1 colspan=1>Meta Layer</td><td rowspan=1 colspan=1>Battle Pass Free Track</td><td rowspan=1 colspan=1>Luyén Gen Al (Al decal, Al companion)</td></tr><tr><td rowspan=1 colspan=1>Meta Layer</td><td rowspan=1 colspan=1>In-App Ads Reward</td><td rowspan=1 colspan=1>Artifact shards, nang cap Artifact</td></tr></table>

# 1.3 Battle Pass

<table><tr><td rowspan=1 colspan=1>Track</td><td rowspan=1 colspan=1>Reward</td></tr><tr><td rowspan=1 colspan=1>Free</td><td rowspan=1 colspan=1>Coin, Decal (gacha thuong), Warp thuong</td></tr><tr><td rowspan=1 colspan=1>Paid</td><td rowspan=1 colspan=1>Coin,Ticket Gen-Al, Warp Premium, Decal Premium</td></tr></table>

# 1.4 In-App Ads (Rewarded Ads)

<table><tr><td rowspan=1 colspan=1>Loai Reward</td><td rowspan=1 colspan=1>Tan suat (cap/ngay)</td><td rowspan=1 colspan=1>G</td></tr><tr><td rowspan=1 colspan=1>Ticket Gen-Al</td><td rowspan=1 colspan=1>1-2 lan/ngay</td><td rowspan=1 colspan=1>Tao nhu cau thu nghiém Al Decal</td></tr><tr><td rowspan=1 colspan=1>Coin</td><td rowspan=1 colspan=1>3-5 lan/ngay</td><td rowspan=1 colspan=1> Gia tri~0.5 tran thang</td></tr><tr><td rowspan=1 colspan=1>Näng luong Event (LiveOps)</td><td rowspan=1 colspan=1>1lan/ngay</td><td rowspan=1 colspan=1> Gän voi event gioi han</td></tr><tr><td rowspan=1 colspan=1>Näng luong PvE (Daily Dungeon)</td><td rowspan=1 colspan=1>1-2 lan/ngay</td><td rowspan=1 colspan=1>Gi chan F2P</td></tr></table>

# 1.5 Shop In-Game

<table><tr><td rowspan=1 colspan=1> Danh muc</td><td rowspan=1 colspan=1>Gia (Coin/Diamond)</td><td rowspan=1 colspan=1> Ghi chu</td></tr><tr><td rowspan=1 colspan=1>Xe Tank</td><td rowspan=1 colspan=1>Coin</td><td rowspan=1 colspan=1>Gia theo tier (Scout ré hon Heavy)</td></tr><tr><td rowspan=1 colspan=1>Artifact</td><td rowspan=1 colspan=1>Coin + shards</td><td rowspan=1 colspan=1>Craft, nang cap,combine</td></tr><tr><td rowspan=1 colspan=1>Decalthuong</td><td rowspan=1 colspan=1>Coin</td><td rowspan=1 colspan=1>Seasonal, rotation</td></tr><tr><td rowspan=1 colspan=1>Decal Premium</td><td rowspan=1 colspan=1>Diamond</td><td rowspan=1 colspan=1>LE, collab</td></tr><tr><td rowspan=1 colspan=1>Warp thuong</td><td rowspan=1 colspan=1>Coin</td><td rowspan=1 colspan=1>RNG cosmetic/decoration</td></tr><tr><td rowspan=1 colspan=1>Warp Premium</td><td rowspan=1 colspan=1>Diamond</td><td rowspan=1 colspan=1>RNG high rarity cosmetic</td></tr></table>

# 2. Công thức Cơ bản (Draft)

# 2.1 Phần thưởng trận (Match Reward Formula)

CoinReward $=$ BaseReward $\times$ ModeMultiplier × (WinBonus/LoseFactor) $\times$ C S

▪ BaseReward: 50 coin.

▪ ModeMultiplier: • Deathmatch $= 1 . 0$ • Capture Base $= 1 . 2$

▪ WinBonus: x1.5.

▪ LoseFactor: x1.0.

▪ Combat Score: • $\mathsf { C S } = ( 0 . 5 \times [ ( \mathsf { K i l } | \mathsf { l } \mathsf { s } ) + ( 0 . 6 \times \mathsf { A s s i s t s } ) - ( 0 . 8 \times \mathsf { D e a t h s } ) ] ) + ( 0 . 4 \times \mathsf { D a m a g e 5 c o r e } )$ + (0.1 × Streak Bonus)

# Ví dụ minh họa

Player thắng Capture Base.

• Kills $= 5$ , Assists $= 4$ , Deaths $= 3 $ KDA Impact = 5 + (0.6×4) ‒ (0.8×3) = 5 + 2.4 ‒ $2 . 4 = 5 . 0$   
• Damage Score $= 1 . 1$ (trên trung bình role). Streak Bonus $= 1 . 0 5$ (Double Kill).   
• $\mathsf { C S } = ( 0 . 5 \times 5 . 0 ) + ( 0 . 4 \times 1 . 1 ) + ( 0 . 1 \times 1 . 0 5 ) = 2 . 5 + 0 . 4 4 + 0 . 1 0 5 = 3 . 0 4 5$

▪ CoinReward $= 5 0 \times 1 . 2 \times 1 . 5 \times 3 . 0 4 5 \approx 2 7 4$ coin (chỉ lấy số nguyên và sẽ làm tròn lén néu só thäp phän lón hon hoäc bäng 0.5,lam tron xuöng néu nhó hdn 0.5)

# 2.2 Chi phí mở Tank

Nguyên tắc thiết kế

▪ F2P-friendly: tất cả Tank đều mở bằng Coin (soft currency). Tier-based progression: chi phí tăng theo Tier để khuyến khích người chơi gắn bó.   
▪ Class factor: mỗi Class (Scout, Assault, Heavy) có hệ số chi phí khác nhau phản ánh độ phức tạp & vai trò.

# ▪ Time-to-Unlock:

• Tank đầu tiên ngoài default $ \leqslant 3$ ngày chơi đều đặn.   
• Tank Tier $_ { 3 } \to$ khoảng 14 ngày.   
• Giữ động lực cho cả F2P và P2W.

# Công thức tính chi phí

TankCost $\underline { { \underline { { \mathbf { \Pi } } } } }$ TierBase × ClassFactor

# TierBase (Coin):

<table><tr><td rowspan=1 colspan=1>Tier</td><td rowspan=1 colspan=1>BaseCost (Coin)</td><td rowspan=1 colspan=1>Ghi chu</td></tr><tr><td rowspan=1 colspan=1>Tier1</td><td rowspan=1 colspan=1>2,000</td><td rowspan=1 colspan=1>Xe nhäp món,dé tiep can</td></tr><tr><td rowspan=1 colspan=1>Tier2</td><td rowspan=1 colspan=1>3,000</td><td rowspan=1 colspan=1>Meta co ban</td></tr><tr><td rowspan=1 colspan=1>Tier 3</td><td rowspan=1 colspan=1>5,000</td><td rowspan=1 colspan=1>Meta nang cao</td></tr><tr><td rowspan=1 colspan=1>Tier 4</td><td rowspan=1 colspan=1>8,000</td><td rowspan=1 colspan=1> Xe hiem/late progression</td></tr></table>

# ClassFactor:

<table><tr><td rowspan=1 colspan=1>class</td><td rowspan=1 colspan=1>He s6</td><td rowspan=1 colspan=1>Giai thich</td></tr><tr><td rowspan=1 colspan=1>Scout</td><td rowspan=1 colspan=1>1</td><td rowspan=1 colspan=1>Dé choi,sat thuong thäp,vai tro m&amp; däu</td></tr><tr><td rowspan=1 colspan=1>Assault</td><td rowspan=1 colspan=1>1.2</td><td rowspan=1 colspan=1>Damage +da dung,d khó vua</td></tr><tr><td rowspan=1 colspan=1>Heavy</td><td rowspan=1 colspan=1>1.3</td><td rowspan=1 colspan=1>Tank trau,late-game manh</td></tr><tr><td rowspan=1 colspan=1>Elemental</td><td rowspan=1 colspan=1>1.4</td><td rowspan=1 colspan=1>Damage to + hieu ung</td></tr></table>

# Bảng chi phí mẫu (Coin)

<table><tr><td rowspan=1 colspan=1>Tier</td><td rowspan=1 colspan=1>Scout</td><td rowspan=1 colspan=1>Assault</td><td rowspan=1 colspan=1>Heavy</td></tr><tr><td rowspan=1 colspan=1>1</td><td rowspan=1 colspan=1>2,000</td><td rowspan=1 colspan=1>2,400</td><td rowspan=1 colspan=1>2,600</td></tr><tr><td rowspan=1 colspan=1>2</td><td rowspan=1 colspan=1>3,000</td><td rowspan=1 colspan=1>3,600</td><td rowspan=1 colspan=1>3,900</td></tr><tr><td rowspan=1 colspan=1>3</td><td rowspan=1 colspan=1>5,000</td><td rowspan=1 colspan=1>6,000</td><td rowspan=1 colspan=1>6,500</td></tr><tr><td rowspan=1 colspan=1>4</td><td rowspan=1 colspan=1>8,000</td><td rowspan=1 colspan=1>9,600</td><td rowspan=1 colspan=1>10,400</td></tr></table>

# Time-to-Unlock (ước tính)

Giả định trung bình CoinReward $\approx 2 0 0$ coin/trận, 15 trận/ngày ( ${ \sim } 3 0$ phút):

▪ Scout Tier $\pm  \mathord { \sim } 1 0$ trận. Assault Tier $2  \mathord { \sim } 1 8$ trận. Heavy Tier $\ 3  \sim 3 8$ trận. Heavy Tier $4  { \sim } 6 0$ trận.

(Voi Battle PassAds va su kien,thoi gian thuc te' sé rut ngan.Tuy nhien Coin con dung cho cac tinh näng khac nén c6 the anh huang tocdo Unlockxe Tank moi cua ngudi chdi)

# 2.3 Warp (Gacha)

# ◦ Normal Warp (Coin):

$N = \pm 0 0$ coin/lần Bảo hiểm (optional): 30 lần guaranteed Rare.

# ◦ Premium Warp (Diamond):

▪ $N = 5 0$ Diamond/lần ▪ Bảo hiểm (optional): 50 lần guaranteed Epic, 90 lần Legendary.

2.4 Battle Pass Progression

Nguyên tắc thiết kế:

• Seasonal cycle: mỗi Battle Pass (BP) kéo dài 6‒8 tuần (theo chu kỳ meta).   
• Dual track: Free track (Coin $^ +$ cosmetic cơ bản) & Paid track (Diamond-exclusive, Gen-AI ticket, premium cosmetic).   
• Match-driven: XP nhận chủ yếu qua chơi trận (Deathmatch, Capture Base).   
• Quest-driven: Daily/Weekly quest tăng tốc độ progression, chống nhàm chán.

# • Time-to-Complete:

◦ Người chơi bình thường (\~10‒15 trận/ngày) hoàn thành $8 0 { - } 9 0 \%$ BP.   
◦ Người chơi hardcore/dùng boost hoàn thành $100 \%$ .

# Công thức tính XP

BP_XP = MatchXP $\times$ ModeMultiplier × WinFactor

# MatchXP:

<table><tr><td rowspan=1 colspan=1>Mode</td><td rowspan=1 colspan=1>MatchXP (thua)</td><td rowspan=1 colspan=1>MatchXP (thäng)</td><td rowspan=1 colspan=1>Ghi chu</td></tr><tr><td rowspan=1 colspan=1>Deathmatch</td><td rowspan=1 colspan=1>10</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>15Tran nhanh, nhip cao,XP thap hon</td></tr><tr><td rowspan=1 colspan=1>Capture Base</td><td rowspan=1 colspan=1>12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>18 Tran dai hon,chien thuät, XP cao hor</td></tr></table>

# ▪ ModeMultiplier:

• Deathmatch $= 1 . 0$ • Capture Base $= 1 . 2$

WinFactor: 1.2 khi thắng, 1.0 khi thua.

▪ QuestBonus: • Daily Quest $= + 3 0$ XP (khoảng 2‒3 trận). • Weekly Quest $= + 1 5 0$ XP (khoảng 8‒10 trận).

# Level Progression (ước tính)

• XP per Level $= 1 0 0$ XP.   
• Target Level: 50‒60 cấp / Season.   
• Completion Time: ◦ Casual: ${ \sim } 1 . 5 { - 2 }$ tuần để đạt lv30. ◦ Active: 5‒6 tuần đạt lv50 (max).

# Reward Distribution:

• Free Track: ◦ Coin (đều đặn mỗi 5 lv). ◦ Warp thường (Coin-based gacha).

◦ Decal thường.   
◦ Một số Artifact shard.

# • Paid Track:

◦ Diamond (refund một phần phí BP).   
◦ Warp Premium (Diamond-based gacha).   
◦ Ticket Gen-AI (tạo Decal/Companion).   
◦ Decal Premium (Seasonal/LE).   
◦ Skin Premium (ở milestone cuối: lv50).

# Ví dụ milestone (Season length $5 0 \left. \mathbf { v } \right.$ )

<table><tr><td rowspan=1 colspan=1>Level</td><td rowspan=1 colspan=1>Free Track</td><td rowspan=1 colspan=1>Paid Track</td></tr><tr><td rowspan=1 colspan=1>1</td><td rowspan=1 colspan=1>Coin X200</td><td rowspan=1 colspan=1>Coin X500 + Ticket Gen-Al</td></tr><tr><td rowspan=1 colspan=1>5</td><td rowspan=1 colspan=1>Warp thuong X1</td><td rowspan=1 colspan=1>Warp Premium X1</td></tr><tr><td rowspan=1 colspan=1>10</td><td rowspan=1 colspan=1>Decal thuong</td><td rowspan=1 colspan=1>Decal Premium</td></tr><tr><td rowspan=1 colspan=1>20</td><td rowspan=1 colspan=1>Coin ×500</td><td rowspan=1 colspan=1>Ticket Gen-Al ×2</td></tr><tr><td rowspan=1 colspan=1>30</td><td rowspan=1 colspan=1>Artifact shard ×10</td><td rowspan=1 colspan=1>Warp Premium ×2</td></tr><tr><td rowspan=1 colspan=1>40</td><td rowspan=1 colspan=1>Warp thuong ×2</td><td rowspan=1 colspan=1>Coin X 1000 + Ticket Gen-Al</td></tr><tr><td rowspan=1 colspan=1>50</td><td rowspan=1 colspan=1>Coin ×1000</td><td rowspan=1 colspan=1>Skin Premium (LE)</td></tr></table>

# Monetization Hooks

• Paid Upgrade: 300‒500 Diamond/Season (\~5‒7 USD).   
• BP Boost: mua thêm “ $^ { 6 6 } + 1 0$ lv” hoặc $^ { 6 6 } { + } 1 0 0 0 \times \mathsf { P } ^ { 9 }$ ” bằng Diamond.   
• Engagement Loop: Paid track gắn Gen-AI & Skin để tạo động lực nạp.

# 3. Guardrails (Chống Pay-to-Win)

# 1. Tank Unlock

Tất cả Tank có thể mua bằng Coin.   
▪ Thời gian mở tank đầu tiên $\leqslant 3$ ngày; tank meta $\leqslant 2$ tuần.

# 2. Artifact Balance (optional)

PvP Ranked: giới hạn Epic trở xuống.   
Legendary/Outstanding chỉ kích hoạt trong PvE/Event.

# 3. Cosmetic Economy

Warp/Decal/Skin không buff chỉ số.

# 4. Ads vs Gameplay

Giá trị $\mathsf { A d s } \leqslant 2 0 \%$ thu nhập Coin trung bình/ngày để tránh bypass gameplay.