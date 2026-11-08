# Version4 服务器运算系统引入盘点（哪些系统还没进来）

```text
文档 ID：DOC-2026-11-06-version4-server-systems-introduction-gap
逻辑域：reviews
产物类型：audit
状态：active
范围：D:/TRbackup/Version4（Terraria 反编译清理版，唯一完整覆盖基线）
      → D:/TRbackup/NLTX/src（2026-11-06 17:40 工作树，含未提交的 MinimalImplementation 删除）
      只统计服务器侧会执行的运算
证据入口：src/NSSLC.Server/NSSLC.Server.csproj:10-15（引用闭包）
          src/NSSLC.Server/Program.cs:322-398（注册表）
          src/NSSLC.Server/Registration/CombatSystemRegistration.cs:46-99
canonical 路径：docs/reviews/audits/2026-11-06-version4-server-systems-introduction-gap.md
verificationStatus：not-run（静态阅读与检索，未编译、未运行、未做行为差分）
```

> **修订说明（2026-11-06 17:40）**：初版写于当天 12:47。之后 14:21 工作树删除了整个
> `src/MinimalImplementation`（781 个文件），并把仍在用的世界持久化类型抽到
> `src/NSSLC.Infrastructure/WorldData`。本修订按新基线重判全部“旧实现”条目：
> **初版 §4 的“仅旧服务器实现”已经不存在了 —— 现在连旧实现也没有。**
> 新增 §0.3（Version4 服务器 tick 逐项对照）与 §6（WorldGeneration 的真实边界）。

## 0. 摘要

### 0.1 现行服务器只引用 6 个工程（含传递引用）

`src/NSSLC.Server/NSSLC.Server.csproj:10-15` 的直接引用加传递闭包：

```text
NSSLC.Server
├── Terraria.Components (src/NSSLC)                        ← ECS 组件与系统
│     └── Components/World                                世界运行时、液体流动、世界物品、Tile 规则
├── NSSLC.Network                                          ← TCP 传输、帧编解码、NPC/世界网络投影
├── NSSLC.Infrastructure.WorldData                         ← .wld 各 section DTO + 端口 + tile 载荷解码
│     └── Terraria.Components                             （基础设施 → 领域，只取 WorldFacts）
└── NSSLC.Infrastructure.WorldFile.Minimal                 ← WorldFile 326 编解码、校验
    ├── WorldData
    └── NSSLC.WorldGeneration                              ← WorldGen 全量移植（346 文件 / 154k 行）
          └── WorldData
```

- 世界运算的权威位置仍是 `src/NSSLC/Components/World/WorldRuntime.cs`（38 个文件），
  它只依赖 ECS 域类型，不依赖 `WorldData`。
- `WorldData` 在现行服务器里的唯一消费者是 `ServerWorldLoader`：把 WorldFile 的 tile
  载荷解码成 `WorldTile`，再把 16 个字段逐字段转成 `WorldTileState`
  （`src/NSSLC.Server/ServerWorldLoader.cs:68-99`）。
- **`NSSLC.WorldGeneration` 在闭包内但零调用入口**（详见 §6）。
- 不在闭包内的：`src/NSSLC.Infrastructure/Network`（590 个 `.cs`）、
  `src/NSSLC.Infrastructure/WorldStorage`（31 个 `.cs`）、`Network/Pocket/*` 编译器工程。
  前两个还引用一个**不存在的** `NSSLC.Application.csproj`，已经无法构建，也从未被服务器引用。

### 0.2 一句话结论

**骨架进来了，规则和内容没进来。** 服务器目前实际执行 30 条 ECS 注册：世界加载、tile
读写、液体流动、时间秒数、玩家输入/背包、空间碰撞、重力、伤害提交链、2 种 NPC AI、
2 种投射物 AI、网络收发。Version4 服务器 tick 里的其余大部分系统（刷怪、NPC 死亡与掉落、
电路、箱子/合成/商店、事件与天气、困难模式与生态生长、物品使用、拾取、世界保存、
住房运行、团队/PvP、运气、图鉴、Journey、死亡惩罚、传送）**没有任何运行路径**。

### 0.3 Version4 服务器 tick 逐项对照

Version4 的服务器每 tick 走 `Main.DoUpdate`（`Terraria/Main.cs:11193`）
→ `DoUpdateInWorld`（`:11411`）。以下按源码执行顺序列出，右侧是 NLTX 现状。

