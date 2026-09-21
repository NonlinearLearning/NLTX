# CR-2026-09-19：System 分区任务目录分层

状态：accepted
级别：moderate（System 分区任务入口路径调整）
提出方式：用户直接请求“在 D:\\TRbackup\\NLTX\\docs\\system-decomposition 创建权威和非权威文件夹管理分区任务”
范围：System 拆分指南、权威/非权威 P01-P20 任务入口、根目录便利副本及其路径引用

## 变更目的

将权威和非权威分区任务从通用计划目录中分离到 `docs/system-decomposition/` 下的
独立目录，使任务入口与 System 拆分领域归属一致，同时保留 `plans/` 中其他实验设计和
实施计划。

## 源路径与目标路径

| 源路径 | 目标路径 | 操作 |
| --- | --- | --- |
| `.agents/skills/ecs-system-domain-splitting/managed/function-body-cleanup/version4-20260916-002/cleaned/Version4/docs/plans/2026-09-02-system-split-markdown-writing-guide.md` | `docs/system-decomposition/2026-09-02-system-split-markdown-writing-guide.md` | move |
| `docs/plans/system-decomposition/2026-09-18-system-decomposition-authoritative-20-partition-tasks.md` | `docs/system-decomposition/authoritative/2026-09-18-system-decomposition-authoritative-20-partition-tasks.md` | move |
| `docs/plans/system-decomposition/2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md` | `docs/system-decomposition/non-authoritative/2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md` | move |

`docs/system-decomposition/reports/` remains the output area for claimed partition reports.
Other files under `docs/plans/system-decomposition/` remain in place.

## 影响与回滚

- Update runner defaults, session references, initialization examples, navigation, manifest and move map; keep existing runtime task-table/state snapshots unchanged.
- Add root-level read-only convenience copies of both task tables; keep the subdirectory files as the only canonical runner inputs.
- Partition IDs, row order, input reports, prompts and output report paths remain unchanged.
- No source code, tests, Version4 input files or partition state semantics change.
- Rollback moves each target back to its corresponding source path and restores the previous path references.

## 验收

- Each moved source exists at exactly one target path.
- `authoritative/` and `non-authoritative/` each contain one P01-P20 task table.
- Runner commands, initialization examples and active session references use the new task-table paths; existing runtime snapshots retain their historical source path by contract.
- The guide and task tables remain readable Markdown, and no old task-table path remains in active references outside those immutable runtime snapshots.
- Root-level copies retain the same partition rows and output definitions, use depth-corrected relative links, and are recorded as `canonical=false`; they do not participate in task claiming or state management.
