# System Decomposition Report: authoritative P13

```yaml
partitionId: P13
taskId: AUTH-SYS-P13
sessionId: 90f3766e5cf742bea166c28a864ded62
inputReport: D:\\TRbackup\\NLTX\\docs\\migration\\ledgers\\authoritative-20-partitions\\P13-NPC-Town-Progression.md
outputReport: D:\\TRbackup\\NLTX\\docs\\system-decomposition\\reports\\2026-09-18-system-decomposition-authoritative-P13-npc-town-progression.md
expectedMemberCount: 106
observedMemberCount: 106
designStatus: proposed
verificationStatus: not-run
sourceModified: false
testsRun: false
migrationStatus: not-claimed
```

## Scope and Evidence

This report covers only the claimed P13 row from the authoritative task table. The manifest scope
is the ten leaf groups and 106 fields in
`docs/migration/ledgers/authoritative-20-partitions/P13-NPC-Town-Progression.md`:

| Leaf group | Members in scope |
|---|---:|
| `NpcBossAndInvasionGlobalState` | 10 |
| `NpcBossAndInvasionState` | 5 |
| `NpcTownRescueState` | 8 |
| `NpcTownPetAdoptionState` | 3 |
| `NpcTownSpawnUnlockState` | 16 |
| `NpcTowerAndEventShieldState` | 10 |
| `NpcProgressionBookAndActiveRegistryState` | 4 |
| `NpcBossDefeatFlags` | 17 |
| `NpcEventDefeatFlags` | 14 |
| `NpcTownHousingAndBreathState` | 19 |
| **Total** | **106** |

The complete member names are: `MoonLordAttacksArray`, `MoonLordAttacksArray2`,
`MoonLordFightingDistance`, `MoonLordCountdown`, `MaxMoonLordCountdown`,
`NaturalMoonlordCountdownTime`, `ItemMoonlordCountdownTime`, `totalInvasionPoints`, `waveKills`,
`waveNumber`; `golemBoss`, `plantBoss`, `crimsonBoss`, `deerclopsBoss`, `netUpdate`;
`savedTaxCollector`, `savedGoblin`, `savedWizard`, `savedMech`, `savedAngler`, `savedStylist`,
`savedBartender`, `savedGolfer`; `boughtCat`, `boughtDog`, `boughtBunny`;
`unlockedSlimeBlueSpawn`, `unlockedSlimeGreenSpawn`, `unlockedSlimeOldSpawn`,
`unlockedSlimePurpleSpawn`, `unlockedSlimeRainbowSpawn`, `unlockedSlimeRedSpawn`,
`unlockedSlimeYellowSpawn`, `unlockedSlimeCopperSpawn`, `unlockedMerchantSpawn`,
`unlockedDemolitionistSpawn`, `unlockedPartyGirlSpawn`, `unlockedDyeTraderSpawn`,
`unlockedTruffleSpawn`, `unlockedArmsDealerSpawn`, `unlockedNurseSpawn`, `unlockedPrincessSpawn`;
`ShieldStrengthTowerSolar`, `ShieldStrengthTowerVortex`, `ShieldStrengthTowerNebula`,
`ShieldStrengthTowerStardust`, `LunarShieldPowerNormal`, `TowerActiveSolar`, `TowerActiveVortex`,
`TowerActiveNebula`, `TowerActiveStardust`, `LunarApocalypseIsUp`;
`combatBookWasUsed`, `combatBookVolumeTwoWasUsed`, `peddlersSatchelWasUsed`,
`npcsFoundForCheckActive`; `downedBoss1`, `downedBoss2`, `downedBoss3`, `downedQueenBee`,
`downedSlimeKing`, `downedPlantBoss`, `downedGolemBoss`, `downedFishron`,
`downedAncientCultist`, `downedMoonlord`, `downedEmpressOfLight`, `downedQueenSlime`,
`downedDeerclops`, `downedMechBossAny`, `downedMechBoss1`, `downedMechBoss2`, `downedMechBoss3`;
`downedGoblins`, `downedFrost`, `downedPirates`, `downedClown`, `downedMartians`,
`downedHalloweenTree`, `downedHalloweenKing`, `downedChristmasIceQueen`, `downedChristmasTree`,
`downedChristmasSantank`, `downedTowerSolar`, `downedTowerVortex`, `downedTowerNebula`,
`downedTowerStardust`; `townNPC`, `nextDialogue`, `travelNPC`, `homeless`, `homelessDespawn`,
`lookForHomeTimeout`, `KickOutLookForHomeTimeout`, `homeTileX`, `homeTileY`, `housingCategory`,
`oldHomeless`, `oldHomeTileX`, `oldHomeTileY`, `closeDoor`, `doorX`, `doorY`, `breath`,
`breathMax`, `breathCounter`.

