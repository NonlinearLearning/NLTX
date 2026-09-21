# Version4 P08 玩家环境、护甲与移动相关状态组件执行计划

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

## 1. 执行范围与不可变状态

本文档描述 P08 的组件拆分实施顺序、路径、依赖、迁移兼容和验证计划。System、Query、Command、Adapter 和 Projection 仍不在本任务范围内；C01 组件源码已保存，其余组件按 checkpoint 顺序实施。本执行记录必须与设计文档的 checkpoint metadata 同步保存；完成一个检查点前不得开始下一个。

执行前必须确认：

- P08 当前会话使用 runner 返回的 `partitionId: P08` 和 `sessionId: 243009d31f4e42d0bfc70eebd9d1fcaf`；不 Claim、伪造 session、手动编辑 ledger 或删除锁文件。
- 只从仓库根目录通过 `Build/Tools/Invoke-SerialDotnet.ps1` 执行 compile-capable 命令；本次 documentation-only 会话不运行 `dotnet`。
- 不执行用户明确禁止的 `git diff --check -- <两份 P08 文档>`；使用路径、metadata、来源序号、重复成员和 Markdown 结构检查。
- 现有 `PlayerAbilityComponent`、`PlayerEquipmentComponent`、`PlayerMountComponent`、`PhysicsStateComponent` 和 `MovementStateComponent` 是现状证据，不能绕过 owner reconciliation。

## 2. planned 组件顺序

| 顺序 | 检查点 | proposed 目标 | 依赖 | 状态 |
|---:|---|---|---|---|
| 1 | C01 | `PlayerZoneAndEnvironmentStateComponent`, `PlayerZoneCommitSystem`, `PlayerZoneQuery`, `ShimmerTransitionAdapter` | world/tile snapshot；Network adapter seam | design-checkpoint-complete |
| 2 | C02 | `PlayerTileTargetingAndRangeStateComponent`, `TileTargetQuery`, `TileRangeCommitSystem`, `ItemPickupRangeAdapter` | input/world coordinates；C01 zone facts；P09 item seam | design-checkpoint-complete |
| 3 | C03 | `PlayerMovementPhysicsStateComponent`, `MovementParameterQuery`, `MovementParameterCommitSystem` | P07 movement；P03 mount；C04 mobility；static defaults review | design-checkpoint-complete |
| 4 | C04 | `PlayerEnvironmentMobilityStateComponent`, `MobilityCapabilitySystem`, `MobilityQuery` | ResetEffects；P07/P09/P14; C03 movement | design-checkpoint-complete |
| 5 | C05 | `PlayerEnvironmentDetectionAndSpawnStateComponent`, `EnvironmentDetectionSystem`, `SpawnQualificationQuery`, `LuckInputProjection` | tile/world scan；P14 spawn；P17 biome；C09 luck | design-checkpoint-complete |
| 6 | C06 | `PlayerArmorAndCombatEffectsComponent`, `ArmorEffectCommitSystem`, `CombatEffectQuery`, `ItemEffectAdapter` | P06 combat；P09 equipment；C05 spawn | design-checkpoint-complete |
| 7 | C07 | `PlayerArmorSetAndTurretStateComponent`, `ArmorSetCommitSystem`, `TurretCapacityQuery` | P06/P05 ability；existing PlayerAbility owner；P09 equipment | design-checkpoint-complete |
| 8 | C08 | `PlayerGravityAndWaterTraversalStateComponent`, `TraversalCapabilitySystem`, `GravityQuery` | P07 physics/input；C03 movement；collision | design-checkpoint-complete |
| 9 | C09 | `PlayerLuckAndRescanStateComponent`, `LuckCalculationQuery`, `LuckCommitSystem`, `WallRescanSystem`, `AudioAdapter` | C05 luck inputs；P09 coin/equipment；NPC ladybug; P07 cache | design-checkpoint-complete |

顺序是显式依赖约束，不是文件或目录顺序。若 integration-review 改变 owner，必须更新两个文档的 metadata、依赖影响和回滚点后才可执行。

## 3. 目标路径、命名空间与迁移影响

下列路径只代表 `status: proposed` 的目标计划，当前不存在这些新类型。实现时按 ECS 文件组织约束，以能力域优先；不创建 `Shared/Components` catch-all。

