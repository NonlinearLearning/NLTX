# Projectile ECS 系统设计

- 日期：2026-10-24
- 适用项目：src/NSSLC/Terraria.Components.csproj
- 设计状态：implemented-foundation
- NSSLC 运行时接入：partial（组件、纯模拟、生命周期、碰撞候选、Combat 命中提交和穿透/免疫已实现；宿主 Tick 装配仍待接入）
- MinimalImplementation 参考状态：partial
- 行为等价：unknown

这份文档设计一个独立的本地 Projectile 领域系统。它参考
D:/TRbackup/NLTX/src/MinimalImplementation/Combat/Projectiles 中已经存在的身份、版本和
错误边界，也参考 D:/TRbackup/最新的客户端/Terraria/Projectile.cs 中的行为事实，但不把
任一代码库的类结构、全局数组或聚合对象作为 NSSLC 的架构模板。网络协议只作为客户端行为
参考，不属于本设计范围。

## 1. 目标和边界

### 1.1 目标

Projectile 系统为一次权威世界 Tick 提供可检查的闭环：

受信任 Spawn 请求
→ 定义解析和身份分配
→ Projectile ECS 实体
→ 行为、运动和子步模拟
→ Tile 与实体碰撞候选
→ Combat 命中准入
→ 穿透、免疫和寿命提交
→ 终止或继续活动
→ 本地观察到已提交结果

必须明确：

- Projectile 根身份和运行句柄的区别；
- 每种组件的最终写入者；
- 运动、额外更新、碰撞、命中、穿透和寿命的顺序；
- Owner、Target、World 的作用域检查；
- 失败、过期引用、重复请求和容量耗尽的语义；
- 本地世界卸载和持久化边界。

### 1.2 第一阶段垂直切片

第一阶段只要求一条窄而完整的权威路径：

一个有效 Player
→ 生成普通 Projectile
→ 在 Tile 空间移动
→ 命中一个有效 NPC
→ Combat 接受一次伤害
→ Projectile 记录 accepted hit，并按 Attributes 计算的上限判断终止
→ 达到上限或寿命到期时提交终止请求
→ 本地状态查询确认伤害与终止结果

第一阶段不承诺完整客户端内容、所有 AI 样式、所有液体效果、反射、复杂附属弹体、
掉落生成、客户端预测、网络同步或旧协议兼容。

### 1.3 非目标

Projectile 核心系统不拥有：

- Player、NPC、敌对生命和伤害减免；
- Tile 数组、WorldFile、网络协议或网络连接；
- 客户端绘制、动画、音频、Trail、粒子或 UI；
- 通用实体继承树；
- 保存所有字段并自行执行所有行为的长期聚合 Projectile 对象；
- 每个字段、读取方法或旧方法的机械拆分。

普通临时 Projectile 默认只存在于运行时。允许某类 Projectile 跨存档时，必须另行批准
持久化契约。

## 2. 当前证据和现状

### 2.1 NSSLC 已有可复用合同

| 已有能力 | 作用 | Projectile 使用方式 |
| --- | --- | --- |
| EntityIdentityComponent / EntityUuid | 权威实例根身份 | 绑定一次 Projectile 实例 |
| EntityRuntimeHandle | world scope + generation 运行时引用 | Owner、索引和提交结果使用 |
| LocationComponent | 位置状态 | 由空间提交端口写入 |
| VelocityComponent | 线速度状态 | 与位置一起由空间提交端口写入 |
| ColliderComponent | 几何和碰撞能力 | 由定义实例化 |
| LiquidContactComponent | 液体接触事实 | 作为空间结果读取 |
| SpatialFrame / SpatialEntitySnapshot | 脱离 ECS 的空间输入 | 提供 world revision 和只读查询 |
| SpatialSimulationSystem | 通用空间计算 | 复用位置、速度、Tile 移动和接触 |
| SpatialSimulationCoordinatorSystem | Arch 组合查询并完成空间写入 | 作为位置、速度、接触的常规提交路径 |
| EcsServerPhase | Preparation/Simulation/Commit/Projection | 使用现有阶段；Projection 不用于网络复制 |
| EcsServerCommandBuffer | 结构变化缓冲 | 用于安全边界上的创建、终止和删除 |

这些合同的 README 都明确说明：共享层存在不等于 Projectile 已经被真实服务器注册或运行。

### 2.2 MinimalImplementation 证据范围

当前最小实现存在：

- Projectile 聚合运行时对象；
- ProjectileManager 作为主要 owner；
- ProjectileAiService、ProjectileMovementService、ProjectileCollisionService、
  ProjectileNpcCombatService；
- ProjectileServerTickPhaseOwner；
- 以普通木箭 type 1 为主的定义；
- owner、协议 identity、revision、commit version 和 tracker 清理。协议身份及 tracker 仅作为
  参考实现事实，不纳入本地 Projectile 设计。

默认 Tick 主要是：

Capture → ProjectileAiService.Tick → 到期时 ProjectileManager.RetireExpired

