# LeashedEntitySimulation Component Design

## 1. 设计元数据

subsystemId: LeashedEntitySimulation  
taskNumber: 03  
sourceReport: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-leashed-entity-simulation-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-leashed-entity-simulation-component-design.md  
designScope: component-only  
designStatus: decision-required  
evidenceStatus: partial  
nltxStatus: missing（局部组件和协议模型为 partial）  
verificationStatus: not-run  
selectionMethod: 按共同读写者、权威 owner、生命周期、持久化边界、空间范围和表现隔离分组；不按字段数量机械拆分

本文件把研究报告中的候选拆分压缩为组件状态设计。designStatus: decision-required 的原因是稳定实体身份、锚点关系持久化 owner、全局 section 活跃状态和部分行为字段的语义尚未由 Version4 主基线闭合。所有未确认内容都保留为候选或 unresolved，不使用 baseline。

## 2. 设计范围与排除范围

### 2.1 纳入范围

本设计只覆盖 LeashedEntitySimulation 直接拥有或直接承载的状态：

- leashed 实例所引用的 definition ID 和实例状态；
- active、spawned、despawn、removed 的生命周期状态；
- 位置、速度、方向和尺寸等运动状态；
- critter 与 kite 两种互斥行为状态；
- 实例与锚点之间的运行时关系、Tile 坐标和可恢复关系候选；
- 实例在 world section 中的成员索引和 section 活跃观察值；
- whoAmI legacy slot 这类兼容字段，但不把它当作稳定身份。

### 2.2 排除范围

以下对象只作为字段证据或边界事实记录，不在本文件中设计为新的 Component：

- Registry.Prototypes 和 19 个 prototype：它们是静态 definition 目录，不附着到单个实体；
- BySection、ActiveSectionList、SectionEntityList：它们是集合索引或工作集，不是实体权威状态；
- TELeashedEntityAnchor 及其 item host：它们是 TileEntity 宿主和触发边界；
- NetModule、网络包、二进制 reader/writer、netOffset 和旧客户端 slot 映射：它们是协议或传输表示；
- Draw、dummy NPC/projectile、oldPos、oldRot、oldSpriteDirection、frame 等纯表现或历史缓存；
- 通用 EntityIdentityComponent、EntityReference、TileCoordinate、SectionCoordinate、持久化 ID 和网络 ID 的最终跨域 owner；
- 除 Component 以外的运行时编排、纯读访问、意图记录、传输接口、存档接口和客户端表现结构。

## 3. 组件设计依据

### 3.1 证据优先级

按 Version4 → 完整可编译参考 → tModLoader 公开文档 → Space Station 14 ECS 结构参考使用证据。完整参考只能补足 Version4 已存在文件的删减或空体，不能把参考独有实现写成 Version4 当前事实。

| 来源 | 实际读取范围 | 用途 | 状态 |
|---|---|---|---|
| D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | Registry.RegisterAll/Register、SectionEntityList、BySection、ByWhoAmI、UpdateEntities、Clear、NetModule.Sync | 确认实例字段、集合索引、注册顺序、生命周期入口和网络字段边界 | confirmed / partial |
| D:\TRbackup\Version4\Terraria\Main.cs | Initialize_AlmostEverything、每帧 LeashedEntity.UpdateEntities | 确认 definition 注册和每帧驱动入口 | confirmed |
| D:\TRbackup\Version4\Terraria\WorldGen.cs | 世界清理中的 ActiveSections.Reset、LeashedEntity.Clear | 确认清理边界和顺序事实 | confirmed |
| D:\TRbackup\Version4\Terraria.DataStructures\ActiveSections.cs、Terraria\RemoteClient.cs | section 活跃判断、60 tick inactive window、客户端 section cache | 区分全局 section 事实与 Leashed 本地索引缓存 | confirmed |
| D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchor.cs、TELeashedEntityAnchorWithItem.cs | 锚点宿主、itemType、移除和 world-load 入口 | 确认 relation 字段来自外部宿主，不能转移 item owner | partial |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs | position、velocity、direction、width、height、section mutation 和实例生命周期补证 | 仅补足 Version4 同文件缺失成员；相关结论仍标 partial | partial |
| D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\*.cs | critter/kite 基类字段、prototype 字段和网络 payload 语义 | 识别行为状态候选并排除表现缓存 | partial |
| D:\TRbackup\NLTX\src、D:\TRbackup\NLTX\dome\src | 当前 anchor、identity、relation、section、Leash 和协议模型 | 只确认当前已有片段和覆盖缺口 | partial |
| D:\TRbackup\tmodloader-api-docs-stable\class_mod_packet.html、class_mod_tile_entity.html、class_tile_entity.html | 公开 packet 和 TileEntity 边界 | 只交叉核对外部边界，不替代 Version4 私有实现 | confirmed |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Construction\Components\AnchorableComponent.cs、EntitySystems\AnchorableSystem.cs、Teleportation\Systems\LinkedEntitySystem.cs | 组件粒度、关系清理和宿主/实体分离 | 只参考结构粒度，不复制名称或语义 | confirmed |

### 3.2 当前可确认的 Version4 局部顺序

以下仅用于解释组件生命周期依赖，不构成新的运行时结构设计：

1. Main.Initialize_AlmostEverything 在 Main.cs:3344-3348 调用 LeashedEntity.Registry.RegisterAll；
2. TELeashedEntityAnchor 的完整参考补证显示，锚点恢复会创建实体并进入 LeashedEntity.AddNewEntity；Version4 对应方法为空体，故当前主基线只确认入口存在；
3. LeashedEntity.AddNewEntity 将 anchor、active、legacy slot 和 section membership 写入实例/索引；完整参考位置为 LeashedEntity.cs:377-410，Version4 同位置的创建成员缺失；
4. Main.cs:11569-11572 在每帧 item 更新后调用 LeashedEntity.UpdateEntities；
5. UpdateEntities 内部先重检 active section，再遍历 active section 中的 active 实体；Version4 的 Remove 和 StreamNetUpdates 为空体；
6. WorldGen.cs:6662-6666 观察到 ActiveSections.Reset 先于 LeashedEntity.Clear；该相对顺序属于已确认的清理事实。

## 4. Version4 成员到 Component 归属表

