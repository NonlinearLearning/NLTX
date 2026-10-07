# T5 宿主基线与交接报告

| 日期 | 状态 | 分支 | 基线提交 |
| --- | --- | --- | --- |
| 2026-10-08 | **PARTIAL — blocked-by-prerequisite** | `codex/arch-migration-t5-handoff` | `e2c686790ab6a4f14505ea914c27478f5959d061` |
源码聚合 fingerprint：`D22E54FEBE9AD29ECF8DB40E872A77D0A7195ECA3174C1625265B23527FC0CFE`

## 结论

T5 完成了当前 Simulation/NetworkServer 的构建基线、一个约 10% 的单次宿主 smoke、真实存档版本失败记录和 A8 只读引用审计。T1–T4 的可接受交接与 A0 冻结性能预算在当前工作树中缺失；四份 T1–T4 计划仍为 active，源码中未发现 Arch PackageReference 或 Arch 原生 API。因此 A6 的 Arch 宿主汇合、Arch 保存/加载和性能验收均未完成，本批次在此前置阻塞处停止。

Smoke 使用的是当前 custom ECS 生产路径，不能作为 Arch 迁移证据。没有修改生产源码、项目文件或 `Context/`，也没有执行 A8 删除。此报告不代表迁移完成、parity 通过或旧框架退出。

## 前置与源码审计

- T1–T4 handoff 目录/可接受交接未出现在本轮证据中；四份计划仍标记 active。T5 计划要求在前置未满足时只做准备和审计，不执行不可逆删除。
- 源码与项目引用搜索未发现 `PackageReference Include="Arch"`、`using Arch.Core`、`Arch.Core.World` 或 `Arch.Buffer`。当前 Simulation、NetworkServer、`LoadedWorldSession`、`WorldStorageRoot` 和 save/load 协调仍围绕 custom `EntityRuntime` 工作；没有 `Arch.Persistence` 接入。
- A8 只读审计发现 62 个 production-candidate 源文件和 43 个测试文件命中旧框架相关符号。文件和符号明细见 [`a8-delete-manifest.csv`](a8-delete-manifest.csv) 与 [`a8-symbol-counts.csv`](a8-symbol-counts.csv)。这是候选清单，不是删除授权；`分类参考` 参考目录单独处理，需结合 compile inclusion 与 T1–T4 依赖闭包复核。
- 本轮源码 fingerprint 与所有记录构建相同。六份既有未跟踪计划/协调文档保持原样，未暂存。

## 构建与输入结果

SDK 为 `10.0.400`（由仓库 `global.json` 选择）。详细命令、退出码、警告/错误计数、输出路径和输入/artifact hashes 记录在本地 `command-log.jsonl` 与 `commands/` 下；生成的日志和二进制不包含在交接提交中。

| 项目 / 命令 | 结果 | 输出与说明 |
| --- | --- | --- |
| `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj -p:FixtureHostBuild=true --nologo` | exit 1；23 warnings，1 error | fixture 输出模式下 generator 写入 `Build/bin/FixtureHost/...`，而 Network 项目目标仍从普通 `Build/bin/NetworkCodecGenerator/...` 启动它；详见 [`simulation-build-failure.md`](simulation-build-failure.md)。 |
| `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --nologo` | exit 0；23 warnings，0 errors | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/`。 |
| `dotnet build src/NSSLC.Tools.NetworkServer/NSSLC.Tools.NetworkServer.csproj --nologo` | exit 0；0 warnings，0 errors | `Build/bin/NSSLC.Tools.NetworkServer/Debug/net10.0/`。 |
| `dotnet build src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --nologo` | exit 0；0 warnings，0 errors | 仅用于生成 WorldFile 319 smoke fixture。 |

跟踪存档 `src/World/科研.wld` 是 WorldFile version 326，SHA256 为 `7B6D6F46D6C805402AE22D381A16CDFE0CD60B676549EF0AFF5DB0A8F8B53B9E`。宿主尝试加载时在 tick 0 因 `world.backgrounds.load` 不支持 version 326 而失败；源文件只读且 hash 未变。此失败不计入通过 smoke 数。

WorldGeneration 随后在 T5 diagnostics 目录生成 WorldFile 319 fixture，SHA256 为 `28DBFA2C8222BE0D88F958912767E728FC2F5290A147BD0833F0B10D966BAE88`。其旧 ECS roundtrip 报告对 27 个 API、5,040,000 个 tile 做了比较；这仅验证 fixture/旧 DTO-Codec 路径，不是 Arch persistence 验收。

## 宿主 smoke 与覆盖率

通过的命令：

```text
dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/ArchMigration/T5/input/319-small-12345/generated.wld 1 --players 1 --seed 12345 --report Build/diagnostics/ArchMigration/T5/host-smoke-319.json
```

结果见本地 [`host-smoke-319.json`](host-smoke-319.json)：exit 0，0 warnings、0 errors；`Succeeded=true`、`Canceled=false`，一名玩家，session 已发布，recovery 为 `Completed`，tick limit 与最终 tick 均为 1，kernel 已停止，`SourceFileUnchanged=true`。输入 hash 与生成 fixture hash 一致。报告的 `ContentSource=simulation-core-v3` 且 session 仍使用 `EntityRuntime`，所以这是 custom ECS 的宿主基线。

按本轮约 10 条代表性核心测试预算，实际通过 1 条复合场景（load → 1 tick → exit），约 10%。未运行：Arch World 集成、两次世界切换、late-finalize rollback/retry、Arch save/reload、取消、零 tick、benchmark 与性能对照。真实 version 326 的失败尝试单独记录，不计为通过项。

## 性能与删除门

A0 性能预算未冻结，本轮未运行 benchmark，也未宣称性能通过。Player 255、NPC 200、Projectile 1000 的基线输入、完整 Simulation tick 的分位数/分配/GC/内存对照均待 A0 和 Arch 实际宿主路径具备后执行。

A8 删除未执行。旧框架引用仍在 production candidates 与测试中；T1–T4、Arch 宿主关键场景及性能证据尚未验收。只有总验收者确认前置线与证据可接受后，才可基于 manifest 审核依赖闭包并单独提出删除提交。

## 交接与回滚点

下一 owner（T1–T4 / 总验收）：

1. 提供并验收 T1–T4 handoff，确认 API、身份、lifecycle、relationships、query 与网络物品线的集成契约。
2. 冻结 A0 性能预算和同输入基线，明确 benchmark 的负载、阈值与采样要求。
3. 在实际 Arch World 已进入默认 Simulation/NetworkServer 装配后重开 T5：补齐切换与 rollback/retry、保存/加载、取消/零 tick、性能对照和对应验收。
4. 由总验收者审阅 A8 manifest 和 compile dependency closure，再决定是否另开删除 commit。

本交接提交的 parent `e2c686790ab6a4f14505ea914c27478f5959d061` 是回滚点；该提交只增加交接报告与只读文字审计证据，不触碰生产代码。需要撤销交接包时可对交接 commit 执行 `git revert <handoff-commit>`。本地完整命令日志、JSON smoke/roundtrip 报告、生成的 `.wld` 与构建产物继续留在 `Build/diagnostics/ArchMigration/T5/`，未纳入提交。
