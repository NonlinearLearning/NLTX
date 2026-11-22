# SpatialSimulation Component Design

## 1. 设计元数据

~~~text
subsystemId: SpatialSimulation
taskNumber: 09
sourceReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-spatial-simulation-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-spatial-simulation-component-design.md
designScope: component-only
designStatus: decision-required
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
selectionMethod: 仅使用当前会话明确产生的研究报告；以 D:\TRbackup\Version4 为行为证据首要来源，以完整参考源码作 version-drift 补证，以 tModLoader 文档作公开语义交叉核对，以有限 ECS 参考作组件粒度核对，并以当前 NLTX 源码核对 existing/partial 覆盖
~~~

本文件只整理 SpatialSimulation 的组件组成、字段归属、状态分类、生命周期、实体组合和 owner 缺口。设计状态为 decision-required，因为 Position/Velocity 的统一 owner、空间与坐标类型、LiquidContact 的生成边界、CollisionResult 接触集合生命周期以及 CollisionPolicy 的最终生产者仍未裁决。

## 2. 设计范围与排除范围

### 2.1 设计范围

SpatialSimulation 的组件设计覆盖以下实体侧状态：

- 持续位置、速度、加速度和重力状态；
- 碰撞几何形状及其局部偏移；
- 实体与父实体、空间和 section 的引用关系；
- 位置/速度/方向的历史快照；
- Tile、实体、液体和斜坡相关的碰撞策略输入；
- 实体本 tick 的液体接触派生状态；
- 本 tick 的碰撞阻挡、法线、接触集合和台阶结果。

这些组件只表达状态及其归属，不把 Version4 Collision 中的几何规则、Tile 读取、副作用或隐藏共享变量变成新的状态对象。

### 2.2 排除范围

以下内容不在本文件中定义：

- 任何运行时执行结构、访问结构、提交结构或外部集成结构；
- 调度顺序、主循环、网络发送、存档、客户端渲染、验证和迁移；
- 液体数量、液体类型写集、传播、合并、buffer、网络集合或存档集合；
- Tile 类型、墙、激活、斜坡、half-brick、actuator 等世界事实的实体镜像；
- Player、NPC、Projectile、Item 的专用行为规则；
- 将静态 Collision flags、缓存、空 Tile 自动创建或调用上下文提升为组件字段。

本设计不宣称组件已创建、已接入或已经替代 Version4 行为。

## 3. 组件设计依据

### 3.1 证据优先级

1. D:\TRbackup\Version4 是真实字段、读写者、生命周期和副作用的首要基线。
2. D:\TRbackup\无任何删减通过编译只补充 Version4 已存在文件的成员和调用链；涉及行号统一视为 version-drift。
3. D:\TRbackup\tmodloader-api-docs-stable 只交叉核对公开字段语义和公开边界，不覆盖 Version4 私有实现。
4. C:\Users\shan\Downloads\ECS\space-station-14-master 只用于核对组件粒度、Entity 组合和状态隔离；没有用于推断 Terraria 语义。
5. 当前 NLTX 的 src、Test、dome\src 和 dome\Test 只用于确认 existing/partial 状态，不等同于行为等价或完成接入。

### 3.2 直接证据摘要

| 证据主题 | 实际路径和成员 | 对组件设计的限制 | evidenceStatus |
|---|---|---|---|
| 实体空间状态 | D:\TRbackup\Version4\Terraria\Entity.cs:6-34；Entity、whoAmI、position、velocity、oldPosition、oldVelocity、oldDirection、direction、width、height、wet、shimmerWet、honeyWet、wetCount、lavaWet | 当前值、历史值、几何尺寸和湿状态在 Version4 Entity 中共存，但 ECS 不能因此形成巨型通用组件 | confirmed |
| 派生几何 | D:\TRbackup\Version4\Terraria\Entity.cs:36-183；AnyWet、Center、边缘点、Hitbox、Size | 几何派生值由位置和形状推导，不作为独立权威字段 | confirmed |
| 碰撞隐藏共享状态 | D:\TRbackup\Version4\Terraria\Collision.cs:9-73；TileContactSide、TileContact、stair、stairFall、honey、shimmer、sloping、up、down、contacts、_cacheForConveyorBelts | 静态 flags、接触临时集合和传送带缓存不是实体组件 | confirmed |
| 液体接触副作用 | D:\TRbackup\Version4\Terraria\Collision.cs:944-1120；GetWaterLine、WetCollision、LavaCollision | Entity 侧湿状态与 LiquidSimulation 的世界液体写集必须分离；WetCollision 对静态 honey/shimmer 有隐式写入 | confirmed |
| 斜坡和 Tile 解析 | D:\TRbackup\Version4\Terraria\Collision.cs:1121-1797、:2736-2801、:3386-3523 | 斜坡、台阶、平台、Tile 接触和传送带是规则输入/结果来源，不是组件集合 | confirmed |
| Tile 世界事实 | D:\TRbackup\Version4\Terraria\Tile.cs:6-24、:56-62、:220-319、:335-447 | liquid、active、type、wall、half-brick、slope、actuator 属于 Tile/World 范围，不复制到实体组件 | confirmed |
| 专用实体字段 | D:\TRbackup\Version4\Terraria\Player.cs:14124-14321、:17445-17602；NPC.cs 约 :6371、:78625-78743；Projectile.cs:194、:202、:250、约 :15043、:15513；Item.cs 约 :258、:48768-48778 | 专用策略进入 CollisionPolicy 的候选输入；专用行为和兼容字段不吞入通用 MovementState | confirmed |
| 公开空间语义 | D:\TRbackup\tmodloader-api-docs-stable\class_entity.html:200-231；position、velocity、oldPosition、oldVelocity、wet、whoAmI | position 是世界坐标左上角，velocity 是每 tick 世界速度；whoAmI 是数组索引而非稳定持久化 ID | confirmed |
| 当前 NLTX 状态 | src\Physics、src\Share\Entity\Components、dome\src\Terraria.Dome.Simulation\Movement、Physics、Liquid、World | 已有类型覆盖了部分目标字段，但统一类型、owner 和生命周期未闭合 | partial |

### 3.3 组件粒度依据

保持七个候选组件的理由是：当前值、几何、空间关系、历史、策略、液体接触和碰撞结果具有不同的失效条件、读写者和状态范围。Position 与 Velocity 在一个实体 tick 中共同描述持续运动，暂保持在同一 MovementState 候选中；但其最终 owner 尚未锁定。LiquidContact 只保留实体接触派生状态，不保存 Tile 液体权威事实。CollisionResult 只保留结果快照，不把接触对象永久转化为实体关系。

### 3.4 当前 NLTX 参照

当前根 src 已有 LocationComponent、VelocityComponent、PhysicsStateComponent、ColliderComponent、SpatialReferenceComponent、MotionHistoryComponent、CollisionPolicyComponent、LiquidComponent 和 CollisionResultComponent。dome\src 另有 Movement、Physics、Liquid 和 World 下的对应或相邻类型。它们是现状证据，不表示本文件的七个 proposed 组件已经存在，也不表示根 src 与 dome\src 的类型已经统一。

## 4. Version4 成员到 Component 归属表

