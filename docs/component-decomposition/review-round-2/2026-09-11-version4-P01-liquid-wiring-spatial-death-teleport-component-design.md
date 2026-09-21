# Version4 P01 液体、机关、空间、死亡惩罚与传送组件拆分设计

partitionId: P01
sessionId: e925fb5b47e04306b42135b5e195a463
designSessionId: 9aa6a84235684669974dc8b346602627
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P01-Liquid-Wiring-Spatial-Death-Teleport.md
designPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md
executionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-execution.md
executionStatus: in-progress
implementationStatus: partial
verificationStatus: c01-budget-system-commit-focused-passed-30-assertions; c02-queue-seam-focused-passed-40-assertions; c03-buffer-seam-focused-passed-36-assertions; c04-publication-seam-focused-passed-23-assertions-plus-delivery-failure-9-assertions; c05-wiring-propagation-focused-passed-38-assertions; focused-c10-c14-plus-c11-capture-discouragement-presentation-clock-passed; c06-scratch-focused-passed-40-assertions; c06-pump-command-focused-passed-14-assertions; c06-traversal-focused-passed-17-assertions; c06-final-cleanup-patch-build-and-focused-passed; c07-invariant-correction-green-passed-22-assertions-serial-build-0-warning-0-error; c10-lifecycle-expiration-invariant-green-passed-13-assertions-serial-build-0-warning-0-error; c10-c14-components-serial-rebuild-passed-0-errors-0-warnings; p01-component-source-audit-and-four-serial-builds-passed-0-warning-0-error; c14-pylon-component-build-and-focused-verifier-passed-0-errors-15-existing-warnings-6-assertions; c13-registry-immutable-view-green-build-and-focused-verifier-passed-0-errors-0-warnings-12-assertions; p01-component-contract-audit-focused-passed-26-assertions; c14-pylon-snapshot-isolation-focused-passed-10-assertions-0-errors-15-existing-warnings; p01-component-boundary-verifier-passed-15-assertions-serial-build-0-warning-0-error; p01-component-boundary-audit-rebuild-4-projects-passed-0-warning-0-error; c13-registry-duplicate-rejection-green-passed-14-assertions-serial-build-0-warning-0-error; c14-refresh-cooldown-invariant-not-verified-no-tests-requested; c10-c14-static-owner-audit-no-tests-requested; c10-c14-static-state-review-2026-09-13-no-tests-requested; c06-pump-scratch-count-invariant-not-verified-no-tests-requested
evidenceStatus: partial
completedComponents: []
partialComponents: [C01, C02, C03, C04, C05, C06, C07, C10, C11, C12, C13, C14]
blockedComponents: [C08, C09]
currentComponent: C06
pendingComponents: [C10, C11, C12, C13, C14]
lastCheckpointUtc: 2026-09-12T16:59:05.1770401Z
evidence-gap-c06-initialize-teleport-reset: 不写入仓库的 reflection RED 探针在当前组件构建产物上确认，`WirePropagationScratchComponent` 已记录 teleport target 后调用 `InitializePropagation()` 会残留 1 个 target；one-iteration block setter 由组件边界控制，探针未伪造外部写入。已在 `src/WorldInteraction/Wiring/WirePropagationScratchComponent.cs` 让 `InitializePropagation()` 和 `BeginPropagation()` 调用组件内 `ResetTeleportState()`，使 C05/C06 共享 scratch 的初始化和新 propagation 开始都清理 C06 瞬态状态。随后通过仓库 wrapper 串行构建 `src/WorldInteraction/Terraria.WorldInteraction.csproj`，退出码 `0`、`0` warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`；不写入仓库的 reflection GREEN verifier 输出 `C06_INITIALIZE_TELEPORT_GREEN=PASS ASSERTIONS=6`，退出码 `0`，覆盖 `InitializePropagation()` 和 `BeginPropagation()` 两条清理路径。该修订仍只覆盖组件内部生命周期。
blocking-decision-c06-initialize-teleport-reset: 该修订仅改变 C05/C06 共享 scratch 的组件内初始化清理，不接入旧 `XferWater`/`Teleport`、Player/NPC position、Tile、Liquid、network 或外部 owner；保留 C06 adapter 的成功/拒绝/异常路径清理。若 GREEN verifier 通过，C06 仍保持 `partial`，不得因此宣称跨域生命周期或运行时迁移完成。
verificationStatus-c06-initialize-teleport-reset: c06-initialize-teleport-reset-red-confirmed; serial-build-0-warning-0-error; c06-initialize-teleport-green-passed-6-assertions
evidence-gap-c10-lifecycle-expiration-invariant: C10 RED reflection probe 先以退出码 `1` 确认 `RevengeMarkerLifecycleComponent` 接受 `ExpiresAtGameTime=-1`；已在 `src/DeathPenaltyAndRevenge/RevengeMarkerLifecycleComponent.cs` 加入组件构造时的非负 deadline 校验。随后通过 `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\DeathPenaltyAndRevenge\Terraria.DeathPenaltyAndRevenge.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"` 串行构建，退出码 `0`、`0` warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`；不写入仓库的 GREEN reflection verifier 输出 `C10_LIFECYCLE_EXPIRATION_GREEN=PASS ASSERTIONS=13`，退出码 `0`。该修订只约束生命周期组件字段，不实现 C10 allocator、legacy marker writer、registry、clock、network 或 persistence。
blocking-decision-c10-lifecycle-expiration-invariant: 只允许在 `RevengeMarkerLifecycleComponent` 内校验绝对 expiration tick；不得把 C12 `ForceExpire`/attempt lock、C13 clock/registry 或 legacy `RevengeMarker` writer 接入 C10。串行 build 与 focused GREEN verifier 已通过，但 C10 继续保持 `partial`、`completedComponents: []`，不调用 runner 结算，直到 integration-review 裁决 allocator/lifecycle、旧 marker writer、registry、network 和 persistence owner。
evidence-gap-c11-component-contract-audit: 本 checkpoint 对 `RevengeTargetSnapshotComponent` 及其 `NpcNetId`、`NpcTypeId`、`WorldPosition` 和 value substrate 执行了只读来源审计。Version4 `CacheEnemy` 在 capture 前要求映射后的 spawn net ID 非零，但其 `RevengeMarker` 构造器仍接受负 net ID；负值由 `SpawnEnemy` 的特殊路径消费，因此不能在 C11 快照中统一拒绝负 `NpcNetId`。Version4 将原始 `npc.type` 保留给 discouragement，将映射后的 `npc.netID` 用于 spawn；当前 `NpcTypeId.IsValid` 只能表达通用 ID 形状，尚无足够证据证明快照公共构造器必须拒绝零或负原始 type。`GetLifePercent()` 是原始 `statLife / statLifeMax`，`SpawnEnemy` 才应用 `Math.Max(0.5f, ...)`；因此 C11 必须保留原始 life ratio，不能在构造器中 clamp、拒绝非有限值或替 C12 处理下限。`baseValue` 仅已有 finite 校验；`coinsValue` 和 `WorldPosition` 的更强边界未由当前 Version4 来源明确规定。没有源码变更；现有 C11 capture qualification/discouragement focused evidence 继续有效，但 NPC owner、capture commit、映射时序和完整行为等价仍未闭合。
blocking-decision-c11-component-contract-audit: 不新增 C11 构造校验或第二份值对象。`SpawnNpcNetId == 0` 继续由 `RevengeEnemyContextPolicy.EvaluateCapture` 在显式 capture input 边界拒绝；负 spawn net ID、原始 NPC type、life ratio、coins value 和位置边界留给已确认的 NPC capture/respawn owner 裁决。不得把 C12 的 HP floor 前移到 C11，不得从当前 NPC slot 回读快照上下文，也不得因纯 Query/build 通过而标记 C11 completed；`completedComponents` 保持空数组。
evidence-gap-c12-component-contract-audit: 本 checkpoint 对 `RevengeMarkerValueComponent`、`RevengeRespawnAttemptComponent` 和 `RevengeRespawnSystem` 执行了只读来源审计。Version4 `NPC.extraValue` 默认归零，死亡路径只有 `extraValue > 0` 才调用 `CacheEnemy`，而 `CacheEnemy` 进一步要求 `extraValue >= MinimumCoinsForCaching`（1000）后才构造 marker；但 Version4 `RevengeMarker` 构造器和读取/恢复路径没有对 `_coinsValue` 或 `_baseValue` 做构造时范围校验，组件也可能作为兼容 substrate 被恢复或单元化创建。当前 `RevengeMarkerValueComponent` 仅拒绝非有限 `baseValue`，没有足够证据把负值、最大值或 coins 的捕获阈值提升为组件构造器的通用不变量。`ForceExpire` 在旧实现中只能置 true，`_attemptedRespawn` 则在离开所有 outer box 后置 false；当前 attempt component 的 internal setter 和 `RevengeRespawnSystem` 的决策顺序保持这两个不同生命周期。没有源码变更；C12 纯 decision/attempt 的既有 focused evidence 继续有效，但 NPC spawn commit、同 tick arbitration、失败重试、marker remove projection 以及 C10/C11/C13 唯一 substrate 仍未闭合。
blocking-decision-c12-component-contract-audit: 不在 `RevengeMarkerValueComponent` 中新增未经恢复/网络证据支持的 coins 或 base value 范围限制，不新增第二份 value/attempt/lifecycle owner，也不把 spawn side effect、HP floor、`timeLeft`、network 或 marker 删除写入组件。保留 finite `BaseValue` 校验、只读 value projection、单向 force-expire 和显式 attempt unlock；待 NPC/registry integration review 裁决 capture/restore contract 后，再决定是否需要更窄的输入类型或边界校验。C12 保持 `partial`，`completedComponents` 保持空数组。
evidence-gap-c10-c14-final-component-boundary-audit: 本 checkpoint 汇总复核 C10-C14 现有组件，未发现可以在当前“仅组件源码”授权和已确认 Version4 证据内继续安全修订的状态缺陷。C10 的 deadline 校验、C11 的 immutable context/ID 分离、C12 的 finite value 与 attempt 生命周期、C13 的 immutable ID view/clock substrate、C14 的 pylon entry 与 current/previous snapshot isolation 均已有 focused contract evidence；剩余缺口全部需要跨组件唯一 owner 或外部副作用整合，包括 legacy marker writer/allocator、NPC capture/spawn、registry payload、TileEntity/SceneMetrics、WorldStorage、network/persistence、自然 spawn arbitration 和 C08/C09 owner 决议。没有源码变更。
blocking-decision-c10-c14-final-component-boundary-audit: 不为 C10-C14 添加新的重复 validation、registry、clock、lifecycle、NPC/TileEntity 引用或外部写入。保持 `completedComponents: []` 和各组件 `partial`；不得用组件 focused verifier 或项目 build 通过替代 runtime owner、行为等价、网络和存档证据。P01 只能在 integration review 给出唯一写者、提交顺序、失败/重试和持久化/网络协议后继续接入。
evidence-c10-c14-boundary-verifier: 确认 `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll` 及其 `Terraria.Items`、`Terraria.Npc`、`Terraria.Relationships` 依赖产物存在后，不写入仓库的 reflection verifier 输出 `P01_COMPONENT_BOUNDARY_VERIFIER=PASS ASSERTIONS=15`，退出码 `0`。验证覆盖 C11 负变体 spawn net ID 保留、spawn/discouragement ID 分离、原始 life ratio（包括非有限输入）不被 C11 改写、enemy hitbox 快照几何、C12 value finite 校验/attempt force-expire 应用和 C10 负 deadline 拒绝。首次探针因只加载主 DLL 而无法解析外部值类型，未触及业务断言；加载四个实际构建产物后重跑通过。
verificationStatus-c10-c14-boundary-verifier: `Terraria.DeathPenaltyAndRevenge.csproj` 通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行 build，退出码 `0`、`0` warning、`0` error，产物确认在 `Build/bin/`；focused boundary verifier 退出码 `0`，输出 `P01_COMPONENT_BOUNDARY_VERIFIER=PASS ASSERTIONS=15`。该证据只覆盖组件公开契约，不覆盖 NPC capture/spawn writer、自然 spawn arbitration、marker registry payload、TileEntity、network、persistence 或跨域行为等价；C10-C14 继续为 `partial`。
evidence-gap-c07-invariant-correction: C07 RED reflection probe 在当前 `Terraria.WorldInteraction` DLL 上以退出码 `1` 确认 5 项状态不变量缺陷；已修改 `src/WorldInteraction/Wiring/WiringMechanismScheduleComponent.cs` 和 `src/WorldInteraction/Wiring/WiringDeviceCooldownComponent.cs`，加入组件本地 bounded setter 校验。随后通过 `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\WorldInteraction\Terraria.WorldInteraction.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"` 串行构建，退出码 `0`、`0` warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`；不写入仓库的 GREEN reflection verifier 输出 `C07_COMPONENT_INVARIANT_GREEN=PASS ASSERTIONS=22`，退出码 `0`。旧 `C07_COMPONENT_VERIFIER=PASS ASSERTIONS=13` 仍仅代表修订前基线。
blocking-decision-c07-invariant-correction: 当前仍只允许组件字段、默认值、Reset 和 setter invariant 修订；不得添加 schedule deduplication、tick/expiry、cooldown decision、Tile frame、network、mechanism effect 或旧数组 writer。串行 build 与 focused GREEN verifier 已通过，但 C07 仍保持 `partial`、`completedComponents: []`，不调用 runner 结算，直到 integration-review 裁决旧机制数组、Tile frame、network 和机制效果的唯一 owner。
evidence-gap: C01 policy/query/state/system/commit shadow substrate 已通过 30 项 focused 断言和串行项目构建，但旧 Liquid writer、Tile/liquid authority、C02/C03 交接和跨世界生命周期仍未闭合；C02 command/query/queue seam 已通过 40 项 focused 断言和串行项目构建，但仍不接管旧 `Main.liquid` 数组；C03 buffer queue、command、commit port 和 C02 drain system 已通过 36 项 focused 断言和串行项目构建，但仍不接管旧 `LiquidBuffer` writer；C03 只返回 Tile `checkingLiquid` set/clear intent，不直接写 Tile，C02/C03 capacity handoff、C02 拒绝后的重试和 Reset 时的 checking flag 清理仍需 integration review；C04 publication snapshot/projection/network adapter 源码已保存但尚未构建或运行 focused verifier，仍未接管旧 `_netChangeSet` writer、WorldGen/loading 外部调用点、NetLiquidModule 或真实连接 owner；C04 的 pending/publishing revision 恢复和发送确认依赖 integration review；C11 capture qualification 纯 Query 已通过 18 项 focused 断言，discouragement outer Query 已通过 20 项 focused 断言，C13 presentation policy 已通过 2 项 focused 断言，RevengeClockState 已通过 5 项 focused 断言；style 2/3 仍依赖 NPC owner 提供的显式结果，C11 capture/discouragement 尚未接入 NPC owner；C10 allocator 尚未接入旧 marker writer，C12 的 NPC spawn arbitration/retry/remove commit 仍未闭合，C13 registry/network/persistence owner 未裁决，C14 Pylon registry/TileEntity/WorldStorage/Dome owner、equality/diff 和 projection 未裁决；C06 已扩展既有 C05/C06 shared scratch owner，完成有界 pump 坐标去重、计数和 reset substrate，但尚未完成 command/adapter/port、半砖 Vector2 endpoint 表达或 focused 验证；C05-C09 仍受目标项目/owner 边界限制。没有唯一 owner、生命周期、存档或网络证据的边界不得继续猜测；分区整体仍未完成行为等价验证。
evidence-gap-c07: C07 仅保存 `src/WorldInteraction/Wiring/WiringMechanismScheduleComponent.cs` 和 `WiringDeviceCooldownComponent.cs` 两个 bounded component；尚未创建 `WiringMechanismSchedulePolicy`、`HopperInteractionPolicy`、`MechanismCooldownSystem` 或 `MechanismActivationQuery`，尚未接管旧 `_mechX/_mechY/_mechTime`、`_numMechs`、`CheckMech`、`UpdateMech`、Tile frame、network 或机制效果；`Terraria.WorldInteraction.csproj` 已通过仓库串行 build，退出码 `0`、`0` errors、`15` 个既有 `WorldStorage` warnings，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`；不写入仓库的 focused reflection verifier 输出 `C07_COMPONENT_VERIFIER=PASS ASSERTIONS=13`，覆盖固定容量、默认值、只读条目视图和组件 Reset。
evidence-gap-c08: C08 的设计最终将 `CollisionQueryScratch` 定义为调用期间的 query-owned scratch，而不是可附加的长期 ECS component；其 `contacts` 依赖当前 `src` 中不存在的 `TileContact` 类型，conveyor cache 依赖未建立的 `Point` 值对象；`src/Physics/CollisionResultComponent.cs` 与 `src/SpatialSimulation/CollisionResultComponent.cs` 仍存在 owner 冲突。按本次只允许组件源码的范围，不创建 `CollisionQueryScratchComponent`、值对象、Query、Definition 或 Projection；C08 没有新增源码或 build 证据。
blocking-decision: partial-implementation-blocked；C01 只保留已验证的 shadow substrate，不修改现有 LiquidWorldRuntimeStateComponent、legacy Liquid writer 或其他会话的 Liquid owner；C02 起先实现不改变现有 owner 的纯 command/query 和显式提交 seam；C06 的 shadow traversal seam 已完成最终 cleanup patch 验证，但在 integration-review 裁决唯一 Liquid/Teleportation/entity owner、half-brick endpoint 表达和生命周期前不得接入旧 XferWater/Teleport 或提交实体/Tile/network 副作用；保留旧 RevengeMarker 为运行时唯一 writer，不创建第二个 allocator、registry 或重叠 lifecycle/value/context 状态；C07 仅保存 bounded schedule component，不接入旧 `_mechX/_mechY/_mechTime` writer、`CheckMech`、`UpdateMech`、Tile frame、network 或机制效果；待 integration-review 裁决后才能接入 C01/C06/C07/C10-C14 的运行时迁移。
blocking-decision-c08: C08 不得把 query-owned scratch 伪装为长期 `CollisionQueryScratchComponent`；在 `TileContact`/conveyor point 值对象、query invocation lifecycle 和 Physics/Spatial result owner 决议前，不创建第三个 collision owner，不修改两个既有 `CollisionResultComponent`，也不接入 Tile、entity、network 或 persistence writer。
evidence-gap-c09: C09 设计中的 `TileContactResult`、`HurtTileResult` 和 `CollisionContactQuery` 都是一次查询的不可变结果/Query 类型，不是 ECS component；当前 `src` 没有可复用的 `TileContact` 或 `HurtTile` 值对象，且 Physics/Spatial 的两个 `CollisionResultComponent` owner 尚未裁决。按本次只允许组件源码的范围，不新增 C09 文件，不修改现有 result component，也不实现 Tile/Player/NPC/network/damage 行为。
blocking-decision-c09: C09 必须等待 C08 query-owned scratch、TileContact/HurtTile 值对象和 Physics/Spatial result owner integration review；不得创建第三个 `CollisionResultComponent`、把结果列表塞入现有组件，或把 Query/伤害/Tile 行为写进组件。
evidence-gap-c10-c14-components: C10-C14 的现有组件源码已完成本轮受影响项目串行重建：`Terraria.DeathPenaltyAndRevenge.csproj` 退出码 `0`、`0` warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`；`Terraria.Teleportation.csproj` 退出码 `0`、`0` warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.Teleportation\Debug\net10.0\Terraria.Teleportation.dll`。C10 legacy allocator/lifecycle、C11 NPC capture owner、C12 spawn arbitration/retry、C13 registry/clock/network/persistence 和 C14 Pylon/TileEntity/WorldStorage/Dome owner 仍未闭合。
evidence-gap-c14-component-lifecycle: `PylonRegistryComponent` 现在保存 current/previous 快照、refresh cooldown 和 revision，并提供纯组件内的 replace/advance/reset 生命周期；`Replace` 会在清理内部列表前复制输入，避免调用者传入 `CurrentPylons` 只读视图时发生别名清空。`Terraria.WorldStorage.csproj` 已通过仓库 wrapper 串行 build，退出码 `0`、`0` error、`15` 个既有 warning，且不写入仓库的只读契约验证输出 `C14_PYLON_COMPONENT_VERIFIER=PASS ASSERTIONS=6`、别名修复回归输出 `C14_PYLON_ALIAS_FIX_VERIFIER=PASS ASSERTIONS=16`。TileEntity 扫描、SceneMetrics、equality/diff、network/persistence 和唯一 registry owner 仍未闭合。
evidence-gap-c13-registry-view: `RevengeMarkerRegistryComponent` 仍只保存 assigned marker ID index；本单元把公开 `MarkerIds` 改为 mutation-resistant frozen snapshot，并在内部 register/unregister/clear 后刷新。`Terraria.DeathPenaltyAndRevenge.csproj` 已通过 `Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 串行构建，退出码 `0`、`0` warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`；不写入仓库的 focused reflection verifier 输出 `C13_REGISTRY_VERIFIER=PASS ASSERTIONS=12`。完整 marker payload registry、legacy `_markers` owner、clock、network/persistence 和 commit boundary 仍未裁决。
blocking-decision-c13-registry-view: frozen ID view 只限制外部绕过组件边界修改 ID index，不声明 registry owner 已切换，也不复制 marker payload 或 legacy list；所有 register/remove 的最终写者仍需 integration review。
blocking-decision-c14-component-lifecycle: 组件只复制显式 entry 列表并改变自身 revision/cooldown；不得在组件内读取 TileEntity、SceneMetrics、网络或存档，也不得据此选择 `WorldStorage`、`Teleportation` 或 Dome 的最终 owner。
evidence-gap-c10-c14-component-contract-audit: 本 checkpoint 没有新增或修改 `src/` 文件；已对现有 C10-C14 组件和 C14 `PylonRegistryEntry`/`PylonRegistryComponent` 执行只读反射契约审计，输出 `P01_COMPONENT_CONTRACT_AUDIT=PASS ASSERTIONS=26`。`Terraria.DeathPenaltyAndRevenge.csproj`、`Terraria.WorldStorage.csproj` 和 `Terraria.Teleportation.csproj` 均经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建，退出码 `0`、`0` warning、`0` error，产物分别位于 `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`、`D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll` 和 `D:\TRbackup\NLTX\Build\bin\Terraria.Teleportation\Debug\net10.0\Terraria.Teleportation.dll`。审计覆盖 identity/lifecycle/value/attempt/context、registry immutable view、pylon entry validity、current/previous snapshot、cooldown、revision、reset 和 alias-safe replacement；不覆盖 NPC spawn、旧 marker writer、TileEntity scan、SceneMetrics、network 或 persistence。
blocking-decision-c10-c14-component-contract-audit: 当前没有新的组件级状态缺陷可安全修改。不得为 C10/C12 增加未经 owner 裁决的重复 lifecycle/validation/reset，不得把 C13 ID index 扩展为完整 marker registry，不得在 C14 组件中接入 TileEntity/SceneMetrics/network/persistence；C10-C14 继续保持 `partial`，`completedComponents` 继续为空。
evidence-gap-c14-pylon-snapshot-isolation-correction: `PylonRegistryComponent` 的旧 `CurrentPylons`/`PreviousPylons` 只读视图此前仍绑定到会被后续 `Replace`/`Reset` 清空的内部列表，不能满足设计要求的 immutable ordered snapshot。已在组件内改为替换 list 实例并为每个实例建立新的 `ReadOnlyCollection` view；`Replace` 仍先复制输入，因此传入 current view 的 alias-safe 行为不变。通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建 `Terraria.WorldStorage.csproj`，退出码 `0`、`15` 个既有 warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`；不写入仓库的 focused verifier 输出 `C14_PYLON_SNAPSHOT_ISOLATION_VERIFIER=PASS ASSERTIONS=10`，覆盖替换、alias 输入、连续替换和 reset 后旧 current/previous view 的稳定性。
blocking-decision-c14-pylon-snapshot-isolation-correction: 修订仅改变 C14 组件的内部快照容器生命周期，不读取 TileEntity、SceneMetrics、网络或存档，也不选择最终 registry owner；修订已验证，但 C14 仍不能标记 completed，C10-C14 继续保持 `partial`。
evidence-gap-c06-final-cleanup: 最终 cleanup-only 源码修订已通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建 `src/WorldInteraction/Terraria.WorldInteraction.csproj`，退出码 `0`，警告 `0`，错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。不写入仓库的 focused verifier 输出 `C06_SCRATCH_VERIFIER=PASS ASSERTIONS=40`、`C06_PUMP_COMMAND_VERIFIER=PASS ASSERTIONS=14`、`C06_TRAVERSAL_VERIFIER=PASS ASSERTIONS=17`，均退出码 `0`。剩余缺口仍是 half-brick `Vector2` endpoint、真实 Liquid/Teleportation/entity owner、Player/NPC position、Tile frame、network 和跨域生命周期，不得据此宣称运行时迁移完成。
blocking-decision-c06-final-cleanup: C06 shadow scratch、command、port 和 adapter 的状态、顺序、失败观察及成功/失败清理已验证；没有唯一 owner 和半砖 endpoint 证据，继续禁止接入旧 `XferWater`/`Teleport` 或提交 Tile、实体位置、network、cooldown 副作用。
evidence-gap-c06-command: C06 bounded scratch substrate 已通过 `Terraria.WorldInteraction` 串行 build（0 warning/0 error）和 `C06_SCRATCH_VERIFIER=PASS ASSERTIONS=40`；pump command 源码已通过同一项目串行 build（0 warning/0 error）和 `C06_PUMP_COMMAND_VERIFIER=PASS ASSERTIONS=14`，覆盖空输入/输出、非法颜色、相同端点拒绝、2x2 稳定顺序、20x20 命令容量和只读命令列表；teleport command/adapter/port 源码已通过同一项目串行 build（0 warning/0 error）和 `C06_TRAVERSAL_VERIFIER=PASS ASSERTIONS=17`，覆盖 endpoint/subject/color rejection、pump-then-teleport ordering、pump/teleport failure observation and scratch cleanup。half-brick Vector2 endpoint、Liquid owner 交接、实体位置/网络副作用仍未闭合。
implementation-note-c06-traversal: `WiringTeleportCommand` 只接收非负 Tile endpoint、wire color 和非空 Player/Npc `EntityReference`，不表达或伪造 half-brick `Vector2`。`WiringTraversalAdapter` 只调用注入的 `IWiringTraversalCommitPort`，按 pump 后 teleport 顺序提交，并在成功、拒绝或异常路径清理两个 scratch；它不写 Player/NPC 位置、Tile、Liquid、network 或 cooldown。
blocking-decision-c06-traversal: C06 traversal seam 的状态和顺序已由 fake commit port 验证，但真实 Liquid/Teleportation/entity commit owner 未裁决；不得把 `IWiringTraversalCommitPort` 接入旧 `XferWater`、`Teleport`、Tile frame、network 或 entity position writer，直到 integration review 提供唯一 owner 和半砖 endpoint 表达证据。

### 9.33 实施 checkpoint：C06 pump transfer command

- 已保存 `src/WorldInteraction/Wiring/PumpTransferPolicy.cs`、
  `PumpTransferCommand.cs`、`PumpTransferCommandBatch.cs` 和
  `PumpTransferCommandBatchStatus.cs`。该最小单元消费既有 bounded pump scratch，按 input index
  外层、output index 内层生成稳定命令；缺少任一侧 pump 返回明确 empty，非法 wire color、数量或
  source/destination pair 返回 rejection。
- 命令只携带两个 Tile 坐标、输入/输出索引和 wire color；拒绝负坐标、相同坐标和非法 wire color，
  不携带或推断 Tile liquid amount/type。批量列表以只读视图返回，没有 Tile、Liquid、网络或实体
  位置副作用。
- 当前只完成源码保存和预期失败的契约探测；`C06_COMMAND_CONTRACT_RED` 证明实现前类型缺失，
  不是 green 验证。重新 build 和 focused command verifier 通过前，C06 仍为 partial。

### 9.34 实施 checkpoint：C06 teleport command and traversal adapter

- 已保存 `WiringTeleportCommand.cs`、`WiringTraversalCommitStatus.cs`、
  `WiringTraversalCommitResult.cs`、`IWiringTraversalCommitPort.cs`、
  `WiringTraversalFlushStatus.cs`、`WiringTraversalFlushResult.cs` 和
  `WiringTraversalAdapter.cs`。适配器的提交顺序为 pump 后 teleport；提交失败停止后续提交，
  `finally` 清理 pump scratch 与 teleport endpoint/block flag。
- `WiringTeleportCommand` 只接收非负 Tile endpoint、wire color 和非空 Player/Npc
  `EntityReference`，不表达或伪造 half-brick `Vector2`，也不拥有实体位置、portal state、network
  或 cooldown。源码保存后发现并修正了异常路径提前返回及 `in` 参数传递问题；当前尚未重新 build。

### 9.35 验证 checkpoint：C06 teleport command and traversal adapter

- `src/WorldInteraction/Terraria.WorldInteraction.csproj` 已通过 `Build/Tools/Invoke-SerialDotnet.ps1`
  串行 build，退出码 `0`，`0` warning，`0` error；产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。
- 不写入仓库的 PowerShell reflection/Reflection.Emit focused verifier 输出
  `C06_TRAVERSAL_VERIFIER=PASS ASSERTIONS=17`，退出码 `0`。覆盖 valid command、same endpoint、
  unsupported subject、invalid wire color、pump-then-teleport order、pump failure short-circuit、
  teleport failure observation，以及 success/failure 后 pump 与 teleport scratch cleanup。
- C06 保持 `partial`：验证的是 shadow commit seam；没有真实 Liquid writer、Teleportation/entity owner、
  half-brick `Vector2` endpoint、Player/NPC position、Tile frame 或 network parity 证据。

## 1. 设计范围与结论

本文件是 P01 的候选组件、系统、查询、命令、适配器和投影设计，并在实施 checkpoint 中记录本次已获准的最小代码单元。它不是运行时迁移完成证明，不宣称公共 API 兼容、网络闭合、持久化闭合或行为等价已经成立。未接入类型仍为 `status: proposed`；本会话只修改 P01 授权的 `src/` 实现和两份 P01 文档，不修改测试、权威报告、其他会话文档或 ledger。

P01 的权威报告包含 5 个正式父级、14 个叶子子系统、111 个字段和 2 个属性，共 113 个成员。正式父级边界必须继续分别保留：

| 正式父级 | 本分区叶子子系统 | 成员数 | 边界决策 |
|---|---|---:|---|
| `LiquidSimulation` | `LiquidFlowBudgetAndPanicState`、`LiquidCellWorkItemState`、`LiquidBufferQueueState`、`LiquidChangePublication` | 24 | 液体世界事实、工作队列和网络发布分开；不得把网络集合变成液体权威状态 |
| `WiringAndMechanisms` | `WiringPropagationAndGateState`、`WiringTeleportAndPumpState`、`WiringMechanismCooldowns` | 30 | 传播、泵/传送和机制冷却保持独立提交边界 |
| `SpatialSimulation` | `CollisionQueryCache`、`CollisionContactAndHurtResults` | 19 | 查询期间缓存、接触值对象和伤害结果不升级为长期实体状态 |
| `DeathPenaltyAndRevenge` | `RevengeMarkerExpirationAndIdentityState`、`RevengeMarkerEnemyContextState`、`RevengeMarkerValueAndRespawnState`、`RevengeRegistryAndCache` | 35 | Marker 实例状态、注册表、时间和网络投影分开；持久化 owner 待整合 |
| `TeleportationAndTraversal` | `TeleportPylonRegistry` | 5 | Pylon 列表由 registry/projection 承担，TileEntity 与 SceneMetrics 通过适配器交接 |

不接受把以上内容合并为泛化的“基础模拟组件”。候选组件按共同读写者、变更原因、生命周期和语义内聚度分组；文件顺序不表达运行时顺序。

## 2. 证据登记

| 来源 | 已核对事实 | 状态 |
|---|---|---|
| P01 权威报告 | 成员序号、声明类型、相对/绝对路径、行列、原始声明、14 个叶子归属；期望与观察均为 113 | `source-inventory-confirmed` |
| `D:\TRbackup\Version4\Terraria\Liquid.cs` | 液体预算字段、`ReInit`、实例 `Update`、`UpdateLiquid`、`NetSendLiquid`、缓冲消费和网络 dirty 写入 | `partial`；调用图与写者闭合仍需 focused verifier |
| `D:\TRbackup\Version4\Terraria\LiquidBuffer.cs` | 计数、坐标条目、`AddBuffer`/`DelBuffer` 及 Tile checking 标记 | `partial` |
| `D:\TRbackup\Version4\Terraria\Wiring.cs` | `Initialize`、`ClearAll`、`UpdateMech`、`HitSwitch`、传播集合、泵/传送数组和机制冷却 | `partial`；`CheckMech`、`XferWater`、`CheckLogicGate` 在 Version4 当前文件为 stub，完整参考只能补充 `version-drift` 证据 |
| `D:\TRbackup\Version4\Terraria\Collision.cs` | Wet/Tile collision 写入环境标志，`BuildTileContacts` 消费 contacts，TileContact/HurtTile 是结果值对象 | `partial`；静态共享缓存的调用期间边界尚未完全闭合 |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs` | Marker 构造、唯一 ID、过期、敌人上下文、价值、respawn lock、registry、时间、网络发送和 Reset | `partial`；Version4 证据未闭合存档恢复与唯一 respawn owner |
| `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs` | TileEntity 扫描、当前/旧列表交换、冷却、增删广播、Reset、玩家加入投影、SceneMetrics 依赖 | `partial` |
| `D:\TRbackup\tmodloader-api-docs-stable` | 本地首页版本 `tModLoader v2026.07`；已定位 Liquid、LiquidBuffer、Collision、CoinLossRevengeSystem、TeleportPylonsSystem 页面 | `reference-only`；不能确认 Version4 私有调用顺序、持久化或网络语义 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | Component/System/Query 的组织粒度与副作用边界参考 | `reference-only`；不复制代码、命名或 Terraria 语义 |
| 当前 NLTX `src/` 与 `dome/src/` | 已有 Liquid、Wiring、Physics、Revenge、Teleportation、WorldStorage 骨架及 verifier 项目 | `partial`；同名或相近类型存在 owner 冲突，不能视为迁移完成 |

