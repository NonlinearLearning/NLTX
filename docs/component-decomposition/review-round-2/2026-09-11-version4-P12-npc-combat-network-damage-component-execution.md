# Version4 P12 NPC 身份、AI、战斗、网络、伤害与交互组件执行计划

partitionId: P12
sessionId: dec7d02830c14d7bbba328dc0317cef3
claimMode: manual
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P12-NPC-Combat-Network-Damage.md
designPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P12-npc-combat-network-damage-component-design.md
executionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P12-npc-combat-network-damage-component-execution.md
executionStatus: in-progress
implementationStatus: partial
verificationStatus: focused-build-and-existing-verifier-passed
evidenceStatus: partial
completedComponents: [C01.NpcPresentationStateComponent, C01.NpcInteractionStateComponent, C01.NpcCatchStateComponent, C02.NpcMovementHistoryComponent]
currentComponent: C02.NpcMovementHistoryComponent.complete
pendingComponents: [C01.NpcActivationState, C01.NpcReceptionPolicy, C01.NpcWorldPolicy, C02.NpcMovementMediumProfile, C02.NpcTeleportState, C02.NpcMovementPolicy, C02.NpcFrameSizingState, C03-C19]
lastCheckpointUtc: 2026-09-12T07:56:51.9252793Z
evidence-gap: C01 三个组件和 C02 movement history 已通过 Terraria.Npc 项目编译，现有 NPC/Town 字段 verifier 已通过但不覆盖这四个新组件；仍未闭合 active/reset、交互命令写者、捕捉/释放所有权、C02 介质速度/传送/物理策略/frame sizing、rarity/taxCollector/takenDamageMultiplier/freeCake 及其持久化、网络和行为等价语义。其余 checkpoint 仍为设计记录。
blocking-decision: 仅实施不依赖行为的组件状态。NPC 槽位与生命周期、type/netID、生命与伤害提交、Projectile/NPC 引用、town/progression 与 commerce、spawn/eligibility、loot、network/session replication、persistence/bestiary 的跨子系统 Owner 仍须由 integration-review 明确；不新增系统、查询、命令、适配器或投影。

## Implementation checkpoint C01

当前会话 `dec7d02830c14d7bbba328dc0317cef3` 已将以下纯状态单元写入 `src/Npc`：

| source file | state members | status | dependency impact |
|---|---|---|---|
| `src/Npc/NpcPresentationStateComponent.cs` | `IsBestiaryIconDummy`, `IsPortraitDummy`, `ForcePartyHatOn`, `NameOverIncrement`, `NameOverDistance`, `NameOver`, `AltTexture`, `TownNpcVariationIndex` | implemented; compiled; dedicated behavior verifier pending | renderer/network 只能读取；表现快照 owner、reset、网络字段仍未闭合 |
| `src/Npc/NpcInteractionStateComponent.cs` | copied `PlayerInteraction`, `LastInteraction` | implemented; compiled; dedicated behavior verifier pending | 只保存交互状态；command、player/session 作用域和 reset 仍未闭合 |
| `src/Npc/NpcCatchStateComponent.cs` | `CatchItem`, `ReleaseOwner` | implemented; compiled; dedicated behavior verifier pending | 只保存捕捉/释放数据；capture owner、slot reuse 和网络/持久化仍未闭合 |

未实现的 C01 成员没有被塞入这些组件：`active` 继续由现有 `NpcLifecycleComponent` 持有，
`NPC_TARGETS_START`、`rarity`、`taxCollector`、`takenDamageMultiplier`、`freeCake` 保持
integration-review 阻塞。没有新增行为方法、注册键、网络写入或持久化代码。

## Implementation checkpoint C02

当前会话 `dec7d02830c14d7bbba328dc0317cef3` 已将 C02 中可独立确定的 movement history 状态写入
`src/Npc/NpcMovementHistoryComponent.cs`：

| source file | state members | status | dependency impact |
|---|---|---|---|
| `src/Npc/NpcMovementHistoryComponent.cs` | `oldPos` -> `OldPositions`, `oldRot` -> `OldRotations` | implemented; compiled; dedicated behavior verifier pending | 默认容量为 10；构造输入防御性复制并以 `ReadOnlyMemory` 暴露；movement commit、trail cadence、teleport invalidation、despawn/reuse reset 仍由 integration-review 指定 |

