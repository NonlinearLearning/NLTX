# Main-thread action caller inventory

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`
  - SHA-256: `844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`
  - `QueueMainThreadAction`: lines `11541-11557`
  - drain after `DoUpdate`: lines `11559-11578`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs`
  - SHA-256: `A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D`
  - section completion caller: lines `10527-10530`
  - background transform/follow-up caller: lines `26103-26125`

## Caller inventory

The source exposes two distinct calls:

1. `Main.sectionManager.SetAllSectionsLoaded` is queued after world generation when the game is
   not in the menu. It mutates host/client section loading state.
2. `TransformWorldOnBackgroundThread` accepts arbitrary `Action transform` and arbitrary
   `Action mainThreadFollowup`; the follow-up is queued after the background task and I/O lock
   release.

`ConsumeAllMainThreadActions` drains arbitrary delegates in FIFO order after `DoUpdate`. Neither
caller supplies a typed command identity, deterministic sequence/phase key, cancellation, retry,
persistence/restart continuation or protocol projection.

## Decision

M-014 remains `deferred` for Simulation because section completion belongs to host/client loading
state, while background world transformation belongs to WorldGeneration/Server orchestration.
No `Queue<Action>` replacement, delegate adapter or arbitrary Simulation action queue was added.
A future card must recover a caller-specific typed state machine before implementation.
