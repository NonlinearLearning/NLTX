# System Decomposition Report: authoritative P07

| Field | Value |
|---|---|
| partitionId | P07 |
| taskId | AUTH-SYS-P07 |
| sessionId | 78cc87429a4a4e8e94bf74ca6488cb65 |
| designStatus | proposed |
| migrationStatus | deferred |
| verificationStatus | not-run |
| sourceModified | false |

## Scope and Evidence

This report covers only the claimed P07 inventory input, Player mobility and armor-set state: 11 leaf groups, 112 fields, 0 properties. It does not claim runtime migration, behavior equivalence, a closed network or persistence boundary, or successful implementation.

- Claimed input: docs/migration/ledgers/authoritative-20-partitions/P07-Player-Mobility.md
- Claimed output: docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P07-player-mobility.md
- Claimed prompt: docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P07-player-mobility-public-decomposition.md
- Claim task set and lease: authoritative-system-decomposition, P07, session 78cc87429a4a4e8e94bf74ca6488cb65.
- The claimed prompt contains an older Component-report output path and a drifted input reference. This report follows the current claimed task row and writes only its authorized System output path.

Evidence identity:

- Version4 source checkout: D:\TRbackup\Version4. It is not a Git checkout. SHA-256 of Terraria/Player.cs: E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86; Terraria/Main.cs: 66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520; Terraria.DataStructures/ArmorSetBonuses.cs: E5B960C22E731357D79A444E6438B53055344B394C6C140F4796C8D4136E8DF3.
- Current NLTX checkout HEAD: 865ca66bfec2b8b455aabe3ed29ce0e60a1f9401. Existing local changes were present before this report; this task did not edit src, Test, project files, inputs, prompts, or runner state.
- Read-only Version4 CPG dataset: import complete; 967 shards, 8,166,789 nodes, 71,038,907 edges, 1,317 diagnostics; manifest SHA-256 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364; project fingerprint 521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B. SourceSnapshotId is null, so the index is not cryptographically bound to the checked-out Version4 file hashes above.
- CPG evidence used the ecs-system CpgEvidence.ps1 read-only Query API. Get-CpgTypeSurface for Player is partial at its 200-item budget. Find-CpgCallSites for JumpMovement in Terraria/Player.cs is partial, returns zero facts, and reports NoMatchingFactInScannedScope with ScannedShardCount 0. The source directly shows the call in Player.Update at line 16319, so the CPG zero result is not treated as absence of a caller. Find-CpgCallSites for RefreshDoubleJumps is complete with two confirmed call sites in Player.cs; direct source locations are lines 12286 and 15660. Get-CpgMemberUses for beetleOrbs is complete for the selected Player.cs path with 28 facts, but some facts have AccessMode Unknown and callee alias mutation is not closed.
- SS14 source under C:\Users\shan\Downloads\ECS\space-station-14-master was consulted only as an organization/API reference. It is not evidence for Terraria behavior.

Member scope copied from the claimed P07 inventory:

| Leaf group | Count | Members |
|---|---:|---|
| PlayerBeetleArmorState | 10 | beetleOrbs, beetleCounter, beetleCountdown, beetleDefense, beetleOffense, beetleBuff, beetlePos, beetleVel, beetleFrame, beetleFrameCounter |
| PlayerSolarAndNebulaArmorState | 10 | solarShields, solarCounter, solarShieldPos, solarShieldVel, solarDashing, solarDashConsumedFlare, nebulaLevelLife, nebulaLevelMana, nebulaManaCounter, nebulaLevelDamage |
| PlayerMagnetAndUtilityAccessoryState | 8 | manaMagnet, lifeMagnet, treasureMagnet, chiselSpeed, lifeForce, hasDeadCellsDownDash, calmed, inferno |
| PlayerDashAndGroundTraversalState | 12 | stairFall, outOfRange, sloping, dashType, dash, dashTime, timeSinceLastDashStarted, dashDelay, accRunSpeed, powerrun, runningOnSand, flapSound |
| PlayerRopeAndPulleyState | 10 | ropeCount, cordage, gem, gemCount, ownedLargeGems, meleeEnchant, pulleyDir, pulley, pulleyFrame, pulleyFrameCounter |
| PlayerSlideAndCarpetTraversalState | 10 | sliding, slideDir, snowBallLauncherInteractionCooldown, iceSkate, carpet, spikedBoots, carpetFrame, carpetFrameCounter, canCarpet, carpetTime |
| PlayerWingsAndFlightState | 6 | wingTime, wings, wingsLogic, wingTimeMax, wingFrame, wingFrameCounter |
| PlayerJumpAvailabilityState | 18 | hasJumpOption_Cloud, canJumpAgain_Cloud, hasJumpOption_Sandstorm, canJumpAgain_Sandstorm, hasJumpOption_Blizzard, canJumpAgain_Blizzard, hasJumpOption_Fart, canJumpAgain_Fart, hasJumpOption_Sail, canJumpAgain_Sail, hasJumpOption_Unicorn, canJumpAgain_Unicorn, hasJumpOption_Santank, canJumpAgain_Santank, hasJumpOption_WallOfFleshGoat, canJumpAgain_WallOfFleshGoat, hasJumpOption_Basilisk, canJumpAgain_Basilisk |
| PlayerJumpExecutionState | 11 | isPerformingJump_DownDash, isPerformingJump_Cloud, isPerformingJump_Sandstorm, isPerformingJump_Blizzard, isPerformingJump_Fart, isPerformingJump_Sail, isPerformingJump_Unicorn, isPerformingJump_Santank, isPerformingJump_WallOfFleshGoat, isPerformingJump_Basilisk, isPerformingPogostickTricks |
| PlayerJumpMobilityModifiers | 5 | downDashTime, autoJump, justJumped, jumpSpeedBoost, extraFall |
| PlayerGrappleAndRocketState | 12 | grappling, grapCount, rocketTime, rocketTimeMax, rocketDelay, rocketDelay2, rocketSoundDelay, rocketRelease, rocketFrame, rocketBoots, vanityRocketBoots, canRocket |
| Total | 112 | All members in the claimed P07 inventory; 0 properties |

The inventory locates declarations in Terraria.Player in Terraria/Player.cs, with source rows spanning lines 565 through 2158. It is authoritative for member scope, not for unique runtime ownership or closed reader/writer sets.

## Prior Component Decomposition Reconciliation

The same-partition Component design and execution documents propose Components and C01-C11 System names. They remain proposal/checkpoint documents and do not prove that a P07 behavior System is implemented or that its runtime API is closed.

Current NLTX contains P07 state candidates under src/NSSLC/Component/Player, including:

- Armor: PlayerBeetleArmorStateComponent, PlayerBeetleOrbKinematicsComponent, PlayerSolarArmorStateComponent, PlayerSolarShieldKinematicsComponent, PlayerNebulaResourceStateComponent.
- Accessories: PlayerUtilityCapabilityComponent.
- Jump: PlayerJumpAvailabilityComponent, PlayerJumpExecutionComponent, PlayerJumpMobilityModifiersComponent.
- Mobility: PlayerDashStateComponent, PlayerGroundTraversalStateComponent, PlayerRopeStateComponent, PlayerPulleyStateComponent, PlayerSlideStateComponent, PlayerCarpetTraversalStateComponent, PlayerFlightStateComponent.
- Grapple: PlayerGrappleRelationComponent, PlayerRocketStateComponent, PlayerRocketCapabilityComponent.

A scoped filename and type search did not find a P07 Jump, traversal, flight, grapple, rope, pulley, or armor-set behavior System. Adjacent systems exist, including PlayerInstantMovementAccumulatorSystem and PlayerAccessoryEffectRebuildSystem. The movement accumulator only exposes frame initialization and additive movement accumulation; it does not establish an owner for the complete Player.Update physical movement pipeline. The state Components and adjacent Systems do not prove behavior migration.

The earlier one-System-per-leaf proposal is retained as a candidate list only. The Version4 call and mutation evidence below supports one explicit ArmorSet boundary and a source-ordered Mobility boundary for now; it does not support 11 independent scheduler nodes.

