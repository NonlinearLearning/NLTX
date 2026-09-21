# Version4 非权威 P06：实体生命周期与归因组件设计提案

partitionId: P06
sessionId: 5e39d05ed99c42b49a89f98d21f791d6
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\06-entity-lifecycle-attribution.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-component-execution.md
designStatus: proposed
executionStatus: completed
implementationStatus: completed
verificationStatus: independently-verified
completedComponents:
- C01 EntityIdentityAndMotionState
- C02 SharedEntitySourceAndAttribution
- C03 MainEntityPoolsAndWorldSlots
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T01:55:40Z
evidence-gap: Direct lifecycle evidence is strong for the reviewed Version4 paths, but complete writer coverage, persistence schemas, replication ownership, cross-partition EntityReference contracts, and final scheduler ownership remain partial or missing; tModLoader is boundary corroboration only and SS14 has no direct core EntityManager/EntityUid evidence in the searched mirror.
blocking-decision: Integration review must decide the cross-partition owners and commit order for EntityId, compatibility slot/generation handles, NetworkId, PersistentId, EntityReference, attribution snapshots, and the final spawn-update-death-recycle scheduler transaction.

implementationCheckpoint:
  completedImplementation: C03 MainEntityPoolsAndWorldSlots
  currentImplementation: none
  modifiedSrc2Files:
  - src2\EntityLifecycleAttribution\EntitySlotHandle.cs
  - src2\EntityLifecycleAttribution\EntityReference.cs
  - src2\EntityLifecycleAttribution\EntitySlotBindingComponent.cs
  - src2\EntityLifecycleAttribution\LocationComponent.cs
  - src2\EntityLifecycleAttribution\VelocityComponent.cs
  - src2\EntityLifecycleAttribution\DirectionComponent.cs
  - src2\EntityLifecycleAttribution\MotionHistoryComponent.cs
  - src2\EntityLifecycleAttribution\MotionHistoryCommitSystem.cs
  - src2\EntityLifecycleAttribution\EntityIdentityState.cs
  - src2\EntityLifecycleAttribution\EntityIdentityComponent.cs
  - src2\EntityLifecycleAttribution\EntitySpawnSourceKind.cs
  - src2\EntityLifecycleAttribution\TileCoordinate.cs
  - src2\EntityLifecycleAttribution\EntitySpawnSourceContext.cs
  - src2\EntityLifecycleAttribution\LegacyEntitySourceRecord.cs
  - src2\EntityLifecycleAttribution\LegacyEntitySourceAdapter.cs
  - src2\EntityLifecycleAttribution\EntityProvenanceComponent.cs
  - src2\EntityLifecycleAttribution\EntityProvenanceCommitSystem.cs
  - src2\EntityLifecycleAttribution\PlayerDeathAttributionSnapshot.cs
  - src2\EntityLifecycleAttribution\DeathAttributionQuery.cs
  - src2\EntityLifecycleAttribution\DeathAttributionCaptureSystem.cs
  - src2\EntityLifecycleAttribution\DeathCauseProjection.cs
  - src2\EntityLifecycleAttribution\LegacyDeathReasonNetworkAdapter.cs
  - src2\EntityLifecycleAttribution\SourceAttributionQuery.cs
  - src2\EntityLifecycleAttribution\EntitySlotPoolKind.cs
  - src2\EntityLifecycleAttribution\WorldItemComponent.cs
  - src2\EntityLifecycleAttribution\WorldItemSlotStore.cs
  - src2\EntityLifecycleAttribution\NpcEntitySlotStore.cs
  - src2\EntityLifecycleAttribution\ProjectileEntitySlotStore.cs
  - src2\EntityLifecycleAttribution\ProjectileIdentityIndex.cs
  - src2\EntityLifecycleAttribution\ProjectileIdentityKey.cs
  - src2\EntityLifecycleAttribution\PresentationEffectKind.cs
  - src2\EntityLifecycleAttribution\PresentationEffectPoolStore.cs
  - src2\EntityLifecycleAttribution\WorldChestState.cs
  - src2\EntityLifecycleAttribution\WorldContainerStore.cs
  - src2\EntityLifecycleAttribution\WorldSignState.cs
  - src2\EntityLifecycleAttribution\WorldSignStore.cs
  - src2\EntityLifecycleAttribution\ItemAnimationDefinition.cs
  - src2\EntityLifecycleAttribution\ItemAnimationCatalog.cs
  - src2\EntityLifecycleAttribution\EntityPoolSet.cs
  - src2\EntityLifecycleAttribution\EntityPoolInitializationSystem.cs
  - src2\EntityLifecycleAttribution\AllocateEntitySlotCommand.cs
  - src2\EntityLifecycleAttribution\ReleaseEntitySlotCommand.cs
  - src2\EntityLifecycleAttribution\EntitySpawnCommitSystem.cs
  - src2\EntityLifecycleAttribution\EntityDespawnRecycleSystem.cs
  - src2\EntityLifecycleAttribution\WorldContainerLifecycleSystem.cs
  - src2\EntityLifecycleAttribution\PresentationEffectPoolSystem.cs
  - src2\EntityLifecycleAttribution\ItemAnimationCatalogSystem.cs
  - src2\EntityLifecycleAttribution\Terraria.EntityLifecycleAttribution.csproj
  - src2\EntityLifecycleAttributionVerification\Program.cs
  - src2\EntityLifecycleAttributionVerification\Terraria.EntityLifecycleAttributionVerification.csproj
  verificationNote: C01/C02/C03 source and focused verifier are saved under src2. The final serial build completed with exit code 0, 0 warnings, and 0 errors; the no-build/no-restore focused verifier completed with exit code 0 and output `P06 verifier passed.`
  verificationEvidence:
    status: independently-verified
    buildCommand: `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\EntityLifecycleAttributionVerification\\Terraria.EntityLifecycleAttributionVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`
    buildProject: src2\EntityLifecycleAttributionVerification\Terraria.EntityLifecycleAttributionVerification.csproj
    buildExitCode: 0
    buildWarnings: 0
    buildErrors: 0
    artifacts:
    - `D:\TRbackup\NLTX\Build\bin\Terraria.EntityLifecycleAttribution\Debug\net10.0\Terraria.EntityLifecycleAttribution.dll`
    - `D:\TRbackup\NLTX\Build\bin\Terraria.EntityLifecycleAttributionVerification\Debug\net10.0\Terraria.EntityLifecycleAttributionVerification.dll`
    verifierCommand: `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\EntityLifecycleAttributionVerification\\Terraria.EntityLifecycleAttributionVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`
    verifierProject: src2\EntityLifecycleAttributionVerification\Terraria.EntityLifecycleAttributionVerification.csproj
    verifierExitCode: 0
    verifierOutput: `P06 verifier passed.`

