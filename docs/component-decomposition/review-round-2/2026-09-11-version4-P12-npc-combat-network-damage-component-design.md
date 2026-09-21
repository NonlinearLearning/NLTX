# Version4 P12 NPC 身份、AI、战斗、网络、伤害与交互组件拆分设计

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

未实现的 C01 成员没有被塞入这些组件：`active` 继续由现有 `NpcLifecycleComponent` 持有，
`NPC_TARGETS_START`、`rarity`、`taxCollector`、`takenDamageMultiplier`、`freeCake` 保持
integration-review 阻塞。没有新增行为方法、注册键、网络写入或持久化代码。

## 1. 范围和状态

本文件是 P12 的候选组件边界、所有权、接口和验证设计。它不是运行时迁移完成证明，也不宣称 API 兼容、网络闭合、持久化闭合或行为等价已经成立。尚未落地的提议类型在 integration-review 通过前均保持 status: proposed；已落地的源码 checkpoint 只代表组件状态已写入，仍需独立验证。

权威报告确认本分区属于正式父级 NpcAndTownSimulation，覆盖 19 个叶子子系统、173 个字段、16 个属性，共 189 个成员。以下 19 个 checkpoint 是报告的叶子边界和人工恢复边界；checkpoint 不是把同一源类机械地复制成 19 个巨型组件。每个 checkpoint 内仍按权威性、访问模式和副作用边界细分候选 Component、System、Query、Projection、Adapter 或 Registry。

本次仅修改 P12 两份文档和本会话实际落地的 NPC 组件源码；不修改 Version4、权威报告、其他会话文档、ledger、锁文件、测试或项目文件。

## 2. 设计不变量

- 领域优先：候选文件从 src/Npc、src/Combat、src/Content 等已有领域根开始；只有形成稳定独立边界、独立测试或足够规模时才增加子目录。
- 一份文件只放一个核心同名 PascalCase 公共类型；不创建 Shared/Components/Common/Misc 等兜底目录。
- 权威状态只能由一个 Owner System 通过显式 Command 或 CommitPort 写入；Query、Projection 和 Adapter 不回写权威状态。
- 计算、资格判断、伤害结算和网络打包分开；I/O、时钟、随机、日志、持久化、消息和 UI 通过端口或适配器隔离。
- 数组、静态注册表、跨实体引用和短生命周期 tracker 不能因为来源类靠近就自动变成公共可变组件。
- 文件顺序不表达运行时顺序；所有调度依赖必须在系统契约和 scheduler graph 中显式记录。
- 兼容迁移先保留旧公共名称、命名空间和调用边界，由 Adapter 保持旧读路径；新 Owner 通过 focused verifier 后才接管唯一写入。
- 本分区证据级别只有 source-inventory-confirmed 和 partial；tModLoader 公开 API 或 SS14 ECS 组织样例不替代 Version4 私有语义证据。

## 3. 权威证据登记

| 证据 | 用途 | 状态 |
|---|---|---|
| P12 权威成员报告 | 189 个成员的文件、类型、行号、声明和叶子归属 | source-inventory-confirmed |
| Version4/Terraria/NPC.cs | NPC 初始化、生命周期、AI、网络更新、交互、受击、死亡、Buff 调用点 | partial；已定位主锚点，尚未闭合全部读写 |
| Version4/Terraria/Main.cs | NPC 初始化、revenge、damage tracker 更新和主循环调度 | partial |
| Version4/Terraria/NetMessage.cs | NPC 网络 packet 23 等写入路径 | partial；需逐字段闭合 |
| Version4/Terraria/MessageBuffer.cs | NPC packet 23、hurt packet 28、buff packet 53 接收路径 | partial |
| Version4/Terraria.IO/WorldFile.cs | NPC 持久化、town manager 和 bestiary 相关读写边界 | partial；完整字段映射未闭合 |
| Version4/Terraria.GameContent/NPCDamageTracker.cs | definition、tracker 生命周期、玩家/世界 damage credit | partial；全局 registry 与短生命周期归属待确认 |
| Version4/Terraria.GameContent/NPCInteraction.cs 和 NPCInteractions.cs | 交互命令、商店注册和本地玩家/谈话 NPC 适配 | partial |
| tModLoader API v2026.07 NPC 文档 | public API 边界交叉参考 | reference-only |
| SS14 Shared/Server NPC、Damageable、MobState 示例 | Component/System/Query 的组织粒度参考 | reference-only |
| NLTX 当前 src/Npc、src/Combat、src/Content、src/Projectile、src/WorldStorage、src/Relationships | 现有骨架和潜在跨域交接点 | partial；不是行为等价证明 |

已检查的主要 Version4 锚点：

- NPC.ResetForNewNPC: NPC.cs:8102
- NPC.SetDefaults: NPC.cs:8133
- NPC.NewNPC: NPC.cs:67051
- NPC.UpdateNPC: NPC.cs:76711
- NPC.UpdateNetworkCode: NPC.cs:76920
- NPC.PlayerInteraction: NPC.cs:66291
- NPC.ApplyInteraction: NPC.cs:66385
- NPC.StrikeNPC: NPC.cs:67461
- NPC.checkDead: NPC.cs:64571
- NPC.NPCLoot: NPC.cs:65312
- Buff methods: NPC.cs:76367-76457、78178、78348
- Main NPC initialization: Main.cs:3452-3474
- Main revenge and damage-tracker update: Main.cs:11451、11472
- Main NPC update loop: Main.cs:11505-11524
- NPC network packet 23 writer: NetMessage.cs around 706
- NPC network packet 23 receiver: MessageBuffer.cs around 1195
- NPC hurt packet 28 receiver: MessageBuffer.cs around 1406
- NPC buff packet 53 receiver: MessageBuffer.cs around 2007
- NPC persistence load: WorldFile.cs around 3998
- town manager and bestiary persistence: WorldFile.cs:3476-3503

## 4. 叶子边界和候选模块

下表的 proposedBoundary 是设计候选，不是已实现类型。一个报告叶子边界可以产生多个小组件，也可以最终被确认是 Registry、Projection、Query 或 Adapter，而不是 Component。

| checkpoint | 权威报告叶子 | 成员 | 候选边界 | 类型分类 | 唯一 Owner / seam | 关键跨系统审查 |
|---|---|---:|---|---|---|---|
| C01 | NpcIdentityInteractionAndPresentationState | 18 | NpcActivationState、NpcInteractionState、NpcPresentationState、NpcCatchState、NpcReceptionPolicy、NpcWorldPolicy | proposed Component/Projection/Registry | lifecycle owner -> immutable snapshots；交互使用 Command | NPC 槽位、town identity、damage reception、客户端表现 |
| C02 | NpcTargetAndMovementHistoryState | 14 | NpcMovementMediumProfile、NpcTeleportState、NpcMovementHistory、NpcFrameSizingState | partial Component; `NpcMovementHistoryComponent` implemented, remaining boundaries proposed | movement owner -> MovementCommitPort | 物理、传送、动画 frame、历史缓存 |
| C03 | NpcIdentityAndStatusState | 4 | NpcParentLifeLink、NpcNameState、NpcShimmerVisualState、NpcBuffCapacityDefinition | proposed Component/Definition | spawn/reset owner；名字和 shimmer 通过快照 | realLife、持久化名字、Buff registry |
| C04 | NpcAiTargetAndIdentityState | 13 | NpcAiState、NpcTargetState、NpcTypeDefinitionRef、NpcPerEntityImmunity | proposed Component/Query | AI owner -> AI commit；type 由 definition/spawn owner | type/netID、target、immune、AI 顺序 |
| C05 | NpcCombatAndLifeState | 19 | NpcCombatStats、NpcDisposition、NpcDamageAcceptancePolicy、NpcRewardValue、NpcDifficultyScaling | proposed Component/Definition | combat owner -> DamageCommitPort | life owner、projectile reflection、friendly、scaling |
| C06 | NpcCollisionAndPresentationState | 17 | NpcHealth、NpcCollisionState、NpcFramePresentation | proposed Component/Projection | health/collision owners；draw 只读 snapshot | life/death、targetRect、renderer/physics |
| C07 | NpcPortalAndSpecialBehaviorState | 11 | NpcAudioProfile、NpcPortalState、SpecialBehaviorRegistry、KingSlimePointCache | proposed Component/Adapter/Registry | special behavior owner；音频/静态 cache 走 adapter | portal、sound、静态 boss state |
| C08 | NpcBuffSlotAndImmunityState | 4 | NpcBuffSlots、NpcBuffImmunityPolicy、NpcBuffVisibilityPolicy | proposed Component | BuffOwnerSystem -> BuffCommitPort | slot 生命周期、packet 53、reset/expiry |
| C09 | NpcElementalDebuffState | 17 | NpcElementalStatusProjection | proposed Projection/Query | Buff snapshot -> pure status projection | 布尔别名、失效与受击计算 |
| C10 | NpcControlAndSocialEffectState | 4 | NpcControlEffectState、NpcSocialEffectProjection | proposed Component/Projection | status/control owner -> command | control/social 状态来源 |
| C11 | NpcWhipAndSpecialEffectState | 9 | NpcWhipMarkState、NpcSpecialHitEffectProjection | proposed Component/Projection | combat effect owner -> effect event | 伤害事件、持续时间、客户端特效 |
| C12 | NpcRegenerationAndProtectionState | 8 | NpcRegenerationState、NpcProtectionPolicy、NpcElectricEffectState | proposed Component | regen/protection owner -> DamageAcceptancePort | health、hostile damage、ghost heal |
| C13 | NpcLifecycleAndCrossDomainRefs | 4 | NpcSlotIndexRegistry、NpcSpawnProtectionRegistry、NpcAudioCooldownState、RevengeServiceAdapter | proposed Registry/Component/Adapter | slot/lifecycle owner；跨域只读引用 | NPC/Projectile、revenge、sound |
| C14 | NpcNetworkReplicationState | 11 | NpcReplicationIntentProjection、NpcReplicationBudgetAdapter、NpcPerPlayerReplicationState | proposed Projection/Adapter | NetworkReplicationAdapter 单向输出 | packet 23、client ownership、net spam |
| C15 | NpcNetworkSyncState | 2 | PlayerNpcSyncCursorAdapter | proposed Adapter | per-connection sync owner | cursor reset、stream order |
| C16 | NpcDamageDefinitionRegistry | 4 | NpcDamageDefinitionCatalog、BossTypeLookup | proposed Definition/Registry/Query | immutable catalog owner | static mutation、content load |
| C17 | NpcDamageRuntimeTracking | 12 | NpcDamageEncounterTracker、DamageTrackerService、RecentTrackerRegistry | proposed Component/Service/Registry | DamageTrackerService 唯一写者 | tick、expiry、world/player credit |
| C18 | NpcDamageCreditProjection | 7 | DamageCreditQuery、DamageCreditMessageProjection | proposed Query/Projection | pure read snapshot -> localized output | mutable Damage、localization、kill timing |
| C19 | NpcInteractionAndCommerce | 11 | NpcInteractionCommand、ShopRegistrationRegistry、NpcInteractionQuery、InteractionAdapter | proposed Command/Registry/Query/Adapter | interaction owner；Main.LocalPlayer 仅 adapter 输入 | talk NPC、shop、quest/progression |

