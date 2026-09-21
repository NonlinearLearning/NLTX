# Version4 P06 玩家战斗、伤害、防御、状态与资源组件执行计划

partitionId: P06
sessionId: fd85b62742c142878ee572f953f944e2
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P06-Player-Combat-Status.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P06-player-combat-status-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P06-player-combat-status-component-execution.md
designStatus: proposed
evidenceStatus: partial
currentNltxStatus: partial
executionStatus: failed
implementationStatus: partial
verificationStatus: focused-c22-c01-c02-c03-c04-c05-passed-c06-c07-c08-c09-c10-c11-c12-c13-c14-c15-c16-c17-c18-c19-c20-c21-source-not-verified
completedComponents: [C22.PlayerDpsTelemetryCore, C01.PlayerStringAndAccessoryEffectState, C02.PlayerCombatDamageProcState, C03.PlayerCombatDodgeAndImmunityState, C04.PlayerCombatBarrierAndRegenState, C05.PlayerManaAndAfkStatus, C06.PlayerDebuffAndRecoveryStatus-source, C07.PlayerDetectionAndCombatStatus-source, C08.PlayerSocialAndDefenseState-source, C09.PlayerFrameAndImmunityState-authority-source, C10.PlayerVitalAndRegenState-source, C11.PlayerCombatModifierAndImmunityState-source, C12.PlayerAmmoAndAccessoryEffects-source, C13.PlayerElementalAndShimmerStatus-source, C14.PlayerSurvivalAndTransformationState-source, C15.PlayerDebuffStatusState-source, C16.PlayerAccessoryCombatModifierState-source, C17.PlayerAccessoryResourceAndInvulnerabilityState-source, C18.PlayerAccessoryDebuffAndDropState-source, C19.PlayerCombatDamageAndCritModifiers-source, C20.PlayerCombatSpeedRangeAndPermissionState-source, C21.PlayerLuckAndCommerceEffects-source]
currentComponent: C22.IntegrationHandoff
pendingComponents: [C09.PresentationAndTownHandoff, C22.NpcProjectileIntegration, C22.LifecycleIntegration, C22.ProjectionIntegration]
lastCheckpointUtc: 2026-09-12T09:16:41.6470356Z
evidence-gap: C01-C05 和 C22 本地 focused verifier 已通过；C06-C21 组件源码已保存但没有本次任务允许的 focused verifier，且 C09 叶子组仍因 presentation/Town handoff 为 partial。C10 与现有 Health/Mana 的唯一 writer、C02/C03/C04/C05/C06/C19/C22 的跨分区 damage/immunity/vital owner、P04 lifecycle、regen commit root、World spawn/eligibility、network/persistence DTO、runtime registration 和 projection registration 仍未闭合。
blocking-decision: integration-handoff
执行前必须确定唯一写者、提交根、快照版本、旧字段兼容窗口和系统调度契约；未确定时保持 planned/not-started，不做双写迁移。

## 1. 计划边界和实施前不变量

本文件以设计计划为基线；当前已追加 C22 本地 core、C01-C06 以及 C07-C21 受限组件源码的真实实施记录。整体仍是 partial implementation：`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: focused-c22-c01-c02-c03-c04-c05-passed-c06-c07-c08-c09-c10-c11-c12-c13-c14-c15-c16-c17-c18-c19-c20-c21-source-not-verified`。未实现或未验证的部分继续保持 planned/not-verified 口径。

- 先建立 focused verifier，再迁移一个 authority writer，再迁移 readers 和 projections。
- 每个字段只能有一个写者；兼容层可以读 owner，但不能双写旧字段和新组件。
- Command 表示意图/结构变化，Query 只做确定性资格和计算，System 执行状态提交，Adapter 处理外部 I/O，Projection 单向输出。
- Item、Buff、Projectile、Network、Persistence、DateTime 和 World 类型通过稳定 port/DTO 边界转换。
- 任何迁移失败都恢复旧读路径并撤销本批新 writer，不以双写掩盖差异。
- 本文不运行用户禁止的 git diff --check -- <两份文档>。

## 2. 建议文件组织和命名空间

当前源码实际位于 dome/src/Terraria.Dome.Simulation。实现阶段建议沿用领域优先：

| 目标边界 | proposed 目录 | proposed 命名空间 | 组织依据 |
|---|---|---|---|
| Player combat components | dome/src/Terraria.Dome.Simulation/Player/Combat/ | Terraria.Dome.Simulation.Player.Combat | P06 已形成稳定能力边界 |
| Player combat systems | dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/ | Terraria.Dome.Simulation.Player.Combat.Systems | 系统调度和验证独立 |
| Player combat queries | dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/ | Terraria.Dome.Simulation.Player.Combat.Queries | 纯资格/数值查询 |
| Player combat commands/events | dome/src/Terraria.Dome.Simulation/Player/Combat/Commands/ and Events/ | Terraria.Dome.Simulation.Player.Combat.Commands/Events | 意图和已提交事实分离 |
| Shared status effects | dome/src/Terraria.Dome.Simulation/StatusEffects/ | Terraria.Dome.Simulation.StatusEffects | Buff 定义/效果不复制到 Player |
| Network adapters | dome/src/Terraria.Dome.Protocol.V1456/ and dome/src/Terraria.Dome.Server/Replication/ | existing protocol/server namespaces | 协议不渗透 simulation authority |
| Persistence adapters | documented WorldStorage/server boundary | existing storage namespace | 文件/流 I/O 不进入 Component |

不预建空目录。一个核心公开类型一个同名 PascalCase 文件；目录移动不自动改变命名空间。路径变化必须记录 source/target/dependency impact 和回滚点。

## 3. 全局执行顺序

hydrate Player identity/base state
→ convert input/network into commands
→ reset derived effects
→ commit equipment/content capability facts
→ commit environment/status facts
→ freeze combat snapshot
→ eligibility query
→ damage calculation query
→ vitality/immunity/proc structural commit
→ death/lifecycle transition
→ regen and timeout tick
→ telemetry observation
→ network/persistence/presentation projection

这是计划中的顺序，不是已确认的 Version4 顺序。实际迁移前必须由 focused verifier 回放 Player.Update、ResetEffects、Hurt、UpdateLifeRegen 和 UpdateManaRegen 的关键调用关系。

## 4. 检查点执行约定

每个 C01-C22 检查点都要完成：成员清单核对、Version4 source anchor 核对、proposed owner/seam、默认值和 reset 规则、跨分区依赖、focused verifier、回滚条件、设计/执行文档进度字段更新。检查点完成前不得开始下一个。

当前实现检查点：C01-C05 与 `C22.PlayerDpsTelemetryCore` 保持既有 focused verifier 证据；C06-C21 的组件源码已保存但未执行本次任务禁止新增的 focused verifier，C09 仍为 partial。C09 presentation/Town handoff、C22 外部 integration、运行时注册、网络和持久化仍 pending；没有接入未裁决的 NPC/Projectile、网络、持久化或运行时注册。

## 6. C01 执行计划：PlayerStringAndAccessoryEffectState

源边界：D:\TRbackup\Version4\Terraria\Player.cs:537-563、:6349-6402、:6928-6932、:8035、:8358-8413、:10277-10437、:14955-14963。

原设计计划目标文件（本次未采用，因当前实现任务限定代码输出在根 `src` 且 dome 跨项目 owner 未闭合）：

- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerAccessoryStringEffectComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerAccessoryEffectRebuildSystem.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerAccessoryCapabilityQuery.cs

实施顺序：

1. 建立字段清单和 ResetEffects focused verifier，确认默认值、private/public 边界、transient 清理和 Tick 衰减。
2. 定义 typed ItemReference/EquipmentCapabilityQuery；禁止把 brain/star/yoyo Item 对象复制进组件。
3. 建立唯一 effect rebuild writer；旧 Player 字段在兼容窗口内只通过 read adapter 暴露。
4. 迁移 Counterweight、stressBall edge、tankPet reset 的读者到只读 Query/事件；不接管 Projectile 或玩家生命周期。
5. 运行 C01 verifier 后再迁移 C02。若发现旧/新双写、装备重算顺序不一致或 Item owner 未定，回滚本批 reader migration，保留设计计划。

依赖影响：P09 装备关系和 Item payload、P11 表现帧、P15 Projectile/tank pet、P05 玩家 progression。跨域 owner 未批准前不得实现。

验收结果：14 个成员均有归属；默认/重置/重复重建、无 Item payload 双份、stressBall edge、counterweight 读取顺序、tankPet 两阶段 reset 和纯 Query 已通过 focused verifier。C01 build 使用 `Build/Tools/Invoke-SerialDotnet.ps1` 构建 `src/PlayerAccessoryVerification/Terraria.PlayerAccessoryVerification.csproj`，退出码 0，0 警告、0 错误；产物为 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 和 `Build/bin/Terraria.PlayerAccessoryVerification/Debug/net10.0/Terraria.PlayerAccessoryVerification.dll`。C01 focused run 使用 `run --project --no-build --no-restore`，退出码 0，输出 `PASS: player accessory string effects rebuild, reset, query and edge semantics`。

## 6.1 C01 实际实现 checkpoint：PlayerStringAndAccessoryEffectState

本次已按 C01 执行顺序创建并保存以下根 `src` 文件：

- `src/Player/PlayerAccessoryStringEffectComponent.cs`
- `src/Player/PlayerAccessoryEffectRebuildInput.cs`
- `src/Player/PlayerAccessoryEffectRebuildSystem.cs`
- `src/Player/PlayerAccessoryCapabilitySnapshot.cs`
- `src/Player/PlayerAccessoryCapabilityQuery.cs`
- `src/PlayerAccessoryVerification/Program.cs`
- `src/PlayerAccessoryVerification/Terraria.PlayerAccessoryVerification.csproj`

唯一 writer 是 `PlayerAccessoryEffectRebuildSystem`。它接收不可变能力输入，保存 14 个 C01 字段，执行 `extraAccessorySlots` 的模式派生、`rapidAttackBonus` 的 `0.005f` tick 衰减、transient accessory reset、`stressBall` current/previous 边沿记忆和 Version4 两阶段 `tankPet` reset。`PlayerAccessoryCapabilityQuery` 只生成快照和 counterweight/yoyo 资格计算，不写 Component；输入和 Component 都不持有 `Terraria.Item` 或 Projectile 对象。

