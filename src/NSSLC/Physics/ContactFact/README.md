# ContactFact 接触事实

回答"本 tick 有哪些接触，以及如何方向中立地交给下游"。本目录不做状态提交。

| 类型 | 角色 |
| --- | --- |
| `SpatialContactPair<TEntity>` | 两个实体之间的几何接触值 |
| `PhysicsContactFact` | tick 级接触事实 |
| `PhysicsContactBuffer` | 世界作用域接触缓冲 |
| `PhysicsContactCaptureSystem` | Arch 系统，只读捕获，写世界作用域缓冲 |

`PhysicsContactCaptureSystem` 只读捕获，不写实体的 `Location` / `Velocity`。伤害、击退
和武器效果由下游领域解释，不在本目录决定。

命名空间：`Terraria.Physics`。