表中的 proposed Component 是目标状态归属，不表示当前 NLTX 已存在同名完整组件。not-component 表示该成员应保留在组件外的目录、索引、宿主或协议边界。

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| Registry.Prototypes | List<LeashedEntity> | 静态 prototype/definition 目录 | 权威 definition 目录；不是实体字段 | 进程初始化至结束 | not-component；被 State.DefinitionId 引用 | confirmed | Version4 LeashedEntity.cs:42-90 |
| Registry.RegisterAll/Register/Get | 静态注册成员 | 19 个有效类型的稳定注册顺序和查找 | 权威 definition ID 分配 | 进程初始化 | not-component | confirmed | Version4 LeashedEntity.cs:46-90；完整参考 :121-177 |
| ByWhoAmI | List<LeashedEntity> | legacy slot 到实例的映射 | 兼容索引；不是稳定身份 | spawn 至 remove，clear 时重置 | LeashedEntityLegacySlotComponent | confirmed / partial | Version4 LeashedEntity.cs:177-194；完整参考 :442-451 |
| whoAmI | int | 可复用的旧网络 slot | 兼容字段 | slot 分配至 remove | LeashedEntityLegacySlotComponent.Slot | confirmed | Version4 LeashedEntity.cs:183-188 |
| BySection[,] | SectionEntityList[,] | section 到实体 bucket 的索引 | 派生索引/缓存 | world session 生命周期 | SectionMembership 的索引来源；本身不是 Component | confirmed | Version4 LeashedEntity.cs:177-194,221-230 |
| ActiveSectionList | List<SectionEntityList> | 当前活跃 section 工作集 | 派生工作集/缓存 | world/session 生命周期 | SectionMembership.IsSectionActive 只能保存本地观察值 | confirmed | Version4 LeashedEntity.cs:177-180,242-261 |
| SectionEntityList.coordinates | Point | section 坐标 | 索引 key；非实体权威字段 | bucket 创建至 clear | SectionMembership.Section 的候选来源 | confirmed | Version4 LeashedEntity.cs:94-109 |
| SectionEntityList.list/count/emptySlots | LeashedEntity[]、int | bucket 内容、长度和空槽计数 | 派生索引/缓存 | entity 加入 bucket 至移除 | not-component | confirmed / partial | Version4 LeashedEntity.cs:94-175；完整参考 :179-283 |
| sectionSlot | int | entity 在 section bucket 内的临时位置 | 派生缓存；不可持久化 | entity 在 bucket 中期间 | SectionMembership.SectionSlot | confirmed / partial | Version4 LeashedEntity.cs:183-184；完整参考 :196-238 |
| SectionEntityList.active | bool | section bucket 是否处于活跃工作集 | 派生索引/工作集状态；不是实体生命周期 | bucket 创建、激活、移出工作集至 clear | SectionMembership.IsSectionActive 的观察值；不得写入 LeashedEntityLifecycleComponent | confirmed / partial | Version4 LeashedEntity.cs:94-109,242-261 |
| LeashedEntity.active | bool | 实例是否仍参与权威生命周期 | 权威生命周期状态 | inactive/active 至 removed | LeashedEntityLifecycleComponent.State | confirmed / partial | Version4 LeashedEntity.cs:185-188,267-284 |
| Type | int | definition/prototype ID | 权威 definition 引用；协议也读取 | 实例创建至 remove | LeashedEntityStateComponent.DefinitionId | confirmed | Version4 LeashedEntity.cs:190；NetModule.Sync:22-31 |
| AnchorPosition | Point16 | 锚点 Tile 坐标 | 权威关系事实，持久化候选 | entity 生命周期 | LeashedEntityAnchorRelationComponent.AnchorCoordinate | confirmed | Version4 LeashedEntity.cs:192-194,397-401 |
| SectionCoordinates | Point | 由 anchor 坐标计算出的 section | 派生值 | 每次索引/相关性访问 | SectionMembership.Section；不得重复写成第二权威坐标 | confirmed | Version4 LeashedEntity.cs:194 |
| position | Vector2 | 实例左上位置 | 权威运动状态候选 | active 实例生命周期 | LeashedEntityMotionComponent.Position | partial | 完整参考 LeashedEntity.cs:297-299,315-325；Version4 主文件缺失 |
| velocity | Vector2 | 实例运动速度 | 权威运动状态候选 | active 实例生命周期 | LeashedEntityMotionComponent.Velocity | partial | 完整参考 LeashedEntity.cs:299-300；Version4 主文件缺失 |
| direction | int | 水平朝向 | 权威运动/行为状态候选 | active 实例生命周期 | LeashedEntityMotionComponent.Direction | partial | 完整参考 LeashedEntity.cs:301；Version4 主文件缺失 |
| width/height | int | 碰撞/绘制尺寸候选 | 权威几何状态候选 | prototype 初始化至实例 remove | LeashedEntityMotionComponent.Width/Height | partial | 完整参考 LeashedEntity.cs:303-305 |
| Center/Size | Vector2 属性 | 由 position/size 计算的视图 | 派生值 | 访问期间 | not-component | partial | 完整参考 LeashedEntity.cs:315-338 |
| LeashedCritter.npcType | int | critter 对应 NPC content ID | 权威行为/definition 映射候选 | critter 实例生命周期 | LeashedCritterBehaviorComponent.NpcType | partial | 完整参考 LeashedCritter.cs:15-31；协议 TerrariaPacketCodec.cs:4203-4212 |
| LeashedCritter.TargetPosition | Point16 | critter 锚点附近目标 Tile | 权威行为状态候选 | critter active 生命周期 | LeashedCritterBehaviorComponent.TargetPosition | partial | 完整参考 LeashedCritter.cs:27-31；协议 TerrariaPacketCodec.cs:4218-4224 |
| LeashedCritter.rand | LCG32Random | critter 随机状态 | 权威行为状态候选；wire 表示为 uint | critter 实例生命周期 | LeashedCritterBehaviorComponent.RandomState | partial | 完整参考 LeashedCritter.cs:23-27；协议 TerrariaPacketCodec.cs:4218-4222 |
| LeashedCritter.WaitTime | short | critter 等待计时 | 权威行为状态候选 | critter active 生命周期 | LeashedCritterBehaviorComponent.WaitTime | partial | 完整参考 LeashedCritter.cs:25-29；协议 TerrariaPacketCodec.cs:4218-4223 |
| LeashedCritter.State | byte | critter 行为阶段 | 权威行为状态候选 | critter active 生命周期 | LeashedCritterBehaviorComponent.State | partial | 完整参考 LeashedCritter.cs:27-31；Walker/Jumper 状态常量 |
| LeashedCritter.anchorStyle/isAquatic | int、bool | 锚点兼容和水生行为标记 | owner 未闭合；definition/behavior/relation 候选 | prototype/instance 生命周期未闭合 | LeashedCritterBehaviorComponent 中仅保留候选字段 | partial | 完整参考 LeashedCritter.cs:13-41；当前 LeashBehaviorRefComponent |
| LeashedKite.projType | int | kite 对应 projectile content ID | 权威行为/definition 映射候选 | kite 实例生命周期 | LeashedKiteBehaviorComponent.ProjectileType | partial | 完整参考 LeashedKite.cs:15；协议 TerrariaPacketCodec.cs:4177-4181 |
| LeashedKite.rotation | float | kite 旋转状态 | 运动/表现边界候选 | kite 实例生命周期 | LeashedKiteBehaviorComponent.Rotation | partial | 完整参考 LeashedKite.cs:17-23；协议 TerrariaPacketCodec.cs:4186-4194 |
| LeashedKite.kiteDistance/windTarget/windCurrent/timeCounter/timeWithoutWind | float、int | kite 运动和风状态 | 权威行为候选；默认值未全部确认 | kite active 生命周期 | LeashedKiteBehaviorComponent | partial | 完整参考 LeashedKite.cs:25-35,127-247 |
| LeashedKite.projectileLocalAI0/projectileLocalAI1 | float、float | projectile-like 运动兼容状态 | 权威候选；不得并入普通 projectile state | kite active 生命周期 | LeashedKiteBehaviorComponent | partial | 完整参考 LeashedKite.cs:37-39,127-247 |
| LeashedKite.frame/frameCounter/spriteDirection | int、int、int | 动画/表现状态 | 表现或表现缓存候选 | client frame 生命周期 | not-component，待表现边界单独裁决 | partial | 完整参考 LeashedKite.cs:17-23,117-126 |
| oldPos/oldRot/oldSpriteDirection/netOffset | 数组、float[]、int[]、Vector2 | 历史轨迹、插值或网络偏移 | 表现/传输缓存 | frame/packet 生命周期 | not-component | partial | 完整参考 LeashedKite.cs:41-47、LeashedCritter.cs:31-35 |
| NetModule.Sync/Deserialize | NetModule 方法 | full/partial/remove 的协议表示 | 快照/兼容边界，不是状态 owner | packet 生命周期 | not-component | partial | Version4 LeashedEntity.cs:13-39；完整参考 :13-119 |
| Draw/DrawEntities | 虚方法/静态方法 | 客户端可见性和绘制入口 | 表现投影 | client frame 生命周期 | not-component | partial | 完整参考 LeashedEntity.cs:528-549；Version4 直接调用者未闭合 |

## 5. Component 定义

下列 8 个 Component 是当前会话的目标清单。已有同名或近似 NLTX 类型没有被默认为已完成；status: partial 表示文件和局部字段存在但 owner、生命周期或闭环仍不完整，status: proposed 表示尚未创建的目标 Component。

