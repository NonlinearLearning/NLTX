# T4-B4 System lifecycle and phase concept map

Status: static source/package map plus a compiled isolated probe. The probe's pre-amendment run is
historical only under the current compile-only gate. This document does not claim production
System migration or runtime parity.

## Package baseline

NuGet metadata for `Arch.System 1.1.0` identifies repository commit
`1defffe021f9891b38ca31932252dd2d322de560`, Apache-2.0, and assets for `net6.0`, `net7.0`, and
`netstandard2.1`. Its nuspec dependency groups are empty. The selected `Arch.System.dll` references
.NET runtime assemblies only; it has no assembly reference to Arch. `Arch 2.1.0` is therefore an
explicit probe dependency for testing actual `Arch.Core.World` binding. The Arch package declares
the transitive dependencies shown by the probe's `project.assets.json` and package listing.

For `net10.0`, restore selected `Arch/lib/net8.0/Arch.dll` and
`Arch.System/lib/net7.0/Arch.System.dll`. The isolated project compiles with those assets. Package,
source, assets, DLL, and PDB hashes are in [source-hashes.md](source-hashes.md).

## Arch.System lifecycle surface

`Arch.System.ISystem<T>` exposes `Initialize`, `BeforeUpdate(in T)`, `Update(in T)`,
`AfterUpdate(in T)`, and `Dispose`. `BaseSystem<W,T>` stores the supplied `W` as `World`; it does
not constrain `W` to `Arch.Core.World` or validate world/session identity. `Group<T>` stores
systems in insertion order, including nested groups.

Source inspection and the probe source identify the observable composition constraints:

- Registration does not invoke `Initialize`; callers invoke lifecycle hooks explicitly.
- `Group.Update` invokes only `Update`; it does not implicitly call `BeforeUpdate` or
  `AfterUpdate`.
- Nested groups forward each explicitly invoked hook in registration order.
- Groups do not own or validate one World. Systems can retain different World references inside a
  single Group.
- System exceptions propagate. A thrown hook stops the current traversal. `Group.Dispose` does
  not continue disposing later entries after a child throws, and it has no idempotence guard.
- No scheduler exists in the package. An initialized group with no tick only has its explicit
  initialization call; the caller defines whether and when update hooks run.

These behavior outcomes were observed in a probe run before the compile-only amendment. They are
retained as historical observations and are excluded from current acceptance. Current evidence
for this batch is the probe project build only.

## Existing phase concepts

Stable ConceptIds below name observable behavior, not old method names. The old host uses
`IWorldSimulationTickPhase` with only `Phase` and `Execute(context)`; it has no Initialize,
BeforeUpdate, AfterUpdate, or Dispose contract. `WorldSimulationKernel` owns the creating thread,
command drain, clock advance, snapshot creation, optional runtime projection, phase ordering, and
tick commit. It sorts registered phases by enum, rejects duplicate and kernel-owned phases, and
does not delegate this order to a System Group.

