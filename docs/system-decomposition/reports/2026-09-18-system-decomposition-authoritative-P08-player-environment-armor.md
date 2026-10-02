# System Decomposition Report: authoritative P08

```yaml
partitionId: P08
taskId: AUTH-SYS-P08
sessionId: ffcb132192224d1e8b2b878e48cddb71
inputReport: docs/migration/ledgers/authoritative-20-partitions/P08-Player-Environment-Armor.md
inputPrompt: docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P08-player-environment-armor-public-decomposition.md
outputReport: docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P08-player-environment-armor.md
designStatus: proposed
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: not-run
sourceModified: false
testsRun: false
buildRun: false
blockingDecision: crossSubsystemOwner=integration-review
```

## Scope and Evidence

本报告只处理 claim 输入中的 P08：`Terraria.Player` 的 9 组、100 个字段、0 个属性；不引入其他分区成员，不把权威字段清单当成运行时 owner 清单。

Version4 源码身份：`D:\TRbackup\Version4\Terraria\Player.cs`，SHA-256 `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`。CPG SQLite manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，project fingerprint 为 `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`，索引包含 967 shards、8,166,789 nodes、71,038,907 edges 和 1,317 diagnostics。CPG 没有 `SourceSnapshotId` 或逐文件源码 hash；`Get-CpgSourceExcerpt` 对 `Terraria/Player.cs` 返回 `unknown`，因此源码事实以下面单独计算的文件 hash 为准，CPG 结构事实不冒充同一源码快照。

只读查询 API 的记录：

- `Find-CpgSymbols` 与 `Get-CpgTypeSurface(Terraria.Player)` 返回 `complete`、1,511 个直接成员、0 gaps。这只确认该类型 shard 的静态成员清单。
- `Get-CpgMemberUses(zone1)` 在 `Player.cs`、`NetMessage.cs`、`MessageBuffer.cs` 范围返回 18 项、0 gaps；`MessageBuffer` 的写入 fact 为 `confirmed`，`NetMessage` 和部分 `Player` 访问的 `AccessMode` 为 `Unknown/partial`。
- `Get-CpgMemberUses(insideUnbreakableWalls)` 在 `Player.cs` 返回 4 项、0 gaps，其中可见写入 fact，也有 `Unknown/partial` 访问。
- `Get-CpgMemberUses(gravity)` 在 `Player.cs`、`Mount.cs` 返回 22 项；`Mount` 的示例访问为 `Unknown/partial`，不能推出唯一写者。
- `Get-CpgMemberUses(luck)` 在 `Player.cs` 返回 15 项；读写闭包仍含 `partial` 项。`Get-CpgMemberUses(maxTurrets)` 在 `Player.cs`、`Projectile.cs` 返回 12 项，也含 `Unknown/partial` 项。
- `Get-CpgCallableFacts(Player.Update(int))` 为 `partial`，只返回方法入口 CFG 节点，无 direct call targets，并有 `CalleeEffectsNotExpanded` gap。`Find-CpgCallSites` 的部分目标查询出现 `NoMatchingFactInScannedScope`。`DoUnbreakableWallScan` 的索引调用点返回两个 `Player.cs` fact，而源码另可见 `Player.Update(int)` 的第三处调用；索引调用点不作闭包证明。

目标源码关系以读取 `Player.cs` 及相关 `NetMessage.cs`、`MessageBuffer.cs` 原文确认：`Player.Update(int)` 相关顺序见 `Player.cs:15207,15296,15298,15308,15319-15321,15348,15353-15356`；Shimmer edge 与 spawn 见 `Player.cs:9937-9945,15099`；墙体扫描见 `Player.cs:17741-17760,21768,21827`；网络区位输入/输出见 `MessageBuffer.cs:1722-1733`、`NetMessage.cs:932-936`。这些是源码静态事实，不闭合运行时调度、未解析调用或所有网络入口。

