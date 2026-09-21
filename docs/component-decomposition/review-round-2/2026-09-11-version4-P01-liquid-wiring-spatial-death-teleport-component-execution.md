# Version4 P01 液体、机关、空间、死亡惩罚与传送组件执行计划

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
evidence-gap: C01 policy/query/state/system/commit shadow substrate 已通过 30 项 focused 断言和串行项目构建，但旧 Liquid writer、Tile/liquid authority、C02/C03 交接和跨世界生命周期仍未闭合；C02 command/query/queue seam 已通过 40 项 focused 断言和串行项目构建，但仍不接管旧 `Main.liquid` 数组；C03 buffer queue、command、commit port 和 C02 drain system 已通过 36 项 focused 断言和串行项目构建，但仍不接管旧 `LiquidBuffer` writer；C03 只返回 Tile `checkingLiquid` set/clear intent，不直接写 Tile，C02/C03 capacity handoff、C02 拒绝后的重试和 Reset 时的 checking flag 清理仍需 integration review；C04 publication snapshot/projection/network adapter 源码已保存但尚未构建或运行 focused verifier，仍未接管旧 `_netChangeSet` writer、WorldGen/loading 外部调用点、NetLiquidModule 或真实连接 owner；C04 的 pending/publishing revision 恢复和发送确认依赖 integration review；C11 capture qualification 纯 Query 已通过 18 项 focused 断言，discouragement outer Query 已通过 20 项 focused 断言，C13 presentation policy 已通过 2 项 focused 断言，RevengeClockState 已通过 5 项 focused 断言；style 2/3 仍依赖 NPC owner 提供的显式结果，C11 capture/discouragement 尚未接入 NPC owner；C10 allocator 尚未接入旧 marker writer，C12 NPC spawn commit/arbitration/retry/remove projection、C13 registry/network/persistence owner、C14 pylon registry/TileEntity/equality-diff/WorldStorage/Dome owner 仍未闭合；C06 已扩展既有 C05/C06 shared scratch owner，完成有界 pump 坐标去重、计数和 reset substrate，但尚未完成 command/adapter/port、半砖 Vector2 endpoint 表达或 focused 验证；C05-C09 仍受目标项目/owner 边界限制，分区整体行为等价和网络/存档验证未运行。
evidence-gap-c07: C07 仅保存 `src/WorldInteraction/Wiring/WiringMechanismScheduleComponent.cs` 和 `WiringDeviceCooldownComponent.cs` 两个 bounded component；尚未创建 `WiringMechanismSchedulePolicy`、`HopperInteractionPolicy`、`MechanismCooldownSystem` 或 `MechanismActivationQuery`，尚未接管旧 `_mechX/_mechY/_mechTime`、`_numMechs`、`CheckMech`、`UpdateMech`、Tile frame、network 或机制效果；`Terraria.WorldInteraction.csproj` 已通过仓库串行 build，退出码 `0`、`0` errors、`15` 个既有 `WorldStorage` warnings，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`；不写入仓库的 focused reflection verifier 输出 `C07_COMPONENT_VERIFIER=PASS ASSERTIONS=13`，覆盖固定容量、默认值、只读条目视图和组件 Reset。
evidence-gap-c08: C08 的设计最终将 `CollisionQueryScratch` 定义为调用期间的 query-owned scratch，而不是可附加的长期 ECS component；其 `contacts` 依赖当前 `src` 中不存在的 `TileContact` 类型，conveyor cache 依赖未建立的 `Point` 值对象；`src/Physics/CollisionResultComponent.cs` 与 `src/SpatialSimulation/CollisionResultComponent.cs` 仍存在 owner 冲突。按本次只允许组件源码的范围，不创建 `CollisionQueryScratchComponent`、值对象、Query、Definition 或 Projection；C08 没有新增源码或 build 证据。
blocking-decision: partial-implementation-blocked；不得移动旧源文件、字段双写、创建第二个 allocator/registry，或删除旧 facade，直到 integration-review 裁决唯一 owner 和生命周期；C01 只保存 shadow substrate，C02 起先实现不改变现有 owner 的纯 command/query 和显式提交 seam；C06 shadow traversal seam 已通过最终 cleanup patch 验证，但不接管旧 XferWater/Teleport，也不提交实体位置、Tile frame 或 network，直到 Liquid 与 Teleportation/entity owner 的提交协议和 Vector2 半砖语义闭合；C07 仅保存 bounded schedule component，不接入旧机制数组、CheckMech/UpdateMech、Tile frame、network 或机制效果。
blocking-decision-c08: C08 不得把 query-owned scratch 伪装为长期 `CollisionQueryScratchComponent`；在 `TileContact`/conveyor point 值对象、query invocation lifecycle 和 Physics/Spatial result owner 决议前，不创建第三个 collision owner，不修改两个既有 `CollisionResultComponent`，也不接入 Tile、entity、network 或 persistence writer。
evidence-gap-c09: C09 设计中的 `TileContactResult`、`HurtTileResult` 和 `CollisionContactQuery` 都是一次查询的不可变结果/Query 类型，不是 ECS component；当前 `src` 没有可复用的 `TileContact` 或 `HurtTile` 值对象，且 Physics/Spatial 的两个 `CollisionResultComponent` owner 尚未裁决。按本次只允许组件源码的范围，不新增 C09 文件，不修改现有 result component，也不实现 Tile/Player/NPC/network/damage 行为。
blocking-decision-c09: C09 必须等待 C08 query-owned scratch、TileContact/HurtTile 值对象和 Physics/Spatial result owner integration review；不得创建第三个 `CollisionResultComponent`、把结果列表塞入现有组件，或把 Query/伤害/Tile 行为写进组件。
evidence-gap-c10-c14-component-contract-audit: 本 checkpoint 没有新增或修改 `src/` 文件；已对现有 C10-C14 组件和 C14 `PylonRegistryEntry`/`PylonRegistryComponent` 执行只读反射契约审计，输出 `P01_COMPONENT_CONTRACT_AUDIT=PASS ASSERTIONS=26`。`Terraria.DeathPenaltyAndRevenge.csproj`、`Terraria.WorldStorage.csproj` 和 `Terraria.Teleportation.csproj` 均经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建，退出码 `0`、`0` warning、`0` error，产物分别位于 `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`、`D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll` 和 `D:\TRbackup\NLTX\Build\bin\Terraria.Teleportation\Debug\net10.0\Terraria.Teleportation.dll`。审计覆盖 identity/lifecycle/value/attempt/context、registry immutable view、pylon entry validity、current/previous snapshot、cooldown、revision、reset 和 alias-safe replacement；不覆盖 NPC spawn、旧 marker writer、TileEntity scan、SceneMetrics、network 或 persistence。
blocking-decision-c10-c14-component-contract-audit: 当前没有新的组件级状态缺陷可安全修改。不得为 C10/C12 增加未经 owner 裁决的重复 lifecycle/validation/reset，不得把 C13 ID index 扩展为完整 marker registry，不得在 C14 组件中接入 TileEntity/SceneMetrics/network/persistence；C10-C14 继续保持 `partial`，`completedComponents` 继续为空。
evidence-gap-c14-pylon-snapshot-isolation-correction: `PylonRegistryComponent` 的旧 `CurrentPylons`/`PreviousPylons` 只读视图此前仍绑定到会被后续 `Replace`/`Reset` 清空的内部列表，不能满足设计要求的 immutable ordered snapshot。已在组件内改为替换 list 实例并为每个实例建立新的 `ReadOnlyCollection` view；`Replace` 仍先复制输入，因此传入 current view 的 alias-safe 行为不变。通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建 `Terraria.WorldStorage.csproj`，退出码 `0`、`15` 个既有 warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`；不写入仓库的 focused verifier 输出 `C14_PYLON_SNAPSHOT_ISOLATION_VERIFIER=PASS ASSERTIONS=10`，覆盖替换、alias 输入、连续替换和 reset 后旧 current/previous view 的稳定性。
blocking-decision-c14-pylon-snapshot-isolation-correction: 修订仅改变 C14 组件的内部快照容器生命周期，不读取 TileEntity、SceneMetrics、网络或存档，也不选择最终 registry owner；修订已验证，但 C14 仍不能标记 completed，C10-C14 继续保持 `partial`。
evidence-gap-c10-c14-components: C10-C14 的现有组件源码已完成本轮受影响项目串行重建：`Terraria.DeathPenaltyAndRevenge.csproj` 退出码 `0`、`0` warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`；`Terraria.Teleportation.csproj` 退出码 `0`、`0` warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.Teleportation\Debug\net10.0\Terraria.Teleportation.dll`。C10 legacy allocator/lifecycle、C11 NPC capture owner、C12 spawn arbitration/retry、C13 registry/clock/network/persistence 和 C14 Pylon/TileEntity/WorldStorage/Dome owner 仍未闭合。
evidence-gap-c14-component-lifecycle: `PylonRegistryComponent` 现在保存 current/previous 快照、refresh cooldown 和 revision，并提供纯组件内的 replace/advance/reset 生命周期；`Replace` 会在清理内部列表前复制输入，避免调用者传入 `CurrentPylons` 只读视图时发生别名清空。`Terraria.WorldStorage.csproj` 已通过仓库 wrapper 串行 build，退出码 `0`、`0` error、`15` 个既有 warning，且不写入仓库的只读契约验证输出 `C14_PYLON_COMPONENT_VERIFIER=PASS ASSERTIONS=6`、别名修复回归输出 `C14_PYLON_ALIAS_FIX_VERIFIER=PASS ASSERTIONS=16`。TileEntity 扫描、SceneMetrics、equality/diff、network/persistence 和唯一 registry owner 仍未闭合。
evidence-gap-c13-registry-view: `RevengeMarkerRegistryComponent` 仍只保存 assigned marker ID index；本单元把公开 `MarkerIds` 改为 mutation-resistant frozen snapshot，并在内部 register/unregister/clear 后刷新。`Terraria.DeathPenaltyAndRevenge.csproj` 已通过 `Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 串行构建，退出码 `0`、`0` warning、`0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`；不写入仓库的 focused reflection verifier 输出 `C13_REGISTRY_VERIFIER=PASS ASSERTIONS=12`。完整 marker payload registry、legacy `_markers` owner、clock、network/persistence 和 commit boundary 仍未裁决。
blocking-decision-c13-registry-view: frozen ID view 只限制外部绕过组件边界修改 ID index，不声明 registry owner 已切换，也不复制 marker payload 或 legacy list；所有 register/remove 的最终写者仍需 integration review。
blocking-decision-c14-component-lifecycle: 组件只复制显式 entry 列表并改变自身 revision/cooldown；不得在组件内读取 TileEntity、SceneMetrics、网络或存档，也不得据此选择 `WorldStorage`、`Teleportation` 或 Dome 的最终 owner。
evidence-gap-c06-final-cleanup: 最终 cleanup-only 源码修订已通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建 `src/WorldInteraction/Terraria.WorldInteraction.csproj`，退出码 `0`，警告 `0`，错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。不写入仓库的 focused verifier 输出 `C06_SCRATCH_VERIFIER=PASS ASSERTIONS=40`、`C06_PUMP_COMMAND_VERIFIER=PASS ASSERTIONS=14`、`C06_TRAVERSAL_VERIFIER=PASS ASSERTIONS=17`，均退出码 `0`。剩余缺口仍是 half-brick `Vector2` endpoint、真实 Liquid/Teleportation/entity owner、Player/NPC position、Tile frame、network 和跨域生命周期，不得据此宣称运行时迁移完成。
blocking-decision-c06-final-cleanup: C06 shadow scratch、command、port 和 adapter 的状态、顺序、失败观察及成功/失败清理已验证；没有唯一 owner 和半砖 endpoint 证据，继续禁止接入旧 `XferWater`/`Teleport` 或提交 Tile、实体位置、network、cooldown 副作用。
evidence-gap-c06-command: C06 bounded scratch substrate 已通过 `Terraria.WorldInteraction` 串行 build（0 warning/0 error）和 `C06_SCRATCH_VERIFIER=PASS ASSERTIONS=40`；pump command 源码已通过同一项目串行 build（0 warning/0 error）和 `C06_PUMP_COMMAND_VERIFIER=PASS ASSERTIONS=14`，覆盖空输入/输出、非法颜色、相同端点拒绝、2x2 稳定顺序、20x20 命令容量和只读命令列表；teleport command/adapter/port 源码已通过同一项目串行 build（0 warning/0 error）和 `C06_TRAVERSAL_VERIFIER=PASS ASSERTIONS=17`，覆盖 endpoint/subject/color rejection、pump-then-teleport ordering、pump/teleport failure observation and scratch cleanup。half-brick Vector2 endpoint、Liquid owner 交接、实体位置/网络副作用仍未闭合。
implementation-note-c06-traversal: `WiringTeleportCommand` 只接收非负 Tile endpoint、wire color 和非空 Player/Npc `EntityReference`，不表达或伪造 half-brick `Vector2`。`WiringTraversalAdapter` 只调用注入的 `IWiringTraversalCommitPort`，按 pump 后 teleport 顺序提交，并在成功、拒绝或异常路径清理两个 scratch；它不写 Player/NPC 位置、Tile、Liquid、network 或 cooldown。
blocking-decision-c06-traversal: C06 traversal seam 的状态和顺序已由 fake commit port 验证，但真实 Liquid/Teleportation/entity commit owner 未裁决；不得把 `IWiringTraversalCommitPort` 接入旧 `XferWater`、`Teleport`、Tile frame、network 或 entity position writer，直到 integration review 提供唯一 owner 和半砖 endpoint 表达证据。

### 11.42 C06 pump transfer command implementation checkpoint

- 已实际保存 `src/WorldInteraction/Wiring/PumpTransferPolicy.cs`、
  `PumpTransferCommand.cs`、`PumpTransferCommandBatch.cs` 和
  `PumpTransferCommandBatchStatus.cs`。`PumpTransferPolicy` 只消费已验证的
  `PumpTransferScratchComponent`，按 input index 外层、output index 内层生成稳定的坐标转移
  command；没有 input 或 output 时返回明确的 empty result，非法 wire color、数量或 pump pair
  返回 rejection。
- `PumpTransferCommand` 只携带 source/destination tile coordinate、input/output index 和
  wire color；拒绝负坐标、相同 source/destination 和非法 wire color，不携带或推断 Tile liquid
  amount/type。批量命令返回只读列表，保持顺序可审计，未写入 Tile、Liquid、网络或实体位置。
- 本最小单元尚未重新执行仓库串行 build 或 focused command verifier；`C06_COMMAND_CONTRACT_RED`
  只证明实现前类型缺失，不能作为 green 证据。下一步必须先 build，再验证 empty/rejection、
  20x20 命令顺序和防御性列表边界。

### 11.44 C06 teleport command and traversal adapter implementation checkpoint

- 已实际保存 `src/WorldInteraction/Wiring/WiringTeleportCommand.cs`、
  `WiringTraversalCommitStatus.cs`、`WiringTraversalCommitResult.cs`、
  `IWiringTraversalCommitPort.cs`、`WiringTraversalFlushStatus.cs`、
  `WiringTraversalFlushResult.cs` 和 `WiringTraversalAdapter.cs`。
- `WiringTeleportCommand` 拒绝负 endpoint、相同 endpoint、非法 wire color 和空/非 Player/Npc
  subject；命令只携带外部实体引用和 one-iteration block intent，不接管实体位置或 portal state。
- `WiringTraversalAdapter` 按 pump 后 teleport 顺序提交；commit port 返回 rejected/failed 时停止
  后续提交并暴露原因，`finally` 始终清理 pump scratch 和 teleport endpoint/block flag。源码复核
  发现并修正了异常路径提前返回及 `in` 参数传递问题；当前尚未重新 build 或运行 focused verifier。

### 11.45 C06 teleport command and traversal adapter verification checkpoint

- 通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建
  `src/WorldInteraction/Terraria.WorldInteraction.csproj`，退出码 `0`，`0` warning，`0` error；
  产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。
- 不写入仓库的 PowerShell reflection/Reflection.Emit focused verifier 输出
  `C06_TRAVERSAL_VERIFIER=PASS ASSERTIONS=17`，退出码 `0`。覆盖 valid command、same endpoint、
  unsupported subject、invalid wire color、pump-then-teleport order、pump failure short-circuit、
  teleport failure observation，以及 success/failure 后 pump 与 teleport scratch cleanup。
- C06 保持 `partial`；验证的是 shadow commit seam；没有真实 Liquid writer、Teleportation/entity
  owner、half-brick `Vector2` endpoint、Player/NPC position、Tile frame 或 network parity 证据。

### 11.43 C06 pump transfer command verification checkpoint

- 通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建
  `src/WorldInteraction/Terraria.WorldInteraction.csproj`，参数为
  `build ./src/WorldInteraction/Terraria.WorldInteraction.csproj -m:1 -nr:false
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；退出码 `0`，
  `0` warning，`0` error。产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。
