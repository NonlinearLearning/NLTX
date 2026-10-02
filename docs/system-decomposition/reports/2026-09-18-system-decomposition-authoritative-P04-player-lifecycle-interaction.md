# System Decomposition Report: authoritative P04

partitionId: P04  
taskId: AUTH-SYS-P04  
taskSetName: authoritative-system-decomposition  
sessionId: 910f8127b89041cc98bf563325fab350  
designStatus: proposed  
evidenceStatus: partial  
verificationStatus: not-run  
sourceModified: false

## Scope and Evidence

本报告按已 claim 的 authoritative P04 输入分区提出 System/API 边界，不实现迁移。覆盖 13 个叶子组、111 个字段和 1 个属性，共 112 个成员。唯一输入、提示词和授权输出如下：

| 用途 | 路径 |
|---|---|
| authoritative 输入 | `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P04-Player-Lifecycle-Interaction.md` |
| claim 专属提示词 | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-11-version4-authoritative-20-partition-prompts\2026-09-11-version4-P04-player-lifecycle-interaction-public-decomposition.md` |
| 唯一授权输出 | `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P04-player-lifecycle-interaction.md` |

证据分层如下：

| 证据层 | 来源与身份 | 可支持的结论 | 限制 |
|---|---|---|---|
| Version4 源码 | 本地目录 `D:\TRbackup\Version4`；本次读取 Player、Main、MessageBuffer、PlayerFileData、WorldFile 及 petting/sitting/sleeping helper。Version4 目录没有可读取的 Git HEAD；下表给出本地文件 SHA-256。 | 关键方法分支、直接状态赋值、同步调用顺序和外部效果 | 只确认实际读到的代码；不能从局部方法推导所有入口、序列化、动态目标或运行期调度闭包 |
| CPG 查询 API | 只读入口 `D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\CpgEvidence.ps1`；数据库 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`；manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；`ImportStatus=complete`、967 shards、8,166,789 nodes、71,038,907 edges、1,317 diagnostics、`SourceSnapshotId=null`。 | 精确符号、所选源码路径中的静态 call-site、字段使用候选 | 查询完整仅指索引在范围/预算内完成；未绑定当前源码快照，不证明运行时闭包、callee effects、alias、动态派发、序列化、事件订阅或 scheduler 顺序 |
| 当前 NLTX | Git HEAD `865ca66bfec2b8b455aabe3ed29ce0e60a1f9401`，目标源码在 `D:\TRbackup\NLTX\src\NSSLC`。本次启动前工作树已含其他路径的未提交变化。 | 组件、Query/System 命名和目标树中能直接看到的引用关系 | 组件存在不是 System owner、调用接入或行为迁移证据；对 P04 组件名的源码搜索未找到对应生产 System 使用点 |
| Space Station 14 | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Hands\EntitySystems\SharedHandsSystem.cs` | 仅作结构参考：同一 System 可承载查询式方法与修改式方法，并由 System 协调组件及容器服务 | 不推断 Terraria 行为，不复制 SS14 命名或领域语义 |

本次核对的 Version4 当前文件 SHA-256：

| 文件 | SHA-256 |
|---|---|
| `D:\TRbackup\Version4\Terraria\Player.cs` | `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86` |
| `D:\TRbackup\Version4\Terraria\Main.cs` | `66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520` |
| `D:\TRbackup\Version4\Terraria\MessageBuffer.cs` | `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE` |
| `D:\TRbackup\Version4\Terraria.GameContent\PlayerPettingInfo.cs` | `8986995B7C398838358B44E77F01148DB0B9ADA745BDB34E6A4180CDF55B867E` |
| `D:\TRbackup\Version4\Terraria.GameContent\PlayerSittingHelper.cs` | `081AD85B6DE42C61BCB54AAC4BA516F640820BDE624D880ADA493872869BA0B8` |
| `D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs` | `ACDC9DAB4657027580FF6BB497D7DC4F9E2F5E03D0857557393AAFB6378EF53A` |
| `D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs` | `87C429514B9A709968BA35F9DE0F668BFA2FB83861B0ABC828F0521516AB0E04` |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs` | `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289` |

CPG 查询使用 `Find-CpgSymbols`、`Find-CpgCallSites`、`Get-CpgMemberUses`。所选静态调用范围中，`Player.Update(int)` 有 1 个 `Main.cs` call-site，`KillMe` 有 3 个，`Spawn` 有 4 个，`Teleport` 有 11 个，均落在所选 `Player.cs`/`Main.cs`/`MessageBuffer.cs` 路径。`itemTime` 在所选 shard 返回 20 个 member-use；部分赋值左侧可分类为写，其余 access mode 为 `Unknown`。查询结果不升级为完整读写闭包；`KillMe` callable facts 为 `partial`，callee effects 未展开。源码片段已另行对照当前文件，CPG manifest 本身没有 per-file source hash。

## Prior Component Decomposition Reconciliation

P04 第二轮 Component 设计/执行材料及当前 NLTX 类型仅作为候选状态核对，不作为迁移事实。

