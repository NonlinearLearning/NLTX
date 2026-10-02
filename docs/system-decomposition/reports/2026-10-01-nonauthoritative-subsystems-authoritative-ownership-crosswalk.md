# 非权威子系统与权威系统归属对照报告

审查日期：2026-10-01

审查状态：`partial`

设计状态：`proposed`

验证状态：`not-run`

## 结论

非权威分区不是“全部非权威行为”的分类。Version4 非权威成员库存中包含不少权威模拟行为；同时也混有客户端表现、目录定义、存档、网络、平台适配和诊断工具。是否归属权威系统，应看它是否拥有游戏模拟不变量、受控状态提交和对应生命周期，不能只看它所在的 P 分区、字段名或读取了权威状态。

本次对照范围为 P01–P15 和 P18。明确的领域重叠主要集中在 P02–P11，并包括 P01 的世界会话/规则状态、P12 的权威规则定义输入，以及少量跨界交互组。P13–P15 和 P18 主要是网络/持久化/平台适配与地图绘制；这些通常协作或投影权威状态，不因此成为权威状态 Owner。P16、P17、P19、P20 已从当前非权威分区范围移除。

这些是**责任面候选映射**，不是最终 Owner 裁决。非权威库存明确说明 349 个细分组只是成员库存边界，并未闭合逐成员读者、写者、生命周期、持久化/网络语义和调度顺序。混合组必须继续拆到行为和唯一写者后，才能落实到具体 System。

## 范围与证据

| 来源 | 本次使用范围 | 证据边界 |
| --- | --- | --- |
| [Version4 非权威逐成员库存](../../migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md) | 原始基线 SHA-256 `B944441D92D5103F218B766A3E354200794528A19E65A862E44C2B40A36F1196`；20 个原始分区，4,542 条基线成员记录、349 个细分库存组 | 完整来源基线；本次工作范围筛选为 P01–P15、P18，共 3,415 条成员记录；基线本身不证明行为 Owner |
| [非权威分区工作包](../../migration/ledgers/non-authoritative-component-partitions/) | 当前纳入范围为 P01–P15、P18；P16、P17、P19、P20 已移除 | 每份保留报告声明细分组不是 Component/System；成员语义与写入闭包仍待补证 |
| [权威模拟子系统审查](../../component-decomposition/baseline/Version4权威游戏模拟子系统全量审查报告-2026-09-05.md) | 32 个一级责任面及其分层；其中 21 个位于 `authoritative-simulation` 层 | Version4 的候选边界审查；不代表当前 NLTX 已迁移 |
| [权威子系统索引](../../migration/ledgers/Version4子系统索引.json) | 系统职责、相邻责任和 Version4 证据路径 | 用于责任面交叉映射，不单独证明非权威库存成员的唯一写者 |

权威基线把责任分为权威模拟、运行时基础设施/适配器、客户端表现/工具和外部依赖等层。报告明确指出，UI、音频、粒子、SceneMetrics 等表现或派生对象不因消费模拟数据而拥有权威状态；存档和网络也属于独立适配/生命周期责任。权威模拟系统与基础设施的调用关系不等于所有权归属。

## 分区对照

表中列出相关的权威模拟 System 或协作责任面；运行时基础设施、Adapter、Query 和投影不会因列在此处而成为权威 Owner。`部分/混合` 项必须按行为拆开；同一组内可能同时含权威状态、查询、缓存和适配器。

