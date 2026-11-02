# Physics 组件与系统分类总览（含 Liquid）

```text
文档 ID：DOC-2026-10-31-physics-classification-index
逻辑域：reviews
产物类型：audit
状态：active
范围：src/NSSLC/Components/Share/Physics 分类研究覆盖 24 个 .cs 文件，
      加 src/NSSLC/Components/Share/Physics/Liquid 全部 9 个 .cs 文件，共 33 个；
      另有 2026-11-01 并入的 Physics/Movement 19 个 .cs 文件（未重新分类，见 §1.2）
证据入口：src/NSSLC/Components/Share/Physics/**/*.cs
canonical 路径：docs/reviews/audits/2026-10-31-physics-classification-index.md
```

## 1. 分类方法

按物理子域划分为四个大类，判定依据是"这个文件回答什么问题"。Liquid 不单列第五大类，
而是并入"空间查询"大类，理由见第 4 节。

| 大类 | 回答的问题 | 成员数 | 目录 | 文档 |
| --- | --- | --- | --- | --- |
| 碰撞几何 | 实体占什么形状、能否相撞、交点与法线怎么算 | 6 | `CollisionGeometry/` | [大类一](2026-10-31-physics-collision-geometry-classification.md) |
| 空间查询与液体规则 | 向谁询问世界、以什么版本为准、输入帧与输出值长什么样、液体接触按什么规则解释、液体如何流动 | 15（`SpatialQuery` 6 + `Liquid` 9） | `SpatialQuery/`、`Liquid/` | [大类二](2026-10-31-physics-spatial-query-classification.md) |
| 空间模拟 | 一帧内位置/速度/接地/液体如何计算并唯一提交 | 8 | `SpatialSimulation/` | [大类三](2026-10-31-physics-spatial-simulation-classification.md) |
| 接触事实 | 本 tick 有哪些接触、如何方向中立地交给下游 | 4 | `ContactFact/` | [大类四](2026-10-31-physics-contact-fact-classification.md) |
| 合计 | — | **33** | 5 个目录 | 与分类研究的 `.cs` 总数一致 |
| 运动与重力（2026-11-01 并入） | 实体如何积累速度、受哪些重力修正、意图如何解析 | 19 | `Movement/` | 见 §1.2 |

### 1.1 目录落地（2026-11-01）

分类结论已落到实际目录：每个子领域一个文件夹，`Physics/` 顶层不再有散落的 `.cs`。

```text
Components/Share/Physics/
  README.md
  CollisionGeometry/   6 .cs + README.md
  SpatialQuery/        6 .cs + README.md
  SpatialSimulation/   8 .cs + README.md
  ContactFact/         4 .cs + README.md
  Liquid/              9 .cs + README.md
  Movement/            19 .cs + README.md
```

目录只表达归属。全部文件的 C# 命名空间保持不变：四个子领域仍为 `Terraria.Physics`，
`Liquid/` 为 `Terraria.Liquid`，`Movement/` 为 `Terraria.Movement`（另有 `Terraria.Entity` 与 `Terraria.Combat` 各若干）。移动未改变公共 API
与运行时行为。

### 1.2 Movement 并入记录（2026-11-01）

`Components/Share/Movement/` 整体移入 `Physics/Movement/`，共 17 个文件
（16 个 `.cs` + `README.md`），用 `git mv` 完成以保留历史。

随后又把 `Components/Share/Entity/` 下属于空间状态的三个文件移入同一目录，
`Movement/` 现为 **19 个 `.cs`**：

| 源文件 | 目标文件 | 类型 | 命名空间 |
| --- | --- | --- | --- |
| `Entity/LocationComponent.cs` | `Movement/LocationComponent.cs` | `LocationComponent`（权威位置） | `Terraria.Entity` |
| `Entity/VelocityComponent.cs` | `Movement/CombatVelocityComponent.cs` | `VelocityComponent`（`[Obsolete]` 投影） | `Terraria.Combat` |
| `Entity/PositionComponent.cs` | `Movement/CombatPositionComponent.cs`（2026-11-01 拆除） | `PositionComponent`（`[Obsolete]` 投影） | `Terraria.Combat` |

后两个文件名加 `Combat` 前缀是**必须的**：`Movement/VelocityComponent.cs` 已被
`Terraria.Movement.VelocityComponent` 占用，同名文件无法共存。类型名保持不变，
因此没有改名，也没有命名空间冲突。