Authoritative source evidence is the read-only Version4 tree at `D:\TRbackup\Version4`, primarily
`Terraria/NPC.cs`, `Terraria/Main.cs`, `Terraria/WorldGen.cs`, `Terraria.IO/WorldFile.cs`,
`Terraria/NetMessage.cs`, and `Terraria/MessageBuffer.cs`. The source inventory declaration ranges
are `NPC.cs:5915-6437`; the source file also contains the relevant reset, death, housing, update, and
breath paths at `NPC.cs:7119-7150`, `8212-8265`, `43033-43045`, `65237-65982`, `76703-76905`,
and `79413-79461`.

The CPG query API was initialized read-only against
`D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite` and the server was started before
queries. Its manifest is `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`,
with 967 shards, 8,166,789 nodes, and 71,038,907 edges. Relevant results were:

- `Get-CpgTypeSurface(Terraria.NPC)` returned 647 members with `complete` status.
- `Find-CpgSymbols` plus `Find-CpgCallSites` for `SetEventFlagCleared` found 26 confirmed static
  call sites across `Terraria/NPC.cs` and `Terraria/Main.cs`. This establishes a shared writer
  seam, not a unique runtime owner.
- `Get-CpgMemberUses` for `downedMoonlord` found six uses: confirmed writes in `WorldGen.cs` and
  `Terraria.IO/WorldFile.cs`, plus partial/unknown reads in NPC, NetMessage, and WorldFile paths.
- `Get-CpgCallSites` for `UpdateHomeTileState` found eight confirmed call sites, all in
  `Terraria/NPC.cs`; the method itself calls `WorldGen.TownManager` and `NetMessage.SendData(60)`.
- `Get-CpgCallSites` for `CheckDrowning` found one confirmed caller in `NPC.UpdateNPC`.
- `Get-CpgMemberUses` for `nextDialogue` returned zero items with the explicit
  `NoMatchingFactInScannedScope` gap. The declaration is confirmed, but its reader/writer contract
  is `unknown`; the empty `CheckDialogue` body cannot be treated as pure or unused.

The CPG index is static evidence only. It does not close dynamic dispatch, reflection, configuration,
serialization branches, callback effects, inbound runtime entry points, or scheduler timing. Any
claim relying on those remains `partial` or `unknown`.

The Space Station 14 checkout at `C:\Users\shan\Downloads\ECS\space-station-14-master` was
used only as an organization reference. `SharedStackSystem.API.cs`, `SharedHandsSystem` partials,
`ExitContainerOnMoveSystem.cs`, `PullingSystem.cs`, and `AtmosphereSystem.Processing.cs` show that
one System can own a cohesive API while partials, event subscriptions, and explicit scheduling
remain coordinated. They do not prove Terraria behavior or an owner for this partition.

## Prior Component Decomposition Reconciliation

The prior P13 Component design/execution documents are inputs, not implementation proof. They
propose 17 component slices and record local source-owner work for 105 members, while explicitly
leaving Version4 callers, scheduling, identity reuse, persistence, network acknowledgement, and
`nextDialogue` open. This System report retains those gaps and resets the verification claim to
`not-run` for this static report.

Current NLTX evidence under `src/NSSLC` is partial:

- `Component/WorldSession/NpcProgression` contains proposed or local state types for Moon Lord,
  invasion waves, boss registry/defeat flags, event defeat flags, tower state, and book usage.