| 当前 NLTX 材料 | 本次观察 | System 级处理 |
|---|---|---|
| `PlayerIdentityComponent`、`PlayerLifecycleComponent`、`PlayerDeathRespawnComponent`、`PlayerDeathRecordComponent` | `IsActive`、`IsDead`、计时、spectating 和死亡记录存在重复表达；其中 `PlayerLifecycleState` 又有 Phase、死亡计数和最后死亡位置。对这些类型名的 `src/NSSLC` 搜索只命中声明/定义，没有 P04 生产 System 使用点。 | proposed 只保留每个不变量一个权威表示；Session/slot/persistent identity 到实体的映射以及 active 的唯一提交者为 `crossSubsystemOwner: integration-review`。 |
| `PlayerRestComponent`/`PlayerRestState` 与 `PlayerPettingComponent`、`PlayerSittingComponent`、`PlayerSleepingComponent` | 聚合 `Mode` 与独立 `IsPetting`/`IsSitting`/`IsSleeping` 可互相矛盾；目前没有找到统一 rest System 调用者。 | proposed 一个 rest coordinator/System 持有互斥模式并提交 capability 状态。不得让两套布尔值成为并行 authority；最后选用/裁剪哪套组件交由整合审查。 |
| `PlayerSpawnPointComponent`、`PlayerSaveCheckpointComponent` | 仅有数据形状；没有 P04 Spawn/Save System 接入。 | 可作为 spawn 路由及保存提交结果的候选数据；恢复仍走 lifecycle owner 校验，不能由 Projection 直接写回。 |
| `PlayerTeleportTransitionComponent` 和 `src/NSSLC/Component/Teleportation` 其他类型 | 传送组件记录有限 transition 标记；本次没有查到其提交位置/网络 ack 的 P04 调用链。 | 按 Teleportation 集成边界提案；P04 不重建移动、pressure plate、network 或 visual owner。 |
| `PlayerContainerRelationComponent`、`PlayerTrackedContainerLinkComponent`、`PlayerTileEntityAnchorComponent` | P09 已标明 Bank/VoidVault 关系边界；锚点组件保存外部 TileEntity 引用身份和坐标。 | 不把容器内容、容量、转移、TileEntity registry 或投射物 registry 接管进 P04；相关读写按 `crossSubsystemOwner: integration-review` 交接。 |
| `PlayerItemActionTimingComponent` | 仅含 animation/use/tool 计时形状，没有 P04 System 消费点；原分区定时成员还包括 Movement、Wiring、Combat、item-delay/global 状态。 | 不将混合计时字段合并成新的 P04 authority；按 Item/Movement/Wiring/Combat 接口整合，保留唯一写者裁决。 |
| `RevengeRespawnSystem` | 有独立复仇重生实现，但没有证据表明它消费上述 P04 生命周期组件，也不等于 Player Spawn owner。 | 保留为 integration-risk；死亡惩罚、复仇重生和玩家生命周期不能根据类名合并。 |

## Conceptual Behaviors

本分区覆盖的是可观察行为切片，不等同于 13 个叶子组各建一个 System。

| 稳定行为 ID | 输入与不变量 | 可观察结果/副作用 | proposed 协作 |
|---|---|---|---|
| `player-session-identity` | 网络/session 输入关联到玩家实体；slot、session、persistent identity 不混用；激活状态只由唯一 authority 写入 | Main 只更新 active slot；连接/断开 hook 仅在 active 变化时触发；名字/host 状态参与连接投影 | `PlayerLifecycleSystem` + Session/Network adapter；唯一 ID 映射待整合 |
| `player-death-resolution` | 只有满足旧前置条件时进入一次死亡转换；死亡记录、dead/respawn 状态和掉落结果属于同一已提交死亡 | PVE/PVP 计数、死亡时间/位置、掉落、音效/粒子、死亡文本、tombstone、spectating 和 respawn timer；重复/被拒绝输入不得重复提交 | 一个同步 lifecycle API 协调自身写入；Combat、Items、DeathPenalty、Network、Presentation 通过显式边界 |
| `player-spawn-and-return` | SpawnContext、死亡/重生状态、个人 spawn、team/world spawn 和 return route；选择及授权发生在提交时 | 状态恢复、位置/速度/免疫重置、激活和网络可见性；return/团队路线需要保持来源 context | `PlayerLifecycleSystem`；Movement/World spawn resolver 与 network/persistence adapter |
| `player-teleport-transition` | 目标位置、style、extraInfo、ack 状态以及调用方身份；死亡/观战和抓钩/重力状态需要一致提交 | 位置、传送效果/音效、pressure plate、spectating、wall scan、portal/pylon metadata、网络 ack 和计时状态 | proposed Teleportation System API；Movement/World/Network/Presentation 协作，owner `integration-review` |
| `player-rest-interaction` | Petting NPC/Projectile/Mount、坐椅、睡床是互斥或有明确定义的协作状态；每 tick 依据输入/目标有效性推进或停止 | petting/sitting/sleeping 标志、偏移/旋转/堆叠 index、rest packet；StopVanityActions 同步清理三个 capability | 单一 `PlayerRestInteractionSystem` coordinator；目标查询、Entity/Tile registry、Network/Presentation 为 adapter/port |
| `player-world-anchor-interaction` | 玩家当前/先前 container、shop、door、tile capability 和 TileEntity anchor 均是不同关系/能力 | 网络/实体关联、TileEntity 查找、容器/商店/门交互以及可视锚点 | 复用 P09、WorldInteraction、Commerce、Presentation owner；P04 不新增 catch-all System |
| `player-item-action-timing` | item timer 与 input、animation、projectile blocking、fall、wiring 和 combat 同步 | item-use/animation 进度、冷却/跌落/格挡可观察变化 | Item/Movement/Wiring/Combat integration；`ItemCheckContext` 保持调用内临时输入 |
| `rabbit-order-frame-projection` | AI 状态和随机/时间输入影响局部帧推进 | `DisplayFrame` 和表现帧变化 | P11/Presentation candidate；P04 不把帧状态当生命周期 authority |

## State Ownership and Write Closure

下表穷尽本次 claim 的 112 个成员。成员清单只用于 P04 覆盖核验；owner 列是 proposed 边界而非已经实现的归属。混合组的跨域子集不由 P04 重新设计。

