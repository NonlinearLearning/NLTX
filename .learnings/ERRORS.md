## [ERR-20260917-001] invoke-serial-dotnet-forwarding

**Logged**: 2026-09-17T00:08:33+08:00
**Priority**: medium
**Status**: pending
**Area**: docs

### Summary
The documented `pwsh -File ... -- build ... -p:...` form does not forward MSBuild properties through the serial build wrapper.

### Error
```
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- Command attempted: the AnalysisEvidenceCatalog build command documented in the skill README and reference guide.
- The wrapper accepts a `DotnetArguments` array; PowerShell binds `-p:` as a wrapper parameter even after `--`.

### Suggested Fix
Document an array-based `-DotnetArguments` invocation or change the wrapper to support the documented delimiter form.

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; .agents/skills/ecs-system-domain-splitting/README.md

---

## [ERR-20260920-001] serial-dotnet-path-prefix

**Logged**: 2026-09-20T20:19:00+08:00
**Priority**: low
**Status**: resolved
**Area**: infra

### Summary
The serial CLI build was invoked with a repository-relative path missing the `.` prefix.

### Error
```
MSBUILD : error MSB1009: 项目文件不存在。
开关:\.agents\skills\ecs-system-domain-splitting\tools\SystemDecomposition.Cli\SystemDecomposition.Cli.csproj
```

### Context
- The command used `\\.agents\\...` from the repository root instead of `.\\.agents\\...`.
- No files were changed by the failed command.

### Suggested Fix
Use the repository's documented `.`-prefixed path when invoking `Invoke-SerialDotnet.ps1`.

### Metadata
- **Reproducible**: yes
- **Related Files**: Build/Tools/Invoke-SerialDotnet.ps1; .agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/SystemDecomposition.Cli.csproj

### Resolution
- **Resolved**: 2026-09-20T20:19:00+08:00
- **Notes**: Corrected the command path before retrying the build.

---

## [ERR-20260919-004] powershell-partition-validation-pipeline

**Logged**: 2026-09-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: docs

### Summary
The first PowerShell validation command used an empty pipeline element after a `ForEach-Object`
expression and failed before checking partition rows.

### Error
```
An empty pipe element is not allowed.
```

### Context
- The command was intended to count P01-P20 rows in the authoritative and non-authoritative task tables.
- Repository files were not modified by the failed command.

### Suggested Fix
Assign the projected objects to a variable before piping to `Format-Table`, or use a complete
pipeline expression inside the command block.

### Metadata
- **Reproducible**: yes
- **Related Files**: docs/system-decomposition/authoritative/2026-09-18-system-decomposition-authoritative-20-partition-tasks.md; docs/system-decomposition/non-authoritative/2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md

### Resolution
- **Resolved**: 2026-09-19T00:00:00+08:00
- **Notes**: Replaced the expression with explicit result collection before formatting.

---

## [ERR-20260919-001] apply-patch-context-mismatch

**Logged**: 2026-09-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: docs

### Summary

The first multi-file patch for removing a Markdown section did not match because the source had an extra blank line before the fenced code block.

### Error

```text
apply_patch verification failed: Failed to find expected lines
```

### Context

- Operation attempted: remove the `## 文档信息架构` section from four identical managed Markdown copies.
- The patch context omitted the blank line between the introductory sentence and the code fence.
- No file was modified by the failed patch.

### Suggested Fix

Use the exact source context, including blank lines, before retrying the patch.

### Metadata

- **Reproducible**: yes
- **Related Files**: `.agents/skills/ecs-system-domain-splitting/managed/function-body-cleanup/version4-20260916-002/original/Version4/docs/plans/2026-09-02-system-split-markdown-writing-guide.md`

### Resolution

- **Resolved**: 2026-09-19T00:00:00+08:00
- **Notes**: Corrected patch context is being applied to all four matching copies.

---

## [ERR-20260919-002] powershell-foreach-pipeline-syntax

