# NPC AI C1 / C1.3 cooldown：主会话宿主验收

日期：2026-10-07（Asia/Shanghai）  
状态：cooldown 规则与有限宿主回归 passed；完整来源闭包 partial/open

Slime 会话修复 Blue/Mother 的 `ai[2] > 1` 递减，保持它位于 `ai[0] == -999` 早退之后、
湿态处理之前；普通路径为 `3→2→1→1`，冻结哨兵保留 3。Blue 的 TargetClosest 端口现在
允许提交 target-selection result 的条件 network intent，仍只在结果声明请求时提交，
没有把 target reacquire 改成无条件网络请求。

owner 的完整命令、源码依据与精确回退记录在
`Build/diagnostics/NpcAiSlimeBehavior/20261007T-current/checkpoint.md`。主会话读取原报告和
构建日志，并独立重跑四条宿主场景及 NPC AI verifier；本记录仅验收该切片。

## 实际执行与绑定产物

owner 的 NPC verifier build exit 0、0 warnings / 0 errors；正常完整依赖 Simulation build
exit 0、17 warnings / 0 errors。命令均使用受影响项目、`--no-restore --nologo -v:minimal`，
并附 `-m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
-p:BuildInParallel=false`；两份日志保留在 owner diagnostics。

主会话执行的 Simulation DLL 为
`Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`，SHA-256
`8D1994A41D4728E4963B0EA524D416FE86C79268FC4354988ED8E4F80DE5BC50`；其实际 `Terraria.Npc.dll`
SHA-256 为 `6A54C035E13F46C0FBBD5EFD52763A8E8A634264CA07A12696BA336CA2576192`。
后续并行修改或构建不能沿用这次通过结论。

运行命令为 `dotnet <Simulation.dll> <generated.wld> <1|120> --players 1 --spawn-npc <1|16>
--report <json>`。主验收的每条精确 args、log、exit-code 与原报告保存在
`Build/diagnostics/NpcAiRedesign/runs/slime-cooldown-root-review-20261007/`。

| 主会话重跑 | `ai[0,1,2,3]` | direction | network intent | 结果 |
| --- | --- | ---: | --- | --- |
| Blue 1 tick | `[-99,-1,1,0]` | -1 | true | exit 0 |
| Blue 120 ticks | `[-1106,-1,1,0]` | 1 | false | exit 0 |
| Mother 1 tick | `[-99,0,1,0]` | -1 | true | exit 0 |
| Mother 120 ticks | `[-1086,0,1,0]` | 1 | false | exit 0 |

全部 `Succeeded=true`、末 tick NPC 更新完成，输入世界未修改。主会话另执行
`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj
--no-build --no-restore`，exit 0、PASS，包含两种 Slime cooldown 与冻结边界断言。
运行期间两个实际程序集、输入世界、Blue/Mother profile 与 RuntimeNpcStore 的前后指纹
一致；`acceptance-result.json` 保存通过结果与这些指纹。

## 结论边界与回退

Blue 首 tick 同时有体内物品初始化，因此 network intent 不能单独归因于 TargetClosest。
Mother 首 tick 的方向和条件 network intent 可观察，但报告没有通用 target slot 字段；
完整目标状态须由 B2 owner 的实际组件/宿主 verifier 验收。

本批没有重跑 32 场景/182 tick source differential，也没有重跑完整 37-evidence Simulation
大回归；不能宣称旧报告的 8 个状态和 11 个 observable 差异已全部关闭。来源比较器的
独立拒绝路径正按[主验收发现](2026-10-07-npc-ai-source-gate-root-acceptance.md)修复；普通/WOF
目标公式也有[已复现的来源反例](2026-10-07-npc-ai-b2-target-root-review.md)。

体内变体、authority/random stream、完整 Tile/collision、Mother death split、save/unload
和其他来源 profile 继续 open。coverage stage 均不升级。Slime 会话已继续实现后续切片，
不会把本宿主回归通过当成完整 Slime 目标完成。

精确回退只撤销新增 cooldown/branch/assertion 与 Blue 端口本批的条件提交启用；保留
其他 profile、宿主分派、task/identity 与并行 owner 改动，不删除任何历史证据。
