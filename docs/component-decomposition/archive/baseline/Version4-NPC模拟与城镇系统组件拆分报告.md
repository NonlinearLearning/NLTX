# Version4 NPC 模拟与城镇系统组件拆分报告

## 1. 结论与范围

本文落实《[Version4 权威游戏模拟系统主要子系统](Version4权威游戏模拟系统主要子系统.md)》中第 9 项
“NPC 模拟与城镇”的边界：NPC 刷新、AI、目标、战斗、生命周期、城镇住房、幸福度、对话、服务和任务。
结论是它不能迁移为单一 `NpcComponent`。参考实现将至少五种生命周期完全不同的状态混在
`Terraria.NPC`、`Main`、`WorldGen` 和静态表中：

1. 单个 NPC 的可变模拟状态；
2. 按玩家和世界条件产生的刷新压力与资格；
3. 以 Tile 坐标为锚点、以 NPC 类型为键的城镇住房存档关系；
4. 内容定义的城镇个性和 NPC 配置；
5. 对话、商店和服务的外部交互入口。

因此在目标实现中建立领域目录 `Npc/` 与 `Town/`，而不是建立全局
`Components/`、`Systems/` 收容目录。组件实现保留 Terraria 规则语义，不复制 Space Station 14
的命名、领域模型或实现。

本报告的静态证据结论不等价于系统行为等价。2026-09-05 已实现并验证下列纯状态组件的字段组合：
`NpcEntityIdentityComponent`、`NpcDefinitionReferenceComponent`、
`NpcBehaviorStateComponent`、`NpcTargetComponent`、`NpcLifetimeComponent`、
`NpcParentRelationComponent`、`NpcReplicationDirtyState`、`TownResidentComponent` 和
`NpcHousingAssignmentComponent`。尚未实现 `System`、`Query`、`Command`、持久化投影、网络 Adapter
或具体 AI/住房/交易规则；这些运行时行为仍未验证。

## 2. 来源、检索范围与证据状态

### 2.1 规则权威来源

规则语义取自只读参考树 `D:\TRbackup\Version4`，重点检索了下列文件和调用点：

| 范围 | 已检查的真实代码 | 得到的证据 |
| --- | --- | --- |
| 刷新与实例初始化 | `Terraria/NPC.cs:185-377, 5152-5785, 8095-8133, 67051-67144` | `Spawner.SpawnNPC`、按玩家的刷新资格/速率/Tile 筛选、槽位选择、`NewNPC` 与 `SetDefaults`。 |
| 单体状态、AI、目标、生命与生命周期 | `Terraria/NPC.cs:5897-6431, 18642, 43033-43070, 53203, 64192-64662, 67461, 76711-77007` | `active`、type/net ID、AI 数组、生命、目标、住房、网络标志，以及 AI/目标/死亡/Tick 的入口。 |
| Tick 调度 | `Terraria/Main.cs:11454, 11511-11524, 13714-13718` | 主循环先调用 `NPC.SpawnNPC()`，逐槽 `UpdateNPC`，并对需要者调用 `WorldGen.QuickFindHome`。 |
| 城镇住房与房间评分 | `Terraria/WorldGen.cs:4558-4575, 4620, 5361-5464, 5642, 59537` | 移房、逐出、特殊城镇 NPC 条件、寻房、共居检查和世界更新中的重评估。 |
| 住房关系与持久化 | `Terraria.GameContent/TownRoomManager.cs:9-176` | 类型到 Tile 位置的正向关系、快速索引、保存/加载、同住资格。 |
| 个性与商店价格 | `Terraria.GameContent/ShopHelper.cs:10-67`，`Terraria.GameContent/Personalities/PersonalityDatabase*.cs` | 个性注册表、Biome 偏好及购物设置派生。 |
| 对话、服务和任务 | `Terraria.GameContent/NPCInteraction.cs:5-34`，`Terraria.GameContent/NPCInteractions.cs:15-297` | 当前对话 NPC 的解析、25 个商店入口和服务动作的注册表。 |
| 复制 | `Terraria/NPC.cs:43033-43044, 76943-77007`，`Terraria/NetMessage.cs:1708-1740, 2587-2603` | NPC 复制包 `23`，住房包 `60`，以及城镇 NPC 的全量/加入同步策略。 |
| 支撑值对象 | `Terraria/NPCSpawnParams.cs`，`Terraria.DataStructures/NPCAimedTarget.cs`，`Terraria.DataStructures/NPCKillAttempt.cs`，`Terraria.Enums/NPCTargetType.cs`、`TownNPCSpawnResult.cs`、`TownNPCRoomCheckFailureReason.cs` | 生成难度覆盖、目标快照、击杀快照和错误/结果枚举。 |

