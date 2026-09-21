# NPC `CheckActive` active-player keep-alive owner (2026-08-30)

## Result

This batch adds a typed, pure owner for the active-player keep-alive branches in
`NPC.CheckActive`. The owner preserves the exact static type set, type `399` timer-refresh
condition, and the night-only types `583..585` condition. It is `verified` as a narrow query
contract and remains `partial` as an NPC lifecycle migration.

Overall status remains:

> **NPC field/property migration: partial.**

## Legacy source contract

The source oracle is `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs` (SHA-256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`). The relevant source
branch is `NPC.cs:64450-64523`:

1. The branch is evaluated inside the loop over active players. If no player in the fixed
   `Main.player[0..254]` scan is active, none of these keep-alive decisions runs.
2. `boss` sets the local keep-alive flag for an active player, but does not itself refresh
   `timeLeft`.
3. The exact static keep-alive types are `7, 10, 13, 35, 36, 39, 87, 127, 128, 129, 130,
   131, 392, 393, 394, 491, 492`.
4. Type `399` always sets the keep-alive flag after an active player is observed. It refreshes
   `timeLeft` only when `ai[0]` is exactly `1f` or `2f`.
5. Types `583`, `584`, and `585` set the keep-alive flag and refresh `timeLeft` only when
   `!Main.dayTime && ai[2] == 0f`.

The earlier `active` guard, the type-690 `ai[0]` guard, inactivity/town registries, player-range
geometry, slot contribution, timer mutation, and deactivation side effects are separate
boundaries. They are not folded into this owner.

## Typed owner

The implementation is in `src/Terraria.Dome.Simulation/Npc`:

| Type | Responsibility |
| --- | --- |
| `LegacyNpcCheckActiveKeepAliveRegistry` | Exact 17-type static keep-alive set and `0..696` domain validation. |
| `NpcCheckActiveKeepAliveInput` | Explicit NPC type, active state, active-player observation, boss flag, `ai[0]`, `ai[2]`, and day-time input. |
| `NpcCheckActiveKeepAliveDecision` | Immutable keep-active and inactivity-timer-refresh result. |
| `NpcCheckActiveKeepAlivePolicy` | Pure source-order query for active-player gating and type/boss/night rules. |

The policy does not infer the static set from faction/category or `NpcDefinition.IsBoss`, and it
does not introduce a generic `float[] ai` component. Inputs are validated for the legacy net-ID
domain and finite AI values; invalid values fail closed with `ArgumentOutOfRangeException`.
The policy never writes `timeLeft`, `despawnEncouraged`, or any NPC/player collection.

The refresh expression is evaluated independently from the keep-alive expression. This preserves
the source overlap where a boss or type `399` both keeps the NPC active and, for `ai[0] == 1f` or
`2f`, refreshes the timer.

## Deliberate stop boundary

This owner is not wired into `DomeSimulation`, `NpcLifecycleSystem`, `PlayerStore`, snapshots,
protocol, or persistence. It does not add:

- a fixed `Main.player` mirror, per-player slot iteration, or `nearbyActiveNPCs` mutation;
- generic AI slots, type-690 behavior, boss definition parity, or event/invasion state;
- `timeLeft`/`despawnEncouraged` writes, `noSpawnCycle`, `active`, `life`, SyncNPC, revenge,
  or worm-segment cleanup;
- collision/recovery, unspawn, network/save projection, or complete `CheckActive` integration.

Consequently full player-range lifecycle behavior, timer/deactivation consequences, type-specific
AI/event/boss behavior, complete NPC parity, and legacy WorldGen deletion remain
`partial`/`deferred`; `canRemoveLegacyWorldGen` remains `false`.

## Verification

- TDD RED: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-keepalive-red/npc-verifier-build-red.log`
  records exit code `1` with the expected missing typed owner symbols; Simulation and Protocol
  dependencies built successfully.
- TDD GREEN: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-keepalive-green/`
  records serial Simulation, Protocol, and NPC verifier builds with exit code `0`, zero warnings,
  and zero errors, plus the focused verifier run.
- Fresh final rerun: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-keepalive-final/`
  repeats Simulation, Protocol, NPC verifier build/run, and Server Release build serially; every
  command exits `0` with zero warnings/errors, and the focused run includes the keep-alive PASS.
- The focused verifier covers registry cardinality and adjacent-type rejection; boss/static
  keep-alive; type `399` refresh and keep-only paths; night/day and `ai[2]` gates for `583..585`;
  no-active-player and inactive-NPC gates; explicit exclusion of type `690`; overlapping
  boss/type-399 refresh; and invalid type, `NaN`, and infinity inputs.
- `source-audit.log` in the GREEN directory records the source hash, exact branch observations,
  typed owner mapping, and the deliberate deferred boundary.
- Final hygiene requires `git diff --check`, changed-block style checks, and JSON parsing of the
  private checkpoint/model-context artifacts. The central checkpoint remains on the WorldGen
  active batch and is not replaced by this independent NPC slice.

## Status

`completed_partial` (active-player keep-alive query owner verified; complete `CheckActive` deferred)
