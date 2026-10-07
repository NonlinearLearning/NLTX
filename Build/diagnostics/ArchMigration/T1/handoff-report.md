# T1 handoff — Arch 2.1.0 baseline and API probe

**Document:** DOC-2026-10-08-ARCH-TRACK-T1  
**Status:** `partial`  
**Commit reference before metadata amendment:** `17c889b3e39fbd010c41da78d3c5b8aef60850b7`  
**Base:** `e2c686790ab6a4f14505ea914c27478f5959d061`  
**Probe:** `Test/Terraria.Arch.Verification/`  
**Pinned source:** Arch commit `04d52e7268eb6f376ca4841a0c204334120c5e9d`

## Completed evidence

- Captured the current source/reference baseline and exact owner/reference inventory in `baseline-inventory.json`, `baseline-reference-owners.json`, and supporting files. The read-only `src/NSSLC.Infrastructure/分类参考/` tree was excluded from the precise legacy-reference scan.
- Added an isolated `net10.0` verification project pinned to Arch 2.1.0. No production ECS, protocol, host composition, or domain component path was modified.
- Restore selected Arch's `lib/net8.0` asset. The NuGet package repository signature verified successfully. Restore, signature verification, and final probe build report zero warnings and zero errors.
- Built a 44-case explicit probe matrix and selected five representative risk cases (11.36%). Four completed successfully in the final sample.
- Compared captured fixed-source files against their recorded SHA-256 values. Three source hypotheses are documented in `api-behavior-matrix.md`; the local audit output is `pinned-source-verification.json`.
- Preserved the incorrect CommandBuffer assertion failure, the source-based correction, and the final raw AccessViolation output.

## Blocking observation

The final selected run passed World ID reuse, cross-World `IsAlive` boundary, critical component access, and query composition. The fifth case then recorded `Set` to an entity lacking the component, recorded `Add` to another entity, and called `Playback`. The process terminated in `Arch.Core.Chunk.GetArray(ComponentType)` from `Arch.Buffer.CommandBuffer.Playback` with `System.AccessViolationException`; process exit was `-1073741819` (`0xC0000005`). The exact output is retained in `commandbuffer-access-violation.raw.txt` and `core-risk-sample-explicit-project.log`.

This is not evidence that the entire API or migration is unusable. It is a concrete blocker for this malformed-target CommandBuffer path. `Chunk.cs` was not captured, so the invalid internal index/access mechanism is **not verified**. Since the process died, the world state after the earlier Add and any recovery/rollback behavior are also **not verified**. Do not treat CommandBuffer as transactional.

## Arch guarantees and NLTX-owned boundaries

| Concern | Evidence from Arch / observed behavior | NLTX owner must enforce |
|---|---|---|
| Entity equality | Arch Entity equality includes local Id, WorldId, and Version. | Durable identity remains an NLTX domain/session identity, not an Arch Entity value. |
| World ownership | `IsAlive` checks local Id and Version and can return true for a foreign entity with colliding local fields. | Check WorldId and the active session/generation token before every entity operation at caller boundaries. |
| Component references | Structural mutation may move entities and invalidate previously held component refs. The selected component case reacquired successfully; pinned `World.Move` repairs entity-data slots. | Reacquire refs after structural changes; do not keep component refs in queued work or across owner-thread mutations. |
| Query composition | Selected All/Any/None/Exclusive semantics passed. | Queries select data; they do not supply identity, authorization, network ownership, or thread safety. |
| CommandBuffer | Playback processes Create, Add, Set, Remove, Destroy groups. Add writes the shared Set value slot. The selected malformed Set path crashed the process. | Treat temporary IDs as buffer-scoped; serialize playback on the owning World thread; do not rely on rollback; keep absent-component Set out of migration assumptions until separately resolved. |
| Lifecycle and reuse | World IDs and entity IDs can be reused; only the selected World ID reuse case completed. | T2/T3 own session identity, stale-handle rejection, lifecycle ordering, and cleanup policy. |
| Performance | No production host or workload was benchmarked. | T5 owns representative performance and host integration measurements. |

## Exact remaining coverage

- 39 of 44 probe cases were not selected. In the selected fifth case, staged creation assertions ran before the failing Set stage, but the composite case as a whole did not complete.
- `Chunk.cs` implementation, malformed Set behavior, process post-crash state, transaction/rollback behavior, same-buffer cross-World ID collisions, stale temporary handles after buffer reuse, full solution build/tests, host assembly, and production integration remain unverified.
- No A0 input-world directory was present in `Build/diagnostics/ArchMigration/A0/`; no input world hash was fabricated.
- T1 does not claim production ECS migration completion.

## Next owner actions

- **T2 — World identity/signatures:** require WorldId plus NLTX session/generation ownership at all entity entry points; avoid persisting Arch IDs as durable identity.
- **T3 — Lifecycle/relationships:** cover destroy/clear/reuse and stale references; own entity creation/destruction and cross-entity relationship policy.
- **T4 — Query/network/items:** use the tested composition semantics while keeping network authority, owner-thread scheduling, and domain filtering explicit; do not use `IsAlive` as a World ownership check.
- **T5 — Host/performance/exit:** plan host-thread playback and measure representative workloads after integration; no performance claim is supported by T1.
- **Coordinator:** keep T1 partial until the CommandBuffer crash path and missing `Chunk.cs` source evidence are independently resolved or explicitly excluded from the agreed migration contract.

## Committed artifact boundaries

The scoped T1 commit contains the verification project and `Build/diagnostics/ArchMigration/T1/` evidence. It excludes the pre-existing untracked T2–T5 planning files and all `Build/bin`, `Build/obj`, package cache, and generated build output.
