# Item-use timer clamp boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`
  - SHA-256: `AF4C868BE773599E271853A549791405AF8F9CD315191B1FE4357E92BD84D7C1`
  - negative animation/time normalization: lines 23454-23460
  - animation decrement and zero transition: lines 23477-23487

## Accepted narrow predicate

`ItemUseCooldownSystem` clamps negative cooldown and animation ticks to zero before decrement and
channel-state evaluation. A forged negative animation timer cannot leave `IsChanneling` true
indefinitely; valid positive timers retain deterministic decrement behavior.

## Deferred source branches

Full item use styles, reuse delay, auto-reuse, item animation tables, player input timing, client
presentation and type-specific item behavior remain deferred.

## Verification

`Test/Terraria.Dome.Items.Verification` covers normal cooldown expiry and forged negative timer
normalization without changing accepted item-use transactions.
