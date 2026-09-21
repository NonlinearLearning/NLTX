# Version4 P04 玩家身份、生命周期与世界交互组件拆分设计

partitionId: P04
sessionId: 5230602080084a98a9b991b67e88416d
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P04-Player-Lifecycle-Interaction.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P04-player-lifecycle-interaction-component-execution.md
designStatus: proposed
executionStatus: in-progress
implementationStatus: in-progress
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: partial
completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13]
currentComponent: ""
pendingComponents: []
lastCheckpointUtc: 2026-09-12T08:38:48.0851915Z
evidence-gap: C01-C12 的允许范围源码已落地，其中 C06、C11、C12 仍为 partial；C09/C13 按证据不创建 ECS component。C06 的 chest relation 与 P09 已有 PlayerContainerRelationComponent owner 冲突，C13 的 P11 owner、随机源注入、表现帧消费和跨客户端复现策略仍未闭合。
blocking-decision: integration-review

## 1. 设计边界

本文件只覆盖 P04 权威报告中的 13 个叶子子系统、111 个字段和 1 个属性，共 112 个成员。它记录 proposed ECS 边界及本次允许的组件源码落地进度，不声称行为等价、公共 API 兼容、网络闭合或持久化闭合。除本文件和绑定执行文档外，本轮不修改 `Test/`、Version4、权威报告、ledger 或其他会话文档。

组件按共同读写者、变更原因、生命周期和副作用重分组；权威报告的叶子组作为覆盖检查点，不自动等于一个巨型 Component。

## 2. 检查点总览

| 检查点 | 权威叶子组 | 成员 | 初步 proposed 边界 | 当前证据 |
|---|---|---:|---|---|
| C01 | `PlayerIdentityAndDeathRecordState` | 10 | 玩家身份/活动与死亡记录 owner | partial |
| C02 | `PlayerRuntimeInteractionAndEffectState` | 13 | 交互能力、运行时效果、外部定义和表现 adapter | partial |
| C03 | `PlayerTeleportTransitionState` | 4 | 传送事务阶段与确认状态 | partial |
| C04 | `PlayerDeathRespawnAndSaveState` | 14 | 死亡/观战/复活、保存时标、战斗/环境交接 | partial |
| C05 | `PlayerSpawnAndReturnState` | 4 | 玩家生成点与 Potion of Return 路由 | partial |
| C06 | `PlayerContainerAndWorldAnchorState` | 17 | 容器关系、交互锚点、世界交互 scratch | partial |
| C07 | `PlayerPortalAndTargetingState` | 11 | 传送门、目标选择和投射物/钩爪索引 | partial |
| C08 | `PlayerItemActionTimingState` | 12 | ItemCheck 时序与跨 Wiring/Combat 的计时 seam | partial |
| C09 | `PlayerItemCheckContext` | 1 | 短生命周期 ItemCheck command payload | confirmed-shape |
| C10 | `PlayerPettingState` | 7 | 宠物目标关系与验证查询 | partial |
| C11 | `PlayerSittingState` | 5 | 座位状态与座椅查询 | partial |
| C12 | `PlayerSleepingState` | 7 | 睡眠状态、床面投影与休息聚合 seam | partial |
| C13 | `PlayerRabbitOrderFrameState` | 7 | 兔子指令帧的表现 projection/随机 adapter | partial |

各 proposed 边界和调度契约已完成设计记录；允许范围内的组件源码已按检查点保存，未实现的 System、Query、Command、Adapter 和 Projection 仍保持为设计项。每个检查点完成后必须同步本文件和执行文档的状态字段。

## 3. 证据等级与来源

`confirmed` 只表示直接源码事实已定位；`existing-evidence` 表示当前 NLTX 已有原型；`partial` 表示调用者、端别、持久化、网络或 owner 仍未完全闭合；`integration-review` 表示本分区不能宣布最终 owner。未运行的 verifier、build 和 test 均保持 `not-run`。

| 来源 | 已核对事实 | 用途 | 状态 |
|---|---|---|---|
| `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P04-Player-Lifecycle-Interaction.md:1-411` | 13 个叶子组、112 个成员、来源序号、声明类型、路径、行列和原始声明 | P04 覆盖分母 | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:140-214` | `RabbitOrderFrameHelper` 的状态字段和 Update/Reset/ChangeToAIState/UpdateFrame 行为 | C13 表现状态和随机副作用 | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:282-304` | `ItemCheckContext.SkipItemConsumption` 是 ItemCheck 过程内 struct 字段 | C09 短生命周期 payload | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:478-523,646-652,1134-1160,1895-1901,2262-2419` | P04 Player 字段的直接声明、默认值、数组/集合容量和可见性 | 成员所有权初始证据 | confirmed |
| `D:\TRbackup\Version4\Terraria.GameContent\PlayerPettingInfo.cs:5-83` | 宠物目标可指向 NPC、Projectile 或 mount，含实体索引和目标校验 | C10 entity reference seam | confirmed |
| `D:\TRbackup\Version4\Terraria.GameContent\PlayerSittingHelper.cs:6-181` | 坐姿更新读 Tile/Player 输入，维护 offset/details/stack，退出可广播 | C11 状态、查询和网络副作用 | confirmed |
| `D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs:7-184` | 睡眠状态、120 tick 完全入睡判定、床面查询、旋转调整和退出广播 | C12 状态/投影/副作用 | confirmed |
| `D:\TRbackup\Version4\Terraria.DataStructures\PlayerInteractionAnchor.cs:3-47` | TileEntity anchor 的 entity ID、坐标、Clear/Set 和 registry lookup | C06 外部实体关系 | confirmed |
| `D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs:3-76` | Projectile owner/identity/type 的跟踪、清理、二进制读写和恢复匹配 | C06/C07 网络/持久化 adapter | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:15725-15728,19678-19741` | Player Tick 调用 petting/sitting/sleeping；StopVanityActions 统一终止它们 | C10-C12 调度顺序 | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:9865-9924,21731-21797` | 传送视觉消费随机/粒子/声音并写回传送样式、门户颜色和重力状态 | C03/C07 effect boundary | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:9948-10124,21798-21895,22572-22678` | 死亡、观战、生成、复活和死亡记录写入入口 | C01/C04/C05 lifecycle owner | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:23435-23505` | ItemCheck 创建 context，校验/更新 itemAnimation、itemTime 并驱动使用行为 | C08/C09 command boundary | confirmed |
| `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:600-730` | 网络输入直接写入 Spawn、respawn/death、teleport acknowledgement、Potion of Return、坐/睡/宠物标志 | C03-C05/C10-C12 network adapter | confirmed |
| `D:\TRbackup\Version4\Terraria\NetMessage.cs:2631-2677,1596-1621` | 玩家加入同步、chest/loadout/slot 快照和 tracked projectile 输出 | C06/C07 外部 projection | confirmed |
| `D:\TRbackup\NLTX\src\Player\PlayerLifecycleComponent.cs`, `PlayerLifecycleState.cs` | NLTX 已有局部死亡/复活/死亡记录状态，但使用 Phase/WorldPosition/SimulationTick 语义 | C01/C04 compatibility input | existing-evidence |
| `D:\TRbackup\NLTX\src\Player\PlayerSpawnPointComponent.cs`, `PlayerSpawnPointState.cs` | NLTX 已有 SpawnX/Y 兼容字段和返回路由，但没有 Version4 network/save 闭合 | C05 compatibility input | existing-evidence |
| `D:\TRbackup\NLTX\src\Player\PlayerRestComponent.cs`, `PlayerRestState.cs` | NLTX 已有 sitting/sleeping 聚合模型和 120 tick 派生判断 | C11/C12 compatibility input | existing-evidence |
| `D:\TRbackup\NLTX\src\Teleportation\*.cs`, `src\Player\PlayerContainerRef.cs`, `src\WorldInteraction\Interaction\InteractionActorContextComponent.cs` | 已有传送门、冷却、玩家容器关系和交互发起者边界 | C03/C06/C07 compatibility input | existing-evidence |
| `D:\TRbackup\tmodloader-api-docs-stable\index.html`, `class_player.html`, `struct_player_sitting_helper.html`, `struct_player_sleeping_helper.html` | 首页显示 `tModLoader v2026.07`；公开 Player/座椅/睡眠类型页面可用于 API 边界交叉参考 | 不替代 Version4 私有语义 | confirmed-public-boundary |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Wires\WiresComponent.cs:6-70`, `WiresSystem.cs` | SS14 将局部持续状态放 Component，将事件/更新/网络/Dirty 处理放 System | 只支持组织粒度 | organization-only |

### 3.1 证据缺口

- Version4 当前源码能证明声明、若干调用和网络入口，但 P04 每个字段的完整读者/写者闭包、服务端/客户端权威端、文件存档版本和失败恢复协议仍为 `partial`。
- `DateTime.Now`、`Main.rand`、粒子/声音、`NetMessage.SendData`、TileEntity/Projectile registry 和 `Main.ShopHelper` 都是 effect boundary；不能进入纯 Query 或无声明副作用的 Component。
- `PlayerLifecycleComponent`、`PlayerRestComponent`、`PlayerSpawnPointComponent` 等 NLTX 原型是 existing evidence，不等于唯一 owner 已确认；迁移前须检查是否与其他分区的 Combat、Teleportation、WorldStorage、WorldInteraction 和 P11 Presentation 重复。
- tModLoader 与 SS14 证据不确认 Version4 私有字段语义、网络字节布局或当前 NLTX 行为。

## 4. 依赖方向总则

外部输入/网络 -> 资格或纯计算 Query -> 显式 Command -> 唯一 Owner System -> 权威 Component/关系 -> committed snapshot -> Network/Persistence/Presentation Adapter。

跨分区关系只通过只读 Query、Command、Event 或 Projection；P04 不宣布 `Combat`、`Teleportation`、`WorldStorage`、`Wiring`、`Items` 或 P11 的最终 owner。文件名和 Markdown 顺序不表达运行时顺序，调度必须由显式 scheduler contract 固定。

## 5. 全量成员归属

本节在 C01-C13 检查点完成时逐步写入。每个成员必须落到 proposed Component、System、Query、Command、Adapter、Projection 或 `deferred`，并保留来源序号与 Version4 行号。

### C01 `PlayerIdentityAndDeathRecord`

**设计状态：** `proposed`。本检查点不把死亡掉落、死亡原因、网络广播或存档字节格式归入 Player Component；它只保留玩家身份事实和死亡结果记录的最小数据面。

#### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerIdentityComponent` | `src/Player/PlayerIdentityComponent.cs` | `active`、`host`、`name` 的实体槽位身份与可见性 | Connect/Disconnect/Spawn command -> identity snapshot | `IPlayerSessionPort`；`PlayerLifecycleSystem` 只读/提交 active |
| `PlayerDeathRecordComponent` | `src/Player/PlayerDeathRecordComponent.cs` | PVE/PVP 死亡计数、死亡位置/时间、最近一次显示标记及掉币结果 | `DeathResolvedCommand` -> record snapshot | `IDeathPenaltyCommitPort`；不得自行 DropItems/广播 |
| `PlayerDeathRecordQuery` | `src/Player/PlayerDeathRecordQuery.cs` | 纯读取死亡可见性、计数和最近死亡快照 | identity/death snapshot -> immutable result | 无写回、无时钟读取 |
| `PlayerDeathRecordProjection` | `src/Player/PlayerDeathRecordProjection.cs` | 网络/UI/存档所需的单向输出 | committed record -> adapter payload | network/persistence owner 待 integration-review |

Component 只保存权威数据；`DateTime.Now`、掉币计算、死亡文本、聊天、成就、墓碑和物品返回均是 System/Adapter 效果。`lastDeathPostion` 的历史拼写必须通过兼容 Adapter 保持，不能在迁移中无证据地改动公开 API。

#### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 445 | `active: bool` | `PlayerIdentityComponent.IsActive`，兼容字段 | `Spawn` 写 `true`；连接/离开和生命周期系统读写；网络 slot sync 读取；entity create/disable 生命周期 | authoritative identity；network/session effect | `Player.cs:478,21847-21852`; `NetMessage.cs:2636-2644`；partial 全调用图 |
| 446 | `host: bool` | `PlayerIdentityComponent.IsHost`，兼容字段 | 主机资格/连接同步读取；session adapter 写入；随连接生命周期清理 | authoritative session fact；network effect | `Player.cs:480`; host 判定在 `NetMessage.DoesPlayerSlotCountAsAHost`/连接流程；partial |
| 454 | `lostCoins: long` | `PlayerDeathRecordComponent.LostCoins` | `KillMe` 在 PVE 非旅途/非 PVP 路径通过 `DropCoins` 写；死亡记录/死亡 penalty 读；掉币结果输出 | authoritative death result；Item/DeathPenalty cross-owner | `Player.cs:497,22593-22672`; effect ordering and owner integration-review |
| 455 | `lostCoinString: string` | `PlayerDeathRecordComponent.LostCoinText`，保留兼容投影 | `KillMe` 从 `lostCoins` 格式化写；死亡聊天/UI 读；下一死亡覆盖 | derived presentation cache；不得作为钱币 authority | `Player.cs:499,22670-22672`; formatting is effect-adjacent |
| 458 | `name: string` | `PlayerIdentityComponent.DisplayName`，保留原公开字段 | join packet/聊天/死亡文本读；网络输入和角色载入写；identity create/reset 生命周期 | authoritative public identity；network/persistence effect | `Player.cs:505`; `MessageBuffer.cs:244`; `NetMessage.cs:2681-2684` |
| 459 | `numberOfDeathsPVE: int` | `PlayerDeathRecordComponent.PveDeathCount` | `KillMe` 在 PVE 分支递增；网络 Spawn packet 读写；存档 projection 待闭合 | authoritative counter；network/persistence partial | `Player.cs:507,22593-22600`; `MessageBuffer.cs:612`; save format missing |
| 460 | `numberOfDeathsPVP: int` | `PlayerDeathRecordComponent.PvpDeathCount` | `KillMe` 在 PVP 分支递增；网络 Spawn packet 读写 | authoritative counter；network/persistence partial | `Player.cs:509,22581-22599`; `MessageBuffer.cs:613` |
| 465 | `lastDeathPostion: Vector2` | `PlayerDeathRecordComponent.LastDeathPosition` + spelling-preserving Adapter | `KillMe` 写 `base.Center`；死亡 UI/地图/网络读者待全量确认；新死亡覆盖 | authoritative event snapshot；DateTime/position input | `Player.cs:519,22601`; report/legacy public name has typo |
| 466 | `lastDeathTime: DateTime` | `PlayerDeathRecordComponent.LastDeathTime` | `KillMe` 直接读取 `DateTime.Now` 写；显示/日志/存档读者待闭合 | authoritative record with hidden clock effect | `Player.cs:521,22601-22603`; replace with explicit clock port before migration |
| 467 | `showLastDeath: bool` | `PlayerDeathRecordComponent.ShowLastDeath` | `KillMe` 写 `true`；死亡记录消费后/清理路径应写 `false`，现有清理证据 partial | authoritative visibility flag; UI/network effect | `Player.cs:523,22601-22603`; reset/consumer missing |

#### 不变量、兼容和 owner

- `active=false` 不能与仍可被连接/网络同步的身份快照混用；`host` 不是死亡状态，不得由死亡系统清理。
- PVE/PVP 计数一次死亡最多递增一次；`KillMe` 的早退条件和 `dead` transition 必须由 verifier 固定。
- `lostCoinString` 始终由同一次 committed `lostCoins` 结果派生，不能被独立网络输入写入。
- `LastDeathTime` 使用显式 `IPlayerClock`；失败或未知时不伪造时间。掉币/物品/聊天由 `IDeathPenaltyCommitPort` 和外部 adapter 负责。
- NLTX 现有 `PlayerIdentityComponent`、`PlayerLifecycleComponent` 与 `PlayerLifecycleState` 只能作为迁移兼容输入；C01 不重复创建第二个玩家身份 owner，必须先进行 integration-review。

#### C01 focused verifier 计划

1. `IdentityActivationTransition`: create/connect/spawn/disconnect 的 `active`、`host`、`name` 组合和重复 connect/disconnect。
2. `DeathRecordTransition`: PVE/PVP、重复 `KillMe`、死亡位置/时间、`showLastDeath` 清除和 respawn 边界。
3. `DeathPenaltyPort`: `lostCoins` 与 `lostCoinString` 同批提交，验证 DropCoins/物品/聊天只由 port 发生一次。
4. `DeathRecordProjection`: network/save/UI 只读快照；记录存档格式和 full reference 缺口，未闭合前 verifier 状态为 `not-run`。

#### C01 检查点

- `completedComponents`: C01 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C02 `PlayerRuntimeInteractionAndEffect`。
- 实现：`implemented`；验证：`not-run`。
- 实际源码：`src/Player/PlayerIdentityComponent.cs`、`src/Player/PlayerDeathRecordComponent.cs`。
- 已落地字段：`IsActive`、`DisplayName`、`LostCoins`、`LostCoinText`、`PveDeathCount`、`PvpDeathCount`、`LastDeathPosition`、`LastDeathTime`、`ShowLastDeath`。
- 依赖缺口：现有 `PlayerLifecycleComponent` 仍包含部分死亡记录字段；本次未接入 writer、clock、DeathPenalty、network 或 persistence adapter，故仅标记源码实现，未标记行为完成。