以下相关文件被发现但不作为本报告的权威运行时状态：

- `Terraria.GameContent/TownNPCProfiles.cs` 是头像、纹理变体等表现资料；
- `Terraria.GameContent.Bestiary/*` 是图鉴进度/表现；
- `Terraria.GameContent/NPCDamageTracker.cs` 属于伤害归因；
- `Terraria.GameContent/ShopHelper.cs` 只产出价格设置，实际商品目录、货币账本和买卖提交属于
  “物品、容器与经济事务”系统；
- `Terraria/SceneMetrics.cs` 是场景扫描结果，应归属 Biome/环境读取侧，不能由城镇系统拥有。

### 2.2 公开 API 语义交叉核对

按本仓库 `public-decomposition` 的离线 API 检索流程，检查了
`D:\TRbackup\tmodloader-api-docs-stable\index.html`；其首页标题为 `tModLoader Documentation`，页眉版本为
`tModLoader v2026.07`。使用 `classes.html` 的实际 href 定位类型页，而非猜测页面文件名。结果如下：

| 查询 | 命中与证据 | 本报告使用方式 | 局限 |
| --- | --- | --- | --- |
| `NPC.TargetClosest` / `NPC.target` | `class_n_p_c.html#ace46ba11f1b49ba040e73aed0b05b1d9`：选择最近存活玩家并写 `target`；`#a24627ed4a99fbc0555b1be4717a9335e` 说明目标通常是 `Main.player` 索引，也可能以 `300-500` 表示 NPC 目标 | 支持把“选择”建为纯 Query、把当前选择建为组件；禁止把整数槽位直接当稳定实体 ID | 不证明 Version4 的每个 AI 分支、目标优先级或 Tick 顺序。 |
| `NPC.ai` / `NPC.localAI` | `class_n_p_c.html#aa057189d345452f8afe82acb7a5dbec3` 与 `#a2a66732c971715a4284207049382b1b9`：二者是多槽状态存储，`localAI` 不与服务器同步 | 支持“不按数组槽位机械拆分”及将网络状态与本地缓存分开 | 不能为任何特定 AI style 推断槽位含义。 |
| `NPCSpawnInfo` | `struct_n_p_c_spawn_info.html`：自然刷新地点和其周围玩家的信息快照 | 支持 `NpcSpawnContextSnapshot` 是短寿命输入而非实体持久状态 | 不给出 Version4 的完整刷怪资格或选择规则。 |
| `NPCHappiness` | `struct_n_p_c_happiness.html`：NPC 类型的 NPC/Biome 关系；`#a561736ffe7f94e4f7b18efd48adf5b6d` 说明关系级别对应商店价格倍率 | 支持个性放内容定义、价格作为 `TownHappinessQuery` 的派生输出 | 不证明 Version4 `ShopHelper` 的完整倍率、特殊情形或交易提交。 |

该镜像不是 NLTX 源码或目标 Version4 的权威运行时实现，也不确认私有封装、持久化或网络顺序。
证据来源记录：`source=D:\TRbackup\tmodloader-api-docs-stable`；`version=tModLoader v2026.07`；
`stop_reason=已取得公开字段/方法语义，运行时顺序和缺失规则回退到 Version4 源码`。

### 2.3 SS14 只读组织参考

只读检查 `C:\Users\shan\Downloads\ECS\space-station-14-master` 的相关目录和类型：

- `Content.Shared/NPC/SharedNPCComponent.cs` 是很薄的网络组件标记；
- `Content.Server/NPC/Components/NPCComponent.cs` 只持有 NPC 对世界观察的黑板；
- `Content.Server/NPC/Components/NPCSteeringComponent.cs` 单独持有转向、路径和卡住检测状态；
- `Content.Shared/NPC/Components/NpcFactionMemberComponent.cs` 与
  `Content.Shared/NPC/Systems/NpcFactionSystem.cs` 将阵营定义、运行时成员资格和邻近空间查询分开；
