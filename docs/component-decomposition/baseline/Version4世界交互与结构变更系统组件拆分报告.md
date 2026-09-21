# Version4 世界交互与结构变更系统组件拆分报告

> 范围：`docs/Version4权威游戏模拟系统主要子系统.md` 的第 7 项“世界交互与结构变更”。
>
> 方法：按 `public-decomposition` 完成上下文锁定、成员盘点、状态所有权标记、访问模式分组、
> Component/System/Query/Command/Adapter/Projection 边界设计和 verifier 设计。
>
> 证据范围：只读 `D:\TRbackup\Version4`；只读参考
> `C:\Users\shan\Downloads\ECS\space-station-14-master`；另以本地
> `D:\TRbackup\tmodloader-api-docs-stable`（tModLoader v2026.07）确认公开 API 语义。
> SS14 仅支持 ECS 组织模式，**不**支持 Terraria 行为、名称或字段语义结论。
>
> 结论状态：这是可实施的拆分设计，不是已迁移代码。当前 NLTX 的 `src/` 没有世界 Tile、
> Wiring 或 TileEntity 运行时实现，因而本文没有声称任何运行时或回归测试已通过。

## 1. 结论和边界

世界交互与结构变更不是一个“世界大组件”。它消费第 2 子系统的只读会话规则和第 3
子系统的权威世界存储，负责验证意图、计算变更、提交变更和产生后续交互。其本身只拥有：

1. 一次交互的意图、操作者和幂等键；
2. 一次电线/逻辑门传播的临时工作集；
3. 需要跨 Tick 存活的机关冷却和 TileEntity 运行时状态；
4. 提交后供网络、存档、表现读取的不可变变更记录。

`TileMapStore`、墙、液体、区段、容器和 TileEntity 主索引仍属于“世界存储与身份”子系统。
第 7 子系统不得把它们复制进交互组件。世界规则、昼夜和长期事件进度仍属于“世界会话与
进度”子系统；本系统只能通过只读 Query 读取它们，必要时提交有来源和去重键的事件请求。

```text
InboundWorldCommand / PlayerInteractionIntent
  -> WorldInteractionCommandAdapter
  -> InteractionValidationQuery
       (session phase, permission, sequence, rate, range, bounds)
  -> TilePlacementSystem / TileBreakSystem / WiringInteractionSystem
       / PressurePlateSystem / TileEntityRuntimeSystem
  -> WorldStructureCommitBuffer
  -> WorldStructureCommitSystem
  -> TileMapStore + TileEntityStore + WorldSectionState
  -> StructureChangeRecord
  -> NetworkStructureProjection / PersistenceProjection / PresentationProjection
```

提交屏障是唯一允许同时改变 Tile、TileEntity 索引、区段脏标记和复制记录的地方。资格
Query 不写状态；网络适配器不直接调用玩法 System；投影不回写权威状态。

## 2. 已核对的真实代码和证据状态