> 本文是非权威的 proposed 组件设计。它只把 P06 的 44 条 Version4 成员映射为候选的 Component、System、Query、Command、Adapter 或 Projection，不表示 NLTX 已经实现、迁移完成、行为等价、网络/持久化闭合或跨分区 owner 已确定。所有候选共享边界均保留 `crossSubsystemOwner: integration-review`。

## 1. 范围与排除范围

### 1.1 范围

本分区包含三个叶子组，共 43 个字段和 1 个属性：

| 叶子组 | 成员 | 设计关注点 |
|---|---:|---|
| `EntityIdentityAndMotionState` | 7 | 实体运行时槽位句柄、当前位置/速度、方向及上一帧历史 |
| `SharedEntitySourceAndAttribution` | 23 | 生成来源上下文、死亡原因事件载荷、外部对象和内容 ID 的适配 |
| `MainEntityPoolsAndWorldSlots` | 14 | 实体运行时槽、世界物品复用门、投射物归因索引、箱子/标牌槽及表现池 |

### 1.2 排除范围

- 不重新定义 NPC、投射物、玩家、物品、战斗、掉落、死亡惩罚、空间、网络、存档或 UI 分区的最终组件 owner。
- 不把 `Main` 数组直接等同于 ECS 的持久化存储；数组索引只能作为兼容槽位或本地运行时句柄候选。
- 不把 `PlayerDeathReason` 或任何 `EntitySource_*` 对象作为长生命周期通用组件；它们应保持事件/上下文语义。
- 不在本分区裁决最终 `EntityId`、`NetworkId`、`PersistentId`、`EntityReference`、归因值对象或跨域调度顺序；这些均是 `crossSubsystemOwner: integration-review`。
- 不创建、移动或修改 C#、测试、项目文件、Version4 源码、第一轮报告、其他分区文档、ledger 或 lock。

## 2. 证据和现状

### 2.1 Version4 事实

证据以 `D:\TRbackup\Version4` 为事实来源；输入库存 SHA-256 为 `b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`。

