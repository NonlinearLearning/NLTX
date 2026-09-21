# Version4 P13 NPC, Town, Event, Boss, and World Progression Component Design

```yaml
partitionId: P13
sessionId: 06c2332691e84e2682405500e43d7be8
inputReport: D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P13-NPC-Town-Progression.md
designPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p13-npc-town-progression-component-design.md
executionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p13-npc-town-progression-component-execution.md
componentDesignPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p13-npc-town-progression-component-design.md
componentExecutionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p13-npc-town-progression-component-execution.md
designStatus: proposed
evidenceStatus: partial
currentNltxStatus: partial
executionStatus: in-progress
implementationStatus: in-progress
verificationStatus: partial
completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C15, C16, C17]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T07:02:53.7727506Z
evidence-gap: C01, C02, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C15, C16, and C17 have isolated owner builds and smoke evidence. C03 isolated registry build and boundary smoke passed, but its runtime allocator bridge, spawn/despawn writer, save/network projection, and behavior equivalence remain open. C04 isolated NPC build, focused verifier, and boundary smoke passed, but the Version4 scheduler caller, transport acknowledgement/retry path, entity lifecycle reset caller, client/server authority, duplicate packet suppression, and behavior equivalence remain open. Existing `NpcIdentityComponent` still marks Version4 stable instance identity as missing, while the C03 key carries the required instance, compatibility slot, generation, and NPC type explicitly. C09 now also has an explicit scan owner that consumes caller-provided active entries, but the real `Main.npc` caller, scheduler placement, lifecycle invalidation hooks, and behavior equivalence remain unverified. C10 WorldGen/Projectile writers and dynamic `downedMoonlord` shield policy, C11 boss defeat event routing and persistence/network adapters, C12 event-end routing and persistence/network adapters, C13 townNPC/dialogue/housing adapter integration, C14 travel selector/entity relation/persistence integration, C15 Town versus WorldGeneration housing owner, resident key mode, revision and projection integration, and C16 tile validity/door system/adapter integration also remain open. C17 Version4 environment caller, life-damage handling after breath reaches zero, liquid exception rules, save/network compatibility, scheduler placement, and behavior equivalence remain unverified. The integrated P13 focused verifier source has passed serial restore, build, and run for the 105 confirmed source-owner members; Version4 initializer parity and full runtime writer/reset/persistence/network closure remain unverified; `nextDialogue` readers and writers remain unproven.
blocking-decision: C03 isolated build and boundary smoke passed using an explicit temporary exclusion for the unrelated WorldGeneration source error; runtime lifecycle routing and legacy reader migration remain deferred until their callers are evidenced. C04 isolated build, existing focused verifier, and boundary smoke passed with one dirty-state owner; transport scheduler, send acknowledgement, retry integration, entity lifecycle caller, and client/server authority remain unverified. C01 remains isolated to immutable definition data and a bounded countdown state; WorldGen reset, item command, save projection, and network projection remain deferred until their writers and adapters are evidenced. C09 scan ownership is explicit and deterministic, but its real active-NPC enumeration caller, invalidation hooks, and runtime scheduling remain deferred because no evidence-backed integration boundary was found. C17 is limited to the confirmed immutable breath rules and deterministic state transition; health damage, environment reads, lifecycle scheduling, and projections remain outside the component until their callers are evidenced. The integrated P13 focused verifier passed its serial restore, build, and run, but the result is bounded to the confirmed source-owner members and does not close runtime integration or `nextDialogue` projection evidence. Integration review must still approve world-level ownership for boss/event/progression flags, NPC network dirty-state integration, housing authority between src/Town and src/WorldSession/WorldGeneration, and the nextDialogue projection boundary.
```

## 1. Scope and status

This document covers only the 10 leaf groups and 106 fields listed in the P13 authoritative report
under the formal parent `NpcAndTownSimulation`. It is a proposed component decomposition and an
implementation input. It is not a code migration, behavior-equivalence claim, API-compatibility
claim, network-closure claim, or persistence-closure claim.

Types, paths, systems, queries, commands, adapters, and projections that remain outside the saved
implementation checkpoints retain `status: proposed`. Existing NLTX files are evidence of partial
coverage only. The current implementation checkpoints have saved C01-C17 source files. C03 has
an isolated affected-project build and boundary smoke, while the complete WorldSession build
remains blocked by the unrelated `WorldGeneration/Systems/HellChestLootCycleSystem.cs:16` source
error. C04 has an isolated NPC build, focused verifier, and boundary smoke. Runtime lifecycle
callers, schedulers, transport acknowledgement/retry, persistence/network projections, and
behavior equivalence remain unverified. The current status is `executionStatus: in-progress`,
`implementationStatus: in-progress`, and `verificationStatus: partial`.

The report contains 106 fields and no properties. The complete member assignment is in Section 6;
the source sequence numbers are the coverage key and must remain unique. The 10 report leaf groups
are preserved as inventory boundaries, but several leaves are decomposed into narrower ownership
units because a legacy static field is not automatically an NPC entity component.

## 2. Boundary decision

The legacy `Terraria.NPC` declaration mixes world authority, per-NPC state, immutable rules, scan
caches, network intent, and compatibility values. The proposed model separates those concerns:

```text
world input / network command / NPC event
  -> pure eligibility or transition query
  -> one owner system / commit port
  -> world or NPC component state
  -> save/load, network, and presentation projections
```

The following rules are mandatory for implementation:

- `MoonLord*`, invasion progress, rescue/adoption/unlock flags, boss/event defeat flags, and tower
  event state are world-scoped unless an integration review proves an entity-scoped owner. They must
  not be copied onto every NPC entity.
- `golemBoss`, `plantBoss`, `crimsonBoss`, and `deerclopsBoss` are world registry slots pointing
  to active NPC instances. They are not the NPC's own identity or health. The final key must be
  resolved with the NPC identity and lifecycle owner before migration.
- `netUpdate` is a per-NPC replication intent. It must be integrated with the existing replication
  dirty-state and transport schedule; it is not a network packet or a second NPC authority.
- `npcsFoundForCheckActive` is a per-scan type cache. It is rebuilt or invalidated by an explicit
  scan owner and is not durable progression state.
- `downedMechBossAny` is retained during compatibility migration because Version4 explicitly stores,
  loads, and synchronizes it. It may become a derived projection only after one owner is proven;
  dual writes are forbidden.
- Tower shield/active/apocalypse runtime state is distinct from persistent `downedTower*` flags.
- `NaturalMoonlordCountdownTime`, `ItemMoonlordCountdownTime`, `KickOutLookForHomeTimeout`, and
  `breathMax` are rule/definition values. They are not mutable authority components.
- `oldHomeless`, `oldHomeTileX`, and `oldHomeTileY` are compatibility snapshots for housing
  transitions, not independent housing facts. `closeDoor`, `doorX`, and `doorY` are short-lived door
  interaction intent. `breath` and `breathCounter` remain per-NPC environmental state.
- `nextDialogue` has only a declaration-level match in the inspected snapshot. It stays in a
  deferred compatibility/projection seam until its complete reader/writer contract is evidenced.

## 3. Evidence register

| Source | Evidence | Design fact supported | evidenceStatus |
|---|---|---|---|
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:5915-5941` | Moon Lord definition/state and invasion progress declarations | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:6007-6015` | active boss index slots and per-NPC `netUpdate` declaration | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:6151-6293` | rescue, adoption, spawn unlock, tower, defeat, book, and active-cache declarations | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:6397-6437` | town, travel, housing, door, dialogue, and breath declarations | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:7119-7142` | active NPC type scan/cache lifecycle | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:8218-8256` | NPC defaults and reset behavior for instance fields | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:43033-43045` | housing relation update and NPC-side notification path | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:76956-76965` | housing update network notification boundary | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:65802-65965` | boss/event defeat writes and progression transitions | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NPC.cs:79413-79447` | breath and breath-counter updates | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\Main.cs:7407-7462` | world event/invasion state reads and writes | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\Main.cs:11807-11829` | world event and Moon Lord runtime transitions | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\Main.cs:13120-13135` | world-state reset and event cleanup paths | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\Main.cs:11656-11680` | active boss index cleanup on NPC lifecycle changes | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\WorldGen.cs:6502-6561` | world reset of progression and event state | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\WorldGen.cs:4828-4832,5102-5109,5271-5275,59527-59542` | town housing generation, assignment, and migration | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\WorldGen.cs:73045-73118` | lunar tower and Moon Lord countdown handling | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria.IO\\WorldFile.cs:1320-1453,2134-2503,3476-3489` | save/load and world progression serialization boundaries | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\NetMessage.cs:282-350,1359-1373` | network progression flags and NPC sync output | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria\\MessageBuffer.cs:1293-1301,2198-2206` | boss index and town-pet network inputs | confirmed |
| Version4 | `D:\\TRbackup\\Version4\\Terraria.GameContent\\TownRoomManager.cs:9-178` | room/housing domain is a separate manager boundary | confirmed |
| tModLoader v2026.07 | `class_n_p_c.html:1232-1234,1247-1249,1417-1419,1493-1498,3474-3477` | public boundary: netID/type, netUpdate, townNPC, whoAmI | confirmed-public-boundary |
| SS14 reference | `Content.Shared\\Anomaly\\Components\\AnomalyComponent.cs:18-20` and `SharedAnomalySystem.cs:27-48,332-369` | narrow component plus system-owned transition organization | organization-only |
| Current NLTX | `src/Npc`, `src/Town`, `src/WorldSession/WorldGeneration`, `src/WorldSession/WorldProgression` | partial existing identity, replication, housing, and progression boundaries | existing-evidence |

Evidence is sufficient to design ownership seams, but not to declare a migration complete. The
remaining gap is the per-member writer/reset/persistence/replication/scheduler matrix, especially
for compatibility values and cross-partition integration.

## 4. Proposed module vocabulary

The target remains domain-first. Paths below are proposed locations, not created files. Each public
type gets one same-named PascalCase file. Runtime order is declared by an explicit schedule, never
by file or directory order.