## Conceptual Behaviors

| Behavior | Invariant and observable result | Source evidence | Evidence status |
|---|---|---|---|
| Armor-set timers, tiers, and orbits | Beetle and Solar counters produce tiered Buff state; Beetle orbs and Solar shields carry per-frame orbit state; Solar also changes dash capability. Nebula levels are read through a helper whose body is empty in this checkout. | Player.UpdateArmorSets at Player.cs:9547; UpdateArmorSets_Always_Solar and ApplySetBonus_Solar at 9597-9682; Beetle update and set-bonus methods at 9684-9832; UpdateBuffs_NebulaBuffs at 6227. | Beetle/Solar behavior is confirmed in the checked source; Nebula effect behavior is unknown. |
| Jump eligibility and execution | Extra-jump capability and remaining-use flags are reset, consumed, or refreshed based on controls, movement, mount restrictions, ground/sliding state, and jump transitions. Derived jump height/speed include mount and mobility modifiers. | UpdateJumpHeight at 11319; JumpMovement at 12055; RefreshDoubleJumps at 13789; Update call sites at 15612, 15660, and 16319. | Source behavior is partial because capability inputs and all writers are not closed. |
| Ground traversal | Dash, wall climb or wall slide, slide, carpet, slope/stair and rope/pulley state change player movement state and feed the sequential movement frame. Dash is called twice at distinct positions in Player.Update. | FindPulley at 11373; DashMovement at 12558; WallClimbMovement at 13005; WallslideMovement at 13152; CarpetMovement at 13246; inline pulley/rope logic around 15723-15820; Player.Update calls at 16072, 16332-16341. | Dash and traversal methods are present; CanMoveForwardOnRope at 14774 returns a default bool, so its intended eligibility behavior is unknown. |
| Flight, rocket, and grapple | Wing time and movement, rocket resource/release gates, grappling projectile slots, and mount transitions affect the same player movement frame. Some rocket behavior is inline in Player.Update rather than a named legacy method. | WingMovement at 13431; GrappleMovement at 13655; inline rocket logic around 16347-16504; grapple call at 17092. | Partial. GetGrapplingForces at 13782 returns default values and RefreshMovementAbilities at 13788 is empty. |
| Utility capabilities | Magnet, utility, defensive, and accessory flags describe capabilities consumed by item pickup, jump, combat, and other effects. | The 8 member declarations are in the claimed inventory. | Unknown as a behavior slice: full consumers, writers, reset rules, and effect owners were not closed in this partition. |

## State Ownership and Write Closure

| State slice | Proposed authoritative state writer | Direct readers/writers and effects observed | Closure |
|---|---|---|---|
| Beetle and Solar armor state | PlayerArmorSetSystem owns only Beetle/Solar counters, tiers, and orbit state transitions. Buff, particle, random, and render effects go through explicit effect ports or existing effect owners. | UpdateArmorSets calls the set-bonus evaluator then always-Beetle and always-Solar updates. ArmorSetBonuses invokes ApplySetBonus_Solar, ApplySetBonus_BeetleDefense, and ApplySetBonus_BeetleDamage. Beetle uses Main.rand and both systems update Buff state; Solar spawns Dust and sets dashType. | Partial: direct Player.cs methods are confirmed, but cross-file writes, Buff lifecycle, deterministic random sequence, rendering, replication, and unique ownership are not closed. |
| Jump and flight ability state | PlayerMobilitySystem owns per-frame jump/flight transition state after receiving capability and equipment inputs. | UpdateJumpHeight reads mount/accessory and status modifiers. JumpMovement changes velocity and jump flags. Wing and rocket paths update time and movement gates. | Partial: capability producers and final physical-state writer are unresolved. |
| Dash, slide, carpet, rope, pulley, and grapple state | PlayerMobilitySystem owns these P07 transition fields while processing ordered movement steps. It must not become the owner of shared spatial or Projectile identity invariants merely because it reads them. | Dash, wall, slide, carpet, pulley, and grapple paths consume control, collision, tile, mount, Projectile, and Player state. GrappleMovement assigns Player velocity from GetGrapplingForces results. Pulley logic adds to instantMovementAccumulatedThisFrame. | Partial/unknown: rope and grapple helper behavior is absent; collision, mount, spatial commit, and Projectile relation contracts are outside the closed evidence. |
| Utility and accessory capability flags | No P07 unique writer is assigned. Equipment/Buff rebuild and downstream capability consumers require integration review. | Candidate Component exists in current NLTX, but this report did not prove a complete Version4 writer or reader closure for all 8 fields. | Unknown; do not make PlayerMobilitySystem or a generic accessory System the final owner yet. |
| Shared position, velocity, collision, Buff, NPC, and Projectile effects | crossSubsystemOwner: integration-review | Movement methods mutate velocity/position and call collision/NPC helpers; set-bonus code changes Buff state; grapple paths read Projectile slots and can remove hooks or mount. | Partial. The report proposes ports/composition only; it does not assign an owner across subsystem boundaries. |

