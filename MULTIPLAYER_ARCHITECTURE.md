# HỆ THỐNG MULTIPLAYER CHO "THE TUNNEL"

## 📋 TÓM TẮT ĐỀ XUẤT

**Giải pháp khuyến nghị:** Unity Netcode for GameObjects (Netcode for GameObjects - NGO)
- ✅ **Miễn phí** và được Unity hỗ trợ chính thức
- ✅ **Ấn định và ổn định cao** cho team nhỏ và độ phức tạp vừa phải
- ✅ **Tích hợp sẵn** trong Unity 6000.2.5f1
- ✅ **Documentation đầy đủ** và community hỗ trợ
- ✅ **Chi phí hosting thấp** - có thể tự host hoặc dùng relay server miễn phí

**Phương án dự phòng:**
- Mirror Networking (mã nguồn mở, community lớn)
- Photon PUN2 (phí trả theo lượt chơi, phù hợp giãn từng giai đoạn)

---

## 🏗️ KIẾN TRÚC TỔNG QUAN

### 1. Mô hình mạng: **Authoritative Client-Server**

```
┌─────────────────────────────────────────────────────────┐
│                     DEDICATED SERVER                      │
│  ┌───────────────────────────────────────────────────┐   │
│  │  NetworkManager (Unity NGO)                       │   │
│  │  - Quản lý kết nối                                │   │
│  │  - Spawn manager                                   │   │
│  └───────────────────────────────────────────────────┘   │
│  ┌───────────────────────────────────────────────────┐   │
│  │  GameSession Manager                              │   │
│  │  - Quản lý lobby/room                             │   │
│  │  - Max players: 4                                 │   │
│  │  - Session state                                  │   │
│  └───────────────────────────────────────────────────┘   │
│  ┌───────────────────────────────────────────────────┐   │
│  │  Game Logic Authority                            │   │
│  │  - Wave spawning                                  │   │
│  │  - Castle HP                                      │   │
│  │  - Enemy spawning                                │   │
│  │  - Dungeon events                                 │   │
│  └───────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                           ▲
                           │ NetworkMessage
                           │ (UDP/TCP Hybrid)
                    ┌──────┴──────┐
                    │              │
         ┌──────────▼───┐   ┌──────▼──────────┐
         │   CLIENT 1    │   │    CLIENT 2     │
         │  (Host/NGO)  │   │  (Client/NGO)  │
         ├───────────────┤   ├─────────────────┤
         │ Player Input  │   │  Player Input   │
         │ Rendering     │   │  Rendering      │
         │ Prediction    │   │  Prediction     │
         └───────────────┘   └─────────────────┘
                  ▲                  ▲
                  └──────┬───────────┘
                         │ State Sync
```

---

## 🧩 CẤU TRÚC HỆ THỐNG CHI TIẾT

### 2.1. Network Layer Architecture

#### **A. Core Network Components**

```
Assets/Code/TheTunnel/Network/
├── Core/
│   ├── NetworkGameManager.cs          # Quản lý phiên chơi multiplayer
│   ├── NetworkSession.cs               # Quản lý session, room state
│   ├── NetworkAuthentication.cs        # Xác thực player
│   └── NetworkSceneManager.cs          # Quản lý scene multiplayer
├── Messages/
│   ├── INetworkMessage.cs              # Interface cho network messages
│   ├── JoinGameMessage.cs               # Client -> Server: Join request
│   ├── StartDefenseMessage.cs           # Server -> Client: Start defense
│   ├── WaveCompleteMessage.cs           # Server -> Client: Wave complete
│   ├── CastleDamageMessage.cs           # Server -> Client: Castle damaged
│   └── PlayerDeathMessage.cs            # Client -> Server: Player died
├── Synchronization/
│   ├── NetworkTransformExtension.cs     # Custom network transform
│   ├── NetworkHealth.cs                 # Đồng bộ máu player
│   ├── NetworkPlayerController.cs       # Đồng bộ input và movement
│   └── NetworkWeaponHandler.cs          # Đồng bộ shooting
└── Authority/
    ├── ServerAuthoritative.cs          # Server authority cho game logic
    ├── WaveSpawnAuthority.cs            # Chỉ server spawn wave
    └── EnemyAuthority.cs                # Server quản lý enemy
```