- 不写入仓库的 PowerShell reflection focused verifier 输出
  `C06_PUMP_COMMAND_VERIFIER=PASS ASSERTIONS=14`，退出码 `0`。覆盖缺少 input/output、非法
  wire color、相同 source/destination、2x2 的 input-outer/output-inner 顺序、20x20 的 400 条
  命令、wire color 保留和只读命令列表；没有检查或修改 Tile liquid amount/type。
- C06 仍为 `partial`；本单元完成的是 shadow command contract，不接管旧 `XferWater`、Liquid
  writer、Tile frame、network 或实体位置。执行游标保持 `currentComponent: C06`，
  `completedComponents: []`，后续进入 teleport command 与 traversal commit seam。

## 1. 执行边界

本文件描述可审查、可回滚的实施顺序。当前实施范围为 C10-C14 的纯/只读最小单元，没有切换运行时 writer，没有修改测试或执行网络/存档 I/O，也没有声称行为等价完成。已实现代码只写入本分区授权的 `src/` 文件；未实现的 registry/system/adapter/projection 仍为 `status: proposed`。

本实现阶段只向同一 P01 的两份文档和 `src/` 写入。Version4 源码、完整参考源码、P01 权威报告、其他分区文档、ledger、锁文件、`Test/` 和项目文件均为只读输入或受保护对象。

## 2. 设计与证据输入

| 输入 | 用途 | 状态 |
|---|---|---|
| P01 权威报告 | 113 成员逐行清单和 14 个叶子边界 | `source-inventory-confirmed` |
| P01 component-design | 候选 owner、状态分类、依赖方向和 C01-C14 checkpoint | `partial` |
| Version4 Liquid/Wiring/Collision/Revenge/Pylon 源码 | 行为、生命周期和副作用锚点 | `partial` |
| NLTX `src/`/`dome/src/` | 既有类型、namespace、项目边界和潜在 owner | `partial` |
| tModLoader API v2026.07 | public API 交叉参考 | `reference-only` |
| SS14 ECS 参考 | 组件/System/Query 粒度参考 | `reference-only` |

## 3. proposed 文件组织与源文件映射

路径是候选目标路径，不代表文件已创建。路径按游戏能力优先，领域规模和独立测试边界允许 `Definitions`、`Components`、`Systems`、`Queries`、`Adapters`、`Snapshots` 子目录；不创建泛化 `Shared/Components`、`Common` 或 `Misc`。

| checkpoint | Version4 源边界 | proposed 目标路径/类型 | namespace | 当前 NLTX 影响 |
|---|---|---|---|---|
| C01 | `Terraria/Liquid.cs:14-42` | `dome/src/Terraria.Dome.Simulation/Liquid/Definitions/LiquidFlowBudgetPolicy.cs`; `.../Liquid/Components/LiquidFlowBudgetAndPanicStateComponent.cs`; `.../Liquid/Systems/LiquidFlowSystem.cs`; `.../Liquid/Queries/LiquidFlowBudgetQuery.cs`; `.../Liquid/Systems/ILiquidFlowCommitPort.cs` | `Terraria.Dome.Simulation.Liquid.*` | 与 `src/WorldStorage/LiquidWorldRuntimeStateComponent.cs`、Dome `LiquidWorldStateComponent.cs`、Tile liquid owner 做整合审查 |
| C02 | `Terraria/Liquid.cs:44-50` | `dome/src/Terraria.Dome.Simulation/Liquid/Commands/LiquidCellWorkItemStateCommand.cs`; `.../Liquid/Systems/LiquidWorkQueueSystem.cs` | `Terraria.Dome.Simulation.Liquid.*` | 与 `src/WorldStorage/LiquidWorkEntry.cs`、`LiquidWorkQueueState.cs`、Dome `LiquidWorkItemComponent.cs` 去重 |
| C03 | `Terraria/LiquidBuffer.cs:5-30` | `dome/src/Terraria.Dome.Simulation/Liquid/Components/LiquidBufferQueueStateComponent.cs`; `.../Liquid/Systems/LiquidBufferCommitSystem.cs`; `.../Liquid/Commands/LiquidBufferCommand.cs` | `Terraria.Dome.Simulation.Liquid.*` | 与 `src/WorldStorage/LiquidBufferEntry.cs` 和 Tile checking 写入根整合 |
| C04 | `Terraria/Liquid.cs:52-64, NetSendLiquid` | `dome/src/Terraria.Dome.Simulation/Liquid/Snapshots/LiquidChangePublicationProjection.cs`; `.../Liquid/Adapters/LiquidNetworkAdapter.cs` | `Terraria.Dome.Simulation.Liquid.*` | 与 `src/WorldStorage/LiquidReplicationDirtySet.cs`、Dome `LiquidReplicationSystem.cs` 只能保留一个发布 owner |
| C05 | `Terraria/Wiring.cs:19-37,65-67` | `dome/src/Terraria.Dome.Simulation/Wiring/Components/WiringPropagationStateComponent.cs`; `.../Wiring/Components/WiringExecutionContextComponent.cs`; `.../Wiring/Systems/WiringPropagationSystem.cs`; `.../Wiring/Commands/WiringPropagationCommand.cs` | `Terraria.Dome.Simulation.Wiring.*` | 与 `src/WorldInteraction/Wiring/WirePropagationScratchComponent.cs`、Dome `WireNetworkComponent.cs`/`WireTraversalStateComponent.cs` 整合 |
| C06 | `Terraria/Wiring.cs:17,39-53` | `dome/src/Terraria.Dome.Simulation/Wiring/Components/WiringTeleportStateComponent.cs`; `.../Wiring/Components/PumpTransferScratchComponent.cs`; `.../Wiring/Commands/PumpTransferCommand.cs`; `.../Wiring/Adapters/WiringTraversalAdapter.cs` | `Terraria.Dome.Simulation.Wiring.*` | 与 `src/WorldInteraction/Wiring/PumpTransferScratchComponent.cs`、Teleportation owner、Liquid owner 整合 |
| C07 | `Terraria/Wiring.cs:55-75` | `dome/src/Terraria.Dome.Simulation/Wiring/Components/WiringMechanismScheduleComponent.cs`; `.../Wiring/Components/WiringDeviceCooldownComponent.cs`; `.../Wiring/Definitions/HopperInteractionPolicy.cs`; `.../Wiring/Systems/MechanismCooldownSystem.cs` | `Terraria.Dome.Simulation.Wiring.*` | 与 `src/WorldInteraction/Wiring/MechanismCooldownComponent.cs`、Dome `MechanismRetryState.cs` 整合 |
| C08 | `Terraria/Collision.cs:55-73` | `dome/src/Terraria.Dome.Simulation/Physics/Queries/CollisionQueryScratch.cs`; `.../Physics/Definitions/CollisionPolicyDefinition.cs`; `.../Physics/Projections/CollisionDerivedResultProjection.cs` | `Terraria.Dome.Simulation.Physics.*` | 与 `src/Physics/CollisionResultComponent.cs`、`src/SpatialSimulation/CollisionResultComponent.cs` 先裁决 owner |
| C09 | `Terraria/Collision.cs:21-52` | `dome/src/Terraria.Dome.Simulation/Physics/Results/TileContactResult.cs`; `.../Physics/Results/HurtTileResult.cs`; `.../Physics/Queries/CollisionContactQuery.cs` | `Terraria.Dome.Simulation.Physics.*` | 当前值对象缺少统一 protocol/persistence 边界；不得直接替换现有组件 |
| C10 | `CoinLossRevengeSystem.cs:18-28,52-64` | `src/DeathPenaltyAndRevenge/RevengeExpirationPolicy.cs`; `.../RevengeMarkerIdentityComponent.cs`; `.../RevengeMarkerIdAllocator.cs`; `.../RevengeMarkerIdentityProjection.cs` | `Terraria.DeathPenaltyAndRevenge` | 与现有 `RevengeMarkerId.cs`、identity/lifecycle 组件及持久化 ID 规则整合 |
| C11 | `CoinLossRevengeSystem.cs:30-54` | `src/DeathPenaltyAndRevenge/RevengeEnemyContextPolicy.cs`; `.../RevengeTargetSnapshotComponent.cs`; `.../RevengeContextQuery.cs` | `Terraria.DeathPenaltyAndRevenge` | 与 NPC type/netID、spawn/eligibility 和现有 target snapshot owner 整合 |
| C12 | `CoinLossRevengeSystem.cs:44-62` | `src/DeathPenaltyAndRevenge/RevengeMarkerValueComponent.cs`; `.../RevengeRespawnAttemptComponent.cs`; `.../RevengeRespawnSystem.cs`; `.../RevengeRespawnAttemptProjection.cs` | `Terraria.DeathPenaltyAndRevenge` | 与 `RevengeMarkerLifecycleComponent`、Player spawn/respawn owner 整合 |
| C13 | `CoinLossRevengeSystem.cs:279-299` | `src/DeathPenaltyAndRevenge/RevengeCachePolicy.cs`; `.../RevengePlayerProximityPolicy.cs`; `.../RevengeMarkerRegistryComponent.cs`; `.../RevengeClockState.cs`; `.../RevengeRegistrySystem.cs`; `.../RevengeNetworkProjection.cs` | `Terraria.DeathPenaltyAndRevenge` | 与现有 registry/lifecycle/identity/target 组件、WorldStorage save/load 和 protocol packets 整合 |
| C14 | `TeleportPylonsSystem.cs:15-23` | `src/Teleportation/PylonRegistryComponent.cs` or existing registry owner; `.../PylonRefreshPolicy.cs`; `.../PylonRegistryProjection.cs`; `.../Adapters/SceneMetricsAdapter.cs` | `Terraria.Teleportation` | 与 `src/WorldStorage/PylonRegistryComponent.cs`、`PylonRegistryState.cs`、`PylonRegistryEntry.cs` 和 Dome Pylon registry 只能保留一个 owner |

### 3.1 源文件到目标文件的实施规则

- 先建立目标类型和只读快照/Query，再切换一个调用点；旧 Version4 facade 和旧字段保持不变直到唯一写入 verifier 通过。
- 每次移动或拆分都记录源路径、目标路径、namespace、调用者和依赖影响；目录改变不自动改变 namespace。
- 一个核心 public 类型一个同名 PascalCase 文件；短生命周期 payload 可按领域放入 `Commands`，不创建长期 ECS entity。
- 兼容层只允许把旧读取映射到新 snapshot；旧字段和新 component 禁止同时接受写入。

## 4. 单一写入 owner 与兼容窗口

| 边界 | 唯一 proposed writer | 只读消费者 | 兼容窗口 | 双写禁止条件 |
|---|---|---|---|---|
| Liquid budget/panic | `LiquidFlowSystem` via `ILiquidFlowCommitPort` | work queue、pump、WorldGen、projection | 旧 `Liquid` static fields 只作 facade read | 任何调用方直接写旧 static 或 component 均失败 |
| Liquid work/buffer | `LiquidWorkQueueSystem` / `LiquidBufferCommitSystem` | flow system、tile snapshot | 旧 arrays 由 adapter 读 | 新旧队列同时入队同一坐标时 verifier 拒绝 |
| Liquid publication | `LiquidChangePublicationProjection` | network adapter、snapshot writer | 旧 `_netChangeSet` 只由兼容 adapter 接收 | network adapter 不能加入或清空 authority set |
| Wiring propagation | `WiringPropagationSystem` | mechanism/pump/teleport queries | `Wiring` facade 转发 command | 传播集合不能由多个 system 清空 |
| Wiring pump/teleport | `WiringTraversalAdapter` + external teleport owner | Liquid/Player/Teleportation | old arrays read-only during migration | P01 不提交实体位置或 Tile liquid |
| Mechanism cooldown | `MechanismCooldownSystem` | mechanism commit coordinator | `MechanismCooldownComponent` mapping only | queue index/time arrays不得被旧方法和新 system 同时写 |
| Collision query | `CollisionQueryPort` per invocation | Physics/Player/NPC/Projectile | old static flags wrapped and cleared | no persistence/network write from scratch |
| Revenge marker identity/lifecycle | `RevengeMarkerLifecycleSystem` | context/value/registry/projection | existing marker components remain facade | ID allocator and persistence restore cannot both advance counter |
| Revenge registry/time | `RevengeRegistrySystem` + explicit clock | respawn query, network projection, save adapter | existing registry component mapped once | list, lock and gameTime no second owner |
| Pylon registry | one owner pending integration-review | teleport query, network projection, joining adapter | WorldStorage/Dome candidates not dual active | no two lists broadcast the same diff |

## 5. Command、Query、Adapter 和 Projection 边界

- Query 只接受显式 snapshot、policy 和 tick；不读取 `DateTime`, `Random`, `Main`, 网络连接或可变全局集合。
- Command 只表达输入意图、队列 entry、结构变更或一次性结果；Command 被拒绝时不得部分改写 authority。
- CommitPort 是唯一状态写入口，负责顺序、失败和重复语义；调用者不能取得可变集合引用。
- NetworkAdapter 将不可变 projection 转换为 packet；投递成功、失败、重试和去重必须记录，不能反向写 authority。
- PersistenceAdapter 只读 owner snapshot 并返回 load result；在格式、版本、ID 恢复证据闭合前保持 disabled/blocked。
- SceneMetrics、TileEntity、WorldStorage 和 legacy Terraria 类型只在 adapter 层出现，不能渗透核心 component。

## 6. 显式调度草案

文件和目录不表达顺序。最终 scheduler contract 至少需要表达以下依赖：

