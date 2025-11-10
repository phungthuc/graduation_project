# The Tunnel

![Unity Version](https://img.shields.io/badge/Unity-6000.2.5f1-blue)
![License](https://img.shields.io/badge/License-Proprietary-red)

## 📝 Mô Tả

**The Tunnel** là một game Unity 3D kết hợp tower defense và dungeon crawling với cơ chế FPS (First Person Shooter). Đây là dự án tốt nghiệp được phát triển sử dụng Cowsins FPS Engine.

### Gameplay

Game bao gồm 2 chế độ chơi chính:

1. **Defense Mode (Chế độ Phòng Thủ)**: Người chơi cần bảo vệ lâu đài khỏi các đợt tấn công của kẻ thù trong thời gian giới hạn (5 giây giữa các level).

2. **Dungeon Mode (Chế độ Dungeon)**: Người chơi khám phá các dungeon và tiêu diệt tất cả kẻ thù để hoàn thành level.

## 🎮 Cơ Chế Game

### Quy Trình Chơi
- Sau khi hoàn thành defense level, người chơi có 5 giây để chuẩn bị
- Sau đó tự động chuyển sang dungeon mode
- Hoàn thành dungeon để qua level tiếp theo
- Game có 10 level, độ khó tăng dần

### Enemy System
- **Melee Enemies**: Kẻ thù cận chiến
- **Range Enemies**: Kẻ thù tầm xa
- **Sword Enemies**: Kẻ thù dùng kiếm
- **Boss Golem**: Boss cuối mỗi wave
- **Dungeon Enemies**: Spider, Giant Worm, Gunner (trong dungeon)

### Castle Defense
- Lâu đài có máu giới hạn (5 HP)
- Khi lâu đài hết máu, người chơi game over
- Hiển thị thanh máu trên UI

## 🛠️ Công Nghệ & Dependencies

### Unity Version
- **Unity**: 6000.2.5f1

### Packages chính
- **Cowsins FPS Engine**: Engine FPS chính
- **com.unity.inputsystem**: 1.14.2 - Hệ thống input mới
- **com.unity.ai.navigation**: 2.0.9 - Navigation AI
- **com.unity.postprocessing**: 3.5.0 - Post-processing effects
- **com.unity.render-pipelines.universal**: 17.2.0 - URP
- **com.crashkonijn.goap**: 2.1.22 - GOAP AI
- **com.unity.nuget.newtonsoft-json**: 3.2.1 - JSON serialization

### GOAP (Goal-Oriented Action Planning)
Game sử dụng hệ thống GOAP cho AI:
- **NPC Behavior**: NPC có thể tấn công, ẩn nấp, hồi máu, di chuyển
- **Enemy AI**: Kẻ thù có thể tuần tra, tìm cover, tấn công người chơi
- **Dynamic Goals**: AI tự động quyết định hành động dựa trên tình huống

## 📁 Cấu Trúc Dự Án

```
Assets/
├── Code/
│   ├── Cowsins/           # FPS Engine của Cowsins
│   │   ├── Scripts/
│   │   │   ├── Movement/  # PlayerMovement, Editor tools
│   │   │   ├── UI/        # UI Controller
│   │   │   ├── Weapon/    # Vũ khí
│   │   │   └── ...
│   │   ├── Prefabs/       # Player, weapons, effects
│   │   ├── Materials/     # Vật liệu
│   │   └── ...
│   └── TheTunnel/         # Game logic chính
│       ├── GameManager.cs      # Quản lý game flow
│       ├── Level/
│       │   ├── LevelManager.cs # Quản lý levels
│       │   └── LevelData.cs    # Cấu trúc dữ liệu level
│       ├── Enemy/
│       │   ├── EnemyManager.cs          # Quản lý enemies trong defense mode
│       │   ├── DungeonEnemyManager.cs   # Quản lý enemies trong dungeon
│       │   └── GOAP/                    # AI system
│       ├── Castle/          # Castle defense
│       ├── NPC/             # NPC system với GOAP
│       ├── Player/          # PlayerData
│       ├── Components/      # TimeCounter, UI helpers
│       ├── Dungeon/         # Dungeon events, teleport gates
│       ├── Turret/          # Turret system
│       └── Weapon/          # Punch weapon
├── Level/
│   ├── GameData/        # Level data, enemy data
│   ├── Prefabs/         # Prefabs của game
│   └── Scenes/          # scene_play, dungeon_gameplay
├── Audio/               # Âm thanh
├── Settings/            # Game settings
└── Tools/               # Hot reload, TextMesh Pro
```

## 🎯 Hệ Thống Quan Trọng

### 1. GameManager
- Quản lý countdown giữa các level
- Singleton pattern
- Event-driven architecture

### 2. LevelManager
- Chuyển đổi giữa defense mode và dungeon mode
- Quản lý enemy spawning
- Player death handling

### 3. EnemySystem
- **EnemyManager**: Quản lý waves trong defense mode
- **DungeonEnemyManager**: Quản lý enemies trong dungeon
- Wave-based spawning
- Event system cho enemy spawn/death

### 4. GOAP AI
Dựa trên plugin com.crashkonijn.goap:
- **Goals**: KillPlayerGoal, HideGoal, FindCoverGoal
- **Actions**: Melee/Ranged attack, TakeCover, Patrol
- **Sensors**: PlayerTargetSensor, CoverTargetSensor, DistanceSensor
- **Dynamic Planning**: AI tự động chọn hành động tốt nhất

### 5. Data Persistence
- `PlayerData`: Lưu level hiện tại và lịch sử gameplay
- Sử dụng PlayerPrefs và JSON serialization
- Tracking dungeon completion

## 🎨 Assets

### 3D Models
- Castle low-poly
- Enemy models (Melee, Range, Golem)
- Dungeon environments
- Effects (Explosion, Fire, Fog)

### UI
- Countdown timer
- Health sliders
- Transition scenes
- Turret UI

## 🎮 Controls

### Player Movement (Cowsins Engine)
- **WASD**: Di chuyển
- **Space**: Nhảy
- **Shift**: Chạy
- **Ctrl**: Cúi
- **Mouse**: Nhìn xung quanh
- **Left Click**: Bắn
- **R**: Reload

### Cheat Codes
- Giữ phím **3** trong 3 giây để thoát dungeon level

## 📊 Level System

Game có 10 level, mỗi level gồm:
1. **Defense Phase**: Giết tất cả enemies trong wave
2. **Dungeon Phase**: Khám phá và hoàn thành dungeon

### Level Data Format (JSON)
```json
{
  "name": "Level 1",
  "waveData": [
    {
      "enemyData": [
        {"enemyId": "melee", "amount": 2},
        {"enemyId": "sword", "amount": 1}
      ],
      "timeNextWave": 5
    }
  ],
  "dungeon": "prefab_dungeon_1",
  "dungeonData": {
    "playerPosition": "x,y,z",
    "enemySpawnData": {
      "zoneName": {
        "spawnPosition": "x,y,z",
        "enemyData": [...]
      }
    }
  }
}
```

## 🔧 Build Settings

### Target Platforms
- Standalone (Windows)
- Minimum resolution: 1920x1080

### Unity Settings
- Default scripting backend: Mono (.NET Framework)
- Graphics API: Direct3D 11
- Color Space: sRGB
- Post-processing: Enabled

## 🚀 Cài Đặt và Chạy

### Yêu Cầu Hệ Thống
- Unity 6000.2.5f1
- Windows 10/11
- DirectX 11 compatible GPU

### Cách Mở Project
1. Clone repository
2. Mở Unity Hub
3. Add project từ thư mục dự án
4. Chọn Unity version 6000.2.5f1
5. Mở scene `Assets/Level/Scenes/scene_play.unity`

### Dependencies
Các package sẽ tự động được tải từ Unity Package Manager:
- Cowsins FPS Engine
- GOAP framework
- Post-processing stack
- Input System

## 🎓 Tính Năng

### ✅ Đã Hoàn Thành
- [x] Defense mode với wave system
- [x] Dungeon mode với enemy spawning
- [x] GOAP AI cho NPC và enemies
- [x] Castle defense mechanics
- [x] Player movement với Cowsins engine
- [x] 10 levels với cấu hình JSON
- [x] UI system (countdown, health)
- [x] Audio system
- [x] Transition scenes
- [x] Player data persistence

### 🔄 Có Thể Mở Rộng
- Multiplayer
- More weapon types
- Skill system
- Loot/upgrade system
- More enemy types
- Boss battle mechanics
- Achievement system

## 📝 Lưu Ý

### Development
- Project sử dụng HOT RELOAD để phát triển nhanh
- Cowsins FPS Engine đã được tùy chỉnh với extensions trong `Assets/Code/TheTunnel/Extensions/Cowsins`
- GOAP system được tích hợp sâu vào enemy và NPC behavior

### Performance
- Static batching được bật cho Standalone builds
- MTRendering enabled cho mobile platforms
- VFX pool system để optimize

## 👤 Thông Tin

- **Tên Project**: The Tunnel
- **Loại**: Unity 3D Game - Graduation Project
- **Engine**: Unity 6000.2.5f1
- **Platform**: Windows Standalone
- **Genre**: Tower Defense + Dungeon Crawler FPS

## 📄 License

Đây là dự án tốt nghiệp, tất cả code và assets thuộc quyền sở hữu của tác giả.

---

**Lưu ý**: Dự án này tích hợp Cowsins FPS Engine và GOAP framework. Vui lòng tham khảo documentation của các engine này để hiểu rõ hơn về các chức năng.
