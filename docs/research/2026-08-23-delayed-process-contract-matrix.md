# Delayed Process Contract Matrix

Source oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:242-244,
11629-11636,11733-11739`.

| Source collection | In-tree caller evidence | Advance phase | Pause behavior | Cancellation/restart/persistence | Replay identity | Decision |
|---|---|---|---|---|---|---|
| `DelayedProcesses` | No `.Add` caller in Version4 tree; public mutable `List<IEnumerator>` | During `DoUpdate`, before menu/UI cleanup | Runs in the outer update path; exact caller intent unavailable | Iterator removal only when `MoveNext()` returns false; no source-backed cancellation or serialized continuation contract | Arbitrary iterator object has no stable typed identity | deferred |
| `DelayedProcessesInGame` | No `.Add` caller in Version4 tree; public mutable `List<IEnumerator>` | During in-game update after `CanPauseGame()` short-circuit | Does not advance when `CanPauseGame()` returns true | Iterator removal only when `MoveNext()` returns false; no source-backed disconnect/retry/persistence contract | Arbitrary iterator object has no stable typed identity | deferred |

## RED Contract

An untyped `IEnumerator` cannot satisfy immutable snapshot/replay requirements: it exposes only
the next execution step, not a domain identity, serializable state, deterministic ordering key,
owner, cancellation policy or restart continuation. The public collections additionally permit
external/plugin callers that are absent from the source oracle.

## Decision

No reachable process family has a complete source-backed contract. M-024 remains
`deferred`. A future implementation must select one named owner and implement a typed
state machine with explicit phase, pause, cancellation, disconnect, persistence and replay
semantics; it must not introduce a generic coroutine queue.
