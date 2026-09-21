# Version4 非权威组件第二轮设计：P11 NPC、城镇与图鉴

partitionId: P11
sessionId: c822e256a5e34730bb0ab0ad5a6a094b
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\11-npc-town-bestiary.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-component-execution.md
designStatus: proposed
executionStatus: completed
implementationStatus: completed
verificationStatus: independently-verified
completedComponents:
- SharedConditionalDialogueSupport
- SharedTownRoomState
- SharedNpcPersonalityCatalog
- BestiaryCatalogAndEntries
- BestiaryUnlockTracking
- BestiaryFiltersAndSorting
- SharedBestiaryInfoElementState
- SharedBestiaryCollectionProviderState
currentComponent: verification-and-documentation
pendingComponents: []
savedButUnverifiedComponents: []
lastCheckpointUtc: 2026-09-12T08:37:48.2712187Z
previousSessionId: de06dd0a16934d8c92ff67a0130cff30
handoffId: p11-npc-town-bestiary-handoff-20260912-02
handoffStatus: completed
src2CodeModified: yes
src2SourceRoot: D:\TRbackup\NLTX\src2\NpcTownBestiary
src2CSharpFileCount: 106
productionSrcModified: no
evidence-gap:
- 第一轮 P11 public-decomposition 研究报告不存在：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-public-decomposition.md；本文件不能把它当作已完成研究输入。
- Version4 的 FreeCakeDialogue 消费实现、LucyAxeMessage 初始化和 Lucy 冷却写入者未闭合；条件对话检查点保留 partial/evidence-gap。
- TownRoomManager 的持久 key 是 NPC type，而当前 NLTX 草稿同时出现 NPC type、PersistentEntityId 和实体关系状态；最终 key mode、实体 owner 和存档迁移未裁决。
- PersonalityDatabase 的内容填充、BiomePreferenceListTrait 枚举行为和 TownNPCProfiles 的第三方/资源 profile 在 Version4 副本中不形成可迁移的运行时闭合实现。
- Version4 的 Bestiary NPC populator 被注释，BestiaryEntry 工厂和完整 UI provider 行为不闭合；ItemDropDatabase 合并、InfoElement UI 和统计刷新只能提出 adapter seam。
- NetBestiaryModule.Deserialize 为空；kill/sight/chat 的网络接收、端别 authority 和 snapshot 协议未闭合。
- WorldFile、UI、音频、网络、时钟、NPC/Player 实体、NPC net ID、persistent ID、Bestiary credit ID 和跨分区调度的最终 owner 统一标记 crossSubsystemOwner: integration-review。
- src2 已保存上述 8 个组件范围的候选实现；最新 focused verifier 已覆盖 8 个场景并通过，但该证据只证明 src2 候选实现的局部契约，不证明当前生产运行时行为等价、网络/存档闭合或跨分区 owner 已裁决。
blocking-decision:
- TownRoomManager 的 NPC type key 是否保留为权威 key，还是迁移到 PersistentEntityId/实例 key；以及 TownHousingRegistryComponent、TownHousingRegistry、NpcHousingAssignmentComponent、TownHousingRelationComponent 的唯一写入 owner。
- Personality catalog 与 ContentPresentationIndex/NpcDefinitionCatalog/TownNPCProfiles 的边界，以及 HelperInfo 中 Player/NPC 实体引用的查询端口。
- Bestiary entry 的 NPC population owner、ItemDropDatabase merge owner、filter/sort registration owner 和所有 catalog 的内容初始化顺序。
- Bestiary kill/sight/chat 的世界 authority、存档版本策略、player-join 同步策略、网络包接收实现和 Bestiary credit ID/NPC net ID 映射。
- InfoElement/UICollection 的 presentation snapshot owner、NPC stats refresh source、UI provider 兼容策略和跨分区 System 调度顺序。
- 条件对话消费提交 owner、LucyAxeMessage 冷却 authority/复制策略，以及所有跨分区 ID、snapshot、network payload 和顺序的 integration-review 裁决。
- SharedTownRoomState 的最终 key mode 和 assignment projection owner 仍未裁决；当前 src2 实现只提供 NPC type key 的未验证候选。
- BestiaryUnlockTracking 的持久化版本、网络 Deserialize、player-join authority 和 Bestiary credit ID/NpcNetId 映射仍未验证。

## 1. 设计声明、范围和排除项

本文件是基于 Version4 源码库存、当前 NLTX 只读证据和本分区输入报告形成的 proposed 组件设计。所有 Component、System、Query、Command、Adapter、Projection、接口、目录和文件路径仍是 proposed；对应的候选实现已保存到 `D:\TRbackup\NLTX\src2\NpcTownBestiary` 并通过本分区 focused verifier。本文件不是生产 ECS 迁移、行为等价证明、API 兼容证明、网络/存档闭合证明，也不代表当前 NLTX 已经具备这些能力。

本分区覆盖 8 个叶子范围和 93 条成员：

| 叶子范围 | proposed 边界 |
| --- | --- |
| SharedConditionalDialogueSupport | 条件对话定义、条件查询、消费和 Lucy 瞬态冷却 |
| SharedTownRoomState | 城镇房间登记、分配/驱逐命令和房屋查询 |
| SharedNpcPersonalityCatalog | NPC 个性定义、生物群系偏好、城镇档案和购物查询输入 |
| BestiaryCatalogAndEntries | 图鉴条目、NPC 索引、注册目录和掉落合并入口 |
| BestiaryUnlockTracking | 击杀、接近、聊天三类持久发现事实及派生进度 |
| BestiaryFiltersAndSorting | 纯筛选、排序和选项元数据查询 |
| SharedBestiaryInfoElementState | 图鉴名称、掉落、统计、肖像、稀有度等只读展示事实 |
| SharedBestiaryCollectionProviderState | 根据解锁事实生成客户端 UICollection snapshot |

排除范围包括 NPC 战斗/死亡最终 owner、ItemDropDatabase 规则最终 owner、网络传输协议最终 owner、WorldFile 总调度最终 owner、UI 渲染框架、资源加载、Achievement 副作用和跨分区 System 调度最终裁决。这些边界只提出交接候选，并写 `crossSubsystemOwner: integration-review`。

## 2. 证据来源和状态

### 2.1 Version4 事实

