# P07 非权威组件设计：战斗、伤害与状态效果

> 本文是第二轮非权威组件设计。设计边界、接口、System、Query、Command、Adapter 和 Projection 仍是 `proposed`；下方 implementation evidence 只记录已保存的局部 `src2` slice，不表示当前 NLTX 已完成迁移、行为等价、网络/持久化闭合或跨分区 owner 裁决。

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
- status: implemented
  component: SharedCombatTextState
  productionProject: D:\TRbackup\NLTX\src2\Combat\Terraria.CombatStatusEffects.csproj
  verifierProject: D:\TRbackup\NLTX\src2\CombatVerification\Terraria.CombatStatusEffectsVerification.csproj
  sourceFiles:
  - src2/Presentation/CombatText/CombatTextColor.cs
  - src2/Presentation/CombatText/CombatTextColorRole.cs
  - src2/Presentation/CombatText/CombatTextPalette.cs
  - src2/Presentation/CombatText/CombatTextEntry.cs
  - src2/Presentation/CombatText/CombatTextEntryComponent.cs
  - src2/Presentation/CombatText/CombatTextPoolPolicy.cs
  - src2/Presentation/CombatText/CombatTextSpawnCommand.cs
  - src2/Presentation/CombatText/CombatTextPresentationQuery.cs
  - src2/Presentation/CombatText/CombatTextPresentationSystem.cs
  - src2/Presentation/CombatText/CombatTextNetworkProjection.cs
  - src2/Presentation/BuffText/ProjectileBuffTextFilter.cs
  verifierScenarios: 6 additional scenarios; 22 total
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
  output: P07 focused verifier passed: 22 scenarios
  scope: local presentation pool/projection only; no Damage/Health mutation, renderer integration, network replay, persistence, or cross-partition owner claim
- status: implemented
  component: SharedItemCombatAndDamageCapabilityState
  productionProject: D:\TRbackup\NLTX\src2\Combat\Terraria.CombatStatusEffects.csproj
  verifierProject: D:\TRbackup\NLTX\src2\CombatVerification\Terraria.CombatStatusEffectsVerification.csproj
  sourceFiles:
  - src2/Content/Items/ItemContentId.cs
  - src2/Content/Items/ProjectileContentId.cs
  - src2/Content/Items/InventorySlotIndex.cs
  - src2/Content/Items/ItemUseId.cs
  - src2/Content/Items/ItemPersistentId.cs
  - src2/Content/Items/ItemNetworkId.cs
  - src2/Content/Items/ItemExternalId.cs
  - src2/Content/Items/ItemCombatCapabilityDefinition.cs
  - src2/Content/Items/ItemProjectileUseDefinition.cs
  - src2/Content/Items/ItemResourceRecoveryDefinition.cs
  - src2/Content/Items/ItemDamageClassCapability.cs
  - src2/Content/Items/ItemPresentationDefinition.cs
  - src2/Content/Items/ItemCapabilitySnapshot.cs
  - src2/Content/Items/ItemCapabilityModifier.cs
  - src2/Combat/Items/ItemCombatCapabilityQuery.cs
  - src2/Combat/Items/ItemResourceUseQuery.cs
  - src2/Combat/Items/Commands/UseItemCombatCommand.cs
  - src2/Combat/Items/ItemCombatCommandBuilder.cs
  - src2/Combat/Items/ItemCapabilityProjection.cs
  - src2/CombatVerification/Program.cs
  verifierScenarios: 7 additional scenarios; 29 total
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
  output: P07 focused verifier passed: 29 scenarios
  scope: local Item definition/query/command/projection slice only; no resource deduction, projectile spawn, damage commit, ammo resolution, network replay/Deserialize, persistence, or cross-partition owner claim
evidence-gap:
- 领取恢复时记录的输入报告 SHA-256 为 `b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`，当前同一路径观测为 `7E1ACF1A7CE9299EDBEFB5530093B603F7F89D1360E13CA0E7F86AA32E60F9DF`；runner 当前仍验证 97 条成员，本会话未修改输入报告，该 source-report drift 需由 integration-review 处理。
- 本分区第一轮专属 outputReport 尚不存在；本设计不能把它当作已完成研究材料。
- Version4 的若干方法和直接调用者是框架裁剪或空实现，不能据此确认完整的网络反序列化、渲染和清理语义。
- 本轮没有读取或裁决其他 P 分区的设计；跨分区 owner 和最终调度顺序仍未闭合。
- `TagEffectState.NetModule.Deserialize`、`WriteNPCChange`、`ApplyTagToNPC`、`EnableProcOnNPC` 和各 `UniqueTagEffect` 回调在 Version4 证据中是裁剪/空实现；网络接收、标记应用和具体效果语义需补证。
- Version4 的 TagEffect 数组按 NPC 槽位索引，稳定 EntityReference 与 NetworkNpcId 的映射、槽位复用清理和重复网络命令语义尚未闭合。
- `LockOnHelper` 没有在已读取的 Version4 文件中给出 `_canLockOn`、`_targets` 和 `_pickedTarget` 的写入者；`SetLockPosition`/`ResetLockPosition` 及完整候选筛选/失效清理语义被裁剪。
- 锁定预测依赖 `NPC.GetNPCLocation`、世界固体查询、重力和当前 ItemID 参数；这些跨空间、输入和物品能力边界的组合尚未由本分区闭合。
- `CombatText.NewText` 的槽位选择、合并/重叠策略和实际动画更新在 Version4 证据中被裁剪；不能从调用点推断完整表现生命周期。
- `CachedProjectileCounterBuffTextHandler.HandleBuffText` 是裁剪/空实现，`projectilesToLookFor` 的缓存失效和文本格式语义未确认。
- Item 的 prefix/variant、ammo 选择、使用资源扣除和具体 Projectile spawn 分支分散在 Player/Projectile/ItemID；本分区不能单方面闭合全部能力组合和失败重试语义。
- Item.rare 的颜色/排序/价值表现消费者不在本分区证据闭环内，不能把稀有度当作战斗权威。
blocking-decision:
- 输入报告哈希漂移是否可作为本轮设计输入，需由 integration-review 结合 97 条成员校验和原始报告来源裁决。
- `EntityReference`、持久化 ID、网络 ID、旧数组槽位之间的最终映射由 `integration-review` 决定。
- 伤害提交、死亡提交、掉落/归因提交之间的事务根和先后关系由 `integration-review` 决定。
- TagEffect 的效果定义注册表、Player 状态 owner、NPC 标记状态和网络同步谁拥有最终写入权，必须在实施前由 `integration-review` 固化；本分区只提出候选边界。
- 锁定候选的筛选 owner、客户端本地选择的生命周期、预测位置的世界查询 owner，以及光标 projection 与 Item/Projectile 使用的调度契约需由 `integration-review` 固化。
- CombatText 槽位池 owner、伤害/治疗事实到客户端表现的去重与网络投影契约，以及 Buff 文本筛选器的缓存 owner 需由 `integration-review` 固化。
- Item 能力定义、实例 prefix/variant 合并、资源扣除、Projectile spawn、伤害提交和稀有度表现的最终 owner 与顺序需由 `integration-review` 固化。

## 1. 范围与排除

当前分区覆盖 `SharedRuntimeMechanisms` 下的六个叶子组，共 97 条成员：目标与免疫 18 条、Tile 命中追踪 17 条、TagEffect 14 条、锁定目标 7 条、CombatText 23 条、Item 战斗能力 18 条。

本设计只提出这些成员的状态所有权、访问边界和后续实施计划。Player/NPC/Projectile 的完整生命、移动、输入、物品容器、网络会话、持久化、掉落、UI 和渲染实现不属于本分区；它们只能作为直接调用者或 integration-risk 出现。不存在把六个叶子重新合并为 `CombatComponent` 的方案。

## 2. 证据角色

Version4 是事实来源。本轮实际读取了以下 Version4 文件及直接调用者：

