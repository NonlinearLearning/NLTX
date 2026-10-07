# NPC AI G3/A4 来源行为差分检查点：Eye 与 Slime 有限 profile

文档 ID：DOC-2026-10-07-NPC-AI-G3-REFERENCE-DIFFERENTIAL  
逻辑域：system-decomposition  
产物类型：evidence  
状态：active  
范围：G3/A4 的可重复真实来源输出差分；Eye of Cthulhu `type=4/netID=4/aiStyle=4` 冲刺切片、Blue Slime `type=1/netID=1/aiStyle=1` 与 Mother Slime `type=16/netID=16/aiStyle=1` 的有限移动切片  
证据入口：[差分 verifier 与运行脚本](../../Test/Terraria.NpcAi.ReferenceVerification/README.md)、[机器报告](../../Build/diagnostics/NpcAiReferenceVerification/20261007T072353828Z/differential-report.json)、[来源构建 provenance](../../Build/diagnostics/NpcAiReferenceVerification/20261007T072353828Z/source-build-provenance.json)、[Eye profile](../../src/NSSLC/Component/Npc/NpcEyeOfCthulhuProfile.cs)、[Blue Slime profile](../../src/NSSLC/Component/Npc/NpcBlueSlimeProfile.cs)、[Mother Slime profile](../../src/NSSLC/Component/Npc/NpcMotherSlimeProfile.cs)、[F1 checkpoint](../migration/ledgers/2026-10-07-npc-ai-f1-eye-of-cthulhu-checkpoint.md)  
canonical 路径：docs/system-decomposition/2026-10-07-npc-ai-g3-reference-differential.md

本检查点证明来源行为切片可以从只读参考产物和 pinned 源码副本重复采样，并记录与生产 profile 的实际差异。2026-10-07 的最新 32 场景/182 tick 运行确认 Eye、Blue Slime 与 Mother Slime 已建模状态和速度均匹配；仍有 3 个 `netUpdate` 可观察差异，来自 profile 尚未建模的 `directionY` 目标朝向同步。它不声明这些有限 profile、完整 NPC AI 或 F1 已实现原版等价。

## 2026-10-07 最新来源差分

从仓库根目录执行 `pwsh -NoLogo -NoProfile -File Test/Terraria.NpcAi.ReferenceVerification/Invoke-ReferenceDifferential.ps1`。使用 PowerShell 7 可避免 Windows PowerShell 5 将中文只读参考路径按本地 ANSI 代码页解码的问题。脚本将参考树复制到 `Build/NpcAiReferenceVerification/`，只在副本内修正 6 个过期 DLL `HintPath`；原参考树与其 `TerrariaServer.exe` 均保持只读。

| 项目 | 结果 |
| --- | --- |
| 来源身份 | 1609 个输入文件 manifest `ccf14390f8d1f40a90318727975e0092d544ef8be6a170d0e45b03e774d80d16`；只读服务器 EXE SHA-256 `cf30ebda6839e9ff9556f3ba6cd84b18385cd3283e5462745baa57d1548b669a`，运行前后不变 |
| 隔离参考源码构建 | SDK `10.0.400`、`net40`、x86；exit 0，85 warnings / 0 errors；EXE `Build/bin/NpcAiReferenceVerification/SourceServer/Debug/net40/TerrariaServer.exe` |
| 差分 verifier 构建 | exit 0，0 warnings / 0 errors；构建期记录的 verifier/profile 源哈希与运行时哈希一致 |
| x86 reference harness | C# 编译 exit 0；本次 harness 源文件 SHA-256 `4335c2c350ef7d6ebc9b401e70777ba69f0719831d7ecd42563892d095e686e0` |
| 来源关联 | 原 EXE 与隔离源码构建的 `NPC.AI()` 和 `AI_001_Slimes()` IL SHA-256 分别一致；32 场景/182 tick 两次来源采样逐字段相同 |
| 生产 profile 比较 | 32 场景/182 tick；状态字段差异 0、速度差异 0、效果/可观察字段差异 3；报告状态 `comparison-complete-with-profile-differences`，门禁 exit 2 |

3 个差异均为首次 `TargetClosest` 后的 `netUpdate`：Blue Slime 湿态上升 1 项、Mother Slime 首次地面初始化 1 项、Mother Slime 湿态上升 1 项。目标位于 NPC 上方，原版将 `directionY` 从 1 改为 -1；`SetTargetTrackingValues` 在方向改变且未碰撞时设置 `netUpdate=true`。真实更新循环会先捕获 `oldTarget`、`oldDirection` 和 `oldDirectionY`，因此来源 harness 已在每次直接调用 `NPC.AI()` 前按当前实例状态写入这三个历史值。修正前 harness 每 tick 都保留构造期历史值，曾产生 11 个错误的重复同步差异；旧报告保留为诊断历史，最新报告才是当前差异结论。

