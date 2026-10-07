# 真实客户端无界面网络测试

真实客户端源码位于 `D:/TRbackup/客户端`。测试入口在 `WindowsLaunch.Main` 的程序集
解析器注册后、`Program.LaunchGame` 前识别 `-networktest`，运行结束直接返回。
测试不构造 `Main`，不调用游戏循环、绘制、Content 加载或 Steam 初始化。
每个场景使用独立进程，成功时检查 `Main.instance`、`Main.graphics` 均为 null。

本阶段 AI 系统暂不实现，不纳入交付与验收。测试不执行 NPC 或弹幕 AI，也不模拟
完整游戏世界；服务端测试 owner 只验证消息字段、可信玩家身份和阶段推进。

## 兼容与输出边界

真实客户端保留现有 `net40`、x86、XNA 依赖，这是本任务的明确兼容例外。
NLTX 网关及验证器仍使用 `global.json` 指定的 SDK 和 `net10.0`。
客户端 `Directory.Build.props` 将输出放到自身 `Build/bin/Terraria/`，中间文件放到
`Build/obj/Terraria/`，并排除旧 `bin/obj` 与新的 `Build/`，避免重复编译生成源码。
使用 `-p:NetworkTestBuild=true` 构建控制台程序 `Terraria.NetworkTests.exe`。
该构建定义 `NETWORK_TEST_BUILD`，未提供测试参数时直接退出 2，不会启动游戏 UI。
测试日志、结果和客户端状态路径位于 NLTX `Build/diagnostics/RealClientNetwork/`。

测试模式复用原 `NetMessage.SendData`、`MessageBuffer.GetData`、`NetManager` 和
`NetPingModule`。新增 `NetworkTestSocket` 只提供有截止时间的 TCP 完整帧读写，
不实现另一套游戏报文编码器。失败输出异常、JSON `success=false` 和非零退出码。
原发送器吞掉 transport 异常，因此测试额外核对实际发送帧数，避免把未发送当作成功。

客户端最小适配点：

- 测试入口先设置独立 SavePath，再经过禁止内联的方法边界访问 Main，避免 CLR 提前
  运行其静态初始化器时 SavePath 尚未设置。
- 测试模式网络诊断使用已有空诊断实现，避免访问游戏 UI 实例。
- packet 3 保留原槽位分配解析；普通网关场景仍跳过完整角色和背包批量上传以隔离阶段测试，`admission` 场景则允许原版 parser 发送完整角色、装备、背包和 loadout 初始化序列。
- Ping 原解析器增加测试坐标观察点，正常客户端仍执行原地图 Ping 行为。
- 世界场景在读取 packet 7 后初始化 tile 数组与 `WorldSections`，关闭地图渲染更新；接收
  packet 10 时将缓冲区限制到当前帧长度，保留原版解压和 tile 解析。
- `world-player` 让原解析器处理 packet 49、129；仅将依赖游戏内容的 `Player.Spawn` 替换为
  出生位置初始化并通过原版发送器发送 packet 12，packet 129 跳过 UI 状态更新。之后发送
  207 帧控制，沿水平方向移动 600 个 tile 并请求经过的 3 个区段；选择收到的区段中的普通
  tile，以原版发送器发送破坏包 17，再用原版解析器确认服务端包 20 的 tile 更新。接着重请
  同一区段并验证包 10 中 tile 仍为空，以覆盖宿主区段缓存失效路径。

## 使用

从 `D:/TRbackup/NLTX` 执行
[自动化脚本](../../../../Test/NSSLC.Infrastructure.Network.Verification/Invoke-RealClientNetworkTests.ps1)：

```powershell
& './Test/NSSLC.Infrastructure.Network.Verification/Invoke-RealClientNetworkTests.ps1'
```

脚本只在缺少 assets 时执行 restore，构建受影响项目后启动
loopback 网关和独立客户端进程。验证器使用 `CreateNoWindow=true`，并观察进程窗口句柄。
可用 `-ClientRoot` 指定客户端源码目录、`-OutputDirectory` 指定诊断输出目录。

单独验证无界面启动：

```powershell
& 'D:/TRbackup/客户端/Build/bin/Terraria/Debug/net40/Terraria.NetworkTests.exe' `
  -networktest startup `
  -testresult 'D:/TRbackup/NLTX/Build/diagnostics/RealClientNetwork/startup.json'
```

连接网关夹具的完整验证：

```powershell
dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore -- --real-client 'D:/TRbackup/客户端/Build/bin/Terraria/Debug/net40/Terraria.NetworkTests.exe' 'D:/TRbackup/NLTX/Build/diagnostics/RealClientNetwork/Results'
```

