# 当前 NLTX 世界宿主的真实客户端测试

使用 `D:/TRbackup/客户端` 中改造后的 Terraria 客户端，对 `NSSLC.Tools.NetworkServer` 做
loopback 测试。宿主读取真实 `.wld` 文件；客户端在 `CreateNoWindow=true` 的独立进程中运行，
不创建游戏实例或图形设备。

| 场景 | 结果 | 证据 |
| --- | --- | --- |
| `world-request` | 通过：客户端完成包 1、3、6、7 世界元数据往返，槽位 0 | `ProductionHost/Results/world-request.json` |
| `world-player` | 通过：客户端解析 19 个区段，处理包 49 并发送出生包 12，服务端以包 129 确认；随后发送 207 帧人物控制，进行长距离移动、跨区段请求，并完成一次物块破坏与区段刷新 | `ProductionHost/Results/world-player.json`、`world-player.server-report.json`、`world-player.process.json` |
| 世界值校验 | 客户端值与宿主快照相同：`RoundTrip 12345`、ID `1904902962`、`4200×1200` | `ProductionHost/world-request.process.json`、`world-player.process.json` |
| 长距离移动 | 客户端脚本移动 9600 像素（600 tile），跨越 3 个区段并请求 3 个远端区段；服务端按槽位 0 记录 207 帧，识别向右和跳跃控制 | `ProductionHost/world-player.process.json`、`world-player.server-report.json` |
| 物块破坏与刷新 | 客户端请求破坏坐标 `(2708, 264)` 的类型 147 普通物块；服务端内存覆盖层将其标记为空并发送 tile 更新。客户端收到更新后重新请求该区段，收到的区段数据仍显示该位置为空 | `ProductionHost/world-player.process.json`、`world-player.server-report.json` |
| 区段同步 | 客户端累计解析 19 个区段；长距离移动触发 3 次远端区段请求，服务端返回区段及 tile 数据 | `ProductionHost/world-player.process.json`、`world-player.server-report.json` |
| 无界面与清理 | 两场景均 `CreateNoWindow=true`、`WindowSeen=false`，客户端和宿主退出码都为 0；客户端确认 `Main.instance` 与图形设备为空 | 对应 `*.process.json` 和结果 JSON |

世界数据来自已生成并写入 `.wld` 的 RoundTrip world。宿主通过
`WorldFileDocumentDecoder` 读取持久化 world document，并从 tile section 构造 packet 10。
客户端使用原版 `MessageBuffer` 解包 packet 7 和 packet 10。测试模式在收到包 7 的尺寸后
分配地块数组和 section manager；地图更新关闭以绕过未启动的地图渲染器。每个测试帧使用
精确长度的接收缓冲区，避免原版 DeflateStream 读取单帧后的缓冲区填充。

## 当前能力边界

宿主提供生产 `PlayerSlotSessionAuthority`：槽位按空闲顺序分配，断开后精确释放，连接共享
一个稳定的游戏会话 ID；如果设置 `NLTX_SERVER_PASSWORD`，握手密码由准入端验证。当前
production host 启用了包 6 世界数据、包 8 初始区段、包 12 出生确认、包 13 人物控制和
包 159 额外区段请求；本次还验证了包 17 普通物块破坏请求和包 20 tile 更新同步。

人物场景发送脚本化位置、速度和按键帧，服务端验证坐标范围并记录控制数据；它没有运行
游戏循环或权威物理模拟。服务端支持本次验证覆盖的普通物块破坏请求，并在内存覆盖层更新
地块、清除区段缓存后同步变更；此测试不会把变更写回 `.wld` 文件，也不代表已实现完整玩法
服务器或地块放置流程。NPC 和弹幕 AI 均不属于本次验证，AI 系统暂不实现。

测试客户端只在 `-networktest` 场景中略过 `Player.Spawn` 依赖的游戏内容初始化，并略过包
129 的 UI 状态更新；包 49、12、129 的网络收发仍经过原版客户端发送器和解析器。该测试夹具
不会启动 Terraria UI。

协议输入只为当前已开放的握手、世界同步、出生、人物控制和普通物块破坏路径初始化。需要
NPC life-width、projectile UUID 等资料的其他 packet families 没有处理器且在准入层默认拒绝；
不能把这个窄范围 host 当作完整 Terraria server 部署。

## 复现

仓库根目录运行：

```powershell
& './Test/NSSLC.Infrastructure.Network.Verification/Invoke-NetworkServerRealClientProbe.ps1'
```

脚本默认使用
`Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld`；可通过
`-WorldFile` 指定其他受支持 WorldFile。脚本构建真实客户端与网络宿主，要求默认端口空闲，
只监听 `127.0.0.1:7777`，运行两个 headless 客户端场景并等待会话清理及宿主有序退出。
需要兼容原有脚本入口时，可加 `-ProbeCurrentServer` 调用同一探测。

结果位于 `Build/diagnostics/CurrentServerProbe/ProductionHost/`：

- `production-server-report.json`：两场景结果、世界值比较、退出码、区段和控制记录。
- `verification-commands.json`：项目、构建命令、退出码、警告/错误计数和产物路径。
- `Results/`：真实客户端 JSON 结果。
- `*.server.stdout.log`、`*.server.stderr.log`、`*.client.*.log`：两端进程日志。

本次探测最后一次客户端和宿主构建均退出 0，均为 0 警告、0 错误。此前客户端完整源码编译
曾输出 82 条既有源码警告。产物位于 `D:/TRbackup/客户端/Build/bin/Terraria/Debug/net40/` 和
仓库 `Build/bin/NSSLC.Tools.NetworkServer/Debug/net10.0/`。

2026-10-06 B1 复跑证据位于 `Build/diagnostics/CurrentServerProbe/B1-20261006-01/`；
两场景均由最新生产 NetworkServer 构建启动，客户端和宿主退出码均为 0，且保持
`CreateNoWindow=true`、`WindowSeen=false`。
