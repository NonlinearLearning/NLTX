# Player Top-Slope Collision Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Migrate the source-backed player contact behavior for Version4 top slopes `1` and `2`
without claiming complete slope, hoik, NPC, or reverse-gravity collision parity.

**Architecture:** Version4 Player calls `TileCollision` and then `SlopingCollision`; therefore the
current Dome `TileCollisionSystem` must not become a mixed AABB/triangle monolith. Player movement
will use a small `TopSlopeContactSystem` after its ordinary tile movement, while NPC movement retains
the existing whole-tile slope fallback. The module accepts only world, transform, velocity, grounded
state and collider, returns no global state, and hides tile-shape lookup plus contact selection.

**Tech Stack:** .NET 10, C#, Arch, executable console verifiers, serial Release gates.

---

### Task 1: Freeze the source and establish RED

**Files:**
- Modify: `Test/Terraria.Dome.PlayerPhysics.Verification/Program.cs`
- Reference: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs:14294-14308`
- Reference: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs:14324-14355`
- Reference: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Collision.cs:1228-1469`

**Step 1: Add failing player scenarios**

Use an active known Type `1` tile with `Slope: 1` and a distinct scenario for `Slope: 2`.
The initial player collider is `1 x 2`; construct the input so the player reaches the physical top
surface from above while moving into the slope's supported direction. Assert final foot height,
vertical velocity, grounded state, and that the old whole-tile wall outcome is rejected.

Add explicit rejected/unchanged cases:

```text
Slope 3/4 remain whole-tile fallback in this slice.
NPC movement retains its existing generic fallback.
No active/non-solid/platform/unknown-tile behavior regresses.
```

**Step 2: Record RED**

Run PlayerPhysics from repository root with serial compiler settings and preserve the nonzero output
as `physics-red.txt` in a new
`Build/diagnostics/main-migration/task-8-player-top-slope/<runId>/` directory.

### Task 2: Add the player top-slope module

**Files:**
- Create: `src/Terraria.Dome.Simulation/Physics/Systems/TopSlopeContactSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Physics/Systems/TileCollisionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test: `Test/Terraria.Dome.PlayerPhysics.Verification/Program.cs`

**Step 1: Preserve the seam**

Give `TopSlopeContactSystem` one operation:

```csharp
public void Resolve(
  WorldGrid world,
  ref TransformComponent transform,
  ref VelocityComponent velocity,
  ref PhysicsStateComponent physics,
  ColliderComponent collider)
```

It owns only active, known ordinary solid tile slope shapes `1` and `2`; unknown active tiles still
fail before physics state is mutated. It must not read `Main`, Player objects, Protocol, Server,
wall-clock time or global random state.

**Step 2: Make ordinary resolution defer only player top slopes**

Extend the existing internal tile-collision path with a narrowly named option used only by
`ResolvePlayerTileCollision`. When enabled, known slope `1/2` tiles do not act as full AABB walls;
non-player/NPC calls retain current whole-tile behavior. Do not change the public authority of
`WorldGrid` or `TileDefinitionRegistry`.

**Step 3: Implement surface contact**

For the supplied unit-grid coordinates, derive the top surface at the appropriate collider foot
edge from the legacy slope shape. If a player arrives from the supported top side and crosses or is
below that surface, correct only vertical position, clear downward vertical velocity and set
`IsGrounded`. Use a deterministic tie-break for a collider spanning tiles; do not create a per-tile
public geometry abstraction.

The system must not claim side entry, ceilings, bottom slopes `3/4`, hoik, `stairFall`, step-up/
step-down, gravity inversion, platform slopes, conveyor slopes, mount, grapple, pulley, or NPC
slope behavior.

**Step 4: Run focused GREEN**

Rerun PlayerPhysics and save `physics-final.txt`. Require all earlier half-brick, platform,
definition-backed solidity, unknown tile and fall-through checks to pass.

### Task 3: Acceptance and documentation

**Files:**
- Modify after gates only: `progress.md`
- Modify after gates only:
  `docs/plans/2026-08-19-main-server-ecs-migration-execution.md`

Run serially: PlayerPhysics, PlayerLifecycle loopback because player-only movement composition
changed, NPC verifier, Liquid verifier, MainBoundary, then root Release with
`-m:1 -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:FixtureHostBuild=false`.

Save source manifests, final outputs, exit codes and `git diff --check` in the evidence directory.
Only then accept the player top-slope slice. Task 8 remains `IN PROGRESS`; all excluded shape and
movement families retain explicit deferred status.
