# 数据包编译器与网关接入

2026-10-05 从 `D:/ProjectItem/SourceCode/Net/NetWork/src/` 复制当前数据包定义、
设计编译器、wire transport 和成员生成器，共 361 个源文件；原目录保持不变。
Compiler/schema 为 17，协议目录为 TerrariaV4/4，握手标识为 Terraria319。
包字段直接定义在消息名称对应的类型中，例如 `HelloPacket.Version`、
`PlayerControlsPacket.Player`、`NetModulesPacket.Data`。

当前阶段 AI 系统暂不实现，不纳入本次交付与验收范围。本次只完成数据包编译、
网关接入和相关网络链路验证；测试中的记录式 owner 不代表游戏 AI 行为已实现。

| 位置 | 职责 |
| --- | --- |
| `DataPacketDefinition/` | 165 个包布局及实际 wire codec，目录说明见其 README |
| 根目录的 C# 文件及 `PacketDesignCompiler.Core.csproj` | 设计检查、图与 codec 源码生成 |
| `Generator/` | 编译期间生成 Members 与函数声明 |
| `PocketSourceFile/` | 166 份协议生成源码、1 份网关 binding 源码及输出清单 |
| `../../../../Build/Tools/NetworkCodecGenerator/` | 读取本地 manifest 并生成 codec/binding 的命令入口 |

`NSSLC.Infrastructure.Network.csproj` 及生成工具均只引用本仓库内的项目，不再依赖
外部 NetWork 目录。网络项目构建时先构建设计编译器与定义，再生成并编译
`PocketSourceFile/*.g.cs`。生成器只按 `network-codec-manifest.txt` 清理自己的旧文件。
此目录按本次用户要求作为生成 C# 的输出例外；DLL、obj、Roslyn 输出和日志仍在
仓库 `Build/` 下。`Generator` 保留 `netstandard2.0`，作为 Roslyn analyzer 的宿主兼容
例外；生产网络、数据包定义、设计编译器及验证项目均为 `net10.0`。

运行进程由宿主先构造一次 `ProtocolInputs`，再创建
`new ProtocolFacts(ProtocolInputs.Instance, version)` 和网络宿主。
生成 reader/writer 直接访问该进程唯一的输入模型；未初始化或重复初始化会失败。
一个进程不能使用多套协议表或不同的 `IsServer` 值。表和委托由调用方提供并保持稳定；
编译工具的占位输入仅用于验证模型形状，实际值不会写入生成源码或作为生产默认值。
模型初始化参数与网关组合方式见[宿主接入说明](../Design/2026-10-03-network-host-integration.md)。

构建与验证：

```powershell
dotnet build Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-restore --nologo -v:minimal
dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore
dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore -- --pocket-wire
```

`--pocket-wire` 在独立进程按上游夹具的输入值检查全部 165 种布局、167 个既有字节样例，
并通过正式网关 binding 检查编码字节和解码往返。样例来自结构重构前的实现；
其通过不代表真实 Terraria 客户端的全部协议分支已验证。
默认验证涵盖网关、连接、缓存、重连、真实 TCP 收发和弹幕 owner 接入。
没有默认开放新增游戏消息；业务处理器仍须由宿主明确注册。

本次验证结果：构建退出 0，0 警告/0 错误；默认验证 58 组通过、0 组失败；
独立字节样例验证 167 个通过，覆盖全部 165 种布局。361 个复制文件在复制时的 SHA-256
与上游文件一致，生成清单与 167 份 `.g.cs` 完全对应。
后续真实客户端联调修正了本地 `NetModulesPacket.cs` 和
`Packet82KnownModuleCodecsV4.cs` 中 module 2 的载荷：Ping 使用两个坐标浮点数，
不是空载荷；原上游目录保持不变。网关保留坐标回显，新增截断、空载荷和尾部字节拒绝验证。
真实 Terraria 客户端无界面运行的 8 个场景已通过，详见
[真实客户端测试说明](../Design/2026-10-05-headless-real-client.md)。
该验证覆盖明确列出的网络分支，不代表完整游戏入服或生产游戏 owner 已完成。

完整复制哈希、构建/验证命令与退出码、MSBuild 输出属性及结果保存在
`Build/diagnostics/PocketIntegration/`。
回退本次接入时恢复网络与生成工具的项目引用、类型调用及对应测试；只移除本次复制
的 Pocket 文件和新增夹具，不回退工作区其他任务的修改。
