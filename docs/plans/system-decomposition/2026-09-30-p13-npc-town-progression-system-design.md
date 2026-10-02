# P13 NPC, Town, Event, Boss, and World Progression System Design

```yaml
partitionId: P13
taskId: AUTH-SYS-P13
sourceReport: D:\\TRbackup\\NLTX\\docs\\system-decomposition\\reports\\2026-09-18-system-decomposition-authoritative-P13-npc-town-progression.md
originalSessionId: 90f3766e5cf742bea166c28a864ded62
authoritativeSource: D:\\TRbackup\\Version4
completeReferenceSource: D:\\TRbackup\\无任何删减通过编译
targetSource: D:\\TRbackup\\NLTX\\src\\NSSLC
designStatus: proposed
executionStatus: partial-core-slice
verificationStatus: focused-passed-10-percent
sourceModified: false
targetModified: true
testsRun: true
migrationStatus: not-claimed
```

## Purpose and Decision

This document turns the completed P13 System decomposition report into a proposed target
boundary. Focused deterministic seams and a verifier now exist in the target tree for first-clear
progression, Moon Lord countdown, invasion tuple, and lunar tower transitions, but this remains a
design proposal: the proposed Systems are not runtime-integrated and Version4 behavior is not
claimed equivalent to the target.

The selected boundary is a set of cohesive world, NPC, town, and projection Systems with one
commit owner per invariant. A single `NpcAndTownProgressionSystem` is rejected because it would
again combine world progression, entity replication, housing, environment cadence, and an
unresolved dialogue value. One System per flag is also rejected because it would duplicate reset,
first-clear, serialization, and cross-System coordination.

The design keeps the following rule visible throughout the migration:

```text
external input or legacy entry point
  -> validated command or read-only query
  -> one System commit owner
  -> authoritative Component state
  -> effect port and save/network Projection
```

## Evidence Baseline

### Sources

| Source | Use | Evidence status |
|---|---|---|
| `D:\\TRbackup\\Version4\\Terraria\\NPC.cs` | P13 declarations, NPC lifecycle, town state, death/event paths, housing, and breath | authoritative Version4; confirmed structure |
| `D:\\TRbackup\\Version4\\Terraria\\Main.cs` | event start/stop, invasion sync, time update, boss-index cleanup | authoritative Version4; confirmed structure |
| `D:\\TRbackup\\Version4\\Terraria\\WorldGen.cs` | world reset, lunar apocalypse, tower and countdown transitions, TownManager calls | authoritative Version4; confirmed structure |
| `D:\\TRbackup\\Version4\\Terraria.IO\\WorldFile.cs` | versioned save/load and TownManager persistence boundary | authoritative Version4; confirmed structure |
| `D:\\TRbackup\\Version4\\Terraria\\NetMessage.cs` and `MessageBuffer.cs` | network projections and inbound commands | authoritative Version4; call and retry closure partial |
| `D:\\TRbackup\\无任何删减通过编译` | complete method bodies used only to close behavior and effect relationships that are cropped in Version4 | supplementary reference; never silently promoted to Version4 fact |
| `D:\\TRbackup\\Version4-cpg-export\\out-dop8-interproc.sqlite` | read-only symbol, call-site, and member-use queries | static evidence only |
| `C:\\Users\\shan\\Downloads\\ECS\\space-station-14-master` | organization examples for cohesive Systems, partial APIs, and explicit scheduling | organization-only reference |
| `D:\\TRbackup\\NLTX\\src\\NSSLC` | current target components and partial seams | current implementation evidence; not an integrated owner |

### Source hashes and CPG manifest

The authoritative Version4 hashes used for this design are:

| File | SHA-256 |
|---|---|
| `Terraria/NPC.cs` | `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17` |
| `Terraria/Main.cs` | `66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520` |
| `Terraria/WorldGen.cs` | `A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D` |
| `Terraria.IO/WorldFile.cs` | `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289` |
| `Terraria/NetMessage.cs` | `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A` |
| `Terraria/MessageBuffer.cs` | `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE` |