关键 Version4 锚点：`Liquid.cs:14-54,56-88,470,1015`；`LiquidBuffer.cs:5-30`；`Wiring.cs:17-75,88-184,151,267,464-467,665`；`Collision.cs:21-73,1001,1472,1633`；`CoinLossRevengeSystem.cs:18-80,263-299,315-458`；`TeleportPylonsSystem.cs:15-88`。行号只作为当前源码检索锚点，迁移前必须重新确认。

## 3. 拆分不变量

- Component 只保存一个内聚概念的权威数据；System 负责显式状态转换；纯 Query 不能写状态。
- Command 表达结构变化、输入意图或短生命周期工作项；不得让任意调用方隐式改写组件。
- Adapter 负责时钟、TileEntity、网络、存档、SceneMetrics、日志和第三方类型转换；Projection 只读输出，不反向成为权威源。
- 每个权威字段只允许一个 Owner System/CommitPort；兼容窗口可保留旧读路径，但禁止双写。
- 实体 ID、持久化 ID、网络 ID、Tile 坐标和外部连接 ID 分别建模；不得用数组下标或文件名作为隐式身份。
- 静态集合和调用期间缓存只有在声明所有权、失效条件、清理时机和线程假设后才能迁移。
- 时间、随机性、网络投递和持久化结果必须通过显式端口观察；不能把“调用返回”当作可靠投递或原子持久化证明。
- 系统先后关系写入 scheduler contract；不得通过目录枚举、文件顺序或项目文件顺序表达运行时顺序。

## 4. 叶子边界与候选模块

| checkpoint | 权威叶子 | 成员 | proposed 模块 | 类型分类 | 唯一 owner/seam | 生命周期与主要风险 |
|---|---|---:|---|---|---|---|
| C01 | `LiquidFlowBudgetAndPanicState` | 15 | `LiquidFlowBudgetPolicy`、`LiquidFlowBudgetAndPanicStateComponent`、`LiquidFlowSystem` | Definition/Component/System/Query | `ILiquidFlowCommitPort` | 世界初始化、每 tick、世界重置；预算和 panic 写者未闭合 |
| C02 | `LiquidCellWorkItemState` | 4 | `LiquidCellWorkItemStateCommand`、`LiquidWorkQueueSystem` | Command/System | `ILiquidWorkQueueCommitPort` | 单次队列消费；不得持久化为长期实体组件 |
| C03 | `LiquidBufferQueueState` | 3 | `LiquidBufferQueueStateComponent`、`LiquidBufferCommitPort` | Component/System | `ILiquidBufferCommitPort` | 溢出队列、入队/删除、Tile checking 清理 |
| C04 | `LiquidChangePublication` | 2 | `LiquidChangePublicationProjection`、`LiquidNetworkAdapter` | Projection/Adapter | 单向 drain snapshot | 网络 dirty 集合只能输出液体提交事实 |
| C05 | `WiringPropagationAndGateState` | 12 | `WiringPropagationStateComponent`、`WiringExecutionContextComponent`、`WiringPropagationSystem` | Component/System/Command | `IWiringPropagationCommitPort` | Initialize/ClearAll/HitSwitch/传播 tick；stub 与完整参考存在 version drift |
| C06 | `WiringTeleportAndPumpState` | 9 | `WiringTeleportStateComponent`、`PumpTransferScratchComponent`、`WiringTraversalAdapter` | Component/Command/Adapter | `IWiringTraversalCommitPort` | 一 tick 传送阻断、泵输入输出、Liquid 交接；不能重复拥有位置 |
| C07 | `WiringMechanismCooldowns` | 9 | `WiringMechanismScheduleComponent`、`WiringDeviceCooldownComponent`、`HopperInteractionPolicy` | Component/Definition/System | `IMechanismCooldownCommitPort` | 机制数组、时间与设备冷却；容量和重复触发需验证 |
| C08 | `CollisionQueryCache` | 10 | `CollisionQueryScratch`、`CollisionDerivedResultProjection`、`CollisionPolicyDefinition` | Query/Scratch/Projection/Definition | `ICollisionQueryPort` | 调用期间清空/覆盖；不可成为长期实体权威状态 |
| C09 | `CollisionContactAndHurtResults` | 9 | `TileContactResult`、`HurtTileResult`、`CollisionContactQuery` | Value/Query | `ISpatialContactResultPort` | 单次查询结果；接触和伤害 tile 不应被跨 tick 泄漏 |
| C10 | `RevengeMarkerExpirationAndIdentityState` | 9 | `RevengeExpirationPolicy`、`RevengeMarkerIdentityComponent`、`RevengeMarkerIdAllocator` | Definition/Component/Adapter/Projection | `IRevengeMarkerLifecycleCommitPort` | ID 分配、过期时间、唯一标识；持久化 ID 恢复未闭合 |
| C11 | `RevengeMarkerEnemyContextState` | 10 | `RevengeEnemyContextPolicy`、`RevengeTargetSnapshotComponent` | Definition/Component/Query | `IRevengeContextCommitPort` | 捕获时快照、复生资格只读消费；NPC netID/type owner 跨分区 |
| C12 | `RevengeMarkerValueAndRespawnState` | 5 | `RevengeMarkerValueComponent`、`RevengeRespawnAttemptComponent`、`RevengeRespawnSystem` | Component/System/Projection | `IRevengeRespawnCommitPort` | 价值、强制过期、一次性尝试锁；与自然重生竞争未裁决 |
| C13 | `RevengeRegistryAndCache` | 11 | `RevengeMarkerRegistryComponent`、`RevengeCachePolicy`、`RevengeClockState`、`RevengeNetworkProjection` | Component/Definition/System/Projection | `IRevengeRegistryCommitPort` | 列表锁、时间推进、移除和网络广播；存档格式缺口 |
| C14 | `TeleportPylonRegistry` | 5 | `PylonRegistryComponent`、`PylonRefreshPolicy`、`PylonRegistryProjection`、`SceneMetricsAdapter` | Component/Definition/Projection/Adapter | `IPylonRegistryCommitPort` | TileEntity 扫描、列表差异、网络和玩家加入；WorldStorage/Dome owner 冲突 |

## 5. 完整 113 成员归属表

下表逐行覆盖权威报告中的 111 个字段和 2 个属性。`proposed owner` 只表示候选所有权，不表示当前 NLTX 已存在同名实现。`state kind` 按 `authoritative`、`definition`、`command`、`cache`、`result`、`registry`、`projection`、`adapter` 分类。

| 序号 | 叶子 | 声明类型.成员 | C# 类型 | proposed owner | state kind | evidence |
|---:|---|---|---|---|---|---|
| 1 | C01 | `Terraria.Liquid.maxLiquidBuffer` | `int` | `LiquidFlowBudgetPolicy` | definition | source-inventory-confirmed |
| 2 | C01 | `Terraria.Liquid.maxLiquid` | `int` | `LiquidFlowBudgetPolicy` | definition | source-inventory-confirmed |
| 3 | C01 | `Terraria.Liquid.skipCount` | `int` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 4 | C01 | `Terraria.Liquid.stuckCount` | `int` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 5 | C01 | `Terraria.Liquid.stuckAmount` | `int` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 6 | C01 | `Terraria.Liquid.cycles` | `int` | `LiquidFlowBudgetPolicy` | definition | source-inventory-confirmed |
| 7 | C01 | `Terraria.Liquid.curMaxLiquid` | `int` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative/derived | source-inventory-confirmed |
| 8 | C01 | `Terraria.Liquid.numLiquid` | `int` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 9 | C01 | `Terraria.Liquid.stuck` | `bool` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 10 | C01 | `Terraria.Liquid.quickFall` | `bool` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 11 | C01 | `Terraria.Liquid.quickSettle` | `bool` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 12 | C01 | `Terraria.Liquid.wetCounter` | `int` | `LiquidFlowBudgetAndPanicStateComponent` | cache/state | source-inventory-confirmed |
| 13 | C01 | `Terraria.Liquid.panicCounter` | `int` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 14 | C01 | `Terraria.Liquid.panicMode` | `bool` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 15 | C01 | `Terraria.Liquid.panicY` | `int` | `LiquidFlowBudgetAndPanicStateComponent` | authoritative | source-inventory-confirmed |
| 16 | C02 | `Terraria.Liquid.x` | `int` | `LiquidCellWorkItemStateCommand` | command | source-inventory-confirmed |
| 17 | C02 | `Terraria.Liquid.y` | `int` | `LiquidCellWorkItemStateCommand` | command | source-inventory-confirmed |
| 18 | C02 | `Terraria.Liquid.kill` | `int` | `LiquidCellWorkItemStateCommand` | command | source-inventory-confirmed |
| 19 | C02 | `Terraria.Liquid.delay` | `int` | `LiquidCellWorkItemStateCommand` | command | source-inventory-confirmed |
| 20 | C04 | `Terraria.Liquid._netChangeSet` | `HashSet<int>` | `LiquidChangePublicationProjection` | projection | source-inventory-confirmed |
| 21 | C04 | `Terraria.Liquid._swapNetChangeSet` | `HashSet<int>` | `LiquidChangePublicationProjection` | projection/cache | source-inventory-confirmed |
| 22 | C03 | `Terraria.LiquidBuffer.numLiquidBuffer` | `int` | `LiquidBufferQueueStateComponent` | authoritative | source-inventory-confirmed |
| 23 | C03 | `Terraria.LiquidBuffer.x` | `int` | `LiquidBufferQueueStateComponent` | queue entry | source-inventory-confirmed |
| 24 | C03 | `Terraria.LiquidBuffer.y` | `int` | `LiquidBufferQueueStateComponent` | queue entry | source-inventory-confirmed |
| 25 | C10 | `RevengeMarker._uniqueIDCounter` | `int` | `RevengeMarkerIdAllocator` | adapter/state | source-inventory-confirmed |
| 26 | C10 | `RevengeMarker._expirationCompCopper` | `int` | `RevengeExpirationPolicy` | definition | source-inventory-confirmed |
| 27 | C10 | `RevengeMarker._expirationCompSilver` | `int` | `RevengeExpirationPolicy` | definition | source-inventory-confirmed |
| 28 | C10 | `RevengeMarker._expirationCompGold` | `int` | `RevengeExpirationPolicy` | definition | source-inventory-confirmed |
| 29 | C10 | `RevengeMarker._expirationCompPlat` | `int` | `RevengeExpirationPolicy` | definition | source-inventory-confirmed |
| 30 | C10 | `RevengeMarker.ONE_MINUTE` | `int` | `RevengeExpirationPolicy` | definition | source-inventory-confirmed |
| 31 | C11 | `RevengeMarker.ENEMY_BOX_WIDTH` | `int` | `RevengeEnemyContextPolicy` | definition | source-inventory-confirmed |
| 32 | C11 | `RevengeMarker.ENEMY_BOX_HEIGHT` | `int` | `RevengeEnemyContextPolicy` | definition | source-inventory-confirmed |
| 33 | C11 | `RevengeMarker.EnemyBoxSize` | `Vector2` | `RevengeEnemyContextPolicy` | definition | source-inventory-confirmed |
| 34 | C11 | `RevengeMarker._location` | `Vector2` | `RevengeTargetSnapshotComponent` | authoritative snapshot | source-inventory-confirmed |
| 35 | C11 | `RevengeMarker._hitbox` | `Rectangle` | `RevengeTargetSnapshotComponent` | derived snapshot | source-inventory-confirmed |
| 36 | C11 | `RevengeMarker._npcNetID` | `int` | `RevengeTargetSnapshotComponent` | external ID | source-inventory-confirmed |
| 37 | C11 | `RevengeMarker._npcHPPercent` | `float` | `RevengeTargetSnapshotComponent` | snapshot | source-inventory-confirmed |
| 38 | C12 | `RevengeMarker._baseValue` | `float` | `RevengeMarkerValueComponent` | authoritative snapshot | source-inventory-confirmed |
| 39 | C12 | `RevengeMarker._coinsValue` | `int` | `RevengeMarkerValueComponent` | authoritative snapshot | source-inventory-confirmed |
| 40 | C11 | `RevengeMarker._npcTypeAgainstDiscouragement` | `int` | `RevengeTargetSnapshotComponent` | definition ref | source-inventory-confirmed |
| 41 | C11 | `RevengeMarker._npcAIStyleAgainstDiscouragement` | `int` | `RevengeTargetSnapshotComponent` | definition ref | source-inventory-confirmed |
| 42 | C10 | `RevengeMarker._expirationTime` | `int` | `RevengeMarkerLifecycleComponent` | authoritative | source-inventory-confirmed |
| 43 | C11 | `RevengeMarker._spawnedFromStatue` | `bool` | `RevengeTargetSnapshotComponent` | snapshot | source-inventory-confirmed |
| 44 | C10 | `RevengeMarker._uniqueID` | `int` | `RevengeMarkerIdentityComponent` | identity | source-inventory-confirmed |
| 45 | C12 | `RevengeMarker._forceExpire` | `bool` | `RevengeMarkerLifecycleComponent` | authoritative | source-inventory-confirmed |
| 46 | C12 | `RevengeMarker._attemptedRespawn` | `bool` | `RevengeRespawnAttemptComponent` | authoritative | source-inventory-confirmed |
| 47 | C13 | `CoinLossRevengeSystem.DisplayCaching` | `bool` | `RevengePresentationPolicy` | definition/compat | source-inventory-confirmed |
| 48 | C13 | `CoinLossRevengeSystem.MinimumCoinsForCaching` | `int` | `RevengeCachePolicy` | definition | source-inventory-confirmed |
| 49 | C13 | `CoinLossRevengeSystem.PLAYER_BOX_WIDTH_INNER` | `int` | `RevengePlayerProximityPolicy` | definition | source-inventory-confirmed |
| 50 | C13 | `CoinLossRevengeSystem.PLAYER_BOX_HEIGHT_INNER` | `int` | `RevengePlayerProximityPolicy` | definition | source-inventory-confirmed |
| 51 | C13 | `CoinLossRevengeSystem.PLAYER_BOX_WIDTH_OUTER` | `int` | `RevengePlayerProximityPolicy` | definition | source-inventory-confirmed |
| 52 | C13 | `CoinLossRevengeSystem.PLAYER_BOX_HEIGHT_OUTER` | `int` | `RevengePlayerProximityPolicy` | definition | source-inventory-confirmed |
| 53 | C13 | `CoinLossRevengeSystem._playerBoxSizeInner` | `Vector2` | `RevengePlayerProximityPolicy` | definition | source-inventory-confirmed |
| 54 | C13 | `CoinLossRevengeSystem._playerBoxSizeOuter` | `Vector2` | `RevengePlayerProximityPolicy` | definition | source-inventory-confirmed |
| 55 | C13 | `CoinLossRevengeSystem._markers` | `List<RevengeMarker>` | `RevengeMarkerRegistryComponent` | registry | source-inventory-confirmed |
| 56 | C13 | `CoinLossRevengeSystem._markersLock` | `object` | `RevengeRegistryCommitPort` | synchronization adapter | source-inventory-confirmed |
| 57 | C13 | `CoinLossRevengeSystem._gameTime` | `int` | `RevengeClockState` | authoritative clock state | source-inventory-confirmed |
| 58 | C12 | `RevengeMarker.RespawnAttemptLocked` | `bool` | `RevengeRespawnAttemptProjection` | projection | source-inventory-confirmed |
| 59 | C10 | `RevengeMarker.UniqueID` | `int` | `RevengeMarkerIdentityProjection` | projection | source-inventory-confirmed |
| 158 | C06 | `Terraria.Wiring.blockPlayerTeleportationForOneIteration` | `bool` | `WiringTeleportStateComponent` | one-tick state | source-inventory-confirmed |
| 159 | C05 | `Terraria.Wiring.running` | `bool` | `WiringPropagationStateComponent` | authoritative | source-inventory-confirmed |
| 160 | C05 | `Terraria.Wiring._wireSkip` | `Dictionary<Point16,bool>` | `WiringPropagationStateComponent` | cache/state | source-inventory-confirmed |
| 161 | C05 | `Terraria.Wiring._wireList` | `DoubleStack<Point16>` | `WiringPropagationStateComponent` | work queue | source-inventory-confirmed |
| 162 | C05 | `Terraria.Wiring._wireDirectionList` | `DoubleStack<byte>` | `WiringPropagationStateComponent` | work queue | source-inventory-confirmed |
| 163 | C05 | `Terraria.Wiring._toProcess` | `Dictionary<Point16,byte>` | `WiringPropagationStateComponent` | work index | source-inventory-confirmed |
| 164 | C05 | `Terraria.Wiring._GatesCurrent` | `Queue<Point16>` | `WiringPropagationStateComponent` | work queue | source-inventory-confirmed |
| 165 | C05 | `Terraria.Wiring._LampsToCheck` | `Queue<Point16>` | `WiringPropagationStateComponent` | work queue | source-inventory-confirmed |
| 166 | C05 | `Terraria.Wiring._GatesNext` | `Queue<Point16>` | `WiringPropagationStateComponent` | work queue | source-inventory-confirmed |
| 167 | C05 | `Terraria.Wiring._GatesDone` | `Dictionary<Point16,bool>` | `WiringPropagationStateComponent` | cache/state | source-inventory-confirmed |
| 168 | C05 | `Terraria.Wiring._PixelBoxTriggers` | `Dictionary<Point16,byte>` | `WiringPropagationStateComponent` | trigger state | source-inventory-confirmed |
| 169 | C06 | `Terraria.Wiring._teleport` | `Vector2[]` | `WiringTeleportStateComponent` | transient result | source-inventory-confirmed |
| 170 | C06 | `Terraria.Wiring.MaxPump` | `int` | `PumpTransferPolicy` | definition | source-inventory-confirmed |
| 171 | C06 | `Terraria.Wiring._inPumpX` | `int[]` | `PumpTransferScratchComponent` | scratch | source-inventory-confirmed |
| 172 | C06 | `Terraria.Wiring._inPumpY` | `int[]` | `PumpTransferScratchComponent` | scratch | source-inventory-confirmed |
| 173 | C06 | `Terraria.Wiring._numInPump` | `int` | `PumpTransferScratchComponent` | scratch | source-inventory-confirmed |
| 174 | C06 | `Terraria.Wiring._outPumpX` | `int[]` | `PumpTransferScratchComponent` | scratch | source-inventory-confirmed |
| 175 | C06 | `Terraria.Wiring._outPumpY` | `int[]` | `PumpTransferScratchComponent` | scratch | source-inventory-confirmed |
| 176 | C06 | `Terraria.Wiring._numOutPump` | `int` | `PumpTransferScratchComponent` | scratch | source-inventory-confirmed |
| 177 | C07 | `Terraria.Wiring.MaxMech` | `int` | `WiringMechanismSchedulePolicy` | definition | source-inventory-confirmed |
| 178 | C07 | `Terraria.Wiring._mechX` | `int[]` | `WiringMechanismScheduleComponent` | authoritative queue | source-inventory-confirmed |
| 179 | C07 | `Terraria.Wiring._mechY` | `int[]` | `WiringMechanismScheduleComponent` | authoritative queue | source-inventory-confirmed |
| 180 | C07 | `Terraria.Wiring._numMechs` | `int` | `WiringMechanismScheduleComponent` | authoritative queue | source-inventory-confirmed |
| 181 | C07 | `Terraria.Wiring._mechTime` | `int[]` | `WiringMechanismScheduleComponent` | authoritative queue | source-inventory-confirmed |
| 182 | C05 | `Terraria.Wiring._currentWireColor` | `int` | `WiringExecutionContextComponent` | execution context | source-inventory-confirmed |
| 183 | C05 | `Terraria.Wiring.CurrentUser` | `int` | `WiringExecutionContextComponent` | execution context | source-inventory-confirmed |
| 184 | C07 | `Terraria.Wiring.cannonCoolDown` | `int` | `WiringDeviceCooldownComponent` | authoritative cooldown | source-inventory-confirmed |
| 185 | C07 | `Terraria.Wiring.bunnyCannonCoolDown` | `int` | `WiringDeviceCooldownComponent` | authoritative cooldown | source-inventory-confirmed |
| 186 | C07 | `Terraria.Wiring.snowballCannonCoolDown` | `int` | `WiringDeviceCooldownComponent` | authoritative cooldown | source-inventory-confirmed |
| 187 | C07 | `Terraria.Wiring.HopperGrabHitboxSize` | `Vector2` | `HopperInteractionPolicy` | definition | source-inventory-confirmed |
| 351 | C09 | `Terraria.Collision.TileContact.Side` | `TileContactSide` | `TileContactResult` | result value | source-inventory-confirmed |
| 352 | C09 | `Terraria.Collision.TileContact.Overlap` | `int` | `TileContactResult` | result value | source-inventory-confirmed |
| 353 | C09 | `Terraria.Collision.TileContact.X` | `int` | `TileContactResult` | result value | source-inventory-confirmed |
| 354 | C09 | `Terraria.Collision.TileContact.Y` | `int` | `TileContactResult` | result value | source-inventory-confirmed |
| 355 | C09 | `Terraria.Collision.TileContact.Slope` | `int` | `TileContactResult` | result value | source-inventory-confirmed |
| 356 | C09 | `Terraria.Collision.TileContact.Type` | `int` | `TileContactResult` | result value | source-inventory-confirmed |
| 357 | C09 | `Terraria.Collision.HurtTile.type` | `int` | `HurtTileResult` | result value | source-inventory-confirmed |
| 358 | C09 | `Terraria.Collision.HurtTile.x` | `int` | `HurtTileResult` | result value | source-inventory-confirmed |
| 359 | C09 | `Terraria.Collision.HurtTile.y` | `int` | `HurtTileResult` | result value | source-inventory-confirmed |
| 360 | C08 | `Terraria.Collision.stair` | `bool` | `CollisionQueryScratch` | query cache | source-inventory-confirmed |
| 361 | C08 | `Terraria.Collision.stairFall` | `bool` | `CollisionQueryScratch` | query cache | source-inventory-confirmed |
| 362 | C08 | `Terraria.Collision.honey` | `bool` | `CollisionQueryScratch` | query cache | source-inventory-confirmed |
| 363 | C08 | `Terraria.Collision.shimmer` | `bool` | `CollisionQueryScratch` | query cache | source-inventory-confirmed |
| 364 | C08 | `Terraria.Collision.sloping` | `bool` | `CollisionQueryScratch` | query cache | source-inventory-confirmed |
| 365 | C08 | `Terraria.Collision.up` | `bool` | `CollisionQueryScratch` | query cache | source-inventory-confirmed |
| 366 | C08 | `Terraria.Collision.down` | `bool` | `CollisionQueryScratch` | query cache | source-inventory-confirmed |
| 367 | C08 | `Terraria.Collision.bottomFluff` | `int` | `CollisionPolicyDefinition` | definition | source-inventory-confirmed |
| 368 | C08 | `Terraria.Collision.contacts` | `List<TileContact>` | `CollisionQueryScratch` | query cache | source-inventory-confirmed |
| 369 | C08 | `Terraria.Collision._cacheForConveyorBelts` | `List<Point>` | `CollisionQueryScratch` | query cache | source-inventory-confirmed |
| 2037 | C14 | `TeleportPylonsSystem._pylons` | `List<TeleportPylonInfo>` | `PylonRegistryComponent` | registry | source-inventory-confirmed |
| 2038 | C14 | `TeleportPylonsSystem._pylonsOld` | `List<TeleportPylonInfo>` | `PylonRegistryComponent` | registry snapshot | source-inventory-confirmed |
| 2039 | C14 | `TeleportPylonsSystem._cooldownForUpdatingPylonsList` | `int` | `PylonRegistryComponent` | scheduler state | source-inventory-confirmed |
| 2040 | C14 | `TeleportPylonsSystem.CooldownTimePerPylonsListUpdate` | `int` | `PylonRefreshPolicy` | definition | source-inventory-confirmed |
| 2041 | C14 | `TeleportPylonsSystem._sceneMetrics` | `SceneMetrics` | `SceneMetricsAdapter` | adapter state | source-inventory-confirmed |

