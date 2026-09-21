# P07 非权威组件实施计划：战斗、伤害与状态效果

> 本文记录非权威实施会话的增量 checkpoint。尚未完成的目标文件和类型仍为 `proposed`；已记录的 `src2` 文件仅表示本会话已保存的 focused implementation slice，不表示当前 NLTX 已完成迁移、行为等价、网络闭合、持久化闭合或跨分区 owner 裁决。

partitionId: P07
sessionId: f675c8223b354b0a810e1418ca700490
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\07-combat-status-effects.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P07-combat-status-effects-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P07-combat-status-effects-component-execution.md
designStatus: proposed
executionStatus: planned
implementationStatus: completed
verificationStatus: partial
completedComponents:
- SharedCombatTargetingAndImmunity
- SharedHitTileTracking
- ItemTagEffectState
- LockOnTargetingState
- SharedCombatTextState
- SharedItemCombatAndDamageCapabilityState
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T07:24:02.8983663Z
currentSessionVerification:
- sessionId: f675c8223b354b0a810e1418ca700490
  status: passed
  build:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    project: D:\\TRbackup\\NLTX\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj
    exitCode: 0
    warnings: 0
    errors: 0
    artifacts:
    - Build/bin/Terraria.CombatStatusEffects/Debug/net10.0/Terraria.CombatStatusEffects.dll
    - Build/bin/Terraria.CombatStatusEffectsVerification/Debug/net10.0/Terraria.CombatStatusEffectsVerification.dll
  run:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    project: D:\\TRbackup\\NLTX\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj
    exitCode: 0
    output: P07 focused verifier passed: 29 scenarios
  scope: current-session build and focused verifier only; no behavior-equivalence, network, persistence, or cross-partition owner claim
implementationEvidence:
- status: implemented
  component: SharedCombatTargetingAndImmunity
  productionProject: D:\TRbackup\NLTX\src2\Combat\Terraria.CombatStatusEffects.csproj
  verifierProject: D:\TRbackup\NLTX\src2\CombatVerification\Terraria.CombatStatusEffectsVerification.csproj
  sourceFiles:
  - src2/Combat/Targeting/CombatTargetKind.cs
  - src2/Combat/Targeting/CombatRectangle.cs
  - src2/Combat/Targeting/CombatTargetSnapshot.cs
  - src2/Combat/Targeting/MultiPointHitboxSnapshot.cs
  - src2/Combat/Targeting/CombatTargetSnapshotQuery.cs
  - src2/Combat/Targeting/TargetValidityQuery.cs
  - src2/Content/StatusEffects/NpcDebuffImmunityDefinition.cs
  - src2/Combat/StatusEffects/NpcDebuffImmunityState.cs
  - src2/Combat/StatusEffects/NpcDebuffImmunityApplyResult.cs
  - src2/Combat/StatusEffects/NpcDebuffImmunityApplySystem.cs
  - src2/Combat/Resolution/NpcKillAttemptSnapshot.cs
  - src2/Combat/Resolution/NpcStrikeCommitCommand.cs
  - src2/Combat/Resolution/NpcStrikeCommitResult.cs
  - src2/Combat/Resolution/NpcKillResolutionResult.cs
  - src2/Combat/Resolution/NpcKillResolutionSystem.cs
  verifierScenarios: 4
  build:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    exitCode: 0
    warnings: 0
    errors: 0
    artifacts:
    - Build/bin/Terraria.CombatStatusEffects/Debug/net10.0/Terraria.CombatStatusEffects.dll
    - Build/bin/Terraria.CombatStatusEffectsVerification/Debug/net10.0/Terraria.CombatStatusEffectsVerification.dll
  run:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    exitCode: 0
  output: P07 focused verifier passed: 4 scenarios
  scope: local focused verifier only; no behavior-equivalence, network, persistence, or cross-partition owner claim
- status: implemented
  component: SharedHitTileTracking
  productionProject: D:\TRbackup\NLTX\src2\Combat\Terraria.CombatStatusEffects.csproj
  verifierProject: D:\TRbackup\NLTX\src2\CombatVerification\Terraria.CombatStatusEffectsVerification.csproj
  sourceFiles:
  - src2/Tiles/Interaction/TileHitTrackingComponent.cs
  - src2/Tiles/Interaction/TileHitEntry.cs
  - src2/Tiles/Interaction/TileHitKind.cs
  - src2/Tiles/Interaction/TileHitTrackingPolicy.cs
  - src2/Tiles/Interaction/TileHitTrackingSystem.cs
  - src2/Tiles/Interaction/TileHitLookupQuery.cs
  - src2/Tiles/Interaction/TileHitCommand.cs
  - src2/Tiles/Interaction/ITileCrackStyleRandom.cs
  - src2/Tiles/Interaction/TileCrackPresentationAdapter.cs
  verifierScenarios: 3 additional scenarios; 7 total
  build:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    exitCode: 0
    warnings: 0
    errors: 0
    artifacts:
    - Build/bin/Terraria.CombatStatusEffects/Debug/net10.0/Terraria.CombatStatusEffects.dll
    - Build/bin/Terraria.CombatStatusEffectsVerification/Debug/net10.0/Terraria.CombatStatusEffectsVerification.dll
  run:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    exitCode: 0
  output: P07 focused verifier passed: 7 scenarios
  scope: local Player-style cache and presentation seam only; no world Tile mutation, network, persistence, or cross-partition owner claim
- status: implemented
  component: ItemTagEffectState
  productionProject: D:\TRbackup\NLTX\src2\Combat\Terraria.CombatStatusEffects.csproj
  verifierProject: D:\TRbackup\NLTX\src2\CombatVerification\Terraria.CombatStatusEffectsVerification.csproj
  sourceFiles:
  - src2/Combat/TagEffects/PlayerTagEffectStateComponent.cs
  - src2/Content/StatusEffects/TagEffectDefinition.cs
  - src2/Content/StatusEffects/WhipTagEffectDefinition.cs
  - src2/Combat/TagEffects/TagEffectLifecycleSystem.cs
  - src2/Combat/TagEffects/TagEffectHitSystem.cs
  - src2/Combat/TagEffects/TagEffectHitEligibilityQuery.cs
  - src2/Combat/TagEffects/Commands/SetActiveTagEffectCommand.cs
  - src2/Combat/TagEffects/Commands/ApplyTagToNpcCommand.cs
  - src2/Combat/TagEffects/Commands/EnableTagProcCommand.cs
  - src2/Combat/TagEffects/Commands/ClearTagProcCommand.cs
  - src2/Combat/TagEffects/Commands/ResetNpcTagStateCommand.cs
  - src2/Combat/TagEffects/TagEffectBehaviorPort.cs
  - src2/Transport/TagEffects/TagEffectNetworkAdapter.cs
  - src2/Transport/TagEffects/TagEffectStateProjection.cs
  verifierScenarios: 4 additional scenarios; 11 total
  build:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    exitCode: 0
    warnings: 0
    errors: 0
    artifacts:
    - Build/bin/Terraria.CombatStatusEffects/Debug/net10.0/Terraria.CombatStatusEffects.dll
    - Build/bin/Terraria.CombatStatusEffectsVerification/Debug/net10.0/Terraria.CombatStatusEffectsVerification.dll
  run:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    exitCode: 0
  output: P07 focused verifier passed: 11 scenarios
  scope: local tag/timer state and command/projection seam only; network Deserialize, NPC callbacks, replay/permission, persistence, and cross-partition owner remain evidence gaps
