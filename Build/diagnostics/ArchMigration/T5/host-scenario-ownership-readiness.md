# T5-B0 host and world ownership readiness

| Status | Snapshot date | T5 source base | Source fingerprint |
| --- | --- | --- | --- |
| **preparatory / blocked-by-prerequisite** | 2026-10-08 | `e2c686790ab6a4f14505ea914c27478f5959d061` | `D22E54FEBE9AD29ECF8DB40E872A77D0A7195ECA3174C1625265B23527FC0CFE` |

This document maps the current host/load ownership and the evidence needed for the T5 scenarios. It does not propose new Arch signatures or claim a migrated path. The T5 checkout still has no Arch production package/API. The only passing host smoke remains the custom ECS load → one tick → exit baseline documented in `host-smoke-319.json`.

## Dependency snapshot

| Track | Current evidence | T5 consequence |
| --- | --- | --- |
| T1 | Commit `17c889b`, status `partial`. Four of five selected probe cases passed; the CommandBuffer case crashed in playback with a process-level AccessViolation. `Chunk.cs`, rollback state and the post-crash world were not verified. | T1 explicitly keeps the track partial until the failure is resolved or the relevant path is explicitly excluded. Do not build host ownership on assumed CommandBuffer rollback. |
| T2 | Commit `57f2c75` adds an isolated Arch probe; its handoff reports `partial / blocked-by-prerequisite`. No production signature migration is present in the T5 base. | The intended session token and UUID mapping are planning constraints, not integrated production APIs. |
| T3 | Commit `f0a34c4`, status `partial / blocked-by-prerequisite`; current custom-runtime lifecycle evidence and one verifier adjustment are recorded. | No migrated caller demonstrates Arch creation, relationship cleanup or failed-candidate disposal. |
| T4 | Commit `9bdde4e` contains the B0/B1 static owner/order matrix, status `partial`. A separate active T4 worktree has uncommitted network-owner and verifier edits; those edits are outside this T5 base. | Preserve current slot, phase, RNG and network owner contracts; do not treat the in-progress edits as accepted Arch access APIs. |

The six plan documents remain active in the T5 checkout. The coordinator has accepted the T5 partial baseline handoff; that acceptance is limited to the baseline and this preparatory matrix. It does not accept T1–T4 as complete or approve A6–A8 production work. No frozen A0 performance budget is present in this evidence set.

## Scenario ownership matrix

| Scenario | Current observed owners and boundaries | Target ownership constraint to carry forward | T5 evidence / gate |
| --- | --- | --- | --- |
| Candidate load, before publication | `WorldStorageCoordinatorFactory.RunGeneratedWorldLoad` creates a fresh `LoadedWorldSession`; `WorldLoadCoordinator` reads, decodes, validates and applies API bindings. `LoadedWorldSession` currently creates `EntityRuntime` and `WorldStorageRoot`. | One candidate session must own one runtime `Arch.World` plus a unique session/generation token. Domain owners prepare and validate values before commit; codecs and file adapters keep consuming persistence DTOs and must not query or mutate the World. | Source audit only. No Arch candidate or publication smoke. |
| Successful load, one tick, exit | The loader publishes only after recovery/effect steps settle. `WorldSimulationKernel` captures its owner thread, owns command drain/tick commit, and orders registered phases. Session disposal currently disposes storage before `EntityRuntime`. | The host creates one kernel against the published session; World mutations and any buffer playback stay on its owner thread. Queued work carries intent plus session identity and resolves entities when consumed. Stop the kernel before retiring the session-owned World. | One passing custom ECS smoke only; Arch ownership and disposal order unverified. Preserve T4's explicit phase, slot and RNG order. |
| Two consecutive world switches | The factory stages a candidate against the current active session, publishes it, records a pending previous-session retirement, and disposes the previous session only after the candidate settles. Reset/recovery paths can restore the previous projection. | Each candidate gets a new token and World. Old queued work, callbacks, relationships, slots and network bindings must fail resolution against the new token. Retire the old World only after host cleanup and replacement commit succeed. | Not run. Verify both switch directions and that no old reference resolves in the replacement session. |
| Cancellation before candidate commit | `CancellationToken` is checked at load and projection boundaries. Cancellation cannot undo a file read or any already completed owner effect by itself. A candidate that did not publish is disposed in the factory's `finally`. | Cancel/abort the candidate, clean its identity mappings, registrations, relations and buffers, then dispose its World. Keep the prior active session usable until replacement publication commits. Record any external effect that has an unknown cancellation result. | Not run. Must assert prior session identity and state remain usable. |
| Late-finalize failure, rollback, retry | Recovery effect finalization and publication have separate stages; candidate publication may be uncertain while projections/reset effects run. The existing factory preserves/reprojects the previous session on failure. | Roll back candidate-owned projections and registrations first, revoke candidate UUID mappings and relationships, then dispose the candidate World. A retry creates a fresh World and token; it must not reuse a partially finalized candidate. | Not run. T3 has no Arch caller evidence and T1 does not prove CommandBuffer transactional behavior. |
| Zero-tick exit | `WorldSimulationKernel.Step` returns without committing a tick when cancellation or stop is already requested; the T5 finite-host smoke covered one tick only. | The host must distinguish “published but zero ticks” from “load never published”, record the stop reason and configured save policy, and release the kernel and session in the correct order. | Not run; host CLI and shutdown behavior need a focused scenario. |
| Save and reload | Snapshot capture reads `LoadedWorldSession` value snapshots; `WorldSaveSnapshotCoordinator` passes a `WorldPersistenceDocument` to `WorldSaveCoordinator`. The latter owns encode, storage write, read-back and validation. | Persist domain UUIDs and DTO values, never Arch `Entity`, `WorldId`, version, component refs or World internals. Reload into a new World/token and rebuild runtime UUID mappings during domain-owner commit. Infrastructure remains outside ECS. | The 319 fixture roundtrip was legacy DTO/Codec evidence, not Arch save/reload. No Arch persistence smoke. |

