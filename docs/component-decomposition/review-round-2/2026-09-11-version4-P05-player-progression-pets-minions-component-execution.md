# Version4 P05 玩家进度、召唤物、宠物与伙伴组件执行计划

```yaml
partitionId: P05
sessionId: 7657bc14436f43bc901049a873107b34
inputReport: D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P05-Player-Progression-Pets-Minions.md
componentDesignPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P05-player-progression-pets-minions-component-design.md
componentExecutionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P05-player-progression-pets-minions-component-execution.md
executionStatus: failed
implementationStatus: partial
designStatus: proposed
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: partial
completedComponents: [PlayerConsumedProgressionLedgerComponent, PlayerFishingCapabilitySnapshotComponent, PlayerUnlockProgressionLedgerComponent, PlayerQuestEventProgressComponent]
implementedComponents: [PlayerConsumedProgressionLedgerComponent, PlayerFishingCapabilitySnapshotComponent, PlayerMinionCapacityComponent, PlayerCoreMinionCapabilityComponent, PlayerCrossoverMinionCapabilityComponent, PlayerMinionDamageHighWaterMarkComponent, PlayerLegacyPetCapabilityComponent, PlayerBossPetCapabilityComponent, PlayerSeasonalEventPetCapabilityComponent, PlayerStandardNamedPetCapabilityComponent, PlayerCrossoverPetCapabilityComponent, PlayerWorldObjectPetCapabilityComponent, PlayerCompanionCapabilityComponent, PlayerMountVehicleIntegrationComponent, PlayerAccessoryEffectSnapshotComponent, PlayerUnlockProgressionLedgerComponent, PlayerQuestEventProgressComponent]
blockedComponents: [PlayerMountVehicleIntegrationComponent.mount]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T10:32:45.5480656Z
evidence-gap: C03-C06, C09-C15 and C17 source components are present but have no post-checkpoint affected-project verification; C03 capacity ownership, C04/C05/C10-C14 content registration, C06 Projectile observation provenance, C09 luck/environment ownership, C11 event gating, C13 Chillet/ChilletIgnis mapping, C14 Tile/world-object ownership, C15 companion entity-command ownership, C17 luck/effect ownership, reset/rebuild systems, catalog fail-closed behavior, pet entity orchestration and aggregation ordering remain unverified. C16 now has a state-only source core for six Player-side vehicle fields, but its raw `Terraria.Mount mount` field remains blocked because Mount/Minecart ownership and the typed replayable vehicle boundary are unresolved. Version4 player serialization is supplemented from the complete same-type reference, but no P05 persistence/network adapter is implemented and the C01-C06 plus C09-C15 and C17 legacy Player fields remain behind an unimplemented compatibility handoff. C02 still lacks complete fishing-attempt rod/bait/water/environment readers, source-backed content mapping and bobber Projectile integration. C07 still lacks external builder preference mapping, expected-revision handling and the old-inventory Super Cart fallback. C08 still lacks caller authorization, reward/event ordering, release-gated load/save and network projection; its transient command-token cache is not durable progress.
blocking-decision: C01, C02, C07 and C08 currently use local explicit writers (`PlayerProgressionCommitSystem` for C01/C07/C08 and `PlayerFishingCapabilityRebuildSystem` for C02), but the C01/C02/C07/C08 transaction or lifecycle roots, expected-revision policy, persistence adapters, network projection and legacy-field handoff remain integration decisions. C02 must select the source-backed fishing content adapter and complete fishing-attempt reader contract without copying rod, bait, water, environment or bobber Projectile state into the Player snapshot. C07 must preserve the external builder preference and old-inventory fallback without a second writer. C08 must select the authorized Angler/Golfer/DD2 event ports and preserve reward ordering; the core cannot accept arbitrary network or NPC writes. Integration review must also assign the shared owner for projectile-backed minion/pet entities, companion cleanup, mount/minecart runtime, and luck/environment effects. C03/C06 require a single Player-versus-Projectile aggregation writer; C04/C05/C10/C11/C13 require content-key registries; C14 requires a Tile/world-object and pet-entity handoff; C15 requires an explicit companion entity command owner; C16 requires a single Mount/Minecart writer and typed snapshot boundary; C17 requires a luck/effect owner for `brokenMirrorBadLuck` and derived luck effects.
```

## 1. Plan contract

This is a partial component implementation record. C01, C02, C07 and C08 isolated cores have been saved
and focused-verified. C03-C06 and C09-C15/C17 state-only component cores have also been saved, but their
integration and post-checkpoint verification remain incomplete; C16 has a six-field state-only core,
while its raw Mount object boundary remains blocked by Mount/Minecart ownership.
The document does not claim network replay, save/load replay, behavior equivalence or full P05 completion.

Only the P05 members in the authoritative report may be migrated under this plan. Source paths,
target paths, dependency impact, single-writer decisions and rollback conditions must be recorded
before an implementation commit. The legacy public boundary stays behind an adapter until focused
evidence proves that the replacement writer is complete. No double-write compatibility window is
allowed for authoritative fields.

## 2. Proposed file organization

The paths below remain proposals for the future full migration; the implemented isolated cores are
flat under `src/Player/Progression/` and are recorded in their checkpoints:

```text
dome/src/Terraria.Dome.Simulation/Player/Progression/
  Components/
  Queries/
  Systems/
  Commands/
  Adapters/
  Projections/
```

The implementation must keep one core public type per same-named PascalCase file, use capability or
domain directories, and express runtime order through an explicit scheduler contract. It must not
create generic shared component directories or allow protocol/persistence types into the core state
components.

## 3. Migration safeguards

