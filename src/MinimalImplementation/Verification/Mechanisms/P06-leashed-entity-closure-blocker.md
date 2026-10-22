# P06 LeashedEntity and TileEntity Closure Blocker

Date: 2026-10-15 (Asia/Shanghai)

Disposition: production closure is blocked. This report records the evidence and the next
owner seams; it does not claim Version4 parity or a runnable P06 server tick.

## Scope and decision

P06 may own mechanism semantics and TileEntity state, but P02 still owns actor/session lifecycle,
P03 owns world tiles and section authorization, P04 owns inventory mutation, P05 owns NPC state,
and the root owns composition, packet registration, and the serialized build window. This is the
boundary in `docs/plans/minimal-implementation/P06-mechanisms-implementation-plan.md:9-30`.

No production motion provider, constant-input provider, gravity-only substitute, NoOp effect
provider, or inferred default facts were added. The two closure blockers are:

1. TileEntity placement is registry/projection work only; it is not an atomic P03 tile/entity
   commit.
2. LeashedEntity has wire-state storage and explicit ports, but no source-backed production
   motion/effect provider, non-wire runtime state, anchor lifecycle adapter, or registered tick
   owner.

## 1. TileEntity and spatial-interaction evidence

The current TileEntity foundation is real but incomplete:

- `src/MinimalImplementation/Mechanisms/TileEntityRegistry.cs:48-92` validates an anchor snapshot,
  allocates P06 identity/IDs, and adds the entity to P06 indexes. It does not receive a P03
  prepare/commit token or commit version for the tile mutation.
- `src/MinimalImplementation/Mechanisms/TileEntityPacketHandler.cs:47-175` validates a supplied
  `WorldSnapshot`, checks geometry and anchor predicates, creates the registry entity, registers a
  pylon when applicable, and creates a sharing intent. The handler has no `MechanismWorldPort`
  dependency and never calls `ApplyTileDeltas`.
- `src/MinimalImplementation/Mechanisms/MechanismWorldPort.cs:5-29` exposes only
  `CaptureSnapshot()` and `ApplyTileDeltas(...)`. It has no placement transaction, section
  authorization query, expected-version prepare, or rollback/commit coupling to the registry.
- `src/MinimalImplementation/World/WorldSnapshot.cs:228-289` supplies immutable tiles, geometry,
  epoch, version, and dirty sections. It does not supply TileEntity lifecycle, NPC facts,
  inventory ownership, or leashed runtime state.
- `src/MinimalImplementation/Mechanisms/TileEntityProjection.cs:22-164` produces deterministic
  packet records from already-committed P06 state; projection is not a world-tile writer.
- `src/MinimalImplementation/Host/WorldAttachmentHydrator.cs:99-157` restores persisted
  TileEntities after checking bounds (`:207-227`) and commits the P06 owner. That is load
  hydration, not the missing placement-time P03/P06 atomic transaction.

The relevant Version4 spatial call chain is present only as reference evidence:

- `D:\TRbackup\Version4\Terraria.DataStructures\TileEntitiesManager.cs:30-45` registers the
  two leashed anchor types, while `:67-85` routes network placement to a type implementation.
- `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:2545-2573` has packet 86's receive branch
  break before reading and routes packet 87 to `TileEntity.PlaceEntityNet`.
- `D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs:137-145` performs the in-world /
  occupied-position gate, but `:97-99` leaves the base `Place` implementation empty in this
  snapshot; concrete leashed anchor validity/instance methods are also empty/default.
- `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:3310-3319` reads packet 156 and calls
  `TELeashedEntityAnchorWithItem.InsertItem` without an inventory transaction.
- `D:\TRbackup\Version4\Terraria\NetMessage.cs:2223-2227` writes TileEntity records in a
  section packet. The inspected scan at `:2010-2072` contains entity cases through pylon type
  597, with no 723/724 leashed-anchor cases; it is not sufficient evidence for a complete
  leashed-anchor section projection.