| 检查点 | proposed 目标路径/命名空间 | 源字段迁移影响 | 关键依赖 |
|---|---|---|---|
| C01 | `src/Player/Environment/PlayerZoneAndEnvironmentStateComponent.cs`; `src/Player/Environment/PlayerZoneCommitSystem.cs`; `src/Player/Environment/PlayerZoneQuery.cs`; namespace `Terraria.Player.Environment` | zone wrappers、shimmer transition、environment immunity 的唯一 writer；NetMessage/MessageBuffer 只接 adapter | `Player.cs`, world/tile snapshot, P17/P11/P14 |
| C02 | `src/Player/Interaction/PlayerTileTargetingAndRangeStateComponent.cs`; `src/Player/Interaction/TileTargetQuery.cs`; `src/Player/Interaction/TileRangeCommitSystem.cs`; namespace `Terraria.Player.Interaction` | per-player last range/adjTile 迁移；static target/range 保留 compatibility adapter | `Player.cs`, `Projectile.cs`, P09 Item pickup, P10 input |
| C03 | `src/Player/Movement/PlayerMovementPhysicsStateComponent.cs`; `src/Player/Movement/MovementParameterQuery.cs`; `src/Player/Movement/MovementParameterCommitSystem.cs`; namespace `Terraria.Player.Movement` | gravity/run/fall fields 迁移；static defaults 不直接复制 | `PhysicsStateComponent`, `MovementStateComponent`, P03/P07 |
| C04 | `src/Player/Environment/PlayerEnvironmentMobilityStateComponent.cs`; `src/Player/Environment/MobilityCapabilitySystem.cs`; `src/Player/Environment/MobilityQuery.cs`; namespace `Terraria.Player.Environment` | ResetEffects/equipment-derived flags 和 swim timer 迁移 | P07, P09, P14, Collision, Mount |
| C05 | `src/Player/Environment/PlayerEnvironmentDetectionAndSpawnStateComponent.cs`; `src/Player/Environment/EnvironmentDetectionSystem.cs`; `src/Player/Environment/SpawnQualificationQuery.cs`; namespace `Terraria.Player.Environment` | detection/progression/luck input projection 分离；presentation flag 不做 P08 authority | P14, P17, P11, C09 |
| C06 | `src/Player/Combat/PlayerArmorAndCombatEffectsComponent.cs`; `src/Player/Combat/ArmorEffectCommitSystem.cs`; `src/Player/Combat/CombatEffectQuery.cs`; `src/Player/Combat/ItemEffectAdapter.cs`; namespace `Terraria.Player.Combat` | thorns/armor effects 单写；Item 不搬进 component | P06, P09, Item domain |
| C07 | `src/Player/Combat/PlayerArmorSetAndTurretStateComponent.cs`; `src/Player/Combat/ArmorSetCommitSystem.cs`; `src/Player/Combat/TurretCapacityQuery.cs`; namespace `Terraria.Player.Combat` | set/turret/vortex flags 与已有 ability component reconciliation | `src/Player/PlayerAbilityComponent.cs`, P05/P06/P09 |
| C08 | `src/Player/Movement/PlayerGravityAndWaterTraversalStateComponent.cs`; `src/Player/Movement/TraversalCapabilitySystem.cs`; `src/Player/Movement/GravityQuery.cs`; namespace `Terraria.Player.Movement` | water walk/gravity control 迁移；不得重复 Physics authority | P03, P07, Collision |
| C09 | `src/Player/Luck/PlayerLuckAndRescanStateComponent.cs`; `src/Player/Luck/LuckCalculationQuery.cs`; `src/Player/Luck/LuckCommitSystem.cs`; `src/Player/Luck/WallRescanSystem.cs`; `src/Player/Audio/PlayerAudioAdapter.cs`; namespaces `Terraria.Player.Luck`/`Terraria.Player.Audio` | luck/rescan/cache/adapter 拆开；audio handle 不进 persistent component | P09, NPC, P07, world/tile/audio |

每次移动/拆分都要记录 source/target/dependency impact，并保持 one core public type per same-named file。文件迁移不得改变 namespace、public API 或运行时语义；目录排序不得被用作调度机制。

## 4. 全局实施不变量