- Confirm the Version4 source row and all known readers/writers before moving a field.
- Keep C01/C07/C08 persistent ledgers separate from resettable C02-C05/C09-C15/C17 snapshots.
- Keep Projectile-owned minion metadata and entity identity out of Player components.
- Make reset, rebuild, commit, trim, spawn, despawn and projection ordering explicit systems.
- Use idempotency tokens for consume, unlock, pet refresh and minion capacity commands.
- Keep persistence/network I/O behind adapters; projections are one-way and cannot mutate simulation.
- Treat C16, `HasGardenGnomeNearby`, `brokenMirrorBadLuck`, C03 and C06 as integration-review seams.
- If a verifier finds a second writer, changed reset ordering, stale replay, entity leak or field
  loss, roll back that unit and do not advance the next one.

## 4. Ordered implementation units

| Unit | Component | Proposed target boundary | Single writer candidate | Gate before next unit |
|---:|---|---|---|---|
|1|`PlayerConsumedProgressionLedgerComponent`|`Player/Progression/Components` plus persistence/query seams|progression commit system|idempotent consume and versioned round-trip |
|2|`PlayerFishingCapabilitySnapshotComponent`|`Player/Progression/Components` plus fishing rebuild system|capability rebuild system|reset/rebuild and pure fishing query |
|3|`PlayerMinionCapacityComponent`|`Player/Progression/Components` plus projectile capacity port|integration-selected capacity writer|fractional slot/count/over-capacity cases |
|4|`PlayerCoreMinionCapabilityComponent`|`Player/Progression/Components` plus core content capability system|capability rebuild system|all 22 flags reset/rebuild and content mapping |
|5|`PlayerCrossoverMinionCapabilityComponent`|`Player/Progression/Components` plus crossover content adapter|crossover capability system|three flags and content isolation |
|6|`PlayerMinionDamageHighWaterMarkComponent`|`Player/Progression/Components` plus projectile observation port|projectile scan/commit seam|max accumulation, tick reset, player isolation |
|7|`PlayerUnlockProgressionLedgerComponent`|`Player/Progression/Components` plus unlock/query/persistence seams|progression commit system|derived Using queries and save/network fields |
|8|`PlayerQuestEventProgressComponent`|`Player/Progression/Components` plus quest/event ports|quest/event commit system|counter bounds, DD2 transition, persistence |
|9|`PlayerLegacyPetCapabilityComponent`|`Player/Progression/Components` plus pet content system|pet capability rebuild system|21 flags and luck handoff |
|10|`PlayerBossPetCapabilityComponent`|`Player/Progression/Components` plus pet content system|pet capability rebuild system|16 boss mappings and refresh idempotency |
|11|`PlayerSeasonalEventPetCapabilityComponent`|`Player/Progression/Components` plus event content system|event pet capability system|9 seasonal/event mappings and reset |
|12|`PlayerStandardNamedPetCapabilityComponent`|`Player/Progression/Components` plus named pet catalog|named pet capability system|13 mappings and unknown-ID fail-closed |
|13|`PlayerCrossoverPetCapabilityComponent`|`Player/Progression/Components` plus crossover adapter|crossover pet capability system|13 mappings and content isolation |
|14|`PlayerWorldObjectPetCapabilityComponent`|`Player/Progression/Components` plus world-object adapter|world-object capability system|4 mappings and tile/entity handoff |
|15|`PlayerCompanionCapabilityComponent`|`Player/Progression/Components` plus companion entity command seam|companion capability system|14 flags and death/disconnect cleanup |
|16|`PlayerMountVehicleIntegrationComponent`|integration-owned Player/Mount/Minecart boundary|integration-review|single vehicle writer and track/boost replay |
|17|`PlayerAccessoryEffectSnapshotComponent`|`Player/Progression/Components` plus equipment/luck seams|equipment effect rebuild system|17 effects, reset/rebuild and luck split |

The sequence is a dependency plan, not a runtime claim. The full Unit 3 and Unit 6 behavior cannot be
closed without the Projectile integration decision; Unit 16 cannot be implemented without Mount/Minecart
ownership. The state-only component cores for Units 3 and 6 are saved, but these gates remain explicit and
do not imply that a proposed owner already exists.

## 5. Component checkpoints

### C01 - `PlayerConsumedProgressionLedgerComponent`

- `status: implemented-core; integration-blocked`; target:
  `src/Player/Progression/PlayerConsumedProgressionLedgerComponent.cs`.
- Source members: rows 468-473 (`usedAegisCrystal`, `usedAegisFruit`, `usedArcaneCrystal`,
  `usedGalaxyPearl`, `usedGummyWorm`, `usedAmbrosia`).
- Migration unit: define a typed persistent ledger and idempotent consume command; map complete-
  reference serialization through an adapter; route one commit path before removing legacy writes.
- Dependency impact: item consume logic, player stats, network progress and persistence depend on the
  commit result. No item or stat component is owned here.
- Rollback: restore legacy read/write adapter if versioned load, duplicate consume or stat side
  effects diverge; retain the new evidence and stop.
- Actual implementation: C01 isolated core is complete. Added the component, upgrade enum, command
  token, command payload, result/status types, pure eligibility query and commit system under
  `src/Player/Progression/`; no existing API, registration key or unrelated source was changed.
- Core behavior: valid supported upgrades commit once; repeated supported upgrades return
  `AlreadyConsumed`; empty tokens and unknown upgrades are rejected without mutation.
- Verification: temporary `P05ProgressionLedgerVerifier` build exit code 0 with 0 warnings/0 errors;
  run exit code 0 with `P05 C01 verifier passed.`. Build artifacts:
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
  `Build/bin/P05ProgressionLedgerVerifier/Debug/net10.0/P05ProgressionLedgerVerifier.dll`.