2026-11-01 复核后，`Movement/CombatPositionComponent.cs` 已删除（连同 `Combat/` 下另外三个
零引用组件壳）。删除依据是同批确认的「全仓零消费者」：过时投影没有任何调用方，而位置权威
已经唯一收敛到 `LocationComponent`，继续保留只会误导后来者。

| 项 | 结果 |
| --- | --- |
| 命名空间 | 全部保持不变：`Terraria.Movement`、`Terraria.Entity`、`Terraria.Combat` |
| 外部消费者 | `Terraria.Movement` 17 处；`LocationComponent` 17 个文件引用；两个 `[Obsolete]` 投影**全仓零消费者**（含全限定名与投影方法调用） |
| 代码改动 | 无。命名空间未变，移动本身即行为保持 |
| 构建 | `Terraria.Components` 与 `NSSLC.Server` 均 0 警告 0 错误 |

两个 `[Obsolete]` 投影是死代码：全仓无任何引用，内容仅为"请改用
`Terraria.Movement`/`Terraria.Entity` 版本"。本次按"移入 Movement"执行，未删除；
若要清理，它们是最安全的删除候选。

**未重新分类**：本文件第 2 节的四大类对照表仍只覆盖分类研究做过的 24 个文件。
Movement 内部按 gravity 前缀（11 个）、movement/velocity（5 个）、
空间状态与兼容投影（3 个）三簇组织，其归属说明以
`Physics/Movement/README.md` 为准，未按 §1 的四问法逐文件判定。

## 2. 四大类全量对照表（24）

| # | 文件 | 核心类型 | 大类 | 类型种类 | 接触 ECS 存储 |
| --- | --- | --- | --- | --- | --- |
| 1 | `ColliderComponent.cs` | `ColliderComponent` | 碰撞几何 | 组件（持续状态） | 是（被查询） |
| 2 | `ColliderShape.cs` | `ColliderShape` | 碰撞几何 | 枚举 | 否 |
| 3 | `EntityCollisionCapabilityComponent.cs` | `EntityCollisionCapabilityComponent` | 碰撞几何 | 能力组件 | 是（被查询） |
| 4 | `TileCollisionCapabilityComponent.cs` | `TileCollisionCapabilityComponent` | 碰撞几何 | 能力组件 | 是（被读取） |
| 5 | `EntityCollisionContact.cs` | `EntityCollisionContact` | 碰撞几何 | 值对象 | 否 |
| 6 | `EntityCollisionSystem.cs` | `EntityCollisionSystem` | 碰撞几何 | 纯几何服务 | 否（输入为脱离序列） |
| 7 | `ISpatialWorldQuery.cs` | `ISpatialWorldQuery` | 空间查询 | 端口接口 | 否 |
| 8 | `ISpatialWorldRevisionSource.cs` | `ISpatialWorldRevisionSource` | 空间查询 | 端口接口 | 否 |
| 9 | `SpatialWorldRevision.cs` | `SpatialWorldRevision` | 空间查询 | 值对象 | 否 |
| 10 | `SpatialFrame.cs` | `SpatialFrame` | 空间查询 | 输入帧 | 否 |
| 11 | `SpatialTileMotionResult.cs` | `SpatialTileMotionResult` | 空间查询 | 查询输出 | 否 |
| 12 | `SpatialLiquidContact.cs` | `SpatialLiquidContact` | 空间查询 | 查询输出（液体） | 否 |
| 13 | `SpatialSimulationSystem.cs` | `SpatialSimulationSystem` | 空间模拟 | 纯计算内核 | 否 |
| 14 | `SpatialSimulationCoordinatorSystem.cs` | `SpatialSimulationCoordinatorSystem` | 空间模拟 | Arch 系统 | **是（唯一写入者）** |
| 15 | `SpatialEntitySnapshot.cs` | `SpatialEntitySnapshot<TEntity>` | 空间模拟 | 短生命周期输入 | 否 |
| 16 | `SpatialBody.cs` | `SpatialBody<TEntity>` | 空间模拟 | 短生命周期输入 | 否 |
| 17 | `SpatialCommit.cs` | `SpatialCommit` | 空间模拟 | 提交载荷 | 是（写入内容） |
| 18 | `SpatialSimulationResult.cs` | `SpatialSimulationResult<TEntity>` | 空间模拟 | 计算结果 | 否 |
| 19 | `SpatialSimulationStatus.cs` | `SpatialSimulationStatus` | 空间模拟 | 枚举 | 否 |
| 20 | `GroundingComponent.cs` | `GroundingComponent` | 空间模拟 | 组件（提交后状态） | 是（被写入） |
| 21 | `SpatialContactPair.cs` | `SpatialContactPair<TEntity>` | 接触事实 | 几何结果值 | 否 |
| 22 | `PhysicsContactFact.cs` | `PhysicsContactFact` | 接触事实 | tick 级事实 | 否 |
| 23 | `PhysicsContactBuffer.cs` | `PhysicsContactBuffer` | 接触事实 | 世界作用域缓冲 | 否 |
| 24 | `PhysicsContactCaptureSystem.cs` | `PhysicsContactCaptureSystem` | 接触事实 | Arch 系统 | 是（只读） |

