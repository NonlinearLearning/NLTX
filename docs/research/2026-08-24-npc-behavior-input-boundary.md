# NPC Behavior Input Boundary

The supported NPC behavior owner now rejects non-finite current or target positions before
evaluating chase or town-home movement. This prevents unordered floating-point comparisons from
producing a false home arrival or a zero-direction chase. Valid behavior state and movement rules
remain unchanged. The related `NpcHomeSystem` also rejects non-finite current positions before
mutating movement intent or timeout state.

Source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, home/AI movement branches
around lines `44094-44100` and `45698-45726`. The source uses finite tile/home state and position
updates during the NPC AI phase; this card protects the supported ECS equivalent from forged numeric
inputs without claiming type-specific AI parity.

Accepted: finite NPC and target positions, existing ordinary-chase/town-home behavior and typed
movement state. Rejected: non-finite positions before any state mutation. Deferred: complete AI
families, pathfinding, collision/LOS, home search/teleport, type tables and client behavior.
