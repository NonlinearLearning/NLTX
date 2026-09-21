# Entity Derived-Class Component Split Design

## Scope

This change applies the approved first migration path from `docs/component-decomposition/baseline/Entity派生类同质组件拆分设计草案.md`.
The authoritative runtime is `dome/src/Terraria.Dome.Simulation`; shared spatial components live in
`src/Share/Entity`.

## Decisions

- `LocationComponent`, `VelocityComponent`, `ColliderComponent`, and `DirectionComponent` are the
  shared spatial state vocabulary.
- Simulation will reference the shared Entity ECS project instead of maintaining duplicate
  `TransformComponent`/`FacingComponent` types.
- `HealthComponent` remains a Simulation combat component for Player/NPC only.
- Projectile direction, lifecycle, network identity, liquid behavior, and presentation/history
  state remain domain-specific.
- Geometry queries consume `LocationComponent` and `ColliderComponent` and remain pure.
- Existing public behavior is preserved; this is a type/path migration, not a runtime rule change.

## Boundaries and data flow

Spawn and domain systems write shared components. Movement and collision systems read/write location
and velocity; geometry queries read location/collider only; replication and compatibility code use
typed projections. No component gains behavior or cross-domain ownership.

## Migration impact

| Source | Target | Impact |
| --- | --- | --- |
| `dome/.../Components/Entity/TransformComponent.cs` | shared `LocationComponent` | remove duplicate type; update all Simulation/test references |
| `dome/.../Components/Entity/FacingComponent.cs` | shared `DirectionComponent` | remove duplicate type; update shared-facing call sites |
| `dome/.../Components/Entity/VelocityComponent.cs` | shared `VelocityComponent` | remove duplicate type; update references |
| `dome/.../Components/Entity/ColliderComponent.cs` | shared `ColliderComponent` | remove duplicate type; update references |
| `src/Share/Entity/Queries/*` | `LocationComponent` parameters | preserve pure query behavior |

The migration does not touch identity Registry, liquid policy, presentation history, or lifecycle
component design.

## Verification

Run a serial build of `Terraria.Dome.Simulation` through
`Build/Tools/Invoke-SerialDotnet.ps1`, then focused no-build verifiers for movement, player physics,
combat, NPC, and general ECS geometry. Confirm generated artifacts are under `Build/bin/` and no
`TransformComponent`/`FacingComponent` declarations or usages remain in the migrated runtime.