| 证据 | 已确认事实 | 设计含义 |
|---|---|---|
| `Terraria/Entity.cs:6-20` | `whoAmI`、`position`、`velocity`、`oldPosition`、`oldVelocity`、`oldDirection`、`direction` 是实体基类字段。 | 位置、速度、方向历史可按内聚概念拆分；`whoAmI` 是兼容槽位候选，不是持久化身份。 |
| `Terraria/Entity.cs:48-183` | 几何属性由位置和尺寸派生。 | 几何值应保持 Query/Projection，不复制进身份或运动组件。 |
| `Terraria/Main.cs:926-957` | 14 个静态数组/索引分别持有表现效果、世界物品、NPC、投射物、投射物归因索引、箱子、标牌和物品动画注册状态。 | 必须按生命周期和读写模式拆成多个 store/catalog，不能形成一个通用池组件。 |
| `Terraria/Main.cs:3452-3505` | 启动阶段初始化多个槽并为若干数组实体设置 `whoAmI`。 | 槽位绑定是显式初始化步骤；分配、激活和发布必须有提交边界。 |
| `Terraria/Main.cs:11411` onward | 玩家、NPC、投射物、世界物品更新及后续网络/世界工作有固定的现有调用序列。 | 候选 scheduler 必须显式表示阶段，不可依靠文件顺序。 |
| `Terraria/Main.cs:11517-11520`, `11534-11543`, `11560-11565` | 失败路径会替换 NPC、投射物或物品槽对象。 | 需要失效旧引用、拒绝重复提交和 generation/stale-handle 验证。 |
| `Terraria/Main.cs:1546-1565` | 物品动画注册表可注册并清理动画。 | 这是按内容 ID 的 catalog/注册 bookkeeping，不是实体状态。 |
| `Terraria/Projectile.cs:10245-10292` | 投射物分配槽、设置 `whoAmI`、owner、identity 并写入 `Main.projectileIdentity`。 | `owner + identity` 是查找键到本地槽位的索引，不是通用实体身份。 |
| `Terraria/Projectile.cs:18525-18546`, `46425-46435` | 通过 owner/uuid 查找活动投射物，并在 kill 时清理索引。 | 索引有独立写入 owner、active 检查和清理顺序。 |
| `Terraria/Item.cs:48686-48859` | 世界物品生成会清理复用延迟、替换槽对象、赋 `whoAmI`、初始化运动并发送网络数据；选槽时检查 active、年龄和延迟。 | 世界物品 slot store 需要分配/延迟/发布/网络 Projection 边界。 |
| `Terraria/WorldItem.cs:357-377` | 世界物品更新会重设槽绑定、递减复用延迟，并在不可复用时返回。 | 复用门与世界物品生命周期由同一 slot owner 管理，但表现更新不应写入其他池。 |
| `Terraria/Chest.cs:68-120`, `418-555`, `1226-1265` | 箱子槽有清空、创建、查找、空后删除和帧/使用更新路径。 | 箱子是世界容器状态和槽位生命周期，不是实体运动组件。 |
| `Terraria/Sign.cs:1-79` | 标牌按坐标查找/懒创建/写文本，后备 tile 失效时作废。 | 标牌需要坐标索引、失效和文本投影边界。 |
| `PlayerDeathReason.cs:8-172` | 八个来源字段、`SourceProjectileType` 派生属性、工厂、死亡文本和二进制读写共存。 | 事件快照、纯派生查询和网络适配必须分离；不保留原始可变对象引用。 |
| `Player.cs:4237-4297`, `22336-22339`, `22660-22661` | 玩家创建来源上下文，并读取投射物来源类型/死亡文本。 | 来源采集与死亡投影有清晰消费点，但最终战斗 owner 仍需整合。 |
| `MessageBuffer.cs:2940-2970`, `NetMessage.cs:1426-1440`, `2338-2357` | 死亡原因在网络读写和玩家伤害/死亡包中传递。 | 网络适配器只传输版本化快照，不暴露 Version4 对象类型。 |

### 2.2 外部文档和 NLTX 现状

- 本地 tModLoader 镜像为 `tModLoader v2026.07`。`class_entity.html` 说明位置/速度/上一帧字段和 `whoAmI`；它明确指出 `whoAmI` 是实体专属数组索引，且投射物在多人客户端之间不稳定。`class_main.html` 说明 `Main.item`/`Main.npc` 的活动槽语义和 `Item.NewItem`/`NPC.NewNPC` 入口。`interface_i_entity_source.html` 将 `IEntitySource` 定义为解释实体/物品为何生成的上下文。`class_player_death_reason.html` 与 `class_player.html` 交叉确认死亡原因工厂、读写和消费边界。
- SS14 搜索镜像只找到内容/集成代码中的 `SpawnEntity` 和 `DeleteEntity` 使用，未找到可直接引用的核心 `EntityManager`/`EntityUid` 源码；因此这里只记录结构粒度参考，不把 SS14 作为本分区事实证据：`Space Station 14 无直接对应证据；以下边界仅由 Version4 真实代码和 NLTX 项目约束决定。`
- 当前 NLTX 的 `src\Share\Entity\Components\EntityIdentityComponent.cs`、`EntityIdentityState.cs`、`LocationComponent.cs`、`VelocityComponent.cs`、`DirectionComponent.cs`、`MotionHistoryComponent.cs`、`NetworkEntityId.cs`、`EntityProvenanceComponent.cs`、`EntityLifecycleState.cs`、`EntityProvenanceKind.cs`、`SpawnAdmissionState.cs`，以及 `src\WorldStorage`、`src\Relationships`、`src\Items`、`src\Projectile`、`src\Combat` 下相关文件，只能作为 partial proposed shape 证据。它们没有证明分配、generation 校验、释放、网络/存档闭合或行为等价已经存在。

## 3. Proposed 组件和边界

### C01 `EntityIdentityAndMotion`

`C01` 只表达实体身份/槽位绑定和运动基础状态。它不拥有实体池容量、来源归因、死亡文本、碰撞几何、网络快照或持久化格式。

Proposed 类型和路径：

