# Invasion Progress Clamp Boundary

The legacy invasion route decrements `Main.invasionSize` from NPC invasion damage and clamps the
remaining size at zero (`NPC.cs:64791-64799`). It then reports progress from
`invasionSizeStart - invasionSize`; completion is handled by `Main.UpdateInvasion` after the zero
state is exposed.

The current typed route preserves this boundary: positive, sequenced progress commands are
deduplicated within a pending tick, `WorldProgressionSystem` applies `Math.Max(0, remaining -
amount)`, and zero normalizes the invasion while publishing one typed completion event. No upper
bound rejection was added because the source explicitly clamps oversized damage rather than
rejecting it.

Source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, lines `64765-64800`.
Existing WorldRules verification covers duplicate sequence, oversized completion, persistence and
typed clear flag. Random NPC damage amounts, NPC tables and announcements remain deferred.
