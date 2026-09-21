# Version4 全量子系统查找：新会话启动指南

## 1. 任务目标

完成一份以 `D:\TRbackup\Version4` 为唯一**全量覆盖基线**的子系统审查。交付物不是把
每个类、Hook、消息号或 TileEntity 都升格为子系统，而是建立一份可审计、可持续更新的分层
清单，说明完整运行时中的每个自有 C# 源文件归属何处、为何归属、与 NLTX 当前实现的关系以及
证据是否闭合。

本轮只做证据盘点和审查产物。不得修改 `src/` 下的 NLTX 生产代码、不得迁移组件、不得补写
游戏规则，也不得改变构建策略。

## 2. 已确认的决策

以下决定已经由需求方确认；新会话不得重新把它们当成待决定事项。

| 主题 | 已确认决定 |
| --- | --- |
| “所有子系统”的含义 | 覆盖 Version4 完整运行时，但分为“权威模拟”“运行时基础设施与外部适配”“客户端表现与工具链”三个层级。 |
| 子系统粒度 | 候选至少满足下列四项中的三项：独立生命周期/调度阶段；独立权威状态、写入根或受控提交；跨越至少两个既有领域并有稳定 I/O 或协议边界；可定义独立 verifier。 |
| 参考证据顺序 | 完整可编译源码 `D:\TRbackup\无任何删减通过编译` 为第一主证据；`D:\TRbackup\Version4` 为删减对照与本轮覆盖基线；本地 tModLoader API 文档仅作公开契约/边界交叉验证；NLTX 仅用于当前映射，不反向证明 Version4 行为；旧报告仅作候选与历史输入。 |
| 覆盖基线 | `D:\TRbackup\Version4`，仅纳入其中自有、可参与运行时行为的 `.cs` 文件；排除 `bin/`、`obj/`。基线中每个纳入文件必须有且仅有一个主归属。 |
| 完整源码的作用 | 它只能补证 Version4 中已有文件的空实现、删减实现或调用链；完整源码独有文件不扩容主清单和覆盖率，写入差异附录并标为 `outside-Version4-baseline`。 |
| 跨域文件 | 每个文件一个 `primary_owner`，可有多个 `related_subsystems` / `cross_domain_edges`。优先按权威状态写入/提交者、独立生命周期/调度器、主导调用闭包判定。`Main.cs` 一类总编排文件归 `RuntimeComposition`。 |
| NLTX 状态 | `confirmed` 要求当前 NLTX 有权威状态、执行链和 focused verifier；`partial` 为模型或局部路径存在但未闭合；`missing` 为无权威模型或仅内容/表现元数据；`excluded` 必须写明所属或排除原因。旧报告状态不可直接沿用。 |
| 参考实现证据状态 | `version4-confirmed`：Version4 本身有声明、实现和调用链；`full-reference-supplemented`：Version4 有调用点/空体/删减体，完整源码有可证明对应实现；`source-gap`：两处均无法闭合或无法证明对应关系。 |
| 旧报告处理 | 新建全量主报告，不覆盖旧报告；新报告要指出它替代哪些旧结论、如何修正及证据原因。 |

## 3. 当前已知证据与材料

先阅读下列材料，再开始新一轮检索。它们是有价值的候选和历史结论，不是可以不经复查就继承的
最终结论。

1. `Context/progress.md`：确认当前没有活动迁移批次或续接计划。
2. `AGENTS.md`：尤其是 .NET 串行构建契约、文档/输出限制和保留用户未提交改动的要求。
3. `docs/Version4权威游戏模拟子系统审查报告.md`：已有 20 个初始责任面的总表与历史边界。
4. `docs/component-decomposition/review-round-1/reports/2026-09-05-version4-authoritative-simulation-subsystem-findings.md`：权威模拟主干和外部边界证据。
5. `docs/component-decomposition/review-round-1/reports/2026-09-05-version4-tmodloader-additional-authoritative-subsystems.md`：传送、钓鱼、世界过渡、日历事件、规则覆写、解锁进度六项补充候选。
6. `docs/component-decomposition/review-round-1/reports/2026-09-05-tmodloader-travel-and-subsystem-boundaries.md`：旅行/传送的独立边界、排除项与 NLTX 映射。
7. `D:\TRbackup\Version4`：覆盖基线和与旧报告对应的删减参考。
8. `D:\TRbackup\无任何删减通过编译`：完整行为补证。
9. `D:\TRbackup\tmodloader-api-docs-stable`：公开 Hook、存档、复制、服务器/客户端职责的交叉验证资料。

已核实的补证案例：

- `D:\TRbackup\Version4\Terraria\Projectile.cs:34073-34074` 的
  `AI_061_FishingBobber` 和 `AI_061_FishingBobber_GiveItemToPlayer` 为空实现；
- 完整源码 `D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51236` 与 `:51496`
  存在对应完整实现，且周围代码包含 FishingAttempt、资格计算、敌对 NPC 判定、掉落判定和
  FishingContext 的相关调用链；
