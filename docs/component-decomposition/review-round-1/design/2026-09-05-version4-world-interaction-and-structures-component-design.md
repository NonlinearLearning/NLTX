# WorldInteractionAndStructures Component Design

## 1. 设计元数据

subsystemId: WorldInteractionAndStructures
taskNumber: 10
sourceReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-interaction-and-structures-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-world-interaction-and-structures-component-design.md
designScope: component-only
designStatus: decision-required
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
selectionMethod: 当前会话明确提供并产生的唯一研究报告路径；未扫描 docs\research，也未选择其他会话报告
evidenceMismatch: none；sourceReport 与实际存在的当前会话研究报告路径一致

designStatus 使用 decision-required，因为 Version4 存在删减的关键成员，Tile/TileEntity 的最终提交归属、液体泵转移的跨域归属、多格结构一致性、ID 分层和 Pylon 注册表 owner 均未锁定。

## 2. 设计范围与排除范围

本设计只整理 WorldInteractionAndStructures 的 Component 状态模型，覆盖：

- Tile 单元的结构事实、frame、线路拓扑和液体工作标记；
- 压力板占用、线路传播工作集、机械冷却和触发者上下文；
- TileEntity 的锚点、种类、运行时身份、持久化/网络身份候选、更新资格和已确认的专属状态；
- 逻辑传感器、训练假人、单物品承载、多槽物品承载、系绳锚点；
- 多格结构 footprint、Chest 结构绑定、Pylon 结构状态和世界级 Pylon 注册状态；
- 当前 NLTX 已有 Component、State 和 value object 与目标 Component 的覆盖关系。

本文件只在字段证据中简短列出 Version4 的成员、初始化和读写位置；不把方法、网络字节、存档记录或外部对象直接当作 Component。泵转移、TileEntity 删除恢复、多格结构原子性、Pylon 注册和统一 revision 的 owner 保留为整合裁决项。

本文件排除客户端表现、音频、掉落生成、完整网络格式、完整存档格式以及其他子系统的完整设计。跨子系统字段不在本文件中宣布最终 owner。

## 3. 组件设计依据

### 3.1 证据来源

| 来源 | 使用方式 | 当前证据状态 |
|---|---|---|
| D:\TRbackup\Version4 | 首要行为基线；重新核对 Tile、Wiring、TileEntity、Chest、WorldGen、碰撞、入站消息和 WorldFile 的实际成员 | confirmed / partial |
| D:\TRbackup\无任何删减通过编译 | 仅对 Version4 同路径、同签名的删减成员补充字段语义；不把完整参考独有成员写成 Version4 事实 | partial |
| D:\TRbackup\tmodloader-api-docs-stable | 只交叉核对公开 TileEntity 校验、读写和 Wiring 公开边界；不替代私有实现 | partial |
| C:\Users\shan\Downloads\ECS\space-station-14-master | 仅参考 Component 粒度、实体组合和 ID 分离；不复制名称、目录或领域语义 | missing / 仅结构参考 |
| D:\TRbackup\NLTX\src、Test、dome\src、dome\Test | 核对已有组件和局部验证覆盖；不能据此推断完整 Version4 行为 | partial |

### 3.2 直接相关的 Version4 证据

- D:\TRbackup\Version4\Terraria\Tile.cs:6-24,68-79 直接证明 Tile 的 type、wall、liquid、四个 header 和 frameX/frameY，构造默认值为零。
- D:\TRbackup\Version4\Terraria\Wiring.cs:17-73,88-113 证明线路 skip 集合、传播栈、逻辑门队列、像素盒触发、传送槽、泵数组、机械数组、当前线路颜色和 CurrentUser 位于静态共享状态；Wiring.cs:129-149 证明部分数组会被清空。
- D:\TRbackup\Version4\Terraria\Wiring.cs:151-180 证明三类炮冷却和机械时间会逐 Tick 处理；Wiring.cs:267-384 证明 switch 触发会直接改 frame 并进入线路触发；Wiring.cs:395-414 证明 actuator 状态切换依赖 Tile 的 actuator/inactive 位。
- D:\TRbackup\Version4\Terraria\Wiring.cs:468-550,609-665 证明四种线路颜色、泵坐标、传送槽、像素盒和逻辑门工作集共享一次传播生命周期；Wiring.cs:464-467,665 中的删减成员不能补造成已确认行为。
- D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs:11-37,80-128,147-170 证明 TileEntity 有位置索引、ID 索引、更新列表、ID、Position、type 和 RequiresUpdates，添加/移除同时影响多个索引。
- D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs:171-226 证明世界写入 ID 和位置，网络写入省略 ID；这不足以证明持久化 ID 与网络 ID 已经分层。
- D:\TRbackup\Version4\Terraria.DataStructures\TileEntitiesManager.cs:30-87 证明 11 类 TileEntity 类型注册、Tile 合法性检查和实例生成是类型边界事实，但注册表本身不是实体 Component。
- 实际 TileEntity 类型目录是 D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities。TELogicSensor.cs:9-37,109-172,217-285 证明传感器字段、更新资格、CountedData 和日期/玩家/液体读取；TETeleportationPylon.cs:5-18,26-78,108-150 证明 Pylon 的 3x4 footprint、597 Tile 类型、frame/支撑校验和风格推导。
- D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEItemFrame.cs:6-13,40-47、TEFoodPlatter.cs:7-14,41-58、TEWeaponsRack.cs:7-16,44-61 证明单物品 tuple 的 type/prefix/stack 形状。
- D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEHatRack.cs:11-41,50-109,214-257 证明 2 个装备槽和 2 个 dye 槽及空内容约束；TEDisplayDoll.cs:14-18,33-37,63-73 证明 9/9/1 槽位；TELeashedEntityAnchorWithItem.cs:6-32 证明带物品系绳锚点的可选 payload。
- D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDeadCellsDisplayJar.cs:6-13,25-26,30-36 证明单物品字段存在，但读写方法为空，不能补造完整兼容语义。
- D:\TRbackup\Version4\Terraria\Chest.cs:17-66,68-110 证明 Chest 的坐标、index、容量、物品数组、名称和 bank 标记，以及按坐标索引和空内容破坏检查；WorldGen.cs:44290-44313,49685-49750,50970-52939,67181-67195,68021-68084 证明放置、破坏、液体和 framing 共同影响结构状态。
- D:\TRbackup\Version4\Terraria\Collision.cs:2471-2600、MessageBuffer.cs:800-950,1460-1710,2535-2580,2980-3339 和 Terraria.IO\WorldFile.cs:1646-1715,3385-3478 只作为边界证据：Tile、Chest、TileEntity 的外部读写要求组合不变量保持。
- tModLoader 文档使用 D:\TRbackup\tmodloader-api-docs-stable\class_mod_tile_entity.html:122-124,138-142,504-505 和 class_wiring.html:101-128；这些页面只证明公开校验、读写和机械/触发边界，不能替代 Version4 私有实现。

### 3.3 当前 NLTX 证据

当前 src\WorldInteraction 已有 Tile、线路、冷却、压力板和 TileEntity 数据轮廓；src\WorldStorage 已有 TileMap、TileEntity、Chest、Pylon 和世界根存储轮廓。它们证明局部数据模型，不证明完整行为闭环。因此已有同名数据模型在本设计中标为 existing 或 partial，不因为旧报告中的 missing 文字而重新标为 proposed。