该组件只保存有界历史状态，没有加入移动记录、传送、物理、渲染、网络或重置行为。C02 的
`waterMovementSpeed`、`lavaMovementSpeed`、`honeyMovementSpeed`、`shimmerMovementSpeed`、
`teleportStyle`、`teleportTime`、`gfxOffY`、`stepSpeed`、`gravity`、`teleporting`、`stairFall`
和 `setFrameSize` 仍分别等待 definition/movement/teleport/frame owner 闭合，不被历史组件重复拥有。

## 1. Plan contract

这是 P12 的原始设计执行计划；当前会话已在其基础上落地 C01 的三个状态组件和 C02 的
`NpcMovementHistoryComponent`，并通过受影响项目编译及现有 NPC/Town 组件字段 verifier；这些证据不代表网络、持久化或行为等价已经成立。
未落地边界的名称和路径仍是 proposed，且必须遵守设计文档的领域优先、唯一写者、显式副作用端口和跨子系统审查约束。

设计 checkpoint 的完成含义是边界、成员归属和依赖已经记录；implementation checkpoint 另行记录实际源码、未验证项和剩余阻塞。每个 checkpoint 保存前，不开始下一个 checkpoint。

执行顺序固定为：

C01 -> C02 -> C03 -> C04 -> C05 -> C06 -> C07 -> C08 -> C09 -> C10 -> C11 -> C12 -> C13 -> C14 -> C15 -> C16 -> C17 -> C18 -> C19

实际系统调度不由这个列表或文件顺序决定，必须采用设计文档的 scheduler graph。

## 2. 全局执行规则

- 每个 checkpoint 先确认成员映射、现有 NLTX 类型和跨域 Owner，再定义最小快照/Command/CommitPort。
- 先引入唯一 Owner，再切换一个读者；禁止新旧路径双写同一事实。
- 每个数组和引用记录长度、索引有效性、reset、despawn、disconnect 和重用语义。
- 任何网络或持久化边界先做 characterization；没有 reader/writer/format 证据就标记 blocked，不发明协议。
- 纯 Query 和 Projection 不写组件；Adapter 只能写外部端口，不能反向改变 authority。
- 失败、重试、时钟和副作用在端口上显式表达；Main.tick、全局网络发送和 UI 不从核心规则中隐式读取。
- 执行 compile-capable command 时，必须从仓库根目录通过 Build/Tools/Invoke-SerialDotnet.ps1，检查 dotnet.exe/csc.exe，使用串行参数和 UseSharedCompilation=false，之后用 --no-build --no-restore verifier，并记录 Build/bin artifact；本次已完成受影响项目编译和现有 verifier，专用新组件行为 verifier 仍缺失。

## 3. C01 初始 checkpoint（已保存；实现状态见上方）

### C01 NPC identity、interaction、presentation

- 候选类型：NpcActivationState、NpcInteractionState、NpcPresentationState、NpcCatchState、NpcReceptionPolicy、NpcWorldPolicy。
- 迁移顺序：先建立 immutable NPC identity/presentation snapshot；交互由 Command 输入；确认 lifecycle owner 后才接 active/reset 写入；takenDamageMultiplier 和 static policy 留在 integration-review。
- 依赖：spawn/reset -> identity；interaction command -> interaction owner；snapshot -> draw/network projection；combat policy -> damage eligibility。
- 回滚：若发现 active、playerInteraction、catch/release 或 name/presentation 的写者不止一个，保留旧 facade，不启用新 Owner，不改变网络或持久化。
- 验证：源码已写入并通过 `Terraria.Npc.csproj` 编译；现有 NPC/Town 组件字段 verifier 已通过，但不覆盖本 checkpoint 的三个新组件。运行时、网络、持久化和行为等价 verifier 仍未执行。

## 4. 提议的 checkpoint 顺序

| checkpoint | 边界 | 计划状态 | 关键输出 |
|---|---|---|---|
| C01 | identity/interaction/presentation | recorded | owner map、snapshot、交互命令 |
| C02 | movement/history | pending | movement commit、teleport/history snapshot |
| C03 | identity/status | pending | parent link、name、shimmer、buff capacity |
| C04 | AI/target/identity | pending | AI/target commit、definition ref、immunity review |
| C05 | combat/life policy | pending | combat policy、damage eligibility、scaling |
| C06 | health/collision/presentation | pending | health/collision commit、draw snapshot |
| C07 | portal/special behavior | pending | portal event、audio/cache adapters |
| C08 | buff slots/immunity | pending | buff owner、expiry/reset、packet mapping |
| C09 | elemental debuff projection | pending | pure status projection and invalidation |
| C10 | control/social effects | pending | control command and social projection |
| C11 | whip/special effects | pending | mark/effect event projection |
| C12 | regeneration/protection | pending | regen/protection commit and damage port |
| C13 | lifecycle/cross-domain refs | pending | slot registry、spawn protection、revenge adapter |
| C14 | network replication | pending | one-way replication adapter、budget snapshot |
| C15 | network sync cursor | pending | per-player stream cursor adapter |
| C16 | damage definition registry | pending | immutable catalog and boss lookup |
| C17 | damage runtime tracking | pending | tracker service、clock、expiry |
| C18 | damage credit projection | pending | pure credit query and localized projection |
| C19 | interaction/commerce | pending | command、shop registry、UI adapter |