| 权威叶子组 | 数量 | 完整成员清单 | proposed owner/分类与证据状态 |
|---|---:|---|---|
| `PlayerIdentityAndDeathRecordState` | 10 | `active`, `host`, `lostCoins`, `lostCoinString`, `name`, `numberOfDeathsPVE`, `numberOfDeathsPVP`, `lastDeathPostion`, `lastDeathTime`, `showLastDeath` | active/host/name 归 session identity/lifecycle 候选；死亡结果字段归 `PlayerLifecycleSystem` 所有的 DeathRecord component 候选。KillMe 中的写入已读到；网络/session/save 的完整 closure `partial`。 |
| `PlayerRuntimeInteractionAndEffectState` | 13 | `MinecartSettings`, `emoteTime`, `creativeTracker`, `chatOverhead`, `GoingDownWithGrapple`, `spelunkerTimer`, `builderAccStatus`, `soulDrain`, `dd2Accessory`, `crystalLeafDamage`, `crystalLeafKB`, `basiliskCharge`, `PaladinsShieldRange` | 混合 Minecart、Presentation、Creative/Progression、Movement、World、Combat、Mount/Episode 状态；按各域 API handoff。P04 独占 owner `unknown`。 |
| `PlayerTeleportTransitionState` | 4 | `teleporting`, `teleportTime`, `teleportStyle`, `unacknowledgedTeleports` | proposed Teleportation transition/ack state；position 与移动提交归 Movement/Teleportation integration；视觉 timer、网络 ack 不是互相替代的同一状态。 |
| `PlayerDeathRespawnAndSaveState` | 14 | `dead`, `deadTime`, `spectating`, `respawnTimer`, `respawnTimerMax`, `DeadSpectatingLockoutTime`, `SpectatingLingerAfterDeath`, `lastTimePlayerWasSaved`, `attackCD`, `potionDelay`, `difficulty`, `wetSlime`, `hitTile`, `hitReplace` | dead/deadTime/spectating/respawn constants 和 timer 由 lifecycle 候选；`lastTimePlayerWasSaved` 属保存提交边界；attackCD/hitTile/hitReplace 是 Combat，potionDelay 是 Items，difficulty 的持久身份 owner unresolved，wetSlime 为 Environment/Movement handoff。 |
| `PlayerSpawnAndReturnState` | 4 | `SpawnX`, `SpawnY`, `PotionOfReturnOriginalUsePosition`, `PotionOfReturnHomePosition` | spawn route/return facts 候选由 lifecycle API 协调；个人 spawn tile、使用位置和 world/session identity 需要 WorldStorage/Items/Movement integration。 |
| `PlayerContainerAndWorldAnchorState` | 17 | `lastChest`, `piggyBankProjTracker`, `voidLensChest`, `chest`, `petting`, `sitting`, `sleeping`, `eyeHelper`, `tileEntityAnchor`, `doorHelper`, `currentShoppingSettings`, `TouchedTiles`, `equippedAnyTileRangeAcc`, `equippedAnyTileSpeedAcc`, `equippedAnyWallSpeedAcc`, `behindBackWall`, `_funkytownAchievementCheckCooldown` | Bank/VoidVault relation 与 P09 integration；petting/sitting/sleeping 由 Rest System；eyeHelper/behindBackWall 为 Presentation，TileEntity/door/tile range 为 WorldInteraction，shopping 为 Commerce；cooldown 所有权 `unknown`。 |
| `PlayerPortalAndTargetingState` | 11 | `ownedProjectileCounts`, `npcTypeNoAggro`, `lastPortalColorIndex`, `_portalPhysicsTime`, `portalPhysicsFlag`, `lastTeleportPylonStyleUsed`, `MountFishronSpecialCounter`, `MinionRestTargetPoint`, `MinionAttackTargetNPC`, `_blackListedTileCoordsForGrappling`, `makeStrongBee` | Portal/Pylon 归 Teleportation/World，owned projectile/minion/bee/aggro 归 Projectile/Combat，Fishron counter 归 Mount，grapple blacklist 归 Movement。全列标 `crossSubsystemOwner: integration-review`。 |
| `PlayerItemActionTimingState` | 12 | `wireOperationsCooldown`, `fallStart`, `fallStart2`, `potionDelayTime`, `restorationDelayTime`, `mushroomDelayTime`, `itemAnimation`, `itemAnimationMax`, `itemTime`, `itemTimeMax`, `toolTime`, `BlockInteractionWithProjectiles` | wire cooldown 为 Wiring，fall starts 为 Movement，potion/restoration/mushroom 和 animation/use timers 为 Items candidate，toolTime 需按真实 consumer 定义，静态格挡值不是 per-player component。P04 不指定唯一 owner。 |
| `PlayerItemCheckContext` | 1 | `SkipItemConsumption` | 方法调用内的 `ItemCheckContext` payload；不作为持久 Component，不暴露写状态 Query。是否被消费及所有消费路径 `unknown`。 |
| `PlayerPettingState` | 7 | `isPetting`, `npc`, `proj`, `type`, `mount`, `offsetFromPet`, `isPetSmall` | Rest System 候选维护互斥关系；NPC/Projectile/Mount identity、slot/type 检验依赖外部 registry；过期引用清理路径 `partial`。 |
| `PlayerSittingState` | 5 | `ChairSittingMaxDistance`, `isSitting`, `details`, `offsetForSeat`, `sittingIndex` | Rest System 候选；椅子 eligibility/ExtraSeatInfo、tile framing 和 stack manager 是 adapter/port 输入；距离常量不是实体状态。 |
| `PlayerSleepingState` | 7 | `BedSleepingMaxDistance`, `TimeToFullyFallAsleep`, `isSleeping`, `sleepingIndex`, `timeSleeping`, `visualOffsetOfBedBase`, `FullyFallenAsleep` | Rest System 候选；`FullyFallenAsleep = isSleeping && timeSleeping >= 120` 是只读派生属性候选；床 offset/helper 的被清空实现 `unknown`。 |
| `PlayerRabbitOrderFrameState` | 7 | `DisplayFrame`, `_frameCounter`, `_aiState`, `AIState_Idle`, `AIState_LookingAtCamera`, `AIState_Resting`, `AIState_EatingCarrot` | 表现状态/Projection candidate；`Player.Update`/`Spawn` 调用 Update/Reset，但随机源和帧消费属于 Presentation/P11 handoff。 |
| **合计** | **112** | **13 个 claim 叶子组；111 fields + 1 property** | **所有者裁决保持 proposed；跨 partition 状态为 integration-review。** |

