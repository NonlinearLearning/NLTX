# System Decomposition Report: authoritative P10

partitionId: P10  
taskNumber: AUTH-SYS-P10  
sessionId: 7096c7ad8a2645708782f2ca139820d2  
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P10-Player-Input-Control.md  
promptPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-11-version4-authoritative-20-partition-prompts\2026-09-11-version4-P10-player-input-control-public-decomposition.md  
outputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P10-player-input-control.md  
designStatus: proposed  
evidenceStatus: partial  
nltxStatus: partial  
verificationStatus: not-run  
sourceModified: false

## Scope and Evidence

This report covers only authoritative partition P10: 11 leaf groups, 110 fields, 8 properties, 118 members total. The complete named inventory and original declaration coordinates are the claimed input report above; no member from another partition is assigned here.

| Leaf group | Members | Candidate behavior slice |
| --- | ---: | --- |
| PlayerInteractionInputState | 14 | Player interaction/UI flags, item reuse, selection, and mixed combat/NPC facts |
| PlayerControlAndReleaseInput | 20 | Raw controls, release edges, hover intent, and direction-repeat timers |
| PlayerItemUseAndChannelIntent | 16 | Item/tile intent, item-use outcome, reuse, channel, and helper state |
| PlayerInformationWorldAndMovementState | 5 | PVP, movement accumulation, step sound, projectile hit, and actuator lock facts |
| PlayerInformationNavigationAndTimeState | 7 | Navigation accessory capability facts and watch/time input |
| PlayerInformationDetectionAndWiringState | 8 | Detection accessory facts, counter, and wiring visibility |
| PlayerBuilderInteractionDefinitions | 13 | Static builder toggle identifiers and count |
| PlayerSelectionState | 15 | Selected item state and selection radial bindings/mode |
| PlayerInputSyncAndMatch | 14 | Input snapshot, channel projectile expectation, and SetMatch request payload |
| PlayerBuilderOverlayState | 2 | Ruler overlay flags |
| PlayerItemSpaceAndSettings | 4 | Item-space result, derived eligibility, and static dash preference |

Evidence is static and separated by source. Version4 source facts below are checked against files under D:\TRbackup\Version4. That checkout has no Git metadata. The following source hashes identify the files read for this report:

| Version4 source | SHA-256 |
| --- | --- |
| Terraria/Player.cs | E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86 |
| Terraria/MessageBuffer.cs | 0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE |
| Terraria/NetMessage.cs | 87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A |
| Terraria/Main.cs | 66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520 |
| Terraria/Projectile.cs | 97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B |
| Terraria/Wiring.cs | 3D4D4C75B7A002207029294D63554B0BF376A29588AC7A06F62A08CC6A998225 |
| Terraria/WorldGen.cs | A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D |
| Terraria/Tile.cs | 43A473E14DDFA7C74F8691F4E5E30284D79005CB134B3ABD0F6A8FC3900BBFAF |

Read-only CPG Query API evidence came from D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite, initialized and queried through the repository's CpgEvidence.ps1 API. The imported manifest SHA-256 is 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364; the database reports 967 shards, 8,166,789 nodes, and 71,038,907 edges. SourceSnapshotId is null and the adjacent CPG source tree is unavailable, so index facts are not bound to the checked source snapshot. Get-CpgTypeSurface for Terraria.Player was complete for its declaration shard and returned 1,511 direct members. Query status only describes indexed coverage; it does not establish behavior, alias closure, dynamic callers, or runtime order.

Selected Get-CpgMemberUses queries covered all 7/7 requested indexed shards for Player.cs, Main.cs, MessageBuffer.cs, NetMessage.cs, Projectile.cs, Wiring.cs, and Tile.cs. Material results:

- controlUseItem returned 15 facts: one confirmed MessageBuffer write, two confirmed Player writes, and partial/unknown access facts in Player, NetMessage, and Projectile. The selected shard query was complete; not every use had a known read/write role.
- releaseUseItem returned 9 facts, including four confirmed Player writes; five Player operations retained unknown access direction.
- selectedItemState returned 5 facts, including a confirmed Player write; access direction for its other Player and MessageBuffer operations was partial.
- Player.channel returned 28 facts across Player, MessageBuffer, and Projectile. Same-name symbols are not merged: another channel field is declared in the same source shard.
- nearbyActiveNPCs returned 12 facts across three requested shards: a confirmed Main write, two confirmed NPC read/write facts, and nine partial/unknown NPC operations.
- lastCreatureHit returned confirmed writes in Player and Projectile.
- InfoAccMechShowWires returned two Player writes; the selected Wiring shard added no resolved use fact. This is not proof that Wiring has no reader.
- instantMovementAccumulatedThisFrame and accWatchTime returned NoMatchingFactInScannedScope for the selected paths with partial status. These results are not negative-use proof.