| # | Version4 服务器步骤 | 源码入口 | NLTX 现状 |
| --- | --- | --- | --- |
| 1 | `UpdateCreativeGameModeOverride`（Journey 权限） | `Main.cs:11362` | ✗ 无 |
| 2 | `UpdateWorldPreparationState` | `Main.cs:1763` | ✗ 无 |
| 3 | `NPC.UpdateProtectedSpawnSlots` | `DoUpdate` 内 | ✗ 无 |
| 4 | `DoUpdate_AutoSave`（自动存档） | `Main.cs:11705` | ✗ 无（服务器全目录 0 处 Save） |
| 5 | `UpdateWindyDayState`（风） | `Main.cs:7465` | ✗ 无 |
| 6 | `Rain.MakeRain` / `updateCloudLayer` | `DoUpdate` 内 | ✗ 无（`Rain` 只有编译占位） |
| 7 | `UpdateWeather`（雨/沙尘暴强度） | `Main.cs:12050` | ✗ 无 |
| 8 | `PortalHelper.UpdatePortalPoints` | `DoUpdate` 内 | ✗ 无 |
| 9 | `player[i].Update(i)` | `Player.cs:14789` | ◐ 只有坐标/背包/重力/移动/属性/Buff/无敌 |
| 10 | `NPC.RevengeManager.Update` | `DoUpdateInWorld` 内 | ✗ 无 |
| 11 | `NPC.SpawnNPC`（刷怪与人口预算） | `DoUpdateInWorld` 内 | ✗ 无（服务器只硬编码 2 只 NPC） |
| 12 | `PressurePlateHelper.Update` | `DoUpdateInWorld` 内 | ✗ 无（`LegacyApi.cs:282` 占位） |
| 13 | `CheckBossIndexes` / `NPC.UpdateFoundActiveNPCs` | `Main.cs:11682` | ✗ 无 |
| 14 | `FixExploitManEaters.Update` | `DoUpdateInWorld` 内 | ✗ 无 |
| 15 | `BestiaryTracker.Sights.ScanWorldForFinds` | `DoUpdateInWorld` 内 | ✗ 无 |
| 16 | `NPCDamageTracker.Update`（伤害归因） | `DoUpdateInWorld` 内 | ✗ 无 |
| 17 | `npc[l].UpdateNPC(l)`（NPC AI） | `NPC.cs` | ◐ 2 种（Guide、Slime） |
| 18 | `projectile[n].Update(n)` | `Projectile.cs` | ◐ 2 种 AI（Straight、Homing） |
| 19 | `item[n].UpdateItem(n)`（掉落物物理/合并/拾取/消失） | `WorldItem.cs` | ✗ 无 tick（只存不推） |
| 20 | `LeashedEntity.UpdateEntities` | `DoUpdateInWorld` 内 | ✗ 无（占位） |
| 21 | `UpdateTime`（日/夜、月相、事件开关） | `Main.cs:12972` | ◐ 只有秒数 + 昼夜布尔 |
| 22 | `WorldGen.UpdateWorld`（tile 更新总入口） | `WorldGen.cs:59400` | ✗ 未接入（移植在 `NSSLC.WorldGeneration`） |
| 23 | `UpdateInvasion` | `Main.cs:12542` | ✗ 无 |
| 24 | `UpdateServer`（掉落物广播、超时、区段检查） | `Main.cs:12716` | ◐ 只有包收发与会话关闭 |
| 25 | `chatMonitor.Update` | `DoUpdateInWorld` 内 | ✗ 无（服务器无聊天广播） |
| 26 | `Chest.UpdateChestFrames` | `DoUpdateInWorld` 内 | ✗ 无 |
| 27 | `_ambientWindSys.Update` | `DoUpdateInWorld` 内 | ✗ 无 |

`WorldGen.UpdateWorld`（第 22 项）自身还是一条完整链路，Version4 里依次调用：
`Wiring.UpdateMech`（电路）、`TileEntity.PerformUpdates`（机关/告示牌/箱子）、
`UpdateLunarApocalypse`（天界柱）、`CountTiles`（tile 统计）、`Liquid.UpdateLiquid`（液体，
含 `LiquidCheck` 的水/岩浆/蜂蜜/微光反应）、`SpawnStormLightning`、
`UpdatePrioritizedTownNPC` + `CheckForHousesNearAPlayer`（城镇 NPC 住房）、
`UpdateWorld_OvergroundTile` / `UpdateWorld_UndergroundTile`（草/树/草药/藤蔓/腐化扩散）、
`SpawnFallingObjects`（沙落/陨石/落星）。**这条链在 ECS 服务器里一个都没有**；
它在 `NSSLC.WorldGeneration/WorldGen.cs:71659` 有完整移植，但见 §6。

### 0.4 完全没有实现的（系统级；全树检索 0 命中，另有标注者除外）

检索排除 `ItemId.g.cs`/`TileID.cs`/`WallID.cs`/`BuffID.cs` 等纯枚举文件。