**Chức năng chính:**
- `NetworkGameManager`: Entry point, quản lý kết nối
- `NetworkSession`: Quản lý room state, player list, game state
- Messages: Giao tiếp client-server dùng Network RPC

---

### 2.2. Game Logic Layer - Multiplayer Extensions

#### **A. Defense Mode Multiplayer**

```
Assets/Code/TheTunnel/Network/
├── Defense/
│   ├── NetworkDefenseManager.cs        # Quản lý defense mode MP
│   │   └── Server spawn waves cho tất cả players
│   ├── NetworkCastle.cs                # Đồng bộ castle HP
│   │   └── Sync HP từ server -> all clients
│   ├── NetworkWaveSpawner.cs          # Wave spawning authority
│   │   └── Chỉ server spawn, clients receive events
│   └── SharedDefenseState.cs           # State đồng bộ
│       └── Current wave, enemies alive, castle HP
└── Dungeon/
    ├── NetworkDungeonManager.cs       # Quản lý dungeon MP
    ├── NetworkDungeonSpawner.cs       # Enemy spawn trong dungeon
    └── SharedDungeonState.cs          # Progress, zones discovered
```

**Workflow Defense Mode:**
```
1. Server: LoadDefenseLevel() -> Spawn enemies theo wave
2. Server -> All Clients: WaveStarted event
3. Clients: Render enemies, players
4. Server: Check enemy deaths, castle damage
5. Server -> All Clients: Update HP, enemies
6. When wave complete:
   - Server -> All Clients: WaveComplete event
   - Wait 5s countdown (synchronized)
   - Server -> All Clients: StartDungeon event
```

---

#### **B. Enemy System - Multiplayer**

```
Assets/Code/TheTunnel/Enemy/
├── Network/
│   ├── NetworkEnemyBase.cs            # Base cho network enemy
│   │   ├── NetworkTransform (position sync)
│   │   ├── NetworkHealth (HP sync)
│   │   └── NetworkState (AI state sync)
│   ├── NetworkEnemyManager.cs         # Quản lý enemy spawning MP
│   │   ├── Server authority
│   │   ├── Spawn tracking
│   │   └── Sync to clients
│   └── NetworkEnemyAttack.cs          # Đồng bộ attacks
│       └── Server validates, clients receive
```

**Enemy Spawning Strategy:**
```
Server (Authority):
├── Calculate spawn positions
├── Select enemy types
├── Spawn NetworkObjects
└── Notify clients

Clients:
├── Receive spawn data
├── Instantiate local instances
└── Apply NetworkObject references
```

---

#### **C. Player System - Multiplayer**

```
Assets/Code/TheTunnel/Player/
├── Network/
│   ├── NetworkPlayerManager.cs        # Quản lý player instances
│   │   ├── Track active players
│   │   ├── Handle disconnections
│   │   └── PlayerList sync
│   ├── NetworkPlayerController.cs     # Player movement & input
│   │   ├── Input authority (owner)
│   │   ├── Server validation
│   │   └── Prediction + reconciliation
│   ├── NetworkPlayerHealth.cs         # Health synchronization
│   │   ├── Server authority
│   │   └── Damage sync to all
│   ├── NetworkPlayerInventory.cs      # Weapon/items sync
│   └── NetworkPlayerUI.cs            # UI cho player state
│       ├── HP bar
│       ├── Ammo count
│       └── Player list
```

**Player Authority Model:**
```
Ownership (Client):
├── Input collection (WASD, mouse)
├── Prediction
└── Send to server

Server:
├── Validate input
├── Apply physics
├── Check collisions
└── Broadcast state to all clients

Clients (Non-Owner):
├── Receive state from server
└── Interpolate/display
```

---

### 2.3. Synchronization Strategy

#### **A. Data Synchronization Matrix**

| Component | Update Frequency | Authority | Priority | Method |
|-----------|------------------|-----------|----------|--------|
| Player Position | 30 Hz | Server | High | NetworkTransform |
| Player Rotation | 30 Hz | Owner → Server | High | Custom RPC |
| Player Health | Event-based | Server | Critical | NetworkVariable |
| Castle HP | Event-based | Server | Critical | NetworkVariable |
| Enemy Position | 20 Hz | Server | Medium | NetworkTransform |
| Enemy AI State | 10 Hz | Server | Low | Custom RPC |
| Weapon Animations | Client-side | Owner | Low | Local only |
| Particle Effects | Client-side | None | Low | Local only |