本 checkpoint 已通过 focused build/verifier；P09/P05/P15/P11 的跨域 owner、运行时注册、网络/持久化和玩家生命周期接入仍未实现，未创建外部 adapter 或双写 writer。

## 7. C02 执行计划：PlayerCombatDamageProcState

源边界：D:\TRbackup\Version4\Terraria\Player.cs:642-644、:688-690、:736-816、:4525、:4643、:10373、:10441-10445、:12575-12646、:14876-15029、:15173-15183。

计划目标文件：

- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerCombatProcStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerCombatProcSystem.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerCombatProcQuery.cs

实施顺序：

1. 定义 committed hit event 的字段、source reference、revision 和 duplicate policy。
2. 将 eocDash/eocHit、timers、onHit flags 和 lifeSteal 分为同一检查点内的窄状态，禁止把 Projectile/NPC 状态复制进 Player。
3. 先迁移只读 proc query，再接入唯一 proc writer；被拒绝伤害不得触发 writer。
4. 将 network/presentation 输出改为 committed fact projection，保留旧公共 API adapter。
5. focused verifier 通过后再开始 C03；命中来源或提交根不明确时停在 integration-review。

依赖影响：CombatAndStatus、ProjectileSimulation、NpcAndTownSimulation、ItemContainerAndEconomy、PlayerGameplay。回滚时恢复旧 proc 读路径并移除新 writer 注册。

验收门槛：重复命令不重复扣血/触发、计时器确定性递减、onHit flags 与 committed event 对齐、Item/Projectile 类型不渗透核心组件。

### 7.1 C02 实际实现 checkpoint：PlayerCombatDamageProcState

已实际保存到 `D:\TRbackup\NLTX\src`：

- `Player/PlayerCombatProcStateComponent.cs`：15 个 C02 字段和 Version4 默认/sentinel。
- `Player/PlayerCommittedCombatHitEvent.cs`、`Player/PlayerCombatProcHitEffects.cs`：带 EventId、SourceId、revision 和 committed 阶段的稳定值类型输入。
- `Player/PlayerCombatProcSystem.cs`：唯一 writer；拒绝 attempted/零伤害/空 ID/空 source/负 revision，按 EventId 至多一次消费，执行 life steal、ghost damage、on-hit flags、EOC、inferno 和 timer 转换；`ResetEffects` 清 transient flags，`Reset` 清 replay history 并开启新生命周期。
- `Player/PlayerCombatProcQuery.cs`、`Player/PlayerCombatProcSnapshot.cs`：纯快照读取，不反向写 component。
- `PlayerCombatProcVerification/Program.cs`：focused verifier，覆盖默认值、committed-only、重复事件、tick/cap、EOC、timer wrap、ResetEffects、Query purity 和跨生命周期 replay 语义。

核心行为：

- committed positive-damage hit 才能更新 proc；相同 EventId 的重复投递不会重复扣减 `LifeSteal` 或累加 `GhostDmg`。
- `ghostDmg` 每 tick 按 `6.6666665f` 衰减并钳制为零；life steal 在 expert/普通模式分别按 `0.5f/0.6f` 回补并钳制至 `70f/80f`。
- `EocDash` 递减至零时清除 `EocHit`；`InfernoCounter` 在 180 回绕；star cloak、titanium storm、petal 和 bone glove timer 非负递减。
- C02 不复制 Item、Projectile、NPC 或 Buff runtime object，不注册外部事件，不创建 network/persistence DTO，不执行表现随机/音效副作用。

实际验证：

- 红阶段 build 通过 wrapper 退出码 1，产生 6 个预期 CS0246（生产类型尚未创建）。
- Build 命令：`Build/Tools/Invoke-SerialDotnet.ps1` + `build .\src\PlayerCombatProcVerification\Terraria.PlayerCombatProcVerification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；退出码 0，0 警告，0 错误。
- Focused run 命令：同一 wrapper + `run --project .\src\PlayerCombatProcVerification\Terraria.PlayerCombatProcVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；退出码 0，输出 `PASS: player combat proc committed-hit, timer and query semantics`。
- 产物：`D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll`、`D:\TRbackup\NLTX\Build\bin\Terraria.PlayerCombatProcVerification\Debug\net10.0\Terraria.PlayerCombatProcVerification.dll`。

未验证项与依赖：P12/P15 committed damage caller、EOC target EntityReference、P04 lifecycle reset caller、NetworkSession/Persistence/ClientPresentation projections 和 runtime registration 仍未实现；C03 是下一个组件。

## 8. C03 执行计划：PlayerCombatDodgeAndImmunityState

源边界：D:\TRbackup\Version4\Terraria\Player.cs:712、:785-793、:812、:4513、:8694-8721、:9961、:10438、:10593、:10633、:10758-10760、:15177-15179、:15349、:22096-22113。

计划目标文件：

- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerDodgeAndImmunityStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerDamageEligibilityQuery.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerDodgeCommitSystem.cs

实施顺序：

1. 先为 DamageRequest 定义 cooldown key、source reference、command id 和 stale revision 规则。
2. 将 brainOfConfusionItem 转为 typed ItemReference，验证空引用和 Item slot reuse 不混淆。
3. 建立纯 PlayerDamageEligibilityQuery，再由唯一 commit system 消费 dodge。
4. 将 animation counter 输出到 projection，禁止 projection 写回免疫/闪避 authority。
5. 通过重复 command、窗口、死亡/重生清理 verifier 后进入 C04。

依赖影响：CombatAndStatus 的免疫 owner、P09 Item relation、P15 Projectile source、P04 Player lifecycle。任何统一 cooldown key 未裁决时保持 planned。

验收门槛：拒绝命中无状态写入，已消费闪避只提交一次，表现计时不影响资格，旧 API 仍由 adapter 提供。

### 8.1 C03 实际实现 checkpoint：PlayerCombatDodgeAndImmunityState

已实际保存到 `D:\TRbackup\NLTX\src`：

- `Player/PlayerDodgeAndImmunityStateComponent.cs`：5 个字段，Item 字段降为 `ItemEntityRef`。
- `Player/PlayerDodgeCapabilityInput.cs`、`PlayerDodgeActivationCommand.cs`、`PlayerDodgeCommitCommand.cs`、`PlayerDamageEligibilityInput.cs`、`PlayerDamageEligibilityRejectionReason.cs`：显式能力/命令/资格输入。
- `Player/PlayerDodgeCommitSystem.cs`：唯一 writer；activation 和 committed damage 均按 command id 去重，拒绝命中不清除 `ShadowDodge`，tick 到期清理 authority，ResetEffects/lifecycle 清理关系和 replay sets。
- `Player/PlayerDamageEligibilityQuery.cs`、`PlayerDamageEligibilityResult.cs`：纯 eligibility calculation。
- `Player/PlayerDodgePresentationProjection.cs`、`PlayerDodgePresentationSnapshot.cs`：纯单向表现输出。
- `PlayerDodgeVerification/Program.cs`：focused verifier。

实际验证：

- 红阶段 wrapper build 退出码 1，12 个预期缺失类型/成员错误。
- Build 命令：`Invoke-SerialDotnet.ps1` + `build .\src\PlayerDodgeVerification\Terraria.PlayerDodgeVerification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；退出码 0，0 警告，0 错误。
- Focused run 命令：同一 wrapper + `run --project .\src\PlayerDodgeVerification\Terraria.PlayerDodgeVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；退出码 0，输出 `PASS: player dodge eligibility, committed consumption and projection semantics`。
- 产物：`D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll`、`D:\TRbackup\NLTX\Build\bin\Terraria.PlayerDodgeVerification\Debug\net10.0\Terraria.PlayerDodgeVerification.dll`。

未验证项与依赖：统一 Combat `ImmunityComponent` owner、P12/P15 damage caller、P04 lifecycle、旧 API adapter 以及 Network/Persistence/Presentation registration 仍未实现；C04 是下一个组件。

## 9. C04 执行计划：PlayerCombatBarrierAndRegenState

源边界：D:\TRbackup\Version4\Terraria\Player.cs:761、:789-796、:4521、:4855-4864、:10439、:10444、:11156、:22657-22658。

计划目标文件：

- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerBarrierAndRegenComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerBarrierSystem.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerBarrierFrameProjection.cs

实施顺序：

1. 先验证生命阈值和 ResetEffects/UpdateDead 的屏障清理。
2. 将 frame counter 和 authority barrier 分成 component/projection 两条路径。
3. 将 palladiumRegen 转为 regen capability input，禁止 barrier system 直接写 health。
4. 通过受伤、死亡、重生和重复 Tick verifier 后再开始 C05。

依赖影响：PlayerVital/Combat regen owner、ClientPresentation frame projection、P09 equipment capability。若生命写入或 frame order 发生双写，回滚 C04 reader migration。

验收门槛：四个成员均有归属，屏障阈值和 frame 循环有证据，regen 与 health writer 分离，当前不运行编译/测试。

### 9.1 C04 实际实现 checkpoint：PlayerCombatBarrierAndRegenState

已实际保存到 `D:\TRbackup\NLTX\src`：

- `Player/PlayerBarrierAndRegenComponent.cs`：四个 C04 字段，表现 frame 与 authority capability 共存于同一窄状态组件，但只有 system 写入。
- `Player/PlayerBarrierCapabilityInput.cs`：显式生命快照、Buff capability 和 palladium capability 输入。
- `Player/PlayerBarrierSystem.cs`：唯一 writer；半血边界采用整数安全比较，active barrier 每 tick 按 `counter > 2` 推进 frame，并在 12 帧后回绕；ResetEffects 只清 authority capability，lifecycle reset 额外清 frame 状态。
- `Player/PlayerBarrierFrameProjection.cs`、`PlayerBarrierFrameSnapshot.cs`：纯表现投影。
- `PlayerBarrierVerification/Program.cs`：focused verifier。

实际验证：

- 红阶段 wrapper build 退出码 1，10 个预期缺失类型错误。
- Build 命令：`Invoke-SerialDotnet.ps1` + `build .\src\PlayerBarrierVerification\Terraria.PlayerBarrierVerification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；退出码 0，0 警告，0 错误。
- Focused run 命令：同一 wrapper + `run --project .\src\PlayerBarrierVerification\Terraria.PlayerBarrierVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；退出码 0，输出 `PASS: player barrier threshold, regen capability and frame projection semantics`。
- 产物：`D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll`、`D:\TRbackup\NLTX\Build\bin\Terraria.PlayerBarrierVerification\Debug\net10.0\Terraria.PlayerBarrierVerification.dll`。