- `Content.Server/NPC/HTN/HTNComponent.cs`、`HTNSystem.cs` 与各 `Preconditions/`、`Operators/`
  将计划、资格和副作用操作拆开；
- `Content.Shared/Store/Components/StoreComponent.cs`、`SharedStoreSystem*.cs` 与
  `Content.Server/Store/Systems/StoreSystem*.cs` 将店铺状态、共享 UI 协议和服务器购买提交分离；
- `Content.Server/NPC/Systems/NPCSteeringSystem.cs:108` 明确将转向安排在物理系统之前。

这证明“薄状态组件 + 专责 System + 纯资格检查 + 显式调度”的组织模式可行；它**不能**证明
Terraria 的 NPC、住房、幸福度或商店规则。目标领域语义和所有字段判断仍以上述 Version4 代码为准。

### 2.4 证据限制

`NPC.cs` 的刷新外壳、状态字段、`UpdateNPC`、`TargetClosest`、死亡及住房同步调用真实存在，
但多项特殊 AI 函数是空体，例如 `AI_047_GolemFist`、`AI_045_Golem`、`AI_123_Deerclops` 等；
`NPCInteractions` 中每个动作的 `Condition`、`GetText`、`Interact` 和货币写入也为默认值/空体。
因此：

- 组件的权威字段、公开入口、住房持久化和网络生命周期为 `confirmed` 或 `partial`；
- 特殊刷怪选择、具体 AI 行为树、服务资格、费用、物品转移和任务结算为 `missing`；
- 缺口不能用 UI 文本、默认返回值或 SS14 的行为补齐。实施这些模块前必须取得未删减规则源码或
  独立的规则规格。

## 3. 状态所有权与成员归属

### 3.1 单体 NPC 成员盘点

| 参考成员/入口 | 读写者和生命周期 | 状态性质 | 候选归属 | 证据 |
| --- | --- | --- | --- | --- |
| `active`、槽位索引 `whoAmI` | `NewNPC` 分配；`UpdateNPC`、网络、死亡路径读写 | 活跃性权威，但槽位不是实体 ID | `NpcSlotStore` + `NpcEntityIdentityComponent` | `NPC.cs:5897, 67051-67144, 76711` |
| `type`、`netID`、默认生命/防御/大小 | `SetDefaults*` 初始化，AI/网络/内容查表读取 | 运行时类型引用；默认值是内容定义 | `NpcDefinitionReferenceComponent` + `NpcDefinitionCatalog` | `NPC.cs:6307, 6391, 8095-8133` |
| `position`、`velocity`、宽高、方向、碰撞标志 | AI、移动/碰撞、网络读取/写入 | 位置权威但应归共享 Movement/Physics 能力 | 既有 `LocationComponent`、`VelocityComponent`、`ColliderComponent`；不新建 NPC 副本 | `NPC.cs:18642, 76711` 及实体基类成员 |
| `ai[4]`、`localAI[4]`、`aiAction`、`aiStyle` | AI 例程每 Tick 读写 | 类型专属运行时状态；数组槽位语义不稳定 | `NpcBehaviorStateComponent`；每个已确认行为族再拆专属状态 | `NPC.cs:6309-6315, 18642` |
| `target`、`targetRect` | `TargetClosest` 与 AI 更新 | 当前目标是权威选择结果，矩形是短寿命快照 | `NpcTargetComponent` + `NpcTargetSelectionQuery` + `NpcTargetSnapshot` | `NPC.cs:6321, 64192`；`NPCAimedTarget.cs` |
| `life`、`lifeMax`、`damage`、`defense`、免疫/Buff 数组 | 战斗、`StrikeNPC`、`checkDead` 读写 | 战斗权威状态 | 留给 `CombatAndStatus`：`HealthComponent`、`NpcCombatStatsComponent`、`NpcStatusSlotsComponent` | `NPC.cs:6323-6343, 6067-6071, 64571, 67461` |
| `timeLeft`、`dontCountMe`、`npcSlots`、`despawnEncouraged` | 刷新压力、更新和清理读写 | 生命周期/人口约束 | `NpcLifetimeComponent`；总量进入世界级 `NpcPopulationPressureState` | `NPC.cs:6051-6063, 6319, 76711` |
| `townNPC`、`friendly`、`housingCategory` | 城镇 AI、住房、网络、个性读取 | 类型能力与住房资格，不是位置状态 | `TownResidentComponent`；类别来自定义目录 | `NPC.cs:6397, 6415, 6423` |
| `homeless`、`homeTileX/Y`、`lookForHomeTimeout`、旧值 | `WorldGen.moveRoom/kickOut/QuickFindHome`、AI、住房包读写 | 当前居民的住房镜像；房间关系本身是世界级 | `NpcHousingAssignmentComponent`，其唯一写者为住房命令；持久化权威为 `TownHousingStore` | `NPC.cs:6403-6415, 43033-43044` |
| `realLife` | 多段 Boss/实体关联与死亡路径读取 | 运行时实体关系；不能当槽位或网络 ID | `NpcParentRelationComponent`，仅在多实体行为族上附加 | `NPC.cs:6039, 67819` |
| `netUpdate`、`netAlways`、`spawnNeedsSyncing`、每玩家同步状态 | `NetMessage` 消费并清除 | 复制调度缓存，不是玩法真相 | `NpcReplicationDirtyState`，由 `NpcNetworkProjection` 独占 | `NPC.cs:6015-6037`，`NetMessage.cs:1708-1740` |
| `frame`、`frameCounter`、`scale`、颜色/纹理变体 | `FindFrame`、渲染读取 | 表现或定义驱动；不驱动权威 AI/住房/交易 | `NpcPresentationProjection` | `NPC.cs:6347-6357, 53203`；`TownNPCProfiles.cs` |