- Current production construction at
  `src/MinimalImplementation/Host/ServerComposition.cs:157-204` creates the TileEntity registry,
  projection, packet handler, and leashed registry/module objects, but not a world transaction
  adapter or P03-to-P06 tile lifecycle event.

Therefore packet 87/156 and tile-break/entity-removal behavior cannot be closed without the P03,
P04, P02, and P05 seams. A registry-only placement would permit entity/tile divergence and is
not accepted as production behavior.

## 2. LeashedEntity implementation and provider audit

The section service deliberately delegates prototype behavior:

- `src/MinimalImplementation/Mechanisms/LeashedEntitySectionService.cs:95-171` iterates active
  sections, calls `ILeashedEntityMotionPort.TryAdvance`, applies the returned Partial state to
  the registry, and returns state-change/periodic projection candidates.
- `src/MinimalImplementation/Mechanisms/ILeashedEntityMotionPort.cs:5-18` is only the movement
  port. `src/MinimalImplementation/Mechanisms/ILeashedEntityTickEffectPort.cs:3-13` is only the
  post-commit effect port.
- `src/MinimalImplementation/Host/LeashedEntityServerTickPhaseOwner.cs:7-41` requires both ports
  and publishes the returned result; it explicitly does not invent movement or discard effects.
- A repository search found no production class implementing either port. The only implementations
  are the verification fake `AdvancingLeashedEntityMotion` and recording effect in
  `src/MinimalImplementation/Verification/ServerTick/Program.cs:301-332`.
- `src/MinimalImplementation/Mechanisms/LeashedEntityRegistry.cs:17-24` stores only the wire
  `LeashedEntityState`, slot/type/anchor/section, identity, and revision. It does not store the
  algorithm state required by the reference prototypes.
- `src/MinimalImplementation/Mechanisms/LeashedPrototypeResolver.cs:26-39` returns the Walker
  prototype for an unknown NPC type. That fallback is not source-backed and is excluded from a
  production closure.
- `src/MinimalImplementation/Mechanisms/MechanismItemFacts.cs:8-15,18-63` defines anchor
  eligibility facts, but no `Version4MechanismItemFacts` instance is constructed or registered in
  the current production composition. `LeashedPrototypeResolver.TryResolveHeldItem` instead
  requires a caller-supplied item-to-NPC map.

The provider search also found no production adapter for `Main.WindForVisuals`, `Main.cloudAlpha`,
`LCG32Random`, NPC/Projectile dummy defaults, kite collision, or leashed-anchor lifecycle. The
existing `ProjectileCollisionService` at
`src/MinimalImplementation/Combat/Projectiles/ProjectileCollisionService.cs:31` is a projectile
domain service, not a kite-compatible leashed motion adapter.

## 3. Version4 leashed source identity and limitations

The current `D:\TRbackup\Version4` hashes are:

| Source | SHA-256 |
| --- | --- |
| `Terraria.GameContent\LeashedEntity.cs` | `DD8A2B1DCC4C3E73ADB65F5D14A39B4C6AD1D5389D972C2C1DDAB7CFA1E0775E` |
| `Terraria.GameContent.LeashedEntities\LeashedCritter.cs` | `23E26EF8C7893294D7BC5C9137862288BD3CF454538B6FBCABEFBB768D2B9E89` |
| `Terraria.GameContent.LeashedEntities\LeashedKite.cs` | `E6AF21C6ECB394672F72EF90E50579C545B7B08C78DDF23DCC575AEB92FFD2F4` |
| `Terraria.GameContent.Tile_Entities\TELeashedEntityAnchor.cs` | `8A4F7D2A369B439D8428AC365D2B807EA3BA8E1EE24212AAC438E4EBBCFAE08F` |
| `Terraria.GameContent.Tile_Entities\TELeashedEntityAnchorWithItem.cs` | `E9E423FA9876BA6D2F44AEFFBE60AE64872B5F0FA4C073B3421609D076EDCAB1` |
| `Terraria.DataStructures\TileEntity.cs` | `8E164326E4876888D111A0CDD125CD846F222D28BA3C65940B5269FFB51B8E1B` |
| `Terraria.DataStructures\TileEntitiesManager.cs` | `784156FBB35011C339E678C63468B44ACE617FB58BA6955424C1B3D03772660D` |
| `Terraria\MessageBuffer.cs` | `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE` |
| `Terraria\NetMessage.cs` | `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A` |

