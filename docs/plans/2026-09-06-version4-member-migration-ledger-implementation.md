# Version4 Member Migration Ledger Implementation Plan

> **状态：已被当前设计和工具实现取代。** 本文保留原实施推演作为历史记录；当前执行入口是
> `docs/plans/2026-09-06-version4-member-migration-ledger-design.md`、
> `.agents/skills/version4-member-migration-ledger/SKILL.md` 和
> `Build/Tools/Version4MemberMigrationScanner`。本文下方早期命令和错误码示例不得作为当前
> contract；当前账本仍必须经过显式 memberScope、target manifest 和 fail-closed audit。

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 建立一份以 Version4 字段/属性为覆盖分母、供 AI 持续恢复迁移工作的成员级账本，并用 PowerShell 审计阻断漏拆、重复归属、非法目标和过期证据。

**Architecture:** `docs/migrations/Version4-member-migration-map.json` 是唯一人工维护的事实源；Roslyn scanner 从 Version4 和显式 target manifest 提取 source/target snapshot，审计器只读验证覆盖、映射、反向目标、fingerprint 和证据，生成器输出 AI 速查表和 context packet，不修改运行时代码。

**Tech Stack:** .NET 10、Roslyn `Microsoft.CodeAnalysis.CSharp 4.14.0`、PowerShell 7、Python、JSON、Markdown、ripgrep；scanner 的文件读取使用有界 `Parallel.ForEachAsync`，编译使用 `WithConcurrentBuild(true)`。

---

## 执行边界

- 当前根项目的 Version4 覆盖根是 `D:\TRbackup\Version4`。
- 文件级分类继续读取 `docs/迁移参考表/Version4源码覆盖.tsv`，不另造 967 文件分母。
- 目标组件扫描根为 `src/` 和 `dome/src/`，由目标索引配置明确限定；不扫描测试和 `Build/` 生成物。
- `dome/docs/migrations/` 只作为历史迁移证据读取，不覆盖、不改写，也不自动当作当前账本。
- 不修改 `src/`、`dome/src/`、测试项目或现有未提交文件；新增测试仅覆盖映射工具。
- 任何写入操作必须使用 `apply_patch`；生成器的派生输出只能写入明确的 `docs/migrations/` 目标文件或 `Build/generated/` 临时路径。

### Task 1: 建立映射协议的失败优先测试

**Files:**

- Create: `Build/Tools/Tests/Version4MemberMigrationMap/Source/Entity.cs`
- Create: `Build/Tools/Tests/Version4MemberMigrationMap/Target/HealthComponent.cs`
- Create: `Build/Tools/Tests/Version4MemberMigrationMap/valid-map.json`
- Create: `Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1`

**Step 1: 写失败优先测试夹具和断言**

测试脚本使用临时目录复制 source/target 夹具，并为以下场景执行审计：

```text
valid move                 → 通过
duplicate sourceMemberId   → 失败
missing disposition        → 失败
move with zero targets     → 失败
move with two targets      → 失败
split without splitReason  → 失败
merge without mergeGroupId→ 失败
missing target declaration → 失败
orphan authoritative field → 失败
fingerprint drift          → 失败并报告 needs-review
blocked without nextWorkItemRef → 失败
deferred without evidence  → 失败
```

测试必须检查退出码和稳定错误代码，而不是只匹配一整段自然语言。例如错误代码固定为：

```text
MAP001 DuplicateSourceMember
MAP002 MissingDisposition
MAP003 InvalidMoveCardinality
MAP004 MissingExceptionReason
MAP005 MissingTargetMember
MAP006 OrphanAuthoritativeTarget
MAP007 FingerprintDrift
MAP008 MissingNextAction
MAP009 MissingEvidence
```

