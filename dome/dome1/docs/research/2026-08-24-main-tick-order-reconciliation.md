# Main Tick-Order Reconciliation Boundary

This card records the source order that can currently be mapped without importing the legacy
loop wholesale.

Source `Main.cs` SHA256:
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`.

## Source Facts

- `Main.cs:11956-11988` updates projectiles and world items before `UpdateTime`.
- `Main.cs:11994-12012` calls `UpdateTime`, then `WorldGen.UpdateWorld`, then `UpdateInvasion`,
  and only then enters `UpdateServer`.
- `Main.cs:13525-13536` resolves `UpdateTimeRate`, advances `time`, and evaluates time-dependent
  event consumers in that method.
- `Main.cs:12958-13033` consumes the post-time `dayRate` for bounded invasion travel and warning.

## Current ECS Boundary

The ECS schedule proves a narrower relation:

`ApplyWorldClock -> entity phases -> CommitDomainCommands -> PublishSnapshot`.

The accepted world-time-rate/invasion card proves that invasion travel consumes the persisted
resolved rate after clock application. The current Simulation does not claim that all legacy
`WorldGen.UpdateWorld` work has an owner, so the source projectile/item-before-time relation and
the complete world-update phase remain `partial`/`unknown`.

No phase was renamed or inserted to make the source and ECS schedules appear equivalent. Future
cards must attach each `WorldGen.UpdateWorld` responsibility to a named domain system before
claiming a stronger order invariant.