未验证项与依赖：P09 Buff/equipment source、C10 vital/regen writer、P04 lifecycle、Network/Persistence/Presentation registration 仍未实现；C05 是下一个组件。

## 10. C05 执行计划：PlayerManaAndAfkStatus

源边界：D:\TRbackup\Version4\Terraria\Player.cs:622-636、:674-676、:4374-4375、:4727-4728、:8570-8571、:10571、:10627-10628、:11264-11281、:15215-15229、:15532-15534；D:\TRbackup\Version4\Terraria\Main.cs:406-407。

计划目标文件：

- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerManaActivityComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Definitions/ManaActivityPolicyDefinition.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/ManaStatusSystem.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerActivityQuery.cs

实施顺序：

1. 把 static/readonly 常量收敛到 immutable Definition，并以测试固定原始默认值。
2. 建立显式 Player activity input（移动/攻击/控制边沿）和纯 AFK Query。
3. 迁移 mana sickness 与 regen bonus 的唯一 writer，分别对接 Buff/Equipment 和 C10 mana owner。
4. 验证暂停、死亡、重生、网络重放和重复 Buff command 的 reset/幂等行为。
5. 只有在 World spawn 和 Combat modifier 交接批准后才迁移 reader。

依赖影响：StatusEffects、P09 equipment/Item、World spawn/eligibility、C10 vitality/regen、C19 damage modifiers。回滚时保留旧 policy reader，禁止新旧 counters 双写。

验收门槛：10 个成员均有归属，static policy 与 mutable state 分离，AFK 查询纯净，mana sickness 不直接改生命/魔力，当前不运行编译/测试。

### 10.1 C05 实际实现 checkpoint：PlayerManaAndAfkStatus

已实际确认并记录到 `D:\TRbackup\NLTX\src\Player`：

- `PlayerManaActivityComponent.cs`：`manaSick`、`manaSickReduction`、`afkCounter`、`afkCounterForKiting`；`ResetEffects` 清除派生效果，`ResetForLifecycle` 清除生命周期计数。
- `ManaActivityPolicyDefinition.cs`：不可变 policy 定义，默认 `manaSickTime = 300`、`manaSickLessDmg = 0.25f`、`AFKTimeNeededForNoWormSpawns = 300`、`AFKTimeNeededForNoLuckyStars = 10800`，并限制 sickness reduction 计算范围。
- `PlayerManaRegenModifierComponent.cs`：`manaRegenBonus` 和 `manaRegenDelayBonus`，`Reset` 恢复为 Version4 的零值。

依赖影响：StatusEffects 提供 Buff 时间事实，C10 持有 mana/vital commit root，C19 固定 mana sickness 对伤害修正的顺序，World/Spawn 提供 AFK 资格环境输入，P09 提供装备能力输入。上述 System、Query、Adapter、Projection 和跨分区 writer 本 checkpoint 未创建或修改；没有在组件中补写缺失行为。

实际验证：

- Build：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerManaActivityVerification\Terraria.PlayerManaActivityVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`；退出码 `0`，警告 `0`，错误 `0`。
- Focused run：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 run --project .\src\PlayerManaActivityVerification\Terraria.PlayerManaActivityVerification.csproj --no-build --no-restore /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`；退出码 `0`，输出 `PASS: player mana sickness, regeneration modifiers, AFK activity and lifecycle semantics`。
- 产物：`D:\TRbackup\NLTX\Build\bin\Terraria.PlayerManaActivityVerification\Debug\net10.0\Terraria.PlayerManaActivityVerification.dll`。

未验证项：C05 的跨分区 writer/reader 唯一性、World spawn 交接、C10/C19 真实提交链、network/persistence DTO 和 projection registration 仍未闭合。C06 是下一个组件。

## 11. C06 执行计划：PlayerDebuffAndRecoveryStatus

源边界：D:\TRbackup\Version4\Terraria\Player.cs:656-670、:734、:741-749、:4484-4500、:5958、:6032、:6087、:10320、:10620-10626、:10687-10689、:11016、:11136-11184、:11361-11497、:13327、:14378-14399、:15565-15573、:17985-17992。

计划目标文件：

- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerDebuffRecoveryStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerSurfaceMovementStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerDebuffRecoverySystem.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerRecoveryQuery.cs

实施顺序：

1. 建立 Buff-derived status alias 的单一 writer 和 reset contract。
2. 将 environment/surface input 转为 immutable snapshot，不从 component 直接读取 Tile/World。
3. 把 recovery capability 与 vitality commit 分离，验证 rejected/expired status 不改生命。
4. 单独验证 miscCounter 的 wrap/消费者，不能借用 DateTime 或 animation counter。
5. 通过 death/reset/paused tick verifier 后再迁移 C07。

依赖影响：StatusEffects、Movement/Spatial、Liquid/World、PlayerVital/C10、ClientPresentation。未裁决 alias owner 时保持旧 reader。

验收门槛：16 个成员均有归属，Buff 与 alias 不双写，surface status 不拥有速度/位置，recovery 不越过 vital commit root。

### 11.1 C06 实际源码 checkpoint：PlayerDebuffAndRecoveryStatus

已实际保存到 `D:\TRbackup\NLTX\src\Player`：

- `PlayerDebuffRecoveryStateComponent.cs`：11 个减益/恢复字段，包含 chilled、dazed、frozen、stoned、ichor、webbed、tipsy、noBuilding、crimsonRegen、ghostHeal 和 ghostHurt；提供 `ResetEffects` 与 `ResetForLifecycle`。
- `PlayerSurfaceMovementStateComponent.cs`：4 个表面/环境字段 sandStorm、sticky、slippy、slippy2；不拥有位置、速度或 Movement commit。
- `PlayerMiscCounterComponent.cs`：miscCounter 的最小独立状态和生命周期清零，不声明未经证实的 wrap/consumer 逻辑。

依赖影响：StatusEffects 的 Buff snapshot/alias owner、Movement/Spatial/Liquid/World 的表面输入、C10 的 vital commit root 和 ClientPresentation 的单向读取仍未闭合。按范围约束没有创建或修改 System、Query、Adapter、Projection、测试、验证程序或其他非组件代码。

验证状态：源码文件已确认存在，16 个 Version4 字段均有组件归属；本 checkpoint 没有合法的 C06 focused verifier，标记为 `not-verified`。未验证 Buff alias rebuild/clear、miscCounter wrap/消费者、recovery ordering、death/reset scheduler 和跨分区唯一 writer。

C07 是下一个待实现组件。

## 12. C07 执行计划：PlayerDetectionAndCombatStatus

源边界：D:\TRbackup\Version4\Terraria\Player.cs:763-783、:3461、:4616、:4629、:4696-4722、:4856、:7782、:7807、:8542、:8713-8717、:9611、:9986-9990、:10482-10485、:10592、:10696-10705、:17904、:22263。

计划目标文件：

- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerCombatDetectionStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerDamageMitigationInputComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerWhipCapabilityComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerCombatDetectionSystem.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerCombatModifierQuery.cs

实施顺序：

1. 先定义 luckPotion、endurance、whip multiplier 的 typed snapshots 和范围。
2. 迁移纯 query，再迁移 Buff/equipment/environment rebuild writer。
3. 把 endurance 接入 damage calculation snapshot，不让 detection system 直接扣生命。
4. 验证 ResetEffects 默认值、luck calculation、whip timing 和 environment flag clearing。
5. 在 Luck、Combat、Projectile、World owner 达成 integration decision 后迁移 readers。

依赖影响：C10/C19 damage/vital、PlayerLuckStateComponent、Projectile use、StatusEffects、World/Liquid。回滚时保留旧 detection reads，删除新 writer registration。

验收门槛：11 个成员有归属，Query 无写回，endurance 顺序有证据，whip capability 与 Projectile 生命周期分离。

### 12.1 C07 实际源码 checkpoint：PlayerDetectionAndCombatStatus

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerCombatDetectionStateComponent.cs`、`src/Player/PlayerDamageMitigationInputComponent.cs`、`src/Player/PlayerWhipCapabilityComponent.cs`、`src/Player/PlayerLuckPotionStateComponent.cs`。
- 11 个字段已按四个组件保存；布尔字段默认 `false`，`luckPotion` 默认 `0`，`endurance` 默认 `0f`，两个鞭子倍率默认 `1f`。
- 组件只包含状态和内部效果重置，不包含计划中的 System、Query、Snapshot、Projectile/Item lifecycle 或伤害提交。
- 未验证项：endurance bounds/damage order、luck calculation、whip timing、environment reset、electrified/dryad/panic reader purity，以及跨分区唯一 owner。
- 依赖影响：C10/C19、PlayerLuck、Projectile、StatusEffects、World/Liquid；保留旧 reader，未发生 reader migration 或双写。

当前实际进度：C07 源码已保存；下一组件为 C08。`completedComponents` 应包含 `C07.PlayerDetectionAndCombatStatus-source`，`pendingComponents` 保持 C08-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 13. C08 执行计划：PlayerSocialAndDefenseState

源边界：D:\TRbackup\Version4\Terraria\Player.cs:904-958、:3848-3971、:4509、:5848、:6051、:6794-6806、:8265、:8987-8990、:10066-10067、:15005-15008、:15087-15089、:21885-21887、:22189-22201、:22287-22361、:22583-22593、:23983-23985。

计划目标文件：

- dome/src/Terraria.Dome.Simulation/Player/PlayerGhostStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerDefenseCapabilityComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Interaction/PlayerInteractionPolicyComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Interaction/PlayerPortableStoolStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerSocialDefenseSystem.cs

实施顺序：

1. 先冻结 C08 20 成员到五个生命周期/能力边界，确认不创建混合巨型组件。
2. 迁移 Ghost/achievement/PvP 状态到 PlayerLifecycle/Combat 的显式 ports。
3. 将 defense capability 接入 DamageCommitPort，将 interaction flags 接入纯 policy Query。
4. 将 portable stool 只转换为 Movement input，不直接改位置。
5. 迁移 social appearance 到 P11 projection seam，并通过 reset/death/PvP verifier。

依赖影响：PlayerLifecycle/P04、CombatAndStatus、WorldInteractionAndStructures、Spatial/Movement、P09 inventory/pickup、P11 presentation。任一最终 owner 未决则不迁移 readers。

验收门槛：20 个成员逐条归属；Ghost 不替代 active/dead；defense 不直接扣血；interaction query 无写回；portable stool 不拥有位置。

### 13.1 C08 实际源码 checkpoint：PlayerSocialAndDefenseState

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerGhostStateComponent.cs`、`src/Player/PlayerSocialAppearancePreferenceComponent.cs`、`src/Player/PlayerDefenseCapabilityComponent.cs`、`src/Player/PlayerInteractionPolicyComponent.cs`、`src/Player/PlayerPortableStoolStateComponent.cs`、`src/Player/PlayerAchievementEligibilityStateComponent.cs`。
- 20 个字段已按六个组件保存；`portableStoolInfo` 的五个稳定值字段已展开，所有新组件均使用 `Terraria.Player` 命名空间。
- 组件只包含状态、默认值和内部生命周期/效果重置，不包含计划中的 SocialDefense System/Query、死亡提交、位置变更、库存变更或表现副作用。
- 未验证项：Ghost/death、PvP death、Paladin sharing、CrystalLeaf cooldown、portable stool transition、interaction policy rejection、achievement timer expiry，以及跨分区唯一 owner。
- 依赖影响：P04 lifecycle、P09 pickup/equipment、P11 presentation、Movement/Spatial、Town/NPC、Combat；未迁移旧 readers，未发生双写。