#### C02 实现检查点

- 实际源码：`src/Player/PlayerRuntimeInteractionComponent.cs`。
- 已落地字段：`EmoteRemainingTicks`、`SpelunkerRemainingTicks`、`BuilderToggleStatuses`，数组按现有 `PlayerBuilderInteractionCatalog.Count` 初始化。
- 依赖缺口：CreativeUnlocksTracker、OverheadMessage、Minecart/Grapple、soulDrain、DD2、basilisk 和 Combat 静态定义仍是外部/跨分区边界；本次未新增 adapter、query 或 system。
- 实现：`implemented`；验证：`not-run`。

#### C03 实现检查点

- 实际源码：`src/Teleportation/PlayerTeleportTransitionComponent.cs`。
- 已落地字段：`IsTeleporting`。
- 依赖缺口：`teleportTime`/`teleportStyle` 仍属于视觉 projection，`unacknowledgedTeleports` 仍属于网络 acknowledgement adapter；本次未新增移动、粒子、随机、网络或 system 代码。
- 实现：`implemented`；验证：`not-run`。

#### C04 实现检查点

- 实际源码：`src/Player/PlayerDeathRespawnComponent.cs`、`src/Player/PlayerSaveCheckpointComponent.cs`。
- 已落地字段：`IsDead`、`DeadElapsedTicks`、`SpectatingTarget`、`RespawnRemainingTicks`、`LastSavedBinaryTimestamp`。
- 依赖缺口：`PlayerLifecycleComponent` 仍是现有兼容聚合；规则常量、死亡效果、Combat/Items handoff、HitTile scratch、观战 registry、clock 和 persistence I/O 未在组件中实现。
- 实现：`implemented`；验证：`not-run`。

#### C05 实现检查点

- 实际源码：复用并更新 `src/Player/PlayerSpawnPointComponent.cs`。
- 已落地字段/派生状态：既有 `SpawnX`、`SpawnY`、`ReturnOriginalUsePosition`、`ReturnHomePosition`，新增 `HasSpawnCoordinates` 和 `HasReturnRoute`，确保两组值按成对语义读取。
- 依赖缺口：未新增 `PlayerReturnRouteComponent`；Spawn selection、type 12/13 network adapter、Potion of Return writer、persistence 和 C04/C03 handoff 仍由外部边界负责。
- 实现：`implemented`；验证：`not-run`。

#### C06 实现检查点

- 实际源码：`src/Player/PlayerTrackedContainerLinkComponent.cs`、`src/Player/PlayerTileInteractionCapabilityComponent.cs`、`src/WorldInteraction/PlayerTileEntityAnchorComponent.cs`；`PlayerContainerRelationComponent` 已由 P09 会话先行落地，本次未修改。
- 已落地字段：Piggy Bank/Void Lens 各自的 tracking flag、local slot、owner slot、identity、type；三个 tile capability flags；TileEntity entity reference、tile coordinate 和 `HasAnchor`。
- 依赖缺口：P09 的 `PlayerContainerRelationComponent` 当前只拥有 bank/void-vault 关系，未包含 P04 的 chest current/previous 字段；本次没有覆盖或追加该共享类型。scratch、容器内容、Projectile/TileEntity registry、network adapter、shop/door/eye projection 或跨域 writer 均未创建；C10-C12 的 petting/sitting/sleeping 未重复建模。
- 实现：`partial`；验证：`not-run`。

#### C07 实现检查点

- 实际源码：`src/Teleportation/PlayerPortalTraversalComponent.cs`、`src/Combat/PlayerMinionTargetComponent.cs`。
- 已落地字段：`LastPortalColorIndex`、`PortalPhysicsRemainingTicks`、`PortalPhysicsRequested`、`LastPylonStyle`、`RestTargetPoint`、`AttackTarget` 和 `HasRestTarget`。
- 依赖缺口：projectile/NPC caches、Fishron/mount handoff、grapple blacklist、strong-bee random effect、NPC registry validation 和 type 99/115 network projection 未实现。
- 实现：`implemented`；验证：`not-run`。

#### C08 实现检查点

- 实际源码：`src/Player/PlayerItemActionTimingComponent.cs`。
- 已落地字段：`AnimationRemaining`、`AnimationDuration`、`UseRemaining`、`UseDuration`、`ToolUseMarker`。
- 依赖缺口：现有 `PlayerUseComponent` 的兼容重叠、delay definition、Wiring cooldown、fall tracking、global projectile block、ItemCheck writer 和 C04 attackCD handoff 未在本次组件范围内接入。
- 实现：`implemented`；验证：`not-run`。

#### C09 实现检查点

- 实际源码：无；按设计证据不创建 ECS component。
- 已确认字段：`SkipItemConsumption` 仅为 `ItemCheckContext` 的短生命周期默认 `false` payload，不进入 Player entity、save 或 network snapshot。
- 依赖缺口：当前未发现可确认的 read/write/consume site；Items consumption、mod/hook 输入、失败/重复语义需后续 evidence closure 后再决定是否添加非组件 command/context adapter。
- 实现：`deferred-no-component`；验证：`not-run`。

#### C10 实现检查点

- 实际源码：`src/Player/PlayerPettingComponent.cs`。
- 已落地字段：`IsPetting`、NPC/projectile slot 与期望类型、`MountId`、`IsMountTarget`、`OffsetFromPet`、`IsPetSmall`。
- 依赖缺口：本次未创建 `PlayerPetTargetReference`、registry query、network adapter 或 vanity system；raw slot 复用、target identity、active/type 校验和 Mount 有效性仍待外部 owner 闭合。
- 实现：`implemented`；验证：`not-run`。

#### C11 实现检查点

- 实际源码：`src/Player/PlayerSittingComponent.cs`。
- 已落地字段：`IsSitting`、`SeatFeatures`、`SeatOffset`、`StackIndex`；`StackIndex` 默认 `-1`。
- 依赖缺口：Version4 `ExtraSeatInfo` 在当前 src 不存在，本组件使用已有 `RestSeatFeatures` 仅承载已确认的厕所位；未新增 seat query/stack adapter/network adapter，也未接入现有 `PlayerRestComponent` writer。
- 实现：`partial`；验证：`not-run`。

#### C12 实现检查点

- 实际源码：`src/Player/PlayerSleepingComponent.cs`。
- 已落地字段：`IsSleeping`、`StackIndex`、`TimeSleeping`、`BedVisualOffset`；`StackIndex` 默认 `-1`。
- 依赖缺口：`SetOffsetbyBed` 仍是 Version4 stub，未实现 bed eligibility、act-up predicate、rotation、sleepingManager、network/presentation adapter 或 `FullyFallenAsleep` query；C11/C12 unique writer 仍需 integration review。
- 实现：`partial`；验证：`not-run`。

#### C13 实现检查点

- 实际源码：无；C13 被设计为 projection-only，且本任务只允许组件源码，不新增 `PlayerRabbitOrderFrameProjection` 或 `RabbitOrderFrameStateDefinition`。
- 已确认边界：DisplayFrame、内部 frame counter/state、随机选择、渲染消费和 Spawn reset 都保留在 P11/Client integration-review；不伪造 ECS authority。
- 依赖缺口：P11 owner、随机源、frame consumer、head-259 gate 和跨客户端 determinism 未闭合。
- 实现：`deferred-no-component`；验证：`not-run`。

### C02 `PlayerRuntimeInteractionAndEffect`

**设计状态：** `proposed`。这 13 个成员不是单一生命周期：玩家局部计时/开关、外部内容 tracker、聊天表现、坐骑/抓钩能力和 Combat 常量的读写者与变更原因不同。因此 C02 采用窄 Component 加 Adapter/Query 的组合，禁止形成重新聚合所有效果字段的巨型组件。

#### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerRuntimeInteractionComponent` | `src/Player/PlayerRuntimeInteractionComponent.cs` | 玩家局部交互计时和建造器开关快照 | interaction command -> committed local facts | `IPlayerInteractionPort` |
| `PlayerCreativeProgressionAdapter` | `src/Player/PlayerCreativeProgressionAdapter.cs` | 转换 `CreativeUnlocksTracker` 外部对象 | content command/snapshot -> external tracker | ContentCatalog/Progression integration-review |
| `PlayerOverheadMessageProjection` | `src/Player/PlayerOverheadMessageProjection.cs` | 输出 `OverheadMessage` 的聊天/尺寸/剩余时间快照 | message input -> client projection | Chat/ClientPresentation owner |
| `PlayerRuntimeEffectQuery` | `src/Player/PlayerRuntimeEffectQuery.cs` | 读取 Grapple/Combat/Mount 外部事实，不写玩家状态 | explicit snapshots -> eligibility/result | P03/P06/P07 integration-review |
| `PlayerCombatTuningDefinitionQuery` | `src/Combat/PlayerCombatTuningDefinitionQuery.cs` | 提供 crystal leaf 与 Paladin shield 常量 | immutable catalog -> deterministic values | Combat owner，不挂 Player entity |

#### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 447 | `MinecartSettings: Minecart.Customization` | `MountVehicleCompatibilityAdapter`，`crossSubsystemOwner: integration-review` | Mount/Minecart 系统读取/写入；随玩家载具状态 hydrate/clear | external value object；不得复制到 Player runtime component | `Player.cs:482`; P03/Mount owner 未在 P04 确认 |
| 448 | `emoteTime: int` | `PlayerRuntimeInteractionComponent.EmoteRemainingTicks` | Player Tick/表现逻辑递减或重置；emote command 写；短生命周期 | authoritative transient；音频/表现输出为 effect | `Player.cs:484`; full reader/writer closure partial |
| 449 | `creativeTracker: CreativeUnlocksTracker` | `PlayerCreativeProgressionAdapter` | 创意模式/内容解锁读写；角色创建/连接 hydrate；对象生命周期由外部 tracker 管理 | external mutable object；不进入纯 Query | `Player.cs:486`; type definition and persistence boundary partial |
| 450 | `chatOverhead: OverheadMessage` | `PlayerOverheadMessageProjection` | 聊天消息创建写 `ParseMessage/GetStringSize`；绘制/UI 读；过期清理 | presentation snapshot；字体解析、日志/UI 是 effects | `Player.cs:488`; `OverheadMessage.NewMessage:455-476` |
| 451 | `GoingDownWithGrapple: bool` | `GrappleTraversalState` seam，`crossSubsystemOwner: integration-review` | Grapple/movement update 写读；玩家输入/钩爪生命周期清理 | authoritative movement capability candidate | `Player.cs:491`; owner in P07/Movement not confirmed |
| 452 | `spelunkerTimer: byte` | `PlayerRuntimeInteractionComponent.SpelunkerRemainingTicks` | Buff/equipment effect 或 Player Tick 写；探测器/表现消费读；reset path partial | transient effect timer；不作为 buff catalog | `Player.cs:493`; `ResetEffects/Update` related readers partial |
| 453 | `builderAccStatus: int[]` | `PlayerRuntimeInteractionComponent.BuilderToggleStatuses` | Builder toggle input 写；tile interaction/visual readers；数组容量为 `BuilderAccToggleIDs.Count` | authoritative player capability snapshot；builder registry external | `Player.cs:495`; `BuilderAccToggleIDs.Count` declaration and Player readers |
| 456 | `soulDrain: int` | `CombatSoulDrainState` seam，`crossSubsystemOwner: integration-review` | Combat/status effects 写读；死亡/复活/效果 reset 可能清理 | authoritative combat status, not P04 owner | `Player.cs:501`; Combat call graph not fully closed |
| 457 | `dd2Accessory: bool` | `Dd2PlayerEventAdapter` seam，`crossSubsystemOwner: integration-review` | DD2 event/equipment logic 写读；Player reset/respawn lifetime | event capability flag; event/achievement effect boundary | `Player.cs:503`; DD2 owner is P13/WorldSession |
| 461 | `crystalLeafDamage: static int = 100` | `PlayerCombatTuningDefinitionQuery` | Combat calculation reads; content setup may configure; process lifetime, no entity instance | immutable-ish definition; static mutable global risk | `Player.cs:511`; keep out of Component, configuration owner partial |
| 462 | `crystalLeafKB: static int = 10` | `PlayerCombatTuningDefinitionQuery` | Combat knockback calculation reads; content setup may configure | definition; no per-player lifecycle | `Player.cs:513`; same static-owner gap |
| 463 | `basiliskCharge: float` | `MountCombatCapabilityState` seam，`crossSubsystemOwner: integration-review` | Mount update/effect reads and writes; dismount/death clear expected | mount-specific authoritative runtime state | `Player.cs:515`; `UpdateDead:9957-9959` shows clear adjacent state, owner P03 |
| 464 | `PaladinsShieldRange: static float = 800f` | `PlayerCombatTuningDefinitionQuery` | shield eligibility reads; content/config setup may configure | definition; no player entity state | `Player.cs:517`; shield logic `Hurt`/Combat owner partial |

#### 组合、不变量和副作用

- `emoteTime`, `spelunkerTimer` 和 builder statuses 可随玩家实体存在，但其 effect consumers must not mutate them through a read query。数组快照必须防御性复制，不能泄露内部可变集合。
- `MinecartSettings`, `GoingDownWithGrapple`, `soulDrain`, `dd2Accessory` 和 `basiliskCharge` 不在 C02 声明最终 owner；它们通过只读 Query/Command/Event 交给 P03/P06/P07/P13。
- `crystalLeafDamage`、`crystalLeafKB`、`PaladinsShieldRange` 是静态定义候选，不得被序列化为每个玩家的 Component；修改需要显式 content/config owner。
- `OverheadMessage.NewMessage` 的文本解析和尺寸计算是表现边界；Projection 只输出不可变结果，不在 Query 内访问字体、音频或网络。

#### C02 focused verifier 计划

1. `RuntimeInteractionReset`: emote/spelunker/builder 状态创建、更新、清理和重复 command。
2. `BuilderToggleSnapshot`: 数组容量、索引范围、防御性复制和无写回读取。
3. `ExternalRuntimeHandoff`: 对 Minecart/Grapple/SoulDrain/DD2/Basilisk 的跨 owner command 不重复写入。
4. `CombatTuningQuery`: 静态常量查询确定性、无实体污染和配置变更可见性。
5. `OverheadProjection`: message parse/size/expiry 只经 projection effect port，错误不驱动权威玩家状态。

#### C02 检查点

- `completedComponents`: C01、C02 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C03 `PlayerTeleportTransition`。
- 实现：`implemented`；验证：`not-run`。
- 实际源码：`src/Player/PlayerRuntimeInteractionComponent.cs`。
- 已落地字段：`EmoteRemainingTicks`、`SpelunkerRemainingTicks`、`BuilderToggleStatuses`。
- 依赖缺口：Creative tracker、overhead message、Minecart/Grapple、soulDrain、DD2、basilisk 和 Combat tuning 仍为外部边界。

## 5.3 C03 `PlayerTeleportTransition`