| 证据 | 关键锚点 | 已确认事实 | 状态 |
| --- | --- | --- | --- |
| D:\TRbackup\Version4\Terraria.GameContent\ConditionalDialogue.cs | 11-42, 55-93 | ItemGroups 在内容建立后补充 Whips/Mounts；registry 按 NPCID 保存条件对话；构造函数默认 ShowIndicator=true；Init 注册 FreeCakeDialogue 到 NPC 208。 | confirmed；消息消费实现 partial |
| D:\TRbackup\Version4\Terraria.GameContent\LucyAxeMessage.cs | 10-37 | MessageSource 有 7 个来源槽；_messageCooldownsByType 每帧递减。 | confirmed；触发/写入者 partial |
| D:\TRbackup\Version4\Terraria\Main.cs | 3402, 3442, 3350-3359, 11351, 11471, 13625-13643 | ConditionalDialogue.Init 执行；LucyAxeMessage.Initialize 被注释；Bestiary database、sorting index、tracker 被初始化；每帧扫描 sight 并计算 progress。 | confirmed；若干行为 partial |
| D:\TRbackup\Version4\Terraria.GameContent\TownRoomManager.cs | 11-177 | 房间列表和 NPC 类型快速索引由 TownRoomManager 持有；SetRoom/KickOut 写入，Save/Load/Clear 管理生命周期；HasRoom 和 CanNPCsLiveWithEachOther 为查询。 | confirmed |
| D:\TRbackup\Version4\Terraria\WorldGen.cs | 4558-4612, 6407-6409 | moveRoom/kickOut 写房间登记；TownManager 查询出生资格；清理世界时重新建立并清空 TownManager。 | confirmed |
| D:\TRbackup\Version4\Terraria.GameContent.Personalities\BiomePreferenceListTrait.cs | 5-46 | BiomePreference 保存 Affection 和 AShoppingBiome；trait 持有 preference list，Add 写入。 | confirmed；枚举/购物应用 partial |
| D:\TRbackup\Version4\Terraria.GameContent.Personalities\HelperInfo.cs | 5-16 | HelperInfo 以 Player、NPC、附近 NPC 列表和 type 位图提供 transient 购物评估输入。 | confirmed；跨实体 owner unresolved |
| D:\TRbackup\Version4\Terraria.GameContent.Personalities\PersonalityDatabase.cs | 5-26 | personality profile 按 NPC id 注册 trait；缺省 trash entry 存在但读取路径不闭合。 | confirmed；fallback partial |
| D:\TRbackup\Version4\Terraria.GameContent.Personalities\PersonalityDatabasePopulator.cs | 5-135 | Populate 建立 biome preference；_currentDatabase 只是填充上下文。 | confirmed；内容生命周期 partial |
| D:\TRbackup\Version4\Terraria.GameContent\TownNPCProfiles.cs | 7-181 | Town NPC profile map 按 NPC id 保存资源路径、头像变体、shimmer/party profile；Instance 是静态入口。 | confirmed；资源 owner partial |
| D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryDatabase.cs | 11-107 | entries、filters、sortSteps 和 byNpcId 是目录状态；Register 建立 NPC 网络 ID 索引；Merge 从 ItemDropDatabase 把 DropRateInfo 写入条目。 | confirmed；NPC population partial |
| D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryEntry.cs | 7-24 | entry 持有 UIInfoProvider 和 Info 列表；工厂和 AddTags 在该副本中被注释。 | confirmed；entry construction partial |
| D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlocksTracker.cs | 5-63 | Kills、Sights、Chats 三个 tracker 共同承担世界持久化、校验、重置和玩家加入同步。 | confirmed |
| D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCKillsTracker.cs | 10-116 | 按 Bestiary credit ID 计数，封顶 999999999，Save/Load/ValidateWorld/Reset/OnPlayerJoining 闭合到调用表面。 | confirmed；网络接收 partial |
| D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCWasNearPlayerTracker.cs | 11-151 | 按 Bestiary credit ID 保存接近事实；扫描玩家图鉴 hitbox 与 NPC hitbox；首次发现广播 sight。 | confirmed；扫描 owner partial |
| D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCWasChatWithTracker.cs | 10-110 | 按 Bestiary credit ID 保存聊天事实；首次加入集合才广播 chat。 | confirmed；聊天 owner partial |
| D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlockProgressReport.cs | 5-22 | EntriesTotal、CompletionAmountTotal 是报告字段；EntriesTotal 为 0 时 CompletionPercent 返回 1。 | confirmed；计算 owner proposed |
| D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetBestiaryModule.cs | 7-44 | kill/sight/chat 包分别使用类型字节 0/1/2 和 NPC net ID；Version4 Deserialize 是空实现。 | confirmed；客户端接收 evidence-gap |
| D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 3278-3281, 3476-3511 | Bestiary tracker 有 ValidateWorld、Save、Load；TownManager 有独立 Save/Load。 | confirmed |
| D:\TRbackup\Version4\Terraria\NPC.cs | 42253-42261, 65323-65403, 79655-79660 | 聊天开始和有效击杀触发图鉴事实；GetBestiaryCreditId 从 ContentSamples 按 NPC net ID 得到持久 credit ID。 | confirmed；跨域 owner unresolved |
| D:\TRbackup\Version4\Terraria\Player.cs | 2542-2550, 3378-3394 | 玩家提供扩大后的图鉴检测 hitbox；SetTalkNPC 触发聊天发现并读取购物设置。 | confirmed |

Version4 是行为和调用链的主证据。完整参考副本和 tModLoader `v2026.07` 本地文档只用于补充公开边界，不得反推 Version4 私有实现已经存在。`NetBestiaryModule.Deserialize`、BestiaryEntry 工厂、NPC population 和部分 UI 实现仍保持 `evidence-gap`。

### 2.2 当前 NLTX 状态

已读取的相关 NLTX 文件包括：

- `src/Town/NpcHousingAssignmentComponent.cs`：已有只读房屋分配状态草稿，包含 Status、HomeTile、SearchCooldownTicks、DespawnWhenHomeless、AssignmentRevision，但没有对应写入 System。
- `src/Town/TownHousingRelationComponent.cs`：已有 proposed 关系状态草稿，包含 homeless/home/revision 不变量，证据状态 partial。
- `src/Town/TownRoomTilePoint.cs`：已有值对象草稿。
- `src/WorldSession/WorldGeneration/TownHousingRegistryComponent.cs` 和 `TownHousingRegistry.cs`：已有两套相近的登记草稿；一个以只读属性和 `TownHousingKeyMode` 表示状态，另一个以公共可变字段表示状态，不能作为双写权威。
- `src/WorldProgressionAndUnlocks/BestiaryDiscoveryCache.cs`：只有 PlayerBestiaryBounds 和 SeenNpcNetworkIds 的瞬态缓存，没有击杀/接近/聊天持久事实。
- `src/Content/ContentIdentityCatalog.cs`：已有 NPC net ID、persistent ID 和 Bestiary credit ID 反向索引查询，可作为 proposed identity adapter 输入，但不能替代最终 owner 裁决。
- `src/Content/ContentPresentationIndex.cs`、`NpcPresentationDefinition.cs`、`NpcTownDefinition.cs`：已有部分 NPC 图鉴排序、稀有度和城镇表现元数据，没有图鉴目录、解锁事实、InfoElement 或 UICollection provider。
- `docs/migration/ledgers/Version4子系统索引.json` 将 NpcAndTownSimulation 标记为 partial、WorldProgressionAndUnlocks 标记为 missing、SharedRuntimeMechanisms 标记为 excluded；实际源码线索比索引更新，状态仍只能写 partial/evidence-gap。

当前 NLTX 没有被本任务修改，也没有证据表明上述草稿已经组成可执行的 NPC、房屋或图鉴运行时。

### 2.3 交叉证据

- tModLoader 本地镜像 `D:\TRbackup\tModLoader-api-docs-stable\index.html`，版本 `v2026.07`；BestiaryDatabase、BestiaryEntry、BestiaryUICollectionInfo 和 BestiaryUnlocksTracker 页面用于确认公开目录、entry Info、UnlockState、Save/Load/Reset/OnPlayerJoining 边界，不用于确认 Version4 私有顺序。
- `C:\Users\shan\Downloads\ECS\space-station-14-master` 中 `StationCargoOrderDatabaseComponent` 和 `CargoSystem` 只用于参考内聚状态与 System/Query 的组织粒度，不复制命名或领域语义。
- 已读取 `references/ecs-evidence-protocol.md` 和 `references/tmodloader-documentation-retrieval.md`；证据优先级以 Version4 源码和调用者为准。

## 3. 组件边界总览

以下全部是 proposed，不代表已创建：