当前实际进度：C08 源码已保存；下一组件为 C09。`completedComponents` 应追加 `C08.PlayerSocialAndDefenseState-source`，`pendingComponents` 保持 C09-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 14. C09 执行计划：PlayerFrameAndImmunityState

源边界：D:\TRbackup\Version4\Terraria\Player.cs:961-986、:10808-10835、:12014-12030、:15034-15063、:20473-20625、:20808-20961、:21846-21906、:22084-22089、:22221-22335、:22652-22655。

计划目标文件：

- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerHitFrameImmunityComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerRegenDelayStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Presentation/PlayerFrameProjection.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerImmunityTickSystem.cs

实施顺序：

1. 先定义 Player immunity key 与 existing HitImmunity/Combat immunity 的归属，停止数组槽位混用。
2. 建立 immune timer/strike focused verifier，确认 collision and hurt paths 的一次提交。
3. 将 alpha/body/leg counters 转为只读 frame projection；maxRegenDelay 转为 explicit mana snapshot query。
4. 以 townNPCs 只读 integration seam 交给 Town/NPC owner。
5. 在 immunity/animation/reconnect tests 未完成前不迁移 legacy writes。

依赖影响：CombatAndStatus、P04 Player lifecycle、P11 Presentation、P13 Town/NPC、C10 vital/regen、P15 Projectile。回滚时保留 old immunity reads，禁止新旧 timer 双写。

验收门槛：11 个成员逐条归属，authority/presentation 分离，cooldown key 不冲突，maxRegenDelay 可重算，townNPCs 不复制。

### 14.1 C09 实际源码 checkpoint：PlayerFrameAndImmunityState

- implementationStatus: partial
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerHitFrameImmunityComponent.cs`、`src/Player/PlayerRegenDelayStateComponent.cs`。
- 已保存 6/11 字段：无敌 authority 的五个字段和 `maxRegenDelay`；仅提供状态默认值与生命周期重置。
- 未保存 `townNPCs`、`bodyFrameCounter`、`legFrameCounter`、`immuneAlphaDirection`、`immuneAlpha`，原因是前者属于 Town/NPC 跨域快照，后四者属于禁止本次实现的 presentation projection。
- 未验证项：immune timer/strike window、hurt cooldown key isolation、alpha/frame projection、`maxRegenDelay` calculation 和唯一 immunity writer。
- 依赖影响：Combat immunity、P04 lifecycle、P11 presentation、P13 Town/NPC、C10 vital/regen、P15 Projectile；未迁移旧 readers，未发生双写。

当前实际进度：C09 authority component 已保存但 C09 叶子组仍 partial；下一组件为 C10。`completedComponents` 应追加 `C09.PlayerFrameAndImmunityState-authority-source`，`pendingComponents` 保留 C09 projection/Town handoff、C10-C21 与 C22 integrations。

## 15. C10 执行计划：PlayerVitalAndRegenState

源边界：D:\TRbackup\Version4\Terraria\Player.cs:1357-1381、:10332、:10386-10392、:10847-11315、:15601-15606、:17668-17674、:21830-21899、:22257-22379。

计划目标文件：

- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerVitalStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerLifeRegenStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerManaRegenStateComponent.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerVitalCommitSystem.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerLifeRegenSystem.cs
- dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerManaRegenSystem.cs

实施顺序：

1. 先建立 13 字段 member/default/reset verifier 和 PlayerVitalSnapshot。
2. 决定 HealthComponent/ManaComponent 与 PlayerVitalState 的唯一 authority；旧组件只能作为 read adapter。
3. 建立 Damage/Heal/Regen/Respawn 四类 command 的同一 vital commit root，禁止独立直接写 current。
4. 按 Version4 证据验证 ResetEffects -> status -> damage -> regen -> clamp -> death transition 的候选顺序。
5. 迁移 network/persistence projection；Version4 Serialize 为空时维持 persistence-blocked。

依赖影响：C03/C04/C09/C11/C19 damage/immune/modifiers、P04 lifecycle/death、P09 equipment/Buff、Network/Session、Persistence。任何第二写者出现即回滚本批。

验收门槛：13 个成员逐条归属，current/max/accumulator invariants，damage/regen/death 顺序，Health/Mana 无双写，snapshot 只读。

### 15.1 C10 实际源码 checkpoint：PlayerVitalAndRegenState

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerVitalStateComponent.cs`、`src/Player/PlayerLifeRegenStateComponent.cs`、`src/Player/PlayerManaRegenStateComponent.cs`。
- 13 个字段已保存；生命/魔力默认值按 Version4 初始化语义记录，regen 派生状态和生命周期累加器分离保存。
- 组件只包含状态数据、基线和内部 reset，不包含 Vital Commit、Life/Mana Regen System、Snapshot、death transition 或网络/持久化行为。
- 未验证项：current/max clamp、damage then regen order、negative regen、mana delay、accumulator threshold、death boundary、respawn restore、duplicate command、network snapshot 和 persistence。
- 依赖影响：C03/C04/C09/C11/C19、P04 lifecycle、P09 equipment/Buff、Network/Session、Persistence；未迁移旧 Health/Mana readers，未发生双写。

当前实际进度：C10 源码已保存；下一组件为 C11。`completedComponents` 应追加 `C10.PlayerVitalAndRegenState-source`，`pendingComponents` 保持 C09 handoff、C11-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 5. 验证命令计划

本设计会话不运行 compile-capable command。获得实现授权后，必须从仓库根目录串行执行：

~~~powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 test .\dome\Test\<AffectedVerifier>\<AffectedVerifier>.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false --no-build --no-restore
~~~

执行前检查 dotnet.exe/csc.exe，记录命令、项目、退出码、警告/错误数量和 Build/bin artifact。当前未运行任何上述命令。

## 16. C11 执行计划：PlayerCombatModifierAndImmunityState

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1351-1431、:3366-3376、:9162-9165、:10320-10585、:14165-14168、:14820-14824、:15349-15633、:17133-17147、:22208-22385、:25630-25637`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerCombatDefenseModifierComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerCombatCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerEnvironmentInteractionSnapshot.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerCombatModifierCommitSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerCombatPermissionQuery.cs`
- `dome/src/Terraria.Dome.Server/Replication/PlayerCombatModifierProjection.cs`

实施顺序：

1. 建立 16 字段的 default/reset 和 `CombatModifierSnapshot` focused verifier，先确认 C11 与 C10/C19 的提交根不重叠。
2. 将 equipment/Buff 事实转换为 immutable input，由唯一 modifier commit system 提交 defense/capability；旧 Player 字段在兼容期只由 read adapter 暴露。
3. 以 `GetArmorPenetration` 的攻击类型分支固定 melee/non-melee 查询，验证 statDefense 非负 clamp 和 noKnockback 的 Hurt 语义。
4. 通过 Movement/Liquid adapter 提供 shimmerImmune/gravDir 快照；验证 P06 不写位置、重力、液体或速度，spaceGun 只进入 item/mana permission query。
5. 通过重复装备重算、volatile counter expiry、snapshot round-trip 和 rejected action verifier 后再开始 C12。

依赖影响：C10 vital/regen、C12/C16 accessory effects、C13 shimmer/elemental、C19 damage modifiers、C20 movement/permission、P09 Item/Buff、P15 Projectile、Liquid/World。回滚时恢复旧 modifier readers，移除新 writer registration；若 owner 或顺序未裁决，保持 planned/not-started。

验收门槛：16 个成员逐条归属，防御/能力/环境快照互不越权，只有一个 stat/penetration writer，gravity/shimmer 只读交接，未运行编译或测试。