### 5.1 LeashedEntityStateComponent

#### 职责

保存一个 leashed 实例对 definition 的权威引用，并作为该实例进入 Leashed entity 组合的最小领域状态。它不保存 active 生命周期、运动细节、锚点宿主对象或 legacy slot。

componentId: COMP-LEASH-STATE  
name: LeashedEntityStateComponent  
status: partial  
componentOwner: LeashedEntitySimulation  
crossSubsystemOwner: integration-review（DefinitionId/identity 跨内容和协议使用）  
entityScope: 单个 Leashed entity  
lifecycle: entity 创建时绑定有效 definition；entity remove 时清除；world load 只能在 definition 可解析时恢复

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| DefinitionId | int | 0 表示未绑定/无效候选值 | 权威状态 | 绑定成功后必须对应注册目录中的有效类型；V1456 当前有效范围候选为 1..19；不得由目录外隐式排序生成 | confirmed / partial | Version4 Registry.RegisterAll:50-69 为 slot 0 留空；Type:190；当前 dome LeashedEntityStateComponent.cs:7-8 |

#### 字段不变量

- DefinitionId 只引用 definition，不携带实例的 active、位置或网络字节。
- 未解析 definition 时不得创建半初始化的权威实体；当前证据不足以决定是拒绝、保留关系还是 tombstone，因此恢复策略为 decision-required。
- EntityIdentityComponent.UUID 是现有 NLTX 的通用身份候选，不等同于 DefinitionId、whoAmI 或持久化 ID。

#### 生命周期

创建时先处于未绑定候选状态；只有 definition 解析成功且关系事实可接受后，才允许进入有效 entity 组合。定义目录本身在进程初始化阶段创建，不随单个 Component 的销毁而改变。

#### Entity/World 范围

仅附着于单个 Leashed entity。不能附着到锚点 TileEntity、section 对象或客户端表现对象上。

#### ID 与关系字段

本 Component 不持有 runtime UUID、persistent entity ID、network ID 或 legacy slot。所有这些身份只能通过组合关系或兼容字段候选表达，最终 owner 需要整合裁决。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityStateComponent.cs:5-15 已有 InstanceId、TypeId、anchor、active、lifecycle、spawned 字段。TypeId 可映射到 DefinitionId；InstanceId 不应原样保留为稳定身份，其他字段应分别迁入 lifecycle 或 anchor relation。状态为 partial。

#### 证据

Version4 Registry.RegisterAll 的 null 占位和固定注册顺序是 definition ID 的直接事实；Version4 Type 属性和网络 full/partial 写入路径进一步证明它是 definition 引用候选。当前缺失独立稳定 entity ID，因此本 Component 不自行补造该字段。

### 5.2 LeashedEntityLifecycleComponent

#### 职责

保存单个实例从未激活到 active、despawning、removed 的生命周期状态，以及保证重复触发可被识别的转换序号候选。它不直接保存 socket、存档 writer、TileEntity 或 item。

componentId: COMP-LEASH-LIFECYCLE  
name: LeashedEntityLifecycleComponent  
status: proposed  
componentOwner: LeashedEntitySimulation  
crossSubsystemOwner: none for local state；transition result may be consumed across subsystems  
entityScope: 单个 Leashed entity  
lifecycle: entity creation → Inactive；accepted activation → Active；section/anchor removal → Despawning；final cleanup → Removed

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| State | LeashedEntityLifecycle | Inactive | 权威状态 | 只能取 Inactive、Active、Despawning、Removed；Removed 不可回到 Active | partial | 当前 dome LeashedEntityLifecycle.cs:3-9；Version4 只有 active bool |
| Spawned | bool | false | 权威状态 | 只能在 relation 和 definition 有效后变为 true；Removed 时必须为 false | partial | 当前 LeashedEntityStateComponent.cs:13-15；完整参考 LeashedEntity.cs:403-406 |
| TransitionSequence | ulong | 0 | 权威幂等辅助状态候选 | 每次生命周期转换单调递增；不能跨 entity 复用；若最终 owner 不需要，应删除而不是镜像到多个组件 | missing | Version4 只有 active 写入，没有 transition revision 证据；完整参考 anchor restore 为 LeashedEntityAnchor.cs:54-61 |

#### 字段不变量

- State 是 active 的唯一候选 owner；不得让 section cache、锚点 host 或 ByWhoAmI 各自维护第二份权威 active。
- section 变为 inactive 时，优先把运行态置为非 spawned 或 Despawning，但 durable anchor relation 是否保留必须独立决定；不能因为暂时不活跃就删除关系。
- anchor 被移除时不能通过宿主字段递归修改自身；关系撤销和实例移除必须可重复处理。

#### 生命周期

默认 Inactive。新实体加入 active section 时进入 Active 并标记 Spawned；section 暂停只影响运行态，不自动等同于持久化关系删除；实体行为或 anchor removal 使状态进入 Despawning；索引、slot 和临时状态清理完成后进入 Removed。

#### Entity/World 范围

单个 entity。world clear 会清除该 world session 中的实例 lifecycle，但是否根据 durable relation 重建实体要由恢复决策决定。

#### ID 与关系字段

只保存本地生命周期，不保存 whoAmI、anchor 坐标或 TileEntity object。

#### 当前 NLTX 映射

dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityLifecycle.cs:3-9 已有枚举，LeashedEntityStateComponent 已有 LifecycleState 和 Spawned。没有证据证明 transition owner 或重复提交策略，因此当前只算 partial 支撑。

#### 证据

Version4 UpdateEntities 在 Update 后检查 active，完整参考 Remove 清理 ByWhoAmI、section bucket 并在 server 方向写 remove packet；Version4 对应实现有空体，因此字段转换和最终清理边界仍是 partial。

### 5.3 LeashedEntityMotionComponent

#### 职责

保存所有 leashed 类型共同需要的权威运动几何状态。位置、速度、方向和尺寸共同服务于运动、section 相关性和 full/partial 状态表示，因此合并在一个运动组件内；不把 critter/kite 私有行为参数塞入其中。

componentId: COMP-LEASH-MOTION  
name: LeashedEntityMotionComponent  
status: proposed  
componentOwner: LeashedEntitySimulation  
crossSubsystemOwner: integration-review（Vector2/Location/Velocity/方向值对象的最终 owner）  
entityScope: 单个 Leashed entity  
lifecycle: prototype/defaults 初始化 → active 更新 → remove 清除

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Position | LocationComponent 候选；Version4 原型为 Vector2 | unresolved；必须在首次 active 前初始化 | 权威状态 | 坐标必须可表示；其语义为左上位置还是中心位置必须与 Center 转换一致；不得由客户端表现位置反写 | partial / evidence-gap | 完整参考 LeashedEntity.cs:297-299,315-325；当前 NLTX LocationComponent.cs:3-13 |
| Velocity | VelocityComponent 候选；Version4 原型为 Vector2 | unresolved；prototype 或恢复资料提供 | 权威状态 | 必须与 Position 使用同一坐标单位；不能把 packed wire value 直接作为组件类型 | partial / evidence-gap | 完整参考 LeashedEntity.cs:299-300；当前 NLTX VelocityComponent.cs:3-13 |
| Direction | DirectionComponent 候选；Version4 原型为 int | unresolved；有效值和默认朝向待行为核对 | 权威行为状态候选 | 若表达水平朝向，必须与现有 DirectionComponent.Horizontal 语义一致；kite 旋转不替代该字段 | partial | 完整参考 LeashedEntity.cs:301；当前 DirectionComponent.cs:3-13 |
| Width | int | unresolved；由 definition/SetDefaults 提供 | 权威几何状态 | 不得为负；必须与 Size/Center 计算一致 | partial | 完整参考 LeashedEntity.cs:303-305,327-338 |
| Height | int | unresolved；由 definition/SetDefaults 提供 | 权威几何状态 | 不得为负；必须与 Size/Center 计算一致 | partial | 完整参考 LeashedEntity.cs:303-305,327-338 |