参考项目 `C:\Users\shan\Downloads\ECS\space-station-14-master` 无可读取的 Git HEAD；只用本地文件 hash 固定参考范围：`AntiGravityClothingSystem.cs` SHA-256 `F159AF4E5B831FC3B9B187C1B5553AE2F2AFA935EEDA5285E557767138CF2928`，`GravityComponent.cs` SHA-256 `EAB2715F86F24C822DBF302BCCA81F38824B5BFAE91FC77827063D1B486D57A6`。`AntiGravityClothingSystem.cs:13-62` 显示该服装 System 订阅装备/姿态事件并调用 `SharedGravitySystem.RefreshWeightless`；`GravityComponent.cs:7-22` 单独存放 networked gravity flags。它只说明一种组织方式，不证明 Terraria 的输入、预测或 owner 语义。

## Prior Component Decomposition Reconciliation

P08 的既有 Component 设计文档记录 9 个设计检查点均为 `proposed`，跨域 owner、网络/存档协议、调度和 writer 尚未闭合；其执行状态标为 `in-progress`，不作为已迁移证据。当前 `src/NSSLC` 中可找到本分区 9 个 Component 声明，但在 `src` 内按类型名检索只命中各自声明文件，没有找到把这些状态接入运行路径的 P08 System。

现有 `PlayerBiomeZonePropertiesQuery` 使用单独的 `PlayerZoneSnapshot`，没有消费 `PlayerZoneAndEnvironmentStateComponent`，故是独立计算/验证证据，不是 P08 组件已接线的证据。`PlayerAbilityComponent` 与 `PlayerSummonCapacityState` 都暴露 `MaximumTurrets`、`PreviousMaximumTurrets`；P08 armor-set Component 又注明排除字段 1300-1301 以复用 `PlayerAbilityComponent`。权威写入者和两个状态类型的关系仍须 integration review，不能据类型注释认定唯一 owner。

本 System 复核调整既有按 Component 检查点组织的候选边界：C05 里的 `insideUnbreakableWalls` 与 C09 的扫描缓存合为一个空间扫描行为；C09 的 luck 聚合与 wall scan 拆开；C05 的 progression、luck 输入和 presentation 字段不与环境检测状态强行合并。Component 只承载状态，不据此推导一组件一 System。

## Conceptual Behaviors

以下每组列出完整的 P08 成员范围。组名是输入清单身份；概念行为 ID 是本报告的候选稳定映射，并不表示已经迁移。

### P08.Environment.ZoneAndTransition

输入组 `PlayerZoneAndEnvironmentState`（7）：`environmentBuffImmunityTimer`, `zone1`, `zone2`, `zone3`, `zone4`, `zone5`, `_wasInShimmerZone`。

行为：保存玩家可见的环境/区域 bit 与免疫计时；在 shimmer 区域边沿触发 Faeling spawn。源码可确认 zone byte 网络写出/读入及 shimmer 检测，但 zone bytes 的唯一环境扫描写者未知。接收入站 zone 包也会对 `zone5[0]` 边沿直接请求 spawn，不能在迁移中未经行为测试去重或合并。

### P08.Interaction.TileTargetAndRange

输入组 `PlayerTileTargetingAndRangeState`（12）：`DefaultTileRangeX`, `DefaultTileRangeY`, `tileRangeX`, `tileRangeY`, `lastTileRangeX`, `lastTileRangeY`, `tileTargetX`, `tileTargetY`, `adjTile`, `defaultItemGrabRange`, `itemGrabSpeed`, `itemGrabSpeedMax`。

行为：把本帧输入与屏幕/重力方向转成 tile target，按世界边界 clamp，并在特定 tile 形态下修正邻接目标；同步维护范围和拾取速度相关值。`Player.cs:1905-1919,1941-1945` 直接声明 defaults、`tileRangeX/Y`、`tileTargetX/Y` 和 item-grab tuning 为 static；`lastTileRangeX/Y` 与 `adjTile` 是实例字段。读取周边 tile 时源码会为 null tile 建立 `Tile`，故不是单纯的纯坐标 Query。全局工作区的多玩家隔离与并发规则 unknown，不得默认把 static 值复制为每玩家权威状态。

### P08.Traversal.PhysicsParameters