## 4. Version4 成员到 Component 归属表

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| Tile.type、Tile.wall、Tile.sTileHeader、bTileHeader、bTileHeader2、bTileHeader3 | ushort / byte | Tile 结构类型、墙体和原始 header | 权威状态 | Tile 创建至清理 | TileCellComponent | confirmed / partial | D:\TRbackup\Version4\Terraria\Tile.cs:6-24,68-79 |
| Tile.liquid 及 lava/honey/shimmer 语义 | byte / 位语义 | 液体量和液体种类 | 权威状态 | Tile 创建、液体变更、清理 | TileCellComponent | confirmed / partial | Tile.cs:12,56-62；WorldGen.cs:4478-4526 |
| Tile.frameX、Tile.frameY | short | Tile frame | 权威状态 | 与 Tile 同生存期，可单独重算 | TileFrameComponent | confirmed | Tile.cs:22-24；WorldGen.cs:67181-67195,68021-68084 |
| Tile.wire、wire2、wire3、wire4、actuator、inActive 语义 | header 位语义 | 四色线路、actuator 和 actuated 状态 | 权威状态 | Tile 创建至破坏 | TileSignalTopologyComponent | confirmed / partial | Tile.cs:14-20；Wiring.cs:395-414,468-550 |
| Tile 液体工作标记 | bool / uint | 液体排队、去重和变更 revision | 缓存 / 工作状态 | 液体工作开始至完成或取消 | TileLiquidWorkStateComponent | partial | NLTX src\WorldStorage\TileCellState.cs:3-23 |
| _wireSkip、_wireList、_wireDirectionList、_toProcess | Dictionary / DoubleStack | 当前线路传播的访问、队列和方向工作集 | 缓存 | 单次传播工作集 | WirePropagationScratchComponent | confirmed / partial | Wiring.cs:21-27,468-503,666-680 |
| _GatesCurrent、_GatesNext、_LampsToCheck、_GatesDone、_PixelBoxTriggers | Queue / Dictionary | 逻辑门、灯和像素盒工作集 | 缓存 | 单次传播或一次逻辑门检查 | WirePropagationScratchComponent | confirmed / partial | Wiring.cs:29-37,387-393,609-665 |
| _teleport | Vector2[2] | 当前传播发现的传送槽 | 快照 / 缓存 | 单次传播 | WirePropagationScratchComponent | confirmed / unresolved | Wiring.cs:39,105-109,495-509 |
| _inPumpX/Y、_outPumpX/Y、_numInPump、_numOutPump | int[] / int | 当前传播发现的泵端点及数量 | 缓存 | 单次传播 | PumpTransferScratchComponent | confirmed / partial | Wiring.cs:41-53,101-104,499-508 |
| _mechX、_mechY、_mechTime、_numMechs | int[] / int | 机械位置和剩余机械时间 | 权威状态候选 | 加入机械冷却至过期或清除 | MechanismCooldownComponent | confirmed / partial | Wiring.cs:55-63,151-180 |
| cannonCoolDown、bunnyCannonCoolDown、snowballCannonCoolDown | int | 三类炮冷却计数 | 权威状态候选 | 初始化至归零或清除 | MechanismCooldownComponent | confirmed | Wiring.cs:69-73,151-166 |
| CurrentUser、SetCurrentUser 的 0..255 约束 | int | 触发线路操作的玩家上下文 | 兼容字段；目标为显式上下文 | 一次交互或边界请求 | InteractionActorContextComponent | confirmed / partial | Wiring.cs:67,77-86 |
| TileEntity.Position | Point16 | TileEntity 原点/锚点 | 权威状态 | 实例创建至移除 | TileEntityAnchorComponent | confirmed | TileEntity.cs:27-30,80-92,110-127 |
| TileEntity.type | byte | TileEntity 类型注册 ID | 权威状态 / 兼容字段 | 类型注册至实例移除 | TileEntityKindComponent | confirmed | TileEntity.cs:31,180-188；TileEntitiesManager.cs:30-54 |
| TileEntity.ID、ByID | int / Dictionary | 当前运行时实例 ID 和索引 | 权威运行时身份 | 当前世界会话 | TileEntityRuntimeIdComponent | confirmed / partial | TileEntity.cs:21,25,27,80-92,159-169 |
| TileEntitiesNextID、存档写入的 ID | int | ID 分配游标和存档字段 | 兼容字段；持久化含义未锁定 | 世界加载/保存 | TileEntityPersistenceIdentityComponent | partial / unresolved | TileEntity.cs:25,39-46,191-214 |
| networkSend 分支和 TileEntity 网络区域 | bool / 消息字段 | 网络身份是否另行存在 | 兼容字段候选 | 网络连接或消息会话 | TileEntityNetworkIdentityComponent | partial / unresolved | TileEntity.cs:191-214；MessageBuffer.cs:2535-2580 |
| TileEntity.RequiresUpdates、UpdateEntities | bool / List | 是否加入更新实例集合 | 权威资格 / 派生索引 | 创建、资格变化、移除 | TileEntityUpdateScheduleComponent | confirmed / partial | TileEntity.cs:19,33,80-92,110-120 |
| TELogicSensor.logicCheck、On、CountedData | enum / bool / int | 传感器类型、当前状态和短暂计数记忆 | 权威状态 / 状态记忆 | 合法实例至失效移除 | LogicSensorComponent | confirmed / partial | TELogicSensor.cs:33-37,109-172,217-285 |
| TETrainingDummy.npc、activationRetryCooldown | int / int | 训练假人关联对象和激活重试状态 | 关系状态 / 缓存 | 实例创建至关联失效 | TrainingDummyComponent | partial | TETrainingDummy.cs:8,16-18,51-63,113 |
| TEItemFrame.item、TEFoodPlatter.item、TEWeaponsRack.item | Item | 单个承载物品的 type/prefix/stack | 权威 payload | 宿主创建、替换、掉落、移除 | 对应单物品 Component | confirmed / partial | TEItemFrame.cs:6-13,40-47；TEFoodPlatter.cs:7-14,41-58；TEWeaponsRack.cs:7-16,44-61 |
| TEHatRack._items、_dyes | Item[2] / Item[2] | 两组各两个槽位 | 权威 payload | 宿主创建至移除 | HatRackComponent | confirmed / partial | TEHatRack.cs:11-41,50-109,229-241 |
| TEDisplayDoll._equip、_dyes、_misc、pose | Item[9] / Item[9] / Item[1] / pose | 多槽承载和姿态 | 权威 payload / 状态 | 宿主创建至移除 | DisplayDollComponent | confirmed / partial | TEDisplayDoll.cs:14-18,33-37,63-73 |
| TELeashedEntityAnchorWithItem.itemType | int | 系绳锚点中的可选物品类型 | 权威 payload | 锚点创建、替换、结构破坏 | LeashedEntityAnchorComponent | confirmed / partial | TELeashedEntityAnchorWithItem.cs:6-32 |
| Chest.x、y、index、maxItems、bankChest、name | int / bool / string | Chest 锚点、索引、容量和分类 | 权威结构绑定 / 兼容字段 | 创建、索引、销毁、恢复 | ChestStructureComponent | confirmed / partial | Chest.cs:38-52,68-110 |
| Chest.item | Item[] | Chest 多槽物品 payload | 权威 payload | 与 Chest 同生存期，容量变化时调整 | MultiSlotItemPayloadComponent | confirmed / partial | Chest.cs:34-42,76-107 |
| PylonRegistryState.CurrentPylons、PreviousPylons、RefreshCooldownTicksRemaining、Revision | List / int / uint | 世界级 Pylon 注册快照和 revision | 快照 / 权威候选 | 加载、刷新、恢复、卸载 | PylonRegistryComponent | partial | NLTX src\WorldStorage\PylonRegistryState.cs:1-12 |
| PylonRegistryEntry.Position、Kind、TileEntityId、IsValid | record struct | Pylon 坐标、种类、关联 ID 和合法性 | 快照；CanTeleport 为派生值 | 注册、合法性变化、移除 | PylonRegistryComponent | partial | NLTX src\WorldStorage\PylonRegistryEntry.cs:1-10 |

## 5. Component 定义

所有条目均是 Component 数据边界。status: existing 表示当前 NLTX 已有同名数据组件且字段形状可直接对应；status: partial 表示已有局部模型但行为、字段或 owner 不完整；status: proposed 表示当前没有可直接作为目标实现的 Component。未决 owner 统一标记 crossSubsystemOwner: integration-review。

### 5.1 TileCellComponent

componentId: WIS-COMP-01
name: TileCellComponent
status: partial
componentOwner: WorldInteraction，最终 Tile/Storage 写入关系未决
crossSubsystemOwner: integration-review
entityScope: 每个 Tile 单元
lifecycle: 世界或 Section 建立时以零值创建；放置、破坏、替换、液体变更和恢复时更新；清理或卸载时删除或重置

#### 职责

保存一个 Tile 单元的结构事实：类型、激活状态、墙体、液体语义和原始 header。不保存传播队列或外部效果。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| TileType | ushort | 0 | 权威状态 | 类型解释必须与 active 和 header 语义一致 | confirmed | NLTX src\WorldInteraction\Tiles\TileCellComponent.cs:1-9；Version4 Terraria\Tile.cs:8,68-79 |
| IsActive | bool | false | 权威状态 | active 语义与结构合法性一致 | confirmed / partial | 同上 |
| WallType | ushort | 0 | 权威状态 | 墙体类型不由 frame 或线路字段替代 | confirmed | 同上 |
| LiquidAmount | byte | 0 | 权威状态 | 0 表示无液体 | confirmed / partial | Version4 Tile.cs:12；NLTX 当前组件 |
| LiquidKind | LiquidKind | LiquidKind.Water | 权威状态 | 只允许已知液体枚举；与 LiquidAmount 共同解释 | confirmed / partial | NLTX src\WorldInteraction\Tiles\LiquidKind.cs:1-9；Tile.cs:56-62 |
| StructuralHeader | ushort | 0 | 权威兼容字段 | 未映射位不得因拆分而丢失 | confirmed / unresolved | Version4 Tile.cs:14；NLTX src\WorldStorage\TileCellState.cs:14 |
| Header1、Header2、Header3 | byte | 0 | 权威兼容字段 | header 位语义必须与 active、线路和 actuator 解释一致 | confirmed / unresolved | Version4 Tile.cs:16-20；TileCellState.cs:5-7 |

#### 字段不变量

Tile 结构字段与 header 是同一单元的原始事实，必须共同恢复和共同清理；液体量和种类必须成对解释。IsCheckingLiquid 等暂态标记不放入本 Component。

#### 当前 NLTX 映射

NLTX 已有 TileCellComponent 的前五个语义字段；raw header 和完整液体工作标记未覆盖，因此为 partial。

#### 证据

Version4 Tile 构造函数把核心字段初始化为零，见 Terraria\Tile.cs:6-24,68-79；当前字段见 NLTX src\WorldInteraction\Tiles\TileCellComponent.cs:1-9。

### 5.2 TileFrameComponent

componentId: WIS-COMP-02
name: TileFrameComponent
status: existing
componentOwner: WorldInteraction / WorldStorage 关系未决
crossSubsystemOwner: integration-review
entityScope: 每个 Tile 单元
lifecycle: 与 Tile 共同创建和清理；放置、frame 重算、switch、传感器状态变化和恢复时更新

#### 职责

保存 Tile 和墙体的 frame 坐标。frame 是结构几何事实，但不是 Tile 类型、线路拓扑或液体工作状态。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| TileFrameX、TileFrameY | short | 0 | 权威状态 | frame 组合必须对应所属 Tile 的合法结构样式 | confirmed | NLTX src\WorldInteraction\Tiles\TileFrameComponent.cs:1-9；Version4 Tile.cs:22-24 |
| WallFrameX、WallFrameY | short | 0 | 权威状态 | 墙体 frame 不能脱离 WallType 单独成为结构事实 | confirmed / partial | 当前组件；WorldGen.cs:67181-67195,68021-68084 |

#### 字段不变量

四个 frame 字段共同表示同一 Tile 单元的 frame 状态；它们与 TileCellComponent 相关联，但不与线路传播缓存合并。

#### 当前 NLTX 映射

NLTX src\WorldInteraction\Tiles\TileFrameComponent.cs 已覆盖字段形状；完整写入 owner 和恢复闭环仍未锁定。

#### 证据

Version4 Tile 直接持有四个 frame 坐标；WorldGen 的 framing 区间证明其可独立变化。

### 5.3 TileSignalTopologyComponent

componentId: WIS-COMP-03
name: TileSignalTopologyComponent
status: existing
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个 Tile 单元
lifecycle: Tile 创建时以 false 创建；线路或 actuator 放置/移除时更新；Tile 清理时清零

#### 职责

保存一个 Tile 的四色线路、actuator 和 actuated 拓扑事实。不保存线路访问队列、逻辑门队列或冷却计数。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| HasWire1、HasWire2、HasWire3、HasWire4 | bool | false | 权威状态 | 每个 bool 对应独立 wire 位；不得用传播缓存替代 | confirmed | NLTX src\WorldInteraction\Tiles\TileSignalTopologyComponent.cs:1-11；Version4 Wiring.cs:468-550 |
| HasActuator | bool | false | 权威状态 | actuator 存在与 actuated 状态分开 | confirmed | 当前组件；Wiring.cs:395-414 |
| IsActuated | bool | false | 权威状态 | 只有存在 actuator 时才具有有效 actuated 语义 | confirmed / partial | 当前组件；Version4 Tile.cs:14-20 |

#### 字段不变量

拓扑字段必须作为一个 Tile 级集合共同清除，但与 frame 和 Tile 结构分开。IsActuated 不得被误读为 IsActive。

#### 当前 NLTX 映射

NLTX src\WorldInteraction\Tiles\TileSignalTopologyComponent.cs 已有同名字段，status: existing；完整传播行为不由该状态证明。

#### 证据

Version4 Wiring.Actuate 见 Wiring.cs:395-414；TripWire 按不同 wire 类型读取拓扑，见 Wiring.cs:468-550。

