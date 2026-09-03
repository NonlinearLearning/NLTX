# NPC `CheckActive` player-range lifecycle boundary (2026-08-30)

## Result

The next source-backed lifecycle read after the deferred type-690 guard is the player-range and
timer-refresh portion of `NPC.CheckActive` (`NPC.cs:64446-64545`). This audit is `completed_partial`
for source/runtime inventory and intentionally `deferred` for implementation. Existing typed
components do not prove equivalent coordinate units, player-slot semantics, or the complete
deactivation side-effect chain.

## Legacy source order

Oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`.

1. `NPC.cs:64446` evaluates the separate `DoesntDespawnToInactivityAndCountsNPCSlots` flag
   (type `668`). If set, the method still scans players for slot accounting but returns before
   decrementing `timeLeft`.
2. `NPC.cs:64448` constructs an `activeRange` rectangle around the NPC center using
   `activeRangeX`/`activeRangeY`. `NPC.cs:64449` constructs a second screen-range rectangle using
   `sWidth`, `sHeight`, and the NPC's pixel `width`/`height`.
3. `NPC.cs:64450-64457` visits exactly `Main.player[0]` through `Main.player[254]`, skips inactive
   entries, obtains `Main.player[i].Hitbox`, and intersects it with the active-range rectangle.
   An intersection sets the keep-alive flag.
4. `NPC.cs:64460-64470` increments that particular player's `nearbyActiveNPCs` for eligible
   NPCs. The source excludes types `25`, `30`, and `33`, requires `releaseOwner == 255` and
   `lifeMax > 0`, and applies the `Main.slimeRainNPC[type]` multiplier when slime rain is active.
5. For non-slot-counting NPCs, `NPC.cs:64476-64480` refreshes `timeLeft` to `activeTime` and
   clears `despawnEncouraged` when the screen-range rectangle intersects a player.
6. `NPC.cs:64482-64523` adds a mutable `boss` keep-alive flag and type-specific exceptions:
   the listed type set is always retained; type `399` also refreshes its timer for `ai[0]` values
   `1` or `2`; types `583-585` are retained and refreshed when `!Main.dayTime && ai[2] == 0`.
7. `NPC.cs:64525-64545` returns for type `668`, otherwise decrements `timeLeft`; expiration
   clears the keep-alive flag and mutates `noSpawnCycle`, `active`, and `life`, sends SyncNPC
   message `23`, optionally calls `RevengeManager.CacheEnemy`, and calls
   `CheckActive_WormSegments`.

## Source constants and units

- `NPC.cs:6047-6049` derives `activeRangeX = 1920 * 2.1 = 4032` and
  `activeRangeY = 1200 * 2.1 = 2520` pixels.
- `NPC.cs:6055` sets `activeTime = 750` ticks.
- `NPC.cs:6618-6620` defines `sWidth = 1920` and `sHeight = 1200` pixels.
- `Entity.cs:171-179` constructs `Hitbox` from pixel `position`, `width`, and `height`. Legacy
  tile consumers divide those pixel positions by 16 (for example `Player.cs:5974`); `CheckActive`
  itself does not perform that conversion.
- `Main.cs:647` sets `maxPlayers = 255`; the player backing array is length 256, but the
  CheckActive loop deliberately stops at index 254. The unowned `releaseOwner` sentinel is 255
  (`NPC.cs:5967` and reset at `NPC.cs:8154`).

## Current Simulation owner inventory

| Source responsibility | Current owner/evidence | Boundary |
| --- | --- | --- |
| NPC timer/health/immortal ordering | `NpcLifecycleSystem.Advance` and `NpcLifecycleComponent` | No player-range refresh or source keep-alive exceptions |
| Active player list | `DomeSimulation.GetActivePlayers` (`DomeSimulation.cs:5231-5244`) | Filters `PlayerLifecycleComponent.IsActive`, enumerates `PlayerStore` values, and does not establish `Main.player[0..254]` slot order |
| Player storage/slots | `Players.PlayerStore` dictionary; `PlayerIdentityComponent.AssignedSlot`; `SimulationEntityLimits.MaximumPlayers = 255` | No fixed slot array, duplicate-slot invariant, or explicit exclusion of slot 255 from the lifecycle scan |
| Player hitboxes | `ColliderComponent` and the Training Dummy-specific `TrainingDummyPlayerHitboxSnapshot` path | Training Dummy floors transform and ceils collider directly; it is not a generic CheckActive owner |
| NPC transforms/colliders | `TransformComponent`, `ColliderComponent`, `TileCollisionSystem`, `NpcSpawnCommitSystem` | These consumers floor against `WorldGrid` tile indices; no declared pixel-to-tile unit contract exists |
| NPC slot budget | `NpcSlotAccountingSystem.CalculateActiveSlots` and `DomeSimulation.CalculateActiveNpcSlots` | Global active sum, not per-player `nearbyActiveNPCs` with range/type/release-owner/slime-rain rules |
| Boss/type/AI branches | Static `NpcDefinition.IsBoss` and behavior-specific state | `NpcAuthorityComponent` has no boss member; no generic NPC `ai[0]` owner or CheckActive type registry |
| Deactivation consequences | `CommitNpcDespawnCommands` updates lifecycle, replication, and Training Dummy ownership | No typed `noSpawnCycle`, `extraValue`/revenge, worm-chain, or direct source-aligned SyncNPC side-effect owner |

The focused Training Dummy query is useful evidence that one pixel-space range can be modeled with
explicit inputs, but it does not establish generic NPC coordinate semantics. In fact,
`EvaluateTrainingDummyLifecycle` currently builds `X/Y` from `MathF.Floor(transform.X/Y)` while
`TrainingDummyActivationEligibilityQuery` compares against a `TilePixelSize = 16` rectangle. The
existing path therefore cannot be reused as proof of a generic pixel-space `CheckActive` contract.

The current NPC definitions also expose the mixed-unit risk: ordinary NPCs use collider values
`1.0f x 2.0f` while the Training Dummy definition uses `18.0f x 40.0f` (`DomeSimulation.cs`),
matching legacy pixel dimensions without a declared conversion. Hard-coding `4032`/`2520` into
the current tile-space lifecycle would therefore be unsafe.

## Decision and deferred boundary

Do not add `activeRange`, `safeRange`, pixel-unit fields, a `Main.player` mirror,
`nearbyActiveNPCs`, generic AI slots, boss timer gates, `noSpawnCycle`, revenge state, network
fields, or worm cleanup in this batch. The source-backed replacement chain is not closed from
immutable player/NPC inputs through a typed query/system, deterministic commit, snapshot,
persistence, and protocol consequence.

The audit is therefore `completed_partial` for the inventory only. Generic `CheckActive`, player
range and timer refresh, type-specific AI/event/boss behavior, collision/recovery, unspawn,
network/persistence parity, complete AI families, and legacy source deletion remain
`partial/deferred`; `canRemoveLegacyWorldGen=false` remains unchanged. No production C# was
changed by this batch.

## Evidence

Fresh reproducible output:

`Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-player-range-audit/checkactive-player-range-audit.log`

The log records every source/runtime anchor, derived constant, expected missing owner, and the
final `decision=DEFERRED`. It is an audit artifact, not a parity claim.

The no-source-change verification gate also passed: Simulation Release and Server Release builds
completed with exit code `0` and zero warnings/errors. `git diff --check` completed with exit code
`0`; both private state documents parsed successfully as JSON. Evidence is kept beside the audit
log in `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-player-range-audit/`.