- `Component/Town/Progression` contains rescue, pet adoption, and spawn-unlock state components.
- `Component/Npc/Network/NpcNetworkSyncIntentComponent.cs` and
  `Component/Npc/NpcReplicationDirtyState.cs` separate a dirty intent from transport, but no
  Version4 transport scheduler or acknowledgement path is evidenced here.
- `Component/Npc/Queries/NpcActivePresenceScanSystem.cs` rebuilds a cache from caller-provided
  entries. The real `Main.npc` enumeration caller, invalidation timing, and lifecycle ownership
  remain unverified.
- `Component/Town/Housing` and `Component/WorldSession/WorldGeneration/TownHousing*` model both
  an NPC relation and a world registry. Their resident key mode and final owner are explicitly
  `integration-review`; they must not double-write one housing invariant.
- `Component/Npc/Environment/NpcBreathStateComponent.cs` captures the 7-tick cadence and 200-point
  bound, but Version4 also applies life damage, low-priority network updates, a lethal-hit guard,
  and random dust effects. Those effects are not proven to be integrated.
- `Component/WorldSession/WorldSessionComponents.cs` still contains a broad
  `WorldEventProgressState` aggregate. It is evidence of existing overlap, not permission to add a
  second writer or to treat the aggregate as the final owner.

No current NLTX file is treated as a completed migration or as proof of behavior equivalence.

## Conceptual Behaviors

The 106 fields form the following behavior slices. Each slice is a conceptual behavior rather
than a one-method mapping.

| Concept ID | Behavior and invariant | State scope | Evidence status |
|---|---|---|---|
| `P13-MoonLord-Encounter-Countdown` | Immutable attack/range/time definition plus a bounded countdown; reaching zero spawns Moon Lord once and emits network/world effects. | world | `confirmed` declarations; transition closure `partial` |
| `P13-Invasion-Wave-Progress` | Total points, kills, and wave number move together; start/stop and sync preserve wave semantics. | world | `confirmed` writes in Main; save/replay closure `partial` |
| `P13-Lunar-Tower-Encounter` | Tower active flags, shields, apocalypse state, and impending-doom transition share an encounter invariant. | world | `confirmed` WorldGen/NPC effects; owner split `partial` |
| `P13-Boss-Registry-Identity` | Four legacy slots resolve an active boss instance and must be cleared on inactive/type-mismatch or slot reuse. | world registry | `confirmed` slot paths; stable identity/generation `partial` |
| `P13-NPC-Replication-Intent` | Per-NPC dirty intent is marked, throttled, acknowledged/retried, and reset on entity reuse. | NPC entity | `confirmed` field and update path; transport closure `partial` |
| `P13-Town-Rescue-Progression` | Eight rescue flags are monotonic, idempotent facts used by town spawn/progression. | world | `confirmed` declarations and AI writer; all readers `partial` |
| `P13-Town-Pet-Adoption` | Pet purchase is idempotent on first use, otherwise rerolls a matching town pet and emits messages. | world plus NPC variation | `confirmed` `UnlockOrExchangePet`; command boundary `partial` |
| `P13-Town-Spawn-Unlock` | Sixteen unlock flags gate spawn eligibility and are reset/serialized as world facts. | world | `confirmed` declarations/reset/save paths; reader closure `partial` |
| `P13-Book-Usage-Progression` | Three one-way book/satchel flags change town/NPC rules and are projected to save/network state. | world | `confirmed` declarations and message inputs; effect closure `partial` |
| `P13-Active-NPC-Presence-Scan` | A rebuilt type-presence cache is valid for one scan revision and is not durable progression. | world scan cache | `confirmed` clear/update/read paths |
| `P13-Boss-Defeat-Progression` | Boss defeat flags are first-clear, idempotent progression facts with downstream world actions. | world | `confirmed` `SetEventFlagCleared`/death paths; shared writer `partial` |
| `P13-Event-Defeat-Progression` | Event and tower defeat flags are first-clear facts with event-specific downstream actions. | world | `confirmed` death/Main paths; shared writer `partial` |
| `P13-Town-Housing-Relation` | Town status, home relation, timeout and compatibility snapshot update atomically; changed housing publishes status. | NPC entity plus world room index | `confirmed` `UpdateHomeTileState`; room ownership `partial` |
| `P13-Travel-NPC-Mode` | Travel NPC mode is a world/session fact used by time-of-day despawn and spawn logic. | world | `confirmed` Main read and NPC write; owner `integration-review` |
| `P13-NPC-Breath-Environment` | Submerged cadence decrements breath, dry environment recovers it, and zero breath applies bounded life damage and sync. | NPC entity | `confirmed` `CheckDrowning`; effect closure `partial` |
| `P13-Conditional-Dialogue` | `nextDialogue` is a conditional dialogue payload whose reader, writer, reset, and projection contract is not evidenced. | NPC entity/compatibility | `unknown` |