| proposed 模块 | 角色 | 权威/派生 | 主要输入 | 主要输出 |
| --- | --- | --- | --- | --- |
| ConditionalDialogueCatalog、ConditionalDialogueQuery、ConditionalDialogueConsumeSystem | definition/catalog、Query、System/Command | 条件定义和消费状态；Query 纯读 | NPC/Player read view、ItemGroup adapter、consume command | eligibility、消费事件、indicator projection |
| LucyAxeCooldownComponent、LucyAxeCooldownSystem | transient Component、System | 瞬态 tick 状态 | 显式 WorldTickPort、trigger command | cooldown transition |
| TownRoomRegistryComponent | world capability Component | 房屋登记权威候选 | resident key、room tile、revision | assigned-room view |
| TownRoomAssignmentSystem、AssignTownRoomCommand、EvictTownResidentCommand | System、Command | 房间/驱逐状态转换 | WorldGen/housing command | registry mutation、assignment event |
| TownRoomQuery、TownHousingCompatibilityQuery、TownRoomAvailabilityProjection | Query、Projection | 纯查询/派生索引 | registry、NPC housing view | room lookup、compatibility、has-room view |
| TownRoomPersistenceAdapter、TownRoomMutationGateAdapter | Adapter/Port | 外部 I/O 和同步边界 | WorldFile、entity creation gate | load/save and commit boundary |
| NpcPersonalityCatalog、NpcPersonalityCatalogBuilder | catalog、build System | 内容定义目录 | NPC type、biome preference、trait definitions | immutable profile view |
| NpcPersonalityEvaluationContext、NpcPersonalityQuery | transient read context、Query | 纯派生 | Player/NPC/nearby read ports、biome query | shopping/personality result |
| TownNpcProfileCatalog、TownNpcProfileAssetAdapter | presentation catalog、Adapter | 资源/头像表现定义 | NPC type、asset path、variant IDs | profile view |
| BestiaryCatalogComponent、BestiaryEntryDefinition、BestiaryNpcEntryIndex | catalog/definition/derived index | 内容目录权威候选和 NPC 索引 | entry info、NPC net ID | entry lookup |
| BestiaryCatalogRegistrationSystem、BestiaryDropMergeAdapter | System、Adapter | 内容注册和外部掉落读取 | content setup、ItemDropDatabase | registered entry/filter/sort and drop facts |
| BestiaryKillCountStateComponent、BestiarySightDiscoveryStateComponent、BestiaryChatDiscoveryStateComponent | persistent Components | 三类世界发现事实 | kill/sight/chat commands/events | unlock read view |
| BestiaryDiscoverySystem、BestiarySightScanSystem、BestiaryUnlockProgressQuery | Systems、Query | 事实转换和派生进度 | NPC death/chat/sight inputs、catalog | state transition、progress report |
| BestiaryUnlockPersistenceAdapter、BestiaryPlayerJoinSyncAdapter、BestiaryDiscoveryNetworkAdapter | Adapters | 存档、加入同步和网络副作用 | tracker snapshot、discovery event | WorldFile/network request |
| BestiaryFilterCatalog、BestiaryFilterQuery、BestiarySortCatalog、BestiarySortQuery | definition/catalog、Query | 纯筛选排序 | entry/presentation/unlock snapshot | ordered/filtered entry keys |
| BestiaryInfoElementState、BestiaryStatsRefreshAdapter、BestiaryInfoElementProjection | presentation state、Adapter、Projection | 只读展示事实 | catalog、drop/stats/unlock ports | UI-neutral info snapshot |
| BestiaryCollectionProviderDefinition、BestiaryCollectionUnlockQuery、BestiaryUICollectionProjection | definition、Query、Projection | 客户端派生 snapshot | persistent ID、kill/sight/chat view、policy | OwnerEntryKey、UnlockState、display metadata |

跨分区的 `NpcEntityId`、`PlayerEntityId`、NPC type、`NpcNetId`、`PersistentEntityId`、`BestiaryCreditId`、Item/Recipe group view、WorldTick、WorldFile snapshot、network payload、UI request 和 System order 均写 `crossSubsystemOwner: integration-review`。文件顺序、目录顺序和静态初始化顺序不能表达运行时先后。

## 4. 组件检查点和成员逐条归属

### 4.1 SharedConditionalDialogueSupport

#### 4.1.1 成员逐条归属

| 来源序号 | Version4 成员 | proposed 归属 | 状态/边界 |
| ---: | --- | --- | --- |
| 2352 | `ConditionalDialogue.ItemGroups.Ore` | proposed `ConditionalDialogueItemGroupAdapter` 输出的静态 ItemGroup view | 内容建立阶段；RecipeGroup owner 为 `crossSubsystemOwner: integration-review` |
| 2353 | `ConditionalDialogue.ItemGroups.Bars` | proposed `ConditionalDialogueItemGroupAdapter` 输出的静态 ItemGroup view | 同上 |
| 2354 | `ConditionalDialogue.ItemGroups.Anvils` | proposed `ConditionalDialogueItemGroupAdapter` 输出的静态 ItemGroup view | 同上 |
| 2355 | `ConditionalDialogue.ItemGroups.Whips` | proposed `ConditionalDialogueItemGroupAdapter` 动态定义项 | PostSetupContent 快照；ContentSamples owner unresolved |
| 2356 | `ConditionalDialogue.ItemGroups.Mounts` | proposed `ConditionalDialogueItemGroupAdapter` 动态定义项 | PostSetupContent 快照；mount type owner unresolved |
| 2357 | `ConditionalDialogue._registry` | proposed `ConditionalDialogueCatalog` registry state | 唯一 registration writer；清理生命周期 partial |
| 2358 | `ConditionalDialogue.ConditionsMet` | proposed `ConditionalDialogueDefinition.ConditionPort` | Query 只读调用；不能直接持有可变 NPC 引用 |
| 2456 | `LucyAxeMessage._messageCooldownsByType` | proposed `LucyAxeCooldownComponent.RemainingBySource` | 每帧递减；authority、触发写入者和端别 partial |
| 3849 | `ConditionalDialogue.ShowIndicator` | proposed `ConditionalDialogueIndicatorProjection.ShowIndicator` | 派生展示事实，不是解锁 authority |

#### 4.1.2 边界、不变量和顺序

- proposed `ConditionalDialogueCatalog` 只保存按 NPC type 的不可变 definition view；重复注册、空 registry 和 content rebuild 必须由 registration System 显式处理。
- proposed `ConditionalDialogueQuery` 接受 `NpcReadView`、显式 `ConditionPort` 和 ItemGroup view，返回 eligibility；不得清理消息、写 NPC/Player、修改 catalog 或发送网络包。
- proposed `ConsumeConditionalDialogueCommand` 携带 NPC entity key、definition key、触发来源和 expected revision；这些 ID 的最终类型是 `crossSubsystemOwner: integration-review`。
- proposed `ConditionalDialogueConsumeSystem` 是消费提交唯一 writer，输出 proposed event/projection request。`GetChatAndClearCondition` 的 clear 事务仍是 blocking-decision，不能在实现计划中简化为普通 getter。
- proposed `LucyAxeCooldownComponent/System` 只保存和递减 source cooldown；时钟通过 proposed `WorldTickPort` 注入，UI、音频和网络只能消费 projection/event。
- 顺序候选：ItemGroup adapter -> registration -> eligibility Query -> consume System -> indicator Projection；Lucy cooldown 在显式 tick 阶段运行。最终顺序 `crossSubsystemOwner: integration-review`。

#### 4.1.3 focused verifier 计划

注册/清理、重复注册、Query 纯度、消费成功/条件失效/revision 冲突/幂等、Lucy cooldown 零下限和多 source 独立性、静态/动态 ItemGroup 快照、indicator 单向 projection。未运行。

### 4.2 SharedTownRoomState

#### 4.2.1 成员逐条归属

| 来源序号 | Version4 成员 | proposed 归属 | 状态/边界 |
| ---: | --- | --- | --- |
| 2574 | `TownRoomManager.EntityCreationLock` | proposed `TownRoomMutationGateAdapter`/`TownRoomAssignmentSystem` 的提交同步 seam | 是并发边界，不进入 Component 领域字段；gate owner 为 `crossSubsystemOwner: integration-review` |
| 2575 | `TownRoomManager._roomLocationPairs` | proposed `TownRoomRegistryComponent.AssignedRoomsByResidentKey` | Version4 以 NPC type + Point 登记；最终 type/instance key mode 未裁决 |
| 2576 | `TownRoomManager._hasRoom` | proposed `TownRoomAvailabilityQuery` 和 `TownRoomAvailabilityProjection` 的派生索引 | 不作为第二份 authority；由 registry mutation/rebuild 失效 |

#### 4.2.2 状态所有权和操作边界