#### 字段不变量

- Position 和 Velocity 是模拟状态；PackedPositionOffset、PackedVelocity 只是协议编码，不进入本 Component。
- Width、Height 不得由客户端 packet 或 dummy 表现对象反向确立；full payload 可以验证或恢复它们，但不改变 owner 方向。
- Center 和 Size 是派生访问结果，不另建 Component，也不能与 Position/Width/Height 双写。

#### 生命周期

在 definition 绑定后初始化尺寸和初始位置；active 期间由行为状态产生新的 Position/Velocity；despawn 时可以保留用于恢复的 durable 资料，但运行态组件应在 Removed 时清理。Version4 未给出所有默认值，故不得将 Vector2.Zero 自动宣称为行为默认。

#### Entity/World 范围

单个实体。section 坐标不是 Motion 的字段，即便它由 anchor 或位置参与相关性计算。

#### ID 与关系字段

不包含任何 ID 或 anchor reference。Motion 只保存几何状态。

#### 当前 NLTX 映射

当前 src\Share\Entity\Components 已有 LocationComponent、VelocityComponent、DirectionComponent，但没有证据证明它们已经绑定到 Leashed entity，也没有完整 motion 读取链。状态为可复用基础片段 partial，不是已完成 Leashed motion。

#### 证据

Version4 主文件没有 position/velocity/size 成员，完整参考同文件才出现这些字段；这构成 gap-v4-motion-005 的直接来源。组件字段类型只能先以当前 NLTX 值对象作为映射候选，不能静默覆盖 Version4 的 Vector2 类型差异。

### 5.4 LeashedEntityAnchorRelationComponent

#### 职责

保存单个 Leashed entity 与锚点之间的关系事实。运行时引用、Tile 坐标和持久化关系候选属于同一个“关系存在性”概念，但 TileEntity 自身的 item、tile 合法性和存储字段不归本 Component 所有。

componentId: COMP-LEASH-ANCHOR-RELATION  
name: LeashedEntityAnchorRelationComponent  
status: proposed  
componentOwner: LeashedEntitySimulation for entity-side relation  
crossSubsystemOwner: integration-review（TileEntity、WorldStorage、EntityReference 和 persistent ID）  
entityScope: 单个 Leashed entity；对应 anchor host 位于独立 TileEntity  
lifecycle: relation accepted → entity active/despawn → relation removed or recovery candidate；exact recovery policy unresolved

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| AnchorReference | EntityReference? 候选 | EntityReference.None | 权威运行时关系候选 | 若非空必须指向有效 anchor entity，并且 scope 必须是 anchor scope；不能把 Arch.Core.Entity 直接当作持久化 ID | partial / unresolved | 当前 src\Relationships\EntityReference.cs:5-10；当前 dome 草图 LeashAnchorStateComponent.cs:10 使用 Arch.Core.Entity? |
| AnchorCoordinate | TileCoordinate 候选；Version4 原型为 Point16 | unresolved；无 anchor 时不得伪造坐标 | 权威关系事实和持久化候选 | 坐标必须与 host tile 一致；它不等同于 section coordinate；同一 relation 不得有两个不同权威坐标 | confirmed / partial | Version4 LeashedEntity.cs:192-194,397-401；当前 TileEntityAnchorComponent.cs:5-9 与两个 TileCoordinate 定义 |
| PersistentAnchorId | TileEntityId? 或最终整合值对象候选 | null | 持久化关系候选 | 只能由 WorldStorage/整合 owner 定义；不得把 runtime UUID、legacy slot 或 network ID 代替它 | missing | Version4 只直接证明 anchor position；当前 WorldStorage TileEntityId 与 TileEntityStore:5-8 存在候选基础 |
| RelationRevision | long? 候选 | null | 关系一致性辅助候选 | 用于拒绝过期的重复 attach/remove；若无最终 owner，不得在多个组件镜像 | missing | 当前 TileEntityStore 有 MutationRevision，但没有 Leashed relation revision；Version4 anchor 方法多为空体 |

#### 字段不变量

- AnchorReference 是运行时引用，AnchorCoordinate 是空间/持久化事实，两者不可互相替代。
- itemType 不属于本 Component；它当前由 TELeashedEntityAnchorWithItem 保存，属于锚点宿主内容状态。
- anchor removal 后，relation 不得指向已销毁 host；但是是否立即丢弃 durable relation 仍由 BD-COMP-02 决定。
- world load 的 FitsItem 校验失败时，不得直接创建无 definition 的 entity。

#### 生命周期

anchor placement 或 world recovery 产生关系候选；关系验证成功后与 entity 组合。anchor 被移除时先使 entity 进入 despawn/remove 边界，再清运行时引用；保存所需坐标或 persistent ID 的保留周期尚未裁决。

#### Entity/World 范围

Component 附着于 Leashed entity；TileEntity host 保留自身 TileEntityAnchorComponent 和 LeashedEntityAnchorComponent。这两个实体范围不能合并为一个“锚点实体”状态。

#### ID 与关系字段

AnchorReference、AnchorCoordinate、PersistentAnchorId 分别表达 runtime relation、空间/持久化坐标和持久化身份候选。三者的最终跨域 owner 都标为 integration-review。

#### 当前 NLTX 映射

src\WorldInteraction\TileEntities\LeashedEntityAnchorComponent.cs:3-7 只有 item host 字段，TileEntityAnchorComponent.cs:5-9 只有 Origin；dome\src\Terraria.Dome.Simulation\Leash\LeashAnchorStateComponent.cs:6-16 已有 Tile 坐标、Entity?、style、active 和 section 派生值，但没有关系清理或持久化 owner。整体状态为 partial。

#### 证据

完整参考 TELeashedEntityAnchor.OnRemoved、OnWorldLoaded、RespawnLeashedEntity 位于 :17-61，说明 anchor 是触发宿主；Version4 相同文件的方法为空体，不能把补证实现写成当前行为。

### 5.5 LeashedEntitySectionMembershipComponent

#### 职责

保存单个实体的 section 成员索引及对 section 活跃事实的本地观察。它服务于按 section 找到实体，不拥有全局 section 活跃真相，也不拥有实体生命周期。

componentId: COMP-LEASH-SECTION-MEMBERSHIP  
name: LeashedEntitySectionMembershipComponent  
status: proposed  
componentOwner: LeashedEntitySimulation for entity membership cache  
crossSubsystemOwner: integration-review（WorldSection/NetworkSession 的 global activity owner）  
entityScope: 单个 Leashed entity；按 world section 建立外部索引  
lifecycle: entity 加入 section → bucket membership → reindex/compact → remove；world clear 时清空

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Section | SectionCoordinate 候选 | unresolved；由 AnchorCoordinate/section 几何计算 | 派生索引值 | 必须由唯一的 section 几何规则计算；不能由 bucket slot 反推；边界 clamp 规则待整合 | confirmed / partial | Version4 SectionCoordinates:194；当前 src\WorldStorage\SectionCoordinate.cs:3；dome WorldSectionCoordinates |
| SectionSlot | int | -1 表示未加入 bucket | 派生缓存 | 仅在当前 bucket 中有效；compact 可改变它；不得持久化、不得作为 client identity | partial | Version4 sectionSlot:183-184；完整参考 SectionEntityList.Add/Remove/CompactIfNecesary:196-238 |
| IsSectionActive | bool | false | section 活跃观察缓存 | 不能替代 ActiveSections 或远端 section 真相；过期观察值必须可刷新 | partial | 当前 dome LeashSectionIndexComponent.cs:7-11；Version4 ActiveSections.IsSectionActive |
| LastActivationTick | long? | null | 活跃观察/缓存 | 只能记录观察到的 tick；不得在本 Component 内自行推导 global inactive window | partial / unresolved | 当前 dome LeashSectionIndexComponent.cs:7-11；Version4 ActiveSections.cs:8-43 使用 uint/GameUpdateCount |