**Step 2: 运行测试确认当前失败**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Tests\Test-Version4MemberMigrationMap.ps1
```

Expected: FAIL，因为 `Test-Version4MemberMigrationMap.ps1` 尚未实现，且测试必须证明每一个负例
都能被最终审计器单独识别。

**Step 3: Commit**

```powershell
git add -- Build/Tools/Tests/Version4MemberMigrationMap/Source/Entity.cs Build/Tools/Tests/Version4MemberMigrationMap/Target/HealthComponent.cs Build/Tools/Tests/Version4MemberMigrationMap/valid-map.json Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1
git commit -m "test: define migration map audit failures"
```

### Task 2: 实现稳定成员身份和保守的 C# 声明提取器

**Files:**

- Create: `Build/Tools/Version4MemberMigrationMap.psm1`
- Test: `Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1`

**Step 1: 实现规范化函数**

在模块中实现以下函数，并为每个函数保留纯输入/输出边界：

```powershell
Normalize-RelativeSourcePath
Get-FullyQualifiedTypeName
Get-MemberSignature
New-SourceMemberId
Get-SourceFingerprint
New-TargetMemberId
ConvertTo-StableJson
```

`New-SourceMemberId` 的格式固定为：

```text
Version4::<fully-qualified-declaring-type>::<canonical-member-signature>
```

路径统一为 `/`；索引器、显式接口属性、嵌套类型和同名声明必须在 signature 中保留足够信息，
不能只取裸成员名。

**Step 2: 实现字段/属性扫描器**

实现 `Get-CSharpMemberDeclarations`，扫描声明字段和属性，至少支持：

- 普通字段、const、readonly、static 字段；
- 普通属性、auto-property、expression-bodied property；
- 属性访问器中的初始化器；
- 泛型类型、数组类型、可空类型和带属性声明；
- 嵌套类型和 namespace block/file-scoped namespace；
- 注释、字符串、字符和 interpolated string 中的大括号不影响结构计数。

扫描器必须保守失败：遇到无法确定声明边界的语法时返回 `unsupportedSyntax`，不能静默跳过成员。
所有声明记录包含 path、line、type、kind、accessibility、modifiers 和 fingerprint。

**Step 3: 运行单元级工具测试**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Tests\Test-Version4MemberMigrationMap.ps1 -Case Parser
```

Expected: PASS，并覆盖上述语法；任何 unsupported syntax 都必须出现在测试输出中，而不是被计数为零。

**Step 4: Commit**

```powershell
git add -- Build/Tools/Version4MemberMigrationMap.psm1 Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1
git commit -m "feat: add stable CSharp member identity extraction"
```

### Task 3: 实现 Version4 源成员候选集生成

**Files:**

- Modify: `Build/Tools/New-Version4MemberMigrationMap.ps1`
- Create: `Build/Tools/Version4MemberMigrationScanner/Program.cs`
- Test: `.agents/skills/version4-member-migration-ledger/tests/test_scanner_concurrency.py`

**Step 1: 写生成器输入和保留行为测试**

测试生成器处理一个最小 Version4Root 和一个已有 map，断言：

- 新源成员生成一条 `todo` 记录；
- 已有扁平 decision 按 `sourceMemberId` 保留；
- 源声明 fingerprint 改变时不覆盖目标决策，而是标记 `needs-review`；
- 源文件删除或重命名时报告 drift，不自动删除账本记录；
- 同一成员不因行号变化生成第二条记录。

**Step 2: 实现生成器入口**

脚本参数固定为：

```powershell
.\Build\Tools\New-Version4MemberMigrationMap.ps1 `
  -Version4Root D:\TRbackup\Version4 `
  -RepositoryRoot D:\TRbackup\NLTX `
  -TargetManifest .\docs\migrations\Version4-target-manifest.json `
  -SourceOutput .\Build\generated\version4-source.json `
  -TargetOutput .\Build\generated\nltx-target.json `
  -MaxDegreeOfParallelism 4
```

生成器必须：

1. 读取并验证 `docs/迁移参考表/Version4源码覆盖.tsv`；
2. 只把约定范围内的 `subsystem-evidence` 源文件加入成员候选集；其他文件通过文件级分类保留为非组件处置，不混入组件字段统计；
3. 递归扫描 Version4 源文件中的字段和属性；
4. 从目标根提取组件字段并生成反向目标索引；
5. 账本只允许显式 `memberScope` 和 target manifest 提供事实范围；不从文件名、目录或历史报告推测 decision；
6. 以 `sourceMemberId`、`targetMemberId` 和 batch 稳定排序；
7. 生成 JSON 时使用 UTF-8、固定缩进和不依赖哈希表遍历顺序的排序；
8. 任何文件读取、语法解析或 semantic extraction 失败都以非零退出，并在目标快照中保留
   `scan-failed`、失败文件和具体文件、行、原因；不能把失败转换为空成员集。