默认 owner 没有自动串起完整的移动、Tile 碰撞、NPC 命中和 TagEffect 链。因此它是行为边界
和缺口证据，不是 NSSLC 最终结构。

### 2.3 最新客户端只能作为行为参考

最新客户端的 Projectile.cs 提供 type、owner、position、velocity、AI、timeLeft、penetrate、
wet、固定数组槽位和 Kill 等行为事实。协议字段和 Packet 27/29/142 不进入本次目标范围。
客户端也把全局数组、端别状态、内容定义、网络、渲染和大量行为放在大型类型中。

NSSLC 不复制：

- Main 式静态全局状态；
- 巨型 Projectile 聚合对象；
- 继承式实体分类；
- 网络协议和同步状态混入 Projectile 领域；
- 静态数组免疫表作为权威状态；
- AI、碰撞、伤害、表现和协议混合在同一方法链中。

## 3. 领域模型

### 3.1 身份模型

一个 Projectile 实例有多个标识空间：

EntityUuid
→ EntityRuntimeHandle(world scope + generation)
  → Arch entity（仅运行时存储句柄）
  → Owner EntityRuntimeHandle

| 标识 | 权威含义 | 不能替代 |
| --- | --- | --- |
| Definition ID | 内容规则和初始组合 | 实例身份 |
| EntityUuid | 一次实例的根身份 | Arch 句柄 |
| Arch Entity | 当前 ECS 存储句柄 | 跨 Tick、网络或存档引用 |
| EntityRuntimeHandle | 当前 world scope/generation 引用 | 根身份 |
| Runtime slot | 本地查找位置 | 代际校验 |
| Persistence ID | 可选跨加载业务身份 | runtime handle |

槽位复用时必须检查 world scope、generation、EntityUuid 和当前状态。旧引用命中新实例属于
错误。

### 3.2 Projectile 是组件组合

Projectile 不建立 ProjectileEntity 继承类。建议的基础组合如下：

| 组件或记录 | 保存内容 | 写入者 |
| --- | --- | --- |
| EntityIdentityComponent | EntityUuid | 生命周期系统 |
| Projectile definition id (spawn/lifecycle input) | 定义 ID、版本 | 生命周期/定义解析；不挂载通用引用组件 |
| ProjectileOwnerComponent | owner handle、归属策略 | 生命周期系统 |
| ProjectileSourceMetadataComponent | 武器/物品/来源实体 | Spawn 准入 |
| LocationComponent | 世界位置 | Spatial commit port |
| VelocityComponent | 当前线速度 | Spatial commit port |
| ColliderComponent | 形状、尺寸、偏移 | 定义/能力 |
| `AiComponent` | 通用 AI 行为绑定声明 | AI 系统/生命周期 |
| AiBehaviorSelectionComponent | `BehaviorId`、`ProfileId` 两个稳定选择键 | 定义解析；AI 只读 |
| ProjectileBehaviorStateComponent | 共享 AI 可观察的阶段、轨迹决策和行为状态 | Projectile AI 适配器 |
| ProjectileKinematicsStateComponent | extra updates、子步和轨迹策略 | Projectile simulation |
| ProjectileLifetimeStateComponent | 剩余寿命、单位和到期策略 | Projectile lifecycle |
| ProjectilePenetrationStateComponent | accepted hit 计数 | Projectile hit resolution；上限由 Attributes 计算 |
| ProjectileCollisionPolicyComponent | Tile/entity 规则和 owner 排除 | Collision system |
| DamagePayloadComponent | 共享伤害载荷（伤害类型、关系、击退和属性穿透） | Combat admission input |
| ProjectileHitImmunityStateComponent | 多目标 handle/generation 短期免疫 list | Hit resolution |
| ProjectileDispositionStateComponent | Active/PendingTerminate/Terminated | Lifecycle system |

可选能力使用组件组合表达，而不是在 `AiBehaviorSelectionComponent` 中保存一组布尔字段：

| 可选能力 | 能力组件 | 运行时状态组件 | 语义 |
| --- | --- | --- | --- |
| 追踪 | `ProjectileHomingCapabilityComponent` | 当前 Tick 的 `ProjectileTargetFact` | 实体存在能力组件时才参与追踪计算 |
| 反弹 | `ProjectileBounceCapabilityComponent` | `ProjectileBounceStateComponent` | 实体存在能力组件时才允许反弹 |
| 重力 | 共享 `GravityCapabilityComponent` | 共享 `GravityStateComponent` | 实体存在重力能力时由重力系统推进 |

能力组件的存在表示“实体拥有这项能力”，状态组件保存跨 Tick 的可变进度。不要新增
`List<ProjectileCapability>`、`UsesHoming`、`CanBounce` 或 `UsesGravity` 作为第二套能力来源。
Arch 查询直接使用精确的组件组合，例如追踪查询要求
`ProjectileHomingCapabilityComponent` 和当前 Tick 的 `ProjectileTargetFact`，没有这些组件的
Projectile 不会被该查询处理。能力组件可以与 `BehaviorId` 组合，但不能绕过 AI、空间或
生命周期的权威提交边界。