### 5.4 TileLiquidWorkStateComponent

componentId: WIS-COMP-04
name: TileLiquidWorkStateComponent
status: proposed
componentOwner: LiquidSimulation 或 WorldStorage 候选
crossSubsystemOwner: integration-review
entityScope: 每个 Tile 的可选液体工作状态
lifecycle: 液体工作排队时激活；工作完成、取消、Section 卸载或 Tile 清除时复位

#### 职责

保存单 Tile 的液体工作标记和液体变更 revision。它不复制液体量和种类。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| IsCheckingLiquid | bool | false | 缓存 / 工作状态 | 仅在存在液体工作候选时为 true | confirmed | NLTX src\WorldStorage\TileCellState.cs:8,18-19 |
| LastLiquidChangedRevision | uint | 0 | 兼容快照 | revision 单调性与实际 owner 的计数规则一致 | confirmed / unresolved | TileCellState.cs:10 |
| ShouldSkipLiquid | bool | false | 缓存 | 清理或完成后必须复位 | confirmed | TileCellState.cs:11 |

#### 字段不变量

这些字段不能成为液体量的第二份权威副本；revision 的 World 还是 Section 范围尚未确定。泵端点工作集不放入本 Component。

#### 当前 NLTX 映射

NLTX TileCellState 有局部字段，但 src\WorldInteraction 没有同名 Component；目标为 proposed，当前覆盖为 partial。

#### 证据

字段直接见 NLTX src\WorldStorage\TileCellState.cs:3-23；Version4 PlaceLiquid 位于 WorldGen.cs:4478-4526，但不锁定工作字段最终 owner。

### 5.5 PressurePlateOccupancyComponent

componentId: WIS-COMP-05
name: PressurePlateOccupancyComponent
status: partial
componentOwner: WorldInteraction 候选
crossSubsystemOwner: integration-review
entityScope: World 或 Section 级压力板到实体的关系集合
lifecycle: 世界加载或关系集合建立时创建；实体进入/离开、Tile 失效时更新；世界清理时清空

#### 职责

保存压力板坐标到实体占用集合的关系状态及首次更新标记。不保存压力板 Tile 的结构字段。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| OccupantsByPlate | IReadOnlyDictionary<TileCoordinate, IReadOnlySet<EntityReference>> | 空字典 | 权威关系状态的只读快照 | occupant 只能出现在有效压力板坐标；外部视图不能反向修改 owner | partial | NLTX src\WorldInteraction\PressurePlates\PressurePlateOccupancyComponent.cs:1-25 |
| NeedsFirstUpdate | bool | false | 生命周期标记 | 初始化/恢复语义必须与首次占用检查一致 | partial / unresolved | 当前组件同上 |

#### 字段不变量

实体集合与压力板坐标必须共同更新；实体销毁、离开有效空间或压力板清除时移除关系。最终写入 owner 未裁决。

#### 当前 NLTX 映射

同名 Component 已存在，status: partial；当前只证明关系字段轮廓。

#### 证据

Version4 TileEntity.IsOccupied 的玩家槽位扫描见 TileEntity.cs:229-244；它证明占用概念存在，但不能锁定当前关系集合的唯一 owner。

### 5.6 MechanismCooldownComponent

componentId: WIS-COMP-06
name: MechanismCooldownComponent
status: partial
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: World 或 Section 级机械冷却集合
lifecycle: 初始化为空；机械触发时加入或更新；每个 Tick 递减；过期、越界或清理时移除

#### 职责

保存已登记机械位置的剩余时间和三类炮的独立冷却计数。不承载线路传播临时队列。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Entries | IReadOnlyList<MechanismCooldownEntry> | 空列表 | 权威状态的只读视图 | 同一坐标不能有冲突活跃 entry；RemainingTicks 不得为负 | confirmed / partial | NLTX src\WorldInteraction\Wiring\MechanismCooldownComponent.cs:1-14；Version4 Wiring.cs:55-63,151-180 |
| CannonCooldownTicks | int | 0 | 权威状态 | 大于零时递减至零，不得负数 | confirmed | 当前组件；Wiring.cs:69,155-158 |
| BunnyCannonCooldownTicks | int | 0 | 权威状态 | 大于零时递减至零，不得负数 | confirmed | 当前组件；Wiring.cs:71,159-162 |
| SnowballCannonCooldownTicks | int | 0 | 权威状态 | 大于零时递减至零，不得负数 | confirmed | 当前组件；Wiring.cs:73,163-166 |

#### 字段不变量

Entries 与三类炮冷却共同属于机械冷却生命周期，但不能和传播 scratch 共用清空操作。MechanismCooldownEntry 当前使用 TileCoordinate Position 和 int RemainingTicks。

#### 当前 NLTX 映射

同名 Component 和 Entry 已存在，status: partial；局部验证只覆盖数据形状和默认值，不证明 Version4 语义等价。

#### 证据

当前字段见 NLTX src\WorldInteraction\Wiring\MechanismCooldownComponent.cs:1-14、MechanismCooldownEntry.cs:1-8；Version4 Tick 处理见 Wiring.cs:151-180。

### 5.7 WirePropagationScratchComponent

componentId: WIS-COMP-07
name: WirePropagationScratchComponent
status: proposed
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 一次线路传播工作集
lifecycle: 传播开始时创建或清空；传播期间填充；成功、拒绝、异常或取消时全部清空，不恢复到世界存档

#### 职责

保存一次线路传播所需的访问、方向、逻辑门、像素盒和传送暂存数据。所有字段都是缓存或快照，不是 Tile 的权威拓扑。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| SkippedTiles | Set<TileCoordinate> | 空集合 | 缓存 | 只服务当前传播，完成后清空 | confirmed / partial | Version4 Wiring.cs:21,115-127,666-689 |
| Frontier | Deque<TileCoordinate> | 空队列 | 缓存 | 坐标与方向项一一对应 | confirmed / partial | Wiring.cs:23,666-684 |
| FrontierDirections | Deque<byte> | 空队列 | 缓存 | 与 Frontier 位置同步 | confirmed | Wiring.cs:25,670-684 |
| TilesToProcess | Map<TileCoordinate, byte> | 空映射 | 缓存 | 同一坐标只保留一个传播标记 | confirmed | Wiring.cs:27,675-680 |
| CurrentGates、NextGates、LampsToCheck、CompletedGates | 队列 / 集合 | 空 | 缓存 | 逻辑门坐标去重，完成后清空 | confirmed / partial | Wiring.cs:29-35,625-665 |
| PixelBoxTriggers | Map<TileCoordinate, byte> | 空映射 | 缓存 | 每个像素盒只保留当前触发标记 | confirmed | Wiring.cs:37,609-623 |
| TeleportTargets | TileCoordinate?[]，两个槽 | 两个空槽 | 快照 / 缓存 | 只表示当前传播发现的端点；具体类型需裁决 | confirmed / unresolved | Wiring.cs:39,105-109,495-509 |
| CurrentWireColor | byte? | null | 缓存 | 只允许已知线路颜色；不成为拓扑副本 | confirmed / partial | Wiring.cs:65,679 |
| IsRunning | bool | false | 缓存 | 工作结束时复位 | confirmed | Wiring.cs:468-473 |
| BlockPlayerTeleportationForOneIteration | bool | false | 兼容快照 | 只对当前传播有效，不跨请求持久化 | confirmed / partial | Wiring.cs:17,659-662 |

#### 字段不变量

scratch 只在单次请求范围内存在；不能挂在每个 wire node 上，也不能成为 TileSignalTopologyComponent 的隐式写者。传送端点类型和外部关系未锁定。

#### 当前 NLTX 映射

没有可证明覆盖完整工作集的直接 Component，目标为 proposed。局部线路文件不能证明完整传播 scratch 已接线。

#### 证据

Version4 初始化和清理见 Wiring.cs:88-149；线路传播见 Wiring.cs:468-550,609-665。

### 5.8 PumpTransferScratchComponent

componentId: WIS-COMP-08
name: PumpTransferScratchComponent
status: proposed
componentOwner: WorldInteraction 候选
crossSubsystemOwner: integration-review
entityScope: 一次传播发现的泵端点工作集
lifecycle: 每种线路颜色传播开始时清空并登记；该颜色处理完成后清空；不跨请求保留

#### 职责

保存一次线路传播发现的输入泵和输出泵坐标。不保存液体量、液体类型或转移结果。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| InputPumpPositions | TileCoordinate[20] 候选 | 20 个空坐标槽 | 缓存 | InputPumpCount 不超过 20 | confirmed / partial | Wiring.cs:41-47,101-104 |
| OutputPumpPositions | TileCoordinate[20] 候选 | 20 个空坐标槽 | 缓存 | OutputPumpCount 不超过 20 | confirmed / partial | 同上 |
| InputPumpCount、OutputPumpCount | int | 0 | 缓存 | 范围 0..20，清理时归零 | confirmed | Wiring.cs:47,53,501-507 |

#### 字段不变量

端点和数量共同构成一次传播暂存集合，数量不能单独持久化。液体转移规则、失败语义和跨域 owner 未由 Version4 当前删减实现完整证明。

#### 当前 NLTX 映射

无直接 Component，status: proposed；LiquidSchedulerState 等存储轮廓不等于泵工作集覆盖。

#### 证据

Version4 MaxPump=20 及四个坐标数组见 Wiring.cs:41-53,101-104；XferWater 在 Wiring.cs:467 为空体，故转移语义为 partial。

### 5.9 InteractionActorContextComponent

componentId: WIS-COMP-09
name: InteractionActorContextComponent
status: proposed
componentOwner: WorldInteraction 候选
crossSubsystemOwner: integration-review
entityScope: 一次交互上下文
lifecycle: 交互入口建立；交互期间只读；请求结束、拒绝或异常时清除

#### 职责

把 Version4 的隐式 CurrentUser 转成一次交互范围内的显式触发者上下文。它不是世界事实，不写入 Tile 或 TileEntity。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| InitiatingPlayerIndex | byte? 候选 | null；兼容映射为 255 | 兼容字段 / 快照 | 有效值为 0..254；无调用者不得伪装成有效玩家 | confirmed / unresolved | Wiring.cs:67,77-86 |

#### 字段不变量

上下文必须随一次交互创建和清除，不得作为静态 World 字段残留；玩家索引到 NLTX EntityReference 的映射由整合裁决。

#### 当前 NLTX 映射

无直接 Component，status: proposed。

#### 证据

SetCurrentUser 将无效值归一为 255，见 Version4 Wiring.cs:67,77-86。

### 5.10 TileEntityAnchorComponent