**Logged**: 2026-09-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: docs

### Summary

A PowerShell inspection command used a pipeline directly after a `foreach` statement and failed to parse.

### Error

```text
ParserError: An empty pipe element is not allowed.
```

### Context

- Operation attempted: summarize cleanup manifest metadata for two managed runs.
- The command attempted to pipe the `foreach` statement directly into `Format-List`.
- No repository artifact was changed by the failed inspection command.

### Suggested Fix

Assign the `foreach` result to a variable before piping it to a formatter.

### Metadata

- **Reproducible**: yes
- **Related Files**: `.agents/skills/ecs-system-domain-splitting/managed/function-body-cleanup/version4-20260916-001/manifest.json`

### Resolution

- **Resolved**: 2026-09-19T00:00:00+08:00
- **Notes**: Re-ran the inspection with an intermediate result variable and obtained the manifest comparison.

---

## [ERR-20260919-003] apply-patch-table-context-mismatch

**Logged**: 2026-09-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: docs

### Summary

A combined navigation update patch did not match the actual document architecture table context and was rejected without applying any hunk.

### Error

```text
apply_patch verification failed: Failed to find expected lines in docs/document-architecture.md
```

### Context

- Operation attempted: synchronize the new System decomposition task directories in navigation, inventory, and architecture metadata.
- The patch also expected an additional artifact-type table row in a context that differed from the file.
- No navigation or manifest change was applied by the failed patch.

### Suggested Fix

Split broad documentation patches into smaller file-specific changes and inspect the exact local context before each patch.

### Metadata

- **Reproducible**: yes
- **Related Files**: `docs/README.md`; `docs/document-architecture.md`; `docs/document-inventory.md`

### Resolution

- **Resolved**: 2026-09-19T00:00:00+08:00
- **Notes**: The next patch is limited to exact navigation and path-mapping lines.

---

## [ERR-20260918-001] skill-root-path-resolution

**Logged**: 2026-09-18T20:47:00+08:00
**Priority**: low
**Status**: resolved
**Area**: docs

### Summary
读取本地 skill 时先后使用了错误的技能根路径，导致两次 `Get-Content` 找不到文件。

### Error
```
Cannot find path 'C:\Users\shan\.codex\skills\writing-for-agents\SKILL.md'
Cannot find path 'C:\Users\shan\.codex\skills\self-improvement\SKILL.md'
```

### Context
- 任务：盘点并建立根目录 `docs/` 的文档信息架构。
- 技能目录实际由当前会话的 roots 映射提供；`r1` 对应 `C:\Users\shan\.agents\skills`，而不是 `.codex\skills`。
- 正确路径已读取 `C:\Users\shan\.agents\skills\self-improvement\SKILL.md`，未影响仓库文档变更。

### Suggested Fix
读取可用 skill 前先使用会话提供的 Skill roots 映射解析别名，不根据个人目录结构猜测绝对路径。

### Metadata
- **Reproducible**: yes
- **Related Files**: AGENTS.md; docs/README.md

### Resolution
- **Resolved**: 2026-09-18T20:47:00+08:00
- **Notes**: 修正路径后完成 skill 读取和文档级校验。

---

## [ERR-20260917-006] focused-verifier-run-project-argument

**Logged**: 2026-09-17T19:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary

Passing a `.csproj` as the first positional argument to `dotnet run` through
the serial wrapper did not select that project with the current SDK.

### Error

```
找不到要运行的项目。请确保 D:\\TRbackup\\NLTX 中存在项目，或使用 --project 传递项目路径。
```

### Context

- Command used the repository wrapper and the focused verifier `.csproj` as a
  positional `run` argument.
- The wrapper is required by the repository build contract.

### Suggested Fix

Pass the project explicitly with `--project <path>` and keep all MSBuild
properties as individual argument elements in the current PowerShell process.

### Metadata

