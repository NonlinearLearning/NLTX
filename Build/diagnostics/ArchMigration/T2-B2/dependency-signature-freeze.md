# T2-B2.0 — Dependency and signature freeze

**Status:** frozen for B2.1 implementation; this is a design/closure record, not migration completion.
**Base:** `57f2c75b3f05bdb713bf421ae1ec7c2161f805e2` (`codex/arch-t2-probe-handoff`).
**Prerequisite:** T1 partial handoff at `0928445dbebdade501bcb39bb944541835538883`; its later blocker update is `937bfba2a739f47d9d2aa34e7e690050f9a68450`.

## 1. Direct package and project boundary

The current production projects have no direct Arch package reference. `Test/Terraria.ArchMigration.T2Probe` is the only project with `Arch 2.1.0` today. Production dependencies are added only where source uses an Arch type or API directly; the package version stays pinned to `2.1.0`.

| Project | Direct Arch 2.1.0? | Reason and boundary |
| --- | --- | --- |
| `Test/Terraria.ArchMigration.T2Probe` | Yes, existing | Isolated API probe only; it is not production evidence. |
| `src/NSSLC/Component/Share/Entity/Terraria.EntityEcs.csproj` | Yes, add in B2.1 | `EntityIdentityRegistry` will store complete `Arch.Entity` values and validate through its owning `Arch.World`. |
| `src/NSSLC.Application/NSSLC.Application.csproj` | Yes, add in B2.1 | `LoadedWorldSession` is the sole creator/owner of the session `World`; it creates the world singleton and exposes its guarded World access. |
| `src/NSSLC/Component/WorldStorage/Terraria.WorldStorage.csproj` | Conditional | Add only if its slot/TileEntity projection stores or validates `Arch.Entity` directly. It must not own a World or generic component access layer. |
| `src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj` | Conditional | Add only for migrated host/runtime code that directly calls World/Entity APIs. Its current legacy callers are not evidence that the package is needed before their signatures change. |
| `src/NSSLC/Component/Relationships/Terraria.Relationships.csproj` | No | Domain references retain UUID plus the session token; no Arch Entity enters relationship or network contracts in this batch. T3 owns relationship behavior. |
| `src/NSSLC/Component/WorldSession/Terraria.WorldSession.csproj` | No | Existing domain component definitions remain ordinary component data; the project does not own or create a World. |
| `src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj` | No, unless direct calls become necessary | Persistence and loading adapters consume domain values/DTOs through session owners. They must not become World owners. |
| Network, server, and world-generation projects | No, unless direct calls become necessary | Preserve token/UUID and DTO boundaries; do not persist or transmit `Arch.Entity`. |

No `Arch.Buffer.CommandBuffer` use is planned in this slice. T1's malformed-target `Set` AccessViolation, missing `Chunk.cs` source, and unknown post-crash/rollback state remain explicit exclusions. No `Arch.Relationships`, `Arch.System`, `Arch-Events`, or SourceGenerator dependency is introduced here.

## 2. Session-token decision

Reuse `Terraria.Relationships.EntityRuntimeId` as the sole session token. Its frozen representation is the existing immutable `Guid Value` plus `IsAssigned`; no equivalent `WorldSessionToken` type or second token field is added. The semantic contract is an in-memory world-session epoch, not an Arch World ID, entity ID/version, durable identifier, or protocol identity. Existing `WorldRuntimeId` and `EntityReference.RuntimeId` names remain at this cut to avoid a second competing token and will be described as this session token in updated contracts.

Lifecycle:

1. `LoadedWorldSession` captures its owner thread and creates a fresh `Arch.World`.
2. Only after `World.Create()` succeeds does it create `EntityRuntimeId(Guid.NewGuid())`, then construct the registry and world-scoped storage. A World creation failure publishes no token or registry.
3. If registry/storage initialization fails, the candidate is not published; it clears any constructed owners and destroys/disposes that candidate World. Its token is never exposed as a published session.
4. A successfully initialized candidate retains the same token through load, publication, ticks, and save. A retry or replacement creates a new World and token, even if Arch later reuses `World.Id`.
5. Requests carry the token and domain UUID/reference, never a retained component ref. Owner-thread entry points reject a stale token before touching the World.
6. Dispose first closes publication/request entry, revokes live identity mappings and domain projections, disposes storage, then releases the World and marks the session disposed. Repeated Dispose is idempotent; failed World disposal remains a failure and does not report a clean session.

`EntityRuntimeId` generation will move out of `EntityRuntime` and into the successful `LoadedWorldSession` World-owner path. Network `WorldRuntimeId` fields remain session-token projections, not Arch identifiers.

## 3. Registry contract

The only live identity mapping is `EntityUuid ↔ (EntityRuntimeId session token, Arch.Entity)`, owned by one `LoadedWorldSession`/`EntityIdentityRegistry` pair. The reverse index uses the complete `(token, Entity)` value; it never stores only `Entity.Id`. `_issued` UUID history remains per registry/session and survives unregister until that registry is retired, preventing accidental reissue within one session. Durable UUID creation and real-rebuild UUID replacement remain domain-owner decisions.

**2026-10-08 correction from B2.1 source/test inspection:** the `_issued` ownership sentence above is superseded. Issued UUID history must survive candidate-session replacement, so it belongs to an `EntityUuidIssuer` whose lifetime spans those candidates; live UUID-to-entity mappings remain scoped to the session registry. `EntityIdentityRegistry` now accepts an injected issuer, but its convenience constructor creates a new one, and `LoadedWorldSession` still accepts or creates a registry rather than a long-lived issuer. Production wiring therefore does not yet guarantee history continuity across candidate sessions. This corrects the ownership decision; it does not claim that session-scoped Arch entity mapping is implemented.

- Registration is owner-thread-only and occurs while a candidate entity is being built. Before publishing either side, validate current token, `entity.WorldId == world.Id`, and `world.IsAlive(entity)`; attach the existing identity data and register the exact pair. If attach or map insertion fails, undo whichever step succeeded.
- Resolution order is fixed: expected session token → `Entity.WorldId` equals the owning World → `World.IsAlive(entity)` → required lifecycle/ability state → requested component access. `IsAlive` alone is insufficient because T1 observed a foreign-World local Id/version collision returning true.
- Resolve by `EntityReference` first matches its token and UUID to the live map, then applies the same WorldId/IsAlive checks. Old token, foreign World, stale Version, disposed World, and terminating/ineligible entities are named rejections.
- Unregister is idempotent and conditional on the exact `(token, Entity, UUID)` mapping. It cannot remove a new mapping after local ID or slot reuse. Normal death/respawn retains UUID; actual destroy/recreate receives a new UUID from the domain creation owner.
- The registry has one owner-thread check. No queued request retains component refs; refs are reacquired after structural changes.

## 4. Production caller closure and handoff

The frozen scan is the existing `Build/diagnostics/ArchMigration/T2/caller-inventory.csv` and `project-reference-graph.txt`: 111 production source files across `NSSLC` (18), `NSSLC.Application` (48), `NSSLC.Infrastructure` (16), `NSSLC.Tools.NetworkServer` (1), `NSSLC.Tools.Simulation` (26), and `NSSLC.Tools.WorldGeneration` (2); the inventory records per-file symbols and ownership. The current narrow scan confirms the concrete construction and storage root edges below.

