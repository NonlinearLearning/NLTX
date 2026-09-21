# Version4 非权威 P06：实体生命周期与归因后续实施计划

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

> 本文是非权威的实施记录和整合前执行边界。C01/C02/C03 的隔离实现已写入 `src2` 并通过 focused build/verifier；这些证据不表示当前生产 `src` 已迁移、Version4 行为等价已证明、网络/持久化已闭合或跨分区最终 owner 已裁决。

## 1. 实施前置和不变约束

本计划只覆盖 P06 输入报告的 44 条成员。任何实际实施都必须先获得 `crossSubsystemOwner: integration-review` 对身份、引用、网络、持久化和调度合同的确认。目标路径是候选路径，不得视为已创建文件。

不得修改或搬移 P06 之外的生产代码、测试、项目文件、Version4、第一轮报告、其他分区输出、ledger 或 lock。实施阶段如需触及其他分区，只能由整合会话形成单独变更。

## 2. Proposed 目标目录和文件

| 能力 | Proposed 目标 | 角色 | 当前证据/状态 |
|---|---|---|---|
| C01 identity/motion | `src2\EntityLifecycleAttribution\EntitySlotBindingComponent.cs` | Component | 已保存并由 focused verifier 覆盖 slot/generation stale-handle；稳定 EntityId/handle owner 仍待 integration review |
| C01 identity/motion | `proposed src2\Share\Entity\Components\LocationComponent.cs`, `VelocityComponent.cs`, `DirectionComponent.cs`, `MotionHistoryComponent.cs` | Components | 同名 NLTX skeleton 只能作为 partial evidence |
| C01 identity/motion | `proposed src2\Share\Entity\Components\EntityIdentityComponent.cs`, `EntityIdentityState.cs` | identity candidate | 现有形状不证明 runtime allocation/network/persistence 已闭合 |
| C02 attribution | `proposed src2\EntityLifecycle\Attribution\EntitySpawnSourceContext.cs` | transient value/payload | 需禁止 Terraria object 泄漏 |
| C02 attribution | `proposed src2\Share\Entity\Components\EntityProvenanceComponent.cs` | optional retained Component | 只保留跨帧必要字段 |
| C02 attribution | `proposed src2\Combat\Attribution\PlayerDeathAttributionSnapshot.cs` | immutable event/snapshot | 与现有 `DeathCause`/`DeathResolution` 需整合审查 |
| C02 attribution | `proposed src2\EntityLifecycle\Attribution\EntityProvenanceCommitSystem.cs`, `DeathAttributionCaptureSystem.cs` | Systems | 单一写入 owner 候选 |
| C02 attribution | `proposed src2\EntityLifecycle\Attribution\SourceAttributionQuery.cs`, `DeathCauseProjection.cs` | Query/Projection | 只读/单向输出 |
| C02 compatibility | `proposed src2\Compatibility\Terraria\LegacyEntitySourceAdapter.cs`, `LegacyDeathReasonNetworkAdapter.cs` | Adapters | 隔离第三方对象和二进制协议 |
| C03 pools | `proposed src2\WorldStorage\PresentationEffectPoolStore.cs` | transient store | dust/star/gore/rain/combatText 不进入存档 |
| C03 pools | `proposed src2\WorldStorage\WorldItemSlotStore.cs` | world item store | 与复用门、WorldItem state 共同提交 |
| C03 pools | `proposed src2\WorldStorage\NpcEntitySlotStore.cs` | NPC slot adapter/store | NPC 状态 owner 交给 NPC 整合 |
| C03 pools | `proposed src2\WorldStorage\ProjectileEntitySlotStore.cs`, `ProjectileIdentityIndex.cs` | projectile store/index | 必须覆盖 stale lookup 和 kill 清理 |
| C03 world objects | `proposed src2\WorldStorage\WorldContainerStore.cs`, `WorldChestState.cs`, `WorldSignStore.cs`, `WorldSignState.cs` | world state/store | 持久化和 tile owner 待整合 |
| C03 catalog | `proposed src2\Content\ItemAnimationCatalog.cs` | definition catalog | 不与 entity lifecycle 混合 |
| C03 systems | `proposed EntityPoolInitializationSystem`, `EntitySpawnCommitSystem`, `EntityDespawnRecycleSystem`, `WorldContainerLifecycleSystem`, `PresentationEffectPoolSystem`, `ItemAnimationCatalogSystem` | Systems | 目标命名、路径和顺序均为 proposed |
| C03 commands | `proposed AllocateEntitySlotCommand`, `ReleaseEntitySlotCommand` | Commands | 命令提交边界待整合 |