- 因此钓鱼不能再以“算法无法从参考代码还原”标为 `source-gap`，应在文件/子系统证据中标为
  `full-reference-supplemented`，并保留两端位置和匹配理由。

## 4. 预期交付物

新会话应创建下列新文件，避免覆盖既有历史报告：

| 文件 | 用途 |
| --- | --- |
| `docs/Version4权威游戏模拟子系统全量审查报告-2026-09-05.md` | 新的主审查报告；解释范围、方法、分层系统、关键证据、NLTX 映射、旧结论修订、风险与未闭合点。 |
| `docs/migration/ledgers/Version4子系统索引.json` | 机器可读的一级子系统索引；适合表达数组、证据链和跨域关系。 |
| `docs/migration/ledgers/Version4源码覆盖.tsv` | 每个 Version4 基线 `.cs` 文件一行；用于核验唯一主归属和覆盖率。 |
| `docs/Version4与完整源码差异附录-2026-09-05.md` | Version4 删减点的补证，以及完整源码独有文件的 `outside-Version4-baseline` 记录。 |
| `Build/Tools/Test-Version4SubsystemCoverage.ps1` | 只读校验脚本；验证 JSON/TSV 与 Version4 文件基线的一致性。 |

### 4.1 JSON 最小模式

`Version4子系统索引.json` 的每个一级子系统至少包含：

```json
{
  "id": "FishingAndCatchSimulation",
  "layer": "authoritative-simulation",
  "responsibility": "...",
  "qualificationCriteriaMet": [
    "independent-lifecycle",
    "controlled-commit",
    "cross-domain-boundary"
  ],
  "version4Evidence": [
    {
      "path": "Terraria/Projectile.cs",
      "line": 34073,
      "referenceStatus": "full-reference-supplemented",
      "fullReferencePath": "D:/TRbackup/无任何删减通过编译/Terraria/Projectile.cs",
      "fullReferenceLine": 51236,
      "reason": "..."
    }
  ],
  "tModLoaderEvidence": [
    {
      "path": "class_mod_player.html",
      "line": 2604,
      "usage": "public-boundary-cross-check"
    }
  ],
  "nltxMapping": {
    "status": "partial",
    "paths": ["src/Player/PlayerFishingCapabilityState.cs"],
    "reason": "..."
  },
  "relatedSubsystems": ["ProjectileSimulation", "ItemContainerAndEconomy"],
  "excludedCandidates": [
    { "name": "Fishing bobber projectile", "reason": "time carrier, not the transaction boundary" }
  ],
  "risks": ["..."]
}
```

允许的 `referenceStatus`：`version4-confirmed`、`full-reference-supplemented`、`source-gap`。
允许的 NLTX `status`：`confirmed`、`partial`、`missing`、`excluded`。

### 4.2 TSV 最小列

`Version4源码覆盖.tsv` 采用制表符分隔、UTF-8、稳定路径排序，至少有下列列：

```text
relative_path	classification	primary_owner	reference_status	full_reference_path	related_subsystems	evidence_location	report_reference	note
```

- `classification` 允许值：`subsystem-evidence`、`shared-runtime-mechanism`、
  `external-dependency-or-generated`、`excluded`。
- `primary_owner` 对每行必须非空；它可以是一级子系统 ID 或约定的共享/外部所有者。
- `relative_path` 必须使用相对 `D:\TRbackup\Version4` 的 `/` 分隔路径。
- `full_reference_path` 仅在 `reference_status=full-reference-supplemented` 时为必填。
- `related_subsystems` 可以为空，多个值以 `;` 分隔；它不改变唯一 `primary_owner`。

## 5. 执行流程

### 阶段 A：建立可重复库存

1. 从 `D:\TRbackup\Version4` 递归找出 `.cs` 文件，排除任意路径段为 `bin` 或 `obj` 的项目。
2. 为每个文件提取相对路径、命名空间、主要类型、文件大小、可能的空方法/恒定返回/生成标记。
3. 只把目录、命名空间和符号名称作为**候选线索**，绝不以它们直接宣布最终子系统归属。
4. 将清单稳定按 `relative_path` 排序，作为 TSV 的初始母表。

### 阶段 B：子系统与证据闭合

1. 从旧报告的 20 个候选责任面开始，不假设它们已经正确或完整。
2. 对每个候选追踪：声明/状态、构造或注册、生命周期入口、主要读者、主要写者、网络/存档/客户端边界。
3. 仅在候选满足已确认的“四项中至少三项”门槛时，将其保留为一级子系统；否则转为已有系统内部机制或 `excluded`，并写明归属。
4. 对跨域文件依据主归属规则裁决一次，并把其他关系写为边；严禁为提高“看起来完整”的程度而复制占有同一文件。
5. 在 Version4 发现空体、删减体或不自然默认返回时，到完整源码按路径、类型、成员签名和邻近调用链确认对应实现；不能可靠匹配时保留 `source-gap`。
6. 对每个一级子系统读取当前 NLTX `src/` 和相关 `Test/`，重新给出 `confirmed`、`partial`、`missing` 或 `excluded`。不得仅因为已有目录或状态记录而标 `confirmed`。
7. tModLoader 文档只用于确认公开 Hook 的时机、持久化/复制方向、服务端/客户端职责和扩展边界；不可用它填充 Version4 私有算法细节。