The `townNPC`, `homeTile*`, `homeless*`, door intent, and breath fields are not interchangeable:
housing relation and environment cadence have different clocks, writers, and side effects. The
static `travelNPC` field is not copied onto every NPC component without an integration decision.

## State Ownership and Write Closure

The proposed ownership is one authoritative writer per invariant. Components hold data; Systems
perform transitions; Queries return read-only eligibility/snapshots; Adapters handle Version4
serialization/network types; Projections never write back.

| Proposed owner | Authoritative state | Direct writers/readers and effects | Decision |
|---|---|---|---|
| `MoonLordEncounterSystem` | `MoonLordEncounterDefinition`, `MoonLordEncounterStateComponent` | `WorldGen.StartImpendingDoom` writes max/countdown; `Main.UpdateTime` decrements; `NetMessage` message 103 projects; `NPC.SpawnOnPlayer` is an effect port. | `separate`; commit only after countdown transition succeeds |
| `InvasionWaveProgressSystem` | `InvasionWaveProgressStateComponent` | `Main.startPumpkinMoon`, `startSnowMoon`, and `stopMoonEvent` write points/kills/wave; `Main.SyncAnInvasion` and message 78 project. | `separate`; preserve tuple atomicity |
| `LunarTowerEncounterSystem` | tower active/shield/apocalypse state | `WorldGen` starts/recomputes apocalypse; `NPC.DoDeathEvents` clears towers; messages 101/103 project; WorldFile saves. | `separate`; coordinate with defeat commit through a port |
| `BossEntityIndexRegistrySystem` | generation-aware registry keyed by boss kind | `MessageBuffer` sets slots on spawn; `Main.CheckBossIndexes` clears invalid slots; lifecycle reset/slot reuse must be supplied by the entity owner. | `separate`; legacy integer slots are projections |
| `NpcReplicationIntentSystem` | per-NPC dirty intent/revision | `NPC.UpdateNPC`, `NetUpdate*`, and `UpdateNetworkCode` mark/consume; `NetMessage` transports; acknowledgement/retry path is not closed. | `separate`; no packet authority in the component |
| `NpcTownProgressionSystem` (partial) | rescue, spawn-unlock, and book-use components | AI saved-state update, message inputs, WorldGen reset, WorldFile and NetMessage projections. | `partial` to keep common world reset/commit policy without one system per flag |
| `TownPetAdoptionSystem` | pet adoption flags plus reroll command result | `MessageBuffer` receives license commands; `UnlockOrExchangePet` writes flag, rerolls variation, broadcasts, and sends message 7. | `separate`; effectful command cannot be a Query |
| `NpcProgressionCommitSystem` (partial) | boss and event defeat components | `NPC.SetEventFlagCleared` is called 26 times from NPC/Main; first-clear hook invokes LanternNight, credits, hardmode/world actions. | `partial` with two component partitions and one first-clear transaction seam |
| `NpcActivePresenceScanSystem` | scan cache `npcsFoundForCheckActive` | `Main`/`WorldGen` clear and rebuild; `NPC.UpdateNPC` reads bee-related presence. | `separate`; cache is revision-scoped and non-durable |
| `TownHousingSystem` | entity housing relation and world room registry | `NPC.UpdateHomeTileState` writes old/current fields, queries TownManager, sends message 60; WorldFile delegates TownManager save/load. | `separate`; final room key/owner is `integration-review` |
| `TravelNpcModeSystem` | world travel-NPC mode | `NPC.UpdateNPC` sets the mode for type 368; `Main.UpdateTime` reads it and calls `WorldGen.UnspawnTravelNPC`. | `separate` candidate; cross-partition owner unresolved |
| `NpcBreathSystem` | breath and breath-counter component | `NPC.UpdateNPC` calls `CheckDrowning`; collision, life damage, low-priority sync, lethal-hit guard, and random dust are effects. | `separate`; effect ports required |
| `ConditionalDialogueCompatibilityAdapter` | `nextDialogue` | Declaration exists, `CheckDialogue` is empty, and CPG found no use in the scanned paths. | `keep/defer`; no new owner API until evidence exists |

