# Version4 P05 玩家进度、召唤物、宠物与伙伴组件拆分设计

```yaml
partitionId: P05
sessionId: 7657bc14436f43bc901049a873107b34
inputReport: D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P05-Player-Progression-Pets-Minions.md
componentDesignPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P05-player-progression-pets-minions-component-design.md
componentExecutionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P05-player-progression-pets-minions-component-execution.md
designStatus: proposed
executionStatus: failed
implementationStatus: partial
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: partial
completedComponents: [PlayerConsumedProgressionLedgerComponent, PlayerFishingCapabilitySnapshotComponent, PlayerUnlockProgressionLedgerComponent, PlayerQuestEventProgressComponent]
implementedComponents: [PlayerConsumedProgressionLedgerComponent, PlayerFishingCapabilitySnapshotComponent, PlayerMinionCapacityComponent, PlayerCoreMinionCapabilityComponent, PlayerCrossoverMinionCapabilityComponent, PlayerMinionDamageHighWaterMarkComponent, PlayerLegacyPetCapabilityComponent, PlayerBossPetCapabilityComponent, PlayerSeasonalEventPetCapabilityComponent, PlayerStandardNamedPetCapabilityComponent, PlayerCrossoverPetCapabilityComponent, PlayerWorldObjectPetCapabilityComponent, PlayerCompanionCapabilityComponent, PlayerMountVehicleIntegrationComponent, PlayerAccessoryEffectSnapshotComponent, PlayerUnlockProgressionLedgerComponent, PlayerQuestEventProgressComponent]
blockedComponents: [PlayerMountVehicleIntegrationComponent.mount]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T10:32:45.5480656Z
evidence-gap: C03-C06, C09-C15 and C17 source components are present but have no post-checkpoint affected-project verification; C03 capacity ownership, C04/C05/C10-C14 content registration, C06 Projectile observation provenance, C09 luck/environment ownership, C11 event gating, C13 Chillet/ChilletIgnis mapping, C14 Tile/world-object ownership, C15 companion entity-command ownership, C17 luck/effect ownership, reset/rebuild systems, catalog fail-closed behavior, pet entity orchestration and aggregation ordering remain unverified. C16 now has a state-only source core for six Player-side vehicle fields, but its raw `Terraria.Mount mount` field remains blocked because Mount/Minecart ownership and the typed replayable vehicle boundary are unresolved. Version4 persistence methods remain stubs, no P05 network/persistence adapter is implemented, and the C01-C06 plus C09-C15 and C17 legacy Player fields are not yet migrated behind compatibility adapters. C02 still requires complete fishing-attempt rod/bait/water/environment readers, source-backed content mapping and bobber Projectile integration. C07 still requires external builder preference mapping for `UsingBiomeTorches`, expected-revision handling and the complete-reference inventory fallback for old Super Cart saves. C08 still requires caller authorization, reward/event ordering, release-gated load/save and network projection; its transient command-token cache is not durable progress.
blocking-decision: C01, C02, C07 and C08 use local explicit writers (`PlayerProgressionCommitSystem` for C01/C07/C08 and `PlayerFishingCapabilityRebuildSystem` for C02), but the C01/C02/C07/C08 transaction or lifecycle roots, expected-revision policy, persistence adapters, network projection and legacy-field handoff remain integration decisions. C02 must select the source-backed fishing content adapter and complete fishing-attempt reader contract without copying rod, bait, water, environment or bobber Projectile state into the Player snapshot. C07 must also preserve the external builder preference and old-inventory fallback without a second writer. C08 must select the authorized Angler/Golfer/DD2 event ports and preserve reward ordering; the core cannot accept arbitrary network or NPC writes. C03/C06 require a single Player-versus-Projectile aggregation writer; C04/C05/C10/C11/C13 require content-key registries; C14 requires a Tile/world-object and pet-entity handoff; C15 requires an explicit companion entity command owner; C16 requires a single Mount/Minecart writer and typed snapshot boundary; C17 requires a luck/effect owner for `brokenMirrorBadLuck` and derived luck effects.
```

## 1. Scope and status

This document covers only the 17 P05 leaf groups and their 164 fields under the formal parent
`PlayerGameplay`. It is a proposed ECS decomposition and a partial component implementation record.
It is not a complete runtime implementation, migration-completion report, behavior-equivalence proof,
API-compatibility proof, network-closure proof, or persistence-closure proof.

Types, paths, systems, queries, commands, adapters, projections, interfaces, and value objects not
covered by an implementation checkpoint below remain `status: proposed`. C01, C02, C07 and C08 isolated
core files, together with the state-only component cores for C03-C06, C09-C15 and C17, were added under
`src/Player/Progression/`; their non-component integration remains proposed or blocked. Existing NLTX
files outside those checkpoints are current-state evidence only. No project files, source reports, or the
session ledger were modified.

The partition boundary is capability-first and lifecycle-first. The 17 source groups are retained
as auditable ownership units, but transient capability flags are not treated as permanent unlock
inventory merely because their legacy fields are public. `ResetEffects`, buff processing, equipment
processing, projectile scans, persistence, and network synchronization have different writers and
lifetimes and therefore remain explicit seams.

## 2. Executive decomposition decision

The proposed target root is:

```text
dome/src/Terraria.Dome.Simulation/Player/Progression/
  Components/
  Queries/
  Systems/
  Commands/
  Adapters/
  Projections/
```

The minion capability and pet capability components remain under Player because the source fields
are player-owned capability snapshots. Projectile entities, projectile slot identity, combat damage,
and pet/minion entity lifetime remain owned by their respective domains. No generic
`Shared/Components/`, `Common/`, or `Misc/` directory is proposed.

The central decision is to separate four state kinds:

1. Persistent player progress: consumed upgrades, explicit unlocks, quest/event counters and the
   player-scoped DD2 progress bit.
2. Runtime capability snapshots: fishing, minion flags, pet flags, companion flags and equipment
   effects rebuilt from buffs/equipment/content each reset cycle.
3. Runtime aggregation/cache: minion count, slot usage and the two minion damage high-water marks.
4. Cross-domain integration state: mount/minecart fields, which cannot be declared PlayerGameplay
   exclusive while `Mount.cs` and `Minecart.cs` read and write the same values.

This prevents a `PetFlags` giant component, prevents projectile entities from being copied into the
Player component, and prevents derived properties such as `UsingBiomeTorches` or
`UsingSuperCart` from becoming independent authoritative fields.

## 3. Evidence register

| Source | Actual evidence read | Fact supported | evidenceStatus |
|---|---|---|---|
| Authoritative partition report | `D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P05-Player-Progression-Pets-Minions.md` | 17 leaf groups, 164 fields, 0 properties, source declaration rows 468-1040 | confirmed |
| Version4 `Player.cs` | `D:\\TRbackup\\Version4\\Terraria\\Player.cs:525-535,818-884,1344-1349,1475-1710` | P05 field declarations, defaults and legacy public names | confirmed |
| Version4 `Player.cs` | `D:\\TRbackup\\Version4\\Terraria\\Player.cs:4300-5539` | buff processing writes minion/pet/companion capability flags and spawns/refreshes pet projectiles | confirmed |
| Version4 `Player.cs` | `D:\\TRbackup\\Version4\\Terraria\\Player.cs:6269-6310` | projectile cache scan updates minion counts and the two damage high-water marks, then clears high-water marks | confirmed |
| Version4 `Player.cs` | `D:\\TRbackup\\Version4\\Terraria\\Player.cs:6865-7047,8138-8330` | equipment/armor/functional equipment writers update fishing, minion and accessory effect fields | confirmed |
| Version4 `Player.cs` | `D:\\TRbackup\\Version4\\Terraria\\Player.cs:10024-10025,10272-10730` | reset lifecycle clears runtime minion, fishing, pet, companion and accessory effect fields | confirmed |
| Version4 `Projectile.cs` | `D:\\TRbackup\\Version4\\Terraria\\Projectile.cs:14743-14767` | minion slot admission, count increment and slot usage are coupled to projectile spawn/update | confirmed |
| Version4 `Projectile.cs` | `D:\\TRbackup\\Version4\\Terraria\\Projectile.cs:31185-31205,38222-` | pet projectile maintenance and owner/death flag interactions exist outside Player alone | partial |
| Version4 `MessageBuffer.cs`/`NetMessage.cs` | `D:\\TRbackup\\Version4\\Terraria\\MessageBuffer.cs:277-287,721,2483-2484`; `D:\\TRbackup\\Version4\\Terraria\\NetMessage.cs:181-192,467,1149-1150` | selected progress flags and counters have explicit network reads/writes; pet and runtime flags are not closed by this scan | partial |
| Complete same-type reference | `D:\\TRbackup\\无任何删减通过编译\\Terraria\\Player.cs:55348-55515,55859-56329` | serialization/deserialization for progress fields, version gates and super-cart compatibility | full-reference-supplemented |
| Version4 persistence boundary | `D:\\TRbackup\\Version4\\Terraria\\Player.cs:26394-26418,26455-26458` | `SavePlayer` delegates to empty `InternalSavePlayerFile`/`Serialize`; `Deserialize` is an empty stub | confirmed |
| tModLoader stable mirror | `D:\\TRbackup\\tmodloader-api-docs-stable\\index.html`, `class_mod_player-members.html:222-231`, `class_player-members.html:1240,1275,1548,1551` | public extension boundary includes `ResetEffects`, `SaveData`, `SyncPlayer`, `SendClientChanges`, `CopyClientState`, `UpdateBuffs`, and `UpdateEquips`; page is `tModLoader v2026.07` | confirmed-public-boundary |
| SS14 reference | `C:\\Users\\shan\\Downloads\\ECS\\space-station-14-master\\Content.Shared\\Research\\Components\\TechnologyDatabaseComponent.cs:9-42`; `SharedResearchSystem.cs:21-46,214-231,246-304` | narrow component data, explicit system writers, `Dirty` and local events for change propagation | organization-only |
| SS14 reference | `C:\\Users\\shan\\Downloads\\ECS\\space-station-14-master\\Content.Shared\\Humanoid\\HumanoidProfileComponent.cs:14-36`; `HumanoidProfileSystem.cs:18-39` | component access control, network/data fields and system-owned mutation/event seam | organization-only |
| SS14 reference | `C:\\Users\\shan\\Downloads\\ECS\\space-station-14-master\\Content.Server\\Objectives\\Components\\HelpProgressConditionComponent.cs:9-11`; `HelpProgressConditionSystem.cs:20-57` | marker component plus event-driven pure progress calculation in a system | organization-only |
| Current NLTX | `D:\\TRbackup\\NLTX\\src\\Player\\PlayerFishingCapabilityState.cs:3-27`, `PlayerFishingCapabilityComponent.cs:3-21`, `PlayerSummonCapacityState.cs:3-21` | partial player capability prototypes already exist; they are not evidence of complete P05 implementation | existing-evidence |
| Current NLTX | `D:\\TRbackup\\NLTX\\src\\Player\\PlayerAbilityComponent.cs:3-15`, `PlayerMountState.cs:3-41`, `PlayerMountComponent.cs:3-25` | overlapping ability/mount state exists and requires integration review before P05 ownership is finalized | existing-evidence |
| Current NLTX | `D:\\TRbackup\\NLTX\\src\\Projectile\\ProjectileMinionCapabilityComponent.cs:3-14`; `dome\\src\\Terraria.Dome.Simulation\\Components\\Projectile\\ProjectileMinionComponent.cs:3-7` | projectile-side minion capability prototypes exist and must not be duplicated in Player | existing-evidence |
| Current NLTX | `D:\\TRbackup\\NLTX\\dome\\src\\Terraria.Dome.Server\\Persistence\\PlayerPersistentStateMapper.cs`; `PlayerStateProjection.cs:10-50` | player persistence/projection infrastructure exists, but P05 fields are not shown as closed by these files | existing-evidence |

SS14 evidence is used only for organization patterns. It does not establish Terraria field
semantics. The tModLoader mirror is a public extension cross-check and cannot fill missing private
Version4 readers or writers.

## 4. Version4 lifecycle and ownership facts

### 4.1 Persistent progress versus runtime effects

The complete reference writes `unlockedBiomeTorches`, `UsingBiomeTorches`, `ateArtisanBread`, the
six `used*` flags and `downedDD2EventAnyDifficulty` at `Player.cs:55377-55386`; it writes
`anglerQuestsFinished` at `:55491`, `golferScoreAccumulated` at `:55508`, and packs
`unlockedSuperCart`/`enabledSuperCart` at `:55513-55515`. Reads occur at `:55859-55877`,
`:56237`, `:56305`, and `:56323-56329` with release/version compatibility branches. The Version4
file cannot itself provide this closure because its serialize/deserialize methods are stubs.