| Proposed type | Kind | Proposed namespace/path | Owner boundary |
|---|---|---|---|
| `MoonLordEncounterDefinition` | Definition | `Terraria.WorldSession.NpcProgression`, `src/WorldSession/NpcProgression/MoonLord/` | immutable attack tables, distances, and rule constants |
| `MoonLordEncounterStateComponent` | Component | `Terraria.WorldSession.NpcProgression`, same capability directory | one world encounter system owns countdown state |
| `InvasionWaveProgressStateComponent` | Component | `Terraria.WorldSession.NpcProgression`, `.../Invasion/` | one invasion commit system owns points, kills, wave |
| `BossEntityIndexRegistryComponent` | Component/registry | `Terraria.WorldSession.NpcProgression`, `.../Boss/` | world registry adapter resolves active NPC identity |
| `NpcNetworkSyncIntentComponent` | Component | `Terraria.Npc.Network`, `src/Npc/Network/` | NPC replication scheduler consumes and clears intent |
| `TownRescueProgressStateComponent` | Component | `Terraria.Town.Progression`, `src/Town/Progression/Rescue/` | rescue command handler |
| `TownPetAdoptionProgressStateComponent` | Component | `Terraria.Town.Progression`, `src/Town/Progression/Pets/` | purchase/adoption command handler |
| `TownSpawnUnlockStateComponent` | Component | `Terraria.Town.Progression`, `src/Town/Progression/Spawn/` | spawn-unlock commit system |
| `NpcProgressionBookUsageStateComponent` | Component | `Terraria.WorldSession.NpcProgression`, `src/WorldSession/NpcProgression/Books/` | progression-book command handler |
| `NpcActivePresenceCache` | Cache/Query workspace | `Terraria.Npc.Queries`, `src/Npc/Queries/` | active scan owner; non-durable and invalidated per scan |
| `LunarTowerEncounterStateComponent` | Component | `Terraria.WorldSession.NpcProgression`, `.../LunarTower/` | tower event system |
| `BossDefeatProgressionStateComponent` | Component | `Terraria.WorldSession.NpcProgression`, `.../Boss/` | defeat commit system and save projection |
| `EventDefeatProgressionStateComponent` | Component | `Terraria.WorldSession.NpcProgression`, `.../Events/` | event defeat commit system |
| `TownResidentStateComponent` | Component | `Terraria.Town.Residents`, `src/Town/Residents/` | NPC resident capability/state system |
| `TravelNpcWorldStateComponent` | Component | `Terraria.WorldSession.Town`, `src/WorldSession/Town/` | world travel-NPC lifecycle owner |
| `TownHousingRelationStateComponent` | Component | `Terraria.Town.Housing`, `src/Town/Housing/` | one housing assignment owner; existing duplicates require review |
| `TownDoorInteractionIntentComponent` | Component/intent | `Terraria.Town.Doors`, `src/Town/Doors/` | door interaction system consumes and clears intent |
| `NpcBreathStateComponent` | Component | `Terraria.Npc.Environment`, `src/Npc/Environment/` | environment/breath system |
| `NpcProgressionSaveProjection` | Projection/adapter | `Terraria.WorldStorage.NpcProgression`, `src/WorldStorage/NpcProgression/` | WorldFile adapter only |
| `NpcProgressionNetworkProjection` | Projection/adapter | `Terraria.Server.Npc.Progression`, `src/Npc/Network/Progression/` | NetMessage/MessageBuffer adapter only |
| `NpcProgressionQuery` | Query | capability-local `Queries/` directories | deterministic, read-only eligibility and views |
| `NpcProgressionCommand` | Command | capability-local `Commands/` directories | explicit state transition input |

No proposed type is an implementation claim. Existing `TownResidentComponent`,
`NpcHousingAssignmentComponent`, `TownHousingRelationComponent`, `NpcReplicationDirtyState`,
`ProgressionCommitStateComponent`, and `ProgressionAggregate` are mapped as partial evidence and
must not be treated as already owning the full P13 inventory.

## 5. Component, system, query, command, adapter, and projection boundaries

### Components

Components contain authoritative data for one clear lifecycle. World components hold world-level
progression and event state; NPC components hold state that varies by NPC instance. Definition values,
scan caches, network packets, file DTOs, and rendering objects do not enter mutable authority
components.

### Systems

Systems own transitions and side effects through explicit ports:

- `MoonLordEncounterSystem`, `InvasionWaveProgressSystem`, and `LunarTowerEncounterSystem` own
  runtime event transitions and emit committed facts.
- `BossEntityIndexSystem` owns registry insert, replacement, and cleanup against NPC lifecycle
  events; it never becomes an NPC entity's identity component.
- `NpcProgressionCommitSystem` owns rescue, adoption, unlock, book, and defeat commands. Each field
  has one writer during migration.
- `TownHousingSystem`, `TownDoorInteractionSystem`, and `NpcBreathSystem` own per-NPC transitions.
- `NpcReplicationProjectionSystem` and `NpcProgressionSaveProjectionSystem` are output boundaries;
  they cannot write authority back while serializing.

### Queries

Queries are deterministic and read-only. Spawn eligibility, rescue eligibility, active-NPC presence,
housing eligibility, tower status, and defeat summaries are query results. `npcsFoundForCheckActive`
is a query workspace/cache with an explicit scan owner and invalidation policy.

### Commands and events

Commands express requested transitions (`RescueTownNpcCommand`, `AdoptTownPetCommand`,
`UnlockNpcSpawnCommand`, `RegisterBossEntityCommand`, `SetNpcDoorIntentCommand`, and
`ApplyNpcDefeatCommand`). Events express committed facts (`NpcRescued`, `BossDefeated`,
`TowerShieldChanged`, `NpcHousingChanged`, and `NpcBreathChanged`). Commands are not components and
events are not durable state unless a projection explicitly commits them.

### Adapters and projections

WorldFile serialization, NetMessage/MessageBuffer packet formats, legacy static field bridges, and
the existing housing registries are adapters. They translate at a boundary and do not become a
second authority. During migration an adapter may read the new owner, but it must never dual-write
the legacy field and the new component.

## 6. Complete 106-member assignment

The following table is the complete P13 inventory. `source` is the authoritative report sequence
number. Every number appears exactly once; no member from another partition is included.