Get-CpgCallableFacts for ItemCheck, ItemCheckWrapped, ResetEffects, SetMatch, StartChanneling overloads, TryUpdateChannel, and TryCancelChannel was partial with CalleeEffectsNotExpanded. Empty DirectCallTargets does not prove that no helper or runtime call exists. Find-CpgCallSites for TryUpdateChannel in Projectile.cs was partial with zero facts; source fallback confirms Projectile.cs:10514 calls it. TryCancelChannel had one indexed Projectile call and source fallback confirms the kill path at Projectile.cs:46443. The query gaps and source checks are both retained.

The SS14 download at C:\Users\shan\Downloads\ECS\space-station-14-master was read only as an organizational comparison. SharedMoverController.Input.cs (SHA-256 F87D83C1E89CF4F187F228547D0D5487194470AD48A98860012819C94B1CEB6A) and SharedMoverController.cs (SHA-256 E1DEC3E8BA3488738A0AF7247323E9320913C8B92C5435C0E58FF0E2B0311) illustrate how one System boundary can coordinate input state with callbacks/effects. Their organization does not establish Terraria ownership or behavior.

Current target evidence is from D:\TRbackup\NLTX\src\NSSLC\Component\Player. Static search found isolated cores for raw control, release/repeat, item-use intent, channel matching, navigation and detection instrument facts, movement accumulation, selection Query, input projection, builder definitions/overlay, and item-space evaluation. The prior P10 Component execution records cross-subsystem integration as pending. Current references found for several cores are in their own definitions and verification harnesses; this report found no evidence that these cores are wired into the production Player tick, packet adapter, or scheduler. This is a bounded static search result, not proof of global absence.

No build, test, runtime verifier, or migration behavior check was run.

## Prior Component Decomposition Reconciliation

The same-partition Component design is proposed and partial. Its execution document reports blocked/in-progress integration and a partial implementation; it records isolated cores and historical focused-verifier results, but also lists protocol, scheduler, owner, lifecycle, and cross-partition work as pending. Those records are not treated as current runtime integration or migration verification.

The prior C01-C13 grouping is a useful inventory, not 13 independent System boundaries. Reconcile it as follows:

- Keep raw input application and release/repeat transitions as separate candidate behavior owners, with releaseUseItem and releaseUseTile excluded from a generic release writer because Player.ItemCheck and tile interaction paths also mutate those facts.
- Keep item-use execution as one synchronous coordinator boundary around the legacy ItemCheck behavior. The existing item-use intent core covers only an isolated state transformation, not item effects or execution.
- Treat channel projectile matching as an ItemUse lifecycle adapter. Projectile owns projectile lifecycle; this report does not propose a second channel-state authority or a stored Projectile reference.
- Keep inventory selection transitions separate from SelectionRadial UI binding state. The unique selected-item owner must be resolved with the adjacent P09 selection authority.
- Keep navigation accessory facts distinct from detection facts. Treat wiring visibility as an integration seam with Wiring/UI; it is not a reason to combine navigation, detection, and Wiring into one System.
- Keep builder identifiers/catalog and ruler overlay as read-only definition/query and one-way projection boundaries.
- Keep item-space evaluation as a Query and DashControl as a settings adapter. Neither is a player-tick System by itself.
- Route hostile/team/NPC pressure, instant movement, step sound, lastCreatureHit, and ActuationRodLock to their behavior owners only after adjacent subsystem review.

Existing files such as PlayerRawControlInputSystem.cs, PlayerReleaseAndRepeatSystem.cs, PlayerItemUseIntentSystem.cs, PlayerNavigationInstrumentSystem.cs, PlayerDetectionInstrumentSystem.cs, and PlayerInstantMovementAccumulatorSystem.cs contain local computations. Their existence does not establish a runtime scheduler node or unique production writer.

## Conceptual Behaviors

Conceptual behavior is based on state transition and observable effects, not the count of fields or methods.