`downedMoonlord`, for example, cannot be owned solely by a local Component method: CPG confirms
WorldGen and WorldFile writes and only partial reads in NPC/NetMessage. The final owner must sit
behind the progression commit port and expose save/network projections. The same rule applies to
all `downed*`, rescue, adoption, unlock, book, and tower fields.

## Boundary Role and Decision

The proposed target layout is domain-first under `src/NSSLC`, using existing component namespaces
where they already exist:

```text
src/NSSLC/Component/WorldSession/NpcProgression/
  MoonLord/        MoonLordEncounterDefinition, MoonLordEncounterStateComponent
  Invasion/        InvasionWaveProgressStateComponent
  LunarTower/      LunarTowerEncounterStateComponent
  Boss/            BossEntityIndexRegistryComponent, BossDefeatProgressionStateComponent
  Events/          EventDefeatProgressionStateComponent
  Town/            rescue, adoption, spawn-unlock, book state components
src/NSSLC/Component/Npc/
  Network/         NpcNetworkSyncIntentComponent, NpcReplicationDirtyState
  Environment/     NpcBreathStateComponent and breath rules
  Queries/         NpcActivePresenceScanSystem and read-only scan views
src/NSSLC/Component/Town/Housing/
  TownHousingRelationStateComponent and room relation ports
src/NSSLC/System/...
  one System owner per capability above; no catch-all NPC progression System
src/NSSLC/Adapter/...
  Version4 WorldFile/NetMessage/TownManager adapters
src/NSSLC/Projection/...
  immutable save/network/event views
```

These are proposed paths, not claims that the proposed Systems already exist. The strongest
alternative is one `NpcAndTownProgressionSystem` owning all 106 fields. It is rejected because it
would mix world progression, entity replication, housing, environment cadence, and unknown
dialogue, recreating the static `Terraria.NPC` authority problem. Creating one System per flag is
also rejected because it would duplicate reset, first-clear, serialization, and cross-system
coordination. `partial` is reserved for one runtime owner whose code sections share an invariant
and commit protocol; it does not add a scheduler node.

The SS14 examples support the organization choice: public API partials remain in one cohesive
System, event-only Systems can be small, and scheduling constraints are explicit. They do not
justify importing SS14 lifecycle or networking semantics into NLTX.

## System API and Legacy Behavior Mapping

Mapping is by stable conceptual behavior, not by method count or signature equality.