| 来源 | 已核对事实 |
|---|---|
| `D:\TRbackup\Version4\Terraria.DataStructures\MultiPointHitbox.cs:5-23` | 三个几何字段，构造时由 points 计算 `BoundingRect`。 |
| `D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs:6-54` | NPC、Player 和可选 tank pet 形成一次性目标快照，`Invalid`、`Center`、`Size` 为派生值。 |
| `D:\TRbackup\Version4\Terraria.DataStructures\NPCDebuffImmunityData.cs:5-37`、`Terraria.ID\NPCID.cs:390-...` | 内容定义按 NPC type 应用到 NPC 的 `buffImmune` 数组。 |
| `D:\TRbackup\Version4\Terraria.DataStructures\NPCKillAttempt.cs:3-24`、`Terraria\Player.cs:11970-11988`、`Terraria\Projectile.cs:12510-12520` | Strike 前创建快照，Strike 后以 `DidNPCDie()` 决定是否进入击杀处理。 |
| `D:\TRbackup\Version4\Terraria.GameContent.Items\TagEffectState.cs:8-245`、`Player.cs:1321,14979,23915-23925,26567-26575`、`Projectile.cs:11876,12460,12520,12632-12665`、`NetMessage.cs:2657` | TagEffect 状态附着 Player，按 NPC 槽位保存倒计时，逐 tick、重置、命中修改和网络同步。 |
| `D:\TRbackup\Version4\Terraria.GameInput\LockOnHelper.cs:10-100`、`Main.cs:11528-11552`、`Player.cs:19633-19642` | 锁定候选和预测位置是输入/客户端计算边界；主循环在投射物和 ItemCheck 前后调用 SETUP/SETDOWN。 |
| `D:\TRbackup\Version4\Terraria\CombatText.cs:6-75`、`Main.cs:948,3499`、`Player.cs:3478-3490,22322-22324`、`NPC.cs:64009-64027,67506-67516,78091` | CombatText 是固定 100 槽的瞬态表现池，伤害/治疗调用可本地创建或经网络发送。 |
| `D:\TRbackup\Version4\Terraria\HitTile.cs:9-82`、`Player.cs:1158-1160,26567-26568` | 每个 Player 初始化两个 501 槽 HitTile 池；条目包含位置、命中累计、TTL 和裂纹动画数据。 |
| `D:\TRbackup\Version4\Terraria\Item.cs:156-296,333-335,451-492`、`Player.cs:3451-3461,23548-23583,23418-23496` | Item 字段同时包含攻击、资源、投射物使用、伤害类别和稀有度；定义初始化和 prefix/variant 修改不属于同一生命周期。 |

公开 API 仅作交叉验证：本地 tModLoader v2026.07 首页 `D:\TRbackup\tmodloader-api-docs-stable\index.html`；`class_combat_text.html:98-100,112-200` 说明 CombatText 是伤害/治疗浮字并可由网络消息同步；`class_hit_tile.html:104-139` 展示 HitTile 的命中、累计、清理、排序/绘制边界；`struct_n_p_c_aimed_target.html:97-120` 和 `class_lock_on_helper.html:141-147` 确认目标快照与锁定预测是公开查询形状；`class_item.html:690-701,808-846,1042-1044` 交叉确认 damage、heal、knockback 和 shootSpeed 的公开语义。公开文档不替代 Version4 私有调用链。

SS14 只读了 `Content.Shared/Damage/Components/DamageableComponent.cs:13-83`、`Content.Server/Destructible/DestructibleSystem.cs:55-181`、`Content.Client/Damage/DamageVisualsSystem.cs:31-54` 和 `Content.Server/Damage/Systems/DamageRandomPopupSystem.cs:13-27`。它仅支持“权威伤害状态、服务端 System、客户端视觉/弹出效果分离”的组织模式，不支持推断 Terraria 字段或行为。

## 3. 当前 NLTX 状态

- `src/Content/ItemCombatDefinition.cs` 已有 `proposed` 方向的 Item 战斗定义形状，包含 Damage、KnockBack、CritChance、ArmorPenetration、BonusTagDamage、ShootTypeId、ShootSpeed 和职业布尔值；它不覆盖本分区 Item 的治疗、生命恢复、法力和稀有度字段。
- `src2/Combat/DamageRequest.cs`、`DamageResult.cs`、`DamageEligibilityQuery.cs` 和 `DamageResolutionSystem.cs` 已存在，但不能据此声称覆盖 Version4 的锁定、TagEffect、HitTile、CombatText、NPC kill attempt 或完整 Item 语义。
- `src2/Combat/ImmunityComponent.cs` 是按窗口建模的现有类型，`src2/Combat/StatusEffectSlotsComponent.cs` 与 `src2/StatusEffects/StatusEffectsComponent.cs` 语义不等价于 Version4 的 NPC 槽位 TagEffect；不得建立未经整合的长期双写。
- 本轮未确认 NLTX 存在 `MultiPointHitbox`、`NPCAimedTarget`、`NPCDebuffImmunityData`、`NPCKillAttempt`、`TagEffectState`、`LockOnHelper`、`CombatText` 或 `HitTile` 的同名实现。
- 当前 NLTX 工作树包含用户/其他会话的既有变更；本会话不修改这些无关变更，只新增本分区指定的两个文档。

## 4. Proposed 边界总览

| proposed 边界 | 权威性/范围 | 主要读者 | 唯一写入方向 | 不承载 |
|---|---|---|---|---|
| `CombatTargetSnapshot` 与 `MultiPointHitboxSnapshot` | 瞬态几何/目标快照 | Combat、NPC AI、Projectile | `TargetSnapshotQuery` 在一次调用中构造 | 长期实体状态、网络/存档真值 |
| `NpcDebuffImmunityDefinition` | 内容定义 | NPC 初始化、StatusEffect 资格 Query | `NpcStatusImmunityApplySystem` 生成实体运行时免疫状态 | 命中冷却、伤害结果 |
| `PlayerTagEffectStateComponent` | Player 附着的暂态权威状态 | TagEffect System、Projectile hit flow、网络 Adapter | `TagEffectLifecycleSystem`/`TagEffectHitSystem` | Player 对象引用、UI 表现、外部网络类型 |
| `TileHitTrackingComponent` | Player 本地/客户端暂态缓存 | Tile interaction、crack projection | `TileHitTrackingSystem` | 世界 Tile 权威、持久化世界地形 |
| `LockOnSelectionComponent` | 客户端输入/选择缓存 | Lock-on Query、Item use adapter | `LockOnInputSystem` | 服务端伤害目标权威 |
| `CombatTextEntryComponent` | 客户端表现状态 | CombatText presentation | `CombatTextPresentationSystem`/pool Adapter | 伤害、治疗、死亡的权威结果 |
| `ItemCombatCapabilityDefinition` 与资源能力定义 | 内容/物品实例快照 | Item use、Projectile spawn、Combat command builder | Content catalog/Item adapter | 受击实体生命和结算结果 |

跨两个或以上子系统的 Entity、NPC、Player、Item、Projectile、Tile 和网络快照引用均保留 `crossSubsystemOwner: integration-review`。

## 5. 检查点一：SharedCombatTargetingAndImmunity

### 5.1 成员逐条归属（18 条）

| 来源成员 | Version4 证据 | proposed 角色 | 状态类型 | 生命周期/写者 | evidenceStatus |
|---|---|---|---|---|---|
| `MultiPointHitbox.PointSize` | `Terraria.DataStructures\MultiPointHitbox.cs:7,13-16` | `MultiPointHitboxSnapshot.PointSize` | 瞬态几何输入 | 构造时写入，调用结束后失效 | confirmed |
| `MultiPointHitbox.Points` | 同上 `:9,13-21` | `MultiPointHitboxSnapshot.Points` 只读快照 | 瞬态几何输入 | Projectile 碰撞阶段创建 | partial（数组防御复制未确认） |
| `MultiPointHitbox.BoundingRect` | 同上 `:11,17-22` | 纯派生值 | 派生 | 由 Points 计算，不单独写入 | confirmed |
| `NPCAimedTarget.Type` | `NPCAimedTarget.cs:8,26-48` | `CombatTargetSnapshot.TargetKind` | 瞬态快照 | NPC targeting System 构造 | confirmed |
| `NPCAimedTarget.Hitbox` | `:10,26-52` | `CombatTargetSnapshot.Hitbox` | 瞬态快照 | 从 NPC/Player/Projectile 读取 | confirmed |
| `NPCAimedTarget.Width` | `:12,30-50` | `CombatTargetSnapshot.Width` | 瞬态快照 | 同上 | confirmed |
| `NPCAimedTarget.Height` | `:14,31-50` | `CombatTargetSnapshot.Height` | 瞬态快照 | 同上 | confirmed |
| `NPCAimedTarget.Position` | `:16,32-51` | `CombatTargetSnapshot.Position` | 瞬态快照 | 同上 | confirmed |
| `NPCAimedTarget.Velocity` | `:18,33-52` | `CombatTargetSnapshot.Velocity` | 瞬态快照 | 同上 | confirmed |
| `NPCAimedTarget.Invalid` | `:20` | `TargetValidityQuery` | 纯查询 | 不写状态 | confirmed |
| `NPCAimedTarget.Center` | `:22` | `TargetGeometryQuery.Center` | 纯派生 | 不写状态 | confirmed |
| `NPCAimedTarget.Size` | `:24` | `TargetGeometryQuery.Size` | 纯派生 | 不写状态 | confirmed |
| `NPCDebuffImmunityData.ImmuneToWhips` | `NPCDebuffImmunityData.cs:7,17-25` | `NpcDebuffImmunityDefinition.ImmuneToWhips` | 内容定义 | NPC definition catalog 初始化 | confirmed |
| `NPCDebuffImmunityData.ImmuneToAllBuffsThatAreNotWhips` | `:9,17-25` | `NpcDebuffImmunityDefinition.ImmuneToNonWhipBuffs` | 内容定义 | 同上 | confirmed |
| `NPCDebuffImmunityData.SpecificallyImmuneTo` | `:11,28-34` | `NpcDebuffImmunityDefinition.SpecificEffectIds` | 内容定义 | 同上；数组排序/去重未确认 | partial |
| `NPCKillAttempt.npc` | `NPCKillAttempt.cs:5,11-16` | `NpcKillAttemptSnapshot.TargetReference` | 瞬态引用快照 | Strike 前构造；死亡处理读取 | partial（旧对象引用不能成为长期 ID） |
| `NPCKillAttempt.netId` | `:7,11-16` | `NpcKillAttemptSnapshot.NetworkNpcId` | 网络快照字段 | Strike 前冻结 | confirmed（网络 ID 语义；不等同实体 ID） |
| `NPCKillAttempt.active` | `:9,11-16` | `NpcKillAttemptSnapshot.WasActive` | 兼容/前置快照 | Strike 前冻结；不作为死亡真值 | confirmed |

