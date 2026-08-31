# Main Host Timing Boundary

## Accepted

`DomeServer` exposes the simulation Tick number for host-boundary verification. Network isolation
enqueue and world-data projection are observational before the server is started: they do not
advance the simulation clock. The server simulation loop remains the sole owner of `DomeSimulation.Tick`;
protocol work is drained and applied at that loop boundary.

The minimal protocol compile repair in this batch supplies the newly required
`ChestTransfer` dispatch field and preserves raw NetModule frame bytes for the declared
`byte[]?` response contract.

## Evidence

- `src/Terraria.Dome.Server/DomeServer.cs`
- `Test/Terraria.Dome.NetworkIsolation.Verification/Program.cs`
- `Build/diagnostics/main-tick/task-11-host-timing/20260824-133000/`

NetworkIsolation, FullClientBootstrap, Persistence loopback, and serial Simulation Release build
are the focused host gates for this boundary.

## Deferred

Full reconnect orchestration, all protocol event projections, and complete server restart
coordination remain deferred until the final host/persistence gate.
