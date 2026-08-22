# World Item Store Position Boundary

`WorldItemStore.Add` now rejects non-finite item coordinates before creating the Arch-backed store
entry. This closes the direct persistence/store path that could bypass `WorldItemSpawnSystem` and
register a NaN/Infinity transform. Positive replication identity, duplicate ownership rejection
and inactive tombstone storage remain unchanged.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`, SHA256
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`, active item update and
owner-processing loops around lines `13162-13194`. This card protects store geometry input only;
it does not claim complete owner search or item lifecycle parity.

Accepted: finite position and positive replication identity. Rejected: non-finite position before
Arch store allocation. Deferred: owner search, persistence cadence, shimmer/encumbrance/enemy pickup
and network/client branches.