报告序号在表中保持唯一；C01-C14 合计 111 个字段和 2 个属性。成员的完整原始声明、源码行列和绝对路径仍以 `inputReport` 为逐行权威来源，本表只添加候选归属，不改写原始声明。

## 6. C01 设计 checkpoint：LiquidFlowBudgetAndPanicState

### 6.1 proposed 类型与路径

| 类型 | proposed 路径 | 命名空间 | 状态 | 责任 |
|---|---|---|---|---|
| `LiquidFlowBudgetPolicy` | `dome/src/Terraria.Dome.Simulation/Liquid/Definitions/LiquidFlowBudgetPolicy.cs` | `Terraria.Dome.Simulation.Liquid.Definitions` | proposed | 只读容量、默认 tick budget 和 `cycles` 定义；不保存 tick 进度 |
| `LiquidFlowBudgetAndPanicStateComponent` | `dome/src/Terraria.Dome.Simulation/Liquid/Components/LiquidFlowBudgetAndPanicStateComponent.cs` | `Terraria.Dome.Simulation.Liquid.Components` | proposed | 世界级预算计数、停滞状态、快速结算状态和 panic 状态 |
| `LiquidFlowSystem` | `dome/src/Terraria.Dome.Simulation/Liquid/Systems/LiquidFlowSystem.cs` | `Terraria.Dome.Simulation.Liquid.Systems` | proposed | 读取 Tile/liquid snapshot 和命令，计算流动 tick，唯一提交 C01 component |
| `LiquidFlowBudgetQuery` | `dome/src/Terraria.Dome.Simulation/Liquid/Queries/LiquidFlowBudgetQuery.cs` | `Terraria.Dome.Simulation.Liquid.Queries` | proposed | 纯计算有效预算、是否进入 panic 和是否允许继续消费 |
| `ILiquidFlowCommitPort` | `dome/src/Terraria.Dome.Simulation/Liquid/Systems/ILiquidFlowCommitPort.cs` | `Terraria.Dome.Simulation.Liquid.Systems` | proposed | 显式提交状态变化；持久化、网络和日志不隐藏在 Query 中 |

领域目录已经存在且具有独立 Liquid 生命周期；`Definitions`、`Components`、`Systems`、`Queries` 是稳定的既有 Dome 组织边界，因此不创建泛化 `Shared/Components` 或 `Common` 目录。所有 proposed public 类型各占同名 PascalCase 文件。

### 6.2 成员、默认值和状态分类

| 成员 | proposed 字段/定义 | 默认值/来源 | 状态分类 | 不变量 |
|---|---|---|---|---|
| `maxLiquidBuffer` | `LiquidFlowBudgetPolicy.MaximumBufferLength` | Version4 `50000` | definition | 大于 0；不能被每 tick 状态写入 |
| `maxLiquid` | `LiquidFlowBudgetPolicy.DefaultMaximumLiquid` | Version4 `25000` | definition/config | 大于 0；配置覆盖必须在初始化时发生 |
| `cycles` | `LiquidFlowBudgetPolicy.CyclesPerUpdate` | Version4 `10` | definition/config | 大于 0；不因单个工作项隐式改变 |
| `skipCount` | `LiquidFlowBudgetAndPanicStateComponent.SkipCount` | `0` on `ReInit` | authoritative counter | 非负；只由 `LiquidFlowSystem` 增量或重置 |
| `stuckCount` | `...StuckCount` | `0` on `ReInit` | authoritative counter | 非负；和 stuck detection 同一提交批次更新 |
| `stuckAmount` | `...StuckAmount` | `0` on `ReInit` | authoritative counter | 非负；不能绕过预算提交 |
| `curMaxLiquid` | `...EffectiveMaximumLiquid` | `maxLiquid` or `5000` when reduced setting applies | derived/authority | 大于 0；由初始化 policy 计算，不能被 Projection 改写 |
| `numLiquid` | `...ActiveLiquidCount` | `0` on `ReInit` | authoritative count | 非负；不得超过明确的容量语义，边界待 verifier 确认 |
| `stuck` | `...IsStuck` | `false` | authoritative phase state | 只有流动算法能设置；清理时回到 false |
| `quickFall` | `...QuickFall` | `false` | authoritative phase flag | 只对当前流动阶段有效；tick 完成后清理规则需确认 |
| `quickSettle` | `...QuickSettle` | `false` | authoritative phase flag | 与快速结算提交同一 owner；不能作为渲染状态 |
| `wetCounter` | `...WetCounter` | `0` | private bounded cache/state | 非负；仅用于原始 wet/settle 过程，不能公开为外部 owner |
| `panicCounter` | `...PanicCounter` | `0` | authoritative counter | 非负；由 panic 状态转换单一写入 |
| `panicMode` | `...PanicMode` | `false` | authoritative mode | mode 与 counter/y 必须保持一致 |
| `panicY` | `...PanicY` | `0` | authoritative cursor | 只在 panic flow active 时有意义；清理时回到定义的起点 |

### 6.3 输入、输出、生命周期和副作用

- 初始化输入：`LiquidFlowPolicy`、WorldGen/reduced-liquid 配置、当前世界尺寸和已恢复液体快照。初始化由 World/Liquid lifecycle owner 通过 `Initialize` command 提交；不能在 component 构造函数读取全局 `Main`。
- 每 tick 输入：只读 Tile/liquid snapshot、`LiquidCellWorkItemStateCommand`、`LiquidBufferQueueState` 和显式 tick budget。`LiquidFlowBudgetQuery` 只计算结果，不修改 component。
- 唯一写入：`LiquidFlowSystem` 通过 `ILiquidFlowCommitPort` 写入 C01；任何网络适配器、存档适配器、Wiring pump 或 UI 只能发 command 或读取 snapshot。
- 输出：`LiquidFlowChangedEvent`、工作项/缓冲命令和 `LiquidChangePublicationProjection` 输入。网络和持久化不直接读取可变字段引用。
- Reset：世界重置/载入失败时由 lifecycle command 清零 counters/flags；`curMaxLiquid` 按 policy 重新计算。Reset 幂等性必须由 focused verifier 证明。
- 失败：预算耗尽是可观察的业务结果，写入 `stuck`/`panic` 或明确拒绝原因；Tile、网络、存档失败必须由 adapter 返回可区分的技术失败，不得伪装成正常 settle。
- 时间：tick 作为显式输入；不在 Query 内读取系统时钟。当前 Version4 使用游戏 tick 语义，具体 tick 调度仍需调用图补证。

### 6.4 C01 seam、依赖和不拆分项

```text
LiquidWorldSnapshot + LiquidFlowBudgetPolicy + tick
  -> LiquidFlowBudgetQuery (纯计算)
  -> LiquidFlowSystem
  -> ILiquidFlowCommitPort
  -> LiquidFlowBudgetAndPanicStateComponent
  -> Liquid work/buffer commands + LiquidChangePublicationProjection input
```

`TileCellComponent` 中的 `LiquidAmount/LiquidKind`、WorldStorage 的 `TileLiquidStateComponent` 和 Dome 的 Liquid components 不能在 C01 内宣布唯一 owner；它们属于 `integration-review`。C01 不把 `LiquidCellWorkItemState`、`LiquidBufferQueueState` 或 `_netChangeSet` 合并进 budget component，因为它们有不同的生命周期和副作用边界。

### 6.5 C01 focused verifier 计划与现状

计划 verifier：

1. `ReInit` 默认值和 reduced maximum 的确定性测试。
2. budget、counter、panicMode/panicY 的状态转换和非负不变量测试。
3. 重复 tick、预算为零、队列达到高水位、恢复和 Reset 幂等测试。
4. Query 无写回测试；Projection/Adapter 不能修改 C01 component 的快照测试。
5. Liquid flow 与 pump、Tile write、network dirty set 的单一写者和顺序测试。

实际状态：未创建 verifier、未编译、未运行测试；`verificationStatus: not-run`。已有 Dome Liquid verifier 只能作为 `existing-evidence` 线索，不能证明 C01 与 Version4 行为等价。

## 7. 跨域边界与 Integration Handoff

| 交接 | P01 候选输出 | 外部候选消费者 | 当前裁决 |
|---|---|---|---|
| Liquid tile authority | `LiquidFlowCommit` / read snapshot | `WorldStorage`、`WorldInteraction.Tiles`、WorldGen | `integration-review`；禁止 C01 自行接管 TileCell owner |
| Liquid network publication | immutable dirty snapshot | `NetMessage`/Dome protocol adapter | P01 只提供 Projection；投递语义、排序和去重待确认 |
| Wiring pump | `LiquidTransferCommand` | Liquid owner、WorldInteraction | `integration-review`；只允许一个 Tile/liquid commit root |
| Wiring teleport | teleport intent/result | `TeleportationAndTraversal`、Player/Entity | P01 不写实体位置；外部 teleport owner 负责 commit |
| Collision flags/results | read-only `CollisionQueryResult` | Physics、Player、NPC、Projectile | 当前 `src/Physics` 与 `src/SpatialSimulation` 有同名 `CollisionResultComponent`，必须整合后再迁移 |
| Revenge marker target | immutable target snapshot | NPC/Spawn/DeathPenalty | NPC type/netID、respawn eligibility 和自然重生竞争待裁决 |
| Revenge persistence | save/load snapshot | `WorldStorage` | 当前 Version4 证据不足，保持 blocked |
| Pylon registry | current/previous pylon snapshot | TileEntity、Teleportation、NetworkSession | WorldStorage 与 Dome registry owner 冲突，保持 `integration-review` |

## 8. 不拆分项和行为保持风险

- 不按字段数量继续机械拆分：同一叶子中共享相同生命周期和写入者的计数/状态暂保留一个候选 component；只有 verifier 证明独立访问模式后才细拆。
- 不把静态常量、容量策略和每实体/每世界状态混在一起；定义值优先放 Definition，运行时值才放 Component。
- 不把 `CollisionQueryCache` 当成可持久化组件；它的首选实现是 Query-owned scratch，缓存跨调用的现状只能作为兼容层暂存。
- 不把 `TileContact`/`HurtTile` 结构改成 ECS entity；它们是一次查询返回的值对象，除非调用图证明需要跨 tick 保留。
- 不把 `_markersLock`、`SceneMetrics`、网络 writer 或存档 writer 复制进 Component；这些是 adapter/commit boundary 的副作用依赖。
- 关键行为风险：Liquid panic 和 buffer 上限、Wiring 传播/泵/传送顺序、Collision 静态 flags 清理、Revenge ID 恢复与一次性 respawn、Pylon 列表差异广播均未通过 focused verifier。

## 9. 当前 checkpoint

### 9.1 C02 设计 checkpoint：LiquidCellWorkItemState

#### 9.1.1 proposed 边界

`Liquid.x`、`Liquid.y`、`Liquid.kill` 和 `Liquid.delay` 描述 `Main.liquid` 中一个待处理工作项的坐标、删除/清理计数和延迟计数。它们随工作项入队、消费和删除变化，生命周期短于世界液体事实，因此不应成为挂在 Tile 或实体上的长期权威 Component。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `LiquidCellWorkItemStateCommand` | `dome/src/Terraria.Dome.Simulation/Liquid/Commands/LiquidCellWorkItemStateCommand.cs` | `Terraria.Dome.Simulation.Liquid.Commands` | proposed | 携带坐标、kill 和 delay 的显式队列输入/更新意图 |
| `LiquidWorkItemReadyQuery` | `dome/src/Terraria.Dome.Simulation/Liquid/Queries/LiquidWorkItemReadyQuery.cs` | `Terraria.Dome.Simulation.Liquid.Queries` | proposed | 纯判断工作项是否可消费以及是否需要重排 |
| `LiquidWorkQueueSystem` | `dome/src/Terraria.Dome.Simulation/Liquid/Systems/LiquidWorkQueueSystem.cs` | `Terraria.Dome.Simulation.Liquid.Systems` | proposed | 唯一管理 active work item 的入队、消费、重试、删除和清理 |
| `ILiquidWorkQueueCommitPort` | `dome/src/Terraria.Dome.Simulation/Liquid/Systems/ILiquidWorkQueueCommitPort.cs` | `Terraria.Dome.Simulation.Liquid.Systems` | proposed | 把 command 原子提交到队列 owner；不暴露可变集合 |

`LiquidCellWorkItemStateCommand` 的核心字段只保留权威报告中的 `X`、`Y`、`Kill`、`Delay`。Dome 现有 `LiquidWorkItemComponent` 的 `LiquidType`、`Amount`、`Sequence` 和 WorldStorage `LiquidWorkEntry` 的 `TileCoordinate`、`Delay`、`KillState` 是相邻系统的现有数据，不能在 C02 内宣布重复 owner；它们通过 `integration-review` 决定是输入定义、队列排序元数据还是另一条工作流。

#### 9.1.2 状态分类与不变量

| Version4 成员 | proposed 表达 | 默认/生命周期 | 不变量 |
|---|---|---|---|
| `x` | `LiquidCellWorkItemStateCommand.X` | 入队时写入；消费后不再保留 | 必须在世界 tile bounds 内；坐标和 Tile 快照必须同一 tick 可解析 |
| `y` | `LiquidCellWorkItemStateCommand.Y` | 入队时写入；消费后不再保留 | 必须在世界 tile bounds 内；不能用实体 ID 或数组下标代替 |
| `kill` | `LiquidCellWorkItemStateCommand.Kill` | 新项为 `0`；每次处理或删除判定显式更新 | 非负；达到当前清理阈值后只能由 queue owner 删除 |
| `delay` | `LiquidCellWorkItemStateCommand.Delay` | 新项为 `0`；lava/honey 等流程可产生延迟 | 非负；未归零的项不能被 `ReadyQuery` 报告为可消费 |

工作队列的关键不变量是“同一坐标最多一个 active entry”。如果输入重复，系统必须采用显式去重/合并结果，而不是让两个工作项分别写同一个 Tile。`kill` 和 `delay` 是工作项过程状态，不应由网络包、存档投影或表现层回写。

#### 9.1.3 Version4 生命周期和副作用

- `Liquid.AddWater` 在 Tile 合法且 active capacity 未满时创建工作项、设 `kill = 0`、`delay = 0`、写入坐标并递增 `numLiquid`；容量不足时改进入 C03 buffer，不应绕过 C02 queue owner。
- `Liquid.Update` 读取坐标对应的邻接 Tile；实心 Tile 或空液体会把 `kill` 推到清理阈值，lava/honey 路径可能递增 `delay`，正常传播会再次入队邻接坐标。
- `Liquid.UpdateLiquid` 按 `wetCounter/cycles` 分段消费 `Main.liquid`，周期结束按 `kill` 删除工作项，并从 buffer 产生新的 C02 输入。这是 C01、C02、C03 的显式调度交接。
- `LiquidBuffer.DelBuffer` 只删除缓冲条目；C02 删除 active item 时必须清除 Tile 的 `checkingLiquid` 标记，清理责任不能分散到 Projection。
- 网络只通过 C04 的 dirty projection 观察 Tile 变化；C02 command 本身不是网络协议。存档默认不保存未提交的工作项；若恢复要求保留 pending work，必须先定义版本化队列快照和幂等重放，当前保持 `integration-review`。

#### 9.1.4 纯 Query 和提交 seam

```text
Tile/liquid read snapshot + C02 command + explicit tick
  -> LiquidWorkItemReadyQuery (pure)
  -> LiquidWorkQueueSystem
  -> ILiquidWorkQueueCommitPort
  -> active queue / C01 flow input / C03 buffer command
```

`LiquidWorkItemReadyQuery` 不访问 `Main.tile`、系统时钟、随机源或网络；调用方先提供只读 Tile snapshot、当前 delay/kill 和清理策略。`LiquidWorkQueueSystem` 是唯一可以改变 active entry、去重索引和清理标记的 owner。C02 不拥有 Tile 的液体数量/种类，也不拥有 C04 的 dirty set。

#### 9.1.5 C02 focused verifier 与风险

计划验证：边界坐标拒绝、重复坐标去重、kill 阈值删除、delay 递减和未就绪阻止消费；重复 command 的确定性；清理 active entry 时 `checkingLiquid` 恢复；队列 drain/requeue 后不泄漏可变集合；world reset 后 active entry 为空。还需做 C01 budget/C03 buffer 的交接测试，证明容量不足只产生一个 buffer command。

当前证据仍为 `partial`：Version4 的 `Main.liquid` 数组 owner、Tile checking bit 的完整写者集合、WorldGen/loading 期间是否允许 pending queue、Dome/WorldStorage 两套 work item 的转换尚未闭合。未创建或运行 verifier，未执行编译。

### 9.2 C03 设计 checkpoint：LiquidBufferQueueState

#### 9.2.1 proposed 边界

`LiquidBuffer.numLiquidBuffer`、`LiquidBuffer.x` 和 `LiquidBuffer.y` 是 active liquid 工作集达到容量后的延迟坐标队列。它不是第二套液体事实，也不是可持久化的实体组件；它只保存等待进入 C02 active queue 的坐标和队列计数。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `LiquidBufferQueueStateComponent` | `dome/src/Terraria.Dome.Simulation/Liquid/Components/LiquidBufferQueueStateComponent.cs` | `Terraria.Dome.Simulation.Liquid.Components` | proposed | 保存有界 deferred coordinate queue、计数和去重索引 |
| `LiquidBufferCommand` | `dome/src/Terraria.Dome.Simulation/Liquid/Commands/LiquidBufferCommand.cs` | `Terraria.Dome.Simulation.Liquid.Commands` | proposed | 表达 enqueue/dequeue/release checking flag 的显式意图 |
| `LiquidBufferCommitSystem` | `dome/src/Terraria.Dome.Simulation/Liquid/Systems/LiquidBufferCommitSystem.cs` | `Terraria.Dome.Simulation.Liquid.Systems` | proposed | 唯一写入 C03 queue，并将 drain 结果交给 C02 |
| `ILiquidBufferCommitPort` | `dome/src/Terraria.Dome.Simulation/Liquid/Systems/ILiquidBufferCommitPort.cs` | `Terraria.Dome.Simulation.Liquid.Systems` | proposed | 原子执行有界入队、删除、清空和 checking flag 交接 |

#### 9.2.2 成员和不变量

| Version4 成员 | proposed 表达 | 默认/来源 | 不变量 |
|---|---|---|---|
| `numLiquidBuffer` | `LiquidBufferQueueStateComponent.Count` | `0` on world reset | `0 <= Count <= MaximumLength`; 只能由 queue owner 更新 |
| `x` | `LiquidBufferQueueEntry.X` | `AddBuffer` 输入 | 与 `y` 成对；坐标必须在 Tile bounds 内 |
| `y` | `LiquidBufferQueueEntry.Y` | `AddBuffer` 输入 | 与 `x` 成对；同一坐标最多一个 deferred entry |

建议 `MaximumLength` 由 `LiquidFlowBudgetPolicy.MaximumBufferLength` 提供单一配置来源，具体是否保持 Version4 的 `50000` 与 `numLiquid < 49998` 的保留槽位语义，需要 focused verifier 确认，不能仅凭字段名改变边界。`LiquidBufferQueueStateComponent` 不拥有 Tile 的 `LiquidAmount/LiquidKind`，只拥有 deferred coordinate 和队列 metadata。

#### 9.2.3 生命周期、提交顺序和副作用

- `Liquid.AddWater` 在 active `numLiquid` 接近 `curMaxLiquid` 时发 `Enqueue(x,y)`；C03 commit 必须先检查 Tile 是否已标记 `checkingLiquid`，重复坐标返回 deterministic no-op。
- `Liquid.UpdateLiquid` 在 active work 清理后计算可释放数量，从 buffer 头部读取坐标，清除 Tile `checkingLiquid`，再发 C02 enqueue；如果 C02 拒绝，C03 不能静默丢弃坐标，应返回明确的拒绝/重试结果并决定是否恢复 checking flag。
- `LiquidBuffer.DelBuffer` 的 swap-with-last 行为是内部实现细节；对外只暴露稳定的 `Drain`/`Release` 结果，不暴露可变 array 引用。
- Reset/world load failure 必须清空 deferred entries 和 dedup index；清空动作幂等。存档默认只保存已经提交的 Tile liquid，不保存临时 buffer；若要保存必须有版本字段、坐标去重和恢复时序。
- C03 不能直接发布网络消息。Tile change 由 C02/C01 commit 产出给 C04 projection；网络或日志失败不得改变 buffer 的权威队列。

#### 9.2.4 依赖与 focused verifier

```text
LiquidFlowState + active capacity result
  -> LiquidBufferCommand.Enqueue
  -> LiquidBufferCommitSystem
  -> LiquidBufferQueueStateComponent
  -> Drain result
  -> LiquidCellWorkItemStateCommand
  -> C02 work queue commit
```

验证计划：容量上限和保留槽位、重复坐标、`checkingLiquid` 已为 true 的入队、drain 后重入、active queue 拒绝、world reset、load failure、异常路径不丢 entry、不可变 snapshot 和 C03->C02 顺序。当前仍未创建/运行 verifier，证据为 `partial`。

### 9.3 C04 设计 checkpoint：LiquidChangePublication

#### 9.3.1 proposed 边界

`Liquid._netChangeSet` 和 `_swapNetChangeSet` 只记录液体 Tile 坐标变更，供服务器在一个更新阶段内交换、去重并生成 `NetLiquidModule` 广播；它们不是液体事实，也不能成为客户端或网络层的写入源。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `LiquidChangePublicationSnapshot` | `dome/src/Terraria.Dome.Simulation/Liquid/Snapshots/LiquidChangePublicationSnapshot.cs` | `Terraria.Dome.Simulation.Liquid.Snapshots` | proposed | 按稳定坐标/版本承载一次发布批次的只读值 |
| `LiquidChangePublicationProjection` | `dome/src/Terraria.Dome.Simulation/Liquid/Projections/LiquidChangePublicationProjection.cs` | `Terraria.Dome.Simulation.Liquid.Projections` | proposed | 从已提交液体事实生成唯一坐标 dirty snapshot，负责交换语义 |
| `LiquidNetworkAdapter` | `dome/src/Terraria.Dome.Simulation/Liquid/Adapters/LiquidNetworkAdapter.cs` | `Terraria.Dome.Simulation.Liquid.Adapters` | proposed | 把 snapshot 编码成 `NetLiquidModule`，隔离网络投递、失败和重试 |
| `ILiquidPublicationPort` | `dome/src/Terraria.Dome.Simulation/Liquid/Adapters/ILiquidPublicationPort.cs` | `Terraria.Dome.Simulation.Liquid.Adapters` | proposed | 只接收不可变发布批次，不接受网络回写 |

#### 9.3.2 Version4 事实、字段归属和不变量

| Version4 成员 | proposed 表达 | 生命周期 | 不变量 |
|---|---|---|---|
| `_netChangeSet` | `PendingLiquidChangeCoordinates` | `NetSendLiquid` 加入；发布交换时清空/转移 | 坐标去重；worldgen/loading 时不应产生对外广播 |
| `_swapNetChangeSet` | `PublishingLiquidChangeCoordinates` | 与 pending set 交换；广播后清空 | 发布批次与下一批写入隔离；失败时不能丢失未确认事实 |

Version4 `NetSendLiquid` 将 `(x & 0xFFFF) << 16 | (y & 0xFFFF)` 放入 pending set，并在 `UpdateLiquid` 末尾交换集合后调用 `NetLiquidModule.CreateAndBroadcastByChunk`。proposed projection 必须保留坐标编码的兼容规则或明确版本化替代；不允许以 HashSet 枚举顺序作为协议顺序。

#### 9.3.3 投影、适配和副作用边界

```text
Committed LiquidTileSnapshot
  -> LiquidChangePublicationProjection
  -> immutable LiquidChangePublicationSnapshot
  -> LiquidNetworkAdapter / PersistenceProjection
  -> NetLiquidModule or versioned snapshot output
```

- C01-C03 的唯一 owner 在 Tile/liquid commit 完成后发出 dirty fact；C04 只读取该 fact 并生成投影。
- projection 可以做坐标去重、chunk 分组、排序和 revision 标记，但不能改写 liquid amount/type 或 C01/C02/C03 状态。
- network adapter 负责投递成功、失败、取消、超时未知和重试策略；不能假设 `Broadcast` 返回即已送达，也不能因发送失败回写或删除权威 Tile。
- `WorldGen.isGeneratingOrLoadingWorld` 是 Version4 外部环境输入；新 projection 应接收显式 publication-enabled flag，而非在纯 projection 中读取全局变量。
- 持久化 snapshot 若需要 dirty revision，只能单向读取 projection；当前没有证据表明 `_netChangeSet` 应保存，默认不保存临时发布缓存。

#### 9.3.4 NLTX 现状与验证缺口

NLTX 已有 `src/WorldStorage/LiquidReplicationDirtySet.cs`（pending/publishing coordinates、last revision）和 Dome `LiquidReplicationSystem`（按 sequence 选最新坐标并生成 revisioned snapshot），并有 Liquid loopback verifier 线索。这些是可复用证据和候选实现，不等于 C04 已闭合：仍需确认唯一 dirty owner、revision 分配、chunk 编码、连接断开/重试和 loading/worldgen 抑制。

