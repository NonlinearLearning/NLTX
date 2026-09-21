# Version4 P08 玩家环境、护甲与移动相关状态组件拆分设计

partitionId: P08
sessionId: 243009d31f4e42d0bfc70eebd9d1fcaf
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P08-Player-Environment-Armor.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P08-player-environment-armor-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P08-player-environment-armor-component-execution.md
designStatus: proposed
executionStatus: in-progress
implementationStatus: in-progress
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: not-verified
completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T08:51:27Z
evidence-gap: C01-C09 组件源码已保存；C07 的 turret capacity 与现有 PlayerAbility owner、vortex stealth projection、P05/P06/P09/P11 交接仍未闭合。C01 的 zone protocol、C02 tile catalog/range writer、C03 static tuning/Physics owner、C04 mobility writer、C05 detection/spawn/projection owner、C06 combat/spawn writer/Item payload seam、C08 world gravity/input/equipment priority 和 Collision migration、C09 luck rules/network/NPC/Item/Movement/audio owners 也仍未闭合。完整网络/持久化格式、Version4 空 Serialize/Deserialize、生产调度和实际 verifier 运行结果仍缺失。
blocking-decision: integration-review; C01-C09 partial until protocol and owner evidence are closed

## 1. 范围与状态

本设计只覆盖 P08 权威报告中的 9 个叶子子系统、100 个字段和 0 个属性，正式父域为 `PlayerGameplay`。它是 ECS 组件边界与迁移输入，不是行为等价证明、公共 API 兼容证明、网络闭合证明或持久化闭合证明。本实现会话只修改本分区的两份 Markdown 和允许范围内的 P08 组件源码，不修改测试、项目文件、权威报告、ledger 或其他会话文档。

叶子组按权威报告顺序作为检查点，但一个叶子组不等于一个运行时巨型组件。组件、System、Query、Command、Adapter 和 Projection 必须按共同读写者、生命周期和副作用边界落位。所有新名称在本文档中均为 `status: proposed`，现有 NLTX 类型只是部分覆盖证据。

| 检查点 | 权威叶子子系统 | 成员 | 建议边界 | 当前状态 |
|---|---|---:|---|---|
| C01 | PlayerZoneAndEnvironmentState | 7 | 区域 bit、环境免疫计时器、shimmer transition latch | design-checkpoint-complete |
| C02 | PlayerTileTargetingAndRangeState | 12 | 玩家侧 tile range/target/邻接状态；静态全局工作区单独审查 | design-checkpoint-complete |
| C03 | PlayerMovementPhysicsState | 8 | 玩家重力、跳跃和奔跑参数；静态默认值与实例值分开 | design-checkpoint-complete |
| C04 | PlayerEnvironmentMobilityState | 12 | 水/岩浆/漂浮/移动能力与交互距离派生状态 | design-checkpoint-complete |
| C05 | PlayerEnvironmentDetectionAndSpawnState | 13 | 环境探测、生成资格、进度输入和 luck projection 输入 | design-checkpoint-complete |
| C06 | PlayerArmorAndCombatEffects | 8 | 反伤、护甲战斗效果和 Item effect payload seam | design-checkpoint-complete |
| C07 | PlayerArmorSetAndTurretState | 19 | 套装、炮塔容量、Vortex 状态和兼容快照 | design-checkpoint-complete |
| C08 | PlayerGravityAndWaterTraversalState | 5 | 水面行走及重力方向/控制资格 | design-checkpoint-complete |
| C09 | PlayerLuckAndRescanState | 16 | luck authority、墙体扫描缓存、移动缓存、void-bag 和音频 seam | design-checkpoint-complete |

## 2. 证据登记

| 来源 | 已核对事实 | 支持的结论 | 状态 |
|---|---|---|---|
| P08 权威报告 | P08 报告第 4.13.78、4.13.79、4.13.80、4.13.81、4.13.82、4.13.89 节及其余 P08 叶子节；来源序号 662..1401 | 9 个叶子组、100 个字段、声明类型、原始类型、Version4 行号和组归属 | confirmed |
| Version4 Player 声明 | `D:\TRbackup\Version4\Terraria\Player.cs:917-933,1905-1945,2124-2260,2423-2457` | P08 字段、默认值、静态/实例可变性和私有字段边界 | confirmed |
| Version4 区域与 shimmer 生命周期 | `Player.cs:2621-2939,9926-9945,26566` | zone property facade、zone 重算、shimmer transition 和初始化清理路径 | confirmed |
| Version4 装备/效果生命周期 | `Player.cs:6865,9547,10272+,14789+,15636-15644` | UpdateEquips、UpdateArmorSets、ResetEffects、移动参数重建、范围缓存和清理顺序 | confirmed |
| Version4 tile/interaction 行为 | `Player.cs:15109-15154,17741-17758,19927-19998` | tile target、wall rescan、拾取范围/速度和相关副作用边界 | confirmed |
| Version4 luck 行为 | `Player.cs:10126-10253,10782-10799,17869-17929` | luck 计算、设备 bonus、ladybug/torch 输入、rescan 和同步触发 | confirmed |
| Version4 网络写入 | `NetMessage.cs:932-936` | zone1..zone5 的网络输出存在；不能外推其他 P08 字段均可复制 | confirmed/partial |
| Version4 网络读取 | `MessageBuffer.cs:1722-1731` | zone1..zone5 输入以及 shimmer-entry 行为边界 | confirmed/partial |
| Version4 跨域调用 | `Projectile.cs:45219-45238`; `Mount.cs:2867,4426`; `NPC.cs:67858-67861` | tile range、slowFall、canFloatInWater、ladyBugLuckTimeLeft 的跨域读写 | confirmed |
| Version4 持久化入口 | `Player.cs:26418,26455` 及保存入口 | 当前 Version4 `Serialize`/`Deserialize` bodies 为空；不能伪造 P08 保存格式 | confirmed-gap |
| 完整参考补证 | `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs` 的对应保存实现 | 只能作为 `full-reference-supplemented` 的待验证 adapter 参考，不能证明当前 Version4 行为 | partial |
| tModLoader stable mirror | `D:\TRbackup\tmodloader-api-docs-stable\class_player.html`，页面版本 `tModLoader v2026.07` | tile range/target、luck、ResetEffects、UpdateEquips、UpdateArmorSets、UpdateLuck 的公开 API 语义交叉参考 | confirmed-public-boundary |
| 当前 NLTX | `src\Player\PlayerAbilityComponent.cs`, `PlayerEquipmentComponent.cs`, `PlayerMountComponent.cs`, `src\Physics\PhysicsStateComponent.cs`, `src\SpatialSimulation\MovementStateComponent.cs` | 已存在能力、装备、坐骑、物理和移动局部 owner 候选；不能重复建立 turret/physics authority | existing-evidence |
| SS14 ECS 只读参考 | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Gravity\GravityComponent.cs`、Atmos/Wires/Movement System | 仅支持窄组件、System/Query、外部 effect 边界的组织粒度，不支持 Terraria 语义 | organization-only |
| 流程缺口 | `D:\TRbackup\NLTX\约束\公共拆分约束.md` 不存在；`Context/progress.md` 显示 migration context cleared | 缺失约束不能被臆造；本设计保留 partial evidence | missing/partial |

证据等级约定：`confirmed` 只表示该事实在指定源中直接看到；`partial` 表示调用点或完整生命周期尚未闭合；`integration-review` 表示 P08 不单方面拥有最终 authority；`proposed` 表示尚未创建或迁移的类型/系统/接口。

## 3. 边界模型与所有权图

```text
world/tile facts + player input + equipment/item facts
  -> environment/tile/physics queries
  -> one owner system commits P08 state
  -> movement/collision, spawn, combat, projectile and presentation queries
  -> explicit network/persistence/audio/UI adapters