- status: implemented
  component: LockOnTargetingState
  productionProject: D:\TRbackup\NLTX\src2\Combat\Terraria.CombatStatusEffects.csproj
  verifierProject: D:\TRbackup\NLTX\src2\CombatVerification\Terraria.CombatStatusEffectsVerification.csproj
  sourceFiles:
  - src2/Input/LockOn/LockOnPolicy.cs
  - src2/Input/LockOn/LockOnSelectionComponent.cs
  - src2/Input/LockOn/LockOnCandidateQuery.cs
  - src2/Input/LockOn/LockOnTargetQuery.cs
  - src2/Input/LockOn/LockOnPredictionQuery.cs
  - src2/Input/LockOn/LockOnInputSystem.cs
  - src2/Input/LockOn/LockOnCursorProjectionAdapter.cs
  verifierScenarios: 5 additional scenarios; 16 total
  build:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    exitCode: 0
    warnings: 0
    errors: 0
    artifacts:
    - Build/bin/Terraria.CombatStatusEffects/Debug/net10.0/Terraria.CombatStatusEffects.dll
    - Build/bin/Terraria.CombatStatusEffectsVerification/Debug/net10.0/Terraria.CombatStatusEffectsVerification.dll
  run:
    command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\CombatVerification\\Terraria.CombatStatusEffectsVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
    exitCode: 0
    output: P07 focused verifier passed: 16 scenarios
  scope: local client selection, prediction and cursor projection only; client prediction is not an authoritative damage target and cross-partition owner remains open
evidence-gap:
- 领取恢复时记录的输入报告 SHA-256 为 `b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`，当前同一路径观测为 `7E1ACF1A7CE9299EDBEFB5530093B603F7F89D1360E13CA0E7F86AA32E60F9DF`；runner 当前仍验证 97 条成员，本会话未修改输入报告，该 source-report drift 需由 integration-review 处理。
- P07 第一轮专属 outputReport 缺失，不能作为已完成的前置审查证据。
- Version4 裁剪实现未闭合部分网络反序列化、渲染更新和部分锁定写入者。
- 跨分区身份、伤害、死亡、掉落和调度 owner 尚未由 integration-review 裁决。
- TagEffect 的网络反序列化、NPC 变更写入和具体 UniqueTagEffect 回调在 Version4 证据中为裁剪/空实现。
- 旧 NPC 数组槽位与稳定 EntityReference/NetworkNpcId 的映射、重复 Proc 与网络重放策略尚未闭合。
- LockOnHelper 的候选列表/选择状态写入者、SetLockPosition/ResetLockPosition 和完整失效清理语义在当前 Version4 证据中缺失。
- 预测位置依赖空间碰撞、重力、NPC 位置定位和 ItemID 参数，跨域 Query/Adapter owner 尚未闭合。
- CombatText.NewText 的槽位分配、满池策略、动画更新和网络表现去重语义在当前 Version4 证据中缺失。
- CachedProjectileCounterBuffTextHandler.HandleBuffText 被裁剪，projectilesToLookFor 的缓存失效与文本格式未闭合。
- Item prefix/variant、ammo 选择、资源扣除和 Projectile spawn 分支分散在 Player/Projectile/ItemID，完整能力组合与失败重试语义未闭合。
- Item.rare 的表现消费者和稀有度映射未由本分区证据闭环。
blocking-decision:
- 输入报告哈希漂移是否可作为本轮设计输入，需由 integration-review 结合 97 条成员校验和原始报告来源裁决。
- 是否以稳定 EntityReference 替换旧 NPC 数组槽位，需要跨分区兼容方案。
- Strike、damage、death 和 loot 的提交事务边界必须在实施前锁定。
- TagEffect 定义注册表、Player 状态唯一写者、NPC 标记清理事件和网络 Adapter owner 需要 `integration-review` 裁决。
- 锁定候选/选择 owner、预测查询依赖、光标 projection scope 和 Item/Projectile 调度契约需要 `integration-review` 裁决。
- CombatText 槽位池、表现事件去重、网络客户端投影及 Buff 文本筛选器 owner 需要 `integration-review` 裁决。
- Item 能力定义、prefix/variant 合并、资源扣除、Projectile spawn、伤害提交和稀有度表现 owner 需要 `integration-review` 裁决。

## 1. 实施约束

后续只允许修改受影响项目中与本分区直接相关的领域文件；第一组已保存的实现证据见本文 checkpoint，剩余目标不得在没有对应 verifier 红灯的情况下扩展。目标路径仍需遵守按领域组织、一个核心公开类型一个同名 PascalCase 文件、Query 无写入、Projection 不反向写模拟的规则。

本计划不把文件枚举顺序当作调度顺序。任何依赖必须在显式 scheduler contract 中表达，并由 focused verifier 覆盖。

## 2. 检查点一：SharedCombatTargetingAndImmunity

### 2.1 Proposed 目标文件、目录和命名空间

| 目标（proposed） | 命名空间（proposed） | 角色 |
|---|---|---|
| `src2/Combat/Targeting/CombatTargetSnapshot.cs` | `Terraria.Combat.Targeting` | 瞬态目标快照 |
| `src2/Combat/Targeting/MultiPointHitboxSnapshot.cs` | `Terraria.Combat.Targeting` | 瞬态多点几何 |
| `src2/Content/StatusEffects/NpcDebuffImmunityDefinition.cs` | `Terraria.Content.StatusEffects` | 内容定义 |
| `src2/Combat/Resolution/NpcKillAttemptSnapshot.cs` | `Terraria.Combat.Resolution` | Strike 前快照 |
| `src2/Combat/Targeting/CombatTargetSnapshotQuery.cs` | `Terraria.Combat.Targeting` | 纯 Query |
| `src2/Combat/Targeting/TargetValidityQuery.cs` | `Terraria.Combat.Targeting` | 纯 Query |
| `src2/Combat/StatusEffects/NpcDebuffImmunityApplySystem.cs` | `Terraria.Combat.StatusEffects` | 免疫状态唯一写入 System |
| `src2/Combat/Resolution/NpcStrikeCommitCommand.cs` | `Terraria.Combat.Resolution` | proposed Command |
| `src2/Combat/Resolution/NpcKillResolutionSystem.cs` | `Terraria.Combat.Resolution` | 击杀事实候选 System |