**设计状态：** `proposed`。四个字段虽然都出现在传送语境中，但访问模式不同：`teleporting` 是 Wiring 一轮内的防重复处理标记，`teleportTime` 和 `teleportStyle` 是传送视觉过渡快照，`unacknowledgedTeleports` 是网络位置确认协议计数。它们不应被重新拼成一个允许任意调用方写入的“传送大组件”。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerTeleportTransitionComponent` | `src/Teleportation/PlayerTeleportTransitionComponent.cs` | 保存一次传送提交所需的短生命周期阶段事实和 Wiring guard；不直接执行移动、粒子或网络发送 | `TeleportRequestedCommand` / `TeleportCommittedEvent` -> committed transition snapshot | `ITeleportCommitPort`；Wiring 只通过 command 申请 |
| `PlayerTeleportVisualProjection` | `src/Teleportation/PlayerTeleportVisualProjection.cs` | 输出 `teleportTime`、`teleportStyle` 的不可变视觉快照；允许视觉系统提交明确的 `VisualTick` 结果，不允许渲染 Query 任意写实体 | committed transition -> P11/particle/audio payload | `ITeleportVisualEffectsPort`、显式 `IRandomSource` |
| `PlayerTeleportAcknowledgementAdapter` | `src/Teleportation/PlayerTeleportAcknowledgementAdapter.cs` | 把网络 type 65 的发送计数、type 3 的确认和位置快照抑制规则转换为协议状态 | decoded network input/output -> ack command/snapshot | `INetworkTransport`；不把网络包直接当作领域写者 |
| `PlayerTeleportTransitionQuery` | `src/Teleportation/PlayerTeleportTransitionQuery.cs` | 纯判断当前是否可提交、是否存在未确认位置和视觉是否仍在播放 | immutable snapshots -> eligibility/result | 无网络、随机、粒子和写回 |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 528 | `teleporting: bool` | `PlayerTeleportTransitionComponent.IsTeleporting`，兼容字段 | Wiring 在同一轮传送筛选前读取，命中后通过 `TeleportRequestedCommand` 置为 true；Wiring 轮次结束统一清除；死亡/实体禁用也必须清除 | transient anti-duplicate guard；不是网络确认或视觉状态 | `Player.cs:646`; `Wiring.cs:2732-2757`; 完整多入口清理仍为 partial |
| 529 | `teleportTime: float` | `PlayerTeleportVisualProjection.TimeRemaining`，旧字段由视觉 Adapter 兼容读取 | `Teleport` 提交成功时写入 `1f`；`UpdateTeleportVisuals` 按 style 消费/递减；style 2/3 会把值重置为 `0.005f`；视觉结束后归零 | presentation/runtime snapshot；随机、Dust、Portal color 和 Pylon dust 是 effect | `Player.cs:648,9865-9924,21790`; P11 owner 和并发写者未闭合 |
| 530 | `teleportStyle: int` | `PlayerTeleportVisualProjection.Style`，兼容字段 | `Teleport` 成功提交时写入传送样式；视觉 Query 只读选择样式；网络/粒子 Adapter 消费，不能由粒子生成器改写 | presentation definition/reference；样式值域和跨版本兼容未确认 | `Player.cs:650,9873-9918,21791`; `MessageBuffer.cs:2348-2350`; partial |
| 531 | `unacknowledgedTeleports: int` | `PlayerTeleportAcknowledgementState.PendingCount`，由 Adapter/协议状态 owner 管理 | NetMessage type 65 向其他客户端发送玩家传送时递增；MessageBuffer type 3 确认时递减；Player 输入同步在计数大于 0 时冻结收到的位置/速度，避免未确认传送被普通位置包覆盖 | network protocol state；必须防止负数、重复确认和跨玩家确认 | `NetMessage.cs:1082-1101`; `MessageBuffer.cs:692-698,2346-2373`; 状态 ID/重放策略 partial |

### 状态转换与副作用顺序

1. Wiring、Pylon、Portal 或其他合法入口只提交 `TeleportRequestedCommand`，不直接写 `teleporting`、位置和网络计数；唯一 `TeleportCommitSystem` 检查实体仍 active、未 dead、目标可用后调用 `ITeleportCommitPort`。
2. Commit port 先记录旧位置和目标位置，再执行位置提交、section/碰撞/重力等世界交接，最后提交视觉快照；只有网络输出成功排队后，`PendingCount` 才按受影响客户端增加。网络发送失败或未知结果不能静默当成已确认。
3. `UpdateTeleportVisuals` 只能消费 `PlayerTeleportVisualProjection` 并发出 Dust/声音/Portal/Pylon effect；随机数必须来自注入的 `IRandomSource`，视觉失败不回滚已经提交的玩家位置。
4. 普通位置同步在 `PendingCount > 0` 时读取冻结策略，但不修改 pending 状态；只有与玩家和传送事务匹配的确认命令才能递减。计数不得小于零，重复/迟到确认进入可观测的丢弃路径。
5. `teleporting` 的 true 生命周期限定在一次 Wiring 扫描；不能用它表达“玩家正在播放传送动画”，也不能因网络 ack 到达而清除它。文件顺序不定义上述顺序，须由 scheduler contract 固定。

### 不变量、兼容和 owner

- 一次 `TeleportRequestedCommand` 最多产生一次位置提交和一次视觉初始化；重复 command 必须返回 rejected/duplicate 结果而不重复发包。
- `teleportTime >= 0`；视觉递减采用明确 tick 输入，不能在纯 Query 中读取系统时钟或随机源。样式 2/3 的 `0.005f` 特殊值必须保留，不能概括为统一的线性衰减。
- `PendingCount` 与实际受影响客户端的未确认集合应在未来协议闭合后保持一致；在当前证据不足时只保留计数兼容，不推断可靠传输、超时重试或存档语义。
- `teleporting`、Portal/Pylon 颜色和重力/portal physics 相关成员存在跨 P04/C07/Teleportation/Wiring 交接；C03 不宣布 P07 或 WorldInteraction 的最终 owner。
- 现有 NLTX `src/Teleportation/TeleportCooldownState*`、Portal 组件和 Wiring scratch 只能作为兼容输入；它们不自动证明 Version4 四个字段已有唯一替代 owner。

### C03 focused verifier 计划

1. `TeleportCommitOnce`: 同一 Wiring 扫描、重复 command、dead/inactive entity 和目标无效时验证单次提交及 `teleporting` 清理。
2. `TeleportVisualProjection`: style 0/1/2/3/4/9 的 timer 初始化、特殊重置、递减下界和 effect port 调用顺序；验证视觉失败不改变权威位置。
3. `TeleportAcknowledgementProtocol`: type 65 发送、type 3 确认、重复/迟到/错误玩家确认、pending 位置冻结和负数保护；网络字节和可靠性语义未闭合前保持 `not-run`。
4. `TeleportCrossPartitionHandoff`: Portal/Pylon/Wiring/Movement 的 command、event 和唯一 writer 检查，确保不把 `teleporting` 当作动画或 ack 状态。

#### C03 检查点

- `completedComponents`: C01-C03 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C04 `PlayerDeathRespawnAndSave`。
- 实现：`implemented`；验证：`not-run`。
- 实际源码：`src/Teleportation/PlayerTeleportTransitionComponent.cs`。
- 已落地字段：`IsTeleporting`。
- 依赖缺口：视觉 timer/style、未确认传送计数、位置提交、随机效果和网络 ack 仍为外部边界。

## 5.4 C04 `PlayerDeathRespawnAndSave`

**设计状态：** `proposed`。这组成员同时包含死亡生命周期、观战目标、规则常量、保存时标、物品/Combat 冷却、坐骑环境效果和 `HitTile` 可变 scratch。组件只承载玩家死亡/复活阶段；常量、保存格式和跨系统 scratch 通过定义、Adapter 或 integration seam 表达，不把所有字段塞进一个死亡组件。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerDeathRespawnComponent` | `src/Player/PlayerDeathRespawnComponent.cs` | `dead`、`deadTime`、`respawnTimer` 和观战目标的生命周期快照 | `DeathCommittedCommand` / `RespawnCommand` / `SpectateCommand` -> committed snapshot | `PlayerDeathRespawnSystem`；网络只通过 adapter 输入命令 |
| `PlayerDeathRespawnRulesDefinition` | `src/Player/PlayerDeathRespawnRulesDefinition.cs` | `respawnTimerMax`、观战锁定和观战 linger 的稳定规则值 | immutable rules -> lifecycle Query | difficulty/world-rule integration-review |
| `PlayerSaveCheckpointComponent` | `src/Player/PlayerSaveCheckpointComponent.cs` | 保存检查点的原始二进制时标；不自行触发文件 I/O | save commit result -> checkpoint snapshot | `IPlayerPersistencePort`、显式 clock |
| `PlayerSpectatingQuery` | `src/Player/PlayerSpectatingQuery.cs` | 纯判断目标是否可观战、生命周期阶段和目标存活窗口 | identity/lifecycle snapshots -> eligibility | `IPlayerRegistryQuery` 只读外部实体快照 |
| `PlayerDeathCombatHandoff` | `src/Combat/PlayerDeathCombatHandoff.cs` | 为 `attackCD`、`difficulty`、死亡惩罚和受击辅助提供 P04 交接契约 | combat/death commands -> external owner | Combat/DeathPenalty integration-review |
| `PlayerTileImpactScratchAdapter` | `src/WorldInteraction/PlayerTileImpactScratchAdapter.cs` | 管理 `HitTile`/`HitTile` replacement 的临时可变对象和清理 | tile-impact command -> scratch effect | `IHitTileEffectPort`；不持久化、不作为纯 Query 状态 |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 768 | `dead: bool` | `PlayerDeathRespawnComponent.IsDead`，兼容字段 | `KillMe` 在死亡结算后置 true；`UpdateDead` 读取；`Spawn` 在非延迟进入时置 false；多种能力 Query 以它拒绝交互 | authoritative lifecycle fact；死亡效果/广播不在 Component | `Player.cs:1134,21798-21850,22572-22652`; network/save closure partial |
| 769 | `deadTime: int` | `PlayerDeathRespawnComponent.DeadElapsedTicks` | `UpdateDead` 每 tick 递增；`Spawn` 清零；`CanSpectate` 用它与 linger 规则比较 | deterministic lifecycle counter；tick 输入必须显式 | `Player.cs:1136,9948-9953,10089-10092,21847-21849` |
| 770 | `spectating: int` | `PlayerDeathRespawnComponent.SpectatingTarget` + `PlayerSpectatingAdapter`；保留 slot-id 兼容表示 | `SetOrRequestSpectating` 读取/写入；非法目标触发 fallback；Spawn/Ghost/死亡清理为 `-1`；网络 type 150 广播 | relationship plus network player-slot ID；不能把裸 int 当稳定 ECS EntityId | `Player.cs:1138,10076-10123,21852-21853`; `MessageBuffer.cs:3276-3277`; entity mapping partial |
| 771 | `respawnTimer: int` | `PlayerDeathRespawnComponent.RespawnRemainingTicks` | `KillMe` 从 `GetRespawnTime` 写；`UpdateDead` 按规则 clamp 递减；Spawn packet 读写并在大于 0 时恢复 dead | authoritative timer；规则/网络/死亡原因交接 | `Player.cs:1140,10059-10072,22649-22652`; `MessageBuffer.cs:609-620`; partial |
| 772 | `respawnTimerMax: int` | `PlayerDeathRespawnRulesDefinition.MaximumRespawnTicks`，不作为实体字段 | `UpdateDead` clamp 的规则上界；初始值 `3600`；未来 world/difficulty rule 只能通过显式 definition 配置 | immutable definition candidate；不得被每个玩家保存/网络复制 | `Player.cs:1142,10063,10071`; save/network ownership not established |
| 773 | `DeadSpectatingLockoutTime: int` | `PlayerDeathRespawnRulesDefinition.DeadSpectatingLockoutTicks` | 只作为死亡后的观战锁定规则候选；当前直接声明为 `60`，调用闭包尚未完整定位 | rule definition；不是生命周期状态 | `Player.cs:1144`; full reader closure missing |
| 774 | `SpectatingLingerAfterDeath: int` | `PlayerDeathRespawnRulesDefinition.SpectatingLingerTicks` | `CanSpectate` 用 `deadTime < 180` 判断死亡目标的观战窗口；规则变更需版本化 | rule definition；不复制为玩家快照 | `Player.cs:1146,10076-10098`; persistence/network semantics partial |
| 775 | `lastTimePlayerWasSaved: long` | `PlayerSaveCheckpointComponent.LastSavedBinaryTimestamp` + persistence Adapter | `PlayerFileData.LastPlayed` 从 `DateTime.FromBinary` 读取；Version4 当前搜索只确认声明和读取投影，未确认写入入口/保存版本 | persistence marker；clock、二进制格式和文件 I/O 是 effect boundary | `Player.cs:1148`; `Terraria.IO/PlayerFileData.cs:20-38`; writer/version missing |
| 776 | `attackCD: int` | `PlayerDeathCombatHandoff.AttackCooldown`，`crossSubsystemOwner: integration-review` | Player Update 递减，`itemAnimation == 0` 时清零；`ApplyAttackCooldown` 按 `itemAnimationMax` 或 frames 写；C04 记录交接但不创建第二个 Player owner | Combat timing state；不得由死亡系统独立重置而破坏 ItemCheck 顺序 | `Player.cs:1150,15071-15078,23957-23971`; Combat/C08 unique writer review |
| 777 | `potionDelay: int` | `PlayerDeathCombatHandoff.PotionRestriction`，`crossSubsystemOwner: Items/Combat integration-review` | buff 21 赋值；Player Update 递减；死亡更新清零；QuickMana/Item use 读取拒绝药水 | item/status timing；物品消费和 buff owner 是 effects | `Player.cs:1152,3779-3788,5890-5893,10031-10034,15079-15081,25360-25362` |
| 778 | `difficulty: byte` | `PlayerDifficultyAdapter.DifficultyCode`，不在 C04 宣布最终持久化 owner | 连接/角色输入按 0..3 解码并校正；死亡掉物/掉币、hardcore ghost 和 Journey 资格读取；角色/世界规则写入待闭合 | authoritative policy input candidate；网络/存档/DeathPenalty cross-owner | `Player.cs:1154,10059-10067,19883-20029,22606,22662`; `MessageBuffer.cs:256-271`; partial |
| 779 | `wetSlime: byte` | `PlayerWetSlimeEffectAdapter`，`crossSubsystemOwner: Movement/Mount integration-review` | Jump/液体/坐骑逻辑读取；每 tick 递减；特定 Mount/Tile interaction 写 `30`，离开 slime mount 时清零；死亡初始化/清理闭包待核对 | transient movement effect；不属于死亡持久状态 | `Player.cs:1156,3191-3194,12168-12171,17367-17383`; Movement/Mount owner partial |
| 780 | `hitTile: Terraria.HitTile` | `PlayerTileImpactScratchAdapter.PrimaryHitTile`，不进入持久化 Component | 玩家初始化构造并在适用的 Tile/Collision 逻辑中消费；`HitTile` 自带 501 项数组和随机源；死亡/重生清理需通过 adapter | mutable scratch/effect state；不能由纯 Query 暴露可变对象 | `Player.cs:1158,26567-26568`; `Terraria/HitTile.cs:9-80`; full player call graph partial |
| 781 | `hitReplace: Terraria.HitTile` | `PlayerTileImpactScratchAdapter.ReplacementHitTile`，与 `hitTile` 分离 | 初始化时独立构造；用于 replacement/alternate tile impact 路径；生命周期和调用闭包未证实，不得与主 scratch 合并 | mutable scratch/effect state；WorldInteraction/Combat seam | `Player.cs:1160,26567-26568`; `Terraria/HitTile.cs:9-80`; reader closure missing |

### 状态转换与副作用顺序

1. `DeathRequestedCommand` 先经过 Combat/DeathPenalty 的资格和结果计算；一次有效死亡向 C01 发布死亡记录结果，同时由 C04 的 lifecycle owner 提交 `dead=true`、`deadTime=0`、`respawnTimer=GetRespawnTime`。掉币、物品、墓碑、音效和聊天只由显式 effect ports 执行，不从 Component setter 触发。
2. 死亡后的 Tick 由 scheduler 先推进 `deadTime` 和 respawn timer，再执行可观战资格、持续清理和 ghost 分支；`respawnTimer` 必须 clamp 到 `[0, MaximumRespawnTicks]`，hardcore/ghost 规则由 difficulty/world policy Query 提供。
3. `SpectateCommand` 只接受 player-slot 到 EntityReference 的 Adapter 转换结果；目标失效、自己、inactive 或超出 linger 窗口时返回 rejected/fallback，并把 type 150 网络广播作为独立 effect。
4. `RespawnCommand` 在 Spawn 系统中原子提交 dead/respawn/spectating 的状态变化，再交给 Spawn/C05 选择位置和网络同步；C04 不重复拥有 `SpawnX/Y` 或 Potion of Return。
5. 保存时标只在 persistence commit 成功后更新；`DateTime.FromBinary` 的外部表示要通过版本化 Adapter 保留，当前没有证据时不把 `lastTimePlayerWasSaved` 解释成 tick、UTC 或保存触发时机。
6. `attackCD`、`potionDelay`、`wetSlime` 和 `HitTile` 不由死亡状态 Query 写入；它们分别通过 Combat/Items、Movement/Mount、WorldInteraction effect seam 接入，死亡清理的先后必须由 focused verifier 固定。

### 不变量、兼容和 owner

- `dead=true` 时不能再次执行死亡结算；`KillMe` 的 early return、C01 记录提交、掉落效果和 C04 dead commit 必须按事务边界验证，不能通过两个 owner 双写。
- `deadTime` 单调递增直到 respawn/reset；`respawnTimer` 不可为负，`spectating=-1` 表示无目标，但 slot-id 仅是网络兼容值，不是持久 EntityId。
- 规则常量不作为每个玩家的可变 Component；`DeadSpectatingLockoutTime` 的调用缺口必须保留为 evidence-gap，不能假设未搜索到的逻辑。
- `lastTimePlayerWasSaved` 只有“二进制时间标记”证据，未确认写入点、存档版本和失败重试，不得在设计中伪造 save protocol。
- `difficulty`、`attackCD`、`hitTile`、`hitReplace` 明确列入 Combat/WorldStorage/WorldInteraction integration-review；C04 不把 P04 的映射写成这些域的最终 owner。
- 现有 NLTX `PlayerLifecycleComponent`/`PlayerLifecycleState` 可作为兼容读模型，但其 Phase/SimulationTick 语义不能直接覆盖 Version4 的 deadTime、spectating slot 和 respawn timer；迁移前必须完成字段级映射。

### C04 focused verifier 计划

1. `DeathRespawnTransition`: 有效/重复死亡、deadTime、respawn timer clamp、hardcore ghost、Spawn reset 和死亡记录跨 owner 提交。
2. `SpectatingEligibility`: 自己/inactive/dead/linger 边界、目标失效 fallback、type 150 输出和 slot-id 到 EntityReference 转换。
3. `SaveCheckpointProjection`: LastPlayed binary round-trip、写入成功/失败/未知结果、版本缺口和不更新旧时标的行为。
4. `CrossOwnerTiming`: attackCD/potionDelay/wetSlime 的唯一 writer、Update/ItemCheck/Movement 清理顺序，确保 C08/C05 不重复创建 owner。
5. `HitTileScratchIsolation`: 两个 `HitTile` 实例独立初始化、清理和随机/effect 边界，验证纯 Query 不返回可变引用。

