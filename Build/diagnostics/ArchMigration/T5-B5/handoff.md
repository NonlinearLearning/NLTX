# T5-B5 宿主清理入口、A0 输入清单与 A8 静态包含审计

| 日期 | 批次状态 | T5 总状态 | 源码基线 |
| --- | --- | --- | --- |
| 2026-10-08 | preparatory / partial | **partial / blocked-by-prerequisite** | `af6f3039fa1611a3e0da0cb6c37707eb96f3b295` |

源码聚合 fingerprint：`D22E54FEBE9AD29ECF8DB40E872A77D0A7195ECA3174C1625265B23527FC0CFE`。本批只新增诊断清单；没有修改 `src/`、项目文件、`Context/` 或六份未跟踪计划。

## 状态与前置

本批完成了只读宿主/清理路径审计、A0 输入资产清点和 A8 生产候选的静态 Compile inclusion 映射。T5 仍使用 custom ECS；工作树中没有 Arch 生产 PackageReference 或 Arch World API。此前唯一通过的宿主 smoke 是 custom ECS 的 load → 1 tick → exit，本批只复用该证据，没有再启动宿主或扩展测试矩阵。

| 前置线 | 当前可用证据 | T5 影响 |
| --- | --- | --- |
| T1 | `partial`；CommandBuffer playback 出现进程级 AccessViolation，`Chunk.cs`、崩溃后世界和 rollback 未核验。 | 不能假定该路径具备事务或 rollback 保证。 |
| T2 | `partial / blocked-by-prerequisite`；隔离 probe 有记录，生产 World/session/token/identity 合同未进入本 worktree。 | 不猜 Arch World 的生产调用签名。 |
| T3 | `partial / blocked-by-prerequisite`。 | Arch 生命周期、关系清理和候选销毁没有宿主调用证据。 |
| T4 | B0/B1 静态审计已提交，整体 `partial`；本 worktree 没有可接受的 B4 lifecycle handoff。 | 不推断 Arch.System lifecycle API，也不将兄弟 worktree 编辑视为已整合接口。 |
| A0 | 输入清单已落盘；性能预算未冻结。 | 不运行性能基准，不作性能结论。 |

总验收接受的只是 T5 partial baseline，不表示 T1–T4 已通过，也不授权 A6–A8 生产实现或删除。

## 宿主、恢复与生命周期审计

- `src/NSSLC.Application/Network/NetworkWorldOwner.cs` 在专属线程调用 session factory 并串行执行排队命令。取消只会在 operation 仍排队时跳过；运行中的操作由 owner 线程完成。`DisposeAsync` 停止接收工作、拒绝未执行命令、等待当前工作结束，再在 owner 线程释放 session。
- `src/NSSLC.Infrastructure/WorldStorage/NetworkWorldSessionLoader.cs` 新建 `LoadedWorldSession`，运行生成的 load API，并要求 recovery outcome 可发布、session 完整且有 source document。其源码注释明确说明它不发布 legacy Main 状态，也不运行 Simulation 的 load recovery effects；失败时它释放候选 session。
- `src/NSSLC.Tools.NetworkServer/Program.cs` 通过 `await using` 持有 `NetworkWorldOwner`。它读取已加载世界文档和 runtime ID，并启动网络宿主。源码明确记录当前没有 authoritative game-tick clock；`NetworkWorldItemOwner` 收到的 tick provider 仍为 `() => 0L`。服务器的 `TimeProvider.System` 用于现有网络调度，不是已接入的游戏模拟 tick。
- `src/NSSLC.Application/WorldStorage/Persistence/WorldLoadRecoveryCoordinator.cs` 静态流程先将已解码数据提交到 candidate session，再投影 load gate、执行 publication、settle、release gate 与 finalize。publication 未开始的失败路径丢弃候选；published 或 publication uncertain 的失败路径执行 reset 并撤销 publication 状态。恢复协调器本身不证明 Arch ownership；session 实际释放由宿主/factory 的生命周期边界负责。
- `src/NSSLC.Application/Simulation/WorldSimulationKernel.cs` 在构造时捕获 owner thread，`Step` 强制同线程调用；每步重新验证 session readiness/current-session。stop 或 cancellation 在 step 开始前已请求时，kernel 返回 `TickCommitted=false` 并停在当前 tick。
- `src/NSSLC.Tools.Simulation/Program.cs` 允许 CLI `tickCount=0`。此时 tick loop 不进入，随后正常 post-loop 代码仍会停止 kernel、生成报告，并可能按 save 参数保存 tick 0 的快照。现有 smoke 只跑了一个 tick，zero-tick exit 没有运行证据。常规 Simulation 成功返回路径未看到显式释放 active session 的调用；本次只记录静态观察，不把进程退出等同于 session dispose 验证。

## 场景覆盖

| 场景 | 本批状态 | 证据边界 |
| --- | --- | --- |
| 加载 → 1 tick → exit | `reused-existing-pass` | `../T5/host-smoke-319.json`：仅 custom ECS，fixture-only。 |
| 两次连续换世界 | `not-run` | 无 Arch host/session token 路径。 |
| 取消 candidate | `not-run` | 只读检查了 custom recovery/cancellation 边界。 |
| zero-tick exit | `not-run` | CLI 和 kernel 接受/处理该路径，但无 smoke 证据。 |
| late-finalize failure → rollback → retry | `not-run` | 旧 custom factory 有恢复代码；Arch World 路径缺失。 |
| Arch save/reload | `not-run` | 319 roundtrip 是旧 DTO/Codec 路径，不是 Arch persistence。 |
| benchmark / 性能对照 | `not-run` | A0 未冻结。 |