`AiBehaviorSelectionComponent` 只保存：

```text
BehaviorId  = 选择具体 AI 行为实现的稳定内容键
ProfileId   = 选择不可变行为参数配置的稳定内容键
```

它不保存“是否追踪/反弹/受重力”等布尔字段，也不保存 Planner、World、Arch Entity 或
运行时目标引用。`ProjectileBehaviorStateComponent` 只保留跨行为共享的观察状态；追踪和反弹
彼此独立的动态状态分别放入对应的 `...StateComponent`，避免形成无法维护的巨型状态容器。

行为选择、参数和输入需求分层如下：

```text
BehaviorId
    → 选择 AiBehavior<TIntent> 的实现
ProfileId
    → 选择不可变 ProjectileBehaviorProfile
AiRequirements
    → 声明该行为需要的组件快照和只读事实
BehaviorState / 能力 State
    → 保存执行期间的动态状态
```

例如 `BehaviorId = HomingProjectile`、`ProfileId = FireballHomingProfile` 时，生命周期
解析器必须同时确认行为目录、Profile 和 `AiSystem<ProjectileAiIntent>` 注册项存在，并确认
实体组合包含追踪能力及其状态组件。缺少需求输入时，AI 返回 `MissingComponents`，不能通过
默认值偷偷补齐。

Kinematics 不复制 Velocity；DamageComponent 只提供伤害输入，不拥有目标 Life；HitImmunity
不使用全局静态数组；definition、profile、规则目录和系统实例是共享的不可变外部定义。只有
实际需要某能力的 Projectile 才附加相应组件。

已有 LiquidContactComponent 可以表示接触事实。只有需要跨 Tick 保存湿润、蜂蜜或微光状态
时，才增加明确的 Projectile 状态组件。

### 3.3 Definition 和实例化

ProjectileDefinition 是不可变内容定义，至少包含：

- ID 和版本；
- 初始组件组合及默认值；
- `BehaviorId` 和默认 `ProfileId`；
- collider 和 Tile policy；
- 基础伤害、击退和阵营；
- lifetime、extra updates 和其他运动默认值；
- owner 忽略规则与接触策略；穿透上限、命中免疫 Tick 和终止阈值由 Attributes 与 Projectile
  运行态共同计算，不作为共享的剩余穿透值保存；
- 是否允许本地创建、反射或持久化；
- 所需随机/时间输入和上限。

定义不保存 Arch entity、EntityUuid、runtime slot、owner 可写对象、网络连接或文件路径。
每次创建都必须生成独立可变组件。

### 3.4 行为目录、Profile 和 AI 绑定

Projectile 行为使用不可变目录解析，不在各个系统中重复 `switch (BehaviorId)`：

```text
ProjectileBehaviorCatalog
├─ StraightProjectile
├─ HomingProjectile
│  ├─ FireballHomingProfile
│  └─ IceHomingProfile
└─ BouncingProjectile
```

实体只保存 `BehaviorId` 和 `ProfileId`。Profile 可以保存最大转向速度、追踪范围、加速度、
反弹角度、最大反弹次数、穿透消费规则等不可变参数，但不能保存 Arch Entity、EntityUuid、
Owner 可写引用、Planner、World 或其他运行时状态。

`AiComponent.BehaviorType` 与 `AiBehaviorSelectionComponent.BehaviorId` 不是两个竞争的
行为来源：

| 标识 | 作用域 | 示例 |
| --- | --- | --- |
| `AiComponent.BehaviorType` | C# AI 行为实现的绑定类型 | `ProjectileHomingAiBehavior` |
| `BehaviorId` | 内容领域稳定行为键 | `HomingProjectile` |
| `ProfileId` | 不可变参数配置键 | `FireballHomingProfile` |

Lifecycle/Definition Resolver 在创建或重新绑定时验证 `BehaviorId → Profile`、
`BehaviorId → AiBehavior<TIntent>` 注册关系，以及 `AiComponent.BehaviorType` 与行为键的
兼容性。解析成功后由 `AiSystem<ProjectileAiIntent>` 绑定；运行时系统不自行分派行为键。

行为实现的需求应由行为自身声明，Profile 只提供参数，不隐含输入读取：

| 行为 | `AiRequirements` / 快照需求 | 额外只读事实 |
| --- | --- | --- |
| StraightProjectile | 位置、速度、`ProjectileBehaviorStateComponent` | Tick 和时间 |
| HomingProjectile | 位置、速度、`ProjectileHomingCapabilityComponent`、`ProjectileTargetFact` | Owner、目标候选、目标位置/有效性 |
| BouncingProjectile | 位置、速度、`ProjectileBounceCapabilityComponent`、`ProjectileBounceStateComponent` | Tile/entity 碰撞结果、接触法线 |

这些需求由 `AiBinding.RequiredComponents` 暴露，快照适配器在每次计算前逐项提供；Owner、
目标和碰撞事实也必须经过同一个 world scope/generation/revision 边界校验。行为不能从
Profile、全局单例或 Arch 引用中偷偷读取未声明输入。