The CPG member-use result for beetleOrbs helps locate direct access shapes within Player.cs, but it does not prove one writer or close alias, helper, or external-file effects. Query and Projection types must remain read-only. State restoration must enter through the authoritative writer after validation rather than directly populating Components.

## Boundary Role and Decision

1. Separate the armor-set behavior from the movement tick as one proposed PlayerArmorSetSystem. UpdateArmorSets is a distinct Player.Update stage at line 15348. ArmorSetBonuses directly calls the public Solar and Beetle set-bonus methods (ArmorSetBonuses.cs:49, 425, 432). Keeping Beetle and Solar in one System preserves their shared armor-set stage and Buff/effect integration. Nebula remains a partial behavior because UpdateBuffs_NebulaBuffs is empty. Solar writes dashType and reads dashDelay, so the system contract must publish its result before the mobility consumer runs.
2. Keep P07 movement behavior under one proposed PlayerMobilitySystem runtime participant for now, with conceptual APIs for Jump, GroundTraversal, Flight/Rocket, and Rope/Grapple. The code applies those behaviors through one Player.Update call chain and repeatedly mutates shared movement state. Its actual order is observable in source. Splitting each inventory group into a scheduled System before identifying a unique movement commit owner, collision contract, and valid grapple/rope behavior would create competing writers or extra ordering assumptions.
3. Keep internal behavior slices separate as APIs and component state, but do not add independent scheduler nodes solely because the source inventory has separate leaf groups. This is a System-level partial decision: no new runtime type or schedule node for each Jump, Dash, Flight, Rope, or Grapple group.
4. Leave PlayerMagnetAndUtilityAccessoryState without a P07 runtime owner until its full capability producer/consumer graph is reviewed. Do not absorb it into ArmorSetSystem or MobilitySystem by adjacency.
5. Reject a single P07 mega-Component: current NLTX already has narrower state components, and the behavior and state lifecycles differ. Also reject one System per leaf group for this report: the Version4 source has a shared update frame and the CPG/caller evidence does not establish independently schedulable owners.
6. If later evidence proves a stable intent/commit boundary for a mobility capability, a separate System may be proposed with an explicit input/output and coordinator contract. That is a later design decision, not this report's migration result.

Candidate domain type names are conceptual only: PlayerArmorSetSystem and PlayerMobilitySystem. File placement and namespace are deferred to implementation review; this report does not create directories or claim these types exist.

## System API and Legacy Behavior Mapping

The compatibility target is the observable behavior, state delta, ordering, failure behavior, and effects of the old Player entry points, not method-count or signature identity.