#### C04 检查点

- `completedComponents`: C01-C04 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C05 `PlayerSpawnAndReturn`。
- 实现：`implemented`；验证：`not-run`。
- 实际源码：`src/Player/PlayerDeathRespawnComponent.cs`、`src/Player/PlayerSaveCheckpointComponent.cs`。
- 已落地字段：死亡/复活/观战快照和 `LastSavedBinaryTimestamp`。
- 依赖缺口：现有 lifecycle aggregate、规则、死亡效果、Combat/Items、HitTile、观战 registry、clock 和 persistence I/O 未接入。

## 5.5 C05 `PlayerSpawnAndReturn`

**设计状态：** `proposed`。出生坐标和 Potion of Return 路由都属于玩家到世界位置的短期关系，但它们的资格判断、世界定位、传送提交和网络快照不是同一职责。优先复用现有 NLTX `PlayerSpawnPointComponent`/`PlayerSpawnPointState` 作为兼容模型，不再创建第二个出生点 owner。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerSpawnPointComponent` | `src/Player/PlayerSpawnPointComponent.cs` | 保存个人出生点关系及 Version4 `SpawnX/Y` 兼容坐标 | `SpawnPointChangedCommand` -> committed spawn snapshot | `IWorldSpawnQuery`、`IPlayerSpawnPersistencePort` |
| `PlayerReturnRouteComponent` | `src/Player/PlayerReturnRouteComponent.cs` 或保留在现有 SpawnPoint 边界内 | 原始使用位置和返回家位置的成对可选路由 | `ReturnRouteStarted/cleared` -> route snapshot | `IPotionOfReturnRoutePort`、C03 teleport commit |
| `PlayerSpawnSelectionQuery` | `src/Player/PlayerSpawnSelectionQuery.cs` | 纯计算 SpawnX/Y、队伍出生和世界出生的优先级及有效性 | spawn snapshot + world snapshot -> tile/position candidate | 无 Tile 写入、无网络和随机副作用 |
| `PlayerSpawnNetworkAdapter` | `src/Player/PlayerSpawnNetworkAdapter.cs` | 编解码 type 12 的 short 坐标和 type 13 的两点 route 位 | network bytes -> spawn/route commands and projection | `INetworkTransport`；不直接改 Component |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 1132 | `SpawnX: int` | `PlayerSpawnPointComponent.SpawnX`，兼容字段；与 `SpawnY` 成对解释 | `Spawn` 仅在 X/Y 都大于等于 0 时调用 `Spawn_SetPosition`；type 12 网络读写为 `Int16`；角色/世界存档 writer 未闭合 | authoritative spawn-route input candidate；坐标校验/网络窄化是 Adapter | `Player.cs:1895,21854-21858`; `NetMessage.cs:431`; `MessageBuffer.cs:609`; partial persistence |
| 1133 | `SpawnY: int` | `PlayerSpawnPointComponent.SpawnY`，与 `SpawnX` 原子消费 | 与 X 同一 Spawn 事务读取；任一坐标为负时不能半应用个人点；type 12 使用 `Int16` | authoritative spawn-route input candidate；溢出和非法 tile 需 Query 拒绝 | `Player.cs:1897,21854-21866`; `NetMessage.cs:431-432`; partial |
| 1134 | `PotionOfReturnOriginalUsePosition: Vector2?` | `PlayerReturnRouteComponent.OriginalUsePosition` | type 13 bits byte 2 bit 6 为 true 时与 HomePosition 一起写/读；bit 未置位时两者都清 null；Version4 当前证据未定位 Item 使用时的赋值入口 | paired transient route; network snapshot/persistence format partial | `Player.cs:1899`; `NetMessage.cs:470,492-495`; `MessageBuffer.cs:708-716`; writer/consumer missing |
| 1135 | `PotionOfReturnHomePosition: Vector2?` | `PlayerReturnRouteComponent.HomePosition` | 必须与 OriginalUsePosition 成对存在；网络清除语义同上；返回目标的合法性和消费后清除仍需证据 | paired transient route; world/teleport effect boundary | `Player.cs:1901`; `NetMessage.cs:470,492-495`; `MessageBuffer.cs:708-716`; partial |

### 状态转换与副作用顺序

1. `SpawnCommand` 先由 `PlayerSpawnSelectionQuery` 计算候选：只有 `SpawnX >= 0 && SpawnY >= 0` 才使用个人坐标；否则按 `teamBasedSpawnsSeed` 选择队伍出生，最后回退世界出生。候选必须通过 tile/区域有效性 Query，Query 不直接清理世界或移动实体。
2. `SpawnCommitSystem` 原子提交位置、active/dead 交接和基础速度/液体重置；C04 负责 dead/respawn 生命周期，C05 只负责位置与返回路由。移动提交后的 section、碰撞扫描、视觉和网络输出通过明确 ports 发生。
3. `PotionOfReturn` 路由只有“两点同时存在”才可提交给 C03 teleport owner；网络 type 13 的 presence bit 是成对快照边界，收到 absence 时同时清除两点，不能只清一个。路线使用/取消的旧源码入口未定位，不伪造 clear 时机。
4. type 12 的 `SpawnX/Y` 在网络边界以 signed 16-bit 读写；在兼容 Adapter 中先做范围和非法值检查，再交给 component，不能把 wire truncation 当成有效坐标。type 13 的两个 Vector2 通过 presence bit 一起编码。
5. 个人出生点、队伍出生和世界出生的优先级由显式 scheduler/Query contract 固定；文件顺序不能改变优先级。C03/Teleportation 只接收已验证目标，不能从 C05 内部直接调用网络或粒子。

### 不变量、兼容和 owner

- `SpawnX` 与 `SpawnY` 要么一起有效、一起用于 Spawn，要么整体回退；不能产生半坐标。`-1` 保留“无个人出生点”的兼容语义，但存档/网络版本尚未闭合。
- Return route 的两个 Vector2 只能成对提交/清除；`HasReturnRoute` 的现有 NLTX 语义可复用，但不证明 Version4 的 Item 使用、持久化或消费协议。
- 不把 `SpawnX/Y` 解释成完整的 `PersonalSpawnTile`、床/队伍/世界出生数据；现有 `PersonalSpawnTile` 是迁移兼容 richer model，必须有字段级转换和回滚条件。
- C04 owns dead/respawn state; C03/Teleportation owns actual teleport transaction/effects; WorldGen/TeamSpawn owns fallback selection; C05 不宣布这些跨分区 owner。
- Version4 当前源码对 Potion of Return 直接赋值入口存在 evidence-gap；只能确认 type 13 的 presence/clear/network projection，不可臆造使用算法或 save bytes。

### C05 focused verifier 计划

1. `SpawnSelectionPrecedence`: X/Y 成对有效、任一负值、非法区域、队伍出生、世界出生和 context 的选择顺序。
2. `SpawnNetworkRoundTrip`: type 12 short 坐标边界、respawn/death companion fields 的 adapter 分离、拒绝截断/越界值。
3. `ReturnRoutePairing`: 两点成对存在、type 13 bit set/clear、使用/取消未知路径的 no-guess 行为和 C03 handoff。
4. `SpawnLifecycleHandoff`: C04 dead reset、C05 position commit、movement/section/visual/network side effects 的唯一 writer 和回滚。

#### C05 检查点

- `completedComponents`: C01-C05 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C06 `PlayerContainerAndWorldAnchor`。
- 实现：`implemented`；验证：`not-run`。
- 实际源码：复用并更新 `src/Player/PlayerSpawnPointComponent.cs`。
- 已落地派生状态：`HasSpawnCoordinates`、`HasReturnRoute`。
- 依赖缺口：Spawn selection、type 12/13 adapter、Potion of Return writer、persistence 和 C04/C03 handoff 未接入。

## 5.6 C06 `PlayerContainerAndWorldAnchor`

**设计状态：** `proposed`。C06 的 17 个成员分别属于容器打开关系、外部投射物引用、TileEntity 锚点、门/商店/眼睛表现、每 tick 世界交互 scratch 和装备派生能力。`petting`、`sitting`、`sleeping` 的字段覆盖保留在权威分区表中，但 owner 明确转交 C10-C12，本检查点不重复建立它们的组件。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerContainerRelationComponent` | `src/Player/PlayerContainerRelationComponent.cs` | 当前/上一个 chest 的玩家关系和切换记忆，不保存 chest 内容 | `OpenContainer/CloseContainer` command -> relation snapshot | `IContainerRegistryQuery`、Items/WorldStorage commit port |
| `PlayerTrackedContainerLinkComponent` | `src/Player/PlayerTrackedContainerLinkComponent.cs` | Piggy Bank/Void Lens 外部 projectile link 的稳定 owner/identity/type 兼容快照 | link set/clear/recover -> network projection | `IProjectileRegistryQuery`、type 142 adapter |
| `PlayerTileEntityAnchorComponent` | `src/WorldInteraction/PlayerTileEntityAnchorComponent.cs` | 玩家到 TileEntity 的 entity ID/坐标关系 | anchor command -> anchor snapshot | `ITileEntityRegistryQuery`、type 122 network adapter |
| `PlayerTileInteractionScratch` | `src/WorldInteraction/PlayerTileInteractionScratch.cs` | 每 tick `TouchedTiles` 和世界遮挡/交互临时输入 | collision scan -> immutable interaction snapshot | `ITileQuery`; 不持久化 |
| `PlayerTileInteractionCapabilityComponent` | `src/Player/PlayerTileInteractionCapabilityComponent.cs` | 三个 tile accessory 能力的当前 tick 派生快照 | equipment/reset effects -> committed capability snapshot | Items/Equipment owner integration-review |
| `PlayerShoppingProjection` | `src/Player/PlayerShoppingProjection.cs` | 当前 NPC 购物价格/幸福度的单向表现/资格输出 | talk-NPC snapshot -> shopping projection | `IShopSettingsPort`、NPC/UI/Achievement adapters |
| `PlayerWorldInteractionAdapter` | `src/WorldInteraction/PlayerWorldInteractionAdapter.cs` | `DoorOpeningHelper`、achievement cooldown 等外部 helper 兼容边界 | movement/teleport/achievement commands -> effects | Door/WorldInteraction/Achievement owner review |
| `PlayerInteractionPresentationAdapter` | `src/Client/PlayerInteractionPresentationAdapter.cs` | `PlayerEyeHelper` 眼睛帧等表现状态的输出 | player snapshot -> presentation payload | P11 Presentation；无权威写入 |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 1310 | `lastChest: int` | `PlayerContainerRelationComponent.PreviousChestId` | `ChestChangeEvents` 在关闭/切换时读取旧 ID，执行 Mimic/GasTrap/glow effect 后更新为当前 `chest`；死亡/实体清理要回到空值 | transition memory；Chest/NPC/Projectile effects 不进入 Component setter | `Player.cs:2264,17824-17842`; full open/close closure partial |
| 1311 | `piggyBankProjTracker: TrackedProjectileReference` | `PlayerTrackedContainerLinkComponent.PiggyBankProjectile` | Player 初始化/clear；type 142 写 owner/identity/type；MessageBuffer 读后按 registry 找匹配 Projectile，找不到则 clear | external relation + network projection；不保存 projectile 对象本身 | `Player.cs:2266,26571-26572`; `TrackedProjectileReference.cs:5-72`; `NetMessage.cs:1596-1602`; partial recovery semantics |
| 1312 | `voidLensChest: TrackedProjectileReference` | `PlayerTrackedContainerLinkComponent.VoidLensProjectile` | 与 Piggy Bank 独立设置、清理和 type 142 编解码；不能共享一个 link 或只用 local index | external relation + network projection；Items/Projectile/WorldStorage seam | `Player.cs:2268,26571-26572`; `TrackedProjectileReference.cs:5-72`; `MessageBuffer.cs:3213-3217` |
| 1313 | `chest: int` | `PlayerContainerRelationComponent.CurrentChestId`，兼容 slot/index | open request 验证未被占用后写；close 写 `-1`；死亡更新清 `-1`；type 80 传播当前索引；内容由 Chest/Items owner 管理 | authoritative player-to-container relation; network/world effect | `Player.cs:2270,10033-10035,17828-17841`; `MessageBuffer.cs:1472-1535`; `NetMessage.cs:1173-1176` |
| 1319 | `petting: PlayerPettingInfo` | `deferred -> C10 PlayerPettingComponent`；C06 只保留兼容转接记录 | Tick/StopVanityActions 读写目标状态；目标验证涉及 NPC/Projectile/Mount registry | external entity relation；不得在 C06 重复建模 | `Player.cs:2284,15725,19705-19717`; `PlayerPettingInfo.cs:5-78`; C10 owner |
| 1320 | `sitting: PlayerSittingHelper` | `deferred -> C11 PlayerSittingComponent`；C06 不拥有坐姿 | Tick/StopVanityActions、网络 bit 和座椅 Tile 查询由 C11 处理 | rest relation/presentation effect；不得在 C06 复制 | `Player.cs:2286,15726,19705-19710`; `MessageBuffer.cs:720`; C11 owner |
| 1321 | `sleeping: PlayerSleepingHelper` | `deferred -> C12 PlayerSleepingComponent`；C06 不拥有睡眠 | Tick/StopVanityActions、网络 bit、床面查询和旋转由 C12 处理 | rest relation/presentation effect；不得在 C06 复制 | `Player.cs:2288,15727,19705-19710`; `MessageBuffer.cs:725`; C12 owner |
| 1322 | `eyeHelper: PlayerEyeHelper` | `PlayerInteractionPresentationAdapter.EyeProjection` | 每 tick 根据盲目、受伤、睡眠、天气等玩家快照计算 EyeFrame；渲染读取 `EyeFrameToShow`；没有 P04 权威业务 writer | presentation-only mutable helper；状态不持久化、不进权威 Query | `Player.cs:2290,15728`; `PlayerEyeHelper.cs:3-154`; P11 integration-review |
| 1323 | `tileEntityAnchor: PlayerInteractionAnchor` | `PlayerTileEntityAnchorComponent.Anchor` | type 122 通过 TileEntity registry 验证后 Set；-1 或死亡/失效时 Clear；坐标与 entity ID 必须成对一致 | external world relation; registry lookup/network send 是 effect | `Player.cs:2292,10034-10035,26528`; `PlayerInteractionAnchor.cs:3-55`; `MessageBuffer.cs:3001-3016` |
| 1324 | `doorHelper: DoorOpeningHelper` | `PlayerWorldInteractionAdapter.DoorOpening` | 移动/速度逻辑允许一段时间内开门；初始化构造；其内部状态不泄露为 P04 通用 Component | mutable world helper；Door/Tile collision effect boundary | `Player.cs:2294,12775-12780,26528-26530`; full helper ownership partial |
| 1325 | `currentShoppingSettings: ShoppingSettings` | `PlayerShoppingProjection.CurrentSettings` | `talkNPC == -1` 时为 `NotInShop`；否则由 `Main.ShopHelper.GetShoppingSettings` 计算；低价格调整触发 achievement | derived projection; NPC registry/shop/UI/achievement effects | `Player.cs:2296,3387-3397`; `ShoppingSettings.cs:1-13`; not authoritative inventory state |
| 1372 | `TouchedTiles: List<Point>` | `PlayerTileInteractionScratch.TouchedTiles` | `UpdateTouchingTiles` 每 tick Clear 后由 Collision 重新生成/替换；`LookForTileInteractions` 只读遍历；快照对外需防御性复制 | per-tick scratch; collision/tile reads only, no save/network | `Player.cs:2391,19778-19783,26822-26854`; mutable list reader closure partial |
| 1381 | `equippedAnyTileRangeAcc: bool` | `PlayerTileInteractionCapabilityComponent.HasTileRangeAccessory` | `ResetEffects` 清 false，装备/物品扫描按内容设置；tile range Query 读取；不作为独立持久化事实 | derived equipment capability; Items/Equipment writer | `Player.cs:2410,6918-6920,8902-8904,8914-8925`; equipment integration-review |
| 1382 | `equippedAnyTileSpeedAcc: bool` | `PlayerTileInteractionCapabilityComponent.HasTileSpeedAccessory` | 与 range 同一 ResetEffects/装备扫描周期；建造/挖掘 speed 计算读取 | derived equipment capability; no direct network/save writer | `Player.cs:2412,6918-6920,8897-8925`; partial |
| 1383 | `equippedAnyWallSpeedAcc: bool` | `PlayerTileInteractionCapabilityComponent.HasWallSpeedAccessory` | Reset 后由装备扫描设置；wall speed 计算读取；不能与 tile speed 合并成未区分能力 | derived equipment capability; Items/WorldInteraction seam | `Player.cs:2414,6918-6920,8909-8925`; partial |
| 1384 | `behindBackWall: bool` | `PlayerTileInteractionScratch.IsBehindBackWall` 或 P11-facing occlusion projection | Tile/wall 查询在表现和 EyeHelper 读取；是当前环境派生值，不是玩家配置；更新时间和完整 writer 未闭合 | derived world occlusion; tile read/presentation effect | `Player.cs:2417`; `PlayerEyeHelper.cs:114-117`; full reader/writer closure partial |
| 1385 | `_funkytownAchievementCheckCooldown: int` | `PlayerWorldInteractionAdapter.FunkyTownAchievementCooldown` | Teleport/Spawn 将其设为 `100`；当前 Version4 搜索只确认声明和两处写入，递减/消费未定位；不把它当普通 Player timer | achievement/effect adapter; no authoritative save/network claim | `Player.cs:2419,21737-21738,21801-21804`; reader/clear missing |