| Checkpoint | Source | Member | C# type | Proposed boundary |
|---|---:|---|---|---|
| C01 | 1587 | `MoonLordAttacksArray` | `int[,,,]` | `MoonLordEncounterDefinition` |
| C01 | 1588 | `MoonLordAttacksArray2` | `int[,]` | `MoonLordEncounterDefinition` |
| C01 | 1589 | `MoonLordFightingDistance` | `int` | `MoonLordEncounterDefinition` compatibility policy |
| C01 | 1590 | `MoonLordCountdown` | `int` | `MoonLordEncounterStateComponent` |
| C01 | 1591 | `MaxMoonLordCountdown` | `int` | encounter rule/config seam; single compatibility owner |
| C01 | 1592 | `NaturalMoonlordCountdownTime` | `int` | immutable encounter definition |
| C01 | 1593 | `ItemMoonlordCountdownTime` | `int` | immutable encounter definition |
| C02 | 1598 | `totalInvasionPoints` | `float` | `InvasionWaveProgressStateComponent` |
| C02 | 1599 | `waveKills` | `float` | `InvasionWaveProgressStateComponent` |
| C02 | 1600 | `waveNumber` | `int` | `InvasionWaveProgressStateComponent` |
| C03 | 1633 | `golemBoss` | `int` | `BossEntityIndexRegistryComponent` |
| C03 | 1634 | `plantBoss` | `int` | `BossEntityIndexRegistryComponent` |
| C03 | 1635 | `crimsonBoss` | `int` | `BossEntityIndexRegistryComponent` |
| C03 | 1636 | `deerclopsBoss` | `int` | `BossEntityIndexRegistryComponent` |
| C04 | 1637 | `netUpdate` | `bool` | `NpcNetworkSyncIntentComponent` |
| C05 | 1705 | `savedTaxCollector` | `bool` | `TownRescueProgressStateComponent` |
| C05 | 1706 | `savedGoblin` | `bool` | `TownRescueProgressStateComponent` |
| C05 | 1707 | `savedWizard` | `bool` | `TownRescueProgressStateComponent` |
| C05 | 1708 | `savedMech` | `bool` | `TownRescueProgressStateComponent` |
| C05 | 1709 | `savedAngler` | `bool` | `TownRescueProgressStateComponent` |
| C05 | 1710 | `savedStylist` | `bool` | `TownRescueProgressStateComponent` |
| C05 | 1711 | `savedBartender` | `bool` | `TownRescueProgressStateComponent` |
| C05 | 1712 | `savedGolfer` | `bool` | `TownRescueProgressStateComponent` |
| C06 | 1713 | `boughtCat` | `bool` | `TownPetAdoptionProgressStateComponent` |
| C06 | 1714 | `boughtDog` | `bool` | `TownPetAdoptionProgressStateComponent` |
| C06 | 1715 | `boughtBunny` | `bool` | `TownPetAdoptionProgressStateComponent` |
| C07 | 1716 | `unlockedSlimeBlueSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1717 | `unlockedSlimeGreenSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1718 | `unlockedSlimeOldSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1719 | `unlockedSlimePurpleSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1720 | `unlockedSlimeRainbowSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1721 | `unlockedSlimeRedSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1722 | `unlockedSlimeYellowSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1723 | `unlockedSlimeCopperSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1724 | `unlockedMerchantSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1725 | `unlockedDemolitionistSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1726 | `unlockedPartyGirlSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1727 | `unlockedDyeTraderSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1728 | `unlockedTruffleSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1729 | `unlockedArmsDealerSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1730 | `unlockedNurseSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C07 | 1731 | `unlockedPrincessSpawn` | `bool` | `TownSpawnUnlockStateComponent` |
| C08 | 1732 | `combatBookWasUsed` | `bool` | `NpcProgressionBookUsageStateComponent` |
| C08 | 1733 | `combatBookVolumeTwoWasUsed` | `bool` | `NpcProgressionBookUsageStateComponent` |
| C08 | 1734 | `peddlersSatchelWasUsed` | `bool` | `NpcProgressionBookUsageStateComponent` |
| C11 | 1735 | `downedBoss1` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1736 | `downedBoss2` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1737 | `downedBoss3` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1738 | `downedQueenBee` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1739 | `downedSlimeKing` | `bool` | `BossDefeatProgressionStateComponent` |
| C12 | 1740 | `downedGoblins` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1741 | `downedFrost` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1742 | `downedPirates` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1743 | `downedClown` | `bool` | `EventDefeatProgressionStateComponent` |
| C11 | 1744 | `downedPlantBoss` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1745 | `downedGolemBoss` | `bool` | `BossDefeatProgressionStateComponent` |
| C12 | 1746 | `downedMartians` | `bool` | `EventDefeatProgressionStateComponent` |
| C11 | 1747 | `downedFishron` | `bool` | `BossDefeatProgressionStateComponent` |
| C12 | 1748 | `downedHalloweenTree` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1749 | `downedHalloweenKing` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1750 | `downedChristmasIceQueen` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1751 | `downedChristmasTree` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1752 | `downedChristmasSantank` | `bool` | `EventDefeatProgressionStateComponent` |
| C11 | 1753 | `downedAncientCultist` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1754 | `downedMoonlord` | `bool` | `BossDefeatProgressionStateComponent` |
| C12 | 1755 | `downedTowerSolar` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1756 | `downedTowerVortex` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1757 | `downedTowerNebula` | `bool` | `EventDefeatProgressionStateComponent` |
| C12 | 1758 | `downedTowerStardust` | `bool` | `EventDefeatProgressionStateComponent` |
| C11 | 1759 | `downedEmpressOfLight` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1760 | `downedQueenSlime` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1761 | `downedDeerclops` | `bool` | `BossDefeatProgressionStateComponent` |
| C10 | 1762 | `ShieldStrengthTowerSolar` | `int` | `LunarTowerEncounterStateComponent` |
| C10 | 1763 | `ShieldStrengthTowerVortex` | `int` | `LunarTowerEncounterStateComponent` |
| C10 | 1764 | `ShieldStrengthTowerNebula` | `int` | `LunarTowerEncounterStateComponent` |
| C10 | 1765 | `ShieldStrengthTowerStardust` | `int` | `LunarTowerEncounterStateComponent` |
| C10 | 1766 | `LunarShieldPowerNormal` | `int` | `LunarTowerEncounterStateComponent` |
| C10 | 1767 | `TowerActiveSolar` | `bool` | `LunarTowerEncounterStateComponent` |
| C10 | 1768 | `TowerActiveVortex` | `bool` | `LunarTowerEncounterStateComponent` |
| C10 | 1769 | `TowerActiveNebula` | `bool` | `LunarTowerEncounterStateComponent` |
| C10 | 1770 | `TowerActiveStardust` | `bool` | `LunarTowerEncounterStateComponent` |
| C10 | 1771 | `LunarApocalypseIsUp` | `bool` | `LunarTowerEncounterStateComponent` |
| C11 | 1772 | `downedMechBossAny` | `bool` | `BossDefeatProgressionStateComponent`, compatibility-preserved |
| C11 | 1773 | `downedMechBoss1` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1774 | `downedMechBoss2` | `bool` | `BossDefeatProgressionStateComponent` |
| C11 | 1775 | `downedMechBoss3` | `bool` | `BossDefeatProgressionStateComponent` |
| C09 | 1776 | `npcsFoundForCheckActive` | `bool[]` | `NpcActivePresenceCache`, scan-owned workspace |
| C13 | 1828 | `townNPC` | `bool` | `TownResidentStateComponent` |
| C13 | 1829 | `nextDialogue` | `Terraria.GameContent.ConditionalDialogue` | deferred dialogue compatibility projection |
| C14 | 1830 | `travelNPC` | `bool` | `TravelNpcWorldStateComponent` |
| C15 | 1831 | `homeless` | `bool` | `TownHousingRelationStateComponent` |
| C15 | 1832 | `homelessDespawn` | `bool` | `TownHousingRelationStateComponent` |
| C15 | 1833 | `lookForHomeTimeout` | `int` | `TownHousingRelationStateComponent` |
| C15 | 1834 | `KickOutLookForHomeTimeout` | `int` | housing rule definition |
| C15 | 1835 | `homeTileX` | `int` | `TownHousingRelationStateComponent` |
| C15 | 1836 | `homeTileY` | `int` | `TownHousingRelationStateComponent` |
| C15 | 1837 | `housingCategory` | `int` | resident/housing relation value |
| C15 | 1838 | `oldHomeless` | `bool` | housing compatibility snapshot |
| C15 | 1839 | `oldHomeTileX` | `int` | housing compatibility snapshot |
| C15 | 1840 | `oldHomeTileY` | `int` | housing compatibility snapshot |
| C16 | 1842 | `closeDoor` | `bool` | `TownDoorInteractionIntentComponent` |
| C16 | 1843 | `doorX` | `int` | `TownDoorInteractionIntentComponent` |
| C16 | 1844 | `doorY` | `int` | `TownDoorInteractionIntentComponent` |
| C17 | 1846 | `breath` | `int` | `NpcBreathStateComponent` |
| C17 | 1847 | `breathMax` | `int` | immutable breath rule definition |
| C17 | 1848 | `breathCounter` | `int` | `NpcBreathStateComponent` |

Inventory checks: 10 leaf groups, 106 fields, 0 properties, 106 total members; source sequence
numbers are unique and span 1587..1848. The apparent gaps in the numeric range are members from
other authoritative partitions and are intentionally excluded.

Leaf-group attribution is explicit below. A report leaf may be refined into multiple checkpoints
when its members have different ownership lifecycles; the source report grouping itself is not
discarded.

| Authoritative report leaf group | Source members | Design checkpoints |
|---|---|---|
| `NpcBossAndInvasionGlobalState` | 1587..1593, 1598..1600 | C01, C02 |
| `NpcBossAndInvasionState` | 1633..1637 | C03, C04 |
| `NpcTownRescueState` | 1705..1712 | C05 |
| `NpcTownPetAdoptionState` | 1713..1715 | C06 |
| `NpcTownSpawnUnlockState` | 1716..1731 | C07 |
| `NpcProgressionBookAndActiveRegistryState` | 1732..1734, 1776 | C08, C09 |
| `NpcTowerAndEventShieldState` | 1762..1771 | C10 |
| `NpcBossDefeatFlags` | 1735..1739, 1744..1745, 1747, 1753..1754, 1759..1761, 1772..1775 | C11 |
| `NpcEventDefeatFlags` | 1740..1743, 1746, 1748..1752, 1755..1758 | C12 |
| `NpcTownHousingAndBreathState` | 1828..1840, 1842..1844, 1846..1848 | C13, C14, C15, C16, C17 |

## 7. Completed checkpoint: C01 MoonLordEncounterDefinitionAndState

The first checkpoint is complete as a design ledger, covering source members 1587..1593. The
proposed split is:

- `MoonLordAttacksArray` and `MoonLordAttacksArray2` are immutable encounter definitions loaded
  from a catalog or initialized definition provider. Their arrays are not placed in every NPC.
- `MoonLordFightingDistance` and `MaxMoonLordCountdown` remain a compatibility policy seam until
  all writers are known. A single encounter definition/config owner must expose them.
- `MoonLordCountdown` is mutable world encounter state and is written only by the Moon Lord
  encounter system through a commit port.
- `NaturalMoonlordCountdownTime` and `ItemMoonlordCountdownTime` are immutable rules and remain
  definition values.

The system must distinguish a countdown request from a committed countdown value. World reset,
item-triggered countdown, NPC despawn, and network/save projections are separate transitions. A
projection may read the committed state but cannot update it.

## 7.1 Completed checkpoint: C02 InvasionWaveProgressState

The three invasion members are one world-scoped progress unit:

| Source | Member | Classification | Owner and write rule |
|---:|---|---|---|
| 1598 | `totalInvasionPoints` | mutable encounter progress | `InvasionWaveProgressStateComponent`; invasion system commits a bounded result |
| 1599 | `waveKills` | mutable wave counter | same owner; increment/reset occurs in the same commit as points |
| 1600 | `waveNumber` | mutable wave phase | same owner; phase transitions are explicit and monotonic within a run |

`InvasionWaveProgressSystem` consumes kill and wave commands, reads a pure point-calculation
definition, and commits all three values together. `InvasionWaveQuery` exposes read-only progress
and eligibility views to spawn, UI, event, and network callers. The system owns reset on invasion
start/end and world reset; no NPC entity owns a copy of the counters.

The existing `ProgressionAggregate` remains a separate generic kill/sight/chat aggregate. It cannot
become a second writer for invasion points or wave values. Save and network adapters read the
committed component, and a repeated kill/event command must be idempotent or rejected by the owner
system before changing the counters.

## 7.2 Completed checkpoint: C03 BossEntityIndexRegistry

The four static integer members are a world registry, not per-NPC components:

| Source | Member | Legacy meaning | Proposed owner |
|---:|---|---|---|
| 1633 | `golemBoss` | active Golem NPC slot/index | `BossEntityIndexRegistryComponent` keyed by `BossKind.Golem` |
| 1634 | `plantBoss` | active Plantera NPC slot/index | same registry, `BossKind.Plant` |
| 1635 | `crimsonBoss` | active Crimson Boss NPC slot/index | same registry, `BossKind.Crimson` |
| 1636 | `deerclopsBoss` | active Deerclops NPC slot/index | same registry, `BossKind.Deerclops` |

`BossEntityIndexSystem` receives explicit register, replace, and clear commands from NPC spawn and
despawn lifecycle events. The registry value must carry or resolve a generation-aware NPC identity;
an unqualified `int` slot is only a legacy adapter view. Queries reject an index whose slot has been
reused, whose NPC type no longer matches the boss kind, or whose entity is inactive. Cleanup runs on
both normal terminal transition and world reset.

The key model (`NpcInstanceId`, legacy slot plus generation, or a dual adapter key) is an
integration-review decision. Until it is approved, no code may make `golemBoss` et al. the sole
identity of an NPC, and no boss registry writer may also live in an NPC component. Network and save
projections expose stable approved keys or omit the runtime slot as required by their protocol.

## 7.3 Completed checkpoint: C04 NpcNetworkSyncIntent

`netUpdate` (source 1637) is a per-NPC intent to make the server schedule an NPC synchronization.
The proposed `NpcNetworkSyncIntentComponent` contains intent state and, if required by the existing
dirty-state contract, a reason or revision value; it does not contain packet DTOs, transport handles,
client cursors, or serialized bytes.

`NpcStateMutationSystem` marks the intent after an authoritative NPC change. The existing
`NpcReplicationDirtyState` and the network scheduler are integration evidence, not a second owner:
the scheduler coalesces repeated marks, emits `NpcProgressionNetworkProjection` through the approved
adapter, and acknowledges/clears the intent only according to send success and the existing retry
contract. A failed send must not silently clear a required synchronization.

The intent is entity-scoped and reset with entity slot reuse. It must be independent from world
progression flags, boss registry state, and network packet shape. The exact composition with
`NpcReplicationFlags` and the server/client authority boundary requires integration-review.

## 7.4 Completed checkpoint: C05 TownRescueProgressionState

The eight rescue flags are world-level progression facts:

| Source range | Members | Proposed state |
|---|---|---|
| 1705..1712 | `savedTaxCollector`, `savedGoblin`, `savedWizard`, `savedMech`, `savedAngler`, `savedStylist`, `savedBartender`, `savedGolfer` | `TownRescueProgressStateComponent` with one typed rescue-key value per legacy flag |

`TownRescueProgressSystem` accepts an explicit rescue commit command, validates the target rescue
kind, and performs an idempotent transition from false to true. Spawn eligibility and town resident
creation read the committed state through a query. The rescued NPC entity, if present, is a result of
the progression transition and is not the owner of the world flag.

The flags are persistent world state and use the P13 save projection. Network/UI notifications are
one-way projections. World reset clears them through the same owner system. A compatibility adapter
may expose the old static names, but cannot write them alongside the component.

## 7.5 Completed checkpoint: C06 TownPetAdoptionProgressionState

The three purchase flags are world-level adoption progression:

| Source | Member | Proposed owner | Transition |
|---:|---|---|---|
| 1713 | `boughtCat` | `TownPetAdoptionProgressStateComponent` | `AdoptTownPetCommand(PetKind.Cat)` |
| 1714 | `boughtDog` | same component | `AdoptTownPetCommand(PetKind.Dog)` |
| 1715 | `boughtBunny` | same component | `AdoptTownPetCommand(PetKind.Bunny)` |

The purchase input adapter validates the item/player interaction and submits a typed command. The
adoption system commits the flag once, emits an adoption event, and exposes a read-only query for
town-pet spawning and dialogue. The pet entity and item transaction are downstream effects; neither
becomes a second writer for the world flag.

The flags use the save projection and any network notification uses the progression projection. A
duplicate purchase command must not repeat the state transition or spawn an unbounded duplicate pet.
World reset and old-save migration use explicit defaults. The static legacy names remain a read
compatibility surface only after the owner is installed.

## 7.6 Completed checkpoint: C07 TownSpawnUnlockState

The sixteen spawn-unlock members are one world progression set:

| Source range | Unlock keys |
|---|---|
| 1716..1723 | `SlimeBlue`, `SlimeGreen`, `SlimeOld`, `SlimePurple`, `SlimeRainbow`, `SlimeRed`, `SlimeYellow`, `SlimeCopper` |
| 1724..1731 | `Merchant`, `Demolitionist`, `PartyGirl`, `DyeTrader`, `Truffle`, `ArmsDealer`, `Nurse`, `Princess` |

`TownSpawnUnlockStateComponent` stores the committed boolean set through typed unlock keys. The
unlock system is the only writer and applies idempotent unlock commands. `NpcSpawnEligibilityQuery`
reads the set along with world conditions and NPC definitions; it does not mutate the progression
state. The component remains world-scoped even when a flag enables a particular NPC type.

The legacy boolean names are a compatibility projection. Save/load and network output use a versioned
unlock projection, with explicit defaults for old worlds. The implementation must keep spawn rules
and unlock facts separate: a failed spawn attempt cannot clear an unlock, and a definition/catalog
change cannot silently rewrite the saved fact.

## 7.7 Completed checkpoint: C08 NpcProgressionBookUsageState

The three book-use members are world progression facts:

| Source | Member | Proposed owner |
|---:|---|---|
| 1732 | `combatBookWasUsed` | `NpcProgressionBookUsageStateComponent`, key `CombatBook` |
| 1733 | `combatBookVolumeTwoWasUsed` | same component, key `CombatBookVolumeTwo` |
| 1734 | `peddlersSatchelWasUsed` | same component, key `PeddlersSatchel` |

`NpcProgressionBookSystem` validates an item-use command and commits a one-way, idempotent state
transition. Item effects, combat modifiers, NPC spawn rules, and UI messages consume the committed
event or a read-only query. `ProgressionAggregate` remains unrelated generic progression data and
cannot write these flags.

The old static fields are read compatibility views. Save/load stores the usage set with an explicit
schema version; network output is a projection. A rejected item use must not set a flag, and a
replayed accepted command must not repeat the effect or increment unrelated progression.

## 7.8 Completed checkpoint: C09 NpcActivePresenceCache

`npcsFoundForCheckActive` (source 1776) is a per-scan `bool[]` workspace keyed by NPC type. It is
not durable progression, NPC identity, or a network state. The proposed `NpcActivePresenceCache`
is owned by the active-population scan system and has an explicit generation/tick or scan revision.

The scan resets or allocates the workspace for each scan, marks types from currently active NPC
entities, and publishes an immutable/read-only query result for callers that need active-presence
information. NPC spawn/despawn lifecycle events invalidate the cache; consumers must not treat a
stale cache as authoritative. The cache is excluded from save/load and network projections and is
not writable by spawn eligibility or progression systems.

If a future optimization retains the array between scans, the revision and invalidation contract must
be part of the cache owner; a raw static mutable array is not an acceptable component boundary.

## 7.9 Completed checkpoint: C10 LunarTowerEncounterState

The ten tower members are runtime event state:

| Source range | Members | Proposed owner |
|---|---|---|
| 1762..1765 | `ShieldStrengthTowerSolar`, `ShieldStrengthTowerVortex`, `ShieldStrengthTowerNebula`, `ShieldStrengthTowerStardust` | `LunarTowerEncounterStateComponent` shield value set |
| 1766 | `LunarShieldPowerNormal` | same component, normalized shield policy/state |
| 1767..1770 | `TowerActiveSolar`, `TowerActiveVortex`, `TowerActiveNebula`, `TowerActiveStardust` | same component active set |
| 1771 | `LunarApocalypseIsUp` | same component encounter phase state |

`LunarTowerEncounterSystem` owns shield damage, tower activation, apocalypse transitions, and reset
on world/event lifecycle. It consumes tower and NPC events through commands and emits committed
shield/phase events. Tile, player, NPC, and network systems read queries or projections; none writes
these fields directly.

The runtime component is intentionally separate from `downedTowerSolar`, `downedTowerVortex`,
`downedTowerNebula`, and `downedTowerStardust` in C12. A tower can be active, shielded, defeated,
or reset according to distinct transitions. The implementation must not infer persistent defeat from
current shield or active state, and must not revive runtime state from a stale defeat flag without an
explicit world transition.

## 7.10 Completed checkpoint: C11 BossDefeatProgressionState

The 17 boss defeat flags are persistent world progression:

| Source range | Members | Proposed owner |
|---|---|---|
| 1735..1739 | `downedBoss1`, `downedBoss2`, `downedBoss3`, `downedQueenBee`, `downedSlimeKing` | `BossDefeatProgressionStateComponent` |
| 1744..1745 | `downedPlantBoss`, `downedGolemBoss` | same component |
| 1747 | `downedFishron` | same component |
| 1753..1754 | `downedAncientCultist`, `downedMoonlord` | same component |
| 1759..1761 | `downedEmpressOfLight`, `downedQueenSlime`, `downedDeerclops` | same component |
| 1772..1775 | `downedMechBossAny`, `downedMechBoss1`, `downedMechBoss2`, `downedMechBoss3` | same component; `Any` compatibility policy |

`BossDefeatProgressionSystem` commits a typed defeat fact once, after the authoritative defeat
event. It owns reset, save/load projection input, and read-only defeat queries. Boss entities and
the active boss registry can emit a defeat command/event, but cannot write the persistent flag
directly.

`downedMechBossAny` is explicitly retained as a compatibility field during migration because the
inspected Version4 paths write, save, load, and synchronize it. The implementation must choose one
of two reviewed policies: (a) store it as a first-class field under the defeat owner, or (b) derive
it in one projection and remove its stored writes after a schema decision. Until that decision is
approved, policy (a) is the plan and all three `downedMechBoss1..3` transitions must update it in
the same commit. No independent derived writer is permitted.

## 7.11 Completed checkpoint: C12 EventDefeatProgressionState

The 14 event defeat fields are persistent world facts:

| Source range | Members | Proposed owner |
|---|---|---|
| 1740..1743 | `downedGoblins`, `downedFrost`, `downedPirates`, `downedClown` | `EventDefeatProgressionStateComponent` |
| 1746 | `downedMartians` | same component |
| 1748..1752 | `downedHalloweenTree`, `downedHalloweenKing`, `downedChristmasIceQueen`, `downedChristmasTree`, `downedChristmasSantank` | same component |
| 1755..1758 | `downedTowerSolar`, `downedTowerVortex`, `downedTowerNebula`, `downedTowerStardust` | same component; distinct from C10 runtime tower state |

`EventDefeatProgressionSystem` commits typed event completion after the authoritative event-end
transition. Invasion, seasonal, Martian, and tower runtime systems emit commands/events but cannot
write the persistent flags. Event eligibility and content unlocks read a query over committed state.

Tower defeat flags are stored here even while tower shield and active values remain in C10. Save/load
and network projections use the event progression owner. World reset and event replay are explicit
commands; duplicate event-end messages must be idempotent.

## 7.12 Completed checkpoint: C13 TownResidentState

`townNPC` (source 1828) is per-NPC resident capability state. The proposed
`TownResidentStateComponent` owns the authoritative resident classification and exposes read-only
capability queries to housing, dialogue, spawn, and town services. It is distinct from world rescue
flags and from housing assignment: being a town resident does not itself assign a room.

`nextDialogue` (source 1829) remains `deferred`. The inspected snapshot establishes its declaration
type (`Terraria.GameContent.ConditionalDialogue`) but does not close its complete readers, writers,
reset behavior, network contract, or persistence contract. The design therefore places it behind a
`TownDialogueCompatibilityProjection` seam with no proposed authoritative component. Creating a
definite owner now would be speculation.

The existing `src/Town/TownResidentComponent.cs` is partial evidence. C13 must reconcile its
capability model and public API with `townNPC` through one adapter, without silently treating the
existing type as a complete migration.

## 7.13 Completed checkpoint: C14 TravelNpcWorldState

`travelNPC` (source 1830) is a world-level runtime marker for the traveling NPC lifecycle. It is
not a property of every NPC entity and is not equivalent to `townNPC`. The proposed
`TravelNpcWorldStateComponent` is owned by a world travel-NPC system that selects, spawns, maintains,
and clears the traveling role through explicit lifecycle commands.

The actual NPC entity, if spawned, references the world role through the approved identity/lifecycle
relation. Despawn, day/event transitions, world reset, and player-visible notification are separate
commands or projections. Save/network persistence of the marker must follow the Version4 contract;
the component must not be duplicated in both a world singleton and each NPC entity.

## 7.14 Completed checkpoint: C15 TownHousingRelationState

The housing members have different semantic roles and must not be copied into one undifferentiated
blob:

| Source | Members | Proposed classification |
|---|---|---|
| 1831..1832 | `homeless`, `homelessDespawn` | authoritative NPC-to-housing relation and despawn policy |
| 1833 | `lookForHomeTimeout` | mutable housing search timer |
| 1834 | `KickOutLookForHomeTimeout` | immutable housing rule/definition |
| 1835..1836 | `homeTileX`, `homeTileY` | assigned home relation value |
| 1837 | `housingCategory` | resident/housing eligibility value; final owner requires town integration |
| 1838..1840 | `oldHomeless`, `oldHomeTileX`, `oldHomeTileY` | transition compatibility snapshot, not independent fact |

`TownHousingRelationStateComponent` is the proposed entity relation boundary, while
`TownHousingSystem` owns assignment, search timeout, homeless transition, and compatibility snapshot
updates. `TownRoomManager`/housing scan code supplies room candidates through a query/adapter; it
does not write NPC state outside the housing commit port. A successful assignment updates the
relation and revision atomically, then publishes a housing-changed event.

The repository currently has `src/Town/NpcHousingAssignmentComponent.cs`,
`src/Town/TownHousingRelationComponent.cs`, and overlapping
`src/WorldSession/WorldGeneration` housing/scan types. C15 does not declare any one existing type
to be the final owner. Integration-review must settle the `TownHousingResidentKey` shape (NPC type,
instance, or dual key), project references, and whether world-generation scanning or Town owns the
commit. Until then, use one adapter and forbid dual writes.

Housing save/network projections serialize the approved stable relation key and assigned tile as
required; query caches and old snapshots are not independently persisted authority unless the legacy
compatibility contract proves they must be retained.

## 7.15 Completed checkpoint: C16 TownDoorInteractionIntent

`closeDoor`, `doorX`, and `doorY` (sources 1842..1844) form one short-lived per-NPC interaction
intent. `TownDoorInteractionIntentComponent` stores the requested action and tile coordinate until
`TownDoorInteractionSystem` validates the current tile/door state and consumes or rejects it. Door
opening/closing, tile mutation, sounds, and network messages are adapter side effects; they do not
belong in the component or command payload after commit.

The intent is cleared after a terminal result or explicit expiry, and is reset on NPC slot reuse.
Repeated ticks must not close the same door repeatedly. Invalid coordinates, changed tiles, missing
doors, and rejected interactions produce a result event without mutating housing relation or world
progression. The movement/door schedule and tile adapter are integration boundaries.

## 7.16 Completed checkpoint: C17 NpcBreathState

The three breath members have separate roles:

| Source | Member | Proposed classification |
|---:|---|---|
| 1846 | `breath` | mutable per-NPC environmental state in `NpcBreathStateComponent` |
| 1847 | `breathMax` | immutable environment rule/definition value |
| 1848 | `breathCounter` | mutable per-NPC timing/substep state in the same component |

`NpcBreathSystem` reads an environment/liquid query and applies deterministic breath transitions to
the NPC-owned state. `breathMax` is supplied by a definition/rules port and is not written as mutable
entity state. NPC reset, environment changes, death/despawn, and slot reuse have explicit reset
semantics. Save/network behavior must follow the proven Version4 contract; a query or projection
cannot advance the counter.

This completes the 17 design checkpoints. `nextDialogue` remains an explicit deferred boundary, not
an unverified component. All proposed types remain `status: proposed`; no C# implementation was
created.

## 8. Planned checkpoint map

| Checkpoint | Proposed owner | Scope | Status |
|---|---|---|---|
| C01 `MoonLordEncounterDefinitionAndState` | `MoonLordEncounterDefinition`, `MoonLordEncounterStateComponent` | world | complete |
| C02 `InvasionWaveProgressState` | `InvasionWaveProgressStateComponent` | world | complete |
| C03 `BossEntityIndexRegistry` | `BossEntityIndexRegistryComponent` | world registry | complete |
| C04 `NpcNetworkSyncIntent` | `NpcNetworkSyncIntentComponent` | NPC entity | complete |
| C05 `TownRescueProgressionState` | `TownRescueProgressStateComponent` | world | complete |
| C06 `TownPetAdoptionProgressionState` | `TownPetAdoptionProgressStateComponent` | world | complete |
| C07 `TownSpawnUnlockState` | `TownSpawnUnlockStateComponent` | world | complete |
| C08 `NpcProgressionBookUsageState` | `NpcProgressionBookUsageStateComponent` | world | complete |
| C09 `NpcActivePresenceCache` | `NpcActivePresenceCache` | scan workspace | complete |
| C10 `LunarTowerEncounterState` | `LunarTowerEncounterStateComponent` | world event | complete |
| C11 `BossDefeatProgressionState` | `BossDefeatProgressionStateComponent` | world | complete |
| C12 `EventDefeatProgressionState` | `EventDefeatProgressionStateComponent` | world | complete |
| C13 `TownResidentState` | `TownResidentStateComponent` plus deferred dialogue projection | NPC entity | complete |
| C14 `TravelNpcWorldState` | `TravelNpcWorldStateComponent` | world | complete |
| C15 `TownHousingRelationState` | `TownHousingRelationStateComponent` | NPC entity + housing manager | complete |
| C16 `TownDoorInteractionIntent` | `TownDoorInteractionIntentComponent` | NPC entity intent | complete |
| C17 `NpcBreathState` | `NpcBreathStateComponent` | NPC entity | complete |

## 9. Dependency direction and schedule

Dependencies point from input adapters to commands, from commands to owner systems, and from owner
systems to components. Queries read components and definitions only. Projections read committed
state only. Cross-partition dependencies use explicit query, command, event, or projection ports.

The required schedule is explicit:

```text
1. Ingest world/NPC/network inputs and validate entity identity.
2. Resolve spawn, rescue, adoption, unlock, defeat, tower, housing, door, and dialogue commands.
3. Advance world encounter/progression systems in their declared schedule phase.
4. Advance NPC housing, door, resident, network-intent, and breath systems.
5. Rebuild or invalidate active presence caches; never persist a stale scan workspace.
6. Commit terminal and progression events exactly once through their owner systems.
7. Publish save, network, legacy-compatibility, and presentation projections.
```

This order is a plan only. The implementation must reconcile it with the Version4 call graph and
the shared NPC, WorldSession, housing, and network schedules before moving code.

## 10. Existing NLTX mapping and integration handoff

Current files provide useful partial boundaries but do not close this partition:

- `src/Npc/NpcReplicationDirtyState.cs` is a partial replication output/dirty-state boundary; C04
  must integrate `netUpdate` without making two dirty-state owners.
- `src/Town/TownResidentComponent.cs` is a partial resident capability model; C13 must reconcile
  `townNPC`, friendliness, housing category, and dialogue without broadening the component silently.
- `src/Town/NpcHousingAssignmentComponent.cs` and `src/Town/TownHousingRelationComponent.cs` are
  overlapping housing candidates. C15 must choose one authority and provide an adapter for the
  other, with `TownHousingResidentKey` identity still requiring an integration decision.
- `src/WorldSession/WorldGeneration/HousingScanStateComponent.cs`,
  `TownHousingRegistryComponent`, and `TownHousingAssignmentComponent` are world-generation and
  scan boundaries, not proof that NPC housing fields already have a single owner.
- `src/WorldSession/WorldProgression/ProgressionCommitStateComponent.cs` owns commit bookkeeping,
  not the P13 boss/event flags. `src/WorldProgressionAndUnlocks/ProgressionAggregate.cs` owns
  generic kill/sight/chat collections, not the P13 flag set.

Integration review must decide the final project references, world entity key model, network dirty
flag composition, save versioning, and the ownership relationship between town and world-generation
housing. Until then, all proposed types remain `status: proposed`.

## 11. Verification plan and evidence gap

Focused verification must cover, at minimum:

- default/reset and world-restart behavior for all world flags and counters;
- Moon Lord countdown rule versus mutable state and duplicate transition rejection;
- invasion wave progression, boss registry slot cleanup, stale entity index rejection, and
  `whoAmI`/identity mapping;
- one-writer checks for rescue, adoption, spawn unlock, book usage, boss defeat, event defeat, and
  `downedMechBossAny`;
- tower runtime state versus persistent tower defeat flags;
- active presence cache rebuild/invalidation and no save/network leakage;
- town resident, travel NPC, housing assignment, old-value compatibility snapshot, door intent,
  and breath reset/expiry behavior;
- save/load round trips and network projections through adapters only;
- `nextDialogue` behavior once reader/writer evidence is available.

Before the C01 implementation checkpoint, no verifier or build had run. C01 now has an affected-
project build and an isolated smoke result; runtime replay, network replay, persistence replay, and
integrated focused verification remain unverified.

## 12. Checkpoint log

| Checkpoint | Status | Evidence note |
|---|---|---|
| C01 `MoonLordEncounterDefinitionAndState` | complete for design ledger | 7 members assigned; mutable countdown separated from definitions |
| C02 `InvasionWaveProgressState` | complete for design ledger | world-scoped atomic points/kills/wave owner recorded |
| C03 `BossEntityIndexRegistry` | complete for design ledger | generation-aware registry recorded |
| C04 `NpcNetworkSyncIntent` | complete for design ledger | entity intent separated from transport |
| C05 `TownRescueProgressionState` | complete for design ledger | eight world rescue flags assigned |
| C06 `TownPetAdoptionProgressionState` | complete for design ledger | typed adoption owner assigned |
| C07 `TownSpawnUnlockState` | complete for design ledger | typed world unlock set assigned |
| C08 `NpcProgressionBookUsageState` | complete for design ledger | typed one-way book-use owner assigned |
| C09 `NpcActivePresenceCache` | complete for design ledger | scan-owned non-durable cache assigned |
| C10 `LunarTowerEncounterState` | complete for design ledger | runtime tower state separated from defeat persistence |
| C11 `BossDefeatProgressionState` | complete for design ledger | persistent boss flags and mech compatibility policy assigned |
| C12 `EventDefeatProgressionState` | complete for design ledger | event defeat persistence separated from runtime state |
| C13 `TownResidentState` | complete for design ledger | `townNPC` assigned; `nextDialogue` deferred |
| C14 `TravelNpcWorldState` | complete for design ledger | world travel role assigned |
| C15 `TownHousingRelationState` | complete for design ledger | relation/timer/rule/snapshot split assigned |
| C16 `TownDoorInteractionIntent` | complete for design ledger | consume-once door intent assigned |
| C17 `NpcBreathState` | complete for design ledger | breath state/rule split assigned |

## 13. Implementation Checkpoint: C01 MoonLordEncounterDefinitionAndState

- Actual source files saved:
  - `src/WorldSession/NpcProgression/MoonLord/MoonLordEncounterDefinition.cs`
  - `src/WorldSession/NpcProgression/MoonLord/MoonLordEncounterStateComponent.cs`
- Core behavior: `MoonLordEncounterDefinition` defensively copies both Version4 attack arrays and
  exposes defensive copies; fighting distance, maximum countdown, natural countdown, and item
  countdown are immutable definition values. `MoonLordEncounterStateComponent` owns only the mutable
  countdown, bounds updates by the supplied maximum, and resets to zero.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. No
  legacy Version4 source, public registration key, WorldGen writer, save adapter, network adapter, or
  other partition document was changed.
- Evidence boundary: the implementation does not claim the Version4 attack-table initializer,
  countdown writer, WorldGen reset path, item command, save format, or network projection are wired.
- Verification evidence: the serial `Terraria.WorldSession` build exited `0` with `0` warnings and
  `0` errors; the isolated smoke exited `0` and passed defensive-copy, countdown-bound, and reset
  assertions. The expected artifact is under
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`.
- Verification boundary: no integrated focused verifier, Version4 initializer parity check, save or
  network replay, or runtime behavior-equivalence result exists.