## 3. 液体全量对照表（9）

| # | 文件 | 核心类型 | 大类 | 类型种类 | 接触 ECS 存储 |
| --- | --- | --- | --- | --- | --- |
| 1 | `LiquidInteractionCapabilityComponent.cs` | `LiquidInteractionCapabilityComponent` | 空间查询 | 能力组件（规则输入） | 是（被读取） |
| 2 | `LiquidContactFlags.cs` | `LiquidContactFlags` | 空间查询 | 位标记枚举 | 否 |
| 3 | `LiquidContactComponent.cs` | `LiquidContactComponent` | 空间查询 | 组件（提交后状态） | 是（被写入） |
| 4 | `LiquidComponent.cs` | `LiquidComponent` | 空间查询 | 遗留类型（未使用） | 否（零引用） |
| 5 | `LiquidKind.cs` | `LiquidKind` | 空间查询 | 枚举 | 否 |
| 6 | `LiquidSimulation.cs` | `LiquidSimulation` | 空间查询 | 液体运算内核（`internal`） | 否（经存储端口） |
| 7 | `LiquidTileStorage.cs` | `LiquidTileStorage` | 空间查询 | 存储接缝（`internal`） | 否 |
| 8 | `LiquidTileStorageBinding.cs` | `LiquidTileStorageBinding` | 空间查询 | 端口实现（`internal`） | 经 `WorldRuntime` |
| 9 | `LiquidBlockingRules.cs` | `LiquidBlockingRules` | 空间查询 | 阻塞规则表（`internal`） | 否 |

## 4. 为什么 Liquid 并入"空间查询"而不是单列大类

| 依据 | 说明 |
| --- | --- |
| 唯一入口是查询 | 液体接触只能经 `ISpatialWorldQuery.GetLiquidContact` 获得 |
| 唯一出口是转换 | `SpatialLiquidContact.ToComponent()` 转为 `LiquidContactComponent` |
| 无独立 ECS 系统 | Liquid 无 `BaseSystem` 实现、无 tick 循环、无提交端口；运算内核不持有调度 |
| 无独立调度 | 液体步进由 `WorldTickSystem` 调用，更新依附世界 tick |
| 能力组件即入参 | `LiquidInteractionCapabilityComponent` 同时是查询入参和下游解释依据 |

因此液体属于"查询 + 规则 + 世界内运算"这一层，与"空间模拟"的实体状态提交分层不同。

### 4.1 液体运算的实际归属（2026-10-31 起）

液体算法已从 `WorldRuntime` 下沉到 `Liquid/LiquidSimulation.cs`：

```text
WorldTickSystem.Update
        ↓
WorldRuntime.AdvanceLiquidSimulation          # 持锁、推进 revision、记变更日志
        ↓
LiquidSimulation.Step(storage, budget)        # 纯算法：队列 + 流动 + 水平均衡
        ↓ LiquidTileChange[Coordinate, Previous, Current]
WorldRuntime.RecordMutationUnsafe → _pendingMutations
        ↓
DrainPendingMutations → 网络投影
```

`WorldRuntime` 只保留世界所有权职责（锁、revision、变更日志、对外端口），算法与队列
状态归 `LiquidSimulation`。存储访问经 `LiquidTileStorage` 端口注入，使算法不直接
依赖 `WorldTileStorageComponent`。

## 5. 组件、系统与契约的分布

只有 2 个类型真正继承 Arch 系统基类：

- `SpatialSimulationCoordinatorSystem`（空间模拟）：唯一常规空间写入者；
- `PhysicsContactCaptureSystem`（接触事实）：只读捕获，写世界作用域缓冲。

另有 2 个**不是 Arch 系统**的"系统"命名类型，容易被误分类：

