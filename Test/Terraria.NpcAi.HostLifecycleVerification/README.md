# NPC 宿主任务结束验收

本程序链接生产 `RuntimeNpcEntity`、`RuntimeNpcStore` 和稳定宿主源码，通过正式
`WorldGen.serverLoadWorldCallBack` 加载输入世界，然后运行生产 `NpcTaskLifecycleProbe`。
不复制 NPC owner、任务规则或实体清理实现。

覆盖任务替换的旧引用拒绝、借用期间释放拒绝、Transform 后 WorldUnload 保留首次结束原因、
同槽复用、普通释放、真实投射物致死父子清理、store Reset 和重复 Dispose。另通过真实
`RuntimeNpcStore` 对 Mother Slime life-root 执行致死命中，分别验证 SinglePlayer/Server 创建
分裂子 NPC、Server 对每个子 NPC 请求同步、Client 不创建子 NPC，并确认生命根和附属实体
结束。输入世界只读，运行前后校验 SHA-256；JSON 保留执行程序集哈希和运行时源文件哈希。

```powershell
dotnet restore Test/Terraria.NpcAi.HostLifecycleVerification/Terraria.NpcAi.HostLifecycleVerification.csproj --nologo
dotnet build Test/Terraria.NpcAi.HostLifecycleVerification/Terraria.NpcAi.HostLifecycleVerification.csproj --no-restore --nologo -v:minimal
dotnet run --project Test/Terraria.NpcAi.HostLifecycleVerification/Terraria.NpcAi.HostLifecycleVerification.csproj --no-build --no-restore -- <world.wld> <report.json>
```

共享依赖源码正在并行变更时，可以在受影响 NPC 领域项目先构建通过后，使用
`-p:BuildProjectReferences=false` 构建本验收程序，以已经存在的依赖产物验收当前生产宿主
源码。该模式必须在证据中明确标注；它不证明当前所有依赖源码能一起构建。完整 Simulation
项目的构建、`Program` 编排和回退验证仍须独立复核。

这项验收不执行来源 AI golden、真实服务器封包/复制、实际 NPC 类型变换、目标/呈现的完整
清理或保存恢复。Server 用例只证明生产宿主创建子实体并提交同步 intent，不证明网络传输。
Transform 用例直接提交任务结束边界；不得把它登记为实际类型变换已经实现。结束原因的
精确协议值由领域 verifier 独立验证。后续异步任务推进协议在
`Terraria.NpcAi.LifecycleVerification` 中验收。

2026-10-07 续批已接入四个引用绑定 adapter，并在本程序的 `ReferenceOperations` 中验证同 kind
重启后旧操作拒绝、有效操作推进、无效原因不变以及重复完成/失败拒绝。Guide 的真实 tick /
home-return 路径由主 Simulation 的七条场景独立复核，证据在
`Build/diagnostics/NpcAiRedesign/runs/task-reference-host-20261007/`。