输入组 `PlayerMovementPhysicsState`（8）：`defaultGravity`, `jumpHeight`, `jumpSpeed`, `gravity`, `maxFallSpeed`, `maxRunSpeed`, `runAcceleration`, `runSlowdown`。

行为：为移动阶段提供跳跃、重力和奔跑参数。`defaultGravity`、`jumpHeight`、`jumpSpeed` 是 static 可变字段（`Player.cs:1921-1925`），实例 `gravity`、`maxFallSpeed`、`maxRunSpeed`、`runAcceleration`、`runSlowdown` 是 per-player 字段（`1927-1935`）。`Player.Update(int)` 又重置 gravity/jump/run 参数（`14798-14802`），其中会写入 static jump tuning；多玩家更新时的共享语义和并行安全 unknown，不能未经行为验证搬成互不共享的每实体值。Mount、装备与移动阶段的最终优先级仍 unknown。

### P08.Traversal.EnvironmentMobility

输入组 `PlayerEnvironmentMobilityState`（12）：`canFloatInWater`, `hasFloatingTube`, `frogLegJumpBoost`, `skyStoneEffects`, `spawnMax`, `blockRange`, `jumpBoost`, `noFallDmg`, `swimTime`, `lavaImmune`, `gills`, `slowFall`。

行为：提供由装备/环境共同影响的移动、液体穿越、跳跃与交互能力。应对移动/碰撞提供只读能力快照；`spawnMax`、`blockRange` 等并不因此自动成为同一不变量，来源及 consumers 仍需分项补证。

### P08.Environment.DetectionAndSpawnInputs

输入组 `PlayerEnvironmentDetectionAndSpawnState`（13）：`killGuide`, `killClothier`, `equipmentBasedLuckBonus`, `lastEquipmentBasedLuckBonus`, `hasCreditsSceneMusicBox`, `findTreasure`, `biomeSight`, `invis`, `detectCreature`, `nightVision`, `enemySpawns`, `insideUnbreakableWalls`, `CanSeeInvisibleBlocks`。

行为：混合玩家探测能力、世界/进度资格、luck 输入、展示标志和墙体扫描结果；它不是一个单一 owner 边界。`findTreasure`、`biomeSight`、`invis`、`detectCreature`、`nightVision`、`enemySpawns`、`CanSeeInvisibleBlocks` 是环境/探测候选；`insideUnbreakableWalls` 归 `P08.Environment.WallRescan`；luck 两字段归 luck 输入/兼容投影；`killGuide`、`killClothier` 和 `hasCreditsSceneMusicBox` 交 P11/进度与展示 owner 审核。

### P08.Combat.ArmorEffects

输入组 `PlayerArmorAndCombatEffects`（8）：`thorns`, `turtleArmor`, `turtleThorns`, `cactusThorns`, `spiderArmor`, `anglerSetSpawnReduction`, `vampireBurningInSunlight`, `honeyCombItem`。

行为：向 combat、spawn 或 Item 交互暴露护甲效果。`honeyCombItem` 是外部 Item payload，不能把 Item 对象所有权移动到玩家组件；护甲装备来源与 combat 消费边界需 P09/P06 integration review。

### P08.Combat.ArmorSetsAndTurretCapacity

输入组 `PlayerArmorSetAndTurretState`（19）：`setSolar`, `setVortex`, `setNebula`, `nebulaCD`, `setStardust`, `setForbidden`, `setForbiddenCooldownLocked`, `setChlorophyte`, `setSquireT3`, `setHuntressT3`, `setApprenticeT3`, `setMonkT3`, `setSquireT2`, `setHuntressT2`, `setApprenticeT2`, `setMonkT2`, `maxTurrets`, `maxTurretsOld`, `vortexStealthActive`。

行为：根据装备重建套装/能力状态并传播 stealth 或容量事实。`vortexStealthActive` 有网络 bit 写入/读入。`maxTurrets` 的改变在 `Player.Update(int)` 中条件调用 `UpdateMaxTurrets()`，但 Version4 当前方法体除调用追踪和非本地玩家 early return 外无容量调整行为；容量裁剪、Projectile 修改或通知语义必须记为 unknown。