componentId: WIS-COMP-10
name: TileEntityAnchorComponent
status: existing
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个 TileEntity 实体
lifecycle: 合法实例生成时设置；结构恢复或定位变化时更新；实体移除时清理

#### 职责

保存一个 TileEntity 的 Tile 原点，用于定位、合法性和多格结构关联。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Origin | TileCoordinate | (0,0) 零值；未放置语义由生命周期区分 | 权威状态 | 位置索引必须与 anchor 一致；有效 anchor 不应绑定两个同类实例 | confirmed / partial | NLTX src\WorldInteraction\TileEntities\TileEntityAnchorComponent.cs:1-7；Version4 TileEntity.cs:23,27-30,80-92 |

#### 字段不变量

Origin 是实体锚点，不是 footprint 的所有格子列表。多格覆盖由 StructureFootprintComponent 表达并与 Origin 一起校验。

#### 当前 NLTX 映射

同名 Component 已存在，status: existing；索引一致性和多格完整性仍需整合裁决。

#### 证据

Version4 ByPosition 使用 Position 建立位置索引，见 TileEntity.cs:21-23,80-92,147-157。

### 5.11 TileEntityKindComponent

componentId: WIS-COMP-11
name: TileEntityKindComponent
status: existing
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个 TileEntity 实体
lifecycle: 类型注册后可创建；实例生成时设置；恢复时先校验；移除时清理

#### 职责

保存 TileEntity 的种类标识，使种类与位置、实例 ID 分离。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Kind | TileEntityKindId | Value=0 | 权威兼容字段 | 必须映射到已注册类型；0 的保留含义由注册边界裁决 | confirmed / partial | NLTX src\WorldInteraction\TileEntities\TileEntityKindComponent.cs:1-7；Version4 TileEntitiesManager.cs:30-65 |

#### 字段不变量

Kind 不是 TileType，也不是 runtime ID；合法性检查以种类和锚点 Tile 组合判断。

#### 当前 NLTX 映射

同名 Component 已存在，status: existing；注册表和恢复拒绝规则未完整覆盖。

#### 证据

Version4 RegisterAll 注册 11 类类型，CheckValidTile 以类型 ID 检查合法 Tile，见 TileEntitiesManager.cs:30-65。

### 5.12 TileEntityRuntimeIdComponent

componentId: WIS-COMP-12
name: TileEntityRuntimeIdComponent
status: partial
componentOwner: WorldInteraction 候选
crossSubsystemOwner: integration-review
entityScope: 每个 TileEntity 实体；索引为 World 级
lifecycle: 实例登记时分配；当前会话保持；移除或世界清理时释放

#### 职责

保存当前世界会话内 TileEntity 的运行时实例 ID，不宣称持久化或网络身份。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Id | TileEntityRuntimeId | Value=0 | 权威运行时状态 | 活跃实体不得共享 ID；与按 ID 索引同步 | confirmed / partial | NLTX src\WorldInteraction\TileEntities\TileEntityRuntimeIdComponent.cs:1-7；Version4 TileEntity.cs:21,25,27,80-92 |

#### 字段不变量

ID 只表示运行时实例身份；不得因为 Version4 存档写入 ID 就默认它等于长期持久化 ID。移除必须同时移除 ID 索引。

#### 当前 NLTX 映射

同名 Component 已存在，status: partial；包装类型存在，但与存档/网络路径尚未闭合。

#### 证据

Version4 ByID、TileEntitiesNextID 和实例 ID 见 TileEntity.cs:19-27,80-92。

### 5.13 TileEntityPersistenceIdentityComponent

componentId: WIS-COMP-13
name: TileEntityPersistenceIdentityComponent
status: proposed
componentOwner: WorldStorage 候选
crossSubsystemOwner: integration-review
entityScope: 每个可持久化 TileEntity
lifecycle: 加载时恢复或标记未分配；保存时保持；删除时的保留/回收策略未决

#### 职责

为 TileEntity 预留持久化身份边界，避免把当前会话 ID 直接当作长期存档身份。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| PersistentId | TileEntityPersistentId? 候选 | unassigned | 兼容字段 | 恢复后同一存档实体的身份应稳定；与 runtime ID 可映射但不混用 | unresolved | Version4 TileEntity.cs:191-214；WorldFile.cs:3385-3441 |

#### 字段不变量

Version4 当前只明确写入 ID，没有足够证据证明另有持久化 ID 类型；因此不补造底层表示、分配时点或旧存档映射。

#### 当前 NLTX 映射

无直接 Component，status: proposed。

#### 证据

TileEntity.WriteInner 在非网络写入中写 ID，ReadInner 读回 ID；这只能证明兼容字段存在。

### 5.14 TileEntityNetworkIdentityComponent

componentId: WIS-COMP-14
name: TileEntityNetworkIdentityComponent
status: proposed
componentOwner: NetworkSessionAndSectionStreaming 候选
crossSubsystemOwner: integration-review
entityScope: 每个连接中的 TileEntity 视图
lifecycle: 网络绑定时创建；连接范围改变时更新；断线或实体移除时清理

#### 职责

为连接范围内的 TileEntity 网络身份预留独立边界，不把网络包字段混入实体权威事实。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| NetworkId | NetworkId? 候选 | none | 兼容字段 / 快照 | 只在有效连接范围内有效；断线后不可当作 runtime ID | unresolved | Version4 TileEntity.cs:191-214；MessageBuffer.cs:2535-2580 |

#### 字段不变量

Version4 网络写入省略 ID，但当前证据不足以确定网络身份是位置、运行时 ID 还是消息上下文；不补造数值，也不宣布最终 owner。

#### 当前 NLTX 映射

无直接 Component，status: proposed。

#### 证据

当前消息区间只证明 TileEntity 网络区域形状不完整，故字段为 unresolved。

### 5.15 TileEntityUpdateScheduleComponent

componentId: WIS-COMP-15
name: TileEntityUpdateScheduleComponent
status: existing
componentOwner: WorldInteraction 候选
crossSubsystemOwner: integration-review
entityScope: 每个 TileEntity 实体；登记为 World 级索引
lifecycle: 实例生成时初始化；资格变化时登记/取消登记；实体移除时清理

#### 职责

保存 TileEntity 是否需要进入更新实例集合的资格状态。不保存更新队列本身。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| RequiresUpdates | bool | false；Logic Sensor 创建时为 true | 权威资格 | true 时实体必须有对应登记；移除时登记必须消失 | confirmed / partial | NLTX src\WorldInteraction\TileEntities\TileEntityUpdateScheduleComponent.cs:1-7；Version4 TileEntity.cs:19,33,80-92,110-120；TELogicSensor.cs:167-172 |

#### 字段不变量

该字段是实体资格，不等同于更新时间、队列位置或 Tick 计数。状态变化必须与登记集合保持一致。

#### 当前 NLTX 映射

同名 Component 已存在，status: existing；WorldStorage 另有 schedule 状态，跨边界仍需整合。

#### 证据

Version4 Add 在 RequiresUpdates 为 true 时加入 UpdateEntities，见 TileEntity.cs:80-92。

### 5.16 LogicSensorComponent

componentId: WIS-COMP-16
name: LogicSensorComponent
status: partial
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个逻辑传感器 TileEntity
lifecycle: 合法宿主通过检查后创建；更新时读取外部事实；宿主失效时删除；加载时恢复可持久化字段

#### 职责

保存逻辑传感器的检查类型、当前 on 状态和短暂状态记忆。日期、玩家空间和液体是读取到的外部事实，不复制进本 Component。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| CheckType | LogicCheckType | None | 权威状态 | 只能使用已知检查类型；必须与宿主 Tile frame 兼容 | confirmed / partial | Version4 TELogicSensor.cs:11-21,33,156-172；NLTX LogicSensorComponent.cs:1-7 |
| IsOn | bool | false | 权威状态 | frame 状态和字段不能长期矛盾 | confirmed | Version4 TELogicSensor.cs:35,109-154 |
| CountedData | int | 0 | 权威状态记忆 / 缓存候选 | 液体暂时失效时按已证规则倒计时，不得为负 | confirmed / partial | Version4 TELogicSensor.cs:37,241-280 |

#### 字段不变量

三字段共同表达一个传感器实体，但 CountedData 不是普通物品计数；Tile frame 不能替代 CheckType 的类型语义。

#### 当前 NLTX 映射

同名 Component 已存在，status: partial；外部事实读取、失效清理和删减成员未被局部验证覆盖。

#### 证据

实际类型路径和字段见 D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs:9-37,109-172,217-285。

### 5.17 TrainingDummyComponent

componentId: WIS-COMP-17
name: TrainingDummyComponent
status: partial
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个训练假人 TileEntity
lifecycle: 合法宿主创建；NPC 关系建立/失效时更新；宿主移除时清理关系

#### 职责

保存训练假人与 NPC 的关系引用和激活重试计数，不保存 NPC 的完整状态。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Npc | EntityReference | EntityReference.None | 关系状态 | 只能引用有效 NPC scope；NPC 销毁后引用必须失效 | partial | NLTX src\WorldInteraction\TileEntities\TrainingDummyComponent.cs:1-8；Version4 TETrainingDummy.cs:8,16-18,51-63 |
| ActivationRetryCooldownTicks | int | 0 | 缓存 / 状态记忆 | 不得为负；只服务当前关联重试 | partial / unresolved | 当前组件；Version4 TETrainingDummy.cs:16-18 |
| IsActive | bool | 由 Npc.IsEmpty 派生 | 派生值 | 不作为第二份可写状态 | confirmed / partial | NLTX TrainingDummyComponent.cs:6-7 |

#### 字段不变量

NPC 全部状态仍归 NPC 实体；训练假人只保存关系和必要的重试记忆。IsActive 不单独存储。

#### 当前 NLTX 映射

同名 Component 已存在，status: partial；Version4 关联生命周期尚未闭合。

#### 证据

当前模型明确使用 EntityReference.None 默认值；Version4 训练假人字段见 TETrainingDummy.cs:8-18,51-63,113。

### 5.18 ItemFrameComponent

componentId: WIS-COMP-18
name: ItemFrameComponent
status: partial
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个 Item Frame TileEntity
lifecycle: 合法宿主创建；放置/替换时更新；掉落或删除时清空；恢复时按 tuple 兼容规则读取

#### 职责

保存 Item Frame 宿主的单个物品 payload。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Item | StoredItemState | type=0,prefix=0,stack=0 的空值 | 权威 payload | Type、Prefix、Stack 作为一个 tuple 验证；空物品不得有正 stack | confirmed / partial | NLTX src\WorldInteraction\TileEntities\ItemFrameComponent.cs:1-7、StoredItemState.cs:1-7；Version4 TEItemFrame.cs:6-13,40-47 |

#### 字段不变量