## 5. 领域优先文件组织候选

以下路径只代表未来迁移的候选位置；本会话不创建这些 C# 文件。

| 能力 | 候选路径 | 命名空间 | 说明 |
|---|---|---|---|
| NPC 权威状态 | src/Npc/Npc*Component.cs | Terraria.Npc | 小领域保持扁平；每文件一个核心公共类型 |
| NPC AI/移动 | src/Npc/Behavior/Npc*Component.cs、Npc*System.cs | Terraria.Npc.Behavior | 形成独立测试和调度边界后才增加目录 |
| NPC 战斗/生命 | src/Combat/Npc/Npc*Component.cs、Npc*System.cs | Terraria.Combat.Npc | 与 DamageCommitPort、DeathResolution 交接 |
| Buff/状态 | src/Npc/Status/Npc*Component.cs、Npc*Projection.cs | Terraria.Npc.Status | C08 权威 slot 与 C09-C11 派生状态分开 |
| 网络适配 | src/Npc/Adapters/NpcNetwork*Adapter.cs | Terraria.Npc.Adapters | packet/connection 边界不成为第二权威源 |
| 伤害追踪 | src/Combat/Damage/NpcDamage* | Terraria.Combat.Damage | service/registry/query 按生命周期区分 |
| 交互经济 | src/Npc/Interaction/NpcInteraction* | Terraria.Npc.Interaction | Command、Registry、Query、UI adapter 分离 |
| 持久化 | src/WorldStorage/Npc/Npc*Adapter.cs | Terraria.WorldStorage.Npc | 格式和字段所有权闭合后才创建 |

不得新增 Shared/Components、Common、Misc 或按文件名排序的运行时调度结构。现有 NLTX 骨架须先确认 namespace、公共 API 和唯一写入责任，不能因为名称相近就创建第二个 Owner。

## 6. 完整 189 成员映射

以下表格由 P12 权威报告逐成员提取；189 行均保留 checkpoint、叶子子系统、成员名、声明类型、字段/属性种类、源类、源文件行号和候选边界。该表保留原始 `proposed boundary` 设计映射；C01/C02 已实施源码的当前状态以文档顶部和 implementation checkpoint 为准，未覆盖的边界仍是 status: proposed。