## 5. 共享依赖和回滚策略

迁移前先在每个 checkpoint 写出 source reader/writer inventory、reset/creation/cleanup inventory 和 external boundary inventory。若其中任何一项无法从 Version4 证据闭合，checkpoint 仍可记录设计，但 implementationStatus 不得变更，且必须在 blocking-decision 写出待审 Owner。

回滚只恢复当前 checkpoint 新增的 facade、snapshot 或 adapter，不撤销其他会话的文档、ledger、锁、源代码或生成物。禁止使用 broad cleanup；本会话没有创建需要删除的临时文件。

## 6. 验证计划

验证按风险分层：

1. 文档层：19 个叶子边界总数为 189，成员无遗漏、无重复、来源行号可回到权威报告；状态字段一致。
2. 设计层：每个候选 Component 有唯一 Owner 和最小 seam；Projection/Query/Adapter 没有反向写 authority；依赖图没有隐式环。
3. focused verifier：按设计文档第 11 节为每个 checkpoint 建立 reset、状态转换、纯度、协议、持久化和重试测试。
4. 构建层：实现源码已发生后，运行受仓库约束的受影响项目 build/test；本执行 checkpoint 当前仍等待串行 build 验证。
5. 网络/持久化层：只有 packet reader/writer、WorldFile、connection reset 和 round-trip 证据闭合后才解除对应 block。

### Integration handoff

- 交给 integration-review 的决策包：唯一 Owner、写入端口、时钟、数组/generation/reset、跨分区引用、packet 23/28/53、WorldFile/bestiary/town-manager 和失败重试。
- 实施者入口：先读取设计文档第 6-9 节和第 11-12 节、P12 权威报告及 Version4 锚点；完成 reader/writer/creation/cleanup evidence ledger 后，按 C01-C19 单 checkpoint 引入 facade、snapshot、Command 或 Adapter。
- 任何未闭合的 network/persistence/behavior-equivalence 证据都保持 blocked；本执行计划不授权写入其他分区、其他会话文档、权威报告或 ledger。

## 7. 完成条件

P12 计划完成不等于实现完成。分区完成前必须满足：

- 两份文档保存了 C01-C19 的设计 checkpoint，并额外记录每个实际源码 checkpoint；
- completedComponents、currentComponent、pendingComponents 与 `src` 中真实实现和阻塞边界一致；
- executionStatus 为 in-progress，implementationStatus 按实际源码状态记录为 partial，verificationStatus 必须与最新真实验证记录同步；当前为 focused-build-and-existing-verifier-passed，而不是全分区完成；
- evidence-gap 和 blocking-decision 明确列出未闭合项，不把计划或未验证源码写成行为完成；
- 通过非编译的文档结构检查；
- 使用原始 partition P12 和当前会话 sessionId `dec7d02830c14d7bbba328dc0317cef3` 调用 Complete。

在上述条件满足前，不调用 Complete。若出现确实无法由当前会话解决的外部阻塞，使用原始 session 调用 Fail；明确放弃才调用 Abandon。

## 8. Checkpoint log