**Step 3: 在最小夹具上验证生成器**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Tests\Test-Version4MemberMigrationMap.ps1 -Case Generation
```

Expected: PASS；旧记录的人工迁移字段保持不变，新成员进入 `todo`，fingerprint drift 不被静默覆盖。

**Step 4: Commit**

```powershell
git add -- Build/Tools/New-Version4MemberMigrationMap.ps1 Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1
git commit -m "feat: generate Version4 member migration ledger"
```

### Task 4: 实现正向和反向映射审计器

**Files:**

- Create: `Build/Tools/Test-Version4MemberMigrationMap.ps1`
- Test: `Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1`

**Step 1: 写审计入口测试**

让测试脚本调用真实审计器，而不是复制业务规则。每个 fixture 都必须断言：

```powershell
$result.ExitCode -eq 0  # valid case
$result.ExitCode -ne 0  # each invalid case
$result.Output -match 'MAP00[1-9]'
```

**Step 2: 实现审计参数和摘要**

审计器参数：

```powershell
.\Build\Tools\Test-Version4MemberMigrationMap.ps1 `
  -Version4Root D:\TRbackup\Version4 `
  -RepositoryRoot D:\TRbackup\NLTX `
  -MapPath .\docs\migrations\Version4-member-migration-map.json `
  -TargetIndexPath .\docs\migrations\Version4-component-target-index.json
```

输出固定包含：

```text
source_candidates
ledger_rows
duplicate_source_members
missing_dispositions
invalid_cardinality
missing_targets
orphan_authoritative_targets
fingerprint_drifts
open_next_actions
verification_not_run
errors
```

**Step 3: 实现正向检查**

逐条检查：

- `sourceMemberId` 唯一且可由当前 Version4 声明重建；
- 每个源成员恰好一条记录且恰好一个 disposition；
- `move` 目标数量必须为 1；
- `split` 必须有至少 2 个目标和 `splitReason`；
- `merge` 必须有 `mergeGroupId` 和 `mergeReason`；
- `derive`、`excluded`、`deferred` 不能拥有伪装成权威的目标；
- `deferred`、`blocked` 必须有 evidence gap、blocker 和 `nextWorkItemRef`；
- 关键权威成员的 evidence、reader/writer status 和 verification status 不能缺失；
- fingerprint drift 让成员进入 `needs-review`，不能维持 `verified`。

**Step 4: 实现反向检查**

对目标索引逐条检查：

- 引用的文件、组件类型和字段真实存在；
- 权威目标必须有 `move`、`split` 或 `merge` 来源，或者显式 `origin=new` 并有设计理由；
- 一个权威目标不能被未经 `merge` 声明的多个源成员共同拥有；
- `derived`、`compatibility`、`integration` 目标不能被统计为权威迁移完成；
- 目标索引中声明的目标字段和账本引用集合一致。

**Step 5: 运行失败优先测试**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Tests\Test-Version4MemberMigrationMap.ps1 -Case Audit
```

Expected: PASS；测试本身 PASS，且每个非法 fixture 的审计退出码非零并含稳定错误代码。

**Step 6: Commit**

```powershell
git add -- Build/Tools/Test-Version4MemberMigrationMap.ps1 Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1
git commit -m "feat: enforce migration map coverage gate"
```

### Task 5: 生成 AI 速查表

**Files:**

- Modify: `Build/Tools/New-Version4MemberMigrationMap.ps1`
- Test: `Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1`

**Step 1: 写速查视图测试**

断言生成的 Markdown 至少包含以下固定区段并且顺序稳定：

```text
当前总览
当前应继续处理
阻塞项和证据缺口
Fingerprint 变化待复审
目标组件字段反向索引
AI 禁止自行推断
```

**Step 2: 实现 Markdown 生成**

速查视图只从 JSON 账本和目标索引生成，不允许手工追加一行。每个待处理条目必须显示：

```text
sourceMemberId
source path/type/member
fileOwner
stateKind
disposition
status
targetMemberId(s)
readerWriterStatus
verificationStatus
evidence
nextWorkItemRef
blocker
```