### P08.Traversal.GravityAndWater

输入组 `PlayerGravityAndWaterTraversalState`（5）：`waterWalk`, `waterWalk2`, `forcedGravity`, `gravControl`, `gravControl2`。

行为：表示水面穿越和重力控制资格/计时。它与物理参数共同投影给 movement/collision，但 P03 Mount、环境和 P07 移动之间的权威优先级未知；不得借用网络中其他 gravity 字段来填补本组范围。

### P08.Luck.RecalculationAndWallRescan

输入组 `PlayerLuckAndRescanState`（16）：`torchLuck`, `happyFunTorchTime`, `ladyBugLuckTimeLeft`, `luck`, `luckMinimumCap`, `luckMaximumCap`, `coinLuck`, `kiteLuckLevel`, `luckNeedsSync`, `disableVoidBag`, `movementAbilitiesCache`, `UnbreakableWallRescanPeriod`, `UnbreakableWallRescanDistance`, `_unbreakableWallScanCooldown`, `_unbreakableWallScanLastPosition`, `_sizzleAudioHandle`。

行为应拆为两个概念：`P08.Luck.Recalculation` 汇总 torch、ladybug、coin、kite、equipment 与 world 输入后提交 luck；`P08.Environment.WallRescan` 负责墙体扫描缓存和变化通知。`disableVoidBag`、`movementAbilitiesCache`、`_sizzleAudioHandle` 的权威范围或生命周期不能按同组假定，均需 integration review/unknown。

## State Ownership and Write Closure

Version4 中可确认的是 `Player` 对象内的直接写入路径，不是 ECS 中已确认的 System owner：

| 状态簇 | 直接源码证据 | 本次可下的结论 |
|---|---|---|
| 派生装备能力 | `ResetEffects` 清理 mobility、detection、combat、gravity-control 和 `maxTurrets` 等字段（`Player.cs:10272-10447`）；`Player.Update(int)` 随后调用 `UpdateEquips` 与 `UpdateArmorSets`（`15296,15348`） | 当前为重置后重建的候选单写协议；装备 hook、Item/ArmorSet effect 和跨域调用闭包 partial，ECS writer proposed |
| 区域与 shimmer | zone 属性访问 `zone1..zone5`；`TrySpawningFaelings` 对 `_wasInShimmerZone` 与 `ZoneShimmer` 做边沿检测并更新 latch（`Player.cs:2931-2939,9937-9945`） | edge 行为 confirmed；zone producer 与本地/网络触发是否共享同一权威阶段 unknown |
| 区域网络 | `NetMessage` 写五个 zone byte（`NetMessage.cs:932-936`）；`MessageBuffer` 读五个 byte、检测 shimmer edge 并调用 spawn（`MessageBuffer.cs:1722-1733`） | 协议片段 confirmed；协议版本、全部发送/接收路径和预测/权威语义 partial |
| 移动/环境能力 | `ResetEffects` 写回许多候选字段；Mount 和 Player 均能引用 `gravity`（CPG member-use 仍有 Unknown/partial） | owner、Mount 优先级、输出何时对 P07/Collision 可见 unknown |
| Luck | `UpdateLuck -> UpdateLuckFactors -> RecalculateLuck`（`Player.cs:17869-17929`）；其他路径也置 `luckNeedsSync`（如 `4365,7025,10137`） | luck 计算局部路径可确认；同步旗标存在其他 writer，network commit owner 未闭合 |
| 墙体扫描 | `DoUnbreakableWallScan` 在 dual-Dungeons 条件下读 player center，按 force/cooldown/distance 扫描并更新 inside flag/cache；变化时 `BroadcastChange`（`Player.cs:17741-17760`） | 此方法的局部写入和广播条件 confirmed；所有调用入口、全局扫描/网络订阅与缓存恢复语义 partial |
| 炮塔容量 | `ResetEffects` 将 `maxTurrets` 重置为 1；update 比较新旧值后调用 `UpdateMaxTurrets` 并保存 old 值（`Player.cs:10447,15353-15356`） | 比较和调用次序 confirmed；被调方法行为不可从名字推断，其 Version4 body 没有容量变更逻辑，故写闭包 unknown |
| 目标 tile | `Player.Update(int)` 从 mouse/screen/gravity 计算 target、clamp 世界边界、初始化 null tile 并修正邻接 tile（`Player.cs:15109-15157`） | 输入转换和局部世界写入 confirmed；实际输入采集、client/server scope、所有目标 consumers partial |