- Unverified/blocking: no permanent verifier, save/load adapter, network projection, expected-
  revision conflict policy, legacy-field handoff or item/stat effect integration exists. C01 is
  therefore `implemented-core; integration-blocked`, not a full migration.

### C02 - `PlayerFishingCapabilitySnapshotComponent`

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerFishingCapabilitySnapshotComponent.cs`.
- Source members: rows 613-619 (`fishingSkill`, `cratePotion`, `sonarPotion`, `accFishingLine`,
  `accFishingBobber`, `accTackleBox`, `accLavaFishing`).
- Actual files: `PlayerFishingCapabilitySnapshotComponent.cs`,
  `PlayerFishingCapabilityRebuildInput.cs`, `PlayerFishingCapabilityRebuildSystem.cs`, and
  `FishingCapabilityQuery.cs`.
- Core behavior: the explicit rebuild input writes all seven snapshot fields through one rebuild
  system; reset clears all seven fields; pure queries return the stored skill and capability flags
  without modifying the snapshot or accessing external state.
- Dependency impact: equipment, buff and content adapters remain external input producers; Fishing/
  Catch systems consume the snapshot; bobber Projectile creation, rod/bait/water/environment rules
  and content mapping remain outside this core. Existing partial fishing types were not renamed or
  modified, so their compatibility and overlap review remain open.
- Verification: `P05FishingCapabilityVerifier` build exit code 0 with 0 warnings/0 errors; run with
  `--no-build --no-restore` exit code 0 and output `P05 C02 verifier passed.`. Artifacts:
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
  `Build/bin/P05FishingCapabilityVerifier/Debug/net10.0/P05FishingCapabilityVerifier.dll`.
- Unverified/blocking: complete fishing-attempt eligibility, rod/bait/pole readers, water/environment
  rules, source-backed content adapter, bobber Projectile command integration, revision handling,
  persistence/network projection and compatibility handoff remain unimplemented. C02 is therefore
  `implemented-core; integration-blocked`, not a full fishing migration.
- Rollback: restore the prior partial `PlayerFishingCapability*` read path if reset/rebuild inputs,
  potion/accessory flags or lava-fishing capability diverge; retain the evidence and block the next
  unit.

### C03 - `PlayerMinionCapacityComponent`

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerMinionCapacityComponent.cs`.
- Source members: rows 620-622 (`maxMinions`, `numMinions`, `slotsMinions`).
- The existing component stores the three aggregate fields with the Version4 default capacity;
  the existing query and delta command remain unverified integration seams.
- Dependency impact: buff/equipment rebuilds `maxMinions`; Projectile admission consumes the
  capacity contract; Player cache scan and Projectile spawn must not both increment the aggregate.
- Blocking gate: integration review chooses whether Player or Projectile owns the reservation
  commit. Verification: `P05MinionCapacityVerifier`, not-run.

Checkpoint: component source exists; no focused verifier or post-checkpoint build evidence.

### C04 - `PlayerCoreMinionCapabilityComponent`

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerCoreMinionCapabilityComponent.cs`.
- Source members: rows 623-645, the 22 core summon capability flags.
- The component source stores all 22 capability flags; reset/rebuild, content-key mapping and
  Projectile spawn/refresh/despawn remain external and are not claimed here.
- Dependency impact: Buff/Equipment inputs rebuild these flags; C03 capacity is consulted before
  spawn; C06 owns damage high-water marks for two related minions; Projectile owns entities.
- Blocking gate: recover an authoritative flag-to-content registry and verify all 22 mappings,
  reset behavior and duplicate refresh handling. Verification: `P05CapabilityResetVerifier`,
  not-run.

Checkpoint: component source saved; no focused verifier or post-checkpoint build evidence.

### C05 - `PlayerCrossoverMinionCapabilityComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerCrossoverMinionCapabilityComponent.cs`.
- Source members: rows 647-649 (`deadCellsMushroomBoiMinion`, `palworldCattivaMinion`,
  `palworldFoxsparksMinion`).
- The component source stores the three capability flags with implicit `false` defaults. It does
  not contain crossover content keys, Projectile identity, damage, spawn timing or capacity.
- Migration unit: an explicit crossover content adapter and fail-closed capability query must still
  route refresh/despawn through the shared entity command seam.
- Dependency impact: the adapter depends on content registration; C03 capacity and Projectile
  entity ownership remain external; no crossover flag is persisted without evidence.
- Blocking gate: content registration, reset, reconnect and duplicate-refresh behavior must be
  verified. Verification: no focused verifier; affected-project build evidence after this checkpoint,
  not-run.

### C05 implementation checkpoint (2026-09-12T07:10:17Z)

- Actual component file: `src/Player/Progression/PlayerCrossoverMinionCapabilityComponent.cs`.
- Core fields: `DeadCellsMushroomBoiMinion`, `PalworldCattivaMinion`, and
  `PalworldFoxsparksMinion`; all are resettable capability state with `internal` mutation access.
- Dependency impact: crossover content registration, reset/rebuild ownership, Projectile entity
  commands, duplicate refresh handling and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

### C06 - `PlayerMinionDamageHighWaterMarkComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerMinionDamageHighWaterMarkComponent.cs`.
- Source members: rows 641 and 646 (`highestStormTigerGemOriginalDamage`,
  `highestAbigailCounterOriginalDamage`).
- The component source stores both high-water values with the C# default `0`. It does not own
  Projectile definitions, observation provenance, entity identity or the reset schedule.
- Migration unit: define an immutable Projectile observation and one max/reset writer; keep the
  cache out of persistence, network authority and Projectile definitions.
- Dependency impact: C04 capability flags are separate; Projectile owns observation provenance;
  Player consumes observations only through an integration-approved port.
- Blocking gate: recover scan/spawn ordering and original-damage provenance. Verification: no focused
  verifier; affected-project build evidence after this checkpoint, not-run.

### C06 implementation checkpoint (2026-09-12T07:14:22Z)

- Actual component file: `src/Player/Progression/PlayerMinionDamageHighWaterMarkComponent.cs`.
- Core fields: `HighestStormTigerGemOriginalDamage` and `HighestAbigailCounterOriginalDamage`; both
  are per-tick cache state with `internal` mutation access and zero defaults.
- Dependency impact: Projectile observation provenance, one Player/Projectile aggregation writer,
  tick reset ordering and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

### C07 - `PlayerUnlockProgressionLedgerComponent`

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerUnlockProgressionLedgerComponent.cs`.
- Source members: rows 923-926 (`unlockedBiomeTorches`, `ateArtisanBread`, `unlockedSuperCart`,
  `enabledSuperCart`).
