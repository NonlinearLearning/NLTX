# Player Blocked Core Archive

This is the formal archive for the Player behavior clusters that are not part
of the verified first core. `Blocked` remains a truthful migration status; the
archive makes each blocker explicit and prevents it from being mistaken for an
unexplained omission.

## Archive Rules

- An archived blocker is deferred, not migrated and not silently dropped.
- Each entry has one owning domain, a named missing dependency, and a reopen
  condition that can be checked against source-backed behavior.
- No entry may be promoted to `Migrated` from a metadata or snapshot field alone.
- The CSV status remains `Blocked` until the reopen condition and its focused
  verifier both exist.

## Archived Entries

| ID | CSV cluster | Owning domain | Missing dependency | Reopen condition |
|---|---|---|---|---|
| PB-001 | `SpecializedMechanics` | specialized mechanics + client adapter | rule oracle for golf, fishing visuals, wings, ropes, mounts, emotes, and mod hooks | source-backed rules, server owner, protocol adapter, and focused verifier exist for each included family |
| PB-002 | `DeferredSpecialMovement` | movement extensions | oracle for wall climb, wallslide, carpet, double jump, wing, grapple, and sticky movement | deterministic intent/physics contract and collision verifier exist |
| PB-003 | `DeferredInventoryModes` | inventory domain | locked inventory, stacking mode, and Void Bag semantics plus protocol projection | persistent/runtime ownership and invalid-input verifier exist |
| PB-004 | `DeferredEnvironmentalMovement` | environment movement | drowning, slope-down, and wet-collision rules | liquid/environment snapshot and deterministic collision verifier exist |
| PB-005 | `DeferredLuckAndCounters` | player stat domain | luck factor oracle and counter update ordering | source-backed stat equations and replay verifier exist |
| PB-006 | `DeferredTeleportAndSpawn` | world interaction | authoritative teleport and spawn-placement rules | validated command/commit path and loopback verifier exist |
| PB-007 | `DeferredDefensiveAbilities` | combat domain | Shadow Dodge, Brain of Confusion, and Ninja Dodge rules | ordered immunity/evasion events and combat verifier exist |
| PB-008 | `DeferredAllyDefense` | multiplayer combat | Paladin shield ally-defense ownership and targeting rules | multi-player authority contract and loopback verifier exist |
| PB-009 | `DeferredDeathDrops` | death-loot domain | tombstone/drop rule oracle and persistence projection | atomic death-drop command and persistence verifier exist |
| PB-010 | `DeferredSpecialItemEffects` | item/combat domain | biome torch, melee scale, summon, projectile, and special item effects | executable item definitions, authority checks, and behavior verifier exist |
| PB-011 | `DeferredBannerAndMeleeRules` | combat domain | banner buffs, melee cooldowns, NPC hit rules, and jellyfish damage | ordered combat rules and protocol/loopback verifier exist |

## Current Disposition

All eleven entries are `deferred-archived` as of 2026-08-29. They remain
explicitly outside the current compact Player iteration. The archive is linked
from `player-legacy-method-status.csv` and the behavior map; reopening one entry
requires a new Flowstate batch and fresh evidence.

The current accepted-slice/deferred-boundary inventory is recorded in
[`player-blocked-core-evidence-reconciliation.md`](player-blocked-core-evidence-reconciliation.md).
Every row remains `Blocked`; the corresponding partial Phase 7 decision is
[`player-blocked-core-phase7-boundary-20260829.md`](player-blocked-core-phase7-boundary-20260829.md).