### 状态转换与副作用顺序

1. `OpenChestCommand` 先通过 container registry 判断索引、占用和权限，再提交 `CurrentChestId`；type 80 是单向网络 projection。`ChestChangeEvents` 在 current/previous transition 上执行 Mimic/GasTrap/glow effect 后再更新 `PreviousChestId`，不把 Chest 内容写入 Player component。
2. Tracked projectile link 只保存 owner/identity/type/local index 的兼容快照；恢复时先查 registry 并验证 identity/type，找不到则 clear。序列化 adapter 不能把失效 link 重新声明为有效容器。
3. TileEntity anchor 的 Set/Clear 由明确 command 进入，registry 查找和 type 122 广播在 adapter；死亡、实体禁用和 TileEntity 删除必须清 anchor。坐标、ID 和 `InUse` 不允许由纯 Query 改写。
4. `TouchedTiles` 在碰撞扫描阶段清除/重建，之后交互系统只消费不可变快照；对外不返回可变 `List<Point>`。tile accessory flags 在 ResetEffects 后由 Equipment/Items 重新派生，不能手工与持久化状态双写。
5. Shopping、Door、Eye 和 achievement helper 保持外部 adapter/projection。`petting/sitting/sleeping` 的调用仅记录为 C10-C12 的交接，不在 C06 的 scheduler 中插入第二套休息/宠物逻辑。

### 不变量、兼容和 owner

- `CurrentChestId` 与 `PreviousChestId` 具有不同语义；不能用一个字段替代 transition memory，也不能让死亡只清 current 而遗留可触发旧 chest effect 的 previous 状态。
- 两个 `TrackedProjectileReference` 必须独立、可清除、按 owner/identity/type 匹配；local index 不是持久化身份。type 142 的字节格式和恢复失败行为由 adapter 保留。
- `PlayerInteractionAnchor` 的 entity ID 与坐标必须成对更新；TileEntity registry/网络 side effect 不能进入纯 Query。
- C10-C12 负责 `petting/sitting/sleeping`，P11 负责 Eye/视觉，Items/WorldStorage 负责容器内容/持久化，WorldInteraction 负责 Tile/Door；C06 不宣布跨分区最终 owner。
- capability flags、behindBackWall、TouchedTiles 和 achievement cooldown 都是派生/临时或 effect-adjacent 数据，除非未来证据闭合，不写入 Player 存档或网络 authority。
- 现有 `PlayerContainerRef`、`PlayerInventoryComponent` 和 `PlayerRest*` 仅作为 NLTX 局部兼容证据，不证明 Version4 的 chest/TileEntity/projectile relation 已完成迁移。

### C06 focused verifier 计划

1. `ContainerRelationTransition`: open/close/switch/death clear、previous/current chest effect ordering、type 80 projection 和 duplicate owner 检查。
2. `TrackedProjectileRecovery`: owner/identity/type round-trip、local index reuse、missing/incorrect projectile clear 和 type 142 byte compatibility。
3. `TileEntityAnchorProtocol`: set/clear/invalid/occupied entity、坐标一致性、type 122 广播和死亡/删除清理。
4. `InteractionScratchIsolation`: TouchedTiles per-tick replacement、defensive snapshot、tile accessory ResetEffects/derive 和 behind-wall reader boundary。
5. `DeferredHelperOwnership`: C10-C12 helper 不在 C06 重复写入，Eye/Shop/Door/Achievement effects 只经 adapter/projection。

#### C06 检查点

- `completedComponents`: C01-C06 已写入设计与执行文档。
- `currentComponent`: C07 `PlayerPortalAndTargeting`。
- 实现：`partial`；验证：`not-run`。
- 实际源码：`PlayerTrackedContainerLinkComponent.cs`、`PlayerTileInteractionCapabilityComponent.cs`、`PlayerTileEntityAnchorComponent.cs`；`PlayerContainerRelationComponent` 由 P09 会话先行落地，本次未修改。
- 依赖缺口：P09 的共享组件当前只拥有 bank/void-vault 关系，未闭合 P04 chest current/previous 字段；scratch、容器内容、registry、network 和 shop/door/eye 边界未实现。

## 5.7 C07 `PlayerPortalAndTargeting`

**设计状态：** `proposed`。本组同时包含两种每 tick cache、Portal/传送视觉交接、Mount 专属计数、召唤物目标关系、抓钩坐标 scratch 和 Combat 随机效果。cache 不持久化，目标不等于裸 slot，Portal 状态不等于 C03 的视觉 timer；按访问模式分别建模。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerOwnedProjectileCache` | `src/Projectile/PlayerOwnedProjectileCache.cs` | 每 tick 从 Projectile registry 重建按 type 计数及相关最高伤害派生值 | projectile snapshot -> immutable cache | `IProjectileRegistryQuery`；P07/P12 owner review |
| `PlayerNpcAggroSuppressionCache` | `src/NPC/PlayerNpcAggroSuppressionCache.cs` | 每 tick 从装备/效果输入生成 NPC type 抑制集合 | equipment/effect snapshot -> NPC targeting query | `INpcTargetingQuery`；不保存数组引用 |
| `PlayerPortalTraversalComponent` | `src/Teleportation/PlayerPortalTraversalComponent.cs` | Portal color、physics 时间/标志和 Pylon 样式的短期交接状态 | portal/teleport command -> committed traversal snapshot | C03 teleport commit、Movement/Mount/Pylon adapters |
| `PlayerMinionTargetComponent` | `src/Combat/PlayerMinionTargetComponent.cs` | 召唤物休息点和 NPC target 的关系/投影 | target command -> minion snapshot/network projection | `INpcRegistryQuery`、P07/P12 owner review |
| `PlayerGrapplingBlacklistScratch` | `src/Movement/PlayerGrapplingBlacklistScratch.cs` | 抓钩本轮禁止 tile 集合的隔离 scratch | grapple/movement command -> read-only tile set | Movement/Grapple owner；不持久化 |
| `PlayerBeeCombatEffectAdapter` | `src/Combat/PlayerBeeCombatEffectAdapter.cs` | 将 `makeStrongBee` 的随机蜂伤害修正留在 Combat effect boundary | bee type/damage query -> combat result | 显式 `IRandomSource`；Combat owner review |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 1363 | `ownedProjectileCounts: int[]` | `PlayerOwnedProjectileCache.CountsByProjectileType` | ResetEffects/ResetProjectileCaches 清零；UpdateProjectileCaches 遍历 active、owner 匹配的最多 1000 个 Projectile 按 type 累加；Buff/装备逻辑读取 | derived per-tick cache；Projectile registry 读取，不持久化/网络复制 | `Player.cs:2372,4875-5263,6269-6314,10770-10775`; capacity `ProjectileID.Count`; partial full reader closure |
| 1364 | `npcTypeNoAggro: bool[]` | `PlayerNpcAggroSuppressionCache.SuppressedTypes` | ResetEffects 和 UpdateDead 清空；装备 type 3090 设置固定 NPC type 集合；NPC target/aggro 读取；每 tick 重新派生 | derived per-tick cache；NPC reads are cross-domain, no save/network | `Player.cs:2374,8332-8357,10055-10058,10770-10773`; `NPC.cs:23701-23911,64234-64287` |
| 1365 | `lastPortalColorIndex: int` | `PlayerPortalTraversalComponent.LastPortalColorIndex` | Teleport style 4 从 extraInfo 写；Teleport visual style 4 读取并调用 `PortalHelper.GetPortalColor`；非 Portal 传送不得误改 | short-lived portal visual/physics input；P11/Portal effect | `Player.cs:2376,9901-9912,21751-21755,21775-21780`; C03 handoff partial |
| 1366 | `_portalPhysicsTime: int` | `PlayerPortalTraversalComponent.PortalPhysicsRemainingTicks` | Update 递减；PortalPhysicsFlag 或选中 item 3384 时设置 30；`PortalPhysicsEnabled` 读取且受 Mount.Active 影响；死亡清 0 | movement physics runtime state；不持久化；Movement/Mount owner review | `Player.cs:2378,3123-3132,9957,15292-15295,15313-15321` |
| 1367 | `portalPhysicsFlag: bool` | `PlayerPortalTraversalComponent.PortalPhysicsRequested` | Style 4 Teleport 设置 true；落地或 jump 时清 false；随后驱动 physics time refresh | transient command/flag；Teleport/Movement seam | `Player.cs:2380,15311-15318,21775-21780`; ordering must be explicit |
| 1368 | `lastTeleportPylonStyleUsed: int` | `PlayerPortalTraversalComponent.LastPylonStyle` | Teleport style 9 写 extraInfo；UpdateTeleportVisuals style 9 读取并调用 Pylon dust effect；不是一般 teleportStyle 的替代 | Pylon visual projection; C03/WorldInteraction integration-review | `Player.cs:2382,9914-9921,21756-21760` |
| 1369 | `MountFishronSpecialCounter: float` | `PlayerMountPortalEffectAdapter.FishronSpecialRemaining`，`crossSubsystemOwner: Mount/Movement integration-review` | Mount.cs 在生命/雨/风状态写 60/420；Player update 递减；UpdateDead 清 0；移动/能力 Query 读取 | mount-specific transient effect；不归 P04 portal component | `Player.cs:2384,9957-9958,15288-15291`; `Mount.cs:4295-4303` |
| 1370 | `MinionRestTargetPoint: Vector2` | `PlayerMinionTargetComponent.RestTargetPoint` | minion target command 写；`HasMinionRestTarget` 以非 Zero 派生；Projectile AI 读取；type 99 网络读写 | external minion projection/relationship; Vector2 不是 NPC entity ID | `Player.cs:2386,3151-3153`; `NetMessage.cs:1340-1343`; `MessageBuffer.cs:2766-2771`; P07/P12 review |
| 1371 | `MinionAttackTargetNPC: int` | `PlayerMinionTargetComponent.AttackTarget` + NPC registry Adapter | target command/network type 115 写 slot；Projectile AI 读取并验证 NPC 可追踪性；无效/死亡 target 清理策略需显式 | relationship plus network slot; 不把裸 int 当稳定 NPC identity | `Player.cs:2388`; `NetMessage.cs:1344-1347`; `MessageBuffer.cs:2774-2779`; P07/P12 owner partial |
| 1379 | `_blackListedTileCoordsForGrappling: HashSet<Point>` | `PlayerGrapplingBlacklistScratch.Tiles` | 抓钩/Movement 逻辑写入候选禁止点；RemoveAllGrapplingHooks/ClearGrapplingBlacklist 清空；Teleport/Ghost 间接清理；当前源码未定位所有写入入口 | mutable per-interaction scratch；不得持久化或从 Query 泄露可写集合 | `Player.cs:2406,3841-3846,19743-19749,21739-21743`; writer closure partial |
| 1380 | `makeStrongBee: bool` | `PlayerBeeCombatEffectAdapter.StrongBeeRoll`，`crossSubsystemOwner: Combat` | `beeType` 读 strongBees 后用 Main.rand 二选一写；beeDamage/beeKB 读并再次随机伤害；Item/Combat effect 生命周期 | random Combat modifier；必须注入随机源，不能成为 Portal/Player persistent state | `Player.cs:2408,6415-6443`; Main.rand/effect boundary |

### 状态转换与副作用顺序

1. 每 tick 的 `ResetEffects` 先清 `ownedProjectileCounts`、`npcTypeNoAggro` 和相关 derived flags，再由 equipment/buff pass 写入抑制集合，之后 `UpdateProjectileCaches` 从只读 Projectile registry 重建计数。Cache Query 只返回不可变快照，不能自己刷新或写回数组。
2. NPC target system 读取 `PlayerNpcAggroSuppressionCache`，但不反向改变它；数组长度跟随显式 registry version，不能复用上一个 registry 的旧索引。死亡路径同样清空 cache，避免死玩家残留 aggro immunity。
3. Portal/Teleport commit 先提交颜色/Pylon/physics facts，再交给 C03 的传送事务和 P11/WorldInteraction effects；`portalPhysicsFlag` 在 grounded/jump 输入下清除，`_portalPhysicsTime` 由 tick 递减并按规则刷新。视觉 adapter 不能写 Portal physics 权威状态。
4. Minion rest point 与 attack target 通过各自 command/网络 projection 更新。NPC slot 需经过 registry Query 验证，失效 target 清理是显式 policy；Projectile AI 只读快照。type 99 和 type 115 不能共用一个 payload。
5. Grapple blacklist 是 interaction scope scratch，Teleport/RemoveHooks/Clear/Ghost 的清理顺序必须由 Movement scheduler 固定。`makeStrongBee` 的随机结果和伤害调整留在 Combat port，日志、渲染和 Portal Query 不得读取/修改它。

### 不变量、兼容和 owner

- `ownedProjectileCounts` 每 tick 由零开始重建，不能增量累积；每个 active owned projectile 恰好计一次，类型索引必须在 registry 范围内。
- `npcTypeNoAggro` 只表达当前 tick 的抑制能力；任何 ResetEffects 路径缺失都会造成跨 tick 泄漏，必须由 verifier 覆盖。
- `lastPortalColorIndex`、`lastTeleportPylonStyleUsed` 和 C03 `teleportStyle` 是不同语义：Portal color/Pylon style 只是 effect metadata，不能覆盖普通样式或 pending ack。
- `MinionAttackTargetNPC` 是网络 slot 兼容值，必须与 NPC registry identity/type/active/can-chase 校验；`MinionRestTargetPoint == Vector2.Zero` 才代表无休息点，不能把任意零坐标外推为 target entity。
- `MountFishronSpecialCounter`、minion target、grapple blacklist 和 `makeStrongBee` 分别交接 Mount/Movement, P07/P12, Grapple and Combat；C07 不宣布其最终跨分区 owner。
- 现有 NLTX Portal/Teleport cooldown 组件、Projectile/Relationship 类型只能作为局部兼容证据，不能直接证明 Version4 cache reset 或网络 type 99/115 已等价。

### C07 focused verifier 计划

1. `ProjectileCacheRebuild`: reset-before-rebuild、owner/type filtering、duplicate count、registry resize、dead-player clear 和 immutable snapshot。
2. `NpcAggroCacheLifetime`: equipment-derived type set、NPC readers、ResetEffects/UpdateDead 清理和跨 tick leakage。
3. `PortalTraversalHandoff`: style 4/9 metadata、physics flag/time decrement/reset、C03 commit and Pylon/Portal effect ordering。
4. `MinionTargetProtocol`: type 99/115 round-trip、invalid NPC slot/type/active handling、rest-point zero semantics和Projectile read-only boundary。
5. `GrappleAndBeeIsolation`: blacklist clear at hook removal/teleport/ghost, missing writer evidence, and injected random Combat modifier with no persistent Player write.

#### C07 检查点

- `completedComponents`: C01-C07 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C08 `PlayerItemActionTiming`。
- 实现：`implemented`；验证：`not-run`。
- 实际源码：`src/Teleportation/PlayerPortalTraversalComponent.cs`、`src/Combat/PlayerMinionTargetComponent.cs`。
- 依赖缺口：projectile/NPC caches、Mount/Movement scratch、Combat random effect、registry validation、type 99/115 network 和 C03 handoff 未实现。

## 5.8 C08 `PlayerItemActionTiming`

**设计状态：** `proposed`。本检查点覆盖权威报告的 12 个成员：物品动作的五元运行时计时、三个由物品规则和药水石修正得到的延迟输入、Wiring 操作冷却、移动/矿车跌落记录，以及一个属于全局输入状态的静态投射物交互阻断值。它们不能机械合并成一个大型 Player component：访问者、更新原因和生命周期分别落在 Player、Items、Wiring、Movement/Minecart 与 Client 输入边界。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerItemActionTimingComponent` | `src/Player/PlayerItemActionTimingComponent.cs` | `itemAnimation`、`itemAnimationMax`、`itemTime`、`itemTimeMax`、`toolTime` 的原子动作时序快照 | `ItemUseCommand`/tick -> timing snapshot | Player ItemCheck owner system；Items definition/query |
| `PlayerItemDelayDefinitionQuery` | `src/Items/PlayerItemDelayDefinitionQuery.cs` | 从 `Item` 静态规则和当前药水石修正计算三个有效 delay frame 值 | player effect snapshot -> immutable delay definition | `IItemDelayDefinitionPort`；不写 countdown |
| `PlayerWiringCooldownHandoff` | `src/WorldInteraction/PlayerWiringCooldownHandoff.cs` | 保留 Wiring 交互冷却在 Player 兼容字段上的输入限制与清除交接 | wire command/network input -> cooldown decision/effect | Wiring owner、`INetworkInputAdapter`；P04 不宣布最终 writer |
| `PlayerFallTrackingAdapter` | `src/Movement/PlayerFallTrackingAdapter.cs` | 将 `fallStart` 与 Minecart 所需的 `fallStart2` 作为移动/物理兼容桥接 | movement transition -> fall snapshot; Minecart read | Movement/Physics owner、`IMinecartCollisionPort` |
| `PlayerProjectileInteractionBlockAdapter` | `src/Client/PlayerProjectileInteractionBlockAdapter.cs` | 适配 static `BlockInteractionWithProjectiles` 的短期全局输入锁 | mouse input/release -> global block effect | Main/client input loop；不是玩家实体 component |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 1309 | `wireOperationsCooldown: int` | `PlayerWiringCooldownHandoff.CooldownFrames`，兼容字段 | Player update 每 tick 递减；ItemCheck 对 type 3611/3625 拒绝使用；MessageBuffer 在对应网络消费后清零；常规设置入口尚未由 focused search 闭合 | Wiring interaction restriction；网络输入和物品使用是 adapter/effect boundary | `Player.cs:2262,10762-10764,25219-25222`; `MessageBuffer.cs:2896`; partial writer closure |
| 1314 | `fallStart: int` | `PlayerFallTrackingAdapter.FallStartTileY` | Movement、重力、滑行、液体/传送等多个转移点按位置重置；跌落/物理消费的完整读者闭包仍需 verifier；不是持久化玩家配置 | movement physics scratch/transition marker；Collision/Mount/Minecart effects remain external | `Player.cs:2273,13050,14269,21774,21872`; broad writer set, reader closure partial |
| 1315 | `fallStart2: int` | `PlayerFallTrackingAdapter.MinecartFallStartTileY` | `Minecart.TrackCollision` 读取；Spawn 路径以 `fallStart` 成对同步；未发现独立长期 writer | Minecart compatibility input；不独立持久化或网络复制 | `Player.cs:2275,17493,21872-21873`; partial |
| 1316 | `potionDelayTime: int` | `PlayerItemDelayDefinitionQuery.PotionDelayFrames` | 初始化及 `ResetEffects` 从 `Item.potionDelay` 计算，药水石修正乘以 `PhilosopherStoneDurationMultiplier`；当前 `ApplyPotionDelay` 是空实现，实际使用闭包缺证据 | derived item rule input；不是 `potionDelay` buff countdown | `Player.cs:2277,15194-15201,25163`; `Item.cs:70`; partial |
| 1317 | `restorationDelayTime: int` | `PlayerItemDelayDefinitionQuery.RestorationDelayFrames` | 与 potion delay 同一 ResetEffects 周期，从 `Item.restorationDelay` 和药水石修正得到；未定位独立消费入口 | derived item rule input；不直接作为 Player cooldown owner | `Player.cs:2279,15195-15201`; `Item.cs:72`; reader closure partial |
| 1318 | `mushroomDelayTime: int` | `PlayerItemDelayDefinitionQuery.MushroomDelayFrames` | 与前两项同一规则计算，从 `Item.mushroomDelay` 和药水石修正得到；未定位独立消费入口 | derived item rule input；不持久化为动作计时 | `Player.cs:2282,15196-15201`; `Item.cs:76`; reader closure partial |
| 1373 | `itemAnimation: int` | `PlayerItemActionTimingComponent.AnimationRemaining` | `SetItemAnimation`/`SetDummyItemTime`/ItemCheck 写；ItemCheck 在异常值时归零、每 tick 递减，动画结束时清 max；Eye、渲染、ItemCheck、伤害和使用逻辑读取 | authoritative per-player action phase; visual/combat effects consume through projections/ports | `Player.cs:2393,3157,3421-3431,23443-23567`; unique timing owner proposed |
| 1374 | `itemAnimationMax: int` | `PlayerItemActionTimingComponent.AnimationDuration` | 与 `itemAnimation` 成对由 `SetItemAnimation` 写；动画为零时 ItemCheck 清零；`ApplyAttackCooldown` 读取但 `attackCD` 已由 C04 覆盖，C08 不建立第二 owner | action-duration baseline; Combat handoff only | `Player.cs:2395,3430-3431,23462-23464,23961`; C04 `attackCD` boundary |
| 1375 | `itemTime: int` | `PlayerItemActionTimingComponent.UseRemaining` | `SetItemTime`、`SetDummyItemTime` 和 ItemCheck 写；负值归零、ItemCheck 每 tick 递减；release-use、projectile/item effects 读取 | authoritative use sub-timer; ItemCheck command/effect boundary | `Player.cs:2397,3402-3423,23458-23460,23565-23567,25442`; unique timing owner proposed |
| 1376 | `itemTimeMax: int` | `PlayerItemActionTimingComponent.UseDuration` | `SetItemTime` 与 dummy-use 路径写；dummy 路径明确使用 `frames + 1`；focused search 未确认独立 reader，不能把它推断成 animation duration | paired baseline/compatibility value；持久化和网络语义未确认 | `Player.cs:2399,3406-3407,3421-3423`; reader/persistence evidence-gap |
| 1377 | `toolTime: int` | `PlayerItemActionTimingComponent.ToolUseMarker`，或后续 Movement/Items handoff | `ItemCheck_StartActualUse` 对镐/斧/锤及 type 4711 置为 `1`；完整清除、递减和消费者未在 focused search 中闭合 | tool-use marker；不能宣称为独立 countdown 或持久化事实 | `Player.cs:2401,25124-25129`; reader/clear closure partial |
| 1378 | `BlockInteractionWithProjectiles: int` | `PlayerProjectileInteractionBlockAdapter.GlobalReleaseBlock` | static 初始化值为 `3`；Main 输入循环在鼠标右键释放条件下递减；没有每个 Player 实体的 owner | global client/input lock；不得放入 ECS Player entity component 或网络 authority | `Player.cs:2403`; `Main.cs:11216-11218`; static/global boundary confirmed |