System owner 决策必须区分 Component 数据位置、业务责任、访问权、最终效果提交与调度并发。以上源码没有证明 P08 任一字段在目标 ECS 中已有唯一 writer；不可把 `Player` 的声明类型、Component 名称或完整 shard 查询升级为唯一 owner。

## Boundary Role and Decision

**决定：采用 `partial` 兼容协调阶段，并提出少量有显式协作契约的 `separate` 能力边界；整体结论为 `proposed`。** 不将九个 Component 检查点机械变成九个 update System，也不把所有字段继续塞进一个永久的 Player mega-System。

初始迁移阶段保留一个单一 Player 更新协调点（目标实现类型/调度器尚未在 `src/NSSLC` 找到，名称仅概念化为 `PlayerUpdateSystem`）。它只保证同步阶段与可见性，不拥有所有 P08 不变量。以下职责可由一个 System 的多个方法先保持 `partial`，待读写闭包和行为测试支撑后再分离调度节点：

- 装备派生重建：`ResetEffects -> UpdateEquips -> UpdateArmorSets`，由一个 `PlayerEquipmentEffectsSystem` 候选顺序协调，写入其批准的 armor/combat/set 事实；移动所需投影交给 traversal owner，不允许多个 System 双写。
- `PlayerEnvironmentSystem` 候选维护已解析 zone/detection snapshot；扫描事实来自显式 World/Tile/Scene 输入。`SceneMetrics.Scan` 当前只证实以 `Player.Center` 扫描 biome center/NPC positions（`Player.cs:9926-9935`），不能证明它是 zone byte 的唯一 writer。
- `PlayerTargetingSystem` 候选隔离输入适配、边界 clamp、tile catalog 读取及 target/range 结果。tile 创建/写入应留在已确认的 World/Tile 边界，不藏在纯 Query 内。
- `PlayerTraversalCapabilitySystem` 候选组合 P08 physics、environment mobility、gravity/water 能力，给 P07/Collision 提供只读快照；不拥有位置/速度积分，且 P03 mount/equipment 的优先级需 integration review。
- `PlayerLuckSystem` 与 `PlayerWallRescanSystem` 应是两个独立能力：luck 是给定输入的确定性聚合加单一提交；wall rescan 是有缓存/时机/网络广播的空间扫描。不要把 C09 作为一个通用 System。

`PlayerArmorEffectsSystem` 与套装效果是否可合并在装备效果协调中，或各自成为 separate System，取决于 `ArmorSetBonuses`、Item hook、turret effect 的完整调用闭包；现有 Version4 和 CPG 证据不能定案。`insideUnbreakableWalls` 从 C05 分到 wall scan；C05 的进度/展示字段不自动归环境 owner。以上候选 System 都不等于已注册、已调度或已实现。

## System API and Legacy Behavior Mapping

使用概念 API 组合而不是旧方法的一对一替换。下列新 API 名称和组合身份均为 `proposed`，由真实目标运行时的 API 规范确认后才可实现：

