# NPC `CheckActive` nearby-slot contribution owner (2026-08-30)

## Result

This batch adds a typed, pure owner for the weighted `nearbyActiveNPCs` contribution branch in
`NPC.CheckActive`. The owner is a source-backed qualification/weight query only. It is
`verified` as a narrow contract and remains `partial` as an NPC lifecycle migration.

Overall status remains:

> **NPC field/property migration: partial.**

## Legacy source contract

The source oracle is `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs` (SHA-256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`). The relevant branch is
`NPC.cs:64460-64470`:

1. It runs only after an active player hitbox intersects the NPC active-range rectangle.
2. It excludes NPC types `25`, `30`, and `33`.
3. It requires `releaseOwner == 255`, the legacy unowned sentinel, and `lifeMax > 0`.
4. It adds the NPC `npcSlots` value to that player's `nearbyActiveNPCs`.
5. While Slime Rain is active and `Main.slimeRainNPC[type]` is true, it multiplies the value by
   `Main.slimeRainNPCSlots`.

The related source values are in `Main.cs` (SHA-256
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`):

- `Main.cs:673` sets `slimeRainNPCSlots = 0.65f`.
- `Main.cs:675` allocates the `slimeRainNPC` table with `NPCID.Count` entries.
- `Main.cs:6037` sets the only default `true` entry, `slimeRainNPC[1] = true` (`BlueSlime`).

The exact static type classification is already owned by
`LegacySlimeRainNpcRegistry` (`NpcTypeCount = 697`, registered type `1`). This batch consumes
that registry; it does not recreate the array or claim the complete Slime Rain event.

## Typed owner

The implementation is in `src/Terraria.Dome.Simulation/Npc/Systems`:

| Type | Responsibility |
| --- | --- |
| `NpcActivitySlotContributionInput` | Explicit NPC active state, type, `lifeMax`, release owner, `npcSlots` weight, Slime Rain flag, and active-range result. |
| `NpcActivitySlotContribution` | Immutable `ShouldContribute` and weighted `SlotWeight` result. |
| `NpcActivitySlotContributionPolicy` | Pure source-order query for exclusions, unowned sentinel, positive life, and the Slime Rain multiplier. |

`NpcDefinition.NpcSlotCost` is the typed definition value corresponding to legacy `npcSlots`, and
`NpcSpawnStateComponent.ReleaseOwner` is the existing typed release-owner value. The policy
validates net IDs in `0..696`, release owners in `0..255`, and finite non-negative slot costs.
An inactive NPC, non-positive life maximum, an owned NPC, an excluded type, or no active-range
intersection returns a zero contribution without mutating input state. The output weight is
checked for a finite result.

The policy does not enumerate players, assign slots, or increment a counter. The
`IsPlayerInActiveRange` input is deliberately supplied by the separate pixel-space
`NpcActivityRangePolicy`; this keeps coordinate units and player storage out of this owner.

## Deliberate stop boundary

This owner is not wired into `DomeSimulation`, `PlayerStore`, or `NpcLifecycleSystem`. It does not
introduce:

- a fixed `Main.player[0..254]` mirror or mutable `nearbyActiveNPCs` state;
- a global/per-player slot counter or NPC spawn-budget replacement;
- timer refresh, `despawnEncouraged`, `noSpawnCycle`, `active`, `life`, SyncNPC, revenge, or worm
  cleanup side effects;
- boss/type/AI/day-time exceptions, `releaseOwner` protocol changes, or Slime Rain lifecycle;
- a generic pixel-to-tile conversion or a complete `CheckActive` integration.

Therefore full player-range lifecycle behavior, timer/deactivation consequences, spawn and event
parity, persistence/network parity, complete AI families, and legacy deletion remain
`partial/deferred`; `canRemoveLegacyWorldGen` remains `false`.

## Verification

- TDD RED: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-slot-contribution-red/verifier-build-red.log`
  records verifier build exit code `1`; the failures are the missing three owner types/symbols.
- The outer legacy `active` guard was then added as a second TDD edge: the pre-owner build in
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-slot-contribution-active-red/verifier-build-red.log`
  failed only on the missing `IsNpcActive` input, and the focused
  `...checkactive-slot-contribution-active-green/` build/run passed after the minimal input
  extension.
- TDD GREEN: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-slot-contribution-green/`
  records the first focused NPC verifier build/run and serial owner builds.
- Final rerun: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-slot-contribution-final/`
  records a fresh focused NPC verifier build/run and serial Release builds for Simulation,
  Protocol, and Server. `verification-status.txt` records exit `0` for all five commands with
  `0` warnings and `0` errors; the verifier output includes
  `PASS: NPC CheckActive slot-contribution policy` and the existing NPC checks.
- `source-audit.log` in the final directory records the source hashes and exact branch lines.
- Final hygiene includes `git diff --check`, new-file whitespace/line-width checks, and JSON
  parsing for the current private checkpoint/model context. The central checkpoint is left on
  the already recorded WorldGen batch and is not overwritten by this NPC slice.

## Status

`completed_partial` (nearby-slot contribution owner verified; complete `CheckActive` deferred)