检查点结论：C11 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 16.1 C11 实际源码 checkpoint：PlayerCombatModifierAndImmunityState

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerCombatDefenseModifierComponent.cs`、`src/Player/PlayerCombatCapabilityComponent.cs`、`src/Player/PlayerEnvironmentInteractionComponent.cs`。
- 16 个字段已按防御修正、战斗能力和环境交互三个组件保存；`shimmerImmune` 不持有 Liquid helper，`gravDir` 仅保留 `1f` 基线。
- 组件只包含状态、Version4 默认值和内部 reset，不包含计划中的 modifier system、permission query、environment adapter、projection 或伤害/位置副作用。
- 未验证项：penetration composition、statDefense clamp、noKnockback 分支、spaceGun permission、shimmer handoff、volatile counter tick、gravDir reader purity 和唯一 writer。
- 依赖影响：C10/C19、C13、C20、P09、P15、Liquid/World/Movement、Network/Presentation；未迁移旧 readers，未发生双写。

当前实际进度：C11 源码已保存；下一组件为 C12。`completedComponents` 应追加 `C11.PlayerCombatModifierAndImmunityState-source`，`pendingComponents` 保持 C09 handoff、C12-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 17. C12 执行计划：PlayerAmmoAndAccessoryEffects

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1391-1413、:10388-10391、:10448-10449、:10583-10588、:10754-10757、:13986-13990、:26068-26200`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerAmmoCostPolicyComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerRangedAccessoryCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerStickyBreakStateComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerAmmoConsumptionQuery.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerAccessoryCapabilityRebuildSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerAmmoCommitSystem.cs`

实施顺序：

1. 先建立 12 字段的 default/reset、seeded random decision 和 stack invariant verifier。
2. 将装备/Buff 转为能力快照，先迁移只读 ammo/projectile Query；旧 Item/Projectile 路径保持唯一行为源。
3. 定义一次发射 command 的 id/revision 和 `IItemStackCommitPort`，仅由 committed consume decision 扣减一次。
4. 独立迁移 phantasmTime timeout 与 stickyBreak movement seam，验证 pause/death/reset 清理不影响 ammo decision。
5. 通过概率组合、重复 command、低 stack、Projectile request 和 Item payload 隔离 verifier 后再开始 C13。

依赖影响：P09 ItemContainer/Equipment、P15 Projectile、C11 capability、C13 elemental、C16 accessory combat、C19 damage/crit、C20 speed/permission。回滚时保留旧 ammo reader/consumer，移除新 command adapter；未裁决随机序列或 stack owner 时保持 planned/not-started。

验收门槛：12 个成员逐条归属，Query 纯净，随机性和扣减副作用隔离，Projectile/Item 外部类型不进入组件，未运行编译或测试。

检查点结论：C12 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 17.1 C12 实际源码 checkpoint：PlayerAmmoAndAccessoryEffects

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerAmmoCostPolicyComponent.cs`、`src/Player/PlayerRangedAccessoryCapabilityComponent.cs`、`src/Player/PlayerStickyBreakStateComponent.cs`。
- 12 个字段已保存；四个 ammo-cost 标记、ammo box/potion、远程配饰能力、`phantasmTime` 和 `stickyBreak` 没有混入 Item、Projectile 或 Movement 对象。
- `ResetEffects` 只清理本帧装备/Buff 派生标记；`PhantasmTime` 和 `StickyBreak` 由生命周期 reset 清除，未来 timeout/movement owner 未实现。
- 未验证项：随机样本边界、stack 下溢/幂等、quiver request、phantasm expiry、stickyBreak 清理和 Item payload 隔离。
- 依赖影响：P09 ItemContainer/Equipment、P15 Projectile、C11/C13/C16/C19/C20、Network/Presentation；未迁移旧 readers，未发生双写。

当前实际进度：C12 源码已保存；下一组件为 C13。`completedComponents` 应追加 `C12.PlayerAmmoAndAccessoryEffects-source`，`pendingComponents` 保持 C09 handoff、C13-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 18. C13 执行计划：PlayerElementalAndShimmerStatus

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1712-1753、:4453-4457、:5882-5997、:10293-10313、:10633-10653、:10847-11234、:15307-15310、:17132-17366、:21744-21750、:22212-22214、:26087-26093`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerElementalStatusComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerSurfaceElementStatusComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerShimmerStateComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Adapters/IShimmerUnstuckAdapter.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerElementalStatusRebuildSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerShimmerEnvironmentSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerElementalQuery.cs`

实施顺序：

1. 先为 21 个字段建立 Buff-derived/reset、regen modifier 和 shimmer enter/leave verifier。
2. 将 Buff type/time 和 Liquid/World contact 转换为稳定 snapshot，迁移唯一 flags writer；旧 Buff reader 保持只读兼容。
3. 通过 `IShimmerUnstuckAdapter` 处理 helper 的 update/clear/failure，禁止 helper 引用进入 authority component。
4. 分别验证 shimmering qualification、time/transparent projection、Teleport/death clear、archery projectile input 和 C10 regen input。
5. 只有在 Liquid/Movement/World owner 及 Buff alias owner 批准后再迁移 readers，并继续到 C14。

依赖影响：StatusEffects/BuffCollection、C10 vital/regen、C11 permission、C12 projectile、C14 survival、C15 debuff、C19 damage、Liquid/World/Movement、P11 Presentation。回滚时恢复旧 flags/environment reader，撤销新 helper adapter registration。

验收门槛：21 个成员逐条归属，Buff type/time 不双写，shimmer helper 与 projection 单向，C13 不拥有生命/位置/液体，未运行编译或测试。

检查点结论：C13 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 18.1 C13 实际源码 checkpoint：PlayerElementalAndShimmerStatus

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerElementalStatusComponent.cs`、`src/Player/PlayerSurfaceElementStatusComponent.cs`、`src/Player/PlayerShimmerStateComponent.cs`。
- 21 个字段已保存；`shimmerUnstuckHelper` 以 `ShimmerUnstuckTimeLeft` 与 `ShimmerUnstuckProtectionActive` 两个稳定 scalar 表示，未引用 Version4 helper 类型。
- 元素/表面组件只提供效果重置；微光组件只保存状态和生命周期 reset，不实现 Liquid/World 交接、透明度时间推进、Buff、regen、Projectile 或 Presentation 行为。
- 未验证项：Buff rebuild/expiry、元素对 regen 的顺序、shimmer enter/leave、transparency clamp、teleport/death clear、helper adapter failure/retry 和 projection purity。
- 依赖影响：StatusEffects/BuffCollection、C10、C11、C12、C14/C15、C19、Liquid/World/Movement、P11；未迁移旧 readers，未发生双写。

当前实际进度：C13 源码已保存；下一组件为 C14。`completedComponents` 应追加 `C13.PlayerElementalAndShimmerStatus-source`，`pendingComponents` 保持 C09 handoff、C14-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 19. C14 执行计划：PlayerSurvivalAndTransformationState

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1755-1793、:5902-6174、:6868-6873、:8017-8041、:8767-9207、:9974-9994、:10635-10658、:11519-11522、:14841-14847、:15240-15270、:17573-17578、:17713-17724、:25283-25290、:25692-25750`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Survival/PlayerSurvivalNeedComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Survival/PlayerTransformationCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Survival/PlayerEnvironmentalPressureComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Survival/PlayerTridentCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Survival/Systems/PlayerSurvivalTransformationSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Survival/Queries/PlayerTransformationQuery.cs`

实施顺序：

1. 先建立 15 字段 default/reset、Buff-derived need 和 wet/mount/transform matrix verifier。
2. 将 Buff、Equipment、World/Liquid/environment input 转换为稳定 snapshot，迁移唯一 survival/transformation writer。
3. 将 noItems/trident 迁移为纯 item-use query，将 windPushed/merman 迁移为 Movement input，不直接写运动状态。
4. 独立验证 sunScorchCounter 的 0..300 clamp、death behavior、clock/effect port 与 C10 regen/damage input。
5. 通过 reset/respawn/teleport、重复 command、mount/wet/lava 组合和 projection verifier 后再开始 C15。

依赖影响：C10 vital/regen、C13 elemental/shimmer、C15 debuff、C20 speed/permission、P04 lifecycle、P09 Item/Buff、Liquid/World/Movement、P11 Presentation。回滚时恢复旧 transform/environment reader，移除新 movement input registration；未裁决 owner 时保持 planned/not-started。

验收门槛：15 个成员逐条归属，survival 不拥有生命/库存，transformation 不拥有位置/重力，environment counter 副作用隔离，未运行编译或测试。

检查点结论：C14 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 19.1 C14 实际源码 checkpoint：PlayerSurvivalAndTransformationState

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerSurvivalNeedComponent.cs`、`src/Player/PlayerTransformationCapabilityComponent.cs`、`src/Player/PlayerEnvironmentalPressureComponent.cs`、`src/Player/PlayerTridentCapabilityComponent.cs`。
- 15 个字段已保存；生存、变身、环境压力和 trident capability 分离，`trident` 仅为当前物品能力事实，不持有 `Terraria.Item` 或物理对象。
- 组件只提供状态默认值和效果/生命周期 reset，不实现 SurvivalTransformation System、wet/mount query、Movement input、sun scorch side effect、Item use 或 projection。
- 未验证项：need reset/regen ordering、noItems gate、wet/mount/merman matrix、wolf/merman reset edge、wind input purity、sunScorch clamp/death behavior、trident query 和 teleport/respawn cleanup。
- 依赖影响：C10/C13/C15/C20、P04 lifecycle、P09 Item/Buff、Liquid/World/Movement、P11；未迁移旧 readers，未发生双写。

当前实际进度：C14 源码已保存；下一组件为 C15。`completedComponents` 应追加 `C14.PlayerSurvivalAndTransformationState-source`，`pendingComponents` 保持 C09 handoff、C15-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 20. C15 执行计划：PlayerDebuffStatusState

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1757-1803、:4466-4506、:5276-5292、:5902-5906、:10654-10674、:10847-11235、:15553-15573、:15721-15723、:16075-16077、:17557-17611、:22221-22335、:25283-25366`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerDebuffStatusAliasComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerDebuffAliasRebuildSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerDebuffEffectQuery.cs`
- `dome/src/Terraria.Dome.Server/Replication/PlayerDebuffStatusProjection.cs`

实施顺序：

1. 先建立八字段的 Buff type/time/expiry and reset verifier，并列出 C06 已有 alias readers，确认唯一 rebuild writer。
2. 把 BuffCollection 的已提交 snapshot 转为 C15 alias，先迁移只读 effect query；保留旧 Player flag reader，禁止新旧 alias 双写。
3. 将 cursed/silence、slow/tongued、brokenArmor、bleed effect 分别接入 Item/Movement/Defense/Regen ports，不让 Query 直接修改外部状态。
4. 验证 death/reset、Buff removal、重复 command、暂停 tick、C06 reader-only 和 projection 单向性后再开始 C16。

依赖影响：C06 recovery、C10 vital/regen、C11 defense、C13 elemental、C14 survival、C20 speed/permission、StatusEffects、Movement/World、ItemContainer。回滚时恢复旧 alias readers，撤销新 rebuild writer；owner 未裁决时保持 planned/not-started。

验收门槛：8 个成员逐条归属，Buff type/time 不复制，alias 只有一个 writer，Query 无写回，effect ordering 有 verifier 计划，未运行编译或测试。