这些路径是设计边界；第一组对应的实现证据已记录在本文 `implementationEvidence`，但 `NPC`、`Player`、`Projectile`、网络和稳定 ID 类型仍不由本组复制。

### 2.2 源成员到目标角色映射

| 源成员集合 | proposed 目标 | 迁移动作 |
|---|---|---|
| `MultiPointHitbox.PointSize`, `Points`, `BoundingRect` | `MultiPointHitboxSnapshot` | 保留 PointSize/Points 输入；BoundingRect 只由构造计算；不注册为长期 Component。 |
| `NPCAimedTarget.Type`, `Hitbox`, `Width`, `Height`, `Position`, `Velocity` | `CombatTargetSnapshot` | 由 NPC/Player/Projectile adapter 构造；去除对旧对象的长期引用。 |
| `Invalid`, `Center`, `Size` | `TargetValidityQuery`/快照派生值 | 纯计算，禁止缓存为权威字段。 |
| `NPCDebuffImmunityData.*` | `NpcDebuffImmunityDefinition` | 从内容目录读取；SpecificEffectIds 用不可变视图。 |
| `NPCKillAttempt.npc` | `NpcKillAttemptSnapshot.TargetReference` | 仅短期兼容引用，最终类型由 integration-review；不得序列化旧对象。 |
| `NPCKillAttempt.netId` | `NpcKillAttemptSnapshot.NetworkNpcId` | 只进入网络/协议边界；不得当作 EntityId 或 PersistentId。 |
| `NPCKillAttempt.active` | `NpcKillAttemptSnapshot.WasActive` | 保留为 Strike 前快照；死亡结论由提交结果产生。 |

### 2.3 实施顺序与单一写入 owner

1. 先建立不可变快照和值对象及纯 Query；没有外部写入。
2. 锁定 `EntityReference`/旧槽位/NetworkId 映射后，再建立 `NpcDebuffImmunityApplySystem`，唯一写运行时免疫状态。
3. 定义 `NpcStrikeCommitCommand` 的版本、重复和失败语义；由 integration-review 指定 DamageResolutionSystem 与 NpcKillResolutionSystem 的提交边界。
4. 最后接入 NPC/Projectile/Player 适配器，并逐个移除旧对象引用的跨边界传播。

单一写入 owner：`NpcDebuffImmunityApplySystem` 负责免疫实体状态；Damage/Health/Death/Drop 的 owner 不在本分区单方面裁决，标为 `crossSubsystemOwner: integration-review`。

### 2.4 双写、兼容、网络和持久化

- 初期允许旧 `NPCAimedTarget`/`NPCKillAttempt` 作为 Compatibility Adapter 的输入，但新核心只消费 proposed snapshot；禁止旧对象和新组件长期双写。
- `npc` 旧对象引用只在同一模拟调用栈中使用；网络包仅携带 NetworkNpcId 或最终整合定义的快照字段。
- 目标快照不持久化。NPC 免疫定义可由内容目录重建；运行时免疫是否进入网络快照需要 integration-review。
- 如果旧客户端仍依赖 `active` 或数组槽位，先建立单向 projection/adapter 和兼容期诊断，再删除旧字段读取。

### 2.5 回滚与验证

回滚条件：目标快照与旧调用者的 Hitbox/Velocity 结果不一致；免疫应用造成跨 NPC 类型的 Buff 资格变化；重复 Strike 产生双重死亡/掉落；或 NetworkId 被错误当成实体主键。回滚步骤是停止新 Adapter 接线，恢复旧调用路径，保留 proposed 文档和诊断，不删除任何持久化数据。

`COMBAT-TARGET-SNAPSHOT`、`NPC-IMMUNITY-APPLICATION`、`NPC-KILL-ATTEMPT-BOUNDARY` 已通过当前 focused verifier（4 个场景）；这只证明 `src2` 快照/免疫/击杀边界 slice，不证明 Player/NPC 集成、网络、持久化或跨分区 owner 已闭合。

明确声明：本检查点已执行局部 C# build/run，但没有行为等价、网络、持久化或跨分区 owner 验证。

## 3. 检查点二：SharedHitTileTracking

### 3.1 Proposed 目标文件、目录和命名空间

| 目标（proposed） | 命名空间（proposed） | 角色 |
|---|---|---|
| `src2/Tiles/Interaction/TileHitTrackingComponent.cs` | `Terraria.Tiles.Interaction` | Player-local 有界状态 |
| `src2/Tiles/Interaction/TileHitEntry.cs` | `Terraria.Tiles.Interaction` | 单个命中条目 |
| `src2/Tiles/Interaction/TileHitKind.cs` | `Terraria.Tiles.Interaction` | 命中种类定义 |
| `src2/Tiles/Interaction/TileHitTrackingPolicy.cs` | `Terraria.Tiles.Interaction` | 容量/TTL 定义 |
| `src2/Tiles/Interaction/TileHitTrackingSystem.cs` | `Terraria.Tiles.Interaction` | 唯一写入 System |
| `src2/Tiles/Interaction/TileHitLookupQuery.cs` | `Terraria.Tiles.Interaction` | 纯查找 Query |
| `src2/Tiles/Interaction/TileHitCommand.cs` | `Terraria.Tiles.Interaction` | proposed Command |
| `src2/Tiles/Interaction/ITileCrackStyleRandom.cs` | `Terraria.Tiles.Interaction` | 随机 Port |
| `src2/Tiles/Interaction/TileCrackPresentationAdapter.cs` | `Terraria.Tiles.Interaction` | 客户端/随机 Adapter |

上述路径是本检查点的 proposed 边界；它们已在 `src2` 保存为 focused implementation slice，完整 Player/World/网络接线仍未完成。

### 3.2 源成员映射与实施顺序

1. 先定义 `TileHitKind`、`TileHitTrackingPolicy` 和不可变 `TileHitEntry`，明确 500 容量、501 数组哨兵和 60 tick TTL 的兼容语义。
2. 创建 `TileHitTrackingComponent`，把 `data` 和 `order` 作为同一有界追踪概念的内部存储；不得把 `rand` 或静态 `lastCrack` 隐式塞入组件。
3. 创建 `TileHitTrackingSystem` 和显式 Commands，接管 AddDamage/UpdatePosition/Clear/Prune；系统失败时返回可诊断结果，不直接修改世界 Tile。
4. 最后接入 Player 的两个频道（旧 `hitTile`/`hitReplace`），再把 crack 动画移到 `TileCrackPresentationAdapter`。

