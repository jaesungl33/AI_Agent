🔷 1. Màn hình chính (Main Menu)
	➡️ Player bấm nút Tìm trận (Find Match)

🔷 2. Module Matchmaking
	➡️ Gửi request ghép trận (Firebase / backend API)
	➡️ Nhận về MatchInfo (session name, game mode, map id, match id)

🔷 3. Module MatchManager (Match Start)
	➡️ Dùng MatchInfo để gọi runner.StartGame()
	➡️ Load scene bản đồ phù hợp (map id từ MatchInfo)

🔷 4. Module MapManager
	➡️ Tạo bản đồ (load hoặc instantiate prefab)
	➡️ Tạo các đối tượng chung:
		✅Map
		✅Network Towers
		✅Shop
		✅Network Props
		✅PlayerTank

🔷 5. Module PlayerSpawnManager
	➡️ Spawn nhân vật player vào vị trí spawn point theo team

🔷 6. Module CountdownManager
	➡️ Đếm ngược (3..2..1..Start) trước khi bắt đầu trận

🔷 7. Gameplay Loop
	➡️ Game running 
		✅Combat
		✅Skill
		✅Kill
		✅Tower destroy
🔷 8. Module MatchResultManager
	➡️ Khi kết thúc game:
		✅Tính kết quả (thắng/thua, elo change)
		✅Hiển thị UI Result

🔷 9. Back to Main Menu