`UsingBiomeTorches` and `UsingSuperCart` are derived property behavior over unlock/enabled fields
(`Player.cs:3046-3070` in Version4; complete reference property at `:3848-3876`). They must be
represented as pure queries or projections, not second authorities.

### 4.2 Reset and rebuild cycle

`UpdateBuffs` writes pet, companion and minion flags while it processes buffs and existing
projectile ownership (`Player.cs:5322-5539`). `UpdateEquips`, `GrantArmorBenefits` and
`ApplyEquipFunctional` write equipment-derived flags and capacity (`:6865-7047,
:8138-8330`). `ResetEffects` clears the same runtime snapshots (`:10272-10730`), and
`numMinions`/`slotsMinions` are explicitly reset at `:10024-10025`. Therefore the proposed
components are rebuilt snapshots with a single reset/rebuild writer, not permanent player
progress ledgers.

### 4.3 Projectile and vehicle integration

`Projectile.cs:14743-14767` admits a minion only when the owner capacity allows it and increments
the owner count/slot usage. `Player.UpdateProjectileCaches` also scans owned projectiles and updates
damage high-water marks (`Player.cs:6269-6310`). These operations require an integration-reviewed
command/query seam so Player and Projectile do not both own the same counter.

`Mount.cs` initializes a shared mount catalog and delegate data (`Mount.cs:293,616-650` and the
catalog writes beginning at `:649`); `Minecart.TrackCollision` reads and writes `lastBoost` and
track state (`Minecart.cs:566-906`). The six scalar Player-side fields can be stored without
copying the Mount object, while the raw `mount` field remains `crossSubsystemOwner:
integration-review`.

## 5. Current NLTX state

The current tree contains partial, overlapping prototypes:

- `src/Player/PlayerFishingCapabilityComponent.cs` contains most fishing booleans and a derived
  effective level, while `PlayerFishingCapabilityState.cs` also contains pole power, bait power and
  level multiplier. This is useful evidence but is not a completed P05 owner.
- `src/Player/PlayerSummonCapacityState.cs` and `src/Player/PlayerAbilityComponent.cs` both contain
  minion capacity/count fields. The future owner must be selected after checking all Player and
  Projectile consumers; this plan does not silently bless either as authoritative.
- `src/Projectile/ProjectileMinionCapabilityComponent.cs` and
  `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileMinionComponent.cs` already
  represent per-projectile slot metadata. P05 must only aggregate player capacity and must not copy
  `MinionSlots` or `MinionPosition` into a player component.
- `src/Player/PlayerMountState.cs` and `PlayerMountComponent.cs` already model mount runtime state.
  P05's mount fields are therefore an integration handoff, not permission to add a second mount
  owner.
- `dome/src/Terraria.Dome.Server/Replication/PlayerStateProjection.cs` projects active/life,
  controls and mount type, but does not close P05 progress/pet/minion serialization.

No current file proves complete P05 behavior. No current file is modified by this report.

## 6. Complete 164-member ownership table

The table below is the complete P05 member set copied from the authoritative report and assigned to
one proposed component or an explicit integration-review seam. `V4 line` is the declaration line
in `D:\\TRbackup\\Version4\\Terraria\\Player.cs`; source row IDs are retained to make the
mapping auditable.

| Source row | Leaf group | Member | C# type | V4 line | State kind | Proposed owner |
|---:|---|---|---|---:|---|---|
|468|PlayerConsumedProgressionFlags|usedAegisCrystal|bool|525|persistent consumed progress|C01 PlayerConsumedProgressionLedgerComponent|
|469|PlayerConsumedProgressionFlags|usedAegisFruit|bool|527|persistent consumed progress|C01 PlayerConsumedProgressionLedgerComponent|
|470|PlayerConsumedProgressionFlags|usedArcaneCrystal|bool|529|persistent consumed progress|C01 PlayerConsumedProgressionLedgerComponent|
|471|PlayerConsumedProgressionFlags|usedGalaxyPearl|bool|531|persistent consumed progress|C01 PlayerConsumedProgressionLedgerComponent|
|472|PlayerConsumedProgressionFlags|usedGummyWorm|bool|533|persistent consumed progress|C01 PlayerConsumedProgressionLedgerComponent|
|473|PlayerConsumedProgressionFlags|usedAmbrosia|bool|535|persistent consumed progress|C01 PlayerConsumedProgressionLedgerComponent|
|613|PlayerFishingCapabilityState|fishingSkill|int|818|runtime derived capability|C02 PlayerFishingCapabilitySnapshotComponent|
|614|PlayerFishingCapabilityState|cratePotion|bool|820|runtime derived capability|C02 PlayerFishingCapabilitySnapshotComponent|
|615|PlayerFishingCapabilityState|sonarPotion|bool|822|runtime derived capability|C02 PlayerFishingCapabilitySnapshotComponent|
|616|PlayerFishingCapabilityState|accFishingLine|bool|824|runtime derived capability|C02 PlayerFishingCapabilitySnapshotComponent|
|617|PlayerFishingCapabilityState|accFishingBobber|bool|826|runtime derived capability|C02 PlayerFishingCapabilitySnapshotComponent|
|618|PlayerFishingCapabilityState|accTackleBox|bool|828|runtime derived capability|C02 PlayerFishingCapabilitySnapshotComponent|
|619|PlayerFishingCapabilityState|accLavaFishing|bool|830|runtime derived capability|C02 PlayerFishingCapabilitySnapshotComponent|
|620|PlayerMinionCapacityState|maxMinions|int|832|runtime aggregate|C03 PlayerMinionCapacityComponent|
|621|PlayerMinionCapacityState|numMinions|int|834|runtime aggregate|C03 PlayerMinionCapacityComponent|
|622|PlayerMinionCapacityState|slotsMinions|float|836|runtime aggregate|C03 PlayerMinionCapacityComponent|
|623|PlayerCoreMinionSummonFlags|pygmy|bool|838|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|624|PlayerCoreMinionSummonFlags|raven|bool|840|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|625|PlayerCoreMinionSummonFlags|slime|bool|842|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|626|PlayerCoreMinionSummonFlags|hornetMinion|bool|844|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|627|PlayerCoreMinionSummonFlags|impMinion|bool|846|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|628|PlayerCoreMinionSummonFlags|twinsMinion|bool|848|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|629|PlayerCoreMinionSummonFlags|spiderMinion|bool|850|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|630|PlayerCoreMinionSummonFlags|pirateMinion|bool|852|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|631|PlayerCoreMinionSummonFlags|sharknadoMinion|bool|854|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|632|PlayerCoreMinionSummonFlags|UFOMinion|bool|856|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|633|PlayerCoreMinionSummonFlags|DeadlySphereMinion|bool|858|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|634|PlayerCoreMinionSummonFlags|stardustMinion|bool|860|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|635|PlayerCoreMinionSummonFlags|stardustGuardian|bool|862|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|636|PlayerCoreMinionSummonFlags|stardustDragon|bool|864|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|637|PlayerCoreMinionSummonFlags|batsOfLight|bool|866|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|638|PlayerCoreMinionSummonFlags|babyBird|bool|868|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|639|PlayerCoreMinionSummonFlags|vampireFrog|bool|870|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|640|PlayerCoreMinionSummonFlags|stormTiger|bool|872|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|642|PlayerCoreMinionSummonFlags|smolstar|bool|876|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|643|PlayerCoreMinionSummonFlags|empressBlade|bool|878|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|644|PlayerCoreMinionSummonFlags|flinxMinion|bool|880|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|645|PlayerCoreMinionSummonFlags|abigailMinion|bool|882|runtime capability flag|C04 PlayerCoreMinionCapabilityComponent|
|647|PlayerCrossoverMinionSummonFlags|deadCellsMushroomBoiMinion|bool|886|runtime capability flag|C05 PlayerCrossoverMinionCapabilityComponent|
|648|PlayerCrossoverMinionSummonFlags|palworldCattivaMinion|bool|888|runtime capability flag|C05 PlayerCrossoverMinionCapabilityComponent|
|649|PlayerCrossoverMinionSummonFlags|palworldFoxsparksMinion|bool|890|runtime capability flag|C05 PlayerCrossoverMinionCapabilityComponent|
|641|PlayerMinionDamageTrackingState|highestStormTigerGemOriginalDamage|int|874|runtime high-water cache|C06 PlayerMinionDamageHighWaterMarkComponent|
|646|PlayerMinionDamageTrackingState|highestAbigailCounterOriginalDamage|int|884|runtime high-water cache|C06 PlayerMinionDamageHighWaterMarkComponent|
|923|PlayerUnlockProgressionState|unlockedBiomeTorches|bool|1475|persistent unlock|C07 PlayerUnlockProgressionLedgerComponent|
|924|PlayerUnlockProgressionState|ateArtisanBread|bool|1477|persistent consumed unlock|C07 PlayerUnlockProgressionLedgerComponent|
|925|PlayerUnlockProgressionState|unlockedSuperCart|bool|1479|persistent unlock|C07 PlayerUnlockProgressionLedgerComponent|
|926|PlayerUnlockProgressionState|enabledSuperCart|bool|1481|persistent preference/derived gate|C07 PlayerUnlockProgressionLedgerComponent|
|858|PlayerQuestAndEventCounters|anglerQuestsFinished|int|1344|persistent counter|C08 PlayerQuestEventProgressComponent|
|859|PlayerQuestAndEventCounters|golferScoreAccumulated|int|1346|persistent counter|C08 PlayerQuestEventProgressComponent|
|860|PlayerQuestAndEventCounters|downedDD2EventAnyDifficulty|bool|1349|persistent event progress|C08 PlayerQuestEventProgressComponent|
|927|PlayerLegacyPetState|suspiciouslookingTentacle|bool|1483|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|928|PlayerLegacyPetState|crimsonHeart|bool|1485|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|929|PlayerLegacyPetState|lightOrb|bool|1487|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|930|PlayerLegacyPetState|blueFairy|bool|1489|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|931|PlayerLegacyPetState|redFairy|bool|1491|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|932|PlayerLegacyPetState|greenFairy|bool|1493|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|933|PlayerLegacyPetState|bunny|bool|1495|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|934|PlayerLegacyPetState|turtle|bool|1497|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|935|PlayerLegacyPetState|eater|bool|1499|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|936|PlayerLegacyPetState|penguin|bool|1501|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|937|PlayerLegacyPetState|HasGardenGnomeNearby|bool|1503|cross-domain luck/environment input|integration-review; C09 observes only|
|939|PlayerLegacyPetState|magicLantern|bool|1508|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|940|PlayerLegacyPetState|rabid|bool|1510|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|941|PlayerLegacyPetState|sunflower|bool|1512|runtime effect capability|C09 PlayerLegacyPetCapabilityComponent|
|942|PlayerLegacyPetState|wellFed|bool|1514|runtime effect capability|C09 PlayerLegacyPetCapabilityComponent|
|943|PlayerLegacyPetState|puppy|bool|1516|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|944|PlayerLegacyPetState|grinch|bool|1518|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|945|PlayerLegacyPetState|miniMinotaur|bool|1520|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|962|PlayerLegacyPetState|blackCat|bool|1554|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|963|PlayerLegacyPetState|spider|bool|1556|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|964|PlayerLegacyPetState|squashling|bool|1558|runtime pet capability|C09 PlayerLegacyPetCapabilityComponent|
|981|PlayerBossPetFlags|petFlagKingSlimePet|bool|1592|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|982|PlayerBossPetFlags|petFlagEyeOfCthulhuPet|bool|1594|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|983|PlayerBossPetFlags|petFlagEaterOfWorldsPet|bool|1596|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|984|PlayerBossPetFlags|petFlagBrainOfCthulhuPet|bool|1598|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|985|PlayerBossPetFlags|petFlagSkeletronPet|bool|1600|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|986|PlayerBossPetFlags|petFlagQueenBeePet|bool|1602|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|987|PlayerBossPetFlags|petFlagDestroyerPet|bool|1604|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|988|PlayerBossPetFlags|petFlagTwinsPet|bool|1606|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|989|PlayerBossPetFlags|petFlagSkeletronPrimePet|bool|1608|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|990|PlayerBossPetFlags|petFlagPlanteraPet|bool|1610|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|991|PlayerBossPetFlags|petFlagGolemPet|bool|1612|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|992|PlayerBossPetFlags|petFlagDukeFishronPet|bool|1614|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|993|PlayerBossPetFlags|petFlagLunaticCultistPet|bool|1616|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|994|PlayerBossPetFlags|petFlagMoonLordPet|bool|1618|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|995|PlayerBossPetFlags|petFlagFairyQueenPet|bool|1620|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|1002|PlayerBossPetFlags|petFlagQueenSlimePet|bool|1634|runtime boss-pet capability|C10 PlayerBossPetCapabilityComponent|
|965|PlayerSeasonalAndEventPetFlags|petFlagDD2Gato|bool|1560|runtime event-pet capability|C11 PlayerSeasonalEventPetCapabilityComponent|
|966|PlayerSeasonalAndEventPetFlags|petFlagDD2Ghost|bool|1562|runtime event-pet capability|C11 PlayerSeasonalEventPetCapabilityComponent|
|967|PlayerSeasonalAndEventPetFlags|petFlagDD2Dragon|bool|1564|runtime event-pet capability|C11 PlayerSeasonalEventPetCapabilityComponent|
|996|PlayerSeasonalAndEventPetFlags|petFlagPumpkingPet|bool|1622|runtime seasonal-pet capability|C11 PlayerSeasonalEventPetCapabilityComponent|
|997|PlayerSeasonalAndEventPetFlags|petFlagEverscreamPet|bool|1624|runtime seasonal-pet capability|C11 PlayerSeasonalEventPetCapabilityComponent|
|998|PlayerSeasonalAndEventPetFlags|petFlagIceQueenPet|bool|1626|runtime seasonal-pet capability|C11 PlayerSeasonalEventPetCapabilityComponent|
|999|PlayerSeasonalAndEventPetFlags|petFlagMartianPet|bool|1628|runtime event-pet capability|C11 PlayerSeasonalEventPetCapabilityComponent|
|1000|PlayerSeasonalAndEventPetFlags|petFlagDD2OgrePet|bool|1630|runtime event-pet capability|C11 PlayerSeasonalEventPetCapabilityComponent|
|1001|PlayerSeasonalAndEventPetFlags|petFlagDD2BetsyPet|bool|1632|runtime event-pet capability|C11 PlayerSeasonalEventPetCapabilityComponent|
|968|PlayerStandardNamedPetFlags|petFlagUpbeatStar|bool|1566|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|969|PlayerStandardNamedPetFlags|petFlagSugarGlider|bool|1568|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|970|PlayerStandardNamedPetFlags|petFlagBabyShark|bool|1570|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|971|PlayerStandardNamedPetFlags|petFlagLilHarpy|bool|1572|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|972|PlayerStandardNamedPetFlags|petFlagFennecFox|bool|1574|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|973|PlayerStandardNamedPetFlags|petFlagGlitteryButterfly|bool|1576|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|974|PlayerStandardNamedPetFlags|petFlagBabyImp|bool|1578|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|975|PlayerStandardNamedPetFlags|petFlagBabyRedPanda|bool|1580|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|976|PlayerStandardNamedPetFlags|petFlagPlantero|bool|1582|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|977|PlayerStandardNamedPetFlags|petFlagDynamiteKitten|bool|1584|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|978|PlayerStandardNamedPetFlags|petFlagBabyWerewolf|bool|1586|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|979|PlayerStandardNamedPetFlags|petFlagShadowMimic|bool|1588|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|980|PlayerStandardNamedPetFlags|petFlagVoltBunny|bool|1590|runtime named-pet capability|C12 PlayerStandardNamedPetCapabilityComponent|
|1003|PlayerCrossoverPetFlags|petFlagBerniePet|bool|1636|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1004|PlayerCrossoverPetFlags|petFlagGlommerPet|bool|1638|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1005|PlayerCrossoverPetFlags|petFlagDeerclopsPet|bool|1640|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1006|PlayerCrossoverPetFlags|petFlagPigPet|bool|1642|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1007|PlayerCrossoverPetFlags|petFlagChesterPet|bool|1644|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1008|PlayerCrossoverPetFlags|petFlagJunimoPet|bool|1646|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1009|PlayerCrossoverPetFlags|petFlagBlueChickenPet|bool|1648|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1010|PlayerCrossoverPetFlags|petFlagSpiffo|bool|1650|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1011|PlayerCrossoverPetFlags|petFlagCaveling|bool|1652|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1015|PlayerCrossoverPetFlags|petFlagDeadCellsSwarmBiter|bool|1660|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1016|PlayerCrossoverPetFlags|petFlagPufferfish|bool|1662|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1018|PlayerCrossoverPetFlags|petFlagChillet|bool|1666|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1019|PlayerCrossoverPetFlags|petFlagChilletIgnis|bool|1668|runtime crossover-pet capability|C13 PlayerCrossoverPetCapabilityComponent|
|1012|PlayerWorldObjectPetFlags|petFlagDirtiestBlock|bool|1654|runtime world-object pet capability|C14 PlayerWorldObjectPetCapabilityComponent|
|1013|PlayerWorldObjectPetFlags|petFlagBoulderPet|bool|1656|runtime world-object pet capability|C14 PlayerWorldObjectPetCapabilityComponent|
|1014|PlayerWorldObjectPetFlags|petFlagRainbowBoulderPet|bool|1658|runtime world-object pet capability|C14 PlayerWorldObjectPetCapabilityComponent|
|1017|PlayerWorldObjectPetFlags|petFlagAxeFairyPet|bool|1664|runtime world-object pet capability|C14 PlayerWorldObjectPetCapabilityComponent|
|1020|PlayerCompanionState|companionCube|bool|1670|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1021|PlayerCompanionState|babyFaceMonster|bool|1672|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1028|PlayerCompanionState|snowman|bool|1686|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1030|PlayerCompanionState|dino|bool|1690|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1031|PlayerCompanionState|skeletron|bool|1692|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1032|PlayerCompanionState|hornet|bool|1694|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1033|PlayerCompanionState|zephyrfish|bool|1696|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1034|PlayerCompanionState|tiki|bool|1698|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1035|PlayerCompanionState|parrot|bool|1700|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1036|PlayerCompanionState|truffle|bool|1702|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1037|PlayerCompanionState|sapling|bool|1704|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1038|PlayerCompanionState|cSapling|bool|1706|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1039|PlayerCompanionState|wisp|bool|1708|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|1040|PlayerCompanionState|lizard|bool|1710|runtime companion capability|C15 PlayerCompanionCapabilityComponent|
|955|PlayerMountAndMinecartEffects|onWrongGround|bool|1540|cross-domain vehicle runtime|C16 PlayerMountVehicleIntegrationComponent; integration-review|
|956|PlayerMountAndMinecartEffects|onTrack|bool|1542|cross-domain vehicle runtime|C16 PlayerMountVehicleIntegrationComponent; integration-review|
|957|PlayerMountAndMinecartEffects|cartRampTime|int|1544|cross-domain vehicle runtime|C16 PlayerMountVehicleIntegrationComponent; integration-review|
|958|PlayerMountAndMinecartEffects|cartFlip|bool|1546|cross-domain vehicle runtime|C16 PlayerMountVehicleIntegrationComponent; integration-review|
|959|PlayerMountAndMinecartEffects|trackBoost|float|1548|cross-domain vehicle runtime|C16 PlayerMountVehicleIntegrationComponent; integration-review|
|960|PlayerMountAndMinecartEffects|lastBoost|Vector2|1550|cross-domain vehicle runtime|C16 PlayerMountVehicleIntegrationComponent; state-only core implemented; writer integration-review|
|961|PlayerMountAndMinecartEffects|mount|Terraria.Mount|1552|cross-domain vehicle runtime|C16 Mount/Minecart runtime owner; integration-review; blocked in P05|
|938|PlayerAccessoryProgressionEffects|brokenMirrorBadLuck|bool|1505|cross-domain luck/effect input|C17 PlayerAccessoryEffectSnapshotComponent; integration-review|
|946|PlayerAccessoryProgressionEffects|flowerBoots|bool|1522|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|947|PlayerAccessoryProgressionEffects|fairyBoots|bool|1524|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|948|PlayerAccessoryProgressionEffects|hellfireTreads|bool|1526|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|949|PlayerAccessoryProgressionEffects|moonLordLegs|bool|1528|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|950|PlayerAccessoryProgressionEffects|deadMansSweater|bool|1530|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|951|PlayerAccessoryProgressionEffects|arcticDivingGear|bool|1532|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|952|PlayerAccessoryProgressionEffects|coolWhipBuff|bool|1534|runtime buff/equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|953|PlayerAccessoryProgressionEffects|cobWhipBuff|bool|1536|runtime buff/equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|954|PlayerAccessoryProgressionEffects|wearsRobe|bool|1538|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|1022|PlayerAccessoryProgressionEffects|magicCuffs|bool|1674|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|1023|PlayerAccessoryProgressionEffects|coldDash|bool|1676|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|1024|PlayerAccessoryProgressionEffects|sailDash|bool|1678|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|1025|PlayerAccessoryProgressionEffects|desertDash|bool|1680|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|1026|PlayerAccessoryProgressionEffects|desertBoots|bool|1682|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|1027|PlayerAccessoryProgressionEffects|eyeSpring|bool|1684|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|
|1029|PlayerAccessoryProgressionEffects|scope|bool|1688|runtime equipment effect|C17 PlayerAccessoryEffectSnapshotComponent|