### 状态转换与副作用顺序

1. `ItemUseCommand` 先由 ItemCheck 读取当前输入和 Item 定义；`SetItemTime` 原子写 `itemTime/itemTimeMax`，`SetItemAnimation` 原子写 `itemAnimation/itemAnimationMax`，dummy-use 路径按源码保留 `frames + 1` 的 `itemTimeMax` 关系。Query 不得直接写这些值。
2. ItemCheck 在每 tick 先把负的 animation/time 修正为零，在 animation 为零时清 `itemAnimationMax`，随后执行 use/reuse 决策并递减 animation/time。`releaseUseItem`、`pendingItemReuse` 和 C09 `ItemCheckContext` 是命令上下文/相邻边界，不并入 C08 组件。
3. `toolTime` 只在实际工具使用入口被置为 `1`；由于当前 Version4 证据没有闭合其清除和读取，设计只记录 marker adapter，不猜测倒计时或 owner。若 verifier 发现其真实消费者属于 Movement/Items，必须通过显式 handoff 调整。
4. 三个 `*DelayTime` 每次 ResetEffects 从 `Item` 定义重算，并可被药水石 multiplier 修正；它们不是 C04 的 `potionDelay` buff countdown，也不能与 `potionDelay` 合并。`ApplyPotionDelay` 当前为空实现是明确 evidence-gap，未来只允许由 Items owner 通过 port 消费。
5. `wireOperationsCooldown` 在 Player tick 中递减，ItemCheck 用它阻断机关物品，网络消费路径可将其清零；网络 decode、Wiring action 和失败/重复语义必须留在 adapter/owner system，不能由纯 Query 改写。
6. `fallStart` 的重置跟随 Movement/Physics 状态转移，`fallStart2` 只作为 Minecart collision 输入并在 Spawn 同步；移动 scheduler 必须固定这些跨域读写顺序，不能依赖文件顺序。
7. static `BlockInteractionWithProjectiles` 由 Main/client 输入循环递减，是全局短期锁；它不能被序列化、广播或复制为玩家实体状态。

### 不变量、兼容和 owner

- `itemAnimation`/`itemAnimationMax` 和 `itemTime`/`itemTimeMax` 是两个不同的成对语义；禁止用一个计时器替代另一个。`itemTimeMax` 的独立 reader 尚未闭合，保留兼容字段而不发明持久化含义。
- ItemCheck 的负值归零和 animation 结束清 max 必须保持；`SetDummyItemTime` 的 `frames + 1` 不能被普通 `SetItemTime` 规则覆盖。
- `attackCD` 已由 C04 标记为 Combat/Items handoff；C08 只提供 animation duration 输入，不创建第二个 attack cooldown writer。
- `potionDelayTime`/`restorationDelayTime`/`mushroomDelayTime` 是规则派生值，`potionDelay` 是 buff 状态倒计时；两组不能合并，也不能在缺少 `ApplyPotionDelay` 证据时声称消费等价。
- `fallStart`/`fallStart2`、`wireOperationsCooldown` 和 static block value 分别交接 Movement/Minecart、Wiring/Network 和 Client input；P04 不宣布跨分区最终 owner。
- 所有网络、鼠标输入、Item registry、时钟/计时推进和 Combat/Movement effect 均通过显式 port/adapter；没有新 C# 类型在本轮创建。

### C08 focused verifier 计划

1. `ItemActionTimerInvariants`: `SetItemTime`、`SetDummyItemTime`、`SetItemAnimation` 的配对写入，负值归零、tick decrement、animation-end 清理和 reuse/release 行为。
2. `ItemDelayDefinitionIsolation`: Item static defaults、药水石修正、三个 delay 的 ResetEffects 生命周期，及 `potionDelay` buff countdown 的非合并证明；`ApplyPotionDelay` 空实现要作为缺口输出。
3. `ToolMarkerClosure`: tool item/type 4711 置位、所有已知清除/消费者检索；在未闭合前不得把 `toolTime` 作为长期计时器。
4. `WiringCooldownHandoff`: ItemCheck 阻断、tick decrement、network clear、初始/常规 writer、重复网络输入和失败语义的唯一 owner。
5. `FallAndGlobalInputIsolation`: Movement/Minecart 的 fallStart/fallStart2 顺序、Spawn 同步、static block 的鼠标释放 decrement，以及无持久化/网络泄漏。

#### C08 检查点

- `completedComponents`: C01-C08 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C09 `PlayerItemCheckContext`。
- 实现：`implemented`；验证：`not-run`。
- 实际源码：`src/Player/PlayerItemActionTimingComponent.cs`。
- 依赖缺口：PlayerUse overlap、delay rules、Wiring cooldown、fall tracking、global input lock、ItemCheck writer 和 C04 attackCD handoff 未接入。

## 5.9 C09 `PlayerItemCheckContext`

**设计状态：** `proposed`。C09 只有一个成员 `SkipItemConsumption`，但它不是持续存在的玩家实体状态：源码将其声明在 `Player.ItemCheckContext` 短生命周期 struct 中，并在 `ItemCheck` 内部按调用创建默认值。当前 Version4 证据没有发现该字段的读取、写入或消费路径，所以只能设计 command/context 边界，不能把它升级成持久化 Component 或假定第三方 Hook 语义。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerItemCheckContext` | `src/Items/PlayerItemCheckContext.cs`（保留 `Terraria.Player` 兼容 public API 的 adapter） | 传递一次 ItemCheck 调用内的消费策略覆盖值 | ItemCheck command -> consumption decision/context | Items consumption commit port；不进入 ECS 持久化查询 |
| `PlayerItemConsumptionDecision` | `src/Items/PlayerItemConsumptionDecision.cs`（仅在消费语义证据闭合后引入） | 将 context 的 skip 意图转换为显式、可验证的消费决策 | Item/use result + context -> decision | `IItemConsumptionPort`；不得由 Query 修改 context |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 429 | `SkipItemConsumption: bool` | `PlayerItemCheckContext.SkipItemConsumption`，短生命周期 command payload；不建立 Component owner | `ItemCheck` 在入口创建 `default(ItemCheckContext)`；当前源码/全 Version4 检索未确认任何 read/write/consume site；只允许在一次 ItemCheck 调用栈内传递 | transient decision input；库存扣减、网络、日志和持久化必须留在 Items/effect port | `Player.cs:282-285,23446-23450`; `rg` closure: declaration and default construction only; evidence partial |

### 状态转换与副作用顺序

1. ItemCheck command 创建局部 context，并将其作为同一次 use evaluation 的可选输入；context 离开调用栈后丢弃，不写入 Player entity snapshot、save 或 network payload。
2. 只有 Items consumption owner 才能根据已确认的 context 计算“是否扣除 stack/资源”；Query 可以返回消费资格或不可变 decision，但不得通过引用修改 `SkipItemConsumption`。
3. 当前 Version4 中 `SkipItemConsumption` 没有可见 consumer，因此本轮不补写一个虚构的 Hook、消耗规则或网络协议。未来若由 mod/hook 或跨分区 command 使用，必须先定义输入来源、优先级、一次性范围、失败/重复语义和唯一 writer。
4. C08 的 item timers、C04 的 `potionDelay`/Combat handoff 与 C09 的 context 是不同边界：context 只影响一次 ItemCheck decision，不成为 cooldown 或 inventory authority。

### 不变量、兼容和 owner

- `ItemCheckContext` 保持短生命周期、不可持久化、不可网络广播；不存在“上一次 ItemCheck 自动沿用 skip”语义。
- `SkipItemConsumption` 的默认值必须保持 `false`，除非未来已证实的 command/adapter 显式设置；不得由查询、日志或渲染路径写入。
- 库存/物品 stack 变更由 Items consumption commit port 负责；C09 不宣布 `Item`、Inventory、Network 或 ModHook 的最终跨分区 owner。
- 当前“只有声明和 default 构造”是 evidence-gap，不等于字段无效，也不允许把未发现的调用点补成实现事实。

### C09 focused verifier 计划

1. `ItemCheckContextLifetime`: 构造、传递、离开 ItemCheck 调用栈后的不可见性，确认没有 Player snapshot/save/network 序列化。
2. `SkipConsumptionClosure`: 全 Version4 读写/调用检索；若发现新 consumer，记录 command 来源、优先级和唯一消费 writer，否则保持 deferred。
3. `QueryNoMutation`: 对 context 和 Item inventory snapshot 的纯查询不得写回 skip 标记或库存 stack。
4. `ConsumptionBoundary`: 在明确 decision 证据后验证正常扣除、skip、失败/重复和 ItemCheck timer 的隔离；未闭合前只运行结构性检查。

#### C09 检查点

- `completedComponents`: C01-C09 已写入设计与执行文档。
- `currentComponent`: C10 `PlayerPetting`。
- 实现：`deferred-no-component`；验证：`not-run`。
- 实际源码：无；`SkipItemConsumption` 保持调用内 payload，未创建 ECS component。
- 依赖缺口：未发现可确认 consumer；Items consumption、mod/hook 输入和失败/重复语义待 evidence closure。

## 5.10 C10 `PlayerPetting`

**设计状态：** `proposed`。C10 的七个旧成员实际表达一个带目标种类的短期关系：NPC、Projectile 或 Mount 三选一，加上交互阶段、相对位置和小型宠物表现标记。旧结构把 `type` 同时作为 NPC/Projectile 类型或 Mount ID 使用，把 `npc`/`proj` 当作数组槽位；拆分后必须显式区分实体命名空间、槽位/identity 与期望类型，不能把裸整数升级成稳定实体 ID。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerPettingComponent` | `src/Player/PlayerPettingComponent.cs` | 保存一次宠物互动的 phase、tagged target reference、offset 和小型宠物标记 | `BeginPetting/StopPetting` command -> immutable relation snapshot | Player vanity-interaction owner system |
| `PlayerPetTargetReference` | `src/Player/PlayerPetTargetReference.cs` | 用显式 kind 表达 NPC slot+expected type、Projectile slot+expected type 或 Mount ID | target selection -> typed reference | `INpcRegistryQuery`、`IProjectileRegistryQuery`、Mount adapter |
| `PlayerPettingTargetQuery` | `src/Player/PlayerPettingTargetQuery.cs` | 只读验证 target active/type/identity 并返回外部 Entity view | relation snapshot -> `Entity?`/validity | registry adapters；Query 不清理关系 |
| `PlayerPettingNetworkAdapter` | `src/Player/PlayerPettingNetworkAdapter.cs` | 映射 type 13 的 `isPetting`/`isPetSmall` bits 和兼容外部状态 | network snapshot -> phase projection | `NetMessage`/`MessageBuffer`; target relation wire contract unresolved |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 373 | `isPetting: bool` | `PlayerPettingComponent.IsPetting` | `StopPettingAnimal` 清 false；`UpdatePettingAnimal` 仅在 true 时校验；type 13 网络发送/接收 bit 4；Player update 每 tick 调用更新 | authoritative interaction phase；网络/表现是 projection | `PlayerPettingInfo.cs:7`; `Player.cs:15725,19712-19729`; `NetMessage.cs:468`; `MessageBuffer.cs:722` |
| 374 | `npc: int` | `PlayerPetTargetReference.NpcSlot`，仅在 kind=NPC 时有效 | NPC 构造器记录 `npc.whoAmI`；TryGetTarget 用该槽位查 `Main.npc`，要求 active 且 type 等于旧 `type`；槽位稳定性和 range/identity policy 未闭合 | external NPC relation; registry read/invalid target is adapter boundary | `PlayerPettingInfo.cs:21-30,54-66`; `Player.cs:19727`; partial identity evidence |
| 375 | `proj: int` | `PlayerPetTargetReference.ProjectileSlot`，仅在 kind=Projectile 时有效 | Projectile 构造器记录 `proj.whoAmI`；TryGetTarget 查 `Main.projectile`，要求 active 且 type 等于旧 `type`；owner/identity/reuse policy 未闭合 | external Projectile relation; registry read, no raw object persistence | `PlayerPettingInfo.cs:32-41,68-78`; `Projectile.cs:38134-38135`; partial |
| 376 | `type: int` | tagged reference 中的 `ExpectedNpcType`、`ExpectedProjectileType` 或 `MountId`，不保留为无标记通用 ID | 三个构造器按不同 kind 写入；NPC/Projectile TryGetTarget 用作 type guard，Mount 分支把它解释成 mount ID；不得跨 kind 复用 | compatibility discriminator; registry/type definitions remain external | `PlayerPettingInfo.cs:21-52,58-78`; overloaded semantics confirmed |
| 377 | `mount: bool` | `PlayerPetTargetReference.Kind == Mount` | Mount 构造器置 true、NPC/Projectile 构造器置 false；TryGetTarget 对 Mount 返回 `null` 且 true，真正的坐骑有效性由 Mount adapter/Player mount 状态验证 | external Mount relation; no fake Entity object from Query | `PlayerPettingInfo.cs:15,29,40,51,67-70`; `Mount.cs:4752-4755` |
| 378 | `offsetFromPet: Vector2` | `PlayerPettingComponent.OffsetFromPet` | NPC/Projectile 构造器保存偏移；UpdatePettingAnimal 用 target.Bottom + offset 做距离判断，方向由 offset X 推导；Mount 构造器为 Zero 并用玩家 direction | relation geometry; position/distance and presentation effects are external queries | `PlayerPettingInfo.cs:17,21-52`; `Player.cs:19727-19737` |
| 379 | `isPetSmall: bool` | `PlayerPettingComponent.IsPetSmall` | 各构造器初始化；Item hold/presentation 读取来选择手臂/缩放表现；type 13 网络发送/接收 bit 5 | presentation/interaction modifier；不是 target identity 或存档 authority | `PlayerPettingInfo.cs:19,21-52`; `Player.cs:24623-24649`; `NetMessage.cs:469`; `MessageBuffer.cs:723` |

