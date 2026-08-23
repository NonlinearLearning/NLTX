# Documentation Iteration DoD

- [x] Every current file under `docs/` has exactly one manifest row.
- [x] Every manifest path exists relative to the repository root.
- [x] Every manifest lifecycle stage is one of `N1` through `N9`.
- [x] Every manifest status is one of `active`, `accepted`, `deferred`, `archived`, or
      `excluded`.
- [x] Build evidence paths are classified separately from generated output and tools.
- [x] No `Build/bin`, `Build/obj`, `Build/generated`, or `Build/packages` path is classified
      as a plan or requirement.
- [x] Canonical Flowstate artifacts cross-link without missing paths.
- [x] Historical documents were not moved or deleted.
- [x] Remaining ambiguity is recorded in `tech-debt.md` or `scope.md`.
- [x] Verification output is recorded before claiming completion.

Status: `passed-with-deferred-findings`

Evidence: `Build/diagnostics/docs-flowstate-normalization/20260822-225500/evidence.json`

Deferred findings: 15 stale `Build/diagnostics` references remain explicitly classified as
`stale-reference`; they require a separate evidence refresh, not silent replacement.

## Active convergence gate (N6 not yet entered)

- [x] B1 current document/build manifest synchronization is reproducible (`docs=251`,
      `manifest=251`, `buildReferences=456`).
- [ ] WorldGen supported-profile differential has zero tile, extended-state, metadata,
      command-sequence, and random-checkpoint mismatches.
- [ ] `canRemoveLegacyWorldGen` is true with a complete source/runtime oracle.
- [ ] No deferred `ServerRelevant` physical deletion remains.
- [ ] TrainingDummy inbound placement direction and rejection semantics are source-backed.
- [ ] Fresh full verifier sweep, direct consumer builds, differential, and ledger gates pass.

The active graph remains incomplete until every unchecked item is evidenced. Focused green
verifiers and the 92/100 evidence score do not satisfy this gate.