### 5.2 Proposed 组件、系统和接口

目标文件和类型均为 `status: proposed`：

- `src2/Combat/Targeting/CombatTargetSnapshot.cs`：`proposed` 瞬态值对象，保存 `TargetKind`、`Hitbox`、`Width`、`Height`、`Position`、`Velocity`；`Center`、`Size`、`IsInvalid` 只读派生。
- `src2/Combat/Targeting/MultiPointHitboxSnapshot.cs`：`proposed` 瞬态值对象；Points 只读或防御复制，BoundingRect 不作为第二份权威状态。
- `src2/Content/StatusEffects/NpcDebuffImmunityDefinition.cs`：`proposed` 内容定义；SpecificEffectIds 由 catalog 提供不可变视图，不能直接暴露可写数组。
- `src2/Combat/Resolution/NpcKillAttemptSnapshot.cs`：`proposed` 一次性快照；保存 `TargetReference` 候选、`NetworkNpcId` 和 `WasActive`，不保存死亡结果。
- `src2/Combat/Targeting/CombatTargetSnapshotQuery.cs`：`proposed` 纯 Query，从实体几何和目标关系构造快照，不修改实体。
- `src2/Combat/Targeting/TargetValidityQuery.cs`：`proposed` 纯 Query，检查 Invalid、实体代际和可命中资格；实体代际规则需 integration-review。
- `src2/Combat/StatusEffects/NpcDebuffImmunityApplySystem.cs`：`proposed` System，把内容定义应用到 NPC 的运行时 StatusEffectImmunity 状态；唯一写入运行时免疫状态，不直接由任意命中调用者修改。
- `src2/Combat/Resolution/NpcStrikeCommitCommand.cs`：`proposed` Command 候选，表达一次 Strike 提交和 `NpcKillAttemptSnapshot` 的边界；实际伤害 owner、死亡 owner 和掉落 owner 留给 integration-review。
- `src2/Combat/Resolution/NpcKillResolutionSystem.cs`：`proposed` System 候选，消费已提交 Strike 结果并生成击杀事实；不得把 `active == false` 单独当作完整死亡证明。

`NpcDebuffImmunityData.ApplyToNPC` 是 Version4 里的有副作用方法（写入 `npc.buffImmune`）；迁移时该效果应集中在 `NpcDebuffImmunityApplySystem`，Query 不得写入。

### 5.3 组合、ID 和顺序

候选数据流：

```text
NPC/Player/Projectile geometry
  -> proposed CombatTargetSnapshotQuery
  -> proposed TargetValidityQuery
  -> proposed NpcStrikeCommitCommand
  -> integration-review damage/health commit
  -> proposed NpcKillResolutionSystem
```

`TargetReference` 不能把当前 `Main.npc[index]` 槽位当作稳定实体身份；`NetworkNpcId` 仅是复制/协议字段；持久化 NPC ID、Entity ID、外部/账户 ID 不能由本分区定义。建议顺序为 `definition catalog load -> immunity apply -> target query -> hit/strike commit -> death/loot handoff`，但 `hit/strike commit` 与死亡/掉落的最终顺序是 `crossSubsystemOwner: integration-review`。

### 5.4 生命周期、失败和副作用

- 目标快照在一次 Query/命中计算内有效；目标已销毁、代际不符或无有效目标时返回显式无效结果，不写实体。
- 免疫定义在 NPC 初始化/重置时应用；SpecificEffectIds 越界、重复和空集合策略未由 Version4 直接确认，必须由 focused verifier 固化。
- Kill attempt 只在 Strike 前后作为值对象传递；网络确认丢失、重复 Strike、死亡已提交和槽位复用应由上游命令版本/去重策略处理。
- 本组不直接进行网络、日志、存档或 UI；网络 ID 只由 proposed Adapter/Projection 处理。

### 5.5 第一组 focused verifier（已执行，局部证据）

`COMBAT-TARGET-SNAPSHOT`、`NPC-IMMUNITY-APPLICATION`、`NPC-KILL-ATTEMPT-BOUNDARY` 已由局部 verifier 运行通过（4 个场景）；它们只证明当前 `src2` focused slice 的快照、免疫应用和击杀提交边界。NPC/Player/tank pet 集成、实际网络/持久化语义和跨分区 owner 仍未闭合。

## 6. 检查点二：SharedHitTileTracking

### 6.1 成员逐条归属（17 条）

| 来源成员 | Version4 证据 | proposed 角色 | 状态类型 | 生命周期/写者 | evidenceStatus |
|---|---|---|---|---|---|
| `HitTileObject.X` | `Terraria\HitTile.cs:13,38-39` | `TileHitEntry.TileX` | Player-local 瞬态缓存 | 命中/移动/清理写入 | confirmed |
| `HitTileObject.Y` | `:15,38-39` | `TileHitEntry.TileY` | Player-local 瞬态缓存 | 同上 | confirmed |
| `HitTileObject.damage` | `:17,40` | `TileHitEntry.AccumulatedDamage` | 瞬态命中累计 | Tile interaction 写入 | confirmed（累计规则在本文件外未闭合） |
| `HitTileObject.type` | `:19,41` | `TileHitEntry.HitKind` | 有限状态 | `UNUSED/TILE/WALL` 分类 | confirmed |
| `HitTileObject.timeToLive` | `:21,42` | `TileHitEntry.RemainingTicks` | 瞬态计时 | 每 tick/prune System 写入 | confirmed |
| `HitTileObject.crackStyle` | `:23,47-50` | `TileHitEntry.CrackStyle` | 表现缓存 | 清理/重新分配时随机选择 | partial（全局随机作用域未闭合） |
| `HitTileObject.animationTimeElapsed` | `:25` | `TileHitEntry.AnimationTicks` | 表现暂态 | crack projection 更新 | confirmed |
| `HitTileObject.animationDirection` | `:27` | `TileHitEntry.AnimationDirection` | 表现暂态 | crack projection 更新 | confirmed |
| `HitTile.UNUSED` | `:54` | `TileHitKind.Unused` | 定义枚举值 | 不写运行时外部状态 | confirmed |
| `HitTile.TILE` | `:56` | `TileHitKind.Tile` | 定义枚举值 | 同上 | confirmed |
| `HitTile.WALL` | `:58` | `TileHitKind.Wall` | 定义枚举值 | 同上 | confirmed |
| `HitTile.MAX_HITTILES` | `:60,74-80` | `TileHitTrackingPolicy.Capacity` | 容量策略 | 初始化时固定为 500 个有效 ID 加哨兵槽 | confirmed |
| `HitTile.TIMETOLIVE` | `:62` | `TileHitTrackingPolicy.DefaultLifetimeTicks` | TTL 策略 | 新条目/刷新时读取 | confirmed |
| `HitTile.rand` | `:64,43-47,73` | `ITileCrackStyleRandom` proposed Adapter | 随机副作用依赖 | 仅裂纹样式选择边界 | confirmed |
| `HitTile.lastCrack` | `:66,47-50` | `TileCrackStyleSelectionState.LastStyle` 候选 | 表现选择缓存 | 清理时防止连续同样样式 | partial（static 作用域与并发未确认） |
| `HitTile.data` | `:68,74-78` | `TileHitTrackingComponent.Entries` | Player-local 有界状态 | Player 初始化/命中/清理 | confirmed |
| `HitTile.order` | `:70,75,79` | `TileHitTrackingComponent.Order` | 索引/缓存 | 初始化和排序/淘汰 System | partial（Version4 该文件未展示完整写者） |

### 6.2 Proposed 组件、系统和边界

- `src2/Tiles/Interaction/TileHitTrackingComponent.cs`（`proposed`）：附着 Player，保存一个命中追踪通道的有界条目集合、顺序索引和 `Channel`；`Entries` 只读暴露，变更只能由 `TileHitTrackingSystem` 提交。`hitTile` 与 `hitReplace` 的双实例关系必须由 Player attachment/integration-review 表达，不复制两个无语义的全局组件。
- `src2/Tiles/Interaction/TileHitEntry.cs`（`proposed`）：保存坐标、命中累计、命中种类、剩余 TTL 以及仅供客户端 crack projection 的动画字段；不保存 Tile 的真实类型、墙体内容或世界持久化状态。
- `src2/Tiles/Interaction/TileHitKind.cs` 和 `TileHitTrackingPolicy.cs`（`proposed`）：分别承载 `UNUSED/TILE/WALL` 和容量/TTL；不以文件顺序定义行为。
- `src2/Tiles/Interaction/TileHitTrackingSystem.cs`（`proposed`）：唯一写 Entry、Order、TTL 和清理；处理 register/add/update/clear/prune。
- `src2/Tiles/Interaction/TileHitLookupQuery.cs`（`proposed`）：按坐标与 kind 纯查找，不能在命中未找到时隐式创建条目。
- `src2/Tiles/Interaction/TileHitCommand.cs`（`proposed`）：表达 `RegisterHit`、`AddDamage`、`UpdatePosition`、`Clear` 和 `ClearAtLocation` 意图；结构变化在 System 统一提交。
- `src2/Tiles/Interaction/ITileCrackStyleRandom.cs` 与 `TileCrackPresentationAdapter.cs`（`proposed`）：隔离随机和 SpriteBatch/客户端绘制；`lastCrack` 是否按 Player、世界或渲染会话归属需 integration-review。