| checkpoint | 保存时间 UTC | 设计记录 | implementationStatus | verificationStatus |
|---|---|---|---|---|
| C01 | 2026-09-11T13:42:01.1139271Z | saved; identity/interaction/presentation remains proposed | not-started | not-run |
| C01-implementation | 2026-09-12T07:23:48.1035466Z | saved; three C01 state components implemented, unverified | partial | not-run |
| C02-implementation | 2026-09-12T07:28:18.4486428Z | saved; NpcMovementHistoryComponent implemented, remaining C02 boundaries blocked or pending | partial | not-run |
| C02 | 2026-09-11T13:52:19.6505228Z | saved; movement/teleport/history remains proposed | not-started | not-run |
| C03 | 2026-09-11T13:57:23.3893934Z | saved; parent/name/shimmer/capacity remains proposed | not-started | not-run |
| C04 | 2026-09-11T13:59:28.5268864Z | saved; AI/target/type/immunity remains proposed | not-started | not-run |
| C05 | 2026-09-11T14:00:56.3015832Z | saved; combat policy/scaling/reward remains proposed | not-started | not-run |
| C06 | 2026-09-11T14:02:31.6969159Z | saved; health/collision/presentation remains proposed | not-started | not-run |
| C07 | 2026-09-11T14:04:06.6841888Z | saved; portal/audio/special behavior remains proposed | not-started | not-run |
| C08 | 2026-09-11T14:05:21.8619782Z | saved; buff slots/immunity/visibility remains proposed | not-started | not-run |
| C09 | 2026-09-11T14:06:53.6991356Z | saved; elemental status projection remains proposed | not-started | not-run |
| C10 | 2026-09-11T14:08:09.0507439Z | saved; control/social effects remains proposed | not-started | not-run |
| C11 | 2026-09-11T14:08:55.9909308Z | saved; whip marks/special effects remains proposed | not-started | not-run |
| C12 | 2026-09-11T14:10:24.4020627Z | saved; regeneration/protection/electric effect remains proposed | not-started | not-run |
| C13 | 2026-09-11T14:13:29.8077784Z | saved; lifecycle/cross-domain references remains proposed | not-started | not-run |
| C14 | 2026-09-11T14:14:49.3491245Z | saved; network replication remains proposed | not-started | not-run |
| C15 | 2026-09-11T14:16:03.3123663Z | saved; network sync cursor remains proposed | not-started | not-run |
| C16 | 2026-09-11T14:18:32.3626541Z | saved; damage definition registry remains proposed | not-started | not-run |
| C17 | 2026-09-11T14:19:14.8402666Z | saved; runtime tracker/service remains proposed | not-started | not-run |
| C18 | 2026-09-11T14:19:58.7392300Z | saved; damage credit query/message projection remains proposed | not-started | not-run |
| C19 | 2026-09-11T14:20:50.9449142Z | saved; interaction/commerce remains proposed | not-started | not-run |

## 9. Checkpoint C02：NPC target、movement 和 history（部分源码已保存）

- 候选边界：`NpcMovementHistoryComponent` 已实施但未验证；NpcMovementMediumProfile、NpcTeleportState、NpcMovementPolicy 和 NpcFrameSizingState 仍为 status: proposed/blocked。
- 迁移步骤：固定 oldPos/oldRot 的默认 10 项边界、显式容量构造和 reset 语义；定义 movement/teleport snapshot 与 command；确认唯一 movement owner；切换一个 reader；最后审查 gfxOffY、stepSpeed、gravity 和 setFrameSize。
- 依赖：activation/reset -> movement initialization；world/physics facts -> movement commit；movement commit -> collision/frame projection；teleport event -> network/history invalidation。
- 回滚：若传送、历史数组、frame sizing 或物理默认值的 writer/清理顺序不闭合，保留 legacy movement facade，不启用新 owner，不改变 network/persistence。
- 验证：`NpcMovementHistoryComponent` 已保存并通过 `Terraria.Npc.csproj` 编译；现有 NPC/Town 组件字段 verifier 已通过，但不覆盖本组件的构造器边界。运行时、网络、持久化和行为等价 verifier 仍未执行；其余 C02 候选边界仍仅保留成员库存、源码行号和候选 seam 记录。

### C01-C02 verification record

- Build guard：每次 compile-capable invocation 前均检查 `dotnet.exe`/`csc.exe`；未发现活动或 owner 不明的编译进程后才启动下一次串行命令。
- Affected project build：`pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -- build '.\src\Npc\Terraria.Npc.csproj' '-m:1' '-nr:false' '-p:UseSharedCompilation=false' '-p:MSBuildNodeReuse=false' '-p:BuildInParallel=false'"`；exit code `0`，warnings `0`，errors `0`；artifact `D:\TRbackup\NLTX\Build\bin\Terraria.Npc\Debug\net10.0\Terraria.Npc.dll`。
- Existing verifier build：`$dotnetArguments = @('build', '.\Test\Terraria.Npc.Components.Verification\Terraria.Npc.Components.Verification.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments`；exit code `0`，warnings `0`，errors `0`；artifact `D:\TRbackup\NLTX\Build\bin\Terraria.Npc.Components.Verification\Debug\net10.0\Terraria.Npc.Components.Verification.dll`。
- Existing verifier run：`$dotnetArguments = @('run', '--project', '.\Test\Terraria.Npc.Components.Verification\Terraria.Npc.Components.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments`；exit code `0`，输出 `PASS: NPC and town component field composition`，artifact `D:\TRbackup\NLTX\Build\bin\Terraria.Npc.Components.Verification\Debug\net10.0\Terraria.Npc.Components.Verification.dll`。
- 以上 verifier 只覆盖既有 NPC/Town 组件字段组合，不覆盖本次四个新组件的构造器边界、reset、movement history 推进、网络或持久化行为；因此当前状态是 focused build/verifier evidence，不是全分区行为等价证明。