### 3.2 世界级、定义级与外部状态

| 参考状态/入口 | 候选归属 | 状态类型 | 不能与其合并的原因 |
| --- | --- | --- | --- |
| `Spawner` 的玩家位置、Biome、事件、世界难度、刷新率/上限和 Tile 检查上下文 | `NpcSpawnContextSnapshot`、`NpcPopulationPressureState`、`NpcSpawnQualificationQuery` | 每次尝试的派生快照 + 世界级压力 | 输入随玩家和 Tile 变化，不能持久化到某 NPC。 |
| `NPC.downed*`、入侵和全局解锁标志 | `WorldEventProgressState` | 世界进度权威 | 不属于某个 NPC 实例，实例销毁不能删除它。 |
| `TownRoomManager._roomLocationPairs`、`_hasRoom` | `TownHousingStore` | 持久化关系和索引缓存 | 键是 NPC 类型和 Tile 坐标，语义不是单体组件；快速布尔数组是缓存。 |
| `PersonalityDatabase`、`PersonalityProfile.ShopModifiers`、Biome/NPC 特征 | `NpcPersonalityDefinitionCatalog` | 内容定义，只读 | 每名居民共享定义；不能复制到每个实例或用作存档。 |
| `ShopHelper.GetShoppingSettings` 输出 | `TownHappinessQuote` | 瞬时派生值/投影 | 价格由交易时上下文计算，不得缓存后反向写入幸福度。 |
| `NPCInteractions.All`、`LocalPlayer.talkNPC`、UI 文本 | `NpcInteractionRegistry`、`NpcDialogueSession`、`DialogueProjection` | 注册表、会话与表现 | 当前实现使用本地玩家，服务器权威模型必须改为明确的玩家实体/会话 ID。 |
| NPC 包 23、住房包 60 | `NpcNetworkProjection`、`TownHousingNetworkProjection` | 外部协议投影 | 客户端消息不是权威可写状态。 |

## 4. 推荐模块边界

以下文件布局遵守领域优先与一个公开核心类型一个同名文件；只有 `Npc/Behavior/` 和
`Town/Interaction/` 因为已存在稳定的独立职责群才建议作为子目录。