| Behavior | Source evidence and invariant | Proposed role |
| --- | --- | --- |
| Capture player controls | Packet 13 decodes control bits and other player state; control readers are spread across movement and action code. Input values are scoped to a player slot and a tick. | Raw-input System and protocol Adapter, with identity, sequence, authority, and scheduler contract pending integration review. |
| Advance release and repeat state | Player update logic consumes release flags and left/right timers. ItemCheck writes releaseUseItem; UpdateReleaseUseTile writes releaseUseTile after ItemCheckWrapped. | Separate movement/control edge System; item and tile release facts remain with their owning behavior. |
| Execute item-use behavior | ItemCheck resets pending reuse, gates on CCed, reads selection/control/release, mutates animation/time/channel/reuse, and invokes item actions. | One ItemUse execution/coordinator System; do not absorb every input or interaction field into it. |
| Select inventory item | SelectedItemState tracks selected, hotbar, buffered, and overridden values; local and remote selection have different behavior. | Candidate selection state owner, subject to P09's unique-authority decision. |
| Maintain radial selection bindings | SelectionRadial holds mode, bindings, selected binding, and count; no proof it is the inventory selection owner. | UI/input-local state; request handoff to inventory selection through an explicit API. |
| Derive navigation facts | Accessory application/reset produces compass, watch, depth, weather, calendar, and stopwatch facts. | Navigation capability System or existing equipment-effect owner; time snapshot production and consumption remain unresolved. |
| Derive detection facts | Accessory application/reset produces fish, jar, third-eye, ore, critter, and dream-catcher facts. | Detection capability System or equipment-effect owner; its consumers remain unresolved. |
| Toggle builder options | BuilderAccToggleIDs is static identity/catalog data with a fixed Count; builderAccStatus is player state with additional ownership not established here. | Definition/Catalog Query plus the existing authority that changes builder status. |
| Project builder overlays | rulerGrid and rulerLine reflect builder option state. | One-way overlay projection; no UI writeback. |
| Synchronize input and match requests | PlayerInputSyncCache copies five controls; SetMatchRequest is a short value payload for static match calculation. Packet 13 also carries non-P10 movement, mount, and player state. | Input projection and network Adapter; SetMatch is a pure-query candidate with a compatibility facade. Do not make the packet or request an authority. |
| Evaluate item-space and settings | ItemSpaceStatus is a result with a derived personal-inventory eligibility property; Settings.DashControl is static configuration. | Item-space Query and settings Adapter; persistence and configuration scope remain unknown. |

The 14-member interaction group is not one coherent owner: team, aggro, nearbyActiveNPCs, sign, interaction UI flags, item reuse, selected-item state, nameLen, changeItem, and selectedKite cross combat, NPC, UI, identity/definition, item-use, selection, and equipment behavior.

## State Ownership and Write Closure

Confirmed local facts:

- MessageBuffer.cs packet 13 writes controlUp/down/left/right, controlJump, controlUseItem, controlUseTile, controlDownHold, hover intent, and other fields, and invokes selectedItemState.Select. The same packet also writes position/velocity, mount, ghost, petting/sitting/sleeping, camera, and other state outside P10. NetMessage.cs packet 13 serializes these values. Packet ownership is a shared protocol contract, not a P10-only System boundary.
- Player.SelectedItemState.Select handles remote selection by assigning selected directly. Local selection may reject an empty non-hotbar slot, buffer a change, clear buffer/override when reselecting the current slot, reset AFK counters, and play a sound when the override differs. These are observable behavior constraints, not merely field assignments.
- Player.Update, in the inspected path, advances movement/collision work, calls ItemCheckWrapped at Player.cs:17661, then PlayerFrame, and later UpdateReleaseUseTile at :17678. Preserve this observation point; source does not establish the surrounding Main tick phases.
- ItemCheck directly changes pendingItemReuse, channel, item animation/time, and releaseUseItem; auto-reuse helpers can also change controlUseItem. It evaluates buffered selection, item/tile controls, and release state before applying item actions. A pure intent System cannot replace this execution closure.
- StartChanneling(Item) records the expected projectile type. TryUpdateChannel records the spawned projectile index for the expected type. TryCancelChannel clears channel only when the tracked key matches, with the aiStyle 99 / ai[0] == -3 exception. Projectile.cs has both the tracking and cancellation handoffs described above.
- Main.cs resets nearbyActiveNPCs; NPC.cs reads and mutates it during NPC spawn-pressure calculations. The complete unique writer/schedule contract is not closed by this partition.
- Player.cs and Projectile.cs both have confirmed writes to lastCreatureHit. Its final owner is a combat/projectile integration decision.
- Player.cs writes accessory instrument facts while applying accessory effects and resets many such facts in ResetEffects. accWatchTime has no resolved member-use fact in the selected CPG scope; its clock input, consumers, and lifetime are unknown.
- The current NLTX PlayerSelectionQuery, BuilderOverlayProjection, InputSyncQuery/Projection, and ItemSpaceQuery are read/compute shapes. They are not writers or runtime adapters by their type names.