The complete-reference hashes are recorded separately so a future implementation can detect a
source change instead of merging the two trees implicitly:

| File | SHA-256 |
|---|---|
| `Terraria/NPC.cs` | `ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0` |
| `Terraria/Main.cs` | `E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F` |
| `Terraria/WorldGen.cs` | `B9F7834CE1BC68C1DD9C656574A2272DB6F79E1407D934E1ADA33EDC3C930F82` |
| `Terraria.IO/WorldFile.cs` | `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289` |
| `Terraria/NetMessage.cs` | `F3066C50715D7C49B8BF2CC852B015303AD7F4C12ADC191C72FC0FE46181E7E2` |
| `Terraria/MessageBuffer.cs` | `48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB` |

The CPG manifest is `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364` with
967 shards, 8,166,789 nodes, and 71,038,907 edges. The query API was initialized read-only and
started before querying. Results used by this design were:

| Query | Result | Interpretation |
|---|---|---|
| `Find-CpgCallSites(SetEventFlagCleared)` | 26 confirmed static call sites in `NPC.cs` and `Main.cs` | shared first-clear seam; not a unique caller |
| `Find-CpgCallSites(UpdateHomeTileState)` | 8 confirmed call sites in `NPC.cs` | housing update has one inspected source owner, but TownManager ownership is open |
| `Find-CpgCallSites(CheckDrowning)` | 1 confirmed caller in `NPC.UpdateNPC` | NPC update reaches breath transition; environment and damage effects remain ports |
| `Get-CpgMemberUses(downedMoonlord)` | 6 items: confirmed writes in `WorldGen.cs` and `WorldFile.cs`, partial/unknown reads elsewhere | progression state crosses WorldGen, persistence, NPC, and networking |
| `Get-CpgMemberUses(nextDialogue)` | zero items with `NoMatchingFactInScannedScope` | unknown contract, not proof of unused behavior |

The query result is static. Dynamic dispatch, reflection/configuration, scheduler timing, packet
acknowledgement/retry, aliasing, and multi-world isolation remain evidence gaps.

### Complete-reference behavior cross-check

The complete reference was read at the following source locations to constrain the proposed API
without promoting its behavior to an authoritative Version4 fact:

| Complete-reference location | Observed behavior | Design consequence | Evidence status |
|---|---|---|---|
| `Terraria/NPC.cs:6054-6060` | `MoonLordCountdown` starts at `0`; max is `3600`; natural and item countdown constants are `3600` and `720`. | Keep definition values immutable and make the active maximum part of the encounter snapshot. | supplementary/confirmed in complete reference |
| `Terraria/Main.cs:65862-65868` | Countdown decrements once per update; the zero crossing requests Moon Lord spawn only when `netMode != 1`. | `TickCountdown` must return a spawn request and leave spawn/network execution to an effect port; exactly-once behavior is still unverified. | supplementary/partial |
| `Terraria/WorldGen.cs:88195-88267` | Lunar update clears inactive tower flags; when no tower or Moon Lord remains it starts impending doom; start clears apocalypse, sets max/current countdown, sends 103, broadcasts, and removes cultists server-side. | `RecomputeApocalypse` must expose a transition request and must not call WorldGen, chat, or network directly. | supplementary/partial |
| `Terraria/Main.cs:11121-11185` | Moon-event stop resets points/kills/wave to zero; Pumpkin/Snow start resets points/kills and starts at wave one on the server. | Invasion points, kills, and wave remain one atomic tuple with explicit `BeginWave`/`StopWave` boundaries. | supplementary/confirmed in complete reference |
| `Terraria/NPC.cs:79749-79775` | A kill adds to `waveKills` and `totalInvasionPoints`; crossing the required threshold resets kills and increments wave; progress and message 78 are published afterward. | The target System may commit the tuple, but threshold lookup, chat, achievements, and message 78 stay effect/projection ports. | supplementary/partial |
| `Terraria/NPC.cs:80549-80571` | Tower death sets the matching `downedTower` flag, clears its active flag, recomputes lunar apocalypse, and publishes its message. | Tower defeat and WorldGen transition are one ordered integration contract, not independent flag writers. | supplementary/partial |