```text
WorldLoad/Reset
  -> LiquidFlowInitialize + WiringInitialize + RevengeRegistryReset + PylonRegistryReset
  -> InputCommandCollection
  -> LiquidWorkQueueCommit
  -> LiquidFlowSystem
  -> WiringPropagationSystem
  -> WiringMechanismCooldownSystem
  -> WiringPumpTransferSystem
  -> CollisionQuery/ContactResult generation
  -> Teleport intent/result commit
  -> Revenge Update/CheckRespawns
  -> Projection phase (liquid, revenge, pylon, network, persistence snapshot)
```

这是依赖草案，不是已执行的运行时顺序。Liquid pump 与 Wiring 的相互依赖、Collision 与 movement 的读写阶段、Revenge respawn 与自然重生竞争、Pylon refresh 与 TileEntity update 的先后都必须由 integration-review 和 focused verifier 定稿。

## 7. C01 执行 checkpoint（计划已保存）

### 7.1 小步实施顺序

1. 锁定 `LiquidFlowBudgetPolicy` 的默认值、配置来源和 reduced-liquid 适配输入；不创建第二份容量常量。
2. 添加纯 `LiquidFlowBudgetQuery` verifier，覆盖非负计数、高水位、panic 进入/恢复和有效预算计算。
3. 建立 `LiquidFlowBudgetAndPanicStateComponent` 的构造、Reset 和不可变读取快照；不接入旧写者。
4. 建立 `ILiquidFlowCommitPort` 与 `LiquidFlowSystem` 的 command/result 接口；副作用只在 commit/adapter 边界。
5. 逐调用点将 `Liquid.ReInit`、`Liquid.UpdateLiquid` 的预算读写接到 facade；先保留旧 API，确认新 system 是唯一写者。
6. 接入 Liquid work/buffer command 的只读输入；C02/C03 未完成前不可删除旧队列字段。
7. 接入 Liquid publication projection 的只读输出；C04 未完成前不可把网络 dirty set 迁出兼容 adapter。
8. 运行 focused verifier，再按仓库规则串行编译受影响项目；本步骤当前未执行。

### 7.2 C01 回滚条件

- 发现 `maxLiquid`, `curMaxLiquid`, `numLiquid` 或 panic flags 存在未记录的第二写者。
- Reduced-liquid 配置、WorldGen 初始化或 Reset 产生与旧默认值不一致。
- Budget exhaustion 与旧 `stuck`/`panicMode` 状态不能形成可重复状态转换。
- 新 component 与 `LiquidWorldRuntimeStateComponent` 或 Dome `LiquidWorldStateComponent` 出现双重持久化/网络 owner。
- Query 访问全局可变状态，或 commit 失败后出现部分更新。

回滚时保留新 verifier 和设计记录，关闭新 facade 路径，恢复旧写入根；不得删除或覆盖其他会话文件。

## 8. focused verifier 与串行构建计划

计划项目须按实际改动选择，且每个 compile-capable command 必须从仓库根目录通过 `Build/Tools/Invoke-SerialDotnet.ps1`，先检查活动 `dotnet.exe/csc.exe`，只运行一个进程，使用 `-p:UseSharedCompilation=false`、`-m:1`、`-nr:false`、`-p:MSBuildNodeReuse=false`、`-p:BuildInParallel=false`。示例计划如下，当前不执行：

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\dome\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\dome\Test\Terraria.Dome.Liquid.Verification\Terraria.Dome.Liquid.Verification.csproj `
  --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

历史计划基线：实际项目路径、artifact 路径、warning/error 统计和退出码当时尚未记录；
获准实施后的真实结果见 11.23，当前 metadata 为 `verificationStatus: focused-c10-c14-passed`。

## 9. C02-C14 后续 checkpoint 入口

| checkpoint | 进入前置条件 | 必须保存的设计/执行事实 |
|---|---|---|
| C02 | C01 文件已保存且唯一 owner 暂不冲突 | work item 是 command，不是持久实体；队列消费和 retry 清理 |
| C03 | C02 command seam 已定义 | buffer count/entries 与 Tile checking 的提交和清理 |
| C04 | C01-C03 publication input 明确 | dirty set 的单向网络 projection、交换和去重 |
| C05 | Wiring 输入与 propagation 触发点闭合 | 队列、gate/lamp phases、current user/color 和 ClearAll |
| C06 | C05 propagation commit 明确 | pump/teleport scratch、Liquid/Teleportation 交接 |
| C07 | mechanism coordinator 依赖明确 | capacity、cooldown、device-specific state 和 retry |
| C08 | Collision owner 决议明确 | query scratch 的清空/失效，不持久化 |
| C09 | C08 result port 明确 | TileContact/HurtTile 值对象、调用期间生命周期 |
| C10 | Revenge marker identity owner 明确 | expiration policy、ID 分配和 persistence identity |
| C11 | NPC context read boundary 明确 | capture snapshot、NPC type/netID 外部引用和资格 Query |
| C12 | respawn commit root 明确 | value、force expire、attempt lock 与自然重生竞争 |
| C13 | registry/time/network/persistence owner 明确 | lock、gameTime、marker list、save/load 和 packet projection |
| C14 | TileEntity/WorldStorage/Dome registry owner 明确 | current/old list diff、refresh、SceneMetrics 和 join projection |

## 10. 完成判定

历史设计阶段的文档完成判定是：设计文件和执行文件覆盖 14 个 checkpoint、113 个成员、源/目标映射、唯一写入 owner、依赖方向、网络/持久化边界、focused verifier、风险、回滚和 Integration Handoff；当时 metadata 为 `executionStatus: planned`、`implementationStatus: not-started`、`verificationStatus: not-run`。该历史判定不是 C# 迁移完成判定；当前 metadata 和真实验证结果见 11.23。

最终 Complete 前必须做只读文档检查：目标文件存在、metadata 一致、113 个成员序号各出现一次、没有其他分区 ID、没有生产代码变更、没有未运行 verifier 的成功声明。按用户要求不执行 `git diff --check`；改用 PowerShell 检查尾随空格、异常空行、关键字段和文件存在性。

## 11. 当前 checkpoint

本节 11.1-11.17 保留设计阶段的历史计划快照，11.18-11.22 保留实施过程中的历史结果；
它们的局部 `completedComponents`、`currentComponent`、`pendingComponents` 和
`verificationStatus` 不覆盖文件顶部 metadata。当前唯一有效状态以 11.23 为准。

### 11.1 C02 执行 checkpoint

#### 实施步骤

1. 盘点 `Liquid.AddWater`、`Liquid.Update`、`Liquid.UpdateLiquid`、`DelWater` 和 `LiquidBuffer` 的所有调用点，记录每个读写者；没有完整调用图前不创建第二个 queue owner。
2. 以 `LiquidWorkEntry`、Dome `LiquidWorkItemComponent` 和权威报告的四个字段建立转换表；明确 `LiquidType/Amount/Sequence` 是外部输入还是另一个子系统的字段，禁止静默丢弃。
3. 添加 `LiquidCellWorkItemStateCommand` 和 `LiquidWorkItemReadyQuery` 的纯 verifier；不接入生产调度。
4. 添加 `ILiquidWorkQueueCommitPort` 与 `LiquidWorkQueueSystem` 的接口草图，明确 enqueue/merge/drain/requeue/remove/reset 的幂等结果。
5. 建立旧 `Main.liquid` 数组到 proposed queue 的只读 facade；在新 owner 通过 verifier 前，旧路径仍是唯一写入者。
6. 逐步切换一个入队和一个消费调用点，验证同一坐标去重、kill/delay 清理和 Tile `checkingLiquid` 标记；任何失败都回滚该调用点。
7. C03 未完成前不改变 capacity-full 分支；C04 未完成前不改变 dirty network publication。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria\Liquid.cs:44-50` | `LiquidCellWorkItemStateCommand.cs` | 保留 `x/y/kill/delay` 语义；旧 `Liquid` facade 仅适配 |
| `D:\TRbackup\Version4\Terraria\Liquid.cs:470-1000` | `LiquidWorkQueueSystem.cs` | 需要 Tile snapshot、C01 flow owner、C03 buffer owner |
| `D:\TRbackup\NLTX\src\WorldStorage\LiquidWorkEntry.cs` | conversion adapter, not replacement | `TileCoordinate`, byte representation 和 kill/delay 的格式兼容待审查 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\Components\LiquidWorkItemComponent.cs` | input adapter, not duplicate owner | `LiquidType/Amount/Sequence` 的命名和来源保持外部边界 |

#### 单一写入、兼容与回滚

- 迁移前：`Main.liquid`/旧 Liquid methods 是唯一写入根；proposed queue 只能接收镜像输入或测试夹具。
- 兼容窗口：旧读路径可由 adapter 从 queue snapshot 提供，旧写 API 只能转成 command 并由一个 commit port 执行。
- 双写禁止：同一坐标不得同时写 `Main.liquid` entry 和新 queue entry；不得由 C01 system 直接修改 C02 queue。
- 回滚触发：坐标去重不等价、kill/delay 清理漏标记、loading/worldgen 时序改变、队列恢复产生重复传播或新旧队列各自提交 Tile。回滚只关闭新的 facade 路径，不删除证据或其他会话文件。

#### C02 验证计划

需要 focused verifier 覆盖 command schema、坐标/计数边界、重复 enqueue、delay/kill 状态机、reset、capacity handoff 和只读集合隔离；需要静态检查确认 Query 不读外部可变状态。获准实现后按仓库 BUILD-CONCURRENCY-1 规则串行 build 受影响项目，再用 `--no-build --no-restore` 运行 verifier；当前没有 exit code、warning/error 统计或 artifact 路径可记录。

### 11.2 C03 执行 checkpoint

#### 实施步骤

1. 比对 Version4 `LiquidBuffer.AddBuffer/DelBuffer` 与 NLTX `LiquidBufferEntry`, `LiquidWorkQueueState`, `TileLiquidWorkStateComponent` 的字段语义和 bounds 检查。
2. 定义 `LiquidBufferQueueStateComponent` 的最大长度、队列条目不可变读取和坐标去重行为；不把 Tile liquid amount/type 复制到 C03。
3. 定义 `LiquidBufferCommand`、`ILiquidBufferCommitPort` 和 `LiquidBufferCommitSystem` 的 enqueue/drain/release/reset 结果，显式说明 C02 拒绝时的恢复策略。
4. 先添加纯 verifier 和 adapter contract，再切换一个 `AddWater` capacity-full 调用点；旧 buffer 仍为唯一 writer 直到 focused verifier 通过。
5. 切换 `UpdateLiquid` 周期 drain：先清除 Tile checking 标记，再提交 C02 command；检查异常路径不造成永远锁定或重复坐标。
6. C04 未完成前，保留旧 C04 dirty publication facade，不从 C03 直接发 network packet。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria\LiquidBuffer.cs:5-30` | `LiquidBufferQueueStateComponent.cs`, `LiquidBufferCommand.cs`, `LiquidBufferCommitSystem.cs` | 保持 count/coordinate/dequeue 语义；实现可更换但清理顺序不可变 |
| `D:\TRbackup\Version4\Terraria\Liquid.cs:1159-1162` | `LiquidBufferCommitSystem` -> C02 command | capacity release 与 active queue 提交必须是显式边界 |
| `D:\TRbackup\NLTX\src\WorldStorage\LiquidBufferEntry.cs` | conversion adapter | WorldStorage 只作为候选 storage substrate，不宣布 C03 owner |
| `D:\TRbackup\NLTX\src\WorldInteraction\Tiles\TileLiquidWorkStateComponent.cs` | tile checking adapter | C03 只发送 set/clear intent，Tile owner 仍需 integration-review |

#### 单一写入与回滚

- 旧窗口：`LiquidBuffer.AddBuffer/DelBuffer` 是唯一修改 active buffer 的路径；new component 只能在 shadow verifier 中构造。
- 接管窗口：一次只切换 capacity-full enqueue 或 cycle drain 的一侧，禁止 enqueue 和 drain 同时切换。
- 双写禁止：`LiquidBuffer.numLiquidBuffer` 和新 `Count` 不得各自接受写入；每个坐标不能同时保留在 active queue 和 deferred queue，除非有明确 retry token。
- 回滚：任何 capacity boundary、checking flag、drain order、duplicate suppression 或 load/reset 差异都回滚新 adapter，保留旧数组路径和测试证据。

#### C03 验证计划

需要验证最大容量、重复入队、drain/requeue、C02 拒绝、Tile flag 清除、Reset、异常和不可变 snapshot。获准实施后按 BUILD-CONCURRENCY-1 规则串行构建受影响项目并核验 `Build/bin/` artifact；当前没有 build/test 结果。

### 11.3 C04 执行 checkpoint

#### 实施步骤

1. 比对 Version4 `NetSendLiquid` 和 `UpdateLiquid` 的 pending/swap/广播顺序，确定坐标编码、worldgen/loading 抑制和批次边界。
2. 以 NLTX `LiquidReplicationDirtySet`、Dome `LiquidReplicationSystem` 和协议 codec 为候选输入，确认 revision、chunk、排序和 latest-value 语义，不创建第二个 dirty owner。
3. 定义 immutable `LiquidChangePublicationSnapshot` 和 `LiquidChangePublicationProjection`；加入同坐标去重、稳定排序和 publishing/pending 隔离 verifier。
4. 定义 `LiquidNetworkAdapter`/`ILiquidPublicationPort`，明确成功、失败、超时未知、取消和重试/去重语义；适配器不回写 C01-C03。
5. 先以 shadow projection 对比旧 `_netChangeSet` 输出，再只切换一个广播调用点；旧集合保持兼容 facade 直到 loopback verifier 通过。
6. 验证 duplicate packet、连接断开、worldgen/load suppression 和批次新增变更；未闭合前不能删除旧集合。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria\Liquid.cs:56-68` | `LiquidChangePublicationProjection` input | 坐标编码和 lock 语义必须保留或显式版本化 |
| `D:\TRbackup\Version4\Terraria\Liquid.cs:1181-1186` | `LiquidNetworkAdapter` | swap then broadcast then clear 的失败语义需定稿 |
| `D:\TRbackup\NLTX\src\WorldStorage\LiquidReplicationDirtySet.cs` | candidate dirty substrate | 不与 Dome replication system 双重发布 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Systems\LiquidReplicationSystem.cs` | candidate pure projection logic | 只读 commands；revision and transport remain adapter concerns |
| `D:\TRbackup\NLTX\dome\Test\Terraria.Dome.Liquid.Loopback.Verification\Program.cs` | focused loopback evidence | 需确认实际 codec/connection contract，当前不宣称通过 |

#### 单一写入、兼容与回滚

- Liquid commit owner 只产生 dirty facts；C04 是唯一 publication owner；network adapter 只发布。
- 兼容窗口允许旧 `_netChangeSet` 继续接收旧 facade 的 dirty 标记，但新 projection 不得再加入旧集合并发写入。
- 如果发送失败且确认状态未知，保留 publishing batch 或转入显式 retry record，不直接清空并假设丢失无害。
- 回滚条件：输出坐标/顺序/最新值与旧协议不等价，worldgen 误广播，重复 packet 造成错误 Tile 结果，或 projection 修改 authority。

#### C04 验证计划

验证 projection 纯度、batch swap、dedup、stable ordering、revision、packet loopback、duplicate/retry、disconnect 和 loading suppression。获准实施后按 BUILD-CONCURRENCY-1 串行 build/test；当前无 exit code、warning/error 或 `Build/bin` artifact 结果。