The query API did not close all readers or writers. Assignment shapes remain Unknown for many uses; CPG callable facts do not expand helper effects; source searches show only the inspected code paths. Local input event registration, all protocol call paths, reflection/configuration, persistence, exceptions, spawn/reconnect cleanup, and dynamic dispatch remain partial or unknown.

Ownership decisions:

- Raw control facts may be committed by one validated input boundary. The current network decoder is a confirmed writer for remote controls; the complete local input writer and sequence/replay policy are unknown.
- Direction/jump/hover/dash release state and repeat timers should have one owner, but ItemCheck remains responsible for releaseUseItem and the tile interaction path for releaseUseTile. PlayerReleaseAndRepeatSystem currently models all of them together; split or coordinate those API writes before claiming unique ownership.
- controlUseItem is shared between packet decode and ItemCheck auto-reuse behavior. One owner or explicit same-frame coordination is required; two independent writers are not acceptable.
- selected item state must have one owner shared with P09 integration. P10 does not decide whether the current P09 SelectedItemComponent or a P10 System becomes authoritative.
- team, hostile, aggro, nearbyActiveNPCs, lastCreatureHit, instantMovementAccumulatedThisFrame, hermesStepSound, ActuationRodLock, InfoAccMechShowWires, selectedKite, and accWatchTime have cross-domain or incomplete ownership. For each, crossSubsystemOwner: integration-review.
- No concurrent schedule is proposed. Same tick order, visibility, and barriers outside the local Player.Update order are unknown.

## Boundary Role and Decision

| Candidate boundary | Decision | Reason and proposed target |
| --- | --- | --- |
| Raw control capture | Keep as a narrow System boundary; current target core exists at src/NSSLC/Component/Player/PlayerRawControlInputSystem.cs. | Own validated raw control commit only. Network/player identity Adapter and production writer connection remain proposed. |
| Control release/repeat | Separate from raw input capture; current isolated core exists at src/NSSLC/Component/Player/PlayerReleaseAndRepeatSystem.cs. | Own only confirmed movement/control edge and timer transitions. Split item/tile release ownership before integration. |
| ItemUse execution | Keep one synchronous boundary; proposed execution surface at src/NSSLC/Component/Player/PlayerItemUseExecutionSystem.cs. | ItemCheck is a mixed behavior coordinator with ordered state and effects. Existing PlayerItemUseIntentSystem.cs is not an execution replacement. |
| Channel lifecycle bridge | Keep as Adapter inside the ItemUse composition; current pure matching core exists at src/NSSLC/Component/Player/PlayerChannelCancellationAdapter.cs. | Projectile type/index matching is a cross-domain handoff. Do not store live Projectile references or introduce a second writer. |
| Inventory selection | Separate state machine; proposed owner/API is conditional on P09 integration. Current PlayerSelectionQuery.cs is a pure query only. | SelectedItemState has its own buffer/override/local-vs-remote rules. SelectionRadial remains UI-local. |
| Navigation and detection instruments | Keep as two separate capability fact boundaries; isolated cores exist in PlayerNavigationInstrumentSystem.cs and PlayerDetectionInstrumentSystem.cs. | Their capability sets and queries differ. Equipment application, frame timing, actual consumers, and writer registration remain integration seams. |
| Movement/world fields | Do not attach to a general input System. Reuse the movement owner for accumulator; route hit, PVP, sound, and wiring facts to their distinct owners. | Current P10 source inventory groups different invariants; type and file co-location is not ownership evidence. |
| Builder definitions and overlay | Keep as Definition/Catalog Query and one-way Projection; current files exist in PlayerBuilderInteractionCatalog.cs and PlayerBuilderOverlayProjection.cs. | Static identifiers and overlay output do not require an update System. builderAccStatus commit owner is outside this decision. |
| Input sync and SetMatch | Adapter/Projection plus pure-query candidate, not an authority System. | Preserve packet 13 compatibility and the public SetMatch request/result contract. Network schema and appearance owner are cross-domain. |
| Item-space and DashControl | Query plus Settings Adapter; current isolated shapes exist. | ItemSpaceStatus is a result, and DashControl is static settings. Inventory/VoidVault commit and preference persistence are not P10 owners. |

Rejected alternatives:

- One PlayerInputSystem owning every P10 member would combine interaction, combat, movement, accessory effects, item execution, UI selection, networking, and settings without a common invariant.
- One System per Component/leaf group would turn static definitions, snapshots, projections, value payloads, and derived results into unnecessary scheduler nodes.
- Treating the existing isolated System-named static classes as production owners would claim runtime wiring that was not observed.
- Moving ItemCheck side effects to a generic event queue would change same-frame execution, failure visibility, and order without source evidence or need.

