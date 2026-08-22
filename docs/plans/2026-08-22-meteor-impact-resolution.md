# Meteor Impact Resolution

## Source Boundary

Legacy `Main.HandleMeteorFall()` reads the pending `WorldGen.spawnMeteor` flag, emits a
client ambience meteor during the `15000..16200` time window, clears the flag after the
cutoff, and then invokes `WorldGen.dropMeteor()` or the low-tiles meteor-shower fallback
(`Main.cs:13982-14017`). The random candidate search and visual ambience are separate concerns.

## Accepted Narrow Owner

`WorldMeteorImpactSystem` owns the authoritative tile-resolution sub-branch after an explicit
impact coordinate has been selected. It validates command/sequence and safe world bounds,
rejects active-entity safety intersections, protected tile types and the bounded meteorite cap,
then emits deterministic `TileChangeCommand` values for the impact crater and meteorite tiles.
The command buffer is committed by the existing Simulation tile phase and survives snapshots.

## Deferred Branches

The following remain deferred and are not inferred from the impact system:

- `WorldGen.dropMeteor()` random candidate search and its terrain/liquid/framing rules
- `StartMeteorShower()` and low-tiles fallback
- client-only `AmbienceServer.ForceEntitySpawn`
- exact pending-flag time-window presentation and source random call order

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-meteor-impact-resolution/20260822-010000/`.
WorldRules verifies deterministic impact, protected regions, active-player safety and snapshot
continuation. Persistence verifies the surviving tile state. Scoped diff check passes.