```text
src/Shared/
  Npc/
    NpcEntityIdentityComponent.cs
    NpcDefinitionReferenceComponent.cs
    NpcBehaviorStateComponent.cs
    NpcTargetComponent.cs
    NpcLifetimeComponent.cs
    NpcParentRelationComponent.cs
    NpcSpawnContextSnapshot.cs
    NpcPopulationPressureState.cs
    NpcSpawnQualificationQuery.cs
    NpcSpawnSelectionQuery.cs
    NpcSpawnCommand.cs
    NpcTargetSelectionQuery.cs
    NpcBehaviorSystem.cs
    NpcLifecycleSystem.cs
    NpcReplicationDirtyState.cs
  Town/
    TownResidentComponent.cs
    NpcHousingAssignmentComponent.cs
    TownHousingStore.cs
    TownHousingQualificationQuery.cs
    TownHousingAssignmentCommand.cs
    TownHousingReevaluationSystem.cs
    NpcPersonalityDefinitionCatalog.cs
    TownHappinessQuery.cs
    TownHappinessQuote.cs
    Interaction/
      NpcDialogueSession.cs
      NpcInteractionRegistry.cs
      NpcInteractionQualificationQuery.cs
      NpcServiceCommand.cs
      QuestSubmissionCommand.cs
      DialogueProjection.cs
src/Server/
  Npc/
    NpcSpawnSystem.cs
    NpcNetworkProjection.cs
  Town/
    TownHousingPersistenceProjection.cs
    TownHousingNetworkProjection.cs
    Interaction/
      NpcInteractionNetworkAdapter.cs
```

| 模块 | Interface | Implementation 与状态写权 | Seam / Depth / Leverage / Locality |
| --- | --- | --- | --- |
| `NpcDefinitionCatalog` | `TryGet(NpcTypeId, out NpcDefinition)` | 内容加载器构建，只读提供默认生命、防御、行为族、居民资格、住房类别 | 用内存定义夹具替换；把 type-ID 条件从 Tick 中推出；高杠杆；局部到内容边界。 |
| `NpcEntityIdentityComponent` | `NpcInstanceId`、`NpcSlot` | `NpcSlotStore` 分配/释放；`NpcInstanceId` 是稳定实体标识，slot 只为兼容投影 | 可用虚拟槽位存储测试复用；避免 `whoAmI` 混作实体/网络/持久化 ID；高杠杆。 |
| `NpcBehaviorStateComponent` | `BehaviorKind`、已确认的通用计时/阶段状态 | `NpcBehaviorSystem` 是唯一写者；未知 `ai[]` 槽位先保留为兼容 blob | 用行为策略假件测试状态变换；不能按 4 个数组元素机械拆成 4 组件；中等局部性。 |
| `NpcTargetSelectionQuery` | `SelectTarget(NpcEntity, TargetingContext)` | 纯读取空间、阵营、可攻击性和目标快照；返回候选/原因 | 可脱离 ECS 运行器测试；把 `NPCAimedTarget` 留为快照而不持久化；高可测性。 |
| `NpcTargetComponent` | `TargetEntity?`、`SelectionTick` | `NpcTargetCommitSystem` 在 Query 后提交 | 目标消失、不可达和多个候选可独立验证；目标实体引用不能写为网络 ID。 |
| `NpcSpawnQualificationQuery` | `Evaluate(NpcSpawnContextSnapshot)` | 纯读取玩家、世界进度、Tile/液体/Biome、人口压力 | 可注入确定性随机数与 Tile 夹具；刷怪资格不在 NPC 上缓存；高杠杆。 |
| `NpcSpawnSelectionQuery` | `SelectDefinition(EligibleSpawnContext, RandomSource)` | 纯选择类型/初始参数，必须记录 RNG 和来源 | 特殊选择规则缺失时拒绝提交而非猜测；分离选择与创建。 |
| `NpcSpawnCommand` | `TrySpawn(NpcSpawnRequest)` | 原子地申请槽位、创建实体、附加定义/生命周期/初始 AI、标记复制 | 用满槽、失效 Tile、重复提交测试回滚；结构变化只在此命令发生。 |
| `NpcLifetimeComponent` + `NpcLifecycleSystem` | TTL、人口槽占用、销毁原因 | 系统递减、判定清理并发出死亡/离场事件 | 以时钟 port 测试；与战斗死亡解耦但消费死亡结果；局部。 |
| `TownHousingStore` | `TryGetRoom(NpcTypeId)`、`GetOccupants(TilePoint)` | 世界会话唯一写者；维护持久化关系和可重建索引 | 用内存存储测试存取与加载；类型键与实体引用明确分离；高杠杆。 |
| `TownHousingQualificationQuery` | `EvaluateRoom(TownResident, TilePoint, HousingScan)` | 纯检查房间、占用、特殊条件和共居；输出失败原因 | `TownNPCRoomCheckFailureReason` 映射为稳定领域原因；可用 Tile fixture 测试。 |
| `TownHousingAssignmentCommand` | `Assign`、`Evict` | 先资格检查，再同时写 Store 与居民镜像，最后触发复制 | 一个提交点消除 `homeless`/Store 双写；移房和逐出都有回滚 seam。 |
| `TownHousingReevaluationSystem` | Tick/世界变更事件输入 | 挑选超时或房间受影响居民，调用 Query 后提交命令 | 它不执行房间扫描规则；可用事件队列/时钟测试节流和顺序。 |
| `NpcPersonalityDefinitionCatalog` | `GetTraits(NpcTypeId)` | 内容加载只写，游戏 Tick 只读 | 可用小型定义集测试；不把爱好附加到实例。 |
| `TownHappinessQuery` | `Quote(TownResident, TownContext)` | 纯读取定义、邻居和 Biome，产出价格/对话因素及解释 | 价格、幸福文本、商店 UI 使用同一报价；无货币写入，局部而深。 |
| `NpcDialogueSession` + `NpcInteractionQualificationQuery` | `Open/Close` 与 `Evaluate(player, npc, action)` | Session 由连接生命周期写；Query 重查距离、活动、世界/任务和冷却 | 把原 `LocalPlayer.talkNPC` 改成显式会话；可用断线、目标死亡、越距测试。 |
| `NpcServiceCommand` / `QuestSubmissionCommand` | `Execute(AuthorizedIntent)` | 调用库存、生命、货币、世界进度等其他领域的原子事务 | 只依赖端口，不能直接改 UI；服务规则缺失，当前只定义接口且不实现。 |
| `DialogueProjection`、`NpcNetworkProjection`、`TownHousing*Projection` | 权威状态到 DTO/网络包 | 只投影、节流、序列化；不反向驱动实体 | 可 snapshot 测试；房屋地址和 NPC 实体/网络 ID 分别编码。 |