客户端参数为 `-networktest <场景>`、`-testresult <绝对路径>`，可指定 `-join <IP>`、
`-port <端口>`、`-password <密码>`、`-testtimeoutms <毫秒>`（默认 60000）。
`startup` 不连接网络；其他场景需要下述网关夹具，不能当作完整原版入服流程使用。
另有 `hello`、`world-request`、`world-section-request` 三个直接连接场景：前者在槽位
响应后退出，`world-request` 请求并解析真实包 7；`world-section-request` 使用窄范围网关
夹具验证未注册包 8 时的默认拒绝策略。针对本仓库生产 host 的包 8/10 同步和人物控制，
运行 `Invoke-NetworkServerRealClientProbe.ps1`，其 `world-player` 场景还验证包 49/12/129
入服流程、600-tile 路线、3 个区段请求、包 17 tile 破坏、包 20 更新以及破坏后的区段刷新。
宿主仅在内存 overlay 中提交单个附近普通 tile；测试不运行权威玩家物理、碰撞、tile 掉落、
保护逻辑或存档。世界数据场景不使用生命值 ACK。当前
服务端探测方式和结果见[服务端现状测试](2026-10-05-current-server-probe.md)。

## 验证范围

| 场景 | 验证内容 |
| --- | --- |
| startup | 最小静态状态初始化，无游戏实例和图形设备 |
| gateway | Hello/槽位解析，6/8/12 阶段确认，13 控制数据、16 生命、56 NPC 名称双向收发，27/29 弹幕 owner |
| admission | 原版 packet 3 后的 packet 4、5、16、42、50、68、147 与约 990 个 packet 5 槽位上传；网关按绑定槽位、阶段、数量和字节预算处理，并以 packet 16 确认 packet 6 阶段推进 |
| password-correct | 原客户端处理 37 并自动发送 38，继续完整 gateway 场景 |
| password-rejected | 错误密码后断开，服务端拒绝诊断且不创建玩家绑定 |
| default-deny | 未注册消息被拒绝，绑定和网络预算释放 |
| ping | 原客户端 module 2 的坐标序列化与解析，以及网关回显 |
| missing-arguments | 测试程序不带参数时直接退出 2，不启动游戏 UI |
| transport-timeout | 连接接受但不回应的 TCP 端，约 2 秒按截止时间失败，JSON success=false、退出码 1 |

阶段确认使用夹具的远端玩家生命响应作为 ACK；客户端收到确认后才发送下一阶段请求，
避免接收端在阶段尚未提交时拒绝后续消息。该 ACK 是测试夹具约定，不是完整世界同步。
弹幕测试保留线上的 owner=201，断言服务端应用端口收到可信绑定 owner=0，检查所有
已设置的选填字段；不触发弹幕生命周期或 AI。

## 实际结果与协议修正

2026-10-05 执行上述脚本，退出码 0。客户端和网关验证项目最终增量构建均为
0 警告/0 错误。前面的客户端源码编译出现 82 个现有源码警告，未调整或屏蔽这些
无关警告；增量构建没有重新编译其全部源文件。
DLL/EXE 的实际位置分别为客户端 `Build/bin/Terraria/Debug/net40/` 和 NLTX
`Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/`，MSBuild 已核对。

真实客户端 9 个场景通过（包含完整 admission 上传），网关 60 组回归通过，165 种布局的 167 个历史字节样例通过。
9 个进程记录均为 `CreateNoWindow=true`、`WindowSeen=false`；成功场景的客户端结果
确认游戏实例和图形设备为空。静默 TCP 场景在 2056 ms 失败，并以退出码 1 结束。
命令参数、工程、退出码、警告/错误数与输出路径保存在
`Build/diagnostics/RealClientNetwork/verification-commands.json`，各场景的 JSON、
stdout/stderr、进程窗口记录和网关诊断保存在 `Results/`。

真实客户端的 Ping 为 `ushort moduleId=2` 后接两个 float 坐标，总帧长度 13。
旧定义将其视为空载荷，真实收发复现 `TrailingBytes`（ID 82，body offset 2）；证据位于
`Build/diagnostics/RealClientNetwork/PingReproduction/ping.gateway.json`。
本地 Pocket 的模块数据类型与模块 codec 已修正，网关按坐标回显，生成与回归已通过。
该修正只修改本地副本，不改上游 NetWork 工作区；原始复制哈希用于记录复制时的基线。

尚未验证全部 162 个 ID 的真实客户端分支。源码检查发现 packet 69 的原客户端发送器
还写入箱子名称字符串，而当前 C2S 定义只读索引和坐标；本轮未启用该业务消息，
此兼容问题仍待后续单独复现和处理。没有发送完整世界或瓦片数据，因此当前结果不代表
完整原版入服、持续游戏仿真或吞吐压测完成。

原始客户端修改前备份及 SHA-256 位于 `Build/diagnostics/RealClientNetwork/Original/`
和 `client-original-hashes.json`；改造后的 10 个文件快照与哈希保存在 `Modified/`
和 `client-modified-hashes.json`。回退只恢复这些已识别文件、移除本次新增测试入口和
输出配置，不覆盖其他客户端改动或 NLTX 工作区变化。