The source itself is not a complete behavior authority:

- `Terraria.GameContent\LeashedEntity.cs:233,290,301-306` leaves removal, streaming, spawn,
  despawn, update, draw, and network virtuals empty.
- `Terraria.GameContent.LeashedEntities\LeashedCritter.cs:42-49` leaves defaults, network,
  spawn, update, visual, dummy-copy, and draw methods empty. Its fields at `:15-39` show the
  non-wire state that a real adapter would need.
- `Terraria.GameContent.LeashedEntities\LeashedKite.cs:15-47` shows wind, distance, counters,
  local AI, trail, rotation, and net-offset state, while `:50-55` leaves network/update/spawn
  methods empty.
- `Terraria.GameContent.Tile_Entities\TELeashedEntityAnchor.cs:8-12` leaves placement/removal/
  world-load/respawn behavior empty. `TELeashedEntityAnchorWithItem.cs:10-11` leaves persistence
  reads/writes empty, even though `:23-29` mutates the item and requests respawn.
- `Terraria.GameContent.Tile_Entities\TEKiteAnchor.cs:23-47` and
  `TECritterAnchor.cs:25-49` leave validity, instance generation, item-fit, and concrete entity
  creation empty/default. The critter prototype setup is commented out at
  `TECritterAnchor.cs:51-70`.

The reference call order still identifies the intended lifecycle: `Terraria\Main.cs:3344-3347`
initializes TileEntities and registers leashed prototypes; `Main.cs:11572` calls
`LeashedEntity.UpdateEntities`; and `Terraria\WorldGen.cs:52726-52903` drops/removes leashed
anchor state on tile break. Those call sites do not supply the missing implementation bodies.

## 4. Registration and owner seams

Current root registration is partial:

- `ServerComposition.cs:178-204` registers the leashed registry, registry commit port, module
  handler, producer, and TileEntity packet handler.
- `ServerComposition.cs:339-353` exposes those objects, but exposes no
  `LeashedEntitySectionService`, motion provider, effect provider, or
  `LeashedEntityServerTickPhaseOwner`.
- The module-13 adapter exists in `src/MinimalImplementation/Host/CompleteModuleHandlers.cs:454-584`,
  but `ServerComposition.cs:463-481` does not add it to `CreateModuleDispatcher`; the pending
  route list still contains the server-authored module-13 rejection policy at
  `src/MinimalImplementation/Modules/DelegatedModuleRejectionHandler.cs:16-30`.
- No current `Version4InboundRouter` call site invokes `TileEntityPacketHandler.HandlePlacement`
  or provides packet-156 item insertion; construction alone is not packet registration.
- `P08BServerRuntimeIntegrations.cs:11-40,54` requires external tick-phase owners. The host
  explicitly records the missing complete owner set as a blocker at
  `P08BServerRuntime.cs:219-226`; it does not synthesize one.

The missing P06 closure facts are therefore:

- P03: a versioned, serialized tile read/prepare/delta commit and tile-to-entity lifecycle event;
- P04: authenticated inventory prepare/commit for leashed-anchor insertion/removal and item
  conservation (the existing `InventoryPaymentCoordinator` only handles payment at
  `InventoryPaymentCoordinator.cs:23-98`);