#### 字段不变量

- SectionSlot 只服务于 bucket mutation，不能和 whoAmI、UUID 或 persistent ID 互换。
- Section 由 anchor 坐标派生时，必须保留 Version4 的 200×150 tile 几何候选；最终 section value object owner 未定。
- IsSectionActive 是缓存/快照分类，不得驱动跨域全局状态的创建。
- section compaction 只能改变 slot，不能改变 entity identity 或 anchor relation。

#### 生命周期

entity 创建后计算 section 并加入对应 bucket；entity remove 时撤销 membership；bucket compact 时只更新 SectionSlot；world clear 时清除索引。section 从 active 到 inactive 时只更新本地观察和运行态关联，不自动删除 durable relation。

#### Entity/World 范围

字段附着于单个 entity；BySection 是按 world section 建立的索引集合。section 的全局活跃状态不在本 Component 中声明最终 owner。

#### ID 与关系字段

仅包含 SectionCoordinate 和 bucket slot 候选，不包含 network ID、persistent ID、runtime UUID 或 anchor runtime reference。

#### 当前 NLTX 映射

dome\src\Terraria.Dome.Simulation\Leash\LeashSectionIndexComponent.cs:5-11 已有四个近似字段，但没有 bucket mutation、section 活跃事实消费或清理闭环，故为 partial。

#### 证据

Version4 BySection、ActiveSectionList 和 SectionEntityList 的读写点证明这是索引/工作集；ActiveSections.LastActiveTime 和 RemoteClient.TileSectionsCheckTime 分属其他范围，不能并入本 Component。

### 5.6 LeashedCritterBehaviorComponent

#### 职责

只附着于 critter 类型的 Leashed entity，承载 critter movement/行为所需的实例状态。NPC content ID 是映射字段，不把 NPC entity、dummy NPC 或完整 NPC 状态嵌入其中。

componentId: COMP-LEASH-CRITTER-BEHAVIOR  
name: LeashedCritterBehaviorComponent  
status: proposed  
componentOwner: LeashedEntitySimulation for critter-specific state  
crossSubsystemOwner: integration-review（NPC content ID、anchor style 和随机状态的共享语义）  
entityScope: 单个 critter Leashed entity  
lifecycle: critter definition 绑定 → defaults → active behavior → remove；表现字段不随本 Component 保存

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| NpcType | int? | null；full definition 未完成前不填充 | 权威 content 映射候选 | 必须是可解析的 NPC content ID；不能把 NPC instance ID 当成它 | partial | 完整参考 LeashedCritter.cs:15-19；协议 TerrariaPacketCodec.cs:4203-4212 |
| AnchorStyle | int | unresolved；prototype/anchor compatibility 提供 | definition/行为兼容候选 | 必须与 anchor host 的 style 兼容；owner 可能转移到 definition catalog 或 relation | partial / unresolved | 完整参考 LeashedCritter.cs:13-17；当前 LeashBehaviorRefComponent.cs:3-8 |
| IsAquatic | bool | unresolved；不能用 false 代替未知 prototype 默认 | 权威行为配置候选 | 只表达 critter 行为能力，不代表 Tile 液体存储状态 | partial | 完整参考 LeashedCritter.cs:37-41 |
| TargetPosition | TileCoordinate 候选；Version4 原型为 Point16 | unresolved；首次有效更新前必须有明确目标策略 | 权威行为状态候选 | 目标坐标必须在 anchor 关系允许的范围内；不能变成外部 entity reference | partial | 完整参考 LeashedCritter.cs:27-31；协议 TerrariaPacketCodec.cs:4218-4224 |
| RandomState | uint wire-compatible candidate；内部可能为 LCG32Random | unresolved | 权威行为状态候选 | 读写必须保持随机游标语义；不能让表现随机或外部全局随机源覆盖它 | partial / evidence-gap | 完整参考 LeashedCritter.cs:23-27；协议 TerrariaPacketCodec.cs:4218-4222 |
| WaitTime | short | unresolved | 权威行为状态候选 | 必须与等待状态的时间单位一致；负值策略未由 Version4 主文件闭合 | partial | 完整参考 LeashedCritter.cs:25-29；协议 TerrariaPacketCodec.cs:4218-4223 |
| State | byte | 0 作为当前状态常量候选；跨 prototype 默认仍需复核 | 权威行为状态 | 状态值必须由对应 critter behavior 解释；未知状态不得静默转为另一个状态 | partial | 完整参考 LeashedCritter.cs:27-31；Walker/JumperLeashedCritter.cs 状态常量 |

#### 字段不变量

- 本 Component 与 LeashedKiteBehaviorComponent 互斥；同一 entity 不得同时拥有 critter 和 kite content 映射。
- NpcType、TargetPosition、RandomState、WaitTime 和 State 是行为状态候选；frame、dummy NPC、绘制 offset 和历史轨迹不属于本 Component。
- RandomState 的内部类型和默认种子尚未由 Version4 主基线完整确认，不能以协议 uint 反推全部运行语义。
- AnchorStyle 的 owner 尚未锁定；若 definition catalog 已能唯一提供它，Component 不应保留重复镜像。

#### 生命周期

仅在 DefinitionId 对应 critter 类型时创建；defaults 和 anchor item 映射完成后成为有效行为状态。active 期间更新目标、随机游标、等待计时和状态；remove 后清除。网络 partial state 不能覆盖 identity 或 anchor relation。

#### Entity/World 范围

单个 critter entity。NPC content catalog、NPC entity 和 anchor TileEntity 是外部对象，不作为本 Component 的嵌套状态。

#### ID 与关系字段

NpcType 是 content ID，不是 NPC runtime ID、network ID 或 entity UUID；anchor relation 只通过 LeashedEntityAnchorRelationComponent 表达。

#### 当前 NLTX 映射

dome\src\Terraria.Dome.Simulation\Leash\LeashBehaviorRefComponent.cs:3-8 已有 BehaviorId、AnchorStyle、NpcType、ProjectileType 和 IsAquatic 的混合引用。目标设计把 NPC 专属字段移入本 Component，把 projectile 专属字段移入 kite Component，BehaviorId 的 definition owner 仍需裁决；状态为 partial。

#### 证据

完整参考 LeashedCritter 直接声明上述行为候选字段；协议解码在 TerrariaPacketCodec.cs:4197-4245 展开了 full/partial 的 NPC、尺寸、随机、等待、状态和目标字段。由于 Version4 的派生行为文件不完整，默认值和完整 owner 仍为 partial。

### 5.7 LeashedKiteBehaviorComponent

#### 职责

只附着于 kite 类型的 Leashed entity，承载 kite 特有的运动控制和兼容状态。它允许复用 projectile-like 的运动机制，但不把 kite 生命周期、锚点关系、section 索引或普通 projectile 状态并入同一组件。

