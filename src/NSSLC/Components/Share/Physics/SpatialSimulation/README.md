# SpatialSimulation 空间模拟

回答"一帧内位置/速度/接地如何计算，并如何唯一提交"。确定性计算与提交在此分层。

| 类型 | 角色 |
| --- | --- |
| `SpatialSimulationSystem` | 纯计算内核（名称含 System，但不继承 Arch 系统基类） |
| `SpatialSimulationCoordinatorSystem` | Arch 系统，**唯一常规空间状态写入者** |
| `SpatialEntitySnapshot<TEntity>` | 从 ECS 复制的短生命周期输入 |
| `SpatialBody<TEntity>` | 单实体空间输入 |
| `SpatialSimulationResult<TEntity>` | 计算结果 |
| `SpatialCommit` | 提交载荷 |
| `SpatialSimulationStatus` | 结果状态枚举 |
| `GroundingComponent` | 提交后的接地状态（持续组件） |

## 提交不变量

`SpatialSimulationCoordinatorSystem` 在一次 Arch 查询中读取完整基础组合，提交前确认世界
revision 未变，然后一起写入 `Location`、`Velocity`、`Grounding` 和已有的
`LiquidContact`。网络包、AI、战斗和表现层都不能直接写 `LocationComponent` 或
`VelocityComponent`。传送是非连续位移，走独立命令，不伪装成普通速度。

命名空间：`Terraria.Physics`。