### 6.3 所有权、生命周期和副作用

Version4 在 `Player` 中初始化两个 `HitTile`（`Player.cs:1158-1160,26567-26568`），说明它不是世界 Tile 的长期权威状态。命中累计和 TTL 只能表示本地交互/动画缓存；真正 Tile 破坏、墙体变更、网络广播和持久化由相邻 Tile/World owner 提交。`HitTileObject.Clear` 读取时间种子并选择裂纹样式（`HitTile.cs:34-50`），迁移时随机源必须显式注入 Adapter，不能在 Component 构造中读取时钟。

建议顺序：`TileHitCommand intake -> TileHitLookupQuery -> TileHitTrackingSystem commit -> TileWorld/TileDamage command handoff -> TileHitPresentationProjection`。若世界提交失败，追踪条目是否保留、重试还是清空不能由本组猜测；标为 `evidence-gap`。

### 6.4 ID、网络和持久化

`X/Y` 是 Tile 坐标值，不是 `TileEntityId`、WorldSectionId 或网络 ID；`type` 只表达 hit kind，不是内容定义 ID。Entries、Order、crackStyle、animationTimeElapsed 和 animationDirection 不应进入世界存档；是否在客户端网络快照中复制只能由 Tile interaction integration-review 决定，默认按 local presentation cache 处理。

### 6.5 第二组 focused verifier（已执行，局部证据）

`TILE-HIT-CAPACITY`：验证 0..500 哨兵/容量、满池淘汰和 Order 一致性；`TILE-HIT-TTL`：验证 60 tick 默认 TTL、递减、清理和位置更新；`TILE-HIT-RANDOM-SEAM`：验证随机 Adapter、连续裂纹样式约束和不读取系统时钟的纯 Component；`TILE-HIT-WORLD-BOUNDARY`：确认本地缓存不会直接写持久化 Tile。

## 7. 检查点三：ItemTagEffectState

### 7.1 成员逐条归属（14 条）

| 来源成员 | Version4 证据 | proposed 角色 | 状态类型 | 生命周期/写者 | evidenceStatus |
|---|---|---|---|---|---|
| `TagEffectState._owner` | `Terraria.GameContent.Items\\TagEffectState.cs:96`；Player 初始化于 `Player.cs:26575` | `PlayerTagEffectStateComponent.OwnerReference` 的兼容输入；核心只保存 proposed `PlayerEntityReference` | 归属引用/兼容字段 | Player 创建时绑定；不得保存旧 `Player` 对象供网络或存档使用 | partial（稳定实体引用未闭合） |
| `TagEffectState._effect` | `TagEffectState.cs:98,203-207`；`ItemID.Sets.UniqueTagEffects[type]` | `ActiveTagEffectDefinition` 的不可变引用/定义 ID | 当前效果定义 | `TagEffectLifecycleSystem` 在切换时替换 | confirmed（定义实例的注册/线程安全未确认） |
| `TagEffectState.TimeLeftOnNPC` | `TagEffectState.cs:100,113-117,138-143,148-164` | `PlayerTagEffectStateComponent.NpcTagMarks[*].TagTicksRemaining`；兼容期可由 NPC identity key 索引 | Player 附着的暂态权威状态 | `ApplyTag`、tick、NPC 槽位重置写入 | confirmed（槽位到稳定 ID 映射 partial） |
| `TagEffectState.ProcTimeLeftOnNPC` | `TagEffectState.cs:102,120-143,148-164` | `PlayerTagEffectStateComponent.NpcTagMarks[*].ProcTicksRemaining` | Player 附着的暂态 Proc 状态 | `EnableProc`、`ClearProc`、tick、槽位重置写入 | confirmed（重复/过期 Proc 规则 partial） |
| `UniqueTagEffect.NetSync` | `UniqueTagEffect.cs:5`；`TagEffectState.cs:77-91,194-214` | `UniqueTagEffectDefinition.NetworkSyncPolicy` | 内容/协议策略 | 内容注册时只读；网络 Adapter 读取 | confirmed |
| `UniqueTagEffect.SyncProcs` | `UniqueTagEffect.cs:7`；`TagEffectState.cs:45-50` | `UniqueTagEffectDefinition.SyncProcTimers` | 内容/协议策略 | 内容注册时只读；决定是否输出 Proc 稀疏数组 | confirmed |
| `UniqueTagEffect.TagDuration` | `UniqueTagEffect.cs:9`；`WhipTagEffect.cs:19` | `TagEffectDefinition.TagDurationTicks` | 效果定义 | 定义加载时设置；应用命令读取 | confirmed |
| `WhipTagEffect.PlayerBuffId` | `WhipTagEffect.cs:7` | `WhipTagEffectDefinition.PlayerBuffId` | 内容效果参数 | 内容注册时设置；Whip effect System 读取 | confirmed |
| `WhipTagEffect.PlayerBuffTime` | `WhipTagEffect.cs:9` | `WhipTagEffectDefinition.PlayerBuffDurationTicks` | 内容效果参数 | 同上 | confirmed |
| `WhipTagEffect.PlayerBuffAppliedManually` | `WhipTagEffect.cs:11` | `WhipTagEffectDefinition.ApplyPlayerBuffManually` | 内容效果参数/策略 | 同上；不可由命中结果直接改写 | confirmed |
| `WhipTagEffect.CritChance` | `WhipTagEffect.cs:13` | `WhipTagEffectDefinition.TagCritChance` | 内容效果参数 | 命中修改 Query/System 读取 | confirmed |
| `WhipTagEffect.TagDamage` | `WhipTagEffect.cs:15` | `WhipTagEffectDefinition.TagDamage` | 内容效果参数 | 命中修改 Query/System 读取 | confirmed |
| `WhipTagEffect.generalWhipMarkDuration` | `WhipTagEffect.cs:17,19` | `TagEffectDefinition.DefaultWhipMarkDurationTicks` | 常量策略 | 定义初始化时使用；不做全局可变状态 | confirmed |
| `TagEffectState.Type` | `TagEffectState.cs:104,194-214` | `PlayerTagEffectStateComponent.ActiveEffectTypeId` | 当前效果选择状态 | `TagEffectLifecycleSystem` 唯一写入；切换时清理旧标记 | confirmed（无效 type 策略 partial） |

`_owner` 和 `_effect` 不能作为核心组件中的第三方对象引用；它们分别转换为稳定 Player 引用候选和内容定义引用。两个 `int[]` 的数组形状是 Version4 的兼容实现，不是最终要求把 NPC 槽位当作实体身份。

### 7.2 Proposed 组件、系统和接口

以下是本检查点的 proposed 边界；对应的局部 implementation slice 已记录在本文 `implementationEvidence`，完整 Player/Projectile/网络接线仍未完成：

- `src2/Combat/TagEffects/PlayerTagEffectStateComponent.cs`：附着 Player，保存 `OwnerReference`、`ActiveEffectTypeId` 和按 NPC 目标键索引的 `NpcTagMarks`；每个 mark 同时保存 `TagTicksRemaining` 与 `ProcTicksRemaining`，但不保存 NPC/Player 对象。
- `src2/Content/StatusEffects/TagEffectDefinition.cs`：不可变定义，保存 `NetworkSyncPolicy`、`SyncProcTimers` 和 `TagDurationTicks`；`UniqueTagEffect` 虚方法由 proposed effect behavior port 承接，不把行为塞回定义数据。
- `src2/Content/StatusEffects/WhipTagEffectDefinition.cs`：不可变鞭子定义，保存 Player buff、标签暴击/伤害和默认标记时长；Player buff 的实际添加仍交给 Player status/buff owner。
- `src2/Combat/TagEffects/TagEffectLifecycleSystem.cs`：读取 set/clear/apply/reset 命令，唯一写 `ActiveEffectTypeId`、标记倒计时和 Proc 倒计时；每 tick 只递减正值并删除/保留过期条目的策略须显式固定。
- `src2/Combat/TagEffects/TagEffectHitSystem.cs`：消费命中事件和 `TagEffectHitEligibilityQuery`，调用 proposed behavior port 产生 damage/crit 修改和 tagged/proc 事件；不直接写 NPC 生命或伤害结果。
- `src2/Combat/TagEffects/TagEffectHitEligibilityQuery.cs`：纯查询 `IsTagged`、`CanProc` 和当前效果是否允许此 Projectile/NPC 组合；无效目标、过期标记和 effect type 不匹配返回显式否定。
- `src2/Combat/TagEffects/Commands/SetActiveTagEffectCommand.cs`、`ApplyTagToNpcCommand.cs`、`EnableTagProcCommand.cs`、`ClearTagProcCommand.cs`、`ResetNpcTagStateCommand.cs`：proposed 命令，表达状态提交边界，不让 Projectile 直接改数组。
- `src2/Combat/TagEffects/TagEffectBehaviorPort.cs`：proposed Adapter/Port，把 `OnSetToPlayer`、`OnRemovedFromPlayer`、`OnTagAppliedToNPC`、`ModifyTaggedHit`、`ModifyProcHit`、`OnTaggedHit` 和 `OnProcHit` 等效果扩展点隔离于核心状态；Version4 裁剪空实现不能被当作完成语义。
- `src2/Transport/TagEffects/TagEffectNetworkAdapter.cs` 与 `src2/Transport/TagEffects/TagEffectStateProjection.cs`：proposed 网络边界，负责 Player NetworkId、effect type 和稀疏 NPC timer payload；核心不依赖 `BinaryReader`、`NetPacket` 或 `Main`。

