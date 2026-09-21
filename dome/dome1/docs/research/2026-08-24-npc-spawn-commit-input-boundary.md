# NPC spawn commit input boundary

`NpcSpawnCommitSystem` is the server-side authority for materializing a
`SpawnNpcCommand`. It now rejects non-finite difficulty scales, undefined
`NpcSpawnSource` values, release owners outside the byte-sized SyncNPC domain,
and difficulty values that would produce an unrepresentable health total.

This keeps event spawn output typed through commit: `Event` source and its
difficulty are not merely eligibility hints and cannot be forged into an entity
with invalid replication/protocol state. The focused NPC verifier covers these
three forged-input paths along with unknown definitions, non-finite positions,
and replication identity overflow.