componentId: COMP-LEASH-KITE-BEHAVIOR  
name: LeashedKiteBehaviorComponent  
status: proposed  
componentOwner: LeashedEntitySimulation for kite-specific state  
crossSubsystemOwner: integration-review（Projectile content ID 与共享运动机制边界）  
entityScope: 单个 kite Leashed entity  
lifecycle: kite definition 绑定 → defaults → active behavior → remove；历史/表现数组不随本 Component 保存

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| ProjectileType | int? | null；full definition 未完成前不填充 | 权威 content 映射候选 | 必须是可解析的 projectile content ID；不能把 projectile runtime entity 当成它 | partial | 完整参考 LeashedKite.cs:15；协议 TerrariaPacketCodec.cs:4177-4181 |
| Rotation | float | unresolved | kite 运动/表现边界候选 | 角度单位和是否属于权威运动必须与 full/partial 语义统一 | partial / evidence-gap | 完整参考 LeashedKite.cs:17-23；协议 TerrariaPacketCodec.cs:4183-4194 |
| KiteDistance | float | 250f，完整参考字段直接初始化 | 权威行为配置候选 | 必须为非负；必须与 anchor 的世界距离单位一致 | partial | 完整参考 LeashedKite.cs:25；协议行为邻域 :127-247 |
| WindTarget | float | unresolved | 权威行为状态候选 | 必须由 kite 风状态解释；不能由 CloudAlpha 反推 | partial | 完整参考 LeashedKite.cs:27,127-247；协议 TerrariaPacketCodec.cs:4186-4194 |
| WindCurrent | float | unresolved | 权威行为状态候选 | 必须与 WindTarget 使用同一单位和更新规则 | partial | 完整参考 LeashedKite.cs:29,127-247 |
| TimeCounter | float | unresolved | 权威行为计时候选 | 计时单位和重置点需保持行为语义；不得与全局 tick 直接混淆 | partial | 完整参考 LeashedKite.cs:31,127-247；协议 TerrariaPacketCodec.cs:4186-4194 |
| TimeWithoutWind | int | unresolved | 权威行为计时候选 | 不能为负；具体超时含义需由行为代码确认 | partial | 完整参考 LeashedKite.cs:35,127-247 |
| ProjectileLocalAI0/ProjectileLocalAI1 | float、float | unresolved | projectile-like 运动兼容状态候选 | 只能服务于 kite 行为；不能写入普通 projectile entity 的 AI owner | partial | 完整参考 LeashedKite.cs:37-39,127-247 |

#### 字段不变量

- 本 Component 与 LeashedCritterBehaviorComponent 互斥。
- ProjectileType 只是 content mapping；ProjectileAttachmentKind.Leashed 不能改变本子系统的生命周期或 owner。
- CloudAlpha、frame、frameCounter、spriteDirection、oldPos、oldRot、oldSpriteDirection 和 netOffset 暂不进入权威 Component；它们应由表现或协议边界按需计算/承载。
- Rotation 是否为权威运动字段尚未确定；在裁决前不得把它与公共 DirectionComponent 合并或双写。

#### 生命周期

仅在 DefinitionId 对应 kite 类型时创建。初始值由 kite definition/item compatibility 提供；active 期间维护风、距离、时间和 projectile-like AI 状态；despawn/remove 时清理运行态。表现历史不随 entity 权威状态恢复。

#### Entity/World 范围

单个 kite entity。普通 projectile 的 registry、owner、damage、lifetime 和 runtime state 不属于本 Component。

#### ID 与关系字段

ProjectileType 是 content ID，不是 projectile runtime ID 或 network ID；kite 与 anchor 的关系仅在 LeashedEntityAnchorRelationComponent 表达。

#### 当前 NLTX 映射

dome\src\Terraria.Dome.Simulation\Leash\LeashBehaviorRefComponent.cs:3-8 只有可选 ProjectileType 混合引用，未有 kite 行为状态组件。dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileAttachmentKind.cs:3-8 只提供 attachment kind，不能视为 kite 状态 owner；当前状态为 missing/partial。

#### 证据

完整参考 LeashedKite.cs:15-47,127-281 展示 kite 字段分布；协议 TerrariaPacketCodec.cs:4172-4195 只暴露一部分 full/partial payload。字段的权威/表现归类尚未全部闭合，故本 Component 是候选设计而非 baseline。

### 5.8 LeashedEntityLegacySlotComponent

#### 职责

隔离 Version4 whoAmI/ByWhoAmI 的兼容 slot。它只保存协议兼容映射，不是 runtime entity identity、persistent ID 或 network module ID 的替代品。

componentId: COMP-LEASH-LEGACY-SLOT  
name: LeashedEntityLegacySlotComponent  
status: proposed  
componentOwner: LeashedEntitySimulation for local compatibility mapping candidate  
crossSubsystemOwner: integration-review（legacy slot 与 protocol/session 的最终 owner）  
entityScope: 单个需要兼容映射的 Leashed entity；仅在兼容端点需要时附着  
lifecycle: allocate on compatibility-visible spawn → reuse-safe while entity exists → clear before slot reuse

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Slot | int | -1 表示未分配 | 兼容字段 | 仅在 0..ByWhoAmI.Count-1 且映射仍指向本 entity 时有效；不能作为持久化 key | confirmed / partial | Version4 ByWhoAmI/whoAmI:177-188；完整参考 AddNewEntity:377-410、TryGet:442-451 |
| SlotGeneration | uint 候选 | 0 | 兼容安全辅助候选 | 若采用，slot reuse 后必须变化；不能伪称为 Version4 已有字段 | missing | Version4/完整参考均只有可复用 slot，无 generation 字段 |

#### 字段不变量

- Slot 不得与 EntityIdentityComponent.UUID、PersistentAnchorId、NetworkId、DefinitionId 共用字段。
- 删除时先使 slot 映射失效，再允许 slot 重用；重复 remove 应不改变新实体。
- SlotGeneration 是否纳入 wire compatibility 尚未裁决；缺少它时，旧 partial packet 误更新新 entity 的风险必须保持为未决风险。

#### 生命周期

普通离线或不需要兼容 slot 的实例可以不附着。需要 Version4 协议映射时，在 slot 分配时创建；entity removed 或 clear 时清除；slot 重用策略和 generation 需要整合 owner 决定。

#### Entity/World 范围

单个 entity 的可选兼容 Component。它不附着到 ByWhoAmI 集合、section、TileEntity 或 session 对象。

#### ID 与关系字段

Slot 是 legacy network slot；它不是 network module ID。Version4 的 module ID 是 13，而 slot 是每个实体的可复用索引，两者必须分开。

#### 当前 NLTX 映射

当前 LeashedEntityStateComponent.InstanceId 是最接近的字段，但没有证据证明它承担 Version4 slot、稳定 UUID 或持久化身份中的哪一种语义。因此不直接复用该命名，目标 Component 状态为 missing。

#### 证据

Version4 full/partial/remove 直接写入或读取 whoAmI，完整参考 Remove 在 :425-440 清除 slot 并发送 remove；没有独立 generation 或 stable ID。该 Component 的 owner 必须保留 crossSubsystemOwner: integration-review。

## 6. Entity 与 Component 组合

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| 单个 critter Leashed entity | State、Lifecycle、Motion、AnchorRelation、SectionMembership、CritterBehavior | LegacySlot；现有 EntityIdentityComponent 作为跨域身份候选 | KiteBehavior | 实例定义、生命周期、运动、锚点、section 成员和 critter 行为共同组成一个可运行实例；kite 行为不能同时存在 |
| 单个 kite Leashed entity | State、Lifecycle、Motion、AnchorRelation、SectionMembership、KiteBehavior | LegacySlot；现有 EntityIdentityComponent 作为跨域身份候选 | CritterBehavior | kite 与 critter 共享实例/空间边界，但行为字段必须按 content kind 分离 |
| Leashed anchor TileEntity | 当前 TileEntityAnchorComponent、LeashedEntityAnchorComponent（已有 NLTX 宿主片段） | 与 Leashed entity 关系的外部关联 | 不附着上述 entity-side Component | TileEntity 持有 Origin、itemType 和 tile 宿主事实；不能拥有 ByWhoAmI、运动或 Leashed lifecycle |
| World section / section bucket | 无新的权威 Leashed Component | 由 SectionMembership 建立的索引观察 | 不把 global section activity 单独复制为 Leashed authority | BySection、ActiveSectionList 和 SectionEntityList 是集合索引/工作集；全局活跃事实 owner 尚未整合 |
| 客户端表现对象或 dummy | 无上述权威 Component | 可从 accepted snapshot 生成临时显示数据 | 不与 server Leashed entity 共用权威运动/生命周期字段 | 防止 Draw、dummy 和历史轨迹反向写入模拟状态 |

