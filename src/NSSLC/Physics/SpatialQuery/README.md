# SpatialQuery 空间查询

回答"向谁询问世界、以什么版本为准、输入帧与输出值长什么样"。本目录只定义端口和
值对象，不含算法、不含跨帧缓冲、不写 ECS。

| 类型 | 角色 |
| --- | --- |
| `ISpatialWorldQuery` | 只读世界端口：`Move(...)` 与 `GetLiquidContact(...)` |
| `ISpatialWorldRevisionSource` | 版本源端口 |
| `SpatialWorldRevision` | 世界代次与版本值 |
| `SpatialFrame` | 批次级不可变输入（tick、revision、世界端口） |
| `SpatialTileMotionResult` | Tile 运动查询输出 |
| `SpatialLiquidContact` | 液体接触查询输出，经 `ToComponent()` 转为持久状态 |

接口约定实现方提供"脱离且带版本的只读世界数据；不接收实体，也不执行提交"。因此查询
无法绕过提交屏障写入 ECS。

命名空间：`Terraria.Physics`。