## Current source anchors

- `src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs`: session construction, current `EntityRuntime` ownership, storage creation and disposal order.
- `src/NSSLC.Infrastructure/WorldStorage/WorldStorageCoordinatorFactory.cs`: `RunGeneratedWorldLoad`, candidate disposal, publication, staged replacement and previous-session retirement/recovery.
- `src/NSSLC.Application/WorldStorage/Persistence/WorldLoadCoordinator.cs`: file/decode/validate/API-binding load path and cancellation boundaries.
- `src/NSSLC.Application/Simulation/WorldSimulationKernel.cs`: owner-thread check, queued command drain, tick stop and ordered phases.
- `src/NSSLC.Application/WorldStorage/Persistence/WorldSaveSnapshotCoordinator.cs` and `src/NSSLC.Application/WorldStorage/Persistence/WorldSaveCoordinator.cs`: value snapshot/document boundary and file commit/read-back validation.
- `src/NSSLC.Infrastructure/WorldStorage/Generation/LoadedWorldPersistenceSnapshotSource.cs`: active/published session checks and owner-thread value snapshot capture.
- T4's `b0-b1-access-ownership-matrix.md`: phase order, slot iteration, spawn visibility, RNG consumption, network queue capture risks, item reservation/revision boundaries and ref invalidation constraints.
- T1's committed `api-behavior-matrix.md`: observed Arch identity, `IsAlive`, ref invalidation and query composition behavior, plus the unverified CommandBuffer crash path.

## Gates before A6/A7/A8

1. T1–T4 owners and the total-acceptance owner must resolve which upstream partial handoffs are accepted for A6–A8; the T5 baseline acceptance does not clear that gate. T1's CommandBuffer failure must be fixed or explicitly excluded for the host path.
2. T2 must provide merged production World/session/token/identity signatures; T3 and T4 must consume those signatures in their lifecycle, query, network and item paths. Sibling-worktree reports alone do not prove the T5 checkout compiles those APIs.
3. A0 must freeze workload and thresholds before a performance comparison. This note authorizes no benchmark and supports no performance claim.
4. After those gates, run the two-switch, cancellation, zero-tick, late-finalize rollback/retry, and Arch save/reload scenarios against the actual host path. Re-run the required 10% selection from the accepted T5 matrix; the previous one-case custom ECS smoke does not cover it.
5. Only then re-evaluate A8 using production compile inclusion and dependency closure. The existing CSV is a candidate inventory, not a deletion authorization.

No source, project, plan or constraint file was changed to produce this matrix. It records the current gap and preserves the original T5 completion scope.