Projectile、NPC 和 Player 使用同一行为模型。Projectile 的 `ProjectileBehaviorStateComponent`
与 NPC 的 `NpcBehaviorStateComponent` 都只是可观察的领域运行态；它们不是 Planner、GOAP
Agent 或 HTN Context 的容器。NPC 使用 `AiComponent + AiBehaviorSelectionComponent +
NpcBehaviorStateComponent`，Player 使用自己的控制/行为声明，三者都通过共享
`AiSystem<TIntent>` 计算，再由各自适配器提交领域意图。这样“Projectile 行为属于共享 AI 系统
的一种实现”同时适用于 NPC 和 Player，而不会产生三套互不兼容的 AI 引擎。

NPC 行为适配器可以提交 `NpcBehaviorStateComponent`、目标关系和 `MovementIntent`/
`CombatIntent`；它不能直接写位置、速度、生命、属性或 Buff。NPC 的属性/Buff 事实通过只读
快照投影提供，接收能力、免疫、叠层和周期效果仍由共享 Buff/Attribute/Combat 系统处理。

## 4. System 边界与所有权

以下是概念职责，不要求第一阶段为每项都公开创建类型；即使内部由一个实现承载，也必须
保持读写集和提交顺序可识别。

### 4.1 ProjectileLifecycleSystem

唯一负责 Projectile 实体生命周期：

- 接受、验证和排序 Spawn；
- 分配 EntityUuid、runtime handle 和 generation；
- 解析 definition，实例化组件；
- 建立 owner 关系和必要的生命周期索引；
- 进入 Active；
- 标记 PendingTerminate；
- 撤销 owner/lifecycle 索引；
- 释放槽位和删除 Arch entity；
- owner entity removal、world unload、definition rejection 清理；
- 创建失败反向清理。

它不负责 AI、位置移动、目标生命、网络 socket 或 WorldFile。

销毁顺序固定为：

校验 handle
→ 标记 PendingTerminate，阻止新的模拟
→ 撤销 owner/lifecycle 索引
→ 删除或回收组件和 Arch entity
→ 使旧 generation 失效

创建失败必须清理 Arch entity、UUID、组件、关系及已建立的生命周期索引。

### 4.2 Definition Resolver

Definition resolver 负责查找 ID、检查版本、映射初始组件和拒绝未知/禁用定义。
它不保存运行时实例，也不修改实体状态。规模较小时可以是生命周期系统内的纯读取部分。

### 4.3 Projectile AI 与 ProjectileSimulationSystem

Projectile 行为属于 AI 系统的一种具体行为实现，不建立独立的 `ProjectileBehaviorSystem`。
共享 `AiSystem<TIntent>` 提供绑定、需求检查、计算、停止和释放；Projectile 领域提供
`ProjectileAiIntent`、Projectile 快照适配器以及实现 `AiBehavior<TIntent>` 的行为类型。

AI 系统读取：

- `AiBehaviorSelectionComponent`；
- `ProjectileBehaviorStateComponent`；
- 与行为匹配的能力组件和能力状态组件；
- 位置、速度、Owner、目标、液体和其他只读事实。

AI 系统输出：

- 方向/速度调整意图；
- 追踪、反弹或行为阶段变更意图；
- 行为产生的命中/终止请求；
- 需要反馈的行为请求。

AI 系统只返回行为状态变更和 Projectile AI intent；Projectile AI 适配器在明确的提交边界
写入 `ProjectileBehaviorStateComponent`。AI 计算本身不能直接写 `LocationComponent`、
`VelocityComponent`、目标 `LifeComponent` 或删除实体。

`ProjectileSimulationSystem` 不执行 AI 决策。它消费已经通过验证的 Projectile AI intent，
负责把意图与空间事实组合成确定性的运动和生命周期结果：

- 速度、方向、轨迹和 extra updates；
- lifetime 递减；
- 生成运动结果、碰撞请求、effect intent 和 termination intent。

输入是 ProjectileSnapshot、已验证的 `ProjectileAiIntent`、Definition、SpatialFrame、
OwnerFacts、WorldFacts、TickContext，以及定义明确需要的 RandomFacts/TimeFacts。输出是
带 source handle/revision 的 ProjectileSimulationResult，包括 motion、collision request、
lifetime delta、effects 和 termination intent。

它不直接写 Arch、AI 行为状态、NPC、Player、World 或日志，也不持有跨阶段可写 ref。

### 4.3.1 能力查询与系统边界

能力组件决定哪些实体进入查询；`BehaviorId` 决定 AI 如何计算，`ProfileId` 提供不可变参数，
`AiRequirements` 决定适配器必须捕获哪些输入。三者不能互相替代：拥有追踪组件不代表一定
选择追踪行为，选择追踪行为也不能跳过能力组件和输入校验。