下表列出直接影响组件字段和状态分类的 Version4 成员。没有合适实体组件归属的成员明确标记为不进入七个组件，而不是强行塞入通用状态。

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| Entity.whoAmI | Entity 数组索引 | 当前实体数组位置 | 兼容/运行时索引，不是稳定 ID | 实体进入数组时可用，离开时失效 | 不进入七个组件；由现有实体身份范围承载 | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:6-8；tModLoader class_entity.html:200-231 |
| Entity.position | 位置向量 | 实体世界坐标左上角 | 权威候选 | 实体创建、每次空间更新、实体销毁 | MovementState (status: proposed).Position | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:10；tModLoader class_entity.html:200-231 |
| Entity.velocity | 速度向量 | 每 tick 世界速度 | 权威候选 | 与 position 同一持续运动生命周期 | MovementState (status: proposed).Velocity | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:12；tModLoader class_entity.html:200-231 |
| Entity.oldPosition | 位置向量 | 上一次采样位置；Projectile extra update 具有特殊语义 | 快照/兼容 | 当前值更新前采样，实体销毁时清理 | MotionHistory (status: proposed).PreviousPosition | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:14；Projectile.cs 约 :15043 |
| Entity.oldVelocity | 速度向量 | 上一次采样速度 | 快照/兼容 | 与 oldPosition 同采样生命周期 | MotionHistory (status: proposed).PreviousVelocity | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:16 |
| Entity.oldDirection | int | 上一次方向 | 快照/兼容 | 实体空间历史生命周期；初始化语义未锁定 | MotionHistory (status: proposed).PreviousDirection | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:18；NPC.cs 约 :78719-78743 |
| Entity.direction | int | 当前朝向/实体行为状态 | 兼容候选，不是所有空间实体的共同不变量 | 随实体行为变化 | 不作为 MovementState canonical 字段；按实体域保留 | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:20 |
| Entity.width、height | int | 实体碰撞包围盒尺寸 | 权威候选 | 与实体形状共同创建和销毁 | CollisionShape (status: proposed).Width/Height | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:22-24 |
| Entity.wet | bool | 是否处于湿状态 | 派生/兼容 | 依据液体接触重新计算，离开液体时清理 | LiquidContact (status: proposed).IsWet | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:26；Player.cs:14144-14162 |
| Entity.shimmerWet | bool | 是否接触 shimmer | 派生/兼容 | 随本次液体接触结果更新 | LiquidContact (status: proposed).IsShimmerWet | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:28；Collision.cs:1001-1084 |
| Entity.honeyWet | bool | 是否接触 honey | 派生/兼容 | 随本次液体接触结果更新 | LiquidContact (status: proposed).IsHoneyWet | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:30；Collision.cs:1001-1084 |
| Entity.wetCount | 未锁定的 Version4 标量 | 连续湿状态计数 | 派生/兼容 | 湿状态持续时递增或维持，干燥时按实体语义清理 | LiquidContact (status: proposed).WetTickCount | partial | D:\TRbackup\Version4\Terraria\Entity.cs:32；Dome LiquidContact 仅作字段形状补证 |
| Entity.lavaWet | bool | 是否接触 lava | 派生/兼容 | 随本次液体接触结果更新 | LiquidContact (status: proposed).IsLavaWet | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:34；Collision.cs:1085-1120 |
| Entity.AnyWet、Center、边缘点、Hitbox、Size | 派生属性 | 从位置、尺寸和湿字段生成的视图 | 派生值 | 读取时计算或随基础字段变化 | 不新增组件；由 MovementState、CollisionShape、LiquidContact 共同表达 | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:36-183 |
| Collision.TileContactSide | enum | Tile 接触方向枚举 | 结果内部值 | 单次碰撞解析或结果快照内有效 | 不单独创建组件；必要信息进入 CollisionResult (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\Collision.cs:11-19 |
| Collision.TileContact | 结构 | 一个 Tile 接触的几何/方向信息 | 结果内部值或快照元素 | 单次解析有效 | CollisionResult (status: proposed).TouchedTiles 的候选元素 | confirmed | D:\TRbackup\Version4\Terraria\Collision.cs:21-44 |
| Collision.stair、stairFall、honey、shimmer、sloping、up、down | static bool | 斜坡、液体和轴结果的共享临时 flags | 缓存/兼容共享状态 | 受方法调用上下文影响，失效边界不独立 | 不进入任何组件；保留为 evidence-gap 和兼容风险 | confirmed | D:\TRbackup\Version4\Terraria\Collision.cs:55-69、:1005-1055、:1232-1237、:1633-1638 |
| Collision.contacts | static 集合 | 碰撞过程中的临时接触集合 | 缓存/临时快照 | 单次调用或重入范围，具体边界未锁定 | 不单独创建组件；接触结果候选进入 CollisionResult (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\Collision.cs:71 |
| Collision._cacheForConveyorBelts | static 缓存 | 传送带相关缓存 | 缓存 | 受世界和调用上下文影响 | 不进入组件 | confirmed | D:\TRbackup\Version4\Terraria\Collision.cs:73、:3386-3523 |
| Collision.CanHit、CanHitWithCheck、CanHitLine、HitLine | 方法 | 空间资格、实体/Tile 路径和线段判断 | 规则/派生结果 | 单次输入计算 | 不创建方法专属组件；策略输入可来自 CollisionPolicy (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\Collision.cs:197-316、:414、:608 |
| Collision.GetWaterLine、WetCollision、LavaCollision | 方法 | 从 Tile/世界状态解析实体液体接触 | 派生计算，当前实现带隐式共享写入 | 单次液体接触解析 | 结果字段进入 LiquidContact (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\Collision.cs:944-1120 |
| Collision.WalkDownSlope、SlopeCollision、TileCollision、StepDown、StepUp | 方法 | 斜坡、Tile、台阶和平台接触规则 | 规则/派生结果 | 单次空间解析 | 结果字段进入 CollisionResult (status: proposed)；规则不变成组件 | confirmed | D:\TRbackup\Version4\Terraria\Collision.cs:1121-1797、:2736-2801 |
| Tile.type、wall、liquid、headers、frameX、frameY | Tile 基础字段 | 世界 Tile 的结构、液体和帧状态 | 权威世界事实 | Tile 创建、结构变化、世界生命周期 | 不进入实体组件；保留在 Tile/World 状态范围 | confirmed | D:\TRbackup\Version4\Terraria\Tile.cs:6-24 |
| Tile.liquidType、nactive、slope、half-brick、lava、honey、shimmer、water | Tile 属性/方法 | 从 Tile 状态推导的世界液体和几何属性 | 权威或派生世界事实 | 随 Tile 状态变化 | 不进入实体组件；LiquidContact 只保存实体接触派生值 | confirmed | D:\TRbackup\Version4\Terraria\Tile.cs:56-62、:220-319、:335-447 |
| Player SlopeDownMovement、WetCollision、TryFloatingInFluid、DryCollision、TileCollision | 方法 | Player 专用的斜坡、湿/干移动选择 | 实体域规则和结果消费者 | Player 空间更新期间 | 不新增通用组件；专用策略映射到 CollisionPolicy (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\Player.cs:14124-14321 |
| Player position、velocity、oldPosition 写回 | 字段写入 | Player 持续位置、速度和历史更新 | 权威候选/快照 | Player 更新生命周期 | MovementState、MotionHistory (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\Player.cs 约 :17445-17602 |
| NPC.collideX、collideY | bool | NPC 的轴碰撞兼容状态 | 派生/兼容 | NPC 碰撞更新时刷新 | 不作为通用字段；由 CollisionResult (status: proposed).BlockedAxes 派生或在 NPC 域保留 | confirmed | D:\TRbackup\Version4\Terraria\NPC.cs 约 :6371-6373、:78625-78717 |
| NPC.UpdateCollision、Collision_MoveWhileDry、ApplyTileCollision | 方法 | NPC 专用干移动、轴结果和 Tile 应用 | 实体域规则和结果消费者 | NPC 空间更新期间 | 不新增通用组件；输入使用 MovementState、CollisionShape、CollisionPolicy (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\NPC.cs 约 :78625-78743 |
| Projectile.tileCollide、ignoreWater、correctSlopeCollision | bool | Projectile 的 Tile、液体和斜坡策略输入 | 权威实体策略 | Projectile 生命周期 | CollisionPolicy (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\Projectile.cs:194、:202、:250 |
| Projectile.oldPosition、HandleMovement、UpdatePosition | 字段/方法 | Projectile 历史、高速分步和位置更新 | 快照、规则和权威候选 | 普通 tick 与 extra update 均可能参与 | MotionHistory、MovementState、CollisionPolicy (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\Projectile.cs 约 :15043、:15513、:15642-15819、:17995-18035 |
| Item.noWet | bool | Item 是否忽略湿碰撞 | 权威实体策略 | Item 生命周期 | CollisionPolicy (status: proposed).IgnoresLiquids 的候选来源 | confirmed | D:\TRbackup\Version4\Terraria\Item.cs 约 :258 |
| WorldItem 的 WetCollision 调用 | 方法调用 | Item 空间消费者的液体接触读取 | 派生结果消费者 | WorldItem 生成/更新路径 | LiquidContact (status: proposed) 的候选消费者 | confirmed | D:\TRbackup\Version4\Terraria\Item.cs 约 :48768-48778 |

## 5. Component 定义

以下七个 Component 全部是设计提案，尚未在当前 NLTX 中以本文件的统一形式创建。字段类型为候选设计类型；标记为 unresolved 的类型、默认值或 owner 不得视为已锁定契约。

### 5.1 MovementState

#### 职责

保存一个实体参与空间模拟所需的持续运动状态。它把同一空间实体的当前位置、速度、加速度和重力参数放在一个候选内聚边界中，但不保存碰撞形状、实体关系、历史快照、液体接触或专用实体行为。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Position | System.Numerics.Vector2 | Zero 候选 | 权威候选 | X/Y 必须为有限值；语义候选为世界坐标左上角；最终位置 owner 未决 | partial | Version4 Entity.position；tModLoader class_entity.html:200-231；NLTX LocationComponent |
| Velocity | System.Numerics.Vector2 | Zero 候选 | 权威候选 | X/Y 必须为有限值；表示每 tick 世界速度；最终速度 owner 未决 | partial | Version4 Entity.velocity；tModLoader class_entity.html:200-231；NLTX VelocityComponent |
| Acceleration | System.Numerics.Vector2 | Zero 候选 | 权威候选 | X/Y 必须为有限值；不得把重力缩放重复写入加速度而未定义规则 | partial | NLTX src\Physics\PhysicsStateComponent.cs:5-14 |
| GravityDirection | Terraria.Physics.GravityDirection | Down 候选；枚举零值语义需保持 | 权威策略输入 | 只能使用已定义枚举值；与 GravityScale 的乘法语义需保持一致 | partial | NLTX src\Physics\PhysicsStateComponent.cs:5-14；Version4 实体移动上下文 |
| GravityScale | float | 0 仅为语言零值，业务默认未锁定 | 权威策略输入 | 必须为有限值；零值究竟表示无重力还是未初始化尚未裁决 | unresolved | NLTX src\Physics\PhysicsStateComponent.cs:5-14 |
| IsGrounded | bool | false | 派生/兼容 | 不能与 Tile 接触结果形成未经裁决的双向权威写入 | partial | NLTX src\Physics\PhysicsStateComponent.cs:5-14；Version4 Player/NPC 台阶和碰撞路径 |
| IsMovementLocked | bool | false | 权威策略状态候选 | 锁定时的 Position/Velocity 更新语义未在本设计中扩展 | partial | NLTX src\Physics\PhysicsStateComponent.cs:5-14 |

#### 字段不变量

- Position 和 Velocity 必须描述同一个实体空间实例；不得与父实体偏移或 Tile 坐标互换。
- Position、Velocity、Acceleration 和 GravityScale 的浮点值必须有限；具体数值范围和溢出策略未锁定。
- IsGrounded 是接触后的兼容/派生状态候选，不复制为独立 Grounded 组件。
- Version4 的位置和速度是 Entity 聚合字段；当前 NLTX 将位置和速度拆为 LocationComponent、VelocityComponent，是否统一由 BD-COMP-01 裁决。

#### 生命周期

实体获得可移动空间能力时创建；空间实体初始化时写入候选零值或实体域提供的初始值；实体每次空间状态更新时更新；实体移除或不再具有空间能力时清理。字段是否参与恢复、回放或跨网络边界不在本文件中裁决。

#### Entity/World 范围

单个实体范围。它不拥有 Tile、World、Section 或 LiquidSimulation 的世界状态。

#### ID 与关系字段

不保存 whoAmI、Guid、NetworkId、PersistentEntityId 或 ParentEntity。实体身份和空间关系分别由现有身份范围与 SpatialReference 承载。

#### 当前 NLTX 映射

目标覆盖为 partial。src\Share\Entity\Components\LocationComponent.cs:3-13 与 VelocityComponent.cs:3-13 已分别存在位置和速度；src\Physics\PhysicsStateComponent.cs:5-14 已存在加速度、重力和锁定状态，但没有统一的 MovementState。Position/Velocity owner 仍由 BD-COMP-01 裁决。

#### 证据

- Version4 Entity.cs:10、:12 证明 position 和 velocity 是实体空间字段。
- tModLoader class_entity.html:200-231 交叉确认 position 是世界坐标左上角、velocity 是每 tick 世界速度。
- 当前 NLTX 组件使用两个 float 字段而非 Vector2，统一类型和 owner 仍为 partial。

componentId: SPATIAL.COMP.MOVEMENT_STATE
name: MovementState
status: proposed
componentOwner: SpatialSimulation（candidate）
crossSubsystemOwner: integration-review
entityScope: 单个可移动实体
lifecycle: 实体空间能力创建时创建，实体持续期间更新，空间能力移除时清理

### 5.2 CollisionShape

#### 职责

保存实体用于空间接触计算的几何包围盒及局部偏移。它只描述形状，不保存位置、速度、Tile 属性或碰撞策略。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Width | float | 0 候选 | 权威几何状态 | 必须为有限值且不得为负；Version4 int 到 float 的转换未锁定 | partial | Version4 Entity.cs:22；NLTX ColliderComponent.cs:3-33 |
| Height | float | 0 候选 | 权威几何状态 | 必须为有限值且不得为负；Version4 int 到 float 的转换未锁定 | partial | Version4 Entity.cs:24；NLTX ColliderComponent.cs:3-33 |
| OffsetX | float | 0 候选 | 权威几何状态 | 必须为有限值；含义不得与 ParentOffset 混淆 | partial | NLTX ColliderComponent.cs:3-33；Version4 Entity 派生 Hitbox |
| OffsetY | float | 0 候选 | 权威几何状态 | 必须为有限值；含义不得与 ParentOffset 混淆 | partial | NLTX ColliderComponent.cs:3-33；Version4 Entity 派生 Hitbox |
| Kind | CollisionShapeKind | Rectangle | 权威几何策略 | 只能使用已声明的形状种类；当前 Version4 证据主要覆盖矩形 | partial | NLTX src\Share\Entity\Components\CollisionShapeKind.cs:3-7 |
| IsEnabled | bool | 命名构造函数候选 true；struct default 为 false | 权威资格状态候选 | 未启用形状不得被解释为零尺寸有效碰撞；默认冲突未裁决 | unresolved | NLTX src\Share\Entity\Components\ColliderComponent.cs:3-33 |

#### 字段不变量

- Width、Height、OffsetX、OffsetY 必须是有限值，Width 和 Height 不得为负。
- OffsetX/OffsetY 是形状相对实体位置的局部偏移；它们不表示父实体关系。
- Kind=Rectangle 是当前覆盖范围内的候选 canonical 形状；其他形状不得因枚举存在而宣称已支持。
- IsEnabled 的构造函数默认与值类型零值冲突，必须在整合裁决后锁定。

#### 生命周期

实体需要空间形状时创建；实体尺寸或形状策略变化时更新；实体移除、形状失效或不再参加空间计算时清理。几何派生的 Hitbox 不作为独立持久状态创建。

#### Entity/World 范围

单个实体范围。Tile 的 slope、half-brick、actuator、active 和 liquid 不属于此组件。

#### ID 与关系字段

不保存实体 ID、父实体引用或 TileCoordinate。形状与实体的关联由 Entity 组合关系表达。

#### 当前 NLTX 映射

当前 src\Share\Entity\Components\ColliderComponent.cs:3-33 已存在，状态为 existing；目标覆盖为 partial。现有组件包含 float 宽高和偏移、Rectangle 形状，但 Version4 使用 int 宽高，转换规则、IsEnabled 默认和非矩形语义尚未锁定。

#### 证据

- Version4 Entity.cs:22-24 提供 width、height 的共同实体字段证据。
- Version4 Entity.cs:48-183 的 Hitbox、Size 和边缘点证明几何值可由基础位置/尺寸派生。
- 当前 NLTX ColliderComponent 的现有默认行为不能直接被解释为 Version4 行为等价。

componentId: SPATIAL.COMP.COLLISION_SHAPE
name: CollisionShape
status: proposed
componentOwner: SpatialSimulation（candidate）
crossSubsystemOwner: integration-review
entityScope: 单个具有空间几何的实体
lifecycle: 实体获得形状时创建，几何变化时更新，形状移除时清理

### 5.3 SpatialReference

#### 职责

保存实体与父实体、空间以及局部偏移之间的关系。它不复制实体当前位置，也不把 section、Tile 坐标或父实体身份编码成新的基础 ID。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| ParentEntity | EntityReference? | null | 关系状态候选 | 非空时必须与 IsParentRelative 和偏移语义一致；EntityReference owner 未决 | partial | NLTX src\Share\Entity\Components\SpatialReferenceComponent.cs:5-20；Relationships\EntityReference.cs:5-10 |
| ParentOffsetX | float | 0 | 关系状态 | 必须为有限值；只表达父关系局部偏移 | partial | NLTX SpatialReferenceComponent.cs:5-20 |
| ParentOffsetY | float | 0 | 关系状态 | 必须为有限值；只表达父关系局部偏移 | partial | NLTX SpatialReferenceComponent.cs:5-20 |
| IsParentRelative | bool | false | 关系状态 | 为 false 时 ParentOffset 是否仍保留仅作数据还是必须清零，未锁定 | partial | NLTX SpatialReferenceComponent.cs:5-20 |
| Space | SpatialSpaceId | World | 关系状态候选 | 必须使用已登记空间；SpatialSpaceId owner 未决 | unresolved | NLTX SpatialReferenceComponent.cs:5-20；SpatialSpaceId.cs:3-8 |

#### 字段不变量

- ParentEntity、ParentOffsetX、ParentOffsetY 和 IsParentRelative 是一个关系表达，不得分散成多个互相镜像的实体字段。
- ParentOffset 不得作为 Position 的替代物；Position 仍表达实体空间位置候选。
- Space 只标识所在空间，不等价于 WorldSectionId 或 TileCoordinate。
- EntityReference、SpatialSpaceId、WorldSectionId、WorldSectionCoordinates 和 TileCoordinate 涉及多个子系统，统一标记 crossSubsystemOwner: integration-review。

#### 生命周期

实体进入某一空间或建立父关系时创建或初始化；父关系、局部偏移或空间变化时更新；实体脱离空间或关系失效时清理。孤立实体的 ParentEntity 候选值为 null。

#### Entity/World 范围

单个实体的关系状态；Space 的实际注册和 World/Section 的状态不由此组件拥有。

#### ID 与关系字段

ParentEntity 使用当前 NLTX EntityReference 候选形式；该类型包含 Guid 与 Scope，不能被解释为 whoAmI、网络 ID 或持久化 ID。WorldSectionId 和 TileCoordinate 不补造为本组件字段。

#### 当前 NLTX 映射

src\Share\Entity\Components\SpatialReferenceComponent.cs:5-20 已存在，状态为 existing；目标覆盖为 partial。现有字段形状可作为候选，但共享类型 owner、父偏移清理语义和 Space 与 section 的关系仍未裁决。

#### 证据

- NLTX SpatialReferenceComponent 已有 ParentEntity、父偏移、IsParentRelative 和 SpatialSpaceId。
- NLTX Relationships\EntityReference.cs:5-10 证明 EntityReference 是 Guid+Scope 的关系值，不是网络或持久化身份。
- 当前没有证据允许本会话宣布共享空间/坐标类型的最终 owner。

componentId: SPATIAL.COMP.SPATIAL_REFERENCE
name: SpatialReference
status: proposed
componentOwner: SpatialSimulation（candidate）
crossSubsystemOwner: integration-review
entityScope: 单个有空间归属或父关系的实体
lifecycle: 建立空间关系时创建，关系变化时更新，关系解除时清理

### 5.4 MotionHistory

#### 职责

保存与当前运动状态对应的上一采样位置、速度、方向和采样 tick。它是历史快照，不是当前运动状态，也不保存渲染轨迹或持久化轨迹。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| PreviousPosition | System.Numerics.Vector2 | Zero | 快照 | 必须与 PreviousVelocity 来自同一采样；必须为有限值 | confirmed | Version4 Entity.oldPosition:14；Dome MotionHistoryComponent.cs:6-47 |
| PreviousVelocity | System.Numerics.Vector2 | Zero | 快照 | 必须与 PreviousPosition 来自同一采样；必须为有限值 | confirmed | Version4 Entity.oldVelocity:16；Dome MotionHistoryComponent.cs:6-47 |
| PreviousDirection | int | 0 候选；Version4 未见明确初始化 | 兼容快照候选 | 方向语义和初始化必须与实体域保持一致 | unresolved | Version4 Entity.oldDirection:18；NLTX 根/Dome MotionHistory 均缺失该字段 |
| RecordedAtTick | long? | null | 快照元数据 | 非空时必须大于等于 0；tick 语义必须覆盖 extra update 边界 | partial | Dome MotionHistoryComponent.cs:6-47；Version4 Projectile extra update 相关路径 |
| Kind | MotionHistoryKind | Tick | 快照元数据 | 必须区分普通 tick 与定义过的历史种类；枚举覆盖未锁定 | partial | NLTX src\Share\Entity\Components\MotionHistoryKind.cs:3-7；dome\src\...\MotionHistoryKind.cs:3-7 |

#### 字段不变量

- PreviousPosition 和 PreviousVelocity 必须作为同一次采样保存，不允许分别来自不同更新。
- RecordedAtTick 非空时不得使用负值。
- Projectile 的 oldPosition/oldVelocity 具有 extra update 语义；普通 tick 与 extra update 不能仅靠同一整数 tick 静默混淆。
- PreviousDirection 是 Version4 兼容候选；在 owner 和初始化语义未裁决前，不得宣称所有实体都需要它。

#### 生命周期

实体需要历史空间状态时创建；首次采样前可保持候选零值和 null tick；每次形成新历史采样时整体替换；实体移除或历史不再需要时清理。清理不得回写当前 Position 或 Velocity。

#### Entity/World 范围

单个实体范围。它不保存世界时间、网络回放或渲染历史。

#### ID 与关系字段

不保存身份或关系 ID。RecordedAtTick 是采样元数据，不是 WorldTime 的共享 owner 声明。

#### 当前 NLTX 映射

根 src\Share\Entity\Components\MotionHistoryComponent.cs:3-16 已存在，dome\src\Terraria.Dome.Simulation\Movement\Components\MotionHistoryComponent.cs:6-47 也已存在，状态均为 existing；目标覆盖为 partial。Dome 版本已有 Record/Clear、tick 和有限值校验，但根/Dome 都缺少 PreviousDirection，且两边生命周期语义未统一。

#### 证据

- Version4 Entity.cs:14、:16、:18 直接提供 oldPosition、oldVelocity、oldDirection。
- tModLoader class_entity.html:200-231 说明 oldPosition/oldVelocity 的公开语义，Projectile extra update 是特别边界。
- 当前实现缺少 oldDirection 的统一字段，构成 EG-COMP-03。

componentId: SPATIAL.COMP.MOTION_HISTORY
name: MotionHistory
status: proposed
componentOwner: SpatialSimulation（candidate）
crossSubsystemOwner: integration-review
entityScope: 单个具有历史采样的实体
lifecycle: 首次空间采样时创建，采样替换时更新，实体或历史能力移除时清理

### 5.5 CollisionPolicy

#### 职责

保存实体参与空间碰撞和液体接触时的策略输入。它只表达资格和开关，不拥有 Player、NPC、Projectile 或 Item 的专用行为。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| CollidesWithTiles | bool | 当前根构造函数必填；零值 false 不能当业务默认 | 权威策略输入 | 由实体域提供时必须有明确来源；不得与 Tile 事实混淆 | partial | NLTX src\Physics\CollisionPolicyComponent.cs:3-39；Dome Physics Components\CollisionPolicyComponent.cs:5-41 |
| CollidesWithEntities | bool | 当前根构造函数必填；零值 false 不能当业务默认 | 权威策略输入 | 只表达实体碰撞资格，不保存接触集合 | partial | NLTX 根/Dome CollisionPolicyComponent |
| IgnoresLiquids | bool | 当前根构造函数必填；零值 false 不能当业务默认 | 权威策略输入 | true 时液体接触结果的处理语义仍需由实体域裁决 | partial | NLTX 根/Dome CollisionPolicyComponent；Projectile.ignoreWater；Item.noWet |
| CanFallThroughPlatforms | bool | false 候选 | 权威策略输入 | 必须与 IsFallingThroughPlatforms 共同解释 | partial | NLTX 根/Dome CollisionPolicyComponent；Player 平台路径 |
| IsFallingThroughPlatforms | bool | false 候选 | 权威策略输入 | 仅在实体当前确实处于穿越状态时为 true | partial | NLTX 根/Dome CollisionPolicyComponent；Player 平台路径 |
| IgnoresDoors | bool | false 候选 | 权威策略输入 | 只表达门碰撞资格，不保存门状态 | partial | NLTX 根/Dome CollisionPolicyComponent |
| IgnoresAetheriumPlatforms | bool | false 候选 | 权威策略输入 | 只表达相应平台资格，不复制 Tile 属性 | partial | NLTX 根/Dome CollisionPolicyComponent |
| AllowsHoikTraversal | bool | true 候选 | 权威策略输入 | hoik 资格的实际来源和默认值需与实体域统一 | partial | NLTX 根/Dome CollisionPolicyComponent |
| SlopeMode | SlopeCollisionMode | Default | 权威策略输入 | 只能使用已定义模式；Projectile correctSlopeCollision 的映射未完全锁定 | partial | NLTX src\Physics\SlopeCollisionMode.cs:3-8；Projectile.correctSlopeCollision |

#### 字段不变量

- CanFallThroughPlatforms 与 IsFallingThroughPlatforms 必须共同解释；单独一个字段不能伪造平台穿越资格。
- tileCollide、ignoreWater、correctSlopeCollision 和 Item.noWet 是实体域策略来源候选，不是通用 MovementState 字段。
- Policy 不拥有专用移动行为、伤害效果、生命周期回调或 Tile 世界事实。
- 根 src 与 dome\src 的字段集合及默认值未完全一致，统一规则由 BD-COMP-05 裁决。

#### 生命周期

实体获得空间碰撞资格时创建；实体策略、装备或专用状态改变时更新；实体销毁或不再参与空间接触时清理。策略字段的来源可以来自实体域，但本组件不声明来源已经唯一确定。

#### Entity/World 范围

单个实体范围。平台、门、斜坡、Tile 和世界液体的事实不属于此组件。

#### ID 与关系字段

不保存 EntityReference、TileCoordinate 或平台/门 ID。策略只保留布尔值和斜坡模式候选。

#### 当前 NLTX 映射

根 src\Physics\CollisionPolicyComponent.cs:3-39 已存在，dome\src\Terraria.Dome.Simulation\Physics\Components\CollisionPolicyComponent.cs:5-41 也已存在，状态为 existing；目标覆盖为 partial。Dome 缺少根组件中的三项基础碰撞开关，且跨实体域的默认值和最终生产者未锁定。

#### 证据

- Version4 Projectile 的 tileCollide、ignoreWater、correctSlopeCollision 位于 Projectile.cs:194、:202、:250。
- Version4 Item.noWet 位于 Item.cs 约 :258。
- Version4 Player/NPC 的平台、斜坡和液体路径证明专用策略不能被一个无差别默认吞并。

componentId: SPATIAL.COMP.COLLISION_POLICY
name: CollisionPolicy
status: proposed
componentOwner: SpatialSimulation（candidate）
crossSubsystemOwner: integration-review
entityScope: 单个参与空间资格判断的实体
lifecycle: 空间资格创建时创建，策略变化时更新，实体或资格移除时清理

### 5.6 LiquidContact

#### 职责

保存实体从当前液体接触解析中得到的派生状态。它不拥有液体数量、液体类型写集、传播、合并、删除、buffer、网络或存档状态。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| DominantLiquidKind | LiquidKind?（canonical 类型未锁定） | null | 派生快照候选 | 只能来自明确的液体接触快照；根 LiquidKind 与 Dome LiquidType 不得直接等同 | unresolved | Version4 Tile liquid 属性；NLTX WorldInteraction\Tiles\LiquidKind.cs:3-9；Dome LiquidType.cs:3-9 |
| IsWet | bool | false | 派生快照 | 应与本次接触快照保持一致；不得成为世界液体事实 | confirmed | Version4 Entity.wet:26；Collision.WetCollision:1001-1084 |
| IsLavaWet | bool | false | 派生快照 | true 时必须有 lava 接触证据；不能仅由 DominantLiquidKind 猜测 | confirmed | Version4 Entity.lavaWet:34；Collision.LavaCollision:1085-1120 |
| IsHoneyWet | bool | false | 派生快照 | true 时必须有 honey 接触证据；静态 Collision.honey 不是此字段 owner | confirmed | Version4 Entity.honeyWet:30；Collision.WetCollision:1005-1055 |
| IsShimmerWet | bool | false | 派生快照 | true 时必须有 shimmer 接触证据；静态 Collision.shimmer 不是此字段 owner | confirmed | Version4 Entity.shimmerWet:28；Collision.WetCollision:1005-1055 |
| WetTickCount | byte | 0 | 派生快照/兼容 | 溢出、连续性和干燥清理规则未锁定 | partial | Version4 Entity.wetCount:32；Dome LiquidContactComponent.cs:5-59 |
| ResolvedAtTick | long? | null | 快照元数据 | 非空时必须对应 LiquidSimulation 提供的明确快照 tick | unresolved | 研究报告 liquid snapshot 边界；当前根/Dome 类型尚未统一 |

#### 字段不变量

- LiquidContact 只保存实体侧接触结果；Tile.liquid 的数量和类型仍属于 Tile/World/LiquidSimulation 范围。
- IsWet、IsLavaWet、IsHoneyWet、IsShimmerWet 允许同时表达接触事实；DominantLiquidKind 不能未经裁决取代全部布尔标志。
- 根 NLTX LiquidKind 含 Nano 哨兵，Dome 使用 LiquidType；两者不能直接视为同一 canonical 类型。
- InLiquid 如果保留，只能是旧兼容字段映射候选，不能与 DominantLiquidKind 同时作为两个独立权威字段。
- 清除和替换必须以一次接触快照为边界，不能依赖静态 Collision.honey 或 Collision.shimmer 的调用残留。

#### 生命周期

实体获得液体接触能力时可创建；每次接触解析后整体替换派生字段；没有接触时按明确清理语义恢复默认值；实体销毁或不再参与液体接触时清理。液体世界状态的创建、传播和提交不由本组件管理。

#### Entity/World 范围

单个实体范围。它不挂在 Tile、World、WorldSection 或 LiquidBuffer 上。

#### ID 与关系字段

不保存 TileCoordinate 集合、液体写集、网络 ID 或存档 ID。若记录 ResolvedAtTick，它只是跨边界快照引用，类型 owner 仍为 integration-review。

#### 当前 NLTX 映射

根 src\Share\Entity\Components\LiquidComponent.cs:3-35 已存在，状态为 existing；dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidContactComponent.cs:5-59 已存在，状态为 existing；目标覆盖为 partial。根 LiquidKind 与 Dome LiquidType 不统一，且根 InLiquid、Dome 连续湿计数和快照生命周期尚未形成一个 canonical 组件契约。

#### 证据

- Version4 Entity.cs:26-34 提供实体湿状态字段。
- Version4 Collision.cs:944-1120 提供液体接触方法及 honey/shimmer 隐式静态写入证据。
- Version4 Liquid.cs:14-69、:1015-1212 和 LiquidBuffer.cs 证明液体数量、传播、buffer 与提交不属于实体接触组件。

componentId: SPATIAL.COMP.LIQUID_CONTACT
name: LiquidContact
status: proposed
componentOwner: SpatialSimulation（candidate）
crossSubsystemOwner: integration-review
entityScope: 单个具有液体接触结果的实体
lifecycle: 液体接触能力创建时创建，按接触快照替换，无接触时清理，实体能力移除时销毁

### 5.7 CollisionResult

#### 职责

保存一次空间碰撞解析得到的实体侧结果快照，包括阻挡轴、阻挡法线、台阶结果和接触对象集合。它不保存当前运动输入、Tile 世界事实或永久实体关系。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| BlockingNormal | System.Numerics.Vector2 | Zero | 派生快照 | 必须为有限值；无阻挡时应为零向量或其他明确无阻挡表示，尚未最终锁定 | partial | Version4 TileContactSide/TileCollision；NLTX CollisionResultComponent.cs:8-34 |
| BlockedAxes | CollisionAxisMask | None | 派生快照 | 轴结果必须与本次解析一致；不得与 CollidedOnX/Y 双写成两个权威来源 | confirmed | NLTX src\Physics\CollisionAxisMask.cs:5-12；CollisionResultComponent.cs:8-34 |
| DidStepUp | bool | false | 派生快照 | 只表示本次结果是否发生台阶上移，不保存台阶 Tile 状态 | partial | Version4 Collision.StepUp:2802、StepDown:2736-2801；NLTX CollisionResultComponent |
| TouchedEntities | IReadOnlyList<EntityReference> | 空不可变集合 | 快照 | 读取不得改变集合；元素引用的 owner 和生命周期未决 | partial | NLTX CollisionResultComponent.cs:8-34；Relationships\EntityReference.cs:5-10 |
| TouchedTiles | IReadOnlyList<跨域 TileCoordinate> | 空不可变集合 | 快照 | 读取不得改变集合；必须防御性复制；TileCoordinate 类型和生命周期未决 | unresolved | NLTX CollisionResultComponent.cs:8-34；WorldStorage/WorldInteraction 两个 TileCoordinate |
| ResolvedAtTick | long? | null | 快照元数据 | 非空时必须对应本次碰撞解析的明确 tick；不能代替世界时间 owner | partial | NLTX CollisionResultComponent.cs:8-34；Dome ContactResultComponent.cs:9-74 |

#### 字段不变量

- BlockedAxes 是轴阻挡的 canonical 候选；CollidedOnX 和 CollidedOnY 只能作为派生或兼容字段。
- TouchedEntities 和 TouchedTiles 是本次结果快照，不是 EntityReference 关系组件，也不是 Tile 世界权威集合。
- 两个列表必须使用不可变视图或防御性复制，避免外部修改改变已形成的结果。
- TouchedTiles 使用哪一个 TileCoordinate、何时清理、是否跨 tick 保留尚未裁决。
- BlockingNormal、BlockedAxes、DidStepUp 和接触集合必须来自同一次碰撞解析，不能跨 tick 拼接。

#### 生命周期

实体获得碰撞结果能力时创建；每次碰撞解析后替换本 tick 结果；没有新结果时按明确语义清理或保留最后结果；实体销毁或结果能力移除时清理。结果是否只在当前 tick 有效由 BD-COMP-04 裁决。

#### Entity/World 范围

单个实体的一次结果快照。TouchedTiles 引用世界 Tile，但不改变 Tile/World 状态；WorldSection 和 TileCoordinate 的共享 owner 未决。

#### ID 与关系字段

TouchedEntities 使用 EntityReference 候选值；它不是 whoAmI、NetworkId、PersistentEntityId 或外部 ID。TouchedTiles 使用跨域 TileCoordinate 候选值；两个类型均标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

根 src\Physics\CollisionResultComponent.cs:8-34 已存在，状态为 existing；dome\src\Terraria.Dome.Simulation\Physics\Components\ContactResultComponent.cs:9-74 已存在，状态为 existing；目标覆盖为 partial。根组件使用 nullable List、实体/Tile 引用以及 CollidedOnX/Y 与 BlockedAxes 双重表示；Dome 使用内部 List 和只读视图；接触集合生命周期尚未统一。

#### 证据

- Version4 Collision.TileContact 和 TileCollision 提供接触方向、Tile 接触和轴结果来源。
- 当前 NLTX CollisionResultComponent 证明已有结果状态，但不能证明列表生命周期、坐标 owner 或行为等价。
- Version4 NPC.collideX/collideY 是实体专用兼容字段，不强行复制为通用 canonical 字段。

componentId: SPATIAL.COMP.COLLISION_RESULT
name: CollisionResult
status: proposed
componentOwner: SpatialSimulation（candidate）
crossSubsystemOwner: integration-review
entityScope: 单个实体的本次碰撞结果快照
lifecycle: 结果能力创建时创建，按解析结果替换，按裁决的 tick 边界清理或保留，实体移除时销毁

## 6. Entity 与 Component 组合

下表表达组件组合候选，不表达执行顺序或已经存在的运行时注册。必需表示该实体要参加一般空间计算时的最低候选组合；可选表示只有在该实体具有相应能力或需要相应派生状态时才挂载。

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| Player | MovementState、CollisionShape、CollisionPolicy | SpatialReference、MotionHistory、LiquidContact、CollisionResult | 无 | Player 具有持续位置/速度、实体形状和专用碰撞策略；历史、液体和结果按能力及状态存在 |
| NPC | MovementState、CollisionShape、CollisionPolicy | SpatialReference、MotionHistory、LiquidContact、CollisionResult | 无 | NPC 的 collideX/collideY 可由结果轴派生或作为实体域兼容状态，不创建第二套通用轴组件 |
| Projectile | MovementState、CollisionShape、CollisionPolicy、MotionHistory | SpatialReference、LiquidContact、CollisionResult | 无 | Projectile 具有 oldPosition/oldVelocity、extra update、Tile/液体/斜坡策略；这些字段分属历史和策略，不塞入运动状态 |
| Item/WorldItem | MovementState、CollisionShape | SpatialReference、MotionHistory、CollisionPolicy、LiquidContact、CollisionResult | 无 | Item 具有位置、速度、尺寸和 noWet；是否始终需要策略、历史及结果取决于实体生命周期 |
| 其他可移动实体 | MovementState、CollisionShape | SpatialReference、MotionHistory、CollisionPolicy、LiquidContact、CollisionResult | 无 | 只组合实体实际需要的能力，避免因为共用 Entity 基类而强制挂载所有状态 |
| Tile、WorldSection、World | 无 | 无 | 不挂载上述实体组件 | Tile 的 active、type、wall、liquid、slope、half-brick、actuator 和 section 版本是世界范围状态，不复制为实体组件 |
| EntityReference 关系 | 无独立关系组件 | 由 SpatialReference.ParentEntity 或 CollisionResult.TouchedEntities 承载 | 不把 TouchedEntities 当永久关系 | 父关系与单次接触集合生命周期不同，不能共享一个关系状态 |

补充组合约束：

- MovementState 与 CollisionShape 的职责相邻但生命周期和不变量不同，不能合并为一个无区分的 Physics 状态。
- MotionHistory 只有在需要上一采样或 Projectile extra update 语义时才挂载；它不应反向成为当前 Position/Velocity 的第二来源。
- LiquidContact 不得挂在 Tile、WorldSection 或液体传播状态上。
- CollisionResult 的接触列表不得被用作长期关系、持久化关系或身份索引。
- SpatialReference 的 ParentOffset 不得覆盖 MovementState.Position；两者关系必须保持显式。

## 7. 组件拆分与合并决策

### 7.1 保持在同一 Component 内的字段

MovementState 中的 Position、Velocity、Acceleration、GravityDirection、GravityScale、IsGrounded 和 IsMovementLocked 暂作为一个候选内聚边界。它们共同描述实体的持续运动状态和空间资格所需输入，拆开会制造当前位置、速度和重力之间的镜像同步问题。IsGrounded 与其余字段的权威分类不同，因此它保留为字段但明确标记为派生/兼容候选；这不是宣称其 owner 已确定。

CollisionResult 中的 BlockingNormal、BlockedAxes、DidStepUp、TouchedEntities、TouchedTiles 和 ResolvedAtTick 保持在同一结果快照内。它们必须描述同一次解析，拆开会产生跨 tick 拼接风险。

### 7.2 必须拆开的边界

| 拆分边界 | 保留的组件 | 拆分理由 |
|---|---|---|
| 当前运动与碰撞几何 | MovementState / CollisionShape | 位置速度会持续变化，形状变化频率和几何不变量不同 |
| 当前运动与历史采样 | MovementState / MotionHistory | 历史是快照，Projectile extra update 可能有特殊生命周期，不能成为当前值镜像 |
| 当前运动与父关系 | MovementState / SpatialReference | 父偏移、空间归属和实体位置不是同一坐标语义 |
| 当前运动与策略开关 | MovementState / CollisionPolicy | 实体专用策略来源和持续运动数值的 owner、更新原因不同 |
| 实体液体接触与世界液体 | LiquidContact / Tile、World、LiquidSimulation 状态 | 一个是实体派生快照，一个是世界权威数量/传播/写集 |
| 碰撞输入与碰撞结果 | MovementState、CollisionShape、CollisionPolicy / CollisionResult | 输入可跨结果复用，结果必须有单次解析生命周期 |
| 实体接触与永久关系 | CollisionResult / SpatialReference | TouchedEntities 是本次快照，ParentEntity 是关系状态 |

### 7.3 不合并为巨型组件

不创建名为 PhysicsState 的七合一组件来容纳所有字段。这样会把不同 owner、不同范围和不同清理条件的状态挤在一起，导致 Position/Velocity、LiquidContact、World Tile 事实和接触列表之间产生隐含同步。当前 NLTX 已有 PhysicsStateComponent 只能作为部分现状映射，不能直接视为本设计的完整目标。

### 7.4 兼容字段的处理原则

- CollidedOnX 和 CollidedOnY 不作为 CollisionResult 的两个 canonical 字段；它们可由 BlockedAxes 派生，或继续作为 NPC 域兼容字段。
- Entity.AnyWet、Center、边缘点、Hitbox 和 Size 不各自创建组件字段。
- Entity.whoAmI 不作为身份组件的稳定 ID；EntityReference 的 Guid+Scope 也不改名为网络或持久化 ID。
- 根 LiquidComponent 的 InLiquid 仅作为兼容映射候选，不与 DominantLiquidKind 建立两个独立权威写入。

## 8. 不单独创建 Component 的对象

| 对象 | 不单独创建的原因 | 归类 |
|---|---|---|
| Entity.Center、边缘点、Hitbox、Size | 都可从 MovementState.Position 与 CollisionShape 尺寸/偏移推导；单独保存会产生镜像同步 | 派生值 |
| Entity.AnyWet | 是多个液体接触字段的聚合判断，不是新的独立生命周期 | 派生值 |
| 单个碰撞轴、单个 AABB 判断 | 它们是一次几何事实或规则输入，不能代表具有独立生命周期的实体状态 | 实例值/规则结果 |
| TileContactSide、TileContact | 是一次解析中的方向和值对象；若需要保留，作为 CollisionResult 快照元素，不拆为组件 | 结果值 |
| Collision.stair、stairFall、honey、shimmer、sloping、up、down | Version4 静态共享 flags，生命周期由调用上下文决定，不能伪装成某个实体的权威状态 | 隐藏共享兼容状态 |
| Collision.contacts | 临时接触集合，边界和重入语义尚未闭合，不是永久实体关系 | 临时快照 |
| Collision._cacheForConveyorBelts | 传送带计算缓存，不是权威 Tile 或实体事实 | 缓存 |
| Collision.CanHit、CanHitWithCheck、CanHitLine、HitLine | 这些是空间资格规则和一次性结果，不具有独立组件生命周期 | 规则/派生结果 |
| Tile.type、wall、liquid、active、slope、half-brick、actuator | 属于 Tile、World 或 Section 范围；复制到实体会破坏世界状态 owner | 世界事实 |
| Liquid 数量、传播集合、buffer、网络变更集合 | 属于 LiquidSimulation 和世界液体状态，不是实体液体接触 | 世界事实/提交状态 |
| Entity.whoAmI | 是数组索引，生命周期和复用语义不满足稳定身份要求 | 兼容索引 |
| EntityIdentityComponent.UUID | 是现有实体身份状态，不是空间状态；不在本设计中重新定义 | 身份状态 |
| EntityReference、TileCoordinate、WorldSectionId | 是跨子系统共享候选类型，不能在本子系统内宣布新的独占组件 owner | 跨域关系/值类型 |
| LiquidRenderer、SceneMetrics、屏幕或表现缓存 | 表现、统计或缓存状态不参与实体权威空间组件内聚 | 表现/缓存 |

Version4 中 GetWaterLine、WalkDownSlope、TileCollision 和 StepDown 对空 Tile 执行 new Tile() 的风险也不转化为组件字段。它是世界读取/写入边界的 evidence-gap，必须保持为未决风险，而不是用一个“空 Tile Component”掩盖。

## 9. 当前 NLTX 组件覆盖

### 9.1 目标组件覆盖表

本节逐项区分当前类型状态和目标组件覆盖状态：当前 NLTX 类型使用 status: existing；目标组件覆盖使用 status: partial。

| 当前 NLTX 类型 | 当前状态 | 目标 Component | 覆盖判断 | 直接证据 |
|---|---|---|---|---|
| src\Share\Entity\Components\LocationComponent.cs | status: existing | MovementState.Position | status: partial | 文件:3-13；字段仍是两个 float，尚未统一为目标 Vector2 和 owner |
| src\Share\Entity\Components\VelocityComponent.cs | status: existing | MovementState.Velocity | status: partial | 文件:3-13；字段仍是两个 float，尚未与 Position 形成统一状态边界 |
| src\Physics\PhysicsStateComponent.cs | status: existing | MovementState.Acceleration、GravityDirection、GravityScale、IsGrounded、IsMovementLocked | status: partial | 文件:5-14；缺少目标位置/速度，默认和 owner 未闭合 |
| src\Share\Entity\Components\ColliderComponent.cs | status: existing | CollisionShape | status: partial | 文件:3-33；已有矩形和 float 几何，Version4 int 转换、IsEnabled 默认和形状范围未锁定 |
| src\Share\Entity\Components\SpatialReferenceComponent.cs | status: existing | SpatialReference | status: partial | 文件:5-20；字段形状接近目标，但共享 ID/坐标 owner 未锁定 |
| src\Share\Entity\Components\MotionHistoryComponent.cs | status: existing | MotionHistory | status: partial | 文件:3-16；有历史位置/速度/种类，但缺 PreviousDirection 和统一 tick 语义 |
| dome\src\Terraria.Dome.Simulation\Movement\Components\MotionHistoryComponent.cs | status: existing | MotionHistory | status: partial | 文件:6-47；有 Record/Clear、tick 和有限值校验，但与根组件未统一 |
| src\Physics\CollisionPolicyComponent.cs | status: existing | CollisionPolicy | status: partial | 文件:3-39；字段较全，但构造默认和 owner 未锁定 |
| dome\src\Terraria.Dome.Simulation\Physics\Components\CollisionPolicyComponent.cs | status: existing | CollisionPolicy | status: partial | 文件:5-41；缺少根组件的三项基础碰撞开关 |
| src\Share\Entity\Components\LiquidComponent.cs | status: existing | LiquidContact | status: partial | 文件:3-35；含根 LiquidKind 和 InLiquid，不能直接等同 canonical 接触设计 |
| dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidContactComponent.cs | status: existing | LiquidContact | status: partial | 文件:5-59；有 LiquidType?、连续湿计数和替换/清理形状，但类型边界未统一 |
| src\Physics\CollisionResultComponent.cs | status: existing | CollisionResult | status: partial | 文件:8-34；已有 nullable List、实体/Tile 引用、CollidedOnX/Y 与 BlockedAxes，生命周期和防御性复制未锁定 |
| dome\src\Terraria.Dome.Simulation\Physics\Components\ContactResultComponent.cs | status: existing | CollisionResult | status: partial | 文件:9-74；内部 List 和只读视图较接近目标，但名称、共享坐标和清理边界未统一 |

### 9.2 不应映射为实体组件的当前 NLTX 类型

| 当前 NLTX 类型 | 当前状态 | 组件设计结论 | 证据 |
|---|---|---|---|
| src\WorldInteraction\Tiles\TileCellComponent.cs | status: existing | 保持 Tile/World 范围，不并入 CollisionShape 或 LiquidContact | 文件:3-10 |
| src\WorldStorage\TileCellState.cs | status: existing | 保存 Tile 世界事实，不复制到实体组件 | 文件:3-22 |
| src\WorldStorage\WorldSectionState.cs | status: existing | 保存 section 范围状态，不并入 SpatialReference | 文件:3-22 |
| dome\src\Terraria.Dome.Simulation\World\WorldTile.cs | status: existing | 不可变 Tile 值属于世界范围，不并入实体组件 | 文件:3-27 |
| dome\src\Terraria.Dome.Simulation\World\WorldSectionSnapshot.cs | status: existing | section 快照属于世界范围，不并入 CollisionResult | 文件:5-45 |
| dome\src\Terraria.Dome.Simulation\World\WorldLiquidSnapshot.cs | status: existing | 世界液体快照不等于实体 LiquidContact | 文件:3-8 |
| dome\src\Terraria.Dome.Simulation\World\WorldGrid.cs | status: existing | WorldGrid 的 Tile/liquid 状态和版本不属于实体组件 | 文件:8-331 |
| dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidWorldStateComponent.cs | status: existing | 世界液体状态不并入 LiquidContact | 文件:5-90 |
| dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidDirtySectionComponent.cs | status: existing | 液体 dirty section 状态不并入实体组件 | 文件:6-25 |

### 9.3 类型重叠与覆盖结论

当前根 src 与 dome\src 存在重复或相近组件，不能把“两个目录都有同名或相近类型”解释为统一实现。特别是：

- LocationComponent、VelocityComponent、PhysicsStateComponent 的分离与目标 MovementState 的统一边界未裁决。
- 根与 Dome 的 MotionHistory、CollisionPolicy、LiquidContact 及 CollisionResult/ContactResult 字段集合不一致。
- 根 LiquidKind 含 Nano 哨兵，Dome LiquidType 具有另一套枚举语义。
- 当前存在两个不同命名空间的 TileCoordinate：src\WorldStorage\TileCoordinate.cs 与 src\WorldInteraction\Tiles\TileCoordinate.cs；其跨域 owner 未确定。

因此当前 NLTX 对七个目标组件的整体状态只能记为 partial；现有类型是覆盖证据，不是 proposed 组件已经创建的证明。

## 10. 组件级 evidence-gap

本节固定记录 9 个尚未闭合的组件级证据缺口。它们影响字段类型、默认值、owner、生命周期或兼容方式，因此 designStatus 保持 decision-required。

### EG-COMP-01：Position/Velocity 与 PhysicsState 的统一边界

- 冲突字段：LocationComponent、VelocityComponent、PhysicsStateComponent 与目标 MovementState 的字段边界。
- 当前证据：Version4 Entity.position/velocity 是共同实体字段；当前 NLTX 将位置、速度、加速度和重力拆开。
- 缺口：无法仅凭现有字段证明应统一为 Vector2，或必须保持 Location/Velocity 分离。
- 影响：MovementState 的字段集合、Position/Velocity 的唯一 owner 和实体组合会改变。
- 状态：unresolved。

### EG-COMP-02：Version4 int 尺寸、float 形状、偏移和 struct default

- 冲突字段：Entity.width/height、ColliderComponent 的 float 几何和 IsEnabled 默认。
- 当前证据：Version4 尺寸为实体 int 字段；NLTX ColliderComponent 使用 float 并存在构造默认与值类型零值差异。
- 缺口：转换、舍入、偏移以及默认未启用形状的语义未闭合。
- 影响：CollisionShape 字段类型、默认值和有效碰撞不变量会改变。
- 状态：unresolved。

### EG-COMP-03：oldDirection、Projectile extra update 与历史生命周期

- 冲突字段：Entity.oldDirection、根/Dome MotionHistory 和 Projectile oldPosition/oldVelocity。
- 当前证据：Version4 有 oldDirection；当前根/Dome MotionHistory 未统一拥有 PreviousDirection；公开文档确认 Projectile extra update 的历史语义特殊。
- 缺口：首次采样、普通 tick、extra update 和清理边界未锁定。
- 影响：MotionHistory 的字段集合、Kind、RecordedAtTick 和组合条件会改变。
- 状态：unresolved。

### EG-COMP-04：根 LiquidKind/Nano、Dome LiquidType 与 dominant 规则

- 冲突字段：根 LiquidComponent、Dome LiquidContactComponent、Version4 wet/lavaWet/honeyWet/shimmerWet。
- 当前证据：根和 Dome 使用不同枚举；Version4 可同时表达多个液体布尔接触状态。
- 缺口：canonical 类型、dominant 选择、InLiquid 兼容映射和快照 tick 未锁定。
- 影响：LiquidContact 的类型、字段数量、清理和跨边界 owner 会改变。
- 状态：unresolved；crossSubsystemOwner: integration-review。

### EG-COMP-05：CollisionResult 接触集合生命周期

- 冲突字段：根 CollisionResultComponent 的 nullable List、Dome ContactResultComponent 的内部 List/只读视图、Version4 contacts/TileContact。
- 当前证据：当前组件都保存接触结果候选，但没有统一证明结果只活一个 tick，或应保留到下一次解析。
- 缺口：列表复制、空值语义、TouchedTiles 坐标类型和清理点未锁定。
- 影响：CollisionResult 的字段类型、快照生命周期和 Entity 组合会改变。
- 状态：unresolved；crossSubsystemOwner: integration-review。

### EG-COMP-06：ParentEntity、SpatialSpaceId、WorldSectionId/Coordinates 的关系

- 冲突字段：SpatialReference 的父关系/Space、WorldSection 状态、两个 TileCoordinate 和 EntityReference。
- 当前证据：NLTX 已有这些类型或相邻类型，但跨子系统使用范围和 owner 不统一。
- 缺口：空间、section、Tile 坐标之间是否存在稳定映射无法确认。
- 影响：SpatialReference 字段、CollisionResult.TouchedTiles 类型和 ID/关系分类会改变。
- 状态：unresolved；crossSubsystemOwner: integration-review。

### EG-COMP-07：根/Dome CollisionPolicy 默认值和字段差异

- 冲突字段：根与 Dome CollisionPolicy 的三项基础碰撞开关、平台和斜坡字段。
- 当前证据：根构造函数要求部分参数；Dome 字段集合不完整；Version4 实体专用策略分散在 Player/NPC/Projectile/Item。
- 缺口：默认值、字段兼容映射和最终生产者未锁定。
- 影响：CollisionPolicy 字段集合、实体组合和策略 owner 会改变。
- 状态：unresolved；crossSubsystemOwner: integration-review。

### EG-COMP-08：空 Tile、actuator、half-brick、slope 的只读表达

- 冲突字段：Collision 计算中的空 Tile 自动创建与 Tile/World 的几何事实。
- 当前证据：GetWaterLine、WalkDownSlope、TileCollision、StepDown 存在 new Tile() 风险；Tile 的 active、slope、half-brick、actuator 和 liquid 属于世界状态。
- 缺口：空 Tile 的缺省读取语义和只读表达尚未闭合。
- 影响：CollisionShape、CollisionResult 的证据边界和世界事实隔离规则会改变，但不应新增空 Tile 实体组件。
- 状态：unresolved。

### EG-COMP-09：根/Dome 重复组件的 namespace、类型和依赖边界

- 冲突字段：根 src 与 dome\src 的 MotionHistory、CollisionPolicy、LiquidContact、CollisionResult/ContactResult，以及两个 TileCoordinate。
- 当前证据：两套类型均已存在，字段和集合封装不完全相同。
- 缺口：canonical namespace、依赖方向和共享值类型 owner 未锁定。
- 影响：七个目标组件的现状映射、跨域字段类型和最终组件数量可能变化。
- 状态：unresolved；crossSubsystemOwner: integration-review。

组件级 evidence-gap 数量：9。

## 11. 未决组件 owner

以下 5 项会改变组件字段、组合或生命周期，当前会话只保留候选方案，不宣布最终 owner。

### BD-COMP-01：Position/Velocity/PhysicsState owner

- 冲突字段：MovementState.Position、MovementState.Velocity、Acceleration、GravityDirection、GravityScale、IsGrounded、IsMovementLocked 与现有 LocationComponent、VelocityComponent、PhysicsStateComponent。
- 候选 owner A：SpatialSimulation 的 MovementState 统一拥有位置、速度和运动参数。影响是目标组件内聚，但需要处理现有根 src 分离字段和其他实体域读者。
- 候选 owner B：共享 Entity 能力继续分别拥有 Location/Velocity，MovementState 只收纳加速度与重力。影响是保持现状映射，但需要长期维护 Position/Velocity 的组合不变量。
- 当前不能裁决的原因：Version4 共同字段结构与当前 NLTX 分离结构不相同，且相关实体域都可能读写持续运动状态。
- 状态：decision-required；crossSubsystemOwner: integration-review。

### BD-COMP-02：EntityReference、SpatialSpaceId、WorldSectionId/Coordinates、TileCoordinate owner

- 冲突字段：SpatialReference.ParentEntity、SpatialReference.Space、CollisionResult.TouchedTiles 及两个不同命名空间的 TileCoordinate。
- 候选 owner A：由一个共享空间/关系边界统一定义。影响是跨子系统类型稳定，但会扩大整合范围。
- 候选 owner B：由 SpatialSimulation 保留候选值类型，其他子系统通过映射使用。影响是本子系统短期清晰，但会产生跨域映射和重复值风险。
- 当前不能裁决的原因：这些类型至少被实体关系、世界 section、Tile 和碰撞结果共同使用，单一子系统没有足够 owner 证据。
- 状态：decision-required；crossSubsystemOwner: integration-review。

### BD-COMP-03：LiquidContact 的生成边界与字段 owner

- 冲突字段：LiquidContact.DominantLiquidKind、IsWet、各液体标志、WetTickCount、ResolvedAtTick，以及根 LiquidComponent、Dome LiquidContactComponent 和 LiquidSimulation 快照。
- 候选 owner A：SpatialSimulation 维护实体侧 LiquidContact，消费 LiquidSimulation 提供的只读接触快照。影响是实体派生字段集中，但要求液体类型和快照 tick 有稳定共享契约。
- 候选 owner B：LiquidSimulation 直接拥有并更新实体接触结果。影响是液体语义集中，但会把实体空间结果的生命周期扩展到液体子系统。
- 当前不能裁决的原因：Version4 的 WetCollision 同时读取 Tile 液体并写实体湿字段，还写入 Collision 静态 flags；NLTX 根/Dome 的字段和枚举未统一。
- 状态：decision-required；crossSubsystemOwner: integration-review。

### BD-COMP-04：CollisionResult 接触集合和生命周期 owner

- 冲突字段：TouchedEntities、TouchedTiles、BlockedAxes、CollidedOnX/Y、DidStepUp 与根/Dome 结果组件的保留、复制和清理语义。
- 候选 owner A：SpatialSimulation 维护严格的本次结果快照，解析后替换并在明确 tick 边界清理。影响是结果生命周期清晰，但所有消费者必须接受短生命周期。
- 候选 owner B：实体域保留部分兼容结果，SpatialSimulation 只提供轴和法线候选。影响是兼容成本较低，但可能形成两套结果来源。
- 当前不能裁决的原因：Version4 contacts 是静态临时集合，NLTX 根与 Dome 的容器语义不一致，且 TileCoordinate owner 未定。
- 状态：decision-required；crossSubsystemOwner: integration-review。

### BD-COMP-05：CollisionPolicy 的最终生产者

- 冲突字段：CollidesWithTiles、CollidesWithEntities、IgnoresLiquids、平台字段、IgnoresDoors、IgnoresAetheriumPlatforms、AllowsHoikTraversal、SlopeMode。
- 候选 owner A：PlayerGameplay、NpcAndTownSimulation、ProjectileSimulation、Item 域分别形成各自实体策略，SpatialSimulation 只消费统一字段。影响是专用语义保留，但需要跨域字段契约。
- 候选 owner B：SpatialSimulation 提供统一默认策略，实体域只覆盖差异字段。影响是组件组合简单，但可能吞并专用行为来源。
- 当前不能裁决的原因：Version4 的策略分散且根/Dome 组件默认值不一致；统一默认会改变行为边界，不能由本会话猜测。
- 状态：decision-required；crossSubsystemOwner: integration-review。

组件级 owner 决策数量：5。

## 12. 最终 Component 清单

以下是本文件的规范化候选清单，共 7 个；每一项仍是 proposed，不是当前 NLTX 已创建类型的声明。

| componentId | name | status | entityScope | componentOwner | crossSubsystemOwner | 核心状态 |
|---|---|---|---|---|---|---|
| SPATIAL.COMP.MOVEMENT_STATE | MovementState | proposed | 单个可移动实体 | SpatialSimulation（candidate） | integration-review | Position、Velocity、Acceleration、重力、接地/锁定候选 |
| SPATIAL.COMP.COLLISION_SHAPE | CollisionShape | proposed | 单个具有空间几何的实体 | SpatialSimulation（candidate） | integration-review | Width、Height、OffsetX、OffsetY、Kind、IsEnabled |
| SPATIAL.COMP.SPATIAL_REFERENCE | SpatialReference | proposed | 单个有空间归属或父关系的实体 | SpatialSimulation（candidate） | integration-review | ParentEntity、父偏移、IsParentRelative、Space |
| SPATIAL.COMP.MOTION_HISTORY | MotionHistory | proposed | 单个具有历史采样的实体 | SpatialSimulation（candidate） | integration-review | PreviousPosition、PreviousVelocity、PreviousDirection、RecordedAtTick、Kind |
| SPATIAL.COMP.COLLISION_POLICY | CollisionPolicy | proposed | 单个参与空间资格判断的实体 | SpatialSimulation（candidate） | integration-review | Tile/Entity/Liquid/Platform/Door/Slope 策略输入 |
| SPATIAL.COMP.LIQUID_CONTACT | LiquidContact | proposed | 单个具有液体接触结果的实体 | SpatialSimulation（candidate） | integration-review | 液体接触标志、dominant 候选、连续湿计数、快照 tick |
| SPATIAL.COMP.COLLISION_RESULT | CollisionResult | proposed | 单个实体的本次碰撞结果快照 | SpatialSimulation（candidate） | integration-review | BlockingNormal、BlockedAxes、DidStepUp、接触快照、解析 tick |

清单之外不新增一个合并型 Physics 组件、不新增空 Tile 组件、不新增单轴组件、不新增永久接触关系组件，也不把 Tile/World 液体或结构状态复制到实体组件。

## 13. 最终声明

本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。

当前设计状态为 decision-required，evidenceStatus 为 partial，nltxStatus 为 partial，verificationStatus 为 not-run。组件级 evidence-gap 数量为 9，组件级 owner 决策数量为 5。