focused verifier 计划：同坐标多次变更只发布最终值；pending/publishing 交换期间新增变更不丢失；空批次不广播；稳定排序和 revision 单调性；网络失败不破坏 authority；客户端接收后只更新投影/Tile adapter；重复 packet 可去重；worldgen/load 阶段不对外发布。当前未运行任何 verifier 或编译。

### 9.4 C05 设计 checkpoint：WiringPropagationAndGateState

#### 9.4.1 proposed 边界

C05 包含传播一次 `TripWire` 所需的 frontier、方向、待处理索引、logic gate/lamp 两阶段队列、pixel box trigger 和执行上下文。它是 `WiringAndMechanisms` 的短生命周期权威执行状态，不是 Tile wire topology，也不是机制结果或实体位置。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `WiringPropagationStateComponent` | `dome/src/Terraria.Dome.Simulation/Wiring/Components/WiringPropagationStateComponent.cs` | `Terraria.Dome.Simulation.Wiring.Components` | proposed | 持有 skip/frontier/to-process/gate/lamp/pixel 工作集 |
| `WiringExecutionContextComponent` | `dome/src/Terraria.Dome.Simulation/Wiring/Components/WiringExecutionContextComponent.cs` | `Terraria.Dome.Simulation.Wiring.Components` | proposed | 保存 `running`、current wire color 和 current user 的一次执行上下文 |
| `WiringPropagationCommand` | `dome/src/Terraria.Dome.Simulation/Wiring/Commands/WiringPropagationCommand.cs` | `Terraria.Dome.Simulation.Wiring.Commands` | proposed | 表达 HitSwitch/TripWire/PokeLogicGate 等输入意图 |
| `WiringPropagationSystem` | `dome/src/Terraria.Dome.Simulation/Wiring/Systems/WiringPropagationSystem.cs` | `Terraria.Dome.Simulation.Wiring.Systems` | proposed | 唯一执行传播、gate pass、pixel pass 和清理 |
| `WiringPropagationQuery` | `dome/src/Terraria.Dome.Simulation/Wiring/Queries/WiringPropagationQuery.cs` | `Terraria.Dome.Simulation.Wiring.Queries` | proposed | 只读 wire topology 和 tile snapshot，计算可传播节点/资格 |
| `IWiringPropagationCommitPort` | `dome/src/Terraria.Dome.Simulation/Wiring/Systems/IWiringPropagationCommitPort.cs` | `Terraria.Dome.Simulation.Wiring.Systems` | proposed | 提交传播结果和结构/机制命令，不直接执行音频、网络或实体副作用 |

NLTX 已有 `src/WorldInteraction/Wiring/WirePropagationScratchComponent`，其字段形状与 C05 高度接近；Dome 另有 `WireNetworkComponent`、`WireTraversalStateComponent`。这些是整合输入，不是允许创建第三个独立 owner 的依据。C05 必须先裁决 topology、traversal budget 和 execution scratch 的边界。

#### 9.4.2 12 个成员和状态分类

| Version4 成员 | proposed 表达 | 默认/生命周期 | 状态分类 |
|---|---|---|---|
| `running` | `WiringExecutionContextComponent.IsRunning` | `false`；TripWire 开始 true，全部 pass 后 false | authoritative phase |
| `_wireSkip` | `WiringPropagationStateComponent.SkippedTiles` | empty on Initialize/clear | per-run cache |
| `_wireList` | `...Frontier` | empty | work queue |
| `_wireDirectionList` | `...FrontierDirections` | empty | work queue |
| `_toProcess` | `...TilesToProcess` | empty | dedup/index cache |
| `_GatesCurrent` | `...CurrentGates` | empty | phase queue |
| `_LampsToCheck` | `...LampsToCheck` | empty | phase queue |
| `_GatesNext` | `...NextGates` | empty | phase queue |
| `_GatesDone` | `...CompletedGates` | empty | cycle guard |
| `_PixelBoxTriggers` | `...PixelBoxTriggers` | empty | deferred tile result |
| `_currentWireColor` | `WiringExecutionContextComponent.CurrentWireColor` | unset/default until wire pass | execution context |
| `CurrentUser` | `WiringExecutionContextComponent.CurrentUser` | Version4 `255` | execution context/external user ID |

`Point16`/`TileCoordinate` 是结构坐标，不是 entity identity；`CurrentUser` 的 `255` 是 Version4 的 sentinel，不应与 Player entity ID、network connection ID 或持久化 ID 混淆。队列和 dictionary 的可变引用只由 C05 owner 持有，外部只能拿 immutable snapshot。

#### 9.4.3 生命周期、调度和副作用

- `Initialize` 分配所有 C05/C06/C07 工作容器；proposed system 需要显式 `InitializeWorld` command，并禁止在查询阶段 lazy-create 共享集合。
- `HitSwitch` 是外部输入入口，调用者包括 network/world interaction/WorldGen 等；它先做 Tile qualification，再发 `WiringPropagationCommand`。Sound、tile frame、NetMessage、NPC/item/projectile effects 都转换为结果 command/event。
- `TripWire` 清理旧 wire queues，按颜色收集节点，调用 `HitWire`，完成四种 wire pass 后运行 `PixelBoxPass`、`LogicGatePass`，最后清除 `running`。这个阶段顺序必须在 scheduler contract 中显式化。
- `PokeLogicGate` 入队 lamp 并触发 gate pass；`_GatesDone` 仅在一个 propagation run 内去重，不能跨 tick 作为长期事实。
- `ClearAll` 当前实现还清理 C06 pump arrays 和 C07 mechanism arrays；迁移后 ClearAll 必须拆成显式 `WiringPropagationReset`、`PumpReset`、`MechanismReset` commands，调用顺序由父级 coordinator 定义。
- Version4 当前 `CheckLogicGate` 是空 stub，完整参考存在实现差异；C05 只能把 gate evaluation seam 设计为 `version-drift`，不能声称传播行为完整。

#### 9.4.4 依赖方向与 focused verifier

```text
WiringInputCommand + WireNetworkSnapshot + TileSnapshot
  -> WiringPropagationQuery
  -> WiringPropagationSystem
  -> WiringPropagationStateComponent / ExecutionContext
  -> Mechanism, Pump, Teleport, Tile and event commands
```

验证计划：初始化和 ClearAll 幂等；四种 wire color 顺序；skip/direction/toProcess 一致性；gate/lamp 两阶段去重和循环终止；`running` 只在传播期间为 true；CurrentUser 边界；重复 HitSwitch；结构变化、网络输入、WorldGen 输入都转换为同一 command；stub/version-drift 被显式标记。未执行 verifier、编译或行为测试。

### 9.5 C06 设计 checkpoint：WiringTeleportAndPumpState

#### 9.5.1 proposed 边界

C06 保存 Wiring 一次传播中的两个 teleport endpoint、泵输入/输出坐标和“一次迭代禁止玩家传送”标志。它只产生 teleport/pump intent，不拥有 Player/NPC 的位置，也不直接提交 Tile liquid 数量。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `WiringTeleportStateComponent` | `dome/src/Terraria.Dome.Simulation/Wiring/Components/WiringTeleportStateComponent.cs` | `Terraria.Dome.Simulation.Wiring.Components` | proposed | 保存最多两个本次传播的 teleport targets 与 one-iteration block flag |
| `PumpTransferScratchComponent` | `dome/src/Terraria.Dome.Simulation/Wiring/Components/PumpTransferScratchComponent.cs` | `Terraria.Dome.Simulation.Wiring.Components` | proposed | 保存最多 20 个 input/output pump 坐标及数量 |
| `PumpTransferCommand` | `dome/src/Terraria.Dome.Simulation/Wiring/Commands/PumpTransferCommand.cs` | `Terraria.Dome.Simulation.Wiring.Commands` | proposed | 把 pump scratch 转为 Liquid owner 可验证的 transfer intent |
| `WiringTeleportCommand` | `dome/src/Terraria.Dome.Simulation/Wiring/Commands/WiringTeleportCommand.cs` | `Terraria.Dome.Simulation.Wiring.Commands` | proposed | 携带 source/destination、wire run 和外部 subject references |
| `WiringTraversalAdapter` | `dome/src/Terraria.Dome.Simulation/Wiring/Adapters/WiringTraversalAdapter.cs` | `Terraria.Dome.Simulation.Wiring.Adapters` | proposed | 对接 Teleportation/Player/NPC 和 Liquid adapter；执行副作用但不拥有其状态 |
| `IWiringTraversalCommitPort` | `dome/src/Terraria.Dome.Simulation/Wiring/Systems/IWiringTraversalCommitPort.cs` | `Terraria.Dome.Simulation.Wiring.Systems` | proposed | 提交 C06 scratch 清理和显式 intent 输出 |

NLTX 已有 `src/WorldInteraction/Wiring/PumpTransferScratchComponent` 和 `src/Teleportation/PortalNetworkState`；它们提供组织参考，但 C06 不能把 portal network、entity position 或 Liquid tile state 复制到 Wiring。Dome `PumpCommandSystem` 已展示从机制激活生成 `LiquidTransferCommand` 的候选方向，仍需与 Version4 完整 `XferWater` 语义对齐。

#### 9.5.2 成员、默认值和状态分类

| Version4 成员 | proposed 表达 | 默认值/边界 | 状态分类 |
|---|---|---|---|
| `blockPlayerTeleportationForOneIteration` | `WiringTeleportStateComponent.BlockPlayerTeleportationForOneIteration` | `false`；LogicGatePass 后按规则清理 | one-tick authority |
| `_teleport` | `TeleportTargets[2]` | 两个 endpoint 为 invalid sentinel `(-1,-1)` | transient result |
| `MaxPump` | `PumpTransferPolicy.MaxPumpCount` | `20` | definition |
| `_inPumpX/_inPumpY` | `InputPumpPositions[20]` | zeroed at Initialize/ClearAll | scratch |
| `_numInPump` | `InputPumpCount` | `0` | scratch count |
| `_outPumpX/_outPumpY` | `OutputPumpPositions[20]` | zeroed at Initialize/ClearAll | scratch |
| `_numOutPump` | `OutputPumpCount` | `0` | scratch count |

Input/output counts must be in `[0, MaxPumpCount]`; the Version4 code stops adding at index 19, so the exact full/overflow behavior needs an explicit verifier. Teleport endpoint validity must be checked before producing a command; endpoint arrays cannot be exposed as mutable references.

#### 9.5.3 生命周期和跨父级顺序

- C05 `TripWire` begins one color pass, clears the two endpoint slots and pump counts, collects C06 facts while `HitWire` runs, then emits pump and teleport intents after each color pass.
- `XferWater` is empty in current Version4; the complete reference performs Tile liquid transfer. This is `full-reference-supplemented` but `version-drift`, so C06 must not claim current behavior is closed.
- Teleport is invoked only after all wire colors have captured endpoint pairs and `running` is cleared. The adapter produces an intent/result; TeleportationAndTraversal or entity owner commits Player/NPC position, history, cooldown and network projection.
- `blockPlayerTeleportationForOneIteration` is a gate/teleport coordination flag, not a persistent player status. Its reset must be tied to the explicit gate pass, including early failure paths.
- Pump transfer ordering is `wire propagation -> validate input/output pairs -> LiquidTransferCommand -> Liquid owner commit -> Liquid publication`; C06 never writes `TileLiquidStateComponent` directly.

#### 9.5.4 focused verifier and non-split items

Verify two endpoint sentinel/ordering rules, invalid/identical endpoints, player block one iteration and reset, pump count bounds, duplicate pump coordinates, input/output pairing, deterministic transfer command ordering, C05 color pass boundaries, Liquid commit rejection and teleport adapter failure. Do not split each pump slot or endpoint into separate components; fixed capacity and shared lifecycle make that over-atomic.

### 9.6 C07 设计 checkpoint：WiringMechanismCooldowns

#### 9.6.1 proposed 边界

C07 把 Wiring 机制调度队列和设备级 cooldown 从传播 scratch 中分开。`_mechX/_mechY/_mechTime` 是坐标化的延迟机制 work set；三个 cannon cooldown 是按设备类型共享的时间状态；`HopperGrabHitboxSize` 是只读几何 policy。C07 不拥有机制产生的 Tile、Item、NPC、Projectile 或音效结果。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `WiringMechanismScheduleComponent` | `dome/src/Terraria.Dome.Simulation/Wiring/Components/WiringMechanismScheduleComponent.cs` | `Terraria.Dome.Simulation.Wiring.Components` | proposed | 有界保存机制坐标和 remaining time |
| `WiringMechanismSchedulePolicy` | `dome/src/Terraria.Dome.Simulation/Wiring/Definitions/WiringMechanismSchedulePolicy.cs` | `Terraria.Dome.Simulation.Wiring.Definitions` | proposed | 提供 `MaxMech=1000` 和延迟校验策略 |
| `WiringDeviceCooldownComponent` | `dome/src/Terraria.Dome.Simulation/Wiring/Components/WiringDeviceCooldownComponent.cs` | `Terraria.Dome.Simulation.Wiring.Components` | proposed | 保存 cannon/bunny/snowball cooldown ticks |
| `HopperInteractionPolicy` | `dome/src/Terraria.Dome.Simulation/Wiring/Definitions/HopperInteractionPolicy.cs` | `Terraria.Dome.Simulation.Wiring.Definitions` | proposed | 保存 192 px hitbox policy；不保存 actor state |
| `MechanismCooldownSystem` | `dome/src/Terraria.Dome.Simulation/Wiring/Systems/MechanismCooldownSystem.cs` | `Terraria.Dome.Simulation.Wiring.Systems` | proposed | 唯一 tick、schedule、expire 和 reset C07 状态 |
| `MechanismActivationQuery` | `dome/src/Terraria.Dome.Simulation/Wiring/Queries/MechanismActivationQuery.cs` | `Terraria.Dome.Simulation.Wiring.Queries` | proposed | 纯判断坐标、剩余时间和 device cooldown 是否允许激活 |

NLTX 已有 `src/WorldInteraction/Wiring/MechanismCooldownComponent/Entry`，Dome 已有 `MechanismRetryState` 和 `MechanismCommitCoordinator`。这些可复用边界证据不能直接证明 Version4 的 `CheckMech` 语义；当前 Version4 `CheckMech` 是 stub，完整参考需单独对照。

#### 9.6.2 成员、默认值和不变量

| Version4 成员 | proposed 表达 | 默认值/来源 | 不变量 |
|---|---|---|---|
| `MaxMech` | `WiringMechanismSchedulePolicy.MaximumEntries` | `1000` | schedule count 在 `[0,1000]` |
| `_mechX` | `ScheduleEntry.X` | zeroed at Initialize/ClearAll | 与 Y/time 同 index |
| `_mechY` | `ScheduleEntry.Y` | zeroed | 与 X/time 同 index |
| `_numMechs` | `WiringMechanismScheduleComponent.Count` | `0` | 非负、不得超过 capacity |
| `_mechTime` | `ScheduleEntry.RemainingTicks` | assigned by `CheckMech` input | 非负；到 0 才产生 expiration result |
| `cannonCoolDown` | `WiringDeviceCooldownComponent.CannonCooldownTicks` | `0` | tick 到 0，不能负数 |
| `bunnyCannonCoolDown` | `...BunnyCannonCooldownTicks` | `0` | tick 到 0，不能负数 |
| `snowballCannonCoolDown` | `...SnowballCannonCooldownTicks` | `0` | tick 到 0，不能负数 |
| `HopperGrabHitboxSize` | `HopperInteractionPolicy.HitboxSize` | Version4 `(192f,192f)` | 只读正值；查询计算不能改写 |

同一机制坐标的重复 schedule 必须有明确结果（拒绝、刷新或取更早过期时间），不能靠数组覆盖顺序。`UpdateMech` 的 reverse iteration、out-of-world/tile-null 删除和到期 Tile frame/network side effects 需要由 system result command 显式表达。

#### 9.6.3 生命周期和副作用

- `Wiring.Initialize` 分配 1000 长度的机制数组；`ClearAll` 清零坐标、时间和计数。proposed system 使用 world lifecycle command，不能让 C05 propagation reset 偷清 C07。
- `CheckMech(i,j,time)` 是外部 schedule intent 入口，当前版本为空实现；完整参考的去重和时间规则需要 `version-drift` 对照，不能假设已运行。
- `UpdateMech` 每 tick 递减三个 device cooldown 和每个 schedule time，检查 Tile validity，过期时产生 tile frame/network/effect command，再移除 schedule entry。
- `cannonCoolDown` 等共享设备状态不是 per-entity component；设备/机制定义和 player/item/NPC effect 由外部 owner 通过 command 消费。
- Hopper hitbox 只作为纯几何输入；任何 Item/actor 拾取和 inventory 改动走 WorldItem/ItemContainer owner。

#### 9.6.4 focused verifier

验证 capacity 999/1000/1001、duplicate schedule、out-of-world cleanup、reverse-removal 不跳过条目、zero/negative time、device cooldown decrement and reset、Hopper hitbox immutability、`CheckMech` version drift、C05/C06 reset order、effect command idempotency 和 network projection 单向性。未执行 verifier、编译或运行时验证。

#### 9.6.5 当前实施 checkpoint：C07 bounded components 源码已保存

- 本 checkpoint 实际保存 `src/WorldInteraction/Wiring/WiringMechanismScheduleComponent.cs` 和
  `src/WorldInteraction/Wiring/WiringDeviceCooldownComponent.cs`。前者使用固定容量 `1000`，
  复用 `MechanismCooldownEntry` 保存机制坐标和剩余 tick，并提供 `Count`、防御性只读条目视图
  与内部 `Reset()`；后者只保存三个设备级 cooldown tick，并通过内部 `Reset()` 恢复为 `0`。
- 两个组件只表达状态容器，不执行 schedule 去重、tick 递减、过期删除、冷却判断、Tile 修改、
  网络发布或机制效果；没有修改旧 `MechanismCooldownComponent`、`MechanismCooldownEntry` 或
  C05/C06 wiring owner。
- 因本次范围只允许写 ECS component 源码，`WiringMechanismSchedulePolicy`、
  `HopperInteractionPolicy`、`MechanismCooldownSystem` 和 `MechanismActivationQuery` 仍未实现。
  源码已通过 `Terraria.WorldInteraction.csproj` 串行 build（退出码 `0`、`0` errors、`15` 个既有
  `WorldStorage` warnings），不写入仓库的 focused reflection verifier 输出
  `C07_COMPONENT_VERIFIER=PASS ASSERTIONS=13`，覆盖固定容量 1000、默认计数/冷却值、只读条目
  视图、schedule entry storage reset 和三个设备 cooldown reset。C07 仍为 `partial`，不能加入
  `completedComponents`；执行游标推进到 `C08`，`pendingComponents` 为
  `[C08, C09, C10, C11, C12, C13, C14]`。

### 9.7 C08 设计 checkpoint：CollisionQueryCache

#### 9.7.1 proposed 边界

C08 重新分类权威报告中的 `CollisionQueryCache`：这些成员是 `WetCollision`、`SlopeCollision`、`TileCollision` 和 conveyor 查询期间的派生结果/工作缓存，不是长期实体或世界权威状态。建议以调用上下文拥有 scratch，或由 `CollisionQueryPort` 在一次查询内创建并返回不可变结果；只有兼容期才允许封装 Version4 静态字段。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `CollisionQueryScratch` | `dome/src/Terraria.Dome.Simulation/Physics/Queries/CollisionQueryScratch.cs` | `Terraria.Dome.Simulation.Physics.Queries` | proposed | 保存一次 query 的 stair/liquid flags、contacts 和 conveyor tile cache |
| `CollisionPolicyDefinition` | `dome/src/Terraria.Dome.Simulation/Physics/Definitions/CollisionPolicyDefinition.cs` | `Terraria.Dome.Simulation.Physics.Definitions` | proposed | 保存 `bottomFluff=40` 等只读几何常量 |
| `CollisionQueryPort` | `dome/src/Terraria.Dome.Simulation/Physics/Queries/CollisionQueryPort.cs` | `Terraria.Dome.Simulation.Physics.Queries` | proposed | 输入 position/velocity/tile snapshot，返回 immutable derived result |
| `CollisionDerivedResultProjection` | `dome/src/Terraria.Dome.Simulation/Physics/Projections/CollisionDerivedResultProjection.cs` | `Terraria.Dome.Simulation.Physics.Projections` | proposed | 将查询结果投影到 Physics/Spatial 消费格式，不成为 authority |

NLTX 同时存在 `src/Physics/CollisionResultComponent.cs`（struct）和 `src/SpatialSimulation/CollisionResultComponent.cs`（sealed proposed class），这是明确的整合风险；C08 不宣布其中任何一个为最终 owner。Dome `ContactResultComponent` 说明了不可变读取和 Replace/Clear 的组织方向，但不替代 Version4 调用图证据。

#### 9.7.2 10 个成员、来源和生命周期

| Version4 成员 | proposed 表达 | 默认/失效 | 状态分类 |
|---|---|---|---|
| `stair` | `CollisionQueryScratch.Stair` | `false` at SlopeCollision start; query end discarded | derived |
| `stairFall` | `CollisionQueryScratch.StairFall` | `false` at SlopeCollision start; query end discarded | derived |
| `honey` | `CollisionQueryScratch.Honey` | `false` at WetCollision start; query end discarded | derived |
| `shimmer` | `CollisionQueryScratch.Shimmer` | `false` at WetCollision start; query end discarded | derived |
| `sloping` | `CollisionQueryScratch.Sloping` | reset at slope query | derived |
| `up` | `CollisionQueryScratch.Up` | set/cleared by collision result path | derived |
| `down` | `CollisionQueryScratch.Down` | set/cleared by collision result path | derived |
| `bottomFluff` | `CollisionPolicyDefinition.BottomFluff` | `40` | definition |
| `contacts` | `CollisionQueryScratch.Contacts` | clear at `BuildTileContacts` start | query cache |
| `_cacheForConveyorBelts` | `CollisionQueryScratch.ConveyorEdgeTiles` | clear at `StepConveyorBelt` use | query cache |

The flags are not independent authority: `WetCollision` overwrites honey/shimmer at entry; `SlopeCollision` overwrites stair/stairFall/sloping; `BuildTileContacts` clears and fills contacts; `StepConveyorBelt` clears and fills conveyor edge tiles. A query caller must not read a flag after another query has started unless it owns an immutable result snapshot.

#### 9.7.3 纯查询边界和副作用

```text
Position/Velocity + immutable TileSnapshot + CollisionPolicyDefinition
  -> CollisionQueryPort
  -> CollisionQueryScratch (local only)
  -> CollisionDerivedResultProjection / C09 contact results
```

- C08 Query may read a supplied tile snapshot and compute flags; it must not write Tile, Entity, Player/NPC/Projectile state, network, persistence, logging or clock state.
- Physics movement owner consumes the result and commits position/velocity; Spatial contact owner consumes C09 values; Liquid contact/transmutation owners consume honey/shimmer as read-only facts.
- If a legacy adapter must update Version4 static flags, it must bracket one call, clear at entry/exit, and prove no concurrent callers share the same scratch. This is a compatibility risk, not the proposed core design.
- `bottomFluff` is a definition, not a mutable field; `contacts` and conveyor cache are bounded by the current query, not by world save or entity snapshot.

#### 9.7.4 focused verifier and non-split items

Verify WetCollision honey/shimmer reset and detection, SlopeCollision stair/up/down reset, TileCollision direction flags, contact list clear/replace, conveyor cache clear, nested/reentrant query isolation, no writes from pure query, immutable projection, and duplicate Physics/Spatial result owner resolution. Do not create one component per flag; they share invocation lifecycle and splitting would increase invalid combinations.

#### 9.7.5 当前实施结论：C08 在组件源码范围内阻塞

- 当前 NLTX 的 `src/Physics` 与 `src/SpatialSimulation` 已有两个 `CollisionResultComponent`，
  但 C08 设计要求的 `CollisionQueryScratch` 是一次调用的临时 scratch，不是长期 ECS entity/world
  state。将其改名为 `CollisionQueryScratchComponent` 会改变生命周期和所有权语义。
- C08 的 `contacts` 与 `_cacheForConveyorBelts` 还分别依赖未裁决的 `TileContact` 和 conveyor
  point 值类型；它们不能用 `object`、未命名 tuple 或第三个结果组件占位。`bottomFluff` 是
  Definition，七个 flags 由 Query 生命周期清空/覆盖，不能在组件中塞入查询行为。
- 因本任务只允许保存 ECS component 源码，C08 本 checkpoint 不新增文件，标记为 `blocked`；
  后续需要先完成 query-owned scratch、值对象和 Physics/Spatial result owner integration review，
  才能实施任何 C08 非组件代码。执行游标推进到 `C09`。

### 9.8 C09 设计 checkpoint：CollisionContactAndHurtResults

#### 9.8.1 proposed 边界

C09 是 `BuildTileContacts` 和 `HurtTiles` 的一次查询结果边界，不是长期实体组件，也不是 Tile、Player 或 NPC 的写入 owner。`TileContact` 表达一次固体 Tile 接触的方向、重叠量、坐标、坡度和类型；`HurtTile` 表达一次伤害 Tile 查询的首个命中或 `type = -1` 的未命中哨兵。建议把值对象和查询放在 Physics 领域，并让调用者只拿不可变结果快照。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `TileContactResult` | `dome/src/Terraria.Dome.Simulation/Physics/Results/TileContactResult.cs` | `Terraria.Dome.Simulation.Physics.Results` | proposed | 承载一次接触的只读 Side/Overlap/X/Y/Slope/Type |
| `HurtTileResult` | `dome/src/Terraria.Dome.Simulation/Physics/Results/HurtTileResult.cs` | `Terraria.Dome.Simulation.Physics.Results` | proposed | 承载一次伤害 Tile 命中或无命中结果 |
| `CollisionContactQuery` | `dome/src/Terraria.Dome.Simulation/Physics/Queries/CollisionContactQuery.cs` | `Terraria.Dome.Simulation.Physics.Queries` | proposed | 从显式 Tile snapshot、geometry 和 hazard context 计算不可变结果 |

`Results/` 与 `Queries/` 是 C09 已有稳定边界的按需子目录：结果类型不会拥有调用期 scratch，查询也不会执行伤害、声音、网络或 Tile mutation。若最终实现项目的规模不需要子目录，可在实施时扁平化到 `Physics/`，但不得因此改变 namespace、owner 或返回值语义。

#### 9.8.2 9 个成员、来源和状态分类

| Version4 成员 | proposed 表达 | 默认/失效 | 状态分类 |
|---|---|---|---|
| `TileContact.Side` | `TileContactResult.Side` | 由接触分支写入；无结果时不存在该 entry | derived result |
| `TileContact.Overlap` | `TileContactResult.Overlap` | 由几何交叠计算；至少为 1 的有效接触量 | derived result |
| `TileContact.X` | `TileContactResult.TileX` | 命中 Tile 坐标 | derived result |
| `TileContact.Y` | `TileContactResult.TileY` | 命中 Tile 坐标 | derived result |
| `TileContact.Slope` | `TileContactResult.Slope` | 来源 Tile 的 slope byte/int 值 | derived result |
| `TileContact.Type` | `TileContactResult.TileType` | 来源 Tile type | derived result |
| `HurtTile.type` | `HurtTileResult.TileType` | 命中时为 Tile type；未命中为 `-1` | derived result/sentinel |
| `HurtTile.x` | `HurtTileResult.TileX` | 命中坐标；未命中时不应被消费 | derived result |
| `HurtTile.y` | `HurtTileResult.TileY` | 命中坐标；未命中时不应被消费 | derived result |