### 7.3 所有权、生命周期、网络和副作用

Version4 的直接链路是 `Player` 初始化状态、Player update 每 tick 调用 `Update`、`Player.ResetNPCSlotData` 清除所有 Player 的对应 NPC 槽位、Projectile 在命中前调用 `ModifyHit`，命中后调用 `TryApplyTag`/`OnHit`/`TryEnableProcOnNPC`，而 `NetMessage` 在玩家同步时请求完整状态同步。由此提出以下单向方向：

```text
Item/Whip definition catalog
  -> proposed TagEffectLifecycleSystem
  -> PlayerTagEffectStateComponent
Projectile hit event
  -> proposed TagEffectHitEligibilityQuery
  -> proposed TagEffectHitSystem
  -> damage/crit or tagged/proc event handoff
Player state
  -> proposed TagEffectStateProjection
  -> network/client projection
```

`TagEffectLifecycleSystem` 是 Player TagEffect 状态唯一写者；`TagEffectHitSystem` 通过命令修改 Proc 清除和命中事件，不直接写 NPC 的生命、Buff、掉落或死亡。效果切换必须先执行旧定义 removal、清理当前 marks，再设置新 type/definition；切换失败不能留下新旧定义与 timer 混合状态。NPC 槽位重置必须在 NPC 销毁/复用事件之后、下一次命中之前完成，具体事件 owner 为 `crossSubsystemOwner: integration-review`。

`NetSync` 只表示该定义允许同步，不是服务端权威标志；客户端收到的状态必须通过版本化 projection/Adapter 应用。`SyncProcs=false` 时不能把 Proc 数组当作必需网络字段。`npcIndex` 是旧协议/数组槽位或网络索引候选，不能直接当作 Entity ID、Persistent ID 或 External ID；Player `whoAmI` 同样是 NetworkId 候选，不是持久化账户 ID。Tag marks 默认不持久化；是否进入客户端快照、是否由服务端广播和断线重同步由 integration-review 决定。

具体 `UniqueTagEffect` 回调可能改变命中伤害/暴击、添加 Player Buff 或生成额外效果，这些是外部副作用。核心只接收显式 effect decision/event；随机、时钟、网络、日志和 UI 不进入 Component。`ApplyTagToNPC`、`EnableProcOnNPC` 和网络反序列化当前证据为空/裁剪，故不能为拒绝、重复、越界、重放和丢包给出已确认行为。

### 7.4 第三组 focused verifier（已执行，局部证据）

`TAG-EFFECT-SWITCH-CLEAR`、`TAG-EFFECT-TIMER-LIFECYCLE`、`TAG-EFFECT-HIT-BOUNDARY`、`TAG-EFFECT-NETWORK-PROJECTION` 已由局部 verifier 运行通过（4 个新增场景）；网络 Deserialize、NPC 回调、重放/权限和跨分区 owner 仍是 evidence-gap。

## 8. 检查点四：LockOnTargetingState

### 8.1 成员逐条归属（7 条）

| 来源成员 | Version4 证据 | proposed 角色 | 状态类型 | 生命周期/写者 | evidenceStatus |
|---|---|---|---|---|---|
| `LockOnHelper.LOCKON_RANGE` | `Terraria.GameInput\\LockOnHelper.cs:13` | `LockOnPolicy.RangePixels` | 输入/选择策略 | 客户端策略定义读取；不属于实体状态 | confirmed |
| `LockOnHelper.LOCKON_HOLD_LIFETIME` | `LockOnHelper.cs:15` | `LockOnPolicy.HoldLifetimeTicks` | 输入/选择策略 | 选择保持策略读取；不属于目标权威 | confirmed |
| `LockOnHelper._canLockOn` | `LockOnHelper.cs:17,82-86,92-96` | `LockOnSelectionComponent.CanLockOn` | 客户端本地输入状态 | proposed `LockOnInputSystem` 写入；当前 Version4 写入者未见 | partial |
| `LockOnHelper._targets` | `LockOnHelper.cs:19,31,45` | `LockOnSelectionComponent.CandidateTargetReferences`；旧 NPC 槽位列表只在 Compatibility Adapter 中存在 | 客户端候选缓存 | 候选收集/清理 System 写入；完整筛选写者未见 | partial（槽位到稳定实体映射未闭合） |
| `LockOnHelper._pickedTarget` | `LockOnHelper.cs:21,25-31,45` | `LockOnSelectionComponent.SelectedCandidateIndex` 与派生 `SelectedTargetReference` | 客户端选择状态 | 输入选择 System 写入；初始化为 -1、失效重选规则未确认 | partial |
| `LockOnHelper.AimedTarget` | `LockOnHelper.cs:23-33` | `LockOnTargetQuery.SelectedTargetSnapshot` | 派生目标查询/兼容投影 | 每次读取按选择状态构造；不写状态 | confirmed（旧 `Main.npc[index]` 引用不能成为核心身份） |
| `LockOnHelper.PredictedPosition` | `LockOnHelper.cs:35-75` | `LockOnPredictionQuery.PredictedAimPosition` | 派生瞄准查询 | 每次读取组合目标快照、速度、世界固体查询、当前 Item 能力和重力 | partial（`GetNPCLocation` 与完整阻挡语义需补证） |

`AimedTarget` 和 `PredictedPosition` 不应成为长期 Component 字段；它们是当前选择的派生视图。`_targets` 里的 `int` 在 Version4 形状上是 NPC 数组槽位候选，不能直接标成 EntityId、PersistentId、NetworkId 或外部账户 ID。

### 8.2 Proposed 组件、系统和边界

以下类型、路径和接口是本检查点的 proposed 边界；对应的局部 implementation slice 已记录在本文 `implementationEvidence`，完整 ItemCheck/Projectile/网络接线仍未完成：

- `src2/Input/LockOn/LockOnPolicy.cs`：保存范围和保持时长等客户端选择策略；常量不注册到模拟实体。
- `src2/Input/LockOn/LockOnSelectionComponent.cs`：附着本地 Player/input session，保存 `CanLockOn`、候选目标引用集合和选择索引；核心集合使用稳定 EntityReference 候选，旧槽位由 Adapter 转换。
- `src2/Input/LockOn/LockOnCandidateQuery.cs`：纯 Query，按范围、目标有效性、可瞄准资格和输入方向读取候选；不修改选择组件。
- `src2/Input/LockOn/LockOnTargetQuery.cs`：纯 Query，从选择状态解析当前目标快照；目标失效时返回显式空结果，不返回悬挂的旧 NPC 对象。
- `src2/Input/LockOn/LockOnPredictionQuery.cs`：纯 Query，复现 `GetNPCLocation`/速度前推、ItemID `LockOnAimAbove`、`LockOnAimCompensation`、世界固体边界和重力坐标转换所需的组合计算。
- `src2/Input/LockOn/LockOnInputSystem.cs`：写入 `CanLockOn`、候选集合和选择索引；候选排序、保持、失效重选和 `LOCKON_HOLD_LIFETIME` 的具体规则必须先补证。
- `src2/Input/LockOn/LockOnCursorProjectionAdapter.cs`：把预测位置转换为客户端光标/瞄准投影；对应 Version4 的 `SetUP`/`SetDOWN` 临时上下文，不反向写伤害或目标生命。

候选数据流：

```text
local input + target snapshots
  -> proposed LockOnCandidateQuery
  -> proposed LockOnInputSystem
  -> LockOnSelectionComponent
  -> proposed LockOnTargetQuery / LockOnPredictionQuery
  -> proposed cursor projection around ItemCheck/projectile update
```

`Main.Update` 在 NPC 更新后调用 `SetUP`，包围 Projectile 更新，再调用 `SetDOWN`；`Player.ItemCheck` 也在 ItemCheck 前后建立同样的临时边界。最终调度器必须表达是否允许嵌套/重复 projection scope，不能依靠文件顺序或静态全局状态推断。

### 8.3 所有权、ID、网络和持久化

`LockOnSelectionComponent` 的 owner 是当前客户端输入会话/本地 Player，不能成为服务端伤害目标真值。`AimedTarget` 可向 Item use adapter 提供一次性目标快照，但实际命中、伤害和死亡仍必须走 Combat/Projectile 的权威命令；`PredictedPosition` 只用于瞄准表现和发射参数候选。