| 非权威分区 | 相关权威或协作责任面 | 判定与应排除内容 |
| --- | --- | --- |
| P01 世界会话与运行时 | `WorldSession`、`WorldCalendarAndEventOrchestration`、`SimulationRuleOverrides`、`WorldProgressionAndTransition`；世界种子/规则对 `WorldGenerationAndEcology` 的输入 | **部分/混合。** `MainClockAndFrameScheduling`、`MainFrameAndWorldRuleControl`、`MainTimeSkipState`、`SharedDifficultyAndRuleMetadata`、`WorldSeedAndExploitRules` 包含权威时间、规则或进度候选；`MainBootstrapAndWorldRules`、`MainRandomAndSeedState`、`MainSaveAndWorldSessionState` 需把世界规则/会话事实与启动、随机辅助及存档适配分开。窗口、宿主、线程调度和派生查询不因此成为权威 Owner。随机源是领域行为的输入依赖，不构成全局权威系统。 |
| P02 世界环境与事件 | `WorldCalendarAndEventOrchestration`、`WorldProgressionAndUnlocks`、`NpcAndTownSimulation`、`SpawnLifecycleAndLoot`、`SimulationRuleOverrides` | **领域重叠明显但组内混合。** `MainSlimeRainState`、`MainCalendarWeatherState`、`MainInvasionState`、`SharedInvasionAndBossTracking`、`SharedCelebrationAndLanternEvents`、`SharedRitualAndStormEvents`、`SharedInvasionDamageTrackingState`、`SharedInvasionWaveAndArenaState` 是权威事件状态/资格候选。天空目录、风雨/瀑布表现、事件文本与视觉计时应归目录、查询或客户端投影；需按具体写集拆分。 |
| P03 世界生成与地牢 | `WorldGenerationAndEcology`；结构提交协作 `WorldInteractionAndStructures` / `WorldStorage` | **领域重叠最强。** 地形 pass、世界生成执行、地牢布局/放置和生成进度影响最终世界状态。`SharedDungeon*`、`SharedBiome*` 与 `WorldSpawnConfigurationState` 是生成行为或其规则/目录输入候选，不应全部建成独立权威 System；菜单/图形字段和静态定义本身不是运行时 Owner。 |
| P04 地块、液体与世界存储 | `LiquidSimulation`、`WorldStorage`、`WorldInteractionAndStructures`、`WiringAndMechanisms`、`WorldGenerationAndEcology` | **部分/混合。** `TileCellMaterialAndLiquidState`、`TileCellFrameAndBitState`、`MainWorldGeometryAndCapacity`、`MainTileBehaviorAndInteractionMetadata`、`TileEntityRegistryAndBaseState`、`TileEntityAnchorAndSensorState`、`TileEntityWorldInteractionState`、`SharedTilePlacementAnchorAndHookModules` 与权威世界状态或结构提交相邻。快照/存档、绘制目标、预览、颜色和渲染缓存不自动归模拟 Owner；共享 Tile 也不能推出所有 Tile 行为由一个 System 写入。 |
| P05 空间移动与物理 | `SpatialSimulation`、`TeleportationAndTraversal`、`MountAndVehicleSimulation`、`ProjectileSimulation`、`LiquidSimulation`、`PlayerGameplay` | **领域重叠明显。** `SharedTeleportAndPortalSupport`、`MinecartMotionAndTrackState`、`EnvironmentDamageAndSeatState`、`EntityBoundsAndFluidState` 是移动/空间/载具/接触行为候选；`TrackedProjectileReferenceState` 是 Projectile 生命周期边界。`SharedPhysicsCollisionQueries` 属于空间行为的 Query/协作者，查询组本身不代表状态 Owner。库存把 `MinecartCustomizationState` 标为 definition/presentation；装饰和外观字段不应据此并入权威车辆 Owner。 |
| P06 实体生命周期与归因 | `SpawnLifecycleAndLoot`、`NpcAndTownSimulation`、`ProjectileSimulation`、`LeashedEntitySimulation`、`PlayerGameplay` | **跨系统混合。** `MainEntityPoolsAndWorldSlots`、`SharedEntitySourceAndAttribution`、`EntityIdentityAndMotionState` 覆盖创建/销毁、来源归因和身份/运动数据，横跨多个权威域。实体槽位、通用 ID、网络 ID 或来源标签不能单独决定 Owner；需按实体类别和实际提交者交由集成审查。 |
| P07 战斗、伤害与状态效果 | `CombatAndStatus`、`PlayerGameplay`、`NpcAndTownSimulation`、`ProjectileSimulation`、`DeathPenaltyAndRevenge`、`ItemContainerAndEconomy` | **领域重叠明显。** `SharedCombatTargetingAndImmunity`、`SharedHitTileTracking`、`ItemTagEffectState`、`SharedItemCombatAndDamageCapabilityState` 是战斗/伤害/能力输入候选。`SharedCombatTextState` 是表现投影；`LockOnTargetingState` 需区分目标意图、派生目标查询和 UI 状态。 |
| P08 玩家输入与玩法 | `PlayerGameplay`、`IntentAndInteraction`、`SpatialSimulation`、`MountAndVehicleSimulation`、`ItemContainerAndEconomy`、`WorldInteractionAndStructures`、`WiringAndMechanisms`、`WorldProgressionAndUnlocks` | **部分/混合。** `PlayerInputRuntimeState`、`PlayerMovementCapabilityState`、`PlayerIntentAndInteractionState`、`EquipmentLoadoutState`、`DoorOpeningInteractionState`、`PressurePlateInteractionState`、`PlayerItemPickupAndRespawnState`、`SharedCreativeUnlockProgress` 涉及玩法提交或资格。原始输入、焦点、绑定、配置、预览、Golf/屏幕状态是输入适配或表现；只把验证后的意图交给权威 Owner。 |
| P09 物品、库存与容器 | `ItemContainerAndEconomy`、`PlayerGameplay`、`WorldInteractionAndStructures`、`CombatAndStatus`、`MountAndVehicleSimulation`、`SpawnLifecycleAndLoot` | **领域重叠明显。** `ChestContainerStorage`、`SharedItemIdentityAndStackState`、`SharedItemProgressionAndWorldInteractionState`、`SharedItemBuffMountAndConsumableEffects`、`SharedWorldItemLifecycleState`、`SharedItemEquipmentSlotState`、`SharedWorldItemIdentityAndEconomyPayloadState`、`SharedItemToolPlacementCapabilityState` 是库存/经济/使用行为候选。`SharedItemDerivedQueries` 是查询；世界物品表现 payload、排序策略和 UI 转移状态不能替代库存唯一写者。 |
| P10 经济、制作、钓鱼与掉落 | `ItemContainerAndEconomy`、`FishingAndCatchSimulation`、`SpawnLifecycleAndLoot`、`NpcAndTownSimulation`；目录输入来自 `ContentCatalog` | **领域重叠最强。** 商店/价格、Crafting、Recipe、FishingAttempt/Condition、Loot、DropRule resolution/selection 和来源归因是权威结果计算候选。Recipe、掉落规则与钓鱼条件目录是规则输入；由哪个 System 消费并提交结果仍需行为/写者证据。 |
| P11 NPC、城镇与图鉴 | `NpcAndTownSimulation`、`WorldProgressionAndUnlocks`、`SpawnLifecycleAndLoot` | **部分/混合。** `SharedConditionalDialogueSupport`、`SharedTownRoomState` 涉及 NPC/城镇领域；`BestiaryUnlockTracking` 对应可保存、可验证的权威解锁事实。NPC personality、Bestiary 条目/过滤/排序和 UI 信息元素属于内容定义或表现；平台成就进度不等同权威图鉴解锁。 |
| P12 内容定义与目录 | `ContentCatalog`、`ContentLifecycleAndRegistration`，以及 `WorldGenerationAndEcology`、`WorldInteractionAndStructures`、`ItemContainerAndEconomy`、`CombatAndStatus` 等消费方 | **权威规则输入，不是普遍的权威状态 Owner。** `SharedItemStaticCapabilityRules`、`ArmorSetBonusDefinitions`、`WingStatsDefinition`、Item variant/prefix 与 TileObject 放置定义等可约束权威行为。目录注册/重建属于基础设施；定义数据被权威系统读取，不意味着目录本身拥有运行时模拟状态。 |
| P13 网络协议与会话 | `NetworkSessionAndSectionStreaming`；由各权威系统拥有的接收后状态提交 | **主要为基础设施/适配器。** 连接、包、socket、section streaming、消息缓冲和网络投影不属于模拟 Owner。`SharedContentNetworkModules` 等协议入口可调用权威命令；发布快照不转移被复制状态的所有权。 |
| P14 持久化、恢复与配置 | `PersistenceAndRecovery`；反序列化后交回 `WorldSession`、`WorldStorage`、玩家/物品/世界生成等领域 Owner | **主要为基础设施/适配器。** 文件格式、临时文件、恢复版本、路径和存档过程拥有独立 I/O 生命周期，不是游戏不变量 Owner。加载器可恢复权威状态，但提交和验证规则应回到对应模拟系统。 |
| P15 外部平台与协议边界 | `ExternalBoundaries`；资格事实可能由 `WorldProgressionAndUnlocks` 提供 | **不属于权威模拟 Owner。** Social、Workshop、IPC、NAT、加密、资源包和平台 API 是外部适配/依赖。基线明确将平台 Achievement 接口与权威资格事实分开。 |
| P18 地图、相机与绘制 | `ClientPresentationAndTools`；读取 `WorldStorage`、`SpatialSimulation` 等提交结果 | **不属于权威 Owner。** SceneMetrics、地图、相机、光照、shader、draw batch、MapTileUpdateQueue 等是派生查询、缓存或客户端投影。权威 Tile/探索事实如有写入，需在相应领域/存储系统单独识别。 |