## System API and Legacy Behavior Mapping

| Legacy entry or behavior | Proposed composition | Compatibility and open contract |
| --- | --- | --- |
| MessageBuffer packet 13 input decode | Protocol Adapter validates packet/session/player identity; dispatches raw control, selection, item-use, and other state to their owners; NetMessage projection serializes the same wire shape. | Preserve the self-player/ServerSideCharacter branch, field defaults, conditional velocity/mount/camera data, and response behavior. Per-session sequence, replay, authorization, and packet failure semantics are unknown. crossSubsystemOwner: integration-review. |
| Player.Update -> ItemCheckWrapped -> ItemCheck | Synchronous PlayerItemUseExecutionSystem.Execute with explicit input facts and existing owner queries/ports; keep the legacy facade until real callers are moved. | Preserve the call point, CCed early exit, selected/buffered gate, mount/kite paths, reuse, mana, animation, item actions, channel, release update, and all effects/errors. Existing CPG callable facts are partial. |
| SelectedItemState.Select and selection properties | Selection owner applies Select intent; PlayerSelectionQuery returns Selected, Hotbar, buffer/override status, and derived immediate-change eligibility. | Preserve local/remote distinction, empty-slot behavior, sound, AFK counters, buffered application, and override reset. ItemUse state is read through a query; the unique owner with P09 is undecided. |
| Player.StartChanneling / TryUpdateChannel / TryCancelChannel | ItemUse owner records expectation; Projectile Adapter submits immutable spawn/termination facts; channel owner decides the matching transition. | Preserve exact projectile type/index matching and the aiStyle exception. Lifecycle delivery and duplicate callback behavior are unknown. crossSubsystemOwner: integration-review. |
| PlayerInputSyncCache construction and PressingAnyInput | Pure PlayerInputSyncQuery builds an immutable projection; Network Adapter writes packet 13 from the committed snapshot. | Preserve the five-field derivation. The cache itself does not authorize writes to movement state. Packet ownership remains shared. |
| Player.SetMatch(SetMatchRequest, ref bool) | Pure match Query candidate behind the existing static facade; adapter maps legacy arguments/output. | Preserve returned slot and ref output behavior. CPG body facts are partial, so full purity and every caller are not confirmed. |
| Player.ItemSpace overloads and ItemSpaceStatus | ItemSpace Query consumes explicit inventory, candidate, and VoidVault facts; inventory owner performs any commit. | Preserve personal-inventory vs VoidVault distinction. Current input snapshot producers, inventory owner, and full source behavior are not closed. |
| BuilderAccToggleIDs, builderAccStatus, rulerLine/rulerGrid | Catalog Query exposes stable definitions; overlay projection reads committed builder facts; existing builder owner writes state. | Preserve identifier values and Count. Definition versioning and actual Wiring/UI consumer handoff remain unknown. |
| Navigation/detection accessory updates and ResetEffects | Separate capability fact systems consume accessory facts and reset at the same equipment frame boundary; consumers read snapshots. | Preserve reset-before-apply semantics. Equipment iteration order and time/detection inputs are not fully evidenced. |
| Player.Settings.DashControl | Settings Adapter exposes read/write preference; input/dash decision code reads it. | Preserve enum/default and static scope until user/profile persistence is traced. Persistence is unknown. |
| instant movement accumulation and other world/combat facts | Delegate to movement, projectile/combat, audio, and Wiring owners through typed snapshot/command seams. | No P10-wide API is proposed for these facts. Owners and exact commit points require integration review. |

No queue, event bus, retry policy, or generic coordinator is required by the inspected synchronous paths. Introduce such a mechanism only if the actual scheduler or protocol contract requires it.

## Call and Dependency DAG

Confirmed local ordering:

- Packet 13 decode assigns fields and calls selection before its handler returns; the packet includes state beyond P10. NetMessage packet 13 is the corresponding serializer. Ordering between receive dispatch and the global player tick was not established.
- Within Player.Update, the inspected path advances movement/collision work, calls ItemCheckWrapped, calls PlayerFrame, then calls UpdateReleaseUseTile. Preserve this local must-before/must-after relation.
- ItemCheckWrapped synchronously calls ItemCheck. ItemCheck gates use on current control/release/selection state, performs the item-use path, advances animation/time, updates releaseUseItem, and may update channel/reuse state. Do not schedule it after a deferred input queue.
- Projectile channel tracking and cancellation callbacks cross from Projectile lifecycle into Player channel state. The source confirms those edges but does not establish their global tick position relative to Player.Update.
- Accessory application writes navigation/detection facts; ResetEffects clears many such facts. Equipment traversal and all readers are not closed.
- nearbyActiveNPCs is reset in Main and read/mutated in NPC. NPC spawn calculations consume this value. The full global phase/order and concurrent visibility contract are unknown.