组合约束：

- 每个 Leashed entity 必须恰好拥有 critter 或 kite 中的一种行为 Component；无法从 DefinitionId 判定 content kind 时保持未绑定，不创建半初始化组合。
- State、Lifecycle、Motion、AnchorRelation 和 SectionMembership 共同描述 entity，但每个字段只能有一个权威 owner。
- EntityIdentityComponent.UUID、LegacySlot.Slot、PersistentAnchorId、DefinitionId 和协议 module ID 必须分别建模。
- relation host 和 relation consumer 可以同时存在，但不把 TileEntity 的 itemType 或 Origin 镜像到 entity relation Component。

## 7. 组件拆分与合并决策

| 决策 ID | 决策 | 处理 | 理由 | 状态 |
|---|---|---|---|---|
| DEC-COMP-01 | Definition 目录与实例状态分开 | 静态 Registry.Prototypes 不创建 Component；实例只保存 DefinitionId | 目录从进程初始化存在到结束，实例从 spawn 到 remove；生命周期和读写者不同 | proposed |
| DEC-COMP-02 | active/spawned/transition 合并为 lifecycle 概念 | 放入 Lifecycle，不让 section 或 anchor 各自拥有 active | 三者共同维护生命周期不变量；socket、存档和 TileEntity 不应进入其中 | proposed；transition owner decision-required |
| DEC-COMP-03 | 运动通用字段合并 | Position、Velocity、Direction、Width、Height 放入 Motion | 共同被行为、几何和状态表示读取；拆开会产生同步镜像；行为私有字段仍分离 | proposed |
| DEC-COMP-04 | critter 与 kite 行为拆开 | 使用两个互斥 Component | content ID、随机/等待/目标状态与风/AI 状态的变更原因和字段子集不同 | proposed |
| DEC-COMP-05 | runtime anchor 与 durable anchor 信息同属 relation，但字段分层 | 一个 relation Component 内同时允许 runtime reference、Tile 坐标和 persistent ID 候选，三者不互相替代 | 它们共同表达同一关系，但失效条件和最终跨域 owner 不同；是否拆出 persistent subrecord 未裁决 | decision-required |
| DEC-COMP-06 | section 成员与 global section activity 分开 | entity 只保留 membership/观察缓存；不宣称 global activity owner | Version4 有 ActiveSections 和 RemoteClient 两套事实/缓存，不能在本会话合并 | integration-review |
| DEC-COMP-07 | legacy slot 与稳定身份分开 | 使用可选 LegacySlot；UUID/persistent/network IDs 不复用 | slot 会回收，且仅是旧协议映射；混合会造成 stale packet 和存档 key 错配 | decision-required |
| DEC-COMP-08 | 协议和表现状态不进入权威 Component | 不把 binary payload、netOffset、old history、dummy 或 CloudAlpha 作为权威字段 | 这些对象的生命周期、读写者和失效条件不同，且会形成反向写入 | proposed |

## 8. 不单独创建 Component 的对象

| 对象 | 不单独创建 Component 的理由 |
|---|---|
| Registry.Prototypes、单个 critter prototype、LeashedKite prototype | 它们是静态 definition/behavior 配置。固定注册顺序由目录维护，不随每个实体复制；实体只持有 DefinitionId 和对应的行为 Component。 |
| BySection、ActiveSectionList、单个 SectionEntityList | 它们是按 section 的索引、工作集和压缩缓存。其 bucket slot 可变，不能成为身份或持久化状态。entity-side membership 只保存必要的索引观察。 |
| SectionCoordinates、Center、Size | 它们可由 anchor/position/width/height 计算。另建 Component 会产生重复权威值和同步要求。 |
| TELeashedEntityAnchor、TELeashedEntityAnchorWithItem、TEKiteAnchor、TECritterAnchor | 它们是 TileEntity 宿主、tile 合法性、item 内容和触发边界；不能拥有 Leashed entity 的 registry、运动或 lifecycle。 |
| itemType、Origin 和 tile placement data | 它们属于 anchor host 的内容/存储状态。entity relation 只引用 anchor，不复制 item inventory。 |
| NetModule、LeashedEntityModulePacket、full/partial/remove payload | 它们是兼容协议表示，生命周期以 packet 为单位；不应把 BinaryReader/Writer 或 wire enum 放入权威组件。 |
| module ID、session ID 和 socket state | 它们属于协议/session 范围。module ID 13 与 entity slot 是不同层次，不能共用 Component 字段。 |
| netOffset、oldPos、oldRot、oldSpriteDirection、dummy NPC/projectile | 它们是传输、插值、历史或表现缓存；不能决定 server authority。 |
| frame、frameCounter、spriteDirection、CloudAlpha | 当前证据无法证明它们是权威模拟事实；优先视作表现状态，待表现边界另行裁决，不进入本组件清单。 |
| ProjectileAttachmentKind.Leashed | 它只是 projectile 侧的 attachment 分类；不能接管 Leashed 的 anchor、section、lifecycle 或 identity owner。 |

## 9. 当前 NLTX 组件覆盖

| 当前路径 | 实际内容 | 目标 Component 映射 | 状态 |
|---|---|---|---|
| D:\TRbackup\NLTX\src\WorldInteraction\TileEntities\LeashedEntityAnchorComponent.cs:3-7 | ItemType、HasItem | 保留为 anchor host 内容；不映射为 entity-side relation 字段 | partial |
| D:\TRbackup\NLTX\src\WorldInteraction\TileEntities\TileEntityAnchorComponent.cs:5-9 | Origin | 保留为 TileEntity host 坐标；与 AnchorCoordinate 建立单向证据映射 | partial |
| D:\TRbackup\NLTX\src\WorldStorage\TileEntityStore.cs:3-13 | TileEntityId/坐标字典、next ID、mutation revision | PersistentAnchorId 的外部候选；不能由本文件宣布 owner | partial |
| D:\TRbackup\NLTX\src\WorldStorage\TileEntityRecord.cs:3-9 | ID、type、anchor、update flag | anchor persistence 候选；不是 Leashed entity state | partial |
| D:\TRbackup\NLTX\src\WorldStorage\WorldSectionState.cs:3-22、SectionCoordinate.cs:3 | section flags、计数和 revision、坐标值对象 | Section 和 global activity 的跨域候选；未证明 Leashed owner | partial |
| D:\TRbackup\NLTX\src\Share\Entity\Components\EntityIdentityComponent.cs:5-17 | Guid UUID identity | 作为 entity identity 候选；不映射为 slot、definition 或 persistent anchor ID | existing；Leashed 绑定 unresolved |
| D:\TRbackup\NLTX\src\Share\Entity\Components\SpatialReferenceComponent.cs:5-20、src\Relationships\EntityReference.cs:5-10 | parent reference、offset、space、Guid relation | AnchorReference 的值对象候选；不证明一对一生命周期 | partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashAnchorStateComponent.cs:6-16 | Tile 坐标、Entity?、style、active、section 派生 | 拆为 AnchorRelation 与 SectionMembership；Arch.Core Entity 类型不直接锁定 | partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashBehaviorRefComponent.cs:3-8 | behavior、anchor style、NPC/projectile type、aquatic | 按 critter/kite 拆分；BehaviorId/AnchorStyle owner 未决 | partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityLifecycle.cs:3-9 | 四值生命周期枚举 | 支撑 Lifecycle.State | existing；组合仍 partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityStateComponent.cs:5-15 | Instance/type/anchor/tile/active/lifecycle/spawned 混合字段 | TypeId → State；active/lifecycle/spawned → Lifecycle；anchor → Relation；InstanceId → slot 语义待裁决 | partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashSectionIndexComponent.cs:5-11 | section、slot、active、last activation tick | SectionMembership | partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\LeashedEntityModulePacket.cs:3-10 | immutable module envelope | 只作为兼容输入/输出表示；不创建同义权威 Component | partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\TerrariaPacketCodec.cs:44,569-645,4172-4297 | module 13、full/partial/remove、kite/critter 字段和 payload 校验 | 用于行为字段证据和类型映射；不把 packet records 作为组件状态 | partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Session\TerrariaSession.cs:243-397 | active session 接收 module 13 并调用 Leashed decoder | 证明协议输入边界存在；未证明 entity authority apply 或组件创建 | partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\ProjectileAttachmentKind.cs:3-8 | Leashed attachment kind | 仅保留 projectile 侧分类；不映射为 Leashed lifecycle/registry Component | partial |

