# [Monetization Module] [Tank War] Garage System

# HỆ THỐNG GARAGE

Phiên bản: v1.0 Người tạo file: Kent (QuocTA) Ngày tạo: 17 - 11 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>Mota</td><td rowspan=1 colspan=1>Nguoi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>17-11-2025</td><td rowspan=1 colspan=1>Taofile</td><td rowspan=1 colspan=1> Kent</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Tổng quan hệ thống (Overview)

Garage là hệ thống tuỳ biến trực quan cho mỗi chiếc Tank, cho phép người chơi cá nhân hoá diện mạo thông qua 3 lớp vật phẩm chính: Warp (tấm phủ thân xe), Decal (hình/biểu tượng dán chồng lên Warp), và Sticker (vật phẩm tiêu hao — chữ/hình do người chơi AI-gen hoặc mua sẵn). Mục tiêu chính: tăng tương tác cá nhân hoá, giữ retention, và tạo kênh monetization bền vững.

# Nguyên tắc chính

• Mỗi Warp/Decal gắn theo từng xe cụ thể (một tank A mua Warp X không thể dùng trên tank B nếu người chơi chưa mua Warp X cho tank B).   
• Inventory lưu theo loại xe (per-tank inventory). Sticker là consumable: khi dán là mất, thay thế không hoàn trả. Decal/Warp có thể gỡ/đổi ‒ cho phép tái sử dụng (trong cùng loại xe đã mua).   
• Hệ thống hỗ trợ kéo-thả (optional UX) khi apply để gia tăng cảm giác “thực hành” (drag & drop).

2. Định nghĩa vật phẩm & thuộc tính (Item definitions)

# 2.1 Warp (Tấm phủ)

• Mô tả: Tấm phủ toàn thân xe (tương tự sơn/skin).

• Đơn vị bán: 1 tấm / 1 tank.

• Thuộc tính (game-design):

◦ ID , Tên , Rarity (Common/Uncommon/Rare/Epic/Legendary), Collection (bộ series), VisualPreview (thumbnail $+ 3 0$ preview angles), Price (currency), ApplySlots (vị trí mặc định toàn thân), IsAnimated (true/false), HasVariants (màu/alpha variants), Duration (nếu temporary skin event), EquipLimit (số lượng tank cùng 1 lúc — thường 1).

# • Hành vi:

◦ Khi mua: thêm vào Inventory của loại tank đã chỉ định.   
◦ Có thể equip / unequip vào tank trong Garage (không tiêu hao).   
◦ Có thể preview trên tank (3D/AR/angle rotate).

• UI: preview full-body, switch variants, animation toggle.

# 2.2 Decal

• Mô tả: Hình ảnh dán chồng lên Warp. Có thể layer nhiều decal (layer order quan trọng).

• Thuộc tính: ID , Name , Rarity , ImageAsset , AllowedPositions (list of position IDs trên tank), Scale/RotationLimits (nếu cho phép chỉnh), Price , Stackable (true/false), IsPermanent (true).

# • Hành vi:

◦ Decal được bán/cho/trao đổi và lưu vào Inventory per-tank.   
◦ Có vị trí dán được chỉ định — người chơi chỉ có thể dán decal vào các vị trí này (xem phần vị trí).   
◦ Decal có thể gỡ ra và trở về Inventory (không tiêu hao).   
◦ Cho phép layer ordering (bring forward / send backward).

• UI: khu vực vị trí decal highlight, snap & grid, tool để thay đổi kích thước/rotation (nếu mở).

# 2.3 Sticker (Consumable, AI-gen)

• Mô tả: Hình/Chữ do player tạo qua prompt AI hoặc mua sẵn; dùng một lần khi dán. • Thuộc tính: ID , Name , Type (Text/Image), GeneratedFromPrompt (true/false), PromptMeta (ẩn/ghi chú), AllowedPositions , Rarity , Price (nếu mua), IsConsumable = true .

• Hành vi:

◦ Consumable: dán xong item biến mất. Không thể gỡ để trả lại. Thay sticker mới lên vị trí đã dán sticker cũ mất (bị ghi đè, không hoàn tiền). ◦ ${ \mathsf { H } } { \hat { \mathsf { e } } }$ thống AI-gen: chặn prompt NSFW và nội dung vi phạm (chi tiết moderation bên dưới).

• UI: form nhập prompt, preview, confirm cost (may be free with cooldown / cost currency).

# 3. Vị trí dán (Attachment Points)

• Mỗi loại tank có một tập các Attachment Points được map sẵn (ví dụ: hatch_left , turret_front , rear_panel , side_skirt_left , roof_center , v.v.).

# • Quy tắc:

◦ Warp phủ toàn thân — không bị ràng buộc vị trí.   
◦ Decal & Sticker chỉ có thể đặt vào Attachment Points được định nghĩa cho tank đó.   
◦ Attachment Points không giới hạn loại decal/sticker; chỉ giới hạn vị trí vật phẩm được đặt.