- `proposed src2\Share\Entity\Components\EntitySlotBindingComponent.cs`: 兼容槽位、generation 和可验证的 `EntityHandle` 关系；`whoAmI` 只进入兼容字段。
- `proposed src2\Share\Entity\Components\LocationComponent.cs`: 当前权威位置；当前 NLTX 同名文件只算 partial skeleton。
- `proposed src2\Share\Entity\Components\VelocityComponent.cs`: 当前权威速度。
- `proposed src2\Share\Entity\Components\MotionHistoryComponent.cs`: 上一位置、上一速度、上一方向；由历史提交 system 在移动前/帧边界写入。
- `proposed src2\Share\Entity\Components\DirectionComponent.cs`: 当前朝向。
- `proposed src2\Share\Entity\Components\EntityIdentityComponent.cs` 与 `EntityIdentityState.cs`: runtime `EntityId`、兼容槽位、generation、候选 network/persistent 引用的最小身份状态；最终跨域 owner 未定。

不变量：槽位句柄必须绑定 generation；释放后旧句柄不能解析到新实体；当前位置/速度只有一个明确写入 owner；历史字段只在规定帧边界推进；几何属性由只读 Query 派生。

### C02 `EntitySourceAttribution`

`C02` 把生成来源和死亡原因分为短生命周期 payload、可选的 retained provenance 和单向适配器。来源上下文只能持有稳定的核心引用/内容 ID 值，不能把 `Terraria.Item`、`Terraria.Chest` 或其他第三方对象放进核心 ECS 状态。

Proposed 类型、端口和路径：

- `proposed src2\EntityLifecycle\Attribution\EntitySpawnSourceContext.cs`: 事件/命令输入值，包含来源种类、核心 entity reference、tile coordinate、item/projectile/mount/ammo/source content IDs。
- `proposed src2\Share\Entity\Components\EntityProvenanceComponent.cs`: 只有跨帧确实需要的最小 retained provenance；不能默认保存完整来源链。
- `proposed src2\Combat\Attribution\PlayerDeathAttributionSnapshot.cs`: 死亡处理和网络投影使用的不可变快照；`SourceProjectileType` 由 Query 派生，不单独存为组件字段。
- `proposed src2\EntityLifecycle\Attribution\EntityProvenanceCommitSystem.cs`、`DeathAttributionCaptureSystem.cs`: 将 command/event payload 提交到唯一 owner 或创建快照。
- `proposed src2\EntityLifecycle\Attribution\SourceAttributionQuery.cs`、`DeathCauseProjection.cs`: 只读归因查询和表现/死亡文本 projection。
- `proposed src2\Compatibility\Terraria\LegacyEntitySourceAdapter.cs`、`LegacyDeathReasonNetworkAdapter.cs`: 隔离 Version4 类型和二进制包格式。

不变量：来源上下文在生成提交后只读；死亡快照在发布后只读；旧槽位引用必须通过 generation/active 查询解析；`SourceProjectileType` 在没有本地投射物来源时返回 absent；网络输入不得直接改变核心归因状态；跨分区归因 owner 为 `crossSubsystemOwner: integration-review`。

### C03 `EntityPoolsAndWorldSlots`

`C03` 按生命周期和副作用拆分 `Main` 的数组，而不是创建通用 `UniversalEntityComponent`。建议的 proposed stores/catalogs：

- `proposed src2\WorldStorage\PresentationEffectPoolStore.cs`: dust、star、gore、rain、combat text 等短生命周期表现/天气池；不进入权威持久化实体集合。
- `proposed src2\WorldStorage\WorldItemSlotStore.cs`: world item 槽、活动状态、年龄和 `timeItemSlotCannotBeReusedFor` 复用门；与 `WorldItemComponent` 组合。
- `proposed src2\WorldStorage\NpcEntitySlotStore.cs`: NPC 运行时槽；NPC 状态 owner 交给 NPC 分区整合。
- `proposed src2\WorldStorage\ProjectileEntitySlotStore.cs` 与 `ProjectileIdentityIndex.cs`: 投射物槽和 owner/identity 到本地槽的索引；索引失效必须先于槽复用。
- `proposed src2\WorldStorage\WorldContainerStore.cs`、`WorldChestState.cs`、`WorldSignStore.cs`、`WorldSignState.cs`: 箱子/标牌世界对象及坐标/槽位索引；tile 合法性验证交给世界/tile 分区。
- `proposed src2\Content\ItemAnimationCatalog.cs`: `itemAnimations` 和 `itemAnimationsRegistered` 的按内容 ID catalog/注册集合。

Proposed systems/commands：`EntityPoolInitializationSystem`、`EntitySpawnCommitSystem`、`EntityDespawnRecycleSystem`、`WorldContainerLifecycleSystem`、`PresentationEffectPoolSystem`、`ItemAnimationCatalogSystem`、`AllocateEntitySlotCommand`、`ReleaseEntitySlotCommand`。这些名称、路径和接口全部是 proposed；每个 store 只有一个写入 owner。

不变量：分配、初始化、激活、发布、死亡、解除引用、释放、generation 增加是可观察的阶段；重复释放幂等或显式拒绝；池耗尽返回可诊断的 admission failure；表现池不参与持久化；`projectileIdentity` 的 owner/identity 键不能越过 active/generation 检查。

## 4. 逐成员 Proposed 归属

下表覆盖输入报告的全部 44 条成员。`事实证据`只描述已在 Version4 中看到的声明或调用；`候选归属`是 proposed 设计，不是当前实现。