| Legacy entry points | Proposed composition | Observable contract to preserve |
|---|---|---|
| `Main.startPumpkinMoon`, `Main.startSnowMoon`, `Main.stopMoonEvent`, `Main.SyncAnInvasion` | `BeginWave`, `StopWave`, `CreateInvasionSyncView` on `InvasionWaveProgressSystem` plus `IInvasionNetworkProjection` | points/kills/wave reset and start values, wave ordering, message 78 payload, and duplicate stop behavior |
| `WorldGen.StartImpendingDoom`, `Main.UpdateTime`, message 103 | `StartCountdown`, `TickCountdown`, `CreateCountdownSyncView` on `MoonLordEncounterSystem` | max/countdown values, zero-crossing spawn exactly once, reset, message order, and failure handling |
| `WorldGen.UpdateLunarApocalypse`, tower death cases, messages 101/103 | `SetTowerActive`, `ApplyShieldDamage`, `RecomputeApocalypse` on `LunarTowerEncounterSystem` | active/shield coupling, tower disappearance scan, impending-doom transition, and replication |
| `Main.CheckBossIndexes`, NPC spawn decode in `MessageBuffer` | `Register`, `TryResolve`, `ClearIfInactiveOrMismatched` on `BossEntityIndexRegistrySystem` | legacy slot compatibility, type checks, generation/slot reuse, and clear timing |
| `NPC.netUpdate`, `NetUpdateIgnoreSpamLimit`, `NetUpdateLowPriority`, `UpdateNetworkCode` | `Mark`, `MarkLowPriority`, `Acknowledge`, `Retry`, `ResetForReuse` on `NpcReplicationIntentSystem` | server authority, spam policy, dirty visibility, retry/ack behavior, and entity reuse reset |
| `AI_007_TownEntities_UpdateSavedStates`, WorldGen reset, save/load | `MarkRescued`/`Reset` plus read-only eligibility Query on `NpcTownProgressionSystem` | first rescue is monotonic, reset scope, and spawn eligibility observations |
| `MessageBuffer` pet license cases, `NPC.UnlockOrExchangePet` | `AdoptPet` command on `TownPetAdoptionSystem` plus variation/effect ports | first use sets the flag; repeat uses reroll or failure; messages and network update occur in the same order |
| NPC spawn/death unlock writes, WorldGen reset, save/load/network flags | `UnlockSpawn` and `IsSpawnUnlocked` Query | idempotent unlocks, world scope, and compatibility projection |
| book/satchel message cases and progression readers | `MarkBookUsed`/`IsBookUsed` | one-way commit, reset, and save/network projection |
| `NPC.SetEventFlagCleared`, `OnGameEventClearedForTheFirstTime`, `DoDeathEvents` | `MarkBossDefeated`/`MarkEventDefeated` on `NpcProgressionCommitSystem` returning a first-clear result consumed by world-effect ports | first-clear-only actions, hardmode/meteor/LanternNight/credits effects, idempotency, and error boundaries |
| `NPC.ClearFoundActiveNPCs`, `UpdateFoundActiveNPCs`, Main/WorldGen reads | `BeginScan`, `MarkActive`, `CommitScan` on `NpcActivePresenceScanSystem` | one scan revision, active/type filtering, reset, and no durable progression writes |
| `NPC.UpdateHomeTileState`, AI housing calls, `WorldGen.TownManager`, message 60 | `CommitHousingRelation`/`PublishHousingChange` on `TownHousingSystem` | atomic old/current snapshot, changed-only notification, household status, room key, and save/load |
| `NPC.travelNPC`, `Main.UpdateTime`, `WorldGen.UnspawnTravelNPC` | `SetTravelNpcMode`/`ExpireForTimeOfDay` on a session/calendar owner | day/time conditions, spawn/despawn and world scope |
| `NPC.CheckDrowning`, `NpcBreathStateComponent` | `ApplyEnvironmentStep` on `NpcBreathSystem`, then `ILifeDamagePort` and `INpcReplicationIntent` | seven-step cadence, +3 recovery capped at 200, damage of 2 at zero, life floor/strike behavior, sync, and visual effects |
| `NPC.nextDialogue`, `CheckDialogue`, `TrySyncingUniqueTownNPCData` | deferred compatibility Adapter; no proposed public API yet | preserve unknown semantics until a reader/writer/reset/projection trace exists |

The APIs above are proposed contracts. They are not a claim that the methods or interfaces exist in
`src/NSSLC` today.

## Call and Dependency DAG

The evidence-backed static dependency shape is:

```text
World load / network input / NPC spawn or death
  -> validation and intent conversion
  -> progression, encounter, housing, registry, or breath owner System
  -> committed Component state
  -> first-clear/world effect or entity effect port
  -> WorldFile / NetMessage / event Projection
```

The relevant edges are:

1. `Main.Update` clears and rebuilds the active-NPC cache, checks boss indexes, updates world time,
   world generation, and invasion state. The exact scheduler phase and cross-world isolation are
   not encoded by file order and remain `partial`.
2. `NPC.UpdateNPC` invokes town-NPC synchronization, AI, `CheckDrowning`, dialogue checking,
   network update, and active checks. CPG confirms the `CheckDrowning` call and the source shows the
   `CheckDialogue` and `TrySyncingUniqueTownNPCData` bodies are empty in this snapshot; their
   behavior is therefore `unknown`.