| 系统 | Version4 证据 | NLTX 现状 |
| --- | --- | --- |
| 刷怪与人口预算 | `NPC.SpawnNPC`、`Main.UpdateNPC` 刷怪预算 | `SpawnNPC`/`NpcPopulation`/`SpawnRate` 全树 0 命中 |
| 电路与机械 | `Terraria/Wiring.cs`（69 KB）、`TileObject` 机关数据 | `Wiring` 只有 `WorldGeneration/Runtime/LegacyApi.cs:482` 的 `dynamic` 空实现；`UpdateMech` 同样为空 |
| Tile 实体（箱子/告示牌/机关/塔） | `Terraria.GameContent.Tile_Entities/`（16 类） | 只有 `.wld` DTO 与 `WorldGeneration/Runtime/LegacyTypes.cs` 的空壳 TE；ECS 运行层 0 |
| 拴绳生物 | `Terraria.GameContent.LeashedEntities/` | `LeashedEntity.Clear` 占位；无 tick |
| 钓鱼与浮标 | `Player.cs` fishing 流程、`FishDropRules/` | `Fishing`、`Bobber` 全树 0 命中 |
| 合成配方 | `Terraria/Recipe.cs`、`RecipeGroup.cs` | `Recipe` 全树 0 命中 |
| 商店/经济/重铸 | `Chest.cs`（shop）、`Player.cs` | 运行层 `Shop`/`Reforge` 0 命中；只有 `ShopOverridePacket` codec（在未接入的 `Network/Pocket` 工程） |
| 困难模式与世界转换 | `WorldGen.cs` hardmode 转换 | `Hardmode` 全树 0 命中 |
| 天气（雨/沙尘暴/风） | `Rain.cs`、`GameContent.Events/Sandstorm.cs` | 只有世界文件字段与 `LegacyApi` 占位；无更新逻辑 |
| 日食 / 霜月 / 南瓜月 | `Main` 事件分支、`GameContent.Events/` | `SolarEclipse`、`FrostMoon` 0 命中；`PumpkinMoon` 只有 ID 与旧投影字段 |
| 入侵事件 | `Main.UpdateInvasion`、`Modules/Events` | `Invasion` 只在未接入的 `Network/Pocket` codec 与世界文件 DTO |
| 派对 / 灯笼夜 / DD2 | `BirthdayParty`、`LanternNight`、`DD2Event` | 只有世界文件字段与 `LegacyApi` 占位 |
| 哨兵与鞭 | `Projectile.cs` sentry/whip 分支 | `Sentry` 0 命中；`Whip` 只有 `AttributeKind` 枚举条目 |
| 宠物 / 召唤物 / 坐骑 / 矿车 | `Mount.cs`、`Minecart.cs`、`Projectile.cs` minion | 只有 `AttributeSourceCategory.Mount` 之类的枚举，没有系统 |
| 生态生长（草/树/草药/藤蔓/沙落/陨石/腐化扩散） | `WorldGen.UpdateWorld` 子链 | 只在 `NSSLC.WorldGeneration` 的移植代码里；运行侧 0 |
| 物品使用（`ItemCheck`/弹药/通道） | `Player.ItemCheck` | `ItemCheck`、`UseItem` 全树 0 命中 |
| 世界物品拾取 / 合并 / 消失 | `WorldItem.UpdateItem` | `WorldItemStore` 无 tick，只有请求驱动 |
| NPC 死亡与掉落 | `NPC.checkDead`、`NPCLoot` | 无；`NpcCombatResultSystem` 只把命中改成 Attack 阶段 |
| 生命/法力/氧气/运气/闪避 | `Player.UpdateLifeRegen/UpdateManaRegen/UpdateLuck` | `Oxygen`、`Dodge` 0 命中；`LifeRegeneration` 只有枚举与 1 条 Buff |
| 击退与状态效果 | `NPC.StrikeNPC`、`Player.AddBuff` | `Knockback` 只是 `DamagePayload` 字段并参与请求排序（`CombatDamageSystem.cs:263`），从不写入速度；Debuff 只有 Buff 域 |
| 死亡惩罚 / 幽灵 | `Player.Ghost`、`Difficulty` 分支 | `PlayerDeathRespawnSystem` 只做重生计时 |
| 传送（塔/传送机/镜/魔刺） | `PlayerTeleport`、`TETeleportationPylon` | `Pylon` 只在未接入的 `Network/Pocket` codec；运行层无系统 |
| 出生点与住房运行 | `WorldGen.CheckForHousesNearAPlayer`、`UpdateSceneMetrics` | 住房评分/校验只在 `WorldGeneration`（`WorldGen.cs:6050`、`WorldEntities.cs:209-324`）里被生成/投影调用，ECS 运行时不注册 |
| 世界保存 / 创建 / 区段流 | `WorldFile.SaveWorld`、`WorldSections` | 服务器 0 处 Save；`WorldFileDocumentEncoder` 存在，但唯一调用方在不可构建、未被服务器引用的 `NSSLC.Infrastructure.WorldStorage`（`WorldStorageCoordinatorFactory.cs:761`） |
| 团队 / PvP / 旁观 | `Player.team`、`Player.hostile`、`SpectateNextClosestPlayer` | `Team`/`Spectat` 只在未接入的 `NSSLC.Infrastructure/Network` 包处理器里；`PvP` 0 命中 |
| 运气 / 图鉴 / Journey | `Player.UpdateLuck`、`Bestiary`、`CreativePowers` | `Bestiary` 只在未接入的 Network 包处理器、WorldData section 与世界生成 tracker；`CreativePower` 只有 `LegacyApi` 占位 |
| 服务器聊天广播 | `Main.NewText` / `ChatHelper.BroadcastChatMessage` | 只有 `LegacyApi.ChatHelper` 占位 |

### 0.5 旧实现已经不在工作树里（相对初版的关键变化）

`git status` 现有 781 条 `D src/MinimalImplementation/**`。这些旧实现**仍可从 git HEAD
取回**（最后一次提交 `dd189f6`），但已不参与构建、也没有任何调用点：

| 旧工程目录 | HEAD 文件数 | 曾承载的系统 |
| --- | --- | --- |
| `Combat/` | 178 | NPC 管理/刷怪/战斗/掉落、投射物完整实现 |
| `Inventory/` | 103 | 背包、箱子、合成、快捷堆叠、拾取协调 |
| `Modules/` | 79 | 事件、图鉴、Journey、城镇 |
| `Host/` | 71 | 世界区段、会话宿主 |
| `Contracts/` | 70 | 跨工程契约 |
| `Player/` | 69 | Buff、运气、团队、旁观、闪避、传送塔 |
| `World/` | 47 | 世界 owner、tick 协调、门、涂色、framing |
| `Mechanisms/` | 46 | 电路、机关定时器、逻辑传感器、告示牌、传送塔 |
| `Persistence/` | 32 | 世界存档、创建、恢复 |
| `Verification/` | 30 | 旧验收工程 |
| `Protocol/` | 23 | 入站路由 |
| `Session/` | 21 | 加入/准入协调 |
| 其他（`Identity/`、`Tools/`、根目录） | 12 | 实体身份、线路编译器 |