| # | checkpoint | 权威叶子 | 成员 | C# 类型 | kind | 源类型 | 源文件:行 | proposed boundary |
|---:|---|---|---|---|---|---|---|---|
| 1 | C01 | NpcIdentityInteractionAndPresentationState | active | bool | field | Terraria.NPC | Terraria/NPC.cs:5897 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 2 | C01 | NpcIdentityInteractionAndPresentationState | NPC_TARGETS_START | int | field | Terraria.NPC | Terraria/NPC.cs:5899 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 3 | C01 | NpcIdentityInteractionAndPresentationState | IsABestiaryIconDummy | bool | field | Terraria.NPC | Terraria/NPC.cs:5901 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 4 | C01 | NpcIdentityInteractionAndPresentationState | IsAPortraitDummy | bool | field | Terraria.NPC | Terraria/NPC.cs:5903 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 5 | C01 | NpcIdentityInteractionAndPresentationState | ForcePartyHatOn | bool | field | Terraria.NPC | Terraria/NPC.cs:5905 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 6 | C01 | NpcIdentityInteractionAndPresentationState | nameOverIncrement | float | field | Terraria.NPC | Terraria/NPC.cs:5943 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 7 | C01 | NpcIdentityInteractionAndPresentationState | nameOverDistance | float | field | Terraria.NPC | Terraria/NPC.cs:5945 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 8 | C01 | NpcIdentityInteractionAndPresentationState | nameOver | float | field | Terraria.NPC | Terraria/NPC.cs:5947 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 9 | C01 | NpcIdentityInteractionAndPresentationState | altTexture | int | field | Terraria.NPC | Terraria/NPC.cs:5961 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 10 | C01 | NpcIdentityInteractionAndPresentationState | townNpcVariationIndex | int | field | Terraria.NPC | Terraria/NPC.cs:5963 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 11 | C01 | NpcIdentityInteractionAndPresentationState | catchItem | short | field | Terraria.NPC | Terraria/NPC.cs:5965 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 12 | C01 | NpcIdentityInteractionAndPresentationState | releaseOwner | short | field | Terraria.NPC | Terraria/NPC.cs:5967 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 13 | C01 | NpcIdentityInteractionAndPresentationState | rarity | int | field | Terraria.NPC | Terraria/NPC.cs:5969 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 14 | C01 | NpcIdentityInteractionAndPresentationState | taxCollector | bool | field | Terraria.NPC | Terraria/NPC.cs:5971 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 15 | C01 | NpcIdentityInteractionAndPresentationState | playerInteraction | bool[] | field | Terraria.NPC | Terraria/NPC.cs:5973 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 16 | C01 | NpcIdentityInteractionAndPresentationState | lastInteraction | int | field | Terraria.NPC | Terraria/NPC.cs:5975 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 17 | C01 | NpcIdentityInteractionAndPresentationState | takenDamageMultiplier | float | field | Terraria.NPC | Terraria/NPC.cs:5977 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 18 | C01 | NpcIdentityInteractionAndPresentationState | freeCake | bool | field | Terraria.NPC | Terraria/NPC.cs:5979 | NpcActivationState / NpcInteractionState / NpcPresentationState / NpcCatchState / policy review |
| 19 | C02 | NpcTargetAndMovementHistoryState | waterMovementSpeed | float | field | Terraria.NPC | Terraria/NPC.cs:5907 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 20 | C02 | NpcTargetAndMovementHistoryState | lavaMovementSpeed | float | field | Terraria.NPC | Terraria/NPC.cs:5909 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 21 | C02 | NpcTargetAndMovementHistoryState | honeyMovementSpeed | float | field | Terraria.NPC | Terraria/NPC.cs:5911 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 22 | C02 | NpcTargetAndMovementHistoryState | shimmerMovementSpeed | float | field | Terraria.NPC | Terraria/NPC.cs:5913 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 23 | C02 | NpcTargetAndMovementHistoryState | teleportStyle | int | field | Terraria.NPC | Terraria/NPC.cs:5929 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 24 | C02 | NpcTargetAndMovementHistoryState | teleportTime | float | field | Terraria.NPC | Terraria/NPC.cs:5931 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 25 | C02 | NpcTargetAndMovementHistoryState | gfxOffY | float | field | Terraria.NPC | Terraria/NPC.cs:5981 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 26 | C02 | NpcTargetAndMovementHistoryState | stepSpeed | float | field | Terraria.NPC | Terraria/NPC.cs:5983 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 27 | C02 | NpcTargetAndMovementHistoryState | gravity | float | field | Terraria.NPC | Terraria/NPC.cs:5985 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 28 | C02 | NpcTargetAndMovementHistoryState | teleporting | bool | field | Terraria.NPC | Terraria/NPC.cs:5987 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 29 | C02 | NpcTargetAndMovementHistoryState | stairFall | bool | field | Terraria.NPC | Terraria/NPC.cs:5989 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 30 | C02 | NpcTargetAndMovementHistoryState | oldPos | Vector2[] | field | Terraria.NPC | Terraria/NPC.cs:6001 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 31 | C02 | NpcTargetAndMovementHistoryState | oldRot | float[] | field | Terraria.NPC | Terraria/NPC.cs:6003 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 32 | C02 | NpcTargetAndMovementHistoryState | setFrameSize | bool | field | Terraria.NPC | Terraria/NPC.cs:6005 | NpcMovementMediumProfile / NpcTeleportState / NpcMovementHistory / NpcFrameSizingState |
| 33 | C03 | NpcIdentityAndStatusState | realLife | int | field | Terraria.NPC | Terraria/NPC.cs:6039 | NpcParentLifeLink / NpcNameState / NpcShimmerVisualState / NpcBuffCapacityDefinition |
| 34 | C03 | NpcIdentityAndStatusState | _givenName | string | field | Terraria.NPC | Terraria/NPC.cs:6041 | NpcParentLifeLink / NpcNameState / NpcShimmerVisualState / NpcBuffCapacityDefinition |
| 35 | C03 | NpcIdentityAndStatusState | shimmerTransparency | float | field | Terraria.NPC | Terraria/NPC.cs:6061 | NpcParentLifeLink / NpcNameState / NpcShimmerVisualState / NpcBuffCapacityDefinition |
| 36 | C03 | NpcIdentityAndStatusState | maxBuffs | int | field | Terraria.NPC | Terraria/NPC.cs:6065 | NpcParentLifeLink / NpcNameState / NpcShimmerVisualState / NpcBuffCapacityDefinition |
| 37 | C04 | NpcAiTargetAndIdentityState | immune | int[] | field | Terraria.NPC | Terraria/NPC.cs:6303 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 38 | C04 | NpcAiTargetAndIdentityState | directionY | int | field | Terraria.NPC | Terraria/NPC.cs:6305 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 39 | C04 | NpcAiTargetAndIdentityState | type | int | field | Terraria.NPC | Terraria/NPC.cs:6307 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 40 | C04 | NpcAiTargetAndIdentityState | ai | float[] | field | Terraria.NPC | Terraria/NPC.cs:6309 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 41 | C04 | NpcAiTargetAndIdentityState | localAI | float[] | field | Terraria.NPC | Terraria/NPC.cs:6311 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 42 | C04 | NpcAiTargetAndIdentityState | aiAction | int | field | Terraria.NPC | Terraria/NPC.cs:6313 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 43 | C04 | NpcAiTargetAndIdentityState | aiStyle | int | field | Terraria.NPC | Terraria/NPC.cs:6315 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 44 | C04 | NpcAiTargetAndIdentityState | justHit | bool | field | Terraria.NPC | Terraria/NPC.cs:6317 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 45 | C04 | NpcAiTargetAndIdentityState | timeLeft | int | field | Terraria.NPC | Terraria/NPC.cs:6319 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 46 | C04 | NpcAiTargetAndIdentityState | target | int | field | Terraria.NPC | Terraria/NPC.cs:6321 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 47 | C04 | NpcAiTargetAndIdentityState | oldDirectionY | int | field | Terraria.NPC | Terraria/NPC.cs:6361 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 48 | C04 | NpcAiTargetAndIdentityState | oldTarget | int | field | Terraria.NPC | Terraria/NPC.cs:6363 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 49 | C04 | NpcAiTargetAndIdentityState | netID | int | field | Terraria.NPC | Terraria/NPC.cs:6391 | NpcAiState / NpcTargetState / NpcTypeDefinitionRef / NpcPerEntityImmunity |
| 50 | C05 | NpcCombatAndLifeState | damage | int | field | Terraria.NPC | Terraria/NPC.cs:6323 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 51 | C05 | NpcCombatAndLifeState | defense | int | field | Terraria.NPC | Terraria/NPC.cs:6325 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 52 | C05 | NpcCombatAndLifeState | defDamage | int | field | Terraria.NPC | Terraria/NPC.cs:6327 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 53 | C05 | NpcCombatAndLifeState | defDefense | int | field | Terraria.NPC | Terraria/NPC.cs:6329 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 54 | C05 | NpcCombatAndLifeState | defLifeMax | int | field | Terraria.NPC | Terraria/NPC.cs:6331 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 55 | C05 | NpcCombatAndLifeState | coldDamage | bool | field | Terraria.NPC | Terraria/NPC.cs:6333 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 56 | C05 | NpcCombatAndLifeState | trapImmune | bool | field | Terraria.NPC | Terraria/NPC.cs:6335 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 57 | C05 | NpcCombatAndLifeState | boss | bool | field | Terraria.NPC | Terraria/NPC.cs:6375 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 58 | C05 | NpcCombatAndLifeState | lavaImmune | bool | field | Terraria.NPC | Terraria/NPC.cs:6381 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 59 | C05 | NpcCombatAndLifeState | value | float | field | Terraria.NPC | Terraria/NPC.cs:6383 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 60 | C05 | NpcCombatAndLifeState | extraValue | int | field | Terraria.NPC | Terraria/NPC.cs:6385 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 61 | C05 | NpcCombatAndLifeState | dontTakeDamage | bool | field | Terraria.NPC | Terraria/NPC.cs:6387 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 62 | C05 | NpcCombatAndLifeState | catchableNPCTempImmunityCounter | int | field | Terraria.NPC | Terraria/NPC.cs:6389 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 63 | C05 | NpcCombatAndLifeState | statsAreScaledForThisManyPlayers | int | field | Terraria.NPC | Terraria/NPC.cs:6393 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 64 | C05 | NpcCombatAndLifeState | difficulty | float | field | Terraria.NPC | Terraria/NPC.cs:6395 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 65 | C05 | NpcCombatAndLifeState | friendly | bool | field | Terraria.NPC | Terraria/NPC.cs:6423 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 66 | C05 | NpcCombatAndLifeState | friendlyRegen | int | field | Terraria.NPC | Terraria/NPC.cs:6431 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 67 | C05 | NpcCombatAndLifeState | reflectsProjectiles | bool | field | Terraria.NPC | Terraria/NPC.cs:6439 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 68 | C05 | NpcCombatAndLifeState | CommonMasterBossLifeReduction | double | field | Terraria.NPC | Terraria/NPC.cs:6447 | NpcCombatStats / NpcDamageAcceptancePolicy / NpcDisposition / NpcRewardValue / NpcDifficultyScaling |
| 69 | C06 | NpcCollisionAndPresentationState | life | int | field | Terraria.NPC | Terraria/NPC.cs:6341 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 70 | C06 | NpcCollisionAndPresentationState | lifeMax | int | field | Terraria.NPC | Terraria/NPC.cs:6343 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 71 | C06 | NpcCollisionAndPresentationState | targetRect | Rectangle | field | Terraria.NPC | Terraria/NPC.cs:6345 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 72 | C06 | NpcCollisionAndPresentationState | frameCounter | double | field | Terraria.NPC | Terraria/NPC.cs:6347 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 73 | C06 | NpcCollisionAndPresentationState | frame | Rectangle | field | Terraria.NPC | Terraria/NPC.cs:6349 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 74 | C06 | NpcCollisionAndPresentationState | color | Color | field | Terraria.NPC | Terraria/NPC.cs:6351 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 75 | C06 | NpcCollisionAndPresentationState | alpha | int | field | Terraria.NPC | Terraria/NPC.cs:6353 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 76 | C06 | NpcCollisionAndPresentationState | hide | bool | field | Terraria.NPC | Terraria/NPC.cs:6355 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 77 | C06 | NpcCollisionAndPresentationState | scale | float | field | Terraria.NPC | Terraria/NPC.cs:6357 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 78 | C06 | NpcCollisionAndPresentationState | knockBackResist | float | field | Terraria.NPC | Terraria/NPC.cs:6359 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 79 | C06 | NpcCollisionAndPresentationState | rotation | float | field | Terraria.NPC | Terraria/NPC.cs:6365 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 80 | C06 | NpcCollisionAndPresentationState | noGravity | bool | field | Terraria.NPC | Terraria/NPC.cs:6367 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 81 | C06 | NpcCollisionAndPresentationState | noTileCollide | bool | field | Terraria.NPC | Terraria/NPC.cs:6369 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 82 | C06 | NpcCollisionAndPresentationState | collideX | bool | field | Terraria.NPC | Terraria/NPC.cs:6371 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 83 | C06 | NpcCollisionAndPresentationState | collideY | bool | field | Terraria.NPC | Terraria/NPC.cs:6373 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 84 | C06 | NpcCollisionAndPresentationState | spriteDirection | int | field | Terraria.NPC | Terraria/NPC.cs:6377 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 85 | C06 | NpcCollisionAndPresentationState | behindTiles | bool | field | Terraria.NPC | Terraria/NPC.cs:6379 | NpcHealth / NpcCollisionState / NpcFramePresentation |
| 86 | C07 | NpcPortalAndSpecialBehaviorState | HitSound | Terraria.Audio.LegacySoundStyle | field | Terraria.NPC | Terraria/NPC.cs:6337 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 87 | C07 | NpcPortalAndSpecialBehaviorState | DeathSound | Terraria.Audio.LegacySoundStyle | field | Terraria.NPC | Terraria/NPC.cs:6339 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 88 | C07 | NpcPortalAndSpecialBehaviorState | lastPortalColorIndex | int | field | Terraria.NPC | Terraria/NPC.cs:6441 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 89 | C07 | NpcPortalAndSpecialBehaviorState | despawnEncouraged | bool | field | Terraria.NPC | Terraria/NPC.cs:6443 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 90 | C07 | NpcPortalAndSpecialBehaviorState | cavernMonsterType | int[,] | field | Terraria.NPC | Terraria/NPC.cs:6445 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 91 | C07 | NpcPortalAndSpecialBehaviorState | mechQueen | int | field | Terraria.NPC | Terraria/NPC.cs:6449 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 92 | C07 | NpcPortalAndSpecialBehaviorState | brainOfGravity | int | field | Terraria.NPC | Terraria/NPC.cs:6451 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 93 | C07 | NpcPortalAndSpecialBehaviorState | kingSlimePointCacheSize | int | field | Terraria.NPC | Terraria/NPC.cs:6453 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 94 | C07 | NpcPortalAndSpecialBehaviorState | kingSlimePointCacheSizeMax | int | field | Terraria.NPC | Terraria/NPC.cs:6455 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 95 | C07 | NpcPortalAndSpecialBehaviorState | kingSlimePointCache | Point[] | field | Terraria.NPC | Terraria/NPC.cs:6457 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 96 | C07 | NpcPortalAndSpecialBehaviorState | empressRageMode | bool | field | Terraria.NPC | Terraria/NPC.cs:6459 | NpcAudioProfile / NpcPortalState / SpecialBehaviorRegistry / KingSlimePointCache |
| 97 | C08 | NpcBuffSlotAndImmunityState | buffType | int[] | field | Terraria.NPC | Terraria/NPC.cs:6067 | NpcBuffSlots / NpcBuffImmunityPolicy / NpcBuffVisibilityPolicy |
| 98 | C08 | NpcBuffSlotAndImmunityState | buffTime | int[] | field | Terraria.NPC | Terraria/NPC.cs:6069 | NpcBuffSlots / NpcBuffImmunityPolicy / NpcBuffVisibilityPolicy |
| 99 | C08 | NpcBuffSlotAndImmunityState | buffImmune | bool[] | field | Terraria.NPC | Terraria/NPC.cs:6071 | NpcBuffSlots / NpcBuffImmunityPolicy / NpcBuffVisibilityPolicy |
| 100 | C08 | NpcBuffSlotAndImmunityState | canDisplayBuffs | bool | field | Terraria.NPC | Terraria/NPC.cs:6073 | NpcBuffSlots / NpcBuffImmunityPolicy / NpcBuffVisibilityPolicy |
| 101 | C09 | NpcElementalDebuffState | midas | bool | field | Terraria.NPC | Terraria/NPC.cs:6075 | NpcElementalStatusProjection |
| 102 | C09 | NpcElementalDebuffState | ichor | bool | field | Terraria.NPC | Terraria/NPC.cs:6077 | NpcElementalStatusProjection |
| 103 | C09 | NpcElementalDebuffState | brokenArmor | bool | field | Terraria.NPC | Terraria/NPC.cs:6079 | NpcElementalStatusProjection |
| 104 | C09 | NpcElementalDebuffState | onFire | bool | field | Terraria.NPC | Terraria/NPC.cs:6081 | NpcElementalStatusProjection |
| 105 | C09 | NpcElementalDebuffState | onFire2 | bool | field | Terraria.NPC | Terraria/NPC.cs:6083 | NpcElementalStatusProjection |
| 106 | C09 | NpcElementalDebuffState | onFire3 | bool | field | Terraria.NPC | Terraria/NPC.cs:6085 | NpcElementalStatusProjection |
| 107 | C09 | NpcElementalDebuffState | onFrostBurn | bool | field | Terraria.NPC | Terraria/NPC.cs:6087 | NpcElementalStatusProjection |
| 108 | C09 | NpcElementalDebuffState | onFrostBurn2 | bool | field | Terraria.NPC | Terraria/NPC.cs:6089 | NpcElementalStatusProjection |
| 109 | C09 | NpcElementalDebuffState | poisoned | bool | field | Terraria.NPC | Terraria/NPC.cs:6091 | NpcElementalStatusProjection |
| 110 | C09 | NpcElementalDebuffState | venom | bool | field | Terraria.NPC | Terraria/NPC.cs:6093 | NpcElementalStatusProjection |
| 111 | C09 | NpcElementalDebuffState | tipsy | bool | field | Terraria.NPC | Terraria/NPC.cs:6095 | NpcElementalStatusProjection |
| 112 | C09 | NpcElementalDebuffState | bleeding | bool | field | Terraria.NPC | Terraria/NPC.cs:6097 | NpcElementalStatusProjection |
| 113 | C09 | NpcElementalDebuffState | hemorrhage | bool | field | Terraria.NPC | Terraria/NPC.cs:6099 | NpcElementalStatusProjection |
| 114 | C09 | NpcElementalDebuffState | shadowFlame | bool | field | Terraria.NPC | Terraria/NPC.cs:6105 | NpcElementalStatusProjection |
| 115 | C09 | NpcElementalDebuffState | soulDrain | bool | field | Terraria.NPC | Terraria/NPC.cs:6107 | NpcElementalStatusProjection |
| 116 | C09 | NpcElementalDebuffState | shimmering | bool | field | Terraria.NPC | Terraria/NPC.cs:6109 | NpcElementalStatusProjection |
| 117 | C09 | NpcElementalDebuffState | oiled | bool | field | Terraria.NPC | Terraria/NPC.cs:6147 | NpcElementalStatusProjection |
| 118 | C10 | NpcControlAndSocialEffectState | confused | bool | field | Terraria.NPC | Terraria/NPC.cs:6117 | NpcControlEffectState / NpcSocialEffectProjection |
| 119 | C10 | NpcControlAndSocialEffectState | loveStruck | bool | field | Terraria.NPC | Terraria/NPC.cs:6119 | NpcControlEffectState / NpcSocialEffectProjection |
| 120 | C10 | NpcControlAndSocialEffectState | stinky | bool | field | Terraria.NPC | Terraria/NPC.cs:6121 | NpcControlEffectState / NpcSocialEffectProjection |
| 121 | C10 | NpcControlAndSocialEffectState | dryadWard | bool | field | Terraria.NPC | Terraria/NPC.cs:6123 | NpcControlEffectState / NpcSocialEffectProjection |
| 122 | C11 | NpcWhipAndSpecialEffectState | markedByScytheWhip | bool | field | Terraria.NPC | Terraria/NPC.cs:6101 | NpcWhipMarkState / NpcSpecialHitEffectProjection |
| 123 | C11 | NpcWhipAndSpecialEffectState | markedByEelWhip | bool | field | Terraria.NPC | Terraria/NPC.cs:6103 | NpcWhipMarkState / NpcSpecialHitEffectProjection |
| 124 | C11 | NpcWhipAndSpecialEffectState | javelined | bool | field | Terraria.NPC | Terraria/NPC.cs:6131 | NpcWhipMarkState / NpcSpecialHitEffectProjection |
| 125 | C11 | NpcWhipAndSpecialEffectState | tentacleSpiked | bool | field | Terraria.NPC | Terraria/NPC.cs:6133 | NpcWhipMarkState / NpcSpecialHitEffectProjection |
| 126 | C11 | NpcWhipAndSpecialEffectState | bloodButchered | bool | field | Terraria.NPC | Terraria/NPC.cs:6135 | NpcWhipMarkState / NpcSpecialHitEffectProjection |
| 127 | C11 | NpcWhipAndSpecialEffectState | celled | bool | field | Terraria.NPC | Terraria/NPC.cs:6137 | NpcWhipMarkState / NpcSpecialHitEffectProjection |
| 128 | C11 | NpcWhipAndSpecialEffectState | dryadBane | bool | field | Terraria.NPC | Terraria/NPC.cs:6139 | NpcWhipMarkState / NpcSpecialHitEffectProjection |
| 129 | C11 | NpcWhipAndSpecialEffectState | daybreak | bool | field | Terraria.NPC | Terraria/NPC.cs:6141 | NpcWhipMarkState / NpcSpecialHitEffectProjection |
| 130 | C11 | NpcWhipAndSpecialEffectState | betsysCurse | bool | field | Terraria.NPC | Terraria/NPC.cs:6145 | NpcWhipMarkState / NpcSpecialHitEffectProjection |
| 131 | C12 | NpcRegenerationAndProtectionState | lifeRegen | int | field | Terraria.NPC | Terraria/NPC.cs:6111 | NpcRegenerationState / NpcProtectionPolicy / NpcElectricEffectState |
| 132 | C12 | NpcRegenerationAndProtectionState | lifeRegenCount | int | field | Terraria.NPC | Terraria/NPC.cs:6113 | NpcRegenerationState / NpcProtectionPolicy / NpcElectricEffectState |
| 133 | C12 | NpcRegenerationAndProtectionState | lifeRegenExpectedLossPerSecond | int | field | Terraria.NPC | Terraria/NPC.cs:6115 | NpcRegenerationState / NpcProtectionPolicy / NpcElectricEffectState |
| 134 | C12 | NpcRegenerationAndProtectionState | immortal | bool | field | Terraria.NPC | Terraria/NPC.cs:6125 | NpcRegenerationState / NpcProtectionPolicy / NpcElectricEffectState |
| 135 | C12 | NpcRegenerationAndProtectionState | chaseable | bool | field | Terraria.NPC | Terraria/NPC.cs:6127 | NpcRegenerationState / NpcProtectionPolicy / NpcElectricEffectState |
| 136 | C12 | NpcRegenerationAndProtectionState | canGhostHeal | bool | field | Terraria.NPC | Terraria/NPC.cs:6129 | NpcRegenerationState / NpcProtectionPolicy / NpcElectricEffectState |
| 137 | C12 | NpcRegenerationAndProtectionState | dontTakeDamageFromHostiles | bool | field | Terraria.NPC | Terraria/NPC.cs:6143 | NpcRegenerationState / NpcProtectionPolicy / NpcElectricEffectState |
| 138 | C12 | NpcRegenerationAndProtectionState | electricEelCounter | int | field | Terraria.NPC | Terraria/NPC.cs:6149 | NpcRegenerationState / NpcProtectionPolicy / NpcElectricEffectState |
| 139 | C13 | NpcLifecycleAndCrossDomainRefs | lazyNPCOwnedProjectileSearchArray | int[] | field | Terraria.NPC | Terraria/NPC.cs:6295 | NpcSlotIndexRegistry / NpcSpawnProtectionRegistry / NpcAudioCooldownState / RevengeServiceAdapter |
| 140 | C13 | NpcLifecycleAndCrossDomainRefs | spawnSlotProtected | int[] | field | Terraria.NPC | Terraria/NPC.cs:6297 | NpcSlotIndexRegistry / NpcSpawnProtectionRegistry / NpcAudioCooldownState / RevengeServiceAdapter |
| 141 | C13 | NpcLifecycleAndCrossDomainRefs | soundDelay | int | field | Terraria.NPC | Terraria/NPC.cs:6299 | NpcSlotIndexRegistry / NpcSpawnProtectionRegistry / NpcAudioCooldownState / RevengeServiceAdapter |
| 142 | C13 | NpcLifecycleAndCrossDomainRefs | RevengeManager | Terraria.GameContent.CoinLossRevengeSystem | field | Terraria.NPC | Terraria/NPC.cs:6301 | NpcSlotIndexRegistry / NpcSpawnProtectionRegistry / NpcAudioCooldownState / RevengeServiceAdapter |
| 143 | C14 | NpcNetworkReplicationState | netUpdatePendingSpamCooldown | bool | field | Terraria.NPC | Terraria/NPC.cs:6017 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 144 | C14 | NpcNetworkReplicationState | netUpdatePendingFullSpamCooldown | bool | field | Terraria.NPC | Terraria/NPC.cs:6019 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 145 | C14 | NpcNetworkReplicationState | netSpamPacketLimit | int | field | Terraria.NPC | Terraria/NPC.cs:6021 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 146 | C14 | NpcNetworkReplicationState | netSpamTicksPerPacket | int | field | Terraria.NPC | Terraria/NPC.cs:6023 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 147 | C14 | NpcNetworkReplicationState | netSpamTicksPerPacketForBosses | int | field | Terraria.NPC | Terraria/NPC.cs:6025 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 148 | C14 | NpcNetworkReplicationState | netSpam | int | field | Terraria.NPC | Terraria/NPC.cs:6027 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 149 | C14 | NpcNetworkReplicationState | netAlways | bool | field | Terraria.NPC | Terraria/NPC.cs:6029 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 150 | C14 | NpcNetworkReplicationState | spawnNeedsSyncing | bool | field | Terraria.NPC | Terraria/NPC.cs:6031 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 151 | C14 | NpcNetworkReplicationState | netStream | int | field | Terraria.NPC | Terraria/NPC.cs:6033 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 152 | C14 | NpcNetworkReplicationState | playerNetSyncState | Terraria.NPC.PlayerNetSyncState[] | field | Terraria.NPC | Terraria/NPC.cs:6035 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 153 | C14 | NpcNetworkReplicationState | netOffset | Vector2 | field | Terraria.NPC | Terraria/NPC.cs:6037 | NpcReplicationIntentProjection / NpcReplicationBudgetAdapter / NpcReplicationStreamAdapter / NpcPerPlayerReplicationState |
| 154 | C15 | NpcNetworkSyncState | skippedSyncs | byte | field | Terraria.NPC.PlayerNetSyncState | Terraria/NPC.cs:34 | PlayerNpcSyncCursorAdapter |
| 155 | C15 | NpcNetworkSyncState | streamCounter | byte | field | Terraria.NPC.PlayerNetSyncState | Terraria/NPC.cs:36 | PlayerNpcSyncCursorAdapter |
| 156 | C16 | NpcDamageDefinitionRegistry | NPCTypes | System.Collections.Generic.List<int> | field | Terraria.GameContent.NPCDamageTracker.CustomDefinition | Terraria.GameContent/NPCDamageTracker.cs:14 | NpcDamageDefinitionCatalog / BossTypeLookup |
| 157 | C16 | NpcDamageDefinitionRegistry | Name | Terraria.Localization.LocalizedText | field | Terraria.GameContent.NPCDamageTracker.CustomDefinition | Terraria.GameContent/NPCDamageTracker.cs:16 | NpcDamageDefinitionCatalog / BossTypeLookup |
| 158 | C16 | NpcDamageDefinitionRegistry | CustomBossDefinitions | Terraria.GameContent.NPCDamageTracker.CustomDefinition[] | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:46 | NpcDamageDefinitionCatalog / BossTypeLookup |
| 159 | C16 | NpcDamageDefinitionRegistry | BossTypeForMob | int[] | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:48 | NpcDamageDefinitionCatalog / BossTypeLookup |
| 160 | C17 | NpcDamageRuntimeTracking | _activeTrackers | System.Collections.Generic.List<Terraria.GameContent.NPCDamageTracker> | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:50 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 161 | C17 | NpcDamageRuntimeTracking | _recentFinishedTrackers | System.Collections.Generic.List<Terraria.GameContent.NPCDamageTracker> | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:52 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 162 | C17 | NpcDamageRuntimeTracking | MAX_RECENT_TRACKERS | int | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:54 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 163 | C17 | NpcDamageRuntimeTracking | EXTRA_RECENT_TRACKER_EXPIRY_TIME | int | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:56 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 164 | C17 | NpcDamageRuntimeTracking | _list | System.Collections.Generic.List<Terraria.GameContent.NPCDamageTracker.CreditEntry> | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:58 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 165 | C17 | NpcDamageRuntimeTracking | _worldCredit | Terraria.GameContent.NPCDamageTracker.WorldCreditEntry | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:60 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 166 | C17 | NpcDamageRuntimeTracking | _lastAttacker | Terraria.GameContent.NPCDamageTracker.CreditEntry | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:62 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 167 | C17 | NpcDamageRuntimeTracking | _ticks | int | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:64 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 168 | C17 | NpcDamageRuntimeTracking | _lastHitTime | int | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:66 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 169 | C17 | NpcDamageRuntimeTracking | IsEmpty | bool | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:68 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 170 | C17 | NpcDamageRuntimeTracking | Duration | int | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:70 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 171 | C17 | NpcDamageRuntimeTracking | TimeSinceLastHit | int | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:72 | DamageTrackerService / NpcDamageEncounterTracker / TrackerClockState / TrackerQuery |
| 172 | C18 | NpcDamageCreditProjection | PlayerName | string | field | Terraria.GameContent.NPCDamageTracker.PlayerCreditEntry | Terraria.GameContent/NPCDamageTracker.cs:31 | DamageCreditQuery / DamageCreditMessageProjection |
| 173 | C18 | NpcDamageCreditProjection | Damage | int | property | Terraria.GameContent.NPCDamageTracker.CreditEntry | Terraria.GameContent/NPCDamageTracker.cs:21 | DamageCreditQuery / DamageCreditMessageProjection |
| 174 | C18 | NpcDamageCreditProjection | Name | Terraria.Localization.NetworkText | property | Terraria.GameContent.NPCDamageTracker.CreditEntry | Terraria.GameContent/NPCDamageTracker.cs:23 | DamageCreditQuery / DamageCreditMessageProjection |
| 175 | C18 | NpcDamageCreditProjection | Name | Terraria.Localization.NetworkText | property | Terraria.GameContent.NPCDamageTracker.PlayerCreditEntry | Terraria.GameContent/NPCDamageTracker.cs:33 | DamageCreditQuery / DamageCreditMessageProjection |
| 176 | C18 | NpcDamageCreditProjection | Name | Terraria.Localization.NetworkText | property | Terraria.GameContent.NPCDamageTracker.WorldCreditEntry | Terraria.GameContent/NPCDamageTracker.cs:43 | DamageCreditQuery / DamageCreditMessageProjection |
| 177 | C18 | NpcDamageCreditProjection | Name | Terraria.Localization.LocalizedText | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:74 | DamageCreditQuery / DamageCreditMessageProjection |
| 178 | C18 | NpcDamageCreditProjection | KillTimeMessage | Terraria.Localization.LocalizedText | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs:76 | DamageCreditQuery / DamageCreditMessageProjection |
| 179 | C19 | NpcInteractionAndCommerce | _shopIndex | int | field | Terraria.GameContent.NPCInteractions.Actions.OpenShop | Terraria.GameContent/NPCInteractions.cs:27 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 180 | C19 | NpcInteractionAndCommerce | _npcType | int | field | Terraria.GameContent.NPCInteractions.Actions.OpenShop | Terraria.GameContent/NPCInteractions.cs:29 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 181 | C19 | NpcInteractionAndCommerce | _customTextKey | string | field | Terraria.GameContent.NPCInteractions.Actions.OpenShop | Terraria.GameContent/NPCInteractions.cs:31 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 182 | C19 | NpcInteractionAndCommerce | All | System.Collections.Generic.List<Terraria.GameContent.NPCInteraction> | field | Terraria.GameContent.NPCInteractions | Terraria.GameContent/NPCInteractions.cs:233 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 183 | C19 | NpcInteractionAndCommerce | ShowExcalmation | bool | property | Terraria.GameContent.NPCInteraction | Terraria.GameContent/NPCInteraction.cs:7 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 184 | C19 | NpcInteractionAndCommerce | LocalPlayer | Terraria.Player | property | Terraria.GameContent.NPCInteraction | Terraria.GameContent/NPCInteraction.cs:9 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 185 | C19 | NpcInteractionAndCommerce | TalkNPC | Terraria.NPC | property | Terraria.GameContent.NPCInteraction | Terraria.GameContent/NPCInteraction.cs:11 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 186 | C19 | NpcInteractionAndCommerce | TalkNPCType | int | property | Terraria.GameContent.NPCInteraction | Terraria.GameContent/NPCInteraction.cs:13 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 187 | C19 | NpcInteractionAndCommerce | ShowExcalmation | bool | property | Terraria.GameContent.NPCInteractions.Actions.StardewValleyBit | Terraria.GameContent/NPCInteractions.cs:50 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 188 | C19 | NpcInteractionAndCommerce | ShowExcalmation | bool | property | Terraria.GameContent.NPCInteractions.Actions.AnglerQuest | Terraria.GameContent/NPCInteractions.cs:72 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |
| 189 | C19 | NpcInteractionAndCommerce | ShowExcalmation | bool | property | Terraria.GameContent.NPCInteractions.Actions.RequestHome | Terraria.GameContent/NPCInteractions.cs:162 | NpcInteractionCommand / ShopRegistrationRegistry / NpcInteractionQuery / InteractionAdapter |