### 状态转换与副作用顺序

1. `BeginPetting` 先构造 tagged reference：NPC/Projectile 记录槽位和期望 type，Mount 记录 Mount ID；随后提交 `IsPetting` 和 geometry snapshot。registry 查询只验证资格并返回 view，不持有外部对象引用。
2. Player update 调用 `UpdatePettingAnimal`；phase 为 false 直接返回。phase 为 true 时先验证 target，再检查 NPC 的 `talkNPC` 一致性和距离；随后检查输入、pulley、Mount 冲突和方向。任何失败只通过唯一 `StopPetting` command 清理 phase。
3. `StopPettingAnimal` 当前源码只清 `isPetting`，没有证据表明它会清空旧 slot/type/offset；兼容 adapter 必须让 `IsPetting=false` 时旧 reference 不可被 Query 当作活动关系，并把旧引用清理策略留给 verifier。
4. Mount dismount 会在宠物目标为 Mount 时调用 Stop；`StopVanityActions` 也统一调用 StopPetting、坐起和停止睡眠。C11/C12 共享 rest/vanity 交接，但不得复制 C10 writer。
5. type 13 player sync 只发送/接收 `isPetting` 与 `isPetSmall` 两个 bits；目标引用的建立、验证、广播顺序和失效重放没有在当前证据中闭合，不能假定两个 bit 足以恢复 NPC/Projectile/Mount target。

### 不变量、兼容和 owner

- `npc`、`proj` 和 `mount` 是互斥 target kinds；只有对应 kind 的字段有效。`type` 必须随 kind 解释，不能把 NPC type、Projectile type 和 Mount ID 放进一个无标签实体表。
- NPC/Projectile target 通过 registry 查询验证 active、type 和必要的 identity/version；local slot 复用、缺失 target、错误 type 和 registry resize 都必须有明确失效策略。
- Mount target 的 `TryGetTarget` 返回 null 是源码兼容行为，不等于目标有效；Mount adapter 必须单独检查 mounted state/ID，不得生成虚构 Entity。
- `StopPettingAnimal` 的 phase 清除是唯一 writer；网络 input、Mount dismount、Vanity stop 和 Update failure 都通过该 command 交接，不能让表现层直接写 phase。
- `isPetSmall` 只作为网络/表现输入；offset 距离、方向和输入终止是关系行为，不持久化为通用 Player config。
- 外部 Entity registry、网络 bits、距离/碰撞、音频/粒子和表现更新均在 adapter/port；C10 不宣布 NPC/Projectile/Mount 的最终跨分区 owner。

### C10 focused verifier 计划

1. `PetTargetReferenceValidity`: 三类构造、kind/type 解释、NPC/Projectile slot reuse、active/type/identity mismatch 和 Mount ID 失效。
2. `PettingStopConditions`: target missing、NPC talk target mismatch、distance > 2、input/pulley、mount conflict 和 direction mismatch 的逐项停止行为。
3. `PettingPhaseLifecycle`: begin/stop/StopVanity/Mount dismount 的唯一 phase writer，旧引用在 phase false 时不可见，以及坐姿/睡眠互斥交接。
4. `PettingNetworkProjection`: type 13 bits 4/5 round-trip、目标关系缺失时的 no-guess 行为、重复/迟到状态和广播顺序。
5. `PettingPresentationIsolation`: `isPetSmall`/offset 对 hold-style 的单向输出，Query 不写 registry 或关系组件。

#### C10 检查点

- `completedComponents`: C01-C10 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C11 `PlayerSitting`。
- 实现：`implemented`；验证：`not-run`。
- 实际源码：`src/Player/PlayerPettingComponent.cs`。
- 依赖缺口：raw slots、target identity、registry validation、Mount validity、network adapter 和 vanity system 未实现。

## 5.11 C11 `PlayerSitting`

**设计状态：** `proposed`。C11 将座位关系拆成 phase、tile-derived seat details、几何偏移和外部 stack index；`ChairSittingMaxDistance` 是规则定义而不是每个玩家的可变状态。座位 tile 的 frame 解析和 `Main.sittingManager` 关系查询属于显式 Query/Adapter，不能通过字段或文件顺序隐含调度。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerSittingComponent` | `src/Player/PlayerSittingComponent.cs` | 保存坐姿 phase、`ExtraSeatInfo`、座位偏移和当前 stack index | `SitDown/SitUp` command -> rest snapshot | Player rest owner；与 C12 共享互斥 seam |
| `PlayerSittingRulesDefinition` | `src/Player/PlayerSittingRulesDefinition.cs` | 暴露 `ChairSittingMaxDistance` 等只读座位规则 | rules -> eligibility query | 无实体写入；当前常量消费者需补证据 |
| `PlayerSeatEligibilityQuery` | `src/WorldInteraction/PlayerSeatEligibilityQuery.cs` | 读取 tile type/frame，计算座位位置、朝向、offset 和 `ExtraSeatInfo` | player/tile snapshot -> immutable seat candidate | `ITileReadPort`；不改 Player/registry |
| `PlayerSeatStackAdapter` | `src/WorldInteraction/PlayerSeatStackAdapter.cs` | 适配 `AnchoredEntitiesCollection` 的座位占用、stack index 和清理 | seat candidate/player -> stack effect/index | `ISittingStackPort`；不把 manager 变成 Component |
| `PlayerSittingNetworkAdapter` | `src/Player/PlayerSittingNetworkAdapter.cs` | 映射 type 13 坐姿 bit 和退出广播 | relation snapshot/command -> network projection | `NetMessage`/`MessageBuffer`; enter protocol unresolved |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 380 | `ChairSittingMaxDistance: const int` | `PlayerSittingRulesDefinition.MaxDistancePixels` | `PlayerSittingHelper` 声明为 `40`；当前 Version4 检索未发现消费者，不能假定进入判定一定使用该常量 | immutable rule definition；不进 Entity/save/network | `PlayerSittingHelper.cs:8`; consumer closure missing |
| 381 | `isSitting: bool` | `PlayerSittingComponent.IsSitting` | `UpdateSitting`/offset/lock/regen/presentation 读取；`SitUp` 清 false；MessageBuffer type 13 接收 bit 2；当前源码未检出本地 true writer，进入协议缺失 | authoritative rest phase candidate; network is projection/adapter | `PlayerSittingHelper.cs:10,38-43,81-90`; `Player.cs:3116-3120,15726,19708`; `NetMessage.cs:466`; `MessageBuffer.cs:720`; partial |
| 382 | `details: ExtraSeatInfo` | `PlayerSittingComponent.SeatDetails` | UpdateSitting 从 tile query 接收并写；SitUp reset default；`details.IsAToilet` 驱动 TryToPoop；值对象仅保留座位特征，不保存 Tile 对象 | derived seat capability; gameplay effect through explicit command/port | `PlayerSittingHelper.cs:12,45-74,81-86`; `ExtraSeatInfo.cs:3-6`; `Player.cs:11131-11133` |
| 383 | `offsetForSeat: Vector2` | `PlayerSittingComponent.SeatOffset` | UpdateSitting 写 tile/frame 计算结果；GetSittingOffsetInfo 读取并结合 `player.Directions`；SitUp 清 Zero；表现/位置投影读取 | seat geometry projection; tile reads and position effects external | `PlayerSittingHelper.cs:14,18-34,72,84`; `Player.cs:3302-3305` |
| 384 | `sittingIndex: int` | `PlayerSittingComponent.StackIndex`，兼容外部 manager index | UpdateSitting 调 `Main.sittingManager.AddPlayerAndGetItsStackedIndexInCoords` 写；offset 计算读取；stack >=2 触发 SitUp；SitUp 清为 -1 | external relation/cache index; manager identity and lifetime adapter | `PlayerSittingHelper.cs:16,54-56,74,85`; `Main.cs:1197,3271`; `AnchoredEntitiesCollection.cs:50-55` |

### 状态转换与副作用顺序

1. `PlayerSeatEligibilityQuery` 读取 `(player.Bottom + (0,-2)).ToTileCoordinates()` 对应 tile，先检查 `TileID.Sets.CanBeSatOnForPlayers` 和 active，再按 tile type/frame 计算 target direction、seat position、offset 与 `ExtraSeatInfo`。Query 不写 Player 或 stack manager。
2. 进入坐姿的 `SitDownCommand` 必须在未来证据闭合后按候选、玩家方向/距离和 stack capacity 原子提交 `IsSitting`、offset、details，并由 stack adapter 分配 index。当前源码没有本地 `isSitting=true` writer，不能伪造进入实现。
3. 每 tick `UpdateSitting` 复核 tile candidate；目标无效、任一移动/输入、pulley、mount、方向变化或 stack index >=2 时调用唯一 `SitUp` writer。成功时更新 offset/details，并通过 manager 取得 stack index。
4. `SitUp` 原子清 phase、offset、index 和 details；当 `multiplayerBroadcast` 且玩家为本地玩家时发送 type 13。网络 adapter 负责广播副作用，组件 setter 不直接调用 NetMessage。
5. `isLockedToATile`、生命再生、厕所效果和表现 offset 只消费已提交快照；C12 睡眠共享互斥 rest seam，但不得从 C11 直接写睡眠字段。`StopVanityActions` 的跨关系停止顺序由显式 scheduler 固定。

### 不变量、兼容和 owner

- `IsSitting=false` 时 `SeatOffset=Zero`、`StackIndex=-1`、`SeatDetails=default` 是 SitUp 后不变量；网络只投影 phase bit，不应凭一个 bit 恢复缺失的座位 geometry。
- seat target 的 tile type/frame、方向和 `ExtraSeatInfo` 必须在同一 Query snapshot 中计算，避免 offset/details/index 来自不同 tick。
- `sittingIndex` 是 `AnchoredEntitiesCollection` 的短期 stack index，不是持久 EntityId；manager 清理、世界重置和玩家离开必须由 adapter 处理。
- `ChairSittingMaxDistance` 当前无消费者证据；不得把它当已确认的判定阈值。进入 writer 缺失时保持 integration-review。
- C12 负责睡眠 phase/床面状态，C11 只负责座位；rest mutual exclusion 需要唯一 scheduler，不得双写或依赖文档顺序。
- Tile/Framing、stack manager、NetMessage、NPC/toilet effect、位置/渲染均经 ports/adapters；C11 不宣布 WorldInteraction/NPC/P11 的最终 owner。

### C11 focused verifier 计划

1. `SeatEligibilityQuery`: active/can-sit tile、frame variants、方向、seat position/offset、toilet flag 和纯读保证。
2. `SittingEntryClosure`: 搜索并验证 `isSitting=true` 进入 writer、距离常量消费者、stack capacity 与 C12 mutual exclusion；缺失时输出 evidence-gap。
3. `SittingUpdateAndExit`: invalid tile、input/pulley/mount/direction、stack >=2、offset/details/index 更新与 SitUp reset。
4. `SittingStackAdapter`: manager add/clear/reload、index reuse、world reset/player removal 和非持久化保证。
5. `SittingNetworkProjection`: type 13 bit 2、退出广播、缺 geometry 的远端状态和重复/迟到消息处理。

#### C11 检查点

- `completedComponents`: C01-C11 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C12 `PlayerSleeping`。
- 实现：`partial`；验证：`not-run`。
- 实际源码：`src/Player/PlayerSittingComponent.cs`。
- 依赖缺口：`ExtraSeatInfo` 不存在于当前 src，使用 `RestSeatFeatures` 仅承载已确认的厕所位；entry writer、tile query、stack manager、network adapter 与 PlayerRest unique owner 未闭合。

## 5.12 C12 `PlayerSleeping`

**设计状态：** `proposed`。C12 将两个常量、四个睡眠状态字段和一个派生属性分成规则定义、睡眠关系、床面资格 Query、stack/网络 adapter 与表现投影。`FullyFallenAsleep` 不是可写字段；它只由 `isSleeping` 和 `timeSleeping >= 120` 计算。C11 坐姿和 C12 睡眠共享“锁定到 Tile”的互斥调度，但保持各自唯一 writer。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerSleepingComponent` | `src/Player/PlayerSleepingComponent.cs` | 保存睡眠 phase、短期 stack index、入睡计时和床面视觉偏移 | `Sleep/StopSleep` command -> rest snapshot | Player rest owner；与 C11 共享互斥 scheduler |
| `PlayerSleepingRulesDefinition` | `src/Player/PlayerSleepingRulesDefinition.cs` | 暴露床面距离和完全入睡阈值等只读规则 | rules -> sleep query/property | 无实体写入；当前距离消费者需补证据 |
| `PlayerBedEligibilityQuery` | `src/WorldInteraction/PlayerBedEligibilityQuery.cs` | 读取床 tile type/frame，计算目标方向、anchor 和 visual offset | tile/player snapshot -> immutable bed candidate | `ITileReadPort`; `SetOffsetbyBed` 缺失时不得猜测 |
| `PlayerSleepingRestAdapter` | `src/WorldInteraction/PlayerSleepingRestAdapter.cs` | 适配 sleepingManager stack、旋转/广播和世界清理 | sleep snapshot -> position/network/stack effects | `ISleepingStackPort`、`INetworkTransport`、Presentation port |
| `PlayerFullyFallenAsleepQuery` | `src/Player/PlayerFullyFallenAsleepQuery.cs` | 纯计算 `isSleeping && timeSleeping >= 120` | sleep snapshot -> bool | 无写入；供 Main/WorldGen/Presentation 读取 |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 385 | `BedSleepingMaxDistance: const int` | `PlayerSleepingRulesDefinition.MaxDistancePixels` | Helper 声明值为 `96`；当前 Version4 检索未发现消费者，不能把它自动写入入床资格 | immutable rule definition；不进 Entity/save/network | `PlayerSleepingHelper.cs:9`; consumer closure missing |
| 386 | `TimeToFullyFallAsleep: const int` | `PlayerSleepingRulesDefinition.FullyAsleepTicks` | 声明值为 `120`；`FullyFallenAsleep` getter 当前直接比较字面量 `120`，未来必须由 verifier 保证常量与实现不漂移 | immutable threshold; query/WorldGen/Presentation reads | `PlayerSleepingHelper.cs:11,21-30`; `Main.cs:11434-11437` |
| 387 | `isSleeping: bool` | `PlayerSleepingComponent.IsSleeping` | `SetIsSleepingAndAdjustPlayerRotation` 写；UpdateState/offset/lock/regen/Eye 读取；MessageBuffer 通过 type 13 bit 0 调该方法 | authoritative rest phase; rotation/network/presentation effects are adapters | `PlayerSleepingHelper.cs:13,50-70,71-126`; `MessageBuffer.cs:725`; `NetMessage.cs:474` |
| 388 | `sleepingIndex: int` | `PlayerSleepingComponent.StackIndex`，外部 manager index | UpdateState 通过 `Main.sleepingManager.AddPlayerAndGetItsStackedIndexInCoords` 写；offset 读取；Stop 清为 -1；stack >=2 触发 Stop | external stack relation/cache; not stable EntityId or persistence | `PlayerSleepingHelper.cs:15,117-124,139-150`; `Main.cs:1199,3272`; `AnchoredEntitiesCollection.cs:50-68` |
| 389 | `timeSleeping: int` | `PlayerSleepingComponent.TimeSleeping` | isSleeping=false 时 UpdateState 清 0；睡眠中每 tick ++；act-up reason 时清 0；Stop 清 0；FullyFallenAsleep/Eye/World logic 读取 | authoritative short-lived phase timer; clock/tick input explicit | `PlayerSleepingHelper.cs:17,21-30,75-85`; `PlayerEyeHelper.cs:65-66`; `Main.cs:11434-11437` |
| 390 | `visualOffsetOfBedBase: Vector2` | `PlayerSleepingComponent.BedVisualOffset` plus Presentation projection | UpdateState 从 bed query 写；GetSleepingOffsetInfo/Player rendering 读取；state false、Stop 清 default；`SetOffsetbyBed` 当前 stub 返回空 Vector2 | presentation/position projection; no gameplay authority from raw offset | `PlayerSleepingHelper.cs:19,33-45,121-125,145-150,184`; `Player.cs:3297-3300,6516-6519`; partial |
| 1418 | `FullyFallenAsleep: bool` | `PlayerFullyFallenAsleepQuery.IsFullyAsleep`，只读派生属性 | getter 返回 `isSleeping && timeSleeping >= 120`；Main.UpdatePlayers 统计 fully asleep；不接受 setter/网络输入 | derived query output; no persistence/network writer | `PlayerSleepingHelper.cs:21-31`; `Main.cs:11434-11437` |

