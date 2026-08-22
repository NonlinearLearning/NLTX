# World Item Pickup Position Boundary

World-item pickup now rejects a non-finite authoritative item position before distance arithmetic.
The existing pickup route already validates player position, pickup range, active identity, delay
and reservation ownership; this card closes the remaining geometry input boundary. A NaN item
position must not pass the distance comparison through unordered floating-point arithmetic.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`, SHA256
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`, lines `13174-13189`.
The legacy server loop treats active world-item state as authoritative for owner processing. The
ECS route keeps that state typed and fails closed before pickup range evaluation.

Accepted predicate: finite item position, finite player position, finite non-negative pickup range,
active item, matching replication id, elapsed pickup delay and authorized player. Deferred: legacy
nearest-owner cadence, shimmer/encumbrance/enemy pickup, item tables and complete lifecycle.