Version4 `TileContact` 和 `HurtTile` 是 public mutable `struct`，但这里的 `proposed` 类型应使用构造后不可变的值语义，避免调用方修改已发布结果。命名可以保留 `X/Y` 与 `type/x/y` 的兼容映射，但内部最终 owner 不应暴露旧字段的可写别名。

#### 9.8.3 Version4 生命周期和结果顺序

- `BuildTileContacts` 在查询入口先对传入列表执行 `Clear()`，再按受限的 X/Y Tile 范围扫描；`TileContact` 的生成顺序由列、行和 Left/Right/Top/Bottom 及坡面分支决定。查询必须保留这个稳定顺序，不能通过 HashSet 或未排序并行结果替代。
- 接触计算读取 active/inactive、solid/solidTop、half-brick、slope、Tile type 和 geometry。`Position` 会在扫描前取整，世界边界使用 `Main.maxTilesX` 和 `Main.maxTilesY - 40` 的等价显式 snapshot policy。边界策略应成为输入，而不是读取全局 `Main`。
- `HurtTiles` 扫描同类 Tile 范围，结合 half-brick、suffocation、slope 和 `CanTileHurt` 资格；找到首个命中立即返回 `{ type, x, y }`，没有命中返回 `{ type = -1 }`。可影响 `CanTileHurt` 的 `Player.fireWalk`、world difficulty 和 Tile hazard tables 必须通过只读 `HurtTileQualificationContext` 传入。
- `HurtTileResult` 只报告资格和位置；Player/NPC 的伤害、免疫、音效和网络同步由外部 commit/effect owner 依据结果产生 command。C09 不保存 Player 引用，也不直接调用 `WorldGen`、`NetMessage` 或伤害方法。
- C08 的 `CollisionQueryScratch` 只在一次 query 期间存在；C09 生成后应复制/冻结结果并立即允许 scratch 清理。不得把 C08 的 `contacts` list 或 C09 返回 list 的可变引用跨 tick 保存。

#### 9.8.4 依赖方向、owner 和不拆分项

```text
Position/geometry + immutable TileSnapshot + hazard context
  -> CollisionContactQuery
  -> immutable TileContactResult list / HurtTileResult
  -> Physics movement, Projectile/Player/NPC hazard consumers
  -> explicit damage or movement commands
```

C09 的唯一 writer 是 `CollisionContactQuery` 的局部结果构造；Physics/Spatial 消费者不能回写结果。当前 NLTX 同时有 `src/Physics/CollisionResultComponent.cs` 与 `src/SpatialSimulation/CollisionResultComponent.cs`，因此实施前必须完成 integration review，不能创建第三个 `CollisionResultComponent` 或把 C09 值对象塞进任一现有长期组件而未经 owner 裁决。`TileContact` 的六个值共享一次扫描和排序不变量，`HurtTile` 的三个值共享首命中/哨兵不变量，按单字段继续拆分会制造无效组合。

#### 9.8.5 C09 focused verifier 与执行风险

验证计划：输入列表在查询开始时被逻辑替换而非外部引用复用；Left/Right/Top/Bottom 和 slope 接触顺序稳定；half-brick、solidTop、inactive、边界坐标和坡面 overlap 与参考一致；`HurtTile` 首命中、`type = -1` 哨兵、suffocation、world difficulty 和 `fireWalk` 条件正确；返回结果不可变；嵌套 query 不污染 C08 scratch；查询不写 Tile/Player/NPC/network/persistence；Physics 与 Spatial 两个现有结果 owner 的整合决策可被 verifier 捕获。当前未创建/运行 verifier，证据为 `partial`，仍需 `version-drift` 和 owner integration review。

#### 9.8.6 当前实施结论：C09 在组件源码范围内阻塞

- C09 的三个 proposed 类型分别是不可变结果值对象和纯 Query，不是可附加的 ECS component；
  创建 `TileContactResultComponent` 或 `HurtTileResultComponent` 会错误地把一次查询结果升级为
  长期实体状态。
- 当前没有可复用的 `TileContact`/`HurtTile` 值类型，且已有 `Physics.CollisionResultComponent`
  与 `SpatialSimulation.Components.CollisionResultComponent` owner 冲突。按本次范围不创建值对象、
  Query、第三个 result component 或任何伤害/Tile/network adapter。
- C09 标记为 `blocked`，没有新增源码或 build 证据；保存本 checkpoint 后执行游标推进到 `C10`，
  `pendingComponents` 为 `[C10, C11, C12, C13, C14]`。

### 9.9.1 当前实施 checkpoint：C10-C14 组件源码复核与串行重建

- C10-C14 可独立确定的组件源码已存在：C10 为 `RevengeMarkerIdentityComponent`、
  `RevengeMarkerLifecycleComponent`；C11 为 `RevengeTargetSnapshotComponent`；C12 为
  `RevengeMarkerValueComponent`、`RevengeRespawnAttemptComponent`；C13 为
  `RevengeMarkerRegistryComponent`；C14 沿用现有 `src/WorldStorage/PylonRegistryComponent.cs`
  与 `PylonRegistryEntry`，没有创建第二个 Pylon registry owner。
- 本 checkpoint 只复核并编译现有组件，没有新增同义组件，也没有改写 legacy marker、NPC、TileEntity、
  network、persistence 或 registry writer。C10/C12 的生命周期重叠、C11 的 NPC owner、C13 的
  registry/clock/projection 和 C14 的 owner/equality-diff 仍属于 integration review。
- 通过的实际命令均经 `Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 串行执行：
  `build .\src\DeathPenaltyAndRevenge\Terraria.DeathPenaltyAndRevenge.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`
  与 `build .\src\Teleportation\Terraria.Teleportation.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；两者均退出码 `0`、`0` warning、`0` error。
- 因跨域 owner 尚未裁决，C10-C14 仍保持 `partial`，`completedComponents` 仍为空；当前游标保持
`C10`，`pendingComponents` 为 `[C10, C11, C12, C13, C14]`。

### 9.9.2 当前实施审计 checkpoint：P01 组件源码映射完整性与串行构建复核

- 使用当前会话已绑定的 P01 `partition/sessionId/report` 完成只读源码映射审计，没有重新领取分区、
  生成 sessionId 或改变 owner。所有可独立确定的 P01 组件候选均已在 `src/` 找到：
  `LiquidFlowBudgetAndPanicStateComponent`、`LiquidBufferQueueStateComponent`、
  `WirePropagationScratchComponent`、`PumpTransferScratchComponent`、
  `WiringMechanismScheduleComponent`、`WiringDeviceCooldownComponent`、
  `RevengeMarkerIdentityComponent`、`RevengeMarkerLifecycleComponent`、
  `RevengeTargetSnapshotComponent`、`RevengeMarkerValueComponent`、
  `RevengeRespawnAttemptComponent`、`RevengeMarkerRegistryComponent`、
  `PylonRegistryComponent` 和 `PylonRegistryEntry`。
- 没有发现需要新增的独立 ECS 组件类型。C08 的 `CollisionQueryScratch` 仍是 query-owned、
  per-invocation scratch；C09 的 `TileContactResult`、`HurtTileResult` 和
  `CollisionContactQuery` 仍是值对象/Query 边界，且依赖未裁决的 contact 类型与
  Physics/Spatial result owner，因此没有创建组件占位类型。
- 在确认没有活动 `dotnet.exe`/`csc.exe` 编译进程后，依次通过
  `Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 构建
  `Terraria.WorldStorage.csproj`、`Terraria.WorldInteraction.csproj`、
  `Terraria.DeathPenaltyAndRevenge.csproj` 和 `Terraria.Teleportation.csproj`；四次退出码均为 `0`，
  均为 `0` warning、`0` error。产物分别确认位于：
  `Build/bin/Terraria.WorldStorage/Debug/net10.0/Terraria.WorldStorage.dll`、
  `Build/bin/Terraria.WorldInteraction/Debug/net10.0/Terraria.WorldInteraction.dll`、
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll` 和
  `Build/bin/Terraria.Teleportation/Debug/net10.0/Terraria.Teleportation.dll`。
- 本 checkpoint 只更新源码审计与构建证据；没有新增 System、Query、Command、Adapter、Projection、
  测试、项目文件或配置，也没有修改其他会话的源码。P01 仍为 `implementationStatus: partial`，
  `completedComponents: []`，`partialComponents` 保持 `[C01, C02, C03, C04, C05, C06, C07, C10, C11, C12, C13, C14]`，
  `blockedComponents: [C08, C09]`，不执行 runner 结算。

### 9.9.3 C14 组件生命周期最小单元实施 checkpoint

- 实际修改 `src/WorldStorage/PylonRegistryComponent.cs`：新增 `Replace`、`AdvanceTick` 和
  `Reset`。`Replace` 防御性复制 current/previous entry 列表、保存显式 cooldown 并递增
  `Revision`；拒绝负 cooldown 和 revision 溢出。`AdvanceTick` 只递减自身 cooldown，`Reset`
  清空两个快照并恢复 cooldown/revision 初值。
- RED 契约探针在旧 `Terraria.WorldStorage.dll` 上确认三个生命周期入口均缺失；源码保存后通过仓库
  wrapper 完成 GREEN build，并由不写入仓库的只读反射探针输出
  `C14_PYLON_COMPONENT_VERIFIER=PASS ASSERTIONS=6`。C14 继续为 `partial`，不加入
  `completedComponents`，因为本证据只覆盖组件容器而不覆盖跨域 owner。
- 该单元没有新增 System、Query、Command、Adapter、Projection、测试或 verifier 文件；组件不读取
  TileEntity/SceneMetrics，不执行 equality/diff、网络、存档、玩家加入或 Dust/UI 副作用。依赖影响仅为
  后续 registry commit owner 可以通过显式列表调用组件状态入口；唯一 owner、持久化和 projection
  仍需 integration review。

### 9.9.4 C13 registry ID 只读视图最小单元实施 checkpoint

- 实际修改 `src/DeathPenaltyAndRevenge/RevengeMarkerRegistryComponent.cs`：内部仍由
  `HashSet<RevengeMarkerId>` 保存 ID index，但公开 `MarkerIds` 改为 `FrozenSet` 快照；成功的
  register/unregister/clear 操作会刷新快照，重复或未分配 ID 的拒绝结果保持不变。
- RED 契约探针在旧 `Terraria.DeathPenaltyAndRevenge.dll` 上确认 `MarkerIds` 的运行时类型是可变
  `HashSet`；探针没有写入仓库。随后通过仓库 wrapper 串行 GREEN build，退出码 `0`、`0` warning、
  `0` error；不写入仓库的 focused reflection verifier 输出
  `C13_REGISTRY_VERIFIER=PASS ASSERTIONS=12`，运行时视图为 `FrozenSet` 实现，并验证注册、重复
  注册、旧快照不可变、注销和清空。C13 仍为 `partial`，不加入 `completedComponents`，因为完整
  registry owner 与 commit boundary 仍需 integration review。
- 该单元没有新增 System、Query、Command、Adapter、Projection、测试或 verifier 文件；没有复制
  marker payload、legacy `_markers`、lock、clock、network 或 persistence 状态。完整 registry owner
  和 register/remove commit boundary 仍需 integration review。

### 9.9.5 C14 registry alias-safe replacement correction checkpoint

- RED 回归探针以旧 `Terraria.WorldStorage.dll` 的 `CurrentPylons` 只读视图作为下一次
  `Replace` 输入，确认原实现会先清空内部 current list，导致替换结果从 1 条变为 0 条。
- 实际仅修改 `src/WorldStorage/PylonRegistryComponent.cs`：`Replace` 在清理 current/previous
  列表前先复制 `IReadOnlyList<PylonRegistryEntry>` 输入，再执行快照转移、cooldown 写入和
  `Revision` 递增。没有新增 owner、系统、查询、适配器、投影或其他状态。
- `Terraria.WorldStorage.csproj` 经 `Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 串行
  build 退出码 `0`、`0` errors、`15` 个既有 warnings；产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`。
  新 DLL 的不写入仓库 focused verifier 输出
  `C14_PYLON_ALIAS_FIX_VERIFIER=PASS ASSERTIONS=16`，覆盖防御性复制、alias-safe replace、
  current/previous 转移、cooldown、revision 和 reset。
- C14 仍为 `partial`：TileEntity/SceneMetrics 扫描、equality/diff、network/persistence 和唯一
  registry owner 仍未完成 integration review。

### 9.9 当前 checkpoint 状态

- C01、C02、C03、C04、C05、C06、C07、C08、C09 已完成设计记录并同步保存设计/执行文档。
- `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09]`；`currentComponent: C10`；`pendingComponents` 为 C10-C14。
- C09 只完成 proposed 结果/查询边界；没有新增 C# 文件，没有切换旧写者，没有执行编译、测试、网络或存档验证。

### 9.31 实施 checkpoint：C05 wiring propagation seam

- C05 已在 `src/WorldInteraction/Wiring/` 保存 propagation scratch、command、query、snapshot、
  commit port 和 system；现有 `WirePropagationScratchComponent` 作为唯一 C05/C06 scratch substrate，
  未创建重复的 `WiringPropagationStateComponent` 或 `WiringExecutionContextComponent`。本次实现
  补充 `QueueNextGate` 显式路径，并对队列读取提供防御性快照。
- C05 的可验证边界为：显式 bounds 预检、`CurrentUser` 的 `255` sentinel、`1..4` wire color、
  frontier/direction 配对、current/next gate 两阶段队列、lamp/completed/pixel 去重、end/reset
  状态清理和无副作用 Query。system 只提交给 `IWiringPropagationCommitPort`，不访问 Tile、网络、
  时钟或实体状态。
- C05 focused verifier 已输出 `C05_WIRING_PROPAGATION_VERIFIER=PASS`、`ASSERTIONS=38`；
  `Terraria.WorldInteraction` 串行 build 退出码 `0`、`0` warning、`0` error，产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。
- 该 checkpoint 不改变 C05 的 proposed owner 结论：旧 `Wiring.TripWire/HitWire`、完整
  `CheckLogicGate`、PixelBox/Tile frame、sound/NetMessage/NPC/item/projectile effects、C05/C06/C07
  coordinator 和旧 facade 仍未接入，因此 C05 保持 `partial`，执行游标推进到 C06。

### 9.10 C10 设计 checkpoint：RevengeMarkerExpirationAndIdentityState

#### 9.10.1 proposed 边界

C10 只拥有复仇标记的 deadline 和稳定 ID 分配/读取。它不拥有敌人位置或类型快照（C11）、金币价值和一次性复生锁（C12），也不拥有 marker registry（C13）。`IsExpired` 的最终判定是跨 C10/C12 的纯计算：`forceExpire || currentGameTick >= expirationTick`；C10 只提供 deadline，不能把 C12 的可变 flag 复制进来。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `RevengeMarkerLifecycleComponent` | `src/DeathPenaltyAndRevenge/RevengeMarkerLifecycleComponent.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed substrate | 持有 expiration tick；force-expire/attempt lock 的重叠必须由 C12 integration review 裁决 |
| `RevengeMarkerIdentityComponent` | `src/DeathPenaltyAndRevenge/RevengeMarkerIdentityComponent.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed substrate | 持有 non-negative/兼容范围内的 unique ID 值语义 |
| `RevengeMarkerIdAllocator` | `src/DeathPenaltyAndRevenge/RevengeMarkerIdAllocator.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 在 marker lifecycle owner 内串行分配 ID，不把静态计数器暴露为全局 authority |
| `RevengeExpirationPolicy` | `src/DeathPenaltyAndRevenge/RevengeExpirationPolicy.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 以 coin value 和 cache tick 计算 deadline；阈值来自显式定义 |
| `RevengeMarkerIdentityProjection` | `src/DeathPenaltyAndRevenge/RevengeMarkerIdentityProjection.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed projection | 对外提供只读 UniqueID；不反向写入 identity |

上述路径是目标组织草案；现有仓库没有 `DeathPenalty` 领域目录时，应在获准实施后按真实规模决定扁平化到 `DeathPenaltyAndRevenge`，不提前创建空目录。C10 的核心公开类型必须一文件一类型，且不复用泛化 `Manager`。

#### 9.10.2 8 个字段与属性、生命周期和不变量

| Version4 成员 | proposed 表达 | 默认/来源 | 状态分类 |
|---|---|---|---|
| `_uniqueIDCounter` | `RevengeMarkerIdentityAllocator.NextId` 的内部计数 | `0`；仅由唯一 registry/lifecycle owner 分配 | authority allocator state |
| `_expirationCompCopper` | `RevengeExpirationPolicy.CopperThreshold` | `Item.buyPrice(0,0,0,1)` 的数值定义 | immutable policy |
| `_expirationCompSilver` | `...SilverThreshold` | `Item.buyPrice(0,0,1)` | immutable policy |
| `_expirationCompGold` | `...GoldThreshold` | `Item.buyPrice(0,1)` | immutable policy |
| `_expirationCompPlat` | `...PlatinumThreshold` | `Item.buyPrice(1)` | immutable policy |
| `ONE_MINUTE` | `RevengeExpirationPolicy.OneMinuteTicks` | `3600` | immutable policy |
| `_expirationTime` | `RevengeMarkerExpirationState.ExpirationTick` | `cacheTick + calculated duration` | authoritative marker state |
| `_uniqueID` | `RevengeMarkerIdentity.Value` / `UniqueId` | supplied ID when restoring; otherwise allocator result | authoritative marker identity |
| `UniqueID` | read-only `UniqueId` projection | `_uniqueID` | public read projection |

`CalculateExpirationTime` 的当前源码按 copper/silver/gold/platinum 分段插值并额外增加 `18000` ticks；源码中同时声明了 `ONE_MINUTE`，但现有表达式出现了直接使用 `3600f` 的版本形态。该常量与表达式必须在 verifier 中锁定，不能仅由常量名推断行为。deadline 使用绝对 game tick，不能使用 wall clock；ID 分配与 expiration 计算均为 deterministic domain calculation。

#### 9.10.3 身份、过期和跨边界副作用

- 新 marker 的 constructor 在没有传入 ID 时消费 allocator；从网络/存档恢复时使用 supplied ID，但必须校验重复、负值和 allocator 是否需要前移，不能让客户端 ID 直接覆盖 server authority。
- `IsExpired(currentGameTick)` 不读取全局 `Main`，只接收显式 tick，并组合 C12 的 `ForceExpire`。过期判定本身不删除 registry entry，也不发送 `RemoveRevengeMarker`；C13 registry system 根据 decision 产生 removal projection。
- `UniqueID` 是 marker identity，不是 NPC slot、Player slot、network connection ID 或 tile-entity ID。V1456 `SyncRevengeMarker`/`RemoveRevengeMarker` 的协议字段宽度由 adapter 单独验证，不能把协议窄字段当成 C10 authority 的类型上限。
- source `WriteSelfTo` 会把 ID 与其他 C11/C12/C13 相关 marker 数据一起写出；C10 只提供 identity/deadline slice，完整 wire/persistence serializer 必须由 projection 组合各 slice，避免重复序列化或字段丢失。
- allocator 的进程重启、world reset、溢出和恢复续接尚未由当前证据裁决。默认 proposed 规则是 world reset 不重用仍可能出现在客户端 projection 中的 ID，持久化恢复必须先建立已占用 ID 集合，再继续分配；若兼容协议要求 signed `int` 范围，溢出应拒绝创建并产生显式 failure。

#### 9.10.4 依赖方向和不拆分项

```text
CoinValue + cacheTick + RevengeExpirationPolicy
  -> expiration deadline
Identity request / restored identity
  -> RevengeMarkerIdentityAllocator
  -> C10 marker identity/expiration slice
  -> C13 registry expiration query
  -> removal/spawn decision and one-way network/persistence projection
```

C10 的唯一写者是 marker lifecycle/identity owner；C11/C12/C13 只能通过只读 identity、deadline 和 decision contract 访问。阈值、一分钟 tick 常量和 ID counter 属于同一 identity/expiration policy family，不按单个常量拆成组件；`UniqueID` 只是同一 identity 的只读 projection。

#### 9.10.5 C10 focused verifier 与风险

验证计划：四个 coin threshold 边界和 interpolation、额外 18000 ticks、`currentTick == expirationTick` 的 inclusive expiry、force-expire 与 normal expiry 组合、默认/恢复 ID 分配、重复/负 ID 拒绝、allocator 溢出、world reset 与 persistence resume、同一 marker 重复 projection 的 ID 稳定性、协议窄字段不截断 authority，以及无 clock/network/registry mutation 的纯度。当前未创建/运行 verifier，证据为 `partial`；`ONE_MINUTE` 命名与表达式版本差异、恢复/溢出策略仍是 `version-drift`/integration review 项。

### 9.11 C11 设计 checkpoint：RevengeMarkerEnemyContextState

#### 9.11.1 proposed 边界

C11 是创建 marker 时复制的敌人上下文快照。它保存位置、固定大小的敌人判定框、复生/网络使用的 NPC ID、生命比例、discouragement 所需的原始 NPC type 与 AI style，以及 statue 来源标志。C11 不持有 NPC entity、`NPC` 引用、slot、当前生命或实时 AI；这些属于 NPC owner，复生资格只通过只读 query 读取 C11 快照。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `RevengeTargetSnapshotComponent` | `src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed substrate | 保存 marker 捕获时的 immutable enemy context |
| `RevengeEnemyContextPolicy` | `src/DeathPenaltyAndRevenge/RevengeEnemyContextPolicy.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 计算 enemy box、捕获资格和显式上下文映射 |
| `RevengeContextQuery` | `src/DeathPenaltyAndRevenge/RevengeContextQuery.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 以 Player/world snapshot 判断交叠和 discouragement 资格 |
| `IRevengeContextCommitPort` | `src/DeathPenaltyAndRevenge/IRevengeContextCommitPort.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 接收捕获/替换 context intent；不提交 NPC entity 状态 |

`RevengeTargetSnapshotComponent` 是 marker-owned state，不是 NPC entity component。若实施时 marker 采用 aggregate record 而非 ECS entity，名称可调整为 `RevengeTargetSnapshot`，但必须保留单一快照 owner 和不可变读取契约。

#### 9.11.2 10 个字段、语义和不变量

| Version4 成员 | proposed 表达 | 默认/来源 | 状态分类 |
|---|---|---|---|
| `ENEMY_BOX_WIDTH` | `RevengeEnemyContextPolicy.EnemyBoxWidth` | `2160` pixels | immutable policy |
| `ENEMY_BOX_HEIGHT` | `RevengeEnemyContextPolicy.EnemyBoxHeight` | `1440` pixels | immutable policy |
| `EnemyBoxSize` | policy 的只读 `Size` projection | `(2160f, 1440f)` | immutable policy projection |
| `_location` | `RevengeTargetSnapshotComponent.Location` | 捕获时 NPC center | authoritative snapshot |
| `_hitbox` | `RevengeTargetSnapshotComponent.EnemyHitbox` | `CenteredRectangle(Location, EnemyBoxSize)` | derived snapshot |
| `_npcNetID` | `SpawnNpcTypeId` | `CacheEnemy` 先应用 `RespawnEnemyID` 映射后的 netID | authoritative snapshot |
| `_npcHPPercent` | `CapturedLifeRatio` | `npc.GetLifePercent()` | authoritative snapshot |
| `_npcTypeAgainstDiscouragement` | `DiscouragementNpcTypeId` | 捕获时原始 `npc.type` | authoritative snapshot |
| `_npcAIStyleAgainstDiscouragement` | `DiscouragementAiStyle` | 捕获时 `npc.aiStyle` | authoritative snapshot |
| `_spawnedFromStatue` | `SpawnedFromStatue` | 捕获时 NPC 标志 | authoritative snapshot |

必须保留 `SpawnNpcTypeId` 与 `DiscouragementNpcTypeId` 的差异：Version4 `CacheEnemy` 可能把 `npc.netID` 映射为 `NPCID.Sets.RespawnEnemyID`，但仍把 `npc.type` 传给 discouragement 字段。不要用一个 `NpcTypeId` 合并二者，也不要在复生时重新从当前 NPC slot 读取上下文。

#### 9.11.3 捕获、查询和副作用隔离

- `CacheEnemy` 的资格筛选仍由 NPC/DeathPenalty integration 负责：boss、worm root、rarity、minimum coin、世界边界和 mapped netID 条件必须成为显式 `RevengeEnemyCaptureInput`，而不是让 C11 policy 读取 `Main`/NPC static state。
- 创建快照时把 NPC center、life ratio、type、AI style、statue flag 和 mapped spawn ID 一次性复制；之后 NPC 被销毁、slot 复用或 type 改变，不得回写已有 marker。
- `_hitbox` 是由 `Location` 和 `EnemyBoxSize` 派生的矩形；`Intersects(rectInner, rectOuter)` 的 current Version4 实现实际只使用 `rectOuter` 与 `_hitbox` 比较，`rectInner` 未参与判断。这一事实必须在 verifier 中锁定，不能凭参数名称猜测应使用 inner box。
- `WouldNPCBeDiscouraged(Player)` 是 query，不应保留 Player 引用。proposed query 接收 Player position/zone snapshot 和显式 world event snapshot，返回 discourage decision；`NPC` spawn、`Main.NewText`、NetMessage 和 active entity mutation 均在 C12/C13 adapter 外部执行。
- `_npcHPPercent` 的下限处理（`SpawnEnemy` 使用 `Math.Max(0.5f, ...)`）属于 C12 respawn policy；C11 只存捕获值，不在 snapshot constructor 中修改它。

#### 9.11.4 依赖方向与不拆分项

```text
NPC capture input + explicit world bounds
  -> RevengeEnemyContextPolicy
  -> RevengeTargetSnapshotComponent
  -> RevengeEnemyContextQuery + Player/world snapshots
  -> C12 respawn intent / NPC spawn adapter
```

C11 的唯一 writer 是 marker capture/replace commit；NPC owner 只提供 capture input，Player/world systems 只提供 query snapshot。位置、hitbox、NPC IDs、life ratio、AI style 和 statue flag 共享同一次捕获版本，拆成独立长期组件会允许版本不一致，因此不继续按字段拆分。

#### 9.11.5 C11 focused verifier 与风险

验证计划：enemy box 常量与 centered rectangle、capture 时 center/life/type/AI/statue 快照不可变、RespawnEnemyID 映射与 discouragement 原始 type 分离、NPC slot 复用不污染 marker、`Intersects` outer-only current behavior、Player inner/outer box 交叠、discouragement world/event branches、无 NPC/Player/Network/clock 写入，以及 capture qualification 的边界条件。当前未创建/运行 verifier，证据为 `partial`；NPC P03 owner、完整 `CacheEnemy` 调用时序和当前/完整参考行为仍需 integration review。

### 9.12 C12 设计 checkpoint：RevengeMarkerValueAndRespawnState

#### 9.12.1 proposed 边界

C12 拆出 marker 的金币价值和复生尝试控制。`_baseValue`/`_coinsValue` 是随 marker 一起传给新 NPC 的价值事实；`_forceExpire` 是被 discouragement/其他生命周期 decision 置为 terminal 的标志；`_attemptedRespawn` 与 `RespawnAttemptLocked` 是对当前玩家交叠窗口的一次性 attempt guard。C12 不拥有 C10 的 identity/deadline、C11 的敌人上下文、C13 的 marker list/clock，也不直接创建 NPC。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `RevengeMarkerValueComponent` | `src/DeathPenaltyAndRevenge/RevengeMarkerValueComponent.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 持有 base value 与 coin value 的只读 marker 事实 |
| `RevengeRespawnAttemptComponent` | `src/DeathPenaltyAndRevenge/RevengeRespawnAttemptComponent.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 持有 force-expire 和当前交叠窗口 attempt lock |
| `RevengeRespawnSystem` | `src/DeathPenaltyAndRevenge/RevengeRespawnSystem.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 根据 C10/C11/C13 输入产生 expire、spawn 或 unlock decision |
| `IRevengeRespawnCommitPort` | `src/DeathPenaltyAndRevenge/IRevengeRespawnCommitPort.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 提交 lock/value/spawn/remove intent，不直接写 NPC 或网络 |
| `RevengeRespawnProjection` | `src/DeathPenaltyAndRevenge/RevengeRespawnProjection.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 将 spawn/remove decision 交给 NPC/network adapter |