- 先建立唯一 writer 和 focused verifier，再迁移 readers；legacy Player 与新 component 不得双写。
- `EntityReference`、Item 句柄、NetworkId、PersistentId、slot index、`SlotId` 和 cache key 保持不同语义；不把数组位置当持久化 ID。
- Component 只保存持续状态；Command 保存一次性意图；Query 纯读；Adapter 处理文件、网络、日志、音频和外部 Item；Projection 输出不可变快照。
- Reset/重建/提交/消费的顺序显式化；不会用字段或文件顺序暗示调度。
- 所有静态 mutable 字段先通过 integration-review；若不能证明并发隔离和 owner，不迁移为实体组件。
- P08 不拥有 P07、P09、P11、P14、P15、P17、P03 的最终跨域状态；文档将其标为 integration-review，不在本会话伪造结论。

## 5. 显式调度执行顺序

1. `PlayerSpawnOrHydrationSystem` 建立 entity 和现有 Player 基础关系。
2. `ResetDerivedPlayerEffects` 清理 C04/C05/C06/C07/C08 的装备派生 flags；保持原 `ResetEffects` 语义。
3. `PlayerZoneCommitSystem` 读取环境快照并提交 C01；发布 shimmer transition event（如有边沿）。
4. `TileRangeCommitSystem` 提交 C02 per-player range/adjacency；static input adapter 在同一 tick 内只服务当前输入上下文。
5. `MovementParameterCommitSystem` 读取 equipment/mount/buff/mobility facts，提交 C03。
6. `MobilityCapabilitySystem` 提交 C04；`EnvironmentDetectionSystem` 提交 C05；spawn 查询只读 committed facts。
7. `ArmorEffectCommitSystem` 和 `ArmorSetCommitSystem` 提交 C06/C07；turret owner 先经过 PlayerAbility reconciliation。
8. `TraversalCapabilitySystem` 提交 C08；P07 movement/Collision/ Mount 只读 `GravityQuery`/`MobilityQuery`。
9. `WallRescanSystem` 使用显式 time/position 输入更新 C09 rescan cache；`LuckCalculationQuery` 纯计算，`LuckCommitSystem` 写 luck/luckNeedsSync。
10. Network/persistence/renderer/UI/audio/logger adapters 消费 snapshots 和 events；任何失败都返回 command result，不隐式改写 authority。

## 6. 各检查点执行步骤与验收

### C01 PlayerZoneAndEnvironmentState

状态：design-checkpoint-complete；实现和验证均未开始。

执行步骤：

1. 为 zone1..zone5 的 default zero、zone property facade、shimmer enter/leave edge 和 environmentBuffImmunityTimer 建立状态转移 verifier。
2. 建立 `PlayerZoneCommitSystem` 的 world/tile/environment snapshot 输入和唯一提交端口；旧 Player 字段仅作为兼容 view。
3. 将 `NetMessage`/`MessageBuffer` zone packet 接到 proposed replication adapter；没有证据的 P08 字段不加入 packet。
4. 将 shimmer audio/transition side effect 交给 adapter/event consumer，禁止 Zone Query 播放/停止音频。

验收：zone bits round-trip、shimmer transition 不重复、timer reset/decrement 顺序可重复、没有 Player/new component 双写。持久化仍 blocked，直到 Version4 格式证据闭合。

### C02 PlayerTileTargetingAndRangeState

执行步骤：

1. 先验证 static target/range 在多玩家/多输入上下文不互相污染；若无法隔离，保留外部 input adapter，不创建 per-player authority。
2. 建立 per-player `lastTileRangeX/Y` 和 `adjTile` 的初始化、清空、equipment invalidation verifier。
3. 将 tile target 计算拆为 `TileTargetQuery` 纯结果和 `TileRangeCommitSystem` 单一写者；`Projectile` 只消费明确 snapshot。
4. 将 item pickup speed/range 的 time、distance、Item transfer effect 分离；Item 迁移由 P09 integration-review 决定。

验收：tile target 计算确定性、adjTile 容量稳定、范围变化只提交一次、pickup 不重复扣减；static field 仍是 compatibility seam 时不得声称组件迁移完成。

### C03 PlayerMovementPhysicsState

执行步骤：

