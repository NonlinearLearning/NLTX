# Main ECS migration M-004 server bootstrap boundary

## Narrow proposition

The server startup path owns world input and default-world construction before network listener
startup. `ServerLaunchOptions` validates an absolute `.wld` path and port, `WorldBootstrap.Load`
imports the value-only snapshot, and `WorldBootstrap.CreateDefault` produces deterministic metadata,
tiles and server-owned chests. `DomeServer` consumes that result without recreating the bootstrapped
world objects.

## Source boundary

The Version4 `Main.DedServ`, `SetWorld` and `SetWorldName` methods also contain interactive world
selection, new-world menus, port forwarding, passwords, console input, save-on-exit and the legacy
server loop. Those branches remain deferred because they mix orchestration, client/console state and
runtime cadence beyond this startup contract.

## Owner and verification

Owner: `Terraria.Dome.Server.Startup.WorldBootstrap` plus `ServerLaunchOptions` and `Program`.
Focused verification: `Terraria.Dome.WorldImport.Verification` covers argument rejection, strict
import failure, deterministic default bootstrap, spawn metadata, chest count and no duplicate
construction. No broad MainBoundary, root Release or full-client suite is required for this focused
card under the current validation scope.

## Status

Accepted narrowly. Full `DedServ` parity, interactive selection, world creation options, cloud/path
naming policy, port forwarding, password handling, save scheduling and legacy cadence remain open.
