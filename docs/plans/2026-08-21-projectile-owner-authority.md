# Projectile Owner Authority Slice

## Source Boundary

The server-side projectile path requires an owner that is a current active player. This card
covers only the command-to-commit ownership guard. Projectile type definitions, motion, collision,
damage, lifetime and replication are separate responsibilities.

## Accepted Route

`DomeSimulation.QueueProjectileSpawn` enqueues the command. During command commit, the Simulation
requires the owner handle to resolve to a player entity whose lifecycle is active. Unknown or
inactive owners are rejected by omission before a replication identity or Arch entity is created.
The commit also rejects non-finite position and velocity inputs before entity creation. This keeps
projectile ownership and input integrity in Simulation and prevents forged commands from creating
invalid server state. A non-positive lifetime is also rejected, preserving the projectile lifecycle
invariant that a newly committed projectile has at least one update tick remaining. Penetration uses
the legacy contract: `-1` means infinite penetration and remains valid; `0` and values below `-1`
are rejected.

## Verification Scope

The Combat verifier submits a forged `PlayerHandle(999)` and proves no projectile snapshot is
created. The Simulation project builds in Release with zero warnings and errors. Full Combat
loopback, MainBoundary, broad regressions and root solution build are intentionally not run in this
validation phase.