排序顺序固定为 active claim、`priority`、`fileOwner`、`batchId`、源路径、`sourceMemberId`、`workItemId`。没有 `nextWorkItemRef`
的未完成记录在生成阶段就失败，而不是生成一个无法继续的条目。

**Step 3: 验证视图无漂移**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Tests\Test-Version4MemberMigrationMap.ps1 -Case QuickReference
```

Expected: PASS；同一输入连续生成两次的 SHA-256 相同；手工修改速查表后，审计器报告 generated view drift。

**Step 4: Commit**

```powershell
git add -- Build/Tools/New-Version4MemberMigrationMap.ps1 Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1
git commit -m "feat: generate AI migration quick reference"
```

### Task 6: 从当前 Version4 基线生成真实账本

**Files:**

- Create: `docs/migrations/Version4-member-migration-map.json`
- Create: `docs/migrations/Version4-component-target-index.json`
- Create: `docs/migrations/Version4-member-migration-quick-reference.md`

**Step 1: 生成初始产物**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\New-Version4MemberMigrationMap.ps1 `
  -Version4Root D:\TRbackup\Version4 `
  -RepositoryRoot D:\TRbackup\NLTX `
  -MapPath .\docs\migrations\Version4-member-migration-map.json `
  -TargetIndexPath .\docs\migrations\Version4-component-target-index.json `
  -QuickReferencePath .\docs\migrations\Version4-member-migration-quick-reference.md
```

Expected：创建三份 UTF-8、稳定排序的产物；源成员默认进入 `todo`，不得凭文件名自动填入目标组件或
标记 `verified`。

**Step 2: 检查初始统计**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Test-Version4MemberMigrationMap.ps1 `
  -Version4Root D:\TRbackup\Version4 `
  -RepositoryRoot D:\TRbackup\NLTX `
  -MapPath .\docs\migrations\Version4-member-migration-map.json `
  -TargetIndexPath .\docs\migrations\Version4-component-target-index.json
```

Expected：审计会明确列出尚未归属的成员和目标字段，而不是把初始 `todo` 账本误报为迁移完成。
如果当前组件目标索引尚未填入必要的 `new`/`derived`/`integration` 来源说明，命令必须失败，
并给出具体字段和 `nextWorkItemRef`。

**Step 3: 记录首批人工映射**

只为一个已具备明确组件边界的窄批次补充 `targetMembers`、evidence、读写状态、batchId、priority
和 `nextWorkItemRef`。优先选择已有 focused verifier 的 Combat 边界，不同时回填所有子系统，避免把推测
批量固化为事实。

**Step 4: 重新生成速查视图并审计**

Run：

```powershell
pwsh -NoProfile -File .\Build\Tools\New-Version4MemberMigrationMap.ps1 `
  -Version4Root D:\TRbackup\Version4 `
  -RepositoryRoot D:\TRbackup\NLTX `
  -MapPath .\docs\migrations\Version4-member-migration-map.json `
  -TargetIndexPath .\docs\migrations\Version4-component-target-index.json `
  -QuickReferencePath .\docs\migrations\Version4-member-migration-quick-reference.md
pwsh -NoProfile -File .\Build\Tools\Test-Version4MemberMigrationMap.ps1 `
  -Version4Root D:\TRbackup\Version4 `
  -RepositoryRoot D:\TRbackup\NLTX `
  -MapPath .\docs\migrations\Version4-member-migration-map.json `
  -TargetIndexPath .\docs\migrations\Version4-component-target-index.json