| 序号 | Version4 成员 | 候选归属 | 状态/生命周期 | 证据状态 |
|---:|---|---|---|---|
| 376 | `Main.dust` | C03 `PresentationEffectPoolStore` | 短生命周期表现池；不持久化 | existing-evidence: `Main.cs:930`, `Dust.cs:49-97` |
| 377 | `Main.star` | C03 `PresentationEffectPoolStore` | 天空/星体表现池；按帧或场景清理 | existing-evidence: `Main.cs:932`, `Star.cs:77-129` |
| 378 | `Main.item` | C03 `WorldItemSlotStore` + `WorldItemComponent` | 世界物品活动槽；生成/更新/回收 | existing-evidence: `Main.cs:934`, `Item.cs:48686-48859`, `WorldItem.cs:357-377` |
| 379 | `Main.timeItemSlotCannotBeReusedFor` | C03 `WorldItemSlotStore` reuse gate | 槽位复用延迟；随帧递减 | existing-evidence: `Main.cs:936`, `Item.cs:48686-48859`, `WorldItem.cs:357-377` |
| 380 | `Main.npc` | C03 `NpcEntitySlotStore` | NPC 运行时槽；失败路径可替换 | existing-evidence: `Main.cs:938`, `Main.cs:3452-3505`, `11411+` |
| 381 | `Main.gore` | C03 `PresentationEffectPoolStore` | 短生命周期表现对象；不持久化 | existing-evidence: `Main.cs:940`, `Gore.cs:45-67` |
| 382 | `Main.rain` | C03 `PresentationEffectPoolStore` | 天气表现池；由天气阶段更新 | existing-evidence: `Main.cs:942`, `Main.cs:11411+` |
| 383 | `Main.projectile` | C03 `ProjectileEntitySlotStore` | 投射物活动槽；分配/更新/kill/recycle | existing-evidence: `Main.cs:944`, `Projectile.cs:10245-10292`, `46425-46435` |
| 384 | `Main.projectileIdentity` | C03 `ProjectileIdentityIndex` | `(owner, identity)` 到本地槽的缓存索引；kill 时清除 | existing-evidence: `Main.cs:946`, `Projectile.cs:18525-18546`, `46425-46435` |
| 385 | `Main.combatText` | C03 `PresentationEffectPoolStore` | 短生命周期 UI/表现池；不持久化 | existing-evidence: `Main.cs:948`, `CombatText.cs:52-75` |
| 386 | `Main.chest` | C03 `WorldContainerStore` / `WorldChestState` | 世界容器槽；清空后可删除 | existing-evidence: `Main.cs:950`, `Chest.cs:68-120`, `491-555` |
| 387 | `Main.sign` | C03 `WorldSignStore` / `WorldSignState` | 坐标索引的世界文本；tile 失效时作废 | existing-evidence: `Main.cs:952`, `Sign.cs:1-79` |
| 388 | `Main.itemAnimations` | C03 `ItemAnimationCatalog` | 按 item content ID 的定义/缓存 | existing-evidence: `Main.cs:954`, `Main.cs:1546-1565` |
| 389 | `Main.itemAnimationsRegistered` | C03 `ItemAnimationCatalog` registration set | catalog 注册 bookkeeping；可清理/重建 | existing-evidence: `Main.cs:957`, `Main.cs:1546-1565` |
| 1030 | `AEntitySource_OnHit.EntityStriking` | C02 `EntitySpawnSourceContext` hit attribution | 生成/命中事件 payload；提交后只读 | existing-evidence: `AEntitySource_OnHit.cs:5`, `Player.cs:4237-4297` |
| 1031 | `AEntitySource_OnHit.EntityStruck` | C02 `EntitySpawnSourceContext` hit attribution | 目标实体引用；必须经 EntityReference/generation 验证 | existing-evidence: `AEntitySource_OnHit.cs:7`; crossSubsystemOwner: integration-review |
| 1032 | `AEntitySource_Tile.TileCoords` | C02 source context tile coordinate | 事件输入坐标；不等同 tile entity identity | existing-evidence: `AEntitySource_Tile.cs:7`; tile owner deferred |
| 1126 | `EntitySource_ByItemSourceId.Entity` | C02 source context entity reference | 创建来源实体引用；短生命周期 | existing-evidence: `EntitySource_ByItemSourceId.cs:5` |
| 1127 | `EntitySource_ByItemSourceId.SourceId` | C02 external/content source ID | item source/content ID；不等同 EntityId | existing-evidence: `EntitySource_ByItemSourceId.cs:7`; owner deferred |
| 1128 | `EntitySource_ByProjectileSourceId.SourceId` | C02 external/content source ID | projectile source/content ID；不等同 projectile slot | existing-evidence: `EntitySource_ByProjectileSourceId.cs:5` |
| 1131 | `EntitySource_ItemUse_WithAmmo.AmmoItemIdUsed` | C02 external item content ID | 生成事件输入；不持久化为实体身份 | existing-evidence: `EntitySource_ItemUse_WithAmmo.cs:5`; item owner deferred |
| 1132 | `EntitySource_ItemUse.Entity` | C02 source context entity reference | 施法/使用者引用；短期 payload | existing-evidence: `EntitySource_ItemUse.cs:5` |
| 1133 | `EntitySource_ItemUse.Item` | C02 `LegacyEntitySourceAdapter` external item handle | `Terraria.Item` 只在 adapter 边界可见 | existing-evidence: `EntitySource_ItemUse.cs:7`; external type boundary |
| 1135 | `EntitySource_Mount.Entity` | C02 source context entity reference | mount 使用者引用；短期 payload | existing-evidence: `EntitySource_Mount.cs:5` |
| 1136 | `EntitySource_Mount.MountId` | C02 external mount/content ID | 内容定义 ID；不等同 EntityId | existing-evidence: `EntitySource_Mount.cs:7`; mount owner deferred |
| 1137 | `EntitySource_OverfullChest.Chest` | C02 `LegacyEntitySourceAdapter` external chest handle | overflow 事件输入；不得进入核心 ECS | existing-evidence: `EntitySource_OverfullChest.cs:5`; chest owner deferred |
| 1138 | `EntitySource_Parent.Entity` | C02 source context parent reference | 父实体引用；需处理父实体先销毁 | existing-evidence: `EntitySource_Parent.cs:5`; crossSubsystemOwner: integration-review |
| 1139 | `EntitySource_TileInteraction.Entity` | C02 source context entity reference | tile interaction 事件输入 | existing-evidence: `EntitySource_TileInteraction.cs:5`; tile owner deferred |
| 1216 | `PlayerDeathReason._sourcePlayerIndex` | C02 `PlayerDeathAttributionSnapshot` | 死亡事件中的兼容玩家槽/索引；网络读写值 | existing-evidence: `PlayerDeathReason.cs:8`, `MessageBuffer.cs:2940-2970` |
| 1217 | `PlayerDeathReason._sourceNPCIndex` | C02 `PlayerDeathAttributionSnapshot` | 死亡事件中的兼容 NPC 槽/索引；不可当持久身份 | existing-evidence: `PlayerDeathReason.cs:10`, `NetMessage.cs:1426-1440` |
| 1218 | `PlayerDeathReason._sourceProjectileLocalIndex` | C02 `PlayerDeathAttributionSnapshot` | 本地投射物槽；需在快照时解析，不能跨客户端复用 | existing-evidence: `PlayerDeathReason.cs:12`; tModLoader `whoAmI` boundary |
| 1219 | `PlayerDeathReason._sourceOtherIndex` | C02 death attribution other-cause value | 兼容其他来源索引；语义和 owner evidence-gap | evidence-gap: source domain and persistence meaning require integration review |
| 1220 | `PlayerDeathReason._sourceProjectileType` | C02 death attribution content snapshot | 投射物内容类型；由 projectile source 解析/复制 | existing-evidence: `PlayerDeathReason.cs:16`, `SourceProjectileType` consumer |
| 1221 | `PlayerDeathReason._sourceItemType` | C02 death attribution content snapshot | 物品内容类型；网络/文本输入 | existing-evidence: `PlayerDeathReason.cs:18` |
| 1222 | `PlayerDeathReason._sourceItemPrefix` | C02 death attribution content snapshot | 物品前缀内容值；不能作为 item entity identity | existing-evidence: `PlayerDeathReason.cs:20` |
| 1223 | `PlayerDeathReason._sourceCustomReason` | C02 death attribution presentation payload | 自定义死亡文本/原因；限消息/投影边界 | existing-evidence: `PlayerDeathReason.cs:22`, `Player.cs:22660-22661` |
| 3698 | `PlayerDeathReason.SourceProjectileType` | C02 `DeathAttributionQuery` derived result | `_sourceProjectileLocalIndex != -1` 时派生 optional 值；无写入 | existing-evidence: `PlayerDeathReason.cs:24`, `Player.cs:22336-22339` |
| 3060 | `Entity.whoAmI` | C01 `EntitySlotBindingComponent.CompatibilitySlot` | 数组索引/复用句柄；释放时失效 | existing-evidence: `Entity.cs:8`, tModLoader `class_entity.html` |
| 3061 | `Entity.position` | C01 `LocationComponent` | 当前权威位置；移动系统唯一写入 owner | existing-evidence: `Entity.cs:10`, `Entity.cs:48-183` |
| 3062 | `Entity.velocity` | C01 `VelocityComponent` | 当前权威速度；运动系统唯一写入 owner | existing-evidence: `Entity.cs:12` |
| 3063 | `Entity.oldPosition` | C01 `MotionHistoryComponent` | 上一位置快照；帧边界推进 | existing-evidence: `Entity.cs:14` |
| 3064 | `Entity.oldVelocity` | C01 `MotionHistoryComponent` | 上一速度快照；帧边界推进 | existing-evidence: `Entity.cs:16` |
| 3065 | `Entity.oldDirection` | C01 `MotionHistoryComponent` | 上一方向快照；帧边界推进 | existing-evidence: `Entity.cs:18` |
| 3066 | `Entity.direction` | C01 `DirectionComponent` | 当前朝向；实体运动/表现边界共享候选 | existing-evidence: `Entity.cs:20`; crossSubsystemOwner: integration-review |

