# Physics 大类三：空间模拟

```text
文档 ID：DOC-2026-10-31-physics-spatial-simulation-classification
逻辑域：reviews
产物类型：audit
状态：active
范围：src/NSSLC/Components/Share/Physics 下"空间模拟"大类，8 个文件
证据入口：src/NSSLC/Components/Share/Physics/*.cs（逐文件读取）
canonical 路径：docs/reviews/audits/2026-10-31-physics-spatial-simulation-classification.md
```

## 1. 大类定义

本类回答："一帧之内，实体的位置、速度、接地和液体接触如何被计算并唯一提交"。

这是 Physics 中唯一包含行为的核心：一个纯计算内核（`SpatialSimulationSystem`）+ 一个
唯一写入端口（`SpatialSimulationCoordinatorSystem`）+ 围绕二者的快照、提交、结果和
接地状态类型。

## 2. 成员清单

| 类型 | 文件 | 角色 | 关键成员 | 跨领域依赖 |
| --- | --- | --- | --- | --- |
| `SpatialSimulationSystem` | `SpatialSimulationSystem.cs` | 纯计算内核（非 Arch 系统） | `Simulate<TEntity>(in SpatialFrame, in SpatialEntitySnapshot<TEntity>, ...9 个输入, float)` | `Terraria.Movement`、`Terraria.Liquid` |
| `SpatialSimulationCoordinatorSystem` | `SpatialSimulationCoordinatorSystem.cs` | Arch 系统（唯一常规写入者） | `Update(in EcsServerTickContext)` | `Terraria.Entity`、`Movement`、`Liquid`、`Arch.System` |
| `SpatialEntitySnapshot<TEntity>` | `SpatialEntitySnapshot.cs` | 短生命周期输入 | `Entity`、`Location`、`Velocity`、`Collider`、`Grounding`、`LiquidContact` | `Entity`、`Movement`、`Liquid` |
| `SpatialBody<TEntity>` | `SpatialBody.cs` | 短生命周期输入（带碰撞策略） | `Snapshot`、`Collision` | 无（组合本域 + 碰撞几何） |
| `SpatialCommit` | `SpatialCommit.cs` | 提交载荷 | `Location`、`Velocity`、`Grounding`、`LiquidContact` | `Entity`、`Movement`、`Liquid` |
| `SpatialSimulationResult<TEntity>` | `SpatialSimulationResult.cs` | 计算结果 | `Status`、`Entity`、`WorldRevision`、`Commit`、`JumpConsumed`、`Detail`、`Applied(...)`、`Rejected(...)` | 无 |
| `SpatialSimulationStatus` | `SpatialSimulationStatus.cs` | 状态枚举 | `Applied`、`Rejected`、`Conflict`、`NotReady` | 无 |
| `GroundingComponent` | `GroundingComponent.cs` | 组件（提交后持续状态） | `IsGrounded`、`OnPlatform`、`SurfaceNormal`、`Airborne` | 无 |

### 2.1 纯内核与协调器分离

`SpatialSimulationSystem` 不引用 `Arch.Core`，签名中不出现 `World` 或 `Entity` 类型参数
以外的 ECS 概念，只接受脱离的 `SpatialEntitySnapshot`。它的注释明确"不访问 ECS 存储，
不提交任何组件"。因此模拟规则可以在无 ECS 环境下单测。

`SpatialSimulationCoordinatorSystem` 继承 `BaseSystem<ArchWorld, EcsServerTickContext>`，
职责严格限定为：

1. 用一个 Arch 组合查询（`Location` + `Velocity` + `Collider` + `Grounding`）取候选实体；
2. 补齐六个能力/意图组件（缺失则跳过该实体）；
3. 构造快照、调用纯内核；
4. 仅在 `Applied` 且世界 revision 未变时，一次性写入 `Location`、`Velocity`、
   `Grounding` 和**已存在**的 `LiquidContact`。

### 2.2 提交不变量

协调器是 Arch 空间状态的唯一常规写入端口。三条硬约束在代码中可见：

- 进入回调先判 `collider.Enabled`，禁用碰撞体的实体不参与空间步进；
- 提交前二次核对 `_spatialWorld.Revision != revision` 则整体放弃本次结果，防止跨快照
  写入；
- `LiquidContact` 只在 `World.Has<LiquidContactComponent>(entity)` 为真时写入，不会为
  实体凭空创建液体组件。