综合判断：当前 NLTX 的 Leash 组件数量和协议模型已经足以形成字段映射草图，但没有证据证明 8 个目标 Component 已组成闭合的 authoritative entity。因而本文件的 nltxStatus 保持 missing，局部文件保持 partial，不声明实现完成。

## 10. 组件级 evidence-gap

| gapId | 缺口 | 影响的 Component | 当前处理 |
|---|---|---|---|
| EG-COMP-01 | Version4 主 LeashedEntity.cs 缺 position、velocity、direction、width、height；完整参考才有同文件字段 | Motion | 字段列为 partial；默认值、值对象映射和首次初始化保持 unresolved |
| EG-COMP-02 | Version4 只有可复用 whoAmI slot，没有独立 stable runtime/persistent ID | State、LegacySlot | UUID、slot、persistent ID 分开候选；owner 留 integration-review |
| EG-COMP-03 | Version4 anchor OnRemoved、OnWorldLoaded、restore 方法为空体，且 TileEntity/WorldStorage owner 未闭合 | AnchorRelation | 只确认 AnchorCoordinate；PersistentAnchorId、RelationRevision 和删除后保留策略为 unresolved |
| EG-COMP-04 | ActiveSections 与 RemoteClient 各有 section 活跃时间/缓存，Leashed 只读取它们 | SectionMembership | IsSectionActive、LastActivationTick 只标缓存；global activity owner 为 integration-review |
| EG-COMP-05 | Version4 Remove、StreamNetUpdates、section mutation 和静态订阅存在空体/缺失 | Lifecycle、SectionMembership | 生命周期和 slot cleanup 设计为候选，不声称当前行为闭合 |
| EG-COMP-06 | critter 默认值、随机内部类型、行为状态和表现字段的边界不能仅由 packet 反推 | CritterBehavior | 保留直接字段候选；未知默认值不补造；variant/extension/表现字段暂缓 |
| EG-COMP-07 | kite 字段中 rotation、wind、CloudAlpha、AI 和历史数组的权威归属不完整 | KiteBehavior、Motion | 只纳入行为候选；历史/表现字段排除；rotation owner 为 decision-required |
| EG-COMP-08 | 当前 NLTX 同时有 Entity?、EntityReference、多个 TileCoordinate/section value object 候选 | AnchorRelation、SectionMembership | 不在本会话创建共享类型；类型最终归属和转换方向留给整合 |

## 11. 未决组件 owner

| decision ID | 冲突字段/范围 | 候选 owner A | 候选 owner B | 各方案对 Component 组成的影响 | 为什么当前不能宣布最终 owner |
|---|---|---|---|---|---|
| BD-COMP-01 | runtime UUID、persistent entity ID、legacy slot、network module ID | EntityIdentityComponent/通用实体层持有 stable runtime identity；Leashed 只持 LegacySlot.Slot | WorldStorage 持有 persistent identity，Leashed 只引用 | A 保持 State 无身份字段，B 需要 Relation 增加 persistent reference；两者都不能让 slot 进入 State authority | Version4 没有独立 stable ID，NLTX UUID 与 slot 映射未建立；跨实体/存档/协议消费者不同 |
| BD-COMP-02 | AnchorReference、AnchorCoordinate、PersistentAnchorId、RelationRevision | WorldInteraction/TileEntity host 持 durable anchor ID，Leashed relation 持 runtime reference/坐标 | Leashed relation 持统一 relation record，WorldStorage 只持 host record | A 的 Relation 需要可选 persistent reference；B 可能把 relation revision 和 durable ID 一起放入 Relation，但会扩大 Leashed owner | Version4 只直接确认 Point16 AnchorPosition，restore/deletion 实现为空；完整参考不能替代主基线 |
| BD-COMP-03 | IsSectionActive、LastActivationTick 与 global section activity | WorldStorage/NetworkSession 提供只读 global fact，Leashed 只缓存 | Leashed 持 section activity authority | A 只保留 SectionMembership cache；B 需要增加 section/world-scope Component 或把缓存升级为 authority | Version4 同时存在 ActiveSections.LastActiveTime 和远端 cache；本任务不能替相邻子系统裁决 |
| BD-COMP-04 | TypeId、BehaviorId、AnchorStyle、NPC/projectile content ID | Definition catalog 唯一保存 content mapping，State 只存 DefinitionId | 行为 Component 保存 content IDs 和兼容 style | A 减少行为 Component 字段；B 保留 full payload 所需字段但产生 definition 镜像 | Version4 prototype、derived anchor 和当前 LeashBehaviorRefComponent 的 owner 关系尚未闭合，且两种 content kind 字段子集不同 |

在上述决策完成前，任何涉及 PersistentAnchorId、SlotGeneration、global section authority 或 AnchorStyle 的字段都只能视为候选，不能升级为 baseline。

## 12. 最终 Component 清单

当前会话提出 8 个目标 Component。数量不包括静态 definition 目录、TileEntity host、section 集合索引、协议 packet、客户端表现对象及现有通用身份值对象。

| componentId | name | entityScope | status | componentOwner | crossSubsystemOwner |
|---|---|---|---|---|---|
| COMP-LEASH-STATE | LeashedEntityStateComponent | 单个 Leashed entity | partial（现有片段；目标字段边界仍为 proposed） | LeashedEntitySimulation | integration-review（DefinitionId/identity 交叉使用） |
| COMP-LEASH-LIFECYCLE | LeashedEntityLifecycleComponent | 单个 Leashed entity | proposed | LeashedEntitySimulation | none for local state |
| COMP-LEASH-MOTION | LeashedEntityMotionComponent | 单个 Leashed entity | proposed | LeashedEntitySimulation | integration-review（运动值对象） |
| COMP-LEASH-ANCHOR-RELATION | LeashedEntityAnchorRelationComponent | 单个 Leashed entity，关系指向独立 anchor TileEntity | proposed | LeashedEntitySimulation for entity-side relation | integration-review |
| COMP-LEASH-SECTION-MEMBERSHIP | LeashedEntitySectionMembershipComponent | 单个 Leashed entity，按 section 建索引 | proposed | LeashedEntitySimulation for local membership | integration-review |
| COMP-LEASH-CRITTER-BEHAVIOR | LeashedCritterBehaviorComponent | 单个 critter Leashed entity | proposed | LeashedEntitySimulation | integration-review（NPC content/style） |
| COMP-LEASH-KITE-BEHAVIOR | LeashedKiteBehaviorComponent | 单个 kite Leashed entity | proposed | LeashedEntitySimulation | integration-review（projectile content/运动边界） |
| COMP-LEASH-LEGACY-SLOT | LeashedEntityLegacySlotComponent | 单个需要兼容映射的 Leashed entity | proposed | LeashedEntitySimulation compatibility candidate | integration-review |

最终组合约束：

- critter entity = 前 5 个通用 Component + CritterBehavior，kite entity = 前 5 个通用 Component + KiteBehavior；两者只能选其一；
- LegacySlot 可选，不参与实体权威身份；
- State 当前只有 partial 覆盖，不能据此声称其余 7 个 Component 已创建；
- 跨子系统共享的 identity、坐标、section、content ID、relation 和 revision 均需保留 crossSubsystemOwner: integration-review。

## 13. 最终声明

本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。
