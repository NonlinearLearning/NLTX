# Projectile Lifetime Authority Slice

## Source Boundary

The retained `Terraria/Projectile.cs` oracle decrements `timeLeft` once during projectile update
and calls `Kill()` when it reaches zero (`Projectile.cs:15232-15236`). A projectile created during a
simulation tick therefore begins lifetime consumption in the following movement phase.

## Accepted Narrow Route

`DomeSimulation.MoveProjectiles` now delegates the decrement and expiry decision to the existing
`ProjectileLifetimeSystem.Advance` instead of duplicating the counter mutation. The replication
tombstone is still published before the queued despawn commit, preserving stable identity and
revision behavior. This card accepts only the one-tick lifetime boundary and single-owner mutation
route.

## Deferred Behavior

Projectile type defaults, extra updates, behavior-specific lifetime changes, collision/damage
semantics, penetration parity, and complete persistence/replication parity remain separate cards.

## Verification Scope

The Combat verifier creates a projectile with `LifetimeTicks = 1`, advances the spawn tick and the
first movement tick, then requires an inactive tombstone with nonzero revision and no remaining
lifetime. The Simulation project is built in Release; broad regressions, MainBoundary and the root
solution build remain outside this focused validation.
