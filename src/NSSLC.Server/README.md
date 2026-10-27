# NSSLC ECS Server

这是一个以 `Terraria.Entity.EcsServer` 为调度核心的最小可执行 Host。它不依赖
`MinimalImplementation.Server` 的生命周期、世界文件或旧路由器。

组合入口在 `Program.cs` 中显式创建并连接：

- `WorldRuntime` 和 `WorldTickSystem`
- `CommunicationHostRuntime`
- `CommunicationSystemRegistration.CreateGame`
- `CombatSystemRegistration.Create`
- `WorldInteractionSystem`
- 共享的 `EntityRuntimeRegistry`
- `EcsServer` 的 Preparation、Simulation、Commit、Projection 阶段

TCP 连接建立时，Host 为会话创建一个 ECS 玩家实体并绑定到
`CommunicationSessionComponent`；连接关闭时，Host 回收对应实体和运行时句柄。
通信系统只把数据转成移动、世界交互和战斗意图，权威提交由后续 ECS 阶段完成。

当前最小实例使用 `256x128` 的空内存世界，监听 `127.0.0.1:7777`，停止时不执行文件保存。