- proposed `TownRoomRegistryComponent` 是唯一房屋登记 authority 候选，最小字段为 `AssignedRoomsByResidentKey`、`Revision` 和经过裁决的 key mode。`TownRoomTilePoint` 只作值对象，不代表实体身份。
- proposed `NpcHousingAssignmentComponent` 只表达 NPC 当前的 assignment/status；现有 `TownHousingRelationComponent` 与其共享 home、homeless、revision 语义，不能双写。若保留，必须降为 compatibility projection 或由 integration-review 选择唯一 owner。
- proposed `TownRoomAssignmentSystem` 处理 `AssignTownRoomCommand` 和 `EvictTownResidentCommand`；前者应复现 SetRoom 的按 resident key 替换语义，后者应复现 KickOut 的删除和 homeless/search timeout 交接。命令包含 expected revision，避免重试覆盖。
- proposed `TownRoomQuery` 提供 HasRoom/room point/occupants；proposed `TownHousingCompatibilityQuery` 读取 NPC housing category，不能把 `CanNPCsLiveWithEachOther` 的规则写入 registry。
- proposed `TownRoomAvailabilityProjection` 只输出 has-room 快速视图；所有 projection cache 都由 registry revision 失效，不可被查询方直接写入。
- proposed `TownRoomPersistenceAdapter` 隔离 WorldFile 的 BinaryReader/BinaryWriter；需保持 Version4 Save/Load 的 entry count、NPC type、X/Y 顺序，迁移到其他 key 前必须有版本策略。`Clear` 在 world reset 时由显式 lifecycle command 调用。
- `WorldGen.moveRoom`、`WorldGen.kickOut` 和 spawn 查询是外部调用者；它们通过 command/query port 交接，不能直接访问 Component 字典。房屋变更和 NPC 实体生命周期的顺序为 `crossSubsystemOwner: integration-review`。

#### 4.2.3 NLTX 重复状态决策

当前 `src/WorldSession/WorldGeneration/TownHousingRegistryComponent.cs` 和 `TownHousingRegistry.cs` 都保存 assigned/homeless/revision，且 `src/Town/NpcHousingAssignmentComponent.cs` 与 `TownHousingRelationComponent.cs` 又保存每 NPC 的 home/homeless/revision。它们是重复草稿，不是已实现能力。提议以 capability-first 的 proposed `TownRoomRegistryComponent` 作为 world reverse index，以一个经 integration-review 选定的 NPC assignment component 作为正向投影；旧 `TownHousingRegistry` 只能作为迁移 compatibility adapter，禁止与新 registry 双写。

#### 4.2.4 focused verifier 计划

SetRoom 同 key 替换、KickOut 删除、空 registry、revision 冲突/重试、has-room projection 重建、NPC type 与 instance key fixtures、房屋兼容 Query 纯度、WorldFile Save/Load/Clear 字节顺序、旧双登记草稿单写入 owner 和 move/kick command 顺序。未运行。

### 4.3 SharedNpcPersonalityCatalog

#### 4.3.1 成员逐条归属

| 来源序号 | Version4 成员 | proposed 归属 | 状态/边界 |
| ---: | --- | --- | --- |
| 2161 | `BiomePreferenceListTrait.BiomePreference.Affection` | proposed `NpcBiomePreference.Affection` | 内容 definition；枚举语义需兼容验证 |
| 2162 | `BiomePreferenceListTrait.BiomePreference.Biome` | proposed `NpcBiomePreference.BiomeKey`/`ShoppingBiomeDefinition` | `AShoppingBiome` 第三方类型由 adapter 隔离 |
| 2163 | `BiomePreferenceListTrait._preferences` | proposed `NpcPersonalityDefinition.BiomePreferences` | 只读有序 view；注册时写入 |
| 2164 | `HelperInfo.player` | proposed `PersonalityEvaluationContext.PlayerEntityId` 输入 | transient query input；Player entity owner 为 `crossSubsystemOwner: integration-review` |
| 2165 | `HelperInfo.npc` | proposed `PersonalityEvaluationContext.NpcEntityId` 输入 | transient query input；NPC entity owner 为 `crossSubsystemOwner: integration-review` |
| 2166 | `HelperInfo.NearbyNPCs` | proposed `PersonalityEvaluationContext.NearbyNpcReadViews` | 每次查询构建；不进入 catalog authority |
| 2167 | `HelperInfo.nearbyNPCsByType` | proposed `NearbyNpcTypeMembershipView` | 派生查询缓存；失效和 type ID owner unresolved |
| 2168 | `PersonalityDatabase._personalityProfiles` | proposed `NpcPersonalityCatalog.ProfilesByNpcType` | 内容目录 authority candidate；按 NPC type 注册 |
| 2169 | `PersonalityDatabase._trashEntry` | proposed `NpcPersonalityLookupResult.Missing` fallback | 不返回可变共享 sentinel；缺失语义需 verifier |
| 2170 | `PersonalityDatabasePopulator._currentDatabase` | proposed `NpcPersonalityCatalogBuilder` 的 build context | 只在内容建立阶段存在，不是运行时 Component |
| 2171 | `PersonalityProfile.ShopModifiers` | proposed `NpcPersonalityDefinition.ShopTraits` | 第三方 `IShopPersonalityTrait` 以 stable trait definition/adapter 表示 |
| 2567 | `TownNPCProfiles.DefaultNPCFileFolderPath` | proposed `TownNpcProfileAssetDefinition.DefaultRoot` | 表现资源路径，不进入购物 authority |
| 2568 | `TownNPCProfiles.ShimmeredNPCFileFolderPath` | proposed `TownNpcProfileAssetDefinition.ShimmeredRoot` | 资源 adapter；路径 owner partial |
| 2569 | `TownNPCProfiles.CatHeadIDs` | proposed `TownNpcVariantProfileDefinition.CatHeadIds` | 只读变体 metadata；资源/头像 ID 为 `crossSubsystemOwner: integration-review` |
| 2570 | `TownNPCProfiles.DogHeadIDs` | proposed `TownNpcVariantProfileDefinition.DogHeadIds` | 同上 |
| 2571 | `TownNPCProfiles.BunnyHeadIDs` | proposed `TownNpcVariantProfileDefinition.BunnyHeadIds` | 同上 |
| 2572 | `TownNPCProfiles._townNPCProfiles` | proposed `TownNpcProfileCatalog.ProfilesByNpcType` | presentation catalog；不与 NpcPersonalityCatalog 双写 profile |
| 2573 | `TownNPCProfiles.Instance` | proposed `NpcTownContentRegistrationPort` composition-root seam | 去除静态可变 singleton 的候选；初始化顺序 unresolved |
| 3828 | `AShoppingBiome.NameKey` | proposed `ShoppingBiomeDefinition.LocalizationKey` | 本地化 key 只读；Localization owner 为 `crossSubsystemOwner: integration-review` |

#### 4.3.2 状态所有权和查询边界

- proposed `NpcPersonalityCatalog` 只保存按 NPC type 的 profile definition；`NpcPersonalityCatalogBuilder` 负责一次内容填充，之后以只读 view 暴露。`_currentDatabase` 不迁移为持久或实体状态。
- proposed `BiomePreferenceListTrait` 的 `Affection`/`Biome` 改为稳定领域值；`AShoppingBiome.IsInBiome(Player)` 通过 `BiomeMembershipQuery` 端口调用，核心 Query 不持有 `Player` 对象。
- proposed `NpcPersonalityQuery` 接受 `PersonalityEvaluationContext`、profile view 和 biome/environment read port，纯计算购物价格/偏好输入；`NearbyNPCs` 和 `nearbyNPCsByType` 只在调用期间存在。
- proposed `TownNpcProfileCatalog` 专门保存资源路径、头像变体、shimmer/party 选择；它可以读取 `NpcPresentationDefinition`，但不得把 UI texture 或 third-party profile 对象写入 personality Component。
- `PersonalityDatabase._trashEntry` 的缺失返回值不能成为可变全局对象；建议返回显式 `Missing` result。缺失 profile 的购物行为是 blocking verifier，不可凭完整参考补回。
- 购物系统、NPC 实体解析、Player 查询、邻居扫描和资源加载均通过 adapter/query port 交接。实体 ID、type ID、localization key 和资源句柄的 owner 由 integration-review 裁决。

#### 4.3.3 focused verifier 计划