| 区域 | Version4 直接证据 | 观察到的职责 | 状态 |
| --- | --- | --- | --- |
| Tile 原子数据 | `Terraria/Tile.cs:6-24, 110-151, 165-190, 631-726, 824-838` | 单格持有 tile/wall/liquid、frame、active、wire 等压缩状态；比较和清理按字段族处理 | confirmed |
| 放置、破坏、框架 | `Terraria/WorldGen.cs:50970-52561, 67181-67221, 68021-68025` | `PlaceTile`、`KillTile`、`SquareTileFrame`、`RangeFrame`、`TileFrame` 同时读取定义、地图、掉落、框架和副作用 | confirmed |
| 放置/破坏公开语义 | `tmodloader-api-docs-stable/class_world_gen.html#a6d89efc164bd01b5920d1f761df42565`；`#aab8789310665bd013cbc7cbc955137ed` | `PlaceTile` 遵守锚点；`KillTile` 会引发框架，框架处理多格物体的剩余 Tile 和放置物掉落 | confirmed |
| 物件定义、锚点与回调 | `Terraria.ObjectData/TileObjectData.cs:12-54, 191-503, 640-679` | 尺寸、坐标、锚点、合法 tile/wall、放置检查和放置回调是只读内容定义，不是运行时 Component | confirmed |
| 电线与机关 | `Terraria/Wiring.cs:17-77, 115-216, 387-468, 468-833, 2777-2781` | 静态类混合操作者、冷却、传播队列、跳过集合、泵、传送端点、逻辑门和 Tile/网络副作用 | confirmed |
| 入站电线命令 | `Terraria/MessageBuffer.cs:900-910, 1478-1484, 2077-2084` | 包处理器设置 `CurrentUser` 后直接调用逻辑门、执行器或开关，并直接广播 | confirmed |
| 压力板 | `Terraria.GameContent/PressurePlateHelper.cs:7-111`; `Terraria/Player.cs:17657, 21764-21782`; `Terraria/Main.cs:11459` | 由玩家前后位置计算进入/离开关系；按下集合在 Tick 中处理 | confirmed |
| TileEntity 根 | `Terraria.DataStructures/TileEntity.cs:11-167`; `TileEntitiesManager.cs:7-82` | ID 索引、坐标索引、更新集合、类型注册表、网络放置和删除的维护点 | confirmed |
| TileEntity 运行时 | `Terraria.GameContent.Tile_Entities/TELogicSensor.cs:9-190`; `TETeleportationPylon.cs:5`; `TEItemFrame.cs:6`; `TEWeaponsRack.cs:7` | 逻辑传感器具有每 Tick 状态；晶塔/展示物/锚点具有不同的持久数据和生命周期 | confirmed/partial |
| 晶塔列表与网络 | `Terraria.GameContent/TeleportPylonsSystem.cs:13-96` | 从 `TileEntity.ByPosition` 重建晶塔列表，比较旧列表后广播；列表是投影缓存 | confirmed |
| Tile 网络投影 | `Terraria/NetMessage.cs:80-84, 2367-2387`; `MessageBuffer.cs:1989-2003` | `SendTileSquare` 是 Tile 变更消息适配；现有代码常与变更行为混在一起 | confirmed |
| SS14 参考模式 | `Content.Shared/Wires/SharedWiresSystem.cs:14-188`; `Wires/WiresPanelComponent.cs:11-58`; `Construction/Components/AnchorableComponent.cs:8-24`; `Construction/EntitySystems/AnchorableSystem.cs:25-220` | Component 保存面板/能力配置；System 负责资格、事件、外部效果和结构操作；显式事件形成扩展点 | confirmed as organizational evidence |

`TELogicSensor` 的 `UpdateStartInternal`、`Update` 与 `UpdateEndInternal` 明确表明：玩家
命中框快照、待触发点和待删除 ID 是一个 TileEntity 更新批次的临时状态；它们不是可存档的
TileEntity 数据。Version4 中若干反编译后为空的方法（例如 `TileEntity.Place`、`Wiring` 的
部分辅助方法、压力板的 `MoveInto`/`PokeLocation`）不能证明其完整算法，本文仅据其可见
调用、字段和生命周期做边界结论，算法细节标为 `partial`。

### 2.1 搜索范围和相关源码清单

检索从 `D:\TRbackup\Version4` 递归按 `WorldGen.(PlaceTile|KillTile|SquareTileFrame|TileFrame|RangeFrame)`、
`Wiring.(HitSwitch|PokeLogicGate|Actuate|SetCurrentUser|UpdateMech)`、
`TileEntity.(Place|Kill|PlaceEntityNet|PerformUpdates|Clear)` 和压力板/晶塔入口完成。下表列出
命中的本系统源码，而不是把未命中这些入口的整个 Terraria 客户端误记为交互系统。每一项的
归属结论在后文组件表中给出。