### 写入闭包与直接关系

- `Main.DoUpdateInWorld` 只对 `active` 玩家槽调用 `Player.Update(i)`，之后按 active/sleeping 状态累计人数；`Main.cs:11415-11452`。CPG API 在所选范围找到 `Main.cs` 到 `Player.Update(int)` 的一个静态调用点。该调用点不是 Player.Update 的全部调度关系。
- `Player.Update(int)` 内部在 `Player.cs:15725-15727` 依次调用 `UpdatePettingAnimal()`、`sitting.UpdateSitting(this)`、`sleeping.UpdateState(this)`；随后还有 `eyeHelper.Update(this)`。这些更新处于 Player tick 内，具体外部 schedule 与所有字段 writer 尚未闭合。
- `Player.StopVanityActions` 在 `Player.cs:19678` 同步按 petting、sitting、sleeping 次序停止；`Teleport`、`Spawn`、`KillMe` 都会调用它。该顺序是可观察的旧行为，不能改成无序广播或延迟队列。
- `MessageBuffer.cs:604-620` 的 type 12 先写 `SpawnX/Y`、`respawnTimer`、死亡计数、team，再调用 `Spawn`；`:652-730` 的 type 13 写入玩家输入、Potion of Return route、坐姿、petting 和睡眠网络位；`:740-770` 的 type 14 切换 `active` 并仅在状态变化时调用 PlayerConnect/PlayerDisconnect hooks；`:771` 起的 type 16 从生命值更新 `dead`。这些网络入口与本地 Player API 共同写入生命周期/交互事实。
- `Get-CpgMemberUses` 能返回 `AccessMode=Unknown` 的字段访问；只有赋值左侧可辨识的操作提升到直接写证据。方法 effects、属性 accessor、字段 alias 及 CPG shard 外的 writer 保持 `partial`/`unknown`，不把零命中解释为无关系。

## Boundary Role and Decision

### Proposed boundary decision

| 决定 | proposed 边界 | 理由与不变量 | 拒绝的替代 |
|---|---|---|---|
| `separate` | `PlayerLifecycleSystem`，仅覆盖 P04 身份激活、死亡/重生、spawn/return 的协调 API；写入身份、lifecycle、death record 和 spawn facts 的各自唯一 component 字段 | 都是玩家 session 生命周期转换；旧 Spawn 同时影响 active/dead/spectating/位置/重生状态，旧 KillMe 同时影响死亡记录与 dead/timer。先以同一同步 System API 协调可避免仅按字段拆 owner。Effect commit 通过 Combat/Items/Network/Persistence ports。 | 一个覆盖所有 Player gameplay 的巨型 System；把 DeathRecord、Respawn、active 重复保存并允许多系统独立写入 |
| `separate` | `PlayerRestInteractionSystem` coordinator，使用 petting/sitting/sleeping capability data，但只允许一个 rest mode authority | Player tick 更新三个 capability；StopVanityActions 按顺序同步终止它们。统一 owner 能维护互斥与 cleanup；目标定位和 network effects 不塞进纯 Query。 | 三个彼此独立的 tick/System 同时写 mode，或用一个无法表达 petting target/seat/bed facts 的最小布尔组件替代全部数据 |
| `separate`，owner 待裁决 | Player teleport transition API 挂接已有 Teleportation/Movement 的 System composition | `Teleport` 有可见的同步提交顺序，范围横跨 movement、world、network、presentation，适合显式协调，但 P04 无权宣布跨分区唯一 owner。 | 新建只写 `IsTeleporting` 的薄 System并宣称取代旧 Teleport；让 Network/Projection 自己写 position/authority |
| `keep`/delegated | Container、TileEntity、shop、door、item timing、targeting 与 runtime effect 保持现有域能力；P04 只提供连接点 | 这些字段服务于 P09、WorldInteraction、Commerce、Items、Movement、Wiring、Combat、Mount、Projectile 与 Presentation，不构成单一 P04 invariant。 | 由于来自同一 Player 类型就建立 `PlayerWorldInteractionSystem`/`PlayerRuntimeSystem` catch-all |
| `deferred` | `PlayerItemCheckContext.SkipItemConsumption` 保持栈内/调用内 payload；RabbitOrderFrame 保持 Presentation projection candidate | context 声明和 ItemCheck 使用点不能证明持久生命周期；rabbit frame 由 Player 侧表现 helper Update/Reset 使用。 | 为 payload 新建 persistent Component；让 gameplay lifecycle System 最终拥有帧动画 |

System 是行为执行边界，不要求一个 Component 只能由一个 System 读取。权威写入仍必须唯一；如 transaction 横跨多个 owner，由显式同步 API/commit protocol 协调，不能借共享 Component 默许双写。

### Query、Command、Adapter 和 Projection