## 7. 跨子系统所有权和边界图

| 交接 | 当前候选权威 | 只读消费者 | 写入方式 | 未决点 |
|---|---|---|---|---|
| NPC slot、active、reset、spawn | NPC lifecycle owner | AI、combat、network、persistence | Spawn/Reset/Despawn Command -> CommitPort | Main.npc 槽位与其他分区交接 |
| type、netID、definition | spawn/definition owner | AI、combat、network、loot | Definition reference commit | type 与 netID 是否同一生命周期 |
| AI、target、movement | NpcBehavior owner | collision、combat、network | AI intent -> AI commit | UpdateNPC 阶段和 writer 数量 |
| life、lifeMax、death | Health/Death owner | damage tracker、network、loot | DamageResolution -> HealthCommit -> DeathEvent | C05/C06 分界 |
| damage、defense、immunity、friendly | Combat policy owner | DamageResolution、target query | CombatPolicyCommitPort | Projectile/NPC damage root |
| buff slots、immunity、derived flags | Buff owner | damage、AI、presentation、network | ApplyBuff/ExpireBuff Command | packet 53 与布尔别名失效 |
| collision/frame/presentation | Collision and presentation owners | renderer、network | Physics/Frame commit | targetRect 与 draw geometry |
| network pending/budget/sync | network adapter | packet writers/readers | snapshot -> packet command | 不得反向写 authority |
| damage tracker | DamageTrackerService | credit query、loot、UI | hit/death events + clock port | global registry and recent expiry |
| interaction/shop | interaction owner | UI、commerce、quest | interaction command | Main.LocalPlayer and talk NPC adapters |
| persistence/bestiary/town | WorldStorage owner | NPC definitions and snapshots | load/save adapter | WorldFile fields and order |