- Actual files: C07 ledger, progression kind, unlock and preference commands, result/status types,
  `UsingBiomeTorchesQuery`, `UsingSuperCartQuery`, and the shared C07 overloads in
  `PlayerProgressionCommitSystem.cs`.
- Core behavior: supported unlocks commit once and repeated unlocks return `AlreadyUnlocked`; empty
  tokens are rejected; Super Cart use is derived from unlock plus enabled preference; the builder
  preference is supplied by the external caller and is not copied into the component.
- Dependency impact: item use, builder preference, vehicle, persistence and network systems consume
  or adapt committed state; no existing public type or registration key was changed, and no item,
  achievement, inventory, packet or save-stream side effect is emitted by the core.
- Verification: temporary `P05ProgressionLedgerVerifier` build exit code 0 with 0 warnings/0 errors;
  run exit code 0 with `P05 C07 verifier passed.`. Artifacts:
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
  `Build/bin/P05ProgressionLedgerVerifier/Debug/net10.0/P05ProgressionLedgerVerifier.dll`.
- Unverified/blocking: expected-revision conflict handling, versioned save/load, packet packing,
  legacy-field compatibility, old-inventory fallback, builder status mapping and achievement/world
  effects remain unimplemented. C07 is `implemented-core; integration-blocked`, not a full migration.
- Rollback: restore the legacy progress adapter if release gates, derived behavior, duplicate
  unlock semantics or external preference mapping diverge; retain the evidence and block the next
  unit.

### C08 - `PlayerQuestEventProgressComponent`

- `status: implemented-core; integration-blocked`; actual target:
  `src/Player/Progression/PlayerQuestEventProgressComponent.cs`.
- Source members: rows 858-860 (`anglerQuestsFinished`, `golferScoreAccumulated`,
  `downedDD2EventAnyDifficulty`).
- Actual files: `PlayerQuestEventProgressComponent.cs`, `RecordAnglerQuestCommand.cs`,
  `AccumulateGolferScoreCommand.cs`, `RecordDd2ProgressCommand.cs`,
  `PlayerQuestEventProgressCommitStatus.cs`, `PlayerQuestEventProgressCommitResult.cs`, and
  the C08 overloads in `PlayerProgressionCommitSystem.cs`.
- Core behavior: valid tokens are required; Angler progress increments once per token; repeated
  Angler and DD2 commands return `AlreadyApplied`; DD2 progress is monotonic; negative Golfer
  deltas are rejected and the accumulated score is clamped at the complete-reference cap of
  `1_000_000_000`.
- Dependency impact: quest/event callers, NPC reward generation, persistence and network systems
  consume or adapt committed progress; no NPC, world, packet or save-stream side effect is emitted
  by this core. The in-memory token cache is only duplicate protection for the current component
  instance and is not a durable persistence or reconnect contract.
- Verification: temporary `P05ProgressionLedgerVerifier` build exit code 0 with 0 warnings/0 errors;
  run exit code 0 with `P05 C07/C08 verifier passed.`. Artifacts:
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
  `Build/bin/P05ProgressionLedgerVerifier/Debug/net10.0/P05ProgressionLedgerVerifier.dll`.
- Unverified/blocking: expected-revision conflict handling, versioned save/load adapters, packet
  packing and authorization, legacy-field compatibility, duplicate reward protection across
  reconnect, event ordering and external Angler/Golfer/DD2 caller integration remain unimplemented.
  C08 is therefore `implemented-core; integration-blocked`, not a full migration.
- Rollback: restore the legacy counter/event adapters if cap, release-gated persistence, network
  order, duplicate reward or world/NPC event behavior changes; retain the evidence and block the
  next unit.

### C09 - `PlayerLegacyPetCapabilityComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerLegacyPetCapabilityComponent.cs`.
- Source members: rows 927-945 and 962-964, the 21 legacy pet/effect flags.
- The component source stores all 21 snapshot flags with implicit `false` defaults. It does not own
  pet entities, Projectile identity, item state or effect calculation.
- Migration unit: split pet capability reads from luck/effect/world-object handoffs while keeping
  one reset/rebuild writer and idempotent entity refresh commands.
- Dependency impact: Buff/ResetEffects feeds the snapshot; pet entities and effect domains consume
  capability projections; WorldObject/Luck integration remains external for `HasGardenGnomeNearby`.
- Blocking gate: assign luck/environment ownership and verify all 21 reset/rebuild/refresh paths.
  Verification: no focused verifier; affected-project build evidence after this checkpoint, not-run.

### C09 implementation checkpoint (2026-09-12T07:16:10Z)

- Actual component file: `src/Player/Progression/PlayerLegacyPetCapabilityComponent.cs`.
- Core fields: the 21 source pet/effect flags, including `HasGardenGnomeNearby`; all are resettable
  snapshot state with `internal` mutation access.