- proposed Queries：`PlayerLifecycleSnapshotQuery`、`PlayerDeathRecordQuery`、`PlayerRestStateQuery`、`PlayerSpawnEligibilityQuery`。输入必须是稳定 snapshot/value，不写 Component、Tile、registry、clock、cache 或网络；不通过 out 参数回填 authority。
- `GetSittingTargetInfo` 和 `GetSleepingTargetInfo` 暂列 `QueryCandidate/partial`：它们读 tile/frame/registry 并含 out 结果，当前源码不足以证明目标解析的强纯度及 lazy tile 影响。先由 adapter 采集不可变 tile snapshot，再调用纯计算；在完成该契约前不能随意重排或重复调用。
- `FullyFallenAsleep` 是纯派生值候选；其布尔结果来自 `isSleeping` 与 120 tick threshold。`GetSittingOffsetInfo`/`GetSleepingOffsetInfo` 只有在以 snapshot 输入时才可作为 Query。
- proposed Commands 是同步 System API 的意图分类：`ActivatePlayer`、`DeactivatePlayer`、`ResolvePlayerDeath`、`SpawnPlayer`、`RequestPlayerTeleport`、`Begin/StopRestInteraction`。没有证据支持新增可排队命令对象或通用消息总线；失败结果、部分提交、幂等性和权限需要在各 API contract 中明确。
- Adapter：Network adapter 校验 packet/session identity 并转为上述 API；Spawn resolver/World adapter 读 tile 与 team/world spawn；Entity registry adapter 检查 NPC/Projectile/Mount/TileEntity；Save adapter 读写持久数据。Adapter 不复制生命周期规则。
- Projection：death/session/rest/teleport committed snapshot 单向供网络、UI、保存或 Presentation 消费；projection 不回写 authority。Rabbit frame 是表现 projection，不因旧实现保存在 Player helper 中而变成 lifecycle authority。

## System API and Legacy Behavior Mapping

迁移兼容按概念行为及观察向量核对：`result + rejection/exception + authoritative state delta + emitted events/effects + order/visibility + lifecycle/scope + retry/idempotency`。下表全为 `proposed`，不是当前 NLTX API。

| Legacy entry / source | Proposed API composition | 必须保持的旧行为与输出 | 状态 |
|---|---|---|---|
| `Main.DoUpdateInWorld -> Player.Update(i)` (`Main.cs:11415-11452`) | 既有 world/player tick orchestrator 调用多个 capability System；P04 在原 Player tick 可见顺序接入 Rest 与生命周期检查 | 只对 active slot 更新；不能把整个 `Update` 等同 Lifecycle API。Player.Update 含大量其他域行为，不在本分区复制或重排 | call-site `confirmed`；完整 schedule `partial` |
| `Player.KillMe(reason,dmg,direction,pvp)` (`Player.cs:22572-22678`)；静态调用含 `MessageBuffer.cs` 与 Player 内部入口 | `PlayerLifecycleSystem.ResolveDeath(playerKey, context)` 同步裁决前置条件/生命周期写入；DeathPenalty/Combat/Items/Network/Presentation adapters 提交各自效果 | 保留 creative/practice/already-dead 早退；StopVanityActions 在死亡写入前；PVP/PVE 计数、时间、位置、coin/drop、文本、timer、spectating 和外部效果的条件及顺序；`whoAmI != Main.myPlayer` 的末尾早退位置。跨域写入原子性待定义 | 入口及部分调用 `confirmed`；effects/callee closure `partial` |
| `Player.Spawn(context)` (`Player.cs:21798-21895`) | `PlayerLifecycleSystem.SpawnPlayer` + SpawnResolver Query(snapshot) + Movement commit + identity activation + network/save projections | 保留 world join 与 revive/recall 分支、death adjustment、vanity cleanup、active/spectating/dead 状态重置、spawn route 和 velocity/fall/immunity reset 顺序。Team spawn 选择体被清空，具体结果 `unknown` | 本地状态/分支 `confirmed`；部分路由 `unknown` |
| `Player.Teleport(newPos,style,extraInfo)` (`Player.cs:21731-21796`) | Teleportation composition: request validation -> stop rest/vanity -> movement commit -> world/pressure plate -> portal/pylon/ack state -> projections | 保留 style 条件、抓钩清除、shimmer 清理、Entry effect 在 position commit 之前、position/net offset/spectating/wall scan、Exit effect 及 timer 写入顺序。旧 catch 为空可能吞异常；异常和部分提交行为须作对照 | 11 个静态所选 call-site `complete`；全闭包 `partial` |
| `StopVanityActions`/petting/sit/sleep helpers (`Player.cs:19678-19715`; helper 文件) | Rest System 的同步协调 API，三个 capability 由单一 owner 清理；Movement/death/spawn/teleport 只调用协调 API | Stop 顺序、条件广播、input/target invalidation、seat/bed rotation/offset/index/timer 状态和同 tick 可见性。停止方法不默认排队 | 直接顺序 `confirmed`；target invalidation 及 stub 路径 `partial/unknown` |
| `MessageBuffer` case 12/13/14/16 与 teleport/death packet 分支 (`MessageBuffer.cs:604-771, 2337-2348, 2969`) | `PlayerNetworkAdapter` 进行 slot/session/auth translation，再同步调 System API；packet decode 不直接作第二 writer | type 12 先写 spawn/death/team 数据再 Spawn；type 13 处理 input/rest/return route；type 14 active 改变后 hooks 仅一次；type 16 life->dead；网络身份 `whoAmI`/player slot 映射不可丢 | 当前源码直接路径 `confirmed`；认证、重放和全部 packet consumers `partial` |
| `Player.SavePlayer`、`PlayerFileData.Player/LastPlayed` (`Player.cs:26394-26455`; `PlayerFileData.cs:10-92`) | Save adapter 形成校验后的 player persistence snapshot；成功提交后 lifecycle save checkpoint 才更新 | 保留 PlayerFileData 与实际 Player 的映射、时间戳的 binary 语义、失败时 last-saved 不前移；不把 save projection 当写入 owner | 序列化/反序列化方法为 stub，持久化 delta/version/error `unknown` |
| `ItemCheck()` 与本地 `ItemCheckContext` (`Player.cs:23435` 起) | 原 Items/System composition 使用调用内 context；P04 不建立 durable Query/Component | 输入消费、item timer、return/spawn 调用、Wiring/Combat side effects 需要沿 Items owner 单独追踪 | context 声明和本地 default `confirmed`；其字段消费者/闭包 `unknown` |
| `PlayerSleepingHelper.FullyFallenAsleep`、sitting/sleep offset helpers | 从 immutable rest/tile snapshot 求派生结果；Rest System 负责提交变更 | threshold 120；复用调用和 offset 不能隐式重置状态；输入 snapshot 版本/范围明确 | threshold `confirmed`；tile purity/stub 算法 `partial/unknown` |

