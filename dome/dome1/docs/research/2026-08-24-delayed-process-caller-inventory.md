# Delayed-process caller inventory

## Source oracle

- Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`
- SHA-256: `844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`
- Public collections: lines `242-244`
- Outer update consumption: lines `11629-11636`
- In-game consumption: lines `11733-11740`

The retained source declares `DelayedProcesses` and `DelayedProcessesInGame` as public mutable
`List<IEnumerator>` values. The first advances in the outer update path. The second advances in
the in-game path and is reached after the source pause/menu gates. Both remove an iterator only
when `MoveNext()` returns false.

## Caller inventory result

A source-tree search across Version4 `.cs` files found no `.Add` caller and no named process type
that supplies identity, owner, cancellation, retry, serialization or restart continuation. The
only source references are the declarations, indexed reads and removals in `Main.cs`.

## Decision

M-024 remains `deferred`. There is no evidence for a server-owned process family that can
be modeled safely. No `IEnumerator` wrapper, generic coroutine scheduler, or Simulation queue was
added. A future card must first recover one named caller and define phase, pause, cancellation,
disconnect, persistence and deterministic replay identity.