- Dependency impact: Buff/ResetEffects rebuild, pet entity refresh, luck calculation and world-object
  observation remain external. The component owns no Tile, world-object, entity or effect writer.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

### C10 - `PlayerBossPetCapabilityComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerBossPetCapabilityComponent.cs`.
- Source members: rows 981-995 and 1002, the 16 Boss pet flags.
- The component source stores all 16 Boss capability flags with implicit `false` defaults. It does
  not own Boss/NPC state, Projectile identity or entity lifecycle.
- Migration unit: map Boss flags to immutable content keys and submit idempotent refresh/despawn
  commands; do not own Boss/NPC state or Projectile entities.
- Dependency impact: `UpdateBuffs`/reset feed the snapshot; repeated buff calls must coalesce; Boss
  content and death/disconnect cleanup remain external seams.
- Blocking gate: close content mapping and entity orchestration. Verification: no focused verifier;
  affected-project build evidence after this checkpoint, not-run.

### C10 implementation checkpoint (2026-09-12T07:17:30Z)

- Actual component file: `src/Player/Progression/PlayerBossPetCapabilityComponent.cs`.
- Core fields: the 16 Boss pet flags; all are resettable capability state with `internal` mutation
  access.
- Dependency impact: Boss content registration, repeated-buff coalescing, pet entity refresh/despawn,
  death/disconnect cleanup and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

### C11 - `PlayerSeasonalEventPetCapabilityComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerSeasonalEventPetCapabilityComponent.cs`.
- Source members: rows 965-967 and 996-1001, the nine seasonal/event pet flags.
- The component source stores all nine seasonal/event capability flags with implicit `false` defaults.
  It does not own durable DD2 progress, event state, Projectile identity or pet entity lifecycle.
- Migration unit: add event-gated capability rebuild and idempotent pet refresh/despawn commands;
  keep durable DD2 progress in C08.
- Dependency impact: event lifecycle supplies read-only eligibility; reset/buff owns the snapshot;
  Projectile/entity owners handle terminal cleanup.
- Blocking gate: verify event start/end, world change, reconnect, death and duplicate refresh order.
  Verification: no focused verifier; affected-project build evidence after this checkpoint, not-run.

### C11 implementation checkpoint (2026-09-12T07:23:43Z)

- Actual component file: `src/Player/Progression/PlayerSeasonalEventPetCapabilityComponent.cs`.
- Core fields: the nine seasonal/event pet flags; all are resettable capability state with `internal`
  mutation access.
- Dependency impact: event lifecycle gating, reset/buff ordering, pet entity refresh/despawn,
  terminal cleanup and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

### C12 - `PlayerStandardNamedPetCapabilityComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerStandardNamedPetCapabilityComponent.cs`.
- Source members: rows 968-980, the 13 standard named pet flags.
- The component source stores all 13 standard named pet flags with implicit `false` defaults. It
  does not own item ownership, content registry entries, Projectile identity or pet entities.
- Migration unit: define an immutable catalog-revision content mapping and idempotent refresh/
  despawn command path; keep item and entity identity external.
- Dependency impact: buff/reset rebuilds the flags; pet entity ownership and content registration
  remain outside the component.
- Blocking gate: verify all mappings, unknown-key fail-closed behavior, reset, reconnect, death
  and duplicate refresh. Verification: no focused verifier; affected-project build evidence after
  this checkpoint, not-run.

### C12 implementation checkpoint (2026-09-12T07:30:18Z)

- Actual component file: `src/Player/Progression/PlayerStandardNamedPetCapabilityComponent.cs`.
- Core fields: the 13 standard named pet flags; all are resettable capability state with `internal`
  mutation access.
- Dependency impact: catalog-revision mapping, unknown-key handling, reset/rebuild ownership, pet
  entity refresh/despawn and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

### C13 - `PlayerCrossoverPetCapabilityComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerCrossoverPetCapabilityComponent.cs`.
- Source members: rows 1003-1011, 1015-1016 and 1018-1019, the 13 crossover pet flags.
- The component source stores all 13 crossover pet capability flags with implicit `false` defaults.
  It does not own item ownership, Projectile identity or crossover-world entities.
- Migration unit: map crossover buff/content keys through a catalog-revision adapter and submit
  idempotent refresh/despawn commands; keep Projectile identity, item ownership and crossover-world
  entities outside the component. The inspected Version4 path closes Bernie through Caveling and
  Dead Cells/Pufferfish mappings, but does not close Chillet/ChilletIgnis writers; that gap must be
  resolved from source evidence before implementation.
- Dependency impact: `UpdateBuffs` and `ResetEffects` rebuild the snapshot; world change, death,
  disconnect and entity terminal cleanup are external lifecycle seams. Unknown or inactive content
  must fail closed and must not leave stale entities.
- Blocking gate: close all 13 content mappings, especially Chillet/ChilletIgnis, and verify reset,
  duplicate refresh, reconnect, death and catalog revision behavior. Verification: no focused verifier;
  affected-project build evidence after this checkpoint, not-run.

### C13 implementation checkpoint (2026-09-12T07:36:48Z)

- Actual component file: `src/Player/Progression/PlayerCrossoverPetCapabilityComponent.cs`.
- Core fields: the 13 crossover pet flags, including `PetFlagChillet` and
  `PetFlagChilletIgnis`; all are resettable capability state with `internal` mutation access.
- Dependency impact: catalog-revision mapping, especially the incomplete Chillet mappings, reset/
  rebuild ownership, pet entity refresh/despawn and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