每个 NPC type 的 preference 注册顺序和重复注册、缺失 profile、Affection/biome 只读性、BiomeMembershipQuery 纯度、HelperInfo transient 生命周期、nearby type index 重建、Town NPC variant profile 映射、shimmer/默认资源路径和 singleton 初始化替换。未运行。

### 4.4 BestiaryCatalogAndEntries

#### 4.4.1 成员逐条归属

| 来源序号 | Version4 成员 | proposed 归属 | 状态/边界 |
| ---: | --- | --- | --- |
| 1357 | `BestiaryDatabase._entries` | proposed `BestiaryCatalogComponent.Entries`/`BestiaryEntryDefinition` | 内容建立后只读；entry population partial |
| 1358 | `BestiaryDatabase._filters` | proposed `BestiaryCatalogComponent.RegisteredFilterKeys` | 仅保存 registration order/ref；实际 filter 由 proposed filter catalog owner |
| 1359 | `BestiaryDatabase._sortSteps` | proposed `BestiaryCatalogComponent.RegisteredSortKeys` | 仅保存 registration order/ref；实际 sort 由 proposed sort catalog owner |
| 1360 | `BestiaryDatabase._byNpcId` | proposed `BestiaryNpcEntryIndex.ByNpcNetId` | 派生索引；NPC net ID 为 `crossSubsystemOwner: integration-review` |
| 1361 | `BestiaryDatabase._trashEntry` | proposed `BestiaryEntryLookupResult.NotFound` | 不迁移可变 sentinel；查询返回显式缺失结果 |
| 1362 | `BestiaryEntry.UIInfoProvider` | proposed `BestiaryEntryDefinition.CollectionProviderRef` | presentation provider port；不把 UI 对象放入 authority |
| 3718 | `BestiaryDatabase.Entries` | proposed `IBestiaryCatalogQuery.GetEntries()` read-only snapshot | 不能暴露可变 List |
| 3719 | `BestiaryEntry.Info` | proposed `BestiaryEntryDefinition.InfoElementDefinitions` | InfoElement definition list；UI projection 单向读取 |

#### 4.4.2 状态所有权和目录边界

- proposed `BestiaryCatalogRegistrationSystem` 在内容初始化阶段注册 entry、filter key 和 sort key，之后冻结 catalog。注册重复、NPC net ID 冲突和 entry 缺失必须显式失败或产生 evidence-gap。
- proposed `BestiaryNpcEntryIndex` 从 entry 的 NPC net ID info 派生，不反向修改 entry；NPC net ID、content identity 和 Bestiary credit ID 不可混用。
- proposed `BestiaryDropMergeAdapter` 读取 ItemDropDatabase，转换成稳定 `DropRateInfoView` 并追加到 entry definition。ItemDropDatabase、drop rule 和 drop calculation 的最终 owner 为 `crossSubsystemOwner: integration-review`。
- Version4 `BestiaryDatabase.Merge` 遍历 `-65` 至 `NPCID.Count`，找不到 entry 时跳过；该范围和缺失行为必须在兼容 verifier 中保留，不能由 tModLoader 文档推断替换。
- `UIInfoProvider` 只存 proposed provider reference/factory key；provider 计算的 UnlockState 属于 SharedBestiaryCollectionProviderState，不成为 catalog authority。
- proposed `BestiaryEntryLookupResult.NotFound` 代替共享 `_trashEntry`，避免一次查询 `Info.Clear()` 污染其他读者。

#### 4.4.3 focused verifier 计划

entry 注册顺序、NPC net ID 反向索引、重复 ID、NotFound 纯度、filter/sort registration 顺序、drop merge 的缺失 entry 行为、entry Info 追加顺序、filter/sort metadata、catalog/provider projection 和 collection unlock 计算已由 8 场景 focused verifier 覆盖；未覆盖项仍保留在 evidence-gap。

### 4.5 BestiaryUnlockTracking

#### 4.5.1 成员逐条归属

| 来源序号 | Version4 成员 | proposed 归属 | 状态/边界 |
| ---: | --- | --- | --- |
| 1365 | `BestiaryUnlockProgressReport.EntriesTotal` | proposed `BestiaryUnlockProgressQuery` 输出 `EntriesTotal` | 派生 snapshot，不是持久 authority |
| 1366 | `BestiaryUnlockProgressReport.CompletionAmountTotal` | proposed `BestiaryUnlockProgressQuery` 输出 `CompletionAmountTotal` | 派生值；基于 provider unlock state |
| 1367 | `BestiaryUnlocksTracker.Kills` | proposed `BestiaryKillCountStateComponent` | 持久击杀事实；aggregate coordinator 只编排生命周期 |
| 1368 | `BestiaryUnlocksTracker.Sights` | proposed `BestiarySightDiscoveryStateComponent` | 持久接近事实；扫描缓存分开 |
| 1369 | `BestiaryUnlocksTracker.Chats` | proposed `BestiaryChatDiscoveryStateComponent` | 持久聊天事实 |
| 1380 | `NPCKillsTracker._entryCreationLock` | proposed `BestiaryDiscoveryMutationGateAdapter` | 并发 seam，不进入 kill component |
| 1381 | `NPCKillsTracker.POSITIVE_KILL_COUNT_CAP` | proposed `BestiaryKillCountPolicy.PositiveCap` | 规则常量；计数写入 System 唯一使用 |
| 1382 | `NPCKillsTracker._killCountsByNpcId` | proposed `BestiaryKillCountStateComponent.CountByBestiaryCreditId` | 持久事实；key 是 Bestiary credit ID，不是 net ID |
| 1392 | `NPCWasChatWithTracker._entryCreationLock` | proposed `BestiaryDiscoveryMutationGateAdapter` | 与 chat writer 的提交边界，不是 chat state |
| 1393 | `NPCWasChatWithTracker._chattedWithPlayer` | proposed `BestiaryChatDiscoveryStateComponent.ChattedBestiaryCreditIds` | HashSet 持久事实 |
| 1394 | `NPCWasNearPlayerTracker._entryCreationLock` | proposed `BestiaryDiscoveryMutationGateAdapter` | 与 sight writer 的提交边界 |
| 1395 | `NPCWasNearPlayerTracker._wasNearPlayer` | proposed `BestiarySightDiscoveryStateComponent.SeenBestiaryCreditIds` | HashSet 持久事实 |
| 1396 | `NPCWasNearPlayerTracker._playerHitboxesForBestiary` | proposed `BestiarySightScanBuffer.PlayerBestiaryBounds` | 每帧 transient query buffer，不存档 |
| 1397 | `NPCWasNearPlayerTracker._wasSeenNearPlayerByNetId` | proposed `BestiarySightScanBuffer.SeenNpcNetIdsThisScan` | 每帧 dedupe/cache，不等同持久 sight state |
| 3720 | `BestiaryUnlockProgressReport.CompletionPercent` | proposed `BestiaryUnlockProgressQuery.CompletionPercent` | `EntriesTotal==0` 返回 1 的规则需保留 |

#### 4.5.2 三类事实和副作用边界

- proposed kill/sight/chat 三个 Component 分别持有单一事实；不能合并成一个含网络和 UI 的巨型 `BestiaryUnlocksTrackerComponent`。一个 proposed lifecycle coordinator 可以统一 Save/Load/Reset/Validate/OnPlayerJoining，但不复制三份集合。
- kill writer 接收来自 NPC 有效死亡边界的 event/command；sight writer 接收 `BestiarySightScanSystem` 的命中；chat writer 接收 Player/NPC 对话开始事件。NPC、Player 和 combat/death owner 是 `crossSubsystemOwner: integration-review`。
- `BestiaryCreditId` 是存档和事实 key；`NpcNetId` 是内容索引/网络包 key；`NpcEntityId`/`PersistentEntityId` 是运行时实体 key。三者必须通过 `ContentIdentityCatalog`/proposed adapter 显式转换。
- proposed `BestiaryUnlockPersistenceAdapter` 保持 Version4 Save/Load/ValidateWorld 的三段顺序和每个 tracker 的 count/string payload；BinaryReader/BinaryWriter 不进入 Component。
- proposed `BestiaryPlayerJoinSyncAdapter` 读取持久事实并按 resolved `NpcNetId` 发同步；proposed `BestiaryDiscoveryNetworkAdapter` 发 kill/sight/chat event。网络包类型、端别 authority、Deserialize 逻辑和 snapshot 版本为 `crossSubsystemOwner: integration-review`，当前为 evidence-gap。
- `BestiarySightScanBuffer` 每帧清空；`_wasSeenNearPlayerByNetId` 只避免同一扫描重复检查，不能被 Save/Load 或 progress Query 读取为发现事实。
- proposed `BestiaryUnlockProgressQuery` 从 catalog entry 和 collection provider view 计算总数/完成量；Query 不触发 Achievement、网络、存档或状态写入。