## 26. Checkpoint C19：NPC interaction、commerce 和 UI adapter（已保存）

- 候选类型：NpcInteractionCommand、ShopRegistrationRegistry、NpcInteractionQuery、InteractionAdapter，覆盖 11 个成员，status: proposed。
- 迁移步骤：冻结 registry 初始化/重复注册；定义 local player/talk NPC snapshot；迁移 pure queries；校验 OpenShop command；连接 UI/commerce adapter。
- 依赖：identity/definition/progression/quest facts -> query；query -> command validation；command -> commerce adapter；UI/commerce I/O 留在 adapter。
- 回滚：若 local player/talk NPC、shop index、quest/progression 或 registry ownership 不闭合，保留旧 interaction path，不改变 NPC authority。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

P12 的 19 个设计 checkpoint 已全部记录；这表示设计/计划文档完成。当前源码实现仍为 partial，受影响项目编译和既有 NPC/Town 组件字段 verifier 已通过，但新组件专用行为、网络和持久化验证仍未完成。

## 25. Checkpoint C18：NPC damage credit query 和 message projection（已保存）

- 候选类型：DamageCreditQuery、DamageCreditMessageProjection，覆盖 7 个成员，status: proposed。
- 迁移步骤：建立 immutable credit snapshot；确认排序/并列/kill-time；定义纯 Name/KillTime projection；连接 UI/command adapter；最后移除旧 mutable read path。
- 依赖：tracker -> credit query；catalog/localization -> message; death -> kill-time; interaction/UI 只读输出。
- 回滚：若 Damage setter、credit ordering、localization 或 expiry query 语义不闭合，保留旧 credit path 和消息实现。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 24. Checkpoint C17：NPC damage runtime tracking（已保存）

- 候选类型：DamageTrackerService registry、NpcDamageEncounterTracker、TrackerClockState、TrackerQuery，覆盖 12 个成员，status: proposed。
- 迁移步骤：记录 Start/Update/Stop/Reset/AddDamage/BossKilled；定义 clock/events；服务管理 active/recent；tracker 输出 immutable credit snapshot；连接 C18 query。
- 依赖：definition -> classification；committed hit/death -> tracker events；clock -> update/expiry；tracker snapshot -> credit query。
- 回滚：若 active/recent 生命周期、expiry、clock 或 credit writer 不闭合，保留 NPCDamageTracker 旧实现和 UI/loot 读路径。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 23. Checkpoint C16：NPC damage definition registry（已保存）

- 候选类型：NpcDamageDefinitionCatalog、BossTypeLookup，覆盖 NPCTypes、Name、CustomBossDefinitions、BossTypeForMob，status: proposed。
- 迁移步骤：盘点 content load/reload/reset writers；定义 catalog snapshot/version；验证去重、排序和无效映射；让 tracker 只读 catalog。
- 依赖：content definition -> catalog；type/netID/boss -> lookup；tracker -> definition query；credit message -> localization snapshot。
- 回滚：若 reload、localized name 或 mapping version 不闭合，保留旧 registry facade，不改变 tracker lifecycle。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 22. Checkpoint C15：NPC network sync cursor（已保存）

- 候选类型：PlayerNpcSyncCursorAdapter，覆盖 skippedSyncs 和 streamCounter，status: proposed。
- 迁移步骤：确认 cursor 的 create/update/skip/release；定义 snapshot；加入 connection/player/world reset；核对 C14 full/delta；最后切换 packet path。
- 依赖：replication budget/stream -> cursor；connection lifecycle -> reset；authority snapshot -> packet selection；不向 NPC authority 写回。
- 回滚：若 cursor scope、溢出、重连或 slot reuse 语义不闭合，保留旧 PlayerNetSyncState 和 packet adapter。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 21. Checkpoint C14：NPC network replication state（已保存）