显式 scheduler graph：

DefinitionLoad -> SpawnSlotCommit -> NpcIdentitySnapshot

NpcIdentitySnapshot -> NpcAiCommit

NpcAiCommit + WorldFacts -> NpcMovementCommit -> NpcCollisionCommit

NpcCollisionCommit + NpcHealthSnapshot -> CombatEligibilityQuery

CombatEligibilityQuery -> DamageResolution -> HealthCommit -> DeathResolution

BuffCommand -> BuffCommit -> Elemental/Control/Whip Projections

Health/AI/Buff/Collision snapshots -> NetworkReplicationProjection -> PacketAdapter

DamageHitEvent + ClockPort -> DamageTrackerService -> DamageCreditQuery -> LocalizedMessageProjection

PlayerInteractionCommand + NPC snapshot + ProgressionQuery -> InteractionOwner -> Shop/UI Adapter

LoadAdapter -> Spawn/Definition/Buff/Health/Interaction owner；Owner snapshots -> SaveAdapter 仅在格式证据闭合后启用。

箭头是数据和提交依赖，不是文件顺序；每条边都需要 verifier 证明没有第二写者、隐式时钟或反向写回。

## 8. 接口、副作用和兼容策略

候选端口：INpcLifecycleCommitPort、INpcBehaviorCommitPort、INpcMovementCommitPort、INpcCollisionCommitPort、IDamageCommitPort、IHealthCommitPort、IBuffCommitPort、INpcReplicationAdapter、IDamageTrackerService、IDamageCreditQuery、INpcInteractionCommitPort、INpcPersistenceAdapter。

NpcSpawnCommand、NpcAiIntent、MovementFacts、CollisionResult、DamageIntent、BuffCommand、NpcReplicationSnapshot、DamageHitEvent、DamageCreditSnapshot、NpcInteractionCommand 和 NpcSaveSnapshot 都是显式边界 payload。时钟来自 IWorldTickClock 或等价端口；网络、音频、渲染、UI、日志和持久化只能在 adapter 中产生副作用。

兼容顺序：先建立快照、Query 和 Adapter，不改变旧字段；引入唯一 Owner；旧读路径暂由 facade 保留；逐个切换读者；focused verifier 通过后才移除 facade。网络和持久化没有 reader/writer/format/清理证据时保持 blocked。C09-C11 先作为 Projection/Query；C16-C19 先按 Definition/Service/Query/Adapter 分离。

## 9. 风险、blocking decision 和 focused verifier

- active/reset、type/netID、realLife、slot arrays 和 projectile references 的写者与清理顺序仍未完全闭合。
- life/lifeMax、defLifeMax、damage/defense、regen、death 必须形成单一 DamageResolution -> HealthCommit -> DeathResolution 链。
- buff 数组与元素、控制、whip 布尔成员可能是别名、缓存或独立状态，未闭合失效路径前不可双写。
- network budget、stream、per-player cursor 受 connection 生命周期约束，Projection/Adapter 不得回写 authority。
- tracker 的全局列表、ticks、expiry、world/player credit 需要显式 clock、清理和并发假设。
- interaction 的 Main.LocalPlayer、talkNPC、quest/progression 和 shop registry 不属于通用 NPC component。

focused verifier 按 C01-C03 的生命周期和表现、C04-C07 的 AI/战斗/碰撞、C08-C12 的 Buff/状态/保护、C13-C15 的槽位/网络、C16-C18 的 definition/tracker/credit、C19 的交互经济分别建立。验证内容包括 reset、状态转换、唯一写者、纯 Query、单向 Projection、packet/connection、持久化 round-trip、时钟和重试。

### Integration handoff

- integration-review 必须为 NPC slot/lifecycle、type/netID、realLife、Projectile reference、health/death/damage、Buff/immunity、town/progression/commerce、network/session replication、WorldFile/bestiary/town-manager 分别指定唯一 Owner、允许的写入端口、清理顺序和失败/重试语义。
- 实施者接收本设计文档、执行文档、P12 权威报告、Version4 锚点清单和逐成员映射；先补齐 reader/writer/creation/cleanup/persistence/network/scheduler evidence ledger，再创建任何 C# 类型。
- entry criteria 是：没有第二写者；数组和索引有长度/generation/reset 契约；Query/Projection/Adapter 无反向写回；packet 23/28/53 和 WorldFile 格式有 characterization；focused verifier 计划绑定到单一 checkpoint。
- 交接时必须保留未落地边界的 status: proposed、evidenceStatus: partial、blocking-decision 和未运行验证声明；已落地组件也不得被当作行为完成或行为等价证明。

## 10. 完成条件

P12 设计计划完成不等于实现完成。当前分区仍需把可独立确定的组件源码逐步落地；两份文档的 completedComponents、currentComponent、pendingComponents 必须与真实源码和阻塞边界一致，未验证源码不得写成行为完成。满足实际交付条件后才可使用原始 P12 和当前 sessionId 调用 Complete。

## 11. Checkpoint C01（已保存；实现状态见文档顶部）

- 候选类型：NpcActivationState、NpcInteractionState、NpcPresentationState、NpcCatchState、NpcReceptionPolicy、NpcWorldPolicy。
- 先建立 identity/presentation snapshot；交互由 Command 输入；确认 lifecycle owner 后才接 active/reset 写入；takenDamageMultiplier 和 static policy 保留 integration-review。
- 依赖：spawn/reset -> identity；interaction command -> interaction owner；snapshot -> draw/network projection；combat policy -> damage eligibility。
- 回滚：若发现 active、playerInteraction、catch/release 或 name/presentation 的写者不止一个，保留旧 facade，不启用新 Owner，不改变网络或持久化。
- 验证：源码已写入并通过 `Terraria.Npc.csproj` 编译；现有 NPC/Town 组件字段 verifier 已通过，但不覆盖本 checkpoint 的三个新组件。运行时、网络、持久化和行为等价 verifier 仍未执行。

## 13. Checkpoint C02：NPC target、movement 和 history（已记录）

边界结论：C02 整体保持 partial；`NpcMovementHistoryComponent` 已写入但尚未验证，其余候选边界保持 status: proposed/blocked。C02 的 14 个成员按物理输入、传送过程、移动历史和 frame sizing 分开，避免把移动规则、碰撞结果和渲染缓存放进同一个可变结构：