3. `NPC.DoDeathEvents` calls `SetEventFlagCleared` for boss/event cases and can call WorldGen,
   projectile, chat, item, or spawn effects. CPG confirms 26 static call sites to the shared
   setter, so boss and event components cannot silently become independent writers.
4. `WorldGen` resets P13 flags and active scans, starts/recomputes the lunar encounter, and invokes
   TownManager and persistence-related paths. The reset is a world-session boundary, not an NPC
   entity reset.
5. `WorldFile` writes and reads progression flags in a versioned binary order and delegates
   TownManager save/load. This is an Adapter boundary; loading must validate before committing.
6. `NetMessage` projects progression bits, tower shields, countdowns, and NPC sync; `MessageBuffer`
   receives spawn/pet/event commands and mutates legacy fields. Server authority and packet retry
   semantics require integration evidence beyond static edges.

Unresolved edges are explicit: dynamic/configured NPC spawn sources, entity slot generation and
identity, TownManager resident-key selection, network acknowledgement/retry, multi-world scope,
serialization version branches not covered by the selected excerpts, and the missing dialogue
reader/writer contract. No edge is removed from the DAG because a query returned zero items.

## Lifecycle and Side Effects

| Lifecycle point | Proposed owner work | Side effects and open evidence |
|---|---|---|
| World create/load | Construct world-scoped progression/encounter/registry components; load through validating adapters. | Preserve WorldFile field order/version branches; bad input must leave prior committed state. Full load closure `partial`. |
| NPC create/reuse | Allocate stable entity identity, initialize housing/breath/replication state, and register boss slot only after type/identity validation. | Slot reuse and generation evidence `unknown`; do not use legacy slot as durable identity. |
| Tick/update | Run encounter countdown, invasion/tower updates, active scan, housing timeout, travel mode, and NPC breath according to explicit scheduler constraints. | Main/NPC interleaving is static evidence only; phase/barrier and multi-world scheduling `partial`. |
| Event/death | Commit first-clear boss/event progression, then publish world effects and loot/spawn/chat projections. | Re-entrant death and duplicate event handling require focused behavior tests. |
| Housing change | Atomically update current and compatibility snapshot, query TownManager, and publish changed status. | Room key, assignment revision, save/load and message 60 compatibility `integration-review`. |
| Network input/output | Convert MessageBuffer input to validated commands; project committed state via NetMessage. | Server authority, acknowledgement, retry, packet ordering and duplicate suppression `unknown/partial`. |
| Save/unload | Flush projections from committed world snapshots; reset world state and scan caches at unload. | No evidence that all static fields are isolated per world/session; this is a blocking integration item. |

Side-effect ports are required for NPC spawn, life damage/strike, chat, item/projectile effects,
WorldGen actions, TownManager assignment, network send/ack, and persistence. Query and Projection
types must not write these effects or return live mutable state.

## Integration Handoff

The following decisions are intentionally handed to integration review rather than finalized in
this partition:

- `crossSubsystemOwner: integration-review` for `WorldProgression` versus `NpcProgression` ownership
  of boss/event flags, Moon Lord countdown, invasion progress, and lunar apocalypse.
- `crossSubsystemOwner: integration-review` for `WorldGeneration` versus `Town` ownership of room
  assignment, resident key mode, revision, and TownManager save/load.
- `crossSubsystemOwner: integration-review` for `Npc` identity/slot generation and the boss registry
  key. `NpcSlot`, `NpcEntityId`, `PersistentEntityId`, and `NetworkId` must remain distinct.
- `crossSubsystemOwner: integration-review` for `NpcNetworkSyncIntent` and the actual transport
  scheduler/acknowledgement path. The component is an intent, never a packet or authority source.
- `crossSubsystemOwner: integration-review` for `travelNPC` because Main time and world spawn
  lifecycle both consume it.
- `crossSubsystemOwner: integration-review` for `nextDialogue`; no reader/writer/reset contract was
  found, and the empty source bodies cannot be treated as no-op behavior.

Integration must also reconcile the existing `WorldEventProgressState` aggregate with proposed
per-capability components before any implementation writer is enabled. Two owners must not write
the same invariant during a shadow or compatibility period.