#### **B. Bandwidth Optimization**

```
Tính toán băng thông ước tính:

Player (4 players):
├── Position: ~40 bytes × 30Hz = 4800 bytes/s per player
├── Rotation: ~12 bytes × 30Hz = 360 bytes/s per player
├── Input: ~32 bytes × 30Hz = 960 bytes/s per player
└── Health: ~4 bytes (event-based) = negligible
Total per player: ~6 KB/s

Enemies (20 enemies avg):
├── Position: ~40 bytes × 20Hz × 20 = 16 KB/s
├── Rotation: ~12 bytes × 10Hz × 20 = 2.4 KB/s
└── State: ~8 bytes × 10Hz × 20 = 1.6 KB/s
Total: ~20 KB/s

Other:
├── Game state: ~100 bytes × 5Hz = 500 bytes/s
├── Events: ~500 bytes (event-based) = negligible
└── UI updates: ~200 bytes (event-based) = negligible
Total: ~700 bytes/s

TOTAL ESTIMATE: ~27 KB/s per client (acceptable for most connections)

Chiến lược optimization:
├── 1. Culling: Chỉ sync enemies trong range 100m
├── 2. Compression: Vector3 quantization
├── 3. Delta compression cho transform
└── 4. Interest management: Players chỉ nhận updates của nearby objects
```

---

### 2.4. Game State Management

#### **Game Flow cho Multiplayer:**

```
┌────────────────────────────────────────────────────────┐
│                     LOBBY STATE                        │
│  - Players join/customize                             │
│  - Host starts game                                   │
└────────────────┬──────────────────────────────────────┘
                 ▼
┌────────────────────────────────────────────────────────┐
│                  DEFENSE STATE                         │
│  Server:                                              │
│  ├── Spawn wave (authoritative)                       │
│  ├── Track enemies                                     │
│  ├── Validate damage                                  │
│  └── Broadcast state updates                          │
│                                                       │
│  Clients:                                             │
│  ├── Display enemies                                  │
│  ├── Player input → Server                           │
│  └── Receive & apply state updates                    │
└────────────────┬──────────────────────────────────────┘
                 ▼
┌────────────────────────────────────────────────────────┐
│                   TRANSITION STATE                     │
│  - 5 second countdown (synchronized)                  │
│  - Server coordinates transition                      │
└────────────────┬──────────────────────────────────────┘
                 ▼
┌────────────────────────────────────────────────────────┐
│                  DUNGEON STATE                         │
│  Server:                                              │
│  ├── Spawn dungeon (authoritative)                    │
│  ├── Manage zone events                               │
│  └── Track progress                                   │
│                                                       │
│  Clients:                                             │
│  ├── Explore together                                  │
│  ├── Shared progression                                │
│  └── Cooperative gameplay                             │
└────────────────┬──────────────────────────────────────┘
                 ▼
         ┌────────────┐       ┌──────────┐
         │  VICTORY  │       │  DEFEAT  │
         └────────────┘       └──────────┘
```

---

## 🔧 COMPONENTS CẦN PHÁT TRIỂN

### 3.1. Network Components (New)

**Core Networking (High Priority):**
1. ✅ `NetworkGameManager` - Main entry point
2. ✅ `NetworkPlayerController` - Player movement sync
3. ✅ `NetworkHealth` - HP synchronization
4. ✅ `NetworkTransform` - Position sync
5. ✅ `NetworkCastle` - Castle HP sync

**Game Logic (High Priority):**
6. ✅ `NetworkEnemyManager` - Enemy spawning authority
7. ✅ `NetworkWaveManager` - Wave management
8. ✅ `NetworkDefenseManager` - Defense mode controller
9. ✅ `NetworkDungeonManager` - Dungeon mode controller

**Synchronization (Medium Priority):**
10. ✅ `NetworkProjectileHandler` - Bullet sync
11. ✅ `NetworkWeaponHandler` - Weapon state sync
12. ✅ `NetworkUIHandler` - UI state sync

**Utilities (Low Priority):**
13. ✅ `NetworkDebugger` - Debug tools
14. ✅ `NetworkProfiler` - Performance monitoring
15. ✅ `NetworkAnalytics` - Metrics collection