- P05: complete NPC definition/instance facts, generation/lifecycle validation, and the dummy
  fields needed by critter behavior. The composition currently creates an empty NPC definition
  catalog at `ServerComposition.cs:142-145`;
- P02: a session-epoch-bound interaction-anchor lifecycle API;
- P06/content adapter: complete item-to-NPC facts, projectile/NPC compatibility instances,
  deterministic random source, non-wire runtime state, and anchor spawn/despawn behavior;
- root: registration of the real section service, providers, effect-to-intent adapter, module-13
  handler, and all tick owners.

## 5. Explicitly excluded assumptions

This round does not treat any of the following as implementation:

- the Walker fallback in `LeashedPrototypeResolver`;
- constant wind, constant gravity, synthetic collision, or a fixed Partial state;
- a verification fake as a production provider;
- an empty Version4 body as evidence of the intended algorithm;
- a wire snapshot as a substitute for kite/critter runtime state;
- a successful registry-only TileEntity placement as an atomic world mutation;
- an empty NPC catalog or empty item map as complete content facts.

## 6. Build and scenario evidence

Build was run once, serially, after the required build-constraint review:

```text
SDK: dotnet 10.0.400
Command: dotnet build src/MinimalImplementation/Mechanisms/MinimalImplementation.Mechanisms.csproj --no-restore
Exit code: 0
Warnings: 0
Errors: 0
Primary output: D:\TRbackup\NLTX\Build\bin\MinimalImplementation.Mechanisms\Debug\net10.0\MinimalImplementation.Mechanisms.dll
Other generated outputs: D:\TRbackup\NLTX\Build\bin\ (transitive project outputs)
Generator note: 166 Version4 codec source files; input fingerprint 88F0CD496866679E656DF223E1EF71CCEB1E72E7D91346A3CE8D40ED5213BA76
```

This is compile evidence for the standalone Mechanisms project only. It is not evidence that the
root Host composition or the complete nine-phase scheduler is runnable.

No functional scenario or verifier was run. In particular, `SC071`, `SC082`–`SC090`, and `SC094`
were not run; no substitute scenario was run either. The build was not counted as a scenario.

## 7. Nine-phase readiness

`src/MinimalImplementation/Persistence/ServerTickPhase.cs:3-13` defines Player, Npc, Projectile,
WorldItem, LeashedEntity, Time, World, Invasion, and PeriodicSync. The scheduler requires exactly
one owner or composite participant for every phase at
`src/MinimalImplementation/Persistence/WorldTickScheduler.cs:25-52`.

| Phase | Current readiness in production composition | Evidence |
| --- | --- | --- |
| Player | Not ready as a supplied scheduler owner | `P08BServerRuntimeIntegrations.cs:23-39` requires external owners. |
| Npc | Not ready as a supplied scheduler owner | No complete P05 tick owner is composed here. |
| Projectile | Partial: `ProjectileServerTickPhaseOwner` is constructed | `ServerComposition.cs:142-151`; this does not complete the scheduler set. |
| WorldItem | Not ready as a supplied scheduler owner | No production owner is supplied to P08-B. |
| LeashedEntity | Not ready; motion/effect ports and owner are unregistered | `LeashedEntityServerTickPhaseOwner.cs:15-40`; no construction in `ServerComposition.cs`. |
| Time | Not ready as a supplied scheduler participant | Time/World composite is an available type, not a registered runtime owner. |
| World | Not ready as a supplied scheduler participant | Same external-owner boundary; scheduler construction is deferred. |
| Invasion | Not ready as a supplied scheduler owner | No production owner is supplied to P08-B. |
| PeriodicSync | Not ready as a supplied scheduler owner | No production owner is supplied to P08-B. |

The overall readiness result is **not ready**. The next valid continuation is owner/API agreement
at the P03/P04/P05/P02/root seams above, followed by source-backed provider implementation and
root registration. No further P06 implementation is authorized from the current facts alone.