The table has 164 rows and no P05 property rows. The `HasGardenGnomeNearby`, `brokenMirrorBadLuck`,
and raw `mount` field remain cross-subsystem findings; the six scalar mount/minecart fields now have
a state-only Player component core but retain an integration-reviewed writer.

## 7. Proposed component boundaries

| ID | Proposed component | State/lifetime | Proposed target path (`status: proposed`) | Single writer candidate |
|---|---|---|---|---|
|C01|`PlayerConsumedProgressionLedgerComponent`|persistent consumed progress|`Player/Progression/Components/PlayerConsumedProgressionLedgerComponent.cs`|progression commit system|
|C02|`PlayerFishingCapabilitySnapshotComponent`|per-reset equipment/buff snapshot|`Player/Progression/Components/PlayerFishingCapabilitySnapshotComponent.cs`|fishing capability rebuild system|
|C03|`PlayerMinionCapacityComponent`|per-tick aggregate/cache|`Player/Progression/Components/PlayerMinionCapacityComponent.cs`|minion capacity system plus explicit projectile commit port|
|C04|`PlayerCoreMinionCapabilityComponent`|per-reset core summon capability|`Player/Progression/Components/PlayerCoreMinionCapabilityComponent.cs`|minion capability rebuild system|
|C05|`PlayerCrossoverMinionCapabilityComponent`|per-reset crossover summon capability|`Player/Progression/Components/PlayerCrossoverMinionCapabilityComponent.cs`|crossover content capability system|
|C06|`PlayerMinionDamageHighWaterMarkComponent`|per-tick calculation cache|`Player/Progression/Components/PlayerMinionDamageHighWaterMarkComponent.cs`|projectile scan/commit seam; integration-review|
|C07|`PlayerUnlockProgressionLedgerComponent`|persistent unlock/preference progress|`Player/Progression/Components/PlayerUnlockProgressionLedgerComponent.cs`|progression commit system|
|C08|`PlayerQuestEventProgressComponent`|persistent counters/event progress|`Player/Progression/Components/PlayerQuestEventProgressComponent.cs`|quest/event commit ports|
|C09|`PlayerLegacyPetCapabilityComponent`|per-reset legacy pet/effect snapshot|`Player/Progression/Components/PlayerLegacyPetCapabilityComponent.cs`|pet capability rebuild system; luck inputs integration-review|
|C10|`PlayerBossPetCapabilityComponent`|per-reset boss pet capability|`Player/Progression/Components/PlayerBossPetCapabilityComponent.cs`|pet capability rebuild system|
|C11|`PlayerSeasonalEventPetCapabilityComponent`|per-reset seasonal/event pet capability|`Player/Progression/Components/PlayerSeasonalEventPetCapabilityComponent.cs`|pet capability rebuild system|
|C12|`PlayerStandardNamedPetCapabilityComponent`|per-reset named pet capability|`Player/Progression/Components/PlayerStandardNamedPetCapabilityComponent.cs`|pet capability rebuild system|
|C13|`PlayerCrossoverPetCapabilityComponent`|per-reset crossover pet capability|`Player/Progression/Components/PlayerCrossoverPetCapabilityComponent.cs`|crossover content capability system|
|C14|`PlayerWorldObjectPetCapabilityComponent`|per-reset world-object pet capability|`Player/Progression/Components/PlayerWorldObjectPetCapabilityComponent.cs`|world-object pet capability system; integration-review|
|C15|`PlayerCompanionCapabilityComponent`|per-reset companion capability|`Player/Progression/Components/PlayerCompanionCapabilityComponent.cs`|companion capability system|
|C16|`PlayerMountVehicleIntegrationComponent`|six Player-side vehicle interaction fields; raw Mount object deferred|`src/Player/Progression/PlayerMountVehicleIntegrationComponent.cs`|state-only core saved; Mount/Minecart writer and raw `mount` owner remain integration-review|
|C17|`PlayerAccessoryEffectSnapshotComponent`|per-reset equipment/buff effect snapshot|`Player/Progression/Components/PlayerAccessoryEffectSnapshotComponent.cs`|equipment effect rebuild system; luck field integration-review|