#### 4.5.3 focused verifier 计划

kill cap、同 credit ID 累加、sight/chat 首次发现幂等、scan buffer 清理和 net ID dedupe、persistent ID 映射、Save/Load/Validate 字节顺序、world reset、player join snapshot、kill/sight/chat 网络事件边界、progress 空目录返回 1 和 Query 纯度。未运行。

### 4.6 BestiaryFiltersAndSorting

#### 4.6.1 成员逐条归属

| 来源序号 | Version4 成员 | proposed 归属 | 状态/边界 |
| ---: | --- | --- | --- |
| 1374 | `Filters.ByInfoElement._element` | proposed `BestiaryInfoElementFilterDefinition.ElementKey`/`BestiaryInfoElementQueryPort` | 不保存第三方 mutable InfoElement；lookup port owner unresolved |
| 3721 | `Filters.BySearch.ForcedDisplay` | proposed `BestiaryFilterDefinition.ForcedDisplay` for Search | 纯 metadata，值为 true |
| 3722 | `Filters.ByUnlockState.ForcedDisplay` | proposed `BestiaryFilterDefinition.ForcedDisplay` for UnlockState | 纯 metadata，值为 true |
| 3723 | `Filters.ByRareCreature.ForcedDisplay` | proposed `BestiaryFilterDefinition.ForcedDisplay` for RareCreature | 纯 metadata，值为 null |
| 3724 | `Filters.ByBoss.ForcedDisplay` | proposed `BestiaryFilterDefinition.ForcedDisplay` for Boss | 纯 metadata，值为 null |
| 3725 | `Filters.ByInfoElement.ForcedDisplay` | proposed `BestiaryFilterDefinition.ForcedDisplay` for InfoElement | 纯 metadata，值为 null |
| 3732 | `SortingSteps.ByNetId.HiddenFromSortOptions` | proposed `BestiarySortDefinition.HiddenFromOptions` for NetId | 值为 true；基础稳定排序 |
| 3733 | `SortingSteps.ByUnlockState.HiddenFromSortOptions` | proposed `BestiarySortDefinition.HiddenFromOptions` for UnlockState | 值为 true；基础稳定排序 |
| 3734 | `SortingSteps.ByBestiarySortingId.HiddenFromSortOptions` | proposed `BestiarySortDefinition.HiddenFromOptions` for SortingId | 值为 false |
| 3735 | `SortingSteps.ByBestiaryRarity.HiddenFromSortOptions` | proposed `BestiarySortDefinition.HiddenFromOptions` for Rarity | 值为 false |
| 3736 | `SortingSteps.Alphabetical.HiddenFromSortOptions` | proposed `BestiarySortDefinition.HiddenFromOptions` for Alphabetical | 值为 false |
| 3737 | `SortingSteps.ByStat.HiddenFromSortOptions` | proposed `BestiarySortDefinition.HiddenFromOptions` for Stat | 值为 false |

#### 4.6.2 纯查询边界

- proposed `BestiaryFilterCatalog` 和 `BestiarySortCatalog` 在 content setup 注册不可变定义；`BestiaryFilterQuery` 和 `BestiarySortQuery` 只读取 entry/presentation/unlock snapshot，不能写 tracker、catalog、UI 或 network。
- BySearch 读取 search text projection；ByUnlockState 读取 `UnlockState`；ByRareCreature/ByBoss 读取 InfoElement facts；ByInfoElement 通过 stable `ElementKey` 查询。所有第三方 `IBestiaryInfoElement` 和 UI image 由 adapter/Projection 隔离。
- ByNetId、ByUnlockState、SortingId、Rarity、Alphabetical、Stat 的比较必须是显式稳定 comparer；相同比较结果需要 proposed `NpcNetId`/entry key tie-break。这个 tie-break 和 filter/sort 注册顺序是 `crossSubsystemOwner: integration-review`。
- `ForcedDisplay` 和 `HiddenFromSortOptions` 是选项元数据，不是 entry unlock authority。文件或注册顺序不能替代显式 order key。

#### 4.6.3 focused verifier 计划

每个 filter 的纯度和 null/true metadata、InfoElement key 缺失、搜索规范化、unlock snapshot 读取、所有排序步骤的比较传递性/稳定 tie-break、隐藏选项列表、empty catalog 和 registration order。未运行。

### 4.7 SharedBestiaryInfoElementState

#### 4.7.1 成员逐条归属

| 来源序号 | Version4 成员 | proposed 归属 | 状态/边界 |
| ---: | --- | --- | --- |
| 1375 | `FlavorTextBestiaryInfoElement._key` | proposed `BestiaryFlavorTextView.LocalizationKey` | 只读本地化 key；UI projection 外置 |
| 1376 | `ItemDropBestiaryInfoElement._droprateInfo` | proposed `BestiaryDropRateView` | 由 DropRate adapter 转换；ItemDrop type owner unresolved |
| 1377 | `NamePlateInfoElement._key` | proposed `BestiaryNamePlateView.LocalizationKey` | 表现事实，不是 unlock state |
| 1378 | `NamePlateInfoElement._npcNetId` | proposed `BestiaryNamePlateView.NpcNetId` | network/content ID 为 `crossSubsystemOwner: integration-review` |
| 1379 | `NPCKillCounterInfoElement._instance` | proposed `BestiaryKillCounterView` 的 Npc read port 输入 | 不保存 NPC object；从 kill state + identity query 读取 |
| 1383 | `NPCPortraitInfoElement._filledStarsCount` | proposed `BestiaryPortraitView.FilledRarityStars` | 只读 portrait metadata |
| 1384 | `NPCStatsReportInfoElement.NpcId` | proposed `BestiaryNpcStatsView.NpcNetId` | identity adapter 输入，非 entity reference |
| 1385 | `NPCStatsReportInfoElement.Damage` | proposed `BestiaryNpcStatsView.Damage` | stats snapshot |
| 1386 | `NPCStatsReportInfoElement.LifeMax` | proposed `BestiaryNpcStatsView.LifeMax` | stats snapshot |
| 1387 | `NPCStatsReportInfoElement.MonetaryValue` | proposed `BestiaryNpcStatsView.MonetaryValue` | stats snapshot |
| 1388 | `NPCStatsReportInfoElement.Defense` | proposed `BestiaryNpcStatsView.Defense` | stats snapshot |
| 1389 | `NPCStatsReportInfoElement.KnockbackResist` | proposed `BestiaryNpcStatsView.KnockbackResist` | stats snapshot |
| 1390 | `NPCStatsReportInfoElement._instance` | proposed `BestiaryStatsRefreshAdapter.NpcReadView` | Refresh source adapter；不持有 NPC object |
| 1391 | `NPCStatsReportInfoElement.HideStats` | proposed `BestiaryNpcStatsView.HideStats` | presentation policy；不能隐藏 authority 数据 |
| 3729 | `NPCNetIdBestiaryInfoElement.NetId` | proposed `BestiaryNpcIdentityView.NpcNetId` | content/network identity boundary |
| 3730 | `NPCNetIdBestiaryInfoElement.BestiaryDisplayIndex` | proposed `BestiaryDisplayIndexQuery` 输出 | 从 ContentPresentationIndex 派生；不缓存无失效规则的副本 |
| 3731 | `RareSpawnBestiaryInfoElement.RarityLevel` | proposed `BestiaryRarityView.RarityLevel` | 只读 rarity fact；filter/sort 通过 Query 读取 |