| Legacy entry or inline behavior | Proposed System API composition | Status and compatibility note |
|---|---|---|
| Player.UpdateArmorSets(i) | PlayerArmorSetSystem.UpdateArmorSetFrame(player, armorSetFrame); compose Beetle and Solar updates and set-bonus APIs. | Confirmed entry. Keep the legacy façade until all ArmorSetBonuses callers and any indirect/reflection callers are closed. |
| ApplySetBonus_Solar, ApplySetBonus_BeetleDefense, ApplySetBonus_BeetleDamage | PlayerArmorSetSystem.ApplySolarBonus / ApplyBeetleDefenseBonus / ApplyBeetleDamageBonus, called through the existing armor-bonus integration boundary. | Direct ArmorSetBonuses calls confirmed. Buff and effect outputs need their actual adapters and deduplication semantics. |
| Player.UpdateJumpHeight() | PlayerMobilitySystem.ResolveJumpParameters(player, capabilitySnapshot, mountSnapshot). | Proposed. Do not define capabilitySnapshot as pure until equipment, mount, status, cache, clock, and random reads are closed. |
| Player.JumpMovement() and RefreshDoubleJumps() | PlayerMobilitySystem.ResolveJumpStep(frame) and RefreshJumpAvailability(frame). Use an explicit per-tick movement frame and preserve both RefreshDoubleJumps call sites. | Partial. JumpMovement has contact-attack and immunity behavior; damage application crosses into Combat integration review. |
| Player.DashMovement() at lines 16072 and 16332 | PlayerMobilitySystem.ResolveDashStep(frame, sourcePhase). | Preserve both calls and their positions in the original frame. Do not collapse them into one tick without behavior evidence. |
| WallClimbMovement, WallslideMovement, CarpetMovement | PlayerMobilitySystem.ResolveWallTraversal / ResolveCarpetStep using the same ordered frame. | Proposed, with Mount and Collision inputs unresolved. |
| WingMovement and inline rocket-use logic | PlayerMobilitySystem.ResolveFlightStep and ResolveRocketStep. | Partial. Rocket behavior is inline; capture its full read/write and effect set before extraction. |
| FindPulley, inline pulley/rope block, CanMoveForwardOnRope | PlayerMobilitySystem.ResolvePulleyStep / ResolveRopeEligibility. | Unknown for the eligibility result because the checked helper returns a default value. Preserve the old behavior only when the source behavior is restored/verified. |
| Player.GrappleMovement() | PlayerMobilitySystem.ResolveGrappleStep using a validated Projectile relation API. | Unknown for force calculation. GetGrapplingForces is a stub, so no force, direction, or velocity compatibility claim is possible. |
| P07 utility capability fields | A read-only PlayerUtilityCapabilityQuery may expose committed capability facts after the producer and consumers are identified. | Proposed only. No query purity, reset, or ownership claim is made yet. |

Proposed Queries may answer eligibility or expose committed capability snapshots, but they must not mutate state, refresh caches, advance time, or consume randomness. The System API should call known owners directly; introduce Commands only for existing deferred or cross-owner effects that require that boundary. Candidate outputs include Buff updates, cosmetic effects, NPC hit intents, mount transitions, Projectile relation changes, and spatial movement commits. Their concrete contracts and owners remain integration-review items. The legacy Player surface remains as a compatibility adapter until static and dynamic inbound callers, serialization, and any generated/reflection paths have been checked.

## Call and Dependency DAG

Confirmed source sequence and explicit dependency evidence:

| Edge or order | Evidence | Design constraint |
|---|---|---|
| Main.DoUpdateInWorld -> active Player.Update | Main.cs:11411-11430; Player.Update begins at Player.cs:14789. | Confirms the inspected world-loop entry in this checkout only. It does not close multiplayer roles, alternate entry points, or scheduler configuration. |
| Armor-set update -> movement stages | Player.Update calls UpdateArmorSets at Player.cs:15348, before jump-parameter update and movement calls. | PlayerArmorSetSystem must publish its committed armor state before PlayerMobilitySystem consumes Solar dash capability. |
| ArmorSetBonuses -> Solar/Beetle bonus methods | ArmorSetBonuses.cs:49, 425, 432. | Preserve the public callback boundary until all callers are migrated. |
| Jump parameters -> eligibility refresh and movement | UpdateJumpHeight at 15612; RefreshDoubleJumps at 15660; JumpMovement at 16319; CPG confirms two RefreshDoubleJumps call sites within Player.cs. | Keep state refresh and consumption order; do not treat refresh as a pure read. |
| Pulley and rope checks -> subsequent movement | FindPulley at 15723 and CanMoveForwardOnRope at 15798 in the checked Update body. | Eligibility and tile/movement integration remain blocked by the stub result and world API boundary. |
| First DashMovement -> JumpMovement -> second DashMovement -> WallClimbMovement or WallslideMovement -> CarpetMovement | Player.Update calls at 16072 and 16319-16341. | Preserve exact order and the two DashMovement phases. |
| Flight movement follows ground traversal | Player.Update calls WingMovement at 16393 under its flight conditions. | Preserve its condition and phase; exact host schedule beyond this method is unknown. |
| GrappleMovement follows prior frame logic | Player.Update calls GrappleMovement at 17092. | Preserve source order; force calculation and projectile relation behavior are unknown. |