No proposed component owns a pet/minion Projectile entity, a `Mount` catalog, an item instance, an
NPC, a world tile, a network packet, or a persistence stream. Those values cross the boundary only
through explicit ports, commands or projections.

## 8. Component checkpoint C01 - consumed progression ledger

`C01 PlayerConsumedProgressionLedgerComponent` is the first completed design checkpoint. It owns
the six `usedAegisCrystal`, `usedAegisFruit`, `usedArcaneCrystal`, `usedGalaxyPearl`,
`usedGummyWorm`, and `usedAmbrosia` fields as persistent player progress. It does not own the item
inventory, the stat changes caused by consuming an item, or network transport.

Target implemented for the isolated core:
`src/Player/Progression/PlayerConsumedProgressionLedgerComponent.cs`
(`status: implemented-core; integration-blocked`). The component namespace is
`Terraria.Player.Progression`, and the source stays flat under the existing small Player domain
boundary rather than introducing a generic `Components` directory.
`PlayerProgressionCommitSystem` is the only writer in the new boundary and accepts
`ConsumePlayerUpgradeCommand` with a valid `PlayerProgressionCommandToken`.
`ConsumedUpgradeEligibilityQuery` is pure and reads only the ledger. The component does not own item
inventory, stat changes, network transport, persistence streams or effect emission.

The persistence adapter and network projection remain proposed and unimplemented. They must read
and write a versioned DTO at an external boundary, may use the complete-reference field order, and
must not expose a binary reader or packet type to the component.

The complete reference confirms serialization and version-gated deserialization, while Version4's
own serialization methods are stubs. The isolated core is `evidenceStatus:
full-reference-supplemented; current-NLTX-implemented-core`, and its focused verifier passed.
Duplicate consume commands are idempotent at the ledger-bit boundary: the command is accepted once
per supported upgrade, the bit is committed once, and no stat/effect side effect is emitted by this
component. Expected-revision, durable round-trip, network ordering and legacy field migration remain
unverified and blocked.

### C01 implementation checkpoint (2026-09-11T17:52:06Z)

- Actual files added under `src/Player/Progression/`:
  `PlayerConsumedProgressionLedgerComponent.cs`,
  `PlayerConsumedProgressionUpgrade.cs`,
  `PlayerProgressionCommandToken.cs`,
  `ConsumePlayerUpgradeCommand.cs`,
  `PlayerProgressionCommitStatus.cs`,
  `PlayerProgressionCommitResult.cs`,
  `ConsumedUpgradeEligibilityQuery.cs`, and
  `PlayerProgressionCommitSystem.cs`.
- Core behavior: six Version4 consumed-progress fields map to typed PascalCase members; fresh
  supported upgrades are eligible; the single commit system rejects empty tokens and unknown
  upgrades, commits each supported bit once, and returns `AlreadyConsumed` on a repeated command.
  The query has no writes or external reads.
- Dependency impact: item consumption, player stat/effect systems, persistence and network remain
  consumers of the commit result; no existing public type or registration key was changed; no item,
  stat, packet, save stream or effect type was copied into the component.
- Verification: the temporary `P05ProgressionLedgerVerifier` built through the serial wrapper with
  exit code 0, 0 warnings and 0 errors, then ran with exit code 0 and printed
  `P05 C01 verifier passed.`. Artifacts were under
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
  `Build/bin/P05ProgressionLedgerVerifier/Debug/net10.0/P05ProgressionLedgerVerifier.dll`.
- Not verified: Version4 behavior equivalence, save/load round-trip, packet projection, expected
  revision conflict handling, item/stat side effects, and legacy public-field adapter integration.
- Rollback: remove the isolated C01 source boundary and retain the prior no-P05 Player source path if
  later integration evidence finds a second writer, field-order mismatch or changed consume semantics.

## 9. Component checkpoint C02 - fishing capability snapshot

`C02 PlayerFishingCapabilitySnapshotComponent` owns the seven runtime fields
`fishingSkill`, `cratePotion`, `sonarPotion`, `accFishingLine`, `accFishingBobber`,
`accTackleBox`, and `accLavaFishing`. These fields are rebuilt by the equipment/buff path, not
loaded as durable unlocks. The current Version4 evidence includes writes in
`Player.cs:4573-4581,6824-6830,6933-6935,7377,8301-8330` and reset at
`Player.cs:10493-10499`; complete fishing attempt readers still need a dedicated audit.

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerFishingCapabilitySnapshotComponent.cs`.
- Actual writer: `PlayerFishingCapabilityRebuildSystem` writes all seven fields from the explicit
  `PlayerFishingCapabilityRebuildInput`, and its reset operation clears the complete runtime snapshot.
- Actual pure query: `FishingCapabilityQuery` returns the stored fishing skill and individual potion/
  accessory/lava-fishing capabilities without mutation or external reads. It deliberately does not
  claim the complete rod/bait/water/environment `FishingEligibility` algorithm proposed for a later
  integration boundary.
- Actual files: `PlayerFishingCapabilitySnapshotComponent.cs`,
  `PlayerFishingCapabilityRebuildInput.cs`, `PlayerFishingCapabilityRebuildSystem.cs`, and
  `FishingCapabilityQuery.cs` under `src/Player/Progression/`.
- Dependency impact: equipment, buff and source-backed content adapters remain external input
  producers; Fishing/Catch systems consume the snapshot; bobber Projectile creation remains an
  explicit integration command. Existing `PlayerFishingCapabilityComponent` and
  `PlayerFishingCapabilityState` were not renamed or modified, so overlap compatibility remains open.
- Verification: `P05FishingCapabilityVerifier` build/run passed with exit code 0; build had 0 warnings
  and 0 errors, and run used `--no-build --no-restore`.
- Unverified/blocking: complete fishing-attempt eligibility, rod/bait/pole readers, water/environment
  rules, source-backed content mapping, bobber Projectile integration, revision handling, persistence/
  network projection and compatibility handoff remain unimplemented. C02 is therefore
  `implemented-core; integration-blocked`, not a full fishing migration.
- Rollback: retain the prior partial capability read path if reset/rebuild inputs or potion/accessory/
  lava-fishing behavior diverge; retain the evidence and block the next unit.

## 10. Component checkpoint C03 - minion capacity aggregate

`C03 PlayerMinionCapacityComponent` owns the three player aggregate fields `maxMinions`,
`numMinions`, and `slotsMinions`. `maxMinions` is rebuilt from buffs/equipment and reset to its
default at `Player.cs:10446`; `numMinions` and `slotsMinions` are per-tick/projectile aggregates
reset at `Player.cs:10024-10025`. Projectile admission reads and increments all three at
`Projectile.cs:14743-14767`, while `Player.UpdateProjectileCaches` also scans owned projectiles.

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerMinionCapacityComponent.cs`.
- The component stores the three typed aggregate fields with a default capacity of one minion and
  internal mutation access. Existing capacity query and commit files remain an integration seam;
  they do not make Projectile ownership or reservation ordering complete.
- The component is a Player aggregate candidate, but the single writer is deliberately unresolved:
  either Projectile owns reservation/commit and Player exposes a projection, or Player accepts
  validated Projectile deltas. `crossSubsystemOwner: integration-review` is mandatory.
- Existing NLTX has duplicate-looking capacity data in `PlayerSummonCapacityState` and
  `PlayerAbilityComponent`, while per-projectile slot data already exists in
  `src/Projectile/ProjectileMinionCapabilityComponent.cs` and the Dome simulation. No duplicate
  per-projectile fields may be added to Player.
- Rollback: restore the prior capacity path if fractional slots, duplicate spawn, owner loss or
  trim behavior changes. Verification: not-run.

### C03 implementation checkpoint (2026-09-12T06:50:00Z)

- Actual component file: `src/Player/Progression/PlayerMinionCapacityComponent.cs`.
- Core fields: `MaxMinions`, `NumMinions`, and `SlotsMinions`; no Projectile entity or per-Projectile
  metadata was copied into the Player component.
- Dependency impact: existing `PlayerMinionCapacityCommitSystem`,
  `RemainingMinionCapacityQuery`, and `SubmitMinionCapacityDeltaCommand` remain unverified and
  require a single Player/Projectile reservation owner.
- Verification status: not-verified. The affected project has not yet been rebuilt after this
  checkpoint.

## 11. Component checkpoint C04 - core minion capability

`C04 PlayerCoreMinionCapabilityComponent` owns the 22 core summon flags from source rows 623-645:
`pygmy`, `raven`, `slime`, `hornetMinion`, `impMinion`, `twinsMinion`, `spiderMinion`,
`pirateMinion`, `sharknadoMinion`, `UFOMinion`, `DeadlySphereMinion`, `stardustMinion`,
`stardustGuardian`, `stardustDragon`, `batsOfLight`, `babyBird`, `vampireFrog`, `stormTiger`,
`smolstar`, `empressBlade`, `flinxMinion`, and `abigailMinion`.

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerCoreMinionCapabilityComponent.cs`.
- Version4 `UpdateBuffs` writes representative flags at `Player.cs:4880-5184`, while
  `ResetEffects` clears the runtime flags at `:10596-10610`; the flags are eligibility/capability
  inputs and do not represent the Projectile entity itself.
- Proposed writer: `PlayerMinionCapabilityRebuildSystem`; proposed query:
  `CoreMinionCapabilityQuery` returns immutable content-key capabilities. Spawn/refresh is an
  explicit Projectile command and must be idempotent.
- `stormTiger` and `abigailMinion` have corresponding high-water data in C06; C04 must not own
  those damage values. The content registry and every flag-to-projectile mapping remain partial.
- Rollback: preserve the legacy buff/projectile path if any flag resets incorrectly, maps to an
  unknown content key, or causes duplicate projectiles. Verification: not-run.

### C04 implementation checkpoint (2026-09-12T06:50:00Z)

- Actual component file: `src/Player/Progression/PlayerCoreMinionCapabilityComponent.cs`.
- Core fields: all 22 source capability flags, mapped to PascalCase members; `UfoMinion` preserves
  the acronym style used by the new C# component boundary.
- Dependency impact: the component contains only resettable capability state. Content registration,
  reset/rebuild ownership, Projectile entity commands, duplicate refresh handling and compatibility
  handoff remain external.
- Verification status: not-verified. No focused C04 verifier exists and the affected project has
  not yet been rebuilt after this checkpoint.

## 12. Component checkpoint C05 - crossover minion capability

`C05 PlayerCrossoverMinionCapabilityComponent` owns only `deadCellsMushroomBoiMinion`,
`palworldCattivaMinion`, and `palworldFoxsparksMinion` (source rows 647-649). They follow the same
reset/rebuild lifecycle as core minions but use a separately versioned crossover content registry.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerCrossoverMinionCapabilityComponent.cs`.
- Proposed writer: `PlayerCrossoverMinionCapabilitySystem`; proposed query returns capability keys
  only when the crossover content adapter confirms the content is registered and enabled.
- The component does not own Projectile identity, damage, spawn timing or capacity. It submits the
  same explicit refresh/despawn command seam as C04 and must fail closed for unknown content.
- The component source stores the three capability flags with implicit `false` defaults. It does
  not contain crossover content keys, Projectile identity, damage, spawn timing or capacity.
- Current evidence confirms declarations and reset behavior pattern, but does not close all content
  registration or network/persistence paths for crossover flags.
- Rollback: disable the crossover adapter and preserve the legacy path if content absence,
  reconnect, reset or duplicate-refresh behavior differs. Verification: no focused verifier;
  affected-project build evidence after this checkpoint, not-run.

### C05 implementation checkpoint (2026-09-12T07:10:17Z)

- Actual component file: `src/Player/Progression/PlayerCrossoverMinionCapabilityComponent.cs`.
- Core fields: `DeadCellsMushroomBoiMinion`, `PalworldCattivaMinion`, and
  `PalworldFoxsparksMinion`; all are resettable capability state with `internal` mutation access.
- Dependency impact: crossover content registration, reset/rebuild ownership, Projectile entity
  commands, duplicate refresh handling and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 13. Component checkpoint C06 - minion damage high-water marks