| 组 | 已阅读的 Version4 文件 | 在本系统中的角色 |
| --- | --- | --- |
| 核心结构 | `Terraria/Tile.cs`、`WorldGen.cs`、`Framing.cs`、`TileObject.cs`、`Terraria.ObjectData/TileObjectData.cs` | 单格状态、放置/破坏、框架、多格对象和只读锚点/回调定义 |
| 信号和触发 | `Terraria/Wiring.cs`、`Terraria.GameContent/PressurePlateHelper.cs` | 开关、机关、四线色传播、逻辑门、泵、传送、压力板关系 |
| TileEntity 根 | `Terraria.DataStructures/TileEntity.cs`、`TileEntitiesManager.cs`、`TileEntityType.cs` | 类型注册、实例 ID、坐标/ID 双索引、更新调度、网络放置 |
| TileEntity 种类 | `TELogicSensor.cs`、`TETeleportationPylon.cs`、`TETrainingDummy.cs`、`TEItemFrame.cs`、`TEDisplayDoll.cs`、`TEWeaponsRack.cs`、`TEHatRack.cs`、`TEFoodPlatter.cs`、`TEDeadCellsDisplayJar.cs`、`TEKiteAnchor.cs`、`TECritterAnchor.cs`、`TELeashedEntityAnchor.cs`、`TELeashedEntityAnchorWithItem.cs`、以及 `DisplayDollPoseID.cs`、`DisplayDollSlot.cs`、`HatRackSlot.cs` | 类型专属状态、合法 Tile 判定、存档/网络和与实体/物品的关系；禁止强塞入一个 `Payload` 组件 |
| Tick 和交互生产者 | `Terraria/Main.cs`、`Player.cs`、`NPC.cs`、`Projectile.cs`、`Minecart.cs`、`Collision.cs`、`Liquid.cs`、`DelegateMethods.cs` | 提供 Tick、玩家位置、实体/液体/碰撞上下文，或直接请求 Tile 变更；不是 TileMap/Wiring 的权威所有者 |
| 网络和复制 | `Terraria/MessageBuffer.cs`、`NetMessage.cs`、`Terraria.IO/WorldFile.cs` | 入站命令、Tile/TileEntity 消息和持久化边界；目标中分别是 Command Adapter 和 Projection |
| 参考，不移植 | `Content.Shared/Wires/SharedWiresSystem.cs`、`WiresPanelComponent.cs`、`Construction/Components/AnchorableComponent.cs`、`Construction/EntitySystems/AnchorableSystem.cs`、`Interaction/SharedInteractionSystem.cs` | SS14 的状态/系统/事件边界和显式调度模式；不作为 Terraria 字段或玩法的权威来源 |

`WorldGen` 的直接调用者还包含上述 `TileObject`、`Wiring`、`Player`、`NPC`、`Projectile`、
`Minecart`、`Collision`、`Liquid` 和 `DelegateMethods`。这些调用者通过 typed intent 或查询
边界接入，不能因“都调用 `WorldGen`”而与 Tile 结构存储合并。

## 3. 成员盘点与归属

### 3.1 Tile 结构变更

| Version4 成员/状态族 | 读者 | 写者/生命周期 | 状态种类 | 目标归属 | 证据 |
| --- | --- | --- | --- | --- | --- |
| `Tile.type`, `active`, `wall`, `liquid`, liquid type | 碰撞、放置、破坏、液体、网络、存档 | 生成、交互、生态、液体；世界加载至卸载 | authoritative | 第 3 子系统 `TileMapStore` 中的 `TileCellState` | confirmed |
| `frameX/frameY`、wall frame | 框架、渲染、对象合法性、网络 | `TileFrame`/`SquareTileFrame`/`RangeFrame`；每结构提交后 | authoritative derived from neighbor topology | `TileFrameState`，由 `TileFramingSystem` 写 | confirmed |
| wire 1-4、actuator、inactive 位 | Wiring、放置/拆除、网络 | 结构变更提交；世界加载至卸载 | authoritative | `TileSignalTopologyState`，与 TileCell 同一存储事务 | confirmed |
| `TileObjectData` anchors、origin、尺寸、hook | 放置资格、框架、多格对象生命周期 | 内容加载后只读 | definition | `TileObjectDefinitionCatalog`，不放入 Component | confirmed |
| `PlaceTile` 的 `mute/forced/plr/style` | 命令调用者 | 单次调用 | command input / presentation hint | `PlaceTileIntent`；声音另成效果 | confirmed |
| `KillTile` 的 `fail/effectOnly/noItem` | 破坏、掉落、表现 | 单次调用 | command input / effect policy | `BreakTileIntent`；不能作为 Tile 持久字段 | confirmed |

