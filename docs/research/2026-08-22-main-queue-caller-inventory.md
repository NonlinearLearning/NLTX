# Main Queue Caller Inventory

This inventory is scoped to the Version4 source oracle and is used to decide whether the legacy
Main queue responsibilities can be assigned to Simulation.

## Delayed Processes

| Source member | In-tree construction/caller evidence | Ownership result |
|---|---|---|
| `Main.DelayedProcesses` (`Main.cs:242`) | No `.Add` call site is present in the Version4 source tree; the public mutable list is advanced at `Main.cs:11629-11636` | Unknown; external/plugin caller identity and phase are unavailable |
| `Main.DelayedProcessesInGame` (`Main.cs:244`) | No `.Add` call site is present in the Version4 source tree; the public mutable list is advanced at `Main.cs:11733-11739` | Unknown; external/plugin caller identity and pause/restart semantics are unavailable |

The absence of in-tree `.Add` calls is not evidence that the lists are unused. Their public mutable
surface permits callers outside the source oracle, so the migration must not infer a server owner or
replace them with a generic ECS coroutine queue.

## Main Thread Actions

| Source call site | Action | Ownership result |
|---|---|---|
| `WorldGen.cs:10529` | `Main.sectionManager.SetAllSectionsLoaded` | Host/client section state; not Simulation authority |
| `WorldGen.cs:26122` | `mainThreadFollowup` delegate after background generation | Background WorldGen continuation; typed server contract not recovered |

The two call sites have different owners and cannot share an arbitrary `Action` queue in Simulation.
`SimulationCommandQueue` remains the typed deterministic boundary.

## Decision

M-014 and M-024 remain `deferred`. A future implementation card must provide one typed
contract per recovered server-owned caller, with phase, cancellation, persistence/restart and replay
semantics before adding any queue or continuation owner.

The caller-level contract matrix is recorded in
`docs/research/2026-08-23-main-thread-action-contracts.md`. It confirms that neither recovered
caller provides a complete Simulation-owned typed command contract.
