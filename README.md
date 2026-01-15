🐍 Snake Game (Unity) — Đồ án Cơ sở Ngành CNTT

Học kỳ 1 – Năm học 2024–2025

📌 Giới thiệu

Snake Game là đồ án môn Cơ sở ngành CNTT, được phát triển bằng Unity (C#). Trò chơi mô phỏng lối chơi “Rắn săn mồi” cổ điển, nơi người chơi điều khiển rắn di chuyển trên lưới, ăn thức ăn để tăng độ dài và ghi điểm, đồng thời tránh va chạm với tường hoặc chính thân rắn.

👤 Thông tin sinh viên

Họ tên: Trần Hoàng Long

MSSV: 110123025

Môn học: Cơ sở ngành CNTT

Học kỳ: HK1 (2024–2025)

Giảng viên hướng dẫn: Khấu Văn Nhựt

🔗 Repository

GitHub:

https://github.com/trhlow/CSN-TranHoangLong-110123025-SnakeGame

🎮 Demo

🎥 Video / GIF demo: (chưa có / sẽ cập nhật)

🕹️ Bản build:

Windows: (nếu có)

macOS: (nếu có)

WebGL: (nếu có)

✨ Tính năng chính

Điều khiển rắn di chuyển theo 4 hướng

Cơ chế ăn mồi → tăng độ dài

Hệ thống tính điểm

Xử lý thua cuộc (Game Over) khi:

Va chạm với tường

Va chạm với thân rắn (self-collision)

Chơi lại (Restart) nhanh chóng

Giao diện UI:

Hiển thị điểm số

Trạng thái game (Playing / Game Over / Pause – nếu có)

🛠️ Công nghệ sử dụng

Game Engine: Unity 2022.3.x LTS (hoặc phiên bản bạn dùng)

Ngôn ngữ: C#

Đồ họa / Shader: ShaderLab, HLSL (nếu có)

Mô hình: 2D Grid-based

Language breakdown (GitHub):

C#: ~65%

ShaderLab: ~29%

HLSL: ~6%

📁 Cấu trúc thư mục 
Assets/ │ ├── Scripts/ # Logic gameplay (Snake, Food, GameManager, UI…) 
          ├── Scenes/ # Scene chính của game 
          ├── Prefabs/ # Prefab rắn, thức ăn, UI 
          ├── Shaders/ # Shader / hiệu ứng hình ảnh 
          │ ProjectSettings/ # Cấu hình project Unity

▶️ Hướng dẫn chạy project Yêu cầu

Unity Hub

Unity phiên bản: 2022.3.x LTS (hoặc đúng phiên bản project)

Git

Các bước

Clone repository:

git clone https://github.com/trhlow/CSN-TranHoangLong-110123025-SnakeGame.git

Mở Unity Hub → Add project → chọn thư mục vừa clone.

Mở scene chính:

Assets/Scenes/MainScene.unity

Nhấn Play để chạy game.

🕹️ Cách chơi

Người chơi điều khiển rắn di chuyển liên tục trên bản đồ

Ăn thức ăn để:

Tăng điểm

Tăng độ dài rắn

Tránh:

Đâm vào tường

Đâm vào thân rắn

🎯 Điều khiển (Controls) Hành động Phím Lên W / ↑ Xuống S / ↓ Trái A / ← Phải D / → Pause (nếu có) Esc / P Restart (nếu có) R 📜 Luật chơi

Rắn di chuyển liên tục theo hướng hiện tại

Khi ăn thức ăn:

+1 điểm

Tăng 1 đốt thân

Có thể tăng tốc độ (chưa triển khai)

Game kết thúc khi:

Rắn va chạm với tường

Rắn va chạm với chính thân của nó

⚙️ Ghi chú kỹ thuật

Game sử dụng grid-based movement để đảm bảo di chuyển chính xác

Thức ăn được spawn ngẫu nhiên và không trùng vị trí thân rắn

Xử lý va chạm bằng:

OnTriggerEnter2D / OnCollisionEnter2D

Kiến trúc tách biệt:

GameManager

SnakeController

FoodSpawner

UI Manager

🏗️ Build game Build cho Windows / macOS

Unity → File → Build Settings

Chọn PC, Mac & Linux Standalone

Add Open Scenes

Build

Build WebG

Build Settings → chọn WebGL

Switch Platform

Build

Upload lên GitHub Pages hoặc itch.io

📄 Báo cáo & tài liệu

📘 Báo cáo đồ án: 

📊 Slide thuyết trình: 

🤝 Đóng góp

Repository này phục vụ mục đích học tập. Mọi góp ý hoặc cải tiến đều được hoan nghênh thông qua:

Issue

Pull Request

📜 License

Dự án phục vụ mục đích học tập

All rights reserved (Có thể đổi sang MIT License nếu muốn public hoàn toàn)
