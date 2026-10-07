# NPC AI C1 / C1.3 source differential checkpoint

日期：2026-10-07（Asia/Shanghai）  
阶段：C 普通敌人和生物  
状态：固定来源身份与多 tick capture 已验证；Blue Slime / Mother Slime profile 对照仍为 failed，未提升 coverage 状态

## 范围

本 checkpoint 只对照 C1 Blue Slime 与 C1.3 Mother Slime 的有限 style-1 profile 切片。样本
包含四个 Blue Slime 场景和四个 Mother Slime 场景，共 146 ticks。完整 capture 批次还包含
24 个 Eye of Cthulhu 场景；这里不据此扩展或提升 Eye profile 状态。C2 / C3 source comparisons
尚未加入。

没有修改 production profile、canonical execution plan、coverage ledger 或 F1 checkpoint。

## 来源身份证据

只读来源树为 `D:\TRbackup\无任何删减通过编译`。来源文件指纹与固定索引一致：

| 来源文件 | SHA-256 |
| --- | --- |
| `Terraria/NPC.cs` | `ed8aa2302730a9e046ef310e543204fba391bd35b1c9c0b213c8065dfbbe39f0` |
| `Terraria/Main.cs` | `e24e61c9903bb7995f47e36c51edbe43481b643226861b717020877e7e22b63f` |
| `Terraria.ID/NPCID.cs` | `e040b9cfffd57842c0099f11322ee5ec1dddd605743a25bae5415db31b0c025d` |

完整只读来源 manifest 含 1,609 个文件，SHA-256 为
`f8eb8115173b0bda98577c5654542c3d63141a0ca8a8bf3e065e72eda3f3d12e`。原始
`TerrariaServer.exe` 的 SHA-256 为
`cf30ebda6839e9ff9556f3ba6cd84b18385cd3283e5462745baa57d1548b669a`，运行前后保持不变。

原始 EXE 与隔离源码构建产物的 `NPC.AI` IL SHA-256 均为
`6269e82f828e8ad9f7c1f371aeb3cf8bb3f4bde65864ee2f925a9fb4c2048bcc`；
`NPC.AI_001_Slimes` IL SHA-256 均为
`4cbb1c35c28d1a11accf9b01c997b3e83780e8cc2e0257f89ae8db785b9df4b9`。
这组结果支持本次 profile comparison 所用来源二进制与固定源码范围的身份关联；它本身不表示
profile 行为已通过。

## 采样行为结果

只读 EXE 与隔离源码构建各执行 32 个场景、182 ticks。两次 capture 的采样状态输出一致，
`NPC.AI` 与 `NPC.AI_001_Slimes` IL 一致，source-to-source report 的差异列表为空。
Blue Slime fixture 为 24×18；Mother Slime fixture 为 36×24。

CallTracker 的 `LoggedMethods.TryAdd`（来源 `CallTracker.cs:18`）使每个 tick 的队列只保留
方法名第一次进入的顺序。Capture 和 report 因此将该字段命名为
`deduplicatedFirstEntryOrder`。它不能证明重复调用次数或完整效果顺序。本次 source-to-source
比较只比较 `TargetClosest` 是否出现，并独立比较最终 `netUpdate` 与其他采样状态字段；不把
去重后的队列解释为完整调用轨迹。

C1 / C1.3 profile 对来源输出仍有 19 项差异：

| 字段 | 数量 | 观察 | 分类 |
| --- | ---: | --- | --- |
| `ai[2]` | 8 | 两个 profile 的 cooldown 场景中，profile 保留 `3`；来源首 tick 变为 `2`，后续采样变为 `1`。 | 直接状态转移差异：profile 当前未复现来源 cooldown decrement。 |
| `netUpdate` | 11 | profile 的 `networkSyncRequested=false`，来源最终 `netUpdate=true`。这 11 ticks 均观察到 profile 请求 target reacquire，且来源 `TargetClosest` first-entry 存在。 | 仍为 failed。归因未闭合：来源 `NPC.TargetClosest` 会调用 `SetTargetTrackingValues`，来源 `NPC.cs:78948` 可在 target / direction tracking 改变且无碰撞时设置 `netUpdate`。本次纯 profile result 不执行 target-selection adapter 的提交效果，故不能把这 11 项直接判作纯状态算法缺失；需由 adapter / composition 接线与验收闭合。 |
| velocity | 0 | 所有采样 velocity 值一致。 | 当前样本无差异。 |