## 5. 系统顺序与数据流

必须显式注册下列部分顺序，不能依赖文件名或目录枚举。没有数据写冲突的纯查询可以并行；下列
有箭头的阶段不能并行。

```text
World/Player snapshot + deterministic RNG
  -> NpcPopulationPressureSystem
  -> NpcSpawnQualificationQuery
  -> NpcSpawnSelectionQuery
  -> NpcSpawnCommand
  -> NpcBehaviorSystem
  -> NpcTargetSelectionQuery -> NpcTargetCommitSystem
  -> MovementPhysics / CombatAndStatus
  -> NpcLifecycleSystem
  -> TownHousingReevaluationSystem
  -> TownHousingQualificationQuery -> TownHousingAssignmentCommand
  -> TownHappinessQuery (only on a shop/dialogue request)
  -> NpcServiceCommand / QuestSubmissionCommand
  -> persistence and network projections
```

来源调度的最小保真证据是 `Main.cs:11454` 在 NPC 更新前请求刷新、`Main.cs:11511-11524`
逐槽更新、`Main.cs:13714-13718` 触发住房寻址。目标设计应将这种隐式大型循环变成上述显式阶段，
但不得在缺少 AI 或刷怪规则时声称与原循环行为等价。

身份规则如下：

| 标识 | 含义 | 是否可复用 | 允许的位置 |
| --- | --- | --- | --- |
| `NpcEntityId` | 目标 ECS 的稳定实体身份 | 不可复用 | 组件关系、命令、内部事件。 |
| `NpcSlot` | 兼容 Version4 `whoAmI` 的容量槽 | 可复用 | `NpcSlotStore` 和旧协议 Adapter。 |
| `NpcNetworkId` | 网络复制时的实体映射 | 连接/协议范围可复用 | Network projection/adapter。 |
| `NpcTypeId` / `netID` | 内容定义类型 | 多实例共享 | 定义引用和住房 Store 的兼容类型键。 |
| `TownRoomTilePoint` | 世界 Tile 坐标 | 仅世界/存档范围 | `TownHousingStore`、住房 Query、持久化。 |