- 候选类型：NpcReplicationIntentProjection、NpcReplicationBudgetAdapter、NpcReplicationStreamAdapter、NpcPerPlayerReplicationState，全部为 status: proposed。
- 迁移步骤：建立只读 replication snapshot；逐字段核对 packet 23 writer/reader；建模 budget 和 per-player state；协议 characterization 通过后连接 adapter。
- 依赖：authority snapshots -> replication projection；C15 cursor -> packet adapter；reader -> validated command/definition boundary；send 是唯一外部副作用。
- 回滚：若 packet full/delta、spam budget、stream 或 connection reset 语义不闭合，保留旧 network fields/packet path，不改变 authority。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 20. Checkpoint C13：NPC lifecycle 和 cross-domain references（已保存）

- 候选类型：NpcSlotIndexRegistry、NpcSpawnProtectionRegistry、NpcAudioCooldownState、RevengeServiceAdapter，全部为 status: proposed。
- 迁移步骤：绘制 slot create/clear/reuse；确认 Projectile index validity；定义 spawn protection clock/cleanup；接入 sound delay snapshot；最后包裹 Revenge service port。
- 依赖：activation -> slot registry；movement/death -> audio/cleanup；Projectile -> read-only index query；revenge -> external adapter。
- 回滚：若 slot reuse、stale Projectile index、protection timer 或 Revenge retry 不闭合，保留旧 arrays/service path，不改变 NPC authority。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 19. Checkpoint C12：NPC regeneration、protection 和 electric effect（已保存）

- 候选类型：NpcRegenerationState、NpcProtectionPolicy、NpcElectricEffectState，全部为 status: proposed。
- 迁移步骤：区分 expected loss 与 HealthDelta；盘点 protection writer/reader；定义 RegenerationTick、DamageEligibilitySnapshot、ElectricEffectCommand；连接 health owner。
- 依赖：combat/status -> regen/protection；clock/world -> regen tick；regen/protection -> health/damage eligibility；death/reset -> clear。
- 回滚：若 regen 与 health 存量、chaseable target query 或 protection writer 不闭合，保留旧 regen/protection facade，不改变 health/death。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 18. Checkpoint C11：NPC whip marks 和 special hit effects（已保存）

- 候选类型：NpcWhipMarkState、NpcSpecialHitEffectProjection，全部为 status: proposed。
- 迁移步骤：盘点标记 writer/duration/refresh/reset；定义 WhipMarkCommand 和 SpecialHitEffectSnapshot；切换一个 combat reader；按证据决定 Component 或 Projection。
- 依赖：combat eligibility -> mark；Buff/status -> special effect；health/death -> clear；network/audio/UI 只消费 snapshot/event。
- 回滚：若标记持续时间、death cleanup 或特效来源不闭合，保留旧 flag facade，不改变 damage tracker 或 client effect path。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 17. Checkpoint C10：NPC control and social effects（已保存）

- 候选类型：NpcControlEffectState、NpcSocialEffectProjection，全部为 status: proposed。
- 迁移步骤：盘点 flag writer/duration/packet/persistence；定义 ControlStatusCommand 和 SocialStatusSnapshot；切换 AI reader；再决定 confused 是否需要独立 authority component。
- 依赖：Buff/status -> control/social；confused -> AI/target；social snapshot -> combat/interaction/network/presentation。
- 回滚：若控制 flag 的 duration/clear 或社会 flag 的派生失效不闭合，保留 legacy flags 和读 facade，不改变 Buff owner。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 16. Checkpoint C09：NPC elemental debuff projection（已保存）

- 候选类型：NpcElementalStatusProjection，覆盖 17 个布尔成员，status: proposed；不自动创建 17 个独立 authority components。
- 迁移步骤：盘点每个 flag 的 writer/reader；建立纯 calculation；定义 BuffCommit invalidation；切换一个 reader；根据证据决定 Component 或 Projection。
- 依赖：BuffSnapshot -> elemental calculation -> combat/control/whip/regen consumers；network 只消费 snapshot。
- 回滚：若某 flag 是独立源事实或失效不完整，保留 legacy flag facade，禁止 projection 与旧路径双写。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 15. Checkpoint C08：NPC buff slots、immunity 和 visibility（已保存）

