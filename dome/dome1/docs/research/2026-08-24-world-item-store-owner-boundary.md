# World Item Store Owner Boundary

`WorldItemStore.RecordPickup` now rejects an invalid `PlayerHandle` before writing the runtime
`ItemOwnershipComponent`. This closes the direct store mutation path independently of the higher
level pickup authorization route; a default/zero handle cannot become an authoritative owner.
Unknown replication IDs continue to reject, and valid owners preserve the existing ownership
revision/source projection.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`, SHA256
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`, item owner/reservation loop
around lines `13162-13194`. The source distinguishes unreserved and active player ownership; this
card enforces the ECS identity precondition without implementing owner search cadence.

Accepted: positive player handle and existing world-item replication identity. Rejected: default or
non-positive player handle before ownership mutation. Deferred: nearest-owner search, reservation
cadence, restart persistence, shimmer/encumbrance/enemy pickup and client/network effects.
