# NPC Valid-Target Ghost Boundary

The player-target branch of legacy `NPC.HasValidTarget` is independently recoverable: an active,
living, non-ghost player is valid; inactive, dead or ghost players are rejected.

Source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, lines `6504-6519`.

The ECS route adds `NpcTargetCandidate.IsGhost` and filters it in
`NpcTargetSelectionSystem.SelectTarget` before nearest-distance and stable-id tie breaking.
Existing behavior for active, healthy non-ghost players is unchanged.

Deferred: legacy NPC-target selection (`SupportsNPCTargets`, target ids `300+`), type-specific
targeting rules, line-of-sight and AI-style target changes. This card accepts only the generic
player-target validity guard.
