# Item.NewItem qualification boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Item.cs`
  - source SHA-256: `932AA78145344BF9D825FB42ECD71A732BDDCE36474442D9CCB7B06A6F385E55`
  - overload/guard/remap/slot path: lines 48686-48789
- The source branch remaps seasonal types, aggregates cached spawns, selects/reuses a legacy item
  slot, applies defaults/prefix, performs wet collision, consumes global `Main.rand` for velocity,
  and emits network/client state.

## Qualification result

| Predicate | Result | Evidence |
|---|---|---|
| bounded API | yes | two overloads with explicit type/stack/position inputs |
| deterministic server-only behavior | no | seasonal flags, ItemID sets, global Main.rand, collision and network/client state |
| unique existing owner | partial | `WorldItemSpawnSystem` owns explicit typed world-item creation but not legacy slot/cache/random behavior |
| replay/persistence contract | no | random initial velocity, cached slot aggregation and time-based item flags are not represented together |
| protocol projection | partial | world-item replication exists, but source network broadcast and slot reuse semantics differ |

## Decision

Legacy `Item.NewItem` remains `unknown/deferred`. The existing explicit `CreateWorldItemCommand`
route is retained as a separate deterministic server primitive; it must not be described as full
`NewItem` parity. No global RNG fallback, seasonal remap, legacy slot cache or client new-item flag
was added.

## Prerequisites

1. Named item spawn random stream and persistence/replay state.
2. Server-owned ItemID seasonal/variant policy and cached spawn aggregation semantics.
3. Revisioned slot/instance identity and collision/velocity projection contract.
4. Network/client presentation boundary separate from authoritative world-item creation.