### 3.2 电线、机关和逻辑门

| Version4 成员/状态族 | 读者 | 写者/生命周期 | 状态种类 | 目标归属 | 证据 |
| --- | --- | --- | --- | --- | --- |
| `CurrentUser`、`_currentWireColor`、`running` | 单次传播、生成来源、机关行为 | 包适配和 `TripWire`；单次传播 | transient context | `WirePropagationContext`，System 私有 | confirmed |
| `_wireList`、`_wireDirectionList`、`_wireSkip`、`_toProcess` | `HitWire` | `TripWire`/`HitWire`；单次传播 | transient work set | `WireTraversalWorkSet`，System 私有且不可持久化 | confirmed |
| `_GatesCurrent`、`_GatesNext`、`_GatesDone`、`_LampsToCheck` | `LogicGatePass` | 开关、灯、逻辑门；单次逻辑批次 | transient work set | `LogicGateWorkSet`，System 私有 | confirmed |
| `_PixelBoxTriggers`、`_teleport`、泵输入/输出数组和计数 | `PixelBoxPass`、`Teleport`、`XferWater` | 电线传播；单次传播 | transient aggregation | `WireEffectAccumulator`，System 私有 | confirmed/partial |
| `_mechX/Y/time`、`_numMechs`、三类炮冷却 | `UpdateMech` | 机关激活，跨 Tick 递减 | authoritative runtime | `MechanismCooldownState`，按世界/区段分片存储 | confirmed |
| `blockPlayerTeleportationForOneIteration` | 逻辑传感器、逻辑门 | 一个逻辑批次结束前 | transient policy | `WirePropagationContext.BlockPlayerTeleportation` | confirmed |

### 3.3 压力板、锚点和 TileEntity

| Version4 成员/状态族 | 读者 | 写者/生命周期 | 状态种类 | 目标归属 | 证据 |
| --- | --- | --- | --- | --- | --- |
| `PressurePlatesPressed` | `Update`、`ResetPlayer`、破坏板 | 玩家移动/离开；当前 Tick 到复位 | runtime relationship | `PressurePlateOccupancyState`，按 Tile 坐标持有按压者集合/位集 | confirmed/partial |
| `PlayerLastPosition`、`pressurePlateBounds` | 玩家位置更新 | 每玩家移动 Tick | cache / scratch | `PressurePlateSystem` 私有快照，不进入 Player 或 Tile Component | confirmed |
| `TileEntity.ID` | 网络、存档、按 ID 查找 | 创建至销毁 | authoritative runtime/network ID | `TileEntityRuntimeId`，只在 `TileEntityStore` 作用域内唯一 | confirmed |
| `TileEntity.Position`、`type` | 放置/破坏、空间查找、晶塔投影 | 创建至销毁 | authoritative relationship/definition ref | `TileEntityAnchor` + `TileEntityKind` | confirmed |
| `ByID`、`ByPosition` | 网络、运行时、晶塔投影 | Add/Remove/Clear | indices, not entity fields | `TileEntityStore` 的双索引 | confirmed |
| `UpdateEntities`、`RequiresUpdates`、UpdateStart/End | `PerformUpdates`、逻辑传感器 | 注册、删除、每 Tick | scheduler membership | `TileEntityUpdateSchedule`；实体只保存 `RequiresTileEntityUpdates` 标签 | confirmed |
| `TileEntitiesNextID`、manager `_nextEntityID` | 创建/网络放置 | 会话装载至卸载 | allocator state | `TileEntityRuntimeIdAllocator`，不能复用 ECS/持久化 ID | confirmed |
| `TELogicSensor.logicCheck/On/CountedData` | 逻辑传感器更新、网络/存档 | 放置至删除；`On` 每 Tick 可变 | authoritative per-kind state | `LogicSensorState`，只附着逻辑传感器 TileEntity | confirmed |
| `TELogicSensor.playerBox/tripPoints/markedIDsForRemoval/inUpdateLoop` | 传感器批次 | 一个 Update 批次 | scratch / deferred command | `LogicSensorEvaluationWorkSet`，System 私有 | confirmed |
| 晶塔 `_pylons/_pylonsOld/cooldown` | 晶塔同步、加入世界 | 由 TileEntity 索引重建 | cache/projection throttle | `TeleportPylonProjectionCache`，不得成为晶塔权威状态 | confirmed |
| 展示物、武器架、食物盘的 `Item` | 对应 TileEntity、存档、网络 | 放置至删除 | per-kind authoritative state | `ItemFrameState`、`WeaponRackState`、`FoodPlatterState` 等专用组件 | confirmed |