检查点结论：C15 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 20.1 C15 实际源码 checkpoint：PlayerDebuffStatusState

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerDebuffStatusAliasComponent.cs`。
- 8 个 alias 字段已保存为当前 tick 的状态快照；不保存 Buff type/time、Item/NPC source，也不拥有 immunity、life、mana 或 movement。
- 组件只提供 `false` 基线和内部 `ResetEffects`，未实现 Buff alias rebuild、effect Query、projection 或任何状态副作用。
- 未验证项：type/time/expiry、ResetEffects/death clear、cursed/silence item gate、slow/tongued movement gate、brokenArmor defense input、bleed/tongued regen ordering、重复 Buff command 和 reader-only 约束。
- 依赖影响：C06、C10、C11、C13/C14、C20、StatusEffects/BuffCollection、Movement/World、ItemContainer；未迁移旧 readers，未发生双写。

当前实际进度：C15 源码已保存；下一组件为 C16。`completedComponents` 应追加 `C15.PlayerDebuffStatusState-source`，`pendingComponents` 保持 C09 handoff、C16-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 21. C16 执行计划：PlayerAccessoryCombatModifierState

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1805-1821、:4588-4592、:8023-8027、:8182-8187、:8231-8236、:8593-8598、:8777-8782、:8933-8948、:9093-9096、:9213-9220、:10675-10683、:12596-12605、:23426-23431、:25719-25725、:25864-25873`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerAccessoryMeleeCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerDroneVisionCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerStarCloakSourceRelationComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerAccessoryCombatRebuildSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerAccessoryCombatQuery.cs`
- `dome/src/Terraria.Dome.Server/Replication/PlayerStarCloakProjection.cs`

实施顺序：

1. 建立 9 字段 reset、equipment enumeration、ItemReference slot-reuse 和 source revision verifier。
2. 先迁移 glove/vision 只读 Query，再建立唯一 accessory rebuild writer；兼容层只能读取旧 Player flag。
3. 定义四种 star cloak relation 的 override priority、resolve failure 和 stale revision 规则，禁止复制 Item。
4. 对接 C12/C19/P15 consumers，验证 knockback/scale/reuse 参数的提交顺序和 drone/presentation 只读性。
5. 通过装备重算、断线重连、空/失效 ItemReference、override 组合和 reset/death verifier 后再开始 C17。

依赖影响：P09 Item/Equipment、StatusEffects、C12/C17/C18/C19、P15 Projectile、P11 Presentation/Drone。回滚时恢复旧 Item source readers，撤销新 relation resolver；owner 或 priority 未裁决时保持 planned/not-started。

验收门槛：9 个成员逐条归属，Item payload 不复制，relation 可检测失效，glove/vision query 纯净，星披风 source 只有一个选择 writer，未运行编译或测试。

检查点结论：C16 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 21.1 C16 实际源码 checkpoint：PlayerAccessoryCombatModifierState

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerAccessoryMeleeCapabilityComponent.cs`、`src/Player/PlayerDroneVisionCapabilityComponent.cs`、`src/Player/PlayerStarCloakSourceRelationComponent.cs`。
- 9 个字段已保存；四个原始 `Terraria.Item` 成员以 `ItemEntityRef` 关系保存，未复制 Item 可变 payload，也未创建 source revision/override type。
- 组件只包含能力状态、`ItemEntityRef.None` 基线和内部 reset，不实现 accessory rebuild、Item resolver、combat/vision Query、projection 或 Projectile/camera 行为。
- 未验证项：ResetEffects/source clearing、equipment enumeration、slot reuse、knockback/scale/auto-reuse、remote vision 及 star-cloak source selection。
- 依赖影响：P09 Equipment/Item、C12/C19/P15/P11、StatusEffects、Network/Presentation；未迁移旧 readers，未发生双写。

当前实际进度：C16 源码已保存；下一组件为 C17。`completedComponents` 应追加 `C16.PlayerAccessoryCombatModifierState-source`，`pendingComponents` 保持 C09 handoff、C17-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 22. C17 执行计划：PlayerAccessoryResourceAndInvulnerabilityState

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1823-1831、:6025-6029、:8778-8787、:9208-9228、:9991、:10684-10701、:15193-15202、:15303-15305、:22100-22121、:22325-22334、:25540-25543`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerAccessoryResourceProtectionComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Definitions/PotionDelayPolicyDefinition.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerAccessoryResourceSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/ResourceProtectionQuery.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Commands/QuickManaCommand.cs`

实施顺序：

1. 建立 5 字段 default/reset、immutable policy version、pStone edge 和 longInvince timer verifier。
2. 将装备/Buff 输入迁移为 capability snapshot，先迁移只读 resource protection Query，旧 resource/immune readers 保持唯一行为源。
3. 建立 QuickMana command/commit/replay contract，由 Mana/Item owner 执行，C17 不直接写 statMana 或 consume Item。
4. 为 moonLeech 保留 unknown-consumer guard，补证前不实现推测性 reader；验证 definition version and snapshot compatibility。
5. 通过 resource clamp、death/reset、重复 command、网络重放和 projection verifier 后再开始 C18。

依赖影响：C09 immunity、C10 vital/mana、C15 Buff alias、C16 accessory、C19 damage、P09 Item/Equipment、StatusEffects、Network/Presentation。回滚时恢复旧 resource/immune readers，撤销新 QuickMana adapter；moonLeech 证据不足时保持 planned/not-started。

验收门槛：5 个成员逐条归属，static policy 与 mutable state 分离，QuickMana/免疫/延迟无第二 writer，unknown consumer 不被猜测，未运行编译或测试。

检查点结论：C17 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 22.1 C17 实际源码 checkpoint：PlayerAccessoryResourceAndInvulnerabilityState

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerAccessoryResourceProtectionComponent.cs`。
- 5 个字段已保存；`PhilosopherStoneDurationMultiplier` 为 `static readonly 0.75f` 类型策略值，四个配饰标记默认 `false` 并由 `ResetEffects` 清理。
- 组件不直接修改 statMana、potion delay、immuneTime、Buff 或 Item；未实现资源保护 System、Query、QuickMana Command、projection 或网络覆盖。
- 未验证项：QuickMana/resource qualification、无敌时长、药水延迟、策略值版本、ResetEffects 和 projection purity。
- 依赖影响：C03/C09/C10、P09 Item/Buff、StatusEffects、Network/Persistence；未迁移旧 readers，未发生双写。

当前实际进度：C17 源码已保存；下一组件为 C18。`completedComponents` 应追加 `C17.PlayerAccessoryResourceAndInvulnerabilityState-source`，`pendingComponents` 保持 C09 handoff、C18-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 23. C18 执行计划：PlayerAccessoryDebuffAndDropState

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1833-1847、:5997-6024、:9993-10005、:10663-10705、:14866-14869、:15557-15563、:15620-15635、:17548-17551、:22585-22588、:23570-23575、:23834-23837`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerAccessoryDebuffCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Interaction/PlayerItemDropTransientComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerAccessoryDebuffSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerAccessoryDebuffQuery.cs`
- `dome/src/Terraria.Dome.Server/Replication/PlayerDropEventAdapter.cs`

实施顺序：

1. 建立 8 字段 default/reset、Buff-derived rebuild、modifier ordering 和 drop event/replay verifier。
2. 先迁移七个 capability 的只读 Query；把 vortex/withered/slow outputs 接入 C11/C19/C20 ports，禁止直接写外部 state。
3. 找到并固化 `JustDroppedAnItem` 的唯一写入源，定义 P09 drop command/event idempotence；当前证据不足时不实现写入。
4. 接入 trap achievement adapter，验证 committed death event only-once、drop/presentation 不双发和 unknown parry/ballista consumers。
5. 通过 Buff expiry、modifier composition、drop replay、death/reset 和 projection verifier 后再开始 C19。

依赖影响：C11/C15/C19/C20、P04 lifecycle/achievement、P09 Item/Drop、P15 Projectile/NPC、StatusEffects。回滚时恢复旧 capability/drop readers，撤销新 event adapter；JustDropped source 未补证时保持 planned/not-started。

验收门槛：8 个成员逐条归属，modifier 与 drop 副作用隔离，drop/achievement 只由 committed event 驱动，未运行编译或测试。

检查点结论：C18 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 23.1 C18 实际源码 checkpoint：PlayerAccessoryDebuffAndDropState

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerAccessoryDebuffCapabilityComponent.cs`、`src/Player/PlayerItemDropTransientComponent.cs`。
- 8 个字段已保存；七个减益能力和 `JustDroppedAnItem` 分离，drop flag 仅为兼容性 transient 状态。
- 组件只包含 `false` 基线和 reset，不实现 Movement/Defense/Damage modifier、drop command、ItemContainer mutation、achievement side effect 或 projection。
- 未验证项：vortex movement input、withered modifier composition、slow precedence、parry/ballista consumer、trap achievement once-only、drop command/event/replay、flag reset。
- 依赖影响：C11/C15/C19/C20、P04、P09、P15、StatusEffects、Movement/World；未迁移旧 readers，未发生双写。

当前实际进度：C18 源码已保存；下一组件为 C19。`completedComponents` 应追加 `C18.PlayerAccessoryDebuffAndDropState-source`，`pendingComponents` 保持 C09 handoff、C19-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 24. C19 执行计划：PlayerCombatDamageAndCritModifiers

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1851-1877、:3074-3078、:4382-4850、:6052-6170、:6939-6950、:10335-10346、:10394-10397、:12607-12609、:15231-15233、:15414-15418、:15532-15535、:15628-15635、:23489-23501、:25859-25883`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerWeaponDamageModifierComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerSummonerDamageModifierComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerWeaponCritModifierComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerCombatDamageModifierRebuildSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerDamageMultiplierQuery.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerCritQuery.cs`

实施顺序：

1. 建立 14 字段 defaults/reset、effective damage formula、modifier ordering 和 seeded crit verifier。
2. 先迁移纯 damage/crit Query，冻结 C11/C15/C18 modifier snapshot 输入；旧 Player damage reader 保持唯一行为源。
3. 把 projectile/NPC weapon owner 接入 ordered `DamageModifierSnapshot`，禁止 target/vital writer 进入 C19。
4. 定义 revolver crit bonus 的 random command/revision 和 replay policy，独立验证 minion damage/KB。
5. 通过 category fallback、withered/mana/debuff order、crit replay、duplicate hit and projection verifier 后再开始 C20。