旧 `Main.npc[index]` 的数组索引是兼容槽位；稳定 EntityId 由 ECS world 分配，NetworkId 由网络层分配，PersistentId 由存档/世界 owner 分配，ExternalId 由平台/账户边界分配。锁定选择默认不进入世界持久化，也不复制为服务端权威组件；若多人客户端需要同步，仅发送版本化输入/选择投影，不能把 `_pickedTarget` 当作跨会话主键。

`LockOnAimAbove` 和 `LockOnAimCompensation` 是 Item 内容定义输入，不由锁定组件复制。World solid/tile 查询、重力坐标转换和 `NPC.GetNPCLocation` 是 Adapter/Query 端口；它们的失败应返回可诊断的无目标/未预测结果，而不是修改候选缓存。`SetLockPosition`/`ResetLockPosition` 当前为空实现，故不能声称光标渲染状态的实际写入或清理已确认。

### 8.4 第四组 focused verifier（已执行，局部证据）

`LOCK-ON-CANDIDATE-VALIDITY`、`LOCK-ON-HOLD-LIFETIME`、`LOCK-ON-PREDICTION`、`LOCK-ON-PROJECTION-SCOPE`、`LOCK-ON-ID-BOUNDARY` 已由局部 verifier 运行通过（5 个新增场景）；ItemCheck/Projectile integration、客户端/服务端权限和跨分区 owner 仍未闭合。

## 9. 检查点五：SharedCombatTextState

### 9.1 成员逐条归属（23 条）

| 来源成员 | Version4 证据 | proposed 角色 | 状态类型 | 生命周期/写者 | evidenceStatus |
|---|---|---|---|---|---|
| `CachedProjectileCounterBuffTextHandler.projectilesToLookFor` | `Terraria.DataStructures\\CachedProjectileCounterBuffTextHandler.cs:5-9` | `ProjectileBuffTextFilter.ProjectileTypeIds` | Buff 文本筛选缓存 | 构造时写入；`BuffTextProjection` 读取 | partial（处理/失效语义被裁剪） |
| `CombatText.DamagedFriendly` | `Terraria\\CombatText.cs:8` | `CombatTextPalette.DamagedFriendly` | 表现颜色定义 | 内容/表现初始化；只读 | confirmed |
| `CombatText.DamagedFriendlyCrit` | `CombatText.cs:10` | `CombatTextPalette.DamagedFriendlyCritical` | 表现颜色定义 | 同上 | confirmed |
| `CombatText.DamagedHostile` | `CombatText.cs:12` | `CombatTextPalette.DamagedHostile` | 表现颜色定义 | 同上 | confirmed |
| `CombatText.DamagedHostileCrit` | `CombatText.cs:14` | `CombatTextPalette.DamagedHostileCritical` | 表现颜色定义 | 同上 | confirmed |
| `CombatText.OthersDamagedHostile` | `CombatText.cs:16` | `CombatTextPalette.OthersDamagedHostile` | 派生表现颜色 | 由 Hostile 颜色缩放派生 | confirmed |
| `CombatText.OthersDamagedHostileCrit` | `CombatText.cs:18` | `CombatTextPalette.OthersDamagedHostileCritical` | 派生表现颜色 | 由 HostileCrit 颜色缩放派生 | confirmed |
| `CombatText.HealLife` | `CombatText.cs:20` | `CombatTextPalette.HealLife` | 表现颜色定义 | 只读 | confirmed |
| `CombatText.HealMana` | `CombatText.cs:22` | `CombatTextPalette.HealMana` | 表现颜色定义 | 只读 | confirmed |
| `CombatText.LifeRegen` | `CombatText.cs:24` | `CombatTextPalette.LifeRegen` | 表现颜色定义 | 只读 | confirmed |
| `CombatText.LifeRegenNegative` | `CombatText.cs:26` | `CombatTextPalette.LifeRegenNegative` | 表现颜色定义 | 只读 | confirmed |
| `CombatText.position` | `CombatText.cs:28` | `CombatTextEntry.Position` | 客户端瞬态表现状态 | NewText/animation System 写入 | confirmed（更新规则 partial） |
| `CombatText.velocity` | `CombatText.cs:30` | `CombatTextEntry.Velocity` | 客户端瞬态表现状态 | NewText/animation System 写入 | confirmed（更新规则 partial） |
| `CombatText.alpha` | `CombatText.cs:32` | `CombatTextEntry.Alpha` | 客户端瞬态表现状态 | 动画 System 写入 | confirmed（更新规则 partial） |
| `CombatText.alphaDir` | `CombatText.cs:34` | `CombatTextEntry.AlphaDirection` | 客户端瞬态表现状态 | 动画 System 写入 | confirmed（初始/边界策略 partial） |
| `CombatText.text` | `CombatText.cs:36` | `CombatTextEntry.Text` | 客户端显示文本 | NewText/本地化 Projection 写入 | confirmed |
| `CombatText.scale` | `CombatText.cs:38` | `CombatTextEntry.Scale` | 客户端瞬态表现状态 | NewText/动画 System 写入 | confirmed（更新规则 partial） |
| `CombatText.rotation` | `CombatText.cs:40` | `CombatTextEntry.Rotation` | 客户端瞬态表现状态 | 动画/绘制 System 写入 | confirmed（更新规则 partial） |
| `CombatText.color` | `CombatText.cs:42` | `CombatTextEntry.Color` | 客户端瞬态表现状态 | NewText/Palette Query 写入 | confirmed |
| `CombatText.active` | `CombatText.cs:44` | `CombatTextEntry.IsActive` | 槽位占用状态 | Pool/clear/expiry System 写入 | confirmed |
| `CombatText.lifeTime` | `CombatText.cs:46` | `CombatTextEntry.RemainingTicks` | 客户端瞬态计时 | Pool/animation System 写入 | confirmed（默认时长 partial） |
| `CombatText.crit` | `CombatText.cs:48` | `CombatTextEntry.IsCritical` | 表现语义标志 | NewText 写入 | confirmed |
| `CombatText.dot` | `CombatText.cs:50` | `CombatTextEntry.IsDamageOverTime` | 表现语义标志 | NewText 写入 | confirmed |

颜色常量是表现定义，不是伤害类别的权威状态；`crit`/`dot` 只描述已经产生的表现事件。`CachedProjectileCounterBuffTextHandler` 与 CombatText 槽位不共享状态 owner，必须保留为 Buff 文本 Projection 的输入缓存。

### 9.2 Proposed 组件、系统和接口

以下是本检查点的 proposed 边界；对应的局部 implementation slice 已记录在本文 `implementationEvidence`，完整 renderer/network 接线仍未完成：

- `src2/Presentation/CombatText/CombatTextPalette.cs`：保存上述十种颜色定义及派生颜色；颜色只读，不从 UI 反写 Combat 事实。
- `src2/Presentation/CombatText/CombatTextEntryComponent.cs`：附着客户端 presentation entity 或池槽，保存位置、速度、透明度、方向、文本、缩放、旋转、颜色、激活、剩余寿命、暴击和 DoT 标志；不保存 Damage/Health/Entity 的权威字段。
- `src2/Presentation/CombatText/CombatTextPoolPolicy.cs`：proposed 100 槽容量/淘汰策略；不能把槽位索引当作实体 ID。
- `src2/Presentation/CombatText/CombatTextSpawnCommand.cs`：从已提交的 damage/heal/regen result 生成一次性文本请求；携带位置快照、颜色语义、文本/数值、crit/dot 和来源事件 ID 候选。
- `src2/Presentation/CombatText/CombatTextPresentationSystem.cs`：唯一写表现条目、推进 alpha/position/lifetime 和清理 inactive 槽；只消费命令/事件，不写模拟实体。
- `src2/Presentation/CombatText/CombatTextPaletteQuery.cs`：纯 Query，根据友方/敌方、暴击、治疗/生命回复和本地/远端来源选择颜色。
- `src2/Presentation/BuffText/ProjectileBuffTextFilter.cs`：承载 `projectilesToLookFor` 的不可变筛选输入；`BuffTextProjection` 通过 Port 查询活动投射物计数，不能把计数缓存升级为战斗权威。
- `src2/Presentation/CombatText/CombatTextNetworkProjection.cs`：proposed 客户端投影/网络 Adapter，消费版本化表现事件；不允许客户端文本包修改伤害结果。

表现数据流：

```text
damage/heal/regen committed fact
  -> proposed CombatTextSpawnCommand
  -> proposed CombatTextPaletteQuery
  -> proposed CombatTextPresentationSystem
  -> CombatTextEntryComponent / client renderer
```

Version4 中 Player/NPC 的 HealEffect、NPC 受伤/生命回复和网络伤害分支调用 `CombatText.NewText`；`Main` 初始化 100 个槽位，`clearAll` 只把槽位 `active` 设为 false。该方向支持“模拟事实 -> 表现 Projection”，不支持反向以文本判断是否造成伤害。

### 9.3 所有权、生命周期、网络和持久化