### 11.4 C05 执行 checkpoint

#### 实施步骤

1. 盘点 `Initialize`, `ClearAll`, `SetCurrentUser`, `HitSwitch`, `PokeLogicGate`, `TripWire`, `HitWire`, `PixelBoxPass` 和 `LogicGatePass` 的完整读写调用图。
2. 将 `WirePropagationScratchComponent`、Dome wire topology/traversal 类型与 Version4 集合逐项对照；确定一个 scratch owner 和一个 topology owner。
3. 建立 `WiringPropagationCommand`, `WiringPropagationQuery`, `WiringPropagationStateComponent`, `WiringExecutionContextComponent` 和 commit port 的 contract verifier；不接入 tile/network side effects。
4. 先迁移 `HitSwitch`/`PokeLogicGate` 输入适配，保留旧 `Wiring` facade；确认 CurrentUser sentinel、wire color、bounds 和重复输入语义。
5. 迁移 frontier/skip/toProcess，再迁移 gate/lamp/pixel phases；每一步验证循环终止和 `running` 清理。
6. 将 tile frame、sound、NetMessage、NPC/item/projectile 结果转换为显式 commands/events；不得在 Query 或 component 内执行副作用。
7. C06/C07 尚未完成前，ClearAll 的跨组 reset 只能由 coordinator 适配，不能让 C05 清空其他组组件。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria\Wiring.cs:19-37,65-67` | C05 propagation/context files | 原集合类型、sentinel 和每 run 清理语义保持 |
| `D:\TRbackup\Version4\Terraria\Wiring.cs:88-127` | `WiringPropagationSystem.InitializeWorld` | 与 C06/C07 初始化 coordinator 分离 |
| `D:\TRbackup\Version4\Terraria\Wiring.cs:267-395` | `WiringInputAdapter` -> `WiringPropagationCommand` | Sound/frame/network/effect 全部外置 |
| `D:\TRbackup\Version4\Terraria\Wiring.cs:477-665` | `WiringPropagationSystem` | current file stubs require full-reference comparison and `version-drift` verifier |
| `D:\TRbackup\NLTX\src\WorldInteraction\Wiring\WirePropagationScratchComponent.cs` | candidate component substrate | 不得与 Dome traversal state 形成第二个写入根 |

#### 单一写入与回滚

- 旧窗口：Version4 `Wiring` 静态集合是唯一写入根；NLTX scratch 只作 shadow or adapter input。
- 接管顺序：先 command/query，后 propagation state，再 effects/projections；不在同一批次切换所有 wire colors。
- 双写禁止：新 system 不得与旧 `TripWire/HitWire` 同时消费或清空同一队列；`running` 只有一个 owner。
- 回滚条件：gate/pixel phase 顺序改变、循环 guard 漏失、CurrentUser 语义改变、输入重复触发额外机制、stub 与完整参考的差异被误当作已实现。

#### C05 验证计划

需要 focused verifier 覆盖 command validation、wire traversal、gate cycle, phase ordering, reset, duplicate input, external effect command ordering and no-query-write。获准实施后按 BUILD-CONCURRENCY-1 串行 build/test，当前无实际构建或测试结果。

### 11.5 C06 执行 checkpoint

#### 实施步骤

1. 对照 current Version4 与完整参考的 `_teleport`, `XferWater`, `Teleport`, pump slot bounds 和 `blockPlayerTeleportationForOneIteration` 调用链，记录 `version-drift`，禁止直接复制完整参考实现。
2. 将 NLTX `PumpTransferScratchComponent`, `PortalNetworkState` 和 Dome `PumpCommandSystem` 的字段/命名与 P01 目标做 owner 分析；确认 pump scratch 不拥有 Liquid/portal authority。
3. 定义 bounded `PumpTransferScratchComponent`、`WiringTeleportStateComponent`、pump/teleport commands 和 adapter ports；添加 endpoint/count/duplicate/invalid input verifier。
4. 在 C05 传播完成后先生成 shadow intents；由 Liquid/Teleportation owner 的 test adapter 验证顺序和失败结果。
5. 逐步切换 pump activation 到 `LiquidTransferCommand`，逐步切换 teleport endpoint 到 external traversal command；保留旧 Wiring facade 作为兼容读路径。
6. 验证 one-iteration block flag 在 gate pass、早退和异常路径都清理；不得将 Player/NPC position 写入 C06。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria\Wiring.cs:17,39-53` | C06 component files | 保持 sentinel、capacity、count 和 reset 语义 |
| `D:\TRbackup\Version4\Terraria\Wiring.cs:495-602` | pump/teleport intent system | current `XferWater` stub requires full-reference verifier |
| `D:\TRbackup\Version4\Terraria\Wiring.cs:2704-2750` | `WiringTraversalAdapter` | Player/NPC teleport remains external commit |
| `D:\TRbackup\NLTX\src\WorldInteraction\Wiring\PumpTransferScratchComponent.cs` | candidate scratch substrate | one owner after integration review |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Wiring\Systems\PumpCommandSystem.cs` | candidate command creation | sequence/type/capacity semantics must match Liquid owner |

#### 单一写入与回滚

- C06 writer owns only endpoint/pump scratch and flags; Liquid owner writes Tile liquid, Teleportation/entity owner writes positions.
- Compatibility window uses shadow commands and adapters; no simultaneous old/new execution for a pump pair or endpoint pair.
- Roll back if pump transfer amount/order differs, endpoint invalidation changes player/NPC targets, block flag leaks, or adapter failure leaves scratch dirty.

#### C06 验证计划

验证 pump/teleport command schema, bounds, deterministic ordering, Liquid handoff, external position commit, one-tick block reset, full-reference drift and loopback effects. 获准实施后按 BUILD-CONCURRENCY-1 串行 build/test；当前没有实际构建或测试结果。

### 11.6 C07 执行 checkpoint

#### 实施步骤

1. 对照 current Version4 `CheckMech` stub 与完整参考，记录 duplicate/cooldown/out-of-world 语义差异；不把 full-reference 行为写成已验证。
2. 比对 NLTX `MechanismCooldownComponent`, `MechanismCooldownEntry`, Dome `MechanismRetryState`/`MechanismCommitCoordinator`，裁决 schedule、retry 和 device cooldown 的 owner。
3. 建立 bounded schedule component、device cooldown component、definitions、activation query 和 `MechanismCooldownSystem` contract verifier。
4. 迁移 `UpdateMech` 的纯 tick/deletion decision 到 system；Tile frame, NetMessage, NPC/item/projectile/sound effects 先产生 commands。
5. 逐步接入 `CheckMech` callers；在 check semantics 未闭合前保留旧 facade，禁止新旧 schedule 双写。
6. 让 C05/C06/C07 reset 由 coordinator 显式排序，验证清理和异常路径。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria\Wiring.cs:55-75` | C07 definitions/components | capacity/cooldown/hitbox defaults preserved |
| `D:\TRbackup\Version4\Terraria\Wiring.cs:151-263` | `MechanismCooldownSystem` | schedule tick and effect commands; Tile/Net side effects external |
| `D:\TRbackup\Version4\Terraria\Wiring.cs:464-465` | `MechanismActivationQuery` / compatibility facade | current stub marked `version-drift` |
| `D:\TRbackup\NLTX\src\WorldInteraction\Wiring\MechanismCooldownComponent.cs` | candidate adapter | no duplicate active writer |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Wiring\Systems\MechanismCommitCoordinator.cs` | cross-domain commit order evidence | must not become hidden global scheduler |

#### 单一写入与回滚

- C07 system is the only writer of schedule entries and three device cooldowns; effect owners only consume commands.
- Compatibility window shadows schedule decisions and keeps legacy arrays as read/write owner until parity verifier passes; no dual enqueue.
- Roll back if check semantics, expiration order, device cooldown ticks, capacity or effect side effects differ, or schedule removal skips an entry.

#### C07 验证计划

需要验证 capacity, duplicate schedule, countdown, expiry, reset, out-of-world cleanup, device cooldown, hopper geometry, full-reference drift, effect command ordering and no-query-write。获准实施后按 BUILD-CONCURRENCY-1 串行 build/test；当前没有实际构建或测试结果。

### 11.47 C07 bounded components 源码实施 checkpoint

- 本 checkpoint 实际保存 `src/WorldInteraction/Wiring/WiringMechanismScheduleComponent.cs` 和
  `src/WorldInteraction/Wiring/WiringDeviceCooldownComponent.cs`。`WiringMechanismScheduleComponent`
  固定容量为 `1000`，复用 `MechanismCooldownEntry` 保存机制坐标与剩余 tick，提供 `Count`、
  防御性只读条目视图和内部 `Reset()`；`WiringDeviceCooldownComponent` 保存
  `CannonCooldownTicks`、`BunnyCannonCooldownTicks`、`SnowballCannonCooldownTicks`，默认值为
  `0`，并提供内部 `Reset()`。
- 两个组件只保存状态，不执行重复 schedule、倒计时、过期删除、冷却判断、Tile frame、网络发布
  或机制效果；未修改旧 `MechanismCooldownComponent`、`MechanismCooldownEntry` 或既有 C05/C06
  wiring owner。
- 按本次“只写 ECS 组件源码”的执行边界，`WiringMechanismSchedulePolicy`、`HopperInteractionPolicy`、
  `MechanismCooldownSystem` 和 `MechanismActivationQuery` 未创建，旧 `_mechX/_mechY/_mechTime`、
  `_numMechs`、`CheckMech`、`UpdateMech` 以及 Tile/network/effect owner 未接入。
- 两个源码文件已保存并通过 `Terraria.WorldInteraction.csproj` 串行 build（退出码 `0`、`0` errors、
  `15` 个既有 `WorldStorage` warnings，产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`），
  不写入仓库的 focused reflection verifier 输出 `C07_COMPONENT_VERIFIER=PASS ASSERTIONS=13`，
  覆盖固定容量 1000、默认计数/冷却值、只读条目视图、schedule entry storage reset 和三个设备
  cooldown reset；因此 C07 保持 `partial`，`completedComponents` 仍为空。保存本 checkpoint 后，
  执行游标为 `currentComponent: C08`，`pendingComponents: [C08, C09, C10, C11, C12, C13, C14]`。

### 11.7 C08 执行 checkpoint

#### 实施步骤

1. 完整盘点 `WetCollision`, `SlopeCollision`, `TileCollision`, `BuildTileContacts`, `StepConveyorBelt` 的所有 flag/cache 读写和嵌套调用。
2. 解决 `src/Physics/CollisionResultComponent.cs` 与 `src/SpatialSimulation/CollisionResultComponent.cs` 的 owner 冲突；在裁决前不创建第三个结果组件。
3. 建立 `CollisionQueryScratch`, policy, query port 和 projection 的纯度/重入 verifier；输入使用 immutable tile snapshot。
4. 先把 legacy static flags 包装为一次调用的 adapter，验证 entry/exit 清理和不可并发假设，再逐个切换 Physics/Spatial callers。
5. 将 `contacts`/conveyor cache 的填充和消费限制在单次 query；C09 的 value results 从 snapshot 交接，不泄露 List 引用。
6. 删除兼容层前验证 nested calls, exception cleanup, honey/shimmer/stair flags, no-persistence and no-network semantics。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria\Collision.cs:55-73` | C08 query/definition files | static scratch 变为 scoped context，常量保留 |
| `D:\TRbackup\Version4\Terraria\Collision.cs:1001-1079` | WetCollision query adapter | honey/shimmer 是结果，不是 entity authority |
| `D:\TRbackup\Version4\Terraria\Collision.cs:1232-1630` | slope/tile query adapter | flags 与 movement commit 分离 |
| `D:\TRbackup\Version4\Terraria\Collision.cs:1472-1540` | C09 contact result input | contacts clear/fill lifecycle preserved |
| `D:\TRbackup\NLTX\src\Physics\CollisionResultComponent.cs` and `src\SpatialSimulation\CollisionResultComponent.cs` | integration review | no duplicate final result owner |

#### 单一写入与回滚

- Query scratch owner writes only its invocation context; Physics/Spatial systems write their own committed movement/contact results.
- Compatibility window is one-call static adapter; no concurrent legacy calls unless the adapter proves isolation.
- Roll back if flag values leak between calls, nested query values overwrite outer context, static concurrency is unsafe, or owner conflict remains unresolved.

#### C08 验证计划

验证 flag reset, nested/reentrant calls, contacts/conveyor cache clear, immutable outputs, pure query and Physics/Spatial owner integration. 获准实施后按 BUILD-CONCURRENCY-1 串行 build/test；当前没有实际构建或测试结果。

### 11.48 C08 组件范围阻塞 checkpoint

- 当前实现范围只允许 ECS component 源码，而 C08 设计中的 `CollisionQueryScratch` 明确是一次
  query invocation 的临时 scratch，不是长期实体/世界组件；新增 `CollisionQueryScratchComponent`
  会改变设计的生命周期与 owner 语义，因此本 checkpoint 不创建该文件。
- `contacts` 需要未在当前 `src` 建立的 `TileContact` 值类型，`_cacheForConveyorBelts` 需要
  未裁决的 conveyor point 类型；`bottomFluff` 属于 Definition，flags 的清理/覆盖属于 Query
  生命周期，不能在组件中实现行为。另有 `Physics` 与 `SpatialSimulation` 两个既有
  `CollisionResultComponent` owner 冲突未裁决。
- C08 标记为 `blocked`，没有新增源码或 build 证据；没有修改既有 Physics/Spatial 结果组件，
  也没有创建值对象、Query、Definition、Projection、Tile/entity/network/persistence adapter。
  保存本 checkpoint 后执行游标推进到 `C09`，`pendingComponents` 为
  `[C09, C10, C11, C12, C13, C14]`。

### 11.8 C09 执行 checkpoint

#### 实施步骤

1. 以 `D:\TRbackup\Version4\Terraria\Collision.cs:21-53,1472-1620,2348-2447` 为只读源，核对 `TileContact`、`HurtTile`、`BuildTileContacts` 和 `HurtTiles` 的声明、边界 clamp、扫描顺序、坡面分支和未命中哨兵；把当前实现与完整参考的差异登记为 `version-drift`，不得直接复制完整参考行为。
2. 对照 C08 的 `CollisionQueryScratch` 以及 `src/Physics/CollisionResultComponent.cs`、`src/SpatialSimulation/CollisionResultComponent.cs`，完成 Physics/Spatial 结果 owner 的 integration review；在裁决前不新建第三个长期结果组件。
3. 定义 `TileContactResult`、`HurtTileResult` 和 `CollisionContactQuery` 的输入/输出 contract。输入使用 immutable Tile snapshot、geometry、world bounds、hazard tables 和只读 Player qualification context；输出使用不可变顺序快照。
4. 先建立纯 query contract verifier，覆盖 contact clear/ordering、half-brick/slope/solidTop、hurt sentinel、first-hit semantics、bounds 和 no-write；不接入 movement、damage、network 或 persistence side effects。
5. 在兼容窗口保留旧 `Collision.BuildTileContacts`/`HurtTiles` facade 为唯一旧 writer，新增 query 只做 shadow comparison；通过 parity 后按调用者逐步切换，禁止旧列表和新结果同时对同一调用执行副作用。
6. 将 movement/contact、projectile hazard、Player/NPC damage 和 network effects 逐一改为消费只读 result 或显式 command；验证异常、嵌套 query 和 early return 都清理 C08 scratch。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria\Collision.cs:21-44` | `Physics/Results/TileContactResult.cs` | 保留 Side、Overlap、Tile 坐标、Slope、Type 的值语义和稳定顺序 |
| `D:\TRbackup\Version4\Terraria\Collision.cs:46-53` | `Physics/Results/HurtTileResult.cs` | 保留命中字段和 `type = -1` 未命中 sentinel，不携带 Player 引用 |
| `D:\TRbackup\Version4\Terraria\Collision.cs:1472-1620` | `Physics/Queries/CollisionContactQuery.cs` | 保留 clamp、扫描和坡面 contact 分支，Tile 读取改为显式 snapshot |
| `D:\TRbackup\Version4\Terraria\Collision.cs:2348-2447` | `CollisionContactQuery` hurt operation | 保留 first-hit、hazard qualification 和 no-hit behavior；伤害 effect 外置 |
| `D:\TRbackup\NLTX\src\Physics\CollisionResultComponent.cs` and `src\SpatialSimulation\CollisionResultComponent.cs` | integration-review decision | 避免重复长期 owner 和不可见双写 |