### 3.3 单一写入 owner、双写和兼容

`TileHitTrackingSystem` 是 `Entries`、TTL 和 Order 的唯一写入 owner。旧 `HitTile` 只能作为 Compatibility Adapter 的输入/输出桥接，过渡期间采用单向读取或 shadow compare，不能由旧池和新 Component 同时推进 TTL。`TileHitEntry` 的表现字段可由 projection 维护，但不能反向修改 `AccumulatedDamage`。

满容量、坐标移动和清理的旧排序规则必须先由 focused verifier 固化，未固化前不得删除旧 pool。世界 Tile 变更由其他 owner 通过 command handoff；不做直接双写。

### 3.4 网络、快照、持久化和回滚

默认不复制/持久化条目，因为 Version4 证据只确认 Player-local 初始化和表现字段；若实际客户端需要网络 crack 状态，应新增版本化 client projection，而不是把 `TileHitEntry` 作为世界存档。`X/Y` 只能映射到 proposed `TileCoordinate`；`TileEntityId`、WorldSectionId 和 NetworkId 由 `integration-review` 统一。

回滚条件：满池淘汰次序变化、TTL 边界变化、裂纹动画随机序列变化、世界 Tile 被缓存提前提交或两个 Player 频道串写。回滚时断开新 System 的 Player adapter，保留旧 `HitTile` 调用路径和诊断数据。

### 3.5 验证计划

第一组和第二组已执行纯 Component/Query/Command focused verifier：build 退出码 0，warning/error 均为 0；第二组随后以 `--no-build --no-restore` 运行退出码 0，输出 `P07 focused verifier passed: 7 scenarios`。Player interaction integration、世界 Tile、网络、持久化和跨分区 owner 仍未闭合；后续组继续按 BUILD-CONCURRENCY-1 串行构建并先观察 verifier 红灯。

明确声明：本检查点已执行局部 C# build/run，但没有行为等价、网络、持久化或跨分区 owner 验证。

## 4. 检查点三：ItemTagEffectState

### 4.1 Proposed 目标文件、目录和命名空间

| 目标（proposed） | 命名空间（proposed） | 角色 |
|---|---|---|
| `src2/Combat/TagEffects/PlayerTagEffectStateComponent.cs` | `Terraria.Combat.TagEffects` | Player 附着的标签/Proc 暂态状态 |
| `src2/Content/StatusEffects/TagEffectDefinition.cs` | `Terraria.Content.StatusEffects` | UniqueTagEffect 内容/协议定义 |
| `src2/Content/StatusEffects/WhipTagEffectDefinition.cs` | `Terraria.Content.StatusEffects` | 鞭子标签参数定义 |
| `src2/Combat/TagEffects/TagEffectLifecycleSystem.cs` | `Terraria.Combat.TagEffects` | 切换、tick、清理唯一写入 System |
| `src2/Combat/TagEffects/TagEffectHitSystem.cs` | `Terraria.Combat.TagEffects` | 命中修改与 tagged/proc 事件边界 |
| `src2/Combat/TagEffects/TagEffectHitEligibilityQuery.cs` | `Terraria.Combat.TagEffects` | 只读资格 Query |
| `src2/Combat/TagEffects/Commands/SetActiveTagEffectCommand.cs` | `Terraria.Combat.TagEffects.Commands` | 当前效果切换 Command |
| `src2/Combat/TagEffects/Commands/ApplyTagToNpcCommand.cs` | `Terraria.Combat.TagEffects.Commands` | 标签应用 Command |
| `src2/Combat/TagEffects/Commands/EnableTagProcCommand.cs` | `Terraria.Combat.TagEffects.Commands` | Proc 激活 Command |
| `src2/Combat/TagEffects/Commands/ClearTagProcCommand.cs` | `Terraria.Combat.TagEffects.Commands` | Proc 清除 Command |
| `src2/Combat/TagEffects/Commands/ResetNpcTagStateCommand.cs` | `Terraria.Combat.TagEffects.Commands` | NPC 槽位清理 Command |
| `src2/Combat/TagEffects/TagEffectBehaviorPort.cs` | `Terraria.Combat.TagEffects` | 效果行为 Adapter/Port |
| `src2/Transport/TagEffects/TagEffectNetworkAdapter.cs` | `Terraria.Transport.TagEffects` | 网络协议 Adapter |
| `src2/Transport/TagEffects/TagEffectStateProjection.cs` | `Terraria.Transport.TagEffects` | 客户端/网络状态 Projection |

上述路径是本检查点的 proposed 边界；ItemTagEffectState 的局部 implementation slice 已记录在本文 `implementationEvidence`，`Player`、`NPC`、`Projectile`、Item definition catalog、Buff、Damage 和网络实体 ID 仍不在本检查点内复制。

### 4.2 源成员到目标角色和实施顺序

1. 先定义 `TagEffectDefinition`、`WhipTagEffectDefinition` 和 `PlayerTagEffectStateComponent`，把 `_owner`/`_effect` 的对象引用改成显式稳定引用候选和定义 ID；保留旧数组索引仅作为 Compatibility Adapter 输入。
2. 建立纯 `TagEffectHitEligibilityQuery` 和不依赖第三方对象的 mark/timer 值对象，先固定过期、无效 type、槽位复用和重复命令规则。
3. 建立 `TagEffectLifecycleSystem`，接管 TrySetActiveEffect、Update、ResetNPCSlotData、ClearProc 和标记应用/Proc 激活的状态变换；系统是 timer、active type 和 marks 的单一写入 owner。
4. 建立 `TagEffectBehaviorPort` 与 `TagEffectHitSystem`，把 `ModifyTaggedHit`/`ModifyProcHit` 放在伤害提交前，把 `OnTaggedHit`/`OnProcHit` 放在命中提交后；行为 Port 的副作用通过命令/事件返回。
5. 最后接入 Projectile、Player update/reset、ItemID definition catalog 和网络 Adapter；先 shadow compare，再移除旧 `TagEffectState` 的直接写入。

### 4.3 单一写入 owner、兼容和网络迁移

`TagEffectLifecycleSystem` 是 `ActiveEffectTypeId`、tag/proc timers 和 mark 集合的唯一写入 owner；`TagEffectHitSystem` 只能提交命令或消费结果。旧 `TimeLeftOnNPC`/`ProcTimeLeftOnNPC` 与新 mark 集合不得并行递减；过渡期可单向读取旧数组作 shadow compare，但只能有一条推进路径。`UniqueTagEffect` 和 `WhipTagEffect` 旧定义通过 Definition Adapter 读取，不建立新旧定义的长期双写。