| Legacy entry / boundary | Concept ID | Proposed composition | Owner / result |
|---|---|---|---|
| `Player.Update(int)` 中的 reset、equip 和 armor-set 阶段 | `P08.Equipment.DerivedEffects` | `P08.PlayerFrameDerivedEffects.v1`: Begin/reset transient state -> read P09 loadout/effect inputs -> resolve equipment effects -> apply armor-set facts -> publish read-only capability snapshots | Equipment effect owner writes only its approved P08 effect components；其他 System 只读 |
| `Player.Update(int)` 中的 target 坐标及 tile 修正 | `P08.Interaction.TileTargetAndRange` | `P08.ResolveTileTarget.v1`: adapt input snapshot -> `PlayerTargetingSystem.Resolve` -> explicit tile lookup/write port -> return target/range snapshot | Targeting owner；World/Tile adapter 执行 tile access side effect |
| Zone properties、`TrySpawningFaelings`、packet 入口 | `P08.Environment.ZoneAndTransition` | `P08.ApplyZoneSnapshot.v1`: normalize authoritative zone input -> `PlayerEnvironmentSystem.ApplySnapshot` -> compare shimmer edge -> `ShimmerTransitionAdapter` 请求 NPC spawn/network projection | Environment owner 提交 zone/latch；NPC/Network owners 执行 effect |
| `UpdateLuck`, `UpdateLuckFactors`, `RecalculateLuck` | `P08.Luck.Recalculation` | `P08.RecalculateLuck.v1`: `PlayerLuckInputQuery` -> pure `LuckCalculationQuery` -> `PlayerLuckSystem.Commit` -> replication adapter if sync is required | Luck owner 写 luck/cap/sync state；查询不得更新 sync flag |
| `DoUnbreakableWallScan` | `P08.Environment.WallRescan` | `P08.RescanUnbreakableWalls.v1`: read position/seed/time -> `PlayerWallRescanSystem.Scan` -> commit cache/result -> broadcast only on the old change condition | Wall-scan owner 写 cache/result；Network adapter 发送变化 |
| `UpdateMaxTurrets` | `P08.Combat.TurretCapacity` | Composition 未定义；先核对完整源码/恢复来源及 Projectile 调用契约 | 当前 Version4 函数体为空，不映射成 trim、delete 或 spawn 行为 |
| `NetMessage` / `MessageBuffer` 中 zone 与 vortex bits | `P08.Network.PlayerCapabilityProjection` | `P08.NetworkCapabilities.v1`: decode/encode versioned snapshot -> owner API -> publish outbound snapshot | Network adapter 不直接绕过 owner 写 Component |

API 不要求增加 CQRS、事件总线、队列或每字段 Command。若目标 System API 是同一同步调用内的直接更新，则直接调用 owner API；只有现有调度/结构变化契约证明需要延迟时才提交命令。`Query` 只读取显式快照，`Projection` 不回写权威状态，Adapter 仅做协议/输入转换。版本、错误、拒绝时机、重复调用、partial commit 和重试语义当前均未闭合。

## Call and Dependency DAG

以下区分源码可见顺序与提案边；不是运行时调度闭包，也不假定全部关系构成 DAG。

```text
[source] Player.Update(int)
  -> ResetEffects
  -> UpdateEquips
  -> DoUnbreakableWallScan(force: true)
  -> UpdateLuck
  -> mount.UpdateEffects(this) when active
  -> UpdateArmorSets
  -> if maxTurretsOld != maxTurrets: UpdateMaxTurrets -> maxTurretsOld = maxTurrets

[source] Player.Update(int)
  -> TrySpawningFaelings
       -> if shimmer false->true: NPC.Spawner.SpawnFaelings

[source] MessageBuffer zone packet
  -> assign zone1..zone5
  -> if zone5[0] false->true: NPC.Spawner.SpawnFaelings
  -> NetMessage.TrySendData(36, ...)

[proposed] loadout (P09) + mount (P03) + world/scene/tile facts
  -> PlayerEquipmentEffectsSystem / PlayerEnvironmentSystem
  -> PlayerTraversalCapabilitySystem -> P07 movement / Collision readers
  -> combat effects -> P06 / Item readers
  -> PlayerLuckSystem -> luck snapshot / network projection

[proposed] player position + scan input
  -> PlayerWallRescanSystem -> insideUnbreakableWalls/cache -> Network adapter on change

[proposed] input/screen + gravity snapshot + World/Tile adapter
  -> PlayerTargetingSystem -> tile target/range snapshot -> tile/item interaction readers
```