- **Reproducible**: yes
- **Related Files**: Build/Tools/Invoke-SerialDotnet.ps1; Context/约束/构建与验证约束.md

---

## [ERR-20260917-002] powershell-new-item-literalpath

**Logged**: 2026-09-17T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: docs

### Summary
PowerShell `New-Item` does not accept `-LiteralPath` for directory creation in this environment.

### Error
```
A parameter cannot be found that matches parameter name 'LiteralPath'.
```

### Context
- Command attempted: create `docs/人工审查` before adding a Markdown document.
- The subsequent `apply_patch` created the directory and document successfully.

### Suggested Fix
Use `New-Item -ItemType Directory -Path <explicit-path>` after testing the exact target path, or let `apply_patch` create the parent path when supported.

### Metadata
- Reproducible: yes
- Related Files: docs/人工审查/禁止AI读取-Version4系统拆分后人工审查与返工.md

### Resolution
- **Resolved**: 2026-09-17T00:00:00+08:00
- **Notes**: No retry was required because the requested document was created by the patch operation.

---

## [ERR-20260917-003] apply-patch-same-file-operations

**Logged**: 2026-09-17T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: infra

### Summary
An apply patch attempted to delete and add the same existing file in one patch.

### Error
```
apply_patch verification failed: invalid patch: multiple operations target D:\\TRbackup\\NLTX\\src\\Player\\PlayerCombatProcSystem.cs
```

### Context
- Operation attempted: bring the validated experiment commit into the current worktree.
- The patch generator emitted `Delete File` and `Add File` operations for the existing System file.

### Suggested Fix
Use one `Update File` operation for an existing path and reserve `Add File` for paths that do not exist.

### Metadata
- **Reproducible**: yes
- **Related Files**: src/Player/PlayerCombatProcSystem.cs

### Resolution
- **Resolved**: 2026-09-17T00:00:00+08:00
- **Notes**: The retry uses a single update operation for the existing file.

---

## [ERR-20260917-004] invoke-serial-dotnet-pwsh-forwarding

**Logged**: 2026-09-17T00:00:00+08:00
**Priority**: medium
**Status**: resolved
**Area**: infra

### Summary
Invoking the serial dotnet wrapper through `pwsh -File` still let `-p:` arguments bind to the wrapper's own parameters.

### Error
```
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- Command attempted: `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(...)`.
- The wrapper accepts a `[string[]]$DotnetArguments` parameter, but the outer `-File` invocation did not preserve the array boundary.

### Suggested Fix
Invoke the wrapper in the current PowerShell process with `& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(...)` so MSBuild properties remain array elements.

### Metadata
- **Reproducible**: yes
- **Related Files**: Build/Tools/Invoke-SerialDotnet.ps1; Context/约束/构建与验证约束.md

### Resolution
- **Resolved**: 2026-09-17T00:00:00+08:00
- **Notes**: The direct script invocation built the focused verifier successfully.

---

## [ERR-20260917-005] git-branch-delete-ancestry-guard

**Logged**: 2026-09-17T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: infra

### Summary
Normal branch deletion rejected a validated experiment branch because its commit was not an ancestor of the current branch.

### Error
```
error: the branch 'codex/ecs-system-split-p06-experiment-20260917' is not fully merged
```

### Context
- The experiment commit was brought into `main` with `git update-ref`, but the two refs have parallel histories.
- The experiment worktree had already been removed and the experiment file tree was verified on `main`.

### Suggested Fix
After verifying the exact experiment tree is present on the destination ref, use `git branch -D` for the explicitly requested disposable branch only.

### Metadata
- **Reproducible**: yes
- **Related Files**: refs/heads/main; refs/heads/codex/ecs-system-split-p06-experiment-20260917

### Resolution
- **Resolved**: 2026-09-17T00:00:00+08:00
- **Notes**: Content was verified on `main`; only the named temporary branch is force-deleted.

---