网络迁移分三步：先由 `TagEffectStateProjection` 生成与旧稀疏数组等价的诊断 payload；再由 `TagEffectNetworkAdapter` 处理 Player NetworkId、effect type、Tag timer 和可选 Proc timer，并验证 `NetSync`/`SyncProcs`；最后切断旧 `NetModule` 直接写核心状态。`Deserialize` 当前缺证，因此不能在计划中假定服务端接收方向、权限、包版本或乱序策略；这些是 `integration-review` blocking decision。

Tag marks、Proc timers 和当前 effect 默认不进入世界持久化。若断线重连需要恢复，只能以版本化客户端/服务端快照定义，不得把 NPC 槽位或 `whoAmI` 写成 PersistentId。Player 的 NetworkId、NPC 的 NetworkId、稳定 EntityId、PersistentId 与外部账户/平台 ID 由跨分区 ID contract 统一。

### 4.4 回滚条件、验证和构建计划

回滚条件：切换效果后旧 timer 残留；NPC 槽位复用导致旧目标获得标签；同一命中重复 Proc；tagged damage/crit 与旧路径不一致；或客户端包改变服务端权威状态。回滚时断开 Projectile/Player/Network Adapter，恢复旧 `TagEffectState` 调用路径，保留 shadow diagnostics，不删除新旧存档字段（本计划默认不新增持久化字段）。

`TAG-EFFECT-SWITCH-CLEAR`、`TAG-EFFECT-TIMER-LIFECYCLE`、`TAG-EFFECT-HIT-BOUNDARY`、`TAG-EFFECT-NETWORK-PROJECTION` 已由局部 verifier 运行通过（4 个新增场景）；build 退出码 0，warning/error 均为 0，随后 `--no-build --no-restore` run 退出码 0，输出 `P07 focused verifier passed: 11 scenarios`。网络 Deserialize、NPC 回调、重放/权限和跨分区 owner 仍是 evidence-gap。

明确声明：本检查点已执行局部 C# build/run；没有行为等价、网络 Deserialize、NPC 回调、重放/权限、持久化或跨分区 owner 验证，未完成接线仍为 proposed。

## 5. 检查点四：LockOnTargetingState

### 5.1 Proposed 目标文件、目录和命名空间

| 目标（proposed） | 命名空间（proposed） | 角色 |
|---|---|---|
| `src2/Input/LockOn/LockOnPolicy.cs` | `Terraria.Input.LockOn` | 范围/保持时长策略 |
| `src2/Input/LockOn/LockOnSelectionComponent.cs` | `Terraria.Input.LockOn` | 本地 Player/input session 选择状态 |
| `src2/Input/LockOn/LockOnCandidateQuery.cs` | `Terraria.Input.LockOn` | 纯候选 Query |
| `src2/Input/LockOn/LockOnTargetQuery.cs` | `Terraria.Input.LockOn` | 当前目标快照 Query |
| `src2/Input/LockOn/LockOnPredictionQuery.cs` | `Terraria.Input.LockOn` | 预测位置 Query |
| `src2/Input/LockOn/LockOnInputSystem.cs` | `Terraria.Input.LockOn` | 候选/选择状态唯一写入 System |
| `src2/Input/LockOn/LockOnCursorProjectionAdapter.cs` | `Terraria.Input.LockOn` | `SetUP`/`SetDOWN` 光标 projection Adapter |

这些路径是本检查点的 proposed 边界；它们已在 `src2` 保存为 focused implementation slice，NPC、Player、ItemID、WorldGen、Collision 和网络身份类型仍不在本检查点复制。

### 5.2 源成员到目标角色与实施顺序

1. 先定义 `LockOnPolicy` 和 `LockOnSelectionComponent`，把旧 NPC 槽位候选包装为 Compatibility Adapter 输入，明确 `SelectedCandidateIndex` 的 `-1`/越界边界。
2. 建立 `LockOnCandidateQuery`、`LockOnTargetQuery` 和 `LockOnPredictionQuery` 的纯输入/输出契约；世界碰撞、重力、NPC 位置和 ItemID 参数通过显式 ports 提供。
3. 建立 `LockOnInputSystem`，接管 `_canLockOn`、候选集合和选中索引的写入；在未补齐候选写入者前不得删除旧静态字段。
4. 建立 `LockOnCursorProjectionAdapter`，在 Main/Player 的显式 scheduler scope 内实现 setup/reset；投影失败只清理投影上下文，不修改 CombatTarget 或 Damage 状态。
5. 连接 Item use/Projectile spawn 的一次性 target/prediction snapshot，并通过 integration-review 验证客户端预测不越权为服务端目标。

### 5.3 单一写入 owner、兼容、网络和持久化

`LockOnInputSystem` 是 `CanLockOn`、候选引用和选择索引的唯一写入 owner；`LockOnTargetQuery`、`LockOnPredictionQuery` 只读。旧 `_targets`/`_pickedTarget` 可以由 Adapter 单向读取作 shadow compare，但不能与新组件同时推进选择状态。`SetUP`/`SetDOWN` 只由 Cursor Projection Adapter 管理临时光标状态。

锁定缓存默认不持久化。网络若需要传递选择意图，应发送版本化输入/目标快照投影，并在服务端重新验证目标；不直接复制客户端预测位置或 NPC 数组槽位。回滚时断开 Item/Projectile 接线，恢复旧 `LockOnHelper` 投影路径，保留候选/预测差异诊断。

### 5.4 回滚条件与验证计划

回滚条件：无效/离范围目标仍被选中；`-1` 或槽位复用造成错误目标；预测位置与旧速度/碰撞/重力结果不一致；Main/Player 的 projection scope 泄漏；或客户端选择被当作服务端伤害目标。`LOCK-ON-CANDIDATE-VALIDITY`、`LOCK-ON-HOLD-LIFETIME`、`LOCK-ON-PREDICTION`、`LOCK-ON-PROJECTION-SCOPE`、`LOCK-ON-ID-BOUNDARY` 已由局部 verifier 运行通过（5 个新增场景）；客户端/服务端权限、ItemCheck/Projectile integration 和跨分区 owner 仍是 evidence-gap。

已按 BUILD-CONCURRENCY-1 先运行纯 Query/Component verifier，再串行 build；build 退出码 0，warning/error 均为 0，随后 `--no-build --no-restore` run 退出码 0，输出 `P07 focused verifier passed: 16 scenarios`。ItemCheck/Projectile integration、客户端/服务端权限和跨分区 owner 仍未验证，`verificationStatus: partial`。

## 6. 检查点五：SharedCombatTextState