源码阶段顺序有行级证据，迁移时应先保持 `ResetEffects` 在重建之前、luck 在当帧 equipment facts 之后、armor set 与 turret comparison 的先后，以及双来源 shimmer edge 的外部观察。Mount effects 位于 luck 后、armor sets 前；其对 P08 字段的影响需用目标源码闭包确认。输入到新系统的 edges、写后何时对 P07/Collision/Network 可见、事件注册顺序和并行冲突均 unknown；禁止用文件顺序代替 scheduler 证据。

## Lifecycle and Side Effects

- **Tick/reset：** `Player.Update(int)` 每次进入该阶段调用 `ResetEffects` 后继续装备/能力更新；`Ghost` 路径也调用 `ResetEffects`（`Player.cs:3855`）。因此 reset 不能直接等同于实体创建初始化，也不能删去特殊生命周期调用。完整 create/destroy/rebuild/unload 路径 unknown。
- **Zone/shimmer：** 区域 byte 有网络投影；本地 update 与入站 packet 都检查进入 shimmer 的边沿并可能触发 Faeling spawn。触发 authority、预测/重复语义和断线重连恢复 unknown。`SceneMetrics.Scan` 的扫描作用域和 zone component 失效条件 unknown。
- **Target/tile：** 输入坐标会受屏幕位置和反重力方向影响；边缘 clamp、部分 tile neighbor 修正及 null tile 创建都是既有可观察行为。UI 输入 scope、服务器路径和异常恢复未闭合。
- **Wall scan：** 只有 dual-Dungeons seed 条件继续执行；当前源码按 force、cooldown 或离上次位置距离触发扫描，变化时 `UnbreakableWallScan.NetModule.BroadcastChange(this)`。组件缓存的重建、玩家传送/死亡/world unload 后失效，以及多个调用点间的扫描频次需要实际 owner/scheduler 证据。
- **Luck/equipment：** luck 计算读取设备、buff、world 和 equipment 派生事实；`luckNeedsSync` 被非单一方法设置。计算、状态提交与网络发布必须区分。`_sizzleAudioHandle`、Item payload 生命周期和异步 audio stop/cleanup 未闭合。
- **Combat/turret/network/save：** `vortexStealthActive` 有 packet bit；zone bytes 有 5-byte 片段。P08 字段的完整协议版本和错误路径 unknown。Version4 `Serialize` / `Deserialize` 方法体为空（`Player.cs:26418,26455`），不能从当前版本宣称 P08 save format 或恢复兼容。`UpdateMaxTurrets` 也是空/存根式 body，外部容量 effect unknown。
- **World/session scope：** 多玩家、多 world、客户端预测、服务器权威、重新连接和 unload 生命周期均未由本次静态证据闭合。

## Integration Handoff

`crossSubsystemOwner: integration-review` 必须保留在以下边界，不由 P08 报告定最终权威写者：

- P03 Mount 与 P07 movement/physics/collision 对 gravity、jump、run、swim、slow-fall 等能力的输入优先级、消费者及 commit 时点。
- P09 equipment/loadout/Item effect 输入；P08 的 equipment-derived facts 和 `honeyCombItem` payload ownership。
- P06 对 `thorns`、armor combat facts 的消费，以及反伤/伤害/护甲事件提交。
- `PlayerAbilityComponent`、`PlayerSummonCapacityState`、P05 ability/integration 与 P08 turret fields 的重复状态、唯一 writer 和 Projectile 外部效果。不得仅按 Component 名称或 `UpdateMaxTurrets` 方法名推断裁剪规则。
- P11/World/SceneMetrics 对 zone、biome、spawn qualification、`killGuide`、`killClothier`、`hasCreditsSceneMusicBox` 和 presentation 状态的 owner 与 snapshot invalidation。
- NPC spawner 对 shimmer transition 的 authority/去重，NetMessage/MessageBuffer 的兼容输入输出，保存加载协议，以及 audio/cache owner。
- tile catalog/world tile access、targeting input scope 与 Item 拾取/交互范围消费者。

交接只说明需要整合的协作面，不复制其他分区成员或裁定它们的最终 owner。

## Migration Behavior Contract