**这意味着**：初版 §4 里“代码在、运行路径不在”的说法已经失效。现在要引入这些系统，
必须**直接从 Version4 重新移植**（或从 git 历史取回旧实现再重写），不能指望“把旧工程加回引用”。
另注：`.agent-workplace/` 下有几个探针工程仍引用 `src/MinimalImplementation/*.csproj`，
路径已不存在，这些探针不可构建（见 §8）。

## 1. 判定口径

### 1.1 四档状态

| 档 | 含义 | 判定证据 |
| --- | --- | --- |
| ✅ 已接入 | 类型存在，且在 `NSSLC.Server` 的注册表/调用链中执行 | `Program.cs` 的 `EcsServerSystemRegistration` 注册或直接调用 |
| ◐ 部分 | 系统或组件存在并接线，但内容/规则/入口只覆盖一小片 | 同上 + 具体缺口列出 |
| ○ 已移植未接入 | 代码在闭包内，但没有任何调用路径 | 引用闭包 + 全树检索调用点 |
| ✗ 无实现 | 全树检索 0 命中（排除纯枚举文件） | 检索命令见 §9 |

### 1.2 现行服务器实际注册的 30 条系统（`Program.cs:346-397`）

| 阶段 | 系统 | 条数 |
| --- | --- | --- |
| Preparation | `CommunicationSessionTickSystem`、`ProjectileSpawnRequestSystem`、`ProjectileMovementIntentSystem` | 3 |
| Simulation | `PlayerInputRequestSystem`、`InventoryAttributeContributionSystem`、`PhysicsContactCaptureSystem`、`ProjectileLifetimeTickSystem`、`ProjectileDamageRequestSystem`、`WorldTickSystem`、`NpcAiIntentAdapter`、`GravityTickSystem`、`AttributeTickSystem`、`BuffTickSystem`、`MovementIntentRequestSystem`、`SpatialSimulationCoordinatorSystem` | 12 |
| Commit | `ProtectionWindowSystem`、`CombatAttackRequestSystem`、`CombatDamageRequestSystem`、`ProjectileDamageResultSystem`、`NpcCombatResultSystem`、`PlayerCombatResultSystem`、`PlayerDeathRespawnSystem`、`LifeRestoreRequestSystem`、`ProjectileTerminationCommitSystem`、`WorldInteractionSystem` | 10 |
| Projection | `NpcNetworkProjectionSystem`、`WorldItemRequestSystem`（跑两遍）、`WorldNetworkProjectionSystem`、`CommunicationOutboundProjectionSystem` | 5 |

其中 `CombatAttackRequestSystem`/`CombatDamageRequestSystem`/`NpcCombatResultSystem`/
`PlayerCombatResultSystem` 通过 `CombatDamageSystem` 串成
`DamageAdmission → DamageMitigation → DamageCommit` 三层，`DamageCommitSystem` 是唯一的
Life 写入者（`CombatDamageSystem.cs:16-44`）。

### 1.3 明确排除（不算“未引入”）

客户端表现与工具：`Terraria.Graphics*`、`Terraria.UI*`、`Terraria.Audio`、`Terraria.Map`、
`Terraria.Cinematics`、`Dust/Gore/Lighting`；社交平台 `Terraria.Social.*`；成就（客户端追踪）；
外部依赖（BCrypt、RailSDK、Steamworks、Ionic.Zip、nativefiledialog）；纯工具工程
（ILSpy、PacketDesignCompiler）。

## 2. 已接入现行服务器的系统