### 阶段 C：撰写与校验

1. 主报告明确本轮是“全量架构盘点”，不是迁移完成、行为等价或 API 兼容声明。
2. 主报告必须列出三层系统总表，以及每项责任、门槛命中、证据状态、NLTX 状态、相邻边界与主要风险。
3. 差异附录必须区分：Version4 文件内的补证点；完整源码独有但不纳入主覆盖基线的文件；无法闭合的 `source-gap`。
4. 校验脚本必须读取 JSON/TSV，并从 Version4 实际重新枚举基线文件，拒绝未分类、重复路径、路径漂移、非法状态、缺失主归属和应有却缺少的完整源码路径。

## 6. 完成门槛

只有同时满足以下条件，才可报告本轮完成：

1. 校验脚本输出 Version4 纳入文件数与 TSV 行数一致，且 `unclassified=0`、
   `duplicate_primary_owner=0`、`invalid_classification_or_status=0`。
2. 每个 JSON 一级子系统都有职责、至少三项命中门槛、Version4 主证据、tModLoader 交叉证据或
   `not-applicable`、NLTX 映射、状态、相邻边界和排除理由。
3. 所有 `full-reference-supplemented` TSV 行都记录完整源码路径、匹配依据和 Version4 删减差异；
   完整源码独有文件只出现在差异附录。
4. 主报告含总文件数、分类分布、一级子系统数量、NLTX 状态分布、修正过的旧结论和剩余
   `source-gap`。
5. NLTX 映射在当前代码基础上经过构建和 focused verifier 检查。验证必须遵守 `AGENTS.md`：
   先检查活跃 `dotnet.exe`/`csc.exe`；所有 compile-capable 命令都从仓库根目录通过
   `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行，使用 `-m:1 -nr:false`
   `-p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`；构建后确认
   `Build/bin/` 工件，并以 `--no-build --no-restore` 执行 verifier。若当前验证失败，只记录失败，
   不把受影响状态升级为 `confirmed`。

## 7. 不得犯的错误

- 不因 `Version4` 参考树被删减，就把完整源码独有文件静默算入当前覆盖率。
- 不把完整源码补证误写成“Version4 基线本身具备完整实现”。
- 不按目录名机械分配 `primary_owner`，尤其不能把 `Main.cs`、`WorldGen.cs`、`NetMessage.cs` 这类跨域
  文件拆成多份或按被调用次数归属。
- 不将单个 AI style、消息号、Hook、UI 控件、音频、视觉粒子、单个 TileEntity、协议注册表升格为
  一级子系统，除非它们独立满足已确认门槛。
- 不用 tModLoader API 文档替代 Version4 的实际调用链或私有行为证据。
- 不将 NLTX 的“存在项目/组件/记录”表述为行为已实现。
- 不修改用户现有未提交改动，不执行广泛删除或清理，也不将生成物写到 `src/`。

## 8. 可直接粘贴到新会话的首条提示

```text
请执行 docs/plans/component-decomposition/2026-09-05-version4-all-subsystem-discovery-new-session-guide.md 中已确认的
“Version4 全量子系统查找”任务。先阅读该指南、Context/progress.md、AGENTS.md 和其中列出的既有审查材料。
不要重新发起需求访谈，除非发现与已确认决策真正矛盾的新证据。

交付新的全量主报告、Version4子系统索引.json、Version4源码覆盖.tsv、完整源码差异附录，以及只读
覆盖校验脚本。覆盖基线严格为 D:\TRbackup\Version4（排除 bin/obj）；完整可编译目录只用于补证
Version4 中已有文件的删减实现，完整源码独有文件仅进差异附录。每个基线 C# 文件只能有一个
primary_owner，但可记录相关子系统边。不得修改 src/ 生产代码。

在报告完成前，按 AGENTS.md 的串行 .NET 合约重新验证当前 NLTX 的相关构建/ focused verifier，
并以实际结果决定映射状态。不要将目录或组件存在误写为 confirmed。
```

## 9. 新会话的第一轮检查清单

- [ ] 检查 `git status --short`，确认并保留现有用户改动。
- [ ] 阅读 `Context/progress.md`、`AGENTS.md` 和本指南。
- [ ] 重新枚举 Version4 基线 `.cs` 文件，记录本次运行的总数，不能依赖旧快照数字。
- [ ] 检查完整源码目录与 Version4 的可比性，并先复验钓鱼补证案例。
- [ ] 读取既有总报告与三份 2026-09-05 研究笔记，列出需要复核而非继承的结论。
- [ ] 建立机器可读 schema 和 TSV 列头，先让校验器能报出未分类项。
- [ ] 再开始逐域证据闭合与 NLTX 状态复查。