依赖影响：C10 vital/regen、C11 defense/penetration、C15 debuff、C18 withered/accessory、C20 speed/permission、P09 Item、P15 Projectile/NPC、Random/Network/Telemetry。回滚时恢复旧 damage readers，移除新 modifier Query/adapter；顺序或 commit root 未裁决时保持 planned/not-started。

验收门槛：14 个成员逐条归属，damage/crit/summon 能力分离，公式顺序和随机性可验证，C19 不写生命/目标，未运行编译或测试。

检查点结论：C19 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 24.1 C19 实际源码 checkpoint：PlayerCombatDamageAndCritModifiers

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerWeaponDamageModifierComponent.cs`、`src/Player/PlayerSummonerDamageModifierComponent.cs`、`src/Player/PlayerWeaponCritModifierComponent.cs`。
- 14 个字段已保存；Version4 的暴击基线为 `4`，伤害倍率基线为 `1f`，additive/knockback/revolver 计数为 `0`。
- 组件只保存 modifier inputs，不持有 Item base damage、Projectile/NPC target、随机源或 hit side effect；未实现 rebuild system、damage/crit Query、random roll 或 projection。
- 未验证项：defaults/reset、bow/gun/specialist formula、additive/multiplicative order、crit clamp/seed replay、revolver transaction、withered/mana/debuff ordering、summon damage/KB。
- 依赖影响：C10/C11/C15/C18/C20、P09、P15、Random/Network/Telemetry；未迁移旧 readers，未发生双写。

当前实际进度：C19 源码已保存；下一组件为 C20。`completedComponents` 应追加 `C19.PlayerCombatDamageAndCritModifiers-source`，`pendingComponents` 保持 C09 handoff、C20-C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 25. C20 执行计划：PlayerCombatSpeedRangeAndPermissionState

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:1849-1893、:3454-3470、:4354-4562、:6783-6994、:7043-7976、:8182-9032、:10276、:10334-10347、:10510-10513、:10657、:15536-15548、:15594-15611、:23570-23575`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerAttackSpeedModifierComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Movement/PlayerMovementSpeedModifierSnapshot.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Interaction/PlayerBuildAndMiningSpeedComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Interaction/PlayerItemPermissionComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerSpeedPermissionRebuildSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Queries/PlayerSpeedQuery.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Queries/PlayerInteractionPermissionQuery.cs`

实施顺序：

1. 建立 9 字段 defaults/reset、speed cap/inversion、permission precedence 和 movement/Tile input verifier。
2. 迁移唯一 speed/permission rebuild writer，先接入只读 Query；旧 Item/Movement/Tile readers 保持唯一外部行为源。
3. 将 attack speed 转换和 pick/tile/wall bounds 固化在一个纯 calculation seam，禁止重复 reciprocal/cap。
4. 将 moveSpeed 发布为 Movement input，将 autoPaint/autoActuator 发布为 WorldInteraction permission；P06 不执行外部 mutation。
5. 通过 noItems/cursed/drop priority、duplicate rebuild、pause/death/reset 和 projection verifier 后再开始 C21。

依赖影响：C14/C15/C18/C19、P09 Item/Equipment、Movement、WorldInteraction/Tile、P11 Presentation。回滚时恢复旧 speed/permission readers，撤销新 input adapter；owner/ordering 未裁决时保持 planned/not-started。

验收门槛：9 个成员逐条归属，speed conversion 单一、movement/Tile/Item side effects 隔离、permission precedence 有证据，未运行编译或测试。

检查点结论：C20 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 25.1 C20 实际源码 checkpoint：PlayerCombatSpeedRangeAndPermissionState

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerAttackSpeedModifierComponent.cs`、`src/Player/PlayerMovementSpeedModifierComponent.cs`、`src/Player/PlayerBuildAndMiningSpeedComponent.cs`、`src/Player/PlayerItemPermissionComponent.cs`。
- 9 个字段已保存；攻击/移动/工具速度默认 `1f`，summoner bonus 为 `0f`，`IsAllowedToHoldItems` 默认 `true`，自动化标记默认 `false`。
- 组件只保存速度和权限输入并恢复基线，不实现 speed/permission Query、Movement/Tile side effect、ItemContainer mutation、animation 或 projection。
- 未验证项：attack speed cap/inversion、summoner conversion、pick/tile/wall boundaries、movement snapshot purity、permission precedence、auto rejection 和 projection one-way。
- 依赖影响：C14/C15/C18/C19、P09、Movement、WorldInteraction/Tile、P11；未迁移旧 readers，未发生双写。

当前实际进度：C20 源码已保存；下一组件为 C21。`completedComponents` 应追加 `C20.PlayerCombatSpeedRangeAndPermissionState-source`，`pendingComponents` 保持 C09 handoff、C21 与 C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 26. C21 执行计划：PlayerLuckAndCommerceEffects

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:2030-2054、:3683-3688、:4326-4350、:6691-6702、:7177-7180、:8273-8275、:8604-8629、:10276-10347、:10490、:10724-10731、:17869-17928、:19996-20002`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerCommerceCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerLuckCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Movement/PlayerWaterAndUtilityCapabilityComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Inventory/PlayerBoneGloveSourceRelationComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerLuckAndCommerceRebuildSystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Queries/PlayerLuckAndCommerceQuery.cs`
- `dome/src/Terraria.Dome.Server/Replication/PlayerLuckAndCommerceProjection.cs`

实施顺序：

1. 建立 13 字段 defaults/reset、equipment/Buff rebuild、discount shop-context derivation、ItemReference identity 和 deterministic luck verifier。
2. 先迁移纯 luck/discount/coin-pickup Query；旧 Player/Item/World reader 保持唯一外部行为源，禁止把 luck Query 连接到掉落或库存 mutation。
3. 将 `boneGloveItem` 转换为 typed `ItemReference`，验证 source revision 和装备槽位复用；ItemContainer owner 只提供 read adapter，不把可变 Item payload 放进 P06 component。
4. 将 `deadCellsPotionStation` 发布给 Buff timing policy，将 `accDivingHelm`/`accFlipper` 发布给 Liquid/Movement utility input；P06 不写 Buff timer、velocity、gravity 或环境实体。
5. 通过 default/reset、discount context、coin pickup, luck calculation, Buff timing, utility purity、duplicate command/replay 和 projection verifier 后，才开始 C22。

依赖影响：C13/C14/C19/C20、P09 Item/Equipment/Inventory、StatusEffects/Buff、Liquid/Movement、World drop/economy、Network/Presentation。回滚时恢复旧 luck/commerce readers，撤销新 ItemReference/utility adapter；owner 或随机/drop 边界未裁决时保持 planned/not-started。

验收门槛：13 个成员逐条归属；discountAvailable 不产生第二 authority；ItemReference 不复制 Item payload；luck/coin/drop、Buff timing 和 water utility 副作用隔离；未运行编译或测试。

检查点结论：C21 执行计划已保存；未创建 C#、测试或项目文件，executionStatus 仍为 planned、implementationStatus 仍为 not-started、verificationStatus 仍为 not-run。

### 26.1 C21 实际源码 checkpoint：PlayerLuckAndCommerceEffects

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际修改：`src/Player/PlayerCommerceCapabilityComponent.cs`、`src/Player/PlayerLuckCapabilityComponent.cs`、`src/Player/PlayerWaterAndUtilityCapabilityComponent.cs`、`src/Player/PlayerBoneGloveSourceRelationComponent.cs`。
- 13 个字段已保存；源名中的 `hasLuck_*` 已按 C# PascalCase 映射为 `HasLuck*`；`boneGloveItem` 使用已有 `ItemEntityRef.None`，未保存 `Terraria.Item` 对象。
- 组件只保存能力/关系状态和效果 reset，不实现 luck/commerce Query、drop/coin mutation、Buff timing、Liquid/Movement side effect、Item resolver 或 projection。
- 未验证项：defaults/reset、equipment rebuild 幂等、discount context、coin pickup range、luck deterministic calculation、Dead Cells Buff timing、ItemReference slot reuse 和 projection one-way。
- 依赖影响：C07、C13/C14/C20、P09 Item/Equipment、StatusEffects/Buff、Liquid/Movement、World economy、Network/Presentation；未迁移旧 readers，未发生双写。

当前实际进度：C21 源码已保存；C07-C21 的可独立组件实现均已结束，下一步为受约束检查、文档最终同步和 runner 失败结算。`completedComponents` 应追加 `C21.PlayerLuckAndCommerceEffects-source`，`pendingComponents` 保持 C09 presentation/Town handoff、C22 integrations；本 checkpoint 不宣称 focused verifier 或编译验证通过。

## 6.1 C01 实际实现 checkpoint：PlayerStringAndAccessoryEffectState

本次已按 C01 执行顺序创建并保存以下根 `src` 文件：

- `src/Player/PlayerAccessoryStringEffectComponent.cs`
- `src/Player/PlayerAccessoryEffectRebuildInput.cs`
- `src/Player/PlayerAccessoryEffectRebuildSystem.cs`
- `src/Player/PlayerAccessoryCapabilitySnapshot.cs`
- `src/Player/PlayerAccessoryCapabilityQuery.cs`
- `src/PlayerAccessoryVerification/Program.cs`
- `src/PlayerAccessoryVerification/Terraria.PlayerAccessoryVerification.csproj`

唯一 writer 是 `PlayerAccessoryEffectRebuildSystem`。它接收不可变的能力输入，保存 14 个 C01 字段，执行 `extraAccessorySlots` 的模式派生、`rapidAttackBonus` 的 `0.005f` tick 衰减、transient accessory reset、`stressBall` current/previous 边沿记忆和 Version4 两阶段 `tankPet` reset。`PlayerAccessoryCapabilityQuery` 只生成快照和 counterweight/yoyo 资格计算，不写 Component；输入和 Component 都不持有 `Terraria.Item` 或 Projectile 对象。

当前 checkpoint 状态：源码已保存；focused build/verifier 尚未运行；`completedComponents` 暂不加入 C01，直到验证命令真实通过。P09/P05/P15/P11 的跨域 owner、运行时注册、网络/持久化和玩家生命周期接入仍未实现，未创建外部 adapter 或双写 writer。

## 27. C22 执行计划：PlayerDpsTelemetryState