1. 为 gravity、fall/run caps、acceleration、slowdown 的 default/reset/equipment/mount 变换建立 verifier。
2. 明确 `defaultGravity`、`jumpHeight`、`jumpSpeed` 的 config/legacy owner；未确认前只用 `MovementDefinitionSnapshot` proposed adapter。
3. `MovementParameterCommitSystem` 在 C04 mobility 和 P03/P07 facts 之后运行，P07 movement 只读 Query。
4. 与现有 `src/Physics/PhysicsStateComponent.cs`、`src/SpatialSimulation/MovementStateComponent.cs` 做 duplicate writer 检查。

验收：同一输入产生同一 movement snapshot，Reset 顺序与 Version4 兼容，mount/equipment 不造成双写，movement/physics 只读已提交参数。

### C04 PlayerEnvironmentMobilityState

执行步骤：

1. 依照 ResetEffects 清空并按装备/Buff/环境事实重建 canFloat、tube、frog jump、sky stone、spawnMax、blockRange、jumpBoost、noFall、lava/gills/slowFall。
2. 将 swimTime 视为 transient movement state，用显式 tick/time 输入，不让 Query 直接读取时钟。
3. 对 blockRange 与 C02 tile range 做边界测试；只能有一个最终 interaction-range projection。
4. 与 Mount/P07/P09/P14/Collision 确认 canFloatInWater、slowFall、noFallDmg、spawnMax 的 owner 和读写方向。

验收：水/岩浆/漂浮 flags 重建可重复、Collision 只读、spawn/query 不修改 mobility component、旧字段不再双写。

### C05 PlayerEnvironmentDetectionAndSpawnState

执行步骤：

1. 将 killGuide/killClothier 与 environment detection 分成 progression input 和 scan facts；不以同一 Query 隐式混合。
2. 建立 findTreasure、biomeSight、detectCreature、nightVision、insideUnbreakableWalls、CanSeeInvisibleBlocks 的纯资格 Query 和提交系统。
3. 与 P14/P17/P11 做 enemySpawns、biomeSight、hasCreditsSceneMusicBox、invis 的 owner review；presentation projection 不成为 P08 authority。
4. equipmentBasedLuckBonus/lastEquipmentBasedLuckBonus 作为 C09 输入/差值快照交接，不能由 C05 直接计算 luck。

验收：墙体扫描、生成资格、progression flags 和 presentation facts 的生命周期互不覆盖，环境 Query 不发生 I/O/日志副作用。

### C06 PlayerArmorAndCombatEffects

执行步骤：

1. 先验证 thorns/turtle/cactus/spider/angler/vampire flags 在 ResetEffects 后的清理和装备重建顺序。
2. `ArmorEffectCommitSystem` 只提交 combat effect facts；P06 Combat resolution 只通过 `CombatEffectQuery` 消费。
3. `honeyCombItem` 不复制 Item 对象；由 `ItemEffectAdapter` 提供短生命周期 effect payload，并定义空/失效 Item 行为。
4. anglerSetSpawnReduction 与 P14 的 spawn query 做 integration-review，避免 P08 和 P14 双写。

验收：combat effect 只存在一个 owner，Item payload 不泄漏持久化/实体 authority，thorns 和 sunlight 状态的时间/世界输入可测试。

### C07 PlayerArmorSetAndTurretState

执行步骤：

1. 验证所有 set flags、nebulaCD 和 forbidden cooldown lock 的 reset/rebuild/expire 行为。
2. 先比较现有 `PlayerAbilityComponent.MaximumTurrets`/`PreviousMaximumTurrets` 与 maxTurrets/maxTurretsOld，选定唯一 authority；未完成前不新增重复字段。
3. 将 vortexStealthActive 作为跨 P06/P11 的 derived projection，禁止 C07 单独创建 stealth writer。
4. 通过 `TurretCapacityQuery` 向 projectile/minion/ability consumers 提供只读结果，网络只消费 snapshot。

验收：套装效果重算、turret capacity diff、nebula cooldown 和 vortex projection 有 focused verifier，旧/新能力组件无双写。

### C08 PlayerGravityAndWaterTraversalState

执行步骤：

1. 建立 waterWalk/waterWalk2 的水面碰撞资格 verifier。
2. 建立 forcedGravity/gravControl/gravControl2 的输入、装备和 world gravity 变换 verifier。
3. `TraversalCapabilitySystem` 只提交 traversal facts；P03 Mount、P07 Movement 和 Collision 通过 `GravityQuery` 读取。
4. 检查 C03 gravity parameter 与 C08 direction/control 依赖，防止同一 tick 中读到半更新状态。