- `SpatialSimulationSystem`：纯函数式计算内核，无 ECS 依赖；
- `EntityCollisionSystem`：纯几何服务，输入为脱离 ECS 的 `SpatialBody` 序列。

可附加到实体的组件共 8 个：

| 组件 | 领域 | 角色 |
| --- | --- | --- |
| `ColliderComponent` | Physics | 碰撞几何 |
| `EntityCollisionCapabilityComponent` | Physics | 实体碰撞资格 |
| `TileCollisionCapabilityComponent` | Physics | Tile 碰撞配置 |
| `GroundingComponent` | Physics | 提交后的接地状态 |
| `LiquidInteractionCapabilityComponent` | Liquid | 液体影响资格 |
| `LiquidContactComponent` | Liquid | 提交后的液体接触 |
| `LiquidComponent` | Liquid | 遗留，未接线 |
| （`LocationComponent`/`VelocityComponent`） | Entity | 被模拟读写，不计入本分类 |

## 6. 跨领域依赖方向

依赖方向经核查为单向，未发现反向依赖：

| 被依赖领域 | 类型 | 使用方 |
| --- | --- | --- |
| `Terraria.Entity` | `LocationComponent`、`VelocityComponent`、`EntityRuntimeHandle`、`EcsServerTickContext`、`IEntityRuntimeHandleResolver` | 几何、模拟、接触捕获 |
| `Terraria.Movement` | `MovementCapabilityComponent`、`GravityCapabilityComponent`、`GravityStateComponent`、`GravityDirection`、`MovementIntent`、`MovementMode` | 空间查询、空间模拟 |
| `Terraria.Liquid` | `LiquidContactComponent`、`LiquidContactFlags`、`LiquidInteractionCapabilityComponent` | 空间查询、空间模拟 |

Physics → Liquid 是单向依赖；Liquid 不反向引用 Physics 任何类型（`SpatialLiquidContact`
位于 Physics 侧，由它引用 Liquid，而非相反）。

## 7. 已发现的分类难点与判定

| 难点 | 判定 |
| --- | --- |
| `EntityCollisionContact` 与 `PhysicsContactFact` 都是"接触" | 前者是单次几何值（无 tick/revision），归碰撞几何；后者是帧级事实，归接触事实 |
| `SpatialFrame` 由模拟侧构造 | 语义是"本次查询的世界视图"，归空间查询 |
| `GroundingComponent` 是组件 | 是模拟提交的直接产物，归空间模拟 |
| `SpatialLiquidContact` 有 `ToComponent()` | 是查询输出，转换到液体领域的持久组件，归空间查询 |
| `LiquidContactComponent` 归哪个大类 | 其产生与写入完全依附空间模拟，且与 `SpatialLiquidContact` 同链，归空间查询 |
| 两个 `*System` 不继承 `BaseSystem` | 按职责归入纯内核/几何服务，不因命名归入"系统" |

## 8. Liquid 归档问题（仅记录，未修改源码）

| 问题 | 证据 |
| --- | --- |
| 命名空间与目录不符 | `LiquidComponent.cs` 为 `namespace EntityEcs.Components;`，同目录其余 3 个文件为 `Terraria.Liquid` |
| 一文件双类型 | `LiquidComponent.cs` 同时声明 `LiquidComponent` 与 `LiquidKind` |
| 零引用 | 全 `src/NSSLC/**` 与 `Test/**` 无命中 |
| 与 `LiquidContactFlags` 语义重复 | `LiquidKind` 的 `Water`/`Honey`/`Lava`/`Shimmer` 与标志位重复，`Nano` 独有 |
| 注释与实现不符 | 注释称"保存实体液体类别、接触标记和接触计时"，实际只有 `LiquidList` 字段 |

详见[大类二第 5 节](2026-10-31-physics-spatial-query-classification.md)。按仓库约束，本次未
删除、移动或合并该文件，仅如实记录。

## 9. 结论与限制

- 本分类为静态源码审阅结论，**未移动、拆分或修改任何源码**，未执行构建或测试。
- 分类反映 2026-10-31 工作树快照；`SpatialSimulationSystem.cs` 存在一处会话前已有的
  未提交改动（AI 意图垂直速度处理），分类依据为工作树实际版本。
- 后续若新增 Physics 或 Liquid 类型，需按同一判定标准更新本文档。
- 本分类不构成运行时行为验证，也不构成 `MinimalImplementation` 接入证明。