## 4. 建议模块与最小契约

建议目录从领域开始，而不是建立全局 `Components/` 或 `Systems/` 收容目录。以下是目标结构，
只有真正增加生产代码时才创建；目前它不是文件移动指令。

```text
src/WorldInteraction/
  Structure/
    PlaceTileIntent.cs
    BreakTileIntent.cs
    StructureChangeRecord.cs
    TilePlacementQualificationQuery.cs
    TilePlacementSystem.cs
    TileBreakSystem.cs
    TileFramingSystem.cs
    WorldStructureCommitSystem.cs
  Wiring/
    MechanismCooldownState.cs
    WirePropagationContext.cs
    WiringInteractionSystem.cs
    LogicGateSystem.cs
  PressurePlates/
    PressurePlateOccupancyState.cs
    PressurePlateSystem.cs
  TileEntities/
    TileEntityAnchor.cs
    TileEntityKind.cs
    TileEntityRuntimeId.cs
    TileEntityUpdateSchedule.cs
    TileEntityRuntimeSystem.cs
    LogicSensorState.cs
    LogicSensorSystem.cs
  Teleportation/
    TeleportPylonProjectionCache.cs
    TeleportPylonProjection.cs
```

第 3 子系统应提供相邻但独立的 `TileMapStore`、`TileEntityStore`、
`TileEntityRuntimeIdAllocator`、`WorldSectionState` 和 `WorldStructureCommitBuffer`。它们保留
身份、索引、分片和存储事务的所有权；第 7 子系统经接口访问，不能绕过它们写裸字典或数组。

