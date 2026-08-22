# Player Authority Persistence Design

## Decision

The server owns a player record after the first successful bootstrap import for a
client UUID. The client may supply bootstrap state only when no record exists.
When a record exists, the server parses the frames for protocol conformance but
restores the server record before the client becomes active. Client-supplied
inventory, buff, equipment, and loadout state must not overwrite that record.

## Evidence

The captured original Terraria client bootstrap sends these frames after
`SyncPlayer` and before `RequestWorldData`:

1. `PlayerUuid` (68)
2. `PlayerLifeMana` (16)
3. `ItemRotationAndAnimation` (42, player mana in the reference)
4. `PlayerBuffs` (50)
5. `SyncLoadout` (147)
6. `SyncEquipment` (5) for player item slots

`SyncEquipment` uses the 990 global slot IDs defined by the reference
`PlayerItemSlotID` class. Its payload includes player slot, global slot ID,
stack, prefix, item type, and item flags. The wire data is richer than the
current ten-slot `InventoryComponent` and must not be truncated on import.

## Boundaries

### Protocol

`Terraria.Dome.Protocol.V1456` owns exact framing and packet parsers. It
exposes immutable bootstrap DTOs for UUID, player vitals, buffs, loadout, and
equipment. Parsers validate exact packet layouts, packet-owned player slots,
slot ranges from 0 through 989, bounded buff termination, non-negative stack
counts, and a canonical UUID.

### Server session

`TerrariaSession` owns a mutable bootstrap accumulator only before
`RequestWorldData`. It admits the captured bootstrap packet family in any
reference-valid order after the player profile. The accumulator becomes an
immutable import request once the world request arrives. Active sessions do
not accept bootstrap equipment or stat updates as state mutations.

### Simulation

`Terraria.Dome.Simulation` owns `PlayerPersistentState`, including the full
990-slot protocol inventory, vital values, buffs, selected loadout, visibility
flags, UUID, and profile. The existing ten-slot `InventoryComponent` remains a
runtime projection for supported item-use systems. Items outside the current
item catalog are retained in persistent state but cannot be executed by the
simulation.

### Persistence

The Dome state format advances from version 1 to version 2. Version 2 appends
an account-record collection keyed by canonical UUID. Version 1 payloads
continue to read as an empty account collection. Saving serializes sorted
records with explicit maximum counts and fixed-size slot arrays; loading
rejects malformed UUIDs, duplicate account keys, malformed item data, and
trailing bytes.

### Replication

Before world entry, the server sends the account record projected into original
message shapes: profile, vital state, buffs, loadout, and complete equipment
slots. On a first import, it sends the imported record. On later connections,
it sends the restored server record after draining and validating the
client-provided bootstrap frames.

## Security Invariants

- A bootstrap packet carrying a player slot must match the assigned session
  slot.
- A UUID identifies one server record and is canonicalized before dictionary or
  persistence use.
- Only a missing UUID record may be initialized from client bootstrap data.
- Existing records are never overwritten by connection-time client state.
- Active-session item, buff, vital, or loadout frames cannot mutate persistent
  state through the bootstrap path.
- Runtime item use is limited to the existing server-known item definitions.

## Verification

- Replay the captured `4 -> 68 -> 16 -> 42 -> 50 -> 147 -> 5... -> 6`
  bootstrap sequence and require it to reach `RequestWorldData`.
- Persist, restore, and compare every field in a record containing all 990
  equipment slots, buffs, vitals, profile, and loadout state.
- Connect a second time with the same UUID and deliberately different client
  equipment. Require the server record to survive unchanged and be replicated
  back to the client.
- Reject a cross-slot frame, an out-of-range equipment slot, a malformed UUID,
  an unterminated buff list, and a post-active bootstrap mutation.
