# NPC AI F1 夜间悬停与三次冲刺：主会话验收

日期：2026-10-07（Asia/Shanghai）  
状态：夜间开场及三次冲刺的有限宿主切片 passed；完整 F1 partial/open  
计划：[NPC AI 执行文档](../../plans/system-decomposition/2026-10-06-npc-ai-system-execution-plan.md)

本记录验收精确身份 `type=4 / netID=4 / aiStyle=4` 的夜间开场。Eye 会话提供八条真实
Simulation 运行，主会话读取原报告、输入条件和程序集指纹，并独立补跑四条第二/第三次
冲刺边界。十二条运行都是新加载同一输入世界后从初态推进到指定 tick 的独立重放，
不是一条逐 tick 全量 trace，也不证明完整原版更新闭包或全部来源效果顺序。

## 运行条件与构建归属

输入世界为 `Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld`，
SHA-256 为 `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241`。
运行参数固定单玩家、seed `1313625176`、中性输入脚本、`--spawn-npc 4`、
`--world-time-rate 0` 和 `--npc-eye-scenario classic-night|expert-night`。
报告确认初态/末态均为夜间；场景在加载后显式配置 Classic 或 Expert，secret seeds 为 None。

复用主会话 task-reference 批次正常构建的 Simulation 程序集：

- 项目：`src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj`。
- 命令：`dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal`。
- 结果：exit 0，17 warnings / 0 errors。
- 日志：`Build/diagnostics/NpcAiRedesign/runs/task-reference-host-20261007/simulation-build.log`。
- 产物：`Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`，
  SHA-256 `80C0C8180C00E99D58044C26F8EEA023D9AB06FBA6ED17B58A96EAE38134D02A`。
- 实际依赖 `Terraria.Npc.dll` SHA-256 为
  `6034B227BDA3ADFB737A1C47103E3F8F265634652C18893777EDE4BD71683D63`。

Eye 自己目录中的 `simulation-build.log` 曾因并行 rollback 文件的两个类型歧义失败，
该失败保留。八条 night 运行随后使用上述已成功构建产物；不能把失败日志标成成功。
源码共享工作区随后仍有并行修改，本记录绑定实际执行的程序集与 Eye profile 指纹，
不把运行结果套用于后续尚未重新构建的全部源码。

## 验收结果

| 场景 / 最终 tick | `ai[0,1,2,3]` | 观察结果 |
| --- | --- | --- |
| Classic 599 | `[0,0,599,0]` | 尚在悬停 |
| Classic 600 | `[0,1,0,0]` | 悬停计时结束；清目标；请求 network update |
| Classic 601 | `[0,2,0,0]` | 首次 `BeginDash`；速度 6；重选目标为玩家 0 |
| Classic 752（主会话补跑） | `[0,2,0,1]` | 第二次 `BeginDash`；速度 6 |
| Classic 903（主会话补跑） | `[0,2,0,2]` | 第三次 `BeginDash`；速度 6 |
| Classic 1053 | `[0,0,0,0]` | 三次冲刺后返回悬停；清目标 |
| Expert 209 | `[0,0,209,0]` | 尚在悬停 |
| Expert 210 | `[0,1,0,0]` | 悬停计时结束；清目标；请求 network update |
| Expert 211 | `[0,2,0,0]` | 首次 `BeginDash`；速度约 7；重选目标为玩家 0 |
| Expert 312（主会话补跑） | `[0,2,0,1]` | 第二次 `BeginDash`；速度约 7 |
| Expert 413（主会话补跑） | `[0,2,0,2]` | 第三次 `BeginDash`；速度约 7 |
| Expert 513 | `[0,0,0,0]` | 三次冲刺后返回悬停；清目标 |

十二条运行 exit 0、`Succeeded=true`，输入世界未修改。主会话补跑还断言实际
`EyeOfCthulhuVelocityX/Y`、`EyeOfCthulhuTargetSlot`、`BeginDash`、network intent、夜间条件
和全部四槽；速度误差阈值为 `0.00001`。边界 tick 的目标为 `-1` 是已声明的清目标语义，
不是要求所有场景始终保留玩家 0。

八条 owner 证据：`Build/diagnostics/NpcAiRedesign/runs/f1-night-dash-20261007/`，
包括原报告、exit-code、`scenario-validation.json`、构建失败和来源/输入/产物指纹。
四条主验收证据：`Build/diagnostics/NpcAiRedesign/runs/f1-night-dash-root-review-20261007/`，
包括每条精确 `args.json`、原报告、run log、exit-code、前后指纹和 `acceptance-result.json`。
主验收四条运行期间 Simulation/Npc 程序集、世界、输入脚本与 Eye profile 均保持不变。

Eye profile 使用的首次冲刺公式已经按来源修正为普通 6、Expert 7、GoodWorld 再加 1；
本次宿主条件仅覆盖 Classic/Expert 的 None seed，GoodWorld 仍需独立宿主验收。

## 未关闭与回退

本批不提升任何 style/profile 为 implemented 或 verified。变身阶段、仆从即时生成、
完整目标输入、authority/network、呈现、save/load/unload、完整 source golden 和删除门禁
继续 open。来源差分的当前 32 场景/182 tick 没有速度差异，但仍有 Slime 的 8 个状态与
11 个 observable 差异；该门禁仍返回 exit 2，不能据此宣布 F1 或全量来源等价。

回退本验收记录只撤销夜间切片的通过登记与中间边界证据引用。生产代码的精确回退由
Eye owner checkpoint 记录；不得删除历史失败、task reference 接线或其他并行 owner 的改动。
后续 Eye/profile/宿主源码改变后，按受影响边界重新构建和运行；旧报告继续保留为历史。
