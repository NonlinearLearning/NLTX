# Tick Order Invasion Boundary

## Source Boundary

The retained `Main.Update` trace invokes world update and `UpdateInvasion` before the projectile
update loop (`Main.cs:11985-12012`). `UpdateInvasion` performs the completion transition in that
world phase (`Main.cs:12958-13033`).

## Accepted Narrow Route

The TickOrder verifier now creates a one-unit invasion, completes it on the next tick, and asserts
that the typed completion event is visible after the tick while `ApplyWorldClock` precedes
`AdvanceProjectiles` in the recorded phase trace. This confirms the selected world-event ordering
without claiming the entire legacy Main loop has been reproduced.

## Deferred Behavior

Other world/entity ordering relationships, exact legacy update subphases, invasion travel and NPC
spawn ordering remain separate cards. The existing phase schedule remains the authoritative ECS
schedule.

## Verification Scope

The focused TickOrder verifier and Simulation Release build pass with zero warnings/errors. MainBoundary,
root solution Release and broad regressions remain outside this validation scope.