Slime profile 的当前切片不接收或返回 `directionY`，TargetClosest owner 的状态提交也未纳入这次 profile 组合比较。因此这 3 项是尚未闭合的 owner/profile 边界差异，不能提升为 verified；建议后续由 B2 target owner 提供方向提交结果和网络同步意图，再在相同来源输入下重跑差分。完整目标策略、碰撞门控、网络 authority、所有 Slime 类型和完整生命周期仍未覆盖。

机器报告 SHA-256 为 `1944e87378d47421f4037aa7c5c945a0db8e7fd8409ba2a15bda85679cfac7b3`，输入、capture、构建日志、binlog 与 provenance 位于 `Build/diagnostics/NpcAiReferenceVerification/20261007T072353828Z/`。

## 首次 Eye-only 试采样（历史）

## 来源身份与隔离执行

只读参考根为 `D:\TRbackup\无任何删减通过编译`。运行前后核验的 `Terraria/NPC.cs`、`Terraria/Main.cs`、`Terraria.ID/NPCID.cs` 指纹与 pinned 值一致；参考树 1609 个输入文件的 manifest SHA-256 为 `ccf14390f8d1f40a90318727975e0092d544ef8be6a170d0e45b03e774d80d16`。只读 `TerrariaServer.exe` SHA-256 运行前后均为 `cf30ebda6839e9ff9556f3ba6cd84b18385cd3283e5462745baa57d1548b669a`。

为确认二进制与 pinned 源码的关联，脚本把参考树复制到 `Build/NpcAiReferenceVerification/`，只在该隔离副本内重定向六个过期 DLL `HintPath` 并设置仓库 `Build/` 输出目录。原参考树没有被修改。副本以 SDK `10.0.400`、`net40` 构建，输出到 `Build/bin/NpcAiReferenceVerification/SourceServer/Debug/net40/TerrariaServer.exe`。

| 身份对象 | SHA-256 |
| --- | --- |
| pinned `Terraria/NPC.cs` | `ed8aa2302730a9e046ef310e543204fba391bd35b1c9c0b213c8065dfbbe39f0` |
| pinned `Terraria/Main.cs` | `e24e61c9903bb7995f47e36c51edbe43481b643226861b717020877e7e22b63f` |
| pinned `Terraria.ID/NPCID.cs` | `e040b9cfffd57842c0099f11322ee5ec1dddd605743a25bae5415db31b0c025d` |
| 1609 个参考树输入的 manifest | `ccf14390f8d1f40a90318727975e0092d544ef8be6a170d0e45b03e774d80d16` |
| 只读参考 `TerrariaServer.exe` | `cf30ebda6839e9ff9556f3ba6cd84b18385cd3283e5462745baa57d1548b669a` |
| 隔离源码构建 `TerrariaServer.exe` | `7c00157f2b14b780acf1bceae336cace7ae6ace29d479ce67f0dd1b8f6daf185` |
| 两个程序集的 `NPC.AI()` IL | `6269e82f828e8ad9f7c1f371aeb3cf8bb3f4bde65864ee2f925a9fb4c2048bcc` |

x86 harness 通过反射直接调用 `Terraria.NPC.AI()`，没有启动服务器入口或交互窗口。AI 调用前禁用了 `CallTracker` 的 flush timer，并把 `Program.SavePath` 指向本次 diagnostics 下的 sandbox。选取的随机种子每 tick 都产生非零 `Next(5)`，避开 `Dust.NewDust`；原二进制和隔离构建的 sandbox 均为空。

## 可重复命令和构建记录