## A0 输入与容量准备

机器可读清单见 [`../A0/input-manifest.json`](../A0/input-manifest.json)。当前找到的真实存档 `src/World/科研.wld` 为 version 326、11,713,581 bytes；既有 T5 custom host 尝试在 tick 0 因 `world.backgrounds.load` 不支持该版本而失败。唯一可通过的输入是 T5 生成的 WorldFile 319 小型 fixture，不能作为真实小/中型世界或 Arch persistence 基线。当前清单未找到兼容的真实小型和中型存档。

T5 合同列出 Player 255、NPC 200、Projectile 1000 的目标 actor 数；A0 尚未冻结代表性组件组合、操作比例、采样方式与验收阈值。容量目标和性能预算因此保持未冻结，不能据现有数字开始 benchmark。

## A8 Compile inclusion 静态映射

逐文件映射见 [`a8-compile-inclusion-map.csv`](a8-compile-inclusion-map.csv)。它从现有 `../T5/a8-delete-manifest.csv` 的 62 条 `production-candidate` 记录出发，以最近的 `.csproj` 归属、SDK 默认 Compile 项设置及项目内 XML `Compile Include` 规则建立静态映射。

| 项目 | 候选文件数 | 静态包含规则 |
| --- | ---: | --- |
| `Terraria.Items.csproj` | 2 | SDK 默认 Compile 项 |
| `Terraria.LeashedEntity.csproj` | 1 | SDK 默认 Compile 项 |
| `Terraria.Npc.csproj` | 2 | SDK 默认 Compile 项 |
| `Terraria.Projectile.csproj` | 4 | SDK 默认 Compile 项 |
| `Terraria.Relationships.csproj` | 4 | SDK 默认 Compile 项 |
| `Terraria.EntityEcs.csproj` | 4 | SDK 默认 Compile 项 |
| `Terraria.WorldStorage.csproj` | 4 | SDK 默认 Compile 项 |
| `NSSLC.Application.csproj` | 14 | SDK 默认 Compile 项 |
| `NSSLC.Infrastructure.Network.csproj` | 7 | explicit `*.cs` include |
| `NSSLC.Infrastructure.WorldStorage.csproj` | 1 | explicit `WorldStorageCoordinatorFactory.cs` include |
| `NSSLC.Tools.NetworkServer.csproj` | 1 | SDK 默认 Compile 项 |
| `NSSLC.Tools.Simulation.csproj` | 18 | SDK 默认 Compile 项 |
| **总计** | **62** | **54 SDK-default; 8 explicit** |

这是项目文件静态审计，不是 MSBuild evaluated Compile item 清单。没有执行 MSBuild 查询，因此不能用它声明完整生产编译闭包或删除门已通过。静态搜索没有发现 `Compile Remove`。现有 CSV 只含 62 个生产候选行；虽然上一份 T5 报告记录过 43 个测试文件命中旧框架符号，该文件没有列出测试文件，本映射不将测试计为零。

`src/NSSLC.Infrastructure/分类参考/` 是独立参考树，62 个生产候选行没有来自该目录的文件；本映射没有以符号相似性把参考材料当生产实现。`docs/` 不位于这些生产项目的默认源树中；`Directory.Build.props` 还将 `Build/**` 加入 SDK `DefaultItemExcludes`。这两项是项目布局与静态项目文件观察，不代替全量 MSBuild evaluation 或 project-reference dependency closure。

该清单不是删除授权。A8 仍需在上游 handoff 可接受后核对动态 compile items、项目引用闭包、测试夹具和例外，再由总验收者决定是否另开删除 commit。

## 本批变更、验证与回滚

- 新增诊断：`../A0/input-manifest.json`、`a8-compile-inclusion-map.csv`、本报告和 `command-log.jsonl`。
- 复用了既有 1/10 custom smoke；本批没有重新运行宿主、测试、构建或性能基准。
- 没有修改源代码、项目文件、测试、`Context/`、用户计划、既有 Build 输出，也没有删除旧 ECS。
- 回滚：仅撤销本批新增的四个诊断文件；之前的 T5 partial baseline 和所有用户计划/Build 输出保持原样。

## 下一 owner

1. T1–T4 owner 提供被总验收接受的 API、身份、生命周期、query/network/item handoff；先处理 T1 CommandBuffer crash 与未核验的 rollback。
2. A0 owner 提供兼容真实小/中型世界、代表性组件/操作构成和冻结阈值。
3. 上述前置进入本 worktree 后，重开实际 Arch Simulation/NetworkServer 装配，并覆盖世界切换、取消、zero tick、late-finalize rollback/retry 与 Arch 保存/重载。
4. 完成这些场景后运行约 10% 的被接受核心选择及对应项目构建；性能部分只按冻结预算执行。
5. 最后复核 A8 动态 Compile item 与依赖闭包，再由总验收者判断是否授权删除。