• Thiết kế UX: tại thời điểm chỉnh sửa, attachment points hiển thị highlight, khi hover/drag sẽ cho thấy snap preview.

# 4. Inventory & Item Management (Per-tank)

• Cấu trúc: Inventory lưu trên cơ sở mỗi loại tank. Khi người chơi mở Garage cho tank X:

◦ Hiển thị Warp Inventory (tấm warp đã mua cho tank X)   
◦ Hiển thị Decal Inventory (decal có thể dùng cho tank X)   
◦ Hiển thị Sticker Inventory (consumables, nếu có) — có thể kết hợp cả shop AI-gen output recent.

# • Chức năng:

◦ filter/sort (by Rarity, Collection, Recently Acquired), search, favorite.   
◦ bulk unequip (ví dụ remove all decals).   
◦ preview mode: xem hiện trạng tank với warp+decals+stickers.

• Edge case: nếu người chơi sở hữu warp A cho tank X rồi chuyển tank X sang phiên bản khác (chỉ xảy ra nếu tank có nhiều variant) check compatibility; nếu incompatible, vẫn giữ trong inventory nhưng không thể equip.

# 5. UX Flow (chi tiết hành trình người chơi)

# 5.1. Mở Garage Edit Tank

1. Player chọn Garage từ menu chính.

2. Chọn tank (list tank owned).

3. Hệ thống load preview tank ở center; sidebar trái: Warp Inventory $^ +$ shop actions; sidebar phải: Decal Inventory , Sticker Inventory , Layers và Position map.

4. Người chơi: chọn 1 Warp preview full-body Confirm Apply (instant). (Optional: drag & drop từ inventory lên model).

5. Sau khi warp apply, player chọn decal từ inventory $ \mathsf { k e o }$ vào Attachment Point (snap). Hệ thống hiển thị ghost preview trước khi drop.

6. Nếu chọn sticker: open prompt UI (nếu AI-gen) hoặc chọn sticker consumable từ inventory chọn Attachment Point System show cost & confirm (sticker sẽ bị tiêu khi Apply).

7. Player có thể save preset tên preset $+$ thumbnail. Preset lưu cấu hình warp+layer order+decals để dùng sau.

# 5.2. Mua & Preview từ Shop

• Từ Garage, player bấm Shop thấy warp/decal/sticker dành cho tank hiện tại (filter pertank).

• Có nút Preview on my tank để xem trước. Nếu mua: checkout item vào Inventory của tank đó.

# 5.3. AI-Generate Sticker Flow

1. Player mở Create Sticker nhập prompt text.

2. System chạy moderation (block NSFW/hate/PII) nếu pass generate thumbnail show options variations (3).

3. Player chọn variant confirm cost & position apply sticker consumable giảm 1.

4. Log prompt meta cho moderation & appeals (ẩn cho người khác).

Note: néu player muon save generated image as tradeable item $\Rightarrow$ [Speculation] co théban khoän phi nho dé mint/convert thänh non-consumable -dé xuät δ phän Monetization.

• Cắt ngang NSFW & nội dung cấm: trước khi gửi prompt vào generator, apply filter keyword $+$ classifier. Nếu nghi ngờ $- >$ refuse $^ +$ show reason.

Takedown / Report: người chơi có thể report sticker; hệ thống có cơ chế undo (ban account, rollback).

Privacy: không cho phép input prompts chứa PII (số điện thoại, CC). Nếu phát hiện, từ chối.

• Audit logs: lưu prompt $^ +$ generated hash $^ +$ userID để kiểm tra khiếu nại.

# 7. Monetization (GD đề xuất & options) [Speculation]

Những đề xuất sau là chiến lược GD để tối ưu hóa doanh thu từ Garage. Do anh chưa đưa ra chính sách tiền tệ, từng đề xuất có thể điều chỉnh.

# 7.1 Cơ chế mua/bán cơ bản

• Mua trực tiếp (Direct Purchase): warp/decal/sticker bán theo giá cố định (với tier rarity).   
• Bundle & Sets: bán warp $^ +$ decal combo với giá giảm (khuyến khích cross-sell).   
• Limited Time/Seasonal Skins: skins theo sự kiện (về mùa, collab), tạo FOMO.   
• Presets/Theme Packs: nhiều decals theme, dành cho preset nhanh.

# 7.2 Sticker AI-gen monetization

Pay-per-generate: mỗi lần generate tốn X (currency), free quota mỗi ngày (win retention).

Premium generator: subscription hàng tháng để mở high-res hoặc unlock more variations.

• Consumable packs: bán gói Sticker consumables (e.g., 10 stickers) rẻ hơn mua lẻ.

• One-click conversion: cho phép convert AI sticker thành non-consumable “badge” NFTlike trong game bằng phí (optional / [Speculation]).

# 7.3 Gacha / Mystery Box (Optional) [Speculation]

• Hộp warp/decal “mystery” (gacha) với tỷ lệ drop rarer. Kích thích repeat purchase. Cần tuân thủ luật địa phương về gacha/gambling. (Nếu dùng: minh bạch tỷ lệ)