| 系统 | Version4 入口 | NLTX 实现（证据） | 状态 |
| --- | --- | --- | --- |
| 世界文件读取 | `Terraria.IO/WorldFile.cs` LoadWorld | `ServerWorldLoader.Load` → `WorldFileDocumentDecoder`/`Validator` + `WorldFileTilePayloadDecoder` | ✅ 只消费 Header/Environment/Tiles 三个 section |
| 世界 Tile 网格 | `Terraria/Tile.cs`、`Main.tile` 数组 | `WorldRuntime` + `WorldTileStorageComponent`（38 文件） | ✅ |
| 入服握手与世界流式传输 | `MessageBuffer` 1/3/4/6/8/12/159、`Netplay` 区段 | `TerrariaProtocolAdmissionHandler`：200×150 区段网格，按玩家位置补发 3×3 区段（本仓库 8400×2400 世界 = 42×16 = 672 个区段） | ✅ 协议层完整；无区段级卸载 |
| Tile 破坏/放置 | `WorldGen.KillTile/PlaceTile` | `WorldInteractionSystem` + `WorldMutationRules` + `WorldTileDropRules` | ◐ 规则为最小实现（见 §2.2） |
| 液体流动 | `Liquid.Update`、`Liquid.UpdateLiquid` | `Share/Physics/Liquid/LiquidSimulation` + `WorldRuntime.AdvanceLiquidSimulation` + `WorldTickSystem` | ✅ 流动与横向均衡 |
| 时间推进 | `Main.UpdateTime` | `WorldTickSystem` + `WorldTimeAdvanceSolver` | ◐ 只有总秒数/当日秒数/昼夜布尔 |
| 世界物品存储 | `WorldItem` 数组 | `WorldItemStore`（400 槽）+ `WorldItemRequestSystem` + `WorldItemRules` | ◐ 无 tick、无拾取/合并/消失 |
| 玩家生命周期 | `Player.Spawn/KillMe` | `PlayerDeathRespawnSystem` + 死亡/重生组件 | ✅ 600 tick 重生 |
| 玩家坐标/背包输入 | 包 13/5 | `PlayerInputRequestSystem` | ◐ 只有 `SetPosition`/`SetInventorySlot` |
| 空间与碰撞 | `Terraria/Collision.cs` | `SpatialSimulationCoordinatorSystem`、`EntityCollisionSystem`、`PhysicsContactCaptureSystem` | ✅ |
| 重力与移动 | `Player.Move`/`NPC.Move` | `GravityTickSystem`、`MovementIntentRequestSystem`、`SpatialSimulationSystem` | ◐ 无跳跃档位/游泳/钩爪/冲刺 |
| 伤害提交链 | `NPC.StrikeNPC`、`Player.Hurt` | `DamageAdmission → Mitigation → Commit` | ✅ 单一 Life 写入者 |
| 受击无敌窗口 | `Player.UpdateImmunity` | `ProtectionWindowSystem`（默认 60 tick） | ✅ |
| 属性 | `Player.UpdateEquips` 数值部分 | `AttributeSystem`/`AttributeTickSystem`/`InventoryAttributeContributionSystem` | ◐ 装备数据为空 |
| Buff | `Player.UpdateBuffs` | `BuffTickSystem` + `BuffSystem` + `BuffAdmissionSystem` | ◐ 内容只有 3 条药水 |
| 背包 | `Player.inventory` | `InventorySystem` + 59 个槽位定义 | ◐ 全部 `General`，无装备槽 |
| NPC 行为 | `NPC.AI` | `NpcAiIntentAdapter` + `GuideAiBehavior`/`SlimeAiBehavior` | ◐ 只有 2 种；NPC 也是启动时硬编码 2 只 |
| NPC 受击反馈 | `NPC.StrikeNPC` | `NpcCombatResultSystem` 把 `Applied` 结果转成 `Attack` 阶段 | ◐ 不处理死亡/击退/免疫 |
| 投射物 | `Projectile.Update` | `ProjectileLifecycleSystem`/`SpawnRequest`/`MovementIntent`/`LifetimeTick`/`HitResolution`/`TerminationCommit` | ◐ 只有 `Straight`/`Homing` 两种 AI |
| 网络会话 | `MessageBuffer.GetData`、`Netplay` | `CommunicationHostRuntime` + `PacketFrameCodec` + `CommunicationSessionTickSystem` | ◐ 入站 13 个 id、出站 11 个 id |

### 2.1 `src/NSSLC/Components/World` 逐文件盘点（38 文件）

这一层是现行的世界运算层，全部在 `Terraria.Components` 内，不依赖 `WorldData`。

| 分组 | 文件（行数） | 实际职责 |
| --- | --- | --- |
| 运行时 | `WorldRuntime`（393） | 行锁、epoch/revision、tile 读写、破坏/放置、液体仿真推进、mutation 队列、`Move` 空间查询、`GetLiquidContact` |
| 状态 | `WorldRuntimeStateComponent`（121）、`WorldTileStorageComponent`（108） | 宽高/tileSize/epoch/revision/LastTick/`TotalTimeSeconds`/`TimeOfDaySeconds`/`DayTime`；稠密 tile 数组与索引 |
| 调度 | `WorldTickSystem`（34） | 每 tick `AdvanceTick` + `AdvanceLiquidSimulation` |
| 交互 | `WorldInteractionSystem`（150）、`WorldInteractionRequestComponent`（83）、`WorldInteractionResultComponent`（42）、`WorldInteractionCapabilityComponent`（8） | `ReadTile`/`BreakTile`/`PlaceTile`，按权限授权，请求必须属于当前 tick |
| 世界物品 | `WorldItemStore`（181）、`WorldItemRequestSystem`（66）、`WorldItemRequestQueue*`（40） | 400 固定槽；`Create`/`Update`/`Release`/`Retire`/`Reserve`；会话预留与版本 |
| 纯规则 | `WorldTileTransitionRules`、`WorldTileDropRules`、`WorldTileMotionSolver`、`WorldTimeAdvanceSolver`、`WorldMutationRules`、`WorldItemRules`、`WorldTileIndexRules`、`WorldInteractionPermissionRules` | 破坏/放置合法性、单 tile 掉落表、逐轴扫掠+接地吸附、时间/revision/reload、mutation 合并、物品规则、索引、权限 |
| 模型 | `WorldFacts`（2026-11-06 从 `WorldData` 迁入） | tile 类型事实表：solid 324 条、solidTop 84 条、frameImportant、saveSlopes |
| 端口 | `IWorldTileGrid`、`IWorldInteractionWorld`、`IWorldItemQuery`、`IWorldItemProjectionSource`、`IWorldProjectionSource` | 只读查询与投影边界 |

#### 2.2 World 层相对 Version4 的缺口（按影响排序）

1. **没有 tile framing**：`WorldGen.TileFrame`/`SquareTileFrame` 在 ECS 世界层没有对应运算，
   破坏/放置会把 `FrameX`/`FrameY` 置 0，多格物体的拼接不会重算。移植版在
   `NSSLC.WorldGeneration/Runtime/Framing.cs`（12 KB，`SelfFrame8Way`/`WallFrame` 等齐全），
   但它操作的是移植层自己的 `Main.tile`，不是 ECS 的 `WorldTileStorageComponent`。
