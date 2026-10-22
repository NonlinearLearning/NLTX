# Verification：既有验证项目阅读指南

[返回总览](../README.md)

## 作用

此目录包含已经存在的可执行验证项目，用于阅读状态不变量、协议样例和跨系统协作。
各项目的 Program 或 Probe 会构造输入、调用领域或 Host、断言结果并输出证据。
它们是理解预期行为的补充，运行时接入范围仍以生产装配和路由为准。

本次任务仅编写阅读文档，没有运行这些项目。下面的覆盖主题来自源码，不是本次通过结果，
也不代表对所有原版行为的覆盖。

## 项目与阅读目的

| 目录 | 项目入口 | 适合阅读的主题 |
| --- | --- | --- |
| Communication | [项目](Communication/MinimalImplementation.Verification.Communication.csproj)、[Program](Communication/Program.cs) | TCP 回环、分帧和信封收发 |
| Protocol | [项目](Protocol/MinimalImplementation.Verification.Protocol.csproj)、[Program](Protocol/Program.cs) | 协议配置、编解码与格式边界 |
| P02 | [项目](P02/MinimalImplementation.Verification.P02.csproj)、[Program](P02/Program.cs) | 握手、密码、会话阶段和认证的 Owner 级场景 |
| World | [项目](World/MinimalImplementation.Verification.World.csproj)、[Program](World/Program.cs) | 世界候选、地形、区块及世界规则 |
| Inventory | [项目](Inventory/MinimalImplementation.Verification.Inventory.csproj)、[Program](Inventory/Program.cs) | 槽与代次、物品事务、归属和并发边界 |
| Combat | [项目](Combat/MinimalImplementation.Verification.Combat.csproj)、[Program](Combat/Program.cs) | NPC、弹幕、受击与战斗准备/提交 |
| Mechanisms | [项目](Mechanisms/MinimalImplementation.Verification.Mechanisms.csproj)、[Program](Mechanisms/Program.cs) | 图格实体、交互和拴系等机关规则 |
| Modules | [项目](Modules/MinimalImplementation.Verification.Modules.csproj)、[Program](Modules/Program.cs) | 模块分发、聊天、状态和输出协作 |
| ServerTick | [项目](ServerTick/MinimalImplementation.Verification.ServerTick.csproj)、[Program](ServerTick/Program.cs) | 阶段顺序、复合世界 Tick、入侵桥接和拴系 owner |
| Integration | [项目](Integration/MinimalImplementation.Verification.Integration.csproj)、[Program](Integration/Program.cs) | 运行时启动、加载、保存、停止与跨领域生命周期 |
| Integration / Packet85 | [独立项目](Integration/MinimalImplementation.Verification.Packet85Router.csproj)、[Probe](Integration/Packet85RouterProbe.cs) | 快速堆叠的 Host 路由组合 |

Integration 还包含 [RuntimeStartupProbe](Integration/RuntimeStartupProbe.cs)、
[CreativePermissionProbe](Integration/CreativePermissionProbe.cs) 和
[PlayerLuckRouterProbe](Integration/PlayerLuckRouterProbe.cs)。具体被哪个项目编译及如何选择
执行入口，应查看对应 csproj，不能从目录中的类名推断每次运行都会执行。

## 如何沿场景理解组件协作

1. 先读 Program 的参数与场景选择，确认本次构造的条件。
2. 找到 Fixture、Composition 或 Owner 创建处，核对内容事实、时钟、随机源和假端口。
3. 跟随准备、失效校验、提交和重复请求断言，理解状态不变量。
4. 再看输出意图、回执和快照确认，区分修改成功与交付成功。
5. 最后回到生产 Host，确认这些领域入口如何接入真实客户端和 Tick。

例如 [Sc041ChestConcurrencyScenario](Inventory/Sc041ChestConcurrencyScenario.cs) 适合看箱子
并发与事务边界；ServerTick 场景适合看一个事务如何跨 Time/World 两阶段只推进一次。
Owner 级验证使用的捕获事实或适配器可能与生产环境不同，需要继续核对装配。

## 参数、产物与历史证据

不同程序的 `--scenario` 和其他参数并不完全一致，以其 Usage 和解析代码为准。
产生文件的场景通常接受 `--output-root`，证据应放在仓库 `Build/diagnostics/` 下，
编译产物应遵循仓库 `Build/bin/`、`Build/obj/` 等输出约定。
执行前先阅读 [构建与验证约束](../../../Context/约束/构建与验证约束.md)。

[P06 阻塞记录](Mechanisms/P06-leashed-entity-closure-blocker.md) 和
[P07 实施记录](Modules/P07-implementation-progress.md) 是历史资料。
源码或装配后续可能变化，不能将这些文件中的旧状态直接当成当前服务器结论。

一次验证结论应带有场景、项目、命令、退出码和证据路径；原版客户端入服、显示与操作
结果还需要相应客户端证据，不能仅由内存断言替代。