### C14 - `PlayerWorldObjectPetCapabilityComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerWorldObjectPetCapabilityComponent.cs`.
- Source members: rows 1012-1014 and 1017, the four world-object-backed pet flags.
- The component source stores all four world-object pet capability flags with implicit `false`
  defaults. It does not own Tile state, world-object identity, Projectile identity or pet entities.
- Migration unit: rebuild the four flags from a source-backed Tile/world-object adapter and submit
  idempotent pet refresh/despawn commands. The component must not own Tile state, object lifecycle,
  Projectile identity or pet entity IDs. `crossSubsystemOwner: integration-review` remains required
  for those boundaries; `HasGardenGnomeNearby` stays with luck/environment integration.
- Dependency impact: `UpdateBuffs` and `ResetEffects` rebuild/clear the snapshot; world transition,
  destroyed objects, death, disconnect and entity terminal cleanup are external seams. Missing or
  inactive objects fail closed.
- Blocking gate: close Tile/object and pet-entity ownership, then verify reset, world transition,
  duplicate refresh, death, disconnect and destroyed-object behavior. Verification: no focused
  verifier; affected-project build evidence after this checkpoint, not-run.

### C14 implementation checkpoint (2026-09-12T07:49:14Z)

- Actual component file: `src/Player/Progression/PlayerWorldObjectPetCapabilityComponent.cs`.
- Core fields: the four world-object pet flags; all are resettable capability state with `internal`
  mutation access.
- Dependency impact: Tile/world-object lookup and lifecycle, pet entity refresh/despawn, world
  transition, terminal cleanup and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

### C15 - `PlayerCompanionCapabilityComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerCompanionCapabilityComponent.cs`.
- Source members: rows 1020-1021, 1028 and 1030-1040, the 14 companion/pet flags.
- The component source stores all 14 companion capability flags with implicit `false` defaults. It
  does not allocate, kill or retain companion entities, Projectile IDs or buff instances.
- Migration unit: rebuild the flags from buff/content inputs and submit explicit companion/pet
  spawn/refresh/despawn commands. The component does not own companion entities, Projectile IDs,
  allocation or terminal cleanup. `crossSubsystemOwner: integration-review` remains until one
  entity-command owner is selected.
- Dependency impact: `UpdateBuffs` writes the flags and `ResetEffects` clears them; death,
  disconnect, reconnect, world change and entity terminal cleanup must be coordinated outside the
  snapshot. Unknown content or missing entity ownership fails closed.
- Blocking gate: choose the companion entity-command owner and verify all 14 flags, reset,
  duplicate refresh, owner death, disconnect, reconnect and cleanup behavior. Verification: no focused
  verifier; affected-project build evidence after this checkpoint, not-run.

### C15 implementation checkpoint (2026-09-12T07:55:46Z)

- Actual component file: `src/Player/Progression/PlayerCompanionCapabilityComponent.cs`.
- Core fields: the 14 companion/pet flags; all are resettable capability state with `internal`
  mutation access.
- Dependency impact: buff/reset rebuild, companion entity command ownership, duplicate suppression,
  owner death, disconnect/reconnect cleanup and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

### C16 - `PlayerMountVehicleIntegrationComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual source target:
  `src/Player/Progression/PlayerMountVehicleIntegrationComponent.cs`.
- Source members rows 955-960 are stored as six public read-only-by-convention properties with
  `internal` mutation access: `onWrongGround` -> `OnWrongGround`, `onTrack` -> `OnTrack`,
  `cartRampTime` -> `CartRampTime`, `cartFlip` -> `CartFlip`, `trackBoost` -> `TrackBoost` and
  `lastBoost` -> `LastBoost`. The scalar defaults match Version4, including `Vector2.Zero` for
  `LastBoost`.
- Version4 evidence: `Player.cs:14805-14808` resets `onWrongGround`; `:17481-17505` clears and
  rebuilds `onTrack`; `:17510-17521` toggles `cartFlip`; `:17533-17536` derives `cartRampTime`;
  `:11542-11545` consumes `trackBoost`; `Minecart.cs:566` receives and mutates `lastBoost` by
  reference; and `Mount.cs:4745-4749,4790-4797` clears cart-related values during dismount or
  mount replacement.
- The source core has no `Terraria.Mount`, Tile, collision, vehicle entity, callback, movement,
  persistence or network behavior. It is only a state shape; `PlayerVehicleIntegrationSystem`,
  Mount/Minecart adapters and the single writer remain outside this checkpoint.
- The row-961 `mount: Terraria.Mount` field remains `blocked; crossSubsystemOwner:
  integration-review`. P03 evidence assigns Mount runtime identity/frame/flight, fatigue/ability,
  variant and drill state to the Mount domain components under `src/Player/Mount/Components`; P05
  does not nest or duplicate that runtime object.
- Blocking gate: select one vehicle writer and verify track loss, ramp/flip transitions, boost reset,
  mount/dismount, invalid owner and reconnect replay without a second Player/Mount/Minecart
  authority. Verification: `P05VehicleIntegrationVerifier`, blocked and not-run until the shared
  vehicle owner and typed replayable Mount boundary are selected.

### C17 - `PlayerAccessoryEffectSnapshotComponent`

- `status: implemented-core; integration-blocked; not-verified`; actual target:
  `src/Player/Progression/PlayerAccessoryEffectSnapshotComponent.cs`.
- Source members: rows 938, 946-954 and 1022-1027, 1029, the 17 accessory/buff effect fields.
- The component source stores all 17 effect snapshot fields with implicit `false` defaults. It does
  not calculate luck or mutate equipment, inventory, world, mount, persistence or presentation state.
- Migration unit: reset and rebuild equipment/buff runtime effects through one proposed equipment
  rebuild system. `brokenMirrorBadLuck` must be handed to a luck/effect owner with
  `crossSubsystemOwner: integration-review`; it must not be merged into a generic equipment flag
  authority. No field is treated as durable inventory or unlock state.