`C06 PlayerMinionDamageHighWaterMarkComponent` owns only
`highestStormTigerGemOriginalDamage` and `highestAbigailCounterOriginalDamage` (source rows 641
and 646). `Player.UpdateProjectileCaches` compares and raises the maxima at
`Player.cs:6285-6296`, then clears both at `:6308-6309`. The fields are per-tick calculation caches,
not persistent player progress and not Projectile definition data.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerMinionDamageHighWaterMarkComponent.cs`.
- The component source stores both high-water values with the C# default `0`. It does not own
  Projectile definitions, observation provenance, entity identity or the reset schedule.
- Proposed observation contract: Projectile simulation emits immutable owned-projectile damage
  observations; one `PlayerMinionDamageTrackingSystem` applies `max(current, observation)` and
  resets at the Version4-equivalent boundary.
- C04 owns `stormTiger`/`abigailMinion` capability flags but not these values. The exact timing of
  cache scan versus Projectile spawn/update and the source of `originalDamage` remain
  `crossSubsystemOwner: integration-review`.
- The cache must never be serialized, networked as authoritative progress, or shared across Player
  entities. Invalid owner or stale projectile revisions are rejected rather than applied.
- Rollback: disable the aggregation writer and keep the legacy scan if max/reset phase, player
  isolation or original-damage semantics differ. Verification: no focused verifier; affected-project
  build evidence after this checkpoint, not-run.

### C06 implementation checkpoint (2026-09-12T07:14:22Z)

- Actual component file: `src/Player/Progression/PlayerMinionDamageHighWaterMarkComponent.cs`.
- Core fields: `HighestStormTigerGemOriginalDamage` and `HighestAbigailCounterOriginalDamage`; both
  are per-tick cache state with `internal` mutation access and zero defaults.
- Dependency impact: Projectile observation provenance, one Player/Projectile aggregation writer,
  tick reset ordering and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 14. Component checkpoint C07 - unlock progression ledger

`C07 PlayerUnlockProgressionLedgerComponent` owns `unlockedBiomeTorches`, `ateArtisanBread`,
`unlockedSuperCart`, and `enabledSuperCart` (source rows 923-926). The complete reference confirms
these fields in versioned persistence (`Player.cs:55377-55379,55513-55515,55859-55863,56323-56329`),
and Version4 has network writes/reads at `NetMessage.cs:181-192` and `MessageBuffer.cs:277-287`.
`UsingBiomeTorches` and `UsingSuperCart` are derived properties, not additional fields.

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerUnlockProgressionLedgerComponent.cs`.
- Actual core files: `PlayerUnlockProgressionLedgerComponent.cs`,
  `PlayerUnlockProgressionKind.cs`, `UnlockPlayerProgressCommand.cs`,
  `SetPlayerSuperCartPreferenceCommand.cs`, `PlayerUnlockProgressCommitStatus.cs`,
  `PlayerUnlockProgressCommitResult.cs`, `UsingBiomeTorchesQuery.cs`,
  `UsingSuperCartQuery.cs`, and the C07 overloads in `PlayerProgressionCommitSystem.cs`.
- The core command is `UnlockPlayerProgressCommand`; its commit path is shared with C01 through
  `PlayerProgressionCommitSystem`. `UsingBiomeTorchesQuery` accepts the externally owned builder
  preference, while `UsingSuperCartQuery` derives `unlockedSuperCart && enabledSuperCart`.
- `enabledSuperCart` is a player preference/gate that has no effect when `unlockedSuperCart` is
  false. The adapter must preserve this invariant and the complete-reference fallback that derives
  unlock state from legacy inventory when required.
- `unlockedBiomeTorches` is set by item use and may trigger world/achievement effects; those are
  explicit commands/events and are not embedded in the component.
- Core behavior: supported unlocks commit once; repeated unlocks return `AlreadyUnlocked`; empty
  tokens are rejected; Super Cart preference changes only the component value and cannot make an
  unavailable unlock active through `UsingSuperCartQuery`. No item, achievement, inventory,
  persistence or network side effect is emitted by this core.
- Verification: the temporary `P05ProgressionLedgerVerifier` built through the serial wrapper with
  exit code 0, 0 warnings and 0 errors, then ran with exit code 0 and printed
  `P05 C07 verifier passed.`. Artifacts were under
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
  `Build/bin/P05ProgressionLedgerVerifier/Debug/net10.0/P05ProgressionLedgerVerifier.dll`.
- Dependency impact: item use, builder preference, vehicle, persistence and network systems still
  consume or adapt the committed state; no existing public type or registration key was changed.
- Unverified/blocking: expected-revision conflict handling, versioned save/load adapters, packet
  packing, legacy-field compatibility handoff, old-inventory Super Cart fallback, builder status
  mapping and achievement/world effects remain unimplemented. C07 is therefore
  `implemented-core; integration-blocked`, not a full migration.
- Rollback: restore the legacy progress adapter if persistence release gates, network bits, derived
  property behavior or duplicate unlock commands differ; retain the evidence and block the next
  unit.

## 15. Component checkpoint C08 - quest and event progress

`C08 PlayerQuestEventProgressComponent` owns `anglerQuestsFinished`, `golferScoreAccumulated`, and
`downedDD2EventAnyDifficulty` (source rows 858-860). Version4 network writes/reads the first two
through `NetMessage.cs:1149-1150` and `MessageBuffer.cs:2483-2484`, while DD2 progress is packed at
`NetMessage.cs:467` and read at `MessageBuffer.cs:721`. The complete reference persists the counters
and DD2 bit at `Player.cs:55491,55508,55386,55877,56237,56305`.

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerQuestEventProgressComponent.cs`.
- Actual core files: `PlayerQuestEventProgressComponent.cs`, `RecordAnglerQuestCommand.cs`,
  `AccumulateGolferScoreCommand.cs`, `RecordDd2ProgressCommand.cs`,
  `PlayerQuestEventProgressCommitStatus.cs`, `PlayerQuestEventProgressCommitResult.cs`, and the
  shared C08 overloads in `PlayerProgressionCommitSystem.cs`.
- Core commands are token-validated and idempotent within the component instance. Angler progress
  increments once per accepted token, DD2 progress is monotonic, and Golfer score rejects negative
  deltas and clamps the accumulated value at one billion (complete reference `Player.cs:4861-4866`).
- The command-token cache is an in-memory duplicate guard only; it is not durable progress and is not
  part of the persistence or network contract.
- The component is player-scoped, but Angler/NPC reward generation, Golfer world/session state and
  DD2 event lifecycle remain external integration seams. It must not own NPC identity or world event
  state.
- Dependency impact: quest/event callers, NPC reward generation, persistence and network systems
  consume or adapt committed progress; no NPC, world, packet or save-stream side effect is emitted
  by this core.
- Verification: the temporary `P05ProgressionLedgerVerifier` built through the serial wrapper with
  exit code 0, 0 warnings and 0 errors, then ran with exit code 0 and printed
  `P05 C07/C08 verifier passed.`. Artifacts were under
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
  `Build/bin/P05ProgressionLedgerVerifier/Debug/net10.0/P05ProgressionLedgerVerifier.dll`.
- Unverified/blocking: expected-revision conflict handling, versioned save/load adapters, packet
  packing and authorization, legacy-field compatibility, duplicate reward protection across
  reconnect, event ordering and external Angler/Golfer/DD2 caller integration remain unimplemented.
  C08 is therefore `implemented-core; integration-blocked`, not a full migration.
- Rollback: restore the legacy counter/event adapters if cap, release-gated persistence, network
  order, duplicate reward or world/NPC event behavior changes; retain the evidence and block the
  next unit.

## 16. Component checkpoint C09 - legacy pet capability

`C09 PlayerLegacyPetCapabilityComponent` groups the 21 legacy pet/effect flags in source rows
927-945 and 962-964: `suspiciouslookingTentacle`, `crimsonHeart`, `lightOrb`, `blueFairy`,
`redFairy`, `greenFairy`, `bunny`, `turtle`, `eater`, `penguin`, `HasGardenGnomeNearby`,
`magicLantern`, `rabid`, `sunflower`, `wellFed`, `puppy`, `grinch`, `miniMinotaur`, `blackCat`,
`spider`, and `squashling`.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerLegacyPetCapabilityComponent.cs`.
- Buff processing writes representative pet flags at `Player.cs:5303-5544`; `ResetEffects` clears
  the runtime values at `:10409-10415,10568-10570`. The component is a reset/rebuild snapshot,
  not a durable pet collection.
- The component source stores all 21 snapshot flags with implicit `false` defaults. It does not own
  pet entities, Projectile identity, item state or effect calculation.
- `HasGardenGnomeNearby` is read by luck calculation (`Player.cs:17914`) and is an environment
  observation, not an ordinary pet capability. It remains `crossSubsystemOwner: integration-review`;
  C09 may expose a read-only handoff but must not own its world-object writer.
- Proposed pet query maps flags to capability keys; pet entity refresh/despawn uses an explicit
  command and Projectile/companion entity owners. `wellFed`, `sunflower`, `rabid` and similar
  effect flags require effect-domain readers rather than a giant pet behavior system.
- Rollback: keep the legacy reset/buff path if a pet refresh, luck effect, death, disconnect or
  world-object observation changes. Verification: no focused verifier; affected-project build
  evidence after this checkpoint, not-run.

### C09 implementation checkpoint (2026-09-12T07:16:10Z)

- Actual component file: `src/Player/Progression/PlayerLegacyPetCapabilityComponent.cs`.
- Core fields: the 21 source pet/effect flags, including `HasGardenGnomeNearby`; all are resettable
  snapshot state with `internal` mutation access.
- Dependency impact: Buff/ResetEffects rebuild, pet entity refresh, luck calculation and world-object
  observation remain external. The component owns no Tile, world-object, entity or effect writer.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 17. Component checkpoint C10 - Boss pet capability

`C10 PlayerBossPetCapabilityComponent` owns the 16 Boss pet flags in source rows 981-995 and 1002:
`petFlagKingSlimePet`, `petFlagEyeOfCthulhuPet`, `petFlagEaterOfWorldsPet`,
`petFlagBrainOfCthulhuPet`, `petFlagSkeletronPet`, `petFlagQueenBeePet`, `petFlagDestroyerPet`,
`petFlagTwinsPet`, `petFlagSkeletronPrimePet`, `petFlagPlanteraPet`, `petFlagGolemPet`,
`petFlagDukeFishronPet`, `petFlagLunaticCultistPet`, `petFlagMoonLordPet`,
`petFlagFairyQueenPet`, and `petFlagQueenSlimePet`.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerBossPetCapabilityComponent.cs`.
- `UpdateBuffs` refreshes Boss pet flags and their pet Projectile time at `Player.cs:5378-5462`;
  `ResetEffects` clears the group at `:10514-10535`. This supports a runtime capability snapshot,
  not a durable Boss collection.
- The component source stores all 16 Boss capability flags with implicit `false` defaults. It does
  not own Boss/NPC state, Projectile identity or entity lifecycle.
- Proposed writer: `PlayerBossPetCapabilitySystem`; proposed query returns Boss pet content keys;
  an explicit pet entity command handles spawn/refresh/despawn. The flag may be set repeatedly by
  multiple buffs (the source has repeated King Slime/Queen Slime calls), so refresh must be
  idempotent.
- Boss/NPC progression is a consumer or content source, not owned by this component. The content
  registry and all death/disconnect cleanup paths remain incomplete.
- Rollback: preserve the source buff/projectile path if repeated buff application, owner death,
  reconnect or Boss content mapping changes. Verification: no focused verifier; affected-project
  build evidence after this checkpoint, not-run.

### C10 implementation checkpoint (2026-09-12T07:17:30Z)

- Actual component file: `src/Player/Progression/PlayerBossPetCapabilityComponent.cs`.
- Core fields: the 16 Boss pet flags; all are resettable capability state with `internal` mutation
  access.
- Dependency impact: Boss content registration, repeated-buff coalescing, pet entity refresh/despawn,
  death/disconnect cleanup and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 18. Component checkpoint C11 - seasonal and event pet capability

`C11 PlayerSeasonalEventPetCapabilityComponent` owns the nine seasonal/event flags
`petFlagDD2Gato`, `petFlagDD2Ghost`, `petFlagDD2Dragon`, `petFlagPumpkingPet`,
`petFlagEverscreamPet`, `petFlagIceQueenPet`, `petFlagMartianPet`, `petFlagDD2OgrePet`, and
`petFlagDD2BetsyPet` (source rows 965-967 and 996-1001).

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerSeasonalEventPetCapabilityComponent.cs`.
- `UpdateBuffs` sets DD2 and event flags at `Player.cs:5326-5539`; `ResetEffects` clears them at
  `:10528-10549`. These are runtime capabilities. C08's durable `downedDD2EventAnyDifficulty`
  remains a separate progress ledger and is not copied here.
- The component source stores all nine seasonal/event capability flags with implicit `false` defaults.
  It does not own durable DD2 progress, event state, Projectile identity or pet entity lifecycle.
- Proposed writer: `PlayerSeasonalEventPetCapabilitySystem`; event state is read through a port,
  while pet entity refresh/despawn uses explicit commands. Unknown or inactive event content must
  fail closed and not leave a stale flag/entity after world change or disconnect.