验收：重力方向切换无旧值泄漏，water walk 只影响预期 Collision 路径，Physics/Movement 无重复 writer。

### C09 PlayerLuckAndRescanState

执行步骤：

1. 将 torch/ladybug/equipment/coin/kite/world 输入建成不可变 `LuckInputSnapshot`，用纯 `LuckCalculationQuery` 计算并应用 min/max caps。
2. `LuckCommitSystem` 单一写入 luck 和 luckNeedsSync；NPC ladybug 写入通过 command/event 进入，不直接跨组件写。
3. `WallRescanSystem` 按 period/distance/cooldown/lastPosition 更新 scan result，使用注入的 time/position，不在 Query 隐式改状态。
4. disableVoidBag 和 movementAbilitiesCache 分别与 P09/P07 owner review；`_sizzleAudioHandle` 交 `PlayerAudioAdapter`，定义释放和重复播放行为。

验收：luck 确定性、sync flag edge、NPC 输入、wall rescan cooldown/distance、cache lifetime 和 audio handle release 全部可验证；未闭合外部 owner 时保持 integration-review。

## 7. 计划验证命令

本会话当前没有执行 compile-capable command。实现阶段从仓库根目录开始，先检查活动 `dotnet.exe`/`csc.exe`，然后一次只运行受影响项目，并通过仓库 wrapper：

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Player\Terraria.Player.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 test .\Test\<affected-player-verifier>.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

上述命令是未来计划，不是本会话结果；不得把 planned 命令报告成已运行。每次实际 build/test 都要记录项目、exit code、warning/error count 和 `Build/bin/` artifact path，并使用 `--no-build --no-restore` 运行后续 verifier。文档检查使用 PowerShell/rg 自定义脚本，不运行被禁止的 `git diff --check --`。

建议的 documentation-only checks：

```powershell
$design = Get-Content -Raw '.\docs\component-decomposition\review-round-2\2026-09-11-version4-P08-player-environment-armor-component-design.md'
$execution = Get-Content -Raw '.\docs\component-decomposition\review-round-2\2026-09-11-version4-P08-player-environment-armor-component-execution.md'
if ($design -notmatch 'partitionId: P08' -or $execution -notmatch 'sessionId: 243009d31f4e42d0bfc70eebd9d1fcaf') { throw 'P08 metadata mismatch' }
if (($design | Select-String -Pattern '^\| (662|665|666|667|668|669|670|1136|1401) \|' -AllMatches).Matches.Count -lt 3) { throw 'mapping probe failed' }
```

该示例只说明检查方向；最终校验必须解析所有 100 个来源序号、两份 metadata 同步、P08 成员集合、状态值和禁止的其他文件修改。

## 8. 回滚与完成条件

每个实现检查点的回滚点是：停止新 writer、恢复 legacy compatibility view、清除该检查点新增的 adapter registration、保留 verifier 以证明旧路径仍工作。不得通过删除整个 `src` 目录或 broad cleanup 回滚。

P08 设计任务完成条件：

- 两份文档存在且 metadata 已同步；当前 `executionStatus: in-progress`、`implementationStatus: in-progress`、`verificationStatus: not-run`。
- 当前真实实现 metadata 为 `completedComponents: [C01, C02]`、`currentComponent: C03`、`pendingComponents: [C03, C04, C05, C06, C07, C08, C09]`；100 个权威来源序号仍在设计 mapping 中各出现一次，成员没有跨 P08 误归属。
- System、Query、Command、Adapter、Projection、测试、项目文件、权威报告和 ledger 未修改；没有声称组件行为、网络或持久化已验证。
- 所有 static/global、cross-partition、persistence、network、audio、Item 和 scheduler gaps 均被记录，未用文件修改时间猜测归属；不执行 `git diff --check --`。

完成文档和检查后，才使用原始 `P08` 与 runner 返回的 `243009d31f4e42d0bfc70eebd9d1fcaf` 调用 runner `Complete`。若证据导致无法安全继续，调用 `Fail`；不生成新 sessionId，不重复 Claim。

## 9. C01 检查点记录