- Dependency impact: equipment writers at `Player.cs:6865-7047,8138-8330`, reset at
  `:10314-10319,10398-10399,10577-10594`, and luck consumption at `:17922-17927` require explicit
  reset/rebuild/luck ordering. The system emits a one-way luck/effect handoff and does not mutate
  world, mount, inventory, persistence or presentation inline.
- Blocking gate: verify all 17 fields, reset/rebuild ordering, effect stacking and removal,
  `brokenMirrorBadLuck` luck calculation, reconnect and duplicate input behavior. Verification: no
  focused verifier; affected-project build evidence after this checkpoint, not-run.

### C17 implementation checkpoint (2026-09-12T08:05:17Z)

- Actual component file: `src/Player/Progression/PlayerAccessoryEffectSnapshotComponent.cs`.
- Core fields: the 17 accessory/buff effect flags; all are resettable runtime snapshot state with
  `internal` mutation access.
- Dependency impact: equipment/buff reset and rebuild ordering, luck/effect ownership for
  `BrokenMirrorBadLuck`, reconnect and compatibility handoff remain external.
- Verification status: not-verified. No focused P05 verifier exists and the affected project has not
  been rebuilt after this checkpoint.

## 6. Proposed execution schedule

The implementation scheduler should make the following ordering explicit:

```text
1. Load persistent Player progress through the adapter and validate revision.
2. Commit accepted progression commands for C01, C07 and C08.
3. Reset runtime snapshots at the ResetEffects-equivalent boundary.
4. Rebuild C02 and C17 from equipment/buff inputs.
5. Rebuild C04, C05 and C09-C15 from buff/content inputs.
6. Resolve Projectile observations and commit C03/C06 according to integration ownership.
7. Submit and commit pet/minion entity refresh, trim and despawn commands.
8. Resolve C16 through Mount/Minecart integration only after owner selection.
9. Emit one-way persistence, network and presentation projections from committed revisions.
```

No file ordering may substitute for this schedule. The exact Version4 relation between projectile
spawn admission, `UpdateProjectileCaches`, pet refresh and terminal cleanup must be preserved by
focused tests before compatibility fields are removed.

## 7. Source-to-target dependency plan

The source boundary is `Version4/Terraria/Player.cs`, with integration readers/writers in
`Projectile.cs`, `MessageBuffer.cs`, `NetMessage.cs`, `Mount.cs` and `Minecart.cs`. The proposed
target is the Player Progression domain plus explicit adapters to Projectile, Fishing, Item,
Equipment, WorldProgression, NPC/DD2, Mount/Minecart, Persistence, Network and Presentation.

No Version4 reference source file is moved in this session. C01, C02, C07 and C08 core files and the C03-C15/C17
state-only component source files are present under the current NLTX Player domain. Each implementation unit must
record source path, target path, namespace/
project reference, dependency impact and rollback condition. Existing NLTX partial
types (`PlayerFishingCapability*`, `PlayerSummonCapacityState`, `PlayerAbilityComponent`, mount
types and projectile minion types) require an overlap review before any new type is introduced.

## 8. Focused verifiers and commands

The design report lists the planned verifier programs in Section 29. The temporary C01, C02, C07 and C08 verifier
runs have completed; no permanent P05 verifier was added. Every compile-capable command in this session was serialized
through the repository wrapper:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\dome\Test\<affected-verifier>\<affected-verifier>.csproj `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

Before each command, active dotnet.exe/csc.exe processes were inspected; the C07 build waited for
unrelated shared MSBuild node processes to finish. The C01, C02, C07 and C08 build commands, projects,
exit codes, warning/error counts and Build/bin artifact paths are recorded in their checkpoints.
Both follow-up verifiers used --no-build --no-restore and exited 0 with their expected passed output.
The prescribed positional wrapper spelling was rejected by PowerShell before compilation because
`-p:UseSharedCompilation=false` was ambiguous with the wrapper's own parameters; that invocation had
exit code 1 and did not start `dotnet`. The equivalent wrapper invocation using explicit
`-DotnetArguments` was then serialized through the wrapper:

```powershell
pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src\\Player\\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
```

It exited with code 1, 0 warnings and 5 errors for `src/Player/Terraria.Player.csproj` targeting
`net10.0`. The errors are the known existing cross-project references in
`src/Player/Progression/PlayerMinionCapacityCommitSystem.cs` and
`src/Player/Progression/SubmitMinionCapacityDeltaCommand.cs` (`Terraria.Relationships`,
`Terraria.Projectile`, `EntityReference` and `ProjectileIdentityComponent`). The expected output path
is `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`; a file exists there, but it is not
treated as a verified artifact because this invocation failed. No successful post-checkpoint compile
evidence exists.
The user-forbidden document git diff check was not run.

## 9. Rollback and completion criteria

Rollback a unit if it introduces a second authoritative writer, changes ResetEffects-equivalent
ordering, loses a release-gated save field, accepts a stale network/progression revision, duplicates
a pet/minion entity, leaks a capability after death/disconnect, or writes through a projection/query.

The remaining plan is implementable only after every member has one owner or an explicit integration-review/
deferred status, all source/target paths are reviewed against ECS file rules, focused verifiers cover
normal/reset/duplicate/invalid/reconnect paths, and serial build/test evidence is recorded. The C01,
C07 and C08 isolated-core gates are satisfied; persistence/network/revision integration and the remaining
component gates are not.

## 10. Integration Handoff

```text
subsystemId: P05-Player-Progression-Pets-Minions
taskNumber: P05
reportPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P05-player-progression-pets-minions-component-execution.md