#### 单一写入、兼容与回滚

- C09 proposed writer 只构造本次 query 的不可变结果；Physics、Player、NPC、Projectile 和网络层分别拥有自己的 committed state/effect。
- 兼容期旧 facade 仍是运行时 writer；新 query 只能 shadow 读取和比较，不能把同一 list、旧 static flags 或新 result 同时交给两个执行路径。
- 若 contact 顺序、首命中 Tile、哨兵值、坡面 overlap、world bounds 或 `fireWalk` qualification 改变，或任何 query 写入 Tile/actor/network，则停止切换并回滚到旧 facade。没有 owner integration review 结论时不删除旧类型。

#### C09 验证计划

需要 focused verifier 覆盖 immutable snapshot、contact clear/order、slope/half-brick/solidTop、hurt first-hit/no-hit、hazard context、nested/reentrant isolation、no side effects 和 Physics/Spatial owner resolution。当前没有 C# 实施、dotnet 命令或验证结果。

### 11.49 C09 组件范围阻塞 checkpoint

- C09 的 `TileContactResult`、`HurtTileResult` 和 `CollisionContactQuery` 都是一次查询的不可变
  结果/纯 Query，不是长期 ECS component；创建 `TileContactResultComponent` 或
  `HurtTileResultComponent` 会改变设计的生命周期语义。
- 当前 `src` 没有可复用的 `TileContact`/`HurtTile` 值对象，且 `Physics` 与 `SpatialSimulation`
  已有两个 `CollisionResultComponent` owner 未裁决。按本次只允许组件源码的执行边界，不创建值对象、
  Query、第三个结果组件，也不修改现有 result owner 或接入 Tile/Player/NPC/network/damage。
- C09 标记为 `blocked`，没有新增源码或 build 证据。保存本 checkpoint 后，执行游标为
  `currentComponent: C10`，`pendingComponents: [C10, C11, C12, C13, C14]`。

### 11.50 C10-C14 组件源码复核与串行重建 checkpoint

- C10-C14 可独立确定的组件源码已存在：C10 为 `RevengeMarkerIdentityComponent`、
  `RevengeMarkerLifecycleComponent`；C11 为 `RevengeTargetSnapshotComponent`；C12 为
  `RevengeMarkerValueComponent`、`RevengeRespawnAttemptComponent`；C13 为
  `RevengeMarkerRegistryComponent`；C14 沿用现有 `src/WorldStorage/PylonRegistryComponent.cs`
  与 `PylonRegistryEntry`，没有创建第二个 Pylon registry owner。
- 本 checkpoint 只复核并编译现有组件，没有新增同义组件，也没有改写 legacy marker、NPC、TileEntity、
  network、persistence 或 registry writer。C10/C12 的生命周期重叠、C11 的 NPC owner、C13 的
  registry/clock/projection 和 C14 的 owner/equality-diff 仍属于 integration review。
- 实际命令均经 `Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 串行执行：
  `build .\src\DeathPenaltyAndRevenge\Terraria.DeathPenaltyAndRevenge.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`
  与 `build .\src\Teleportation\Terraria.Teleportation.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；两者均退出码 `0`、`0` warning、`0` error。
- 因跨域 owner 尚未裁决，C10-C14 仍保持 `partial`，`completedComponents` 仍为空；当前游标保持
`C10`，`pendingComponents` 为 `[C10, C11, C12, C13, C14]`。

### 11.51 P01 组件源码映射完整性与串行构建复核 checkpoint

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

### 11.52 C14 组件生命周期最小单元实施 checkpoint

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

### 11.53 C13 registry ID 只读视图最小单元实施 checkpoint

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

### 11.54 C14 registry alias-safe replacement correction checkpoint

- RED 回归探针以旧 `Terraria.WorldStorage.dll` 的 `CurrentPylons` 只读视图作为下一次
  `Replace` 输入，输出 `C14_ALIAS_REPLACE_BEFORE_COUNT=1`、`C14_ALIAS_REPLACE_AFTER_COUNT=0`，
  确认原实现的别名清空缺陷；探针没有写入仓库。
- 实际仅修改 `src/WorldStorage/PylonRegistryComponent.cs`：`Replace` 在清理 current/previous
  列表前先复制 `IReadOnlyList<PylonRegistryEntry>` 输入，再执行快照转移、cooldown 写入和
  `Revision` 递增；没有新增 System、Query、Command、Adapter、Projection、测试或 verifier 文件。
- 通过仓库 wrapper 串行 build `src/WorldStorage/Terraria.WorldStorage.csproj`：退出码 `0`、
  `0` errors、`15` 个既有 warnings，产物为
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`。
  新 DLL 的不写入仓库 focused reflection verifier 输出
  `C14_PYLON_ALIAS_FIX_VERIFIER=PASS ASSERTIONS=16`，覆盖防御性复制、alias-safe replace、
  current/previous 转移、cooldown、revision 和 reset。
- C14 仍为 `partial`，`completedComponents` 仍为空；TileEntity/SceneMetrics 扫描、equality/diff、
  network/persistence 和唯一 registry owner 仍需 integration review。

### 11.9 当前 checkpoint 状态

- C01、C02、C03、C04、C05、C06、C07、C08、C09 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09]`；`currentComponent: C10`；`pendingComponents` 为 C10-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

### 11.10 C10 执行 checkpoint

#### 实施步骤

1. 以 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:18-28,52,56,62-176` 为只读源，核对 ID counter、coin threshold、expiration calculation、force-expire 交接、constructor supplied ID 和 `UniqueID` projection；将 `ONE_MINUTE`/literal 差异登记为 `version-drift`。
2. 对照 C12 的 `_forceExpire`、C13 的 `_gameTime` 和 marker registry，先画出 deadline/force flag/registry removal 的依赖图；禁止把 C10 变成完整 RevengeMarker aggregate 或复制 C13 的 clock/lock。
3. 定义 identity allocator、expiration policy 和 marker identity/deadline contract。allocator 必须明确串行 owner、恢复 ID 校验、重复 ID、负 ID、溢出和 world reset 行为。
4. 建立纯 policy verifier，覆盖 coin thresholds、interpolation、inclusive deadline、explicit tick、force-expire input、default/restored identity 和 no-side-effect contract；不接入 network/persistence/SpawnEnemy。
5. 在兼容窗口保留旧 `RevengeMarker` constructor/`IsExpired` 为唯一运行时 writer；新 policy 只做 shadow comparison。通过 parity 后再让 C13 registry 使用 immutable C10 slice，禁止旧 marker 和新 slice 同时推进同一 deadline。
6. 由独立 projection adapter 组合 `WriteSelfTo` 所需的 identity/deadline 相关字段，并验证 V1456 marker packet/removed marker packet 的 ID mapping 不截断、不回写 C10 authority。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:18-28` | `DeathPenalty/RevengeExpirationPolicy.cs` | 保留 coin threshold、3600 tick 定义和当前插值版本差异登记 |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:52,56,62-64` | `src/DeathPenaltyAndRevenge/RevengeMarkerLifecycleComponent.cs`; `RevengeMarkerIdentityComponent.cs`; `RevengeMarkerIdentityProjection.cs` | deadline/ID 只读公开，不能暴露旧可写字段；C12 lifecycle overlap requires owner review |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:80-100` | identity/expiration lifecycle seam | supplied ID restore、allocator 和 C11/C12 constructor data 必须组合而非重复拥有 |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:158-176` | `src/DeathPenaltyAndRevenge/RevengeExpirationPolicy.cs` | `gameTime` 显式传入；`forceExpire` 由 C12 输入；不直接移除 registry |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\SyncRevengeMarkerPacket.cs`; `RemoveRevengeMarkerPacket.cs` | protocol projection adapter | 协议 ID 宽度和 authority identity 分离验证 |

#### 单一写入、兼容与回滚

- C10 writer 只负责 allocator 和 deadline slice；C12 writer 负责 force-expire/attempt lock，C13 writer 负责 registry entry 的移除/保留。
- 兼容窗口旧 marker 保持运行时 authority，新 policy 只 shadow；不得让新 allocator 和旧 `_uniqueIDCounter` 同时发号。
- 若过期边界、coin interpolation、恢复 ID、溢出、协议映射或 force-expire 组合改变，则停止切换并回滚到旧 marker lifecycle。未完成 identity/registry integration review 时不删除 `UniqueID` facade。

#### C10 验证计划

需要 focused verifier 覆盖 expiration policy、identity allocation/restore、overflow/duplicate handling、force-expire handoff、explicit tick purity 和 protocol projection。当前没有 C# 实施、dotnet 命令或验证结果。

### 11.11 当前 checkpoint 状态

- C01、C02、C03、C04、C05、C06、C07、C08、C09、C10 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10]`；`currentComponent: C11`；`pendingComponents` 为 C11-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

### 11.12 C11 执行 checkpoint

#### 实施步骤

1. 以 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:30-54,80-100,179-184,221-261` 为只读源，核对 enemy box 常量、constructor capture、`_npcNetID` mapped input、discouragement fields、centered hitbox 和 current outer-only intersection。
2. 对照 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:315-338` 与 NPC/P03 owner，记录 `RespawnEnemyID` 映射、minimum coin/world bounds/boss/worm/rarity qualification；C11 只接收显式 capture input，不复制 NPC entity state。
3. 定义 `RevengeTargetSnapshotComponent`、`RevengeEnemyContextPolicy`、`RevengeEnemyContextQuery` 和 commit port；snapshot 只能通过 capture/replace 建立，外部只能读。
4. 建立纯 context verifier，覆盖 box geometry、snapshot immutability、mapped spawn ID vs original discouragement type、outer-only intersection、Player/world query input 和 no-side-effect contract。
5. 在兼容窗口让旧 `CacheEnemy`/`RevengeMarker` 保持唯一 capture writer，新 snapshot 只做 shadow comparison；parity 后再让 C12 respawn 使用 snapshot，禁止旧 marker 与新 component 同时捕获同一 NPC。
6. 将 `WouldNPCBeDiscouraged` 转为显式 context query，保留 Player/world branch 的输入快照；spawn/effect/network/persistence 由外部 adapters 消费 decision，不在 C11 运行。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:30-34` | `src/DeathPenaltyAndRevenge/RevengeEnemyContextPolicy.cs` | 保留 2160x1440 enemy box；中心/矩形计算纯化 |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:36-54` | `src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs` | marker-owned immutable location/hitbox/ID/context snapshot |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:80-100` | capture commit port | 区分 mapped spawn ID 与 discouragement type/AI style |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:179-184` | `RevengeEnemyContextQuery` | 保留 current `rectOuter` only 语义并显式记录 unused inner input |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:221-261` | context query + world/player adapter | 不把 Player 引用或 Main global state 写入 C11 |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:315-338` | NPC capture integration | P03/NPC owner 保留 capture qualification 和调用时序 |

#### 单一写入、兼容与回滚

- C11 writer 只写 marker-owned context snapshot；NPC owner 写 NPC state，Player/world adapters 提供只读 query input，C12 writer 负责 respawn result。
- 兼容窗口旧 `CacheEnemy` 为唯一运行时 capture writer，新 policy 只 shadow；禁止新旧路径重复添加 marker 或从已复用 slot 再次读取 context。
- 若 enemy box、outer-only intersection、mapped ID/type 分离、life ratio 或 statue flag 发生漂移，或 query 直接写 NPC/Player/World，则停止切换并回滚到旧 marker capture。

#### C11 验证计划

需要 focused verifier 覆盖 capture policy、immutable snapshot、geometry/intersection、ID mapping、discouragement query、P03 handoff 和 no side effects。当前没有 C# 实施、dotnet 命令或验证结果。

### 11.12 当前 checkpoint 状态

- C01、C02、C03、C04、C05、C06、C07、C08、C09、C10、C11 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11]`；`currentComponent: C12`；`pendingComponents` 为 C12-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

- C01、C02、C03、C04、C05、C06、C07 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03, C04, C05, C06, C07]`；`currentComponent: C08`；`pendingComponents` 为 C08-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

- C01、C02、C03、C04、C05、C06 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03, C04, C05, C06]`；`currentComponent: C07`；`pendingComponents` 为 C07-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

- C01、C02、C03、C04、C05 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03, C04, C05]`；`currentComponent: C06`；`pendingComponents` 为 C06-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

- C01、C02、C03、C04 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03, C04]`；`currentComponent: C05`；`pendingComponents` 为 C05-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

- C01、C02、C03 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03]`；`currentComponent: C04`；`pendingComponents` 为 C04-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

- C01、C02 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02]`；`currentComponent: C03`；`pendingComponents` 为 C03-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

### 11.13 C12 执行 checkpoint

#### 实施步骤