Proposed dependency graph:

Main world player iteration
  -> Player.Update compatibility/orchestration
     -> PlayerArmorSetSystem
     -> PlayerMobilitySystem.ResolveJumpParameters
     -> PlayerMobilitySystem.ResolveJumpStep and refresh
     -> PlayerMobilitySystem.ResolvePulleyStep
     -> PlayerMobilitySystem.ResolveDashStep (first phase)
     -> PlayerMobilitySystem.ResolveJumpStep
     -> PlayerMobilitySystem.ResolveDashStep (second phase)
     -> PlayerMobilitySystem.ResolveWallTraversal and ResolveCarpetStep
     -> PlayerMobilitySystem.ResolveFlightStep and ResolveRocketStep
     -> PlayerMobilitySystem.ResolveGrappleStep
     -> existing spatial/collision and effect owners (integration-review)

The listed ordering edges are confirmed only inside this Player.Update body. They do not establish parallel safety, host scheduler registration, event order, server/client authority, or a cross-world schedule. Do not encode order through filenames or project item order.

## Lifecycle and Side Effects

Confirmed lifecycle evidence is limited to per-Player update calls and the inspected methods. Some fixed-size arrays have declaration initializers in Player.cs, but that does not close entity creation, reset, death, disconnect, clone, persistence, network, unload, or exception cleanup.

Direct effects observed in Version4 include:

- Beetle random orb movement uses Main.rand; Solar/Beetle logic adds and removes Buffs, Solar creates Dust and uses armor dye shaders.
- Jump and dash paths change velocity and call collision/NPC helpers; jump and dash include NPC contact and immunity effects.
- Wing, rocket, and grapple paths modify movement/resource state and emit or clear visual/audio state.
- GrappleMovement reads Main.projectile slots; other source paths remove hooks or perform mount transitions. Projectile identity validity and terminal cleanup are not established by this partition evidence.
- Pulley/rope logic reads world tile/rope information and contributes instant movement. Collision, tile mutation, and final transform commit semantics require their owning domain APIs.
- UpdateBuffs_NebulaBuffs, GetGrapplingForces, RefreshMovementAbilities, and CanMoveForwardOnRope have empty/default bodies in this Version4 checkout. Their intended side effects and results are unknown.

The proposed System may own P07 transition state, but direct Buff, RNG, Dust/audio, NPC damage, Projectile, Mount, collision, and spatial effects must go through explicit owners or ports selected by integration review. Prediction, replay, effect deduplication, persistence, and network serialization semantics are unknown. Any System-local cache would require a separate lifecycle and invalidation contract before implementation.

## Integration Handoff

All unresolved shared owners use the required marker crossSubsystemOwner: integration-review.

| Handoff | Required decision/evidence |
|---|---|
| Movement commit, position/velocity, collision, world geometry, tile and liquid interaction | Identify the authoritative spatial/physics API, commit point, query/command boundary, and ordering relative to this mobility frame. |
| Jump and dash contact damage, NPC immunity, crit/random outcome | Confirm the authoritative Combat owner, hit intent contract, timing, and whether effects are predicted or committed. |
| Grapple attachment/removal and Projectile slots | Define stable Projectile identity, relation validation, stale-target behavior, count/slot invariants, and cleanup on Player/Projectile lifecycle. The current helper stub blocks a behavior contract. |
| Mount restrictions and transitions | Confirm mount jump/dash/flight capability queries and the commit owner for mount changes triggered during movement. |
| Utility/accessory and rocket capability production | Trace equipment/Buff rebuild inputs and downstream Item, world interaction, lighting, jump, and combat consumers before assigning a writer. |
| Buff, random, dust, sound, and animation output | Select authoritative effect APIs and preserve source tick/random ordering and replay deduplication. |
| Network, persistence, prediction, rollback, and reconnect | Locate actual save/sync/restore entry points and define which values are authoritative, transient, or projected. No closure is established here. |

