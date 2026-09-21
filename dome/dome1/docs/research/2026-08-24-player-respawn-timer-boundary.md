# Player respawn timer boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`
  - SHA-256: `AF4C868BE773599E271853A549791405AF8F9CD315191B1FE4357E92BD84D7C1`
  - `respawnTimerMax`: line 1142
  - `UpdateDead` timer clamp: lines 10061-10071

## Accepted narrow predicate

`PlayerDeathSystem` now rejects a respawn delay above the source maximum of 3600 ticks, and
`PlayerRespawnSystem` rejects a negative timer before any transform, velocity, health or lifecycle
mutation. Zero remains the only eligible timer value for a respawn transition.

## Deferred source branches

This card does not model `KillMe` inventory drops, death reasons, PVP/PVE counters, achievements,
death timestamps, presentation, buffs, mount handling, network timing or client respawn policy.

## Verification

`Test/Terraria.Dome.PlayerLifecycle.Verification` covers the accepted delay, duplicate death,
above-maximum rejection, delayed respawn, state reset and forged negative-timer rejection.
