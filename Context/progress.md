# Project Progress Summary

Current source baseline: L0/L1 components plus restored L2 sources, updated on 2026-10-13.

- The active project is `src/NSSLC/Terraria.Components.csproj` (`net10.0`).
- Outside the protected infrastructure directory, source retains the 6 L0 and 62 L1
  components in section 3.1 of the [L0/L1 plan](../docs/plans/2026-10-08-arch-l1-basic-game-components-execution-plan.md),
  plus their 38 enums/value definitions. On 2026-10-13, all 145 L2 component files in the
  [grading list](../docs/reviews/audits/2026-10-08-simple-game-component-grading.csv) were restored
  and moved from `src/NSSLC/Component/` to `src/NSSLC/Components/`, grouped by domain.
  The project includes both source directories. Their source content was preserved;
  this was file organization only, without dependency repair or behavior changes.
- L2 recovery used 41 files from the pre-cleanup snapshot and 104 files from grading commit
  `5bbdd78b78fb5bc8bfd926063de6e3aa542db503`. External reference files under `D:/Component/`,
  including `小众` and `当前不好改的`, were not moved or modified.
- Systems, runtime storage, application/host code, compatibility APIs, generators and their old
  verification projects were removed at the user's explicit request.
- `src/NSSLC.Infrastructure/` is protected in full. Its 2,232 files, including existing local
  modifications and reference code, were verified unchanged. Its old projects are outside the
  active build and may retain references to removed projects.
- The 2026-10-09 L0/L1 component library built with 0 warnings and 0 errors. That historical
  result does not cover the restored L2 files. Per the user's instruction, no compilation,
  tests or other verification were performed for the 2026-10-13 organization change.
- Current scope, evidence and recovery:
  [cleanup record](../docs/cr/CR-2026-10-09-components-only-cleanup.md) and
  [component inventory](../docs/reviews/audits/2026-10-09-components-only-inventory.md).

The following cleared-context card is historical and does not describe the current source baseline.

Context status: cleared

The current migration continuation context was cleared on 2026-09-01 at the user's request.
No active batch, checkpoint, deferred-work queue, or migration resume narrative is retained in
this model-facing card.

The following were intentionally not changed:

- source code and runtime behavior under src/;
- verification programs under Test/;
- project/build policy files;
- historical migration, research, protocol, server-completion, and WorldGen documents;
- Build/diagnostics/ evidence.

For repository build rules, use
[构建与验证约束](约束/构建与验证约束.md).
For general documentation lifecycle rules, use
[dome/dome1/docs/flowstate/README.md](../dome/dome1/docs/flowstate/README.md).
There is no active migration plan/task selected by this card.

This is a context cleanup only. It is not a migration-completion, parity, deletion-gate, or
runtime-behavior claim.
