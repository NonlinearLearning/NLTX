# Simulation Random Stream Contract

## Decision

The server owns a domain-scoped world-event random stream in `Terraria.Dome.Simulation`.
Its state is immutable, advances through an explicit deterministic transition, belongs to the
`DomeSimulationSnapshot`, and is persisted by `DomeStatePersistenceFormat` version 22.

The stream is an ownership boundary for future probabilistic world events. It is not a claim that
the ECS server reproduces the global legacy `Main.rand` call order.

## Source Constraints

The legacy source has a global `Main.rand` and consumes it from broad update paths, including
`Main.UpdateTime`. The call order is affected by unrelated legacy systems and is not a durable
per-domain state contract. Reconstructing it would require an executable source trace and a fixed
set of all consumers, which this migration does not currently have.

Existing generation and item paths already use separate deterministic seed/state rules. The new
state therefore remains limited to future world-event scheduling and does not silently replace
those paths.

## Contract

`WorldEventRandomState` is a `uint` value. `NextExclusive` and `NextInclusive` return both the next
state and the selected value. Invalid bounds reject with `ArgumentOutOfRangeException`.

`DomeSimulationSnapshot` derives a backward-compatible initial state from the world seed when an
older caller does not provide one. Version 22 writes the exact state. Version 1 through 21 readers
derive the documented seed fallback and do not consume a nonexistent trailing field.

The simulation restores the state from its input snapshot and includes it in every persistence
snapshot. No client, protocol, filesystem or `Terraria.Main` dependency is introduced into
Simulation.

## Accepted Evidence

- WorldRules verifier proves a fixed seed produces the same advancing sequence.
- WorldRules verifier proves a non-default state survives `DomeSimulation` snapshot creation.
- Persistence verifier proves exact v22 round-trip, v21 seed fallback, and truncated-tail rejection.
- WorldRules verifier was run twice with the same PASS set.
- WorldImport and Protocol Compatibility verifiers passed.
- MainBoundary found 559 Simulation source files and zero forbidden dependencies.
- Serial Release build passed with zero warnings and zero errors.

## Explicit Non-Goals

- Exact global `Main.rand` call-order parity.
- Replacing existing derived deterministic item or loot random paths.
- WLD rain or wind import.
- Probabilistic meteor scheduling or any event probability without its own source-backed card.
- Client/UI randomness or presentation effects.

## Follow-Up Gate

Before implementing WLD wind or probabilistic event behavior, create a separate source card that
proves the legacy algorithm, seed relation, version branch and observable state. If that proof is
unavailable, keep the branch explicitly deferred rather than defaulting to zero or inventing a
mapping.