## 14. Implementation Checkpoint: C02 InvasionWaveProgressState

- Actual source file saved:
  - `src/WorldSession/NpcProgression/Invasion/InvasionWaveProgressStateComponent.cs`
- Core behavior: the component owns `TotalInvasionPoints`, `WaveKills`, and `WaveNumber` as one
  world-scoped commit. Points and kills must be finite/non-negative; wave number must be non-negative
  and cannot move backwards without an explicit reset. Validation completes before any field is
  assigned, and reset returns all three values to their Version4 defaults.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. The
  component does not reference or write `ProgressionAggregate`, NPC entities, invasion event APIs,
  persistence, or network transport.
- Evidence boundary: invasion kill/start/end commands, point calculation, legacy compatibility
  reads, save/network projections, and scheduler integration remain unwired and unverified.
- Verification status: affected-project build and isolated smoke passed; integrated invasion caller,
  projection, and behavior-equivalence verification remain not-run.

## 15. Verification Checkpoint: C02 InvasionWaveProgressState

- The serial `Terraria.WorldSession` build exited `0` with `0` warnings and `0` errors, producing
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`.
- The isolated smoke exited `0` and verified atomic points/kills/wave commit, rejection of a
  backwards wave without mutating prior state, and reset to zero.
- No invasion caller, `ProgressionAggregate` integration, save/network projection, scheduler, or
  Version4 behavior-equivalence evidence exists.

## 16. Implementation Checkpoint: C05 TownRescueProgressionState

- Actual source files saved:
  - `src/Town/Progression/Rescue/TownRescueKind.cs`
  - `src/Town/Progression/Rescue/TownRescueProgressStateComponent.cs`
- Core behavior: one typed rescue key maps to the eight Version4 rescue fields
  (`savedTaxCollector`, `savedGoblin`, `savedWizard`, `savedMech`, `savedAngler`, `savedStylist`,
  `savedBartender`, and `savedGolfer`). `MarkRescued` is idempotent, `IsRescued` is read-only, and
  `Reset` clears all eight values.
- Dependency impact: only the `Terraria.Town` project source set is extended. Existing NPC spawn,
  rescue event, save, network, and world-generation paths remain unchanged; no compatibility bridge
  or second writer was added.
- Evidence boundary: the source does not claim to replace Version4 rescue writers, spawn readers,
  WorldFile serialization, network flags, or scheduler registration.
- Verification evidence: the serial `Terraria.Town` build exited `0` with `0` warnings and `0` errors;
  the artifact is `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll`. The isolated smoke
  exited `0` and covered all eight typed rescue keys, idempotence, readback, and reset. Legacy
  writer replacement, projections, and behavior equivalence remain unverified.

## 17. Implementation Checkpoint: C06 TownPetAdoptionProgressionState

- Actual source files saved:
  - `src/Town/Progression/Pets/TownPetKind.cs`
  - `src/Town/Progression/Pets/TownPetAdoptionProgressStateComponent.cs`
- Core behavior: `TownPetKind` maps `Cat`, `Dog`, and `Bunny` to the three Version4 purchase flags.
  `Adopt` is idempotent, `IsAdopted` is read-only, and `Reset` clears all three world progression
  values. Item/player validation, pet spawning, and notifications remain outside the component.
- Dependency impact: only the `Terraria.Town` project source set is extended. No item transaction,
  pet population, WorldFile, MessageBuffer, network projection, registration key, or legacy writer
  was changed.
- Evidence boundary: Version4 purchase input and output paths are not replaced; save/network round
  trip and duplicate command behavior remain to be verified at the adapter/system boundary.
- Verification evidence: the serial `Terraria.Town` build exited `0` with `0` warnings and `0` errors;
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The isolated smoke exited `0`
  and covered Cat, Dog, and Bunny first adoption, duplicate rejection, readback, and reset. Item,
  spawn, save/network, and behavior-equivalence integration remains unverified.

## 18. Implementation Checkpoint: C07 TownSpawnUnlockState

- Saved source files before verification:
  - `src/Town/Progression/Spawn/TownSpawnUnlockKind.cs`
  - `src/Town/Progression/Spawn/TownSpawnUnlockStateComponent.cs`
- Core behavior: `TownSpawnUnlockKind` provides typed keys for all sixteen Version4 spawn-unlock
  fields (`SlimeBlue`, `SlimeGreen`, `SlimeOld`, `SlimePurple`, `SlimeRainbow`, `SlimeRed`,
  `SlimeYellow`, `SlimeCopper`, `Merchant`, `Demolitionist`, `PartyGirl`, `DyeTrader`, `Truffle`,
  `ArmsDealer`, `Nurse`, and `Princess`). `TownSpawnUnlockStateComponent` owns the corresponding
  boolean facts, accepts each unlock once, returns `false` for a repeated unlock, exposes read-only
  status through `IsUnlocked`, and clears all sixteen values through `Reset`.
- Dependency impact: only the `Terraria.Town` project source set is extended. No existing NPC spawn
  writer, `NpcSpawnEligibilityQuery`, WorldFile projection, network projection, registration key, or
  scheduler integration was changed, and no second owner was introduced.
- Evidence boundary: the source does not claim that failed spawn attempts, NPC definitions, legacy
  boolean compatibility views, save/load, network output, or world reset callers are wired. The
  component remains a world-scoped state owner; query and adapter integration require separate
  evidence.
- Verification status: affected-project build and isolated smoke are pending; the component is still
  `currentComponent: C07` and remains in `pendingComponents` until both are recorded.

## 19. Verification Checkpoint: C07 TownSpawnUnlockState

- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The isolated reflection smoke
  exited `0` and verified all sixteen typed unlock keys, initial locked state, first-write acceptance,
  duplicate rejection, readback, invalid-key rejection, and reset. Spawn-query behavior, legacy
  writer replacement, save/network projection, scheduler placement, and behavior equivalence remain
  unverified.

## 20. Implementation Checkpoint: C08 NpcProgressionBookUsageState

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/Books/NpcProgressionBookKind.cs`
  - `src/WorldSession/NpcProgression/Books/NpcProgressionBookUsageStateComponent.cs`