`CombatTextPresentationSystem` 是 `CombatTextEntryComponent` 的唯一写者；`CombatTextPalette` 是只读定义；`CombatTextSpawnCommand` 只能由 Damage/Health/Status/Network Projection adapter 生成，不能由渲染代码伪造权威结果。池满时的替换、合并、丢弃和 100 槽选择规则在 Version4 `NewText` 中被裁剪，必须在 verifier 中固定。

条目从 spawn 到动画过期只在客户端或表现会话存在，不进入世界持久化。网络同步应优先传递已提交的表现事件或由客户端根据权威 Damage/Heal 事实生成；远端来源颜色（`OthersDamagedHostile*`）不能说明客户端拥有伤害 owner。位置是一次性空间快照，不是实体关系；来源 EntityId、NetworkId、PersistentId 和 ExternalId 只作为可选事件 metadata，并分别由各 owner 提供。槽位索引不是任何 ID。

`clearAll` 语义是表现池清理，不取消 Damage、Heal、Regen 或状态效果；世界生成/重载调用时需要由 scheduler 显式清理 presentation scope。颜色、文本、本地化、字体、绘制和网络属于表现/Adapter 副作用；模拟核心不读取 CombatText 条目。

### 9.4 第五组 focused verifier（已执行，局部证据）

`COMBAT-TEXT-PALETTE`、`COMBAT-TEXT-POOL`、`COMBAT-TEXT-SPAWN-PROJECTION`、`COMBAT-TEXT-CLEAR-BOUNDARY`、`BUFF-TEXT-PROJECTILE-FILTER`、`COMBAT-TEXT-NETWORK-ID` 已由局部 verifier 运行通过（6 个新增场景）；renderer integration、网络去重/重放、持久化和跨分区 owner 仍未闭合。

## 10. 检查点六：SharedItemCombatAndDamageCapabilityState

### 10.1 成员逐条归属（18 条）

| 来源成员 | Version4 证据 | proposed 角色 | 状态类型 | 生命周期/写者 | evidenceStatus |
|---|---|---|---|---|---|
| `Item.damage` | `Terraria\\Item.cs:156`；Player `GetWeaponDamage`、Projectile spawn 读取 | `ItemCombatCapabilityDefinition.BaseDamage` | 内容/实例战斗能力 | Item definition/prefix adapter 写入；combat command builder 读取 | confirmed（prefix 合并顺序 partial） |
| `Item.knockBack` | `Item.cs:158`；Player `GetWeaponKnockback`、Projectile spawn 读取 | `ItemCombatCapabilityDefinition.BaseKnockback` | 内容/实例战斗能力 | 同上 | confirmed |
| `Item.healLife` | `Item.cs:160`；Player item use / heal path 读取 | `ItemResourceRecoveryDefinition.LifeRecovery` | 消耗/恢复能力定义 | content/item use adapter 写入；resource command builder 读取 | confirmed（使用条件 partial） |
| `Item.healMana` | `Item.cs:162`；Player inventory/use path 读取 | `ItemResourceRecoveryDefinition.ManaRecovery` | 消耗/恢复能力定义 | 同上 | confirmed |
| `Item.rare` | `Item.cs:224`；`OriginalRarity` 派生读取 | `ItemPresentationDefinition.RarityTier` | 内容/表现元数据 | catalog/prefix adapter 写入；UI/loot presentation 读取 | confirmed（颜色/价值映射 partial） |
| `Item.shoot` | `Item.cs:226`；Player `ItemCheck`/Projectile spawn 读取 | `ItemProjectileUseDefinition.ProjectileTypeId` | 投射物使用定义 | item catalog/prefix adapter 写入；use command builder 读取 | confirmed |
| `Item.shootSpeed` | `Item.cs:228`；Projectile spawn 读取 | `ItemProjectileUseDefinition.ProjectileSpeed` | 投射物使用定义 | 同上 | confirmed |
| `Item.lifeRegen` | `Item.cs:236`；Player equipment/stat aggregation 读取 | `ItemResourceRecoveryDefinition.LifeRegen` | 资源/统计能力定义 | loadout/stat System 聚合读取 | confirmed（装备/消耗语义需分离） |
| `Item.manaIncrease` | `Item.cs:238`；Player stat aggregation 读取 | `ItemResourceRecoveryDefinition.MaxManaIncrease` | 资源/统计能力定义 | 同上 | confirmed |
| `Item.mana` | `Item.cs:242`；Player `ItemCheck`/mana payment 读取 | `ItemResourceRecoveryDefinition.ManaCost` | 使用资源成本 | item use command builder 读取 | confirmed |
| `Item.crit` | `Item.cs:280`；Player crit aggregation/weapon use 读取 | `ItemCombatCapabilityDefinition.CriticalChance` | 战斗能力定义 | item catalog/prefix adapter 写入；combat stat/query 读取 | confirmed |
| `Item.armorPenetration` | `Item.cs:282`；Projectile damage setup 读取 | `ItemCombatCapabilityDefinition.ArmorPenetration` | 战斗能力定义 | 同上 | confirmed |
| `Item.bonusTagDamage` | `Item.cs:284`；Projectile TagEffect setup 读取 | `ItemCombatCapabilityDefinition.BonusTagDamage` | 战斗/TagEffect 交接能力 | 同上；TagEffect 读取而不拥有 Item 字段 | confirmed |
| `Item.melee` | `Item.cs:288`；Player multiplier/ItemCheck 读取 | `ItemDamageClassCapability.IsMelee` | 分类能力定义 | content/catalog 写入；stat/weapon query 读取 | confirmed |
| `Item.magic` | `Item.cs:290`；Player multiplier/ItemCheck 读取 | `ItemDamageClassCapability.IsMagic` | 分类能力定义 | 同上 | confirmed |
| `Item.ranged` | `Item.cs:292`；Player multiplier/ItemCheck 读取 | `ItemDamageClassCapability.IsRanged` | 分类能力定义 | 同上 | confirmed |
| `Item.summon` | `Item.cs:294`；Player multiplier/ItemCheck 读取 | `ItemDamageClassCapability.IsSummon` | 分类能力定义 | 同上 | confirmed |
| `Item.sentry` | `Item.cs:296`；Player/Projectile sentry branches 读取 | `ItemDamageClassCapability.IsSentry` | 分类能力定义 | 同上 | confirmed |

当前 NLTX 的 proposed `ItemCombatDefinition` 已覆盖 Damage、KnockBack、CritChance、ArmorPenetration、BonusTagDamage、ShootTypeId、ShootSpeed 和职业标志，但不覆盖本组的治疗/法力/生命回复/最大法力/稀有度；这里提出扩展方向，不声明现有类型已完成这些成员的迁移。

### 10.2 Proposed 组件、系统和接口

以下仍是 `proposed` 边界；对应的局部 `src2` implementation slice 已保存并由本组 focused verifier 覆盖，不能据此宣称全链路迁移完成：

- `src2/Content/Items/ItemCombatCapabilityDefinition.cs`：保存 damage、knockback、crit、armor penetration、bonus tag damage 等战斗能力定义或经 prefix 合并后的实例快照；不保存实体生命或伤害结果。
- `src2/Content/Items/ItemProjectileUseDefinition.cs`：保存 `ProjectileTypeId` 和 `ProjectileSpeed`，由 Item use command builder 消费；Projectile 实体状态由 Projectile owner 负责。
- `src2/Content/Items/ItemResourceRecoveryDefinition.cs`：保存 heal life/mana、life regen、max mana increase 和 mana cost 的资源能力定义；实际资源变更通过显式 command 交给 Player resource owner。
- `src2/Content/Items/ItemDamageClassCapability.cs`：保存 melee/magic/ranged/summon/sentry 的分类能力；互斥性、组合合法性和 sentry/minion 语义需 verifier 固化。
- `src2/Content/Items/ItemPresentationDefinition.cs`：保存 rare 等表现/内容元数据；不得被 DamageResolutionSystem 读取为战斗规则。
- `src2/Combat/Items/ItemCombatCapabilityQuery.cs`：纯 Query，把内容定义和实例 prefix/variant 快照转换为 DamageRequest/ProjectileUsePlan 候选。
- `src2/Combat/Items/ItemResourceUseQuery.cs`：纯 Query，判断法力/生命使用资格和成本结果，不修改 Player resources。
- `src2/Combat/Items/Commands/UseItemCombatCommand.cs`：proposed Command，表达 Item use 的提交意图、能力快照和一次性 use identity；实际资源扣除/Projectile spawn 的 owner 由 integration-review 组合。
- `src2/Combat/Items/ItemCombatCommandBuilder.cs`：proposed Adapter/Builder，把 Item/Player/LockOn/TagEffect 输入转成稳定的 command payload；不把旧 Item 对象跨网络或持久化边界传播。
- `src2/Combat/Items/ItemCapabilityProjection.cs`：proposed projection，把 rare/能力分类供 UI/tooltip/客户端 use preview 读取；不反向写 Item 或 combat state。
- `src2/Content/Items/ItemCapabilitySnapshot.cs`：组合一次 Item use 所需的不可变能力定义；不作为 Player/NPC 长期可变组件。
- `src2/Content/Items/ItemCapabilityModifier.cs`：显式、有序的 prefix/variant modifier 输入；重复顺序值被拒绝，合并结果由纯 Query 确定。
- `src2/Content/Items/*Id.cs`：分别表达 content、projectile content、inventory slot、use、persistent、network 和 external identity；这些值对象不互相替代。