---

### 3.2. Modified Components

**Cần chỉnh sửa các components hiện có:**

```
GameManager.cs:
├── Thêm NetworkSession support
├── Multiplayer countdown sync
└── Server authority validation

LevelManager.cs:
├── Network authoritative loading
├── Sync level transitions
└── Multiplayer state management

EnemyManager.cs:
├── NetworkObject support
├── Server authority cho spawning
└── Sync enemy death events

EnemyBase.cs:
├── Inherit NetworkBehaviour
├── Health synchronization
└── Animation sync (optional)

Castle.cs:
├── NetworkObject support
├── HP sync to all clients
└── Death event broadcast

PlayerMovement.cs:
├── Network owner check
├── Input authority validation
└── Server reconciliation
```

---

## 🎮 GAMEPLAY MECHANICS - MULTIPLAYER

### 4.1. Cooperative Defense Mode

**Mục tiêu:**
- Người chơi bảo vệ lâu đài chung
- Máu lâu đài dùng chung
- Gieo đánh theo wave giữa server và các client
- Chia sẻ điểm và thưởng

**Implementation:**
```
Server Authority:
├── Wave spawning (tất cả player thấy cùng wave)
├── Castle HP management
├── Win/lose conditions
└── Enemy damage validation

Cooperative Features:
├── Shared ammo drops (first come, first served)
├── Shared score pool
├── Revival system (player có thể revive teammate)
└── Communication UI (pings, markers)
```

---

### 4.2. Cooperative Dungeon Mode

**Mục tiêu:**
- 2-4 người chơi khám phá dungeon
- Tiến độ chung
- Quái spawn dựa trên số người chơi
- Hỗ trợ tái xuất dành cho đồng đội

**Implementation:**
```
Dungeon Navigation:
├── Shared mini-map
├── Zone-based spawning
├── Boss scaling theo player count
└── Loot distribution

Synchronization:
├── Dungeon progress (shared)
├── Doors, teleporters (shared state)
├── Events triggered by server
└── Completion check (all players must reach exit)
```

---

### 4.3. Player Roles & Specialization

**Suggested roles (optional enhancement):**
```
Role System (Future Enhancement):

1. Tank (Defender)
   - Higher HP
   - Draws agro
   - Defensive abilities

2. DPS (Damage)
   - Higher damage
   - Better weapons
   - Mobility

3. Support
   - Healing capabilities
   - Buffs teammates
   - Utility items

4. Scout
   - Faster movement
   - Map awareness
   - Enemy detection

Nếu implement roles:
├── Role selection in lobby
├── NetworkPlayerProfile system
└── Perk/ability distribution
```

---

## 💰 CHI PHÍ & LỰA CHỌN HOSTING

### 5.1. Phương án miễn phí (Khuyến nghị cho prototype)

**Unity NGO + Self-hosting:**
```
Chi phí: $0

Setup:
├── Người chơi 1: Host server (máy local)
├── Người chơi 2-4: Connect qua LAN hoặc hamachi
└── Port forwarding cần thiết

Ưu điểm:
├── Hoàn toàn miễn phí
├── Full control
└── Không giới hạn connections

Nhược điểm:
├── Cần kiến thức networking
├── Cần port forwarding
└── Phụ thuộc vào host connection

Hướng dẫn:
1. Setup NetworkManager scene
2. Host starts server
3. Clients get IP từ host
4. Clients connect qua lobby system
```

---

### 5.2. Unity Cloud Relay (Miễn phí tier)

**Unity NGO Cloud Connectivity:**
```
Chi phí: FREE (dưới giới hạn)

Giới hạn miễn phí:
├── 100 CCU/month (concurrent users)
├── More than enough cho 4 players
└── Không cần port forwarding

Setup:
1. Import NGO package
2. Enable Unity Cloud Authentication
3. Get relay endpoint từ Unity Dashboard
4. Configure NetworkManager

Ưu điểm:
├── Không cần port forwarding
├── Work qua NAT
├── Miễn phí cho development
└── Dễ dàng setup

Nhược điểm:
├── Cần internet connection
└── Dependency vào Unity service

Tutorial:
https://docs-multiplayer.unity3d.com/netcode/current/learn/relay/
```

---

### 5.3. Dedicated Server Options