`C01 PlayerZoneAndEnvironmentState` 已完成最小组件源码实现并保存：`src/Player/Environment/PlayerZoneAndEnvironmentStateComponent.cs`。实际字段为 `EnvironmentBuffImmunityTimer`、`Zone1`-`Zone5` 和 `WasInShimmerZone`；zone 字段使用 `byte`，因为当前 NLTX 没有 `Terraria.BitsByte` 类型。没有添加 Zone System、Query、Adapter、网络/持久化代码或测试。C01 状态为 `partial/implemented`；未验证唯一 writer、zone round-trip、shimmer edge、网络端别或持久化。

保存本 checkpoint 后执行游标推进为 `currentComponent: C02`，`pendingComponents: [C02, C03, C04, C05, C06, C07, C08, C09]`；`completedComponents` 只记录实际已保存的 C01 组件实现，不把设计检查点误报为源码完成。

C01-C08 当时的下一检查点已依次保存，当前已完成全部 9 个设计检查点。

## 11. C03 检查点记录

`C03 PlayerMovementPhysicsState` 的执行计划已保存。已保存内容：实例 movement state 的目标路径和唯一 writer、static default/jump tuning 的 definition adapter 前置条件、与 P03/P07/Physics/Movement 现有组件的依赖影响、显式 scheduler 顺序、focused verifier 和 rollback 条件。未开始生产实现、编译、测试或 static tuning 的隔离改造。

C03 的执行前置条件是先确认 `MovementDefinitionSnapshot` 的来源、版本和端别，并在 `MovementParameterCommitSystem` 运行前提交 Mount/Buff/Equipment facts。下一检查点为 C04；C04 完成前不得把 C05 标记为 current 或开始其实现计划执行。

## 12. C04 检查点记录

`C04 PlayerEnvironmentMobilityState` 已完成最小组件源码实现并保存：`src/Player/Environment/PlayerEnvironmentMobilityStateComponent.cs`。实际字段为 `CanFloatInWater`、`HasFloatingTube`、`FrogLegJumpBoost`、`SkyStoneEffects`、`SpawnMax`、`BlockRange`、`JumpBoost`、`NoFallDamage`、`SwimTime`、`LavaImmune`、`Gills` 和 `SlowFall`，默认值为 `false`/`0`。没有添加 ResetEffects、装备/Buff 重建、时钟、Collision、Mount、spawn 或 P09 资源代码。C04 状态为 `partial/implemented`；未验证唯一 mobility writer、`swimTime` 时序、`blockRange` projection、网络或持久化。

C04 的执行前置条件仍是先确定 `canFloatInWater` 的 Mount/P08 writer、`noFallDmg` 的 Combat/Movement boundary、`spawnMax` 的 spawn owner 和 `blockRange` 的唯一 range projection。保存本 checkpoint 后执行游标推进为 `currentComponent: C05`，`pendingComponents: [C05, C06, C07, C08, C09]`。

## 13. C05 检查点记录

`C05 PlayerEnvironmentDetectionAndSpawnState` 已完成最小组件源码实现并保存：`src/Player/Environment/PlayerEnvironmentDetectionAndSpawnStateComponent.cs`。实际字段覆盖 progression 输入、luck bonus 快照、环境探测资格、spawn 资格、墙体扫描结果和可见性能力；`invis` 在组件中命名为 `IsInvisible`。没有添加 spawn query、luck calculation、wall scan、renderer/audio 或跨分区 adapter。C05 状态为 `partial/implemented`；未验证 P14/P17/P11 owner、wall scan lifecycle、网络或持久化。

C05 的执行前置条件仍是先确定 `enemySpawns`/`spawnMax`/`anglerSetSpawnReduction` 的单一 spawn query owner，确定 `biomeSight` 的 P17 输入协议，并把 `hasCreditsSceneMusicBox`/`invis` 保留为 P11 projection seam。保存本 checkpoint 后执行游标推进为 `currentComponent: C06`，`pendingComponents: [C06, C07, C08, C09]`。

## 14. C06 检查点记录

`C06 PlayerArmorAndCombatEffects` 已完成最小组件源码实现并保存：`src/Player/Combat/PlayerArmorAndCombatEffectsComponent.cs`。实际字段为 `Thorns`、`TurtleArmor`、`TurtleThorns`、`CactusThorns`、`SpiderArmor`、`AnglerSetSpawnReduction` 和 `VampireBurningInSunlight`；`honeyCombItem` 未进入组件。没有添加 ArmorEffectCommitSystem、CombatEffectQuery、ItemEffectAdapter、Item 对象或测试。C06 状态为 `partial/implemented`；未验证唯一 combat writer、P14 handoff、Item payload 生命周期、网络或持久化。

