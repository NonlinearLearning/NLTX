# NLTX Flowstate Context

`docs/flowstate/` is the committed authority for the documentation lifecycle (N1-N9).
Historical evidence keeps its original path; this directory indexes and gates it.

## Authority

| Concern | File |
| --- | --- |
| Requirements and deferred policy | `requirements.md` |
| Iteration boundary | `scope.md` |
| Active risks | `risks.md` |
| Execution plans and batch tasks | `plan/`, `task/` |
| Acceptance gate | `dod-checklist.md` |
| Debt and follow-up | `tech-debt.md`, `retrospective.md` |
| Documentation/build inventories | `document-manifest.csv`, `build-context-manifest.csv` |
| Build concurrency contract | [`AGENTS.md#dotnet-build-concurrency-contract`](../../AGENTS.md#dotnet-build-concurrency-contract) |

The active migration continuation context and its plan/task Markdown files were cleared on
2026-09-01. There is no active migration plan or task Markdown under docs/flowstate/.

The repository source, tests, migration reports, research notes, and Build/diagnostics/
evidence were not changed by this cleanup. This cleanup must not be interpreted as migration
completion or parity acceptance.

## Build concurrency entry point

The repository-wide build rule is defined once in the root
[`AGENTS.md#dotnet-build-concurrency-contract`](../../AGENTS.md#dotnet-build-concurrency-contract).
It applies to all human, Codex, subagent, and parallel sessions sharing this checkout:
source edits may be parallel when write sets are disjoint, while every `dotnet` command that can
compile or write shared outputs is one serial critical section. `progress.md` exposes the same
link for model-context startup. The executable launch path is
`Build/Tools/Invoke-SerialDotnet.ps1`, which queues shared-output `dotnet` commands on the
checkout-specific named Mutex.

## Boundaries

- Every file under `docs/` has one manifest row and a canonical Flowstate artifact.
- `Build/diagnostics/` is immutable verification evidence; `Build/bin`, `obj`, `generated`,
  and `packages` are generated output.
- `.agent-workplace/` is private process state and is not documentation context.
- Do not move, delete, or bulk-rewrite historical documents without a separate `fst-change`.

## Lifecycle

`N1 inventory -> N2 scope -> N3 information architecture -> N4 batch normalization ->
N6 document/link/schema gate -> N7 release decision -> N8 retrospective`.

Use `fst-workplace` and `fst-iterate` for active work; route changes through `fst-change` and
completion through `fst-review` plus the DoD checklist.
