# Arch migration T4 B0/B1 handoff

Track: T4
Status: partial
Overall prerequisite status: blocked-by-prerequisite
Batch status: B0 inventory complete; B1 production access migration blocked-by-prerequisite.
Baseline: e2c686790ab6a4f14505ea914c27478f5959d061
Branch: codex/arch-migration-t4
Date: 2026-10-08

## Current acceptance amendment: T4-B4 compile-only

On 2026-10-08 the user accepted CR-2026-10-08-arch-compile-only-acceptance.md and changed this
batch's gate to incremental `dotnet build` only. The CR is stored outside this worktree at
`D:\TRbackup\NLTX\.agent-workplace\changes\CR-2026-10-08-arch-compile-only-acceptance.md`.
The user's later steering confirms that prior runtime samples are historical and do not count as
current acceptance. No runtime command was run after that steering.

T4-B4 remains partial and T4 remains blocked by T2/T3 production contracts. The compile-only
handoff, build log, phase concept map, failure record, and artifact hashes are in
[T4-B4](../T4-B4/handoff.md). This amendment supersedes the earlier B4 sample-test wording as the
current gate; the probe and B4 reports are recorded in commit
`9550753cb35344e44d3132bce8cfb6a37e943d10`, based on `9bdde4eb7b316e27d78eb6463da68355dafd6fb9`.
It does not claim query/ref, network, item, lifecycle, serialization, admission, or host behavior
is verified.

## Goal and result

This handoff covers the T4-B0 owner/access/order audit and the part of T4-B1 that can be completed without changing production Arch APIs. The system ownership matrix is in b0-b1-access-ownership-matrix.md. It records read/write component groups, structural transitions, side effects, phase/slot/RNG order, queue capture risks, struct-copy rules, item revisions/reservations and one stale packet-29 path.

No production source or Context constraint changed. The T4 checkout still uses custom EntityRuntime/RuntimeEntityHandle and has no Arch package, Arch World, QueryDescription or native query use in the audited production paths. No query API was guessed and no compatibility implementation was added.

## Dependency evidence

- T1: the T1 worktree has an isolated Arch probe and diagnostics, but no handoff.md/API behavior matrix was present when inspected. The latest thread snapshot was active/in progress and still diagnosing a CommandBuffer behavior assertion. Per the user's instruction, this handoff does not wait for T1.
- T2: The current accepted checkpoint is `a1efd50` and remains `partial`. Its production World/identity contract is not integrated into this T4 checkout; T4 must continue to treat the production access signatures as unavailable here.
- T3: The current accepted checkpoint is `7e472d2`, an isolated compile-only result that remains `partial`. Its Arch lifecycle/relationship production contract is not integrated into this T4 checkout.
- Consequently, T2 production World/identity access and T3 Arch lifecycle/relationship resolution remain unavailable in the current T4 production checkout. T4-B1 production implementation is `blocked-by-prerequisite`.

## Validation and tests

The accepted B0/B1 commit changed only diagnostics. No T4 production project build, restore, test or simulation smoke was run for that committed batch. The T4 test selection budget and Arch-specific smoke are not claimed as complete. Prior T2/T3 builds and tests are external evidence only; they are not attributed to T4.

### Follow-up verification result (2026-10-08)

A transient packet-29 owner-rejection experiment was stopped and its source/test edits were discarded. It is not part of the accepted commit and does not count as completed T4-B2 work. Its verification result is retained here because it exposed the current gateway rejection behavior:

| Command | Scope | Exit | Warnings / errors | Result |
| --- | --- | ---: | ---: | --- |
| `dotnet build Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-restore -v:minimal` | Network verification project and dependencies | 0 | 0 / 0 | Exploratory build outputs were under `Build/bin/`, including `Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/NSSLC.Infrastructure.Network.Verification.dll`. |
| `dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore -- --projectile-runtime-owner` | Projectile network owner sample | -532462766 | 0 / 1 | Failed with `The gateway did not submit the projectile command to its Application owner.` A missing-identity packet was rejected first; `PacketGateway` propagated the rejected result as a protocol exception and closed that session, so the subsequent valid packet in the same peer could not run. The experiment was discarded without changing the accepted source tree. |

No follow-up verification was run. The current checkout contains no retained production or test edits from this experiment.

Static evidence commands and outcomes are in command-log.md. Source/input fingerprints and the consumed T2/T3 report hashes are in source-hashes.md. Prior and current inspection failures plus corrections are recorded in failure-audit.md.

## Changed files and commit

- Build/diagnostics/ArchMigration/T4/b0-b1-access-ownership-matrix.md
- Build/diagnostics/ArchMigration/T4/command-log.md
- Build/diagnostics/ArchMigration/T4/failure-audit.md
- Build/diagnostics/ArchMigration/T4/handoff.md
- Build/diagnostics/ArchMigration/T4/source-hashes.md
- Build/diagnostics/ArchMigration/T4/environment.txt (existing T4 environment record)

Accepted B0/B1 commit: `9bdde4eb7b316e27d78eb6463da68355dafd6fb9`.

## Open findings and uncovered scope

- No B1 native query/ref migration, production project build, or Arch smoke.
- No T4-B2 queued network request rewrite or DTO serialization scan against a migrated Arch entity model.
- The transient packet-29 rejection probe above did not complete T4-B2; protocol rejection closes the gateway session, and no nonfatal rejection contract was implemented.
- Packet 29 stale/missing/inactive/duplicate terminate currently returns accepted no-op after ignoring TryTerminateNetwork false; re-evaluate once T2/T3 contracts are integrated.
- No T4-B3 owner-specific item mutation conflict replacement or quantity conservation verification. Existing WorldItemReservationSystem is a separate named domain protocol; RuntimeItemRegistry still composes generic component revisions.
- Historical plan item only: the approximately 10% T4 core behavior-test sample was not run. It is inapplicable to the current accepted compile-only gate and remains runtime-unverified, not passed.
- No full Simulation/NetworkServer integration, cross-world stale request proof, Arch ref/structural boundary proof, pickup expiry/duplicate proof, or DTO/wire proof.

## Next owner action

After T1's accepted API matrix and T2/T3 production contracts are available in an integrated checkout, resume T4-B1 from the recorded candidate order. Start with one ordered, non-structural per-entity access slice; keep the existing owners and slot traversal; then prove one ref/copy/structural boundary before expanding. Follow with T4-B2 queue intent and consumption-time re-resolution, T4-B3 named item conflict protection, then the explicitly budgeted T4-B4 sample.
