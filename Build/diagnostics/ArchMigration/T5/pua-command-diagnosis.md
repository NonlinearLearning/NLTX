# PUA command failure diagnosis (2026-10-08)

[自动选择：🟠 阿里味 | 因为：连续两次路径/命令构造失败 | 改用：最小内联证据采集，跳过通用脚本的相对路径初始化]

## Step 1 — failures observed

1. Read-only source query passed `src/NSSLC\Component` through PowerShell. `rg` reported `src/NSSLCComponent: 系统找不到指定的文件。 (os error 2)` while returning other matches. The original exec wrapper did not preserve the process exit code; it is recorded as `not-captured`, not inferred. This was a command argument/path construction defect, not evidence of a repository or environment failure.
2. Diagnostic recorder invocation `dotnet --version` exited 1 before launching dotnet. Exact error: `Exception calling "GetRelativePath" with "2" argument(s): "The path is empty. (Parameter 'path')"`. No build or test process ran; no output file, source hash, DLL/PDB hash, or input hash was produced.

## Step 2 — raw context and hypotheses

Read the complete recorder context around its source fingerprint loop (lines 10–20) and the current PowerShell working directory. The explicit worktree root is non-empty and an explicit `Path.GetRelativePath(worktree, Program.cs)` probe succeeded.

Hypotheses checked:

- H1: the worktree root or script root is empty. The inline probe resolved both roots to `C:\\Users\\shan\\.codex\\worktrees\\b69b\\NLTX`; this is excluded.
- H2: the recorder combines different item types and reads a missing `.FullName` property. The fingerprint loop combines `FileInfo` objects with root-input strings, then reads `$file.FullName` for both. Root-input strings have no `FullName`; this is the direct cause of the empty path.
- H3: PowerShell `Path.GetRelativePath` is unsupported or fails for this worktree. It succeeded with explicit non-empty absolute inputs; this is excluded.

Reverse assumption: there is no observed SDK/runtime environment problem. The recorder itself has a path-normalization bug.

## Step 3 — changed approach

The generic recorder will not be retried. Subsequent dotnet operations will be executed from the known worktree root with direct inline output capture. Path/hash inputs will be normalized to absolute strings before use. This changes the capture path instead of altering build arguments.

## Step 4 — validation

A corrected `rg` query used forward-slash project roots and reached the intended source tree. Its exact exit status and output are captured in the next command result and the final command ledger.

## Step 5 — evidence gaps

The first search’s exit code was not retained by the initial wrapper. The failed SDK preflight never invoked dotnet, so it does not establish the SDK version. The failed helper and this diagnosis record are retained under the T5 diagnostics directory.

## L2 follow-up — second evidence-capture failure

Exact third failure: `Set-Content: Could not find a part of the path 'C:\Users\shan\.codex\worktrees\b69b\NLTX\Build\diagnostics\ArchMigration\T5\commands\sdk-version.log'.` The inline command had already invoked `dotnet --version`, but stopped while writing logs; its dotnet exit code and stdout were lost. The outer PowerShell exit code 1 belongs to the logging failure and is not attributed to dotnet.

Raw command context: the inline sequence chose `$logPath` under `T5/commands/` and called `Set-Content` before creating that child directory. The preceding recorder had also failed before creating it.

Three distinct hypotheses were tested:

1. Missing parent directory — confirmed: `T5` existed, `T5/commands` did not. `New-Item -Force` created it.
2. Wrong worktree/output root — excluded: current root and absolute diagnostics path resolved to this worktree; `T5` existed.
3. General write/ACL failure — excluded: after directory creation, a write probe succeeded; SHA-256 `47AA4CC7F746D4FCFD676A1FD6ED5B1EBA85AFF98301E876828D794FB6064274` is in `commands/directory-write-probe.txt`.

Search/original context: reviewed the full inline command sequence and the exact `Set-Content` error. The earlier recorder source shows it creates `commands` only after source fingerprinting; its own failure happened before that point. Both defects are now understood. No build/test command has run.

New direction: invoke dotnet directly through the tool so the tool retains the process exit code; only after that result is known, persist stdout and metadata under the verified `commands` directory. This does not rely on recorder path initialization or implicit parent creation.