**Runtime identity separation:** 运行时 player slot (`whoAmI`/Main.player index)、网络/session identity、persistent character identity 和本 runner 的 `sessionId` 是不同身份域。P04 runner id `910f8127b89041cc98bf563325fab350` 只用于本次文档会话 Complete；严禁把它当作任何游戏玩家 ID 或 API 参数。

## Call and Dependency DAG

### Source-observed edges

| Source -> target | 关系 | 证据 |
|---|---|---|
| `Main.DoUpdateInWorld` -> `Player.Update(int)` | active-player 主循环直接调用 | `Main.cs:11415-11430`；CPG `Find-CpgCallSites` 在所选 shard 返回 1 个 call-site；current source 对照确认 |
| `Player.Update(int)` -> `UpdatePettingAnimal` -> `PlayerSittingHelper.UpdateSitting` -> `PlayerSleepingHelper.UpdateState` | Player 内每 tick capability 更新顺序 | `Player.cs:15725-15727` |
| `Player.Teleport` -> `StopVanityActions` -> petting/sitting/sleeping stop | teleport 前同步清理 rest 状态 | `Player.cs:21731-21755,19678-19683` |
| `Player.Spawn` / `Player.KillMe` -> `StopVanityActions` | spawn/death 前同步清理 rest 状态 | `Player.cs:21808`、`:22572` 起、`:19678-19683` |
| `MessageBuffer` type 12 -> direct spawn state assignment -> `Player.Spawn` | 网络 spawn packet 的写入顺序 | `MessageBuffer.cs:604-620` |
| `MessageBuffer` type 13 -> input/rest/return state assignment | 网络 player update 的 direct write path | `MessageBuffer.cs:652-730` |
| `MessageBuffer` type 14 -> `active` transition -> `Player.Hooks.PlayerConnect/PlayerDisconnect` | active 改变后触发 lifecycle hooks | `MessageBuffer.cs:740-770` |

CPG 的 selected-source call graph 是若干静态 `CallTargets`，不是闭合 DAG。以下 proposed composition 只固定需要保留的接口顺序，实际框架 phase、订阅时点和并行约束仍待 integration review：

```text
Network / local intent adapters
  -> validate player/session identity
  -> PlayerLifecycleSystem or PlayerRestInteractionSystem API
  -> unique authoritative component commit
  -> Network / persistence / presentation projection

Main player tick
  -> legacy movement/input state available
  -> PlayerRestInteractionSystem.UpdatePlayerRest (proposed at legacy rest-update point)
  -> rest result visible to later Player update consumers

Teleport/Death/Spawn command
  -> synchronous stop-rest coordination
  -> lifecycle/teleport authoritative transition
  -> external domain commit ports
  -> projections after committed result
```

不得根据文件顺序安排 System。两个 writer 的冲突、业务先后、缓冲结构变化可见性是三个不同问题；本报告没有 scheduler/access-set 数据，因此并行许可与 barrier 均 `unknown`。已知旧入口是同步直接写时，不新增延迟 queue。

## Lifecycle and Side Effects

- **Create/connect/disconnect:** `MessageBuffer` type 14 在 slot 非活动到活动时可重建 `new Player()`，修改 active 后根据变化触发 connect/disconnect hook。`Spawn` 也设 `active=true`，因此 active 不是只由网络入口写。slot reuse 初始化、host/name/persistent identity 的重连协议未闭合。
- **Tick:** `Main` 过滤 active player，`Player.Update` 内顺序推进 movement 之后的 rest helper，调用后 Main 读取 `FullyFallenAsleep`。若迁移改变 rest 更新点或将 120 tick 派生状态缓存化，会改变同帧观察窗口。
- **Death:** `KillMe` 先做 creative/practice/already-dead early return，再停止 vanity、更新 PVP/PVE 和记载死亡位置/`DateTime.Now`，执行掉落、音效、随机粒子等效果，设置 dead/respawn/spectating 并广播文本及 coin/tombstone effects；末尾仅某些 `whoAmI` 分支继续。该方法横跨 Items、Combat、Network、Presentation 和 World，effects/error closure 不由 P04 独立宣布。
- **Spawn/reset:** `Spawn` 依 `PlayerSpawnContext` 调整世界进入时死亡 timer，停止 vanity，重置部分表现/兔子帧/环境/免疫/位置/速度，再设 active 并清 spectating；Team spawn helper 是空 stub (`Player.cs:22083`)，行为 `unknown`。进入顺序和 world spawn 搜索必须保留。
- **Teleport:** 方法可能移除抓钩、清 shimmer 状态与 buff、调用 Entry effect、先后更新 pressure plate/position/netOffset/spectating/wall scan/gravity/portal fields、发音效并调用 Exit effect，最后写 teleport timer/style。外层空 catch 影响失败可见性，必须纳入失败后状态 delta 对照，不能用普通成功返回取代。
- **Rest:** `StopVanityActions` 固定 stop 次序；sitting/sleeping 各自更新 tile eligibility、input、mount、方向和 stacking manager；停止可发 type 13 packet。`DoesPlayerHaveReasonToActUpInBed` (`PlayerSleepingHelper.cs:47`) 与 `SetOffsetbyBed` (`:181`) 是被清空/占位 body，相关入睡原因及偏移算法 `unknown`。查询不应调用带隐藏清理/缓存的 resolver。
- **Persistence:** `PlayerFileData.Player` 会同步其 Name；`LastPlayed` 从 `Player.lastTimePlayerWasSaved` 的 binary 值派生；`Main.ActivePlayerFileData` 是当前文件数据，Main tick 更新 play timer。`Player.SavePlayer` 委托 `InternalSavePlayerFile`/`Serialize`，这些实现为空 stub；`Deserialize` 也为空 stub。`WorldFile` 是世界档案，不可用它闭合 P04 player 序列化。数据版本、字段兼容、save failure/recovery 均 `unknown`。
- **Session/world scope:** Main 的全局固定 Player slots、network `whoAmI`、PlayerFileData、持久 ID 和未来 ECS Entity reference 不等价。是否支持一个进程内多 world/multi-session 并行、跨 world 持久恢复和销毁重建语义均 `unknown`。
- **Exception/retry:** `Main` Player.Update loop 有条件捕获并按 ignoreErrors 决定吞掉或重抛；Teleport 自身吞异常；KillMe/Spawn 的部分 effects 在状态写入前后混排。没有证据可安全重排、自动重试、补偿或声明幂等。

