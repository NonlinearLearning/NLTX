# Tile Command Sequence Overflow Boundary

`TileChangeCommitSystem` now rejects `long.MaxValue` command sequences before any tile mutation.
The commit result's `NextSequence` is computed as the largest sequence plus one; accepting the
maximum value would overflow that authoritative cursor after applying the batch. Negative,
duplicate, out-of-world and invalid-kind checks remain unchanged, as do deterministic ordering and
atomic application for valid batches.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`, SHA256
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`, mutable tile update ownership
and world update flow. This card is a deterministic command-boundary repair, not full legacy tile
mutation parity.

Accepted: sequence in `[0, long.MaxValue)`, valid coordinates/kind and unique batch sequence.
Rejected: maximum sequence before mutation. Deferred: complete Main tile update/client/network
branches.
