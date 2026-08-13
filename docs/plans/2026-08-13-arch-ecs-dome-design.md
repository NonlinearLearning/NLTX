# Arch ECS Dome Design

## Goal

Create a small, authoritative Terraria-like server simulation using Arch ECS and a
separate console client Dome that proves real client/server interaction. This is a
new simulation slice, not a rewrite of the legacy `net40` Terraria server.

## Scope

The Dome demonstrates a Player moving and firing, an Npc selecting and following
that Player, and a Projectile damaging and despawning against the Npc. It preserves
the separation between gameplay simulation and host/client concerns.

The Dome deliberately does not implement the legacy Terraria protocol, rendering,
UI, persistence, inventories, tiles, buffs, mounts, or full legacy AI. Those are
future adapters or game domains; they must not define this ECS model.

## Model

`Arch.Core.Entity` is the internal runtime handle. There is no new `Entity.cs`, no
universal `EntityState`, and no Player/Npc/Projectile inheritance hierarchy.

Common capabilities:

```text
TransformComponent       current position
VelocityComponent        current motion
FacingComponent          horizontal facing independent of velocity
ColliderComponent        AABB size
PhysicsStateComponent    transient collision/ground outcomes
HealthComponent          damageable capability
```

Domain capabilities:

```text
PlayerTagComponent
PlayerInputComponent
PlayerControlStateComponent

NpcTagComponent
NpcTargetComponent
NpcAiStateComponent

ProjectileTagComponent
ProjectileOwnerComponent
ProjectileDamageComponent
ProjectileLifetimeComponent
```

Spatial derived values such as centre, hitbox and range are calculated from
`TransformComponent` and `ColliderComponent`; they are not stored as duplicate
state. Structure changes and destruction are collected as domain commands and
applied once per tick through `Arch.Buffer.CommandBuffer.Playback`.

## Modules

```text
Terraria.Dome.Simulation
  Pure Arch World, components, systems, commands and snapshots.

Terraria.Dome.Server
  TCP listener, session-to-player mapping, fixed ticks and snapshot publication.

Terraria.Dome.Client
  Console input, TCP frames and authoritative snapshot display.

Terraria.Dome.Verification
  Executable behavior and TCP integration verification without test-framework
  dependency risk.
```

The Simulation module knows neither sockets nor client connections. The Server
maps one connection to one server-created Player entity and can only submit control
intentions. The Client never submits authoritative positions, damage, targets, or
Arch entity IDs.

## Tick Pipeline

```text
1. Begin tick and clear command queues.
2. Apply player input to control state.
3. Apply player control to velocity and projectile spawn requests.
4. Select Npc targets and run Npc chase behaviour.
5. Apply movement and world bounds/ground collision.
6. Create requested projectiles.
7. Advance projectile movement and lifetime.
8. Detect projectile/Npc overlaps and enqueue damage/despawn commands.
9. Resolve damage and enqueue deaths.
10. Commit Arch structural commands once.
11. Build an immutable state snapshot.
```

## Transport

The first Dome uses a private line-delimited JSON frame only for interaction
verification. It is intentionally outside the Simulation interface. Future
Terraria packet adapters can convert their packets to the same simulation input and
read the same snapshots without reintroducing `Main.player`, `whoAmI`, or `ai[]` to
gameplay code.

## Verification

The executable verifier must prove all of the following:

- input moves the server-owned Player;
- an Npc selects the Player and reduces the distance to it;
- firing creates a Projectile which reduces Npc health and despawns;
- an independently running TCP server accepts input and publishes the resulting
  authoritative snapshot;
- production projects build from repository root with output below `Build/`.