evidenceStatus: partial
nltxStatus: partial
verificationStatus: partial

confirmedOwners:
- persistent candidates are C01, C07 and C08; runtime snapshots are C02-C05 and C09-C15/C17.
- C03/C06 and the six scalar fields of C16 require integration-reviewed writers; C16 row 961
  `mount` remains a blocked Mount/Minecart boundary.

proposedTypes:
- C01-C17 state-only component cores are implemented in `src/Player/Progression/` where their boundaries
  are independently determinable, but their systems, queries, commands, adapters and projections remain
  proposed or integration-blocked; C16 row 961 `mount` has no source representation.

sharedTypesForIntegrationReview:
- Player/projectile/pet entity identity, persistent/network IDs, Mount/Minecart runtime and luck/environment inputs.

crossSubsystemReaders:
- Projectile, Fishing, Equipment, Combat, WorldProgression, NPC/DD2, Network, Persistence and Presentation.

crossSubsystemWriters:
- item/equipment/buff systems, Projectile observation/commit paths, quest/event systems and Mount/Minecart candidates.

orderingConstraints:
- persistent commit -> runtime reset -> capability rebuild -> projectile aggregation -> entity commands -> projections.

boundaryChallenges:
- avoid duplicate current NLTX Player capacity/mount owners; do not treat runtime pet flags as durable unlocks.

evidenceGaps:
- incomplete Version4 runtime pet/minion network/teardown matrix; Version4 persistence stubs; partial current NLTX mapping.

blockingDecisions:
- progression transaction root, minion capacity root, pet entity orchestration, vehicle root and luck/environment root.

notImplemented:
- C01 persistence/network adapters, legacy-field compatibility handoff, expected-revision conflict handling and effect/event integration.
- C02 complete fishing-attempt eligibility, rod/bait/pole readers, water/environment rules, source-backed content mapping, bobber Projectile integration, persistence/network adapters, revision handling and compatibility handoff.
- C07 persistence/network adapters, legacy-field compatibility handoff, expected-revision conflict handling, builder preference mapping, old-inventory fallback and achievement/world effect integration.
- C08 persistence/network adapters, legacy-field compatibility handoff, expected-revision conflict handling, caller authorization, reward/event ordering and external Angler/Golfer/DD2 integration.
- C03-C06 and C09-C15/C17 state-only component cores are implemented, but their reset/rebuild, aggregation, content, entity, luck, compatibility, system, query, command, adapter and projection integration remains unimplemented or integration-blocked.
- C16 six scalar Player-side vehicle fields are implemented as a state-only core; its raw `mount` field,
  vehicle writer, typed replayable boundary and all integration remain unresolved.
- No permanent focused verifier was added; the temporary C01/C02/C07/C08 verifier runs were removed after the recorded checkpoints.

verifierPlan:
- C01, C02, C07 and C08 temporary focused verifier builds/runs passed; C03-C06 and C09-C15/C17 source cores
  and the new C16 six-field core have no post-checkpoint focused verification; C16 vehicle verification is
  blocked by unresolved ownership; the permanent P05 verifier and all integration assertions remain not-run.
```

## 11. Final declaration

This document is an execution record for the P05 partial component implementation. C01, C02, C07 and C08
have isolated core implementations with focused build/run evidence. C03-C06 and C09-C15/C17 state-only
component cores are present but lack post-checkpoint affected-project verification and integration closure;
C16 has a six-field state-only source core, while the raw `mount` field remains blocked. The record does not claim that the P05 source has been
migrated, that behavior is equivalent, or that network/persistence verification has passed.
`executionStatus: in-progress`, `implementationStatus: partial`, and `verificationStatus: partial` reflect
the current state until this retry session is settled.

## 12. Component Field Audit Checkpoint (historical, 2026-09-12T09:32:40.1761957Z)

- The authoritative P05 report contains 164 field rows. The source audit matched all 157 fields
  assigned to C01-C15 and C17 to the corresponding public component properties with matching C# types;
  mismatch count: 0.
- At this checkpoint, the seven C16 rows were intentionally absent from `src/Player/Progression/`
  because Mount/Minecart ownership and the typed replayable vehicle boundary were unresolved. The
  subsequent Section 13 checkpoint covers six of those rows with a state-only source core.
- This checkpoint changes no source file and is evidence of field coverage only. It does not upgrade
  the existing `not-verified`, integration-blocked or runner-failed states.

## 13. C16 Scalar Vehicle State Checkpoint (2026-09-12T10:03:12.1524519Z)

- The source audit now matches 163 of the 164 authoritative P05 field rows to public component
  properties with matching C# types. The six newly covered C16 rows are `onWrongGround` (`bool`),
  `onTrack` (`bool`), `cartRampTime` (`int`), `cartFlip` (`bool`), `trackBoost` (`float`) and
  `lastBoost` (`Vector2`).
- The actual source file is
  `src/Player/Progression/PlayerMountVehicleIntegrationComponent.cs`. It is state-only and has no
  system, query, command, adapter, projection, collision, Tile, Mount, Minecart, persistence or
  network behavior.
- The remaining row is `mount` (`Terraria.Mount`). It is intentionally not represented by a nested
  component, `object`, or guessed handle. P03 Mount evidence owns the runtime Mount boundary, but no
  typed replayable cross-domain protocol has been selected.
- This checkpoint reduces the C16 source-shape gap but does not close the writer, integration,
  serialization, network, persistence or verification gates.

## 14. C16 Affected-Project Verification Checkpoint (2026-09-12T10:32:45.5480656Z)

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

## 15. Current Revalidation Checkpoint (2026-09-12T10:57:10.003Z)

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

## 16. Mount Reference Boundary Audit (2026-09-12T11:02:07.470Z)

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