| 模块 | 输入 | 权威输出/唯一写入 | Interface、seam 和深度 |
| --- | --- | --- | --- |
| `TilePlacementQualificationQuery` | session phase、actor 权限/距离、坐标、`TileObjectDefinition`、只读 Tile/墙快照 | `PlacementEligibility`；无写入 | `Evaluate(PlaceTileIntent, IWorldInteractionView)`；纯函数，可单测锚点、边界、原点和拒绝理由 |
| `TilePlacementSystem` | 已验证 `PlaceTileIntent` | `StructureMutationPlan`，不直接网络发送 | `PlanPlacement(...)`；定义、地图和玩家库存通过端口输入，隔离原 `WorldGen.PlaceTile` 的混合副作用 |
| `TileBreakSystem` | 已验证 `BreakTileIntent`、Tile/对象定义 | Tile/多格对象/掉落/TileEntity 清理的原子计划 | `PlanBreak(...)`；确保“破坏→框架→多格清理→掉落”的顺序不被拆散 |
| `TileFramingSystem` | 被提交 Tile 坐标和邻域 | frame 更新、受影响区段 | `PlanFrames(ChangedTileRegion, ITileReadView)`；不能单独暴露为任意调用方的写接口 |
| `WorldStructureCommitSystem` | `StructureMutationPlan` | `TileMapStore`、`TileEntityStore`、区段脏标记、`StructureChangeRecord` | `Commit(plan)`；唯一多存储写入点，测试原子性和失败不提交 |
| `WiringInteractionSystem` | `WireTriggerIntent`、Tile signal topology、`MechanismCooldownState` | 一次传播的结构变更计划和后续触发 | `Propagate(...)`；`WirePropagationContext` 和工作集必须是局部对象或显式租借，异常路径清理 |
| `LogicGateSystem` | 本批次灯/门候选 | 有序 `WireTriggerIntent` | `Resolve(...)`；只产生命令，不递归直接改 Tile 或调用网络 |
| `PressurePlateSystem` | `LocationComponent`、旧位置快照、只读 TileMap | `PressurePlateTransition` 和 `WireTriggerIntent` | `ObserveMovement(...)`；占用关系按 Tile 坐标，不能把 255 槽数组写进 Player Component |
| `TileEntityRuntimeSystem` | 已提交的 Tile 变更、类型目录、网络放置命令 | TileEntity 创建、删除、双索引和更新调度 | `Place/Remove/Tick`；严格维护 ID 索引、位置索引和更新集合的一致性 |
| `LogicSensorSystem` | `LogicSensorState`、时钟/实体位置/液体的只读视图 | 后续 `WireTriggerIntent`、删除请求和变更计划 | 三阶段 `BeginTick/Evaluate/EndTick`；前后阶段属于 System 调度，不属于实体状态 |
| `TeleportPylonProjection` | `TileEntityStore` 的只读晶塔查询 | 网络/加入世界快照和差异 | `BuildDelta(...)`；缓存可丢弃并可重建，禁止反向驱动 TileEntity |
| `WorldInteractionCommandAdapter` | 网络包/本地输入 | 已验证的 typed intent 或拒绝 | `TryAdapt(...)`；拥有序列、权限、频率、范围、session-ready 验证，不调用 `Wiring` 或 `WorldGen` |
| `NetworkStructureProjection` | 已提交 `StructureChangeRecord` | 协议消息 | `Project(...)`；以 `SendTileSquare` 的职责为参考，但不能借投影修改世界 |

## 5. Component 组合与不变量

### 5.1 需要长期保存的组件/状态

`MechanismCooldownState`、`PressurePlateOccupancyState`、`TileEntityAnchor`、
`TileEntityKind`、`TileEntityRuntimeId`、`LogicSensorState` 和各 TileEntity 专用数据都可成为
权威持久/运行时状态。它们按共同读写者和生命周期分组：例如，逻辑传感器的检测类型、开关
状态和被计数数据总由同一传感器 System 读取和写入，适合在一个 `LogicSensorState` 中；展示
物持有的物品则不应并入该组件。

必须保持以下不变量：

1. 一个活动 TileEntity 同时恰好有一个运行时 ID、一个锚定 Tile 坐标和一种 TileEntity kind。
2. `TileEntityStore` 的 ID 索引、位置索引和更新调度要么一起包含同一实体，要么一起不包含它；
   删除先移除调度和两种索引，再执行类型专属清理。
3. 同一结构提交中，Tile 的 active/type/frame、TileEntity 创建/清除和区段脏标记是原子的。
4. 任何传播内同一坐标最多按照该颜色和批次的规则处理一次；逻辑门重入必须显式排队。
5. 压力板占用只反映有效的移动实体和有效 Tile；破坏压力板必须移除关系并生成一次离开/触发
   后续处理。
6. `TileEntityRuntimeId`、ECS Entity ID、网络客户端 ID、世界 GUID 和持久化 ID 是不同类型；
   不以 `int` 直接互转。