由此推导出对下游的约束：网络包、AI、战斗和表现层都不得直接写 `LocationComponent` 或
`VelocityComponent`；传送是非连续位移，必须走独立的传送命令与提交规则，不能伪装成普通
速度。

### 2.3 单帧计算顺序

`SpatialSimulationSystem.Simulate` 的顺序是固定的，且每一步都有显式拒绝分支：

1. `intent.Tick != frame.Tick` → `Conflict`；
2. `deltaTime` 非有限或非正 → `NotReady`；
3. 碰撞体或移动能力未启用 → `NotReady`；
4. 重力方向未解析（非 `Normal`/`Reversed`） → `NotReady`；
5. 按 `MovementMode` 分流：`Flight`/`Free` 走自由速度解析，其余走地面速度解析；
6. 跳跃消耗：`JumpRequested && CanJump && IsGrounded`，按重力方向取反 `JumpSpeed`；
7. 重力累加：`gravityCapability.Enabled` 且非 `Flight` 时施加，钳制在 `±MaxFallSpeed`；
8. Tile 运动查询：`tileCollision.Enabled` 时调用 `frame.World.Move`，否则退化为直线位移；
9. 非有限结果 → `Rejected`；
10. 液体接触查询：`liquidInteraction.Enabled` 时调用，否则置 `None`；
11. 由 `CollidedY` 推出 `GroundingComponent`，组装 `SpatialCommit` 并返回 `Applied`。

AI 来源的意图有一处专门处理：`MovementIntentSource.Ai` 只接管水平转向，`desired.Y`
保留当前垂直速度，避免 AI 的 `y=0` 目标每 tick 抵消下落。

### 2.4 查询组合要求

协调器的查询是 `Location + Velocity + Collider + Grounding` 四元组，六个能力/意图组件
通过 `World.TryGet` 逐个补齐。任一缺失即跳过该实体，不产生部分写入。这意味着实体必须
完整具备移动能力、重力能力、重力状态、Tile 碰撞能力、液体交互能力和移动意图，才会
进入空间步进的写入路径。

## 3. 与其他大类的衔接

| 衔接点 | 说明 |
| --- | --- |
| ← 碰撞几何 | 使用 `ColliderComponent` 尺寸；把 `TileCollisionCapabilityComponent` 传给世界查询 |
| ← 空间查询 | 经 `SpatialFrame.World`（`ISpatialWorldQuery`）获取 Tile 运动与液体接触 |
| ← 液体规则 | `LiquidInteractionCapabilityComponent` 作为 `GetLiquidContact` 入参；产物 `SpatialLiquidContact` 经 `ToComponent()` 变成 `LiquidContactComponent` |
| ← 移动领域 | `MovementCapabilityComponent`、`GravityCapabilityComponent`、`GravityStateComponent`、`MovementIntent` 是模块输入，不属于 Physics |
| ← 实体领域 | `LocationComponent`、`VelocityComponent` 是读写目标，不属于 Physics |
| → 接触事实 | 协调器不产出接触事实；接触捕获由 `PhysicsContactCaptureSystem` 独立完成 |

## 4. 归类判定依据

- 8 个文件共同表达"如何算、如何提交"的完整闭环：输入（`SpatialEntitySnapshot`、
  `SpatialBody`）、规则（`SpatialSimulationSystem`）、结果（`SpatialSimulationResult`、
  `SpatialSimulationStatus`、`SpatialCommit`）、落地（`SpatialSimulationCoordinatorSystem`）
  与落地后的支撑状态（`GroundingComponent`）。
- `GroundingComponent` 虽是"组件"，但它是空间步进的直接提交产物，语义是"上一次提交的
  支撑接触"，与模拟闭环不可分割，故归本类而非碰撞几何类。
- `SpatialBody` 把快照与碰撞策略配对，仅服务本域的批量输入构造，不构成独立子域。

## 5. 校验记录

- 本类 8 个文件中，5 个文件含 `Terraria.*` 依赖，全部指向 `Entity` / `Movement` /
  `Liquid`，方向单向，Physics 未被反向依赖。
- 只有 `SpatialSimulationCoordinatorSystem.cs` 引用 `Arch.*`；纯内核无 Arch 依赖。
- 未发现同文件多类型；未发现文件名与类型名不一致。
- 本文档编写时未移动、新增或修改任何源码；未执行构建。后续的目录落地与液体下沉为独立变更，见文末。

## 目录落地

自 2026-11-01 起，本类文件位于 `Components/Share/Physics/SpatialSimulation/`，
命名空间仍为 `Terraria.Physics` 未变。
