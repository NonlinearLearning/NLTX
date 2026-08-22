# Main Thread Action Contracts

Source oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:11541-11577`
and `Terraria\WorldGen.cs:10529,26122`.

`Main.QueueMainThreadAction(Action)` accepts arbitrary delegates and
`ConsumeAllMainThreadActions` drains them after `DoUpdate` on the host/game thread. The queue
does not carry command identity, ordering key, cancellation behavior, persistence state, restart
continuation or protocol visibility. It is therefore not a Simulation command contract.

| Caller | Input/effect | Owner | Phase/order | Cancellation/retry/persistence | Protocol visibility | Decision |
|---|---|---|---|---|---|---|
| `WorldGen.cs:10529` | `Main.sectionManager.SetAllSectionsLoaded` mutates section-manager completion state after generation | Host/client section subsystem | Main-thread drain after `DoUpdate` | Source does not express a replayable command; lifecycle is coupled to section manager | Host/client presentation and loading state | deferred outside Simulation |
| `WorldGen.cs:26122` | `mainThreadFollowup` arbitrary delegate after background world-generation work | WorldGen continuation owner | Main-thread drain after background task completion | Delegate identity, cancellation, retry, persistence and restart state are unavailable | Depends on caller-provided delegate | deferred |

## RED Contract

A candidate Simulation route must reject an arbitrary `Action` because it cannot declare a typed
identity, deterministic sequence/phase, snapshot representation or replay behavior. The existing
`SimulationCommandQueue` already accepts typed command values; this card does not add a generic
delegate queue.

## Decision

No caller in the source inventory has a complete, source-backed Simulation contract. M-014 remains
`unknown` for arbitrary callers. A future card may add one caller-specific typed command only after
recovering its owner, deterministic phase, cancellation, persistence/restart and projection
semantics.