1. 以 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:44-46,58-79,186-219,358-425` 为只读源，核对 value capture、force-expire、attempt lock、player overlap、discouragement、spawn/remove 顺序和 side effects。
2. 对照 `D:\TRbackup\Version4\Terraria\NPC.cs:64529-64543,66509-66520`，记录 `noSpawnCycle -> CheckRespawns -> Spawner.SpawnNPC` 的 current ordering，并将 natural spawn competition 与 failure retry 标为 integration decision。
3. 审核现有 `D:\TRbackup\NLTX\src\DeathPenaltyAndRevenge\RevengeMarkerLifecycleComponent.cs` 与 `RevengeTargetSnapshotComponent.cs` 的字段重叠；裁决一个唯一 substrate 后，再定义 `RevengeMarkerValueComponent`、`RevengeRespawnAttemptComponent`、system 和 commit port，禁止平行双写。
4. 建立纯 respawn decision verifier，输入 C10 deadline/ID、C11 context、C13 active-player/registry query 和 NPC spawn capacity；输出 lock/expire/spawn/remove intents，不调用 `NPC.NewNPC`、`NetMessage`、UI 或 clock。
5. 在兼容窗口保留旧 `CoinLossRevengeSystem.CheckRespawns`/`SpawnEnemy` 为唯一 writer，新 system 只做 shadow decision；逐个通过 value payload、lock reset、discouragement 和 remove parity 后再切换 spawn commit。
6. 由单一 NPC spawn commit owner 组合 C11 context/C12 value，处理 HP floor、timeLeft、value/statue、special AI 和 network commands；显式验证同 tick natural spawn 与 revenge spawn 的 arbitration，不能由文件顺序隐式决定。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:44-46,80-100` | `src/DeathPenaltyAndRevenge/RevengeMarkerValueComponent.cs` | 保留 base/coin capture value；与现有 TargetSnapshot 重叠需裁决 |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:58-79` | `src/DeathPenaltyAndRevenge/RevengeRespawnAttemptComponent.cs` | force-expire 与 attempt lock 分离；与现有 Lifecycle 重叠需裁决 |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:358-425` | `src/DeathPenaltyAndRevenge/RevengeRespawnSystem.cs` | preserve player list/outer intersection/lock/discourage/spawn/remove order |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:186-219` | `IRevengeRespawnCommitPort` + NPC adapter | entity, HP, timeLeft, value, statue, network and UI effects remain external |
| `D:\TRbackup\Version4\Terraria\NPC.cs:64529-64543,66509-66520` | spawn scheduler contract | natural spawn and revenge spawn arbitration; noSpawnCycle gate preserved |
| `D:\TRbackup\NLTX\src\DeathPenaltyAndRevenge\RevengeMarkerLifecycleComponent.cs`; `RevengeTargetSnapshotComponent.cs` | integration review | no duplicate C10/C11/C12 writers or duplicated value/lifecycle fields |

#### 单一写入、兼容与回滚

- C12 owns only value and attempt/force state; C10 owns identity/deadline; C11 owns context; C13 owns registry/clock/player candidate projection; NPC spawn owner owns entity mutation.
- Compatibility mode keeps old `CheckRespawns` and `SpawnEnemy` as authority. Shadow decisions must never set old lock/force flags or call spawn, and a marker may not be consumed twice.
- Roll back if lock clears while still overlapping, lock remains after leaving all outer boxes, force-expire is retried/cleared, value payload changes, marker removal precedes confirmed spawn, natural spawn order changes, or spawn failure causes an unowned retry loop.

#### C12 验证计划

需要 focused verifier 覆盖 value/attempt state, respawn decision order, player overlap, discouragement, spawn payload, remove projection, failure semantics, natural spawn arbitration, existing component owner resolution and no direct side effects。当前没有 C# 实施、dotnet 命令或验证结果。

### 11.14 当前 checkpoint 状态

- C01、C02、C03、C04、C05、C06、C07、C08、C09、C10、C11、C12 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12]`；`currentComponent: C13`；`pendingComponents` 为 C13-C14。
- 历史计划快照：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；后续实际结果见 11.23。

### 11.15 C13 执行 checkpoint

#### 实施步骤

1. 以 `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:279-357,358-462` 为只读源，核对 cache policy、player box geometry、marker register/remove、clock update/reset、cleanup、join snapshot 和 lock/I/O 交互。
2. 对照 `D:\TRbackup\Version4\Terraria\Main.cs:11450-11455`、`NPC.cs:66509-66520`、`WorldGen.cs:6437` 和 `NetMessage.cs:2558`，固定 Update/Spawn/Reset/Join scheduler anchors；不要以文件顺序推导运行时顺序。
3. 审核 `D:\TRbackup\NLTX\src\DeathPenaltyAndRevenge\RevengeMarkerRegistryComponent.cs` 的 ID-only HashSet 与 legacy `_markers` 的关系；裁决一个 payload registry owner、一个 ID index owner 和一个 commit boundary，禁止 register/remove 双写。
4. 定义 `RevengeCachePolicy`、`RevengeClockState`、registry system/commit port 和 network projection；将 `DisplayCaching` 置于 presentation adapter，将 `_markersLock` 限制为兼容期短临界区。
5. 建立 focused verifier，覆盖 stable order、duplicate/remove、exactly-once clock、reset、player inner/outer snapshots、expired/invalid materialization、join snapshot after lock release、V4/V1456 packet mapping 和 lazy enumeration behavior；不执行 network/persistence I/O。
6. 在兼容窗口保留 legacy `CoinLossRevengeSystem` 为 authority，新 registry 只 shadow snapshots；先切换 read projections，再切换 register/remove/clock commit，最后才能删除 lock/facade。任何 persistence owner 未裁决时不落盘 marker。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:279-293` | `src/DeathPenaltyAndRevenge/RevengeCachePolicy.cs`; `RevengePlayerProximityPolicy.cs`; `RevengePresentationPolicy.cs` | minimum coin 和 inner/outer box policy；DisplayCaching 只作为 adapter input |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:295-313` | `src/DeathPenaltyAndRevenge/RevengeMarkerRegistryComponent.cs` | active marker collection 和 register owner；legacy lock 不进入 persisted component |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:340-357` | `src/DeathPenaltyAndRevenge/RevengeClockState.cs`/`RevengeRegistrySystem.cs` | reset/tick exactly-once 与 C10 expiration 同一 tick |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:428-447` | cleanup decision/projection | materialize IDs before removal；保留 current lazy-enumeration drift 证据 |
| `D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:450-462` | `RevengeNetworkProjection` | lock 内取 snapshot、lock 外发送；join I/O 与 authority 分离 |
| `D:\TRbackup\Version4\Terraria\Main.cs:11450-11455`; `NPC.cs:66509-66520`; `WorldGen.cs:6437`; `NetMessage.cs:2558` | scheduler/reset/join adapters | explicit order and lifecycle handoff |
| `D:\TRbackup\NLTX\src\DeathPenaltyAndRevenge\RevengeMarkerRegistryComponent.cs` | integration review | ID-only HashSet 不能与 legacy payload list 双写 |

#### 单一写入、兼容与回滚

- C13 writer owns active marker registry and simulation clock only; C10/C11/C12 own marker slices; network, UI, persistence and NPC systems consume immutable projections.
- Compatibility mode keeps legacy `_markers` and `_gameTime` as writer; proposed registry receives shadow snapshots and never sends packets or mutates legacy state.
- Roll back if marker order/ID changes, cleanup emits different removal IDs, clock advances twice, reset leaves marker/clock residue, join sends under lock, V4/V1456 packet shape is conflated, or persistence restores only IDs without payload slices.

#### C13 验证计划

需要 focused verifier 覆盖 registry lifecycle、clock/reset、cache policy、player box query、cleanup/projection、lock isolation、join snapshot、protocol version drift、persistence gap 和 existing owner resolution。当前没有 C# 实施、dotnet 命令或验证结果。

### 11.16 当前 checkpoint 状态

- C01、C02、C03、C04、C05、C06、C07、C08、C09、C10、C11、C12、C13 计划已写入并与设计文档同步保存。
- `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13]`；`currentComponent: C14`；`pendingComponents` 为 C14。
- 未执行任何 C# 实施、dotnet 命令、测试或网络/存档验证。

### 11.17 C14 执行 checkpoint

#### 实施步骤

1. 以 `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:15-90` 为只读源，核对 current/old list、cooldown、TileEntity scan、`HasPylonOfType`、reset、join sync 和 add/remove broadcast 顺序；以 `TeleportPylonInfo.cs:6-14` 记录 current equality stub 的 `version-drift`。
2. 对照 `D:\TRbackup\Version4\Terraria\Main.cs:3364,13119`、`WorldGen.cs:6565`、`MessageBuffer.cs:594`，建立 explicit initialize/update/reset/join scheduler contract；不得通过 `Main.PylonSystem` 全局静态引用隐藏 owner。
3. 审核 `D:\TRbackup\NLTX\src\WorldStorage\PylonRegistryComponent.cs`、`PylonRegistryState.cs`、`PylonRegistryEntry.cs`、`dome/src/Terraria.Dome.Simulation/Teleportation/PylonRegistryComponent.cs` 和 `src/WorldInteraction/Structures/PylonStructureComponent.cs`，裁决一个 domain registry writer，其余只作为 storage DTO、adapter、view 或兼容 facade。
4. 定义 `PylonRefreshPolicy`、registry/query/system、commit port、TileEntity adapter、projection 和 SceneMetrics adapter。核心 component 只收 immutable current/previous snapshot、revision 和 refresh state，不引用 TileEntity、SceneMetrics 或 network writer。
5. 建立 focused verifier，覆盖 cooldown/refresh、scan validity、stable equality/diff、atomic swap、revision、reset/restore、join snapshot、module 7 packet boundary、stale/duplicate projection、SceneMetrics non-authority 和 dust isolation；不执行 network/persistence I/O。
6. 在兼容窗口保留旧 `TeleportPylonsSystem` 为唯一 writer，新 registry 只 shadow scan/diff；通过 equality、diff order、HasPylonOfType 和 join projection parity 后，再切换 replace/reset owner。未裁决 WLD/Dome owner 前不删除 WorldStorage/Dome 类型或旧 facade。

#### 源/目标与依赖影响