### 5.2 明确不成为 Component 的数据

| 数据 | 原因 | 所属 |
| --- | --- | --- |
| `TileObjectData`、锚点和 placement hook | 内容定义，加载后只读，多个实体共享 | `ContentCatalog` |
| `TileMapStore`、墙/液体数组、世界区段 | 世界级稠密存储和第 3 子系统的事务根 | `WorldStorage` |
| `_wireList`、跳过集合、门队列、泵聚合、传送端点 | 单次传播临时工作集；存档/网络无意义 | `WiringInteractionSystem` 私有工作集 |
| 逻辑传感器的玩家矩形快照、待触发点、待删除 ID | 单个更新批次的 scratch/deferred command | `LogicSensorSystem` 私有工作集 |
| 晶塔旧/新列表和刷新冷却 | 可重建投影缓存；不是晶塔存在的真值 | `TeleportPylonProjection` |
| 音效、尘粒、UI、`mute` | 表现效果；失败或重放不能修改权威结构 | Presentation adapter |
| `CurrentUser` | 一次命令的因果来源；全局静态会在嵌套/异步情况下污染 | `InteractionActor` 参数和 `WirePropagationContext` |

## 6. 调度、效果和失败边界

建议的服务器 Tick 顺序如下。顺序是调度约束，不通过文件名或目录枚举隐式表达。

1. `WorldInteractionCommandAdapter` 解码、验证并排队 Intent。
2. `PressurePlateSystem` 观察已完成移动，生成压力板转换和触发 Intent。
3. `TilePlacementSystem` 与 `TileBreakSystem` 生成结构变更计划。
4. `WiringInteractionSystem` 在每个触发的本地上下文中传播；`LogicGateSystem` 处理同批次队列。
5. `TileEntityRuntimeSystem` 处理由结构计划产生的创建/删除；`LogicSensorSystem` 执行
   `BeginTick -> Evaluate -> EndTick`，其 EndTick 生成下一批触发而不递归绕过调度。
6. `TileFramingSystem` 计算所有受影响框架。
7. `WorldStructureCommitSystem` 原子写 `TileMapStore`、`TileEntityStore` 和区段状态，输出
   `StructureChangeRecord`。
8. 网络、存档和表现 Projection 只读取已提交记录；发布失败不得回滚或半写玩法状态。

主要副作用登记：结构提交读取显式世界快照并写 Tile/TileEntity/区段；网络、音频、尘粒和
日志是提交后的观察效果。若提交前验证失败，输出拒绝结果而不触及存储。若提交发生异常，
事务边界必须保证无部分索引、无孤立 TileEntity 和无已发布的“成功”网络消息。网络重试按
`actor + sequence + target + intent type` 去重，不能把协议重放变成重复破坏、掉落或机关触发。

## 7. 迁移建议与兼容策略

这是设计迁移顺序，不是本次已经执行的移动：

1. **先建读模型和记录。** 为现有 `Main.tile`、`TileEntity.ByID/ByPosition` 建立只读
   `ITileReadView`、`ITileEntityReadView` 和 `StructureChangeRecord` 投影，不引入第二权威源。
2. **提取纯资格。** 从 `WorldGen.PlaceTile/KillTile`、`TileObjectData` 和网络入口提取边界、
   锚点、对象原点、权限、距离和 session phase Query；用固定输入复现拒绝理由。
3. **建立提交计划。** 把放置、破坏、框架和 TileEntity 清理由裸写改为 `StructureMutationPlan`；
   旧 API 只能单向适配到新计划，禁止新旧 Tile/索引双写。
4. **隔离电线。** 先将 `CurrentUser`、传播工作集和机关冷却从静态全局分开，再将开关/逻辑门
   入站包适配为 `WireTriggerIntent`。保持每颜色、泵、传送、逻辑门的既有顺序，通过回放样例
   固化。