## Integration Handoff

subsystemId: authoritative-P04-player-lifecycle-interaction  
taskNumber: AUTH-SYS-P04  
reportPath: `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P04-player-lifecycle-interaction.md`

evidenceStatus: partial  
nltxStatus: partial  
verificationStatus: not-run

confirmedOwners:
- Version4 的 `Player.Update` 中 P04 petting/sitting/sleeping 更新次序及 `StopVanityActions` 的同步 stop 次序。
- Version4 `KillMe`、`Spawn`、`Teleport` 和 MessageBuffer 所读到的直接状态变更及外部调用顺序；不含 callee effect 的完整闭包。

proposedTypes:
- `PlayerLifecycleSystem` 与已有 identity/lifecycle/death-record/spawn component 的唯一写者方案（最终 ID/session owner pending）。
- `PlayerRestInteractionSystem` coordinator；petting/sitting/sleeping data 保留 capability 差异，共用一个 rest mode authority。
- Teleportation transition API composition、snapshot Query、Network/World/Entity registry/Save adapters、Network/Presentation projections。
- 同步 `ResolvePlayerDeath`、`SpawnPlayer`、`RequestPlayerTeleport`、rest start/stop API 草案；不新增通用队列或 CQRS。

sharedTypesForIntegrationReview:
- Player slot/session/persistent identity 与 Entity reference 的映射。
- Death/Combat/Items/DeathPenalty 的一次提交及其网络、存档输出。
- Teleportation、Movement、World pressure plate、Portal/Pylon 和 Network acknowledgement 状态。
- P09 container relation、WorldInteraction TileEntity/door/tile capability、Commerce shopping、Item/Movement/Wiring/Combat timer 分工。
- P11 RabbitOrderFrame/P11 presentation projection。

crossSubsystemReaders:
- Main/player tick、network state synchronization、Combat/Items、Movement/Teleportation、WorldInteraction/TileEntity、Commerce、Presentation 和 persistence consumers（具体完整列表 `partial`）。

crossSubsystemWriters:
- MessageBuffer type 12/13/14/16、Teleport/death packet handlers、Player internal call paths、save/restore adapter candidate。索引范围外/动态写入仍 `unknown`。

orderingConstraints:
- Rest stop 在 Version4 Teleport/Spawn/KillMe 继续前同步发生。
- 死亡记录、life-state、掉落及通知顺序需按旧 `KillMe` 条件保留；其跨域 commit boundary 待裁决。
- network spawn type 12 的直接输入字段写入先于 `Spawn`；type 14 hooks 只在 active 变化后发生。
- Player tick 的 rest update 保持在旧 `Player.Update` 内部相同观察点；完整 System schedule 尚未证明。

boundaryChallenges:
- `active` 同时由网络连接包与 Spawn 写入；不得由 Identity 和 Lifecycle 两个独立 System 双写。
- lifecycle/death state 在多个 NLTX component 中重复；rest aggregate mode 与三个独立 bool 重复。
- P04 不接管 P09 容器事实，也不将多个相邻领域归为 `PlayerWorldInteractionSystem`。
- `ItemCheckContext` 是临时 payload；RabbitOrderFrame 是表现候选。

evidenceGaps:
- CPG 无 SourceSnapshotId/per-file hash；AccessMode unknown、callee effects 未展开。
- KillMe、Teleport、Spawn 的全部 effects、错误后部分提交、重试及动态入口闭包未完成。
- player save/restore 序列化方法和 Team spawn helper 是 stub；save version/recovery unknown。
- Session/persistent identity 与 slot、ECS entity 的实际绑定，以及多个 world/session scope 未确认。
- petting/seat/bed registry、lazy tile、stack manager 和 Rest packet 的所有读写者未闭合。
- 当前 NLTX P04 Component 没有观察到生产 System consumer；组件重复权威尚未裁决。

blockingDecisions:
- **Session/lifecycle owner:** 选项 A 由单一 PlayerLifecycleSystem 协调 identity activation 与 death/spawn 状态，降低跨 owner 原子性风险；选项 B 由 Session owner 写 `active`、Lifecycle API 通过同步 session command 协调，边界清晰但必须定义 commit/失败可见性。等 identity/session integration review 定案。
- **Rest state authority:** 选项 A 以 aggregate `Mode` 唯一表示互斥，capability component 保存额外数据；选项 B 以 capability flags 为权威并由一个 Rest System 验证互斥。当前 NLTX 两类表示并存，不得同时作为 authority。
- **Cross-domain death/teleport transaction:** Combat/DeathPenalty/Items 与 Movement/World/Network effect 的 commit 和失败补偿边界须由 P04 与相邻 owner 联合决定；本报告不宣布最终跨分区 owner。