## 3. 44 条成员到实施阶段的映射

| 序号 | 源成员 | 实施阶段 | 目标角色/候选文件 | 迁移注意 |
|---:|---|---|---|---|
| 376 | `Main.dust` | 3 | `PresentationEffectPoolStore` | 只迁移表现池访问；不生成持久 EntityId |
| 377 | `Main.star` | 3 | `PresentationEffectPoolStore` | 场景/帧清理由表现 owner 负责 |
| 378 | `Main.item` | 3 | `WorldItemSlotStore` + `WorldItemComponent` | 保留兼容读适配层，单写入 owner |
| 379 | `Main.timeItemSlotCannotBeReusedFor` | 3 | `WorldItemSlotStore` reuse gate | 复用延迟必须与 slot generation 同步 |
| 380 | `Main.npc` | 3 | `NpcEntitySlotStore` proposed adapter | 不替 NPC 分区裁决状态 owner |
| 381 | `Main.gore` | 3 | `PresentationEffectPoolStore` | 池耗尽不得影响 gameplay |
| 382 | `Main.rain` | 3 | `PresentationEffectPoolStore` | 天气 update 与权威世界状态隔离 |
| 383 | `Main.projectile` | 3 | `ProjectileEntitySlotStore` | 先完成 slot/generation 合同 |
| 384 | `Main.projectileIdentity` | 3 | `ProjectileIdentityIndex` | 释放前清索引，查询验证 active/generation |
| 385 | `Main.combatText` | 3 | `PresentationEffectPoolStore` | UI/表现 projection only |
| 386 | `Main.chest` | 3 | `WorldContainerStore` / `WorldChestState` | 空后删除与存档 owner 需整合 |
| 387 | `Main.sign` | 3 | `WorldSignStore` / `WorldSignState` | 坐标和 tile validity 不转为 EntityId |
| 388 | `Main.itemAnimations` | 3 | `ItemAnimationCatalog` | content ID keyed definition |
| 389 | `Main.itemAnimationsRegistered` | 3 | `ItemAnimationCatalog` | 注册集合可重建，不进入世界存档 |
| 1030 | `AEntitySource_OnHit.EntityStriking` | 2 | `EntitySpawnSourceContext` | adapter 成核心 EntityReference/value |
| 1031 | `AEntitySource_OnHit.EntityStruck` | 2 | `EntitySpawnSourceContext` | stale target 必须显式 absent |
| 1032 | `AEntitySource_Tile.TileCoords` | 2 | source context tile coordinate | tile owner deferred |
| 1126 | `EntitySource_ByItemSourceId.Entity` | 2 | source context entity reference | 不复制外部对象 |
| 1127 | `EntitySource_ByItemSourceId.SourceId` | 2 | external/content source ID | 不当 EntityId |
| 1128 | `EntitySource_ByProjectileSourceId.SourceId` | 2 | external/content source ID | 不当 projectile slot |
| 1131 | `EntitySource_ItemUse_WithAmmo.AmmoItemIdUsed` | 2 | external item content ID | 由 item/content owner 验证 |
| 1132 | `EntitySource_ItemUse.Entity` | 2 | source context entity reference | source payload immutable |
| 1133 | `EntitySource_ItemUse.Item` | 2 | `LegacyEntitySourceAdapter` | Terraria.Item 只在边界存在 |
| 1135 | `EntitySource_Mount.Entity` | 2 | source context entity reference | mount/player owner deferred |
| 1136 | `EntitySource_Mount.MountId` | 2 | external mount/content ID | 不当 EntityId |
| 1137 | `EntitySource_OverfullChest.Chest` | 2 | `LegacyEntitySourceAdapter` | 只转换 overflow 所需值 |
| 1138 | `EntitySource_Parent.Entity` | 2 | parent reference in context | parent 先销毁需可解析为 absent |
| 1139 | `EntitySource_TileInteraction.Entity` | 2 | source context entity reference | tile interaction owner deferred |
| 1216 | `PlayerDeathReason._sourcePlayerIndex` | 2 | `PlayerDeathAttributionSnapshot` | 兼容槽位字段，不是 persistent ID |
| 1217 | `PlayerDeathReason._sourceNPCIndex` | 2 | `PlayerDeathAttributionSnapshot` | NPC slot only |
| 1218 | `PlayerDeathReason._sourceProjectileLocalIndex` | 2 | `PlayerDeathAttributionSnapshot` | snapshot 时解析 local slot |
| 1219 | `PlayerDeathReason._sourceOtherIndex` | 2 | other-cause value in snapshot | semantic evidence-gap remains |
| 1220 | `PlayerDeathReason._sourceProjectileType` | 2 | attribution content snapshot | type ID only |
| 1221 | `PlayerDeathReason._sourceItemType` | 2 | attribution content snapshot | item content ID only |
| 1222 | `PlayerDeathReason._sourceItemPrefix` | 2 | attribution content snapshot | prefix value only |
| 1223 | `PlayerDeathReason._sourceCustomReason` | 2 | death presentation payload | network/text boundary only |
| 3698 | `PlayerDeathReason.SourceProjectileType` | 2 | `DeathAttributionQuery` | pure derived optional result |
| 3060 | `Entity.whoAmI` | 1 | `EntitySlotBindingComponent.CompatibilitySlot` | generation required for stale rejection |
| 3061 | `Entity.position` | 1 | `LocationComponent` | one movement writer |
| 3062 | `Entity.velocity` | 1 | `VelocityComponent` | one movement writer |
| 3063 | `Entity.oldPosition` | 1 | `MotionHistoryComponent` | frame boundary commit |
| 3064 | `Entity.oldVelocity` | 1 | `MotionHistoryComponent` | frame boundary commit |
| 3065 | `Entity.oldDirection` | 1 | `MotionHistoryComponent` | frame boundary commit |
| 3066 | `Entity.direction` | 1 | `DirectionComponent` | shared candidate; owner integration-review |