5. **拆 TileEntity 根和种类。** 迁移双索引、分配器和更新调度后，再逐类迁移
   `TELogicSensor`、晶塔、展示物、锚点等专用状态。不要建包含 `object Payload` 的泛用
   `TileEntityComponent`。
6. **最后切换投影。** `NetMessage.SendTileSquare`、TileEntity 网络消息和晶塔加入世界同步从
   `StructureChangeRecord`/快照读取；旧消息编号可由 Adapter 保持兼容，不泄漏至领域 System。

建议的文件迁移记录应逐批登记源路径、目标路径、依赖影响和回滚方式。例如，
`Terraria/Wiring.cs` 的字段不能整体移动到一个 `WiringComponent.cs`；它须按第 3.2 节拆为
`MechanismCooldownState` 和两个 System 私有工作集。当前没有迁移生产文件，故本报告不声明
任何源文件已移动或任何公开 API 已改变。

## 8. Focused verifier 计划和当前结果

| verifier | 覆盖的边界与断言 |
| --- | --- |
| `TilePlacementQualificationTests` | 越界、未 Ready、权限/距离/序列/频率拒绝；锚点、原点、单格和多格资格；Query 绝不写 Tile |
| `StructureCommitAtomicityTests` | 成功时 Tile、框架、TileEntity、区段脏标记和记录一致；任一失败时无半提交或孤立索引 |
| `TileBreakAndFramingTests` | `fail`/`effectOnly`/`noItem` 的计划语义；多格物体破坏后框架和 TileEntity 清理顺序；掉落只出现一次 |
| `WirePropagationTests` | 四种线色、环路去重、嵌套逻辑门、机关冷却、泵聚合、传送的 actor 来源和同 Tick 顺序 |
| `PressurePlateTransitionTests` | 进入、离开、传送、死亡、重置和板被破坏；玩家位置快照不泄漏为持久状态 |
| `TileEntityStoreInvariantTests` | 创建、网络放置、删除、清理、ID/坐标冲突、更新调度和 allocator 单调性；四种 ID 不混淆 |
| `LogicSensorPhaseTests` | Begin/Evaluate/End 的玩家快照、延迟触发、延迟删除和阻止一次传送的作用域 |
| `StructureProjectionTests` | 已提交记录到 Tile/TileEntity/晶塔消息的投影；重复命令去重；投影异常不反写世界 |

实际结果：已完成文档、Version4、tModLoader 本地 API 文档和 SS14 参考代码的只读检索；未修改
运行时代码、未移动 ECS 文件、未运行编译或测试。因此所有 verifier 均为**未执行**计划，本报告
不构成迁移完成或行为等价的证明。

## 9. 未确认项和实施前补证据

| 缺口 | 原因 | 实施前动作 |
| --- | --- | --- |
| `TileEntity.Place`、部分 Wiring/PressurePlate 私有方法的完整算法 | 当前 Version4 文件包含反编译后空实现 | 回退到可执行的未删减源码或使用行为回放；补齐后才锁定细粒度算法测试 |
| 机关、泵、传送和逻辑门的全部交叉效果顺序 | `Wiring.HitWireSingle` 大量分支且依赖空辅助实现 | 构建来源 Tile 拓扑的 golden replay，记录每个变化、掉落和消息顺序 |
| 每种 TileEntity 的存档/网络字段 | 已发现的种类具有不同数据和接口，未逐类型完整盘点 | 按种类建立字段清单，确认字段的持久化、网络与删除生命周期后再迁移 |
| 当前协议的完整验证条件 | `MessageBuffer` 可见直接调用；安全/距离/频率校验可能分散在其他包分支 | 为每类入站命令建立 Adapter 验收表，拒绝任何无法确认的裸世界写入 |

在这些缺口关闭前，可以实施存储/命令/投影边界和 focused verifier 骨架，但不得声称已保持全部
Terraria 机关细节。该限制是证据协议要求，不是将系统范围缩小为容易通过的子集。