| 能力/边界 | 查询输入 | 输出或写入 |
| --- | --- | --- |
| Homing AI | `ProjectileHomingCapabilityComponent`、行为 Profile、位置、速度、Owner 和当前 Tick 的目标事实 | `ProjectileAiIntent` |
| Bounce 解释 | `ProjectileBounceCapabilityComponent`、`ProjectileBounceStateComponent`、Tile/entity collision result、Profile | `BounceIntent` 或速度调整意图；适配器提交 Bounce state |
| GravityTickSystem | 共享 `GravityCapabilityComponent`、Gravity 配置、修饰器和当前速度 | 共享 `GravityStateComponent` |
| ProjectileSimulationSystem | 已验证的 AI/Bounce intent、`GravityStateComponent`、Kinematics、Lifetime、SpatialFrame | motion、collision request、lifetime delta、termination intent |
| SpatialSimulationCoordinatorSystem | Location、Velocity、GravityState、AI/Bounce motion、Tile/entity facts | 唯一常规写入 `LocationComponent`、`VelocityComponent`、`GroundingComponent` 和已有 `LiquidContactComponent` |
| Lifetime/Penetration/Immunity | Lifetime、AcceptedHitReceipt、Penetration、HitImmunity state | 对应 Projectile 状态；不写目标 Life |

Homing 行为可以更新自己的 Homing state，Bounce 解释可以更新自己的 Bounce state，但两者
都不能直接写 `LocationComponent` 或 `VelocityComponent`。重力系统只写 `GravityStateComponent`；
空间协调器在统一提交时消费重力、AI 和反弹结果。这样同一实体仍只有一个位置和速度写入者。

### 4.4 Projectile 命中候选

Projectile 命中候选适配器负责：

- 在 Arch 查询边界读取 Projectile 和可受击实体的当前组件值；
- 应用 Projectile 的 Tile/Entity 参与策略、Owner/阵营排除和同 Tick/子步目标去重；
- 调用共享 Physics 碰撞子系统计算 Collider 几何接触、Layer/Mask、接触点和法线；
- 将物理接触与 Projectile/目标 handle、revision、伤害输入组合为 Entity hit candidate。

Physics 只提供通用碰撞事实，不依赖 Projectile、Combat 或实体类别。Projectile 候选适配器
不扣生命、不创建掉落、不写目标、不广播网络、不直接删除 Projectile。Tile sweep 属于共享
空间/Tile 查询；Tile 修改归 WorldInteraction owner。

### 4.5 Combat 命中协调

命中路径为：

ProjectileHitCandidate
→ Combat admission
→ damage/knockback/status commit
→ AcceptedHitReceipt
→ Projectile penetration/immunity/termination commit

Combat owner 才能写目标 Life、伤害减免、击退和状态。Projectile 只写自身免疫、穿透、
行为、寿命和终止状态。第一阶段使用“AcceptedHit 后消费”规则，除非 definition
明确声明接触消费。

Commit 前必须重新解析目标 handle、world scope、generation 和 target revision。目标槽位
复用、目标死亡或 revision 冲突时，拒绝或重新捕获，不能写旧快照对应的新实体。

### 4.6 Persistence Adapter

普通临时 Projectile 默认不写 WorldFile。允许持久化时，只保存声明的语义状态、定义版本、
owner/persistence identity 和必需关系。加载必须分配新 EntityUuid、world scope、generation，
重建关系；旧 runtime/Arch handle 不复用。

## 5. 读写集和阶段顺序

### 5.1 读写矩阵

| 边界 | 读取 | 唯一常规写入 | 禁止直接写入 |
| --- | --- | --- | --- |
| Lifecycle | definition、identity、spawn/terminate | 身份、组件关联、生命周期、必要索引 | Location、NPC Life、网络 I/O |
| Definition | 不可变目录 | 无；返回初始化值 | 实体运行状态 |
| AI | `AiComponent`、`BehaviorId`/`ProfileId`、能力组件、`AiRequirements` 快照和只读 facts | `ProjectileBehaviorStateComponent`、能力状态变更、`ProjectileAiIntent` | Location、Velocity、目标 Life、结构、网络 I/O |
| Gravity | Gravity 能力、配置、修饰器、速度事实 | `GravityStateComponent` | Location、Velocity、目标 Life、结构 |
| Simulation | 查询边界提供的 Projectile 组件值、已验证的 ProjectileAiIntent、SpatialFrame、facts | 运动结果和 intent | Arch、AI 行为状态、目标、网络 I/O |
| Collision | motion、World query、目标当前组件值 | collision result | Life、penetration、结构 |
| Combat | hit candidate、目标战斗状态 | 目标 Life、击退、效果 | Projectile 组件 |
| Penetration/Immunity | accepted receipt、Projectile 当前组件 | 穿透、免疫 | 目标 Life |
| Spatial commit | motion、entity/world revision | Location、Velocity、Grounding、LiquidContact | Life、网络 I/O |
| Persistence | 允许持久化的 snapshot | 存档记录 | runtime handle、实时组件 |