StoredItemState 是 value object，不是独立 Component。宿主破坏时 payload 只能消费一次；具体外部效果不在本文件设计。

#### 当前 NLTX 映射

ItemFrameComponent 和 StoredItemState 已存在，status: partial；完整合法性、掉落和恢复链未覆盖。

#### 证据

Version4 构造创建空 Item，读写顺序为 type/prefix/stack，见 TEItemFrame.cs:6-13,40-47。

### 5.19 FoodPlatterComponent

componentId: WIS-COMP-19
name: FoodPlatterComponent
status: partial
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个 Food Platter TileEntity
lifecycle: 合法宿主创建；物品替换、掉落、恢复和移除时更新或清理

#### 职责

保存 Food Platter 宿主的单个物品 payload。它不与 Item Frame 合并，因为宿主 Tile、合法性和清理边界不同。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Item | StoredItemState | 空值 | 权威 payload | tuple 原子一致；空 payload 不得产生正 stack | confirmed / partial | NLTX FoodPlatterComponent.cs:1-7；Version4 TEFoodPlatter.cs:7-14,41-58 |

#### 字段不变量

物品 payload 与 Food Platter 宿主共同创建/删除；不能把单物品的类型化清理规则推广为所有承载宿主的统一行为。

#### 当前 NLTX 映射

同名 Component 已存在，status: partial；完整宿主生命周期和版本读写未覆盖。

#### 证据

Version4 TЕFoodPlatter 构造和读写见 D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEFoodPlatter.cs:7-14,41-58。

### 5.20 WeaponRackComponent

componentId: WIS-COMP-20
name: WeaponRackComponent
status: partial
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个 Weapon Rack TileEntity
lifecycle: 合法宿主创建；物品写入、替换、掉落或恢复时更新；宿主破坏时清理

#### 职责

保存 Weapon Rack 的单个物品 payload；结构 footprint 和物品内容分开。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Item | StoredItemState | 空值 | 权威 payload | 宿主不可破坏性检查必须读取该 tuple 的空/非空状态 | confirmed / partial | NLTX WeaponRackComponent.cs:1-7；Version4 TEWeaponsRack.cs:7-16,44-61,97-114 |

#### 字段不变量

Weapon Rack 的 item 非空会影响结构破坏资格；该依赖是组合不变量，不把 footprint 和 payload 合并。

#### 当前 NLTX 映射

同名 Component 已存在，status: partial；完整 footprint 和非空破坏闭环未覆盖。

#### 证据

Version4 单 item 读写和非空破坏路径见 TEWeaponsRack.cs:44-61,97-114。

### 5.21 HatRackComponent

componentId: WIS-COMP-21
name: HatRackComponent
status: existing
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个 Hat Rack TileEntity
lifecycle: 宿主创建时建立 2+2 空槽；槽位操作和恢复时更新；宿主破坏/移除时清理

#### 职责

保存 Hat Rack 的装备槽和 dye 槽。两组固定容量共同维护空槽和宿主破坏条件。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Items | StoredItemState[2] 的只读视图 | 两个空槽 | 权威 payload | 长度固定为 2；每个槽 tuple 自洽 | confirmed | NLTX HatRackComponent.cs:1-18；Version4 TEHatRack.cs:21-32 |
| Dyes | StoredItemState[2] 的只读视图 | 两个空槽 | 权威 payload | 长度固定为 2；dye 槽与装备槽不能混用 | confirmed | 当前组件；TEHatRack.cs:23-36 |
| ContainsItems | bool 派生 | false | 派生值 | 两组槽位至少一个非空时为 true | confirmed | NLTX HatRackComponent.cs:17-18；TEHatRack.cs:229-241 |

#### 字段不变量

装备和 dye 两组槽位共同参与 ContainsItems；数组长度是结构契约，不能降级为不定长通用容器。

#### 当前 NLTX 映射

同名 Component 已存在，status: existing；Version4 完整宿主校验和恢复未覆盖。

#### 证据

Version4 构造创建两组长度为 2 的 Item 数组，读写和空内容破坏检查见 TEHatRack.cs:21-41,50-109,214-257。

### 5.22 DisplayDollComponent

componentId: WIS-COMP-22
name: DisplayDollComponent
status: partial
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个 Display Doll TileEntity
lifecycle: 宿主创建时建立固定槽位；槽位/姿态改变、恢复和宿主移除时更新或清理

#### 职责

保存 Display Doll 的装备、dye、miscellaneous 槽位和姿态。表现所需的派生外观不作为权威字段复制。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Equipment | StoredItemState[9] 的只读视图 | 9 个空槽 | 权威 payload | 固定长度 9；槽序不能改变 | confirmed / partial | NLTX DisplayDollComponent.cs:1-29；Version4 TEDisplayDoll.cs:14-18,33-37,63-73 |
| Dyes | StoredItemState[9] 的只读视图 | 9 个空槽 | 权威 payload | 固定长度 9，与 Equipment 一一对应 | confirmed / partial | 同上 |
| Miscellaneous | StoredItemState[1] 的只读视图 | 1 个空槽 | 权威 payload | 固定长度 1 | confirmed / partial | 同上 |
| Pose | byte | 0 | 权威状态 | 只允许已知姿态编码；未知值恢复策略需补证 | partial | NLTX DisplayDollComponent.cs:20-24；Version4 TEDisplayDoll.cs:16-18,386-388 |
| ContainsItems | bool 派生 | false | 派生值 | 三组槽位全空时为 false | confirmed / partial | 当前组件 |

#### 字段不变量

三组槽位共同决定宿主是否为空，但不同槽位资格仍由宿主规则决定。Pose 与 payload 同属一个生命周期，但不是 item tuple 的一部分。

#### 当前 NLTX 映射

同名 Component 已存在，status: partial；默认槽位已覆盖，Version4 版本差异和完整生命周期未锁定。

#### 证据

当前组件的 9/9/1 数组和 pose 见 NLTX src\WorldInteraction\TileEntities\DisplayDollComponent.cs:1-29；Version4 字段见 TEDisplayDoll.cs:14-18,33-37,63-73。

### 5.23 DeadCellsDisplayJarComponent

componentId: WIS-COMP-23
name: DeadCellsDisplayJarComponent
status: partial
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个 Dead Cells Display Jar TileEntity
lifecycle: 宿主创建、放置、替换、掉落或移除时更新；读写生命周期保持未决

#### 职责

保存 Dead Cells Display Jar 的单个物品 payload，并保留 Version4 读写证据不足的状态。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Item | StoredItemState | 空值 | 权威 payload | tuple 自洽；空值不可误解释为有效展示物 | confirmed / partial | NLTX DeadCellsDisplayJarComponent.cs:1-7；Version4 TEDeadCellsDisplayJar.cs:6-13,30-36 |

#### 字段不变量

Version4 的写读方法为空，不能从完整参考补造 Version4 的存档或网络语义；当前只锁定 payload 形状和空值约束。

#### 当前 NLTX 映射

同名 Component 已存在，status: partial。

#### 证据

TEDeadCellsDisplayJar.cs:25-26 的写读空体是明确 evidence-gap；不得声明完整持久化覆盖。

### 5.24 LeashedEntityAnchorComponent

componentId: WIS-COMP-24
name: LeashedEntityAnchorComponent
status: partial
componentOwner: WorldInteraction
crossSubsystemOwner: integration-review
entityScope: 每个带物品的 Leashed Anchor TileEntity
lifecycle: 带物品锚点创建时建立；物品替换、掉落、恢复或宿主破坏时更新/清理

#### 职责

保存带物品变体的系绳锚点中可选物品类型。锚点关系由 common TileEntity identity 和外部关系边界表达。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| ItemType | int | 0 | 权威 payload | 0 表示无物品；正值必须是有效物品类型 | confirmed / partial | NLTX LeashedEntityAnchorComponent.cs:1-8；Version4 TELeashedEntityAnchorWithItem.cs:6-32 |
| HasItem | bool 派生 | false | 派生值 | 等价于 ItemType > 0，不单独写入 | confirmed | 当前组件 |

#### 字段不变量

不带物品的系绳锚点不能因为共享名称而被强制附加正物品；该 Component 是可选组合。

#### 当前 NLTX 映射

同名 Component 已存在，status: partial；带物品/不带物品变体和掉落生命周期仍需补证。

#### 证据

当前字段和派生值见 NLTX src\WorldInteraction\TileEntities\LeashedEntityAnchorComponent.cs:1-8。

### 5.25 StructureFootprintComponent

componentId: WIS-COMP-25
name: StructureFootprintComponent
status: proposed
componentOwner: WorldInteraction 候选
crossSubsystemOwner: integration-review
entityScope: 每个多格 TileEntity 或 Chest 结构
lifecycle: 结构放置并通过合法性检查后创建；frame/支撑变化时重检；结构破坏、恢复失败或卸载时清理

#### 职责

描述多格结构相对于 Origin 的覆盖范围和宿主 Tile 约束。不保存每个覆盖 Tile 的副本，也不保存物品 payload。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Origin | TileCoordinate | 未绑定 | 权威结构绑定 | 与 common anchor 一致 | confirmed / unresolved | Version4 TETeleportationPylon.cs:26-78,119-124 |
| Width | int | 1 候选；宿主可覆盖 | 权威兼容字段 | 大于 0；Pylon 为 3 | confirmed / partial | TETeleportationPylon.cs:9-11；TEHatRack.cs:15-17 |
| Height | int | 1 候选；宿主可覆盖 | 权威兼容字段 | 大于 0；Pylon 为 4 | confirmed / partial | 同上 |
| HostTileType | ushort? | null，由宿主绑定 | 兼容字段 | 覆盖 Tile 类型必须一致；缺格时结构失效 | confirmed / unresolved | Pylon Tile 597；WorldGen.cs:50970-52939 |

#### 字段不变量

所有覆盖格必须与 Origin、Width、Height 和 HostTileType 一致；任何一格缺失或底部支撑不合法都可能使整体失效。覆盖范围是派生坐标集合，不复制整个 Tile 数组。

#### 当前 NLTX 映射

当前没有通用 footprint Component，status: proposed；多格信息散落在宿主和存储事实中。

#### 证据

Pylon 明确声明 3x4 并遍历覆盖格与底部支撑；Hat Rack 也声明 3x4。不同宿主的完整 footprint 表尚未集中证明。

### 5.26 ChestStructureComponent

componentId: WIS-COMP-26
name: ChestStructureComponent
status: proposed
componentOwner: WorldStorage 候选
crossSubsystemOwner: integration-review
entityScope: 每个世界 Chest
lifecycle: 创建时分配 anchor/slot；按坐标建立索引；结构删除时先处理 payload 再移除绑定；加载时恢复索引并拒绝重复坐标

#### 职责