```

必须保持以下方向：

- `PlayerZoneAndEnvironmentStateComponent` 只保存玩家当前区域事实和 transition latch；区域 property 继续作为兼容 query facade，不能让 UI 或网络 adapter 直接写组件。
- `PlayerTileTargetingAndRangeStateComponent` 只拥有玩家实例的 `lastTileRangeX/Y` 与 `adjTile` 候选状态。`tileTargetX/Y`、`tileRangeX/Y`、默认 range 和 item-grab 常量当前是 legacy static/global 或定义值，不能因字段名相近而复制为每个实体的 authority。
- `PlayerMovementPhysicsStateComponent` 只拥有实例化移动参数。`defaultGravity`、`jumpHeight`、`jumpSpeed` 是静态可变 legacy 输入，须经明确的 definition/config adapter 或 integration-review 才能进入 ECS。
- `ResetEffects` 清理的能力字段必须由单一效果提交系统重建；移动、碰撞和 spawn 只通过 Query 消费 committed facts，不跨组件隐式写入。
- luck 是从 torch、ladybug、equipment、coin、kite 和世界事实计算的派生权威结果；计算函数保持纯，提交和同步由单一 owner system 负责。`luckNeedsSync` 是 replication flag，不是 luck 计算输入。
- `Item`、`PlayerMovementAccsCache`、`SlotId`、日志句柄和音频句柄不直接渗透为 P08 巨型组件；需要显式 domain payload、cache owner 或 adapter seam。
- 文件、目录和 Markdown 顺序不定义运行时顺序。所有先后关系写入 scheduler contract，并由 focused verifier 固定。

## 4. 建议的最小模块边界

| 检查点 | proposed 类型/模块 | authority/状态类型 | 输入 | 输出与副作用 |
|---|---|---|---|---|
| C01 | `PlayerZoneAndEnvironmentStateComponent`; `PlayerZoneCommitSystem`; `PlayerZoneQuery`; `ShimmerTransitionAdapter` | zone bits、免疫计时器、transition latch；authoritative per-player state | tile/world snapshot、player position、environment effects | zone query、zone replication snapshot、shimmer transition event |
| C02 | `PlayerTileTargetingAndRangeStateComponent`; `TileTargetQuery`; `TileRangeCommitSystem`; `ItemPickupRangeAdapter` | last range、adjacency；static target/range 先留 compatibility seam | input/world coordinates、equipment range facts、tile catalog | immutable target/range snapshot、Item pickup command |
| C03 | `PlayerMovementPhysicsStateComponent`; `MovementParameterQuery`; `MovementParameterCommitSystem` | per-player gravity/run/fall/jump parameters；static defaults external | input、mount/equipment/buff facts、config snapshot | movement facts、explicit movement command/query |
| C04 | `PlayerEnvironmentMobilityStateComponent`; `MobilityCapabilitySystem`; `MobilityQuery` | equipment-derived mobility and transient swim state | equipment/buff/environment facts | collision/movement/spawn capability facts |
| C05 | `PlayerEnvironmentDetectionAndSpawnStateComponent`; `EnvironmentDetectionSystem`; `SpawnQualificationQuery`; `LuckInputProjection` | detection facts, progression inputs, luck sync inputs | tile scan、equipment、world progression、network input | detection/spawn queries、luck input event；presentation field only as projection |
| C06 | `PlayerArmorAndCombatEffectsComponent`; `ArmorEffectCommitSystem`; `CombatEffectQuery`; `ItemEffectAdapter` | armor combat effect facts; `honeyCombItem` remains external payload seam | equipment/use/hit facts | combat effect events/queries；Item effect adapter side effect |
| C07 | `PlayerArmorSetAndTurretStateComponent`; `ArmorSetCommitSystem`; `TurretCapacityQuery` | set flags、nebula cooldown、turret capacity、vortex stealth；old max as snapshot | equipment/ability facts | ability/combat/projectile queries；network snapshot adapter |
| C08 | `PlayerGravityAndWaterTraversalStateComponent`; `TraversalCapabilitySystem`; `GravityQuery` | water walk and gravity control facts | equipment/input/world gravity facts | movement/collision query；no direct physics double-write |
| C09 | `PlayerLuckAndRescanStateComponent`; `LuckCalculationQuery`; `LuckCommitSystem`; `WallRescanSystem`; `AudioAdapter` | luck authority, rescan cache, replication marker and explicit sub-seams | luck inputs, position, time port, movement cache | luck snapshot, wall facts, sync command, audio effect |

每个 proposed 类型都必须先有唯一 writer、读写矩阵和 focused verifier，再开始实现。当前 C01-C03 的组件源码已保存；writer、协议、static defaults 和跨域 owner 仍保持 integration-review，不能把组件存在误报为完整迁移。

## 5. 全量成员归属表

以下表格逐行覆盖 P08 权威报告的 100 个来源序号。来源序号唯一且保持报告顺序；“候选归属”只表示设计意图，所有类型/系统均为 `status: proposed`。静态字段、兼容字段、派生字段和外部句柄不会被默认当作 per-entity authority。

### C01 PlayerZoneAndEnvironmentState

| 来源序号 | 成员 | C# 类型 | Version4 行 | 状态/候选归属 | 证据与边界 |
|---:|---|---|---:|---|---|
| 662 | environmentBuffImmunityTimer | int | 917 | authoritative timer -> proposed `PlayerZoneAndEnvironmentStateComponent` | `Player.cs:917`; Reset/环境效果 writer 需闭合 |
| 665 | zone1 | Terraria.BitsByte | 923 | authoritative zone bit -> proposed component | `Player.cs:923`; NetMessage zone output confirmed |
| 666 | zone2 | Terraria.BitsByte | 925 | authoritative zone bit -> proposed component | `Player.cs:925`; NetMessage zone output confirmed |
| 667 | zone3 | Terraria.BitsByte | 927 | authoritative zone bit -> proposed component | `Player.cs:927`; NetMessage zone output confirmed |
| 668 | zone4 | Terraria.BitsByte | 929 | authoritative zone bit -> proposed component | `Player.cs:929`; NetMessage zone output confirmed |
| 669 | zone5 | Terraria.BitsByte | 931 | authoritative zone bit -> proposed component | `Player.cs:931`; NetMessage zone output confirmed |
| 670 | _wasInShimmerZone | bool | 933 | transition latch -> proposed component/adapter seam | `Player.cs:933,9926-9945`; event ordering partial |

### C02 PlayerTileTargetingAndRangeState

| 来源序号 | 成员 | C# 类型 | Version4 行 | 状态/候选归属 | 证据与边界 |
|---:|---|---|---:|---|---|
| 1136 | DefaultTileRangeX | int | 1905 | static readonly definition -> proposed range definition adapter | `Player.cs:1905`; not per-player authority |
| 1137 | DefaultTileRangeY | int | 1907 | static readonly definition -> proposed range definition adapter | `Player.cs:1907`; not per-player authority |
| 1138 | tileRangeX | int | 1909 | static mutable legacy input -> integration-review | `Player.cs:1909`; public API and Projectile readers exist |
| 1139 | tileRangeY | int | 1911 | static mutable legacy input -> integration-review | `Player.cs:1911`; public API and Projectile readers exist |
| 1140 | lastTileRangeX | int | 1913 | per-player cache -> proposed tile range component | `Player.cs:1913,15636-15644`; invalidation required |
| 1141 | lastTileRangeY | int | 1915 | per-player cache -> proposed tile range component | `Player.cs:1915,15636-15644`; invalidation required |
| 1142 | tileTargetX | int | 1917 | static input workspace -> compatibility/input adapter | `Player.cs:1917,15109-15154`; no per-player copy without concurrency decision |
| 1143 | tileTargetY | int | 1919 | static input workspace -> compatibility/input adapter | `Player.cs:1919,15109-15154`; no per-player copy without concurrency decision |
| 1152 | adjTile | bool[] | 1939 | per-player mutable adjacency state -> proposed tile component | `Player.cs:1939,26566`; TileID.Count capacity must be verified |
| 1153 | defaultItemGrabRange | int | 1941 | static definition/config -> proposed pickup-range adapter | `Player.cs:1941,19927-19998`; no entity authority yet |
| 1154 | itemGrabSpeed | float | 1943 | static tuning value -> proposed pickup-range adapter | `Player.cs:1943,19927-19998`; time/clock seam required |
| 1155 | itemGrabSpeedMax | float | 1945 | static tuning value -> proposed pickup-range adapter | `Player.cs:1945,19927-19998`; time/clock seam required |

### C03 PlayerMovementPhysicsState

| 来源序号 | 成员 | C# 类型 | Version4 行 | 状态/候选归属 | 证据与边界 |
|---:|---|---|---:|---|---|
| 1144 | defaultGravity | float | 1921 | static mutable default -> integration-review/config adapter | `Player.cs:1921`; do not duplicate into every entity |
| 1145 | jumpHeight | int | 1923 | static mutable tuning -> integration-review/config adapter | `Player.cs:1923`; UpdateJumpHeight writer partial |
| 1146 | jumpSpeed | float | 1925 | static mutable tuning -> integration-review/config adapter | `Player.cs:1925`; UpdateJumpHeight writer partial |
| 1147 | gravity | float | 1927 | per-player authoritative movement fact -> proposed component | `Player.cs:1927,14789+`; mount/equipment ordering required |
| 1148 | maxFallSpeed | float | 1929 | per-player derived movement fact -> proposed component | `Player.cs:1929,14789+`; reset/rebuild ordering required |
| 1149 | maxRunSpeed | float | 1931 | per-player derived movement fact -> proposed component | `Player.cs:1931,14789+`; P07 integration-review |
| 1150 | runAcceleration | float | 1933 | per-player derived movement fact -> proposed component | `Player.cs:1933,14789+`; P07 integration-review |
| 1151 | runSlowdown | float | 1935 | per-player derived movement fact -> proposed component | `Player.cs:1935,14789+`; P07 integration-review |

### C04 PlayerEnvironmentMobilityState

| 来源序号 | 成员 | C# 类型 | Version4 行 | 状态/候选归属 | 证据与边界 |
|---:|---|---|---:|---|---|
| 1240 | canFloatInWater | bool | 2124 | derived mobility capability -> proposed mobility component | `Mount.cs:4426` writes; Mount/P07 owner review required |
| 1241 | hasFloatingTube | bool | 2126 | equipment-derived mobility fact -> proposed mobility component | `Player.cs:2126`; ResetEffects/equipment rebuild |
| 1242 | frogLegJumpBoost | bool | 2128 | equipment-derived mobility fact -> proposed mobility component | `Player.cs:2128`; jump query consumer |
| 1243 | skyStoneEffects | bool | 2130 | equipment-derived effect fact -> proposed mobility component | `Player.cs:2130`; effect reset/rebuild |
| 1244 | spawnMax | bool | 2132 | environment/spawn capability -> proposed mobility component | `Player.cs:2132`; P14 integration-review |
| 1245 | blockRange | int | 2134 | derived interaction range -> proposed mobility query output | `Player.cs:2134`; overlaps C02 range seam |
| 1258 | jumpBoost | bool | 2160 | equipment-derived jump capability -> proposed mobility component | `Player.cs:2160`; P07 scheduler dependency |
| 1259 | noFallDmg | bool | 2162 | derived damage-immunity capability -> integration-review with P06/P07 | `Player.cs:2162`; combat/movement boundary |
| 1260 | swimTime | int | 2164 | transient movement/frame state -> proposed mobility component | `Player.cs:2164`; clock and collision writer required |
| 1266 | lavaImmune | bool | 2176 | derived environment immunity -> proposed mobility component | `Player.cs:2176`; lava resource owner P09 review |
| 1267 | gills | bool | 2178 | equipment-derived breathing capability -> proposed mobility component | `Player.cs:2178`; P09 resource query input |
| 1268 | slowFall | bool | 2180 | derived movement capability -> proposed mobility component | `Mount.cs:2867` reads; P03/P07 integration-review |

### C05 PlayerEnvironmentDetectionAndSpawnState

| 来源序号 | 成员 | C# 类型 | Version4 行 | 状态/候选归属 | 证据与边界 |
|---:|---|---|---:|---|---|
| 1261 | killGuide | bool | 2166 | progression/spawn input -> proposed detection component; P14 owner review | `Player.cs:2166`; not environment scan fact |
| 1262 | killClothier | bool | 2168 | progression/spawn input -> proposed detection component; P14 owner review | `Player.cs:2168`; not environment scan fact |
| 1263 | equipmentBasedLuckBonus | float | 2170 | luck input projection -> proposed luck seam | `Player.cs:2170,10126-10253`; P09 equipment input |
| 1264 | lastEquipmentBasedLuckBonus | float | 2172 | compatibility/delta snapshot -> proposed luck seam | `Player.cs:2172`; sync/invalidation partial |
| 1265 | hasCreditsSceneMusicBox | bool | 2174 | presentation/equipment projection -> P11 integration-review | `Player.cs:2174`; not P08 authority by default |
| 1269 | findTreasure | bool | 2182 | environment detection fact -> proposed detection component | `Player.cs:2182`; ResetEffects rebuild |
| 1270 | biomeSight | bool | 2184 | environment detection fact -> proposed detection component | `Player.cs:2184`; P17 query boundary |
| 1271 | invis | bool | 2186 | derived presentation/detection fact -> P11/P06 integration-review | `Player.cs:2186`; do not duplicate stealth authority |
| 1272 | detectCreature | bool | 2188 | environment detection fact -> proposed detection component | `Player.cs:2188`; query-only consumers |
| 1273 | nightVision | bool | 2190 | environment presentation/detection fact -> proposed detection component | `Player.cs:2190`; renderer adapter reads |
| 1274 | enemySpawns | bool | 2192 | spawn qualification fact -> proposed detection component; P14 owner review | `Player.cs:2192`; world spawn query consumes |
| 1282 | insideUnbreakableWalls | bool | 2208 | wall scan result -> proposed detection projection | `Player.cs:2208,17741-17758`; cache invalidation required |
| 1283 | CanSeeInvisibleBlocks | bool | 2210 | derived detection capability -> proposed detection component | `Player.cs:2210`; wall/tile query consumer |

### C06 PlayerArmorAndCombatEffects

| 来源序号 | 成员 | C# 类型 | Version4 行 | 状态/候选归属 | 证据与边界 |
|---:|---|---|---:|---|---|
| 1275 | thorns | float | 2194 | authoritative combat effect fact -> proposed armor effect component | `Player.cs:2194`; P06 consumes, equipment writes |
| 1276 | turtleArmor | bool | 2196 | armor effect fact -> proposed armor effect component | `Player.cs:2196`; ResetEffects/rebuild |
| 1277 | turtleThorns | bool | 2198 | armor effect fact -> proposed armor effect component | `Player.cs:2198`; P06 combat query |
| 1278 | cactusThorns | bool | 2200 | armor effect fact -> proposed armor effect component | `Player.cs:2200`; P06 combat query |
| 1279 | spiderArmor | bool | 2202 | armor effect fact -> proposed armor effect component | `Player.cs:2202`; equipment effect writer |
| 1280 | anglerSetSpawnReduction | bool | 2204 | armor-derived spawn fact -> P14 integration-review | `Player.cs:2204`; spawn query must read explicit fact |
| 1281 | vampireBurningInSunlight | bool | 2206 | combat/environment effect fact -> proposed armor effect component | `Player.cs:2206`; time/world-light input seam |
| 1308 | honeyCombItem | Terraria.Item | 2260 | external Item effect payload -> proposed `ItemEffectAdapter` seam | `Player.cs:2260`; Item remains external authority |

### C07 PlayerArmorSetAndTurretState

| 来源序号 | 成员 | C# 类型 | Version4 行 | 状态/候选归属 | 证据与边界 |
|---:|---|---|---:|---|---|
| 1284 | setSolar | bool | 2212 | armor set fact -> proposed armor set component | `Player.cs:2212`; UpdateArmorSets writer |
| 1285 | setVortex | bool | 2214 | armor set fact -> proposed armor set component | `Player.cs:2214`; P06/P11 reads require seam |
| 1286 | setNebula | bool | 2216 | armor set fact -> proposed armor set component | `Player.cs:2216`; UpdateArmorSets writer |
| 1287 | nebulaCD | int | 2218 | set-effect cooldown -> proposed armor set component | `Player.cs:2218`; timer semantics require verifier |
| 1288 | setStardust | bool | 2220 | armor set fact -> proposed armor set component | `Player.cs:2220`; UpdateArmorSets writer |
| 1289 | setForbidden | bool | 2222 | armor set fact -> proposed armor set component | `Player.cs:2222`; UpdateArmorSets writer |
| 1290 | setForbiddenCooldownLocked | bool | 2224 | armor set cooldown gate -> proposed armor set component | `Player.cs:2224`; ability query consumes |
| 1291 | setChlorophyte | bool | 2226 | armor set fact -> proposed armor set component | `Player.cs:2226`; equipment effect writer |
| 1292 | setSquireT3 | bool | 2228 | armor set fact -> proposed armor set component | `Player.cs:2228`; ability/P05 integration-review |
| 1293 | setHuntressT3 | bool | 2230 | armor set fact -> proposed armor set component | `Player.cs:2230`; ability/P05 integration-review |
| 1294 | setApprenticeT3 | bool | 2232 | armor set fact -> proposed armor set component | `Player.cs:2232`; ability/P05 integration-review |
| 1295 | setMonkT3 | bool | 2234 | armor set fact -> proposed armor set component | `Player.cs:2234`; ability/P05 integration-review |
| 1296 | setSquireT2 | bool | 2236 | armor set fact -> proposed armor set component | `Player.cs:2236`; ability/P05 integration-review |
| 1297 | setHuntressT2 | bool | 2238 | armor set fact -> proposed armor set component | `Player.cs:2238`; ability/P05 integration-review |
| 1298 | setApprenticeT2 | bool | 2240 | armor set fact -> proposed armor set component | `Player.cs:2240`; ability/P05 integration-review |
| 1299 | setMonkT2 | bool | 2242 | armor set fact -> proposed armor set component | `Player.cs:2242`; ability/P05 integration-review |
| 1300 | maxTurrets | int | 2244 | turret capacity authority candidate -> reconcile existing `PlayerAbilityComponent` | `Player.cs:2244`; duplicate authority risk |
| 1301 | maxTurretsOld | int | 2246 | compatibility/diff snapshot -> existing ability integration-review | `Player.cs:2246`; not independent capacity authority |
| 1302 | vortexStealthActive | bool | 2248 | derived stealth projection -> P06/P11 integration-review | `Player.cs:2248`; do not create second stealth writer |

### C08 PlayerGravityAndWaterTraversalState

| 来源序号 | 成员 | C# 类型 | Version4 行 | 状态/候选归属 | 证据与边界 |
|---:|---|---|---:|---|---|
| 1303 | waterWalk | bool | 2250 | traversal capability -> proposed traversal component | `Player.cs:2250`; Collision reads |
| 1304 | waterWalk2 | bool | 2252 | traversal capability -> proposed traversal component | `Player.cs:2252`; Collision reads |
| 1305 | forcedGravity | int | 2254 | gravity direction fact -> proposed traversal component | `Player.cs:2254`; Physics/P07 integration-review |
| 1306 | gravControl | bool | 2256 | gravity input capability -> proposed traversal component | `Player.cs:2256`; input/equipment writer |
| 1307 | gravControl2 | bool | 2258 | gravity secondary capability -> proposed traversal component | `Player.cs:2258`; input/equipment writer |

### C09 PlayerLuckAndRescanState

| 来源序号 | 成员 | C# 类型 | Version4 行 | 状态/候选归属 | 证据与边界 |
|---:|---|---|---:|---|---|
| 1386 | torchLuck | float | 2423 | luck input authority -> proposed luck state | `Player.cs:2423,10126-10253`; torch/world input |
| 1387 | happyFunTorchTime | bool | 2425 | luck input flag -> proposed luck state | `Player.cs:2425`; timer/lifecycle writer partial |
| 1388 | ladyBugLuckTimeLeft | int | 2429 | luck timer -> proposed luck state; NPC writer integration-review | `Player.cs:2429`; `NPC.cs:67858-67861` writes |
| 1389 | luck | float | 2431 | derived committed luck -> proposed luck state | `Player.cs:2431,10126-10253`; pure calculation then commit |
| 1390 | luckMinimumCap | float | 2433 | rule/config cap -> proposed luck rules adapter | `Player.cs:2433`; configurable but not effect writer |
| 1391 | luckMaximumCap | float | 2435 | rule/config cap -> proposed luck rules adapter | `Player.cs:2435`; configurable but not effect writer |
| 1392 | coinLuck | float | 2437 | luck input projection -> proposed luck state | `Player.cs:2437`; coin/economy input seam |
| 1393 | kiteLuckLevel | byte | 2439 | luck input projection -> proposed luck state | `Player.cs:2439`; item/world input seam |
| 1394 | luckNeedsSync | bool | 2441 | replication flag -> proposed luck sync adapter | `Player.cs:2441`; not luck authority |
| 1395 | disableVoidBag | int | 2443 | inventory capability/input -> P09 integration-review | `Player.cs:2443`; no duplicate void-bag owner |
| 1396 | movementAbilitiesCache | Terraria.DataStructures.PlayerMovementAccsCache | 2445 | movement cache -> P07/P03 integration-review | `Player.cs:2445`; cache owner/lifetime unclosed |
| 1397 | UnbreakableWallRescanPeriod | int | 2449 | static readonly scan rule -> proposed wall-rescan rules adapter | `Player.cs:2449`; not entity state |
| 1398 | UnbreakableWallRescanDistance | int | 2451 | static readonly scan rule -> proposed wall-rescan rules adapter | `Player.cs:2451`; not entity state |
| 1399 | _unbreakableWallScanCooldown | int | 2453 | per-player rescan cache -> proposed luck/rescan component | `Player.cs:2453,17741-17758`; time port required |
| 1400 | _unbreakableWallScanLastPosition | Vector2 | 2455 | per-player rescan cache -> proposed luck/rescan component | `Player.cs:2455,17741-17758`; position invalidation required |
| 1401 | _sizzleAudioHandle | SlotId | 2457 | external audio handle -> proposed `AudioAdapter` seam | `Player.cs:2457,17725-17732`; no persistent ECS authority |

## 6. 调度、网络、持久化与副作用边界

### 6.1 调度契约

1. Spawn/rehydration 创建 Player entity、已有 PlayerAbility/Equipment/Mount/Physics 关系和 P08 proposed state；未完成初始化不得运行 environment/tile Query。
2. `ResetDerivedPlayerEffects` 清理 C04-C08 中由装备/Buff 重建的 flag；该步骤只能有一个写者，不能让 legacy Player 和 proposed component 双写。
3. `PlayerZoneCommitSystem` 读取 world/tile snapshot 后提交 zone bits、免疫 timer 和 shimmer latch；zone property facade 只读 committed state。
4. `TileRangeCommitSystem` 读取 equipment/input facts，更新 per-player last range/adjacency；static tile target/range 仍通过 compatibility adapter，不能并发共享写入。
5. `MovementParameterCommitSystem` 在装备/Buff/Mount facts 已提交后计算 gravity/run/fall/jump facts；之后 P07 movement system 和 Collision 只 Query 读取。
6. `MobilityCapabilitySystem` 与 `ArmorEffectCommitSystem` 在 Reset 后重建环境能力和 combat effects；spawn/Combat 只消费结果。
7. `ArmorSetCommitSystem` 提交套装和 turret snapshot，必须先与现有 `PlayerAbilityComponent.MaximumTurrets`/`PreviousMaximumTurrets` 做唯一 owner reconciliation。
8. `WallRescanSystem` 使用显式 `ITimeSource`/位置输入更新 rescan cache；`LuckCalculationQuery` 保持纯，`LuckCommitSystem` 再写 luck/luckNeedsSync。
9. Network、persistence、audio、UI、renderer 和 logger adapters 只能消费 committed snapshot/command result；不能从外部回写 P08 authority。

### 6.2 网络边界

已确认 zone1..zone5 的 NetMessage 写出和 MessageBuffer 读入/shimmer 行为。其余字段没有在本会话中取得完整 protocol matrix，因此只设计 `ZoneReplicationAdapter`、`LuckSyncAdapter` 和必要的 committed snapshots，均为 `status: proposed`。没有证据时不能把 armor set、movement、luck 或 static range 字段加入现有 packet，也不能宣称 client/server 端别已闭合。

### 6.3 持久化边界

当前 Version4 `Serialize`/`Deserialize` 为空实现。不得将完整参考目录的字段顺序直接写成 Version4 协议。若后续实现持久化，必须先建立版本化 `PlayerPersistenceAdapter`，区分 authority state、rebuildable projection、cache、static definition 和外部 Item/Audio handle，并为未知/缺失字段定义默认值和回滚策略。P08 当前 `verificationStatus` 仍为 `not-run`。

### 6.4 副作用隔离

时间、随机性、日志、网络发送、音频停止/播放、Item effect 和文件流均是 effect boundary。`_sizzleAudioHandle` 只由 audio adapter 管理；`honeyCombItem` 只作为外部 Item payload seam；wall scan 通过 time/position input 计算，不在 Query 中隐式读取系统时间或写回缓存。

## 7. 不拆分项与兼容策略

- 不创建一个 `PlayerEnvironmentComponent` 包含全部 100 个字段；tile target、movement、armor set、luck 和 audio 的生命周期及访问者明显不同。
- 不把 static `tileTargetX/Y`、`tileRangeX/Y`、`defaultGravity`、`jumpHeight`、`jumpSpeed`、默认 item grab 参数和 wall-rescan 常量伪装成每实体数据；先保留 definition/input adapter seam。
- 不复制 `maxTurrets` 到新的独立 authority；当前 `PlayerAbilityComponent` 已有 `MaximumTurrets` 和 `PreviousMaximumTurrets`，C07 必须先 reconciliation。
- 不把 `honeyCombItem` 或 `movementAbilitiesCache` 直接嵌入新组件来绕开 Item/Movement owner；保留显式 adapter 或 integration-review。
- 不把 `invis`、`vortexStealthActive`、`hasCreditsSceneMusicBox` 当作 P08 单独拥有的表现 authority；与 P06/P11 只通过 Query/Projection 交接。
- 旧 Player 字段在迁移期间只能是兼容 view 或 adapter 输入，不能与新组件双写。源/目标路径、依赖影响和回滚点必须在实现前记录。

## 8. Focused verifier 计划

本会话未创建或运行 verifier。实现阶段至少需要以下 focused checks，结果在实现文档和本文件中保持 `verificationStatus: not-run` 直到实际执行：

| 验证组 | 必测不变量 | 失败时阻止 |
|---|---|---|
| metadata/report verifier | 两份文档 metadata 同步；100 个来源序号在 mapping 区域各一次；P08 成员无遗漏/重复 | Complete |
| zone/replication | zone1..5 round-trip、默认 zero、shimmer enter/leave 只发一次 transition | C01 implementation |
| tile/range | static workspace 不跨玩家污染；last range/adjTile 清理和 invalidation 正确；pickup range 不双扣 | C02 implementation |
| movement | Reset -> equipment/mount/buff -> parameter commit -> movement 消费顺序可重复；gravity/run/fall defaults 正确 | C03 implementation |
| mobility/traversal | water/lava/gills/slowFall/waterWalk flags 只由唯一 writer 重建，Collision 只读 | C04/C08 implementation |
| detection/spawn | wall scan cooldown/distance、inside wall、enemy spawn 和 progression flags 不互相伪装 | C05 implementation |
| armor/set/turret | ResetEffects 后效果重建；maxTurrets 与 PlayerAbility 无双写；thorns/turtle/vortex 查询一致 | C06/C07 implementation |
| luck | pure luck calculation、caps、ladybug/NPC input、coin/kite、luckNeedsSync commit 可重复 | C09 implementation |
| persistence/network/effects | 空 Serialize/Deserialize 不被误报为闭合；未知字段、网络重复、audio release 和 Item adapter 资源安全 | final integration |

## 9. C01 检查点记录

`C01 PlayerZoneAndEnvironmentState` 已保存组件源码 `src/Player/Environment/PlayerZoneAndEnvironmentStateComponent.cs`。已落地：7 个字段的窄组件边界、零/false 默认值、内部写入边界和 P08 来源序号注记；由于 NLTX 当前没有 `Terraria.BitsByte` 或组件注册/协议属性基础设施，zone 字节暂以 `byte` 表示。未闭合：全部 zone property reader/writer 矩阵、唯一 zone writer、网络端别、持久化格式、世界扫描的最终 owner 和 P17/P11/P14 交接。组件实现状态为 `partial/implemented`，验证状态仍为 `not-run`。

C01-C08 当时的下一检查点已依次保存，当前已完成全部 9 个设计检查点。

## 11. C03 检查点记录

`C03 PlayerMovementPhysicsState` 已完成设计检查点并已与执行文档同步保存。已闭合：8 个字段的来源序号/声明行、gravity/maxFall/maxRun/acceleration/slowdown 的 per-player 候选 owner、`defaultGravity`/`jumpHeight`/`jumpSpeed` 的 static definition/config seam、`UpdateJumpHeight` 与移动参数重建的顺序风险，以及现有 Physics/Movement 组件的重复 writer 风险。未闭合：P03 Mount 与 P07 Movement 的最终 authority、jump tuning 的并发隔离、时间步长语义、网络/持久化格式和实际 verifier 结果。

C03 不把 static defaults 复制为实体状态；实现前必须先确定 `MovementDefinitionSnapshot` 的版本化来源和单一 `MovementParameterCommitSystem`。下一检查点为 C04；在 C04 两份文档保存前不得开始 C05。

## 12. C04 检查点记录

`C04 PlayerEnvironmentMobilityState` 已保存组件源码 `src/Player/Environment/PlayerEnvironmentMobilityStateComponent.cs`。实际落地 12 个标量状态：`CanFloatInWater`、`HasFloatingTube`、`FrogLegJumpBoost`、`SkyStoneEffects`、`SpawnMax`、`BlockRange`、`JumpBoost`、`NoFallDamage`、`SwimTime`、`LavaImmune`、`Gills` 和 `SlowFall`，默认值为 `false`/`0`。未添加 ResetEffects、装备/Buff 重建、时钟、Collision、Mount、spawn 或 P09 资源行为；`NoFallDamage` 保持与 Version4 `noFallDmg` 的语义对应。未闭合：最终 mobility owner、Mount 对 `canFloatInWater` 的写入迁移、lava/breath 资源与 P09 的边界、`blockRange` 的唯一 range projection、network/persistence 格式和实际 verifier 结果。组件实现状态为 `partial/implemented`，验证状态仍为 `not-run`。

C04 的候选组件不持有输入、Item 或世界对象；只保存可查询的 capability facts。`swimTime` 仍需要显式 tick/clock port，`blockRange` 需要与 C02 的范围 projection 选择唯一 writer。保存本 checkpoint 后执行游标推进为 `currentComponent: C05`，`pendingComponents: [C05, C06, C07, C08, C09]`。

## 13. C05 检查点记录

`C05 PlayerEnvironmentDetectionAndSpawnState` 已保存组件源码 `src/Player/Environment/PlayerEnvironmentDetectionAndSpawnStateComponent.cs`。实际落地 13 个事实字段，包括 progression 输入、luck bonus 快照、环境探测资格、spawn 资格、墙体扫描结果和可见性能力；`invis` 以语义化的 `IsInvisible` 表示。没有添加 spawn query、luck calculation、wall scan、renderer/audio 或 P11/P14/P17 adapter。未闭合：P14 对 spawn flags 的最终 owner、P17 biome query、P11 presentation/stealth projection、wall scan 的完整生命周期、网络/持久化格式和实际 verifier 结果。组件实现状态为 `partial/implemented`，验证状态仍为 `not-run`。

C05 的 `EnvironmentDetectionSystem` 只提交事实；`SpawnQualificationQuery`、`LuckInputProjection` 和 P11 projection 不能互相写回。`killGuide`/`killClothier` 不得伪装成扫描结果，`insideUnbreakableWalls` 也不得由 UI 或 renderer 直接修改。保存本 checkpoint 后执行游标推进为 `currentComponent: C06`，`pendingComponents: [C06, C07, C08, C09]`。

## 14. C06 检查点记录

`C06 PlayerArmorAndCombatEffects` 已保存组件源码 `src/Player/Combat/PlayerArmorAndCombatEffectsComponent.cs`。实际落地 7 个可独立保存的 armor/combat facts：`Thorns`、`TurtleArmor`、`TurtleThorns`、`CactusThorns`、`SpiderArmor`、`AnglerSetSpawnReduction` 和 `VampireBurningInSunlight`。权威报告中的 `honeyCombItem` 未复制进组件，继续保留为 ItemEffectAdapter 的短生命周期外部 payload seam。未闭合：P06 combat effect 的最终 owner、P14 spawn reduction 的最终 owner、Item payload 的失效/释放语义、网络/持久化格式和实际 verifier 结果。组件实现状态为 `partial/implemented`，验证状态仍为 `not-run`。

C06 只提交不可变或可复制的 combat effect facts；Combat resolution 不由 P08 直接写 hit/cooldown，Item effect adapter 也不得把完整 `Terraria.Item` 复制进组件。保存本 checkpoint 后执行游标推进为 `currentComponent: C07`，`pendingComponents: [C07, C08, C09]`。

## 15. C07 检查点记录

`C07 PlayerArmorSetAndTurretState` 已保存组件源码 `src/Player/Combat/PlayerArmorSetAndTurretStateComponent.cs`。实际落地 17 个可独立保存的套装、冷却和隐身事实；`maxTurrets`/`maxTurretsOld` 未复制，因为现有 `PlayerAbilityComponent.MaximumTurrets`/`PreviousMaximumTurrets` 已是候选 owner。未闭合：能力域最终 owner、P05 minion/turret 消费协议、P06 stealth/combat 交接、P09 equipment writer、网络/持久化格式和实际 verifier 结果。组件实现状态为 `partial/implemented`，验证状态仍为 `not-run`。

C07 不创建第二份 turret capacity authority；先做 existing component reconciliation，再决定是扩展已有 owner、建立只读 adapter，还是将 proposed 字段仅作为兼容 projection。套装 flags 的提交只能发生在装备事实已提交且 ResetEffects 已完成之后。保存本 checkpoint 后执行游标推进为 `currentComponent: C08`，`pendingComponents: [C08, C09]`。

## 16. C08 检查点记录

`C08 PlayerGravityAndWaterTraversalState` 已完成最小组件源码实现并保存：`src/Player/Movement/PlayerGravityAndWaterTraversalStateComponent.cs`。实际字段为 `WaterWalk`、`WaterWalk2`、`ForcedGravity`、`GravControl` 和 `GravControl2`，默认值为 `false`/`0`；组件只保存 traversal/gravity facts，不写 velocity、position 或 movement behavior。已闭合：5 个字段的来源序号/声明行、waterWalk/waterWalk2 的 Collision capability 边界、forcedGravity 的方向事实、gravControl/gravControl2 的输入/装备资格，以及它们与 C03 movement parameters、P03 Mount、P07 Movement/Physics 的依赖方向。未闭合：最终 traversal owner、world gravity 与 input/equipment 的优先级、Collision 迁移顺序、网络/持久化格式和实际 verifier 结果。组件实现状态为 `partial/implemented`，验证状态仍为 `not-run`。

C08 只提交 traversal/gravity facts，不能直接修改 Physics/Movement 的速度或位置；C03 负责参数，C08 负责方向/控制资格，P07/P03/Collision 通过 Query 消费。保存本 checkpoint 后执行游标推进为 `currentComponent: C09`，`pendingComponents: [C09]`。

## 17. C09 检查点记录

`C09 PlayerLuckAndRescanState` 已完成最小组件源码实现并保存：`src/Player/Luck/PlayerLuckAndRescanStateComponent.cs`。实际落地 9 个 per-player 状态字段：`TorchLuck`、`HappyFunTorchTime`、`LadyBugLuckTimeLeft`、`Luck`、`CoinLuck`、`KiteLuckLevel`、`LuckNeedsSync`、`UnbreakableWallScanCooldown` 和 `UnbreakableWallScanLastPosition`；默认值由 C# 默认初始化提供，为 `0`/`false`/`Vector2.Zero`。`LuckMinimumCap`/`LuckMaximumCap` 保留为 luck rules/config seam，`DisableVoidBag` 保留为 P09 inventory capability，`MovementAbilitiesCache` 保留为 P07/P03 movement cache owner，`UnbreakableWallRescanPeriod`/`UnbreakableWallRescanDistance` 保留为 static scan rules，`_sizzleAudioHandle` 保留为 AudioAdapter 外部句柄，均未复制进组件。已闭合：16 个字段的来源序号/声明行、torch/ladybug/equipment/coin/kite 输入与纯 luck calculation 的边界、caps 和 `LuckNeedsSync` 的提交语义、NPC 对 `LadyBugLuckTimeLeft` 的跨域写入、wall rescan period/distance/cooldown/last position 的 cache 生命周期以及跨分区 handoff 方向。未闭合：NPC/Item/Movement 的最终 owner、time/random 输入端口、网络同步字段、持久化格式、audio release failure 策略和实际 verifier 结果。组件实现状态为 `partial/implemented`，验证状态仍为 `not-run`。

C09 不把静态重扫常量、音频句柄或外部 movement cache 当作持久 ECS authority。`LuckCalculationQuery` 必须纯计算，`LuckCommitSystem` 是唯一 luck writer，`WallRescanSystem` 才能写 per-player scan cache；完成所有 checkpoints 不等于实现或验证完成。

## 18. 全部分区交付检查

- 9 个叶子检查点已逐个写入两份文档并按完成顺序保存；当前真实实现 metadata 为 `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08]`、`currentComponent: C09`、`pendingComponents: [C09]`。
- 100 个 P08 来源序号在本设计 mapping 中各出现一次，0 个属性；来源声明、类型和 Version4 行号保持与权威报告一致。
- C01-C09 的组件源码已保存，但仍是 partial implementation；System、Query、Command、Adapter 和 Projection 仍是设计候选，未声称已实现。执行状态保持 `in-progress`，实现状态保持 `in-progress`，验证状态保持 `not-run`。
- 跨分区 owner、static/global state、网络、持久化、时间、音频、Item、cache 和 scheduler gap 已明确记录；不存在基于文件修改时间的任务归属判断。
- 本会话已保存 C08/C09 组件源码；已通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建 `src/Player/Terraria.Player.csproj`，结果为 exit code 1、0 warnings、5 errors。错误均来自任务范围外的既有 `src/Player/Progression` 缺失类型引用；没有修改测试、项目文件、权威报告或 ledger，也未执行被禁止的 `git diff --check --`。预期的 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 存在但未由本次失败构建验证，视为 stale/unverified artifact。

## 10. C02 检查点记录

`C02 PlayerTileTargetingAndRangeState` 已保存组件源码 `src/Player/Interaction/PlayerTileTargetingAndRangeStateComponent.cs`。实际落地的状态是 `LastTileRangeX`、`LastTileRangeY` 和 `AdjacentTiles`；static `tileTargetX/Y`、`tileRangeX/Y`、默认 range 与 item-grab tuning 没有复制进实体组件。由于当前 NLTX 没有 `TileID.Count` 或等价 tile catalog 容量，`AdjacentTiles` 以空数组初始化，等待 tile definition/commit owner 提供容量；这不是邻接计算或范围提交实现。未闭合：多玩家并发时 static workspace 的隔离策略、装备改变后的 range invalidation、Item transfer 的最终 owner、tile catalog 容量、网络/持久化格式和 C02 的实际 verifier 结果。组件实现状态为 `partial/implemented`，验证状态仍为 `not-run`。

C02 的设计不把 static mutable fields 复制进 ECS entity；只有完成 input-context 或全局定义 owner 的 integration-review 后才允许改变该决定。保存本 checkpoint 后执行游标推进为 `currentComponent: C03`，`pendingComponents: [C03, C04, C05, C06, C07, C08, C09]`。

## C03 实现检查点

`C03 PlayerMovementPhysicsState` 已保存组件源码 `src/Player/Movement/PlayerMovementPhysicsStateComponent.cs`。实际落地的状态是 `Gravity`、`MaxFallSpeed`、`MaxRunSpeed`、`RunAcceleration` 和 `RunSlowdown`，默认值分别为 `0.4f`、`10f`、`3f`、`0.08f` 和 `0.2f`，与权威报告中的 Version4 实例初始化一致；三个 static tuning 字段 `defaultGravity`、`jumpHeight` 和 `jumpSpeed` 未复制进组件。没有添加 Movement System、Query、definition adapter 或测试。C03 状态为 `partial/implemented`；未验证 static tuning owner、P07/P03 调度、Physics/Movement duplicate writer、网络或持久化。

保存本 checkpoint 后执行游标推进为 `currentComponent: C04`，`pendingComponents: [C04, C05, C06, C07, C08, C09]`。
