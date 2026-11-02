# Physics 大类一：碰撞几何

```text
文档 ID：DOC-2026-10-31-physics-collision-geometry-classification
逻辑域：reviews
产物类型：audit
状态：active
范围：src/NSSLC/Components/Share/Physics 下"碰撞几何"大类，6 个文件
证据入口：src/NSSLC/Components/Share/Physics/*.cs（逐文件读取）
canonical 路径：docs/reviews/audits/2026-10-31-physics-collision-geometry-classification.md
```

## 1. 大类定义

本类回答："一个实体占据什么形状、能否与谁相撞、相撞时的交点与法线如何计算"。

只做几何与资格判定，不写任何组件、不结算伤害、不决定击退。所有类型均为无状态纯函数
或不可变值，系统实例可由调用方按需构造或复用。

## 2. 成员清单

| 类型 | 文件 | 角色 | 关键成员 | 跨领域依赖 |
| --- | --- | --- | --- | --- |
| `ColliderComponent` | `ColliderComponent.cs` | 组件（持续状态） | `Size`、`Offset`、`Shape`、`Enabled`、`Width`、`Height` | 无（仅 `System.Numerics`） |
| `ColliderShape` | `ColliderShape.cs` | 枚举 | `AxisAlignedBox = 0` | 无 |
| `EntityCollisionCapabilityComponent` | `EntityCollisionCapabilityComponent.cs` | 能力组件 | `Enabled`、`Layer`、`Mask`、`CanCollideWith(uint)` | 无 |
| `TileCollisionCapabilityComponent` | `TileCollisionCapabilityComponent.cs` | 能力组件 | `Enabled`、`FallThroughPlatforms`、`UseSlopes`、`UseHoikCorrection` | 无 |
| `EntityCollisionContact` | `EntityCollisionContact.cs` | 值对象（输出） | `Point`、`Normal` | 无 |
| `EntityCollisionSystem` | `EntityCollisionSystem.cs` | 纯几何服务（非 Arch 系统） | `FindContacts<TEntity>(IReadOnlyList<SpatialBody<TEntity>>)`、`TryGetContact(...)` | `Terraria.Entity.LocationComponent` |

### 2.1 状态与能力的区分

- `ColliderComponent` 只描述形状、尺寸和偏移；碰撞响应由系统计算，组件不承载行为。
- 两个 `*CapabilityComponent` 只声明配置与资格，不保存系统实例、回调或世界引用。
- `TileCollisionCapabilityComponent` 与 `EntityCollisionCapabilityComponent` 分属 Tile 与
  实体两类碰撞域，字段不重叠，故不合并为单一"碰撞能力"。

### 2.2 系统边界

`EntityCollisionSystem` 不继承 `BaseSystem`，不持有 `Arch.Core.World`，输入是脱离 ECS 的
`SpatialBody<TEntity>` 序列。它只回答"是否相交、交点在哪、法线朝哪"，明确不决定谁受伤、
谁被击退、是否触发武器效果或是否删除实体。

`ColliderComponent` 构造时校验 `Size` 为有限正数、`Offset` 有限、`Shape` 已定义；
`EntityCollisionContact` 构造时校验 `Point` 与 `Normal` 有限。非法几何在构造期即失败，
不把 `NaN` 传入下游查询。

## 3. 与其他大类的衔接

| 衔接点 | 说明 |
| --- | --- |
| → 空间模拟 | `ColliderComponent.Size` 作为 Tile 运动查询与液体查询的输入尺寸 |
| → 空间模拟 | `TileCollisionCapabilityComponent` 直接作为 `ISpatialWorldQuery.Move` 参数 |
| → 接触事实 | `EntityCollisionSystem.FindContacts` 产出的 `EntityCollisionContact` 装入 `SpatialContactPair` |

## 4. 归类判定依据

- 6 个类型都只处理"形状、资格、交点"，不含 tick、revision、快照、提交或缓冲语义。
- `EntityCollisionContact` 虽与"接触事实"字面接近，但它是单次几何计算的结果值，
  不带 tick、revision、substep，也不是可缓冲的世界作用域事实，故归本类；
  `PhysicsContactFact` 才归"接触事实"大类。
- `EntityCollisionSystem` 与 `PhysicsContactCaptureSystem`（接触事实大类）职责不同：
  前者纯几何，后者负责从 ECS 取样、调用前者、写入缓冲。

## 5. 校验记录

- 目录内 24 个 `.cs` 文件，每个文件只声明一个核心公开类型，无同文件多类型。
- 本类 6 个文件中，仅 `EntityCollisionSystem.cs` 有 `Terraria.*` 依赖，依赖方向单向
  指向 `Entity`，不反向依赖 Physics 之外的运行时状态。
- 本文档编写时未移动、新增或修改任何源码；未执行构建。后续的目录落地与液体下沉为独立变更，见文末。

## 目录落地

自 2026-11-01 起，本类文件位于 `Components/Share/Physics/CollisionGeometry/`，
命名空间仍为 `Terraria.Physics` 未变。
