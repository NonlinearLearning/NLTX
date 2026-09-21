# Version4 Non-Authoritative Top-13 Peer Refinement Implementation Plan

> **For Codex:** Preserve unrelated worktree changes. This pass changes the source-inventory
> report boundary only; it does not claim runtime ECS implementation.

## Goal

在已有五层拆分结果上继续执行用户指定的 13 个即时 peer 的第六层拆分，保留全部成员
事实和历史 lineage，并生成可独立验证的最终 349-peer 报告。

## Task 1: Add the sixth-level mapping [complete]

**Files:**

- Modify: `Build/Tools/Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`

添加 13 个第六层退休 peer、26 个子组定义和 `Get-SixthLevelFineSubsystemId`。映射使用
声明类型、源码路径或稳定成员族，未匹配成员直接失败；每个子组保留原始基线和直接
`PreviousPeer`。

## Task 2: Integrate nested lineage and final ranking [complete]

**Files:**

- Modify: `Build/Tools/Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`
- Modify: `docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`

将 13 个退休 peer 从最终定义集合移除，加入 26 个第六层终端定义，递归展开第三至
第五层 lineage，并把最终活跃数量从 `336` 更新为 `349`。重新生成报告时保持输入成员
事实、父级聚合、基线聚合和 SHA-256 追踪不变。

## Task 3: Strengthen focused split verification [complete]

**Files:**

- Modify: `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemSplit.ps1`

验证 13 个退休 peer、26 个子组、精确的成员统计、直接 peer 关系、嵌套退休 peer 的
rollup 和最终 349 个活跃 peer，并继续保留前面各层拆分的断言。

## Task 4: Strengthen full report verification [complete]

**Files:**

- Modify: `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemReport.ps1`

更新完整报告解析和断言，覆盖第六层 lineage、成员统计、父级/基线闭合、完整排名、
来源序号、字段/属性总量和 ID 类文件排除。用户提供的“排名 1–13”作为拆分前 peer
清单验证；最终报告的 349 行排名在拆分后重新计算。

## Task 5: Regenerate and independently audit [complete]

生成器和两个验证器已从仓库根目录串行运行。另以只读 PowerShell 审计重新解析最终报告，
独立确认完整排名、明细序号、第六层 lineage 和各子组 rollup。

## Verification Record

| Command | Exit code | Result |
|---|---:|---|
| `pwsh -NoProfile -File .\Build\Tools\Generate-Version4NonAuthoritativeFineSubsystemReport.ps1` | 0 | 生成 `D:\TRbackup\NLTX\docs\migration\ledgers\Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；349 groups；4,017 fields；525 properties；4,542 total members。 |
| `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemSplit.ps1` | 0 | 77 same-level boundaries、23 retired mixed peers、50 previous top-ranked baselines、5 third-level、30 fourth-level、12 fifth-level 和 13 sixth-level peer splits 全部通过；最终 349 groups。 |
| `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemReport.ps1` | 0 | 输入/报告各 4,542 行；4,017 fields + 525 properties；349 groups 完整排名和逐组统计通过；11 parent rows、349 fine rows、9 populated parent totals 通过；ID 类文件为 0。 |
| Independent read-only ranking/lineage audit | 0 | `1..349` 排名连续；4,542 条明细序号连续为 `1..4,542`；字段/属性 `4,017/525`；13 条第六层 lineage 均恰有两个子组且 rollup 闭合。 |
| `git diff --check` | 0 | 无空白错误；仅保留 Git 的既有 LF-to-CRLF 规范化提示。 |

本轮没有运行 `dotnet`，因为没有修改 C#、项目文件或编译产物；生成产物位于
`Build/bin/` 之外的既有 Markdown 报告路径，未写入源代码目录旁的二进制或中间文件。

## Evidence Gaps

报告和验证器证明的是源码成员库存、归属边界、统计和 lineage 闭合，不证明目标 ECS
组件已经实现，也不证明完整读者/写者、生命周期、持久化、网络序列化或 System 调度
顺序。后续运行时迁移必须为这些端口补充 focused tests 和行为证据。