现有 `src/DeathPenaltyAndRevenge/RevengeMarkerLifecycleComponent.cs` 同时拥有 `ExpiresAtGameTime`、`ForceExpire` 和 `RespawnAttemptLocked`，而 `src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs` 同时拥有 `CoinValue` 与 `BaseValue`。这些是现有候选 substrate，不是新 C12 的授权最终 owner；实施前必须决定是拆出 C10/C12/C11 的字段、保留一个 aggregate 并以 view 映射，还是废弃候选层，禁止新增第二份可写状态。

#### 9.12.2 5 个成员、状态分类和不变量

| Version4 成员 | proposed 表达 | 默认/生命周期 | 状态分类 |
|---|---|---|---|
| `_baseValue` | `RevengeMarkerValueComponent.BaseValue` | capture 时 NPC `value`；spawn 时写入新 NPC | authoritative marker value |
| `_coinsValue` | `RevengeMarkerValueComponent.CoinsValue` | capture 时 NPC `extraValue`；用于 cache threshold/respawn value | authoritative marker value |
| `_forceExpire` | `RevengeRespawnAttemptComponent.ForceExpire` | `false`；discouraged/explicit expiry 后单向置 true | terminal lifecycle flag |
| `_attemptedRespawn` | `RevengeRespawnAttemptComponent.AttemptedRespawn` | `false`；离开玩家 outer box 后清 false | per-window guard |
| `RespawnAttemptLocked` | `RevengeRespawnAttemptComponent.IsLocked` projection | 只读映射 attempted flag | public read projection |

`_forceExpire` 当前没有对应的 clear 方法，因而 proposed 语义是 monotonic until marker removal/reset；不能把它当作每 tick 可复用的 attempt lock。`_attemptedRespawn` 则会在 marker 不再与任何 active/non-dead player 的 outer box 相交时清除，并允许下一次重新尝试。金币 value 不能在 respawn 时重新从 NPC 或 Item 计算，否则会把捕获事实和当前定义混用。

#### 9.12.3 Current Version4 顺序、复生和副作用边界

- `NPC.SpawnNPC` 先消费 `noSpawnCycle`；若没有该 gate，调用 `RevengeManager.CheckRespawns()`，之后才进入自然 `Spawner.SpawnNPC()`。这建立了“复仇尝试在自然刷新前”的当前顺序，但尚未证明两个路径在同一 tick 的 slot/容量/失败 arbitration 已闭合。
- `CheckRespawns` 在找到 active/non-dead player 列表后，先清理 expired/invalid marker，再逐个 marker 找到第一个交叠玩家；无交叠时清除 attempt lock；有交叠且已锁定时跳过；否则先设置 lock，再运行 discouragement query。
- discouragement 为 true 时只调用 `SetToExpire`，不 spawn；marker 会在下一次 registry cleanup 看到 force-expire。为 false 时调用 `SpawnEnemy`，将 C11 context 与 C12 value 组合成 spawn intent，成功后 marker 从 registry 删除并由 server 发 remove projection。
- `SpawnEnemy` 的 `NPC.NewNPC`、Center、special-spawning AI、`timeLeft += 3600`、`extraValue`/`value`/statue、HP floor `max(0.5, capturedLifeRatio)`、SyncNPC/extra-value network 和 `DisplayCaching` UI 全部是 adapter/commit side effects，不属于 C12 component 或纯 decision。
- proposed system 必须明确 external spawn commit 的失败语义。为保持当前行为的候选默认是“attempt lock 先提交；若 spawn commit 返回 rejected/unknown，产生可审计 failure，不自动静默重试”；是否允许 retry 必须由 NPC spawn owner 裁决，不能在 C12 猜测。

#### 9.12.4 依赖方向、自然复生竞争和不拆分项

```text
C10 identity/deadline + C11 enemy context + C12 value/attempt state
  + C13 active-player/registry query
  -> RevengeRespawnSystem
  -> attempt-lock / force-expire / spawn-or-remove decision
  -> IRevengeRespawnCommitPort
  -> NPC spawn owner + network/presentation projections
```

只有一个 spawn commit owner 可以消费 C12 的 spawn intent；自然 `Spawner`、复仇 respawn 和其他 NPC spawn requests 必须在该 owner 内按 scheduler contract 排序、容量检查和去重。`Value` 的两个字段共享 capture/spawn 版本，`ForceExpire` 与 attempt lock 共享 marker lifecycle 但具有不同单调性，继续按单字段拆分会破坏这些不变量。

#### 9.12.5 C12 focused verifier 与风险

验证计划：value capture/restore、base/coin 不混淆、force-expire monotonicity、无交叠 unlock、交叠 first-attempt lock、重复 tick skip、discouragement-to-expire、spawn success/remove、spawn reject/unknown、HP floor、timeLeft/value/statue command payload、`noSpawnCycle -> revenge -> natural spawn` 顺序、同 tick natural spawn arbitration、C10/C11/C13 slice consistency 和 no-direct-NPC/network side effects。当前未创建/运行 verifier，证据为 `partial`；自然刷新竞争、spawn failure retry、现有 Lifecycle/TargetSnapshot owner 选择仍未裁决。

### 9.13 C13 设计 checkpoint：RevengeRegistryAndCache

#### 9.13.1 proposed 边界

C13 拥有 marker 的 world/session registry、cache qualification policy、显式 simulation tick 和单向网络/join projection。它不拥有 marker 的 ID/deadline（C10）、enemy/value/attempt fields（C11/C12），也不把 `DisplayCaching`、锁对象或网络 packet 当成可存档的 ECS authority。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `RevengeMarkerRegistryComponent` | `src/DeathPenaltyAndRevenge/RevengeMarkerRegistryComponent.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed substrate | 以稳定顺序保存 active marker aggregate references/snapshots，并提供唯一 register/remove owner |
| `RevengeCachePolicy` | `src/DeathPenaltyAndRevenge/RevengeCachePolicy.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 保存 minimum coin policy；debug display 仅作 adapter input |
| `RevengePlayerProximityPolicy` | `src/DeathPenaltyAndRevenge/RevengePlayerProximityPolicy.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 保存 player inner/outer box geometry 并计算候选交叠 |
| `RevengePresentationPolicy` | `src/DeathPenaltyAndRevenge/RevengePresentationPolicy.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed adapter policy | 保存 DisplayCaching compatibility input；不成为 simulation authority |
| `RevengeClockState` | `src/DeathPenaltyAndRevenge/RevengeClockState.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 保存显式 simulation tick，唯一推进/重置 clock |
| `RevengeRegistrySystem` | `src/DeathPenaltyAndRevenge/RevengeRegistrySystem.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 捕获提交、tick、expired/invalid cleanup、respawn consumption 和 snapshot 输出的协调 owner |
| `IRevengeRegistryCommitPort` | `src/DeathPenaltyAndRevenge/IRevengeRegistryCommitPort.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 接收 register/remove/clock/reset intent；不执行 network/UI |
| `RevengeNetworkProjection` | `src/DeathPenaltyAndRevenge/RevengeNetworkProjection.cs` | `Terraria.DeathPenaltyAndRevenge` | proposed | 从 immutable registry snapshots 生成 add/remove/join projections |

现有 `src/DeathPenaltyAndRevenge/RevengeMarkerRegistryComponent` 只有 `HashSet<RevengeMarkerId>`，它能作为 ID index substrate，但不能单独替代保存完整 marker payload 的 registry。它与 C13 proposed registry 的关系必须通过 integration review 裁决：扩展为 index + aggregate owner、改为 projection index，或由唯一新 owner 接管；不得让 ID HashSet 与 legacy `_markers` 各自接受 register/remove 写入。

#### 9.13.2 11 个字段、状态分类和不变量

| Version4 成员 | proposed 表达 | 默认/生命周期 | 状态分类 |
|---|---|---|---|
| `DisplayCaching` | `RevengePresentationPolicy.DebugDisplayEnabled` / presentation adapter input | `false` | presentation/config, not authority |
| `MinimumCoinsForCaching` | `RevengeCachePolicy.MinimumCoins` | `Item.buyPrice(0,0,10)` | capture policy |
| `PLAYER_BOX_WIDTH_INNER` | `RevengePlayerProximityPolicy.PlayerInnerWidth` | `1968` pixels | immutable geometry definition |
| `PLAYER_BOX_HEIGHT_INNER` | `RevengePlayerProximityPolicy.PlayerInnerHeight` | `1200` pixels | immutable geometry definition |
| `PLAYER_BOX_WIDTH_OUTER` | `RevengePlayerProximityPolicy.PlayerOuterWidth` | `2608` pixels | immutable geometry definition |
| `PLAYER_BOX_HEIGHT_OUTER` | `RevengePlayerProximityPolicy.PlayerOuterHeight` | `1840` pixels | immutable geometry definition |
| `_playerBoxSizeInner` | policy `PlayerInnerBoxSize` | `(1968f,1200f)` | derived policy value |
| `_playerBoxSizeOuter` | policy `PlayerOuterBoxSize` | `(2608f,1840f)` | derived policy value |
| `_markers` | `RevengeMarkerRegistryComponent.ActiveMarkers` | empty at construction/reset; register/remove only through registry owner | authoritative world registry |
| `_markersLock` | legacy adapter synchronization boundary | object initialized once; not persisted or exposed | compatibility guard |
| `_gameTime` | `RevengeClockState.CurrentTick` | `0` at construction/reset; increment once per simulation update | authoritative world tick |

`_markers` 是唯一 active marker collection；registry 对外只给 immutable snapshot，不能泄露 mutable List。`_markersLock` 不是 ECS state：单线程 simulation 应以 scheduler ownership 替代它；若兼容期必须保留 lock，只能把 lock 限制在 registry commit adapter 的短临界区，网络发送不得持锁。`_gameTime` 是 simulation tick，不是 wall clock，且 C10 expiration、C12 attempt 和 C13 cleanup 必须读取同一 tick snapshot。

#### 9.13.3 注册、清理、时间和 projection 生命周期

- `CacheEnemy` 先做 boss/worm/rarity/minimum coins/world bounds 资格判断，再将 C10/C11/C12 slices 组装成 marker；C13 只负责原子 register 和后续 projection。register 成功后 legacy 会发送 marker add projection；新设计应先提交 immutable fact，再由 projection adapter 投递。
- `Update` 每次世界更新只推进一次 `CurrentTick`。Version4 的 `Main.Update` 在 `_gameUpdateCount++` 后调用 `NPC.RevengeManager.Update()`，随后进入 `NPC.SpawnNPC()`；scheduler contract 必须记录这条顺序，不能依赖文件位置。
- `CheckRespawns` 需要 active/non-dead player snapshot，按 player slot 的稳定顺序生成 inner/outer boxes；C13 提供只读候选查询，C12 产生 attempt/expire/spawn decision，C13 再以一个 commit batch 删除已消费 marker。
- `RemoveExpiredOrInvalidMarkers` 应先 materialize expired/invalid IDs，再从 registry remove，最后把 removal projection 放到 adapter queue。当前 Version4 先构造 lazy `IEnumerable`，随后 `RemoveAll`，再枚举原 enumerable；这可能导致被删除 marker 的 network removal 集合为空，属于必须由 focused verifier 锁定的 current behavior/bug-risk，不应在设计层静默“修复”。
- `Reset` 清空 marker registry 并把 clock 归零；C10 allocator 是否归零不由 C13 决定。WorldGen reset、server restart、load failure 和 persistence restore 必须使用显式 lifecycle commands。
- `SendAllMarkersToPlayer` 应在 registry commit 后取得 immutable join snapshot，释放 registry ownership，再由 network adapter 顺序发送。不能在 `_markersLock` 内执行 I/O；join snapshot 必须有 revision/identity 规则以应对 duplicate/stale clients。
- 当前 Version4 `NetMessage` case 126 通过 `RevengeMarker.WriteSelfTo` 写出完整 marker 字段，而 NLTX V1456 research 将 `SyncRevengeMarker` 定义为严格 2-byte `Int16 id`；case 127/remove 则是 4-byte unique ID。C13 必须把 V4 legacy serializer 与 V1456 protocol projection 分离并记录 `version-drift`，不能用任一 packet shape 反向定义 marker authority。

#### 9.13.4 存档、并发和不拆分项

当前没有证据表明 `_markers`、`_gameTime` 或 `DisplayCaching` 已有完整 WLD/存档恢复契约；`WriteSelfTo` 只是网络 writer。proposed persistence adapter 必须明确 marker slices、registry order/revision、clock resume、ID allocator resume、invalid/expired marker handling 和 partial-load failure，未裁决前不声称可持久化。

`_playerBoxSizeInner/Outer` 和四个尺寸常量共享一个几何 policy；`_markers`、lock、clock 共享 registry lifecycle，但 lock 不进入 component。按单个 box 维度、单个 list/lock 字段或单个 tick 再拆会导致 policy/registry 不一致，因此保持 C13 的最小内聚边界。

#### 9.13.5 C13 focused verifier 与风险

验证计划：register/remove 原子性、stable marker order、duplicate ID、empty registry、clock exactly-once tick/reset、minimum coin/world bounds handoff、inner/outer box dimensions、active/non-dead player snapshot、expired/invalid materialization、lazy-enumerable removal behavior、join snapshot after lock release、add/remove projection ordering、V4 full marker serializer vs V1456 narrow protocol shape、stale/duplicate packet handling、persistence resume/failure，以及 DebugDisplay 不影响 authority。当前未创建/运行 verifier，证据为 `partial`；完整存档格式、唯一 registry owner、现有 ID HashSet 与 legacy list 的整合仍未裁决。

### 9.14 C14 设计 checkpoint：TeleportPylonRegistry

#### 9.14.1 proposed 边界

C14 只拥有“当前可用水晶塔集合”的 registry 生命周期、刷新决策和单向 add/remove/join projection。TileEntity 是水晶塔事实的输入/结构 owner；`TETeleportationPylon.TryGetPylonType` 通过 adapter 转成显式 `PylonScanInput`；Player/NPC/Teleportation eligibility 不由 C14 直接决定。`SceneMetrics` 仅是外部派生环境查询 adapter，不进入 registry authority。

| 类型 | proposed 路径 | namespace | status | 责任 |
|---|---|---|---|---|
| `PylonRegistryComponent` | `src/Teleportation/PylonRegistryComponent.cs` 或经 integration review 确认的既有唯一 owner | `Terraria.Teleportation` | proposed/owner-decision | 保存 current/previous immutable registry snapshot、revision 和 refresh state |
| `PylonRegistryEntry` | 复用 `src/WorldStorage/PylonRegistryEntry.cs`、Dome entry，或确定一个 domain entry owner | owner-dependent | proposed/owner-decision | 保存 tile coordinate、pylon kind、tile-entity identity 和 validity；不得复制同义 entry |
| `PylonRefreshPolicy` | `src/Teleportation/PylonRefreshPolicy.cs` | `Terraria.Teleportation` | proposed | 保存 refresh cooldown 和“是否需要重建”的纯规则 |
| `PylonRegistryQuery` | `src/Teleportation/PylonRegistryQuery.cs` | `Terraria.Teleportation` | proposed | 提供 `HasPylonOfType` 和只读类型/位置查询 |
| `PylonRegistrySystem` | `src/Teleportation/PylonRegistrySystem.cs` | `Terraria.Teleportation` | proposed | 读取 TileEntity snapshot、重建 current/previous、生成差异 intents |
| `IPylonRegistryCommitPort` | `src/Teleportation/IPylonRegistryCommitPort.cs` | `Terraria.Teleportation` | proposed | 提交 replace/reset/revision，不执行网络或 SceneMetrics I/O |
| `PylonRegistryProjection` | `src/Teleportation/PylonRegistryProjection.cs` | `Terraria.Teleportation` | proposed | 从 immutable diff snapshot 生成 add/remove/join projection |
| `TileEntityPylonAdapter` | `src/Teleportation/Adapters/TileEntityPylonAdapter.cs` | `Terraria.Teleportation.Adapters` | proposed | 将 TileEntity/Tile frame/validity 转成显式 scan input |
| `SceneMetricsAdapter` | `src/Teleportation/Adapters/SceneMetricsAdapter.cs` | `Terraria.Teleportation.Adapters` | proposed | 只读交接 SceneMetrics 派生查询，不拥有 pylon list |

`src/WorldStorage/PylonRegistryComponent.cs`、`src/WorldStorage/PylonRegistryState.cs`、`dome/src/Terraria.Dome.Simulation/Teleportation/PylonRegistryComponent.cs` 和 `src/WorldInteraction/Structures/PylonStructureComponent.cs` 都是现有候选/存储/结构 substrate，不代表可以再创建第四个长期 registry。最终 owner 必须在 integration review 中选择一个；其余类型只能成为 persistence DTO、adapter view、兼容 facade 或被明确淘汰。

#### 9.14.2 5 个字段、生命周期和状态分类

| Version4 成员 | proposed 表达 | 默认/生命周期 | 状态分类 |
|---|---|---|---|
| `_pylons` | `PylonRegistryComponent.CurrentPylons` | empty at construction/reset；successful rebuild 后替换 | authoritative registry snapshot |
| `_pylonsOld` | `PylonRegistryComponent.PreviousPylons` | rebuild 前交换/复制；只用于 diff | previous snapshot/cache |
| `_cooldownForUpdatingPylonsList` | `PylonRegistryComponent.RefreshCooldownTicksRemaining` | `0` 允许 rebuild；update 后置为 policy cooldown 并递减 | scheduler state |
| `CooldownTimePerPylonsListUpdate` | `PylonRefreshPolicy.CooldownTicks` | Version4 `int.MaxValue` | immutable definition |
| `_sceneMetrics` | `SceneMetricsAdapter` 的外部实例引用或 query context | 不进入 registry/persistence；按调用 scope 使用 | adapter/cache, not authority |

`_pylons` 与 `_pylonsOld` 必须由同一个 registry owner 原子替换；对外只发布 immutable ordered snapshot。`_sceneMetrics` 在当前 Version4 文件中没有参与列表重建或 `HasPylonOfType`，所以不得为了字段完整性把它放进 component。若后续资格判断需要 SceneMetrics，应作为明确输入版本化，而不是隐式读取全局 `Main.SceneMetrics`。

#### 9.14.3 Version4 事实、扫描和 projection 顺序

- `Main.Initialize_AlmostEverything` 创建 `PylonSystem`；主更新循环调用 `PylonSystem.Update()`；`WorldGen` reset 调用 `PylonSystem.Reset()`；玩家加入流程调用 `PylonSystem.OnPlayerJoining(playerIndex)`。这些是 scheduler/lifecycle anchors，必须写入 coordinator contract，不能由文件顺序表达。
- `Update` 当前在 cooldown 大于 0 时递减并返回，否则把 cooldown 设为 `int.MaxValue`，随后交换 current/old list、清空 current，并枚举 `TileEntity.ByPosition.Values`。只有 `TETeleportationPylon.TryGetPylonType` 成功的 entity 才加入 current list。
- `HasPylonOfType` 只读 current list；Placement preview 通过它阻止相同类型的 placement。该查询不能读取 previous list、网络 projection 或 SceneMetrics，避免 projection 回流 authority。
- add/remove diff 当前使用 `Except`。但 Version4 `TeleportPylonInfo.Equals` 在当前源码是 stub（返回 `new bool()`），因此差异行为未闭合；proposed verifier 必须以完整参考/明确 identity equality 裁决位置+类型或 tile-entity identity，不能假设 `Except` 已按值工作。
- current Version4 `NetTeleportPylonModule` 把 selector、tile X/Y 和 pylon type 编码为 NetModule payload；NLTX V1456 decoder 固定 module ID 7 和 payload 长度/字段宽度。C14 projection 只能从 registry diff 生成 protocol DTO，客户端 packet 不能反向修改 registry。
- `OnPlayerJoining` 应先取得带 revision 的 immutable current snapshot，释放 registry owner，再由 network adapter 向指定 client 发送 add projection。加入同步不能持有 component lock，也不能把 old snapshot 当成 current。
- `SpawnInWorldDust` 是客户端 presentation effect，不属于 C14 registry；它应由独立 `PylonPresentationAdapter` 消费 pylon type/dust box result。C14 不调用 `Dust.NewDust`、音频、UI 或 Tile mutation。

#### 9.14.4 NLTX owner 冲突、存档和 SceneMetrics 边界

NLTX 的 WorldStorage entry 使用 `TileCoordinate + TileEntityId`，Dome entry 使用 `WorldTileCoordinate + int PersistentId`；两者字段近似但身份类型不同。WorldStorage component 保持 current/previous read-only views，state DTO 则暴露 mutable lists；Dome component 已有 `Replace`, `AdvanceTick`, `Clear` 和 revision。不能把三种容器同时当成 authority。建议 integration review 优先选择 Teleportation domain owner，WorldStorage 只保留序列化 DTO，Dome 通过 adapter/contract 消费 domain snapshot；但在裁决前不宣布该建议已落地。

`PylonRegistryState` 的 current/previous list、cooldown 和 revision 不能未经 versioned save/load contract 直接作为 WLD authority。恢复必须验证 entry identity 唯一性、kind 有效性、tile entity 是否仍存在、current/previous/revision 关系、cooldown 边界和 partial-load rollback；否则应从 committed TileEntity snapshot 重建并产生明确 recovery result。`SceneMetrics` 只提供派生环境查询，不能写回 registry、WorldStorage 或 TileEntity。

#### 9.14.5 依赖方向和不拆分项

```text
TileEntity snapshot + pylon tile validation
  -> TileEntityPylonAdapter
  -> PylonRegistrySystem / PylonRefreshPolicy
  -> PylonRegistryComponent current/previous + revisioned diff snapshot
  -> PylonRegistryProjection
  -> protocol/join adapter, teleport eligibility query, presentation adapter
```

`current`/`previous`/cooldown/revision 共享同一 refresh transaction；不按单个 list、坐标或 type 拆成多个 registry。SceneMetrics、network DTO、dust and join I/O remain adapters/projections. C14 不拥有 Teleportation player position、pylon placement mutation、tile entity lifecycle 或 teleport command execution。

#### 9.14.6 C14 focused verifier 与风险

验证计划：initial update、cooldown decrement/zero/overflow、current/previous atomic swap、stable entry ordering、duplicate TileEntity positions、invalid TileEntity/pylon type、placement `HasPylonOfType` query、`TeleportPylonInfo` equality/diff semantics、add/remove projection order、revision monotonicity、reset/restore/rebuild、join snapshot isolation、module 7 payload bounds、stale/duplicate packet rejection、SceneMetrics non-authority、dust side-effect isolation，以及 WorldStorage/Dome/domain owner uniqueness。当前未创建/运行 verifier，证据为 `partial`；Version4 equality stub、refresh trigger semantics、存档恢复和三个现有 registry substrate 的唯一 owner 仍需 integration review。

### 9.15 设计 checkpoint 状态（实施前基线）

- C01-C14 全部完成设计记录并同步保存设计/执行文档；覆盖权威报告的 113 个成员，未增加其他分区成员。
- `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；`currentComponent: none`；`pendingComponents: []`。
- C01-C14 在设计阶段均保持 proposed/计划边界；实施阶段的当前状态见下方 checkpoint。

### 9.16 实施 checkpoint：C10 最小单元

- 实际新增文件：`src/DeathPenaltyAndRevenge/RevengeExpirationPolicy.cs`、
  `src/DeathPenaltyAndRevenge/RevengeMarkerIdentityProjection.cs`。
- `RevengeExpirationPolicy` 只实现 Version4 已确认的 coin threshold、分段插值、额外
  `18000` tick 和显式 `currentGameTick` 输入；不读取全局时钟、不修改 marker registry、
  不发送网络或持久化数据。
- `RevengeMarkerIdentityProjection` 只读映射既有 `RevengeMarkerId`；没有接管
  `_uniqueIDCounter`，没有改变 `RevengeMarkerIdentityComponent` 的旧公共 API。
- 未实现：allocator、restored-ID 校验、重复/溢出策略、lifecycle writer 切换、C11-C14。
- `verificationStatus: not-run`：本 checkpoint 尚未运行 focused verifier 或 dotnet；不得把
  这两个文件描述为行为等价已验证。

### 9.17 实施 checkpoint：C11 最小单元

- 实际新增文件：`src/DeathPenaltyAndRevenge/RevengeProximityBox.cs`、
  `src/DeathPenaltyAndRevenge/RevengeEnemyContextPolicy.cs`、
  `src/DeathPenaltyAndRevenge/RevengeContextQuery.cs`。
- 实际修改文件：`src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs`。
- `RevengeProximityBox` 是不持有实体引用的不可变几何值对象；`RevengeEnemyContextPolicy`
  只提供 Version4 已确认的 `2160 x 1440` 敌人框；`RevengeContextQuery` 明确复现当前
  `Intersects` 只读取 outer box 的行为，inner box 不反向改变结果。
- 快照保留原有 `NpcNetId`/`NpcTypeId` 公共属性，并新增语义别名
  `SpawnNpcNetId`/`DiscouragementNpcTypeId` 与派生 `EnemyHitbox`；没有保存 NPC 引用、
  Player 引用或全局状态。
- 未实现：NPC 捕获资格、`RespawnEnemyID` 映射、discouragement 的 NPC/Player 环境分支、
  C12 respawn commit。上述行为证据依赖 P03/NPC owner，仍为 integration-review 缺口。
- `verificationStatus: not-verified`：已完成受影响项目的编译验证，但尚未运行 focused
  geometry/context verifier 或行为等价、网络、存档验证。

### 9.18 实施 checkpoint：C12 最小单元

- 实际新增文件：`src/DeathPenaltyAndRevenge/RevengeMarkerValueComponent.cs`、
  `src/DeathPenaltyAndRevenge/RevengeRespawnAttemptComponent.cs`、
  `src/DeathPenaltyAndRevenge/RevengeRespawnDecisionKind.cs`、
  `src/DeathPenaltyAndRevenge/RevengeRespawnDecision.cs`、
  `src/DeathPenaltyAndRevenge/RevengeRespawnSystem.cs`。
- 实际修改文件：`src/DeathPenaltyAndRevenge/RevengeMarkerLifecycleComponent.cs`、
  `src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs`。
- `RevengeMarkerValueComponent` 是 base/coin value 的单一内部承载；快照通过只读属性保持
  既有公共 API。`RevengeRespawnAttemptComponent` 是 force-expire/attempt lock 的单一内部
  承载；旧生命周期属性通过委托保持兼容。
- `RevengeRespawnSystem.Evaluate` 只读取显式布尔输入并返回 decision；
  `ApplyAttemptState` 只改变显式传入的 attempt state，不创建 NPC、不发送网络、不读取时钟。
  `RequestSpawn` 仅表示意图，不能被解释为 spawn 已成功。
- 已按 ECS 文件组织约束将 `RevengeRespawnDecisionKind` 从 decision 文件拆到同名文件；
  没有新增 `Manager`、`Helper` 或泛化 `Data` 类型。
- 未实现：NPC spawn commit、自然 spawn 同 tick arbitration、失败 retry、marker remove
  projection、C10/C11/C13 完整 slice assembly。上述行为仍需 NPC/registry integration review。
- `verificationStatus: not-verified`：C12 RED 契约已因类型缺失失败，但新增代码尚未运行 focused
  decision verifier 或项目构建。