## 重点归属判断

1. **明确应纳入权威 System 边界复核：**世界日历/事件与进度、世界生成和结构提交、Tile/液体/空间/传送/载具、玩家移动/交互/装备、NPC/投射物/战斗、容器/物品/经济/钓鱼/掉落、实体生成与生命周期，以及已验证的 Bestiary/解锁事实。对应非权威输入集中在 P01–P11。
2. **只作为权威系统的规则或数据输入：**P03 地牢/地形定义、P10 配方/掉落/钓鱼目录、P11 NPC/Bestiary 定义和 P12 内容能力目录。目录本身通常属于 `ContentCatalog` / `ContentLifecycleAndRegistration`，其消费者才拥有具体模拟不变量。
3. **不要并入权威 Owner：**当前范围内的 P13 网络、P14 存档、P15 外部平台和 P18 地图/绘制。它们可以触发命令、恢复状态或投影状态，但需保留单向边界。
4. **默认需要 `crossSubsystemOwner: integration-review`：**玩家/实体身份与 ID、Tile/液体与结构写入、NPC 生成和战斗归因、Projectile/Leashed Entity、库存/掉落、事件进度、网络复制、存档恢复、成就解锁、世界快照及 RNG/调度顺序。仅凭本报告不指定跨分区唯一写者。

## 缺口与后续核验

- 本报告用权威子系统审查与非权威成员分区文档做静态概念映射；筛选范围为 3,415 条基线成员记录，未逐一回读目标源码、所有调用者、间接副作用和生命周期，因此不能输出逐成员 Owner 清单。原始 4,542 条来源库存保留作基线，不代表四个已移除分区仍在当前任务范围内。
- 现有非权威分区的细分组有混合职责；需对重点成员补齐读者、直接/间接写者、提交点、生命周期、网络/存档副作用和调度边后，才能把候选改为 `static-confirmed`。
- 权威基线记录当前 NLTX 映射包含 `missing`、`partial` 和 `excluded`；本报告不声称非权威 Version4 成员已经进入 NLTX，也不声称迁移、行为等价或生产接线完成。
- 没有运行构建、测试或行为验证；`verificationStatus: not-run`。
