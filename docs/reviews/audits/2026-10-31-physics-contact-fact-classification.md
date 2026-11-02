# Physics 大类四：接触事实

```text
文档 ID：DOC-2026-10-31-physics-contact-fact-classification
逻辑域：reviews
产物类型：audit
状态：active
范围：src/NSSLC/Components/Share/Physics 下"接触事实"大类，4 个独有文件（另复用碰撞几何类的
`EntityCollisionSystem`）
证据入口：src/NSSLC/Components/Share/Physics/*.cs（逐文件读取）
canonical 路径：docs/reviews/audits/2026-10-31-physics-contact-fact-classification.md
```

## 1. 大类定义

本类回答："本 tick 发生了哪些几何接触，如何在不偏向任何业务方向的前提下把它们交给
下游领域"。

这是一个 tick 级、世界作用域的事实通道：Physics 只产出"谁和谁在什么位置以什么法线
接触"，不判定攻击者与目标，也不结算后果。

## 2. 成员清单

| 类型 | 文件 | 角色 | 关键成员 | 跨领域依赖 |
| --- | --- | --- | --- | --- |
| `SpatialContactPair<TEntity>` | `SpatialContactPair.cs` | 几何结果值 | `First`、`Second`、`Contact` | 无 |
| `PhysicsContactFact` | `PhysicsContactFact.cs` | tick 级事实值 | `First`、`Second`、`Contact`、`Tick`、`Revision`、`Substep` | `Terraria.Relationships`（`EntityRuntimeHandle`） |
| `PhysicsContactBuffer` | `PhysicsContactBuffer.cs` | 世界作用域缓冲 | `Count`、`Tick`、`Revision`、`BeginCapture(...)`、`Add(in ...)`、`Drain()`、`Clear()` | `Terraria.Relationships` |
| `PhysicsContactCaptureSystem` | `PhysicsContactCaptureSystem.cs` | Arch 系统（捕获者） | `Update(in EcsServerTickContext)` | `Arch.*`、`Entity`、`Relationships` |
| `EntityCollisionSystem`（复用） | `EntityCollisionSystem.cs` | 见"碰撞几何"大类 | `FindContacts<TEntity>` | 见该大类 |

### 2.1 方向中立

`PhysicsContactFact` 的注释明确"端点方向刻意保持中立；由领域适配器决定哪个端点是攻击者、
目标或交互参与者"。因此事实类型不存在 `Attacker`/`Target` 这类带方向语义的字段，只有
`First`/`Second`。这是 Combat、Interaction 等领域各自解释接触的前提。

`PhysicsContactFact` 构造时校验两个运行时句柄有效且互不相同，并要求 `tick >= 0`、
`revision.IsValid`、`substep >= 0`。非法事实在构造期失败。

### 2.2 缓冲的帧约束

`PhysicsContactBuffer` 是 tick 作用域容器，不变量由三处代码保证：

- `BeginCapture(tick, revision)` 校验 tick 非负、revision 有效，并清空上一帧；
- `Add(in ...)` 拒绝 tick 或 revision 不属于当前捕获帧的事实的写入；
- `Add(in ...)` 对同一 `Substep` 下相同端点对（含顺序颠倒）去重，避免同一几何接触
  被重复投递。

`Drain()` 返回只读快照并清空内部列表，`Clear()` 同时复位 tick 与 revision，使缓冲区
回到"未开始捕获"状态。缓冲区是世界作用域的，不是实体组件。

### 2.3 捕获系统边界

`PhysicsContactCaptureSystem` 继承 `BaseSystem<ArchWorld, EcsServerTickContext>`，其查询是
`Location + Collider + EntityCollisionCapability` 三元组。执行步骤：

1. 读取 `ISpatialWorldQuery.Revision` 并 `BeginCapture`；
2. 查询所有具备空间三要素的实体，逐个经 `IEntityRuntimeHandleResolver` 解析运行时句柄，
   解析失败则跳过；
3. 速度用 `World.TryGet` 取，缺失时退化为 `VelocityComponent.Stationary`（捕获几何不需要
   速度，此处仅为构造统一快照）；
4. 组装 `SpatialBody` 列表，交给 `EntityCollisionSystem.FindContacts` 做纯几何计算；
5. 把 `SpatialContactPair` 与句柄、tick、revision 组合成 `PhysicsContactFact` 写入缓冲，
   `substep` 固定为 `0`。

该系统只读 ECS、只写缓冲，不写任何组件。它是"接触事实"大类中唯一接触 ECS 的部分。

### 2.4 与空间模拟互不依赖

接触捕获与空间步进是两条独立链路：`SpatialSimulationCoordinatorSystem` 不产出接触事实，
`PhysicsContactCaptureSystem` 也不读写 `Location`/`Velocity` 的提交结果。二者共享
`ISpatialWorldQuery`、`EntityCollisionSystem` 和几何组件，但不互相调用，调度顺序由宿主
决定而非文件顺序。

## 3. 与其他大类的衔接

| 衔接点 | 说明 |
| --- | --- |
| ← 碰撞几何 | `EntityCollisionSystem` 提供候选对与接触点/法线；`ColliderComponent`、`EntityCollisionCapabilityComponent` 是查询条件 |
| ← 空间查询 | `ISpatialWorldQuery.Revision` 标注事实所属世界版本 |
| ← 液体规则 | 实体几何接触事实与液体接触事实并列，同属"本帧事实"语义；液体侧由 `LiquidContactComponent` 承载，不进本类缓冲 |
| → Combat / Interaction | 下游领域按自身规则解释 `First`/`Second`，决定伤害、击退或交互 |
| → 实体领域 | `EntityRuntimeHandle`、`IEntityRuntimeHandleResolver` 来自 `Terraria.Entity` / `Terraria.Relationships` |

## 4. 归类判定依据

- 4 个独有文件共同表达"本 tick 的接触事实如何被捕获、去重、按帧缓冲和取走"，
  与碰撞几何的"单次几何判定"和空间模拟的"状态提交"都不同。
- `SpatialContactPair` 是 `EntityCollisionSystem` 的输出，也是事实的雏形；它不含 tick 与
  revision，因此归本类作为"事实的几何基元"，与 `EntityCollisionContact`（更底层的单次
  接触值）分开。
- `EntityCollisionSystem` 本身归"碰撞几何"大类；本类只复用它，不重复计入成员总数。

## 5. 校验记录

- 本类 4 个独有文件中，`PhysicsContactFact.cs`、`PhysicsContactBuffer.cs` 依赖
  `Terraria.Relationships`；`PhysicsContactCaptureSystem.cs` 依赖 `Arch.*`、`Terraria.Entity`、
  `Terraria.Relationships`；`SpatialContactPair.cs` 无外部依赖。
- 四大类成员数合计 6 + 10 + 8 + 4 = 28，对应 Physics 目录 24 个 `.cs` 加 Liquid 目录
  4 个 `.cs`，一一对应，无遗漏、无重复计数。`EntityCollisionSystem` 归"碰撞几何"大类，
  本类表格中的该行只是跨类复用说明，不计入本类成员数。
- 两个目录另有 2 个 `README.md`，是领域说明而非分类产物，不计入成员。
- 本文档编写时未移动、新增或修改任何源码；未执行构建。后续的目录落地与液体下沉为独立变更，见文末。

## 目录落地

自 2026-11-01 起，本类文件位于 `Components/Share/Physics/ContactFact/`，
命名空间仍为 `Terraria.Physics` 未变。