| 源 | 目标 | 依赖影响 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:15-23` | `src/Teleportation/PylonRegistryComponent.cs`; `PylonRefreshPolicy.cs` | current/old/cooldown/constant 保持同一 refresh transaction；SceneMetrics 不进入 component |
| `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:25-43` | `src/Teleportation/PylonRegistrySystem.cs`; `PylonRegistryQuery.cs` | update cooldown 和 `HasPylonOfType` 只读 current snapshot |
| `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:45-72` | `TileEntityPylonAdapter` + `PylonRegistryProjection` | TileEntity scan、equality/diff、add/remove network side effects 外置 |
| `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:74-90` | `IPylonRegistryCommitPort` + join adapter | reset/join snapshot 与 network I/O 分离 |
| `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonInfo.cs:6-14` | entry identity/equality verifier | current `Equals` stub requires full-reference/owner decision |
| `D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetTeleportPylonModule.cs:7-25` | `PylonRegistryProjection` + protocol adapter | selector/X/Y/type payload one-way; no client write-back |
| `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETeleportationPylon.cs:14-18,138-158` | `Adapters/TileEntityPylonAdapter.cs` | pylon type qualification and tile frame reads stay outside core registry |
| `D:\TRbackup\NLTX\src\WorldStorage\PylonRegistry*`; Dome `Teleportation/PylonRegistry*` | integration review | WorldStorage/Dome/domain cannot be three active registry writers |

#### 单一写入、兼容与回滚

- C14 proposed writer owns only current/previous registry, refresh state and revision; TileEntity owner writes structure, WorldStorage persists DTO, network/presentation/SceneMetrics consume projections.
- Compatibility mode keeps legacy `TeleportPylonsSystem` as authority; proposed scan/diff is shadow-only and cannot call NetManager, mutate TileEntity, or update WorldStorage.
- Roll back if equality/diff semantics, cooldown, HasPylonOfType, reset/join order, revision, module payload, or owner mapping changes; no legacy or candidate registry is deleted until one-owner integration review and focused verifier pass.

#### C14 验证计划

需要 focused verifier 覆盖 pylon registry lifecycle、TileEntity scan, equality/diff, cooldown, query, reset/restore, join/network projection, module 7 compatibility, SceneMetrics/dust isolation and WorldStorage/Dome owner resolution。当前没有 C# 实施、dotnet 命令或验证结果。

### 11.18 实施 checkpoint：C10/C11 实际结果

- C10/C11 的实际修改文件已保存：
  `src/DeathPenaltyAndRevenge/RevengeExpirationPolicy.cs`、
  `src/DeathPenaltyAndRevenge/RevengeMarkerIdentityProjection.cs`、
  `src/DeathPenaltyAndRevenge/RevengeProximityBox.cs`、
  `src/DeathPenaltyAndRevenge/RevengeEnemyContextPolicy.cs`、
  `src/DeathPenaltyAndRevenge/RevengeContextQuery.cs`、
  `src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs`。
- RED 契约检查（在新增 C11 类型前）按预期失败：`RevengeEnemyContextPolicy is not
  implemented.`；该检查证明验证目标尚不存在，未被误记为通过。
- 记录的首次 wrapper 调用因 PowerShell 参数歧义退出码 `1`，没有启动 dotnet，不是构建
  结果；之后改用 `-DotnetArguments` 数组正确执行。
- 串行构建命令：

  ```powershell
  $dotnetArguments = @('build', '.\\src\\DeathPenaltyAndRevenge\\Terraria.DeathPenaltyAndRevenge.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
  & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
  ```

  项目为 `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`，退出码 `0`，
  `0` warning，`0` error；产物为
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- 当前 `completedComponents: []`；`partialComponents: [C10, C11]`；`currentComponent: C11`；
  `pendingComponents` 保留 C10-C14 和 C01-C09。C10 allocator/lifecycle integration、C11
  capture/discouragement integration、C12-C14 仍未完成。
- `verificationStatus: not-verified`：只有编译证据，没有 focused verifier、网络、存档或行为
  等价证据。

### 11.20 实施 checkpoint：C12 实际结果

- C12 的实际修改文件已保存：
  `src/DeathPenaltyAndRevenge/RevengeMarkerValueComponent.cs`、
  `src/DeathPenaltyAndRevenge/RevengeRespawnAttemptComponent.cs`、
  `src/DeathPenaltyAndRevenge/RevengeRespawnDecisionKind.cs`、
  `src/DeathPenaltyAndRevenge/RevengeRespawnDecision.cs`、
  `src/DeathPenaltyAndRevenge/RevengeRespawnSystem.cs`、
  `src/DeathPenaltyAndRevenge/RevengeMarkerLifecycleComponent.cs`、
  `src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs`。
- C12 RED 契约按预期失败：三个新类型均缺失；失败原因是目标行为尚不存在，不是测试拼写
  错误。
- 当前没有 C12 的 GREEN 构建结果；下一步必须先检查活动 `dotnet.exe/csc.exe`，再通过
  `Invoke-SerialDotnet.ps1 -DotnetArguments` 串行构建同一受影响项目。
- C12 的副作用边界：decision 不调用 NPC、网络、UI、持久化或时钟；spawn rejected/unknown
  的 retry 语义仍未实现，避免在没有 NPC owner 证据时猜测。
- 当前 `completedComponents: []`；`partialComponents: [C10, C11, C12]`；
  `currentComponent: C12`；`pendingComponents` 保留 C10-C14 和 C01-C09。
- `verificationStatus: not-verified`：只有 C10/C11 先前的项目构建证据；C12 focused verifier、
  C12 构建、网络、存档和行为等价仍未运行。

### 11.21 实施 checkpoint：C10/C13/C14 实际结果

- C10 新增：`src/DeathPenaltyAndRevenge/RevengeMarkerIdAllocator.cs`。
- C13 新增：`src/DeathPenaltyAndRevenge/RevengeCachePolicy.cs`、
  `src/DeathPenaltyAndRevenge/RevengePlayerProximityPolicy.cs`。
- C14 新增：`src/Teleportation/PylonRefreshPolicy.cs`。
- C11 修改：`src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs`，增加显式
  spawn net ID/discouragement type capture path 和只读投影；没有改变旧构造函数签名。
- C10 allocator 的重复恢复 ID、越界和 exhaustion 行为已在代码中表达，但尚未通过 focused
  verifier，且没有接入 legacy counter；C13/C14 policy 同样未接入 authority。
- 当前 `completedComponents: []`；`partialComponents: [C10, C11, C12, C13, C14]`；
  `currentComponent: C14`；`pendingComponents` 保留 C10-C14 和 C01-C09。
- 本 checkpoint 保存前未开始下一个组件；`verificationStatus: not-verified`。

### 11.22 实施 checkpoint：C11 构造路径修复

- 真实串行构建首先发现 `RevengeTargetSnapshotComponent` 的兼容构造函数与分离 capture
  构造路径发生 `CS0111` 重载冲突；失败命令退出码 `1`，`0` warning、`1` error，未形成
  可采信的 P01 新 artifact。
- 已保存的最小修复文件为
  `src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs`：公共八参数构造函数保持
  不变，私有构造路径加入 `snapshotNpcTypeId`，`Capture` 继续保持 spawn ID 与
  discouragement type 的显式分离。该修复未改变 NPC、网络、持久化或 registry owner。
- 该 checkpoint 记录于修复后验证之前；后续 11.23 已记录 P01 项目构建和纯策略 focused verifier，
  行为等价、网络和存档验证仍未运行；
- 第二次串行构建发现公共构造函数链多传一个 `NpcTypeId`，产生 `CS1729`；退出码 `1`，
  `0` warning、`1` error。已移除该多余实参，未改变公共构造签名或状态 owner。
- 该 checkpoint 记录于修复后验证之前；后续 11.23 已记录 P01 项目构建和纯策略 focused verifier，
  行为等价、网络和存档验证仍未运行；
  当前 `completedComponents: []`，`partialComponents: [C10, C11, C12, C13, C14]`，
  `currentComponent: C14`，`verificationStatus: not-verified`。

### 11.19 最终执行 checkpoint 状态（旧设计基线）

- C01-C14 的设计计划已写入并与设计文档同步保存；实施阶段新增的 C10/C11 checkpoint
  见 11.18，不得使用本旧基线描述覆盖实际实现状态。
- `completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；`currentComponent: none`；`pendingComponents: []`。
- 历史设计基线：当时未执行 C# 实施、dotnet 命令、测试或网络/存档验证；所有 proposed
  文件路径在该 checkpoint 时仍是未创建的目标草案。该历史记录不覆盖后续 11.18、11.20、
  11.21 和 11.22 的实际实施状态。

### 11.23 当前有效执行 checkpoint：C10-C14 最小单元验证

- 当前状态：`executionStatus: in-progress`、`implementationStatus: partial`、
  `verificationStatus: focused-c10-c14-passed`；`completedComponents: []`，
  `partialComponents: [C10, C11, C12, C13, C14]`，`currentComponent: C14`，
  `pendingComponents: [C10, C11, C12, C13, C14, C01, C02, C03, C04, C05, C06, C07, C08, C09]`。
- 实际修改的 `src/` 文件为：C10 的 `RevengeExpirationPolicy.cs`、
  `RevengeMarkerIdentityProjection.cs`、`RevengeMarkerIdAllocator.cs`；C11 的
  `RevengeProximityBox.cs`、`RevengeEnemyContextPolicy.cs`、`RevengeContextQuery.cs` 和
  `RevengeTargetSnapshotComponent.cs`；C12 的 `RevengeMarkerValueComponent.cs`、
  `RevengeRespawnAttemptComponent.cs`、`RevengeRespawnDecisionKind.cs`、
  `RevengeRespawnDecision.cs`、`RevengeRespawnSystem.cs`、`RevengeMarkerLifecycleComponent.cs`；
  C13 的 `RevengeCachePolicy.cs`、`RevengePlayerProximityPolicy.cs`；C14 的
  `Teleportation/PylonRefreshPolicy.cs`。
- 串行构建命令均通过 `Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 执行：
  `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj` 退出码 `0`、
  `0` warning、`0` error，artifact 为
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`；
  `src/Teleportation/Terraria.Teleportation.csproj` 退出码 `0`、`0` error，依赖
  `WorldStorage` 报告 `15` 个既有 `CS0649`/`CS0169` warning，artifact 为
  `Build/bin/Terraria.Teleportation/Debug/net10.0/Terraria.Teleportation.dll`。
- 不写入仓库的 PowerShell 反射 focused verifier 加载最新 `Build/bin` DLL，验证 C10-C14 的
  45 项纯/只读断言，输出 `FOCUSED_VERIFIER=PASS`、`ASSERTIONS=45`，退出码 `0`。
- 未完成：C10 legacy allocator/lifecycle 接入、C11 NPC capture qualification/discouragement
  分支、C12 NPC spawn commit/arbitration/retry/remove projection、C13 registry/clock/network/
  persistence owner、C14 pylon registry/TileEntity/equality-diff/WorldStorage/Dome owner，
  以及 C01-C09。没有这些 owner 和行为证据，不得把 partial 提升为 completed。
- runner 不结算：原始 runner session 已为 `completed`，本次实现没有新的 sessionId；不得重复
  `Complete`、`Retry`、`Claim` 或手工修改 ledger。

### 11.24 当前执行 checkpoint：C11 capture qualification Query

- 已保存源码：`src/DeathPenaltyAndRevenge/RevengeEnemyCaptureBounds.cs`、
  `RevengeEnemyCaptureInput.cs`、`RevengeEnemyCaptureRejectionReason.cs`、
  `RevengeEnemyCaptureResult.cs`、`RevengeEnemyContextPolicy.cs`。
- 实际行为：纯 `EvaluateCapture` 按 Version4 `CacheEnemy` 的顺序返回资格结果；
  `Position` 明确表示 NPC 左上角，`Width/Height` 用于边界检查，`SpawnNpcNetId` 明确要求
  NPC owner 先完成 `RespawnEnemyID` 映射。拒绝原因可审计，Query 没有任何状态写入或外部 I/O。
- 依赖影响：只依赖 `WorldPosition`、`NpcNetId` 和已有 `RevengeCachePolicy`；旧
  `CoinLossRevengeSystem` 仍是唯一 capture writer，未修改 NPC、网络、存档或 registry。
- 未验证项：本单元尚未执行 focused verifier/build；C11 的 discouragement 分支、NPC owner
  适配和 capture commit 仍未实现。保存此 checkpoint 后才允许开始下一最小单元。

### 11.25 当前执行 checkpoint：C11 capture qualification build 与 focused verifier

- 首次 build 失败：wrapper 命令退出码 `1`，`RevengeEnemyContextPolicy.cs(15,1)` 报 `CS1529`；
  根因是同一类型被重复追加，已删除重复定义后重跑。该次结果不作为成功验证。
- 修复后真实命令为：
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments
  @('build','.\src\DeathPenaltyAndRevenge\Terraria.DeathPenaltyAndRevenge.csproj','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')`；退出码 `0`，warning `0`，error `0`；artifact 为
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- focused verifier 不写入仓库，最终输出 `C11_CAPTURE_VERIFIER=PASS`、`ASSERTIONS=18`、
  exit code `0`。验证覆盖有效捕获、boss、非 root realLife、正 rarity、金币不足、世界边界、
  零 spawn ID、边界包含和缓存阈值；首次调用错误是 PowerShell `[ref]` 绑定问题，修正后未再失败。
- 当前真实状态：`completedComponents: []`；`partialComponents: [C10, C11, C12, C13, C14]`；
  `currentComponent: C11`；`pendingComponents: [C10, C11, C12, C13, C14, C01, C02, C03, C04, C05, C06, C07, C08, C09]`。
  C11 discouragement、NPC owner 适配和 capture commit 仍未实现；保存本 checkpoint 后才允许开始下一最小单元。

### 11.26 当前执行 checkpoint：C11 discouragement outer Query

- 已保存源码：`src/DeathPenaltyAndRevenge/RevengeDiscouragementInput.cs`、
  `src/DeathPenaltyAndRevenge/RevengeContextQuery.cs`。
- 实际行为：`IsNpcDiscouraged` 是纯 Query；style 2/3 通过显式 NPC adapter 结果保持外层
  语义，style 6 复现 type 513 的地下沙漠条件和 type 10/39/95/117/510 的表面判断，
  default 复现 spawn ID 253/490 的 eclipse/daytime 条件。
- 依赖影响：无 NPC、Player、Main、网络、时钟或持久化访问；没有新增长期状态、registry 或
  第二个 writer。AI style 2/3 的完整算法和 capture commit 仍属于 NPC owner 缺口。
- build 和 focused verifier 已在后续 11.27 checkpoint 完成；保存本 checkpoint 后才允许开始下一最小单元。

### 11.27 当前执行验证 checkpoint：C11 discouragement outer Query

- 通过仓库 wrapper 串行构建 `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`：
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments
  @('build','./src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')`；退出码 `0`，`0` warning、`0` error。
- 产物已确认位于
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的 focused verifier 加载最新 DLL，输出
  `C11_DISCOURAGEMENT_VERIFIER=PASS`、`ASSERTIONS=20`，退出码 `0`。覆盖 style 2/3、style 6
  的 type 513 地下沙漠分支、type 10/39/95/117/510 表面分支、非白名单类型、表面边界、
  default spawn net ID 253/490/其他 ID，以及非有限世界表面值异常。
- 首次 verifier 调用因 PowerShell 将 `[double]::NaN` 当作文本参数而失败，未触及业务断言；
  改用变量传入非有限值后通过。该调用错误不作为业务验证结果。
- 当前状态保持 `completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`、
  `currentComponent: C11`；`pendingComponents: [C10, C11, C12, C13, C14, C01, C02, C03, C04, C05, C06, C07, C08, C09]`。
  C11 的 NPC owner 适配和 capture commit、C12-C14 owner integration 以及 C01-C09 仍未完成；
  没有新的 runner sessionId，因此不执行 runner 结算。

### 11.28 当前执行回归 checkpoint：C10-C14 已实现纯单元

- 在同一新生成的 `DeathPenaltyAndRevenge` DLL 上运行不写仓库的 baseline focused verifier，
  输出 `C10_C14_BASELINE_VERIFIER=PASS`、`ASSERTIONS=29`，退出码 `0`。
- 验证内容包括：C10 expiration 分段/绝对 deadline、allocator 默认分配/恢复 ID/重复/未分配/
  耗尽；C12 spawn/unlock/force-expire decision 与 attempt state；C13 cache threshold、
  inner/outer proximity geometry 和 outer-only intersection；C14 refresh/cooldown/负值拒绝。
- verifier harness 的初始失败均为测试脚本参数表达式问题，没有改变 C# 源码：gold threshold
  期望修正为当前 Version4 分段插值的 `234000`，C# 异常改为检查 PowerShell 包装异常的
  inner exception，`int.MaxValue` 改为局部变量传入。最终只采用修正后的 `PASS` 结果。
- 当前状态保持 `completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`、
  `currentComponent: C11`、`pendingComponents: [C10, C11, C12, C13, C14, C01, C02, C03, C04, C05, C06, C07, C08, C09]`。
  该回归不证明 runtime writer、registry、NPC spawn、network 或 persistence integration；
  没有新的 runner sessionId，因此不执行 runner 结算。

### 11.29 当前执行 checkpoint：C13 presentation policy

- 实际新增源码：`src/DeathPenaltyAndRevenge/RevengePresentationPolicy.cs`。
- 该 value type 将 Version4 `DisplayCaching` 转为显式 immutable
  `DebugDisplayEnabled` 输入；不拥有 marker、registry、clock、NPC、网络或 UI I/O，旧
  `CoinLossRevengeSystem.DisplayCaching` 仍保持唯一运行时 writer。
- 保存源码和本 checkpoint 后已完成该最小单元的串行 build/verifier；当时
  `verificationStatus` 仍为 `not-verified`，`completedComponents: []`、
  `partialComponents: [C10, C11, C12, C13, C14]`，`currentComponent` 暂为 C13。

### 11.30 当前执行验证 checkpoint：C13 presentation policy

- 通过仓库 wrapper 串行构建 `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`：
  退出码 `0`，`0` warning、`0` error；产物为
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的 focused verifier 输出 `C13_PRESENTATION_VERIFIER=PASS`、`ASSERTIONS=2`、
  退出码 `0`，覆盖 `DebugDisplayEnabled=false` 和 `true` 的显式 presentation 输入。
- 当前状态保持 `completedComponents: []`、
  `partialComponents: [C10, C11, C12, C13, C14]`、`currentComponent: C13`、
  `pendingComponents: [C10, C11, C12, C13, C14, C01, C02, C03, C04, C05, C06, C07, C08, C09]`。
  C13 registry/clock/network/persistence owner、C11 NPC integration、C12/C14 owner integration
  和 C01-C09 仍未完成；没有新的 runner sessionId，因此不执行 runner 结算。

### 11.31 当前执行 checkpoint：C13 explicit revenge clock state

- 实际新增源码：`src/DeathPenaltyAndRevenge/RevengeClockState.cs`。
- `RevengeClockState` 保存显式 `CurrentTick`，默认从 `0` 开始；`Advance()` 单次递增并返回
  新 tick，`Reset()` 归零。它没有 wall-clock、网络、存档、registry 或 legacy writer 副作用。
- 当前 `CoinLossRevengeSystem._gameTime` 仍由 legacy writer 所有；本 state 只是已确认语义的
  shadow substrate。C13 唯一 clock commit owner、restore/resume 和 scheduler integration
  仍未裁决，因此不能把 C13 标成 completed。
- 保存源码和本 checkpoint 后，`verificationStatus` 保持 `not-verified`，
  `completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`、
  `currentComponent: C13`、`pendingComponents` 保持原值；等待共享 checkout 空闲后串行 build/verifier。

### 11.32 当前执行验证 checkpoint：C13 explicit revenge clock state

- 通过仓库 wrapper 串行构建 `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`：
  退出码 `0`，`0` warning、`0` error；产物为
  `Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的 focused verifier 输出 `C13_CLOCK_VERIFIER=PASS`、`ASSERTIONS=5`、退出码 `0`，
  覆盖默认 tick、连续推进、reset 和 unchecked overflow。
- 当前状态保持 `completedComponents: []`、`partialComponents: [C10, C11, C12, C13, C14]`、
  `currentComponent: C13`、`pendingComponents` 保持原值。C13 clock commit owner、scheduler、
  restore/resume、registry、网络和存档仍未完成；没有新的 runner sessionId，因此不结算 runner。

### 11.33 当前执行 checkpoint：C01 budget substrate implementation

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

### 11.34 当前执行验证 checkpoint：C01 budget/system commit

- C01 已实际保存的源码文件为：`src/WorldStorage/LiquidFlowBudgetPolicy.cs`、
  `LiquidFlowBudgetDecision.cs`、`LiquidFlowBudgetQuery.cs`、
  `LiquidFlowBudgetAndPanicStateComponent.cs`、`LiquidFlowTickInput.cs`、
  `LiquidFlowStateUpdate.cs`、`LiquidFlowCommitResult.cs`、`ILiquidFlowCommitPort.cs`、
  `LiquidFlowStateCommitPort.cs`、`LiquidFlowTickResult.cs` 和 `LiquidFlowSystem.cs`。
- 按 TDD 先执行 Version4 panic 边界 RED 检查：`45000` 缓冲、旧
  `panicCounter=3600`、`panicMode=false` 观测为 `false`，输出
  `C01_PANIC_BOUNDARY_RED=EXPECTED_FAIL observed=False expected=True`。修复后
  `LiquidFlowBudgetQuery.Evaluate` 使用递增后的 `nextPanicCounter`，保留 Version4 的
  “先递增、再判断 `> 3600`”顺序。
- 构建命令：
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments
  @('build','./src/WorldStorage/Terraria.WorldStorage.csproj','-m:1','-nr:false',
  '-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')`。
  退出码 `0`，`0` error，`15` 个既有 warning，产物路径
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`。
- focused verifier 为不写入仓库的 PowerShell reflection 检查，输出
  `C01_BUDGET_SYSTEM_VERIFIER=PASS`、`ASSERTIONS=30`、`C01_FOCUSED_EXIT_CODE=0`。
  覆盖预算/reduced 模式、容量、高水位、panic 第 3601 tick、quick-fall 边界、state
  构造/reset、system commit、panic entry、非法 commit rejection/no partial update。
- 实际结果只完成 C01 的 shadow substrate 验证；没有切换旧 `Liquid` writer，也没有闭合
  Tile/liquid owner、C02/C03 交接、网络/存档或世界生命周期。因此 `implementationStatus`
  保持 `partial`，`completedComponents` 保持 `[]`，C01 保持在 `partialComponents`。
- 保存本 checkpoint 后执行游标推进到 C02：`currentComponent: C02`；
  `pendingComponents: [C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；
  `verificationStatus: c01-budget-system-commit-focused-passed-30-assertions; c02-queue-seam-not-verified; focused-c10-c14-plus-c11-capture-discouragement-presentation-clock-passed`。

### 11.35 当前执行 checkpoint：C02 queue seam implementation

- 已实际保存 C02 源码：`src/WorldStorage/LiquidCellWorkItemStateCommand.cs`、
  `LiquidWorkItemReadiness.cs`、`LiquidWorkItemReadyQuery.cs`、
  `LiquidWorkQueueOperationResult.cs`、`LiquidWorkQueueDrainStatus.cs`、
  `LiquidWorkQueueDrainResult.cs`、`ILiquidWorkQueueCommitPort.cs`、
  `LiquidWorkQueueStateCommitPort.cs` 和 `LiquidWorkQueueSystem.cs`。
- 按执行计划，command 只表达 `x/y/kill/delay`；纯 Query 负责 bounds、kill threshold、ready
  和 delay decrement；state commit port 负责 active queue 的显式 enqueue/merge/requeue/
  remove/reset，system 负责通过 port 进行 drain。重复坐标返回幂等 duplicate，byte 表示无法
  承载的值返回 `byte-range` rejection，避免静默改变 Version4 字段。
- 当前源码尚未经过串行 build 或 focused verifier；旧 `Main.liquid`/`Liquid` methods、Tile
  checking bit、C03 buffer 和 C04 dirty publication 仍保持原 owner。未修改现有
  `LiquidWorkQueueState` 公共字段，未创建 `Main` 或网络/存档适配器。
- 保存本 checkpoint 后，`implementationStatus: partial`、`completedComponents: []`、
  `partialComponents: [C01, C02, C10, C11, C12, C13, C14]`、`currentComponent: C02`、
  `pendingComponents: [C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14]`；
  `verificationStatus: c01-budget-system-commit-focused-passed-30-assertions; c02-queue-seam-not-verified; focused-c10-c14-plus-c11-capture-discouragement-presentation-clock-passed`。

### 11.36 C02 queue seam verification checkpoint

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

### 11.37 C03 buffer queue verification checkpoint

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

### 11.58 C10 lifecycle deadline invariant correction checkpoint

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

### 11.59 C10 lifecycle deadline invariant verification checkpoint

- 按 BUILD-CONCURRENCY-1 确认无活动 `dotnet.exe`/`csc.exe` 后，通过仓库 wrapper 串行构建
  `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj`；退出码 `0`、`0` warning、
  `0` error，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`。
- 不写入仓库的 focused reflection verifier 输出
  `C10_LIFECYCLE_EXPIRATION_GREEN=PASS ASSERTIONS=13`，退出码 `0`；覆盖负 deadline 拒绝、零/最大
  deadline 接受、inclusive expiration、force-expire 覆盖和 attempt lock 设置/释放。
- 验证只覆盖组件级 deadline/lifecycle 契约；C10 allocator/legacy writer、C12 respawn、C13
  registry/clock、network、persistence 和跨域 owner 仍未闭合，因此 C10 不标记 `completed`。

### 11.56 C07 bounded component invariant correction checkpoint

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

### 11.57 C07 bounded component invariant verification checkpoint

- 通过仓库 wrapper 从根目录串行构建 `src/WorldInteraction/Terraria.WorldInteraction.csproj`：实际命令为
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\WorldInteraction\Terraria.WorldInteraction.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`；退出码 `0`，`0` warning，`0` error。
- 已确认产物存在于 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。
- 不写入仓库的 focused reflection verifier 输出 `C07_COMPONENT_INVARIANT_GREEN=PASS ASSERTIONS=22`，退出码 `0`；覆盖默认 `Count`/cooldown、`Count=0/1000`、`Count=-1/1001` 拒绝、三个 cooldown 的非负值和负值拒绝、`Int32.MaxValue`，以及 `Reset()` 恢复零值并清空条目。
- 验证只覆盖组件契约；旧机制数组 writer、`CheckMech`/`UpdateMech`、Tile frame、network、机制效果和最终 owner 仍是 evidence-gap，因此 C07 不标记 `completed`。

### 11.55 P01 组件契约审计 checkpoint

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

### 11.46 C06 final cleanup patch verification checkpoint

- 最终 cleanup-only 源码修订已保存并验证，实际修改的 C06 源文件为
  `src/WorldInteraction/Wiring/WiringTraversalAdapter.cs`；同一 C06 单元的源码集合还包括
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

### 11.41 C06 bounded scratch substrate implementation checkpoint

- C06 复用已由 C05 建立的 `src/WorldInteraction/Wiring/WirePropagationScratchComponent.cs`
  作为 C05/C06 唯一共享传播 scratch owner，没有创建重复的
  `WiringTeleportStateComponent`。新增的内部边界只负责记录最多两个非负 teleport tile
  endpoint、拒绝相同或超出容量的 endpoint，并提供清除 endpoint 与
  `BlockPlayerTeleportationForOneIteration` 的显式 reset。
- `src/WorldInteraction/Wiring/PumpTransferScratchComponent.cs` 现在提供内部
  `TryAddInputPump`/`TryAddOutputPump` 和 `Reset`：容量严格为 `MaxPumpCount = 20`，坐标必须
  非负，同一 input 或 output 列表内重复坐标被拒绝，reset 清空坐标数组和计数。只读列表仍
  不暴露可写数组，组件不拥有 `TileLiquidStateComponent`、Liquid amount/type 或任何实体位置。
- 实际修改文件仅为上述两个 `src/` 文件；没有写入 `PortalNetworkState`、
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

### 11.40 C05 wiring propagation implementation and focused verification checkpoint

- 已实际修改并保存的 C05 源码文件为：`src/WorldInteraction/Wiring/WirePropagationScratchComponent.cs`、
  `WiringPropagationCommandKind.cs`、`WiringPropagationCommand.cs`、
  `WiringPropagationQuery.cs`、`WiringPropagationSnapshot.cs`、
  `WiringPropagationCommitStatus.cs`、`WiringPropagationCommitResult.cs`、
  `IWiringPropagationCommitPort.cs`、`WiringPropagationCommitPort.cs` 和
  `WiringPropagationSystem.cs`。本 checkpoint 追加了 `QueueNextGate` command/commit path，
  并将 scratch 的 frontier、direction、current/next gate 与 lamp 读取改为防御性只读集合；
  C05 reset/initialize 的变更检测覆盖 `NextGates` 与 `TilesToProcess`。
- 核心行为：C05 只拥有一次 wiring propagation 的 skip/frontier/to-process、lamp、current/next
  gate、completed-gate 和 pixel trigger 状态；`WiringPropagationSystem` 先验证显式 world bounds
  和 command，再经 `IWiringPropagationCommitPort` 更新现有 scratch。wire color 只接受 `1..4`，
  `CurrentUser` 保留 Version4 `255` sentinel，重复 tile/gate/lamp/pixel 输入返回 `Duplicate`，
  end 清理 running/current color，C05 reset 不清理 C06 的 teleport block flag。
- 串行构建命令：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 构建
  `src/WorldInteraction/Terraria.WorldInteraction.csproj`，使用 `-m:1`、`-nr:false`、
  `-p:UseSharedCompilation=false`、`-p:MSBuildNodeReuse=false` 和
  `-p:BuildInParallel=false`；退出码 `0`，`0` warning，`0` error。产物已确认位于
  `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`。
- 不写入仓库的 PowerShell reflection focused verifier 输出
  `C05_WIRING_PROPAGATION_VERIFIER=PASS`、`ASSERTIONS=38`、`C05_FOCUSED_EXIT_CODE=0`。
  覆盖初始 sentinel、bounds、重复 begin、非法/四种 wire color、frontier/direction 对齐、
  current/next gate 与 lamp 去重、pixel trigger、稳定快照、end/reset 和 Query 边界。
- 依赖影响：只依赖 `WorldInteraction.Tiles.TileCoordinate` 和 BCL；没有写 Tile、sound、
  NetMessage、Player/NPC/item/projectile、网络、存档、时钟或 C06/C07 状态。未验证项仍包括
  旧 `Wiring.TripWire/HitWire/LogicGatePass/PixelBoxPass` 接管、完整 `CheckLogicGate` 行为、
  Tile frame/effect owner、C05/C06/C07 reset coordinator、旧 facade 的 parity 和跨域生命周期。
  因 owner 仍未闭合，C05 保持 `partial`，不加入 `completedComponents`；执行游标推进至
  `currentComponent: C06`，`pendingComponents: [C06, C07, C08, C09, C10, C11, C12, C13, C14]`，
  `lastCheckpointUtc: 2026-09-12T00:09:00.1605973Z`。

### 11.38 C04 publication seam implementation checkpoint

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

### 11.39 C04 publication seam verification checkpoint

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

### 11.37 C03 buffer queue verification checkpoint

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

### 11.60 当前执行 checkpoint：P01 组件边界复核与串行重建

- 本 checkpoint 使用既有人工绑定 `partitionId: P01`、`sessionId: 9891b103b31246b981d200e001e5c302`，没有重新调用 `Claim`、`ClaimNext`、`Retry`、`Cleanup`、`Complete` 或 `Fail`，也没有手动修改 runner ledger。
- 只读复核确认 C01-C07、C10-C14 的组件源码已保存；C08 的 `CollisionQueryScratch` 依赖调用期间的 Query 生命周期、缺少的 `TileContact`/conveyor `Point` 值对象以及 Physics/Spatial result owner 裁决，C09 的结果值对象和 Query 同样不属于本次允许新增的 ECS component。没有新增 `src/` 文件，也没有把 Query、占位值对象、第三个 collision owner、Tile/NPC/Player 行为或跨域提交写入组件。
- 验证前按 `BUILD-CONCURRENCY-1` 检查活动 `dotnet.exe`/`csc.exe`，结果为空。随后依次执行以下四个串行构建命令（均通过仓库 wrapper，未使用 raw `dotnet`）：
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src\\WorldStorage\\Terraria.WorldStorage.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src\\DeathPenaltyAndRevenge\\Terraria.DeathPenaltyAndRevenge.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src\\Teleportation\\Terraria.Teleportation.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src\\WorldInteraction\\Terraria.WorldInteraction.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
- 四个命令均退出码 `0`、`0` warning、`0` error，输出路径分别为：
  - `D:\TRbackup\NLTX\Build\bin\Terraria.WorldStorage\Debug\net10.0\Terraria.WorldStorage.dll`
  - `D:\TRbackup\NLTX\Build\bin\Terraria.DeathPenaltyAndRevenge\Debug\net10.0\Terraria.DeathPenaltyAndRevenge.dll`
  - `D:\TRbackup\NLTX\Build\bin\Terraria.Teleportation\Debug\net10.0\Terraria.Teleportation.dll`
  - `D:\TRbackup\NLTX\Build\bin\Terraria.WorldInteraction\Debug\net10.0\Terraria.WorldInteraction.dll`
- evidence-gap：本轮构建未验证旧 Liquid/Wiring writer、Tile/TileEntity、NPC capture/spawn、marker registry payload、Pylon registry owner、network、persistence、自然 spawn arbitration、C08/C09 owner 或跨域行为等价；focused component evidence 不能替代这些 integration review。
- blocking-decision：保持 `implementationStatus: partial`、`completedComponents: []`、`partialComponents: [C01, C02, C03, C04, C05, C06, C07, C10, C11, C12, C13, C14]`、`blockedComponents: [C08, C09]`、`currentComponent: C14` 和 `pendingComponents: [C10, C11, C12, C13, C14]`。没有新的组件级局部不变量需要安全修订，不推进执行游标，不调用 runner 结算。
- verificationStatus：`p01-component-boundary-audit-rebuild-4-projects-passed-0-warning-0-error`；顶部 metadata 的 `lastCheckpointUtc` 已更新为 `2026-09-12T13:19:57.8472235Z`。

### 2026-09-12 C13 registry duplicate-input invariant checkpoint

- 修改前的不写入仓库 RED reflection probe 输出为
  `C13_REGISTRY_DUPLICATE_RED=CONFIRMED ACCEPTED_DUPLICATE`，退出码 `1`：
  `RevengeMarkerRegistryComponent(IEnumerable<RevengeMarkerId>)` 在重复 marker ID 输入时曾由
  `HashSet.Add` 静默去重并成功构造。
- 本次实际修改文件仅为
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
- 本次实际修改文件仅为 `src/WorldStorage/PylonRegistryComponent.cs`。新增私有
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

### 2026-09-13 C06 泵 scratch 计数不变量实施 checkpoint

- 本 checkpoint 使用当前 runner 绑定的 `partitionId: P01` 和 `sessionId: e925fb5b47e04306b42135b5e195a463`；没有重新调用 `ClaimNext`、`Claim`、`Retry`、`Cleanup`，也没有手动修改 runner ledger。
- 实际保存组件源码 `src/WorldInteraction/Wiring/PumpTransferScratchComponent.cs`。组件的 `InputPumpCount` 与 `OutputPumpCount` 仍保持原有公开属性和程序集内写入 API，但通过私有 backing field 统一校验 `0..MaxPumpCount`；`TryAddPump` 额外拒绝负计数，保证固定 20 个槽位的索引安全。该单元没有改变泵命令生成、提交顺序或外部副作用边界。
- 本 checkpoint 只修改 ECS 组件字段存储和局部不变量；没有新增或修改 System、Query、Command、Adapter、Projection、Port、事件、测试、测试夹具、验证器、项目文件或配置，也没有写入其他分区源码。
- `completedComponents: []`；`partialComponents` 保持 `[C01, C02, C03, C04, C05, C06, C07, C10, C11, C12, C13, C14]`；`blockedComponents: [C08, C09]`；`currentComponent: C06`；`pendingComponents: [C10, C11, C12, C13, C14]`。C06 仍为 `partial`，因为真实 Liquid/Teleportation/entity commit owner、half-brick endpoint 语义、网络/存档和旧 writer 接管均未闭合。
- 按用户要求未编写或运行测试、focused verifier、构建或其他 compile-capable 验证；本次仅完成源码存在性和改动范围静态核对。`verificationStatus: c06-pump-scratch-count-invariant-not-verified-no-tests-requested`，不得据此宣称编译、运行时或行为等价已验证。