- NpcMovementMediumProfile：waterMovementSpeed、lavaMovementSpeed、honeyMovementSpeed、shimmerMovementSpeed 是移动介质定义或实体覆盖值。它只提供给 movement owner，不能由 renderer 或 network adapter 写入。
- NpcTeleportState：teleportStyle、teleportTime、teleporting 由 teleport command/owner 管理；传送完成必须产生明确的 position/history reset 或提交事件。
- NpcMovementHistory：oldPos 和 oldRot 默认使用长度 10 的有界历史，并允许初始化时按已确认容量构造；读取者只拿不可变快照，禁止将数组引用泄露给 AI、网络或表现层。当前 `NpcMovementHistoryComponent` 只保存这两个数组，不实现历史推进或清理行为。
- NpcMovementHistory：gfxOffY、stairFall 是 movement/presentation 交接候选，需由调用点决定是权威过程状态还是派生 draw input。
- NpcMovementPolicy：stepSpeed 和静态 gravity 先作为 policy/input 候选；不能因为声明在 NPC 类中就成为每实体组件的第二物理常量。
- NpcFrameSizingState：setFrameSize 只由 frame sizing owner 清除和设置，renderer 只能读取提交后的结果。

迁移顺序：先固定 history 长度和 reset 语义；定义 MovementFacts、TeleportCommand 和 NpcMovementSnapshot；接入唯一 movement owner；再将一个 legacy reader 切到 snapshot；最后才评估 gfxOffY、stepSpeed 与 frame sizing 的跨域归属。

依赖：C01 的 activation/reset -> C02 初始化；world/physics facts -> movement owner；movement commit -> collision/frame projection；teleport event -> network replication 和 history invalidation。C02 不拥有 NPC slot、health、AI decision 或 network cursor。

验证记录：`NpcMovementHistoryComponent` 源码已保存并通过 `Terraria.Npc.csproj` 编译；现有 NPC/Town 组件字段 verifier 已通过，但不覆盖本组件的构造器边界。默认容量、显式容量、长度不匹配和输入防御性复制，以及介质速度默认值、传送中/完成转换、spawn/despawn/reuse 清理、历史推进、gravity/stepSpeed 的单一来源和 scheduler 依赖仍属于后续 focused verifier。C02 其余边界仍为 proposed/blocked；下一 checkpoint 为 C03。

### C01-C02 verification record

- Build guard：每次 compile-capable invocation 前均检查 `dotnet.exe`/`csc.exe`；未发现活动或 owner 不明的编译进程后才启动下一次串行命令。
- Affected project build：`pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -- build '.\src\Npc\Terraria.Npc.csproj' '-m:1' '-nr:false' '-p:UseSharedCompilation=false' '-p:MSBuildNodeReuse=false' '-p:BuildInParallel=false'"`；exit code `0`，warnings `0`，errors `0`；artifact `D:\TRbackup\NLTX\Build\bin\Terraria.Npc\Debug\net10.0\Terraria.Npc.dll`。
- Existing verifier build：`$dotnetArguments = @('build', '.\Test\Terraria.Npc.Components.Verification\Terraria.Npc.Components.Verification.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments`；exit code `0`，warnings `0`，errors `0`；artifact `D:\TRbackup\NLTX\Build\bin\Terraria.Npc.Components.Verification\Debug\net10.0\Terraria.Npc.Components.Verification.dll`。
- Existing verifier run：`$dotnetArguments = @('run', '--project', '.\Test\Terraria.Npc.Components.Verification\Terraria.Npc.Components.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments`；exit code `0`，输出 `PASS: NPC and town component field composition`，artifact `D:\TRbackup\NLTX\Build\bin\Terraria.Npc.Components.Verification\Debug\net10.0\Terraria.Npc.Components.Verification.dll`。
- 以上 verifier 只覆盖既有 NPC/Town 组件字段组合，不覆盖本次四个新组件的构造器边界、reset、movement history 推进、网络或持久化行为；因此当前状态是 focused build/verifier evidence，不是全分区行为等价证明。

## 14. Checkpoint C03：NPC identity、parent link 和 status（已记录）

边界结论保持 status: proposed。C03 的四个成员不是一个同质状态块：

- NpcParentLifeLink：realLife 表示跨 NPC 的父生命/关联目标候选。它只能由 spawn/definition 或 boss/death owner 通过受校验的 entity reference 写入；父实体失效、slot reuse、分裂/合并和死亡清理必须显式处理。
- NpcNameState：_givenName 是身份/本地化输入候选。持久化和网络是否保存它尚未由完整 WorldFile/packet 证据闭合，迁移初期只提供只读 snapshot。
- NpcShimmerVisualState：shimmerTransparency 属于表现投影候选。其更新必须由 shimmer/status owner 提交，renderer 不得直接修改。
- NpcBuffCapacityDefinition：maxBuffs 是静态定义/容量契约候选，不应复制成每个 NPC 都可变的容量字段；Buff slot owner 只引用同一 catalog。

迁移顺序：先确认 realLife 的所有建立、读取、死亡和 slot reuse 调用点；再建立 name/shimmer snapshot；最后冻结 maxBuffs 与 C08 BuffSlots 的容量不变量。不要在没有持久化和网络格式证据时添加 name 的新写入。

依赖：C01 lifecycle/reset -> C03 初始化；C03 parent link -> C05/C06 death and health；name/shimmer snapshot -> C06/C14 presentation/network projection；maxBuffs -> C08 buff owner。C03 不拥有 parent NPC 的生命值，也不拥有 Buff 数组。

验证记录：需要覆盖 parent reference 的有效/无效索引、slot reuse 清除、name 默认值和 round-trip 决策、shimmer reset/expiry、maxBuffs 与实际 Buff 数组长度一致性。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C04。

## 15. Checkpoint C04：NPC AI、target、type 和 per-entity immunity（已记录）

边界结论保持 status: proposed。C04 先按访问模式拆出四个候选边界：

- NpcAiState：directionY、ai、localAI、aiAction、aiStyle、justHit、oldDirectionY、timeLeft 是 AI 过程状态或 AI/lifecycle 交接输入。ai 与 localAI 必须保持固定长度和索引契约，禁止将裸数组引用暴露给任意系统。
- NpcTargetState：target 和 oldTarget 由 target selection/AI owner 提交；目标玩家索引必须在快照边界验证，旧目标清理不能依赖数组越界异常。
- NpcTypeDefinitionRef：type、netID 和部分 aiStyle 的来源是 spawn/definition owner 候选。网络接收可申请 definition ref，但不能让 packet adapter 直接改写任意 AI 状态。
- NpcPerEntityImmunity：immune 是 per-player 或 per-source 的伤害免疫计时候选，Owner 尚未在 AI 与 combat 之间闭合；C04 只记录读取契约，不能由 target system 写入。

迁移顺序：冻结 ai/localAI 长度和默认值；列出 UpdateNPC、target selection、spawn/reset、network reader 的全部 writer；定义 AI intent 与 target commit；将 type/netID 变成受校验的 definition reference；最后在 C05/C08 的 damage/buff owner 评审后决定 immune 的最终归属。

依赖：C03 identity/parent -> AI initialization；definition load -> type/netID/ai policy；target query -> AI intent；C02 movement facts -> AI step；AI/target snapshot -> C05 combat eligibility、C06 collision、C14 replication。C04 不拥有 health、damage tracker 或 packet stream cursor。

验证记录：需要覆盖 AI 数组长度、reset 和 slot reuse、target 无效/死亡/切换、type/netID definition mismatch、justHit 一次性语义、timeLeft 与 lifecycle 的交接、immune writer 唯一性和客户端 packet 不回写规则。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C05。

## 16. Checkpoint C05：NPC combat policy、scaling 和 reward（已记录）

边界结论保持 status: proposed。C05 只负责战斗资格和策略事实，不直接拥有 C06 的 life/lifeMax：

- NpcCombatStats：damage、defense、defDamage、defDefense、defLifeMax 是当前值与默认基线候选。defLifeMax 是 health 初始化输入，不是第二份生命存量。
- NpcDamageAcceptancePolicy：coldDamage、trapImmune、lavaImmune、dontTakeDamage、catchableNPCTempImmunityCounter、reflectsProjectiles 是 damage eligibility/response 输入。counter 必须由明确 tick/command owner 更新。
- NpcDisposition：boss、friendly 和 friendlyRegen 是目标选择、伤害资格与恢复规则的交接候选；friendlyRegen 不得绕过 C12 regen owner。
- NpcDifficultyScaling：statsAreScaledForThisManyPlayers 和 difficulty 是世界/人数输入后的提交结果；缩放必须可重复计算并记录输入快照。
- NpcRewardValue：value 与 extraValue 供 loot/reward query 读取，不能被 damage projection 随意改变。
- CommonMasterBossLifeReduction 是 combat definition constant，候选为不可变 catalog/definition，不属于实体可变状态。

迁移顺序：先区分 baseline 与 current stats；定义 DamageEligibilityQuery 和 DamageResolution 输入；确认 projectile、trap、hostile、capture 的单一资格 owner；再接入 difficulty scaling；最后将 reward value 输出为只读 snapshot。C05 不能直接写 life、death、loot 或 tracker credit。

依赖：C04 type/AI/target -> combat eligibility；C08/C09 status snapshot -> damage policy；world difficulty -> scaling；C06 health commit -> death；C17 tracker 只消费已提交的 hit/death event。C05 的 policy 计算不得产生音频、网络、持久化或 UI 副作用。

验证记录：需要覆盖默认/缩放 stat、friendly/boss/hostile 资格、dontTakeDamage 与 counter、trap/lava/cold policy、projectile reflection、重复 DamageIntent、reward value 只读和 CommonMasterBossLifeReduction 的 catalog 版本。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C06。

## 17. Checkpoint C06：NPC health、collision 和 presentation（已记录）

边界结论保持 status: proposed。C06 把 17 个成员按 authority 和副作用分成三个候选 owner：

- NpcHealth：life、lifeMax 是唯一 health/death owner 的权威存量。lifeMax 的初始化输入可以来自 C05 defLifeMax，但 C05 不得保有可写副本；所有 damage、regen、heal 和 death 都通过 HealthCommitPort。
- NpcCollisionState：targetRect、noGravity、noTileCollide、collideX、collideY 是物理/碰撞提交结果。碰撞计算消费 movement、AI、health 和世界 tile facts，不向 presentation 反向写入。
- NpcFramePresentation：frameCounter、frame、color、alpha、hide、scale、rotation、spriteDirection、behindTiles 是表现快照候选。renderer 只能读取不可变输出；scale、rotation 和 spriteDirection 的具体 writer 需按调用点拆分。
- knockBackResist 是 combat/physics policy 交接候选，不因紧邻 collision 字段就归入 NpcCollisionState。

迁移顺序：先建立 HealthSnapshot 和 DamageResolution -> HealthCommit -> DeathEvent；再冻结 targetRect/collision result 的 geometry contract；最后建立 FramePresentationSnapshot 和 renderer adapter。health 的持久化、network packet 和 death/loot 清理必须在各自证据闭合后迁移。