2. **没有墙体操作**：`TryPlaceTile`/`TryBreakTile` 只处理 tile；`WorldTileState.Wall` 只能从文件读入。
3. **斜坡/半砖/促动器/涂色不进规则**：字段存在于 `WorldTileState`，读写规则里没有分支。
4. **破坏/放置的判据只有 `Active`**：`CanBreak`/`CanPlace` 不检查工具、锤/镐等级、支撑面，
   也没有树/门/床/雕像这类多格物体的整体破坏规则；`Placed` 不处理该格已有液体。
5. **世界物品没有 tick**：`WorldItemStore` 只存 `Position`/`Velocity`，没有逐 tick 运动系统；
   掉落物不会下落、不会合并、不会超时消失，位置只由包 21 或破坏产生的 `Create` 请求写入。
6. **没有 Tile 更新**：沙落、草/腐化蔓延、植物生长、门、平台、藤蔓均无。
7. **液体无反应**：只做流动与横向均衡，没有水/岩浆/蜂蜜/微光互转（`Obsidian` 只在 ItemId 与注释里）。
8. **没有区段**：整图一次性常驻内存，无 `WorldSections` 式按区段加载/卸载。
9. **没有 TileEntity/箱子/告示牌/机关**：世界文件能解码这些 section，世界层没有对应 owner。
10. **没有保存路径**：`WorldRuntime` 只有加载与内存变更，`DrainPendingMutations` 交给投影后即丢弃。
11. **时间只有三个量**：`TotalTimeSeconds`/`TimeOfDaySeconds`/`DayTime`，没有月相、事件覆盖、天气。

## 3. 部分引入，缺口明确

| # | 缺口 | 证据 | 影响 |
| --- | --- | --- | --- |
| 1 | 玩家 59 个槽位全是 `InventorySlotKind.General` | `Registration/PlayerEntityDefinition.cs:84-91` | 装备/饰品/武器槽不存在，属性贡献匹配不到任何装备 |
| 2 | 物品定义全是宽松兜底（`MaxStack=short.Max`、`ItemCategoryMask.Any`） | `Program.cs:306-320` | 逐物品 `SetDefaults` 数据（堆叠、分类、伤害、前缀）完全没有 |
| 3 | 生命再生只有枚举与 Buff 贡献，没有消费者 | `AttributeKind.LifeRegeneration` 全仓 2 处引用 | 再生药水不产生回血 |
| 4 | 无击退、无状态效果、无闪避、无法力、无氧气、无运气 | `Knockback` 只是 `DamagePayload` 字段；`Mana` 只在未接入的 Network 包处理器；`Oxygen`/`Dodge` 0 命中 | 战斗只到“扣血” |
| 5 | 只有一条攻击定义（`player.default`，10 点近战） | `Program.cs:482-490` | 没有武器、NPC 攻击、暴击来源数据 |
| 6 | 投射物只有 `Straight`/`Homing` 两种行为 | `ProjectileAiRegistration.cs:25-28` | Version4 数百种 AI（type 1–1000+）未覆盖 |
| 7 | `ProjectileSimulationSystem` 零调用者 | 全仓只有定义处与注释提到 | `extraUpdates` 子步语义未生效 |
| 8 | 战斗无 NPC 攻击入口、无掉落、无死亡处置 | `CombatSystemRegistration` 只注册玩家侧；`IsAlive`/`IsDead` 只在 Player 域消费 | 击杀后 NPC 不消失、不掉落；NPC 也不会主动攻击玩家 |
| 9 | NPC 只有 2 只，且是启动时硬编码 | `Program.cs:95-110`（Guide type 22、Slime type 1） | 没有刷怪、没有人口、没有城镇 NPC 入住 |
| 10 | 服务器不保存世界 | `src/NSSLC.Server` 全目录 0 处 `Save`/`Encoder` 调用；`WorldFileDocumentEncoder` 的调用方在不可构建的 `NSSLC.Infrastructure.WorldStorage` | 会话结束即丢失全部运行时状态 |
| 11 | 世界文件 28 个 section 都能解码，服务器只消费 3 个 | `WorldData/Persistence/WorldData/*Section.cs`（28 个）vs `ServerWorldLoader:58-69` | 箱子/告示牌/NPC/TileEntity/压力板/城镇/进度读进来就丢弃 |
| 12 | 入站包只有 13 个 id | `GameCommunicationAdapter.cs:29-35`（7 个）+ 握手 1/4/6/8/12/159（6 个） | Version4 `MessageBuffer.GetData` 有 160 个 id 分支（见 §9） |
| 13 | 出站包只有 11 个 id | `NpcNetworkProjectionSystem:30`、`WorldNetworkProjectionSystem:20-23`、握手响应 | 没有 NPC 战斗/城镇/事件/图鉴/小地图同步 |
| 14 | 没有服务器聊天 | `ChatHelper.BroadcastChatMessage` 只有占位 | 事件/死亡/加入消息不播报 |

## 4. 现在连“旧实现”也没有了

初版 §4 记录的是“代码在旧服务器里、运行路径不在 ECS 服务器”。**2026-11-06 14:21 之后，
那批旧实现已从工作树整体删除**（781 个文件，`git status` 可见，HEAD 仍可回溯）。
因此下表全部从“○ 仅旧实现”降级为“✗ 无实现（需重新移植）”：

