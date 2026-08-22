# Server Tick Boundary

## Source Audit

Legacy `Main.UpdateServer` mixes periodic WorldData broadcasts, client spam updates, item
ownership/replication, timeout termination and section checks (`Main.cs:13132-13214`). Its
cadence depends on mutable global counters and client arrays.

## Current Authority

`DomeServer.SimulationLoopAsync` owns the server tick boundary. It drains a bounded protocol
command queue, normalizes inputs, advances `DomeSimulation`, commits tile changes, advances
environment state when sessions exist, publishes immutable snapshots, replicates domain state and
flushes the bounded network-isolation layer. The protocol queue and isolation layer reject or
bound overload before simulation mutation.

## Decision

Accept the server-owned loop and bounded command/replication contract narrowly. Do not copy legacy
counter cadence, client timeout arrays, spam updates or periodic `NetMessage.SendData(7)` into
Simulation. Exact legacy network timing and item-owner cadence remain deferred.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-server-tick-boundary/20260822-017000/`.
NetworkIsolation and World.Server verifiers cover bounded queues, command draining and section
replication; scoped diff check passes.