The Version4 read-only CPG query was rerun for the same behavior names. It returned one
`StartImpendingDoom` symbol with two static call sites, two indexed `UpdateLunarApocalypse`
records with five static call sites, one `SyncAnInvasion` symbol with one call site, eight
`UpdateTime` method records with two selected `Main.cs` call sites, and the shared
`SetEventFlagCleared` symbol with 26 static call sites. The reader reported `complete` within its
result budget; that status means the index query completed, not that the runtime call graph,
dispatch, scheduler, or effect closure is complete.

The SS14 reference was inspected for organization only. `Content.Server/Armor/ArmorSystem.cs`
uses a domain-local System with explicit `Initialize`/subscription wiring, while
`Content.Server/Alert/ServerAlertsSystem.cs` and `Content.Client/AlertLevel/AlertLevelDisplaySystem.cs`
show server/client specializations and partial/shared System surfaces. These examples support
keeping P13 files domain-local and keeping scheduling/effect registration explicit; they do not
prove NLTX runtime semantics or replace Version4 evidence.

## Scope

The P13 ledger covers 106 members in ten leaf groups:

| Leaf group | Members | Proposed semantic scope |
|---|---:|---|
| `NpcBossAndInvasionGlobalState` | 10 | Moon Lord encounter definition/countdown and invasion tuple |
| `NpcBossAndInvasionState` | 5 | boss registry slots and per-NPC replication intent |
| `NpcTownRescueState` | 8 | monotonic rescued-town facts |
| `NpcTownPetAdoptionState` | 3 | pet adoption flags and reroll command result |
| `NpcTownSpawnUnlockState` | 16 | world-scoped NPC spawn unlock facts |
| `NpcTowerAndEventShieldState` | 10 | tower shields, active state, and lunar apocalypse |
| `NpcProgressionBookAndActiveRegistryState` | 4 | book use and revision-scoped active presence |
| `NpcBossDefeatFlags` | 17 | first-clear boss progression facts |
| `NpcEventDefeatFlags` | 14 | first-clear event and tower facts |
| `NpcTownHousingAndBreathState` | 19 | resident relation, travel mode, dialogue seam, and breath |

No P12/P14/P16/P17/P18 member is added to this scope. Cross-partition dependencies are recorded
as integration handoffs below.

## Conceptual Behaviors

The System boundary follows behavior and invariant, not the number of fields or legacy methods.