| 系统 | 旧实现位置（HEAD `dd189f6`） | 与 Version4 的对应 |
| --- | --- | --- |
| 电路与机械 | `MinimalImplementation/Mechanisms`（46） | `Terraria/Wiring.cs`、`TileObject` 机关数据 |
| Tile 实体 / 告示牌 / 传送塔 | 同上 | `GameContent.Tile_Entities/`（16 类） |
| 拴绳生物 | 同上 + `Modules` | `GameContent.LeashedEntities/` |
| NPC 刷怪与人口 | `Combat/Npcs` | `NPC.SpawnNPC`、`Main.UpdateNPC` 刷怪预算 |
| NPC 战斗/掉落/Buff | 同上 | `NPC.StrikeNPC/checkDead/NPCLoot` |
| 投射物完整实现 | `Combat/Projectiles` | `Projectile.Update/AI/Kill` |
| 背包与容器 | `Inventory/`（103） | `Player.inventory`、`Chest`、`Recipe` |
| 玩家扩展 | `Player/`（69） | `Player.Update*` 系列 |
| 事件 | `Modules/Events` | `Main` 事件分支、`GameContent.Events` |
| 图鉴 / Journey | `Modules/Bestiary`、`Modules/Creative` | `GameContent.Bestiary`、`GameContent.Creative` |
| 世界存档/创建/区段 | `Persistence/`（32）、`Host/`（71） | `WorldFile.SaveWorld`、`WorldSections` |
| 会话与协议 | `Session/`（21）、`Protocol/`（23） | `MessageBuffer`、`NetMessage`、`RemoteClient` |

## 5. 按 Version4 权威 20 分区的覆盖

| 分区 | 主题 | 现状 | 主要缺口 |
| --- | --- | --- | --- |
| P01 | Liquid-Wiring-Spatial-Death-Teleport | ◐ | 液体 ✅、空间/死亡重生 ✅；电路、死亡惩罚/复仇、传送全无 |
| P02 | Leashed-Entity | ✗ | 拴绳/风筝/锚点无实现（旧工程已删） |
| P03 | Mount-Vehicle | ✗ | 坐骑、矿车无任何系统 |
| P04 | Player-Lifecycle-Interaction | ◐ | 生成/死亡/重生 ✅；拾取、丢弃、团队、交互范围不足 |
| P05 | Player-Progression-Pets-Minions | ✗ | 进阶解锁、宠物、召唤物、鞭均无 |
| P06 | Player-Combat-Status | ◐ | 生命与无敌窗口 ✅；法力、氧气、运气、击退、状态效果无 |
| P07 | Player-Mobility | ◐ | 基础移动+重力 ✅；跳跃档、冲刺、钩爪、游泳、传送无 |
| P08 | Player-Environment-Armor | ✗ | 环境检测、护甲套装、区域判定无 |
| P09 | Player-Inventory-Equipment | ◐ | 槽位与转移 ✅；装备槽、前缀、弹药、耐久、使用无 |
| P10 | Player-Input-Control | ◐ | 只有坐标与背包槽两种请求 |
| P11 | Player-Presentation | — | 客户端表现，排除 |
| P12 | NPC-Combat-Network-Damage | ◐ | 伤害提交链 ✅；NPC 攻击、击退、免疫、死亡掉落无 |
| P13 | NPC-Town-Progression | ✗ | 城镇 NPC AI、商店、税收、幸福度、入住运行无 |
| P14 | NPC-Spawn-Eligibility | ✗ | 刷怪预算/资格无（旧工程已删，Version4 也未移植） |
| P15 | Projectile | ◐ | 生命周期/命中/穿透 ✅；特化 AI、Tile 效果、子步无 |
| P16 | World-Lifecycle-Housing-Metrics | ◐ | 加载与时间 ✅；保存、住房运行、度量未接入 |
| P17 | World-Terrain-Biomes-GenVars | ✗ | 运行时地形转换（困难模式、腐化/神圣扩散、生长）无 |
| P18 | World-Seeds-Skyblock | ✗ | 秘密种子/空岛规则只有生成侧 |
| P19 | World-Generation-Execution | ○ | 全量移植在 `NSSLC.WorldGeneration`，ECS 服务器无调用入口 |
| P20 | World-Generation-Actions-Shapes | ○ | 同上 |

## 6. 已移植但未接入：`NSSLC.WorldGeneration` 的真实边界

`NSSLC.WorldGeneration` 有 346 个 `.cs`、154,532 行，其中 `WorldGen.cs` 86,886 行。
它通过 `WorldFile.Minimal → WorldGeneration` 进入服务器程序集，**但没有任何调用点**：

- `WorldFile.Minimal` 除 5 个 WorldFile 编解码文件外，还编译了 `GeneratedWorldDocumentProjection.cs`；
  该类型全树零调用方（旧宿主里的生成入口已随 `MinimalImplementation` 删除），
  `NSSLC.Server` 只读现有 `.wld`。
- `WorldGeneration` 有自己的静态世界模型：`Runtime/MainHost.cs:83` 新建
  `tile = new Tile[maxTilesX, maxTilesY]`，与 ECS 的 `WorldTileStorageComponent` 是两套数据。
- 生成侧是真实移植：`WorldGen.cs` 包含全部 GenPass、生物群系、地牢、洞穴、沙漠、住房扫描；
  `Runtime/Liquid.cs`（38 KB）包含 `UpdateLiquid`/`LiquidCheck`/`LavaCheck`/`HoneyCheck` 等
  真实反应逻辑；`Runtime/Framing.cs`（12 KB）包含真实的 8-way 拼接。