同一字段只能有一个常规写入者。Location/Velocity 只能由空间提交端口写；Life 只能由
Combat owner 写；Projectile lifetime、penetration、immunity 和 disposition 由 Projectile
owner 写。

### 5.2 NSSLC 阶段

Preparation：
1. 处理本地 Spawn、Reflect、Terminate 意图；
2. 校验 world scope、owner handle 和 definition；
3. 在结构安全点建立实体或登记待终止实体；
4. 在查询边界读取 Projectile、Owner、Target 和 Spatial 当前组件值；只保留本 Tick 的结果值。

Simulation：
1. AI 系统计算 Projectile 行为并生成 ProjectileAiIntent；
2. ProjectileSimulationSystem 按 extra updates 执行受控子步；
3. 计算速度、轨迹、lifetime 和液体解释；
4. 查询 Tile/entity collision；
5. 产生 hit candidates、effect intents 和 termination intents。

Commit：
1. 重新校验 EntityUuid、world scope、generation、revision；
2. 通过 SpatialSimulationCoordinatorSystem 提交位置/速度/接触；
3. 由 Combat owner 提交 accepted damage；
4. 由 Projectile AI 适配器提交行为/能力状态，由 Projectile owner 提交穿透、免疫和寿命；
5. 处理 PendingTerminate、撤销索引并删除实体；
6. 记录冲突、拒绝和需要重新捕获的结果。

Projection：
1. 提供已提交状态的本地只读快照；
2. 生成可选的本地表现 effect intent；
3. 不执行网络发送或网络确认。

结构变化不能在 Arch 查询遍历中直接增删，使用 command buffer 或等价安全边界。
同一 Projectile 一个 Tick 只允许一个权威运动提交者；旧专属 owner 接入前必须停止推进同一
实体的位置。

### 5.3 Extra updates 和寿命

ExtraUpdates 必须是 definition 约束的有限值。每个子步使用明确序号，
重新执行 Tile sweep 和实体候选，按 handle/generation 去重，并在终止后停止后续子步。

默认寿命按游戏 Tick 递减，除非 definition 明确按子步递减。此规则必须在 definition 版本中
固定，不能由客户端字段临时改变。

## 6. API 契约

下列契约中的 Spawn、Hit、Termination 关键类型已经落到 NSSLC C#；仍保留的概念部分用于
约束宿主接入，不代表所有特殊 Projectile 行为都已实现。

### 6.1 Spawn

ProjectileSpawnCommand：
DefinitionId、OwnerEntityRuntimeHandle、SourceMetadata、Position、Velocity、
BehaviorId、ProfileId、InitialBehaviorState、InitialLifetime、InitialPenetration、SourceTick。

`BehaviorId`/`ProfileId` 是行为选择输入；`InitialBehaviorState` 只是状态初始化值，不能替代
行为目录解析，也不能成为第二个行为实现入口。当前已经实现基础行为目录、AI 注册、行为
状态组件、脱离 ECS 模拟、生命周期、碰撞候选、Combat 接收、Life 提交、穿透/免疫和结构安全
终止已实现；宿主仍需把适配器按阶段注册，并补齐特殊 Projectile 的反弹策略、Tile 影响策略和跨
领域 runtime resolver。

ProjectileSpawnResult：
Applied、Rejected、Conflict、CapacityExhausted、DefinitionUnknown；
并可返回 EntityUuid、EntityRuntimeHandle、FailureReason。

成功顺序：定义校验 → owner 解析 → 身份/generation 分配 → 独立组件实例化 →
owner/lifecycle 登记 → Active 可见。

### 6.2 Tick

不暴露可写组件引用。输入包含 Tick、DeltaTime、WorldRevision、ProjectileSnapshot、
DefinitionSnapshot、OwnerFacts、SpatialFacts、TargetFacts 和可选 RandomFacts。输出包含
Status、SourceHandle、SourceRevision、MotionResult、HitCandidates、StateChanges、
TerminationIntent、EffectIntents。结果不等于已经写入；Commit 必须重检 revision/world version。

### 6.3 Hit 和 penetration

ProjectileHitCandidate 包含 ProjectileHandle/Revision、TargetHandle/Revision、HitPoint、
HitNormal、Substep 和 ContactKind。

Combat 返回 AcceptedHitReceipt，包含 HitId、ProjectileHandle、TargetHandle、AcceptedDamage、
AcceptedAtTick、TargetRevisionAfter 和 Status。

Projectile owner 使用 receipt 提交同 Tick 去重、免疫、accepted hit 计数、命中后终止或
继续运行，以及本地状态 revision。`AttributeKind.ProjectilePenetrationLimit` 提供有效
穿透上限，`AttributeKind.ProjectileHitImmunityTicks` 提供每目标免疫 Tick；Projectile
只保存 accepted hit 计数、免疫关系/到期 Tick 和 PendingTerminate 请求，不保存一份独立的
剩余穿透或共享免疫时长。Combat 拒绝的命中不产生 accepted receipt，也不更新这些状态。

### 6.4 Terminate