保存 Chest 的结构绑定、世界索引身份和兼容分类；物品数组单独归入 MultiSlotItemPayloadComponent。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Anchor | TileCoordinate | 未绑定 | 权威结构绑定 | 与坐标索引一致 | confirmed / partial | NLTX WorldStorage\WorldChestState.cs:1-11；Version4 Chest.cs:44-48,76-92 |
| Slot | ChestSlot | 未分配 | 权威运行时 / 兼容身份 | 一个活跃 Chest 只能占一个 slot；按坐标索引回到同一 Chest | partial | 当前 WorldChestState.cs:5；Chest.cs:48,86-92 |
| IsLegacyBankChest | bool | false | 兼容分类 | bank 语义不能改变普通结构锚点 | confirmed / partial | 当前 WorldChestState.cs:9；Chest.cs:50 |
| Name | string | string.Empty | 权威容器元数据 | 长度和编码满足既有兼容约束 | confirmed / partial | 当前 WorldChestState.cs:8；Chest.cs:52 |

#### 字段不变量

Anchor、Slot 和 bank 分类共同构成 Chest 结构绑定，但不包含 item payload。Chest 是否可破坏需要同时读取本 Component、footprint 和多槽 payload。

#### 当前 NLTX 映射

WorldChestState 已有局部同构字段，但没有同名 Interaction Component；目标 status: proposed，当前覆盖 partial。

#### 证据

Version4 Chest.Assign 同时写数组索引和坐标字典，见 Chest.cs:68-92；当前字段见 WorldChestState.cs:1-11。

### 5.27 MultiSlotItemPayloadComponent

componentId: WIS-COMP-27
name: MultiSlotItemPayloadComponent
status: proposed
componentOwner: WorldStorage 候选
crossSubsystemOwner: integration-review
entityScope: 每个 Chest 或其他明确多槽容器
lifecycle: 容器创建时分配空槽；放入、取出、容量调整和恢复时更新；结构销毁时先决定 payload 保留/掉落，再清理

#### 职责

保存 Chest 这类多槽容器的 item 数组和容量。不保存结构坐标、slot、bank 分类或网络对象。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Items | ItemState[] | 空数组；实例化后长度应等于容量 | 权威 payload | 每个槽位合法；数组长度与容量一致 | confirmed / partial | NLTX WorldStorage\WorldChestState.cs:6；Version4 Chest.cs:38-42,76-84,94-107 |
| ItemCapacity | int | 0 当前 NLTX；Version4 默认常量候选为 40 | 权威结构参数 / 兼容字段 | 不小于 0，不超过已确认上限；调整不能静默丢物品 | confirmed / unresolved | 当前 WorldChestState.cs:7；Version4 Chest.cs:34-38,94-107 |

#### 字段不变量

Items 与 ItemCapacity 必须共同恢复、调整和清理；Chest 空内容破坏判断必须读取所有槽位。ItemState 不转换为 StoredItemState，除非整合边界明确制定转换。

#### 当前 NLTX 映射

无同名 Component，status: proposed；WorldChestState.Items 只提供局部存储覆盖。

#### 证据

Version4 Chest 有 maxItems 和 Item[]，并在 Resize 时调整槽数组；当前字段见 WorldChestState.cs:6-7。

### 5.28 PylonStructureComponent

componentId: WIS-COMP-28
name: PylonStructureComponent
status: proposed
componentOwner: WorldInteraction 候选
crossSubsystemOwner: integration-review
entityScope: 每个 Pylon TileEntity
lifecycle: 通过合法性检查后创建；frame/支撑变化时失效或更新；结构销毁时移除

#### 职责

保存一个 Pylon 结构的种类和宿主 Tile 约束。Pylon 注册集合不放在单个 Pylon Component 内。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| PylonKind | TeleportPylonType 候选 | unknown | 权威兼容状态 | 必须能由宿主 style/frame 推导并通过合法性检查 | confirmed / unresolved | Version4 TETeleportationPylon.cs:14-18,138-158 |
| TileType | ushort | 597 | 兼容字段 | 必须与 Pylon 宿主 Tile 类型一致 | confirmed | TETeleportationPylon.cs:7,108-117 |
| RequiresSolidSupport | bool | true 候选 | 规则状态 | 底部三个支撑格满足 Version4 支撑检查 | confirmed / partial | TETeleportationPylon.cs:56-59 |

#### 字段不变量

PylonKind 与 frame style 的推导关系必须保持；合法性是 3x4 footprint 和底部支撑共同成立。IsValid 不在此保存为第二份状态。

#### 当前 NLTX 映射

无直接 Interaction Component，status: proposed；PylonRegistryState 只能覆盖世界集合，不能证明实体结构组件已存在。

#### 证据

Pylon 实际文件、3x4、Tile 597 和支撑检查见 D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETeleportationPylon.cs:5-11,26-78,108-150。

### 5.29 PylonRegistryComponent

componentId: WIS-COMP-29
name: PylonRegistryComponent
status: partial
componentOwner: WorldStorage 候选
crossSubsystemOwner: integration-review
entityScope: World 级 Pylon 注册集合
lifecycle: 世界加载时建立；Pylon 合法性或结构变化时刷新；恢复时保留必要快照；卸载时清空

#### 职责

保存 World 级 Pylon 注册快照、刷新冷却和 revision。不复制每个 Pylon 的全部结构字段。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| CurrentPylons | List<PylonRegistryEntry> | 空列表 | 权威候选 / 快照 | 有效 Pylon 不重复；条目引用存在的结构身份 | partial | NLTX src\WorldStorage\PylonRegistryState.cs:3-5；PylonRegistryEntry.cs:3-9 |
| PreviousPylons | List<PylonRegistryEntry> | 空列表 | 快照 | 不与 current 共享可变存储 | partial | PylonRegistryState.cs:4 |
| RefreshCooldownTicksRemaining | int | 0 | 状态记忆 | 不得为负；零值语义与 HasPendingRefresh 需裁决 | partial / unresolved | PylonRegistryState.cs:7-8 |
| Revision | uint | 0 | 快照 revision | 变更时单调增加；与其他 revision 是否统一未决 | partial / unresolved | PylonRegistryState.cs:9 |

#### 字段不变量

注册条目的 Position、Kind、TileEntityId、IsValid 必须与 Pylon 实体及 footprint 保持一致；CanTeleport 是派生值，不单独存储。Current/Previous 的交换不能形成第二份 owner。

#### 当前 NLTX 映射

PylonRegistryState 和 PylonRegistryEntry 提供局部覆盖，status: partial；最终 owner 为 crossSubsystemOwner: integration-review。

#### 证据

当前字段见 PylonRegistryState.cs:1-12 和 PylonRegistryEntry.cs:1-10；Version4 Pylon 合法性与完整注册刷新边界未形成单写 owner 证据。

## 6. Entity 与 Component 组合

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| Tile 单元 | TileCellComponent、TileFrameComponent、TileSignalTopologyComponent | TileLiquidWorkStateComponent | 无 | Tile 结构、frame、线路拓扑共享坐标，但液体工作是可选暂态 |
| 压力板关系集合 | PressurePlateOccupancyComponent | InteractionActorContextComponent | 无 | 占用是 World/Section 关系，不把 occupant 引用复制到压力板 Tile |
| 机械冷却集合 | MechanismCooldownComponent | 无 | WirePropagationScratchComponent 不能承担冷却 | 冷却跨 Tick，传播 scratch 只跨一次交互 |
| 一次线路传播工作集 | WirePropagationScratchComponent | PumpTransferScratchComponent、InteractionActorContextComponent | MechanismCooldownComponent 不承担 scratch | 线路、逻辑门、像素盒和传送槽需临时共存；泵因液体 owner 不同拆开 |
| 普通 TileEntity | TileEntityAnchorComponent、TileEntityKindComponent、TileEntityRuntimeIdComponent、TileEntityUpdateScheduleComponent | TileEntityPersistenceIdentityComponent、TileEntityNetworkIdentityComponent、StructureFootprintComponent | 无 | common identity 与更新资格组成实体基础；不同 ID 生命周期不能合并 |
| 逻辑传感器 | 普通 TileEntity 组合、LogicSensorComponent | StructureFootprintComponent（1x1 候选） | 单物品或多槽 payload | 传感器状态依赖 Tile 事实，但不拥有玩家/液体实体状态 |
| 训练假人 | 普通 TileEntity 组合、TrainingDummyComponent | StructureFootprintComponent | 单物品或多槽 payload | 只保存 NPC 关系和重试记忆 |
| Item Frame | 普通 TileEntity 组合、ItemFrameComponent、StructureFootprintComponent | TileEntityPersistenceIdentityComponent | HatRackComponent、DisplayDollComponent、MultiSlotItemPayloadComponent | 单物品 tuple 和 footprint 共同约束宿主 |
| Food Platter | 普通 TileEntity 组合、FoodPlatterComponent、StructureFootprintComponent | 无 | 其他物品宿主 Component | 宿主类型和单 item 生命周期独立 |
| Weapon Rack | 普通 TileEntity 组合、WeaponRackComponent、StructureFootprintComponent | 无 | 其他物品宿主 Component | 非空 payload 影响可破坏性 |
| Hat Rack | 普通 TileEntity 组合、HatRackComponent、StructureFootprintComponent | 无 | DisplayDollComponent、MultiSlotItemPayloadComponent | 2+2 固定槽位和 3x4 结构是不变量 |
| Display Doll | 普通 TileEntity 组合、DisplayDollComponent、StructureFootprintComponent | 无 | HatRackComponent、单物品宿主 Component | 9/9/1 槽位和 pose 不与 Hat Rack 合并 |
| Dead Cells Display Jar | 普通 TileEntity 组合、DeadCellsDisplayJarComponent、StructureFootprintComponent | 无 | 其他物品宿主 Component | Version4 读写空体，保持独立 gap |
| 带物品系绳锚点 | 普通 TileEntity 组合、LeashedEntityAnchorComponent、StructureFootprintComponent | 无 | 无物品变体不得附加该 Component | ItemType=0 的可选组合表达变体 |
| Chest | ChestStructureComponent、StructureFootprintComponent、MultiSlotItemPayloadComponent | TileEntityPersistenceIdentityComponent 不默认附加 | 所有 TileEntity 类型 Component | Chest 使用坐标/slot 索引和多槽 payload，不伪装成 TileEntity |
| Pylon | 普通 TileEntity 组合、PylonStructureComponent、StructureFootprintComponent | PylonRegistryComponent 只在 World 上存在 | ChestStructureComponent、单物品宿主 Component | 实体结构与 World 注册快照分开 |
| WorldInteraction 工作上下文 | 无固定 TileEntity Component | WirePropagationScratchComponent、PumpTransferScratchComponent、InteractionActorContextComponent | 稳定 Tile 事实不挂入工作上下文 | 暂存、触发者和稳定事实生命周期不同 |