| Concept ID | Required invariant | Scope | Candidate owner | Status |
|---|---|---|---|---|
| `P13-MoonLord-Encounter-Countdown` | bounded definition and countdown; zero crossing spawns once | world | `MoonLordEncounterSystem` | transition closure partial |
| `P13-Invasion-Wave-Progress` | points, kills, and wave move as one tuple | world | `InvasionWaveProgressSystem` | save/replay closure partial |
| `P13-Lunar-Tower-Encounter` | shield, active, apocalypse, and impending-doom transitions agree | world | `LunarTowerEncounterSystem` | owner split partial |
| `P13-Boss-Registry-Identity` | registry resolves active, correctly typed, generation-aware instances | world registry | `BossEntityIndexRegistrySystem` | identity/generation unknown |
| `P13-NPC-Replication-Intent` | dirty intent is marked, acknowledged/retried, and reset on reuse | NPC entity | `NpcReplicationIntentSystem` | transport closure partial |
| `P13-Town-Rescue-Progression` | rescue facts are monotonic and idempotent | world | `NpcTownProgressionSystem` | reader closure partial |
| `P13-Town-Pet-Adoption` | first adoption commits; repeat adoption rerolls or reports failure | world plus NPC variation | `TownPetAdoptionSystem` | command/effect closure partial |
| `P13-Town-Spawn-Unlock` | unlock facts are idempotent and world-scoped | world | `NpcTownProgressionSystem` | reader closure partial |
| `P13-Book-Usage-Progression` | book/satchel use is one-way and projected | world | `NpcTownProgressionSystem` | effect closure partial |
| `P13-Active-NPC-Presence-Scan` | cache is valid only for one increasing scan revision | scan workspace | `NpcActivePresenceScanSystem` | confirmed current seam |
| `P13-Boss-Defeat-Progression` | first clear emits exactly the required world effects | world | `NpcProgressionCommitSystem` | shared writer partial |
| `P13-Event-Defeat-Progression` | event/tower first clear is idempotent and ordered | world | `NpcProgressionCommitSystem` | shared writer partial |
| `P13-Town-Housing-Relation` | current relation and compatibility snapshot commit atomically | NPC plus room index | `TownHousingSystem` | room owner integration-review |
| `P13-Travel-NPC-Mode` | travel mode is a world/session fact with time-of-day expiry | world/session | `TravelNpcModeSystem` | cross-partition owner |
| `P13-NPC-Breath-Environment` | cadence, recovery cap, zero-breath damage, and sync are ordered | NPC entity | `NpcBreathSystem` | effect closure partial |
| `P13-Conditional-Dialogue` | reader, writer, reset, and projection must be found before ownership | compatibility | deferred adapter | unknown |

## Target Components and Systems

### Components and definitions

The current target already contains several proposed data components under
`src/NSSLC/Component`. They are evidence of local shape, not proof of final ownership.

| Component or definition | Lifetime | Authority rule |
|---|---|---|
| `MoonLordEncounterDefinition` and `MoonLordEncounterStateComponent` | world/session | definition is immutable; state is committed only by the encounter System |
| `InvasionWaveProgressStateComponent` | world/session | points, kills, and wave commit together |
| `LunarTowerEncounterStateComponent` | world/session | shield/active/apocalypse changes use one transition boundary |
| `BossEntityIndexRegistryComponent` | world/session | registry key contains instance, legacy slot, generation, and type; legacy integer is a projection |
| `BossDefeatProgressionStateComponent` and `EventDefeatProgressionStateComponent` | world/session | first-clear methods return whether the commit was new; no dual writers |
| town rescue, pet, and spawn-unlock components | world/session | monotonic facts; reset and persistence are explicit |
| `NpcProgressionBookUsageStateComponent` | world/session | one-way flags with save/network projections |
| `NpcActivePresenceCache` | scan revision | cache is invalidated and rebuilt; it is not durable progression |
| `NpcNetworkSyncIntentComponent` and `NpcReplicationDirtyState` | NPC instance | intent is not a packet or authority source; transport owns acknowledgement |
| `TownHousingRelationStateComponent` | NPC instance plus room relation | old relation is a compatibility snapshot, not a second fact |
| `TravelNpcWorldStateComponent` | world/session | travel selection and expiry remain integration-owned |
| `NpcBreathStateComponent` | NPC instance | component stores cadence state; collision, damage, and visual effects are ports |

### System boundaries