| ConceptId | Current owner / location | Current ordering and effects | Arch.System mapping note |
| --- | --- | --- | --- |
| `sim.tick.command-drain-fifo` | `WorldSimulationKernel.EnqueueCommand` / `Step` | Drains queued actions before clock advance; commands can close over mutable state. A session change stops an uncommitted tick. | Keep the command owner and re-resolution protocol explicit; `Group.Update` is not a command queue. |
| `sim.tick.clock-snapshot-projection` | `WorldSimulationKernel.Step` | Advances clock, creates a committed snapshot, then optionally projects runtime state. | Kernel-level preconditions surround the domain systems; they are not a per-System `BeforeUpdate` substitute. |
| `sim.tick.ordered-phase-commit` | `WorldSimulationPhase`, `WorldSimulationKernel`, Simulation host | Runs Player → NPC → Projectile → WorldItems → TileEntities → WorldSystems, then commits the tick number and snapshot. | Explicit phase order remains authoritative; Group registration order alone does not express enum order or commit boundary. |
| `sim.phase.player-move-pressure-weapons` | `ActivePlayerTickPhase` | Player movement/lifecycle update → pressure plate interactions → scripted weapons. Pressure-plate state is projected before wire-triggered effects. | Preserve the composed order within the Player phase. |
| `sim.phase.npc-authority-slot-order` | `ActiveNpcTickPhase`, `RuntimeNpcStore` | Client path observes references only. Authoritative path runs natural spawn, captures references, scans NPC slots ascending, then applies contact damage from snapshots. | Do not replace with unspecified archetype/chunk query order; see the NPC concept below. |
| `sim.phase.projectile-bounds-coordinator` | `ProjectileSimulationTickPhase` | Requires committed world descriptor, derives bounds, creates an adapter for the tick, then runs the projectile coordinator. | Adapter creation and descriptor validity are phase-level inputs, not automatic System lifecycle hooks. |
| `sim.phase.world-item-slot-motion-pickup` | `ActiveWorldItemsTickPhase`, `RuntimeWorldItemStore` | Scans item slots ascending; validates root/world projection, computes physics, removes expired items, commits motion, then checks player pickup in store order. | Keep named item owner/revision and slot semantics; current production Arch commit contracts are still unintegrated. |
| `sim.phase.tile-entity-snapshot` | `ActiveTileEntityTickPhase`, `TileEntityUpdateSchedule` | Captures scheduled IDs once, dispatches registered type handlers, and completes the schedule snapshot in `finally`. Handlers may commit entity/tile state, NPC bindings, and wiring effects. | Query presence cannot replace the schedule snapshot or owner validation. |
| `sim.phase.liquid-owner-commit` | `ActiveLiquidTickPhase` | Binds the active session to legacy Liquid mutation observation; each update drains changed tile coordinates, commits them to the loaded-world owner, then verifies projection parity. | This phase owns process-global legacy state and a session boundary; do not duplicate it per entity query. |
| `sim.tick.exception-fault-no-rollback` | `WorldSimulationKernel.Step` | Phase exceptions fault the kernel and rethrow. Earlier mutations remain; tick number/snapshot commit happen only after all registered phases. Stop/session invalidation can also return an uncommitted tick. | System exception handling must not silently turn a partial tick into a committed tick. |

The simulation host assembles these phases in `src/NSSLC.Tools.Simulation/Program.cs`; the kernel
sorts them by `WorldSimulationPhase`. No production `Arch.System` reference was added.

## NPC movement concept candidate

`sim.npc.authoritative-slot-ordered-tick` is the stable candidate for a later System/Query design.
The path is:

```text
Simulation host phase registration
  -> WorldSimulationKernel ordered phase execution
  -> ActiveNpcTickPhase authority gate and natural spawn pass
  -> RuntimeNpcStore.Update slot scan
  -> UpdateMovement per surviving NPC
  -> RuntimeNpcEntity short component snapshots and commits
```

Static source evidence shows:

- `ActiveNpcTickPhase` returns to an observation-only branch on non-authoritative clients. The
  authoritative path runs natural spawn before the live slot scan, then applies contact damage
  after NPC updates.
- `RuntimeNpcStore.Update` scans `0..Capacity-1`, performs natural despawn/release before movement,
  and writes the updated tick marker after movement. A query with unspecified chunk order would
  change an observed ordering dependency.
- Movement reads value snapshots of location, velocity, grounded state, collider, previous
  collision flags, direction, target, AI, housing and task state. It also reads player snapshots,
  world clock/descriptor/tile state, legacy collision flags, and authority/global runtime state.
- The ordinary path begins a movement tick, evaluates gravity and town housing synchronization,
  constructs player target snapshots, evaluates AI, commits AI slots and physics flags, resolves
  tile collision or world bounds, then commits movement. Profile-specific branches can instead
  consume RNG, spawn entities, open doors, request network updates, or record presentation effects.
- `RuntimeNpcEntity.Movement` reads three component snapshots and commits position, velocity and
  grounded state through a multi-component edit. `CommitAiState` separately writes local AI slots
  and behavior state; a later failure can leave earlier writes in place.
- The store contains seeded runtime RNG plus legacy `Main.rand` use. Slot/profile order can affect
  random consumption and same-tick spawn visibility.

This is a source-level inventory, not an executed host behavior proof. Future migration must either
retain the owner slot walk for this ConceptId or explicitly revise its behavior contract before
using Arch query order.