**Nếu cần dedicated server:**

| Option | Chi phí/tháng | CCU | Notes |
|--------|---------------|-----|-------|
| Unity Cloud (Free) | $0 | 100 | Đủ cho prototype |
| DigitalOcean Droplet | $6-$12 | Vô hạn | VPS Linux |
| AWS EC2 t3.micro | $8-15 | Vô hạn | Linux server |
| Linode | $5-10 | Vô hạn | VPS đơn giản |

**Khuyến nghị cho production:**
```
DigitalOcean Droplet:
├── 1GB RAM, 1 vCPU
├── Ubuntu 22.04
├── Setup Unity NGO headless server
└── Chi phí: ~$6/tháng

Deploy:
1. Build Unity server build (Linux)
2. Upload to droplet
3. Run như service
4. Expose port
```

---

## 📊 SO SÁNH GIẢI PHÁP

### 6.1. Unity Netcode for GameObjects vs Mirror

| Criteria | Unity NGO | Mirror |
|-----------|-----------|--------|
| **Chi phí** | Free | Free (open source) |
| **Documentation** | Excellent | Good |
| **Community** | Growing | Large & mature |
| **Performance** | Good | Very good |
| **Learning curve** | Moderate | Moderate-St out at once |
| **WebRTC support** | Yes | Plugins available |
| **Scene management** | Automatic | Manual |
| **Migration effort** | Low (Unity native) | Medium (integration needed) |
| **Production ready** | Yes (v1.5+) | Yes (v70+) |

**Verdict cho dự án này:** Unity NGO - vì native integration và không phải tích hợp thêm external library.

---

### 6.2. Unity NGO vs Photon PUN2

| Criteria | Unity NGO | Photon PUN2 |
|-----------|-----------|-------------|
| **Chi phí** | Free | ~$95/tháng (100 CCU) |
| **Setup complexity** | Low | Low |
| **Server authority** | Built-in | Built-in |
| **Customization** | Full control | Limited |
| **Relay service** | Free tier | Paid |
| **Open source** | No (but free) | No |
| **Best for** | Indie/small teams | Commercial games |
| **Lock-in** | None | Vendor lock-in |

**Verdict cho dự án này:** Unity NGO - vì miễn phí và không phụ thuộc vendor.

---

## 🚀 LỘ TRÌNH TRIỂN KHAI

### Phase 1: Setup & Core (1-2 tuần)

```
Week 1:
├── Day 1-2: Import NGO package, setup basic scene
├── Day 3-4: Implement NetworkGameManager
├── Day 5: Test basic connection between 2 clients

Week 2:
├── Day 1-2: Implement NetworkPlayerController
├── Day 3: Add NetworkTransform support
├── Day 4-5: Test player movement sync
└── Verify: 2 players can move, see each other
```

**Deliverables:**
- ✅ Basic multiplayer connection working
- ✅ 2 players can join same session
- ✅ Players can see each other move

---

### Phase 2: Game Logic Integration (2-3 tuần)

```
Week 3:
├── Day 1-2: Convert EnemyManager to network
├── Day 3-4: Add NetworkEnemy spawning
├── Day 5: Test enemy spawn/despawn sync

Week 4:
├── Day 1-2: NetworkCastle implementation
├── Day 3-4: NetworkHealth sync cho players
├── Day 5: Test damage & health sync

Week 5 (Nếu cần):
├── Day 1-2: Fix synchronization bugs
├── Day 3-4: Optimize network traffic
└── Day 5: Final testing 4 players
```

**Deliverables:**
- ✅ Defense mode multiplayer working
- ✅ Enemies spawn & sync correctly
- ✅ Castle HP shared between players
- ✅ Win/lose conditions working

---

### Phase 3: Dungeon Mode (2 tuần)

```
Week 6:
├── Day 1-2: NetworkDungeonManager setup
├── Day 3-4: Zone-based enemy spawning
└── Day 5: Test dungeon exploration

Week 7:
├── Day 1-2: Teleporter/gate synchronization
├── Day 3-4: Dungeon events network support
├── Day 5: Complete dungeon flow test
```

**Deliverables:**
- ✅ Dungeon mode multiplayer working
- ✅ Shared dungeon progress
- ✅ Cooperative gameplay functional

---

### Phase 4: Polish & Optimization (1-2 tuần)