## 4. 实施顺序和检查点

### 阶段 0：合同冻结和保护性基线

1. 从输入报告导出 44 条成员的 machine-readable coverage fixture，记录 Version4 声明行、已知读写路径和证据状态。
2. 由整合审查批准 runtime EntityId、compatibility slot/generation、NetworkId、PersistentId、EntityReference 和 attribution snapshot 的命名/owner。
3. 为旧 `Main` 数组访问建立 proposed read adapter 和诊断计数；不得先引入双写造成两个 authority。

### 阶段 1：C01 身份与运动

1. 先落地 `EntitySlotBindingComponent` 的 handle/generation 不变量，再接入 Location/Velocity/Direction/MotionHistory。
2. 只允许 pool commit 写 slot binding，movement owner 写当前运动，history commit system 写上一帧值。
3. 在没有 active/generation 验证时拒绝把 `whoAmI` 转换为 EntityReference。
4. 完成 slot lifecycle tests 和运动历史边界 tests 后，才进入来源迁移。

### 阶段 2：C02 来源和死亡归因

1. 先实现 adapter 的值转换和 typed absence，再实现 `EntitySpawnSourceContext`。
2. 只保留需要跨帧消费的字段到 proposed `EntityProvenanceComponent`；其余 context 在 command/event 消费后释放。
3. 在 death event commit 点创建不可变 `PlayerDeathAttributionSnapshot`，由 `DeathAttributionCaptureSystem` 统一写入。
4. 实现 `DeathAttributionQuery`、`SourceProjectileType` 的 optional 语义和 network adapter round-trip；不直接序列化 Version4 对象。

### 阶段 3：C03 池和世界槽

1. 先实现容量、空槽、generation 和 admission error 的通用 slot contract，再按 domain 启用 world item、projectile、NPC store。
2. 实现 world item reuse gate；迁移 `projectileIdentity` 时先添加 owner/identity/active/generation 校验，再切换 kill/despawn 清理。
3. 独立迁移 chest/sign container stores，明确 tile validity、empty-before-remove、文本 snapshot 和存档 owner。
4. 最后迁移 presentation pools 和 item animation catalog，确保池耗尽或 catalog 重建不改变权威 gameplay state。

### 阶段 4：投影、兼容退出和整合验证

1. 接入网络、存档、UI/诊断 projections；projection 只能读 snapshot/query，不得反写 component/store。
2. 开启兼容读路径和 shadow comparison，比较 active/generation、来源快照和槽位复用结果。
3. 满足回滚阈值并通过 focused verifier 后，才由整合会话决定移除旧数组写路径。

