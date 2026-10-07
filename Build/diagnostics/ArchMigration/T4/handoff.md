# Arch migration T4 B0/B1 handoff

Track: T4
Status: partial
Batch status: B0 inventory complete; B1 independent assessment complete; production access migration blocked-by-prerequisite.
Baseline: e2c686790ab6a4f14505ea914c27478f5959d061
Branch: codex/arch-migration-t4
Date: 2026-10-08

## Goal and result

This handoff covers the T4-B0 owner/access/order audit and the part of T4-B1 that can be completed without changing production Arch APIs. The system ownership matrix is in b0-b1-access-ownership-matrix.md. It records read/write component groups, structural transitions, side effects, phase/slot/RNG order, queue capture risks, struct-copy rules, item revisions/reservations and one stale packet-29 path.

No production source or Context constraint changed. The T4 checkout still uses custom EntityRuntime/RuntimeEntityHandle and has no Arch package, Arch World, QueryDescription or native query use in the audited production paths. No query API was guessed and no compatibility implementation was added.

## Dependency evidence

- T1: the T1 worktree has an isolated Arch probe and diagnostics, but no handoff.md/API behavior matrix was present when inspected. The latest thread snapshot was active/in progress and still diagnosing a CommandBuffer behavior assertion. Per the user's instruction, this handoff does not wait for T1.
- T2: Build/diagnostics/ArchMigration/T2/handoff.md exists, hash recorded in source-hashes.md, but declares status partial and production signature migration blocked by missing T1. It documents the intended World/session token and identity validation order, and explicitly leaves component Get/TryGet/ref/Set/Add/Remove and QueryDescription semantics unproven. Its isolated probe commit 57f2c75b3f05bdb713bf421ae1ec7c2161f805e2 does not switch this T4 checkout's production signatures.
- T3: docs/architecture/execution/2026-10-08-arch-migration-track-t3-lifecycle-relationships-execution.md and T3-command-index.json exist in the T3 worktree. The report declares partial and Arch prerequisite blocked-by-prerequisite; it describes current custom-runtime lifecycle evidence, not production Arch signatures. T3 commit f0a34c4a5e7707978cafd3ed1184ad053f53598f is in another worktree and does not provide an integrated Arch API to this branch.
- Consequently T1 query/ref behavior, T2 production World/identity access signatures and T3 Arch lifecycle/relationship resolution remain missing from the current T4 production checkout. T4-B1 implementation is blocked-by-prerequisite.

## Validation and tests

This batch changed only generated diagnostics. No T4 production project build, restore, test or simulation smoke was run. The T4 test selection budget and Arch-specific smoke are not claimed as complete. DLL/PDB hashes are not applicable because this batch generated no build outputs. Prior T2/T3 builds and tests are external evidence only; they are not attributed to T4.

Static evidence commands and outcomes are in command-log.md. Source/input fingerprints and the consumed T2/T3 report hashes are in source-hashes.md. Prior and current inspection failures plus corrections are recorded in failure-audit.md.

## Changed files and commit

- Build/diagnostics/ArchMigration/T4/b0-b1-access-ownership-matrix.md
- Build/diagnostics/ArchMigration/T4/command-log.md
- Build/diagnostics/ArchMigration/T4/failure-audit.md
- Build/diagnostics/ArchMigration/T4/handoff.md
- Build/diagnostics/ArchMigration/T4/source-hashes.md
- Build/diagnostics/ArchMigration/T4/environment.txt (existing T4 environment record)

Commit: scoped diagnostics commit; exact revision is reported in the task final response.

## Open findings and uncovered scope

- No B1 native query/ref migration, production project build, or Arch smoke.
- No T4-B2 queued network request rewrite or DTO serialization scan against a migrated Arch entity model.
- Packet 29 stale/missing/inactive/duplicate terminate currently returns accepted no-op after ignoring TryTerminateNetwork false; re-evaluate once T2/T3 contracts are integrated.
- No T4-B3 owner-specific item mutation conflict replacement or quantity conservation verification. Existing WorldItemReservationSystem is a separate named domain protocol; RuntimeItemRegistry still composes generic component revisions.
- No T4-B4 approximately 10% T4 core tests were run.
- No full Simulation/NetworkServer integration, cross-world stale request proof, Arch ref/structural boundary proof, pickup expiry/duplicate proof, or DTO/wire proof.

## Next owner action

After T1's accepted API matrix and T2/T3 production contracts are available in an integrated checkout, resume T4-B1 from the recorded candidate order. Start with one ordered, non-structural per-entity access slice; keep the existing owners and slot traversal; then prove one ref/copy/structural boundary before expanding. Follow with T4-B2 queue intent and consumption-time re-resolution, T4-B3 named item conflict protection, then the explicitly budgeted T4-B4 sample.