| Closure slice | Exact source entry points / owners | T2 action |
| --- | --- | --- |
| Runtime and UUID authority | `src/NSSLC/Component/Share/Entity/System/EntityRuntime.cs`; `EntityIdentityRegistry.cs`; `ComponentAccess.cs`; `ComponentStore.cs`; `src/NSSLC/Component/Relationships/System/RuntimeEntityHandle.cs`; `EntityRuntimeId.cs` | Replace production live mapping with complete Arch Entity plus session token; do not add a generic Arch facade. |
| Session and storage construction | `src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs`; `src/NSSLC/Component/WorldStorage/System/WorldStorageRoot.cs`; `TileEntityStore.cs`; `TileEntityRecord.cs`; `EntitySlotStore.cs`; `src/NSSLC/Component/WorldStorage/TileEntityBindingComponent.cs` | Single World owner; migrate root and slot projections; keep capacity/order/cooldown domain behavior. |
| Standalone constructors to close | `src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs` (lines 79, 413); `RuntimePlayerStore.cs` (lines 38, 402); session constructor in `LoadedWorldSession.cs` | Remove independent runtime creation from production assembly; use the session World/registry owner. |
| Simulation runtime consumers | `RuntimeNpcEntity.cs`, `RuntimeNpcStore.cs`, `RuntimePlayerEntity.cs`, `RuntimePlayerStore.cs`, `RuntimeProjectileStore.cs`, `RuntimeItemRegistry.cs`, `RuntimeWorldItemStore.cs`, `RuntimeWorldItemStore.NetworkSync.cs`, `RuntimeSpatialEntityVerification.cs`, `RuntimeWorldLoadRollbackVerification.cs`, `RuntimeTileEntityReloadVerification.cs`, and `Program.cs` | Migrate only signatures/identity access needed to compile and exercise the T2 owner. Do not rewrite T3 lifecycle or T4 item/network rules. |
| Application/network callers | `NetworkPlayerOwner.cs`, `NetworkPlayerOwner.Loadout.cs`, `NetworkPlayerOwner.State.cs`, `NetworkWorldItemOwner.cs`, `NetworkWorldOwner.cs`, `ProjectileNetworkCommandOwner.cs`, `SocialNpcEffectOwner.cs`, `WorldSimulationKernel.cs`, `WorldLoadCoordinator.cs`, `WorldLoadRecoveryCoordinator.cs` | Preserve behavior; resolve token/UUID at owner-thread entry. Business logic stays with T3/T4/T5. |
| World authority reads/writes | `src/NSSLC/Component/WorldSession/System/WorldSessionRestoreState.cs`; `WorldSessionRestoreSystem.cs`; `WorldSpawnConfigurationRestoreSystem.cs`; `src/NSSLC.Infrastructure/WorldStorage/Generation/LoadedWorldPersistenceSnapshotSource.cs`; `LegacyWorldSessionProjection.cs`; `WorldStorageCoordinatorFactory.cs`; `src/NSSLC.Tools.WorldGeneration/WorldRoundTripMetadataComparison.cs`; `src/NSSLC.Tools.Simulation/Program.cs` | Move authoritative Rules/TimeWeather/Progression/Descriptor/etc. to existing components on the World singleton. Keep persistence DTO/snapshot inputs as values, not writable runtime authority. |

Full reference counts and remaining source files are in the referenced per-file inventory; after the first production build, every remaining reference must be classified as migrated, T3/T4/T5-owned, or read-only reference material. The `src/NSSLC.Infrastructure/分类参考/` tree is excluded and remains read-only.

Out-of-scope owners and exact handoff:

- **T3:** relationship semantics, lifecycle and destruction rules: `NpcTaskLifecycleSystem.cs`, `NpcTaskReference.cs`, `LeashedEntityRegistrationSystem.cs`, `ProjectileLifecycleSystem.cs`, `ProjectileStaticNpcImmunitySystem.cs`, `ProjectileTickContext.cs`, and `ProjectileTickCoordinator.cs`; also parent-child relationship cleanup in `RuntimeNpcStore.cs`. T2 may make compile-only entity/token signature changes and must not alter business lifecycle behavior.
- **T4:** query/system/network/item business behavior: `NetworkPlayerOwner*.cs`, `NetworkWorldItemOwner.cs`, `NetworkWorldOwner.cs`, `ProjectileNetworkCommandOwner.cs`, `RuntimeItemRegistry.cs`, and `RuntimeWorldItemStore*.cs`. Queue payloads remain token/UUID/intent values; no command-buffer transaction assumption.
- **T5:** scheduling and host lifecycle/performance: `WorldSimulationKernel.cs`, `ActiveLiquidTickPhase.cs`, `ActivePressurePlateTickPhase.cs`, `ActiveTileEntityTickPhase.cs`, and the Simulation host tick/exit path. T2 provides the World/token/Dispose entry point and does not convert the system model or claim performance.
- **A8 / final exit audit:** old generic implementation files `EntityRuntime.cs`, `ComponentStore.cs`, `ComponentAccess.cs`, and `RuntimeEntityHandle.cs` have no permanent compatibility role. T2 must remove active production callers in its closure or state an exact remaining owner/file/symbol; final repository-wide deletion authorization remains outside B2.