## 5. ID、引用和快照分离

| 名称 | 语义 | 允许的 P06 用法 | 禁止的混用 |
|---|---|---|---|
| runtime `EntityId` | ECS 运行时实体身份 | C01 候选引用目标 | 不从 `whoAmI` 推导持久身份 |
| compatibility slot | `whoAmI`/数组索引/本地槽 | adapter、slot store 查找 | 不跨客户端、跨重启或跨回收周期稳定化 |
| generation | 槽位复用世代 | stale-handle 拒绝 | 不作为内容类型或网络序号 |
| PersistentId | 世界/存档身份 | 仅在确有存档语义时由其他分区提供 | P06 不为所有表现/实体池强行生成 |
| NetworkId | 会话/复制身份 | 网络 snapshot/projection 输入 | 不用本地 projectile slot 代替 |
| projectile lookup key | `owner + identity/uuid` | `ProjectileIdentityIndex` 查询 | 不当作通用 EntityId 或 PersistentId |
| external/content ID | item、projectile、NPC、mount、tile、chest/sign 等外部定义或坐标 | C02 payload/adapter | 不作为运行时实体身份 |
| `EntitySource_* SourceId` | 来源/内容上下文 ID | provenance 输入值 | 不当作被生成实体 ID |

所有跨分区共享的 ID、引用、快照和值对象候选均标记：`crossSubsystemOwner: integration-review`。