```
Week 8:
├── Day 1-2: Bug fixes
├── Day 3: Performance optimization
├── Day 4: UI improvements (player list, health bars)
└── Day 5: Final testing

Week 9 (Optional):
├── Balance game difficulty for MP
├── Add voice chat (optional)
└── Add player scoreboard
```

**Deliverables:**
- ✅ Production-ready multiplayer
- ✅ Optimized performance
- ✅ All features working
- ✅ Documentation complete

---

## ⚠️ CHALLENGES & SOLUTIONS

### 7.1. Latency Management

**Challenge:**
- FPS yêu cầu độ trễ thấp
- Client-side prediction

**Solution:**
```
NetworkPlayerController.cs:

public class NetworkPlayerController : NetworkBehaviour
{
    // Client-side prediction
    private Vector3 serverPosition;
    private float serverRotation;
    
    private void Update()
    {
        if (IsOwner)
        {
            // Owner: Predict locally
            HandleInput();
        }
        else
        {
            // Other clients: Interpolate
            InterpolateMovement();
        }
    }
    
    // Server validates
    [ServerRpc]
    private void MoveServerRpc(Vector2 input)
    {
        // Validate input
        ApplyMovement(input);
        
        // Broadcast to clients
        MoveClientRpc(input);
    }
}
```

---

### 7.2. Enemy Spawning Synchronization

**Challenge:**
- Đảm bảo spawn ở cùng vị trí, thời điểm
- Cân bằng dựa trên số người chơi

**Solution:**
```
NetworkEnemyManager.cs:

public class NetworkEnemyManager : NetworkBehaviour
{
    [ServerRpc]
    public void SpawnWaveServerRpc(int waveIndex)
    {
        // Chỉ server spawn
        var enemyCount = CalculateEnemyCount();
        var spawnPositions = CalculateSpawnPositions();
        
        foreach (var pos in spawnPositions)
        {
            SpawnEnemy(pos);
        }
        
        // Broadcast to clients
        EnemySpawnedClientRpc();
    }
    
    private int CalculateEnemyCount()
    {
        // Scale enemies theo player count
        return baseCount * Players.Count;
    }
}
```

---

### 7.3. HP/Damage Authority

**Challenge:**
- Tránh hack HP
- Validate tất cả tính toán

**Solution:**
```
NetworkHealth.cs:

public class NetworkHealth : NetworkBehaviour
{
    private NetworkVariable<int> health = new NetworkVariable<int>();
    
    public bool TakeDamage(int amount)
    {
        if (!IsServer) return false; // Chỉ server validate
        
        health.Value -= amount;
        
        if (health.Value <= 0)
        {
            Die();
        }
        
        return true;
    }
    
    // Client chỉ request damage
    [ServerRpc]
    public void RequestDamageServerRpc(int amount)
    {
        TakeDamage(amount); // Server validates
    }
}
```

---

### 7.4. GOAP AI Integration

**Challenge:**
- AI hoạt động ở server
- Đồng bộ trạng thái AI

**Solution:**
```
NetworkEnemyAI.cs:

public class NetworkEnemyAI : NetworkBehaviour
{
    private NetworkVariable<EnemyState> currentState 
        = new NetworkVariable<EnemyState>();
    
    void Update()
    {
        if (!IsServer) return; // Chỉ server chạy AI
        
        // GOAP logic
        var nextAction = brain.Decide();
        if (nextAction != currentState.Value)
        {
            currentState.Value = nextAction;
            // Automatically syncs to clients
        }
    }
}
```

---

## 📈 METRICS & MONITORING

### 8.1. Performance Targets

```
Target Metrics:

├── Network Latency:
│   ├── LAN: <30ms
│   ├── Internet (same region): <100ms
│   └── Internet (global): <200ms
│
├── Server FPS:
│   └── Stable 30+ FPS with 4 players + 20 enemies
│
├── Client FPS:
│   └── 60+ FPS (shooting, movement)
│
├── Bandwidth:
│   ├── Per client: <50 KB/s
│   └── Total server: <200 KB/s
│
└── Packet Loss:
    └── <2% acceptable
```

---

### 8.2. Network Profiling