- **运行侧是空壳**：`Runtime/LegacyApi.cs`（510 行，50 个类型）用 `dynamic` 空实现补齐
  WorldGen 的调用点，包括 `Wiring.UpdateMech`、`PressurePlateHelper`、`Rain`、`Sandstorm`、
  `LeashedEntity`、`NPCDamageTracker`、`DD2Event`、`BirthdayParty`、`LanternNight`、
  `CreativePowerManager`、`NetMessage`、`Netplay`、`ChatHelper` 等全部返回 `default`；
  `Runtime/LegacyTypes.cs`（84 行）再补一批空壳 TileEntity（`TEItemFrame`、`TEWeaponsRack`、
  `TELeashedEntityAnchorWithItem`、`TETrainingDummy` 等）与 `CreativePowers` 假实现。

**结论**：`WorldGeneration` 可以作为“生成/规则参考实现”复用，但不能当作“运行时系统已移植”。
把 `WorldGen.UpdateWorld` 接进 ECS 之前，必须先解决两件事：世界模型统一（`Main.tile`
↔ `WorldTileStorageComponent`），以及 `LegacyApi` 里 50 个空壳的真实实现。

## 7. 建议的引入顺序（依据依赖，不是排期承诺）

1. **内容数据层**：逐物品 `ItemDefinition`（堆叠、分类、属性修饰）、装备槽位定义、
   Buff 全量表。没有它，属性、Buff、背包、战斗数值都只能在样例上跑。
2. **生命恢复与状态效果**：`LifeRegeneration` 消费者、击退、免疫、Debuff 周期伤害。
   这是 P06 与 P12 的共同前置。
3. **NPC 掉落与死亡处置**：接上 `NpcCombatResultSystem` 的结果意图 → 世界物品/掉落表。
4. **世界物品 tick**：下落/合并/拾取/消失，是掉落与容器的共同前置。
5. **刷怪与人口**：刷怪预算的领域化重写 + 世界时段时间窗，接 P14。
6. **物品使用与武器**：`ItemCheck`、弹药消耗、通道、投射物生成入口，接 P09/P10。
7. **电路与机械**：先 `Wiring.UpdateMech` 的 ECS 化（信号 → Tile 变更提交），
   再 Tile 实体与告示牌/箱子。
8. **世界保存**：`WorldFileDocumentEncoder` 已存在，先做“加载→修改→保存”闭环。
9. **世界模型统一**：决定 `WorldGeneration` 的 `Main.tile` 与 ECS 世界网格的关系，
   否则 §6 的移植代码永远接不上。
10. **事件与城镇**：依赖 4、5、8，放到内容层稳定之后。

## 8. 证据缺口

- 本盘点只做静态检索与阅读，未编译、未运行、未做行为差分，因此**不能**据此声称任何
  系统“行为等价”或“已验证”。
- `NSSLC.WorldGeneration` 是否与 Version4 逐行等价未验证，只确认是完整移植
  （`WorldGen.cs` 86,886 行 vs Version4 `WorldGen.cs` 1.90 MB）。
- Version4 的“服务器运算”边界按 `dedServ`/`netMode != 1` 与 dedicated server 路径人工判断；
  少数逻辑（粒子、音效触发的游戏效果、客户端预测）存在灰色地带，已在 §1.3 按表现层排除。
- `.agent-workplace/GenerationReadiness/`、`.agent-workplace/scratch/NpcAiDamageProbe/`、
  `.agent-workplace/scratch/NpcLiveAcceptance/`、`.agent-workplace/WorldTickDiagnostic/`
  仍引用 `src/MinimalImplementation/*.csproj`，这些探针在当前工作树不可构建；本次未修改它们。
- `src/NSSLC.Infrastructure/Network`（590 个 `.cs`）与 `src/NSSLC.Infrastructure/WorldStorage`
  （31 个 `.cs`）引用不存在的 `NSSLC.Application.csproj`，已不可构建；其中的包处理器
  （`WorldDataPacketHandler`、`PlayerExtendedPacketHandlers` 等）**不属于**现行服务器，
  本文按“未接入”处理，未逐文件核对。

## 9. 检索命令（可复现）

```powershell
# 现行服务器注册表
Select-String -Path src\NSSLC.Server\Program.cs -Pattern 'EcsServerSystemRegistration\(|registrations.AddRange'

# 工程引用闭包
Get-Content src\NSSLC.Server\NSSLC.Server.csproj
Get-Content src\NSSLC.Infrastructure\WorldStorage\NSSLC.Infrastructure.WorldFile.Minimal.csproj

# 旧实现删除量（781 条 D）
(git status --porcelain=v1 | Where-Object { $_ -match 'src/MinimalImplementation' }).Count

# 全树功能命中（排除枚举文件）
$files = Get-ChildItem src -Recurse -Filter *.cs |
  Where-Object { $_.FullName -notmatch '\\Build\\' -and $_.Name -notmatch '^(ItemId|TileID|WallID|BuffID)\.' }
foreach ($p in 'Fishing','Recipe','Sentry','Hardmode','Wiring','SpawnNPC','ItemCheck','Oxygen','Dodge','PvP') {
  "{0,-12} {1}" -f $p, ($files | Select-String -Pattern "\b$p" -CaseSensitive -List | Measure-Object).Count
}

# Version4 包分支数（160 个消息 id）
(Get-Content D:\TRbackup\Version4\Terraria\MessageBuffer.cs |
  Where-Object { $_ -match '^\t\tcase \d+:' }).Count
```