### 9.19 实施 checkpoint：C10/C13/C14 纯策略单元

- C10 实际新增 `src/DeathPenaltyAndRevenge/RevengeMarkerIdAllocator.cs`。它是实例级、可测试
  的 allocator，支持显式 next value、恢复 ID 观察、重复 ID 拒绝和 `int.MaxValue` 耗尽失败；
  尚未接管旧 Version4 `_uniqueIDCounter`，也未决定 world reset/跨进程恢复 owner。
- C13 实际新增 `src/DeathPenaltyAndRevenge/RevengeCachePolicy.cs` 和
  `src/DeathPenaltyAndRevenge/RevengePlayerProximityPolicy.cs`。二者只计算 minimum coin
  资格和 inner/outer box，不保存 marker list、clock、lock、Player 或 network 引用。
- C14 实际新增 `src/Teleportation/PylonRefreshPolicy.cs`。它只复现 Version4 的 cooldown
  sentinel、递减和 refresh 判断；没有创建第二个 `PylonRegistryComponent`，没有读取或写入
  TileEntity、WorldStorage、Dome registry、SceneMetrics 或网络。
- C11 实际修正 `RevengeTargetSnapshotComponent.Capture`，显式保存 spawn `NpcNetId` 和
  discouragement `NpcTypeId` 两个输入；兼容构造函数仍把旧单值映射为两者相同。新增
  `DiscouragementAiStyle`、`CapturedLifeRatio` 只是只读语义投影。
- C10-C14 均为 partial，未完成的 owner integration 不得标成 completed；当前仍没有
  registry/clock/pylon equality-diff/TileEntity adapter、network/persistence projection 或
  runtime writer 切换。
- 本 checkpoint 的 verificationStatus 仍为 `not-verified`，等待集中 focused verifier 和
  串行构建结果。

### 9.20 实施 checkpoint：C11 构造路径修复

- 首次串行构建发现 `RevengeTargetSnapshotComponent` 的兼容构造函数与分离 capture 构造
  路径参数类型完全相同，产生 `CS0111`；该错误发生在 P01 源码编译阶段，退出码为 `1`，
  `0` warning、`1` error。
- 已仅修改 `src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs`：保留原八参数
  公共构造函数，在内部构造路径加入显式 `snapshotNpcTypeId` 参数，并让 `Capture` 独立传入
  spawn `NpcNetId` 与 discouragement `NpcTypeId`。没有改变旧公共构造签名或引入第二份状态 owner。
- 依赖影响仅限 C11 snapshot 的构造调用；C10/C12/C13/C14 和其他分区文件未修改。
- 第二次串行构建发现公共构造函数链多传一个 `NpcTypeId`，产生 `CS1729`；退出码为 `1`，
  `0` warning、`1` error。已移除该多余实参，保留显式双值 `Capture` 路径。
- 当时记录该 checkpoint 时，修复后的构建与 focused verifier 尚未运行；后续 9.21 已记录
  成功构建和 45 项 focused verifier 结果。`completedComponents: []`、
  `partialComponents: [C10, C11, C12, C13, C14]` 保持不变。

### 9.21 实施 checkpoint：C10-C14 focused 验证与串行构建

- `DeathPenaltyAndRevenge` 串行构建命令：

  ```powershell
  $dotnetArguments = @('build', '.\\src\\DeathPenaltyAndRevenge\\Terraria.DeathPenaltyAndRevenge.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
  & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
  ```

  退出码 `0`，`0` warning、`0` error；artifact：
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- `Teleportation` 串行构建使用同一参数形状，将项目替换为
  `src/Teleportation/Terraria.Teleportation.csproj`；退出码 `0`，`0` error，依赖
  `WorldStorage` 报告 `15` 个既有 `CS0649`/`CS0169` warning；artifact：
  `Build/bin/Terraria.Teleportation/Debug/net10.0/Terraria.Teleportation.dll`。
- focused verifier 是不写入仓库的 PowerShell 反射命令，加载上述两个项目及其
  `Terraria.Items`/`Terraria.Npc` 依赖 DLL，覆盖 C10-C14 的过期阈值/插值/包含边界、allocator
  分配/恢复/重复/溢出、C11 双 ID 与 outer-only 几何、C12 decision/attempt 状态、C13
  cache/player box 和 C14 cooldown；输出 `FOCUSED_VERIFIER=PASS`、`ASSERTIONS=45`，退出码 `0`。
- 本次 focused 结果只证明已实现的纯/只读最小单元；C10-C14 仍为 partial，
  `completedComponents: []`，`currentComponent: C14`，C01-C09 仍 pending。网络、存档、
  runtime writer 切换、registry owner 和行为等价验证仍未执行，整体 `verificationStatus`
  仅表示 `focused-c10-c14-passed`。

### 9.22 实施 checkpoint：C11 capture qualification Query

- 实际新增文件：`src/DeathPenaltyAndRevenge/RevengeEnemyCaptureBounds.cs`、
  `RevengeEnemyCaptureInput.cs`、`RevengeEnemyCaptureRejectionReason.cs`、
  `RevengeEnemyCaptureResult.cs`；修改文件：
  `src/DeathPenaltyAndRevenge/RevengeEnemyContextPolicy.cs`。
- `RevengeEnemyContextPolicy.EvaluateCapture` 将 Version4 `CacheEnemy` 的已确认筛选条件
  转为显式纯输入：boss、非 root `realLife`、正 rarity、最低 1000 coin、世界边界四项
  margin、以及 NPC owner 已完成映射后的非零 spawn net ID。输入使用 NPC `position` 左上角
  和 `width/height`，没有误把快照中心 `Location` 当作资格坐标。
- 该 Query 只返回 `RevengeEnemyCaptureResult`，不创建 marker、不修改旧 `CacheEnemy`、
  不读取 `Main`/NPC 静态状态、不调用 `RespawnEnemyID`、网络、时钟或持久化；映射 owner 和
  capture commit 仍由 NPC/legacy adapter 保持唯一写入责任。
- 源/目标依赖：`Version4/Terraria.GameContent/CoinLossRevengeSystem.cs:315-338` ->
  `RevengeEnemyCaptureInput`/`RevengeEnemyContextPolicy`；`WorldPosition`、`NpcNetId` 和
  `RevengeCachePolicy` 是现有显式输入依赖。新增类型均一文件一核心公开类型，没有新增
  泛化 `Manager`、`Helper` 或重复 registry。
- 当前仍未实现：`RespawnEnemyID` 字典适配器、NPC capture commit、discouragement Player/world
  分支、C12 spawn commit 和 C13/C14 owner integration；因此 C11 继续标记 `partial`。
- 当前 checkpoint 的 focused verifier 和串行 build 尚未运行；本 Query 不声明为已验证。

### 9.23 实施 checkpoint：C11 capture qualification build 与 focused verifier

- 首次串行 build 发现 `RevengeEnemyContextPolicy.cs` 因补丁保留旧类型块并追加新类型块而产生
  `CS1529`（退出码 `1`，1 error）；已删除重复块，保留单一 `RevengeEnemyContextPolicy`
  定义。该失败未被记为成功证据。
- 修复后通过仓库 wrapper 串行构建：
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments
  @('build','.\src\DeathPenaltyAndRevenge\Terraria.DeathPenaltyAndRevenge.csproj','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')`；退出码 `0`，warning `0`，error `0`。
- 产物已确认位于 `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的反射 focused verifier 输出 `C11_CAPTURE_VERIFIER=PASS`、`ASSERTIONS=18`、
  退出码 `0`；覆盖有效捕获、七类拒绝条件、边界包含语义和缓存阈值。首次 verifier 仅因
  PowerShell 调用 `in` 参数缺少 `[ref]` 失败，未触及业务断言；改为 `[ref]` 后通过。
- `verificationStatus` 更新为 `focused-c10-c14-plus-c11-capture-passed`；C11 仍为 partial，
  因此没有更新 `completedComponents`，也没有调用 runner 结算。

### 9.24 实施 checkpoint：C11 discouragement outer Query

- 实际新增文件：`src/DeathPenaltyAndRevenge/RevengeDiscouragementInput.cs`；修改文件：
  `src/DeathPenaltyAndRevenge/RevengeContextQuery.cs`。
- `RevengeContextQuery.IsNpcDiscouraged` 保留 Version4 `WouldNPCBeDiscouraged` 的外层顺序：
  AI style 2 读取 NPC owner 已计算的 `AiStyle2IsDiscouraged`，style 3 反转
  `AiStyle3IsNotDiscouraged`，style 6 只实现已确认的地下沙漠/类型白名单/世界表面分支，
  default 只按 spawn net ID `253`/`490` 与 eclipse/daytime 判断。
- Query 的输入只包含值类型和显式世界/玩家快照，不保存 NPC/Player 引用，不读取 `Main`、
  NPC 静态集合、时钟、网络或持久化；style 2/3 的完整 NPC 算法继续由 NPC owner 负责，
  P01 不复制其实时上下文或猜测 `NPCID.Sets` 内容。
- 依赖影响：C11 的 `RevengeTargetSnapshotComponent` 仍只提供 immutable context；本 Query
  不创建 marker、不修改 attempt/lifecycle/registry，也不改变旧 `WouldNPCBeDiscouraged` writer。
- focused verifier 和串行 build 已在后续 9.25 checkpoint 完成；本 checkpoint 的未闭合项为
  style 2/3 NPC adapter、C12 spawn commit 和 C13 registry owner。

### 9.25 实施验证 checkpoint：C11 discouragement outer Query

- 通过仓库 wrapper 串行构建 `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`：
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments
  @('build','./src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')`；退出码 `0`，`0` warning、`0` error。
- 产物已确认位于
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的 focused verifier 加载最新 DLL，使用 `[ref]` 调用 `in` 参数，输出
  `C11_DISCOURAGEMENT_VERIFIER=PASS`、`ASSERTIONS=20`，退出码 `0`。覆盖 style 2/3、style 6
  的 type 513 地下沙漠分支、type 10/39/95/117/510 表面分支、非白名单类型、表面边界、
  default spawn net ID 253/490/其他 ID，以及非有限世界表面值异常。
- 首次 verifier 失败是 PowerShell 将 `[double]::NaN` 作为文本参数传入的调用错误，未触及业务
  断言；改用变量传入 `NaN` 后通过。该调用错误不作为业务验证结果。
- 当前状态保持 `completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`、
  `currentComponent: C11`；`pendingComponents` 保持
  `[C10, C11, C12, C13, C14, C01, C02, C03, C04, C05, C06, C07, C08, C09]`。
  C11 仍未完成 NPC owner 适配和 capture commit，因此没有标记 completed，也没有 runner 结算。

### 9.26 实施回归 checkpoint：C10-C14 已实现纯单元

- 在同一 `DeathPenaltyAndRevenge` build 产物上运行不写仓库的 baseline focused verifier，
  输出 `C10_C14_BASELINE_VERIFIER=PASS`、`ASSERTIONS=29`，退出码 `0`。
- 覆盖 C10 expiration 分段/绝对 deadline、allocator 默认分配/恢复 ID/重复/未分配/耗尽，
  C12 spawn/unlock/force-expire decision 与 attempt state，C13 cache threshold、inner/outer
  proximity geometry 和 outer-only intersection，以及 C14 refresh/cooldown/负值拒绝。
- verifier 过程中三次 harness 调整均未修改源码：首次误设 gold threshold 期望值，随后修正为
  Version4 当前第三段插值的 `234000`；异常类型和 `int.MaxValue` 的 PowerShell 参数表达式
  也改为正确的 inner exception/局部变量断言。最终结果只采用修正后的 `PASS` 输出。
- 当前仍保持 `completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`、
  `currentComponent: C11`；该回归只证明已实现的纯/只读 substrate，没有证明 C10-C14 的
  runtime writer、registry、NPC spawn、network 或 persistence integration。

### 9.27 实施 checkpoint：C13 presentation policy

- 实际新增文件：`src/DeathPenaltyAndRevenge/RevengePresentationPolicy.cs`。
- `RevengePresentationPolicy` 只承载 Version4 `DisplayCaching` 的显式
  `DebugDisplayEnabled` 输入；它是 immutable presentation adapter value，不拥有 marker、
  registry、clock、NPC、网络或 UI I/O，也不会改变 C10-C14 的权威状态。
- 依赖影响仅限未来 presentation adapter 的输入映射；旧 `CoinLossRevengeSystem.DisplayCaching`
  仍是运行时 writer，未创建第二个显示 writer，也未修改旧 facade 或跨分区代码。
- 本文件保存后，新增源码已在后续 9.28 checkpoint 通过 build/verifier；当时
  `completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`，
  `currentComponent` 暂为 C13。

### 9.28 实施验证 checkpoint：C13 presentation policy

- 通过仓库 wrapper 串行构建 `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`：
  退出码 `0`，`0` warning、`0` error；产物为
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的 focused verifier 加载该新 DLL，验证
  `DebugDisplayEnabled=false/true` 两个显式输入，输出
  `C13_PRESENTATION_VERIFIER=PASS`、`ASSERTIONS=2`，退出码 `0`。
- `verificationStatus` 更新为
  `focused-c10-c14-plus-c11-capture-discouragement-presentation-passed`；
  `completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`、
  `currentComponent: C13`、`pendingComponents` 保持
  `[C10, C11, C12, C13, C14, C01, C02, C03, C04, C05, C06, C07, C08, C09]`。
  该证据不表示 C13 registry/clock/network/persistence owner 已裁决，也不执行 runner 结算。

### 9.29 实施 checkpoint：C13 explicit revenge clock state

- 实际新增文件：`src/DeathPenaltyAndRevenge/RevengeClockState.cs`。
- `RevengeClockState` 将 Version4 `CoinLossRevengeSystem._gameTime` 的已确认生命周期转为
  显式 state：默认 tick 为 `0`，`Advance()` 每次只推进一个 tick 并返回新值，`Reset()`
  归零；状态不读取 wall clock、`Main`、网络、存档或 marker registry。
- `unchecked` 递增保留 Version4 `int` 计数器的自然溢出语义；没有新增持久化恢复或跨进程
  续接契约，也没有让该 state 接管 legacy `_gameTime` writer。C13 registry coordinator
  仍需在 integration review 后决定唯一 clock commit owner。
- 源码保存后，`completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`、
  `currentComponent: C13`、`pendingComponents` 保持不变；本单元等待串行 build/verifier。

### 9.30 实施验证 checkpoint：C13 explicit revenge clock state

- 通过仓库 wrapper 串行构建 `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`：
  退出码 `0`，`0` warning、`0` error；产物为
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的 focused verifier 输出 `C13_CLOCK_VERIFIER=PASS`、`ASSERTIONS=5`，退出码 `0`；
  覆盖默认 tick、连续单 tick 推进、reset 和 `int.MaxValue` 后 unchecked 溢出。
- 当前状态保持 `completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`、
  `currentComponent: C13`、`pendingComponents` 保持原值。该结果只证明 clock substrate，
  不证明 C13 唯一 clock commit owner、scheduler、restore/resume、registry、网络或存档已闭合。

### 12. 2026-09-11 C01 budget substrate implementation checkpoint

- 已实际保存源码：`src/WorldStorage/LiquidFlowBudgetPolicy.cs`、
  `src/WorldStorage/LiquidFlowBudgetDecision.cs`、
  `src/WorldStorage/LiquidFlowBudgetQuery.cs`、
  `src/WorldStorage/LiquidFlowBudgetAndPanicStateComponent.cs`。
- 首个实现单元表达 Version4 已确认的 `50000` buffer 上限、`25000` 默认 liquid 上限、
  reduced 模式 `5000` 上限、`cycles = 10`、buffer `45000` 高水位、panic counter `> 3600`
  进入条件和 reduced 模式 `numLiquid > 2000` 的 quick-fall 条件。Query 只接受显式数值与
  policy 输入；state component 只通过构造/Reset 建立初始值，tick 写入口仍未接入。
- 实际项目边界为现有 `src/WorldStorage/Terraria.WorldStorage.csproj`、命名空间为
  `Terraria.WorldStorage`；设计阶段的 Dome 路径不能在本任务用户限定的 `src/` 输出范围外写入。
  现有 `LiquidWorldRuntimeStateComponent` 仍未修改，避免产生第二个 active writer。
- 依赖影响：只使用 BCL；没有 Tile、WorldGen、Main、网络、存档、时钟或日志副作用。尚未
  创建 `ILiquidFlowCommitPort`/`LiquidFlowSystem`，也未切换 `Liquid.ReInit`/`UpdateLiquid`。
- RED 契约检查在源码新增前按预期失败：构建产物中不存在
  `Terraria.WorldStorage.LiquidFlowBudgetPolicy`。源码保存后尚未完成 GREEN 构建和 focused
  verifier；因此本单元 `verificationStatus: not-verified`，不能把 C01 标为 completed。
- 当前 metadata：`completedComponents: []`；`partialComponents: [C01, C10, C11, C12, C13, C14]`；
  `currentComponent: C01`；`pendingComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`。

### 13. 2026-09-11 C01 budget/system commit verification checkpoint

- 已保存并验证的 C01 源码为：`src/WorldStorage/LiquidFlowBudgetPolicy.cs`、
  `LiquidFlowBudgetDecision.cs`、`LiquidFlowBudgetQuery.cs`、
  `LiquidFlowBudgetAndPanicStateComponent.cs`、`LiquidFlowTickInput.cs`、
  `LiquidFlowStateUpdate.cs`、`LiquidFlowCommitResult.cs`、`ILiquidFlowCommitPort.cs`、
  `LiquidFlowStateCommitPort.cs`、`LiquidFlowTickResult.cs` 和 `LiquidFlowSystem.cs`。
- 发现并修正一个 Version4 顺序边界：`Liquid.UpdateLiquid` 先递增 `panicCounter`，再以
  `> 3600` 判断；`LiquidFlowBudgetQuery.Evaluate` 现在以 `nextPanicCounter` 判断，因而
  `bufferedLiquidCount = 45000`、旧值 `panicCounter = 3600` 的 tick 会进入 panic。修复前的
  RED 检查输出为 `C01_PANIC_BOUNDARY_RED=EXPECTED_FAIL observed=False expected=True`。
- 串行构建命令为：
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1`，参数为
  `build ./src/WorldStorage/Terraria.WorldStorage.csproj -m:1 -nr:false
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`。
  退出码 `0`，`0` error，`15` 个 warning（均为既有 WorldStorage 未使用/未赋值字段警告），
  产物为 `Build/bin/Terraria.WorldStorage/Debug/net10.0/Terraria.WorldStorage.dll`。
- focused verifier 未写入仓库，加载上述 DLL 后输出
  `C01_BUDGET_SYSTEM_VERIFIER=PASS`、`ASSERTIONS=30`、退出码 `0`。覆盖默认/reduced 预算、
  可用容量、缓冲高水位和 panic 边界、quick-fall、state 构造/reset、system commit、panic
  状态转换，以及非法 commit 被拒绝且不产生部分状态写入。
- 本 checkpoint 只证明 C01 shadow substrate 的源码、构建和 focused 行为；旧 Liquid writer、
  Tile/liquid authority、C02/C03 队列交接、网络/存档和世界生命周期仍没有唯一 owner 证据。
  因此 `C01` 仍为 `partial`，不更新 `completedComponents`，不调用 runner 结算。