C06 的执行前置条件仍是先选定 thorns/armor effect 的唯一 writer，并为 `honeyCombItem` 定义空 Item、过期 Item、重复消费和 adapter failure 行为；P08 不直接写 Combat cooldown 或完整 Item。保存本 checkpoint 后执行游标推进为 `currentComponent: C07`，`pendingComponents: [C07, C08, C09]`。

## 15. C07 检查点记录

`C07 PlayerArmorSetAndTurretState` 已完成最小组件源码实现并保存：`src/Player/Combat/PlayerArmorSetAndTurretStateComponent.cs`。实际字段为 17 个套装/冷却/隐身状态；`maxTurrets`/`maxTurretsOld` 未复制，继续由现有 `PlayerAbilityComponent` 做 owner reconciliation。没有添加 ArmorSetCommitSystem、TurretCapacityQuery、stealth projection、网络或测试。C07 状态为 `partial/implemented`；未验证 turret 唯一 owner、套装重建、vortex handoff、网络或持久化。

C07 的执行前置条件仍是先选定 `MaximumTurrets`/`maxTurrets` 的单一 authority，并验证 `maxTurretsOld` 只承担 diff/compatibility 角色；之后才能迁移 turret/minion readers。vortex stealth 必须通过跨分区 projection handoff，不由 C07 单独发布第二个 stealth state。保存本 checkpoint 后执行游标推进为 `currentComponent: C08`，`pendingComponents: [C08, C09]`。

## 16. C08 检查点记录

`C08 PlayerGravityAndWaterTraversalState` 已完成最小组件源码实现并保存：`src/Player/Movement/PlayerGravityAndWaterTraversalStateComponent.cs`。实际字段为 `WaterWalk`、`WaterWalk2`、`ForcedGravity`、`GravControl` 和 `GravControl2`，默认值为 `false`/`0`。组件只保存 traversal/gravity facts，不写 velocity、position 或 movement behavior。C03 负责 gravity parameters，C08 负责 gravity direction/control facts；P03 Mount、P07 Movement/Physics 和 Collision 仍通过后续 Query/commit 边界消费。组件状态为 `partial/implemented`，未运行编译或测试。

C08 的未闭合前置条件仍是确定 world gravity、input、equipment 与 forcedGravity 的优先级，并保证未来 `TraversalCapabilitySystem` 提交完整快照后 P07/P03/Collision 才消费；不允许 C08 直接写 velocity/position。保存本 checkpoint 后执行游标推进为 `currentComponent: C09`，`pendingComponents: [C09]`。

## 17. C09 检查点记录

`C09 PlayerLuckAndRescanState` 已完成最小组件源码实现并保存：`src/Player/Luck/PlayerLuckAndRescanStateComponent.cs`。实际字段为 `TorchLuck`、`HappyFunTorchTime`、`LadyBugLuckTimeLeft`、`Luck`、`CoinLuck`、`KiteLuckLevel`、`LuckNeedsSync`、`UnbreakableWallScanCooldown` 和 `UnbreakableWallScanLastPosition`，默认值为 `0`/`false`/`Vector2.Zero`。未复制 `LuckMinimumCap`/`LuckMaximumCap`（luck rules/config seam）、`DisableVoidBag`（P09 capability）、`MovementAbilitiesCache`（P07/P03 cache owner）、静态重扫常量和 `_sizzleAudioHandle`（AudioAdapter 句柄）。组件状态为 `partial/implemented`；未添加 LuckCalculationQuery、LuckCommitSystem、WallRescanSystem、NPC command/event、AudioAdapter 或测试。

C09 的后续 integration-review 仍须确定 luck/network owner、NPC 输入命令、wall scan 的时间来源、movement cache 生命周期、void-bag capability owner 和 audio handle 的重复/失败释放语义；同时确认 luck cap 的配置来源和唯一 writer。C09 只保存事实，不实现计算、提交、扫描、音频或跨分区行为。保存本 checkpoint 后执行游标推进为 `currentComponent: none`，`pendingComponents: []`；整体 `implementationStatus` 仍为 `in-progress`，`verificationStatus` 仍为 `not-run`。

