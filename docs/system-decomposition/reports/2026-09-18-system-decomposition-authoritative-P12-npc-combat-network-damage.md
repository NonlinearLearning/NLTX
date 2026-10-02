# System Decomposition Report: authoritative P12

partitionId: P12
taskId: AUTH-SYS-P12
sessionId: 64850edbdb464104ba04d413bfc362e1
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P12-NPC-Combat-Network-Damage.md
outputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P12-npc-combat-network-damage.md
expectedMemberCount: 189
observedMemberCount: 189
designStatus: proposed
verificationStatus: not-run
sourceModified: false
testsRun: false
migrationStatus: not-claimed
sourceProject: D:\TRbackup\Version4
targetArea: D:\TRbackup\NLTX\src\NSSLC

## Scope and Evidence

本报告只覆盖已领取的权威分区 P12，正式父级为 `NpcAndTownSimulation`。输入 ledger 是成员范围的唯一权威；本轮未领取、未分析、未裁决其他分区的成员 owner。输入声明为 19 个叶子组、173 个字段、16 个属性，共 189 个成员，计数与 runner 的 `expectedMemberCount`/`observedMemberCount` 一致。

| Leaf group | Fields | Properties | Total | Ledger declaration section |
|---|---:|---:|---:|---|
| `NpcIdentityInteractionAndPresentationState` | 18 | 0 | 18 | `P12...md:83-117` |
| `NpcTargetAndMovementHistoryState` | 14 | 0 | 14 | `P12...md:118-148` |
| `NpcIdentityAndStatusState` | 4 | 0 | 4 | `P12...md:149-169` |
| `NpcAiTargetAndIdentityState` | 13 | 0 | 13 | `P12...md:170-199` |
| `NpcCombatAndLifeState` | 19 | 0 | 19 | `P12...md:200-235` |
| `NpcCollisionAndPresentationState` | 17 | 0 | 17 | `P12...md:236-269` |
| `NpcPortalAndSpecialBehaviorState` | 11 | 0 | 11 | `P12...md:270-297` |
| `NpcBuffSlotAndImmunityState` | 4 | 0 | 4 | `P12...md:298-318` |
| `NpcElementalDebuffState` | 17 | 0 | 17 | `P12...md:319-352` |
| `NpcControlAndSocialEffectState` | 4 | 0 | 4 | `P12...md:353-373` |
| `NpcWhipAndSpecialEffectState` | 9 | 0 | 9 | `P12...md:374-399` |
| `NpcRegenerationAndProtectionState` | 8 | 0 | 8 | `P12...md:400-424` |
| `NpcLifecycleAndCrossDomainRefs` | 4 | 0 | 4 | `P12...md:425-445` |
| `NpcNetworkReplicationState` | 11 | 0 | 11 | `P12...md:446-473` |
| `NpcNetworkSyncState` | 2 | 0 | 2 | `P12...md:474-492` |
| `NpcDamageDefinitionRegistry` | 4 | 0 | 4 | `P12...md:493-513` |
| `NpcDamageRuntimeTracking` | 9 | 3 | 12 | `P12...md:514-543` |
| `NpcDamageCreditProjection` | 1 | 6 | 7 | `P12...md:544-568` |
| `NpcInteractionAndCommerce` | 4 | 7 | 11 | `P12...md:569-600` |
| **Total** | **173** | **16** | **189** | input ledger |

The ledger tables retain every member name, declaring type, type, declaration and source line. This report uses the complete group inventory above for scope and uses focused source slices for behavior. It does not silently add Town/Progression, Spawn/Eligibility, Loot, persistence, shared IDs, or network-session members from neighboring partitions.

Evidence sources and limits:

| Source | Evidence used | Status and limit |
|---|---|---|
| P12 authoritative ledger | Complete 19-group/189-member inventory and source declaration locations | `confirmed` inventory only; it does not prove a unique runtime writer |
| Version4 `Terraria/NPC.cs` | Declarations around `5897-6447`; `StrikeNPCNoInteraction`/`StrikeNPC` around `67438-67890`; `checkDead` around `64571+`; `UpdateNPC` around `76711-76861`; `UpdateNetworkCode` around `76920-76968`; `GetHurtByDebuff` at `78077-78099`; `ResetForNewNPC`/`SetDefaults` around `8102-8300` | Source facts are current-checkout facts; many branches and helper effects remain outside this focused closure |
| Version4 `Terraria/MessageBuffer.cs` | packet 23 reader around `1182-1307`; packet 24 around `1309-1318`; packet 28 around `1406-1437` | Reader ordering is visible; authorization, malformed input and all handlers are not closed |
| Version4 `Terraria/NetMessage.cs` | packet 23 payload around `680-760`; packet 28 payload `845-851`; broadcast/skip logic around `1705-1747` | Wire fields and recipient predicates are visible; transport guarantees and reconnect behavior are partial |
| Version4 `Terraria/Player.cs`, `Projectile.cs`, `Main.cs` | Player damage at `11960-11983`; Projectile damage at `12510-12520`; `Main` calls `NPCDamageTracker.Update` at `11472` and `NPCInteractions.Initialize` at `3348` | Selected inbound edges confirmed; complete dynamic/scheduler closure is unknown |
| Version4 `Terraria.GameContent/NPCDamageTracker.cs` | registration, active/recent lists, `AddDamage`, `BossKilled`, `Update`, expiry, world credit and player-name credit | `partial`; `CreditEntry.CompareTo` and `InvasionDamageTracker.IncludeDamageFor` are empty/cleared in this checkout, so behavior is `unknown` |
| Version4 `Terraria.GameContent/NPCInteractions.cs` | `Initialize`, 25 shop registrations, action registration, `Shop`, `Register` around `233-297` | Registration facts confirmed; action conditions and effects are stubbed/unknown |
| Read-only CPG API | SQLite export `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`, manifest SHA-256 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`, import complete, 967 shards, 8,166,789 nodes, 71,038,907 edges, 1,317 diagnostics; `Find-CpgSymbols`, `Get-CpgTypeSurface`, `Get-CpgMemberUses`, `Find-CpgCallSites`, `Get-CpgCallableFacts` | Static relation evidence only. `complete` means query completion, not runtime closure; source snapshot is not bound, several access modes are `Unknown`, callee effects are not expanded |
| Current NLTX target | `src/NSSLC/Component/Npc`, `Component/Combat`, `Component/Content` | Existing target evidence only; no proposed type is promoted to an owner and no target code was changed |
| SS14 reference | A small `Content.Shared` System sample was read to compare component/System/event organization | Organization analogy only; no Terraria behavior, names, IDs or verification was copied |

The CPG API was used for relationship queries before source inspection. Its static edges do not close reflection, virtual dispatch, scheduler order, aliases, packet registration, persistence, or external effects. Empty method bodies are recorded as `unknown`, never as evidence of no effect.

## Prior Component Decomposition Reconciliation

The prior P12 Component design/execution documents define C01-C19 candidate data boundaries and record historical partial implementation/build claims. They do not establish a System owner. Their historical `focused-build-and-existing-verifier-passed` statement is retained as `existing-evidence` only and was not rerun or upgraded in this session.

| Prior target evidence | Reconciliation for this System report |
|---|---|
| `NpcDamageTrackingSystem` models active/recent trackers, max recent count 3 and 54000 expiry | Consistent with the observed Version4 constants and list shape. It remains a proposed target tracker System; Version4’s `realLife` resolution, owner normalization, empty tracker behavior and cleared methods are not proven equivalent. |
| `NpcHealthComponent` exposes immutable `CurrentLife`/`MaximumLife` | It is a storage shape, not proof of the direct `life` writes in `StrikeNPC`, parent `realLife` mirroring, debuff damage, or network death paths. A future health commit owner must be integrated with combat and lifecycle. |
| `DamageResolutionSystem` and `DamageEligibilityQuery` | They are generic target code. The observed Version4 damage path includes `CalculateDamageNPCsTake`, crit, `takenDamageMultiplier`, owner 255, town/AI changes, knockback, `HitEffect`, `checkDead`, and packet emission; equivalence is `unknown`. |
| `NpcIdentityComponent`, `NpcInteractionStateComponent`, `NpcBehaviorComponent` | Their comments already say `proposed`/`partial` and `crossSubsystemOwner: integration-review`. They may be read by a future System, but stable instance identity, interaction command ownership, AI write closure and network IDs remain unresolved. |
| `NpcClientReplicationState` | `SkippedSyncCount`/`LastAcknowledgedRevision` do not directly prove Version4 `skippedSyncs`/`streamCounter` semantics. Packet 23 skip/reset logic remains an Adapter/Projection concern pending protocol closure. |
| `NpcDefinitionCatalog`/`INpcDefinitionQuery` | `TryGetByNetId` is useful query shape, but does not establish `CustomBossDefinitions`, `BossTypeForMob`, registration order or fallback behavior. |

No prior Component owner is treated as a confirmed System owner. The proposed System boundaries below must pass a later read/write and behavior contract review before implementation.

## Conceptual Behaviors

Each behavior is a composition of Query inputs, one authoritative commit path, and explicit effect ports. The stable IDs are conceptual and do not imply existing classes.

| ConceptId | Inputs and preconditions | Authoritative delta and invariants | Outputs/effects, order and status |
|---|---|---|---|
| `NPC.DefinitionAndIdentity` | Type/netID, spawn parameters, slot identity, `realLife` relation; `SetDefaults` or packet 23 full state | Initialize/reset all owned NPC fields, preserve content ID vs runtime slot vs parent relation; one lifecycle commit | Definition Query, spawn/lifecycle Command, network/persistence projections. `realLife` owner and stable entity ID are `partial/unknown`. |
| `NPC.TargetMovementAndAI` | committed active NPC, target/player/world queries, AI/localAI, movement medium and collision facts | Evaluate AI, target, direction, velocity, position, teleport/history and frame state in one ordered authority phase; no competing behavior writer | Movement/AI effects and network snapshot. `UpdateNPC` order is source-confirmed locally, scheduler barrier is `unknown`. |
| `NPC.StatusAndBuffs` | buff slots, immunities, elemental flags, regen/protection inputs and tick | Reset/apply/expire flags and slots, apply DoT through the damage commit path, preserve slot and immunity decay order | Combat text, sound, packet 54/28 and status projections. Several helper bodies and cross-system writers are `partial`. |
| `NPC.DamageAdmission` | damage request from Player, Projectile, NPC/environment/network; active/life/immortal/defense/crit/owner | Validate and calculate accepted amount once; apply `takenDamageMultiplier`, parent `realLife` life delta, just-hit/AI/network flags and knockback; never double-credit | `NPCDamageTracker.AddDamage` after accepted amount; `HitEffect`, `checkDead`, packet 28/23 via ports. Exact all-branch closure `partial`. |
| `NPC.DebuffDamage` | `lifeRegenCount`/debuff tick and owner 255 | World credit is recorded, parent life is decremented, lethal path sets life to 1 then invokes `StrikeNPCNoInteraction(9999)` | Combat text and packet 28. Must be ordered so damage is not credited twice; source confirms entry at `78077-78099`. |
| `NPC.DeathAndLifecycle` | life <= 0, `checkDead`, despawn/transform/loot/progression inputs | Resolve special boss phases, loot and active=false once; close encounter only at the observed death event | Loot, progression, announcements, spawns and network are external effects. Final Loot/Town/Progression owner is `crossSubsystemOwner: integration-review`. |
| `NPC.DamageEncounterTracking` | accepted damage, NPC type/`realLife`, contributor identity, monotonic tick | Start/find tracker, add bounded credit, move active to recent, expire recent entries, mark boss killed; tracker is not health authority | Read-only snapshots and credit projections. Version4 ordering and cleared comparison/strategy methods leave behavior `partial/unknown`. |
| `NPC.NetworkReplication` | committed NPC snapshot, packet 23/28 target and section visibility, per-client sync state | Encode/decode packet fields; receiver validates then commits; sender resets skip counters at the source-defined points | Transport/broadcast effects. Adapter cannot allocate unvalidated authority; packet protocol and malformed input are `partial`. |
| `NPC.InteractionCommerce` | local player/talk NPC, interaction registry, shop index/custom key, quest/progression facts | Query available interactions; command execution may mutate commerce/UI state, not core NPC facts except an explicit interaction commit | UI/shop/economy adapters. `Initialize` registration is confirmed; action conditions/effects are `unknown` in the current source. |
| `NPC.PresentationProjection` | immutable identity/status/movement/network snapshots | Derive name, frame, trail, opacity, combat text inputs without writing authority | Renderer/combat-text/sound ports. Render ownership and cache writes are not closed. |

The damage slices intentionally separate `NPC.DamageAdmission`, `NPC.DebuffDamage`, and `NPC.DamageEncounterTracking`: `StrikeNPC` and `GetHurtByDebuff` both touch life/tracker state, so the migration must prove one accepted-damage event and one credit event per hit.

## State Ownership and Write Closure

The following is a proposed ownership map constrained by observed writes. `unique writer` is not source-confirmed unless stated; unresolved shared ownership is explicitly handed to integration review.

| Leaf group / state | State category and observed write closure | Proposed owner and boundary |
|---|---|---|
| `NpcIdentityInteractionAndPresentationState` (`active`, names, presentation flags, catch/release, player interaction, taken multiplier, static flags) | Mixed authority, presentation and interaction. `SetDefaults`, `ResetForNewNPC`, `StrikeNPC`, interaction paths and reset arrays write subsets; all readers are not closed. | `NpcAuthoritySystem` for active/identity facts; `NpcInteractionCommand` and presentation Projection for their domains. `rarity`, `taxCollector`, `freeCake`, and interaction owner remain `crossSubsystemOwner: integration-review`. |
| `NpcTargetAndMovementHistoryState` | `UpdateNPC`, AI, teleport/collision and reset visibly write movement/oldPos/oldRot; exact helper and scheduler edges are partial. | `NpcMovementSystem` with one movement/history commit; world collision/teleport ports are external. |
| `NpcIdentityAndStatusState` (`realLife`, name, shimmer, max buffs) | `realLife` is assigned in SetDefaults and AI; parent life mirrors are written in StrikeNPC; name/shimmer writers are not fully closed. | Identity/lifecycle component plus `NpcStatusSystem`; parent relation and stable ID are `crossSubsystemOwner: integration-review`. |
| `NpcAiTargetAndIdentityState` | AI/target/type/netID/immunity/justHit/timeLeft fields are written in SetDefaults, packet 23 reader, AI and UpdateNPC; dynamic AI writes are extensive. | `NpcAuthoritySystem` owns committed AI/target facts; AI evaluators only return transition facts. NetID/runtime identity adapter remains external. |
| `NpcCombatAndLifeState` | `StrikeNPC`, debuff damage, AI and packet 28 write life/damage-related facts. `realLife` causes parent and child mirror writes. | `NpcCombatSystem` is sole proposed damage/life commit; death/loot coordinator consumes the result. Health component alone is insufficient. |
| `NpcCollisionAndPresentationState` | Collision, frame, rotation and display state are written across UpdateNPC, AI and helpers; read/write closure is partial. | Movement/collision authority plus presentation Projection; no renderer writeback. |
| `NpcPortalAndSpecialBehaviorState` | Portal, shimmer, sound/effect and special counters have scattered AI/helper writes; several effects are external. | `NpcSpecialBehaviorSystem` only if a source-backed transition contract is closed; otherwise adapter/effect port and `unknown`. |
| `NpcBuffSlotAndImmunityState` | SetDefaults/reset clear arrays; packet/update and AddBuff/DelBuff paths mutate slots; immunity decrements in UpdateNPC. | `NpcStatusSystem` sole slot/immunity writer; packet/UI paths use Commands/Adapters. Exact all writers `partial`. |
| `NpcElementalDebuffState` | Buff application and reset write flags; DoT reads flags and invokes `GetHurtByDebuff`; helper effect bodies are incomplete. | Status projection plus `NpcDebuffDamage` command into combat commit; elemental flag owner `partial`. |
| `NpcControlAndSocialEffectState` | Control/social fields can be changed by AI, hit reactions and NPC interaction; town/progression readers cross boundary. | `NpcControlSystem` candidate; Town/Progression final owner `crossSubsystemOwner: integration-review`. |
| `NpcWhipAndSpecialEffectState` | Whip mark/effect fields are set by Player/Projectile/NPC interactions and read by status/effect code; callers are not closed. | Effect command/Projection; no duplicate writes from combat tracker. Owner unresolved until Player/Projectile partitions integrate. |
| `NpcRegenerationAndProtectionState` | Regen values are reset, computed in UpdateNPC and consumed by debuff damage; protection/immortal checks occur in combat. | Status/Protection Query feeds `NpcCombatSystem`; protection policy shared with combat and integration review. |
| `NpcLifecycleAndCrossDomainRefs` | Slot, spawn, parent, portal, Projectile/Player/other-NPC references are reset and mutated in creation/death/AI paths. | `NpcLifecycleSystem` coordinates slot and relation commits; external entity registries and cleanup are `crossSubsystemOwner: integration-review`. |
| `NpcNetworkReplicationState` | `UpdateNetworkCode`, `NetMessage.SendData(23)`, packet 23 receiver and send broadcast mutate network flags/counters. | `NpcReplicationAdapter` emits/accepts validated snapshots; it never becomes NPC authority. |
| `NpcNetworkSyncState` | `PlayerNetSyncState.skippedSyncs` is cleared/incremented in NetMessage broadcast and remote-send paths; `streamCounter` is incremented by proximity streaming. | Per-client `NpcNetworkSyncProjection`/adapter owns cursors; target `NpcClientReplicationState` is not yet equivalent. |
| `NpcDamageDefinitionRegistry` | Static `CustomBossDefinitions`/`BossTypeForMob` registration occurs in tracker static initialization; readers use type and boss status. | Immutable definition Query after explicit bootstrap; registration and mod ordering are `crossSubsystemOwner: integration-review`. |
| `NpcDamageRuntimeTracking` | `_activeTrackers`, `_recentFinishedTrackers`, ticks and credit entries are mutated by tracker methods; `Main` calls `Update`. | `NpcDamageTrackingSystem` sole proposed tracker writer, fed only by accepted-damage/death commands. Cleared strategy methods make behavior `partial/unknown`. |
| `NpcDamageCreditProjection` | Credit list/name/world credit and duration are mutable tracker internals; `CreditEntry.CompareTo` is cleared. | Pure snapshot Query and localized message Projection; ordering/tie behavior is blocked until source semantics are recovered. |
| `NpcInteractionAndCommerce` | `NPCInteractions.Initialize` appends registry entries through `Register`; `OpenShop` carries shop data; action methods are stubbed. | `NpcInteractionSystem` query/command facade plus commerce/UI Adapter; final commerce/progression owner is integration review. |

No Query is allowed to mutate authority or lazily write an unowned cache. A Command or Adapter may produce an effect request, but the single authority System must commit the component delta. Network, persistence, UI, loot, town, player and projectile writes are projections/adapters or cross-partition commands until integration closes them.

## Boundary Role and Decision

### Boundary Decision

| Candidate boundary | Decision | Responsible for | Explicitly not responsible for |
|---|---|---|---|
| `NpcAuthoritySystem` | `partial` / proposed | identity, active/type/netID compatibility facts, AI/target transition commit and ordered tick orchestration | final stable IDs, town/progression, network transport, persistence |
| `NpcMovementSystem` | `separate` / proposed | movement medium, teleport, collision and old-position/history commit | world tile authority and rendering |
| `NpcStatusSystem` | `separate` / proposed | buff slots, immunity decay, elemental/status flags, regen inputs | accepted damage/life commit and external UI |
| `NpcCombatSystem` | `separate` / proposed | damage eligibility composition, accepted amount, parent-life delta, hit flags and combat result | loot/progression/effects transport; it calls tracker port once |
| `NpcDeathLifecycleSystem` | `separate` / proposed | terminal/phase transitions, active state and lifecycle cleanup after combat result | final loot/Town/Progression ownership |
| `NpcDamageTrackingSystem` | `separate` / proposed | tracker registry, bounded active/recent lifecycle, credit commit and snapshots | health, death, localization and packet writing |
| `NpcReplicationAdapter` plus `NpcNetworkSyncProjection` | `separate` adapter/projection | packet 23/28 encode/decode, recipient/skip cursor and validated apply request | authority allocation and combat rules |
| `NpcInteractionSystem` | `partial` / proposed | read-only interaction availability and explicit commerce commands | quest/progression/town final owner |
| `NpcPresentationProjection` | `separate` projection | name/frame/trail/combat-text/sound render outputs | gameplay state writes |
| Damage definitions/catalog | `separate` Definition/Query | immutable boss/type mapping and registration snapshot | runtime encounter state |

Rejected boundary: a monolithic `NpcSystem` would combine 189 fields, AI, status, combat, network, tracker, UI and external effects, creating multiple hidden writers and no meaningful commit barrier. Also rejected: one System per field or per NPC type; the source uses shared parent/segment, network and tracker invariants that require grouped commits.

## System API and Legacy Behavior Mapping

The APIs below are conceptual and proposed. They use Query APIs for reads and explicit Commands/commit ports for writes. They are not existing target signatures.

| LegacyEntryPoint | ConceptId | CompositionId / proposed API order | Commit owner and effects | Evidence status |
|---|---|---|---|---|
| `NPC.SetDefaults`, `ResetForNewNPC`, `NewNPC` | `NPC.DefinitionAndIdentity` | `DefinitionQuery.Resolve` -> `NpcSpawnCommand` -> `NpcAuthoritySystem.CommitSpawn` -> lifecycle/index projection | lifecycle owner commits slot, type, defaults and reset; spawn/network effects through adapters | source calls confirmed; full defaults and external cleanup `partial` |
| `NPC.UpdateNPC(i)` | `NPC.TargetMovementAndAI` + `NPC.StatusAndBuffs` | `NpcStatusSystem.Advance` -> `NpcAuthoritySystem.EvaluateAI` -> `NpcMovementSystem.Commit` -> `NpcReplicationAdapter.Flush` -> `NpcDeathLifecycleSystem.Reconcile` | stage barrier required before projections; `UpdateNPC` order locally confirmed | `partial`; global scheduler and dynamic AI closure unknown |
| `NPC.StrikeNPCNoInteraction` -> `StrikeNPC` | `NPC.DamageAdmission` | `DamageEligibilityQuery` -> `NpcCombatSystem.ResolveAndCommit` -> `DamageEncounterCommand` -> `NpcDeathLifecycleSystem` -> effect/network projections | one life commit (parent if `realLife`), one tracker credit, one death decision | static call and core writes confirmed; all branches/effects `partial` |
| `Player.ApplyDamageToNPC` | `NPC.DamageAdmission` | Player damage Query/armor facts -> `NpcCombatCommand(owner=player)` -> combat result -> Player post-hit Projection | Player remains caller/effect owner; NPC combat is sole NPC writer | `confirmed` caller at `Player.cs:11971`; cross-domain effects partial |
| `Projectile` NPC-hit path | `NPC.DamageAdmission` | Projectile hit facts -> `NpcCombatCommand(owner=projectile owner or world)` -> combat result -> owner kill/DPS Projection | Projectile adapter supplies attribution and receives result; no direct life write | `confirmed` caller at `Projectile.cs:12511`; owner/hostile variants partial |
| `NPC.GetHurtByDebuff` | `NPC.DebuffDamage` | status tick Query -> `WorldDamageCommand(owner=255)` -> combat/death composition -> packet Projection | combat owner must suppress duplicate tracker credit when lethal strike follows | source sequence confirmed at `78077-78099`; exact death effects partial |
| `NPCDamageTracker.AddDamage`, `BossKilled`, `Update` | `NPC.DamageEncounterTracking` | accepted damage event -> tracker commit; death event -> `MarkKilled`; tick phase -> `Advance` | tracker System only; snapshots are read-only | source methods/callers confirmed; strategy/order gaps `partial/unknown` |
| `NetMessage.SendData(23/28)` | `NPC.NetworkReplication` | committed snapshot -> `NpcReplicationAdapter.Encode` -> transport/broadcast policy | network adapter owns wire/recipient effects; per-client cursor projection owns skip state | packet writer/broadcast branches confirmed; delivery/reconnect partial |
| `MessageBuffer` case 23/28 | `NPC.NetworkReplication`/`NPC.DamageAdmission` | parse/validate Query -> `NetworkApplyCommand` -> authority commit -> response Projection | no direct mutation before validation; packet 28 routes to combat | reader order confirmed; malformed/authorization semantics partial |
| `NPCInteractions.Initialize`, `Shop`, `Register` | `NPC.InteractionCommerce` | definition/interaction registry bootstrap -> read Query -> explicit `InteractionCommand` -> commerce/UI Adapter | registry bootstrap writer only; commerce owner external | registration facts confirmed; action bodies unknown |

Composition rule: a legacy method may map to several proposed APIs, but every observable state delta has one commit owner. A Query result is not a commit acknowledgement unless the future contract explicitly returns a committed revision/result.

## Call and Dependency DAG

### Confirmed or source-checked edges

```text
Main initialization
  -> NPCInteractions.Initialize
  -> NPCDamageTracker static initialization (on first use)

