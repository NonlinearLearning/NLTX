# Open Blocker Qualification Re-audit

Date: 2026-08-22

This is a source-backed re-audit of the remaining M-001, M-014 and M-024 blockers. It does not
introduce a replacement aggregate initializer, a generic action queue or a generic coroutine
scheduler.

## Source identity

- `Terraria/Main.cs`
  - Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`
  - SHA-256: `844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`
  - Relevant ranges: `3732-3859`, `11541-11577`, `11629-11636`, `11733-11739`
- `Terraria/WorldGen.cs`
  - Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs`
  - SHA-256: `A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D`
  - Relevant ranges: `10529`, `26103-26122`

## M-001: `Initialize_AlmostEverything`

The unresolved families remain `TileEntity.InitializeAll`, `TorchID.Initialize`,
`LeashedEntity.Registry.RegisterAll`, `NPCInteractions.Initialize`, fish-drop rules, pylons,
shops/travel-shop state, armor-set lookup, WorldGen hooks, item-content repair and chat
initialization.

No unresolved family currently satisfies all qualification fields:

1. one unique server owner;
2. bounded source calls and ordering;
3. observable immutable state;
4. falsifiable invalid/duplicate behavior;
5. persistence or protocol effect where the source exposes one.

Existing child cards cover only narrow definition/registration slices. They do not justify an
aggregate `Simulation.InitializeAlmostEverything`. M-001 remains `planned`.

## M-014: main-thread actions

The source inventory still contains two concrete callers:

- `WorldGen.cs:10529`: `Main.sectionManager.SetAllSectionsLoaded`, owned by host/client section
  loading state;
- `WorldGen.cs:26122`: the arbitrary `mainThreadFollowup` delegate after background generation,
  whose owner is supplied by the caller.

Neither caller provides a typed identity, deterministic sequence/phase key, cancellation or retry
rule, persistence/restart representation, or a stable protocol projection. The existing typed
`SimulationCommandQueue` remains the boundary; no `Queue<Action>` is added. M-014 remains
`unknown/deferred`.

## M-024: delayed processes

`Main.DelayedProcesses` and `Main.DelayedProcessesInGame` remain public mutable
`List<IEnumerator>` collections. A source-tree search found no `.Add` caller, so there is no
reachable named process family to model. The two lists advance in different phases and differ when
`CanPauseGame()` is true, but an `IEnumerator` still has no stable identity, serializable state,
cancellation/disconnect contract, persistence/restart continuation or replay key.

M-024 remains `unknown/deferred`; a future implementation must be a named typed state machine,
not a generic coroutine queue.

## Next executable candidates

The next useful work is to recover one complete named contract, if source evidence appears, for a
single M-001 family or one M-014/M-024 caller. Until then the correct action is additional source
coverage and explicit deferral. Full NPC/projectile/item tables, random starts, complete event
lifecycle and client/presentation branches remain outside this re-audit.
