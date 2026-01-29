# Snake Game (Unity) — Đồ án Cơ sở Ngành CNTT (HK1 2024–2025)

**Snake Game** là đồ án môn *Cơ sở ngành Công nghệ Thông tin* (HK1 – Năm học 2024–2025), phát triển bằng **Unity + C#**. Trò chơi mô phỏng gameplay “Rắn săn mồi” cổ điển: người chơi điều khiển rắn ăn mồi để tăng điểm và tăng độ dài, đồng thời tránh va chạm để không bị **Game Over**.

- Repository: https://github.com/trhlow/CSN-TranHoangLong-110123025-SnakeGame
- Branch đang dùng: `new`

---

## Thông tin sinh viên
- **Họ tên**: Trần Hoàng Long  
- **MSSV**: 110123025  
- **Môn học**: Cơ sở ngành CNTT  
- **Học kỳ**: HK1 (2024–2025)  
- **Giảng viên hướng dẫn**: Khấu Văn Nhựt  

---

## Tài liệu đính kèm trong repo
Repo có kèm các file phục vụ báo cáo và thuyết trình:

- Báo cáo: `CSN-110123025-TrầnHoàngLong-SnakeGame-BCCSN.pdf`
- Poster: `Poster.pdf`, `POSTERPDFFINAL.pdf`
- Slide: `slideCSN.pptx`
- Gói nén (nếu cần): `SnakeGame.rar`
---

## Tính năng chính
- Điều khiển rắn di chuyển theo **4 hướng**
- Cơ chế **ăn mồi → tăng độ dài**
- **Hệ thống tính điểm**
- **Game Over** khi:
  - Va chạm với **tường**
  - Va chạm với **thân rắn** (self-collision)
- **Restart** (chơi lại) nhanh chóng
- UI cơ bản:
  - Hiển thị điểm số
  - Trạng thái game (Playing / Game Over / Pause — nếu có)

---

## Công nghệ sử dụng
- **Game Engine**: Unity **6000.2.7f2** (theo `ProjectSettings/ProjectVersion.txt`)
- **Ngôn ngữ**: C#
- **Shader/Đồ hoạ**: ShaderLab, HLSL (repo có sử dụng shader)

**Language breakdown (GitHub)**:
- C#: ~65.4%
- ShaderLab: ~28.7%
- HLSL: ~5.9%

---

## Cách chơi
- Người chơi điều khiển rắn di chuyển liên tục trên bản đồ.
- Ăn thức ăn ��ể:
  - Tăng điểm
  - Tăng độ dài rắn
- Tránh:
  - Đâm vào tường
  - Đâm vào thân rắn

---

## Điều khiển (Controls)
| Hành động | Phím |
|---------|-----------|
| Lên     |   W  /  ↑ |
| Xuống   |   S  /  ↓ |
| Trái    |   A  /  ← |
| Phải    |   D  /  → |
| Pause   |  Esc /  P |
| Restart |      R    |
|---------------------|
---

## Luật chơi (Gameplay rules)
- Rắn di chuyển liên tục theo hướng hiện tại.
- Khi ăn thức ăn:
  - +1 điểm *(hoặc theo luật tính điểm trong game)*
  - Tăng thêm 1 đốt thân
  - Có thể tăng tốc theo thời gian *(tuỳ bản triển khai)*
- Game kết thúc khi:
  - Rắn va chạm với tường
  - Rắn va chạm với chính thân của nó

---

## Hướng dẫn chạy project (Unity)
### Yêu cầu
- Unity Hub
- Unity Editor: **6000.2.7f2**
- Git

### Chạy game trong Editor
1. Clone repo:
   ```bash
   git clone https://github.com/trhlow/CSN-TranHoangLong-110123025-SnakeGame.git
   cd CSN-TranHoangLong-110123025-SnakeGame
   ```
2. Mở **Unity Hub** → **Add** → chọn thư mục project.
3. Mở scene chính:
   - `[Điền tên scene chính, ví dụ: Assets/Scenes/MainScene.unity]`
4. Nhấn **Play** để chạy.

> Nếu bạn cho mình biết chính xác scene nào là scene chính (hoặc mình truy cập được `Assets/Scenes`), mình sẽ điền đúng đường dẫn scene vào README.

---

## Cấu trúc thư mục (tham khảo)
Dự án Unity thường có cấu trúc tương tự:

- `Assets/`
  - `Scripts/` — logic gameplay (Snake, Food, GameManager, UI…)
  - `Scenes/` — scene chính của game
  - `Prefabs/` — prefab rắn, thức ăn, UI…
  - `Shaders/` — shader/hiệu ứng hình ảnh
  - `Resources/` — tài nguyên load runtime (nếu dùng)
- `Packages/` — package manifest
- `ProjectSettings/` — cấu hình project Unity

---

## Ghi chú kỹ thuật (Implementation notes)
- Game có thể áp dụng **grid-based movement** để đảm bảo rắn di chuyển chính xác theo ô (grid).
- Food nên spawn ngẫu nhiên và **không trùng** vị trí với thân rắn.
- Xử lý va chạm thường dùng:
  - `OnTriggerEnter2D` / `OnCollisionEnter2D` (tuỳ cách setup Collider/Rigidbody)
- Tổ chức code gợi ý (có thể khác trong project):
  - `GameManager`
  - `SnakeController`
  - `FoodSpawner`
  - `UIManager`

---

## Build (Xuất bản game)
### Build Windows / macOS
1. Unity → **File** → **Build Settings**
2. Chọn **PC, Mac & Linux Standalone**
3. **Add Open Scenes**
4. **Build**

### Build WebGL *(nếu có)*
1. Build Settings → chọn **WebGL**
2. **Switch Platform**
3. **Build**
4. Upload bản build lên GitHub Pages hoặc itch.io.

---

## Đóng góp
Repo phục vụ mục đích học tập. Mọi góp ý/cải tiến đều được hoan nghênh qua:
- **Issues**
- **Pull Requests**

---

## License
Dự án phục vụ mục đích học tập.  
Mặc định: **All rights reserved** *(bạn có thể đổi sang MIT License nếu muốn public và cho phép tái sử dụng)*.