## 6. 生命周期、写入 owner 和调度契约

### 6.1 候选状态流

```text
AllocateSlotCommand
  -> bind runtime EntityId + compatibility slot + generation
  -> initialize authoritative C01 state
  -> capture immutable C02 source context
  -> commit active visibility
  -> copy current motion into history at the defined frame boundary
  -> run entity-domain systems in explicit scheduler order
  -> emit death/despawn event and capture attribution snapshot
  -> publish network/persistence/presentation projections
  -> detach references and clear projectile/container indexes
  -> release slot, increment generation, and make the slot reusable
```

### 6.2 写入 owner

| 状态 | 唯一 proposed 写入 owner | 读取者/交接 | 失败与重试 |
|---|---|---|---|
| 槽位、active、generation | `EntityPoolInitializationSystem` / `EntitySpawnCommitSystem` / `EntityDespawnRecycleSystem` | slot query、adapter、projection | 分配失败返回 admission failure；重复 release 幂等或显式拒绝 |
| 位置/速度/方向 | C01 movement/domain owner，最终由整合审查指定 | 空间、碰撞、表现、网络 projection | 不在 Query 中修正；失败保留上一有效状态并记录诊断 |
| 运动历史 | `MotionHistoryCommitSystem` | 插值/表现/快照 query | 每帧一次；不能被多个域重复推进 |
| source context | `EntityProvenanceCommitSystem` | 生成、战斗、掉落、诊断 | 外部对象解析失败保留 typed absence，不伪造 ID |
| death attribution snapshot | `DeathAttributionCaptureSystem` | 死亡处理、文本、网络 | 快照只读；缺来源时发送显式 unknown/absent |
| projectile identity index | `ProjectileIdentityIndex` owner | projectile lookup query | kill/despawn 先清索引；映射不活动槽则拒绝 |
| chest/sign state | `WorldContainerLifecycleSystem` 与 tile/world owner | 世界存储、交互、网络 projection | tile 失效或空容器按候选策略 tombstone/remove，owner 待整合 |
| presentation pools | `PresentationEffectPoolSystem` | render/UI/audio projection | 池耗尽可丢弃表现但不得影响权威 gameplay state |

### 6.3 显式调度候选

以下是候选顺序，不是最终裁决；共享部分均为 `crossSubsystemOwner: integration-review`：

1. `EntityPoolInitializationSystem` 建立容量和空槽不变量。
2. `EntitySpawnCommitSystem` 处理分配命令、绑定槽位/generation、初始化 C01，并在提交点发布活动状态。
3. `LegacyEntitySourceAdapter` 将外部来源转换为不可变 C02 payload；`EntityProvenanceCommitSystem` 在生成提交边界保存需要跨帧的最小 provenance。
4. `MotionHistoryCommitSystem` 在移动前按约定推进上一帧值；位置/速度/方向 domain systems 之后才消费新的当前值。
5. NPC、投射物、物品、空间和战斗分区的 domain systems 运行；P06 只提供只读 query/port。
6. `DeathAttributionCaptureSystem` 在死亡/kill 事件仍能解析来源时生成 snapshot；随后由死亡、网络和诊断 projection 读取。
7. `EntityDespawnRecycleSystem` 清理 projectile identity、world slot 引用、source references，递增 generation 后释放。
8. `PresentationEffectPoolSystem` 和 `ItemAnimationCatalogSystem` 处理非权威表现/定义注册；它们不能改变 C01/C02 的权威状态。

## 7. 网络、持久化和客户端投影边界

