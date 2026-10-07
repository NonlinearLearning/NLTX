# T1 API behavior matrix — Arch 2.1.0

**Status: partial.** Package restore, package signature verification, probe build, selected risk cases,
and fixed-source comparison are recorded. The final CommandBuffer case terminated the process with an
`AccessViolationException`; inspection of Arch's `Chunk.cs` implementation is
**blocked-by-prerequisite** because the source snapshot is absent and further capture was stopped for
the accepted T1 closeout. The internal cause is not verified.

## Provenance

- Package: `Arch 2.1.0`, package SHA-256 `F29C58EC7A884DF0411FB1A54E154A2C1597A9B7F8815A8C29AD0CB0E33A7505`.
- Probe target: `net10.0`; restore selected Arch's `lib/net8.0/Arch.dll` runtime asset.
- NuGet repository signature verification completed successfully.
- Pinned source comparison: commit `04d52e7268eb6f376ca4841a0c204334120c5e9d`; captured file hashes are checked in `pinned-source-verification.json`.

## API matrix

| Area | Arch behavior or guarantee | Evidence and result | NLTX owner responsibility / limit |
|---|---|---|---|
| World identity | World IDs can be released and reused after destroy. | `world.id-reuse` passed in the final selected run. | Do not use a recyclable World ID as a durable session identity. Pair it with the NLTX session/generation token. |
| Entity identity | Entity identity/equality includes `Id`, `WorldId`, and `Version`. | Pinned `Entity.cs` lines 145–210; probe also checks equality. | Persist domain UUIDs independently from Arch's runtime fields. |
| `World.IsAlive` | Checks positive version, local entity-data existence, and version equality; it does not compare `WorldId`. | `world.isalive-worldid-boundary` passed: an entity with colliding local Id/version is alive in another World. `upstream-World.cs` lines 1640–1649. | Every caller accepting an entity must validate `WorldId` and the current NLTX session token before calling Arch. `IsAlive` alone is not an ownership guard. |
| Component refs and structural moves | `Get<T>` / `TryGetRef<T>` expose mutable refs. Structural changes can move an entity between archetypes; the caller must reacquire a ref afterwards. The normal `World.Move` path updates `EntityInfo` for any swapped entity and writes the moved entity's new archetype and slot into its `EntityData`. | `component.struct-class-access-critical` passed, including ref reacquisition after Add. Static comparison: `upstream-World.cs` lines 325–347. | Do not retain component refs across Add/Remove or other structural changes. The source review does not stress every `EntityInfo` capacity path. |
| Struct/class component behavior | A returned struct copy does not commit edits; class components are reference values, and separately created class instances remain independent. | `component.struct-class-access-critical` passed. | Preserve class aliasing semantics deliberately in adapters; do not assume Arch deep-copies reference components. |
| Query composition | All allows extra components; Any matches at least one requested type; None excludes any requested type; Exclusive requires the exact signature. | `query.composition-critical` passed for all four in one composed scene. | T4 may depend on the tested composition semantics; network authorization and ownership remain outside query semantics. |
| `CommandBuffer.Create` | Create returns a negative-ID staged entity; playback creates it. The source registers that ID to a buffer index (`Size` before increment) and stores the same index in `CreateCommand`. | Assertions before the failing section in `command-buffer.create-and-groups-critical` completed; `upstream-CommandBuffer.cs` lines 138–182. | Treat staged IDs as buffer-local temporary handles. A multi-World/same-local-Id buffer scenario and stale staged handles across buffer reuse were not runtime-probed. |
| `CommandBuffer` operation groups | Playback groups operations in order: Create, Add, Set, Remove, Destroy. `Add<T>` writes both the structural Add set and the shared Set value set, so later recorded values overwrite that value slot. | `upstream-CommandBuffer.cs` lines 238–251 and 283–400. One earlier assertion expected the prior Set value and failed; pinned source corrected that assumption. Final value-only assertion was not reached in the process-aborting run. | Do not interpret playback as a transaction. Do not rely on unobserved rollback/partial-state behavior. |
| `CommandBuffer.Set` for absent component | The selected malformed-target case reached `Chunk.GetArray(ComponentType)` and terminated the process with `System.AccessViolationException` (`-1073741819` / `0xC0000005`). | Original output is preserved in `core-risk-sample-explicit-project.log` and `commandbuffer-access-violation.raw.txt`; caller site is `upstream-CommandBuffer.cs` line 349. | Treat this path as blocked/unsafe for the migration contract until a separately approved investigation. The `Chunk.cs` source and post-crash world state were not verified. |
| `Clear`, stale handles, retained commands, destruction order | Probe cases exist for these behaviors, but they were not in the selected risk sample. | See `selected-sample-report.md`; cases are listed as skipped. | Do not infer these behaviors from the selected run. T3 owns lifecycle coverage. |

## Three source hypotheses checked locally

1. **Create ID/index mapping conflict:** the pinned source shows each normal staged Create uses `-(Size + 1)`, stores the buffer entry at index `Size`, then records `CreateCommand(Size - 1)` after `Register` increments `Size`. This aligns generated ID lookup with the entry index for a live buffer. It does not prove behavior for cross-World entities with equal local IDs or stale handles.
2. **Add/Set shared value slot:** confirmed. `Add<T>` records the structural add and writes the component value through the same `SetIndex` used by `Set<T>`. The actual value-only runtime assertion was not reached after the process-level crash.
3. **EntityData stale after archetype movement:** rejected for the normal `World.Move` path by source. It repairs the entity displaced by source removal and updates the moved entity's `EntityData.Archetype` and `EntityData.Slot`. This is source evidence, not a capacity stress test.