## 5. First implementation slice and verification

First source slice: change `EntityIdentityRegistry` from the old runtime-handle maps to token-scoped full Arch Entity mappings, and make `LoadedWorldSession` create/own the Arch World and session token before storage construction. Add `Arch 2.1.0` directly to `Terraria.EntityEcs.csproj` and `NSSLC.Application.csproj`; do not add it to relationship, network, persistence, or every component project by transitive convenience. Continue the slice through the actual constructor/caller compile closure without retaining a legacy overload or two writable state paths.

Initial verification commands, after the relevant edits and with SDK `10.0.400`:

```powershell
dotnet restore src/NSSLC/Component/Share/Entity/Terraria.EntityEcs.csproj
dotnet restore src/NSSLC.Application/NSSLC.Application.csproj
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --nologo
```

Record each command's exit code, warning/error counts, `Build/bin/` output, source/assets hashes, and output DLL/PDB hashes under `Build/diagnostics/ArchMigration/T2-B2/`. This first build verifies only the edited compile closure; it is not a replacement for the five selected B2 behavior cases or the production one-tick/exit run.

## 6. Evidence status

- **Observed:** T1 Arch 2.1.0 `WorldId`/`Version`/`IsAlive` boundary probe and partial handoff; malformed-target CommandBuffer path remains excluded.
- **Observed in current T2 source:** the direct constructors and state-read/write call sites listed above; no production Arch package reference is present at this freeze point.
- **Freeze scan command:** `rg -n --glob '!**/分类参考/**' 'new EntityRuntime\s*\(|new WorldStorageRoot\s*\(|WorldSessionRestoreState' src` plus the existing per-file caller inventory and project-reference graph.
- **Inventory SHA-256:** `caller-inventory.csv` = `893954FAFCE3A092BB8CCE4A7781A530D3F15A8183FFBDFC4CAAA92BEF75B9A1`; `project-reference-graph.txt` = `736E8720F096C46C90FEB7E8A22B8B85F90630F8E98F52E815E69C33BC3B2FBE`.
- **Owner source SHA-256 at freeze:** `LoadedWorldSession.cs` = `E8FC0BF003C50D26CC1F96DE4B443DACFB82C2DEEB51252FFB937F7CA06909EE`; `EntityIdentityRegistry.cs` = `CB8328EC854152AB7E83922924F898947CBD2B0A0BA5B133EC840C0AB4DEA87A`; `EntityRuntime.cs` = `981F4F029600688FCF679F02574F75B1B99F8EDB0DC854AD54FCB5E6F21FAA89`; `WorldStorageRoot.cs` = `690A5C62A0E13649B04E244A9564770AF30C880E13F062C72BD8ECDCEB7DFE24`; `EntityRuntimeId.cs` = `E765B17EF115091BE31AC0F2A99E38231917820B478CE3033FF7701074DCF94C`.
- **Not run:** production restore/build, production World/session/token/registry behavior, world singleton, candidate-failure cleanup, slot/entity reuse after migration, and one-tick/exit on Arch.
- **Next state:** begin B2.1 implementation; do not describe this freeze as production migration progress or as A2 completion.