Proposed composition only:

- Network/local adapters -> validated input facts -> raw input owner -> control/movement, interaction, selection, or ItemUse consumers through read APIs.
- Control input -> movement edge/repeat transition; ItemUse and tile interaction keep their own release gates.
- ItemUse call point -> synchronous ItemUse execution -> typed projectile/world/inventory/animation effects through their actual owners.
- Accessory/equipment facts -> separate navigation and detection updates -> immutable consumers' snapshots/projections.
- Definitions and Queries -> read-only consumers; Projections -> network/UI output after their owning state is committed.

This is not a proven scheduler DAG. Phase registration, event subscription order, barriers, cross-world execution, and parallel access declarations are unknown. No ordering is inferred from source file order.

## Lifecycle and Side Effects

- Construction: Player initializes SelectedItemState with selected/hotbar zero and buffered/overridden unset. Constructor/reset behavior for all P10 members and slot reuse is not closed.
- Input frame: control and release state are consumed across Player movement and item/tile paths. instantMovementAccumulatedThisFrame is reset in Player.cs and incremented in movement logic; the consumer and integration schedule need confirmation.
- Item-use frame: ItemCheckWrapped is synchronous at the observed Player.Update point. ItemCheck mutates action/reuse/channel fields and invokes effects. It also changes releaseUseItem; moving it changes visibility and repeated-use timing.
- Equipment frame: ResetEffects clears navigation/detection and other capability facts before accessory effects can set them. Preserve the actual equipment-phase ordering when integrated; full phase order is unknown.
- Selection: local selection may buffer rather than immediately switch; remote selection assigns directly. Selection can play a sound and reset AFK counters. Network/UI callers and failure delivery remain partial.
- Channel lifetime: expected projectile type is recorded on channel start, matching spawn supplies an index, matching kill may cancel. The channel owner must survive the source lifecycle and must not retain a stale entity reference.
- Synchronization: packet 13 serializes and deserializes several P10 and non-P10 facts in a single compatibility envelope. Versioning, duplicate/reordered packet policy, save/load ownership, and disconnect/reconnect cleanup are unknown.
- Settings: DashControl is static, so player, process, profile, and world scope cannot be assumed equivalent. Persistence and unload semantics are unknown.
- Errors, retry, and idempotency: no new retry, compensation, or idempotency behavior is proposed. Item effects, network failure, audio, and partial-commit exception behavior need source/callee/runtime review.
- Multi-world/session scope: legacy player slots and whoAmI do not prove an ECS entity/session mapping or multi-world isolation. This mapping is an integration decision.

## Integration Handoff

subsystemId: authoritative-P10-player-input-control  
taskNumber: AUTH-SYS-P10  
reportPath: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P10-player-input-control.md  
evidenceStatus: partial  
nltxStatus: partial  
verificationStatus: not-run

confirmedOwners:

- Version4 packet 13 decode/encode directly carries multiple control, selection, item-use, and non-P10 movement/session facts through MessageBuffer and NetMessage.
- Player.Update's inspected local order places ItemCheckWrapped before PlayerFrame and UpdateReleaseUseTile.
- SelectedItemState.Select contains distinct local/remote selection transitions and observable sound/AFK effects.
- ItemCheck synchronously coordinates item eligibility/action, reuse, animation/time, channel, and release behavior; its full helper effect closure remains partial.
- The current NLTX directory contains isolated P10 input, selection query, capability, projection, adapter, and item-space cores; production integration was not observed in this scoped search.

proposedTypes:

- Reuse and integrate PlayerRawControlInputSystem and PlayerReleaseAndRepeatSystem only after separating item/tile release ownership and defining the protocol Adapter. Target root: src/NSSLC/Component/Player.
- Proposed PlayerItemUseExecutionSystem and API facade under src/NSSLC/Component/Player; do not substitute PlayerItemUseIntentSystem for full execution.
- Reuse PlayerChannelCancellationAdapter as a value-only boundary between Projectile and ItemUse; channel state ownership is pending integration review.
- Reuse PlayerSelectionQuery for reads. Any PlayerSelectionSystem or selected-state Component is conditional on the P09 unique-owner decision; SelectionRadial stays a UI-local state boundary.
- Reuse the separate PlayerNavigationInstrumentSystem and PlayerDetectionInstrumentSystem cores after equipment/reset ordering and consumer integration are defined.
- Reuse builder catalog, overlay projection, input snapshot projection, ItemSpace Query, and DashControl settings adapter as non-authoritative API roles. Their production readers/adapters remain unobserved.
- Keep movement accumulator under Movement; route hostile/team/NPC pressure, lastCreatureHit, ActuationRodLock, wiring display, audio, and accessory/time inputs to their integration owners.