- Core behavior: `NpcProgressionBookKind` provides typed keys for `CombatBook`,
  `CombatBookVolumeTwo`, and `PeddlersSatchel`. `NpcProgressionBookUsageStateComponent` owns the
  corresponding three one-way usage facts, accepts each first `MarkUsed` transition, returns `false`
  for a repeated transition, exposes read-only status through `IsUsed`, and clears all three values
  through `Reset`.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. The
  old `NpcWorldUnlockFlags` aggregate, item-use path, item effects, save projection, network
  projection, registration key, and `ProgressionAggregate` remain unchanged; no second writer was
  added.
- Evidence boundary: item validation and command handling, committed usage events, old static read
  compatibility, versioned save/load, network output, world reset callers, and scheduler integration
  are not wired by this state component.
- Verification status: affected-project build and isolated smoke are pending; the component is still
  `currentComponent: C08` and remains in `pendingComponents` until both are recorded.

## 21. Verification Checkpoint: C08 NpcProgressionBookUsageState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The isolated
  reflection smoke exited `0` and verified all three typed book keys, initial unused state,
  first-use acceptance, duplicate rejection, readback, invalid-key rejection, and reset. Item
  command validation, legacy writer replacement, save/network projection, scheduler placement, and
  behavior equivalence remain unverified.