## 18. 当前实现交付检查

- 9 个设计检查点仍完整保存在两份文档中；当前真实实现 metadata 为 `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08]`、`currentComponent: C09`、`pendingComponents: [C09]`。
- C01-C09 的实际源码包括 `src/Player/Environment/PlayerZoneAndEnvironmentStateComponent.cs`、`src/Player/Interaction/PlayerTileTargetingAndRangeStateComponent.cs`、`src/Player/Movement/PlayerMovementPhysicsStateComponent.cs`、`src/Player/Environment/PlayerEnvironmentMobilityStateComponent.cs`、`src/Player/Environment/PlayerEnvironmentDetectionAndSpawnStateComponent.cs`、`src/Player/Combat/PlayerArmorAndCombatEffectsComponent.cs`、`src/Player/Combat/PlayerArmorSetAndTurretStateComponent.cs`、`src/Player/Movement/PlayerGravityAndWaterTraversalStateComponent.cs` 和 `src/Player/Luck/PlayerLuckAndRescanStateComponent.cs`；System、Query、Command、Adapter、Projection、测试和项目文件未修改。
- 设计 mapping 仍覆盖 100 个 P08 来源序号且无重复；设计状态为 `proposed`，执行状态为 `in-progress`，实现状态为 `in-progress`，验证状态为 `not-run`。
- 本会话已执行 compile-capable 命令；已修改 C08/C09 组件源码及两份 P08 Markdown，未修改测试、项目文件、权威报告或 ledger，也未执行用户禁止的 `git diff --check --`。实际命令为 `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src\\Player\\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`；exit code 1，0 warnings，5 errors，错误来自任务范围外的既有 `src/Player/Progression` 缺失类型引用。预期输出路径 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 存在但未由本次失败构建验证，视为 stale/unverified artifact。

## 19. 交付后的实现顺序

后续实现会话必须按 C01 -> C09 的顺序重新核对 owner 和 verifier。每个 checkpoint 实现后更新两份文档并记录 source/target/dependency impact、实际命令、exit code、warning/error count 和 `Build/bin/` artifact；跨分区 integration-review 未完成时保持 blocked/planned，不把 proposed 类型直接落地。

## 10. C02 检查点记录

`C02 PlayerTileTargetingAndRangeState` 已完成最小组件源码实现并保存：`src/Player/Interaction/PlayerTileTargetingAndRangeStateComponent.cs`。实际字段为 `LastTileRangeX`、`LastTileRangeY` 和 `AdjacentTiles`；static target/range/tuning、Item pickup adapter、Projectile snapshot、tile target calculation 和 range invalidation 均未实现。由于当前 NLTX 没有 `TileID.Count` 或等价 tile catalog 容量，`AdjacentTiles` 默认为空数组，后续 tile definition/commit owner 必须在初始化阶段提供容量。C02 状态为 `partial/implemented`；未验证 static workspace 隔离、adjacency capacity、range invalidation、网络或持久化。

C02 的执行前置条件仍是证明多玩家输入上下文不会共享写入 `tileTargetX/Y`；证明不足时保持 compatibility adapter。保存本 checkpoint 后执行游标推进为 `currentComponent: C03`，`pendingComponents: [C03, C04, C05, C06, C07, C08, C09]`。

## C03 实现检查点

`C03 PlayerMovementPhysicsState` 已保存组件源码 `src/Player/Movement/PlayerMovementPhysicsStateComponent.cs`。实际字段为 `Gravity`、`MaxFallSpeed`、`MaxRunSpeed`、`RunAcceleration` 和 `RunSlowdown`，默认值分别为 `0.4f`、`10f`、`3f`、`0.08f` 和 `0.2f`，与权威报告中的 Version4 实例初始化一致；三个 static tuning 字段 `defaultGravity`、`jumpHeight` 和 `jumpSpeed` 未复制进组件。没有添加 Movement System、Query、definition adapter 或测试。C03 状态为 `partial/implemented`；未验证 static tuning owner、P07/P03 调度、Physics/Movement duplicate writer、网络或持久化。

保存本 checkpoint 后执行游标推进为 `currentComponent: C04`，`pendingComponents: [C04, C05, C06, C07, C08, C09]`。
