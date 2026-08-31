## [LRN-20260830-001] correction_item_migration_is_code

**Logged**: 2026-08-30T00:00:00+08:00
**Priority**: high
**Status**: pending
**Area**: backend

### Summary
For the Item field/property migration, the requested deliverable is a bounded C# implementation
slice with focused verification, not documentation reorganization.

### Details
- The active migration report is context and acceptance authority only.
- Each next source-backed field or behavior must land in the Simulation owner chain and be exercised
  by a focused verifier before status is updated.
- Keep full Item parity and explicitly deferred behavior marked partial.

### Suggested Action
When resuming this migration, inspect the current red test and implement the smallest C# owner,
consumer, snapshot, and regression slice; do not create docs-only process artifacts as the primary
deliverable.

### Metadata
- Source: user_feedback
- Related Files: docs/migrations/item-field-property-ecs-migration.md; Test/Terraria.Dome.Items.Verification/Program.cs
- Tags: item-migration, ecs, code-first, correction

---
