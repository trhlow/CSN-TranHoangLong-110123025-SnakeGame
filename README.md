# 🐍 Snake Game (Unity / C#)

### Academic Project – Game Development Fundamentals

**Snake Game** là một dự án game 2D được phát triển bằng **Unity (C#)**, dựa trên gameplay “Rắn săn mồi” cổ điển.
Dự án tập trung vào **gameplay logic, điều khiển nhân vật, xử lý va chạm, quản lý trạng thái game và UI cơ bản**, phù hợp làm **portfolio cho vị trí Game Developer Intern / Fresher Unity Developer**.

---

## 👨‍💻 Developer

* **Name:** Trần Hoàng Long
* **Student ID:** 110123025
* **Major:** Information Technology
* **Course:** Core IT Foundations (HK1 – 2024–2025)
* **Supervisor:** Khấu Văn Nhựt

---

## 🔗 Project Links

* **GitHub Repository:**
  [https://github.com/trhlow/CSN-TranHoangLong-110123025-SnakeGame](https://github.com/trhlow/CSN-TranHoangLong-110123025-SnakeGame)
* **Active Branch:** new

---

## 🎯 Project Goals

* Xây dựng một game 2D hoàn chỉnh bằng **Unity + C#**
* Áp dụng các kiến thức nền tảng:

  * Game loop
  * Player input handling
  * Collision detection
  * State management (Playing / Game Over / Restart)
* Rèn luyện tư duy **tách logic – quản lý đối tượng – tổ chức code**

---

## ✨ Key Features

* 2D **grid-based snake movement**
* 4-direction player input (Keyboard)
* Food spawning & snake growth mechanic
* Score tracking system
* **Self-collision & wall-collision detection**
* Game states:

  * Playing
  * Game Over
  * Restart
* Basic UI:

  * Score display
  * Game Over state

---

## 🛠️ Tech Stack

* **Engine:** Unity **6000.2.7f2**
* **Language:** C#
* **Rendering:** 2D
* **Shaders:** ShaderLab, HLSL
* **Platform:** PC (Editor), expandable to WebGL

**GitHub language usage:**

* C#: ~65%
* ShaderLab: ~29%
* HLSL: ~6%

---

## 🎮 Gameplay Overview

* Snake moves continuously in the current direction.
* Eating food:

  * Increases score
  * Increases snake length
* Game ends when the snake:

  * Hits the wall
  * Collides with its own body

---

## ⌨️ Controls

| Action             | Key         |
| ------------------ | ----------- |
| Move Up            | `W` / `↑`   |
| Move Down          | `S` / `↓`   |
| Move Left          | `A` / `←`   |
| Move Right         | `D` / `→`   |
| Pause *(optional)* | `Esc` / `P` |
| Restart            | `R`         |

---

## 🧠 Technical Implementation Highlights

* **Grid-based movement** to ensure precise and predictable snake behavior
* Food spawning logic avoids overlapping with snake body
* Collision handling via:

  * `OnTriggerEnter2D` / `OnCollisionEnter2D`
* Modular code structure (separation of concerns):

  * GameManager` – game state & flow control
  * SnakeController` – movement & body growth
  * FoodSpawner` – food generation logic
  * UIManager` – score & state display

---

## 📁 Project Structure

text
Assets/
├── Scripts/
│   ├── GameManager.cs
│   ├── SnakeController.cs
│   ├── FoodSpawner.cs
│   └── UIManager.cs
├── Scenes/
│   └── MainScene.unity
├── Prefabs/
│   ├── Snake.prefab
│   └── Food.prefab
├── Shaders/
└── Resources/
Packages/
ProjectSettings/
```

---

## ▶️ Run the Project

### Requirements

* Unity Hub
* Unity Editor **6000.2.7f2**
* Git

### Steps

```bash
git clone https://github.com/trhlow/CSN-TranHoangLong-110123025-SnakeGame.git
cd CSN-TranHoangLong-110123025-SnakeGame
```

1. Open **Unity Hub** → **Add Project**
2. Select project folder
3. Open `Assets/Scenes/MainScene.unity`
4. Press **Play**

---

## 📄 Documentation

This repository includes academic documentation:

* 📘 Final report (PDF)
* 📊 Presentation slides
* 🖼️ Poster

> These documents demonstrate the ability to **explain design decisions and technical implementation**, not only coding.

---

## 🚀 Future Improvements

* Difficulty scaling (speed increase over time)
* Sound effects & background music
* Mobile (touch) controls
* High-score persistence (PlayerPrefs)
* WebGL build & online demo

---

## 📌 Why This Project Matters

This project demonstrates:

* Solid understanding of **Unity fundamentals**
* Ability to complete a **full gameplay loop**
* Clean and structured C# scripting
* Readiness for **Game Developer Intern / Fresher Unity Developer** roles

---

## 📜 License

Educational project – **All rights reserved**.
(Open to re-licensing if required.)

---

### 📬 Contact

If you’re a recruiter or reviewer and would like to know more about this project or my skills, feel free to reach out via GitHub.