## 5. 单一写入 owner 和副作用边界

| 资源 | 唯一写入 owner 候选 | 允许的副作用 | 禁止 |
|---|---|---|---|
| slot/active/generation | spawn/recycle systems | 事件、诊断、projection input | Query 修改 slot；多个 store 双写 |
| current motion | domain movement owner | 运动事件、snapshot input | adapter 或 projection 直接写运动 |
| motion history | history commit system | 插值/诊断读取 | 多个阶段重复推进 |
| provenance | provenance commit system | provenance event/diagnostic projection | 完整外部对象进入 component |
| death snapshot | death attribution capture system | network/text/death projections | 任意 consumer 修改 snapshot |
| projectile index | projectile slot/index owner | lookup query | stale map 指向新槽 |
| world container | world container lifecycle owner | persistence/network projection | tile query 直接改存储 |
| presentation pool | presentation effect pool owner | UI/render/audio output | 表现池影响 authoritative state |

I/O、时钟、随机、日志、网络、持久化和 UI 均通过 adapter/projection/显式 port 隔离。Query 只能计算或读取；Command 只在 commit system 边界提交状态转换。

## 6. 双写、兼容和迁移策略

- 默认策略是单一 authority 加兼容 read adapter。旧 `Main` 数组在迁移期间可以被 adapter 读取，但不能让旧数组和新 store 同时拥有写权限。
- 如确需 shadow comparison，只允许新 store 作为候选真值、旧路径作为只读观察源；差异记录到诊断 projection，不通过第二次写入修正。
- 事件/网络迁移采用版本化 snapshot adapter。旧 `PlayerDeathReason` 字段映射到新快照时，缺失字段必须保留 absent/unknown，而不是使用默认实体 ID。
- 槽位迁移需携带 generation；恢复或重连不能把旧 `whoAmI` 直接视为当前 EntityReference。
- `Main.item`, chest, sign 等世界槽只有在存档 snapshot round-trip 和失败恢复 verifier 通过后，才能关闭兼容写入口；本计划不执行该关闭。

## 7. 网络、快照和持久化迁移

| 边界 | Proposed 处理 | 必须先决 |
|---|---|---|
| 网络输入 | `LegacyDeathReasonNetworkAdapter` 解码到受限 command/snapshot | 包版本、字段缺失和非法 slot 策略 |
| 网络输出 | 从 entity/world/death snapshot 生成 projection | NetworkId 与 compatibility slot 的协议区分 |
| 世界存档 | world item/chest/sign 的明确 snapshot/restore adapter | PersistentId、tile anchor、revision/rollback owner |
| 临时表现 | 不写入世界存档；按场景/会话清理 | 表现丢弃是否可观测 |
| 死亡归因 | 默认 event/session lifetime；是否存档由整合 owner 决策 | 死亡惩罚、掉落、网络回放需求 |

任何共享 snapshot、serializer、NetworkId、PersistentId、EntityReference 或调度契约都标记 `crossSubsystemOwner: integration-review`。

## 8. 回滚步骤和条件

### 回滚步骤

1. 停止新 spawn/death/recycle commit system 的写入入口，保留只读 diagnostics。
2. 将兼容 read adapter 的 authority 标志切回旧路径，禁止新 store 继续发布网络/存档 snapshot。
3. 清空或隔离未提交的新 generation/index 映射，确保旧引用不会解析到新槽；不得复用已发布 generation。
4. 依据最近一次有效旧协议 snapshot 恢复 world item/chest/sign 的活动状态，记录无法回放的 attribution 事件。
5. 保留差异日志和失败原因，重新运行 slot/index/attribution focused tests 后再决定重试。

### 触发条件

- 检测到 stale handle 可解析到新实体、projectile identity 指向错误活动槽或重复 release 改变活动状态。
- 网络或存档 round-trip 改变 death attribution、world container 内容或兼容槽语义。
- 新旧 shadow comparison 出现未解释的 active/generation、位置历史、来源或回收顺序差异。
- 任何 projection 反写权威组件，或表现池耗尽影响 gameplay state。

## 9. Focused verifier 和串行构建记录

本轮已完成 `src2` 隔离实现的编译和 focused verifier；`verificationStatus: independently-verified`。这不是生产 `src` 迁移、网络/持久化闭合或 Version4 行为等价证明。