Main update
  -> NPCDamageTracker.Update
  -> NPC.UpdateNPC(i) for active NPCs

Player.ApplyDamageToNPC
  -> NPC.StrikeNPC(..., owner=whoAmI)
  -> NetMessage.SendData(28)

Projectile hit path
  -> NPC.StrikeNPC or StrikeNPCNoInteraction
  -> owner kill/DPS and tag projections

NPC.StrikeNPCNoInteraction
  -> NPC.StrikeNPC(owner=255)
  -> NPCDamageTracker.AddDamage (accepted, non-immortal damage)
  -> parent or local life write
  -> HitEffect
  -> checkDead

NPC.GetHurtByDebuff
  -> NPCDamageTracker.AddDamage(owner=255)
  -> parent/local life decrement
  -> lethal StrikeNPCNoInteraction(9999)
  -> NetMessage.SendData(28)

NPC.UpdateNPC
  -> buff reset/set/expire/DoT helpers
  -> AI / collision / frame
  -> UpdateNetworkCode
  -> CheckActive

UpdateNetworkCode
  -> NetMessage.SendData(23)
  -> StreamUpdatesToNearbyPlayers

MessageBuffer case 23
  -> ResetForNewNPC/SetDefaults when full/new
  -> commit position, velocity, target, life, AI and active

