# Player respawn tick clamp boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`
  - SHA-256: `AF4C868BE773599E271853A549791405AF8F9CD315191B1FE4357E92BD84D7C1`
  - `UpdateDead` respawn timer clamp: lines 10061-10071

## Accepted narrow predicate

`PlayerLifecycleSystem` normalizes a negative inactive respawn timer to zero before evaluating the
normal countdown path. The forged state does not enqueue an automatic respawn command. Positive
timers still decrement once per simulation tick and enqueue only when they reach zero.

## Deferred source branches

Hardcore/ghost rules, client-requested respawn, death presentation, inventory drops, death reasons,
network timing and all other `Player.UpdateDead` behavior remain deferred.

## Verification

`Test/Terraria.Dome.PlayerLifecycle.Verification` covers negative-timer normalization and no-command
behavior in addition to the existing death/respawn checks.