**Tools needed:**
```
Unity NGO Tools:
├── Network Statistics (built-in)
├── Unity Profiler (Network tab)
└── NGO debugger

Third-party:
├── NVIDIA Nsight
├── Wireshark
└── Unity Analytics (custom events)

Custom Implementation:
├── NetworkMetrics.cs
│   ├── Track RTT (round-trip time)
│   ├── Track packet loss
│   ├── Track bandwidth usage
│   └── Display in-game overlay
└── Log to file for analysis
```

---

## 🧪 TESTING STRATEGY

### 9.1. Testing Matrix

```
Test Scenarios:

├── Connection:
│   ├── 2 players join successfully
│   ├── 4 players join successfully
│   ├── Player disconnect handling
│   ├── Reconnection after disconnect
│   └── Host migration (nếu có)
│
├── Gameplay:
│   ├── Defense mode with 2 players
│   ├── Defense mode with 4 players
│   ├── Dungeon mode with 2 players
│   ├── Dungeon mode with 4 players
│   ├── Castle takes damage correctly
│   └── Win/lose conditions
│
├── Synchronization:
│   ├── Enemy positions sync
│   ├── Enemy deaths sync
│   ├── Player positions sync
│   ├── Player HP sync
│   └── Projectiles sync
│
└── Edge Cases:
    ├── All players die at once
    ├── Enemies spawn outside view
    ├── High latency (>200ms)
    └── Packet loss (>5%)
```

---

### 9.2. Testing Tools

**Built-in NGO tools:**
```
Development Testing:
├── Unity NGO Test Runner
├── Network simulator (add latency, packet loss)
└── Debug mode (show network stats)

Manual Testing:
├── Multi-boot setup (multiple Unity editors)
├── LAN testing với real clients
└── Internet testing với friends
```

---

## 🎓 LEARNING RESOURCES

### 10.1. Official Documentation

```
Unity NGO:
├── https://docs-multiplayer.unity3d.com/
├── Getting Started: https://docs-multiplayer.unity3d.com/netcode/current/learn/getting-started/
├── Network Variables: https://docs-multiplayer.unity3d.com/netcode/current/learn/network-variables/
├── Client-Server Model: https://docs-multiplayer.unity3d.com/netcode/current/learn/network-objects/
└── RPC Guide: https://docs-multiplayer.unity3d.com/netcode/current/learn/network-rpcs/

Video Tutorials:
├── Unity official NGO tutorial series
├── Code Monkey NGO series
└── YouTube: "Unity multiplayer tutorial NGO"
```

---

### 10.2. Code Examples

**Useful Unity NGO patterns:**
```
1. NetworkObject lifecycle
2. Owner authority pattern
3. ServerRpc/ClientRpc pattern
4. NetworkVariable synchronization
5. Custom serialization
6. Scene management
```

---

## ✅ TÓM TẮT VÀ KHUYẾN NGHỊ

### Giải pháp cuối cùng: Unity Netcode for GameObjects

**Lý do:**
1. ✅ Miễn phí 100%
2. ✅ Native Unity integration
3. ✅ Dễ dàng implementation
4. ✅ Tài liệu đầy đủ
5. ✅ Độ ổn định được chứng minh
6. ✅ Chi phí hosting thấp
7. ✅ Không phụ thuộc vendor

**Estimated Effort:**
- **Timeline:** 6-9 tuần cho full multiplayer
- **Complexity:** Medium
- **Cost:** $0-$6/month (server hosting)
- **Risk:** Low (stable tech)

**Next Steps:**
1. Import NGO package vào project
2. Follow Unity NGO documentation
3. Implement theo phases nêu trên
4. Test với team nhỏ trước
5. Deploy và monitor performance

---

## 📝 KẾT LUẬN

Dự án "The Tunnel" có thể chuyển đổi thành multiplayer với **Unity Netcode for GameObjects**. Giải pháp này:

- **Chi phí thấp:** Có thể chạy free với self-hosting hoặc Unity Cloud
- **Độ ổn định cao:** Native Unity, tested và production-ready
- **Phù hợp team size:** Không cần dev kỹ năng networking chuyên sâu
- **Scalable:** Dễ mở rộng lên dedicated server về sau

Cấu trúc đề xuất ở trên mô tả đầy đủ system architecture, components cần thiết, và lộ trình triển khai chi tiết.

**Ready to implement?** Bắt đầu với Phase 1 và follow từng bước!