notImplemented:
- 当前目标源码中未发现连接本报告所列 P04 lifecycle/rest/teleport components 的生产 System API 或 consumer call-site。
- 不存在本次新建的 P04 System、Query、Command、Adapter、Projection 或行为验证结果。

verifierPlan:
- activation/deactivation 与 slot reuse：状态变化时 hook 恰一次，spawn 与 disconnect 不产生双 writer。
- death resolution：creative/practice/already-dead early return、PVP/PVE、重复死亡、coin/drop/text、respawn/spectating、末尾 local-player 分支；观察完整 state/effect/order/error 向量。
- spawn/return：各 PlayerSpawnContext、个人/team/world 路由、stub blocking、免疫/速度/active/spectating 提交及网络可见性。
- teleport：Style 4/9/10 等分支、grapple/shimmer/pressure plate/portal metadata、ack/position/Entry/Exit 顺序和抛错/吞错后的部分提交。
- rest：petting/sitting/sleeping 互斥、停止次序、input/target失效、seat/bed eligibility、120 tick、网络广播去重、连续 Query 不写状态。
- persistence：SavePlayer/Deserialize 完整实现后验证 timestamp、版本/缺字段、失败恢复和身份绑定；当前 stub 条件下不能形成通过判定。
- ItemCheck/Timing 与 Items、Movement、Wiring、Combat owner 联调；禁止仅比较组件最终字段，需包括重复调用、同 tick 可见性和外部 effects。

## Migration Behavior Contract

以下是迁移前的候选合同，所有条目均为 `proposed`，不是行为等价结论：

| Contract ID | 输入/作用域 | authority 与提交 | 错误、顺序、可观察输出 | 当前状态 |
|---|---|---|---|---|
| `player.lifecycle.activate` | validated session identity + entity/slot binding | 唯一 identity/lifecycle owner 写 active/host/name；runtime slot 与 persistent ID 分离 | 若 active 未变化不重复发 connect/disconnect hook；Spawn activation 与 packet activation 要幂等。完整重连语义 `unknown` | proposed / blocking owner |
| `player.lifecycle.death` | death reason、damage、direction、pvp、现有 lifecycle snapshot | 同步判定 early return；一个受控 API 协调 death record 与 life state，外域 effect 由明确 port 提交 | 保留原 `KillMe` 顺序、局部/全局随机和 DateTime effect、partial commit 及 local-player branch；不得直接自动重试 | proposed / effects partial |
| `player.lifecycle.spawn` | SpawnContext、死亡/计时、spawn/return snapshot、World snapshot | validated Spawn API commit identity/life/movement through owner APIs | 保留各 context 的条件 reset 与原同步可见点；空 team spawn helper 阻止该分支 equivalence 判断 | proposed / route unknown |
| `player.teleport` | player identity、目标、style、extraInfo、world/tile + pending ack | Teleportation/Movement composition commit position；仅一个 owner 写 transition/ack | 保留 effect、position、pressure plate、network、error ordering；无同步 snapshot 时不能做资格 Query 后直接提交 | proposed / integration-review |
| `player.rest.transition` | player key、intent、input/tile/entity snapshot、tick | 单一 Rest owner 提交 mode 与能力数据；stop API 同步调用 | rest target失效/重复 stop/广播可重入和去重规则按旧条件；查询重排仅在纯 snapshot contract 成立后允许 | proposed / target effects partial |

每个合同的行为对照必须使用结果、拒绝/异常、权威状态 delta、事件/外部效果、顺序/可见性、生命周期作用域、重试/幂等性作为观察向量。字段相同、旧 facade 还可调用、编译或报告完成均不能替代该观察。

## Evidence Gaps and Blocking Decisions

1. **唯一写者未闭合。** `active` 同时由 packet 14 与 Spawn 写；death fields 同时分布在 lifecycle/death-record components；rest mode/flags 重复。整合必须先选定一个 authority 和跨 owner commit protocol。
2. **API/callee closure 未闭合。** CPG 有静态 call-site 与部分赋值 shape；`Get-CpgCallableFacts(KillMe)` 标记 partial，callee effects、反射、hooks、alias 和完整 inbound caller 不由查询结果保证。关键源代码已回查，但其他 packet、hook 和 runtime entry 仍待审查。
3. **存档事实缺失。** Player 序列化与反序列化入口在当前 Version4 是 stub；`PlayerFileData` 可证 `LastPlayed` 派生，不能证明 112 成员保存集合。`WorldFile` 不补足 player serializer。
4. **spawn/rest 结果有空实现。** Team spawn、sleep reason 和 bed style offset helper 不能由 stub 还原。依赖这些分支的设计及 verifier 留作 `unknown`/`blocked-by-evidence`，不推测默认行为。
5. **Scheduler/session 证据缺失。** 当前只确认 Player tick 内局部顺序和 Main active loop；没查到 ECS scheduler 注册、event dispatch、access declaration 或 multi-world lifecycle contract。不能称作真实 System DAG。
6. **跨分区 owner 未定。** Teleport、Combat/DeathPenalty、Items、Movement、P09 containers、WorldInteraction、Commerce 与 P11 Presentation 都需 integration review。P04 不为它们重分配成员。

## Verification Plan

`verificationStatus: not-run`。本次只做文档静态证据整理；未运行 build、test、run 或行为 verifier。未修改生产源码、测试、项目文件、输入 report、claim prompt 或其他报告；只写本 claim 授权的 `outputReport`。完整 verifier 计划见 Integration Handoff。

没有行为测试覆盖新 System composition，因此本文不声称 API 等价、行为等价、迁移成功或运行时接入完成。runner `Complete` 只结算本次文档任务。

本报告是依据 Version4 当前源码、只读 CPG 查询、当前 NLTX 组件树和有限 SS14 System 结构参考形成的 proposed System/API 分解。缺少闭包、实现与行为验证的关系保持 `partial` 或 `unknown`，跨 partition owner 留给 integration review。