#### 4.7.2 表现状态边界

- proposed `BestiaryInfoElementState` 只保存稳定的 UI-neutral facts；`ProvideUIElement`、Texture、Asset、UIElement、Localization service 和绘制顺序全部由 `BestiaryInfoElementProjection`/UI adapter 处理。
- `NPC` instance 不得进入 InfoElement state。proposed `BestiaryStatsRefreshAdapter` 从 NPC definition/read port 生成 stats snapshot，并以显式 refresh event 更新；`OnRefreshStats` 的调用时机和实体读取 owner 为 `crossSubsystemOwner: integration-review`。
- DropRateInfo、NPC net ID、Bestiary sorting ID、rarity、localization key 和 item identity 必须分别建模；不能用一个 `NpcId` 或 object reference 兼容所有用途。
- Kill counter 只读 `BestiaryKillCountStateComponent`；NamePlate/FlavorText/Portrait/Stats 不反写 unlock tracker。`HideStats` 只影响 projection。

#### 4.7.3 focused verifier 计划

各 InfoElement snapshot 的构造和 null/缺失 identity、DropRate 转换、kill count 只读、stats refresh 重复/顺序、NPC object 不泄漏、sorting/rarity 映射、HideStats 单向 projection、UI adapter 不修改 authority。未运行。

### 4.8 SharedBestiaryCollectionProviderState

#### 4.8.1 成员逐条归属

| 来源序号 | Version4 成员 | proposed 归属 | 状态/边界 |
| ---: | --- | --- | --- |
| 1363 | `BestiaryUICollectionInfo.OwnerEntry` | proposed `BestiaryUICollectionSnapshot.OwnerEntryKey` | 用 stable entry key 替代可变 object reference；entry owner 为 integration-review |
| 1364 | `BestiaryUICollectionInfo.UnlockState` | proposed `BestiaryUICollectionSnapshot.UnlockState` | 派生状态，不持久化为第二份 authority |
| 1370 | `CommonEnemyUICollectionInfoProvider._persistentIdentifierToCheck` | proposed `BestiaryCommonEnemyProviderDefinition.BestiaryCreditId` | persistent ID 与 net ID 分开；identity owner integration-review |
| 1371 | `CommonEnemyUICollectionInfoProvider._quickUnlock` | proposed `BestiaryCommonEnemyUnlockPolicy.QuickUnlock` | 只读解锁策略 |
| 1372 | `CommonEnemyUICollectionInfoProvider._killCountNeededToFullyUnlock` | proposed `BestiaryCommonEnemyUnlockPolicy.FullKillCountNeeded` | 从 banner/content adapter 计算的 definition value |
| 1373 | `CritterUICollectionInfoProvider._persistentIdentifierToCheck` | proposed `BestiaryCritterProviderDefinition.BestiaryCreditId` | sight tracker 查询输入 |
| 1398 | `TownNPCUICollectionInfoProvider._persistentIdentifierToCheck` | proposed `BestiaryTownNpcProviderDefinition.BestiaryCreditId` | chat tracker 查询输入 |
| 3726 | `IBestiaryEntryDisplayIndex.BestiaryDisplayIndex` | proposed `BestiaryCollectionDisplayIndexQuery` contract | presentation query；不写 entry |
| 3727 | `IBestiaryEntryFilter.ForcedDisplay` | proposed `BestiaryFilterMetadataQuery` contract | option metadata；实际 filter 属于 FiltersAndSorting |
| 3728 | `IBestiarySortStep.HiddenFromSortOptions` | proposed `BestiarySortMetadataQuery` contract | option metadata；实际 sort 属于 FiltersAndSorting |

#### 4.8.2 provider、snapshot 和解锁边界

- proposed `BestiaryCollectionUnlockQuery` 根据 provider kind 读取 kill/sight/chat view 并返回 Version4 `BestiaryEntryUnlockState` 的 proposed 等价值：普通敌人基于 kill count/quick unlock，critter 基于 sight，Town NPC 基于 chat。
- proposed `BestiaryUICollectionProjection` 输出 `OwnerEntryKey`、`UnlockState`、display/filter/sort metadata；它不保存 persistent discovery facts，也不成为 catalog authority。
- CommonEnemy 的 kill threshold 读取 banner/content identity adapter；Critter/Town provider 分别读取 sight/chat。provider 定义只能保存 stable ID 和策略，不持有 `Main.BestiaryTracker`、NPC/Player object 或 UI object。
- `IBestiaryEntryDisplayIndex`、`IBestiaryEntryFilter`、`IBestiarySortStep` 的接口 contract 跨越 catalog、presentation 和 query；最终分层 owner 与 System 顺序为 `crossSubsystemOwner: integration-review`。
- progress report 从 collection projection 读取；不允许 provider Query 在读取时修改 tracker、发送网络或触发存档。

#### 4.8.3 focused verifier 计划

四种 UnlockState 边界、普通敌人 kill threshold/quick unlock、critter sight、Town NPC chat、persistent ID 缺失、OwnerEntryKey 稳定性、snapshot 不可变性、display/filter/sort metadata 单向性和 progress 聚合。未运行。

## 5. 依赖方向、生命周期和跨边界顺序

建议的非权威实现顺序如下，最终调度均标记 `crossSubsystemOwner: integration-review`：

1. 内容 identity/presentation adapter 提供 NPC type、NpcNetId、PersistentId、BestiaryCreditId、sorting/rarity 和资源 key 的只读 view。
2. Town content 与 personality catalog 完成 definition/build；TownRoom registry 在 world load/reset 生命周期创建，NPC assignment 只通过命令读取/写入。
3. Bestiary catalog registration 建立 entries、filter/sort refs 和 NPC index；ItemDrop adapter 在 content setup 后执行 merge。
4. Bestiary kill/sight/chat facts 由显式 event/command 写入；scan buffer、network adapter、persistence adapter 和 player-join adapter 只在各自副作用边界工作。
5. InfoElement facts 和 collection provider Query 读取 catalog/unlock snapshots；filters/sorting Query 只消费稳定 snapshots。
6. UI、音频、网络、存档和 achievement adapter 只消费 event/projection，不能回写 authority。

禁止通过文件顺序、静态 singleton 初始化、共享可变列表、Query 隐式写入或 projection 反向写 authority 表达顺序。WorldFile、NetBestiaryModule、NPC/Player 事件和 UI 线程边界均需 integration-review 形成显式调度契约。

## 6. 不拆分项、兼容风险和验证状态

- 不把 TownRoom registry、NPC housing relation、compatibility rule 和 persistence writer 合并成一个 `TownManager` 巨型组件；登记、正向 assignment、Query、projection 和 I/O adapter 分离。
- 不把 personality definition、HelperInfo 实体引用、Town NPC texture profile 和 shopping evaluation 合并成一个 mutable NPC component。
- 不把 Bestiary entries、解锁事实、InfoElement presentation 和 UICollection snapshot 合并成一个图鉴巨型组件；三类 discovery fact 分别保存。
- 不把 `NpcNetId`、`NpcEntityId`、`PersistentEntityId`、`BestiaryCreditId` 和 `NPC type` 当作同一 ID；每个转换必须由明确 adapter 和 integration owner 提供。
- Version4 中 FreeCakeDialogue 消费、Lucy 初始化、Bestiary NPC populator、entry factory、NetBestiaryModule.Deserialize、stats/UI provider 有删减或空实现；完整参考不能直接回填行为。
- 兼容迁移必须先建立 read-only adapter 和 focused verifier，禁止旧 registry/new catalog 或旧 tracker/new tracker 双写。回滚条件包括重复房屋登记、key mode 改变、重复解锁、Save/Load 字节漂移、网络包重复、UnlockState 变化、排序不稳定和 UI projection 反向写入。
- 本轮已执行 src2 候选实现的串行编译和 focused verifier；没有执行生产 `src` 迁移、行为等价验证或两份第二轮 Markdown 的 `git diff --check`。`verificationStatus: independently-verified` 仅表示下列局部 verifier 证据。