依赖：C05 eligibility -> health commit；C02 movement + world tiles -> collision commit；C03 shimmer、C04 AI、C07 sound/portal -> presentation inputs；health/collision/frame snapshots -> C14 replication、loot、renderer。C06 不拥有 damage tracker 的 credit list，也不拥有 network sync cursor。

验证记录：需要覆盖 life 边界、lifeMax 初始化、零/负伤害、死亡一次性事件、regen/heal 交接、targetRect 计算、noGravity/noTileCollide、collision flags、frame reset、alpha/color/scale/rotation 和 server/client 输出隔离。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C07。

## 18. Checkpoint C07：NPC portal、audio 和 special behavior（已记录）

边界结论保持 status: proposed。C07 的 11 个成员具有不同的生命周期和副作用：

- NpcAudioProfile：HitSound 和 DeathSound 是不可变音频定义或 output descriptor；播放行为只能由 audio adapter 执行，NPC component 不直接调用音频 I/O。
- NpcPortalState：lastPortalColorIndex 和 despawnEncouraged 是 portal/lifecycle 过程状态候选。portal 变化应产生可重放事件，despawn 只通过 lifecycle command 生效。
- SpecialBehaviorRegistry：cavernMonsterType、mechQueen、brainOfGravity、empressRageMode 是全局或 encounter 级特殊行为注册/状态候选，不挂到任意单个 NPC entity。
- KingSlimePointCache：kingSlimePointCacheSize、kingSlimePointCacheSizeMax、kingSlimePointCache 是固定容量、有明确失效条件的 cache 候选。cache 不能成为另一份 portal 或 collision authority。

迁移顺序：先确认 HitSound/DeathSound 的定义加载和调用点；再定义 portal event 与 despawn command；盘点各特殊行为静态 writer；最后为 KingSlime cache 加入容量、清理、world/encounter invalidation contract。C07 的 cache 和 registry 不得由 network packet 直接重建。

依赖：C01 lifecycle -> portal/despawn；C03 shimmer/presentation -> portal output；C06 death -> DeathSound event；special definition load -> registry；world/encounter facts -> KingSlime cache；所有输出 -> C14 replication 或 audio/render adapter。

验证记录：需要覆盖 hit/death 音频事件一次性语义、portal color reset、despawn eligibility、静态 registry 初始化/重载、empress rage scope、cache capacity/eviction/invalidation 和 server/client side effect isolation。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C08。

## 19. Checkpoint C08：NPC buff slots、immunity 和 visibility（已记录）

边界结论保持 status: proposed。C08 是 Buff runtime 的候选权威边界：

- NpcBuffSlots：buffType 和 buffTime 必须成对维护，固定容量来自 C03 的 maxBuffs 定义；slot 添加、刷新、合并、过期、清空和 reset 只能由 BuffOwnerSystem 通过 BuffCommitPort 执行。
- NpcBuffImmunityPolicy：buffImmune 是定义/实体覆盖的免疫策略候选。它不能被 elemental projection 或 renderer 写回；Buff apply 必须先读取不可变 policy snapshot。
- NpcBuffVisibilityPolicy：canDisplayBuffs 是表现和网络输出策略候选，与 buff 是否实际存在分离；隐藏不得删除 slot 或改变 damage semantics。

迁移顺序：冻结 slot capacity/index contract；盘点 ApplyBuff、ClearBuff、expire、ResetForNewNPC 和 packet 53 的 writer/reader；引入 BuffCommand 和 BuffSnapshot；再将 packet 映射改为 adapter；最后才迁移 C09-C11 的派生 flags。

依赖：C03 maxBuffs -> slot initialization；C04 type/definition -> immunity policy；BuffCommit -> C09 elemental、C10 control/social、C11 whip projections；BuffSnapshot -> C05 damage eligibility、C06 presentation、C14 network。C08 不拥有 localization、renderer 或 packet stream cursor。

验证记录：需要覆盖 slot 容量、重复 buff、duration refresh、过期顺序、免疫拒绝、reset/despawn/reuse、packet 53 round-trip、隐藏 Buff 的读写隔离和 derived projection invalidation。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C09。

## 20. Checkpoint C09：NPC elemental debuff projection（已记录）

边界结论保持 status: proposed。midas、ichor、brokenArmor、onFire/onFire2/onFire3、onFrostBurn/onFrostBurn2、poisoned、venom、tipsy、bleeding、hemorrhage、shadowFlame、soulDrain、shimmering、oiled 共 17 个成员先作为 NpcElementalStatusProjection 的输出，不自动创建 17 个独立组件。

Projection 输入是 C08 的 BuffSnapshot、定义 catalog 和必要的世界/时间事实；输出是不可变的 status flags snapshot，供 damage eligibility、AI、UI、renderer 和 network projection 读取。Projection 不得写 buffType、buffTime、buffImmune 或 C05/C06 的权威状态。

迁移顺序：列出每个 flag 的全部 writer 和 reader；建立纯计算函数；定义 BuffCommit 后的 invalidation；先切换一个只读消费者；确认 flag 是源事实、别名还是缓存后，再决定是否需要单独 Component。保存和网络字段在格式证据闭合前不迁移。

依赖：C08 BuffCommit -> C09 calculation -> C05/C10/C11/C12 consumers；C03 shimmer -> shimmering output；C14 只消费 snapshot。不要允许 C09 反向触发 ApplyBuff 或 DamageCommit。

验证记录：需要覆盖每个 flag 的 Buff 到期/刷新、同时存在多个效果、免疫拒绝、reset/despawn、重复计算确定性、缓存失效和无写回；特别检查 onFire 系列与 shimmer/oiled 的差异语义。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C10。

## 21. Checkpoint C10：NPC control and social effects（已记录）

边界结论保持 status: proposed：

- NpcControlEffectState：confused 是 AI/control 过程状态候选。它应由 Buff/status command 提交，供 AI target/movement 查询；UI、social query 和 renderer 不能直接清除或设置。
- NpcSocialEffectProjection：loveStruck、stinky、dryadWard 先作为从 Buff/定义/世界事实派生的社会和表现输出。它们可能影响 interaction、damage、target 或 renderer，但 Projection 不拥有这些系统的权威状态。

迁移顺序：确认四个 flag 的独立 writer、duration 和 packet/persistence 语义；定义 ControlStatusCommand 和 SocialStatusSnapshot；把 AI 读取切到 snapshot；再确认是否需要 control 的权威 Component。C10 不复制 C09 的 Buff-derived calculation，也不把社会输出塞进 interaction registry。

依赖：C08 BuffCommit/C09 status snapshot -> C10 control/social output；C04 AI/target consumes confused；C05 eligibility 和 C19 interaction 可读取 snapshot；C14 network/renderer 只消费不可变 output。

验证记录：需要覆盖 confused 对 AI 的持续时间与清理、loveStruck/stinky/dryadWard 的派生失效、同时效果、reset/despawn、交互读取纯度和 server/client side effect isolation。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C11。

## 22. Checkpoint C11：NPC whip marks 和 special hit effects（已记录）

边界结论保持 status: proposed。C11 的九个成员先划分为两个候选层：

- NpcWhipMarkState：markedByScytheWhip、markedByEelWhip 是战斗命令设置、由明确持续时间和 reset 规则清除的目标标记候选。它们可以被 damage eligibility 读取，但不等于 damage credit。
- NpcSpecialHitEffectProjection：javelined、tentacleSpiked、bloodButchered、celled、dryadBane、daybreak、betsysCurse 先作为从命中特效事件、Buff snapshot 或内容定义派生的输出。其客户端特效、音频和 UI 只能通过 adapter 消费。

迁移顺序：盘点各标记的 writer、duration、refresh、death/reset 和 network/persistence 语义；定义 WhipMarkCommand 与 immutable SpecialHitEffectSnapshot；把一个 combat reader 切换到 mark snapshot；最后再决定哪些 flag 是长期权威状态、哪些只是 projection。

依赖：C05 DamageEligibility -> mark effect；C08 BuffCommit/C09-C10 status -> special effect projection；C06 HealthCommit/DeathEvent -> clear；C14 network 和 C07 audio 只消费 event/snapshot。C11 不直接写 C17 tracker credit 或 C18 localized message。

验证记录：需要覆盖标记叠加/刷新/过期、不同鞭标记并存、死亡和 slot reuse 清理、special flag 的重复事件、damage reader 纯度、server/client effect isolation 和无反向写回。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C12。

## 23. Checkpoint C12：NPC regeneration、protection 和 electric effect（已记录）

边界结论保持 status: proposed：

- NpcRegenerationState：lifeRegen、lifeRegenCount、lifeRegenExpectedLossPerSecond 是 regen calculation/accumulator 候选。它们不能直接改写 life；regen owner 产生 HealthDelta，再通过 IHealthCommitPort 提交。
- NpcProtectionPolicy：immortal、chaseable、canGhostHeal、dontTakeDamageFromHostiles 是 damage eligibility、target eligibility 和 ghost-heal policy 输入。chaseable 的读取者不能把 target selection 变成第二 owner。
- NpcElectricEffectState：electricEelCounter 是特定效果的有界计数/计时状态，必须记录 tick source、上限、清零和与 C11 special effects 的关系。

迁移顺序：区分 regen expected loss 与 actual HealthDelta；列出 protection flag 的全部 writer/reader；定义 RegenerationTick、DamageEligibilitySnapshot 和 ElectricEffectCommand；最后连接 C06 health owner。C12 不拥有 death event、damage credit 或 network packet。

依赖：C05 policy/C09-C11 status -> protection and regen input；clock/world facts -> regen tick；regen/protection -> C06 HealthCommit/DamageEligibility；death/reset -> clear；C17 tracker 只消费最终 hit/death events。

验证记录：需要覆盖 regen tick 的确定性、lifeRegenCount 累积、expected loss 仅作预估、immortal/dontTakeDamageFromHostiles、chaseable target query、ghost heal 条件、电鳗计数边界和 death/reset 清理。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C13。

## 24. Checkpoint C13：NPC lifecycle 和 cross-domain references（已记录）

边界结论保持 status: proposed。C13 的四个成员按生命周期和副作用分离：

- NpcSlotIndexRegistry：lazyNPCOwnedProjectileSearchArray 是 NPC 槽位到 Projectile 查询的跨域索引候选。它只能由 slot/lifecycle owner 在 spawn、despawn 和 slot reuse 时维护；Projectile 系统通过只读查询获取结果。
- NpcSpawnProtectionRegistry：spawnSlotProtected 是槽位保护/重用策略候选，必须明确单位、倒计时来源和清理时点，不能被普通 combat reader 任意减少。
- NpcAudioCooldownState：soundDelay 是实体级音频节流候选；音频播放仍由 C07 audio adapter 执行，计时更新需要显式 tick。
- RevengeServiceAdapter：RevengeManager 是外部 CoinLossRevengeSystem 的服务适配器候选。NPC component 不持有全局服务实例，跨域失败和重试由 adapter 记录。

迁移顺序：先绘制 Main.npc 槽位的创建/清理/reuse 图；确认 Projectile 查询的索引有效性；再定义 spawn protection 的时钟和清理；接入 sound delay snapshot；最后把 RevengeManager 包在显式 service port 后。不得由 network/persistence adapter 直接重建这些 registry。