| System | Owns | Must not own |
|---|---|---|
| `MoonLordEncounterSystem` | countdown definition/state and zero-crossing commit | NPC identity, packet encoding, or `downedMoonlord` persistence bytes |
| `InvasionWaveProgressSystem` | invasion start/stop and tuple transitions | unrelated world calendar events or client display state |
| `LunarTowerEncounterSystem` | tower shield/active/apocalypse transitions | boss defeat persistence or NPC slot identity |
| `BossEntityIndexRegistrySystem` | register, resolve, replace, and clear registry entries | the NPC entity's durable identity or health |
| `NpcReplicationIntentSystem` | dirty/revision intent and reuse reset | network packet format and client authority |
| `NpcTownProgressionSystem` | rescue, spawn-unlock, and book commits | pet variation side effects or housing room assignment |
| `TownPetAdoptionSystem` | adoption command, reroll result, and effect ordering | read-only eligibility queries |
| `NpcProgressionCommitSystem` | boss/event first-clear transaction and result | direct file/network serialization |
| `NpcActivePresenceScanSystem` | scan revision and cache rebuild | durable progression writes |
| `TownHousingSystem` | atomic relation commit and changed-only publication | world generation ownership until integration review |
| `TravelNpcModeSystem` | world travel mode and time-of-day expiry | NPC instance identity and spawn slot allocation |
| `NpcBreathSystem` | breath cadence and bounded state transition | collision implementation, life damage, and dust implementation |

`NpcProgressionCommitSystem` and `NpcTownProgressionSystem` are intentionally marked `partial`.
They are one runtime owner only while their common transaction/reset protocol remains coherent. If
the integration review assigns these invariants to `WorldProgression`, these names become adapter
seams rather than competing authorities.

The target currently contains deterministic implementations for `MoonLordEncounterSystem`,
`InvasionWaveProgressSystem`, and `LunarTowerEncounterSystem` beside their state components. They
return immutable views or transition requests and do not execute spawn, WorldGen, network, chat,
save, or achievement effects. Their existence is implementation evidence for the focused slice,
not proof that any legacy caller reaches them.

## Commands, Queries, Adapters, and Projections

Commands and Queries are responsibilities in this design, not a requirement to create a type for
every row.

| Responsibility | Proposed API | Read/write rule |
|---|---|---|
| Encounter transition | `StartCountdown`, `TickCountdown`, `CreateCountdownSyncView` | `TickCountdown` commits once; sync view is immutable |
| Invasion transition | `BeginWave`, `StopWave`, `CreateInvasionSyncView` | tuple commit is atomic; duplicate stop is idempotent |
| Tower transition | `SetTowerActive`, `ApplyShieldDamage`, `RecomputeApocalypse` | shield and active changes share explicit visibility |
| Boss registry | `Register`, `TryResolve`, `TryReplace`, `ClearIfInactiveOrMismatched` | requires typed generation-aware key |
| Replication intent | `Mark`, `Acknowledge`, `Retry`, `ResetForReuse` | no packet construction in the component |
| Town progression | `MarkRescued`, `UnlockSpawn`, `MarkBookUsed`, `IsUnlocked` | first commit returns a new/duplicate result |
| Pet adoption | `AdoptPet` | command may emit chat, variation, and network effects through ports |
| Defeat progression | `MarkBossDefeated`, `MarkEventDefeated` | returns `FirstClearResult`; effects consume the result |
| Active scan | `BeginScan`, `MarkActive`, `CommitScan` | query reads only the current revision |
| Housing | `CommitHousingRelation`, `PublishHousingChange` | changed-only publication after atomic commit |
| Travel mode | `SetTravelNpcMode`, `ExpireForTimeOfDay` | scope must be resolved by calendar/world owner |
| Breath | `ApplyEnvironmentStep` | returns a transition result; effect ports apply damage/sync/visuals |
| Dialogue | compatibility read/write adapter only | no public new API until source contract exists |

`WorldFile` is a save adapter. `NetMessage` and `MessageBuffer` are network adapters. Legacy static
fields may be read through compatibility adapters during shadow migration, but an adapter must not
dual-write legacy fields and new components.

## State and Effect Closure

Every System must expose a read/write/emit contract before implementation:

