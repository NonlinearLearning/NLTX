# NPC Combat Lifecycle Gap

## Evidence

The current ECS has a narrow NPC slice: deterministic chase/targeting, projectile and contact
damage, health revisions, death/despawn, deterministic loot in direct Simulation tests, protocol
projection and PVS replication.

Legacy `NPC.cs` is substantially broader. `NPC.Spawner.SpawnNPC` and `FindSpawnTile` depend on
player readiness, invasion/event flags, tile predicates and global `Main.rand`; `UpdateAI`,
`SetDefaults`, `StrikeNPC` and `NPCLoot` cover hundreds of type-specific branches. These are not
represented by the fixture NPC definition registry.

## Original RED

`Terraria.Dome.Combat.Loopback.Verification` fails reproducibly after the two-session combat trace:

```text
Combat PVS or authoritative NPC loot ownership was violated. Items=0, Frame=.
```

The direct Combat verifier passed deterministic NPC loot, but the real loopback path initially
ended with no world item visible to the assertion. This was a real integration failure until the
item lifecycle/PVS timing was traced; it was not hidden by changing the assertion.

Instrumentation exposed `Created=0, PickedUp=0`, ruling out immediate pickup. The source trace then
showed that lethal projectile damage updated the NPC replication to inactive during command commit,
while `AdvanceNpcLifecycles` skipped inactive NPCs. Therefore `NpcDespawnReason.Killed` was never
recorded, `PublishNpcDeaths` never queued a death result, and `CommitNpcLoot` never ran.

## Narrow Fix and GREEN

`DomeSimulation.AdvanceNpcLifecycles` now still evaluates an inactive NPC when its health is zero
and its lifecycle reason is `None`. This closes the pending lethal transition exactly once without
reactivating the NPC or changing PVS behavior. The Server exposes read-only created/picked-up event
views only for verifier evidence; it does not move item ownership out of Simulation.

The loopback verifier now reports created and picked-up event details when it fails, preserving a
replayable diagnostic boundary rather than weakening the acceptance predicate.

Verified GREEN:

- Combat Loopback: two-session PVS and one server-owned drop.
- Combat direct: bounded damage/death/loot lifecycle.
- Combat Protocol: V1456 projection and PVS revision cursors.
- Two independent loopback runs produced the same PASS result.
- TickOrder: named phase ordering and deterministic command ordering.
- MainBoundary: 579 Simulation files, 0 violations.
- Serial root Release: 0 warnings, 0 errors.

## Decision

Keep the Main NPC responsibility `unknown`. Treat the following as separate cards:

- loot spawn and world-item visibility/pickup timing in loopback;
- NPC spawn eligibility and tile selection;
- AI family coverage;
- death/clear side effects and progression;
- type definitions and replication completeness.

The direct and loopback combat slice is accepted only for the repaired NPC death-to-loot/PVS route.
The Main NPC responsibility remains `unknown`: spawn eligibility, tile selection, AI families, type
definitions, global-random branches, and broader clear/progression effects still require separate
source-backed cards. This does not satisfy full `NPC.cs` lifecycle acceptance.