## 7. 组件拆分与合并决策

### 7.1 必须保持在一起的字段

- TileCellComponent 的类型、active、wall 和 raw header 共同描述一个 Tile 单元的原始结构事实；拆成多个可写镜像会产生 header 与语义字段漂移。
- TileFrameComponent 的 Tile frame 四元组共同恢复和清理，不能拆成两个独立 owner。
- TileSignalTopologyComponent 的四色线路、actuator 和 actuated 位共同清理，但与 frame、Tile 类型分开。
- MechanismCooldownComponent 的机械条目与三类炮计数共同属于机械冷却生命周期；单个炮计数不值得另建 Component。
- 每个单物品宿主的 StoredItemState 三元组必须共同写入、验证和清空；type、prefix、stack 不能拆成三个状态 owner。
- Hat Rack 的装备槽与 dye 槽共同决定 ContainsItems 和宿主破坏资格；Display Doll 的三组固定槽位共同决定空内容状态。
- Chest 的 Anchor、Slot、bank 分类共同构成结构绑定；Chest 的 Items 与 ItemCapacity 共同构成多槽 payload。
- Pylon 的 footprint、宿主 Tile 类型、支撑条件和 PylonKind 共同构成单实体合法结构事实，但注册快照不并入该实体。

### 7.2 必须拆开的字段

- Tile 稳定事实与 TileLiquidWorkStateComponent 拆开：液体工作标记频繁失效，不能成为液体量的第二份权威副本。
- TileSignalTopologyComponent 与 WirePropagationScratchComponent 拆开：拓扑跨世界生命周期，scratch 只跨一次传播。
- PumpTransferScratchComponent 与线路 scratch 拆开：泵的液体转移 owner 与 LiquidSimulation 的关系未决，不能隐藏在线路缓存中。
- TileEntity anchor、kind、runtime、persistent、network 身份不合并成一个大 ID Component：它们的生命周期分别是实体位置、类型注册、会话运行时、世界存档和连接范围。
- StructureFootprintComponent 与物品 payload 拆开：多格结构可独立做完整性判断，物品内容又有独立容量、槽位和掉落约束。
- Chest 与 TileEntity 类型 Component 拆开：Chest 的坐标索引、slot 数组和内容破坏规则不能等同于 TileEntity 的 ByID/ByPosition 模型。
- Pylon 实体结构与 PylonRegistryComponent 拆开：前者是单实体结构事实，后者是 World 级集合、上一版本快照和 revision。
- InteractionActorContextComponent 与稳定世界事实拆开：CurrentUser 是一次操作上下文，不能继续作为静态全局 owner。

### 7.3 不因复用而强行合并

StoredItemState、TileCoordinate、LiquidKind、EntityReference 和各种 ID wrapper 是 value object 或关系类型，不因字段相似而合并成通用 Component。相同 item tuple 由不同宿主 Component 使用，是复用值语义，不是共享可写状态。

## 8. 不单独创建 Component 的对象

- 单个 wire node、switch、actuator、pump、逻辑灯、像素盒和单个网络消息不单独创建 Component；它们是 Tile 事实、传播 scratch 或边界输入中的对象。
- StoredItemState 不单独创建 Component；它作为单物品和多槽 payload 的值对象，并保持 type/prefix/stack 原子性。
- MechanismCooldownEntry 不单独创建 Component；它是 MechanismCooldownComponent.Entries 的条目值。
- TileCoordinate、LiquidKind、EntityReference、TileEntityKindId、TileEntityRuntimeId 和候选 ID wrapper 不单独创建 Component；它们是字段类型。
- TileEntitiesManager 的类型注册表不单独创建 Component；它属于类型元数据边界，不能和某个 TileEntity 实例混合。
- TileMapStore、TileEntityStore、WorldStorageRoot 和 WorldChestState 不直接作为单个 ECS Component；它们是当前 NLTX 的存储聚合或 State 轮廓，映射到若干 Component 后仍需整合 owner。
- IsActive（Training Dummy）、HasItem、ContainsItems、CanTeleport、HasPendingRefresh 和有效 footprint 坐标集合都是派生值，不创建可写镜像 Component。
- HopperGrabHitboxSize、TileEntity 类型注册实例、frame 重算临时坐标和数组容量常量不是实体状态，不单独创建 Component。

## 9. 当前 NLTX 组件覆盖

| 当前 NLTX 类型 | 当前状态 | 目标 Component | 已覆盖内容 | 未覆盖内容 |
|---|---|---|---|---|
| src\WorldInteraction\Tiles\TileCellComponent.cs | partial | TileCellComponent | type、active、wall、liquid amount/kind | raw header、完整液体工作 owner |
| src\WorldInteraction\Tiles\TileFrameComponent.cs | existing | TileFrameComponent | 四个 frame 字段 | 完整写入 owner 和恢复闭环 |
| src\WorldInteraction\Tiles\TileSignalTopologyComponent.cs | existing | TileSignalTopologyComponent | 四线、actuator、actuated | Version4 全传播行为 |
| src\WorldInteraction\Wiring\MechanismCooldownComponent.cs、MechanismCooldownEntry.cs | partial | MechanismCooldownComponent | entry 和三类炮计数 | Version4 机械登记和越界清理 |
| src\WorldInteraction\PressurePlates\PressurePlateOccupancyComponent.cs | partial | PressurePlateOccupancyComponent | 坐标到 EntityReference 集合、首次更新标记 | 唯一写入 owner、实体离场和恢复语义 |
| src\WorldInteraction\TileEntities\TileEntityAnchorComponent.cs | existing | TileEntityAnchorComponent | Origin | 索引一致性和多格完整性 |
| src\WorldInteraction\TileEntities\TileEntityKindComponent.cs | existing | TileEntityKindComponent | Kind wrapper | 注册表与恢复拒绝规则 |
| src\WorldInteraction\TileEntities\TileEntityRuntimeIdComponent.cs | partial | TileEntityRuntimeIdComponent | runtime ID wrapper | runtime/persistent/network 分层 |
| 无直接同名文件 | proposed | TileEntityPersistenceIdentityComponent | 无 | Version4 ID 的长期存档含义 |
| 无直接同名文件 | proposed | TileEntityNetworkIdentityComponent | 无 | 网络身份来源和连接生命周期 |
| src\WorldInteraction\TileEntities\TileEntityUpdateScheduleComponent.cs | existing | TileEntityUpdateScheduleComponent | RequiresUpdates | 更新登记 owner 与恢复关系 |
| src\WorldInteraction\TileEntities\LogicSensorComponent.cs | partial | LogicSensorComponent | check type、on、counted data | 日期/玩家/液体读取、失效删除和删减成员 |
| src\WorldInteraction\TileEntities\TrainingDummyComponent.cs | partial | TrainingDummyComponent | NPC EntityReference、retry、派生 active | Version4 关联生命周期 |
| src\WorldInteraction\TileEntities\StoredItemState.cs | existing（value object） | 单物品宿主 Component 的字段类型 | 空 tuple 和基本字段 | 不作为独立 Component；各宿主合法性不同 |
| src\WorldInteraction\TileEntities\ItemFrameComponent.cs | partial | ItemFrameComponent | 单 item 字段 | 完整宿主生命周期和版本读写 |
| src\WorldInteraction\TileEntities\FoodPlatterComponent.cs | partial | FoodPlatterComponent | 单 item 字段 | 完整宿主生命周期和版本读写 |
| src\WorldInteraction\TileEntities\WeaponRackComponent.cs | partial | WeaponRackComponent | 单 item 字段 | footprint、非空破坏闭环 |
| src\WorldInteraction\TileEntities\HatRackComponent.cs | existing | HatRackComponent | 2+2 槽位、ContainsItems | 完整宿主验证和恢复 |
| src\WorldInteraction\TileEntities\DisplayDollComponent.cs | partial | DisplayDollComponent | 9/9/1 槽位、pose、ContainsItems | Version4 版本差异和完整生命周期 |
| src\WorldInteraction\TileEntities\DeadCellsDisplayJarComponent.cs | partial | DeadCellsDisplayJarComponent | 单 item 字段 | Version4 空读写方法对应的兼容语义 |
| src\WorldInteraction\TileEntities\LeashedEntityAnchorComponent.cs | partial | LeashedEntityAnchorComponent | ItemType、HasItem | 带物品/不带物品变体和清理 |
| 无直接同名文件 | proposed | StructureFootprintComponent | 无 | 通用 footprint owner、支撑/缺格语义 |
| src\WorldStorage\WorldChestState.cs | partial | ChestStructureComponent + MultiSlotItemPayloadComponent | Anchor、Slot、Items、capacity、Name、legacy 标记 | 结构与 payload 的独立 owner、索引和内容破坏 |
| src\WorldStorage\TileMapStore.cs、TileCellState.cs | partial | TileCellComponent + TileFrameComponent + TileLiquidWorkStateComponent | Tile 数组、frame、header、液体工作字段 | Interaction 与 Storage 的单写关系 |
| src\WorldStorage\TileEntityStore.cs | partial | TileEntity common identity + persistence identity 候选 | ID/anchor 字典、next ID、revision 轮廓 | runtime/persistent/network 分离和恢复 |
| 无直接同名文件 | proposed | PylonStructureComponent | 无 | 3x4、597 Tile、style/kind、support 组合 |
| src\WorldStorage\PylonRegistryState.cs、PylonRegistryEntry.cs | partial | PylonRegistryComponent | current/previous、cooldown、revision、条目 | 最终 WorldStorage/WorldInteraction owner |
| src\WorldStorage\WorldStorageRoot.cs | partial | 多个 World/Storage 级 Component 的容器边界 | TileMap、TileEntities、Containers、Sections 集合 | 不能直接作为单一 Component owner |

当前验证程序 D:\TRbackup\NLTX\Test\Terraria.WorldInteraction.Components.Verification\Program.cs:1-74 只覆盖默认值和局部字段形状，因此不改变上述 partial 判断。

## 10. 组件级 evidence-gap