sharedTypesForIntegrationReview:

- Player slot, whoAmI, session/connection identity, active validation, and ECS Entity mapping.
- Packet 13 schema, local vs remote authority, input sequence/replay/duplicate behavior, and all non-P10 fields in the same envelope.
- P09 selected-item authority, SelectionRadial-to-inventory command path, entity handles, and selectedKite/equipment ownership.
- ItemUse state and Inventory/Item/Tile/Wiring/entity interaction commits, including controlUseItem, controlUseTile, reuse, mana cost, success, and errors.
- Projectile identity/lifecycle and the single channel writer.
- Movement accumulator consumption, hostile/team/PVP, step-sound playback, NPC pressure aggregation, Wiring overlay, equipment facts, clock/time source, ItemSpace/VoidVault facts, DashControl persistence, and appearance/SetMatch readers.

crossSubsystemReaders:

- Movement, mount, flight/dash, tile/entity interaction, ItemUse, UI/input, projectile/combat, NPC spawning, wiring, equipment effects, network replication, and item/inventory consumers. The full set is partial; the 7-shard member query is not a project-wide reader closure.

crossSubsystemWriters:

- MessageBuffer packet 13 writes control and selection-related values; Player.ItemCheck and helper paths also write item-use/control state; Player.Update writes release/use and movement-related facts; Main and NPC reset/aggregate NPC pressure; Projectile participates in channel and lastCreatureHit writes; equipment effects populate instrument facts; ResetEffects clears them. Dynamic and unscanned paths remain unknown.

orderingConstraints:

- Preserve packet adapter's legacy acceptance and field mapping before its values become visible to the applicable player tick; exact global order is unknown.
- Preserve Player.Update's observed movement/collision -> ItemCheckWrapped -> PlayerFrame -> UpdateReleaseUseTile order.
- Preserve ItemCheck's synchronous gates, side effects, and state update order before later same-frame readers.
- Preserve ResetEffects before equipment-derived capability facts and the old accessory application ordering; full phase is unknown.
- Channel start -> projectile tracking -> possible matching projectile cancellation is a value handoff; cross-system tick ordering remains unknown.

boundaryChallenges:

- controlUseItem is written by the packet decoder and ItemCheck auto-reuse behavior.
- releaseUseItem and releaseUseTile are not exclusive release-state System writes.
- SelectionRadial and SelectedItemState have different lifecycle and authority; P09 owns an unresolved shared selection seam.
- ItemCheck is too effectful to call a pure intent processor, while its helper closure is not available from CPG.
- P10 groups several cross-domain facts by inventory lineage; they must not become a catch-all PlayerInput owner.
- Type 13 network and settings scope cross player/session/world boundaries.

evidenceGaps:

- CPG has no SourceSnapshotId; the selected shard facts are not source-snapshot-bound. Many access directions are Unknown; callable effects and some call-site searches are partial.
- Local input binding/producer, global Player tick schedule, event registration, full packet caller closure, replay handling, persistence, reconnect/reset, and runtime error behavior are unknown.
- accWatchTime has no resolved member-use fact in the selected query; input source, timer semantics, readers, reset, and persistence are unknown.
- Selection owner with P09, instrument consumers, wiring readers, ItemSpace snapshot producer/commit owner, settings scope, and multi-world identity are unresolved.
- SS14 is only an organizational reference and does not confirm Version4 behavior.

blockingDecisions:

- Input authority: define the unique local/remote raw input writer and packet sequence/authority policy before using the current isolated raw input core in production.
- Release ownership: split movement edges from item and tile release state so the current multiple writes do not become competing Systems.
- Selection authority: jointly settle P09 and P10 selected-item ownership before creating another authoritative component/System.
- ItemUse and Projectile transaction: define synchronous ItemCheck owner plus cross-domain effect ports and the channel event handoff without duplicate channel state.
- Player slot/session/world mapping: define identity, connection lifecycle, network serialization and reuse behavior before adapters commit ECS components.

notImplemented:

- This report proposes no production code or test changes.
- The checked NSSLC cores are not evidence of production tick, network, equipment, projectile, inventory, or scheduler integration.
- No behavior equivalence, migration completion, build result, test result, or runtime verification is claimed.

## Migration Behavior Contract