MessageBuffer case 28
  -> PlayerInteraction
  -> NPC.StrikeNPC(fromNet=true) or direct death branch
  -> broadcast packet 28 and packet 23 when life <= 0

NPCInteractions.Initialize
  -> Shop -> Register -> All.Add
```

### Proposed target DAG and barriers

```text
DefinitionQuery/bootstrap
  -> Spawn/NetworkApply validation Query
  -> NpcLifecycleSystem commit (identity, slot, relation)
  -> NpcStatusSystem phase
  -> NpcAuthoritySystem AI/target phase
  -> NpcMovementSystem commit
  -> NpcCombatSystem damage/debuff commit
  -> NpcDamageTrackingSystem credit/death event commit
  -> NpcDeathLifecycleSystem terminal reconciliation
  -> Network/Presentation/Commerce projections
```

The proposed graph requires: (1) status inputs visible before damage, (2) one life commit before tracker/kill projection, (3) parent `realLife` commit visible before death evaluation, (4) authority state frozen before packet encoding, and (5) projections after commit. These are design barriers, not proven Version4 scheduler facts.

Unresolved edges that must remain in the DAG:

- dynamic AI dispatch, NPC type-specific methods and virtual effect handlers: `partial/unknown`;
- all writes to `active`, `life`, `realLife`, `ai`, `localAI`, buff arrays and network flags outside focused ranges: `partial`;
- reflection, packet registration, event subscribers, persistence and mod hooks: `unknown`;
- `NetMessage` recipient section activity, reconnect and per-client cursor reset: `partial`;
- cross-domain Player/Projectile/Town/Progression/Loot/Persistence writers: `crossSubsystemOwner: integration-review`.

No edge is omitted because a CPG query returned zero or `complete`; those results only bound the selected static query.

## Lifecycle and Side Effects

| Stage | Version4 evidence | Proposed seam, failure and retry contract |
|---|---|---|
| Definition/bootstrap | `NPCDamageTracker` static initializer registers boss mappings; `NPCInteractions.Initialize` registers shops/actions from `Main` | One bootstrap writer and immutable Definition Query. Duplicate/reinitialize/mod ordering behavior is `unknown`; retry must be idempotent or explicitly rejected. |
| Create/spawn | `NewNPC` allocates slot, resets, sets defaults, assigns position/AI/target, sets active/timeLeft and spawn sync | `NpcLifecycleSystem.CommitSpawn` owns one atomic slot/entity transition; failed definition or slot allocation must not partially publish. |
| Network hydrate | packet 23 may reset/reuse slot, set defaults and then write position/velocity/life/AI | Adapter validates all fields before one lifecycle/authority commit; malformed/truncated packet leaves prior state unchanged. Current source does not prove this transactional property. |
| Tick/update | `UpdateNPC` performs status, DoT, AI, movement, collision, network and active checks in visible order | Scheduler must snapshot reads and expose barriers; time/random/world errors need explicit policy. Exact phase integration is `unknown`. |
| Damage/credit | `StrikeNPC` records accepted damage then writes parent/local life; debuff path records world credit before decrement and may call lethal strike | Combat commit and tracker command must be idempotent per accepted hit; lethal debuff must not double-credit or double-death. |
| Death/terminal | `checkDead` handles phase transitions, loot, announcements, progress and sets `active=false`; `UpdateNPC` also deactivates when life <= 0 | Death System coordinates one terminal transition; Loot/Town/Progression effects are adapters under integration review. Duplicate death calls must be harmless. |
| Despawn/reset/reuse | `ResetForNewNPC`, `SetDefaults`, packet 23 and slot reuse clear arrays/relations/replication state | Reset must clear all P12-owned buffers and tracker links before a new identity is visible. Cross-domain slot cleanup is unknown. |
| Network send | `UpdateNetworkCode` sends packet 23 under spam limits; `NetMessage` applies section and skipped-sync rules; packet 28 has separate recipient predicate | Adapter owns transport and per-client retry/skip state; failed send, disconnect and reconnect behavior is `unknown`. |
| Interaction/commerce | `NPCInteractions.Initialize` populates `All`; conditions/actions currently stubbed | Registry is bootstrap state; command execution must be explicit and cannot write NPC authority implicitly. Commerce/UI/progression transaction is integration review. |
| Persistence/world/session | No complete P12 save/load/session teardown closure was found in focused source | `unknown`; integration must define world/session scope, disconnect cleanup, persistence format, cancellation and multi-world isolation before implementation. |

Side-effect ports are proposed, not existing APIs: `INpcWorldQuery`, `INpcCollisionPort`, `INpcDamageEffectPort`, `INpcLootPort`, `INpcProgressionPort`, `INpcNetworkTransport`, `INpcPersistencePort`, `INpcInteractionAdapter`, `INpcPresentationSink`, and `INpcPlayerProjectilePort`. Each port must be one-way from the owner and must report failure explicitly; no adapter may silently mutate a component.

## Integration Handoff

All items below are deliberately left for integration review. They are not final P12 owner assignments.

| Handoff item | Required closure | Disposition |
|---|---|---|
| Stable NPC identity, runtime slot, netID and `realLife` parent/segment identity | Define identity generations, slot reuse, parent life authority and network/content ID conversion | `crossSubsystemOwner: integration-review`; blocking |
| Player/Projectile damage attribution | Define owner normalization, world owner 255, armor/crit inputs, post-hit callbacks and one accepted-damage event | `crossSubsystemOwner: integration-review`; blocking |
| Town/Progression/Spawn/Loot | Decide who owns town death, eligibility, loot, invasion/progression and spawn side effects triggered by `checkDead` | `crossSubsystemOwner: integration-review`; out of P12 final scope |
| Status/Combat ordering | Decide whether DoT, immunity, protection and `takenDamageMultiplier` are read-only facts or committed in one status phase | `crossSubsystemOwner: integration-review`; partial |
| Network protocol and session replication | Close packet 23/28 field schema, validation, authority, section recipient rule, skip counters, reconnect and versioning | `crossSubsystemOwner: integration-review`; blocking |
| Damage definitions and credit projection | Recover ordering/tie/localization semantics, custom boss registration and clear strategy methods | `crossSubsystemOwner: integration-review`; blocking |
| Interaction and commerce | Define local-player/talk-NPC scope, shop registry ownership, quest/progression command and failure behavior | `crossSubsystemOwner: integration-review`; blocking |
| Scheduler and world/session lifecycle | Fix update phase, barriers, reset/unload/disconnect and multi-world isolation | `crossSubsystemOwner: integration-review`; blocking |
| Existing NLTX components | Reconcile proposed Component shapes with one System writer; specifically health, identity, interaction, behavior and replication state | `crossSubsystemOwner: integration-review`; no code change in this report |

Integration must not interpret this report as a migration authorization. The only handoff artifact produced here is this static report.

## Migration Behavior Contract

This is a later implementation contract, not a claim that migration succeeded. For every entry point, comparison must use:

```text
Observation = (return_or_error, authoritative_state_delta,
               emitted_events_and_external_effects, order_and_visibility,
               lifecycle_and_scope, retry_and_idempotency)