The integration review must reconcile these seams before implementation splits Components across writers or changes existing Player APIs. This partition does not claim ownership for any other subsystem and does not copy members from another partition.

## Migration Behavior Contract

Any later migration must preserve, for the same input and prior state:

- Input and scope: per-player controls, prior P07 state, equipment/armor/Buff capability facts, mount state, valid Projectile relations, and collision/world observations. The exact source of each input is still partly unknown.
- Output: P07 state delta, player movement state, Buff and combat outcomes, Projectile/mount transitions, and visible effects. Shared output owners are integration-review.
- Ordering: the confirmed intra-Player.Update sequence above, including two DashMovement phases, grounded/sliding refresh behavior, Solar dash state before mobility consumption, and conditional wall/flight steps.
- Errors and terminal conditions: invalid or stale Projectile slots, mount transitions, destruction/disconnect, and collision failure need explicit contracts. Their current failure/retry/idempotency behavior is unknown.
- Determinism and effects: preserve Main.rand use and effect timing or supply equivalent recorded inputs; avoid duplicate external effects under prediction/replay.
- Compatibility: retain legacy public fields and callbacks until direct, indirect, dynamic, reflection, configuration, generated, serialization, and network references are closed.

No item in this contract has been behavior-tested against a new owner.

## Evidence Gaps and Blocking Decisions

- CPG caller coverage is partial for JumpMovement despite a direct source call; the selected CPG shard was not scanned. Zero query results are not absence evidence.
- CPG member uses cover only the selected Player.cs shard and do not establish cross-file writer/reader closure, alias writes, nested callable effects, dynamic dispatch, reflection, or runtime registration.
- CPG source snapshot binding is unknown. The recorded Version4 hashes identify the files inspected, not the imported CPG snapshot.
- Nebula buff update, grapple force calculation, movement ability refresh, and rope-forward eligibility have empty/default implementations in this checkout. Their intended behavior is unknown.
- Unique writers for accessory capability fields, player physical state, and external Buff, NPC, Projectile, Mount, and spatial effects are not established.
- Player spawn/reset/death/unload, save/restore, network replication, prediction, rollback, mult-world isolation, and exception/retry paths remain unknown.
- NLTX has candidate state Components but no located P07 behavior System implementing this source behavior. Component presence is not migration evidence.
- Blocking implementation decisions: choose the physical movement commit owner and coordinator API; close utility capability producers/consumers; define Projectile and rope semantics; assign external effect owners; prove lifecycle and network/persistence behavior. Until those decisions are resolved, keep implementation deferred.

## Verification Plan

designStatus: proposed
migrationStatus: deferred
verificationStatus: not-run

Focused verification to run only after implementation is authorized:

1. Compare jump availability, refresh/consume/reset transitions across control, ground, slide, mount, and extra-jump cases; assert one authoritative writer.
2. Capture both DashMovement phases and surrounding jump, wall, carpet, wing, rocket, and grapple ordering for identical inputs and prior state.
3. Verify Solar/Beetle counter, tier, Buff, random-orb, shield, dash capability, and effect timing; include armor removal and repeated update cases.
4. Exercise pulley, rope, grapple attach/remove, stale Projectile identity, invalid slots, mount changes, and terminal cleanup after the missing helper behavior is specified.
5. Verify unique movement commit ownership, collision outcomes, Combat handoff, prediction/replay effect deduplication, and utility capability reset.
6. Verify create/reset/death/disconnect/reconnect, save/restore, network replication, rollback, and multi-world isolation for every persistent or transient field.

No build, test, or behavior verifier was run for this static report, as requested. The report does not claim migration success or behavior equivalence.