## Migration Behavior Contract

This is a plan for a later migration. The required observation vector for each slice includes:

- returned result and rejection reason;
- exact state delta and invariant preservation;
- first-clear, duplicate, retry, idempotency, and reset behavior;
- event/chat/item/projectile/world effect and their order;
- save/load bytes, version handling, network bits/messages, authority, and acknowledgement;
- lifecycle behavior at create, reuse, unload, and multi-world boundaries;
- scheduler visibility and any delayed structural change;
- compatibility reads for legacy public fields and the removal gate for old writers.

Recommended migration order is: establish world/session scope and identity keys; route one
progression commit port; add save/network adapters; migrate encounter/invasion/tower state; route
boss registry and replication intent; migrate town rescue/adoption/unlock/book flags; migrate
housing and travel; migrate breath effects; resolve `nextDialogue`; only then remove legacy writers.
Each step remains `proposed` until the real callers and behavior tests exercise the new owner.

The existing Component execution document's local builds or focused checks do not satisfy this
contract. A report, compile, isolated verifier, or callable legacy facade cannot establish
migration success or behavior equivalence.

## Evidence Gaps and Blocking Decisions

Blocking decisions:

1. **BD-P13-01: progression owner.** Decide whether WorldSession or NpcProgression owns the
   shared first-clear transaction for boss/event flags, Moon Lord countdown, invasion, and lunar
   state. Until then the proposed partial commit System is not an implementation owner.
2. **BD-P13-02: identity and slot reuse.** Confirm the stable instance/generation source and the
   lifecycle that clears or replaces `golemBoss`, `plantBoss`, `crimsonBoss`, and `deerclopsBoss`.
3. **BD-P13-03: housing authority.** Confirm whether Town or WorldGeneration owns the resident key,
   room assignment, revision, and TownManager persistence boundary.
4. **BD-P13-04: replication contract.** Confirm dirty-state scheduling, server authority, packet
   acknowledgement/retry, and duplicate suppression for `netUpdate` and progression projections.
5. **BD-P13-05: dialogue contract.** Locate the reader, writer, reset, and network/persistence
   behavior for `nextDialogue`, or retain it as a compatibility seam without reclassification.
6. **BD-P13-06: breath effects.** Confirm the environment collision owner, life-damage/strike
   semantics, and the precise network/visual side effects before extracting the breath System.
7. **BD-P13-07: multi-world scope.** Prove that static Version4 fields are isolated or define the
   session boundary that replaces them; do not infer isolation from component names.

Evidence gaps that remain `unknown` or `partial` include dynamic/reflection/configuration entries,
complete inbound callers outside the selected source paths, scheduler phase/barrier, save/load
version branches, network decode/retry, slot reuse, TownManager key mode, `nextDialogue` behavior,
and full effect closure for death, spawn, breath, and lunar transitions. Cleared or empty method
bodies are not evidence of no behavior.

## Verification Plan

`verificationStatus: not-run` is intentional. No build, test, runtime verifier, migration runner,
or behavior-equivalence check was run for this report.

Before implementation or legacy-writer deletion, the real migration project should run focused
tests through the proposed owners, serially under the repository build contract, covering:

- encounter countdown zero-crossing, restart, save/load, message 103, and exactly-once spawn;
- invasion start/stop, wave transitions, reset, sync message 78, and duplicate commands;
- tower shield/active/apocalypse transitions and message 101/103 ordering;
- boss registry type validation, generation/slot reuse, inactive clear, and network spawn decode;
- rescue/adoption/unlock/book idempotency, first-clear effects, reset, save/load, and network bits;
- active-NPC scan revision replacement and consumers;
- housing assignment, homeless snapshots, timeout/door intent, TownManager persistence, and message 60;
- travel NPC day/time despawn and world-session reset;
- breath cadence, recovery cap, life floor/strike behavior, low-priority sync, and effect ports;
- `nextDialogue` only after a source-backed reader/writer contract is found;
- cross-slice trace ordering, multi-world isolation, retries, and compatibility facade parity.

The expected acceptance is behavior evidence from the real migration path. A completed document
task, a successful build, a local verifier, or an old API that remains callable must not be labeled
migration success.