### 6.1 Proposed 目标文件、目录和命名空间

| 目标（proposed） | 命名空间（proposed） | 角色 |
|---|---|---|
| `src2/Presentation/CombatText/CombatTextPalette.cs` | `Terraria.Presentation.CombatText` | 颜色语义定义 |
| `src2/Presentation/CombatText/CombatTextEntryComponent.cs` | `Terraria.Presentation.CombatText` | 100 槽表现状态 |
| `src2/Presentation/CombatText/CombatTextPoolPolicy.cs` | `Terraria.Presentation.CombatText` | 槽位容量/淘汰策略 |
| `src2/Presentation/CombatText/CombatTextSpawnCommand.cs` | `Terraria.Presentation.CombatText` | 表现生成请求 |
| `src2/Presentation/CombatText/CombatTextPresentationSystem.cs` | `Terraria.Presentation.CombatText` | 条目唯一写入与动画推进 |
| `src2/Presentation/CombatText/CombatTextPaletteQuery.cs` | `Terraria.Presentation.CombatText` | 纯颜色选择 Query |
| `src2/Presentation/BuffText/ProjectileBuffTextFilter.cs` | `Terraria.Presentation.BuffText` | 投射物计数文本筛选输入 |
| `src2/Presentation/CombatText/CombatTextNetworkProjection.cs` | `Terraria.Presentation.CombatText` | 网络/客户端单向 Projection |

上述路径是本检查点的 proposed 边界；它们已在 `src2` 保存为 focused implementation slice，不把 `Main.combatText` 或 Version4 静态槽位直接作为最终 ECS 结构。

### 6.2 源成员到目标角色和实施顺序

1. 先建立 `CombatTextPalette`、`CombatTextEntryComponent` 和 `CombatTextPoolPolicy`，把颜色常量与 100 槽容量从可变表现条目中分离。
2. 建立 `CombatTextSpawnCommand`、`CombatTextPaletteQuery` 和事件来源契约，明确只消费已提交 Damage/Heal/Regen 事实。
3. 建立 `CombatTextPresentationSystem`，统一处理 NewText、slot allocation、animation update、expiry 和 clearAll；禁止 renderer 直接写条目。
4. 建立 `ProjectileBuffTextFilter`，通过显式 activity query 读取投射物，不让 `projectilesToLookFor` 依赖全局 `Main.projectile`。
5. 最后接入 Player/NPC/Damage/Health/Network adapters，并用 shadow compare 固定颜色、文本、槽位和清理差异。

### 6.3 单一写入 owner、网络/持久化和兼容

`CombatTextPresentationSystem` 是表现条目唯一写入 owner；`CombatTextPalette` 和 `CombatTextPoolPolicy` 只读。旧 `CombatText.NewText` 可在兼容期转换为 `CombatTextSpawnCommand`，但不能同时让旧静态池和新 component 推进 lifetime。`clearAll` 只映射到 presentation clear command，不影响模拟状态。

网络迁移优先保留服务端 Damage/Heal 事实或版本化表现事件作为源；客户端 Adapter 负责构造/接收文本条目。表现条目、颜色、alpha、位置、旋转和槽位不持久化。`CachedProjectileCounterBuffTextHandler` 的缓存输入通过一次性不可变投影传递，处理结果缺证时不改变现有 Buff 协议。

### 6.4 回滚条件与验证计划

回滚条件：伤害/治疗事实被文本创建路径吞掉或反向修改；满池/清理造成表现泄漏；网络重复事件生成双文本；颜色/crit/dot 语义变化；或槽位索引被错误序列化。回滚时断开 NewText/renderer/network Adapter，恢复旧表现池，保留事件差异诊断。

`COMBAT-TEXT-PALETTE`、`COMBAT-TEXT-POOL`、`COMBAT-TEXT-SPAWN-PROJECTION`、`COMBAT-TEXT-CLEAR-BOUNDARY`、`BUFF-TEXT-PROJECTILE-FILTER`、`COMBAT-TEXT-NETWORK-ID` 已由局部 verifier 运行通过（6 个新增场景）；build 退出码 0，warning/error 均为 0，随后 `--no-build --no-restore` run 退出码 0，输出 `P07 focused verifier passed: 22 scenarios`。renderer integration、网络去重/重放、持久化和跨分区 owner 仍未闭合。

## 7.5 第六组实现证据

`ITEM-CAPABILITY-COVERAGE`、`ITEM-COMBAT-QUERY`、`ITEM-RESOURCE-QUERY`、`ITEM-PROJECTILE-USE`、`ITEM-PREFIX-VARIANT-MERGE`、`ITEM-PRESENTATION-BOUNDARY` 和 `ITEM-ID-BOUNDARY` 已由局部 verifier 运行通过（7 个新增场景；累计 `P07 focused verifier passed: 29 scenarios`）。新增的 `src2` slice 包含不可变 Item capability/resource/projectile/presentation definitions、强类型 content/slot/use/persistent/network/external identity、纯 capability/resource Query、一次性 use Command、显式有序 modifier merge 和单向 presentation projection。

build 通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行，退出码 0，warning/error 均为 0；生产和 verifier 产物均位于 `Build/bin/`。随后以 `--no-build --no-restore` 运行，退出码 0。该证据只覆盖 definition/Query/Command/Projection slice，不覆盖资源扣除、ammo 解析、Projectile spawn、Damage commit、网络 Deserialize/replay、持久化或跨分区 owner。

## 7. 检查点六：SharedItemCombatAndDamageCapabilityState

### 7.1 Proposed 目标文件、目录和命名空间

| 目标（proposed） | 命名空间（proposed） | 角色 |
|---|---|---|
| `src2/Content/Items/ItemCombatCapabilityDefinition.cs` | `Terraria.Content.Items` | 战斗能力定义/实例快照 |
| `src2/Content/Items/ItemProjectileUseDefinition.cs` | `Terraria.Content.Items` | 投射物类型/速度定义 |
| `src2/Content/Items/ItemResourceRecoveryDefinition.cs` | `Terraria.Content.Items` | 恢复/资源成本定义 |
| `src2/Content/Items/ItemDamageClassCapability.cs` | `Terraria.Content.Items` | 职业分类能力 |
| `src2/Content/Items/ItemPresentationDefinition.cs` | `Terraria.Content.Items` | 稀有度等表现元数据 |
| `src2/Combat/Items/ItemCombatCapabilityQuery.cs` | `Terraria.Combat.Items` | 纯战斗能力转换 Query |
| `src2/Combat/Items/ItemResourceUseQuery.cs` | `Terraria.Combat.Items` | 纯资源资格 Query |
| `src2/Combat/Items/Commands/UseItemCombatCommand.cs` | `Terraria.Combat.Items.Commands` | Item use 提交 Command |
| `src2/Combat/Items/ItemCombatCommandBuilder.cs` | `Terraria.Combat.Items` | 外部 Item/Player/LockOn/TagEffect Adapter/Builder |
| `src2/Combat/Items/ItemCapabilityProjection.cs` | `Terraria.Combat.Items` | UI/tooltip/use preview Projection |
| `src2/Content/Items/ItemCapabilitySnapshot.cs` | `Terraria.Content.Items` | 一次使用的不可变能力组合快照 |
| `src2/Content/Items/ItemCapabilityModifier.cs` | `Terraria.Content.Items` | 显式有序 prefix/variant 合并输入 |
| `src2/Content/Items/*Id.cs` | `Terraria.Content.Items` | content、slot、use、persistent、network、external identity 值对象 |