| gapId | 涉及 Component | 未确认内容 | 影响 |
|---|---|---|---|
| WIS-EG-01 | TileCellComponent、TileSignalTopologyComponent、TileFrameComponent | raw header 各位到 active、wire、actuator、slope 等语义的完整映射，以及 Tile 清理时的共同不变量 | 不能锁定 Tile 结构字段是否需要进一步拆分 |
| WIS-EG-02 | WirePropagationScratchComponent、MechanismCooldownComponent | Version4 CheckMech、CheckLogicGate、MassWireOperationInner、GeyserTrap、DeActive、ReActive、XferWater 存在空体/删减 | 不能声称传播、机械、液体或 actuator 行为完整 |
| WIS-EG-03 | PumpTransferScratchComponent、TileLiquidWorkStateComponent | 泵端点与 LiquidSimulation 液体权威状态的 owner、容量/液体类型规则和失败语义 | 不能锁定泵 scratch 粒度 |
| WIS-EG-04 | TileEntityAnchorComponent、StructureFootprintComponent、各 TileEntity 专属 Component | Version4 Place、删除、恢复和宿主失效的完整生命周期；部分子类型 lifecycle 为空 | 不能锁定多格实体创建/销毁原子性 |
| WIS-EG-05 | TileEntityRuntimeIdComponent、TileEntityPersistenceIdentityComponent、TileEntityNetworkIdentityComponent | ID 在存档中出现但网络写入省略 ID；无证据证明独立 persistent/network ID 的格式和分配 owner | 不能把任一 ID 宣布为唯一跨边界身份 |
| WIS-EG-06 | LogicSensorComponent、TileEntityUpdateScheduleComponent | CountedData 是否存档、更新资格和失效移除如何与恢复严格对应 | 只能保留候选字段 |
| WIS-EG-07 | 单物品宿主、HatRackComponent、DisplayDollComponent、DeadCellsDisplayJarComponent | 不同宿主的 Item 合法性、掉落、容量、网络/存档字段；Jar 读写空体 | 不能合并成通用可写 payload Component |
| WIS-EG-08 | ChestStructureComponent、MultiSlotItemPayloadComponent、StructureFootprintComponent | Chest footprint、默认容量、bank/legacy 语义、空内容破坏和索引恢复边界 | 不能锁定 Chest 是否复用 TileEntity common identity |
| WIS-EG-09 | PylonStructureComponent、PylonRegistryComponent | registry 刷新 owner、Previous/Current 快照含义、Kind 与 TileEntityId 身份层级、revision 规则 | 不能宣布 Pylon 注册表最终 owner |
| WIS-EG-10 | 所有 World/Section 级 Component | TileMap mutation revision、TileEntity mutation revision、Pylon revision 是否统一，以及 compatibility version 来源 | 不能新增统一 revision 字段并声称兼容 |
| WIS-EG-11 | PressurePlateOccupancyComponent、InteractionActorContextComponent | 玩家/实体占用、当前玩家索引和 EntityReference 的跨域映射，以及首次更新默认值 | 不能锁定占用关系唯一写入边界 |

上述 gap 保持为 partial 或 unresolved；本文件没有为缺失证据补造默认值，也没有把完整参考独有成员写成 Version4 当前事实。

## 11. 未决组件 owner

所有以下事项均明确标记 crossSubsystemOwner: integration-review。本文件只给出候选边界，不宣布最终 owner。

### BD-COMP-01 — Tile 与液体工作状态 owner

- 冲突字段：TileCellComponent.LiquidAmount/LiquidKind、TileLiquidWorkStateComponent、PumpTransferScratchComponent。
- 当前候选 owner：Tile 结构事实由 WorldInteraction；液体量/工作状态由 LiquidSimulation 或 WorldStorage；泵端点由本责任面暂存。
- 组成影响：若液体权威在 LiquidSimulation，TileCell 只保留可读液体快照；若在 WorldStorage，则 TileCell 与存储恢复需共用不变量。
- 未决原因：Version4 XferWater 是空体，当前证据无法确认容量、液体类型和失败语义。
- crossSubsystemOwner: integration-review

### BD-COMP-02 — Tile、TileEntity、Chest 结构 owner

- 冲突字段：TileCellComponent、TileFrameComponent、TileEntityAnchorComponent、StructureFootprintComponent、ChestStructureComponent。
- 当前候选 owner：WorldInteraction 维护结构事实；WorldStorage 维护持久化索引；或由整合边界维护共享提交不变量。
- 组成影响：结构 owner 方案要求所有变化先落到 Tile 组合；Storage 方案要求 Storage 直接拥有可恢复快照；共享方案需要明确旧值、revision 和拒绝语义。
- 未决原因：Version4 放置、framing、破坏和存档跨越多个类，多格原子性未被完整证明。
- crossSubsystemOwner: integration-review

### BD-COMP-03 — 多格结构完整性 owner

- 冲突字段：StructureFootprintComponent 与 Pylon、Hat Rack、Display Doll、Weapon Rack、Chest 的宿主字段。
- 当前候选 owner：每个宿主类型负责自身 footprint；或通用结构边界负责覆盖格和支撑检查。
- 组成影响：宿主自有规则保留类型差异；通用 owner 可统一缺格检测，但会引入宿主 Tile 类型、frame 和掉落规则共享依赖。
- 未决原因：已确认 Pylon 3x4 和部分多格宿主，但所有宿主范围、支撑和破坏补偿不完整。
- crossSubsystemOwner: integration-review

### BD-COMP-04 — TileEntity ID 分层 owner

- 冲突字段：TileEntityRuntimeIdComponent、TileEntityPersistenceIdentityComponent、TileEntityNetworkIdentityComponent、PylonRegistryEntry.TileEntityId。
- 当前候选 owner：WorldStorage 负责 persistent ID；WorldInteraction 负责 runtime ID；网络边界按连接分配 NetworkId。
- 组成影响：三层身份分离会增加映射字段；复用同一 int 会错误合并会话、存档和连接生命周期。
- 未决原因：Version4 存档写 ID、网络读写省略 ID，但没有足够证据确认独立网络身份。
- crossSubsystemOwner: integration-review

### BD-COMP-05 — Pylon 注册表 owner

- 冲突字段：PylonStructureComponent.PylonKind、PylonRegistryComponent.CurrentPylons/PreviousPylons/Revision。
- 当前候选 owner：WorldStorage 维护世界快照；WorldInteraction 维护结构合法性；Pylon 专属域维护可传送集合。
- 组成影响：WorldStorage owner 需把实体变化反映到注册快照；结构 owner 需把世界快照写回 Storage；专属域 owner 会引入跨域 ID 和 revision 依赖。
- 未决原因：当前 NLTX 只有 PylonRegistryState 轮廓，Version4 完整刷新和恢复边界未证实。
- crossSubsystemOwner: integration-review

### BD-COMP-06 — World/Section revision owner

- 冲突字段：TileMapStore.MutationRevision、TileEntityStore.MutationRevision、PylonRegistryState.Revision 和跨域兼容 revision 候选。
- 当前候选 owner：各存储集合各自维护；或由 WorldStorage 维护统一 revision。
- 组成影响：各自维护避免跨域耦合，但快照比较需要多 revision；统一维护便于一致性检查，却会改变多个 Component 的生命周期和提交边界。
- 未决原因：当前实现没有证明统一 revision 的来源、位宽、持久化或网络语义。
- crossSubsystemOwner: integration-review

### BD-COMP-07 — 压力板占用和触发者 owner

- 冲突字段：PressurePlateOccupancyComponent、InteractionActorContextComponent、EntityReference。
- 当前候选 owner：SpatialSimulation 维护占用；WorldInteraction 只读；或 WorldInteraction 维护压力板关系并接收玩家实体引用。
- 组成影响：只读关系方案减轻本责任面写入；关系 owner 方案需要明确进入/离开、实体销毁和首次更新的写入边界。
- 未决原因：Version4 使用玩家槽位扫描，当前 NLTX 使用 EntityReference，稳定映射尚未证实。
- crossSubsystemOwner: integration-review

## 12. 最终 Component 清单

| componentId | name | status | entityScope | 当前 NLTX 映射 | owner 状态 |
|---|---|---|---|---|---|
| WIS-COMP-01 | TileCellComponent | partial | 每 Tile | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-02 | TileFrameComponent | existing | 每 Tile | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-03 | TileSignalTopologyComponent | existing | 每 Tile | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-04 | TileLiquidWorkStateComponent | proposed | 每 Tile 可选状态 | TileCellState 局部字段 | crossSubsystemOwner: integration-review |
| WIS-COMP-05 | PressurePlateOccupancyComponent | partial | World/Section 关系集合 | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-06 | MechanismCooldownComponent | partial | World/Section | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-07 | WirePropagationScratchComponent | proposed | 一次传播工作集 | 无直接组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-08 | PumpTransferScratchComponent | proposed | 一次传播工作集 | 无直接组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-09 | InteractionActorContextComponent | proposed | 一次交互上下文 | 无直接组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-10 | TileEntityAnchorComponent | existing | 每 TileEntity | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-11 | TileEntityKindComponent | existing | 每 TileEntity | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-12 | TileEntityRuntimeIdComponent | partial | 每 TileEntity | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-13 | TileEntityPersistenceIdentityComponent | proposed | 每可持久化 TileEntity | 无直接组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-14 | TileEntityNetworkIdentityComponent | proposed | 每连接中的 TileEntity 视图 | 无直接组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-15 | TileEntityUpdateScheduleComponent | existing | 每 TileEntity | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-16 | LogicSensorComponent | partial | 每逻辑传感器 | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-17 | TrainingDummyComponent | partial | 每训练假人 | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-18 | ItemFrameComponent | partial | 每 Item Frame | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-19 | FoodPlatterComponent | partial | 每 Food Platter | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-20 | WeaponRackComponent | partial | 每 Weapon Rack | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-21 | HatRackComponent | existing | 每 Hat Rack | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-22 | DisplayDollComponent | partial | 每 Display Doll | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-23 | DeadCellsDisplayJarComponent | partial | 每 Display Jar | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-24 | LeashedEntityAnchorComponent | partial | 每带物品系绳锚点 | 同名组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-25 | StructureFootprintComponent | proposed | 每多格结构 | 无直接组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-26 | ChestStructureComponent | proposed | 每世界 Chest | WorldChestState 局部字段 | crossSubsystemOwner: integration-review |
| WIS-COMP-27 | MultiSlotItemPayloadComponent | proposed | 每多槽容器 | WorldChestState 局部字段 | crossSubsystemOwner: integration-review |
| WIS-COMP-28 | PylonStructureComponent | proposed | 每 Pylon TileEntity | 无直接组件 | crossSubsystemOwner: integration-review |
| WIS-COMP-29 | PylonRegistryComponent | partial | World | PylonRegistryState/Entry | crossSubsystemOwner: integration-review |

Component 数量为 29。当前 NLTX 组件的 existing/partial 状态描述的是局部文件覆盖，不代表 Version4 行为已经完整实现；proposed 只表示设计提案。

## 13. 最终声明

本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。

当前 designStatus: decision-required、evidenceStatus: partial、nltxStatus: partial、verificationStatus: not-run。未使用子代理，未修改源码、测试、Version4、完整参考源码或文档，未运行构建或测试。