ProjectileTerminationCommand 包含 EntityRuntimeHandle、Reason、Source、RequestId 和 Tick。

原因至少包括 Expired、HitLimitReached、TileCollision、OutOfBounds、OwnerRemoved、
WorldUnloaded、Reflected、DefinitionRejected、StaleReference、
AdministrativeCleanup。

终止是幂等的。已 Terminated 的重复请求返回 AlreadyTerminated 或第一次结果，不重新发布
伤害、掉落或其他外部事件。

## 7. Command / Query 分类

### 7.1 Commands

下列动作改变权威状态，可由 System 方法承担；只有真实存在排队、跨边界、重试或结构安全点
需求时才提取独立 command record：

- Spawn Projectile；
- Reflect Projectile；
- accepted hit 后更新 penetration/immunity；
- Terminate Projectile；
- owner entity removal cleanup；
- world unload cleanup；
- persistence hydrate/rehydrate。

每项 Command 契约要写明 authority、写入组件、可见时点、失败和幂等。

### 7.2 Queries

不必机械创建 Query 类的读取包括：

- 读取不可变 definition；
- 在查询边界读取 Projectile 组件事实；
- 捕获 owner/target facts；
- 获取 collision candidates；
- 读取 revision、generation 和 active。

Query 不返回内部可变引用，不隐式刷新索引，不触发网络、日志、随机或时间；若依赖阶段或
缓存刷新点，要在契约中标明调用窗口。校验和写入必须原子时，不能暴露先 CanHit 再 ApplyHit
的 TOCTOU 窗口。

## 8. 失败、冲突和重试

| 状态 | 含义 | 处理 |
| --- | --- | --- |
| Applied | 权威状态已提交 | 进入下一阶段 |
| Rejected | 输入或权限无效，未写入 | 记录原因，不自动重试 |
| StaleReference | handle/generation/world scope 过期 | 重新捕获或丢弃 |
| Conflict | revision/world version 在提交前变化 | 重新捕获后按策略重算 |
| CapacityExhausted | 没有可用资源 | 延迟或拒绝，不能覆盖活动实体 |
| DefinitionUnknown | 内容未注册/版本不兼容 | 拒绝 |
| Duplicate | 请求已处理 | 返回既有结果或忽略 |
| AlreadyTerminated | 终止已完成 | 不重复效果 |
| NotReady | world/definition/owner 未准备 | 等待规定边界或拒绝 |

stale/conflict 只能重新捕获后重算，不能重放旧结果。Damage 已被 Combat 接受后，不能因
Projectile penetration 提交冲突而重复施伤。world unload 后旧 scope 的命令全部拒绝。

## 9. 典型流程

### 9.1 正常生成

Player/Weapon owner → Spawn command → Lifecycle 验证 → 分配 UUID/generation →
组件组合 → Preparation 结束可参与模拟 → Simulation 产生 motion/hit intent
→ Commit 写入空间和 Projectile 状态 → 本地只读查询观察提交结果。

### 9.2 NPC 命中

Projectile 组件事实 + spatial frame → sweep/AABB candidate → 重解析 target handle/revision
→ Combat admission → accepted damage → Projectile immunity/penetration → 耗尽则 terminate
→ 本地状态查询观察命中、穿透和终止结果。

### 9.3 Tile 碰撞

Projectile substep → tile sweep → TileCollisionResult → bounce/stop/terminate policy →
Spatial commit（若仍有效）或 Lifecycle termination。Tile 数据由 World query 提供，Projectile
不修改 Tile 数组。

### 9.4 Owner 移除/世界卸载

owner entity removal/world unload → 按 handle 查找 → PendingTerminate → 撤销本地 owner 索引
→ 删除实体 → 旧 handle 永久拒绝。

## 10. 验证矩阵

以下场景尚未运行，状态均为 proposed。

生命周期和身份：

- 有效 definition/owner Spawn；
- definition 不存在或版本不兼容；
- owner 不存在、已终止或 world scope 不匹配；
- capacity exhausted 不覆盖活动 Projectile；
- Spawn 中途失败完全清理；
- slot reuse 后旧 handle 不能解析新实体；
- world scope 切换后旧命令拒绝；
- 重复 Terminate 不重复效果；
- owner removal/world unload 清理所有本地关系和索引。

模拟和空间：

- 有限速度、位置和 collider；
- 没有追踪/反弹/重力能力组件时，不进入对应 Arch 查询；
- `BehaviorId`、`ProfileId` 和 `AiComponent.BehaviorType` 注册不兼容时拒绝创建或重绑定；
- `AiRequirements` 缺少输入时返回 `MissingComponents`，不使用隐式默认值；
- 追踪/反弹状态只由其适配器更新，位置和速度仍只有空间协调器写入；
- extra updates 的边界和配置上限；
- 子步 Tile 碰撞后停止；
- lifetime 1、0 和非法值；
- Tick/子步寿命策略；
- world revision 冲突拒绝整次提交；
- 共享空间端口没有第二个位置 writer；
- 液体只在能力开启时影响行为。