### 状态转换与副作用顺序

1. `PlayerBedEligibilityQuery` 读取 `(player.Bottom + (0,-2)).ToTileCoordinates()` 的 tile，先验证 `TileID.Sets.CanBeSleptIn` 与 active，再按 frame 计算 target direction、anchor 和 visual offset。当前 `SetOffsetbyBed` 是 stub，缺失部分必须返回 evidence-gap，不得猜床型偏移。
2. `SleepCommand` 只有在床候选、方向/距离、stack capacity 和 C11 rest mutual exclusion 闭合后才能提交 `IsSleeping`；当前 Version4 未提供完整本地入床 writer，网络接收的 `SetIsSleepingAndAdjustPlayerRotation` 只能作为兼容入口，不能据此推断完整进入协议。
3. `SetIsSleepingAndAdjustPlayerRotation(true)` 原子切换 phase 并设置 `fullRotation = PI/2 * -direction` 与 rotation origin；false 清旋转/origin/bed visual offset。rotation 与网络效果在 adapter，Component 不直接调用 graphics/NetMessage。
4. 每 tick `UpdateState` 在未睡眠时清 `timeSleeping`，睡眠时递增并调用 act-up reason；随后复核床 candidate、输入/pulley/mount/方向、持有物伤害/钓竿/ForcesBreaksSleeping 和 stack >=2。任何失败调用唯一 `StopSleeping`。
5. 成功睡眠更新 visual offset 并从 `sleepingManager` 取得 stack index；`GetSleepingOffsetInfo` 以 `visualOffset * player.Directions` 加 stack Y offset 输出表现位置。`StopSleeping` 清 phase/time/index/offset，并按条件发送 type 13。
6. `FullyFallenAsleep` 只读计算，不能由网络 bit 或外部 UI 写入；Main/WorldGen/Eye/Presentation 只消费快照。C11/C12 的更新顺序由 rest scheduler 固定，不能依赖文档或文件顺序。

### 不变量、兼容和 owner

- `IsSleeping=false` 时 `TimeSleeping=0`、`StackIndex=-1`、`BedVisualOffset=Zero` 且 full rotation 已恢复；停止路径必须保持这些清理顺序。
- `FullyFallenAsleep` 等价于 `IsSleeping && TimeSleeping >= 120`；常量 `TimeToFullyFallAsleep` 与字面量比较的漂移必须由 verifier 捕获。
- `BedSleepingMaxDistance` 没有当前消费者证据，不能宣称为已生效距离规则；床面 anchor 输出也不能在 stub 缺失时被伪造。
- `sleepingIndex` 是 sleepingManager 的短期 stack index，不是持久 ID；世界 reset、玩家离开和 manager 清理由 adapter 负责。
- `isLockedToATile` 的坐姿优先/睡眠回退语义、`StopVanityActions` 的坐姿后睡眠停止顺序必须显式测试；C11/C12 不允许双写对方 phase。
- Full rotation、tile read、stack manager、NetMessage、Eye/WorldGen/Presentation 和 Item interrupt 均经显式 ports/adapters；C12 不宣布跨分区最终 owner。

### C12 focused verifier 计划

1. `BedEligibilityQuery`: active/sleepable tile、frame direction、anchor、visual offset 及 stub/missing evidence 处理。
2. `SleepingLifecycle`: enter/rotation, tick increment, act-up reset, invalid bed/input/item/stack exits, Stop cleanup and no negative index。
3. `FullyAsleepProjection`: 120 tick boundary、isSleeping false、Main/WorldGen/Eye readers 和 no-writer property。
4. `SleepingStackAdapter`: manager add/clear/reload、index reuse、world reset/player removal and non-persistence。
5. `SleepingNetworkAndRestMutex`: type 13 bit 0 round-trip, rotation side effect, exit broadcast, C11/C12 mutual exclusion and scheduler order。

#### C12 检查点

- `completedComponents`: C01-C12 已写入设计与执行文档，并已保存允许范围内的组件源码。
- `currentComponent`: C13 `PlayerRabbitOrderFrame`。
- 实现：`partial`；验证：`not-run`。
- 实际源码：`src/Player/PlayerSleepingComponent.cs`。
- 依赖缺口：bed offset/eligibility、act-up writer、rotation、stack manager、network/presentation adapter、FullyFallenAsleep query 和 C11/C12 unique owner 未闭合。

## 5.13 C13 `PlayerRabbitOrderFrame`

**设计状态：** `proposed`。C13 是兔子头部表现的局部 frame projection，不是玩家权威行为或持久化 AI。它包含一个向渲染读取的 `DisplayFrame`、两个只在 helper 内部变化的运行时投影变量和四个状态定义常量；`Main.rand`、表现调用点和 Spawn reset 都必须停留在显式 Client/P11 adapter 边界。

### 边界与最小接口

| proposed 类型 | 建议路径 | 核心职责 | 输入/输出 | seam |
|---|---|---|---|---|
| `PlayerRabbitOrderFrameProjection` | `src/Client/PlayerRabbitOrderFrameProjection.cs` 或 P11 Presentation 目录（待 owner review） | 按兔子头部表现条件推进 frame state machine 并输出 `DisplayFrame` | player visual snapshot + tick -> frame projection | `IRandomSource`、P11 renderer；无 authority/save/network writer |
| `RabbitOrderFrameStateDefinition` | `src/Client/RabbitOrderFrameStateDefinition.cs`（仅在需要共享定义时） | 保存 Idle/Looking/Resting/Eating 的状态编号和 frame ranges | immutable definitions -> projection | 无实体写入 |

### 成员归属与证据

| 来源序号 | Version4 成员 | proposed 归属 | 读者/写者与生命周期 | 副作用/状态 | 证据 |
|---:|---|---|---|---|---|
| 411 | `DisplayFrame: int` | `PlayerRabbitOrderFrameProjection.DisplayFrame` | `UpdateFrame` clamp/increment；PlayerFrame 在 head 259 且未跳过动画时驱动 Update，P11/绘制路径消费；Spawn Reset 间接回到 Idle/update | presentation output；不持久化、不网络复制 | `Player.cs:142,200-212,20964-20966,21821-21823`; reader/writer closure partial |
| 412 | `_frameCounter: int` | `PlayerRabbitOrderFrameProjection.FrameCounter`，私有运行时状态 | `ChangeToAIState` 清 0；`UpdateFrame` postfix increment，达到 per-frame threshold 后清 0；无外部 reader | projection timing state；不作为游戏 tick authority | `Player.cs:144,191-198,200-210` |
| 413 | `_aiState: int` | `PlayerRabbitOrderFrameProjection.State`，私有 projection state | `ChangeToAIState` 唯一写；Update switch 读取；Idle 随机退出到 1-3，非 Idle 状态退出到 0；不得与 NPC/Combat AI 共用 owner | local presentation state；无网络/save claim | `Player.cs:146,158-183,191-198,204-210` |
| 414 | `AIState_Idle: const int` | `RabbitOrderFrameStateDefinition.Idle = 0` | Update switch 的状态定义；private const，无实体生命周期 | immutable definition; no component field | `Player.cs:148,160-164` |
| 415 | `AIState_LookingAtCamera: const int` | `RabbitOrderFrameStateDefinition.LookingAtCamera = 1` | Update case 1 使用 frame 7-9、20 tick/frame、结束回 Idle；private const | immutable definition; no network/save | `Player.cs:150,165-167` |
| 416 | `AIState_Resting: const int` | `RabbitOrderFrameStateDefinition.Resting = 2` | Update case 2 使用 frame 10-16，frame 13 特殊 120 tick/frame，否则 8；结束回 Idle | immutable definition; no gameplay rest owner | `Player.cs:152,168-177` |
| 417 | `AIState_EatingCarrot: const int` | `RabbitOrderFrameStateDefinition.EatingCarrot = 3` | Update case 3 使用 frame 17-26、4 tick/frame、结束回 Idle；private const | immutable definition; no inventory/Item consumption owner | `Player.cs:154,178-180` |

### 状态转换与副作用顺序

1. PlayerFrame 只有在 `head == 259 && !skipAnimatingValuesInPlayerFrame` 时调用 projection `Update`；其他玩家或跳过动画的帧不推进 C13 状态。渲染读取 `DisplayFrame` 的具体路径需由 P11 verifier 闭合。
2. Idle 状态调用 `Main.rand.Next(1, 4)` 选择 1-3，并调用 `Main.rand.Next(180, 3600)` 选择等待帧；随机源必须改为显式 `IRandomSource`，但本轮不改变 Version4 行为或引入全局随机 owner。
3. `UpdateFrame` 先将 `DisplayFrame` clamp 到当前状态范围，再递增 `_frameCounter`；达到阈值后清 counter、递增 frame，超出范围通过 `ChangeToAIState(exitAIState)` 转移。`ChangeToAIState` 会清 counter 并立即调用一次 Update，递归/边界行为必须由 verifier 固定。
4. `Reset` 通过 `ChangeToAIState(0)` 回 Idle 并立即推进一次；当前 Spawn 路径调用 `rabbitOrderFrame.Reset()`，不能把 reset 当成存档加载或网络确认。
5. C13 只输出表现帧；不写头部装备、NPC AI、Item inventory、网络 bit 或 Player persistence。`skipAnimatingValuesInPlayerFrame` 是调用方条件，不在 C13 重复建模。

### 不变量、兼容和 owner

- `DisplayFrame` 始终受当前状态的 min/max clamp；状态 1/2/3 的 frame ranges 和 per-frame thresholds 不能被泛化为一个共享动画规则而改变边界。
- `_aiState` 只能取 0-3；定义常量是 immutable catalog，不是 ECS entity state，也不应进入 save/network snapshot。
- `_frameCounter` 是表现局部计时器，必须在 state transition/reset 时清零；它不消费或驱动 C08 Player item timers。
- `Main.rand`、tick 输入、渲染和 Spawn reset 都通过显式 ports/adapters；不能让日志、UI 或渲染回调反向改变 authoritative Player state。
- 当前没有检出 C13 save/network writer；在 P11 owner 和跨客户端 determinism 未闭合前，保持 projection-only 和 integration-review。
- 不拆出通用 Rabbit/NPC AI component；C13 的状态编号与玩法 AI 无替换关系，文件顺序也不能定义投影调度。

### C13 focused verifier 计划

1. `RabbitFrameStateMachine`: state ranges, clamp, counter thresholds, transition-to-idle and immediate Update behavior.
2. `RabbitRandomSource`: Idle random state/wait bounds, injected deterministic source and no `Main.rand` leakage into authority.
3. `RabbitProjectionGate`: head 259 and `skipAnimatingValuesInPlayerFrame` gating, DisplayFrame consumer and no update when gated off.
4. `RabbitResetLifecycle`: Spawn/reset behavior, default state/frame, counter reset and no save/network serialization.
5. `RabbitPresentationOwnership`: P11/Client owner handoff, unique writer, separation from Player item/rest/NPC AI state and cross-client reproduction policy.

#### C13 检查点

- `completedComponents`: C01-C13 已处理并写入设计与执行文档。
- `currentComponent`: 空。
- `pendingComponents`: `[]`。
- 实现：`deferred-no-component`；验证：`not-run`。
- 实际源码：无；C13 是 projection-only，且本任务只允许组件源码。
- 依赖缺口：P11 owner、随机源、frame consumer、head-259 gate 和跨客户端 determinism 未闭合。

## 6. Integration Handoff 初始清单

| 交接对象 | P04 成员/边界 | 不在本分区宣布的决定 |
|---|---|---|
| Combat/DeathPenalty | `lostCoins`, `lostCoinString`, `difficulty`, `attackCD`, `hitTile`, `hitReplace`, death effects | 死亡结算、掉落、受击冷却和死亡惩罚的唯一写者 |
| Teleportation/WorldInteraction | `teleporting`, portal flags, `lastTeleportPylonStyleUsed`, `fallStart`, `wireOperationsCooldown` | 传送提交、Portal/Pylon、Wiring 和移动根的交接 |
| Items/WorldStorage | chest relations, tracked projectile references, shopping settings | 容器内容、持久化字节格式、投射物恢复和交易状态 owner |
| P11 Presentation | `chatOverhead`, `emoteTime`, `rabbitOrderFrame`, `visualOffsetOfBedBase` | 表现快照、渲染、音频和随机帧消费 owner |
| NPC/Projectile | pet target IDs, `ownedProjectileCounts`, `npcTypeNoAggro`, minion target | 外部实体 ID、目标有效性和缓存刷新 owner |

## 7. 设计状态

当前已保存 C01-C08、C10-C12 的允许范围组件源码；C06/C11/C12 保留 partial 依赖，C09/C13 按证据不创建组件。Teleportation、Combat、WorldInteraction 项目编译和两个既有 smoke verifier 已有证据；Player 项目被其他会话遗留的 5 个编译错误阻塞，因此总体 `verificationStatus: partial`。本分区行为等价、网络闭合、持久化闭合和跨分区 owner 仍未验证。

## 8. 实际验证记录

所有 compile-capable 命令均先检查活动 `dotnet.exe`/`csc.exe`，再从仓库根目录经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行。为避免 PowerShell 将 `-p:` 误解析为包装脚本参数，实际调用使用包装脚本的显式 `-DotnetArguments` 数组；未运行 raw `dotnet`。

| 项目/验证器 | 实际命令参数 | 结果 | 输出或 artifact |
|---|---|---|---|
| `src/Player/Terraria.Player.csproj` | `build .\src\Player\Terraria.Player.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `1`；0 warnings，5 errors | 未生成可信的本次 artifact；`Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 的既有文件时间早于本次构建，视为 stale/untrusted。错误来自其他会话的 `src/Player/Progression/PlayerMinionCapacityCommitSystem.cs` 与 `SubmitMinionCapacityDeltaCommand.cs`，缺少 `Terraria.Relationships`/`Terraria.Projectile` 引用。 |
| `src/Teleportation/Terraria.Teleportation.csproj` | `build .\src\Teleportation\Terraria.Teleportation.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`；0 warnings，0 errors | `Build/bin/Terraria.Teleportation/Debug/net10.0/Terraria.Teleportation.dll` |
| `src/Combat/Terraria.Combat.csproj` | `build .\src\Combat\Terraria.Combat.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`；0 warnings，0 errors | `Build/bin/Terraria.Combat/Debug/net10.0/Terraria.Combat.dll` |
| `src/WorldInteraction/Terraria.WorldInteraction.csproj` | `build .\src\WorldInteraction\Terraria.WorldInteraction.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`；0 warnings，0 errors | `Build/bin/Terraria.WorldInteraction/Debug/net10.0/Terraria.WorldInteraction.dll` |
| `Test/Terraria.WorldInteraction.Components.Verification` | `run --project .\Test\Terraria.WorldInteraction.Components.Verification\Terraria.WorldInteraction.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`；输出 `PASS: world interaction component field composition` | 使用既有 `Build/bin/Terraria.WorldInteraction.Components.Verification/Debug/net10.0/Terraria.WorldInteraction.Components.Verification.exe` |
| `Test/Terraria.Combat.Verification` | `run --project .\Test\Terraria.Combat.Verification\Terraria.Combat.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`；输出 `PASS: combat resolution, immunity, contribution, and attribution` | 使用既有 `Build/bin/Terraria.Combat.Verification/Debug/net10.0/Terraria.Combat.Verification.exe` |

上述 verifier 是既有域级 smoke verifier，不覆盖 P04 全部生命周期、网络、持久化和跨分区 writer；当前没有 P04 专用 Player/Teleportation verifier。`Player` 项目失败后未修改其他会话的 Progression 源码，也未将旧 artifact 当作本次编译证据。
