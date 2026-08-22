# NPC Spawn Position Boundary

`NpcSpawnCommitSystem` now rejects non-finite spawn coordinates even when called directly, before
Arch entity creation. This closes the bypass where a caller could skip
`NpcSpawnEligibilitySystem` and commit a NaN/Infinity transform. Existing definition, replication,
difficulty and occupancy checks remain unchanged; valid spawn commands still receive their typed
identity, lifecycle and replication components.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, natural spawn/new-NPC
coordinate checks around lines `352-376` and `5171`. The card accepts only the numeric input
boundary, not the complete source spawn search or rate algorithm.

Accepted: finite position, positive replication identity, known definition, positive difficulty and
unoccupied world-grid footprint where applicable. Rejected: non-finite position before entity
allocation. Deferred: spawn search/rate formulas, type tables, random starts and client effects.
