# Main Simulation Entity Capacity Boundary

`SimulationEntityLimits` is now owned by `Terraria.Dome.Simulation`, so the Simulation layer does
not depend on Server configuration types. `DomeServer` maps validated bootstrap limits into the
Simulation constructor. Player and NPC creation reject capacity before handle allocation or ECS
mutation. World-item creation rejects at the commit boundary. Projectile creation stops accepting
new commands once the configured active replication count reaches its limit.

Evidence:

- World.Server verifier: exit `0`
- Server Release build: exit `0`, `0` warnings, `0` errors
- MainBoundary: exit `0`, `0` violations
- `git diff --check`: exit `0`
- Evidence: `Build/diagnostics/main-tick/task-1-entity-capacity/20260824-154500/`

Limits are process/bootstrap configuration and are not yet serialized into arbitrary snapshot
formats; legacy array slot reuse and complete static table parity remain deferred.