| System | Read set | Write set | Emit set |
|---|---|---|---|
| `MoonLordEncounterSystem` | world clock, encounter definition, spawn availability | countdown state | spawn command, message 103 projection |
| `InvasionWaveProgressSystem` | event mode, wave input | points/kills/wave tuple | chat and message 78 projection |
| `LunarTowerEncounterSystem` | active NPC presence, tower facts | shields/active/apocalypse | tower effects and message 101/103 projections |
| `BossEntityIndexRegistrySystem` | lifecycle event, typed identity | registry entries | legacy slot projection |
| `NpcReplicationIntentSystem` | state dirty request, revision, ack | intent/revision state | packet scheduling request |
| `NpcProgressionCommitSystem` | defeat command and current flags | boss/event flags | first-clear world-effect request |
| `TownHousingSystem` | candidate room, NPC relation, TownManager result | current/old relation | message 60 and room projection |
| `NpcBreathSystem` | collision result, breath state, life status | breath/counter | life-damage, strike, sync, dust requests |

The following effects stay outside Components and pure Queries: NPC spawning/despawning, life
damage/strike, chat, item/projectile drops, WorldGen mutations, TownManager assignment, network
send/ack/retry, and file I/O.

## Scheduling and Visibility

The proposed schedule is a contract to be proven by the future runtime, not an inference from file
order:

```text
world/session load or network input
  -> validate and convert to command
  -> world progression / registry / scan / town / breath System
  -> commit barrier for authoritative Components
  -> effect ports
  -> save and network Projections
```

Within a game tick:

1. identity and lifecycle events establish valid NPC handles;
2. active-presence scan and world clock snapshots are available;
3. encounter, invasion, tower, town, housing, travel, and breath Systems commit their own state;
4. first-clear results and entity effects are published after the corresponding commit;
5. network and save projections read committed snapshots only.

The exact phase, barrier implementation, re-entrancy behavior, and multi-world scheduler are
`partial` until the real runtime entry points are traced.

## Compatibility and Integration Handoff

The following are deliberately not finalized in this partition:

- `crossSubsystemOwner: integration-review` for `WorldProgression` versus `NpcProgression` ownership
  of boss/event flags, Moon Lord countdown, invasion progress, and lunar apocalypse.
- `crossSubsystemOwner: integration-review` for `WorldGeneration` versus `Town` ownership of room
  assignment, resident key, revision, and `TownManager` persistence.
- `crossSubsystemOwner: integration-review` for NPC identity, slot generation, and the boss registry
  key. `NpcSlot`, `NpcEntityId`, `PersistentEntityId`, and `NetworkId` stay distinct.
- `crossSubsystemOwner: integration-review` for dirty-state scheduling, server authority, packet
  acknowledgement/retry, ordering, and duplicate suppression.
- `crossSubsystemOwner: integration-review` for `travelNPC`, because Main time and world spawn
  lifecycle both consume it.
- `crossSubsystemOwner: integration-review` for `nextDialogue`; the complete reader/writer/reset
  and projection contract is not evidenced.

The existing `WorldEventProgressState` aggregate must be reconciled with the proposed per-capability
components before any writer is enabled. Shadow mode may compare projections, but it must not allow
two authorities to write the same invariant.

## Non-goals

- No broad production routing, project-wide migration, or full behavior test is authorized by
  this design. The focused target seam and verifier are recorded in the execution document.
- No claim that any proposed System is runtime registered or callable in `src/NSSLC`.
- No automatic conversion of full-reference behavior into Version4 behavior.
- No new owner for unresolved cross-partition state.
- No deletion of legacy fields or writers.
- No migration-success or behavior-equivalence claim.

## Verification Gate

The design is accepted as a plan only when each later implementation slice can produce the full
observation tuple:

```text
Observation = (return_or_error, authoritative_state_delta,
               emitted_events_and_external_effects, order_and_visibility,
               lifecycle_and_scope, retry_and_idempotency)
```

The report and this document provide the design and static evidence baseline. The design remains
`designStatus: proposed`; the execution has only a focused core slice
(`executionStatus: partial-core-slice`, `verificationStatus: focused-passed-10-percent`). Full
runtime integration, behavior equivalence, and migration success remain unverified and are not
claimed.