profile gate 仍以 19 项差异返回 exit 2。`netUpdate` 项保留为失败，不因 target reacquire 意图而豁免；
在组合效果解释、接线和验收完成前，不提升 C1 / C1.3 profile 的来源 parity 状态。

## 比较器负向检查

在 `negative-path/mutated-source-capture.json` 中复制 source capture，将
`c1-blue-slime-init-ground-jump` 第一个 tick 的 `rotationBits` 从 `0` 改为 `1`。直接比较器与
PowerShell `-CompareOnly` 均在 report 中检出一项 source-to-source `rotation` 差异，令
`sampledReferenceOutputsMatch=false`，准确返回 exit 2，并保留各自 report 和日志。

## 验证与证据

| 验证 | 命令 / 结果 | 输出 |
| --- | --- | --- |
| SDK | `dotnet --version` → `10.0.400` | 仓库 `global.json` 选定 SDK |
| Verifier build | `dotnet build Test/Terraria.NpcAi.ReferenceVerification/Terraria.NpcAi.ReferenceVerification.csproj --no-restore --configuration Debug --nologo -v:minimal`；exit 0，0 warning / 0 error | `Build/bin/Terraria.NpcAi.ReferenceVerification/Debug/net10.0/` |
| 隔离来源构建 | `dotnet build <copied TerrariaServer.csproj> --no-restore --configuration Debug --nologo -v:minimal`；exit 0，85 warnings / 0 errors | `Build/bin/NpcAiReferenceVerification/SourceServer/Debug/net40/TerrariaServer.exe` |
| x86 reference harness 编译 | .NET Framework 4 `csc.exe /target:exe /platform:x86`；exit 0 | `Build/bin/NpcAiReferenceVerification/ReferenceHarness/ReferenceHarness.exe` |
| 完整 source differential | `powershell -NoProfile -ExecutionPolicy Bypass -File Test/Terraria.NpcAi.ReferenceVerification/Invoke-ReferenceDifferential.ps1 -ReferenceRoot 'D:\TRbackup\无任何删减通过编译'`；capture 完成，profile gate exit 2，19 项差异 | `Build/diagnostics/NpcAiReferenceVerification/20261007T035310365Z/` |
| 直接比较器负向 fixture | `dotnet run --project Test/Terraria.NpcAi.ReferenceVerification/Terraria.NpcAi.ReferenceVerification.csproj --no-build --no-restore -- --compare ...`；exit 2，mutation 被检出 | `negative-path/direct-comparator-report.json`、`direct-comparator.log` |
| PowerShell `-CompareOnly` 负向 fixture | `Invoke-ReferenceDifferential.ps1 -CompareOnly ...`；exit 2，mutation 被检出且 compare log 保留 | `negative-path/compare-only-report.json`、`compare-only-wrapper.log` |

完整输入、原始 / 源码构建 captures、provenance、build logs、IL 比较与 differential report
均保存在 `Build/diagnostics/NpcAiReferenceVerification/20261007T035310365Z/`。负向 fixture 及
两个失败 report 位于其 `negative-path/` 子目录。所有构建产物都在 `Build/bin/` 或 `Build/` 下。

## 未闭合项

- C1 / C1.3 的 `ai[2]` cooldown 差异仍是 profile 状态转移缺口，需由 production 工作流修复并重跑来源对照。
- 11 项 `netUpdate` 差异仍 failed，需对照 target-selection adapter 的效果提交与来源 `SetTargetTrackingValues`，再完成组合路径验收。
- 当前样本没有 C2 / C3 source comparison；不据本 checkpoint 提升任何 profile 或 coverage row 为 `verified`。
- 其它世界集成、碰撞、authority / replication、保存加载与完整 AI helper 效果仍不在本次结论范围内。