上述路径是 proposed 边界；对应的 focused implementation slice 已保存于 `src2`。现有 `src/Content/ItemCombatDefinition.cs` 只作为部分重叠证据，不在本会话修改。

### 7.2 源成员到目标角色和实施顺序

1. 建立内容定义和不可变实例快照，先覆盖 damage/knockback/crit/armor penetration/bonus tag damage、shoot/shootSpeed、resource fields、class flags 与 rare。
2. 建立 `ItemCombatCapabilityQuery`、`ItemResourceUseQuery` 和 `ItemCapabilityProjection`，分别固定 combat/use eligibility 与 presentation 读取边界。
3. 建立 `UseItemCombatCommand` 和 `ItemCombatCommandBuilder`，把 LockOn target snapshot、TagEffect modifier、resource check 和 ProjectileUsePlan 作为显式输入；不把 Item 对象引用跨边界传递。
4. 在 integration-review 固化资源扣除、Projectile spawn、DamageRequest 生成和失败/重试事务后，接入 Player.ItemCheck、Projectile setup 和现有 `ItemCombatDefinition` 的兼容 Adapter。
5. 先 shadow compare 旧 `Item` 读取与新 Query/command payload，再逐个关闭旧字段直接读取；`rare` 只接入 presentation/tooltip consumer。

### 7.3 单一写入 owner、兼容、网络和持久化

内容 catalog/prefix adapter 是 definition/instance snapshot 的写入 owner；`ItemCombatCapabilityQuery` 和 `ItemResourceUseQuery` 只读。`UseItemCombatCommand` 是一次性意图，不能由 Query 直接扣除资源或生成 Projectile；Player resource、Projectile、Damage 和 inventory owner 通过显式 command/result 交接，`crossSubsystemOwner: integration-review`。

现有 `src2/Content/ItemCombatDefinition.cs` 可作为兼容输入，但不能与 proposed definitions 长期双写。旧 `Item` 对象、inventory slot 和 static content arrays 只在 adapter 边界读取；网络传输使用版本化 content/ability payload，服务端重新解析 content ID 并重新验证资源、锁定和 TagEffect 输入。一次性 use snapshot 默认不持久化，库存/prefix 由 inventory/persistence owner 单独处理；`rare` 只由 presentation/tooltip projection 消费。

### 7.4 回滚条件与验证计划

回滚条件：prefix/variant 合并改变 damage/knockback/crit；mana 或生命资源被重复扣除；Projectile spawn 与 shoot/shootSpeed 不一致；bonusTagDamage/armorPenetration 越过 TagEffect/Damage 的明确边界；职业分类导致错误倍率；客户端 rare/preview 反向修改模拟；或 content ID 被当作 EntityId/NetworkId。回滚时断开 ItemCheck/Projectile/Network/Presentation adapters，恢复旧 Item 读取路径，保留 capability/query 差异诊断。

focused verifier `ITEM-CAPABILITY-COVERAGE`、`ITEM-COMBAT-QUERY`、`ITEM-RESOURCE-QUERY`、`ITEM-PROJECTILE-USE`、`ITEM-PREFIX-VARIANT-MERGE`、`ITEM-PRESENTATION-BOUNDARY` 和 `ITEM-ID-BOUNDARY` 已运行通过（7 个新增场景；累计 `P07 focused verifier passed: 29 scenarios`）。按 BUILD-CONCURRENCY-1，build 经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行，退出码 0，warning/error 均为 0，`Build/bin/` 下存在生产和 verifier 产物；随后 `--no-build --no-restore` run 退出码 0。该证据仅覆盖 definition/Query/Command/Projection slice，没有执行 Player.ItemCheck/Projectile integration、网络 Deserialize/replay、持久化或行为等价验证，故全分区 `verificationStatus: partial`。

## 8. Integration Handoff：全分区依赖与交接

候选依赖方向为：content definitions/catalogs -> immunity/tag-effect/item capability definitions -> local target/lock-on queries and hit tracking -> item-use/tag-effect/strike commands -> integration-review damage/resource/projectile/death commits -> CombatText、Tile crack 和 lock-on client projections。定义加载、纯 Query、Command 构造、单一写入 System 和权威提交结果之间的顺序必须由显式 scheduler contract 表达；不能用文件顺序或静态数组顺序推断。

本分区交接给 integration-review 的内容包括：97 条成员的 proposed 归属；目标/免疫/击杀快照、HitTile Player-local cache、TagEffect Player state、LockOn local selection、CombatText presentation pool、Item capability/resource definitions 的候选 owner；以及 Damage/Health/Death/Loot、Tile world、Player resources、Projectile spawn、NPC/Player/Buff、Network session、Inventory/Persistence 和 client renderer 的交接点。

### 8.1 Evidence gap、blocking decision 和验证状态

仍需补证：第一轮 P07 专属报告缺失；Version4 裁剪的 TagEffect 网络接收/效果回调、LockOn 候选写入/光标写入、CombatText 槽位动画、Buff 文本处理、Item 资源/Projectile 分支语义；旧槽位到稳定实体/网络身份映射；跨分区 owner、失败重试、去重和最终调度。`integration-review` 必须裁决 ID contract、Damage/Health/Death/Loot/Resource/Projectile 事务、各状态/表现的单一写入 owner、网络权限/版本/重放、Item prefix/variant 合并与客户端 projection scope。

ID 边界必须显式保留：`EntityId/EntityReference` 只表示模拟实体生命周期，`PersistentId` 只表示存档身份，`NetworkId` 只表示复制协议身份，`ExternalId` 只表示平台/账户身份；NPC 槽位、Player `whoAmI`、Item inventory slot 和 CombatText slot 都不能替代这些 ID。

本会话已在 `src2` 创建六组 focused implementation、生产项目和 verifier，并已串行 build/run；没有修改 `src`、Test、Version4 源码、其他分区文档、runner ledger 或 lock。当前 `verificationStatus: partial`，代表 29 个局部 verifier 场景通过，不代表全 P07 行为等价、网络/持久化闭合或跨分区 owner 已裁决。

