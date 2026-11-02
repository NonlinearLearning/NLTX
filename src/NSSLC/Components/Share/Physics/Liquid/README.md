# Liquid 液体

液体查询输出和液体规则输入分开保存：

- `LiquidContactComponent` 是空间系统提交的本帧接触事实；
- `LiquidInteractionCapabilityComponent` 表示实体是否受某类液体影响；
- `LiquidContactFlags` 用位标记表示接触类别。

玩家、NPC 和 Projectile 可以共享接触事实格式，但每个领域系统仍可按自己的能力组件
解释水、蜂蜜、岩浆和微光的后续效果。不要用一个公共 `Wet` 布尔值替代这些规则。

## 运算内核

| 类型 | 角色 |
| --- | --- |
| `LiquidSimulation` | tile 级确定性流动运算：队列、向下流动、水平均衡（`internal`） |
| `LiquidTileStorage` | 存储接缝端口，使算法不直接依赖具体 tile 存储（`internal`） |
| `LiquidTileStorageBinding` | 端口实现，薄封装，无锁无日志（`internal`） |
| `LiquidBlockingRules` | 液体阻塞判定规则表（`internal`） |
| `LiquidComponent` / `LiquidKind` | 遗留类型，零引用，待评估删除 |

`LiquidSimulation` 只做计算。锁、revision 推进与变更日志仍由 `WorldRuntime` 持有，
它调用运算内核并记录 `LiquidTileChange`。

命名空间：`Terraria.Liquid`。