命中和战斗：

- owner 排除；
- 同 Tick 同目标只接受一次；
- target slot reuse 被 generation 检出；
- target revision 冲突不扣旧生命；
- 有效穿透上限由 `AttributeKind.ProjectilePenetrationLimit` 计算；accepted hit 计数达到上限时终止，负值表示无限；
- Combat 拒绝时按 definition 决定是否消费；
- Projectile commit 冲突时 damage 不重复；
- 免疫按 Projectile/target generation 隔离；
- Life 只能由 Combat owner 更新。

持久化边界：

- 临时 Projectile 不进入 WorldFile；
- 允许持久化的定义只保存声明字段；
- hydrate 分配新 UUID/generation；
- 旧 runtime/Arch handle 不复用；
- owner 无法解析时拒绝或终止，不留下半初始化实体。

## 11. 实施顺序

1. 确认 Definition ID、owner handle、source metadata 和基础 Projectile 组件；
2. 复用 Location、Velocity、Collider 和 SpatialSimulationCoordinatorSystem；
3. 建立 Lifecycle 的 spawn/terminate 失败清理；
4. 实现普通 Projectile 的 snapshot → motion result → spatial commit；
5. 接入一个 Tile collision query；
6. 以一个 NPC target 实现 accepted hit、penetration 和 termination；
7. 验证本地状态查询能观察到已提交的命中、穿透和终止结果。

第一阶段通过后再按实际内容增加更多行为 Profile、免疫、反射、液体效果、追踪、Minion/Sentry/Trap
和特殊持久化。每增加一种能力，都要检查是否引入新的权威写者或跨领域不变量。

迁移 MinimalImplementation 时，Manager 的 identity/revision/slot/cleanup 事实迁移为
Lifecycle 合同；Projectile 字段拆到组件；AiService 使用 detached snapshot；MovementService
通过 Spatial adapter 接入；CollisionService 只保留候选和策略；NpcCombatService 转为
Combat candidate/receipt。不要把 Manager 原样改名为
新的万能 owner，也不要恢复 Main.projectile 数组或静态免疫表。

## 12. 决策和开放问题

已决定：

- 组件组合，不建立大型 Projectile 聚合实体；
- 追踪、反弹和重力用能力/状态组件表达，不使用 Projectile 行为布尔字段或能力列表；
- `BehaviorId` 选择 AI 行为，`ProfileId` 选择不可变参数，`AiRequirements` 声明输入；
- Projectile 行为使用共享 `AiSystem<ProjectileAiIntent>`，不建立独立 `ProjectileBehaviorSystem`；
- 位置/速度复用现有空间提交合同；
- Combat 拥有目标生命；Attributes 计算 Projectile 的穿透上限和免疫时长，Projectile
  拥有 accepted hit 计数、免疫能力 list、寿命和终止请求；
- 本地身份只使用 EntityUuid 和 world-scoped runtime handle，不引入协议身份；
- 本阶段不实现网络会话、入站协议或出站复制；
- 普通 Projectile 默认不持久化；
- 结构变化在安全边界执行；
- 旧引用由 world scope + generation + revision 拒绝；
- 第一阶段只做普通 Projectile 端到端垂直切片。

实现前仍需补证据：

- 当前 Arch component capture/store 的最终适配入口；
- Tile sweep 和实体 broadphase 的生产接口；
- Combat accepted-hit 的原子提交/冲突恢复策略；
- 是否有必须跨存档保存的 Projectile；
- 各 projectile type 的行为等价范围。

## 13. 证据等级

| 结论 | 状态 | 依据 |
| --- | --- | --- |
| NSSLC 有阶段、身份和空间提交基础合同 | confirmed（合同层） | NSSLC Entity/Physics/Movement 源码和 README |
| NSSLC Projectile 基础实现 | partial | NSSLC Projectile 组件、行为目录、AI 意图、纯模拟、Arch 生命周期、碰撞候选、Combat 命中/生命提交和穿透/免疫已实现；宿主 Tick 装配仍 partial |
| MinimalImplementation 有 Projectile 领域骨架 | partial | Projectiles 目录、Manager、services、Host owner |
| MinimalImplementation 默认串完整运动/碰撞/命中链 | partial（未成立） | README 和 Host owner 表明默认链主要是 AI + expiry |
| 最新客户端有 Projectile 行为事实 | confirmed（参考范围） | Projectile.cs |
| 新设计与最新客户端架构不同 | proposed | owner、component、phase、adapter 约束 |
| 新设计已接入并通过运行时测试 | partial | 生产项目构建和脱离生产目录的 Arch/Combat 冒烟通过；完整宿主 Tick/行为等价测试仍未运行 |

本次实现没有修改 `src/NSSLC.Infrastructure`，没有恢复旧 Projectile 文件，也没有加入网络
协议。已新增 NSSLC Projectile 生命周期、候选、命中提交和实现说明；宿主 Tick 注册、完整
Tile/反弹策略和旧客户端行为等价仍需后续按文档边界接入。