实施后按以下顺序验证：

1. 文档/静态覆盖：44/44 成员映射、目标命名符合 ECS 文件组织与组件命名约束、无 `UniversalEntityComponent`、无跨域未标记 owner。
2. 单元测试：slot/generation、motion history、source adapter、death snapshot、derived `SourceProjectileType`、container lifecycle、catalog registration。
3. 集成测试：spawn -> active -> update -> death -> attribution projection -> detach -> recycle；重复 command、失败 retry、池耗尽和网络包缺失字段。
4. 存档/网络测试：world item/chest/sign round-trip、旧包兼容、不同客户端 local slot 不泄漏为稳定 ID。
5. 本轮 focused verifier 覆盖的已执行行为为：stale slot/generation、motion history、world-item reuse delay、projectile identity cleanup、source adaptation/provenance one-time commit、death snapshot/network round-trip、container lifecycle、presentation expiry、animation catalog cleanup、按表现种类隔离，以及 spawn/despawn pool ownership。网络/存档 world snapshot、完整 update/death scheduler、重复事件重试和生产运行时接线仍未验证。

6. 最终实际构建命令为：

```powershell
pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\EntityLifecycleAttributionVerification\\Terraria.EntityLifecycleAttributionVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"
```

该构建退出码为 `0`，警告 `0`，错误 `0`；产物为：

- `D:\TRbackup\NLTX\Build\bin\Terraria.EntityLifecycleAttribution\Debug\net10.0\Terraria.EntityLifecycleAttribution.dll`
- `D:\TRbackup\NLTX\Build\bin\Terraria.EntityLifecycleAttributionVerification\Debug\net10.0\Terraria.EntityLifecycleAttributionVerification.dll`

随后实际运行：

```powershell
pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\EntityLifecycleAttributionVerification\\Terraria.EntityLifecycleAttributionVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"
```

运行退出码为 `0`，输出为 `P06 verifier passed.`。每次 compile-capable 命令前均检查了 `dotnet.exe`/`csc.exe`，并等待共享 checkout 的其他构建结束后再执行。

## 10. 风险、evidence-gap 和 blocking-decision

### 风险

- 兼容槽被误当成稳定实体身份，导致多人网络、重连或回收后陈旧引用命中错误实体。
- 双写形成两个 authority，造成位置历史、active 状态、世界物品延迟或投射物索引不一致。
- 将 `EntitySource_*` 的第三方对象保存到核心组件，造成生命周期泄漏和不可序列化状态。
- 死亡原因在事件、网络和死亡惩罚之间被不同 owner 重新解释，导致文本、掉落或复仇逻辑不一致。
- 世界容器的 tile validity、empty-before-remove、revision 和存档恢复尚未被一个事务 owner 闭合。

### evidence-gap

Direct lifecycle evidence is strong for the reviewed Version4 paths, but complete writer coverage, persistence schemas, replication ownership, cross-partition EntityReference contracts, and final scheduler ownership remain partial or missing; tModLoader is boundary corroboration only and SS14 has no direct core EntityManager/EntityUid evidence in the searched mirror.

### blocking-decision

Integration review must decide the cross-partition owners and commit order for EntityId, compatibility slot/generation handles, NetworkId, PersistentId, EntityReference, attribution snapshots, and the final spawn-update-death-recycle scheduler transaction.

## 11. Integration handoff 和完成条件

提交给整合会话：

- 批准 C01/C02/C03 的候选目标和单一写入 owner。
- 批准 compatibility slot、generation、runtime EntityId、NetworkId、PersistentId、projectile lookup key 和 external/content ID 的不同命名与序列化规则。
- 批准 death attribution 的 event-only、session snapshot、network projection 和可选 persistence 生命周期。
- 批准 spawn/update/death/projection/recycle 的事务顺序、失败重试和回滚阈值。
- 批准 world item/chest/sign 的持久化 owner以及表现池/catalog 的非权威边界。

本记录不代表权威生产迁移或跨分区整合已完成。当前明确状态为：

- `designStatus: proposed`
- `executionStatus: completed`
- `implementationStatus: completed`
- `verificationStatus: independently-verified`
- `src2CodeModified: yes`
- `productionSrcModified: no`
- authoritative runtime integration, behavior equivalence, network/persistence closure and cross-partition owner decisions remain deferred