- The source has both event progress and event pet flags, so the implementation must test the
  ordering between event lifecycle, reset, buff processing and projectile terminal cleanup.
- Rollback: retain the legacy event/buff path if event gating, reconnect, death or duplicate pet
  refresh changes. Verification: no focused verifier; affected-project build evidence after this
  checkpoint, not-run.

### C11 implementation checkpoint (2026-09-12T07:23:43Z)

- Actual component file: `src/Player/Progression/PlayerSeasonalEventPetCapabilityComponent.cs`.
- Core fields: the nine seasonal/event pet flags; all are resettable capability state with `internal`
  mutation access.
- Dependency impact: event lifecycle gating, reset/buff ordering, pet entity refresh/despawn,
  terminal cleanup and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 19. Component checkpoint C12 - standard named pet capability

`C12 PlayerStandardNamedPetCapabilityComponent` owns the 13 standard named pet flags in source rows
968-980: `petFlagUpbeatStar`, `petFlagSugarGlider`, `petFlagBabyShark`, `petFlagLilHarpy`,
`petFlagFennecFox`, `petFlagGlitteryButterfly`, `petFlagBabyImp`, `petFlagBabyRedPanda`,
`petFlagPlantero`, `petFlagDynamiteKitten`, `petFlagBabyWerewolf`, `petFlagShadowMimic`, and
`petFlagVoltBunny`.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerStandardNamedPetCapabilityComponent.cs`.
- `UpdateBuffs` writes these flags and their pet Projectile maintenance at
  `Player.cs:5330-5374,5539`; `ResetEffects` clears the group at `:10536-10548`.
- The component source stores all 13 standard named pet flags with implicit `false` defaults. It
  does not own item ownership, content registry entries, Projectile identity or pet entities.
- Proposed writer: `PlayerStandardNamedPetCapabilitySystem`; proposed query maps stable content
  keys to entity refresh commands. The component owns neither item ownership nor spawned entity
  identity.
- Content mappings must be immutable per catalog revision, unknown keys must fail closed, and
  repeated buff evaluation must not create duplicate pet entities.
- Rollback: preserve the legacy path if any named pet mapping, reset, duplicate refresh, reconnect
  or death cleanup differs. Verification: no focused verifier; affected-project build evidence after
  this checkpoint, not-run.

### C12 implementation checkpoint (2026-09-12T07:30:18Z)

- Actual component file: `src/Player/Progression/PlayerStandardNamedPetCapabilityComponent.cs`.
- Core fields: the 13 standard named pet flags; all are resettable capability state with `internal`
  mutation access.
- Dependency impact: catalog-revision mapping, unknown-key handling, reset/rebuild ownership, pet
  entity refresh/despawn and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 20. Component checkpoint C13 - crossover pet capability

`C13 PlayerCrossoverPetCapabilityComponent` owns the 13 crossover pet flags in source rows
1003-1011, 1015-1016 and 1018-1019: `petFlagBerniePet`, `petFlagGlommerPet`,
`petFlagDeerclopsPet`, `petFlagPigPet`, `petFlagChesterPet`, `petFlagJunimoPet`,
`petFlagBlueChickenPet`, `petFlagSpiffo`, `petFlagCaveling`, `petFlagDeadCellsSwarmBiter`,
`petFlagPufferfish`, `petFlagChillet` and `petFlagChilletIgnis`.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerCrossoverPetCapabilityComponent.cs`.
- Version4 declares these fields at `Player.cs:1636-1652,1660-1668`; the selected
  `UpdateBuffs` evidence maps Bernie through Caveling at `:5464-5504` and Dead Cells/Pufferfish
  at `:5517-5527`. `ResetEffects` clears the group at `:10551-10567`. The complete writer/content
  mapping for `petFlagChillet` and `petFlagChilletIgnis` is not closed by the inspected path and
  remains an evidence gap rather than an invented mapping.
- The component source stores all 13 crossover pet capability flags with implicit `false` defaults.
  It does not own item ownership, Projectile identity or crossover-world entities.
- Proposed writer: `PlayerCrossoverPetCapabilitySystem`; a catalog-revision-bound crossover
  adapter maps source buff/content keys to capability keys and submits idempotent pet entity
  refresh/despawn commands. The component owns capability state only; it does not own item
  ownership, Projectile identity, or crossover-world entities.
- Unknown, inactive, or unavailable crossover keys fail closed. Repeated buff evaluation must
  coalesce by player, capability and lifecycle revision, and a reset, death, disconnect or world
  change must not leave a stale crossover entity. No crossover flag is a persistent unlock merely
  because the legacy field is public.
- Rollback: retain the legacy crossover buff/projectile path if catalog revision, missing-key
  behavior, Chillet mapping, duplicate refresh, reconnect or terminal cleanup differs.
  Verification: no focused verifier; affected-project build evidence after this checkpoint, not-run.

### C13 implementation checkpoint (2026-09-12T07:36:48Z)

- Actual component file: `src/Player/Progression/PlayerCrossoverPetCapabilityComponent.cs`.
- Core fields: the 13 crossover pet flags, including `PetFlagChillet` and
  `PetFlagChilletIgnis`; all are resettable capability state with `internal` mutation access.
- Dependency impact: catalog-revision mapping, especially the incomplete Chillet mappings, reset/
  rebuild ownership, pet entity refresh/despawn and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 21. Component checkpoint C14 - world-object pet capability

`C14 PlayerWorldObjectPetCapabilityComponent` owns the four world-object-backed pet flags in source
rows 1012-1014 and 1017: `petFlagDirtiestBlock`, `petFlagBoulderPet`, `petFlagRainbowBoulderPet`
and `petFlagAxeFairyPet`.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerWorldObjectPetCapabilityComponent.cs`.
- Version4 declares the fields at `Player.cs:1654-1658,1664`; `UpdateBuffs` sets them through
  world-object/content buff cases at `:5505-5527`, and `ResetEffects` clears them at
  `:10560-10566`. This confirms a resettable runtime capability snapshot, not ownership of the
  backing Tile, world object, or spawned pet entity.
- The component source stores all four world-object pet capability flags with implicit `false`
  defaults. It does not own Tile state, world-object identity, Projectile identity or pet entities.
- Proposed writer: `PlayerWorldObjectPetCapabilitySystem`; a source-backed Tile/world-object
  adapter resolves object/content keys and submits idempotent pet refresh or despawn commands.
  `crossSubsystemOwner: integration-review` applies to Tile/object lookup, object lifecycle and
  entity identity. `HasGardenGnomeNearby` remains a luck/environment handoff and is not absorbed
  into this component.
- Missing, inactive, destroyed or unavailable world objects fail closed. Reset, world change,
  death and disconnect must clear capability state and reconcile any pet command without copying a
  Tile reference or entity ID into Player state. These flags are not durable unlocks.
- Rollback: retain the legacy world-object buff/projectile path if Tile resolution, object
  lifecycle, duplicate refresh, world transition or pet terminal cleanup differs. Verification: no
  focused verifier; affected-project build evidence after this checkpoint, not-run.

### C14 implementation checkpoint (2026-09-12T07:49:14Z)

- Actual component file: `src/Player/Progression/PlayerWorldObjectPetCapabilityComponent.cs`.
- Core fields: the four world-object pet flags; all are resettable capability state with `internal`
  mutation access.
- Dependency impact: Tile/world-object lookup and lifecycle, pet entity refresh/despawn, world
  transition, terminal cleanup and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 22. Component checkpoint C15 - companion capability

`C15 PlayerCompanionCapabilityComponent` owns the 14 companion/pet capability flags in source rows
1020-1021, 1028 and 1030-1040: `companionCube`, `babyFaceMonster`, `snowman`, `dino`, `skeletron`,
`hornet`, `zephyrfish`, `tiki`, `parrot`, `truffle`, `sapling`, `cSapling`, `wisp` and `lizard`.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerCompanionCapabilityComponent.cs`.
- Version4 sets `companionCube` during `UpdateBuffs` at `Player.cs:5320-5323` and sets the remaining
  companion flags through pet buff cases at `:5675-5869`. `ResetEffects` clears the corresponding
  state at `:10420-10428,10568,10590-10594`. These are runtime capability snapshots; the companion
  entity, Projectile identity and buff lifetime remain outside the Player component.
- The component source stores all 14 companion capability flags with implicit `false` defaults. It
  does not allocate, kill or retain companion entities, Projectile IDs or buff instances.
- Proposed writer: `PlayerCompanionCapabilitySystem`; it rebuilds the 14 flags and submits explicit
  spawn/refresh/despawn commands through the pet/companion entity port. The command owner must
  reconcile duplicate refreshes, owner death, disconnect, world change and terminal entity cleanup;
  the component must not directly allocate, kill or retain companion entities.
- A missing content key or unavailable entity owner fails closed and produces no stale companion.
  `companionCube` and the other flags are not durable inventory or unlock state. Any shared
  companion orchestration is `crossSubsystemOwner: integration-review` until one authoritative
  entity-command owner is selected.
- Rollback: preserve the legacy companion buff/projectile path if entity command ordering, duplicate
  suppression, reset, death, disconnect or reconnect behavior differs. Verification: no focused
  verifier; affected-project build evidence after this checkpoint, not-run.

### C15 implementation checkpoint (2026-09-12T07:55:46Z)

- Actual component file: `src/Player/Progression/PlayerCompanionCapabilityComponent.cs`.
- Core fields: the 14 companion/pet flags; all are resettable capability state with `internal`
  mutation access.
- Dependency impact: buff/reset rebuild, companion entity command ownership, duplicate suppression,
  owner death, disconnect/reconnect cleanup and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 23. Component checkpoint C16 - mount and vehicle integration

`C16 PlayerMountVehicleIntegrationComponent` covers the Player-side scalar vehicle interaction state
in source rows 955-960: `onWrongGround`, `onTrack`, `cartRampTime`, `cartFlip`, `trackBoost` and
`lastBoost`. The raw `mount` object in row 961 remains a separate Mount/Minecart integration seam.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerMountVehicleIntegrationComponent.cs`.
- The source core stores six fields with public read access and `internal` mutation access. Boolean,
  integer and floating-point fields use their CLR defaults; `LastBoost` therefore starts at
  `Vector2.Zero`, matching Version4. The component contains no `Terraria.Mount`, Tile, collision,
  vehicle entity, callback or side effect.
- Version4 `Player.cs` resets `onWrongGround` at `:14805-14808`, clears and rebuilds `onTrack` around
  `:17481-17505`, toggles `cartFlip` at `:17510-17521`, derives `cartRampTime` at `:17533-17536`,
  and consumes/resets `trackBoost` at `:11542-11545`. `Minecart.TrackCollision` receives and mutates
  `lastBoost` by reference at `Minecart.cs:566`, while `Mount.Dismount` and `Mount.SetMount` clear
  cart-related values at `Mount.cs:4745-4749` and `:4790-4797`. These facts support a Player-side
  state shape but require an integration-reviewed writer and ordering contract.
- P03 evidence establishes that Mount runtime identity/frame/flight, fatigue/ability, variant and
  drill state belong to the Mount domain components under `src/Player/Mount/Components`. C16 does
  not duplicate those components and does not represent the raw `mount` field. The row-961 object
  remains `blocked; crossSubsystemOwner: integration-review` until a typed Mount runtime handle or
  snapshot protocol is selected.
- Proposed writer: `PlayerVehicleIntegrationSystem` only after the shared owner is selected. Mount
  and Minecart adapters own collision, track sampling, ramp/flip transitions, boost production,
  mount/dismount and object lifetime; Player-facing commands cross that boundary through an explicit
  port. No direct inline tile or physics mutation is allowed from this component.
- Persistence and network projection must not serialize the raw mount object or tick-local collision
  state until a protocol owner and replay contract exist. Invalid track, dismount, owner loss,
  reconnect and boost reset must be deterministic and must not create a second writer. Verification:
  not-run.
- Rollback: remove the six-field component core and retain the existing Mount/Minecart path if the
  owner decision, reset ordering, track collision, boost transition or reconnect replay cannot be
  represented by one explicit boundary. Do not add a placeholder for row 961.

## 24. Component checkpoint C17 - accessory and effect snapshot

`C17 PlayerAccessoryEffectSnapshotComponent` owns the 17 accessory/buff effect fields in source rows
938, 946-954 and 1022-1027, 1029: `brokenMirrorBadLuck`, `flowerBoots`, `fairyBoots`, `hellfireTreads`,
`moonLordLegs`, `deadMansSweater`, `arcticDivingGear`, `coolWhipBuff`, `cobWhipBuff`, `wearsRobe`,
`magicCuffs`, `coldDash`, `sailDash`, `desertDash`, `desertBoots`, `eyeSpring` and `scope`.

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerAccessoryEffectSnapshotComponent.cs`.
- Equipment and functional-equipment writers are evidenced by `Player.cs:6865-7047,8138-8330`;
  selected effect assignments include `desertBoots` at `:8193-8196`, while reset clears the
  snapshot at `:10314-10319,10398-10399,10577-10594`. These fields are rebuilt runtime effects,
  not durable inventory or unlock records.
