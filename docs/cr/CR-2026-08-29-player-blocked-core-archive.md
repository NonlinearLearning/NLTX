# CR-2026-08-29 Player Blocked Core Archive

## Request

> 明确归档这些 Blocked 核心条目

## Classification

- Level: `minor`
- Flowstate nodes: `N5 -> N6`
- Decision: `accepted and archived`
- Scope: documentation and migration-status traceability only
- Code behavior change: none

## Impact

- The eleven existing `Blocked` Player clusters remain `Blocked`.
- Each cluster now has an archive ID, owner domain, missing dependency, and
  explicit reopen condition in `docs/migrations/player-blocked-core-archive.md`.
- CSV and behavior-map references now point to the archive.
- No legacy source is deleted, no simulation behavior is promoted, and no
  verification claim is broadened.

## Acceptance

- All 11 `Blocked` CSV rows resolve to PB-001..PB-011.
- The behavior map explains that archived blockers are not migrated behavior.
- The formal Player plan links the archive and preserves the Phase 7 gate.
- Future work must reopen one PB entry through a new Flowstate batch with fresh
  source-backed evidence.
