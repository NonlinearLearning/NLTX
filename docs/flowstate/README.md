# NLTX Flowstate Context

This directory is the canonical, committed context for the NLTX documentation lifecycle.
It applies the Flowstate N1-N9 workflow without moving or rewriting the historical evidence
under `docs/archive`, `docs/migrations`, `docs/plans`, `docs/protocol`, `docs/research`,
`docs/server-completion`, or `docs/worldgen`.

## Authority rules

- `docs/flowstate/requirements.md` is the current requirement classification.
- `docs/flowstate/scope.md` is the signed iteration boundary. Unknown scope stays deferred.
- `docs/flowstate/risks.md` is the active risk register.
- `docs/flowstate/plan/` contains formal phases and declares the execution strategy. The
  active convergence plan is `plan/2026-08-22-server-ecs-convergence.md`.
- `docs/flowstate/task/` contains batch-level tasks and acceptance criteria. The active
  convergence graph is `task/2026-08-22-server-ecs-convergence.md`.
- `docs/flowstate/dod-checklist.md` is the current N6 acceptance gate.
- `docs/flowstate/tech-debt.md` records compromises and deferred documentation work.
- `docs/flowstate/document-manifest.csv` maps every `docs/` document to a lifecycle stage,
  status, evidence class, and canonical Flowstate artifact.
- `Build/diagnostics/` is immutable verification evidence. It is referenced, never used as
  a hand-edited planning surface.
- `Build/bin`, `Build/obj`, `Build/generated`, and `Build/packages` are generated output and
  are not documentation context.
- `.agent-workplace/` is private process state and is intentionally ignored by Git.

## Lifecycle

```text
N1 inventory -> N2 scope freeze -> N3 information architecture
N4 normalize in batches -> N6 document/link/schema gate -> N7 release decision
N8 retrospective -> next N4 batch
```

Historical documents remain at their original paths so source anchors and evidence links do
not break. New work must link to this directory and must not create an unregistered planning
document elsewhere under `docs/`.

## Use

Start with `fst-workplace`, then `fst-iterate` using the plan and task files here. A new
requirement or changed scope must go through `fst-change`; completion must go through
`fst-review` and the DoD checklist.