- 网络输入只经 `LegacyDeathReasonNetworkAdapter` 或同类 proposed adapter 解码为受限 snapshot/command；不得把二进制 reader 写入组件字段。
- 网络输出只由 proposed `DeathCauseProjection`、实体快照 projection 和世界槽 projection 产生。兼容槽位可以作为协议字段，但必须带会话/实体类型语义，不能宣称为稳定 EntityId。
- 世界物品、箱子和标牌可能进入持久化，但 P06 只提供候选 snapshot/port；表现池、rain、dust、star、gore、combatText、item animation registration 不应成为世界存档记录。
- `PlayerDeathAttributionSnapshot` 是否存档、保存多久及重放格式由死亡/持久化整合 owner 决定；本分区不扩大其生命周期。
- 客户端只消费 projection，不能反向写入 `EntitySlotBindingComponent`、`EntityProvenanceComponent` 或 slot store。

## 8. Evidence-gap 与 blocking-decision

### 8.1 evidence-gap

- Version4 已确认主要声明、分配、更新、清理和网络调用者，但尚未形成每个数组成员的全调用图；失败分支之外的所有 writer、线程/帧边界和并发假设仍需 focused verifier。
- `PlayerDeathReason._sourceOtherIndex` 的完整枚举语义、持久化要求和跨版本兼容规则未闭合。
- `EntitySource_*` 的完整调用者集合、哪些来源需要跨帧保留、`IEntitySourceTarget` 到核心 `EntityReference` 的失效策略尚未闭合。
- `src2` 中的 slot store、projectile identity index、provenance/death 类型和 focused verifier 已保存；本轮已验证 allocation、generation、release、stale-reference rejection、projectile identity cleanup、container lifecycle、catalog lifetime 以及 spawn/despawn pool transitions。真实 NLTX 运行时接线、网络/持久化闭合和跨分区 writer 仍未验证。
- SS14 未提供本分区可直接引用的核心 ECS 实现；tModLoader 只能 corroborate public boundary。

### 8.2 blocking-decision

- `EntityIdentityComponent` 与 `EntitySlotBindingComponent` 的最终组合、`EntityId`/`NetworkId`/`PersistentId` 的 owner 由 integration review 决定。
- 空间/运动、投射物、NPC、物品、战斗、掉落、死亡惩罚、网络和持久化分区必须共同批准 spawn/death/recycle 的事务边界和调度顺序。
- 世界物品、箱子、标牌的持久化 owner 以及表现池的丢弃/回收策略尚不能由 P06 单方面决定。
- attribution snapshot 是否复制、存档或只存在于死亡事件中必须由死亡/网络整合会话决策。

## 9. Focused verifier 记录

本轮 `verificationStatus: independently-verified`。验证范围是 `src2` 隔离实现和其 focused verifier，不是当前生产 `src` 的迁移完成或 Version4 行为等价证明：

- 静态成员覆盖计划：输入报告 44 条成员逐条映射；本轮未声称生产 `Main` 全局数组已移除。
- 已执行 slot lifecycle：stale slot/generation、world-item reuse delay、generation advancement 和 release rejection。
- 已执行 projectile identity：owner/identity 映射、活动检查、release 清理和槽复用 generation。
- 已执行 world item/container：复用延迟、空箱子删除、标牌文本及 tile invalidation。网络/存档 round-trip 仍未执行。
- 已执行 attribution：source adaptation、外部类型隔离、一次性 provenance、death optional projection 和 network snapshot round-trip。
- 已执行 presentation/catalog：效果过期回收、按表现种类隔离、动画定义注册/清理。
- 已执行 spawn/despawn：projectile allocation/indexing/recycle，以及 world-item/presentation command routing。完整 update/death scheduler、重复事件重试仍未执行。
- 最终通过仓库规定的串行 wrapper 构建 `src2\EntityLifecycleAttributionVerification\Terraria.EntityLifecycleAttributionVerification.csproj`，退出码 `0`、警告 `0`、错误 `0`；实现 DLL 和验证器 DLL 均位于 `Build\bin\...\Debug\net10.0`。
- 随后以 `--no-build --no-restore` 运行同一验证器，退出码 `0`，输出 `P06 verifier passed.`；验证器覆盖 stale slot/generation、motion history、world-item reuse delay、projectile identity cleanup、source adaptation/provenance one-time commit、death snapshot/network round-trip、container lifecycle、presentation expiry、animation catalog cleanup 和 spawn/despawn pool ownership。

## 10. Integration Handoff

提交给整合审查的候选合同：

1. `EntitySlotBindingComponent` 的 slot/generation 只能作为兼容引用；稳定实体、网络和持久化 ID 必须分别命名和分别所有。
2. C01 运动状态、C02 attribution payload、C03 pool/store 的写入 owner 不得交叉隐式写字段。
3. spawn、active publish、death snapshot、projection、reference detach、recycle 的顺序必须成为 scheduler/transaction contract，而不是由文件顺序决定。
4. 所有跨分区共享类型、ID、snapshot、reference、query 和顺序保留 `crossSubsystemOwner: integration-review`。
5. C01/C02/C03 的 `src2` 隔离实现已经保存并独立验证；设计仍为 `designStatus: proposed`，当前文档状态为 `executionStatus: completed`、`implementationStatus: completed`、`verificationStatus: independently-verified`。权威运行时接线和跨分区 owner 仍交由 integration review。