### 10.3 所有权、依赖方向、ID 和生命周期

建议数据流：

```text
Item content catalog + prefix/variant snapshot
  -> proposed ItemCombatCapabilityQuery / ItemResourceUseQuery
  -> proposed UseItemCombatCommand / ProjectileUsePlan
  -> integration-review resource + projectile + damage commit
  -> DamageRequest / CombatText event handoff
```

`ItemCombatCapabilityDefinition`、`ItemProjectileUseDefinition`、`ItemResourceRecoveryDefinition`、`ItemDamageClassCapability` 和 `ItemPresentationDefinition` 都是 definition/immutable snapshot 候选，不应附着到 Player/NPC 作为长期可变组件；使用中的临时快照只在一次 ItemCheck/use transaction 内有效。`ItemCombatCapabilityQuery` 和 `ItemResourceUseQuery` 不写 Player、Projectile 或 Item inventory。

`Item.type`/`ProjectileTypeId`/`AmmoTypeId` 等内容 ID 是 catalog/content ID，不是 EntityId、PersistentId、NetworkId 或外部 ID；本组没有 `Item.type` 成员但 `shoot` 的解释依赖它。Item inventory slot/index 是容器位置，不是 Item instance identity。Item instance 的稳定实体/持久化身份和网络复制由 inventory/content owner 提供。Item `rare` 是内容/表现元数据，不得参与伤害结算。

定义加载/注册 -> prefix/variant 合并 -> capability/resource Query -> lock-on/tag-effect/Item use adapter -> resource/projectile command -> damage resolution -> CombatText projection 是候选 System 顺序。`resource commit`、`projectile spawn`、`damage commit` 的事务和失败重试顺序由 `crossSubsystemOwner: integration-review` 裁决。

网络只发送版本化 Item capability/use payload 或由服务端重新解析 content ID；不发送旧 Item 引用、数组槽位、UI rarity text 或客户端预测伤害作为权威。一般不持久化一次性 use snapshot；内容定义由 catalog/version 重建，inventory owner 另行持久化实际物品及 prefix。

### 10.4 第六组 focused verifier（已执行，局部证据）

`ITEM-CAPABILITY-COVERAGE`、`ITEM-COMBAT-QUERY`、`ITEM-RESOURCE-QUERY`、`ITEM-PROJECTILE-USE`、`ITEM-PREFIX-VARIANT-MERGE`、`ITEM-PRESENTATION-BOUNDARY` 和 `ITEM-ID-BOUNDARY` 已由局部 verifier 运行通过（7 个新增场景；累计 29 个场景）。build 退出码 0，warning/error 均为 0；随后以 `--no-build --no-restore` 运行退出码 0，输出 `P07 focused verifier passed: 29 scenarios`。资源扣除、Projectile spawn、ammo、Damage commit、网络 Deserialize/replay、持久化和跨分区 owner 仍未闭合。

## 11. 全分区依赖与调度契约

以下是本分区的 proposed 依赖方向，不是最终调度裁决：

```text
content definitions/catalogs
  -> immunity/tag-effect/item capability definitions
  -> local target/lock-on queries and hit tracking
  -> item-use/tag-effect/strike commands
  -> integration-review damage/resource/projectile/death commits
  -> CombatText and crack/lock-on client projections
```

推荐的最小顺序约束是：定义加载和实例快照先于纯资格 Query；纯 Query 先于 Command 构造；Command 先于单一写入 System；权威提交结果先于 CombatText/锁定/Tile crack 等 Projection。HitTile 的本地缓存提交与世界 Tile 权威提交之间、TagEffect 的命中回调与 Damage commit 之间、Item resource commit 与 Projectile spawn 之间均需显式成功/失败/重试策略。任何跨分区顺序只标为 `crossSubsystemOwner: integration-review`。

## 12. ID、网络、持久化与客户端边界汇总

- `EntityId/EntityReference`：ECS 世界内实体生命周期标识；目标快照、Tag marks、锁定候选和 Item command 只提出候选，不在本分区定义分配/代际规则。
- `PersistentId`：世界/玩家/物品存档标识；本分区不把 NPC 槽位、Player `whoAmI`、Item inventory slot 或 CombatText slot 当作持久化 ID。
- `NetworkId`：协议/复制标识；`NPCKillAttempt.netId`、Player `whoAmI`、NPC `whoAmI` 和旧数组索引必须经 Network Adapter 解读，不等同 EntityId 或 PersistentId。
- `ExternalId`：平台/账户/第三方标识；本分区不读取或产生此类 ID。
- 网络边界：TagEffect、锁定输入、Item use 和表现文本只能通过 proposed versioned Adapter/Projection；核心 Component/System 不依赖 `NetPacket`、`BinaryReader`、`Main` 静态数组或旧对象引用。
- 持久化边界：目标快照、HitTile cache、Tag marks、LockOn selection、CombatText entries 和一次性 Item use snapshot 默认不存档；Item content definitions 从 catalog/version 重建，实际库存/prefix 归 inventory owner。
- 客户端边界：LockOn predicted position、CombatText、Tile crack 和 rarity/tooltip 是单向 projection；不回写服务端伤害、死亡、掉落、Tile world 或资源状态。

## 13. 证据缺口、阻塞决策与 focused verifier 总表

本分区仍有以下 evidence-gap：第一轮 P07 输出报告缺失；Version4 裁剪实现未闭合 TagEffect 网络接收/具体回调、LockOn 候选写入/光标写入、CombatText 槽位动画、Buff 文本处理和 Item 资源/Projectile 分支的完整语义；旧槽位与稳定实体/网络身份映射未确认；跨分区 owner、失败重试、去重和最终系统顺序未裁决。

blocking-decision 汇总：`integration-review` 必须裁决 Entity/Network/Persistent/External ID contract；Damage/Health/Death/Loot/Resource/Projectile 的提交事务；TagEffect/LockOn/HitTile/CombatText 的单一写入 owner；网络包权限、版本、重放和快照投影；Item prefix/variant 合并与资源扣除/Projectile spawn 顺序；客户端 projection scope。上述决定未由本分区单方面作出。

focused verifier：已完成 `COMBAT-TARGET-SNAPSHOT`、`NPC-IMMUNITY-APPLICATION`、`NPC-KILL-ATTEMPT-BOUNDARY`（一次运行共 4 个场景并通过）；`TILE-HIT-CAPACITY`、`TILE-HIT-TTL`、`TILE-HIT-RANDOM-SEAM`、`TILE-HIT-WORLD-BOUNDARY`、`TAG-EFFECT-SWITCH-CLEAR`、`TAG-EFFECT-TIMER-LIFECYCLE`、`TAG-EFFECT-HIT-BOUNDARY`、`TAG-EFFECT-NETWORK-PROJECTION`、`LOCK-ON-CANDIDATE-VALIDITY`、`LOCK-ON-HOLD-LIFETIME`、`LOCK-ON-PREDICTION`、`LOCK-ON-PROJECTION-SCOPE`、`LOCK-ON-ID-BOUNDARY`、`COMBAT-TEXT-PALETTE`、`COMBAT-TEXT-POOL`、`COMBAT-TEXT-SPAWN-PROJECTION`、`COMBAT-TEXT-CLEAR-BOUNDARY`、`BUFF-TEXT-PROJECTILE-FILTER`、`COMBAT-TEXT-NETWORK-ID`、`ITEM-CAPABILITY-COVERAGE`、`ITEM-COMBAT-QUERY`、`ITEM-RESOURCE-QUERY`、`ITEM-PROJECTILE-USE`、`ITEM-PREFIX-VARIANT-MERGE`、`ITEM-PRESENTATION-BOUNDARY`、`ITEM-ID-BOUNDARY`，共 29 个场景，均已通过当前局部 verifier。

## 14. Integration Handoff

本分区交付的是 6 个叶子组、97 条 Version4 成员的 proposed 组件边界与后续计划，不是实现或行为等价结论。整合会话需要接收：

- 目标/免疫/击杀快照、HitTile Player-local cache、TagEffect Player state、LockOn local selection、CombatText presentation pool、Item capability/resource definitions 的候选 owner；
- Damage/Health/Death/Loot、Tile world、Player resources、Projectile spawn、NPC/Player/Buff、Network session、Inventory/Persistence 和 client renderer 的跨分区交接点；
- 上述 ID 区分、单一写入 owner、命令提交、projection 方向和候选 System 顺序。

整合前必须补齐第一轮 P07 报告证据缺口、Version4 裁剪方法的真实语义、旧槽位到稳定实体映射、网络权限/版本/重放和失败重试策略。建议以 focused verifier 结果为准逐组接线，并在每个跨域事务有明确 owner 后才删除兼容适配器。

本会话已在 `src2` 创建六组 focused implementation、生产项目和 verifier，并已串行 build/run；没有修改 `src`、Test、Version4 源码、其他分区文档、runner ledger 或 lock。当前 `verificationStatus: partial`，代表 29 个局部 verifier 场景通过，不代表全 P07 行为等价、网络/持久化闭合或跨分区 owner 已裁决。