- 当前 metadata：`completedComponents: []`；`partialComponents: [C01, C10, C11, C12, C13, C14]`；
  `currentComponent: C02`；`pendingComponents: [C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；
  `verificationStatus: c01-budget-system-commit-focused-passed-30-assertions; c02-queue-seam-not-verified; focused-c10-c14-plus-c11-capture-discouragement-presentation-clock-passed`。

### 14. 2026-09-11 C02 queue seam implementation checkpoint

- 已实际保存源码：`src/WorldStorage/LiquidCellWorkItemStateCommand.cs`、
  `LiquidWorkItemReadiness.cs`、`LiquidWorkItemReadyQuery.cs`、
  `LiquidWorkQueueOperationResult.cs`、`LiquidWorkQueueDrainStatus.cs`、
  `LiquidWorkQueueDrainResult.cs`、`ILiquidWorkQueueCommitPort.cs`、
  `LiquidWorkQueueStateCommitPort.cs` 和 `LiquidWorkQueueSystem.cs`。
- `LiquidCellWorkItemStateCommand` 只携带权威报告中的 `X`、`Y`、`Kill`、`Delay`，并以
  非负校验和显式 `Coordinate` 表达工作项；`LiquidWorkItemReadyQuery` 是纯计算，接收
  调用方提供的 world dimensions 与 kill threshold，输出 ready/remove/delay-decrement
  结果。没有读取 `Main`、Tile、时钟、随机源、网络或存档。
- `LiquidWorkQueueStateCommitPort` 复用现有 `LiquidWorkQueueState`，提供 enqueue、重复坐标
  幂等、requeue、remove、reset 和复制快照；由于现有 `LiquidWorkEntry` 的 delay/kill 是
  `byte`，超出 `byte.MaxValue` 的 command 明确拒绝，禁止静默截断。`LiquidWorkQueueSystem`
  只通过 `ILiquidWorkQueueCommitPort` 执行 bounds 检查、delay 递减、kill 清理和 ready drain。
- 该单元仍未构建或运行 focused verifier；`C02` 状态为 `partial/not-verified`。没有接入旧
  `Liquid.AddWater`、`UpdateLiquid`、Tile `checkingLiquid` writer、C03 buffer 或 C04 dirty
  publication，也没有宣布 WorldStorage queue 为最终 runtime owner。
- 当前 metadata：`completedComponents: []`；`partialComponents: [C01, C02, C10, C11, C12, C13, C14]`；
  `currentComponent: C02`；`pendingComponents: [C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；
  `verificationStatus: c01-budget-system-commit-focused-passed-30-assertions; c02-queue-seam-not-verified; focused-c10-c14-plus-c11-capture-discouragement-presentation-clock-passed`。

### 15. 2026-09-11 C02 queue seam verification checkpoint

- C02 实际修改的 9 个源码文件为：`src/WorldStorage/LiquidCellWorkItemStateCommand.cs`、
  `LiquidWorkItemReadiness.cs`、`LiquidWorkItemReadyQuery.cs`、
  `LiquidWorkQueueOperationResult.cs`、`LiquidWorkQueueDrainStatus.cs`、
  `LiquidWorkQueueDrainResult.cs`、`ILiquidWorkQueueCommitPort.cs`、
  `LiquidWorkQueueStateCommitPort.cs` 和 `LiquidWorkQueueSystem.cs`。
- 通过仓库串行 wrapper 构建 `src/WorldStorage/Terraria.WorldStorage.csproj`：退出码 `0`，
  `0` errors，`15` warnings；警告均为该项目已有的未使用/未赋值字段警告。产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`。
- focused verifier 为不写入仓库的 PowerShell reflection 检查，输出
  `C02_QUEUE_VERIFIER=PASS`、`ASSERTIONS=40`、退出码 `0`。覆盖 command schema、非负字段、
  readiness/kill threshold、delay decrement、bounds、duplicate/capacity/byte-range rejection、
  FIFO ready drain、deferred requeue、empty queue、read-only snapshot、kill removal 和 reset。
- 该证据只证明 C02 shadow queue seam 的实现和确定性行为；没有接入旧 `Liquid.AddWater`、
  `Liquid.UpdateLiquid`、Tile `checkingLiquid` writer、C03 buffer 或 C04 dirty publication，
  因此 C02 仍为 `partial`，不更新 `completedComponents`。C02 的 owner、C02/C03 capacity handoff、
  Tile checking intent 和 WorldStorage/Dome work-item 转换仍需 integration review。
- 保存 checkpoint 后推进执行游标：`currentComponent: C03`；
  `pendingComponents: [C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；
  `verificationStatus: c01-budget-system-commit-focused-passed-30-assertions; c02-queue-seam-focused-passed-40-assertions; focused-c10-c14-plus-c11-capture-discouragement-presentation-clock-passed`。

### 16. 2026-09-11 C03 buffer queue verification checkpoint

- C03 实际新增源码为：`src/WorldStorage/LiquidBufferQueueStateComponent.cs`、
  `LiquidBufferCommandKind.cs`、`LiquidBufferCommand.cs`、
  `LiquidBufferCheckingIntentKind.cs`、`LiquidBufferCheckingIntent.cs`、
  `LiquidBufferCommitStatus.cs`、`LiquidBufferCommitResult.cs`、
  `ILiquidBufferCommitPort.cs`、`LiquidBufferCommitPort.cs`、
  `LiquidBufferDrainStatus.cs`、`LiquidBufferDrainResult.cs` 和
  `LiquidBufferCommitSystem.cs`。
- `LiquidBufferQueueStateComponent` 复用 `LiquidBufferEntry` 的坐标值对象，默认从
  `LiquidFlowBudgetPolicy.MaximumBufferLength = 50000` 保留 Version4 的两个槽位，实际入队上限为
  `49998`；它只保存 deferred coordinate、计数和去重索引，不复制 Tile liquid amount/type。
- `LiquidBufferCommitPort` 负责有界 enqueue、坐标去重、FIFO head release、reset 和防御性 snapshot；
  `LiquidBufferCommitSystem` 在 C02 active queue 接受或幂等 duplicate 后才 release buffer entry，
  C02 拒绝时保留 entry 并返回可重试结果。Tile `checkingLiquid` 只通过 set/clear intent 返回，
  不直接写 `TileLiquidWorkStateComponent`，也不发布网络消息。
- 通过仓库串行 wrapper 构建 `src/WorldStorage/Terraria.WorldStorage.csproj`：退出码 `0`，
  `0` errors，`15` warnings；警告均为既有 WorldStorage 字段警告。产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`。
- focused verifier 为不写入仓库的 PowerShell reflection 检查，输出
  `C03_BUFFER_VERIFIER=PASS`、`ASSERTIONS=36`、退出码 `0`。覆盖默认上限、`49998` 保留槽位边界、
  duplicate、checking-liquid no-op、bounds、capacity rejection、read-only snapshot、C02 reject
  retain/retry、successful drain/release、active duplicate、FIFO head protection 和 idempotent reset。
- C03 仍为 `partial`：没有接管旧 `LiquidBuffer.AddBuffer/DelBuffer`、旧 Tile writer 或
  `Liquid.UpdateLiquid` drain callsite；reset 时现有 Tile checking flag 的完整清理集合和
  WorldStorage/Dome owner 仍待 integration review，不更新 `completedComponents`。
- 保存 checkpoint 后推进执行游标：`currentComponent: C04`；
  `pendingComponents: [C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；
  `verificationStatus: c01-budget-system-commit-focused-passed-30-assertions; c02-queue-seam-focused-passed-40-assertions; c03-buffer-seam-focused-passed-36-assertions; focused-c10-c14-plus-c11-capture-discouragement-presentation-clock-passed`。

### 9.40 C10 lifecycle deadline invariant correction checkpoint

- 本 checkpoint 实际修改文件仅为 `src/DeathPenaltyAndRevenge/RevengeMarkerLifecycleComponent.cs`；
  在构造时拒绝负的 `expiresAtGameTime`，保留既有只读 `ExpiresAtGameTime`、`ForceExpire`、
  `RespawnAttemptLocked` 和显式 `IsExpiredAt` 语义。
- 修改前的不写文件 RED reflection probe 输出：
  `C10_LIFECYCLE_EXPIRATION_RED=CONFIRMED ASSERTIONS=1 ACCEPTED=RevengeMarkerLifecycleComponent.ExpiresAtGameTime=-1`，退出码 `1`。
- RED 结果确认组件会接受无效的负 deadline；源码修订后尚未运行 build，因此本 checkpoint 的
  `verificationStatus` 为 `c10-lifecycle-expiration-invariant-not-verified`。C10 继续为 `partial`，
  `completedComponents` 保持为空。
- 本 checkpoint 不修改 `RevengeMarkerIdAllocator`、`RevengeMarkerIdentityComponent`、C12 respawn
  state、C13 registry/clock、legacy marker writer、network 或 persistence；这些依赖继续等待 owner review。

### 9.41 C10 lifecycle deadline invariant verification checkpoint

- 按 BUILD-CONCURRENCY-1 确认无活动 `dotnet.exe`/`csc.exe` 后，通过仓库 wrapper 串行构建
  `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`；退出码 `0`、`0` warning、
  `0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的 focused reflection verifier 输出
  `C10_LIFECYCLE_EXPIRATION_GREEN=PASS ASSERTIONS=13`，退出码 `0`；覆盖负 deadline 拒绝、零/最大
  deadline 接受、inclusive expiration、force-expire 覆盖和 attempt lock 设置/释放。
- 验证只覆盖组件级 deadline/lifecycle 契约；C10 allocator/legacy writer、C12 respawn、C13
  registry/clock、network、persistence 和跨域 owner 仍未闭合，因此 C10 不标记 `completed`。

### 9.37 C07 bounded component invariant correction checkpoint

- 本 checkpoint 实际修改文件仅为 `src/WorldInteraction/Wiring/WiringMechanismScheduleComponent.cs` 和
  `src/WorldInteraction/Wiring/WiringDeviceCooldownComponent.cs`。通过 private backing field 保留既有属性名；
  `Count` 现在只接受 `0..MaximumEntryCount`，三个 device cooldown 只接受非负值，非法输入抛出
  `ArgumentOutOfRangeException`，默认值和 `Reset()` 仍为零。
- 修改前的不写文件 RED reflection probe 输出：
  `C07_COMPONENT_INVARIANT_RED=CONFIRMED ASSERTIONS=5 ACCEPTED=WiringMechanismScheduleComponent.Count=-1,WiringMechanismScheduleComponent.Count=1001,WiringDeviceCooldownComponent.CannonCooldownTicks=-1,WiringDeviceCooldownComponent.BunnyCannonCooldownTicks=-1,WiringDeviceCooldownComponent.SnowballCannonCooldownTicks=-1`，退出码 `1`。
- RED 结果确认原内部 setter 没有执行设计要求的 bounds 校验；修订后尚未运行 build，因此本 checkpoint 的
  `verificationStatus` 为 `c07-invariant-correction-not-verified`。既有 13 项 C07 verifier 只代表修订前基线。
- 本 checkpoint 不创建 policy/system/query，不接入 `_mechX/_mechY/_mechTime`、`_numMechs`、`CheckMech`、
  `UpdateMech`、Tile frame、network 或机制效果；C07 继续为 `partial`，`completedComponents` 保持为空。

### 9.38 C07 bounded component invariant verification checkpoint

- 通过仓库 wrapper 从根目录串行构建 `src/WorldInteraction/Terraria.WorldInteraction.csproj`：实际命令为
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\WorldInteraction\Terraria.WorldInteraction.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`；退出码 `0`，`0` warning，`0` error。
- 已确认产物存在于 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。
- 不写入仓库的 focused reflection verifier 输出 `C07_COMPONENT_INVARIANT_GREEN=PASS ASSERTIONS=22`，退出码 `0`；覆盖默认 `Count`/cooldown、`Count=0/1000`、`Count=-1/1001` 拒绝、三个 cooldown 的非负值和负值拒绝、`Int32.MaxValue`，以及 `Reset()` 恢复零值并清空条目。
- 验证只覆盖组件契约；旧机制数组 writer、`CheckMech`/`UpdateMech`、Tile frame、network、机制效果和最终 owner 仍是 evidence-gap，因此 C07 不标记 `completed`。

### 9.9.6 P01 组件契约审计 checkpoint

- 本 checkpoint 重新读取当前 P01 设计/执行文档和 C10-C14 组件源码，未发现新的、能够在不
  改变 owner 或跨域语义的前提下修复的组件级状态缺陷，因此没有新增或修改 `src/` 源码。
- 对 `RevengeMarkerIdentityComponent`、`RevengeMarkerLifecycleComponent`、
  `RevengeTargetSnapshotComponent`、`RevengeMarkerValueComponent`、
  `RevengeRespawnAttemptComponent`、`RevengeMarkerRegistryComponent`、
  `PylonRegistryEntry` 和 `PylonRegistryComponent` 执行了不写文件的 reflection contract audit，
  输出 `P01_COMPONENT_CONTRACT_AUDIT=PASS ASSERTIONS=26`。覆盖 identity/lifecycle/value/attempt/context
  快照、registry immutable view、entry validity、current/previous snapshot、cooldown、revision、
  reset 和 alias-safe replacement。
- 按 BUILD-CONCURRENCY-1 先确认没有活动 `dotnet.exe`/`csc.exe`，再依次通过
  `Build/Tools/Invoke-SerialDotnet.ps1` 构建 `Terraria.DeathPenaltyAndRevenge.csproj`、
  `Terraria.WorldStorage.csproj` 和 `Terraria.Teleportation.csproj`。三次退出码均为 `0`，
  均为 `0` warning、`0` error；产物均确认位于 `Build/bin/` 对应的 `Debug/net10.0` 目录。
- 本审计不证明 NPC spawn、旧 marker writer、TileEntity scan、SceneMetrics、network、persistence
  或跨域 lifecycle integration。C10-C14 仍为 `partial`，`completedComponents: []`，
  `blockedComponents: [C08, C09]`；当前游标继续为 `C10`，`pendingComponents` 继续为
  `[C10, C11, C12, C13, C14]`。

### 9.36 验证 checkpoint：C06 final cleanup patch

- 最终 cleanup-only 源码修订已保存并验证，实际修改的 C06 源文件为
  `src/WorldInteraction/Wiring/WiringTraversalAdapter.cs`；此前同一 C06 单元已保存
  `PumpTransferScratchComponent.cs`、`WirePropagationScratchComponent.cs`、
  `PumpTransferPolicy.cs`、`PumpTransferCommand.cs`、`PumpTransferCommandBatch.cs`、
  `PumpTransferCommandBatchStatus.cs`、`WiringTeleportCommand.cs`、
  `IWiringTraversalCommitPort.cs`、`WiringTraversalCommitResult.cs`、
  `WiringTraversalCommitStatus.cs`、`WiringTraversalFlushResult.cs` 和
  `WiringTraversalFlushStatus.cs`。
- 通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建
  `src/WorldInteraction/Terraria.WorldInteraction.csproj`，退出码 `0`，警告 `0`，错误 `0`；
  产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。
- 不写入仓库的 focused verifier 输出
  `C06_SCRATCH_VERIFIER=PASS ASSERTIONS=40`、
  `C06_PUMP_COMMAND_VERIFIER=PASS ASSERTIONS=14`、
  `C06_TRAVERSAL_VERIFIER=PASS ASSERTIONS=17`，退出码均为 `0`。最新验证覆盖独立 pump/teleport
  cleanup、pump-before-teleport 顺序、提交失败可观察性、拒绝/失败路径清理和异常清理路径。
- 依赖影响：只验证 shadow command/adapter seam；未写入 Liquid、Tile、Player/NPC position、
  network 或 cooldown owner。half-brick `Vector2` endpoint、真实 Liquid/Teleportation/entity commit
  owner 和跨域生命周期仍为 evidence-gap，因此 C06 保持 `partial`，不加入
  `completedComponents`。保存本 checkpoint 后执行游标推进到 `C07`，
  `pendingComponents` 为 `[C07, C08, C09, C10, C11, C12, C13, C14]`。

### 17. 2026-09-11 C04 publication seam implementation checkpoint

- C04 实际新增源码为：`src/WorldStorage/LiquidChangePublicationSnapshot.cs`、
  `LiquidPublicationProjectionStatus.cs`、`LiquidPublicationProjectionResult.cs`、
  `LiquidChangePublicationProjection.cs`、`LiquidPublicationDeliveryStatus.cs`、
  `LiquidPublicationDeliveryResult.cs`、`ILiquidPublicationPort.cs`、
  `LiquidNetworkPublishStatus.cs`、`LiquidNetworkPublishResult.cs` 和
  `LiquidNetworkAdapter.cs`。
- `LiquidChangePublicationProjection` 以现有 `LiquidReplicationDirtySet` 为候选状态 substrate，
  维护 pending/publishing 批次交换、坐标去重、按 X/Y 稳定排序和单调 revision；
  `LiquidChangePublicationSnapshot` 防御性复制坐标并保留 Version4 的
  `((x & 0xFFFF) << 16) | (y & 0xFFFF)` 编码。
- `publicationEnabled=false` 时不交换 pending；未确认的 publishing 批次保持可重试，只有注入的
  `ILiquidPublicationPort` 返回 `Delivered` 才显式 acknowledge 并清空 publishing。失败、取消、
  unknown 和异常均返回稳定的 adapter result，不回写 C01-C03 或 Tile liquid authority。
- 该实现尚未构建或运行 focused verifier；未接入旧 `_netChangeSet`、`NetLiquidModule`、
  连接生命周期或真实网络 codec，因此 C04 保持 `partial/not-verified`。

### 18. 2026-09-11 C04 publication seam verification checkpoint

- C04 串行构建 `src/WorldStorage/Terraria.WorldStorage.csproj`：退出码 `0`，`0` errors，
  `15` warnings；警告均为既有 WorldStorage 字段警告。产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`。
- 主 focused verifier 为不写入仓库的 PowerShell reflection 检查，输出
  `C04_PUBLICATION_VERIFIER=PASS`、`ASSERTIONS=23`、退出码 `0`。覆盖 pending 坐标去重、
  loading/publication suppression、pending/publishing 批次隔离、稳定 X/Y 排序、Version4 坐标编码、
  revision 单调递增、stale acknowledge、defensive snapshot、revision exhaustion 和 reset。
- 追加 delivery verifier 输出 `C04_DELIVERY_FAILURE_VERIFIER=PASS`、`ASSERTIONS=9`、退出码 `0`；
  `Failed`、`Unknown` 和 `Cancelled` 三种 transport 结果均保留 publishing batch，未清除未确认坐标。
- C04 仍为 `partial`：没有接管 legacy `_netChangeSet`、`Liquid.NetSendLiquid`、
  `NetLiquidModule` 或真实连接生命周期；adapter 只接收显式不可变 snapshot，网络 codec、重试调度和
  唯一 publication owner 仍需 integration review。
- 保存 checkpoint 后推进执行游标：`currentComponent: C05`；
  `pendingComponents: [C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；
  `verificationStatus: c01-budget-system-commit-focused-passed-30-assertions; c02-queue-seam-focused-passed-40-assertions; c03-buffer-seam-focused-passed-36-assertions; c04-publication-seam-focused-passed-23-assertions-plus-delivery-failure-9-assertions; focused-c10-c14-plus-c11-capture-discouragement-presentation-clock-passed`。

### 9.32 实施 checkpoint：C06 bounded scratch substrate

- C06 复用已由 C05 建立的 `src/WorldInteraction/Wiring/WirePropagationScratchComponent.cs`
  作为 C05/C06 唯一共享传播 scratch owner，没有创建重复的
  `WiringTeleportStateComponent`。新增的内部边界只负责记录最多两个非负 teleport tile
  endpoint、拒绝相同或超出容量的 endpoint，并提供清除 endpoint 与
  `BlockPlayerTeleportationForOneIteration` 的显式 reset。
- `src/WorldInteraction/Wiring/PumpTransferScratchComponent.cs` 现在提供内部
  `TryAddInputPump`/`TryAddOutputPump` 和 `Reset`：容量严格为 `MaxPumpCount = 20`，坐标必须
  非负，同一 input 或 output 列表内重复坐标被拒绝，reset 清空坐标数组和计数。只读列表仍
  不暴露可写数组，组件不拥有 `TileLiquidStateComponent`、Liquid amount/type 或任何实体位置。
- 该最小单元实际修改文件为上述两个 `src/` 文件；没有写入 `PortalNetworkState`、
  `PortalEndpointComponent`、Player/NPC 状态或旧 `Wiring` facade。现有 Version4 `_teleport`
  是 `Vector2[]`，且完整参考包含 half-brick `Y + 0.5f`；当前 shadow substrate 使用现有
  `TileCoordinate?` 表达，因此浮点 endpoint/半砖行为仍是明确 `evidence-gap`，后续命令不能把
  此差异写成已验证的行为等价。
- 依赖影响：C05 传播 owner 继续拥有传播队列；C06 scratch 只接受显式 endpoint/pump 输入，
  未来 pump command 交给 Liquid owner，未来 teleport command 交给 Teleportation/entity owner。
  未接管旧 `XferWater`、`Teleport`、Tile frame、network、Player/NPC position 或 cooldown。
- 首次通过仓库 wrapper 构建 `src/WorldInteraction/Terraria.WorldInteraction.csproj` 时发现
  `CS0206`：属性不能直接作为 `ref` 参数；已将两个计数更新改为局部变量接收并回写，未改变
  组件契约。修复后的源码已保存，但尚未重新执行 build 或 focused verifier，故顶部
  `verificationStatus` 仍记录为 `c06-scratch-not-verified`；`C06` 保持 `partial`，
   `completedComponents` 仍为空，`currentComponent` 和 `pendingComponents` 不推进。

### 16. 2026-09-11 C03 buffer queue verification checkpoint

- C03 实际新增源码为：`src/WorldStorage/LiquidBufferQueueStateComponent.cs`、
  `LiquidBufferCommandKind.cs`、`LiquidBufferCommand.cs`、
  `LiquidBufferCheckingIntentKind.cs`、`LiquidBufferCheckingIntent.cs`、
  `LiquidBufferCommitStatus.cs`、`LiquidBufferCommitResult.cs`、
  `ILiquidBufferCommitPort.cs`、`LiquidBufferCommitPort.cs`、
  `LiquidBufferDrainStatus.cs`、`LiquidBufferDrainResult.cs` 和
  `LiquidBufferCommitSystem.cs`。
- `LiquidBufferQueueStateComponent` 复用 `LiquidBufferEntry` 的坐标值对象，默认从
  `LiquidFlowBudgetPolicy.MaximumBufferLength = 50000` 保留 Version4 的两个槽位，实际入队上限为
  `49998`；它只保存 deferred coordinate、计数和去重索引，不复制 Tile liquid amount/type。
- `LiquidBufferCommitPort` 负责有界 enqueue、坐标去重、FIFO head release、reset 和防御性 snapshot；
  `LiquidBufferCommitSystem` 在 C02 active queue 接受或幂等 duplicate 后才 release buffer entry，
  C02 拒绝时保留 entry 并返回可重试结果。Tile `checkingLiquid` 只通过 set/clear intent 返回，
  不直接写 `TileLiquidWorkStateComponent`，也不发布网络消息。
- 通过仓库串行 wrapper 构建 `src/WorldStorage/Terraria.WorldStorage.csproj`：退出码 `0`，
  `0` errors，`15` warnings；警告均为既有 WorldStorage 字段警告。产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`。
- focused verifier 为不写入仓库的 PowerShell reflection 检查，输出
  `C03_BUFFER_VERIFIER=PASS`、`ASSERTIONS=36`、退出码 `0`。覆盖默认上限、`49998` 保留槽位边界、
  duplicate、checking-liquid no-op、bounds、capacity rejection、read-only snapshot、C02 reject
  retain/retry、successful drain/release、active duplicate、FIFO head protection 和 idempotent reset。
- C03 仍为 `partial`：没有接管旧 `LiquidBuffer.AddBuffer/DelBuffer`、旧 Tile writer 或
  `Liquid.UpdateLiquid` drain callsite；reset 时现有 Tile checking flag 的完整清理集合和
  WorldStorage/Dome owner 仍待 integration review，不更新 `completedComponents`。
- 保存 checkpoint 后推进执行游标：`currentComponent: C04`；
  `pendingComponents: [C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；
  `verificationStatus: c01-budget-system-commit-focused-passed-30-assertions; c02-queue-seam-focused-passed-40-assertions; c03-buffer-seam-focused-passed-36-assertions; focused-c10-c14-plus-c11-capture-discouragement-presentation-clock-passed`。

### 2026-09-12 P01 组件边界复核与串行重建 checkpoint

- 本 checkpoint 使用既有人工绑定 `partitionId: P01`、`sessionId: 9891b103b31246b981d200e001e5c302`，没有重新调用 `Claim`、`ClaimNext`、`Retry`、`Cleanup`、`Complete` 或 `Fail`，也没有修改 runner ledger。
- 只读复核确认：C01-C07、C10-C14 的已落地组件源码仍存在于 `src/`；C08 的 `CollisionQueryScratch` 仍是 query-owned、per-invocation scratch，不应伪装成长期 ECS component；C09 的 `TileContactResult`、`HurtTileResult` 和 `CollisionContactQuery` 仍是 value/query 边界。当前 `src` 缺少统一 `TileContact`/`HurtTile` 值对象，且 `src/Physics/CollisionResultComponent.cs` 与 `src/SpatialSimulation/CollisionResultComponent.cs` 的 owner 决议仍未完成。
- 本轮没有新增或修改 `src/` 组件源码。基于现有证据，不新增 `CollisionQueryScratchComponent`、第三个 `CollisionResultComponent`、占位值对象、重复 registry/clock/lifecycle/value owner，也不把跨域 integration 行为塞入组件。
- 按 `BUILD-CONCURRENCY-1` 先确认没有活动 `dotnet.exe`/`csc.exe`，随后依次通过 `Build/Tools/Invoke-SerialDotnet.ps1` 使用 `--no-restore`、`-m:1`、`-nr:false`、`-p:UseSharedCompilation=false`、`-p:MSBuildNodeReuse=false` 和 `-p:BuildInParallel=false` 构建四个受影响项目。四次命令退出码均为 `0`，警告均为 `0`，错误均为 `0`：
  - `src/WorldStorage/Terraria.WorldStorage.csproj` -> `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`
  - `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj` -> `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`
  - `src/Teleportation/Terraria.Teleportation.csproj` -> `D:\TRbackup\NLTX\Build\bin\Terraria.Teleportation\Debug\net10.0\Terraria.Teleportation.dll`
  - `src/WorldInteraction/Terraria.WorldInteraction.csproj` -> `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`
- evidence-gap：构建只证明现有组件源码在当前项目边界内可编译，不覆盖旧 Liquid/Wiring writer、Tile/TileEntity、NPC capture/spawn、marker registry payload、Pylon owner、network、persistence、自然 spawn arbitration、C08/C09 owner 或跨域行为等价。
- blocking-decision：保持 `implementationStatus: partial`、`completedComponents: []`、`partialComponents: [C01, C02, C03, C04, C05, C06, C07, C10, C11, C12, C13, C14]`、`blockedComponents: [C08, C09]`、`currentComponent: C14` 和 `pendingComponents: [C10, C11, C12, C13, C14]`。未获得 integration review 的唯一写者、生命周期、协议和 owner 证据前，不继续扩展组件或宣称分区完成。
- verificationStatus：`p01-component-boundary-audit-rebuild-4-projects-passed-0-warning-0-error`；顶部 metadata 的 `lastCheckpointUtc` 已更新为 `2026-09-12T13:19:57.8472235Z`。

### 2026-09-12 C13 registry duplicate-input invariant checkpoint

- 修改前的不写入仓库 RED reflection probe 输出为
  `C13_REGISTRY_DUPLICATE_RED=CONFIRMED ACCEPTED_DUPLICATE`，退出码 `1`：
  `RevengeMarkerRegistryComponent(IEnumerable<RevengeMarkerId>)` 在重复 marker ID 输入时曾由
  `HashSet.Add` 静默去重并成功构造。
- 实际修改文件仅为
  `src/DeathPenaltyAndRevenge/RevengeMarkerRegistryComponent.cs`。构造函数现在检查
  `_markerIds.Add(markerId)` 的返回值，重复 marker ID 抛出 `ArgumentException`；未分配 ID 的既有
  校验、`MarkerIds` 冻结只读视图以及 `TryRegister`/`TryUnregister`/`Clear` 的组件内语义保持不变。
- 按 `BUILD-CONCURRENCY-1` 先确认无活动 `dotnet.exe`/`csc.exe`，再通过
  `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建
  `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`；命令使用
  `--no-restore`、`-m:1`、`-nr:false`、`-p:UseSharedCompilation=false`、
  `-p:MSBuildNodeReuse=false` 和 `-p:BuildInParallel=false`，退出码 `0`，警告 `0`，错误 `0`。
  产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的 GREEN reflection verifier 输出为
  `C13_REGISTRY_DUPLICATE_GREEN=PASS ASSERTIONS=14`，退出码 `0`。验证覆盖唯一 assigned ID
  构造、重复 ID 拒绝、未分配 ID 拒绝、冻结视图、重复注册拒绝、新 ID 注册及视图刷新、注销和
  清空行为。
- blocking-decision：该修订只增加 C13 registry 构造输入的局部冲突校验，不新增 marker payload
  registry、legacy writer、allocator、network、persistence 或跨域 owner；C13 继续为 `partial`，
  `completedComponents` 继续为空，未调用 runner 结算。完整 marker payload 的唯一 owner、旧
  writer、网络/存档恢复和跨域生命周期仍需 integration review。

### 2026-09-12 C14 refresh-cooldown setter invariant checkpoint

- 静态复核确认 `PylonRegistryComponent.Replace` 已拒绝负 `refreshCooldownTicks`，但公开可读、程序集内
  可写的 `RefreshCooldownTicksRemaining` setter 原先可以绕过同一不变量直接写入负值；当前源码没有其它
  该属性写入者。
- 实际修改文件仅为 `src/WorldStorage/PylonRegistryComponent.cs`。新增私有
  `_refreshCooldownTicksRemaining` backing field，并让 `RefreshCooldownTicksRemaining` 的
  `internal set` 统一拒绝负值；`Replace`、`AdvanceTick`、`Reset` 的 current/previous 快照、revision
  和 cooldown 语义未扩展到任何外部 owner。
- 应用户当前要求，本 checkpoint 不执行测试、focused verifier、构建或其它 compile-capable 验证；源码文件
  已确认存在，故 `verificationStatus` 明确追加
  `c14-refresh-cooldown-invariant-not-verified-no-tests-requested`。
- blocking-decision：修订只增加 C14 组件内剩余冷却的局部输入不变量，不接入 TileEntity、SceneMetrics、
  equality/diff、network、persistence 或重复 registry owner；C14 继续为 `partial`，
  `completedComponents` 继续为空，未调用 runner 结算。后续需在用户允许测试/构建后补验证。

### 2026-09-12 C10-C14 static owner audit checkpoint

- 本 checkpoint 只读复核了 C10-C14 当前组件源码、Version4 对应的
  `CoinLossRevengeSystem`/`TeleportPylonsSystem` 字段和调用边界，以及当前 NLTX 的引用关系。
  未发现能够在组件内部安全修订、且不改变既有 owner 或跨域语义的新的确定性缺陷。
- `PylonRegistryComponent.Revision` 的 `internal set` 暂不收紧：当前它没有本分区源码写者，但
  未来存档恢复/版本协调的写入契约尚未闭合；将其改为 `private set` 或新增 setter 校验会提前决定
  外部 owner。C10 生命周期内的 attempt state 与 C12 attempt component 仍保持单一可写 substrate，
  不新增第二份状态源。
- 本 checkpoint 没有修改 `src/`，没有新增 System、Query、Command、Adapter、Projection、测试或
  验证器；按用户当前要求不运行测试、focused verifier、构建或其它 compile-capable 命令。
- 状态保持 `implementationStatus: partial`、`completedComponents: []`、
  `partialComponents: [C01, C02, C03, C04, C05, C06, C07, C10, C11, C12, C13, C14]`、
  `blockedComponents: [C08, C09]`、`currentComponent: C14` 和
  `pendingComponents: [C10, C11, C12, C13, C14]`。未闭合项仍包括 legacy marker writer/allocator、
  NPC capture/spawn、自然 spawn arbitration、marker payload registry、TileEntity/SceneMetrics、
  Pylon 唯一 owner、network、persistence、equality/diff 以及 C08/C09 owner 决议。

### 2026-09-13 C10-C14 局部状态复核 checkpoint（不执行测试）

- 本 checkpoint 只读复核 C10-C14 当前源码、Version4 对应字段和当前 NLTX 引用关系。生命周期、attempt、registry、clock、target snapshot、value 和 pylon 快照的局部写入入口均已核对；没有发现可以在组件内部安全修订、且不提前决定跨域 owner 的新确定性缺陷。
- `RevengeMarkerLifecycleComponent` 继续通过唯一的 `RevengeRespawnAttemptComponent` substrate 暴露 force-expire/attempt lock；`RevengeMarkerRegistryComponent` 继续使用冻结 ID 快照并拒绝重复构造输入；`RevengeTargetSnapshotComponent` 继续区分 spawn NPC net ID 与 discouragement NPC type，并保留原始 life ratio；`PylonRegistryComponent` 继续保持 Replace 输入复制、current/previous 旧视图稳定和非负 cooldown setter。
- `PylonRegistryComponent.Revision` 的 `internal set` 暂不收紧。当前本分区没有对应写者，但存档恢复/版本协调 owner 尚未闭合；将其收紧会提前决定外部写入契约。没有新增 registry payload、NPC/TileEntity writer、network、persistence 或第二份生命周期/attempt/value state。
- 本 checkpoint 没有修改 `src/`，没有新增 System、Query、Command、Adapter、Projection、测试或验证器。按用户要求没有运行测试、focused verifier、构建或其它 compile-capable 命令；因此本次结论是静态审阅结果，不是当前源码的编译或运行时验证。
- 状态保持 `implementationStatus: partial`、`completedComponents: []`、`partialComponents: [C01, C02, C03, C04, C05, C06, C07, C10, C11, C12, C13, C14]`、`blockedComponents: [C08, C09]`、`currentComponent: C14` 和 `pendingComponents: [C10, C11, C12, C13, C14]`。未闭合的跨域 owner、网络/存档协议、NPC capture/spawn、TileEntity/SceneMetrics、自然 spawn arbitration、C08/C09 决议继续等待 integration review。

### 2026-09-13 C06 泵 scratch 计数不变量修订 checkpoint

- 本 checkpoint 使用当前 runner 绑定的 `partitionId: P01` 和 `sessionId: e925fb5b47e04306b42135b5e195a463`；没有重新领取分区，也没有修改 runner ledger、权威报告或其他分区文件。
- 实际修改文件仅为 `src/WorldInteraction/Wiring/PumpTransferScratchComponent.cs`。`InputPumpCount` 和 `OutputPumpCount` 保留原有公开属性与程序集内写入 API，改由组件内部 backing field 保存，并拒绝小于 `0` 或大于 `MaxPumpCount = 20` 的值；泵坐标追加路径同时保留边界保护，避免非法计数造成槽位越界。
- 本修订只属于 C06 ECS 组件的字段不变量和最小初始化/存储边界；没有新增或修改 System、Query、Command、Adapter、Projection、Port、事件、测试、验证器、项目文件或配置，也没有接入 Liquid、Tile、Player/NPC、Teleportation、网络或存档 owner。
- `completedComponents: []`；`partialComponents` 保持 `[C01, C02, C03, C04, C05, C06, C07, C10, C11, C12, C13, C14]`；`blockedComponents: [C08, C09]`；`currentComponent: C06`；`pendingComponents: [C10, C11, C12, C13, C14]`。C06 仍是局部组件 substrate，跨域提交 owner 和运行时接管尚未完成。
- 按用户要求本 checkpoint 未编写或运行测试、focused verifier、构建或其他 compile-capable 验证；源码文件存在性和改动范围已静态确认，因此 `verificationStatus: c06-pump-scratch-count-invariant-not-verified-no-tests-requested`。未验证编译、运行时行为、网络、持久化和行为等价。