- `brokenMirrorBadLuck` is distinct from ordinary equipment capability: Version4 reads it in luck
  calculation at `Player.cs:17922-17927`. It therefore carries `crossSubsystemOwner: integration-review`
  for luck/effect ownership and must be handed off through an explicit effect input or event; the
  accessory snapshot must not become the authoritative luck calculator. The other 16 fields are
  equipment/buff runtime snapshots with a proposed equipment-effect rebuild writer.
- The component source stores all 17 effect snapshot fields with implicit `false` defaults. It does
  not calculate luck or mutate equipment, inventory, world, mount, persistence or presentation state.
- Proposed writer: `PlayerAccessoryEffectRebuildSystem`; it resets and rebuilds the snapshot from
  equipment/buff inputs, then publishes a one-way luck/effect handoff for `brokenMirrorBadLuck`.
  It must not mutate world tiles, mount state, inventory, persistence or presentation inline.
- Rollback: preserve the legacy equipment/luck path if any reset/rebuild order, effect stacking,
  luck calculation, reconnect, duplicate input or buff removal differs. Verification: no focused
  verifier; affected-project build evidence after this checkpoint, not-run.

### C17 implementation checkpoint (2026-09-12T08:05:17Z)

- Actual component file: `src/Player/Progression/PlayerAccessoryEffectSnapshotComponent.cs`.
- Core fields: the 17 accessory/buff effect flags; all are resettable runtime snapshot state with
  `internal` mutation access.
- Dependency impact: equipment/buff reset and rebuild ordering, luck/effect ownership for
  `BrokenMirrorBadLuck`, reconnect and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 25. State ownership and side-effect seams

### 9.1 Ports and commands

The following interfaces are proposals only and are not current NLTX types:

```text
status: proposed
IPlayerProgressionCommitPort.Commit(PlayerEntityId, ProgressionMutation) -> CommitResult
IPlayerCapabilityResetPort.ResetRuntimeSnapshots(PlayerEntityId)
IProjectileMinionCapacityPort.Submit(PlayerEntityId, MinionCapacityDelta) -> CapacityCommitResult
IPetEntityCommandPort.Submit(SpawnOrRefreshPetCommand) -> PetCommandResult
IPlayerProgressionPersistenceAdapter.Load(PlayerPersistentId) -> ProgressionLoadResult
IPlayerProgressionPersistenceAdapter.Save(PlayerPersistentId, ProgressionSnapshot)
IPlayerProgressionNetworkProjection.Project(PlayerProgressionSnapshot) -> NetworkProjection
```

Commands carry intent and a source/sequence token. Systems perform validation and commit. A query
never writes a component. A projection never writes simulation state. Persistence, network, logging,
clock access, random selection, item inventory and entity allocation remain adapters or ports.

### 9.2 Entity references and IDs

`PlayerEntityId`, persistent player ID, legacy player slot, network player slot, projectile entity
ID, projectile owner slot, pet content ID and item instance ID are distinct. The P05 report cannot
assign a shared `EntityReference` or `NetworkId` owner; that is
`crossSubsystemOwner: integration-review`. Pet flags are capability keys, not pet entity IDs.

### 9.3 Failure and retry

Progression commits use an expected revision or idempotency token. A stale revision rejects without
partial writes. A duplicate pet refresh command is coalesced by `(player, capability, tick)` and
does not spawn a second entity. A missing projectile owner causes a rejected capacity delta and a
diagnostic event, not a direct Player mutation. Network/persistence adapters retry at their own
boundary and never replay a simulation command without its idempotency key.

## 26. System, Query, Command, Adapter and Projection boundaries

### Proposed systems

| Proposed system (`status: proposed`) | Reads | Writes/outputs | Side effects |
|---|---|---|---|
|`PlayerProgressionCommitSystem`|consume/unlock/quest commands, content and current ledgers|C01, C07, C08; committed progress events|through persistence/network/event ports only|
|`PlayerCapabilityResetSystem`|player lifecycle and reset boundary|clears C02-C05, C09-C15 and C17 runtime snapshots|none beyond explicit component writes|
|`PlayerFishingCapabilityRebuildSystem`|equipment, buff and content queries|C02|none; emits a capability revision|
|`PlayerMinionCapacitySystem`|C03, projectile snapshots and capacity deltas|C03|submits trim/destroy commands, never destroys inline|
|`PlayerMinionCapabilityRebuildSystem`|buff/equipment/content inputs|C04-C05|pet/minion spawn intent only through commands|
|`PlayerMinionDamageTrackingSystem`|owned projectile damage snapshots|C06|updates high-water cache and publishes read-only result|
|`PlayerPetCapabilityRebuildSystem`|buff state, content catalog, lifecycle|C09-C15|submits idempotent pet/companion entity commands|
|`PlayerVehicleIntegrationSystem`|mount/minecart queries and player vehicle commands|C16 only after integration owner is selected|Mount/Minecart ports and collision commands|
|`PlayerAccessoryEffectRebuildSystem`|equipment/buff/environment inputs|C17|publishes luck/effect handoff, no direct world mutation|

### Proposed pure queries

`UsingBiomeTorchesQuery`, `UsingSuperCartQuery`, `ConsumedUpgradeEligibilityQuery`,
`FishingCapabilityQuery`, `RemainingMinionCapacityQuery`, `PetCapabilitySetQuery`,
`MinionDamageHighWaterMarkQuery`, and `VehicleRuntimeQuery` are read-only and deterministic for a
given snapshot. `RemainingMinionCapacityQuery` must clamp or reject invalid negative deltas according
to an integration-approved policy; it must not fix the component itself.

### Proposed commands and events

`ConsumePlayerUpgradeCommand`, `UnlockPlayerProgressCommand`, `RecordAnglerQuestCommand`,
`AccumulateGolferScoreCommand`, `RecordDd2ProgressCommand`, `SubmitMinionCapacityDeltaCommand`,
`RefreshPetCapabilityCommand`, `DespawnPetCapabilityCommand`, and `SetVehicleRuntimeCommand` are
proposed commands. `PlayerProgressionCommittedEvent`, `PetEntityRefreshRequestedEvent`,
`MinionCapacityCommittedEvent`, and `PlayerCapabilityRevisionChangedEvent` are proposed events.

### Adapters and projections

The persistence adapter is the only proposed owner of binary/versioned save format. The network
projection is the only proposed owner of packet field packing. A content adapter maps buff/item/
projectile IDs to capability keys. A pet entity adapter maps capability keys to Projectile/NPC or
companion entity commands. A presentation projection maps committed capability snapshots to UI,
lighting, sound and draw inputs. No external protocol or content prototype type enters the core
components.

## 27. Explicit system order

The scheduler must express this order in a dispatch contract; file or directory order is irrelevant:

```text
1. Resolve player lifecycle, load/reconnect and pending progression commands.
2. Apply accepted persistent progression commits (C01, C07, C08).
3. Reset runtime capability snapshots at the same boundary as Version4 ResetEffects.
4. Rebuild accessory/equipment and fishing capability snapshots (C02, C17).
5. Rebuild minion and pet/companion capability snapshots (C04, C05, C09-C15).
6. Read projectile ownership and submit minion capacity/damage aggregation (C03, C06).
7. Commit minion trim and pet refresh/despawn commands through entity owners.
8. Resolve Mount/Minecart integration commands only after the shared owner is selected (C16).
9. Publish persistence, network and presentation projections from committed revisions.
```

The exact relation between projectile scan and projectile spawn must be recovered before completing
C03/C06 integration. That relation is a blocking integration decision because it changes whether the
capacity aggregate is a scan result, a reservation ledger, or a projectile-owned commit.

## 28. Persistence, network and client projection

Persistent candidates are C01, C07 and C08. The complete reference is the only current evidence for
their binary field order and release gates; Version4 itself is `full-reference-supplemented`, not
closed. C02-C06 and C09-C15/C17 are runtime snapshots/caches by current evidence and must not be saved
unless a future source audit proves a durable contract. C16 remains an unresolved cross-domain vehicle
boundary and is not represented by a component source.

Network evidence confirms selected progress fields and counters in `MessageBuffer`/`NetMessage`.
The network projection must carry a revision and player identity and must not pack runtime pet
flags, minion counts or mount runtime until their cross-domain protocol owners are reviewed.
Client projections are one-way: they cannot write C01-C17. Reconnect restores persistent ledgers,
then rebuilds runtime snapshots from current buffs/equipment/content and reconciles entity commands.

## 29. Focused verifier plan

The temporary `P05ProgressionLedgerVerifier` covered the implemented C01/C07/C08 core paths and
was removed after its recorded build/run evidence. The following are the focused verifiers and
their current evidence:

| Verifier | Required assertions | Status |
|---|---|---|
|`P05ProgressionLedgerVerifier`|C01/C07/C08 isolated commands are idempotent; C07 derived Using queries do not write; Golfer cap and DD2 monotonic progress are covered; versioned load/save round-trips and stale revision rejection remain required|C01/C07/C08 core build/run passed; integration assertions not-run|
|`P05CapabilityResetVerifier`|reset clears C02-C05/C09-C15/C17 runtime flags; rebuild restores only from inputs; no persistent bit is cleared|not-run|
|`P05MinionCapacityVerifier`|projectile reservation/count/slot deltas are single-writer, fractional slots are exact, over-capacity trim is deterministic, owner loss is fail-closed|not-run|
|`P05PetCapabilityVerifier`|each capability group maps to content keys; duplicate refresh is idempotent; death/disconnect/reset clears entity commands correctly|not-run|
|`P05DamageHighWaterVerifier`|C06 resets at the same boundary as Version4, retains max rather than last value, and does not leak across players|not-run|
|`P05VehicleIntegrationVerifier`|Mount/Minecart shared writer, cart boost transitions, invalid track and dismount behavior are resolved without duplicate Player owner|blocked; not-run (C16 owner unresolved)|
|`P05ProjectionVerifier`|persistent network fields use correct revision/order, runtime state is rebuilt after reconnect, projection cannot mutate simulation|not-run|

Future compile-capable commands must use the repository serial wrapper and affected project only.
The command shape is planned, not executed:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\dome\Test\<affected-verifier>\<affected-verifier>.csproj `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

The prescribed positional wrapper spelling was rejected by PowerShell before compilation because
`-p:UseSharedCompilation=false` was ambiguous with the wrapper's own parameters; that invocation had
exit code 1 and did not start `dotnet`. The equivalent wrapper invocation using explicit
`-DotnetArguments` then ran the affected-project build:

```powershell
pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src\\Player\\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
```

The wrapper build exited with code 1, 0 warnings and 5 errors for
`src/Player/Terraria.Player.csproj` targeting `net10.0`. The errors are the known existing
cross-project references in `src/Player/Progression/PlayerMinionCapacityCommitSystem.cs` and
`src/Player/Progression/SubmitMinionCapacityDeltaCommand.cs` (`Terraria.Relationships`,
`Terraria.Projectile`, `EntityReference` and `ProjectileIdentityComponent`). The expected output path
is `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`; a file exists there, but it is not
treated as a verified artifact because this invocation failed. No successful post-checkpoint compile
evidence exists.

The user-forbidden `git diff --check -- <P05 documents>` command was not run.

## 30. Do-not-split decisions

- Do not create one component per pet flag, minion flag, progress bit, quest counter or damage
  event; the selected groups share lifecycle and access patterns.
- Do not merge all pet groups into one `PetFlags` component; boss, seasonal/event, standard,
  crossover and world-object flags have distinct content and integration readers.
- Do not merge persistent ledgers with resettable equipment/buff snapshots.
- Do not place per-projectile `MinionSlots`/`MinionPosition` in Player; retain them on Projectile.
- Do not make `UsingBiomeTorches`, `UsingSuperCart`, remaining slots or effective fishing level
  independent authorities; they are queries/derived values.
- Do not place `Mount` object graphs, minecart collision, tile effects, NPC/pet entities, network
  packet types or persistence streams inside P05 components.

## 31. Compatibility and behavior risks

The compatibility window must preserve the legacy public field names at an adapter while exactly one
new writer owns each migrated field. A field must not be removed until focused coverage proves
normal, reset, reconnect, death, duplicate-command, invalid-owner and versioned load paths.

The main risks are: losing a capability when `ResetEffects` runs before a rebuild; counting a
projectile twice during spawn and scan; treating a transient pet flag as durable ownership; replaying
an old network progress packet over a newer commit; clearing high-water marks at the wrong phase;
and allowing Mount/Minecart to write a Player component while a parallel Player system also writes it.
Rollback for any of these is to restore the prior adapter read path, disable the replacement writer,
retain the evidence, and block the next component.

## 32. Evidence gaps and blocking decisions

`evidence-gap` items are ordinary incomplete evidence and do not stop this design:

- complete pet/minion content registration and every projectile reader/writer are not closed;
- Version4 network coverage for runtime flags is partial;
- full player lifecycle teardown/reconnect behavior is distributed across Player, Main and network;
- current NLTX P05 persistence and projection mapping is partial;
- source-level caller coverage for `HasGardenGnomeNearby` and `brokenMirrorBadLuck` crosses luck and
  environment domains.

The following are `blocking-decision` items because different answers change ownership or ordering:

1. Progress commit root: either (A) one PlayerProgression aggregate commits C01/C07/C08, or (B)
   event-specific systems commit separate ledgers through a shared transaction port. A leaves one
   atomic revision; B preserves domain locality but needs a transaction coordinator.
2. Minion capacity root: either (A) Projectile owns reservations and Player exposes a read-only
   aggregate, or (B) Player owns reservations and Projectile submits commands. A matches spawn
   locality; B matches player capacity semantics. Integration review must choose one writer.
3. Pet entity root: either (A) pet capability systems own refresh commands and Projectile owns
   entities, or (B) a shared companion orchestrator owns both. A is narrower; B may simplify
   duplicate suppression but increases cross-domain coupling.
4. Vehicle root: Mount/Minecart may own C16, Player may own a typed snapshot with vehicle commands,
   or the final design may split the seven fields. The current evidence does not justify choosing
   one unilaterally.
5. Luck/environment root: `HasGardenGnomeNearby` and `brokenMirrorBadLuck` may remain compatibility
   inputs, move to a luck component, or split into world/environment and player effect projections.

## 33. Integration Handoff

```text
subsystemId: P05-Player-Progression-Pets-Minions
taskNumber: P05
reportPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P05-player-progression-pets-minions-component-design.md