## 22. Implementation Checkpoint: C09 NpcActivePresenceCache

- Saved source file before verification:
  - `src/Npc/Queries/NpcActivePresenceCache.cs`
- Core behavior: `NpcActivePresenceCache` owns a scan-local `bool[]` keyed by the Version4 integer
  NPC type range. `BeginScan` requires a strictly increasing scan revision, clears the previous
  workspace, and marks the new revision valid. `TryMarkActive` accepts only current-revision and
  in-range type indices; `TryGetActive` provides a read-only point query; and
  `TryGetActiveNpcTypes` returns a new ascending snapshot. `Invalidate` clears the workspace and
  makes current results unavailable.
- Dependency impact: only the existing `Terraria.Npc` project source set is extended. No NPC
  lifecycle writer, spawn/progression writer, save projection, network projection, registration key,
  or scheduler integration was changed; the cache is intentionally not a durable world component.
- Evidence boundary: the active-population scan caller, NPC spawn/despawn invalidation hooks, exact
  cross-world ownership, save/network exclusion wiring, and runtime scheduler placement remain
  unverified. The cache API rejects stale revisions but does not itself discover active NPC entities.
- Verification status: affected-project build and isolated smoke are pending; the component is still
  `currentComponent: C09` and remains in `pendingComponents` until both are recorded.

## 23. Verification Checkpoint: C09 NpcActivePresenceCache

- Verification command: `$dotnetArgs = @('build', '.\\src\\Npc\\Terraria.Npc.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` exists. The isolated reflection smoke exited
  `0` and verified per-scan clearing, strictly increasing revisions, current-revision marking and
  reading, ascending type snapshot, invalidation, stale-revision rejection, and type-range bounds.
  The active scan caller, lifecycle invalidation hooks, save/network exclusion, scheduler placement,
  and behavior equivalence remain unverified.

## 24. Implementation Checkpoint: C10 LunarTowerEncounterState

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/LunarTower/LunarTowerKind.cs`
  - `src/WorldSession/NpcProgression/LunarTower/LunarTowerEncounterStateComponent.cs`
- Core behavior: `LunarTowerKind` provides typed keys for Solar, Vortex, Nebula, and Stardust.
  `LunarTowerEncounterStateComponent` owns the four runtime shield values, the immutable normal
  shield rule, four runtime active flags, and `LunarApocalypseIsUp`. Shield updates are bounded to
  `0..LunarShieldPowerNormal`; `ApplyShieldDamage` clamps at zero; active and apocalypse transitions
  are explicit; and `Reset` clears runtime encounter state. Persistent `downedTower*` flags are not
  represented or inferred here.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. No
  `WorldGen`, `Projectile`, NPC defeat writer, `LunarProgressState`, save projection, network
  projection, registration key, or scheduler integration was changed; no second runtime/defeat owner
  was added.
- Evidence boundary: Version4 `ShieldStrengthTowerMax` halves the normal value after
  `downedMoonlord`, but the C11 owner is not implemented, so this isolated component intentionally
  exposes only the confirmed normal shield bound. WorldGen activation/update, projectile damage,
  NPC defeat transitions, network/save projections, and event scheduling remain unwired.
- Verification evidence: the serial `Terraria.WorldSession` build exited `0` with `0` warnings and
  `0` errors; `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists.
  The isolated smoke exited `0` and verified all four typed tower keys, shield bounds and damage,
  active flags, apocalypse state, invalid input rejection, and reset. WorldGen activation/update,
  projectile damage, NPC defeat transitions, network/save projections, event scheduling, and
  behavior equivalence remain unverified. The checkpoint is complete for the isolated component.

## 25. Verification Checkpoint: C10 LunarTowerEncounterState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The isolated
  smoke exited `0` and verified all four tower keys, shield bounds and damage, active flags,
  apocalypse state, invalid input rejection, and reset. WorldGen/Projectile integration, the
  dynamic `downedMoonlord` policy, save/network projection, scheduler placement, and behavior
  equivalence remain unverified.

## 26. Implementation Checkpoint: C11 BossDefeatProgressionState

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/Boss/BossDefeatProgressionKind.cs`
  - `src/WorldSession/NpcProgression/Boss/BossDefeatProgressionStateComponent.cs`
- Core behavior: `BossDefeatProgressionKind` provides typed keys for the 17 authoritative boss
  defeat fields. `BossDefeatProgressionStateComponent` owns one explicit boolean for each field,
  accepts each first `MarkDefeated` transition, returns `false` for a repeated transition, exposes
  read-only status through `IsDefeated`, and clears all flags through `Reset`. The compatibility
  field `DownedMechBossAny` is stored by the same owner and is set in the same transition as each
  `DownedMechBoss1..3` flag; no independent derived writer was added.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. No
  NPC defeat event writer, active-boss registry, achievement/spawn reader, WorldFile projection,
  network projection, registration key, or scheduler integration was changed.
- Evidence boundary: authoritative defeat event routing, stale active-boss event rejection,
  versioned old-save default/load, save/network round trip, legacy static compatibility, and
  behavior equivalence remain unverified. This checkpoint intentionally implements the isolated
  state owner and its typed access boundary only.
- Verification status: build and isolated smoke are pending; C11 remains `currentComponent: C11`
  and in `pendingComponents` until both are recorded.

## 27. Verification Checkpoint: C11 BossDefeatProgressionState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The isolated
  reflection smoke exited `0` and verified all 17 typed boss keys, initial false state, first defeat
  acceptance, duplicate rejection, readback, atomic `DownedMechBossAny` updates with each concrete
  mechanical boss flag, invalid-key rejection, and reset. The existing `Terraria.Npc.Components.Verification`
  project does not reference `Terraria.WorldSession`, so it was not used for this component. Defeat
  event routing, stale active-boss rejection, old-save/load compatibility, save/network projection,
  legacy static compatibility, scheduler placement, and behavior equivalence remain unverified.

## 28. Implementation Checkpoint: C12 EventDefeatProgressionState

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/Events/EventDefeatProgressionKind.cs`
  - `src/WorldSession/NpcProgression/Events/EventDefeatProgressionStateComponent.cs`
- Core behavior: `EventDefeatProgressionKind` provides typed keys for all 14 event defeat fields.
  `EventDefeatProgressionStateComponent` owns one explicit boolean for each event fact, accepts each
  first `MarkDefeated` transition, returns `false` for a repeated transition, exposes read-only
  status through `IsDefeated`, and clears all flags through `Reset`. `DownedTowerSolar`,
  `DownedTowerVortex`, `DownedTowerNebula`, and `DownedTowerStardust` are persistent event facts
  here and are intentionally distinct from C10 runtime shield and active values.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. No
  invasion, seasonal, Martian, or tower runtime writer, old `InvasionHistoryStateComponent`,
  WorldFile projection, network projection, registration key, or scheduler integration was changed.
- Evidence boundary: authoritative event-end routing, old-save default/load, save/network round trip,
  legacy static compatibility, runtime-to-persistence adapter behavior, and behavior equivalence remain
  unverified. This checkpoint implements only the isolated event progression owner and typed access
  boundary.
- Verification status: build and isolated smoke are pending; C12 remains `currentComponent: C12`
  and in `pendingComponents` until both are recorded.

## 29. Verification Checkpoint: C12 EventDefeatProgressionState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The corrected
  strict-error isolated smoke exited `0` and verified all 14 event keys, initial false state, first
  event-end acceptance, duplicate rejection, readback, tower defeat persistence remaining separate
  from C10 shield/active runtime state, invalid-key rejection, and reset. Event-end routing,
  old-save/load compatibility, save/network round trip, legacy static compatibility, scheduler
placement, and behavior equivalence remain unverified.