## 6. 不拆分项与兼容策略

| 项目 | 决定 | 原因和约束 |
| --- | --- | --- |
| `ai[]` 与 `localAI[]` | 暂不按槽位机械拆分 | 每个槽位语义随 AI 类型改变；只有获得某行为族读写证据后才将该族拆成专用状态组件。 |
| 位置/速度/碰撞 | 不归 Npc 领域新建副本 | 它们是多实体共享能力，应使用运动和物理领域的权威组件。 |
| 生命、伤害、Buff、掉落 | 不放本报告的 Npc 核心组件 | 它们归 `CombatAndStatus`、`SpawnLifetimeLoot`；NPC 只通过事件和窄接口协作。 |
| `TownRoomManager._hasRoom` | 可重建索引，不持久化为第二真相 | 存档只保存类型和坐标关系；加载后由 `TownHousingStore` 重建索引并校验范围。 |
| 幸福度价格 | Query 输出，不做实体字段 | 每笔交易时使用当前邻居/Biome/世界状态重算；价格显示不能成为扣款输入。 |
| 头像、对话文字、帧动画 | Projection/表现层 | 不得让渲染或本地 UI 回写房屋、价格、服务或任务。 |
| NPC 商店库存与买卖 | 只保留窄的服务命令端口 | 商品目录、货币和物品提交在经济子系统；本报告不复制或重新实现其事务。 |
| 特殊 AI、特殊刷怪、服务规则 | 显式暂缓 | 参考树存在空体，无法确认规则；必须补证据后再实现。 |

兼容迁移应是单向的：`Version4Adapter -> 新权威状态 -> Projection`。迁移期不得同时让
`NPC.homeless/homeTile*` 和 `TownHousingStore` 成为可写真相；同理，不能由客户端对话索引或商店
UI 价格直接写入服务结果。

## 7. 风险、缺口与验证计划

### 7.1 风险表

| 风险 | 证据状态 | 后果 | 实施前处理 |
| --- | --- | --- | --- |
| 特殊刷新选择与稀有事件分支 | `missing` | 错误的 NPC 类型、刷怪频率或世界事件进度 | 取得未删减 `NPC.Spawner` 规则，按分支补充 `NpcSpawnSelectionQuery` 测试。 |
| 大量 `AI_*` 空体 | `missing` | 无法保持移动、攻击、召唤、Boss 阶段和城镇日常行为 | 按已读写字段的行为族逐批实施，不给“通用 AI”编造默认规则。 |
| `NPCInteractions` 条件、费用与提交空体 | `missing` | 客户端可绕过资格或产生部分扣款/任务提交 | 只开放 server-authoritative intent；补足每个动作规则和事务回滚后才启用。 |
| 住房 Store 使用 NPC 类型键 | `partial` | 同类型多实例或特殊居民可能冲突 | 兼容导入保持类型键；目标长期模型须以 `NpcEntityId` 关系表示实例住房，并写数据迁移策略。 |
| 住房扫描和世界 Tile 并发修改 | `partial` | 分配到失效房间、Store 与实体镜像不一致 | 在世界结构变更提交后执行重评估；以房间版本/Tile 区段版本使资格结果失效。 |
| 网络包 23/60 的隐含条件 | `partial` | 重连或分区可见性下漏同步/重复同步 | 独立 DTO 和投影测试；不复用 `netUpdate` 作为玩法状态。 |

### 7.2 focused verifier

在补齐缺失规则并开始迁移前，应继续扩展下列 focused verifier。当前 verifier 已覆盖组件字段组合；表中所列
系统级场景仍为**未实现/未验证**：