## 7. Integration Handoff

subsystemId: SharedRuntimeMechanisms / P11 NPC-Town-Bestiary
taskNumber: P11-second-round-component-plan
partitionId: P11
sessionId: c822e256a5e34730bb0ab0ad5a6a094b
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\11-npc-town-bestiary.md
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-component-design.md
evidenceStatus: partial
nltxStatus: partial; src2 candidate implementation is executable and focused-verified, but no authoritative current-NLTX P11 runtime or parity claim is made
verificationStatus: independently-verified
src2CodeModified: yes
src2SourceRoot: D:\TRbackup\NLTX\src2\NpcTownBestiary
src2CSharpFileCount: 106
productionSrcModified: no
confirmedOwners:
- Version4 ConditionalDialogue owns the definition registry and condition predicate surface.
- Version4 TownRoomManager owns the current room list, NPC type quick index and Save/Load surface.
- Version4 BestiaryUnlocksTracker and its three trackers own the current persistence method surface.
- Version4 BestiaryDatabase owns the current entry/filter/sort registration and drop merge surface.
proposedTypes:
- proposed TownRoomRegistryComponent, TownRoomAssignmentSystem, TownRoomQuery, TownHousingCompatibilityQuery
- proposed NpcPersonalityCatalog, NpcPersonalityQuery, TownNpcProfileCatalog and content adapters
- proposed BestiaryCatalogComponent, BestiaryEntryDefinition, BestiaryNpcEntryIndex and drop merge adapter
- proposed BestiaryKillCountStateComponent, BestiarySightDiscoveryStateComponent, BestiaryChatDiscoveryStateComponent
- proposed BestiaryUnlockProgressQuery, BestiaryFilterQuery, BestiarySortQuery, BestiaryInfoElementProjection
- proposed BestiaryCollectionUnlockQuery and BestiaryUICollectionProjection
sharedTypesForIntegrationReview:
- NPC type, NpcEntityId, PlayerEntityId, PersistentEntityId, NpcNetId and BestiaryCreditId
- TownRoomTilePoint, room key mode, BestiaryEntryKey, WorldFile snapshot and collection snapshot
- ItemDrop/RecipeGroup view, WorldTick, network packet/payload, UI/audio request and System order
crossSubsystemReaders:
- proposed WorldGen housing commands and NPC spawn queries
- proposed NPC/Player interaction, shopping, combat/death and sight scan event sources
- proposed ContentIdentityCatalog, ContentPresentationIndex, ItemDrop adapter and UI collection
crossSubsystemWriters:
- proposed TownRoomAssignmentSystem and persistence adapter
- proposed PersonalityCatalogBuilder and BestiaryCatalogRegistrationSystem
- proposed kill/sight/chat discovery Systems and explicit network/persistence adapters
orderingConstraints:
- content/identity adapters before catalogs; catalog freeze before provider/filter/sort Query
- room load before housing Query; discovery state load before collection projection and player join sync
- kill/sight/chat event commit before progress/collection Query; InfoElement projection after catalog/unlock snapshots
- all cross-subsystem ordering remains crossSubsystemOwner: integration-review
boundaryChallenges:
- choose one town registry and one NPC assignment owner across current NLTX drafts
- preserve three independent Bestiary facts while separating persistent IDs from network IDs and transient scan buffers
- keep catalog, unlock authority, presentation facts, filter/sort Query and UICollection snapshot one-way
- close missing entry population, network Deserialize, stats refresh, persistence versioning and UI provider behavior
evidenceGaps:
- missing first-round report
- conditional dialogue clear/consume and Lucy trigger/init writer
- town key mode and NLTX duplicate registry/assignment owner
- personality enumerator/biome application and resource profile lifecycle
- Bestiary NPC population, entry factory, InfoElement/UI behavior and ItemDrop ownership
- NetBestiaryModule.Deserialize and client-side kill/sight/chat receive path
blockingDecisions:
- town key/assignment owner and WorldFile migration format
- personality/profile catalog owner and Player/NPC/nearby query ports
- Bestiary content registration/drop merge/filter/sort owner and schedule
- discovery authority, network/persistence version and identity mapping
- InfoElement/UICollection projection owner and stats refresh source
- conditional dialogue/Lucy authority and all cross-subsystem types/order
implementedInSrc2:
- all 8 P11 component ranges listed in `completedComponents` have saved candidate source under `src2\NpcTownBestiary`
unverifiedOrDeferred:
- authoritative current-NLTX integration, behavior equivalence, network receive closure, persistence-version closure, UI/resource ownership and cross-partition scheduling remain unverified or deferred
verifierPlan:
- focused pure Query, state transition, persistence byte-order, network payload, catalog freeze, ID mapping, projection one-way and compatibility checks; executed as recorded below
verificationEvidence:
- buildCommand: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src2\\NpcTownBestiaryVerification\\Terraria.NpcTownBestiaryVerification.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
  project: `src2\\NpcTownBestiaryVerification\\Terraria.NpcTownBestiaryVerification.csproj`
  exitCode: 0
  warnings: 0
  errors: 0
  artifacts:
  - `D:\\TRbackup\\NLTX\\Build\\bin\\Terraria.NpcTownBestiary\\Debug\\net10.0\\Terraria.NpcTownBestiary.dll`
  - `D:\\TRbackup\\NLTX\\Build\\bin\\Terraria.NpcTownBestiaryVerification\\Debug\\net10.0\\Terraria.NpcTownBestiaryVerification.dll`
- verifierCommand: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src2\\NpcTownBestiaryVerification\\Terraria.NpcTownBestiaryVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
  exitCode: 0
  result: `P11 focused verifier passed: 8 scenarios`

本检查点已记录 8 个组件的 proposed 归属；所有 93 条输入成员均在 4.1-4.8 的逐成员表中覆盖。8 个候选组件已保存到 `src2` 并通过 focused verifier；两份文档仍保留 proposed/evidence-gap 和 `crossSubsystemOwner: integration-review` 边界，不宣称生产迁移、行为等价、网络闭合或存档闭合。

## 8. Handoff 后 Component 实施检查点

handoffStatus: completed
previousSessionId: de06dd0a16934d8c92ff67a0130cff30
sessionId: c822e256a5e34730bb0ab0ad5a6a094b
handoffId: p11-npc-town-bestiary-handoff-20260912-02
implementationStatus: completed
verificationStatus: independently-verified
src2SourceRoot: D:\\TRbackup\\NLTX\\src2\\NpcTownBestiary
productionSrcModified: no

本次接手实际修改的唯一 Component 文件：

- `src2/NpcTownBestiary/BestiaryKillCountStateComponent.cs`
  - `Add` 对负数候选值收敛到 `0`，对超过 `PositiveCap` 的候选值收敛到上限，避免 Component 持有负击杀事实或整数溢出结果。
  - 未新增或修改 System、Query、Command、Adapter、Projection、测试、测试项目、注册或装配代码。

本次验证证据：

- 非权威 runner contract：`Test-Version4NonAuthoritativePartitionSession.ps1`，exit code `0`，输出 `PASS: non-authoritative partition session contract tests passed.`
- Component build：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 构建 `src2/NpcTownBestiary/Terraria.NpcTownBestiary.csproj`，exit code `0`，warnings `0`，errors `0`。
- build artifact：`D:\\TRbackup\\NLTX\\Build\\bin\\Terraria.NpcTownBestiary\\Debug\\net10.0\\Terraria.NpcTownBestiary.dll`
- focused verifier：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 以 `--no-build --no-restore` 运行 `src2/NpcTownBestiaryVerification/Terraria.NpcTownBestiaryVerification.csproj`，exit code `0`，`P11 focused verifier passed: 8 scenarios`。

本检查点仍不宣称生产 `src` 迁移、行为等价、网络/存档闭合或跨分区 owner 裁决；现有 `evidence-gap`、`blocking-decision` 和非 Component 排除范围继续有效。