## 39. Implementation Checkpoint: C03 BossEntityIndexRegistry

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/Boss/BossKind.cs`
  - `src/WorldSession/NpcProgression/Boss/BossEntityIndexKey.cs`
  - `src/WorldSession/NpcProgression/Boss/BossEntityIndexRegistryComponent.cs`
  - `src/WorldSession/Terraria.WorldSession.csproj`
- Core behavior: `BossEntityIndexKey` requires a valid `NpcInstanceId`, assigned legacy
  `NpcSlot`, positive slot generation, and valid `NpcTypeId`. The registry accepts one entry per
  `BossKind`, rejects duplicate registration, permits replacement only when the expected current
  generation-aware key matches, rejects wrong Version4 NPC types (Golem 245, Plant 262, Crimson
  266, Deerclops 668), rejects inactive or stale resolution, and clears only an exact current key.
  `GolemBoss`, `PlantBoss`, `CrimsonBoss`, and `DeerclopsBoss` are read-only legacy slot views and
  return `-1` when unregistered.
- Dependency impact: `Terraria.WorldSession` now references the existing `Terraria.Npc` project
  for the established identity, slot, and type value types. No Version4 runtime lifecycle caller,
  NPC spawn/despawn writer, save projection, network projection, registration key, or other
  partition document was changed.
- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false', '-p:CustomAfterMicrosoftCommonTargets=D:\\TRbackup\\NLTX\\Build\\Tools\\P13-WorldSession-Verification-Exclusions.targets'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`. The temporary exclusion removed only the unrelated `WorldGeneration/Systems/HellChestLootCycleSystem.cs` source and was deleted after verification.
- Verification status: the isolated affected-project build exited `0` with `0` warnings and `0`
  errors after excluding only the unrelated `HellChestLootCycleSystem.cs` source; the artifact is
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`. The existing NPC/Town
  focused verifier exited `0` with `PASS: NPC and town component field composition`. The C03
  boundary smoke exited `0` with `PASS: BossEntityIndexRegistry C03 boundary smoke` and covered
  registration, duplicate rejection, active resolution, wrong type, inactive resolution, slot
  reuse, stale clear, current clear, and legacy slot cleanup. The complete WorldSession build still
  cannot be claimed because the excluded external source remains unresolved.

## 30. Implementation Checkpoint: C13 TownResidentState

- Saved source file before verification:
  - `src/Town/Residents/TownResidentStateComponent.cs`
- Core behavior: `TownResidentStateComponent` is an entity-scoped resident classification owner.
  `IsTownResident` starts false, `SetResident` applies the explicit classification transition, and
  `Reset` clears the value for entity reuse. Housing assignment, rescue progression, friendly and
  capability values remain separate state boundaries.
- Dependency impact: only the existing `Terraria.Town` project source set is extended. The partial
  `TownResidentComponent`, `NpcHousingAssignmentComponent`, rescue/spawn progression owners,
  dialogue types, WorldFile projection, network projection, registration key, and scheduler were
  not changed; no second resident writer was introduced.
- Evidence boundary: the current `nextDialogue` declaration is `Terraria.GameContent.ConditionalDialogue`,
  but its readers, writers, reset behavior, persistence, and network contract are not evidenced.
  It remains a deferred `TownDialogueCompatibilityProjection`; no authoritative dialogue component
  was guessed. Legacy `townNPC` adapters, resident query/command integration, entity-reuse hooks,
  save/network round trip, and behavior equivalence remain unverified.
- Verification status: build and isolated smoke are pending; C13 remains `currentComponent: C13`
  and in `pendingComponents` until both are recorded.

## 31. Verification Checkpoint: C13 TownResidentState

- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, followed by the existing verifier through the same wrapper with `run --project .\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Verification result: `Terraria.Town` build exited `0` with `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The existing NPC/Town verifier
  executed and exited `0` with `PASS: NPC and town component field composition`. The C13 isolated
  smoke exited `0` and verified resident default false, explicit true/false transitions, and reset
  for entity reuse. `nextDialogue`, legacy `townNPC` adapter integration, housing/spawn interaction,
  save/network projection, scheduler placement, and behavior equivalence remain unverified.

## 32. Implementation Checkpoint: C14 TravelNpcWorldState

- Saved source file before verification:
  - `src/WorldSession/Town/TravelNpcWorldStateComponent.cs`
- Core behavior: `TravelNpcWorldStateComponent` is a world-level runtime marker owner for the
  traveling NPC role. `IsTravelNpcActive` starts false, `SetTravelNpcActive` applies an explicit
  lifecycle transition, and `Reset` clears the role. The marker is not copied into every NPC entity.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. No
  travel-NPC selector, spawned-entity identity relation, despawn/day-event writer, notification
  projection, WorldFile projection, network projection, registration key, or scheduler integration
  was changed.
- Evidence boundary: Version4 call-path evidence for selector ownership, entity spawn/despawn,
  stale-role cleanup, save/network persistence, reconnect behavior, and behavior equivalence remains
  open. This checkpoint implements only the isolated world role owner and explicit reset boundary.
- Verification status: build and isolated smoke are pending; C14 remains `currentComponent: C14`
  and in `pendingComponents` until both are recorded.

## 33. Verification Checkpoint: C14 TravelNpcWorldState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The strict
  isolated smoke exited `0` and verified travel role default inactive, explicit activation and
  deactivation, and world reset cleanup. Selector ownership, spawned-entity relation, stale-role
  cleanup, save/network persistence, reconnect behavior, scheduler placement, and behavior
  equivalence remain unverified.

## 34. Implementation Checkpoint: C15 TownHousingRelationState

- Saved source files before verification:
  - `src/Town/Housing/TownHousingRuleDefinition.cs`
  - `src/Town/Housing/TownHousingRelationStateComponent.cs`
- Core behavior: `TownHousingRuleDefinition` preserves the confirmed immutable
  `KickOutLookForHomeTimeout` value of `3600`. `TownHousingRelationStateComponent` owns the typed
  NPC housing relation: homeless state, homeless-despawn policy, non-negative home-search timeout,
  optional `TownRoomTilePoint` assignment, housing category, and transition compatibility snapshot.
  `CommitRelation` validates the homeless/home invariant, captures the previous relation atomically
  within the same owner, and applies the new relation. `Reset` clears relation and snapshot state.
- Dependency impact: only the existing `Terraria.Town` project source set is extended. Existing
  `TownHousingRelationComponent`, `NpcHousingAssignmentComponent`, WorldGeneration housing registry
  and key types, room scan code, save/network projection, registration key, and scheduler were not
  changed; no cross-domain writer or second relation owner was added.
- Evidence boundary: `TownHousingResidentKey` type/instance/dual mode, assignment revision semantics,
  Town versus WorldGeneration commit ownership, room query adapter, legacy coordinate sentinel shape,
  housingCategory integration with resident capabilities, save/load, network notifications, slot
  reuse, and behavior equivalence remain unresolved. This checkpoint intentionally implements the
  relation boundary and confirmed rule value without selecting the cross-domain owner.
- Verification status: build and isolated smoke are pending; C15 remains `currentComponent: C15`
  and in `pendingComponents` until both are recorded.

## 35. Verification Checkpoint: C15 TownHousingRelationState

- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, followed by the existing verifier through the same wrapper with `run --project .\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Verification result: `Terraria.Town` build exited `0` with `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The existing NPC/Town verifier
  executed and exited `0` with `PASS: NPC and town component field composition`. The C15 isolated
  smoke exited `0` and verified rule value `3600`, assigned and homeless relation transitions,
  compatibility snapshot capture, negative-timeout and homeless-with-home rejection, and reset.
  Cross-domain key mode, assignment revision, Town/WorldGeneration commit ownership, room adapter,
  save/load, network notification, slot reuse, scheduler placement, and behavior equivalence remain
  unverified.

## 36. Implementation Checkpoint: C16 TownDoorInteractionIntent

- Saved source files before verification:
  - `src/Town/Doors/TownDoorInteractionIntent.cs`
  - `src/Town/Doors/TownDoorInteractionIntentComponent.cs`
- Core behavior: `TownDoorInteractionIntent` is a typed value containing the requested close/open
  action, non-negative tile coordinates, a monotonic sequence, and a non-negative expiry tick.
  `TownDoorInteractionIntentComponent` stores at most one pending intent, exposes read-only legacy
  field views, rejects invalid coordinates/ticks, consumes a pending intent once through
  `TryConsume`, and clears it through expiry, rejection, or `Reset`. Tile mutation, audio, network,
  and housing effects are not performed by the component.
- Dependency impact: only the existing `Terraria.Town` project source set is extended. No tile/door
  adapter, movement schedule, housing relation, NPC lifecycle writer, network projection, registration
  key, or scheduler integration was changed; no external side-effect writer was added.
- Evidence boundary: Version4 tile validity, moved-door detection, terminal result events, exact expiry
  scheduling, slot-reuse caller, adapter side effects, save/network behavior, and behavior equivalence
  remain unverified. This checkpoint implements only the explicit short-lived intent owner.
- Verification status: build and isolated smoke are pending; C16 remains `currentComponent: C16`
  and in `pendingComponents` until both are recorded.

## 37. Verification Checkpoint: C16 TownDoorInteractionIntent

- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, followed by the existing verifier through the same wrapper with `run --project .\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Verification result: `Terraria.Town` build exited `0` with `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The existing NPC/Town verifier
  executed and exited `0` with `PASS: NPC and town component field composition`. The corrected strict
  isolated smoke exited `0` and verified coordinates, monotonic sequence, field readback,
  consume-once behavior, expiry, rejection, invalid input, and reset. Tile validity, moved-door
  handling, terminal result events, slot-reuse caller, adapter-only effects, save/network behavior,
  scheduler placement, and behavior equivalence remain unverified.

## 38. Implementation Checkpoint: C17 NpcBreathState

- Saved source files before verification:
  - `src/Npc/Environment/NpcBreathRuleDefinition.cs`
  - `src/Npc/Environment/NpcBreathStateComponent.cs`
- Core behavior: `NpcBreathRuleDefinition` preserves the confirmed immutable `BreathMax` value of
  `200`, drowning cadence of `7`, and recovery step of `3`. `NpcBreathStateComponent` defaults to
  `Breath=200` and `BreathCounter=0`, validates the bounded state, advances the counter only while
  submerged, decrements breath at the confirmed cadence, recovers and resets the counter when not
  submerged, clamps breath to `0..200`, reports when breath reaches zero, and resets for entity
  initialization/death/despawn/slot reuse. Health damage, environment queries, logging, and network
  effects remain outside the component.
- Dependency impact: only the existing `Terraria.Npc` project source set is extended. No NPC health,
  lifecycle, liquid/environment query, network projection, save projection, registration key, or
  scheduler integration was changed; no side-effect writer was added.
- Evidence boundary: exact Version4 environment caller, life-damage/defeat handling after breath reaches
  zero, liquid exception rules, save/network compatibility, scheduler placement, and behavior
  equivalence remain unverified. This checkpoint implements only the confirmed breath state/rule
  boundary.
- Verification status: the affected-project build, existing focused verifier, and isolated C17 smoke
  passed. The build used the serial wrapper and exited `0` with `0` warnings and `0` errors; the
  artifact is `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`. The focused verifier exited
  `0` with `PASS: NPC and town component field composition`. The strict reflection smoke exited
  `0` with `PASS: NpcBreathStateComponent C17 boundary smoke`, covering definition constants,
  default state, cadence, recovery, bounds, zero signal, invalid inputs, and reset. C17 is now in
  `completedComponents`; at this earlier checkpoint, C03 and C04 remained pending. Environment caller integration, life-damage
  handling after breath reaches zero, liquid exception rules, save/network compatibility, scheduler
  placement, and behavior equivalence remain unverified.

## 40. Implementation Checkpoint: C04 NpcNetworkSyncIntent

- Saved source files before verification:
  - `src/Npc/Network/NpcNetworkSyncIntentComponent.cs`
  - `src/Npc/NpcReplicationDirtyState.cs`
- Core behavior: `NpcNetworkSyncIntentComponent` owns the pending synchronization intent and a
  monotonic revision. Repeated `Mark` calls coalesce while a send is pending; `Acknowledge` clears
  only the exact current revision; `Retry` leaves a failed send observable; and
  `ResetForEntityReuse` clears pending and acknowledgement state without reusing the revision
  sequence. `NpcReplicationDirtyState` now derives `StateDirty` from that component and exposes
  `MarkStateChanged`, `AcknowledgeStateChanged`, `RetryStateChanged`, and
  `ResetForEntityReuse`. Its existing `Flags` setter remains a compatibility adapter over the same
  owner, so no second dirty bit is introduced.
- Dependency impact: only the existing `Terraria.Npc` project source set is extended. No packet
  DTO, transport handle, client cursor, save writer, network scheduler, registration key, or
  authority boundary was added; existing `SpawnNeedsSync`, `ForceFullSync`, `RemovalNeedsSync`,
  client state, and stream cursor fields remain outside the new intent owner.
- Evidence boundary: the local component, dirty-state composition, affected-project build, focused
  verifier, and boundary smoke passed. The Version4 scheduler caller, successful-send
  acknowledgement path, failed-send retry path, entity lifecycle reset caller, client/server
  authority, duplicate packet suppression, and behavior equivalence remain unverified.

## 41. Verification Checkpoint: C04 NpcNetworkSyncIntent

- Verification command: `$dotnetArgs = @('build', '.\\src\\Npc\\Terraria.Npc.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, followed by the existing verifier through the same wrapper with `run --project .\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Verification result: `Terraria.Npc` build exited `0` with `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` exists. The existing focused verifier
  exited `0` with `PASS: NPC and town component field composition`. The strict C04 boundary smoke
  exited `0` with `PASS: NpcNetworkSyncIntent C04 boundary smoke`, covering coalesced marks,
  revision matching, stale and duplicate acknowledgement rejection, retry visibility, entity reuse
  reset, compatibility `Flags` projection, and single-owner dirty-state cleanup. Runtime scheduler,
  transport, persistence/network projection, and behavior equivalence remain unverified.

## 42. Current verification boundary

- The final serial build of `src/WorldSession/Terraria.WorldSession.csproj` exited `1` with `0`
  warnings and `1` error at
  `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs:16`: the call to
  `TryAdvanceAfterSuccessfulPlacement` has no one-argument overload. This source belongs to another
  session and was not changed. The C03 isolated build remains the applicable evidence for the
  registry boundary; it used a temporary exclusion for that unrelated source and produced the
  artifact recorded in Section 39.
- C04 remains verified only at the `Terraria.Npc` component boundary. Its scheduler, lifecycle,
  transport, persistence/network projection, and behavior-equivalence callers remain evidence-gaps.

## 43. Implementation Boundary: C09 Active-Presence Scan Owner

- Saved source files:
  - `src/Npc/Queries/NpcActivePresenceScanEntry.cs`
  - `src/Npc/Queries/NpcActivePresenceScanSystem.cs`
- Core behavior: `NpcActivePresenceScanEntry` is an explicit caller-provided `(NpcType, IsActive)`
  input. `NpcActivePresenceScanSystem.Rebuild` starts a new strictly increasing scan revision on
  the existing `NpcActivePresenceCache`, submits only active entries, and leaves invalid NPC types
  rejected by the cache boundary. The system does not read `Main`, a clock, randomness, network, or
  persistence and does not create entities or mutate other state.
- Dependency impact: only the existing `Terraria.Npc` source set is extended. The scan owner keeps
  the cache's scan-local, non-durable ownership explicit and introduces no registration key,
  scheduler, lifecycle hook, save projection, or network projection.
- Evidence boundary: Version4 `NPC.cs:7119-7142` supports clearing and rebuilding active presence
  from active NPCs and type range. The actual `Main.npc` caller, scheduler placement, spawn/despawn
  invalidation hooks, cross-world ownership, and behavior equivalence remain unverified.
- Component state: C09 remains in `completedComponents`; `currentComponent` is `none` and
  `pendingComponents` is empty. Focused verification is still pending and therefore the document
  remains `verificationStatus: partial`.

## 44. Verification Boundary: C09 Active-Presence Scan Owner

- Verification commands, each executed serially through
  `Build/Tools/Invoke-SerialDotnet.ps1`:
  - `restore .\src\NpcC09FocusedVerifier\Terraria.NpcC09FocusedVerifier.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0`.
  - `build .\src\NpcC09FocusedVerifier\Terraria.NpcC09FocusedVerifier.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0` with `0` warnings and `0` errors.
  - `run --project .\src\NpcC09FocusedVerifier\Terraria.NpcC09FocusedVerifier.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0` and printed `PASS: NpcActivePresenceScanSystem C09 focused verifier`.
- Artifact: `Build/bin/Terraria.NpcC09FocusedVerifier/Debug/net10.0/Terraria.NpcC09FocusedVerifier.dll`.
- Coverage: active entries at revisions 10 and 11, inactive filtering, duplicate type
  deduplication, ascending snapshots, invalid negative/capacity types, replacement clearing,
  stale revision rejection, invalidation, and non-increasing revision rejection.
- The initial no-restore build attempt exited `1` with `NETSDK1004` because the new verifier had no
  assets file; the serialized restore above resolved that bootstrap condition. This does not alter
  the C09 implementation result.
- Verification boundary: the isolated scan owner is verified. The real `Main.npc` caller, runtime
  scheduler placement, lifecycle invalidation hooks, cross-world ownership, and behavior
  equivalence remain evidence-gaps, so overall `verificationStatus: partial` is retained.

## 45. Affected Project Verification: C09

- Command, executed serially through `Build/Tools/Invoke-SerialDotnet.ps1`:
  `build .\src\Npc\Terraria.Npc.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Result: exit code `0`, `0` warnings, `0` errors. Artifact:
  `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`.
- This confirms the C09 cache and scan-owner source files compile as part of the affected NPC
  project. It does not change the documented runtime integration evidence boundary.

## 46. Existing NPC/Town Regression Verification

- Command, executed serially through `Build/Tools/Invoke-SerialDotnet.ps1`:
  `run --project .\Test\Terraria.Npc.Components.Verification\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Result: exit code `0`; output `PASS: NPC and town component field composition`.
- Scope: existing identity, definition, behavior, lifetime, target, parent relation, replication,
  resident capability, and housing assignment composition. This is regression evidence only and
  does not close the P13 runtime caller, persistence, network, or behavior-equivalence gaps.

## 47. Integrated P13 focused verifier source checkpoint

- Saved source files:
  - `src/NpcTownProgressionP13FocusedVerifier/Program.cs`
  - `src/NpcTownProgressionP13FocusedVerifier/Terraria.NpcTownProgressionP13FocusedVerifier.csproj`
- Scope: the verifier exercises the saved C01-C17 component boundaries across the existing NPC,
  Town, and WorldSession source owners, including field readback, reset behavior, monotonic or
  duplicate-operation guards, housing transitions, door-intent consumption, active-presence scan,
  shield damage, and breath cadence. It covers the 105 members with confirmed source owners;
  `nextDialogue` remains a deferred projection because its reader, writer, reset, persistence, and
  network evidence is still missing.
- Dependency impact: this is a source-only focused verifier under `src/`; it introduces no runtime
  registration, scheduler, persistence, network, or production behavior change. Its project file
  explicitly compiles the WorldSession owner files so verification can remain bounded while the
  unrelated `HellChestLootCycleSystem.cs` error is unresolved.
- Verification status: source is saved, but restore/build/run have not yet been executed for this
  verifier. `verificationStatus: partial` is therefore retained. No integrated verifier result or
  full P13 behavior-equivalence claim is made by this checkpoint.

## 48. Integrated P13 focused verifier verification

- Commands, each executed serially through `Build/Tools/Invoke-SerialDotnet.ps1`:
  - `restore .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0`.
  - `build .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0` with `0` warnings and `0` errors.
  - `run --project .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0` and printed `PASS: P13 NPC/Town progression focused verifier (105 implemented members; nextDialogue deferred)`.
- The first no-restore build before verifier API correction exited `1` with `0` warnings and two
  verifier-only errors (`CS8117` for matching a `void` return and `CS0246` for the missing
  `Terraria.Town` using). The verifier was corrected from the actual component declarations and the
  serial build above then passed. No production component source was changed for this correction.
- Artifact: `Build/bin/Terraria.NpcTownProgressionP13FocusedVerifier/Debug/net10.0/Terraria.NpcTownProgressionP13FocusedVerifier.dll`.
- Coverage: C01-C17 boundary assertions for the 105 members with confirmed owners, including field
  readback, reset, duplicate-operation rejection, monotonic revisions, housing transitions,
  door-intent consume/expiry, active-presence scan replacement, tower shield state, and breath
  cadence. `nextDialogue` is explicitly reported as deferred.
- Verification status: the integrated focused verifier is verified at its bounded source-owner
  boundary. Overall `verificationStatus: partial` remains correct because Version4 runtime callers,
  scheduler placement, lifecycle wiring, persistence/network adapters, and behavior equivalence are
  still outside the available evidence.

## 49. Final P13 session verification record

- Session metadata is now bound to the active manual claim: `partitionId=P13` and
  `sessionId=06c2332691e84e2682405500e43d7be8`. No active `dotnet.exe` or `csc.exe` process was
  present before the compile-capable commands.
- The affected-project commands were executed serially through
  `Build/Tools/Invoke-SerialDotnet.ps1` with `-m:1 -nr:false -p:UseSharedCompilation=false
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false`:
  - `build .\src\Npc\Terraria.Npc.csproj --no-restore` exited `0` with `0` warnings and `0`
    errors. Artifact: `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`.
  - `build .\src\Town\Terraria.Town.csproj --no-restore` exited `0` with `0` warnings and `0`
    errors. Artifact: `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll`.
  - `build .\src\WorldSession\Terraria.WorldSession.csproj --no-restore` exited `1` with `0`
    warnings and `1` error at `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs:16`:
    `TryAdvanceAfterSuccessfulPlacement` has no one-argument overload. This external source was not
    modified, and no complete WorldSession artifact is claimed from the failed build.
- The integrated verifier command
  `build .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj
  --no-restore` exited `0` with `0` warnings and `0` errors. Artifact:
  `Build/bin/Terraria.NpcTownProgressionP13FocusedVerifier/Debug/net10.0/Terraria.NpcTownProgressionP13FocusedVerifier.dll`.
- The verifier command
  `run --project .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj
  --no-build --no-restore` exited `0` and printed
  `PASS: P13 NPC/Town progression focused verifier (105 implemented members; nextDialogue deferred)`.
- The 105 confirmed source-owner members are therefore covered by the bounded verifier. The
  `nextDialogue` member remains deferred because its complete reader, writer, reset, persistence,
  and network evidence is unavailable; runtime scheduling, lifecycle, persistence, network, and
  behavior-equivalence evidence remain open. `implementationStatus: in-progress` and
  `verificationStatus: partial` remain intentional.