从仓库根目录运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Test/Terraria.NpcAi.ReferenceVerification/Invoke-ReferenceDifferential.ps1
```

| 项目 / 命令 | 结果 | 输出 |
| --- | --- | --- |
| Verifier restore：`dotnet restore Test/Terraria.NpcAi.ReferenceVerification/Terraria.NpcAi.ReferenceVerification.csproj --verbosity minimal` | exit 0 | `verifier-restore.log` |
| Verifier build：`dotnet build Test/Terraria.NpcAi.ReferenceVerification/Terraria.NpcAi.ReferenceVerification.csproj --no-restore --configuration Debug --nologo -v:minimal` | exit 0；0 warnings / 0 errors | `Build/bin/Terraria.NpcAi.ReferenceVerification/Debug/net10.0/Terraria.NpcAi.ReferenceVerification.dll`；`verifier-build.log` |
| 隔离来源 build：`dotnet build <Build/NpcAiReferenceVerification/ReferenceSource-<runId>/TerrariaServer.csproj> --no-restore --configuration Debug --nologo -v:minimal -bl:<diagnostics>/reference-source-build.binlog` | exit 0；85 warnings / 0 errors | `Build/bin/NpcAiReferenceVerification/SourceServer/Debug/net40/TerrariaServer.exe`；`reference-source-build.log` 与 binlog |
| x86 harness compile：`C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /nologo /target:exe /platform:x86 /r:System.Web.Extensions.dll /out:Build/bin/NpcAiReferenceVerification/ReferenceHarness/ReferenceHarness.exe Test/Terraria.NpcAi.ReferenceVerification/ReferenceHarness.cs` | exit 0 | `Build/bin/NpcAiReferenceVerification/ReferenceHarness/ReferenceHarness.exe`；`reference-harness-compile.log` |
| Harness 捕获：`ReferenceHarness.exe --assembly <original-or-isolated-exe> --cases <differential-cases.json> --output <capture.json> --sandbox <sandbox>` | 两次运行均 exit 0 | `original-executable-capture.json`、`isolated-source-build-capture.json` 及对应日志 |
| 原始 EXE 与源码构建 `NPC.AI()` IL | SHA-256 均为 `6269e82f828e8ad9f7c1f371aeb3cf8bb3f4bde65864ee2f925a9fb4c2048bcc` | provenance 与差分报告 |
| 参考输出关联检查 | 24 个场景、36 个 tick 的两份参考采样一致；来源关联成立 | `differential-report.json` |

本次运行的完整输入、捕获、日志、哈希、binlog 和报告位于
`Build/diagnostics/NpcAiReferenceVerification/20261006T195702981Z/`。

输入/输出身份：case 输入 SHA-256 为
`c1e8e61215f0a2a720c352b879da82fa27fa3fb47856c547df5d1c7cc8ba1ec4`；链接进 verifier 的生产
profile 源文件集 manifest 为
`f2b3c7730edd8c55ce894001682102828738c4b75d46d0b5b1a66d39d9fd8af8`。原 EXE 捕获 SHA-256 为
`485a86abdc2f252a6c935e664ff9b80d638808b526afc94cce1919770f8c0964`，隔离源码构建捕获为
`e862924cd326f8d6c330c852ebf010fef43e382beeefbf7ebecdc9229a763940`；provenance 为
`ec5a686fcf4f398c810d95ea3917cd02d9e4349558460dfe1428a4df054c4eb5`，差分报告为
`092654c7f1e7a808c6412ceab6ecd2cde3773acb4ad5e6d1ba32857d85a19b6f`。

## Profile 差分结果

样本固定为 `ai=[0,2,39,0]`，初始速度 `(2.75,-1.125)`，在普通、Expert、Expert+GetGoodWorld
模式下各推进 5 tick；另有三种模式各 7 个围绕严格 `±0.1` clamp 的相邻浮点边界输入。profile
通过项目引用链接生产 `NpcEyeOfCthulhuProfile` 源码，没有在 verifier 中复制算法。

| 比较项 | 结果 |
| --- | --- |
| 场景 / tick | 24 / 36 |
| `ai[0..3]` 状态字段差异 | 0 |
| 速度差异 | 9 个值；均出现在 Expert 或 Expert+GetGoodWorld 多 tick 场景；最大绝对差 `2.3841858e-7`，最大 4 ULP |
| 单 tick `±0.1` clamp 边界 | 全部匹配 |
| profile 请求及已记录观察字段 | 0 个差异；本组没有触发 dust、dash 起始、目标重选或 network update，effect-port 调用列表为空 |

速度差异与浮点运算次序一致：profile 先合并阻尼因子再乘一次速度，参考分支按难度/世界条件
对速度依次原位相乘。普通模式多 tick 样本与边界样本相同。报告保留每个差异 tick 的十六进制
浮点位型；此处不扩大容差，也不将这些差异记为相等。

## 证据边界与后续

报告在该切片中还记录了 position、localAI、rotation、`reflectsProjectiles`、target、生命值、
难度和世界条件。position/localAI 在采样期间保持不变，但不属于 profile 输出契约；rotation
没有建模；参考 AI 对 `reflectsProjectiles` 的写入没有对应 profile 字段。harness 未运行 AI
之后的宿主位置积分；由于所有随机 roll 都非零，也没有执行 dust 渲染。该批没有触发需要验证的
正向 effect 调用顺序。

因此，本证据只完成了 G3/A4 的第一条可重复真实来源多 tick 差分，并确认原二进制的
`NPC.AI()` IL 与 pinned 源码隔离构建一致。9 个 Expert 速度差异、未建模状态/效果、完整 AI
调度、目标策略、Boss 后续阶段、authority/network、保存/卸载和完整 F1 来源 golden 仍开放；
F1 仍是 partial，任何 coverage stage 均未据此提升为 verified。
