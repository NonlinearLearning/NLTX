# Invasion Start Eligibility Contract

## Source Boundary

`Main.StartInvasion` counts players with `active` and `statLifeMax >= 200`, and only enters the
start transition when the count is positive (`Main.cs:13047-13072`).

## Accepted Narrow Route

`WorldInvasionStartEligibilitySystem` owns a pure predicate over an immutable
`WorldInvasionPlayerSnapshot` list. It accepts invasion types 1-4 only when at least one active
player has `MaximumHealth >= 200`, including the exact boundary value 200. It does not accept a
client-provided count. The overload `TryQueueWorldInvasion(command, players)` now gates queue
insertion through this predicate. `DomeSimulation.CreateWorldInvasionPlayerSnapshots` and its
server projection produce the immutable facts from authoritative lifecycle/health components. The
legacy no-snapshot overload remains unchanged until compatibility migration is complete.

The overload `TryQueueWorldInvasion(invasionType, sequence, players)` composes the eligibility
count and `WorldInvasionSizeSystem` internally, so callers cannot provide a conflicting size.

## Deferred Behavior

`WorldInvasionSizeSystem` now owns the source type-specific size formulas: types 1/2 use
`80 + 40*n`, type 3 uses `120 + 60*n`, and type 4 uses `160 + 40*n`; zero qualified players are
rejected. Command integration still remains separate, as do random start side, the legacy
no-snapshot compatibility route and all travel/warning behavior. No client input is trusted for
readiness.

`WorldInvasionTravelSystem` now captures the source movement policy independently: move toward the
spawn tile by `max(dayRate, 1)` and clamp to the target when crossed. It has no persistence or chat
side effects yet.

`WorldInvasionWarningSystem` captures the source warning counter rule independently: movement
decrements the counter, zero resets it to `3600` and requests a warning, and arrival requests a
warning without changing the counter. Chat/client text remains outside this policy.

`WorldInvasionClearFlagSystem` captures the source completion mapping without mutating progression:
types 1/2/3/4 map to Goblins/Frost/Pirates/Martians respectively. Named persistent flags remain a
separate owner card.

`WorldInvasionStartPositionSystem` isolates the deterministic Martian branch from
`Main.StartInvasion` (`Main.cs:13047-13072`): invasion type 4 resolves its initial position to
`spawnTileX - 1.0`. Types 1-3 use the legacy `Main.rand` branch and therefore remain unresolved
until the authoritative random stream and persistence boundary are established.

## Verification Scope

The WorldRules verifier covers inactive players, the 199/200 threshold, invalid invasion type and
the deterministic type-4 start position. The Simulation Release build, Server Release build and
scoped diff check pass. MainBoundary, root solution Release and broad regressions remain outside
this focused contract verification.

Evidence refreshed at:
`Build/diagnostics/main-migration/task-9-invasion-start-position/20260821-224117/`.
The global diff check remains non-zero only because of pre-existing trailing whitespace in the
unrelated legacy `WorldFile.cs`; random start side for types 1-3, named progression ownership and
runtime integration remain deferred.