# 7.4 Cosmetic Progression & Rewards

• Battle pass / Season pass: reward warp/decal tiers as progression incentive.   
• Daily/Weekly Login quests: tặng decal/sticker consumables để drive DAU.   
• Achievements: reward exclusive decal for milestones.

# 7.5 Marketplace & Trading (Optional) [Speculation]

• Cho phép người chơi trade/auction decals giữa nhau (take cut fee). Tăng LTV nhưng cần hệ thống anti-fraud.

# 7.6 Pricing Strategy (example tiers)

• Common Warp: low price (encourage ownership)

• Rare/Epic/Legendary: progressively higher — Legendary limited.

• Decals: cheap/common; special artist/brand decals pricier.

• Stickers: consumable sold in packs; AI generation has per-use fee.

# Theo dõi KPIs (Monetization):

• ARPDAU for cosmetics, Conversion Rate (visit Shop buy), LTV per user, Repeat-purchase rate (buy same collection), Sticker gen ARPU, $\%$ of players using AI-gen.

# 8. Economy & Balance considerations

• Avoid pay-to-win: cosmetics only; ensure no gameplay advantage.

• Prevent inflation: giới hạn supply cho rare items.

• Exchange fairness: nếu có gacha, đảm bảo pity system (garantee rare after N pulls).

• Consumable sticker replacement policy: không refund; nhưng có thể thiết lập “grace” policy (1-time refund for accidental purchase) — [Speculation].

# 9. UX details & Accessibility

• Drag & Drop khi apply warp/decal (optional) — improves tactile feel.

• Undo/Redo cho thao tác chỉnh sửa trước khi Save.

• Snap & Grid cho precise decal placement; kèm tooltip hiển thị thông số (scale/rotation).

Preview Modes: static 360, animated (rotate), battle preview (in-match look).

• Mobile vs PC: tối ưu UI positions — mobile: simplified drag & drop with gestures.

• Save Presets: allow multiple named presets $+$ set primary for matchmaking display.

• Thumbnail auto-gen: for sharing (social) — share preview to socials.

# 10. Social / Display mechanics

• Showcase: tank cosmetics hiển thị trong lobbies, profile, leaderboards.

Emote/Badge: generate small badges from sticker to show under player name (optional).

• Share/Export: share preview images to socials (encourage UGC & acquisition).

# 11. Analytics & Instrumentation (Game Design reqs)

• Track events:

◦ garage_enter , warp_preview , warp_apply , decal_apply , sticker_generate_request , sticker_apply , shop_visit , shop_purchase , preset_save .

• Metrics: Conversion Funnel (visit preview buy), Time Spent in Garage, Average spend per session, Sticker generation acceptance rate, Reported/staged moderation incidents.

# 12. Data model (conceptual, non-tech)

Player owns Tanks[]

Tank has Inventory { Warps[] , Decals[] , Stickers[] } + EquippedState (current warp $^ +$ decals list $^ +$ stickers applied) $^ +$ Presets[]

Item base class with subtype Warp|Decal|Sticker $^ +$ attributes described ở phần 2.

([Inference]): đây là mô hình conceptual phục vụ giao tiếp giữa GD và Dev/PM.

# 13. Roadmap ngắn (GD milestones)

1. Wireframe & UX prototype: Garage main screen, inventory panels, preview.

2. Item spec sheet: templates for warp/decal/sticker metadata.

3. AI-sticker moderation policy doc & testcases.

4. Monetization offers & pricing experiment plan (A/B).

5. KPI dashboard spec.

# 14. Một số đề xuất tối ưu hoá (Monetization & Engagement) — [Speculation]

• Daily free sticker credit (1 free generate per day) để giữ DAU.

• Try-before-buy: cho phép preview limited-time overlay từ shop (xem 1 trận có watermark) tăng conversion.

• Seasonal challenges unlock exclusive decal for limited time retention.

• Social contest: best-decorated tank share award rare decal free marketing.

• Bundle discounts when buy Warp $^ +$ Decal together for same tank. (Tất cả trên cần A/B test.)

# 15. Ghi chú tuân thủ (policy / legal)

Nếu áp dụng gacha: công khai tỉ lệ drop.

• AI-generation: tuân thủ luật bản quyền, không cho phép gen art sao chép verbatim art có bản quyền (design policy cần rõ). [Speculation]

# 16. Tóm tắt ngắn cho PM / Stakeholders

• Garage gồm 3 lớp: Warp (per-tank, permanent), Decal (per-tank, re-usable, attachment-pointlimited), Sticker (per-tank, consumable, AI-gen).

• Inventory lưu theo tank; UX drag&drop $^ +$ preview $+$ presets.

• Monetization đa kênh: direct buy, bundles, sticker-pay-per-gen, seasonal limited items — kèm KPI & moderation.

• Yêu cầu tiếp theo: wireframe UI và danh sách metadata item chuẩn để hand-off.