This is a proposed contract for later implementation review, not evidence that the migration occurred.

| Input | Required output and state delta | Order/error/effect constraints |
| --- | --- | --- |
| Accepted local or remote input frame | Same player-scoped control facts become available to the same consumers. Rejected input leaves prior authoritative state unchanged. | Preserve packet 13 identity/authority rules. Sequence and replay rules are unknown and must not be invented as an implemented guarantee. |
| Selection request | Same selected/hotbar/buffer/override result for local and remote players. | Preserve empty-slot rejection, immediate vs buffered behavior, override handling, selection sound, and AFK counter changes. |
| One Player.Update item-use tick | Same item result, inventory/equipment/projectile/world state delta, animation/time/reuse/channel state, network-visible result, and side-effect sequence. | Execute synchronously at the legacy point; preserve CCed, mount/kite, selected-buffer, release, and retry timing branches. |
| Projectile channel lifecycle fact | Only expected type/index match or the legacy special projectile cancels the current channel. | Do not cancel on unrelated projectile spawn/kill. Duplicate callbacks and termination order remain unknown. |
| Navigation/detection accessory facts | Same reset and derived capabilities become readable in the same equipment frame. | Preserve accessory precedence and aggregation; clock and consumer snapshots are unknown. |
| Builder/selection/input/item-space read | Same values derived from committed facts, with no authority mutation. | Query/projection must remain one-way and deterministic over the supplied snapshot. |
| DashControl setting read/write | Same enum and default are exposed at the legacy scope. | Preserve static behavior until profile, save, and world boundaries are traced. |

Compatibility has three open dimensions: call compatibility for public Player APIs; boundary compatibility for packet 13 and settings persistence; semantic compatibility for state, effects, visibility, order, errors, lifecycle, and scope. A static mapping alone does not establish any of these for the new composition.

## Evidence Gaps and Blocking Decisions

The main boundary is supported by source-level behavior: raw controls, selection, item execution, instrument facts, read projections, and definitions have distinguishable invariants. The strongest decisions are the separate selected-item transition, synchronous ItemCheck composition, and the fact that packet 13 and several P10 fields span other owners. Unique production writers and runtime schedule are not closed.

Cross-subsystem ownership tags:

- crossSubsystemOwner: integration-review for P09 selection authority, raw input protocol/identity, P03 movement and dash/hover, Item/Inventory/Tile/Wiring interaction, Projectile/Combat channel and hit state, NPC pressure, equipment/time instrument facts, builder/Wiring/UI overlay, network projection, settings persistence, and world/session lifecycle.
- No owner is assigned here for team, hostile, aggro, nearbyActiveNPCs, lastCreatureHit, instantMovementAccumulatedThisFrame, hermesStepSound, ActuationRodLock, InfoAccMechShowWires, selectedKite, or accWatchTime.

The report does not block delivery of this proposed static split. It blocks dependent implementation claims that require unresolved owner, schedule, protocol, or lifecycle evidence.

## Verification Plan

verificationStatus: not-run

Do not treat the historical focused-verifier entries in the prior Component execution as verification of this System composition. Before implementation acceptance, run focused behavior checks for:

- Packet 13 local, remote, ServerSideCharacter, invalid/inactive player, repeated/ordered frame, conditional velocity/mount/camera payload, and encode/decode compatibility.
- Release edges and direction timers at start/reset, opposite-direction transitions, hover/hold, item release, tile-release suppression, mouse interface, and same-tick consumption point.
- ItemCheck branches for CCed, item-use failure/success, buffered selection, mount and kite, mana/cost, auto-reuse, animation/time, tile/entity interaction, and observable side effects/order.
- Selection local vs remote, hotbar/inventory validation, empty slots, overrides, buffered apply/cancel, sound, AFK reset, P09 shared authority, and selection radial command handoff.
- Channel expected type/index matching, unrelated projectile, special aiStyle case, spawn/kill order, channel reset, and duplicate lifecycle notifications.
- Equipment frame reset/apply precedence for navigation and detection facts; accWatchTime source/reset/consumers and third-eye counter behavior.
- Pure Query/Projection determinism and no writeback for input sync, SetMatch compatibility, builder definitions/overlay, and ItemSpace/VoidVault evaluation.
- One-writer and same-frame visibility checks for nearbyActiveNPCs, controlUseItem, releaseUseItem, releaseUseTile, lastCreatureHit, and ActuationRodLock after integration owners are decided.

No tests, build, verifier, or runtime check was run for this report. Any unknown behavior, integration, persistence, packet, scheduling, or exception path remains unknown until those checks are executed against the new owner and complete API composition.