未来真实迁移应为每个 Concept ID 建立旧入口到新组合映射，并比较完整观察向量：输入和默认值、最终状态 delta、不变量、事件次数/payload/顺序、网络字节/版本、错误或拒绝时机、生命周期作用域、重复调用/重试语义及副作用。

最低行为约束候选：

- Equipment rebuild：一次 `ResetEffects` 与装备/套装更新后，比较所有 P08 派生字段默认值、保留态和重新赋值；保持 `UpdateEquips`、Mount effects、luck、armor-set 和 turret comparison 的旧可见顺序，直到测试证明可放宽。
- Zone/shimmer：对本地更新和网络输入分别检查五个 zone bytes、false->true/true->false/no-change 转换、Faeling spawn 次数及后续消息；不把两个当前路径折叠为一个事件而不确认权威规则。
- Target/range：覆盖屏幕坐标、反重力、世界边界、neighbor 特殊修正、null tile 处理、范围更新和拾取速度结果；同时记录 World/Tile 写入。
- Traversal：覆盖默认与 equipment/mount/world 重叠、reset 后重建、movement/collision 可见时点及不得由 P08 写 position/velocity。
- Luck：在显式 torch/ladybug/coin/kite/equipment/world 输入上核对数值、caps、potion/计时状态、luck sync 边沿；确认失败和网络提交语义。
- Wall scan：覆盖 seed gate、force/cooldown/距离边界、扫描结果不变/改变时状态、cache 更新和广播次数，以及传送/死亡/reset 恢复。
- Turret：实现前必须先取得 `UpdateMaxTurrets` 的行为来源和 Projectile owner 契约；否则该 Concept 保持 unknown，不构造行为等价基线。

回滚/补偿目前没有可核实协议。静态报告、索引查询、旧字段仍可访问或局部规则通过均不构成行为等价或 migration success 证据。

## Evidence Gaps and Blocking Decisions

- `Player.Update(int)` 的 CPG callable body 不足；CPG 没有源快照绑定，源码片段不可用。原始 Version4 源码补足已观察到的直接调用，但不能补上动态、被清空实现或外部回调闭包。
- CPG `complete` 仅表示索引查询在其选定 shard/结果预算内完成；field `AccessMode=Unknown`、`partial` facts、零命中和已观察到的 callsite 差异均不证明无写者/无调用者。
- Zone byte 的生产者、scene/world tile 扫描与 P08 zone owner 的关系 unknown；网络 authority、protocol evolution、预测、spawn 去重和全生命周期状态 unknown。
- Equipment hooks、ArmorSet bonus 闭包、Item/Projectile/NPC 调用关系和线程/phase 调度 unknown；静态 `Player.Update` 顺序不是独立 System 调度证明。
- P08 turret 状态在 NLTX 存在重复 Component 字段；Version4 `UpdateMaxTurrets` 空 body 使其关键 effect unknown，是任何 turret capacity migration 的 blocking decision。
- `Serialize`/`Deserialize` 空方法、缓存清除、world/session/multi-world 范围和 audio cleanup 证据缺失；不能伪造持久化、重连或 teardown 语义。
- 当前 `src/NSSLC` 没找到 P08 新 System/组件接线；实现后的真实入口命中情况和行为测试皆未验证。

上述 gap 不阻止交付本 proposed 静态边界报告；依赖某个 gap 的最终 owner/API/行为等价结论须保持 unknown，等 integration review 与真实迁移证据补足。

## Verification Plan

本次是 System 设计报告会话：`verificationStatus: not-run`，没有修改 `src/`、`Test/`、项目或输入文件，没有运行 build、test、behavior verifier 或代码执行验证；不称迁移成功。

报告级静态验收只检查：claim 身份/路径匹配、9 组和 100 字段覆盖、标题顺序、状态标签、P08 范围未越界、Markdown whitespace/diff 检查、源码保持未修改。未来实现阶段再依上节行为约束运行目标项目必需行为测试，测试必须命中新 owner 与新 API composition；只有这些测试和 integration owner 决议满足后才可评估 `verified` / `migration-success`。