- 候选类型：NpcBuffSlots、NpcBuffImmunityPolicy、NpcBuffVisibilityPolicy，全部为 status: proposed。
- 迁移步骤：冻结 slot capacity/index；盘点 Apply/Clear/expire/reset 与 packet 53；建立 BuffCommand/BuffSnapshot；连接 packet adapter；再交给 C09-C11 派生投影。
- 依赖：maxBuffs -> slot initialization；definition -> immunity；BuffCommit -> elemental/control/whip projections；BuffSnapshot -> combat/presentation/network。
- 回滚：若 slot reorder、expiry、packet 53 或 reset writer 不闭合，保留旧 Buff arrays 和 packet facade，不启用新的 derived writers。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 14. Checkpoint C07：NPC portal、audio 和 special behavior（已保存）

- 候选类型：NpcAudioProfile、NpcPortalState、SpecialBehaviorRegistry、KingSlimePointCache，全部为 status: proposed。
- 迁移步骤：确认音频定义和调用点；建立 portal event/despawn command；盘点特殊行为 static writers；定义 KingSlime cache 容量、清理和失效。
- 依赖：lifecycle -> portal/despawn；death -> audio event；definition/world facts -> registry/cache；snapshots -> network/audio/render adapters。
- 回滚：若特殊行为 scope、cache invalidation 或 audio event ownership 不闭合，保留 legacy fields 和 adapters，不改变 NPC lifecycle 或 network protocol。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 13. Checkpoint C06：NPC health、collision 和 presentation（已保存）

- 候选类型：NpcHealth、NpcCollisionState、NpcFramePresentation、NpcKnockbackPolicy，全部为 status: proposed。
- 迁移步骤：建立 HealthSnapshot 和 HealthCommitPort；冻结 targetRect/collision geometry；建立 FramePresentationSnapshot；最后连接 renderer adapter。
- 依赖：combat eligibility -> health; movement/world facts -> collision; identity/AI/status -> presentation; snapshots -> network/loot/renderer。
- 回滚：若 life/death、collision writer 或 frame output 不闭合，保留 legacy health/collision/presentation facade，不改变 damage tracker 或 network protocol。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 12. Checkpoint C05：NPC combat policy、scaling 和 reward（已保存）

- 候选类型：NpcCombatStats、NpcDamageAcceptancePolicy、NpcDisposition、NpcDifficultyScaling、NpcRewardValue、CombatDefinitionCatalog，全部为 status: proposed。
- 迁移步骤：区分 baseline/current stats；建立 DamageEligibilityQuery 和 DamageResolution 输入；确认 projectile/trap/hostile/capture owner；接入 difficulty scaling；输出 reward snapshot。
- 依赖：AI/target/type -> eligibility；Buff/status -> policy；world difficulty -> scaling；health owner -> death；hit/death events -> tracker。
- 回滚：若 health、damage policy、friendly/boss 或 reward 的 writer 无法唯一化，保留旧 combat facade，不改变 death/loot/tracker。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。

## 10. Checkpoint C03：NPC identity、parent link 和 status（已保存）

- 候选类型：NpcParentLifeLink、NpcNameState、NpcShimmerVisualState、NpcBuffCapacityDefinition，全部为 status: proposed。
- 迁移步骤：闭合 realLife 的建立/读取/死亡/slot reuse 调用点；建立 name/shimmer snapshot；冻结 maxBuffs 与 C08 BuffSlots 的容量契约。
- 依赖：lifecycle/reset -> identity initialization；parent link -> health/death；name/shimmer -> presentation/network projection；capacity -> BuffOwner。
- 回滚：若 realLife 的跨实体清理、name 的格式或 shimmer 的失效语义不闭合，保留旧字段 facade，不改变 persistence/network。
- 验证：该边界尚未新增源码；保留成员库存和候选所有权记录，未执行编译、运行时、网络或持久化 verifier。

## 11. Checkpoint C04：NPC AI、target、type 和 per-entity immunity（已保存）

- 候选类型：NpcAiState、NpcTargetState、NpcTypeDefinitionRef、NpcPerEntityImmunity，全部为 status: proposed。
- 迁移步骤：冻结 ai/localAI 长度；列出 UpdateNPC、target、spawn/reset、network reader 的 writers；建立 AI/target commit；校验 type/netID definition；在 C05/C08 决定 immune owner。
- 依赖：definition -> type/netID/AI policy；target query -> AI intent；movement facts -> AI step；AI snapshot -> combat/collision/network。
- 回滚：若 AI 数组、target 清理、type/netID mismatch 或 immune writer 不闭合，保留旧字段和旧 packet facade，不改变 lifecycle 或 damage ownership。
- 验证：只完成成员库存、源码行号和候选 seam 记录；未执行实现、编译、运行时、网络或持久化 verifier。
## 27. Checkpoint state register