依赖：C01 activation -> slot registry；C02 movement/teleport -> audio cooldown；C06 death/despawn -> slot/protection cleanup；Projectile query -> C05/C06；revenge event -> external service adapter。C13 不拥有 NPC identity、damage、network cursor 或 UI。

验证记录：需要覆盖 slot reuse、Projectile index stale entry、spawn protection 倒计时和 reset、sound delay tick/side effect isolation、Revenge service failure/retry/idempotence，以及多世界或多 session 的服务作用域。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C14。

## 25. Checkpoint C14：NPC network replication state（已记录）

边界结论保持 status: proposed。C14 的 11 个成员明确属于网络输出/节流边界，而不是 NPC 业务权威组件：

- NpcReplicationIntentProjection：netUpdatePendingSpamCooldown、netUpdatePendingFullSpamCooldown、netAlways、spawnNeedsSyncing 和 netOffset 是从 authority snapshot 生成复制意图/快照的候选。意图只交给 packet adapter。
- NpcReplicationBudgetAdapter：netSpamPacketLimit、netSpamTicksPerPacket、netSpamTicksPerPacketForBosses、netSpam 是发送预算和节流状态候选。预算不能被 combat 或 renderer 直接改变。
- NpcReplicationStreamAdapter：netStream 是 packet stream 编码/解码边界候选，具体格式必须以 NetMessage writer 和 MessageBuffer reader 逐字段核对。
- NpcPerPlayerReplicationState：playerNetSyncState 是每连接/每玩家的复制状态候选，不能成为每 NPC 公共业务状态；disconnect、player reuse 和 world reset 必须清除。

迁移顺序：建立只读 NpcReplicationSnapshot；盘点 packet 23 writer 和 reader 的字段顺序、条件分支及 spawn/full/update 语义；建模 budget 和 per-player state；在协议 characterization 通过后连接 adapter。禁止先改 packet 或让接收器直接写 C04/C06/C08 authority。

依赖：C01-C13 的 authority snapshots -> C14 projection；C15 stream cursor -> packet adapter；packet reader -> validated command/definition boundary；network send 是唯一外部副作用。C14 不拥有 life、AI、Buff、damage tracker 或 persistence source。

验证记录：需要覆盖 packet 23 的 full/delta/spawn、netAlways、spam limit/ticks、boss budget、netStream、per-player state、disconnect/reconnect、client/server role、packet duplicate/out-of-order 和 adapter 单向性。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C15。

## 26. Checkpoint C15：NPC network sync cursor（已记录）

边界结论保持 status: proposed。skippedSyncs 和 streamCounter 属于 C14 复制协议中的 per-player/per-connection cursor，不属于 NPC 的 AI、战斗或持久化 authority。

候选类型为 PlayerNpcSyncCursorAdapter。它只保存或计算连接级同步游标，并为 packet adapter 提供不可变读取；收到网络数据后只能进入已校验的同步 command，不能直接改写 cursor 以外的 NPC 状态。cursor 的作用域必须包含 connection、player slot、NPC slot 或等价 generation，避免 slot reuse 把旧 cursor 应用到新 NPC。

迁移顺序：确认 PlayerNetSyncState 的创建、更新、跳过同步和释放调用点；定义 stream cursor snapshot；加入 disconnect/reconnect/world reset 清理；核对 C14 packet 23 的 full/delta 选择和顺序；最后才切换旧 packet path。

依赖：C14 replication budget/stream -> C15 cursor；connection lifecycle -> cursor reset；authority snapshot -> packet selection；C15 不向 C04/C06/C08 写回。

验证记录：需要覆盖 skippedSyncs 的饱和/清零、streamCounter 溢出、full sync 重置、断线重连、player/NPC slot reuse、重复/乱序 packet 和多连接隔离。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C16。

## 27. Checkpoint C16：NPC damage definition registry（已记录）

边界结论保持 status: proposed。C16 的四个成员是内容定义和查询注册表，不是 NPC entity runtime：

- NpcDamageDefinitionCatalog：CustomDefinition.NPCTypes 和 Name 是自定义 boss/伤害命令的不可变定义输入。LocalizedText 由 localization boundary 提供，不能在 tracker 内部临时生成全局 mutable name。
- BossTypeLookup：CustomBossDefinitions 和 BossTypeForMob 是由内容加载阶段构建的查找表候选。加载完成后应暴露只读快照；更新需通过明确的 catalog version/replace command。

迁移顺序：盘点 content load、mod/content reload、world/session reset 的注册表 writers；定义 catalog snapshot 和 version；验证 NPCTypes 的去重/排序和 BossTypeForMob 的无效索引策略；再让 C17 tracker 只读查询 catalog。

依赖：definition/content load -> C16 catalog；C04 type/netID、C05 boss/reward -> BossTypeLookup；C17 tracker -> definition query；C18 message projection -> LocalizedText。C16 不拥有 NPC slot、health、damage credit 或 packet state。

验证记录：需要覆盖定义加载确定性、重复 type、缺失/越界映射、localized name 版本、reload/replace、world reset 和 tracker 在 catalog 变化期间的快照一致性。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C17。

## 28. Checkpoint C17：NPC damage runtime tracking（已记录）

边界结论保持 status: proposed。C17 的 12 个成员按服务级 registry、每目标 tracker 和只读度量拆分：

- DamageTrackerService registry：_activeTrackers、_recentFinishedTrackers、MAX_RECENT_TRACKERS、EXTRA_RECENT_TRACKER_EXPIRY_TIME 由服务 owner 管理，范围应包含 world/session，不能挂到 NPC entity。
- NpcDamageEncounterTracker：_list、_worldCredit、_lastAttacker 是单一 encounter 的 credit/runtime state。Hit、heal、death 和 reset 通过显式事件进入，不能让 loot/UI 直接修改。
- TrackerClockState：_ticks 和 _lastHitTime 由 IWorldTickClock 驱动的 tracker owner 维护。不得隐式读取 Main.tick；recent expiry 必须明确包含边界条件。
- TrackerQuery：IsEmpty、Duration、TimeSinceLastHit 是只读派生查询。Duration 的语义必须从源码确认是 encounter duration 还是 last-hit time，不能按属性名猜测。

迁移顺序：先记录 NPCDamageTracker 的 Start/Update/Stop/Reset/AddDamage/BossKilled 生命周期；定义 clock port 和 hit/death events；把 active/recent list 置于 service；把单 tracker 的 credit 暴露为 immutable snapshot；最后才连接 C18 query。C17 不直接触发 loot、网络、UI 或 localization。

依赖：C16 definition catalog -> tracker classification；C05/C06 committed hit/death -> service events；clock -> update/expiry；C18 query -> tracker snapshot；C19 interaction 只能读取命令结果，不能写 tracker。

验证记录：需要覆盖 start/update/stop/reset、active/recent 转移、recent cap、expiry、tick 单调性、same-tick hits、last attacker/world credit、duplicate event、world/session isolation、服务失败重试和 query 纯度。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C18。

## 29. Checkpoint C18：NPC damage credit query 和 message projection（已记录）

边界结论保持 status: proposed。C18 的七个成员是 C17 tracker 快照到 UI/命令消息的查询与投影边界：

- DamageCreditQuery：CreditEntry.Damage 和 PlayerCreditEntry.PlayerName 只从 tracker snapshot 读取。现有 Damage setter 的 writer 必须在实现前闭合；Query 不允许为了显示而修改 credit。
- DamageCreditMessageProjection：CreditEntry/PlayerCreditEntry/WorldCreditEntry 的 Name、NPCDamageTracker 的 Name 和 KillTimeMessage 通过 localization/network message adapter 生成。NetworkText 和 LocalizedText 只能作为输出值，不成为 NPC authority。
- PlayerName 是不可信输入边界，必须明确空值、长度、编码和 session 作用域；WorldCreditName 走固定 localization key。

迁移顺序：建立 immutable DamageCreditSnapshot；确认 C17 tracker 的排序、并列和 kill-time 语义；定义纯 Name/KillTime projection；为 UI/command 输出加 adapter；最后才移除旧的可变 credit 读路径。

依赖：C17 tracker -> C18 query；C16 catalog/localization -> message projection；C06 death event -> kill-time message；C19 interaction/UI 只消费输出。C18 不写 tracker、health、network cursor 或 NPC entity。

验证记录：需要覆盖玩家/世界 credit、Damage 汇总、并列排序、空名/特殊字符、localized key、NetworkText 输出、kill time、tracker expiry 后查询、重复读取无副作用和权限/session 隔离。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；下一 checkpoint 为 C19。

## 30. Checkpoint C19：NPC interaction、commerce 和 UI adapter（已记录）

边界结论保持 status: proposed。C19 的 11 个成员拆成命令 payload、注册表、查询和外部适配器：

- NpcInteractionCommand：OpenShop 的 _shopIndex、_npcType、_customTextKey 是短生命周期命令参数，不是 NPC entity component。命令必须校验 shop、NPC type、custom text key 的来源和作用域。
- ShopRegistrationRegistry：NPCInteractions.All 是 interaction/shop 行为注册表候选，由 content/session 初始化，提供只读注册快照；不能被一个 NPC 实体持有或由 UI 直接追加。
- NpcInteractionQuery：NPCInteraction.ShowExcalmation、TalkNPCType 及三个具体 action 的 ShowExcalmation 是纯资格/提示查询。它只读取 progression、quest、local NPC snapshot 和注册表。
- InteractionAdapter：LocalPlayer、TalkNPC 是 Main/UI/session 的适配器输入。adapter 负责将 local player/talk NPC 解析成已校验 snapshot，再发 NpcInteractionCommand；核心 interaction owner 不直接访问 Main。

迁移顺序：先冻结 interaction registry 初始化和重复注册规则；定义 LocalPlayer/TalkNPC snapshot；将 ShowExcalmation 和 TalkNPCType 转成纯 Query；建立 OpenShop command validation；最后连接 UI/commerce adapter。C19 不拥有 NPC active、shop inventory、quest progression、health 或 damage credit。

依赖：C01 interaction snapshot、C03 identity、C16 definitions -> interaction query；player/progression/quest facts -> query；query -> command validation；command -> shop/commerce adapter；C14 只对需要的交互输出做网络投影。C19 的 UI 和 commerce I/O 均在 adapter。

验证记录：需要覆盖无 local player、talkNPC=-1、无效 NPC type、shop index 越界、custom text key、重复注册、quest/progression 变化、ShowExcalmation 纯度、OpenShop 重复命令、client/server role 和 adapter 失败重试。本 checkpoint 仍未执行实现、编译、运行时、网络或持久化验证；P12 的设计和执行计划已记录完毕。

## 12. Checkpoint log

| checkpoint | 保存时间 UTC | 记录 | executionStatus | implementationStatus | verificationStatus |
|---|---|---|---|---|---|
| C01 | 2026-09-11T13:42:01.1139271Z | saved; identity/interaction/presentation remains proposed | planned | not-started | not-run |
| C01-implementation | 2026-09-12T07:23:48.1035466Z | saved; three C01 state components implemented, unverified | in-progress | partial | not-run |
| C02-implementation | 2026-09-12T07:28:18.4486428Z | saved; NpcMovementHistoryComponent implemented, remaining C02 boundaries blocked or pending | in-progress | partial | not-run |
## 31. Checkpoint state register

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