evidenceStatus: partial; source-inventory-confirmed; Version4-source-confirmed; full-reference-supplemented; current-NLTX-partial; C01-C02-C07-C08-core-verified; C03-C06-C09-C15-C17-source-implemented-not-verified; C16-six-field-core-implemented-not-verified; raw-mount-blocked
nltxStatus: partial; C01-C02-C07-C08-isolated-core; C03-C06-C09-C15-C17-state-only-components-present; C16-six-field-state-only-core-present; raw-mount-blocked; integration-blocked
verificationStatus: partial

confirmedOwners:
- C01 consumed progression fields are player-scoped persistent candidates.
- C07 unlock/preference fields and C08 quest/event fields are player-scoped persistent candidates.
- C02-C05 and C09-C15/C17 are player-scoped runtime capability snapshots rebuilt by reset/buff/equipment/content inputs.
- C06 is a player-scoped high-water cache whose projectile writer remains integration-review.

implementedTypes:
- state-only Component cores: C01-C15 and C17 are present under `src/Player/Progression/`; C01/C02/C07/C08 have historical focused-core evidence and C03-C06/C09-C15/C17 remain not-verified after the latest source additions
- blocked Component boundary: C16 row 961 `mount` has no source because Mount/Minecart ownership is unresolved; rows 955-960 have a state-only source core

proposedTypes:
- proposed full Component integration: C01-C15 and C17 systems, queries, commands, adapters and projections; C16 six-field writer and raw-mount protocol remain integration-blocked
- proposed System: progression, capability rebuild, capacity, pet and vehicle systems in Section 26
- proposed Query: Using, eligibility, capacity, pet and high-water queries
- proposed Command: progression, capacity, pet refresh and vehicle commands
- proposed Adapter: persistence, content, entity-command and network adapters
- proposed Projection: persistence snapshot, network and presentation projections

sharedTypesForIntegrationReview:
- PlayerEntityId, persistent/network player IDs, projectile owner identity, pet entity reference, content ID and revision
- Mount/Minecart runtime owner and collision/track state
- HasGardenGnomeNearby and brokenMirrorBadLuck luck/environment ownership

crossSubsystemReaders:
- ProjectileSimulation reads minion capability/capacity and may submit count/damage observations.
- FishingAndCatchSimulation reads C02.
- Combat/status and equipment systems read C01/C02/C06/C17-derived values.
- WorldProgression, NPC/DD2 and quest systems read or commit C07/C08.
- Network, persistence and presentation consume projections only.

crossSubsystemWriters:
- ProjectileSimulation may submit C03/C06 deltas but cannot directly mutate the components after owner selection.
- Item/equipment/buff systems rebuild C02/C04/C05/C09-C15/C17 through explicit ports.
- Mount/Minecart systems are candidate writers for C16.

orderingConstraints:
- persistent commits before runtime reset; reset before capability rebuild; rebuild before projectile capacity scan; projections after commit.
- capacity and high-water ownership must be selected before implementing spawn/trim ordering.

boundaryChallenges:
- retain 17 auditable source groups while avoiding one component per flag and avoiding a giant PetFlags component.
- keep runtime flags separate from durable unlocks and from spawned entities.
- resolve existing NLTX duplicate Player capacity and mount prototypes before migration.

evidenceGaps:
- incomplete Version4 pet/minion network and teardown matrix; Version4 persistence stubs; partial NLTX P05 projections; missing final shared owners.

blockingDecisions:
- progression transaction root; projectile/player minion capacity owner; pet entity orchestrator; Mount/Minecart owner; luck/environment owner.

notImplemented:
- C01 persistence/network adapters, legacy-field compatibility handoff, expected-revision conflict handling and effect/event integration remain unimplemented.
- C02 complete fishing-attempt eligibility, rod/bait/pole readers, water/environment rules, source-backed content mapping, bobber Projectile integration, persistence/network adapters, revision handling and compatibility handoff remain unimplemented.
- C07 persistence/network adapters, legacy-field compatibility handoff, expected-revision conflict handling, builder preference mapping, old-inventory fallback and achievement/world effect integration remain unimplemented.
- C08 persistence/network adapters, legacy-field compatibility handoff, expected-revision conflict handling, caller authorization, reward/event ordering and external Angler/Golfer/DD2 integration remain unimplemented.
- C03-C06, C09-C15 and C17 state-only component cores are implemented, but their reset/rebuild, aggregation, content, entity, luck, compatibility, system, query, command, adapter and projection integration remains unimplemented or integration-blocked.
- C16 six scalar Player-side fields are implemented as a state-only component core, but the raw `mount` field remains intentionally unimplemented because no typed Mount/Minecart snapshot protocol or single writer has been selected.
- No P05 test project or permanent focused verifier was added; the temporary C01/C02/C07/C08 verifier runs were removed after the recorded checkpoints.

verifierPlan:
- C01, C02, C07 and C08 temporary focused verifier builds/runs passed; C03-C06 and C09-C15/C17 source cores have no post-checkpoint focused verification; C16 vehicle verification is blocked by unresolved ownership; the permanent P05 verifier and all integration assertions remain not-run.
```

## 34. Final declaration

This report is based on Version4, the complete same-type reference source, the tModLoader public
documentation mirror and limited ECS structure references. C01, C02, C07 and C08 have isolated NLTX core
implementations and focused build/run evidence. C03-C06 and C09-C15/C17 also have state-only component
cores in current NLTX, but they have no post-checkpoint affected-project verification and their integration
remains open. C16 has a six-field state-only core, while its raw `mount` field remains absent because its
Mount/Minecart owner is unresolved. This remains
a partial implementation record, not a migration-completion report, behavior-equivalence proof,
API-compatibility proof, persistence/network closure proof, or claim that the full P05 runtime exists.

## 35. Component Field Audit Checkpoint (historical, 2026-09-12T09:32:40.1761957Z)

- The authoritative P05 report contains 164 field rows. The source audit matched all 157 fields
  assigned to C01-C15 and C17 to the corresponding public component properties with matching C# types;
  mismatch count: 0.
- At this checkpoint, the seven C16 rows were intentionally absent from `src/Player/Progression/`
  because Mount/Minecart ownership and the typed replayable vehicle boundary were unresolved. The
  subsequent Section 36 checkpoint covers six of those rows with a state-only source core.
- This checkpoint changes no source file and is evidence of field coverage only. It does not upgrade
  the existing `not-verified`, integration-blocked or runner-failed states.

## 36. C16 Scalar Vehicle State Checkpoint (2026-09-12T10:03:12.1524519Z)

- The source audit now matches 163 of the 164 authoritative P05 field rows to public component
  properties with matching C# types. The six newly covered C16 rows are `onWrongGround` (`bool`),
  `onTrack` (`bool`), `cartRampTime` (`int`), `cartFlip` (`bool`), `trackBoost` (`float`) and
  `lastBoost` (`Vector2`).
- The actual source file is
  `src/Player/Progression/PlayerMountVehicleIntegrationComponent.cs`. It is a state-only component;
  it does not write collision, Tile, Mount, Minecart, movement, persistence or network state.
- The remaining row is `mount` (`Terraria.Mount`). It is intentionally not represented by a nested
  component, `object`, or guessed handle. P03 Mount evidence owns the runtime Mount boundary, but no
  typed replayable cross-domain protocol has been selected.
- This checkpoint reduces the C16 source-shape gap but does not close the writer, integration,
  serialization, network, persistence or verification gates.

## 37. C16 Affected-Project Verification Checkpoint (2026-09-12T10:32:45.5480656Z)

- The affected project build was run serially through the repository wrapper:

  ```powershell
  pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src\\Player\\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
  ```

- Project: `src/Player/Terraria.Player.csproj`; target: `net10.0`; exit code: `1`; warnings: `0`;
  errors: `5`. The errors are in pre-existing `PlayerMinionCapacityCommitSystem.cs` and
  `SubmitMinionCapacityDeltaCommand.cs` references to `Terraria.Relationships`,
  `Terraria.Projectile`, `EntityReference` and `ProjectileIdentityComponent`.
- The expected output path is
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`. A file exists there, but the
  invocation failed and the file is therefore not trusted as a successful artifact.
- No permanent verifier or non-component workaround was added. C16 remains integration-blocked on
  the raw `mount` field and has no post-change focused verification evidence.
- Runner settlement for the retry session: `Fail`, partition `P05`, session
  `7657bc14436f43bc901049a873107b34`, runner exit code `5`, returned `status: failed`,
  `lockReleased: true`, ledger `exitCode: null`.

## 38. Current Revalidation Checkpoint (2026-09-12T10:57:10.003Z)

- The affected project was rebuilt after the previous checkpoint through the required serial
  wrapper:

  ```powershell
  pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src\\Player\\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
  ```

  It exited `1` with `0` warnings and `5` errors. The errors remain confined to the existing
  `PlayerMinionCapacityCommitSystem.cs` and `SubmitMinionCapacityDeltaCommand.cs` references to
  `Terraria.Relationships`, `Terraria.Projectile`, `EntityReference` and
  `ProjectileIdentityComponent`.
- `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` exists, but its filesystem timestamp
  predates this invocation and the build failed, so it is not verification evidence for this
  checkpoint.
- A read-only boundary audit found only mount-type/control representations in the existing dome
  `PlayerMountStateComponent`, `PlayerStateProjection`, and `LegacyPlayerControlsState`. None is a
  complete `Terraria.Mount` snapshot or a typed replayable Mount/Minecart protocol. The P05 raw
  `mount` row therefore remains blocked and no additional component field is justified.
- No source, report, runner ledger, system, query, command, adapter, projection, test, or project
  file was changed by this revalidation. The prior `Fail` settlement remains authoritative.

## 39. Mount Reference Boundary Audit (2026-09-12T11:02:07.470Z)

- The existing `src/Relationships/EntityReferenceScope.cs` has no `Mount` scope. The separate
  `EntityProvenanceKind.Mount` value in `src/Share/Entity/Components/EntityProvenanceKind.cs` is
  only an attribution/source classification and does not identify or resolve a Mount entity.
- `src/Player/Terraria.Player.csproj` has no project reference to `Terraria.Relationships`, and no
  existing Mount identity, handle, entity relation, or replayable snapshot contract was found in
  the searched `src`, `dome/dome1/src`, or P03 component surfaces.
- Consequently, replacing the authoritative `mount: Terraria.Mount` member with
  `EntityReference`, a provenance enum, `MountType`, or an untyped value would change the member's
  semantics and either create a second authority or require an out-of-scope project/protocol
  change. The raw row remains blocked pending an integration-owned Mount identity and typed
  snapshot protocol.