```

| Behavior slice | Required observation and invariant |
|---|---|
| Spawn/reset | Compare slot allocation, defaults, arrays, identity generation, parent relation, active/timeLeft and spawn-sync flags; failed allocation/definition must not publish partial state. |
| Tick/AI/movement | Compare target, direction, position/velocity, old history, status decay, AI/localAI and network flags at each barrier; preserve source order and random/time consumption. |
| Player/Projectile damage | Compare accepted amount, crit/defense/multiplier, parent/local life, knockback, justHit, tracker credit, callbacks and packet sequence; assert exactly one life commit and one credit. |
| Debuff damage | Compare owner 255 credit, parent life decrement, combat text, lethal `StrikeNPCNoInteraction` and packet order; assert no duplicate credit/death. |
| Death/loot/progression | Compare phase transitions, active flag, `BossKilled`, loot/progression effects, announcements and network snapshots; external owners must expose explicit result/error. |
| Damage tracker lifecycle | Compare active/recent lists, tracker start/stop, max recent count, 54000 expiry, duration, world/player credit and kill state; unresolved comparison/strategy semantics remain a test blocker. |
| Packet 23 apply/publish | Compare full/partial field presence, smoothing, reset/defaults, active/life, AI, recipient selection, skip counters and reconnect behavior; malformed packets must have no partial commit. |
| Packet 28 apply/publish | Compare interaction mark, damage arguments, direct death branch, broadcast and follow-up packet 23 parent/child behavior. |
| Interaction/commerce | Compare registry order, condition results, shop index/custom text, command effects and repeated initialization; stubbed current methods require accepted source/trace evidence. |

Migration success is not established by this report, a static mapping, a compile, a partial verifier, or a retained facade. It can only be claimed after the real target project executes the required scenarios through the new owner composition and passes the complete Observation comparison.

## Evidence Gaps and Blocking Decisions

| Gap | Status | Blocking decision |
|---|---|---|
| Full writer/readers for 189 members, especially `active`, `life`, `realLife`, AI arrays, buff arrays and static flags | `partial/unknown` | Complete source-backed read/write/lifecycle inventory before selecting final owners. |
| Dynamic AI dispatch, mod hooks, reflection, event and packet registration | `unknown` | Enumerate registrations and all dynamic targets; no negative conclusion from CPG zero hits. |
| `NPCDamageTracker.CreditEntry.CompareTo` and `InvasionDamageTracker.IncludeDamageFor` cleared bodies | `unknown` | Obtain accepted behavior/source or keep credit ordering/strategy semantics blocked. |
| `realLife` parent/segment death and health synchronization | `partial` | Define parent authority, child mirror timing and duplicate-death prevention. |
| Damage eligibility parity | `partial` | Reconcile generic NLTX `DamageEligibilityQuery` with Version4 active/life/immortal, owner, defense, crit, multiplier and special AI rules. |
| Buff/status/DoT and protection ordering | `partial` | Close all status writers and prove one DoT-to-combat path. |
| Packet 23/28 malformed input, authorization, schema/version and reconnect | `partial/unknown` | Pair reader/writer and transport contract; validate before commit. |
| Per-client `skippedSyncs`/`streamCounter` semantics vs target replication fields | `partial` | Decide cursor owner, reset points and visibility barrier. |
| NPC interaction actions and commerce/progression side effects | `unknown` | Recover implementations or approved traces; leave adapter boundary unresolved. |
| Persistence, unload, disconnect, multi-world and cancellation behavior | `unknown` | Integration owner must define scope and cleanup contract. |
| Target System registration/scheduler and duplicate writers in `src/NSSLC` | `unknown` | Inventory target readers/writers and choose one registered owner before code changes. |
| CPG source binding and callee-effect expansion | `partial` | Reconcile CPG manifest with current source and supplement with focused source evidence. |

Blocking decisions for this proposal are: identity/generation contract; parent `realLife` authority; status/combat ordering; Player/Projectile attribution; network/persistence transaction; tracker definition/credit semantics; interaction/commerce owner; and scheduler/session lifecycle. Until these are closed, every System and API decision in this report remains `proposed` or `partial`.

## Verification Plan

`verificationStatus: not-run`. This session performed read-only CPG queries and source inspection only. It did not modify production code, tests, project files, input reports, prompts or other reports, and did not run `dotnet`, build, test, verifier, runtime, publish or migration commands. Existing historical component build/verifier claims are not current verification.

Future focused verification, after the blockers are resolved and implementation is separately authorized:

1. Inventory and ownership checks: assert all 19 groups/189 members map once, every authoritative fact has one writer, and Queries/Projections have no hidden writes.
2. Lifecycle checks: spawn, packet hydrate, reset, slot reuse, parent/segment creation, deactivation, terminal death, disconnect and world reset with no stale references.
3. Damage checks: Player, Projectile, NPC, world/debuff and network damage; compare accepted amount, parent/local life, knockback, effects, tracker credit and packet order.
4. Tracker checks: definition registration, composite/mob mapping, world/player credit, active/recent transitions, max-three retention, 54000 expiry, kill state and deterministic projection ordering.
5. Network checks: packet 23/28 round trips, partial/full/remove cases, malformed/truncated input, authority rejection, section visibility, skip counters, reconnect and version mismatch.
6. Interaction/commerce checks: initialization idempotency/order, condition/query purity, command effects, shop index/custom text and repeated invocation failure behavior.
7. Scheduler/effect checks: stage barriers, deterministic time/random input, effect-port failure/retry, projection purity and cross-partition ownership assertions.

No verification result is claimed, and no migration success is claimed. The report is a static `proposed` decomposition for integration review.
