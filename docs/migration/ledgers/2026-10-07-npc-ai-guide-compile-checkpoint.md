# NPC AI Guide CS8156：主会话编译复核

日期：2026-10-07（Asia/Shanghai）  
状态：当前 NPC 项目 compile passed；Guide 完整行为与网络服务器整体构建未据此验收

网络 root 的生产构建 59 观察到 `NpcGuideSourceProfile.cs:169` CS8156：
`effectPort.TryForceSitting(in result.ForceSittingRequest)` 试图将属性按引用传入。
主会话读取当前文件时，Guide owner 已在该调用点使用局部值：

```csharp
NpcGuideForceSittingRequest sittingRequest = result.ForceSittingRequest;
forceSittingCommitted = effectPort.TryForceSitting(in sittingRequest);
```

该适配只改变合法传参方式，保留 sitting 请求值、调用顺序和布尔提交结果。主会话没有
覆盖 owner 的其他源码，而是对该实际源码执行受影响项目增量构建：

`dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:minimal -m:1`

结果 exit 0、0 warnings / 0 errors。产物为
`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`，SHA-256
`143CC6B9B312A69D8573B20DD75382171390AC3E0BEF9B5B80714246A223B398`。
Guide profile 构建前后 SHA-256 均为
`9837DBBF350CB1572D2E9AD02417BF8D4953CD949CD7767FFCB45EF17C3204AC`。

精确命令参数、原 build log、exit-code、产物/源码指纹在
`Build/diagnostics/NpcAiRedesign/runs/guide-cs8156-root-verification-20261007/`，
汇总为 `npc-build-result.json`。

这项复核只证明所述 CS8156 在当前 NPC 项目已消除；没有把旧服务器 DLL 当成当前
服务器构建成功，也没有把先前 Guide verifier/Simulation 报告套到新 sitting API。
Guide owner 继续行为 verifier 与来源/宿主闭包，网络 owner 自行复验其整体生产构建。
后续该源码再变化时，以新构建证据为准；原构建 59 的失败记录继续保留。
