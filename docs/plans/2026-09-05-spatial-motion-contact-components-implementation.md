# Spatial Motion Contact Components Implementation Record

**Goal:** Add the missing ECS state components from the spatial, movement,
collision, and liquid decomposition without duplicating existing authoritative
position, velocity, shape, liquid queue, door, mount, or portal state.

**Status:** The state-only slice is implemented in the root `src` workspace.
`dome` is a historical/reference workspace and is not an implementation target.

**User direction:** Do not create or modify tests or console verifier projects
for this slice. This record therefore contains production build checks only;
it does not prescribe a test-first workflow or deferred verifier work.

## Authority and placement

| Capability | Production authority | Change |
| --- | --- | --- |
| Entity position and coordinate relationship | `src/Share/Entity/Components/LocationComponent.cs`, `SpatialReferenceComponent.cs` | Add space/parent relationship without copying position. |
| Velocity and kinematics | `VelocityComponent.cs`, `src/Physics/PhysicsStateComponent.cs` | Keep velocity authoritative; add acceleration, gravity, grounded, and movement-lock state. |
| Shape and collision policy | `ColliderComponent.cs`, `src/Physics/CollisionPolicyComponent.cs` | Extend shape metadata and collision options while retaining compatible constructors. |
| Intent and contact facts | `MovementIntentComponent.cs`, `CollisionResultComponent.cs` | Separate validated intent and one-Tick collision facts from raw player input. |
| Liquid entity/world state | `LiquidComponent.cs`, `src/WorldStorage/TileCellState.cs`, `LiquidWorkQueueState.cs` | Keep entity contact facts separate from Tile liquid and queue authority. |
| World liquid projections | `LiquidSchedulerState.cs`, `LiquidSettlingRecoveryState.cs`, `LiquidReplicationDirtySet.cs` | Add scheduler, recovery, and replication staging state without flow behavior. |
| Doors, mounts, and teleportation | `DoorTraversalComponent.cs`, `PlayerMountState.cs`, `src/Teleportation/` | Keep entity-local traversal, mount flags, portal state, and cooldown distinct from Tile mutation and transport resolution. |

## Delivered files

The implementation adds or expands the production state components enumerated in
`docs/空间移动碰撞与液体组件字段设计.md`. New ECS files follow the domain-first
layout: shared entity state is in `src/Share/Entity/Components`, collision and
movement resolution state is in `src/Physics`, player ingress/mount state is in
`src/Player`, Tile liquid state is in `src/WorldStorage`, and portal state is in
`src/Teleportation`.

Components contain only state fields and read-only derived properties. No behavior
System, geometry query, Tile mutation, network sender, liquid-flow algorithm,
automatic-door transaction, minecart rule, teleport transaction, queue operation,
or Tick-transition API is part of this slice. Those actions need explicit
scheduling and effect ownership before they can safely consume these components.

## Production build checks

Only affected production projects may be compiled. Before every build, inspect
active `dotnet.exe` and `csc.exe` processes, then invoke the root wrapper with
one project at a time:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\src\Physics\Terraria.Physics.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

The same command shape applies to `EntityEcs`, `Player`, `Teleportation`, and
when unrelated workspace changes permit, `WorldStorage`. Record only actual
production build evidence. Do not add a test project or a verifier as part of
this work.