源边界：`D:\TRbackup\Version4\Terraria\Player.cs:2018-2026、:7000-7004、:11971-11975、:26241-26260、:26417-26418、:26455-26458`；`D:\TRbackup\Version4\Terraria\Projectile.cs:12516-12519`。

计划目标文件：

- `dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerDpsTelemetryComponent.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Systems/PlayerDpsTelemetrySystem.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Queries/PlayerDpsTelemetryQuery.cs`
- `dome/src/Terraria.Dome.Simulation/Player/Combat/Ports/IPlayerTelemetryClock.cs`
- `dome/src/Terraria.Dome.Server/Telemetry/PlayerDpsTelemetryProjection.cs`

本次实际目标文件：

- `src/Player/PlayerDpsTelemetryComponent.cs`
- `src/Player/PlayerCommittedDamageEvent.cs`
- `src/Player/IPlayerTelemetryClock.cs`
- `src/Player/IPlayerDpsTelemetryCommitPort.cs`
- `src/Player/IPlayerDpsTelemetryQuery.cs`
- `src/Player/PlayerDpsTelemetrySystem.cs`
- `src/Player/PlayerDpsTelemetryQuery.cs`
- `src/Player/PlayerDpsTelemetrySnapshot.cs`
- `src/PlayerDpsTelemetryVerification/Program.cs`
- `src/PlayerDpsTelemetryVerification/Terraria.PlayerDpsTelemetryVerification.csproj`

实施顺序：

1. 已建立 5 字段 window state、clock port、committed damage event contract、EventId 去重/replay policy 和 reset verifier；Component/Query 不调用 `DateTime.Now`。
2. 尚未接收真实 NPC/Projectile 已提交伤害事实；当前 verifier 使用本地构造的 committed event contract，未宣称 P12/P15 source 已接入。
3. 本地唯一 `PlayerDpsTelemetrySystem` 已实现窗口启动、累加、last-hit/end 更新、显式 `ReconcileCapability(false)` 停止 seam 和 reset；停止先读取 clock 再关闭窗口，`accDreamCatcher`、死亡/重生/断线调用以及旧 `addDPS` 兼容切换仍未执行，禁止新旧 writer 并行。
4. `PlayerDpsTelemetryQuery` 已计算 duration/DPS snapshot，并处理零时长、负时间、负伤害拒绝和 `int` 饱和；Projection 尚未创建，不向 UI/network/diagnostics 输出。
5. 本地 committed-only、window lifecycle、clock injection、duplicate/replay、reset、capability removal、停止提交顺序、纯 Query 和 no-authority-feedback verifier 已通过；真实跨分区事件 source、lifecycle、projection 和网络/持久化仍等待 integration-review。

依赖影响：C02/C03/C09/C10/C19、P04 lifecycle、P12 NPC combat、P15 Projectile、P09 equipment capability、Network/ClientPresentation/Diagnostics、clock and persistence adapters。回滚时移除新 telemetry event adapter/system registration，恢复旧 `addDPS` reader/writer；clock、event owner 或 DTO 未裁决时保持 planned/not-started。

验收结果：5 个成员逐条归属；时间访问隔离；本地 only-positive committed damage 计入；EventId 去重/replay、reset、capability removal、停止提交顺序、早于窗口开始时的非负时长边界、Query 不修改组件字段、纯 Query 和 telemetry 不写战斗 authority 已通过 focused verifier。`accDreamCatcher`、death/respawn/disconnect、真实 NPC/Projectile source、projection、网络和持久化尚未验证。

检查点结论：`C22.PlayerDpsTelemetryCore` 已创建并保存；focused verifier 已通过。执行状态为 `executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: focused-c22-passed`；C22 外部 integration 和 C01-C21 仍 pending。

## 28. 全分区实施与交接计划

### 28.1 实施前门槛

本分区 22 个检查点均已完成设计记录；C07-C21 的可独立组件边界已在未越过外部 owner 的范围内实施，C09 仍缺 presentation/Town handoff，C22 仍缺外部 integration。剩余工作必须取得 integration-review 对唯一 owner、输入/输出 DTO、生命周期、系统调度、网络和持久化边界的确认。关键 owner 未确认时，剩余部分保持 planned/not-started。

### 28.2 建议实施批次

1. C22 本地 core 已建立成员覆盖、唯一 writer、纯 Query 和 verifier；后续组件仍应先建立对应 verifier，不创建未获批准的生产组件。
2. 以 C10 vital/regen、C03/C09 immunity、C15 Buff aliases 为基础，先冻结 authority snapshot 和 committed event contract，避免既有 `HealthComponent`/`ManaComponent` 与提案组件双写。
3. 迁移 C01、C12、C16、C17、C20、C21 的装备/能力读取为 typed definition/reference adapters，再迁移 C11/C14/C18 的 modifier、environment、drop 输入；Item/Buff/World owner 保持外部 mutation。
4. 迁移 C02/C19 的 proc、damage/crit calculation Query，接入 P12/P15 的 committed damage event；C22 本地 contract 已固定，但真实事件 ID、source revision、去重、replay 和唯一消费路径仍必须由 integration-review 确认。
5. 最后按显式 scheduler phases 切换 readers/projections，逐批执行兼容窗口和回滚演练；任何行为偏差恢复旧 reader，撤销本批新 writer，不以双写掩盖。

### 28.3 全局接口方向

- `Component -> read-only state`: 组件只暴露不可变 snapshot/read view。
- `Input/Network/World -> Command`: 外部输入先转换为带 ID/revision 的意图。
- `Command + Query -> System/CommitPort`: System 验证并提交唯一 authority。
- `Committed Event -> Adapter/Projection`: 事件通过边界输出到 Network/Persistence/Presentation/Telemetry。
- `Projection/Adapter -X-> Authority`: 输出层禁止反向写组件、Buff、Item、World 或生命状态。

### 28.4 验证记录要求

每个批次记录 source/target path、dependency impact、唯一 writer、输入 revision、回滚点、命令/事件 trace、warning/error 计数和输出路径。本次已按根目录 `AGENTS.md` 通过 `Invoke-SerialDotnet.ps1` 串行验证 C22 本地 core：Build 项目为 `src/PlayerDpsTelemetryVerification/Terraria.PlayerDpsTelemetryVerification.csproj`，命令参数为 `build <project> -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`，退出码 0，0 警告、0 个错误，产物位于 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 和 `Build/bin/Terraria.PlayerDpsTelemetryVerification/Debug/net10.0/Terraria.PlayerDpsTelemetryVerification.dll`；focused run 命令参数为 `run --project <project> --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`，退出码 0，输出 `PASS: player DPS telemetry committed-event lifecycle and pure query`，包含 stop clock ordering、capability removal 和 reset replay-set 清理断言。

### 28.5 Network and persistence handoff

Network 只接收版本化、只读 snapshot；客户端 command 必须重新验证，不能把客户端 projection 当 authority。Persistence 由独立 owner 定义 schema、版本迁移、默认值、异常和回滚。由于 Version4 `Serialize`/`Deserialize` 当前为空，存档 compatibility 处于 `persistence-blocked`，不得在实施计划中伪造 wire field order 或宣称闭合。

### 28.6 Side-effect matrix

| 效果 | 允许入口 | P06 禁止事项 |
|---|---|---|
| clock | `IPlayerTelemetryClock` | Component/Query 读取系统时间 |
| randomness | explicit RandomSource/command | Query 推进 RNG 或直接掉落 |
| Item/Equipment | typed reference/read adapter | 复制可变 Item、改 stack/inventory |
| Buff | status snapshot/timing policy port | 第二 timer、绕过 immunity/expiry |
| World/Liquid/Movement | immutable input/permission result | 改位置、速度、重力、Tile、coin/drop entity |
| logging | diagnostic adapter | 以日志副作用改变 authority |
| network | versioned projection/validated command | 客户端快照反写服务器状态 |
| persistence | owner DTO adapter | 猜测空 Serialize/Deserialize 的格式 |
| presentation | one-way projection | UI/alpha/frame 回写 combat authority |

### 28.7 最终 handoff decisions

- 明确 C10 与现有生命/魔力组件的唯一 writer 和 death commit root。
- 明确 C03/C09/C17 的 eligibility、hurt cooldown、immune frame 与 presentation frame/alpha 分层。
- 明确 StatusEffects 对 C06/C15/C18 alias、Buff type/time/immunity 和 expiry 的事务 owner。
- 明确 P09 对 ItemReference、equipment enumeration、slot reuse、drop/coin mutation 的 resolver 和 commit owner。
- 明确 Liquid/Movement/WorldInteraction 对 C13/C14/C20/C21 的 wet/shimmer/gravity/speed/utility/permission 输入。
- 明确 P12/P15 committed damage event 的 ID、去重、replay 和 C22 唯一消费路径。
- 明确 Network/Persistence 的 DTO version、ID 类型、默认值、SerializedClone 语义，以及 `Serialize`/`Deserialize` 补证计划；当前 persistence 保持 blocked。

本文件现在同时包含计划和 C07-C21 组件源码的实施记录。C22 本地 core 与 focused verifier 项目、C01-C05 的既有验证记录保持不变；未创建网络协议、存档格式、运行时注册或外部 event adapter。整体状态为 `executionStatus: failed`、`implementationStatus: partial`、`verificationStatus: source-only-c06-c21-not-verified`，未实现/未验证内容不能写成完成；runner 已按原始 manual session 记录 Fail。

## 29.10 本次组件源码构建记录

- 命令：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Player\Terraria.Player.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`
- 项目：`src/Player/Terraria.Player.csproj`，目标框架 `net10.0`；退出码 `1`；`0` 个警告、`5` 个错误。
- 真实错误：既有 `src/Player/Progression/PlayerMinionCapacityCommitSystem.cs` 的 `Terraria.Relationships` 引用，以及既有 `src/Player/Progression/SubmitMinionCapacityDeltaCommand.cs` 的 `Terraria.Projectile`、`Terraria.Relationships`、`EntityReference` 和 `ProjectileIdentityComponent` 引用未在当前项目提供。
- 预期产物路径：`Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`；该文件存在但时间戳早于本次失败构建，未被本次构建刷新，不能作为成功编译证据。
- C07-C21 组件源码未新增上述非组件错误；没有再添加测试、verifier、System、Query、Command、Adapter、Projection、项目文件或构建脚本来绕过失败。