```

Expected：生成和审计均成功，输出的未完成数量、阻塞数量和首个 `nextWorkItemRef` 与速查表一致。

**Step 5: Commit**

```powershell
git add -- docs/migrations/Version4-member-migration-map.json docs/migrations/Version4-component-target-index.json docs/migrations/Version4-member-migration-quick-reference.md
git commit -m "docs: add Version4 member migration ledger"
```

### Task 7: 接入 AI 迁移入口和文档索引

**Files:**

- Create: `docs/migrations/README.md`
- Modify: `docs/plans/2026-09-05-version4-all-subsystem-discovery-new-session-guide.md`

**Step 1: 写 AI 入口说明**

`docs/migrations/README.md` 明确以下固定顺序：

```text
1. 先读 Version4-member-migration-quick-reference.md
2. 选择 priority 最高且未完成的条目
3. 读取源声明、读者、写者和目标组件
4. 只能通过显式 disposition 更新账本
5. 运行 Test-Version4MemberMigrationMap.ps1
6. 重新生成 quick-reference
7. 把未完成事项留在 `nextWorkItemRef` 和结构化 work item，不依赖聊天历史
```

同时明确禁止：依据组件数量判断完成、依据命名相似度补目标、无证据标记 verified、静默删除
旧记录、覆盖 `dome/docs/migrations/` 历史证据。

**Step 2: 在新会话指南加入唯一入口链接**

只增加指向当前根项目速查表和审计器的入口，不重写历史审计结论，不改变 Version4 文件覆盖分母。

**Step 3: 做文档结构检查**

Run：

```powershell
git diff --check -- docs/migrations/README.md docs/plans/2026-09-05-version4-all-subsystem-discovery-new-session-guide.md
```

Expected：无 whitespace 错误；速查表、账本、目标索引和审计脚本路径全部存在。

**Step 4: Commit**

```powershell
git add -- docs/migrations/README.md docs/plans/2026-09-05-version4-all-subsystem-discovery-new-session-guide.md
git commit -m "docs: make migration ledger the AI entry point"
```

### Task 8: 最终门禁和交付记录

**Files:**

- Modify: `Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1`
- Create: `Build/diagnostics/version4-member-migration-map-validation-2026-09-06.txt`

**Step 1: 执行工具测试**

Run：

```powershell
pwsh -NoProfile -File .\Build\Tools\Tests\Test-Version4MemberMigrationMap.ps1
```

Expected：PASS；输出包含所有正例/负例数量和稳定错误代码断言。

**Step 2: 执行真实账本审计**

Run：

```powershell
pwsh -NoProfile -File .\Build\Tools\Test-Version4MemberMigrationMap.ps1 `
  -Version4Root D:\TRbackup\Version4 `
  -RepositoryRoot D:\TRbackup\NLTX `
  -MapPath .\docs\migrations\Version4-member-migration-map.json `
  -TargetIndexPath .\docs\migrations\Version4-component-target-index.json
```

Expected：根据当前账本实际状态返回；如果仍有 `todo`、`blocked` 或 `deferred`，输出必须明确列出，
但不能因此伪造“失败的迁移项目已全部完成”。只有覆盖门禁自身通过，才能报告“映射机制可用”。

**Step 3: 检查输出路径和工作区边界**

Run：

```powershell
Test-Path -LiteralPath .\docs\migrations\Version4-member-migration-map.json
Test-Path -LiteralPath .\docs\migrations\Version4-component-target-index.json
Test-Path -LiteralPath .\docs\migrations\Version4-member-migration-quick-reference.md
git status --short
```

Expected：三份产物位于 `docs/migrations/`；没有新文件写入 `src/`、`dome/src/`、项目文件旁、
`Build/bin/` 或 `Build/obj/`；既有未提交改动仍保留。

**Step 4: 记录验证证据**

将实际命令、退出码、错误/警告计数、源成员数、账本行数、目标字段数、未完成数和产物路径写入
`Build/diagnostics/version4-member-migration-map-validation-2026-09-06.txt`。这是工具验证证据，
不是迁移完成声明。

**Step 5: Commit**

```powershell
git add -- Build/Tools/Tests/Test-Version4MemberMigrationMap.ps1 Build/diagnostics/version4-member-migration-map-validation-2026-09-06.txt
git commit -m "test: record migration ledger validation"
```

## 交付判定

本计划完成后，只能宣称以下事实：

- AI 有稳定的成员级迁移恢复入口；
- 源成员遗漏、重复、非法目标和无理由例外可以被审计器阻断；
- 目标组件字段可以反向核对来源；
- 当前未完成项和下一步动作可以脱离聊天历史恢复。

不能据此宣称所有 Version4 行为已经迁移、所有组件已经可运行、网络/持久化已经闭合，或
Version4 与 NLTX 已经行为等价。
