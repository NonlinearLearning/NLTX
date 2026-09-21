# Player Kill/Respawn lifecycle boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`
  - SHA-256: `AF4C868BE773599E271853A549791405AF8F9CD315191B1FE4357E92BD84D7C1`
  - `KillMe`: lines 22572-22658
  - respawn reset/position path: lines 21846-21858
- Current owner: `PlayerDeathSystem`, `PlayerRespawnSystem`, `PlayerLifecycleSystem` and
  `DomeSimulation` typed respawn command route.

## Accepted narrow predicate

The server-owned lifecycle transitions an active player with zero health exactly once to inactive,
records a bounded respawn timer, rejects movement/projectile input while inactive, and restores
health, position and velocity only after the timer reaches zero. Account identity and persistent
inventory remain separate from the runtime lifecycle.

## Deferred source branches

Legacy `KillMe` also performs inventory drops, PVP/PVE counters, achievement events, death timestamp
and presentation state, random dust/gore, mount dismount, child-safety immunity and many buffs.
These are not folded into the lifecycle system without separate owners and replay/projection
contracts.

## Verification note

`PlayerLifecycle.Verification` passed. The first `PlayerAuthority.Verification` run timed out in its
existing network bootstrap `ReadFrameAsync` helper, with no source change or reported server fault.
A clean rerun exited 0 and completed the UUID bootstrap/overwrite rejection assertions. The first
run is retained as a timing-flake observation, while the rerun is the current focused evidence.