| 验证对象 | 最小场景 | 断言 |
| --- | --- | --- |
| 刷新资格 | 玩家死亡、创造模式禁刷、超出人口、屏幕内、无效 Tile、事件中 | Query 给出稳定的允许/拒绝原因；拒绝不申请槽位、不消耗 RNG。 |
| 刷新提交 | 槽满、创建中断、定义不存在、重复请求 | 无半创建实体；成功时身份、定义、初始生命周期与复制脏标记一次性出现。 |
| 目标选择 | 多目标、目标消失、坦克宠物、越界/不可攻击 | Query 纯粹可重复；Commit 只在选择改变时写 `NpcTargetComponent`。 |
| 生命周期 | TTL 到期、战斗死亡、父实体死亡、人口计数 | 销毁原因唯一；槽位仅在最终清理后可复用。 |
| 住房资格 | 合格/不合格房间、已占用、共居类别冲突、特殊居民条件 | 返回明确失败原因；不修改 Store 或 NPC。 |
| 移房和逐出 | 成功、资格失败、网络发送失败、世界 Tile 改变 | `TownHousingStore` 和 `NpcHousingAssignmentComponent` 不会半提交；重试幂等。 |
| 存档 | 空、重复、越界、未知类型、损坏记录 | 加载清理无效关系并重建索引；合法关系 round-trip。 |
| 幸福度 | Biome 偏好、邻居喜恶、无家、交易时上下文变化 | 相同输入同报价；报价不会改写定义、住房或货币。 |
| 交互和服务 | 越距、NPC 死亡、断线、重放、余额不足 | Session 被关闭或拒绝；服务/任务只完整提交一次，绝不部分扣款。 |
| 网络投影 | 新生成、住房更新、加入世界、分区不可见、断线重连 | 仅投影权威状态；实体 ID、槽位、网络 ID 和住房 Tile 坐标不混淆。 |

## 8. 建议实施批次

1. 建立 `NpcDefinitionCatalog`、身份/槽位、人口压力、基础生命周期和复制 DTO；只迁移已确认的
   创建、清理和投影壳，建立刷新增量测试。
2. 将已确认的目标选择、移动意图和通用 AI 状态迁入 `NpcBehaviorSystem`，与
   `MovementPhysics`、`CombatAndStatus` 通过事件/窄接口连接；特殊 AI 保持未迁移。
3. 建立 `TownHousingStore`、住房资格 Query、移房/逐出命令、持久化与复制投影；先用房间关系
   round-trip 和原子一致性验证收口。
4. 接入个性定义和 `TownHappinessQuery`，只为报价与对话投影提供数据，不接入扣款。
5. 在取得每项动作的规则证据后，按商店、治疗、税收、任务等逐一实现 server-authoritative
   service command，并为每项事务添加失败回滚和重放验证。

每个批次结束时只构建受影响项目，并按仓库根 `AGENTS.md` 的串行 .NET 命令契约运行相应
verifier；在实际运行前不得将本报告的静态发现表述为 NPC 或城镇系统已经实现或通过回归。

## 9. 已实现组件与验证证据

本批次仅落实字段设计文档第 3 节的状态组件。源文件按领域放在 `src/Npc/`、`src/Town/`，没有创建
泛化 `Components/` 目录，也没有让组件承担跨实体行为。

| 已实现模块 | 路径 | 验证的边界 |
| --- | --- | --- |
| 身份、定义、行为、目标、生命周期、父关系 | `src/Npc/` | 稳定实例 ID、可复用槽位、类型/变体 ID、四个权威 AI 槽、目标与父关系均保持分离。 |
| 复制脏状态 | `src/Npc/NpcReplicationDirtyState.cs` | 网络投影脏标记独立于玩法和住房状态。 |
| 居民与住房镜像 | `src/Town/` | 居民能力由 flags 派生；无家状态、搜索资格和住房 Tile 镜像分开表达。 |

2026-09-05 的 focused verifier 结果如下：

```text
Command: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' 'run' '--project' '.\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj' '-m:1' '-nr:false' '-p:UseSharedCompilation=false' '-p:MSBuildNodeReuse=false' '-p:BuildInParallel=false'"
Project: Test/Terraria.Npc.Components.Verification/Terraria.Npc.Components.Verification.csproj
Exit code: 0
Warnings: 0
Errors: 0
Output: PASS: NPC and town component field composition
Artifact path: Build/bin/Terraria.Npc.Components.Verification/Debug/net10.0/Terraria.Npc.Components.Verification.dll
```

这个 verifier 不覆盖刷新资格、AI 状态转移、目标选择、生命周期清理、住房扫描/提交、持久化、网络协议或
交易原子性。因此它仅证明组件 API 与字段组合，不支持更广泛的 NPC/城镇运行时完成声明。
