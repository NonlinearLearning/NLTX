# Invasion Lifecycle Decision

## Legacy Source Trace

`Main.cs:12958-13033` owns `UpdateInvasion`:

1. It does nothing when `invasionType <= 0`.
2. When `invasionSize <= 0`, it sets a type-specific NPC cleared flag for types 1-4, emits
   progression/achievement effects, announces completion, clears `invasionType` and
   `invasionDelay`, then projects world state.
3. While an invasion is travelling, it moves `invasionX` toward `spawnTileX` by
   `max(dayRate, 1)`, decrements/restarts `invasionWarn`, and announces arrival/warnings.

`Main.cs:13047-13100` owns `StartInvasion`:

- rejects an already active invasion;
- counts active players with `statLifeMax >= 200`;
- derives initial size from type and qualified player count;
- records `invasionSizeStart`, UI progress fields and warning state;
- chooses travel position from `Main.rand` except for type 4;
- starts an NPC damage tracker.

WLD load stores `invasionDelay`, `invasionSize`, `invasionType` and `invasionX`
(`WorldFile.cs:2168-2171` and `3677-3680`).

## Current ECS Boundary

`WorldProgressionState` currently owns only `InvasionType` and `InvasionSize`. The existing
`WorldProgressionSystem` supports server-validated type/size start and deterministic progress
decrements, including normalization to zero. It does not own travel position, delay, original
size, warning timing, qualified-player selection, type-specific clear flags, source random choice,
NPC damage tracking or announcement/protocol effects.

Consequently, current ECS completion normalization is not source-equivalent to `UpdateInvasion`.
It cannot set the required type-specific clear fact and must not claim lifecycle parity.

## Required State Model Before Implementation

An accepted lifecycle card must introduce explicit authoritative owners for:

| Source fact | Required owner | Notes |
|---|---|---|
| `invasionDelay` | immutable progression state | WLD/persistence fact; semantic unit needs source trace |
| `invasionX` | immutable progression state | `Double`; arrival uses spawn coordinate and day rate |
| `invasionSizeStart` | immutable progression state | required for progress projection and restart continuity |
| `invasionWarn` | immutable progression state or transient server event state | must not silently turn chat timing into persisted gameplay state |
| downed Goblins/Frost/Pirates/Martians | named progression facts | cannot be inferred from a generic type/size zero state |
| qualified players | read snapshot | server-owned; no client claim |
| start-side choice | domain random state | needs independently accepted legacy compatibility relation or explicit new-domain behavior |

## Decision

This card is **deferred**. No source field is imported and no invasion transition is modified.
The next implementation must start with one transition only, preferably restoring a persisted
active invasion travel state after the required fields have owners. Completion/clear flags and
random start selection stay separate cards.

## Explicitly Deferred

- NPC spawn tables, NPC damage tracker and enemy AI.
- Client/chat/UI announcements and invasion progress rendering.
- Exact `Main.rand` side-selection parity.
- Type-specific clear flags until named progression facts exist.
- `FakeLoadInvasionStart` reconstruction.
