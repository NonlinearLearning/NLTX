# Documentation Iteration DoD

## Documentation gate

- [x] Every current `docs/` file has exactly one manifest row.
- [x] Manifest paths and canonical artifacts exist; stages are `N1`-`N9` and statuses are allowed.
- [x] Build evidence, generated output, tooling, and stale references are distinct.
- [x] Historical files were not moved or deleted.
- [x] Remaining ambiguity is recorded in `scope.md` or `tech-debt.md`.

**Status:** `passed-with-deferred-findings`
**Evidence:** `Build/diagnostics/docs-flowstate-normalization/20260822-225500/evidence.json`
**Deferred:** 15 stale diagnostic references; refresh requires a separate evidence batch.

## Active ECS convergence gate

The convergence gate remains open. WorldGen differential, legacy-deletion, and final clean-process
acceptance are unchecked; TrainingDummy message-87 is bounded/accepted but broader parity remains
outside that batch. Focused green verifiers and evidence scores do not close this gate.
