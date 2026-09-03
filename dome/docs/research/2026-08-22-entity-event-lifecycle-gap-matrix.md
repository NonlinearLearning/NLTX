# Entity and Event Lifecycle Gap Matrix

This matrix executes the lifecycle-accounting part of the remaining ECS migration plan. It is an
inventory, not a gameplay-parity claim. Each row names the current authority or the missing
source-backed contract required before implementation.

## Source Scope

| Domain | Legacy source anchors | Current authority | Status |
|---|---|---|---|
| Player lifecycle | `Main.cs:player[]`, `Player.cs` death/respawn/update paths | `PlayerLifecycleSystem`, `PlayerDeathSystem`, `PlayerRespawnSystem` | accepted narrow slices; collision, buffs, mounts and complete projection remain open |
| NPC spawn and identity | `Main.cs:npc[]`, `NPC.NewNPC`, `NPC.SetDefaults` | `NpcSpawnEligibilitySystem`, `NpcSpawnCommitSystem`, `NpcStore` | accepted narrow identity/position/definition boundaries; tables and spawn search remain open |
| NPC behavior and targeting | `NPC.cs:SupportsNPCTargets`, AI and target branches | `NpcTargetSelectionSystem`, `NpcTargetRoutingSystem`, `NpcBehaviorSystem` | accepted input/routing/chase boundaries; type tables, LOS and AI families remain open |
| NPC death and segments | `NPC.cs:CheckActive`, `CheckActive_WormSegments`, `checkDead`, `NPCLoot` | `NpcLifecycleSystem`, `NpcDeathSystem`, `NpcSegmentLifecycleSystem`, `NpcLootSystem` | accepted typed death/loot/worm children; boss, event, random and network branches remain open |
| Projectile spawn/motion/lifetime | `Main.cs:projectile[]`, `Projectile.SetDefaults`, `Projectile.Update` | projectile definition, spawn, behavior, lifetime and replication systems | accepted validation/motion/target/tile-stop children; reflection, bounce, hostile damage and complete type table remain open |
| World item lifecycle | `Main.cs:item[]`, `WorldItem.cs`, pickup/drop paths | `WorldItemStore`, spawn/motion/pickup/stack systems | accepted identity/geometry/ownership/merge children; nearest-owner re-evaluation, persistence and enemy pickup remain open |
| Invasion event | `Main.cs:StartInvasion`, `UpdateInvasion`, `invasion*` fields | world progression, start, delay, travel, progress, clear and warning systems | accepted bounded state transitions; random starts, NPC spawn tables and full client/chat effects remain open |
| Weather/events | `Main.cs:UpdateWeather`, `StartRain`, `StartSlimeRain`, lantern/night branches | world rule/progression systems and WLD compatibility projection | accepted raw/rule boundaries; secret-seed repair, historical random weather and presentation remain open |

## Explicitly Unresolved Contract Families

| Gap | Why it cannot be inferred from the current owner | Required next evidence |
|---|---|---|
| Complete NPC static table and AI families | Current definitions are a small supported registry; legacy `SetDefaults` contains type-specific flags and branches | source version/type table plus one bounded family contract |
| Complete projectile static table and AI branches | type defaults, reflection/bounce and hostile-player behavior consume unavailable type tables | source defaults and collision/AI owner with deterministic inputs |
| Complete item static table and use branches | item behavior, shimmer, encumbrance and enemy pickup depend on absent definitions | source-backed item family definition and owner |
| Random invasion starts | source consumes global `Main.rand` among other event prerequisites | bounded legacy random call trace and restart oracle |
| NPC/table-driven invasion spawning | source uses NPC tables, spawn tile/search and player state | typed spawn-table owner, tile/search predicates and lifecycle projection |
| Client/presentation branches | rendering, chat, sound, ambience and UI have no server ECS authority | explicit exclusion or protocol/presentation owner |
| M-014 main-thread callers | callers are host/client section state and arbitrary world-generation follow-up | typed caller identity, phase, cancellation, retry and restart contract |
| M-024 delayed processes | source exposes mutable `IEnumerator` lists without recovered add callers or serializable state | named process caller and phase/cancellation/restart/replay contract |
| B-007 global random ordering | domain stream state is not equivalent to legacy global call order | executable source trace with all preceding consumers |

## Decision

No production code or default table is added by this card. Existing narrow owners remain accepted only
for their cited predicates. All unresolved rows stay `deferred` or `excluded` until their required
source evidence is available. This matrix therefore advances accounting without claiming complete
entity/event lifecycle coverage.