每次 checkpoint 保存时同步记录以下状态；本表是恢复用登记，不改变最终状态字段。历史设计行保留当时的 proposed 状态，最新 implementation 行反映当前源码和阻塞边界。

| checkpoint | saved UTC | completedComponents | currentComponent | pendingComponents | evidence-gap | blocking-decision | verificationStatus |
|---|---|---|---|---|---|---|---|
| C01 | 2026-09-11T13:42:01.1139271Z | C01 | C02 | C02-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C02 | 2026-09-11T13:52:19.6505228Z | C01,C02 | C03 | C03-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C03 | 2026-09-11T13:57:23.3893934Z | C01,C02,C03 | C04 | C04-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C04 | 2026-09-11T13:59:28.5268864Z | C01,C02,C03,C04 | C05 | C05-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C05 | 2026-09-11T14:00:56.3015832Z | C01,C02,C03,C04,C05 | C06 | C06-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C06 | 2026-09-11T14:02:31.6969159Z | C01,C02,C03,C04,C05,C06 | C07 | C07-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C07 | 2026-09-11T14:04:06.6841888Z | C01,C02,C03,C04,C05,C06,C07 | C08 | C08-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C08 | 2026-09-11T14:05:21.8619782Z | C01,C02,C03,C04,C05,C06,C07,C08 | C09 | C09-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C09 | 2026-09-11T14:06:53.6991356Z | C01,C02,C03,C04,C05,C06,C07,C08,C09 | C10 | C10-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C10 | 2026-09-11T14:08:09.0507439Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10 | C11 | C11-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C11 | 2026-09-11T14:08:55.9909308Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11 | C12 | C12-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C12 | 2026-09-11T14:10:24.4020627Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12 | C13 | C13-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C13 | 2026-09-11T14:13:29.8077784Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13 | C14 | C14-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C14 | 2026-09-11T14:14:49.3491245Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13,C14 | C15 | C15-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C15 | 2026-09-11T14:16:03.3123663Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13,C14,C15 | C16 | C16-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C16 | 2026-09-11T14:18:32.3626541Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13,C14,C15,C16 | C17 | C17-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C17 | 2026-09-11T14:19:14.8402666Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13,C14,C15,C16,C17 | C18 | C18-C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C18 | 2026-09-11T14:19:58.7392300Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13,C14,C15,C16,C17,C18 | C19 | C19 | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C19 | 2026-09-11T14:20:50.9449142Z | C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13,C14,C15,C16,C17,C18,C19 | complete | none | partial; readers/writers, lifecycle, persistence, network, scheduler and behavior-equivalence evidence remain incomplete | implementation-blocked; integration-review must assign cross-system owners and write ports | not-run |
| C02-implementation | 2026-09-12T07:28:18.4486428Z | C01.NpcPresentationStateComponent,C01.NpcInteractionStateComponent,C01.NpcCatchStateComponent,C02.NpcMovementHistoryComponent | C02.NpcMovementHistoryComponent.complete | C01.NpcActivationState,C01.NpcReceptionPolicy,C01.NpcWorldPolicy,C02.NpcMovementMediumProfile,C02.NpcTeleportState,C02.NpcMovementPolicy,C02.NpcFrameSizingState,C03-C19 | partial; implemented source exists but build/runtime/network/persistence evidence is not yet available; unresolved owners remain | implementation continues only for independently attributable component state; existing AI/health/target/replication owners are not duplicated | not-run |
| C02-verification | 2026-09-12T07:56:51.9252793Z | C01.NpcPresentationStateComponent,C01.NpcInteractionStateComponent,C01.NpcCatchStateComponent,C02.NpcMovementHistoryComponent | C02.NpcMovementHistoryComponent.complete | C01.NpcActivationState,C01.NpcReceptionPolicy,C01.NpcWorldPolicy,C02.NpcMovementMediumProfile,C02.NpcTeleportState,C02.NpcMovementPolicy,C02.NpcFrameSizingState,C03-C19 | Terraria.Npc build and existing NPC/Town component verifier passed; dedicated new-component behavior, runtime, network and persistence evidence remains unavailable; unresolved owners remain | implementation remains limited to independently attributable component state; existing AI/health/target/replication owners are not duplicated | focused-build-and-existing-verifier-passed |