## 9. 97 条成员覆盖索引

本实施计划的目标角色覆盖与设计文档逐成员表一一对应：

| 叶子组 | 成员数 | proposed 目标覆盖 |
|---|---:|---|
| `SharedCombatTargetingAndImmunity` | 18 | `CombatTargetSnapshot`、`MultiPointHitboxSnapshot`、`NpcDebuffImmunityDefinition`、`NpcKillAttemptSnapshot` 及其 Query/System/Command |
| `SharedHitTileTracking` | 17 | `TileHitTrackingComponent`、`TileHitEntry`、`TileHitKind`、`TileHitTrackingPolicy` 及其 System/Query/Command/Adapter |
| `ItemTagEffectState` | 14 | `PlayerTagEffectStateComponent`、TagEffect/Whip definitions、Lifecycle/Hit System、Commands、Behavior Port、Network Adapter/Projection |
| `LockOnTargetingState` | 7 | `LockOnPolicy`、`LockOnSelectionComponent`、Candidate/Target/Prediction Query、Input System、Cursor Projection Adapter |
| `SharedCombatTextState` | 23 | `CombatTextPalette`、`CombatTextEntryComponent`、Pool Policy、Spawn Command、Presentation System/Query、Buff filter、Network Projection |
| `SharedItemCombatAndDamageCapabilityState` | 18 | Item combat/projectile/resource/class/presentation definitions、capability/resource Query、use Command/Builder、capability Projection |
| **总计** | **97** | 所有成员均已在 proposed 设计文档逐条归属；六组 `src2` focused implementation slice 已保存，但未完成全分区行为等价迁移 |

### 9.1 成员到 proposed 目标的交叉索引

下表把输入报告的每个成员名显式映射到本计划的 proposed 目标；同一行只合并具有相同目标边界的成员，不表示它们已经实现：

| 输入成员 | proposed 目标文件/角色 |
|---|---|
| `MultiPointHitbox.PointSize`, `MultiPointHitbox.Points`, `MultiPointHitbox.BoundingRect` | `MultiPointHitboxSnapshot.cs`；`BoundingRect` 为派生值 |
| `NPCAimedTarget.Type`, `Hitbox`, `Width`, `Height`, `Position`, `Velocity` | `CombatTargetSnapshot.cs` |
| `NPCAimedTarget.Invalid`, `Center`, `Size` | `CombatTargetSnapshotQuery.cs` / `TargetValidityQuery.cs`；纯派生/查询 |
| `NPCDebuffImmunityData.ImmuneToWhips`, `ImmuneToAllBuffsThatAreNotWhips`, `SpecificallyImmuneTo` | `NpcDebuffImmunityDefinition.cs` |
| `NPCKillAttempt.npc`, `netId`, `active` | `NpcKillAttemptSnapshot.cs`；旧对象引用、NetworkId、前置 active 快照分别隔离 |
| `HitTileObject.X`, `Y`, `damage`, `type`, `timeToLive`, `crackStyle`, `animationTimeElapsed`, `animationDirection` | `TileHitEntry.cs` |
| `HitTile.UNUSED`, `TILE`, `WALL` | `TileHitKind.cs` |
| `HitTile.MAX_HITTILES`, `TIMETOLIVE` | `TileHitTrackingPolicy.cs` |
| `HitTile.rand` | `ITileCrackStyleRandom.cs` / `TileCrackPresentationAdapter.cs` |
| `HitTile.lastCrack` | `TileCrackStyleSelectionState` 候选；作用域待 integration-review |
| `HitTile.data`, `order` | `TileHitTrackingComponent.cs` |
| `TagEffectState._owner`, `_effect`, `TimeLeftOnNPC`, `ProcTimeLeftOnNPC`, `Type` | `PlayerTagEffectStateComponent.cs`；稳定 owner/definition ID、tag/proc marks、active effect type |
| `UniqueTagEffect.NetSync`, `SyncProcs`, `TagDuration` | `TagEffectDefinition.cs` |
| `WhipTagEffect.PlayerBuffId`, `PlayerBuffTime`, `PlayerBuffAppliedManually`, `CritChance`, `TagDamage`, `generalWhipMarkDuration` | `WhipTagEffectDefinition.cs` / `TagEffectDefinition.cs` |
| `LockOnHelper.LOCKON_RANGE`, `LOCKON_HOLD_LIFETIME` | `LockOnPolicy.cs` |
| `LockOnHelper._canLockOn`, `_targets`, `_pickedTarget` | `LockOnSelectionComponent.cs` / `LockOnInputSystem.cs`；旧槽位仅 Compatibility Adapter |
| `LockOnHelper.AimedTarget` | `LockOnTargetQuery.cs` |
| `LockOnHelper.PredictedPosition` | `LockOnPredictionQuery.cs` |
| `CachedProjectileCounterBuffTextHandler.projectilesToLookFor` | `ProjectileBuffTextFilter.cs` |
| `CombatText.DamagedFriendly`, `DamagedFriendlyCrit`, `DamagedHostile`, `DamagedHostileCrit`, `OthersDamagedHostile`, `OthersDamagedHostileCrit`, `HealLife`, `HealMana`, `LifeRegen`, `LifeRegenNegative` | `CombatTextPalette.cs` |
| `CombatText.position`, `velocity`, `alpha`, `alphaDir`, `text`, `scale`, `rotation`, `color`, `active`, `lifeTime`, `crit`, `dot` | `CombatTextEntryComponent.cs` / `CombatTextPresentationSystem.cs` |
| `Item.damage`, `knockBack`, `crit`, `armorPenetration`, `bonusTagDamage` | `ItemCombatCapabilityDefinition.cs` |
| `Item.healLife`, `healMana`, `lifeRegen`, `manaIncrease`, `mana` | `ItemResourceRecoveryDefinition.cs` |
| `Item.rare` | `ItemPresentationDefinition.cs` |
| `Item.shoot`, `shootSpeed` | `ItemProjectileUseDefinition.cs` |
| `Item.melee`, `magic`, `ranged`, `summon`, `sentry` | `ItemDamageClassCapability.cs` |

此交叉索引合计覆盖 `3+9+3+3 = 18` 个目标/免疫成员、17 个 HitTile 成员、14 个 TagEffect 成员、7 个 LockOn 成员、23 个 CombatText 成员和 18 个 Item 成员，即 97 条；六组目标仍是 non-authoritative proposed 边界，当前 `implementationStatus: completed` 仅表示六组 `src2` 局部实现均已保存，整体 `verificationStatus: partial`。
