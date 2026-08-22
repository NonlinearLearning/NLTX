# NPC Talk Authority Boundary

Legacy `NPC.CanBeTalkedTo` is a small predicate, but it has no current ECS authority owner.

Source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, lines `6492-6501`.

The source requires `isLikeATownNPC`, `aiStyle == 7`, and zero vertical velocity. A nearby
`CanBeTalkedTo` variant also depends on `NPCID.Sets.IsTownPet[type]`. The current Simulation has
no authoritative NPC definition fields for those predicates and no conversation command/session
owner. Protocol `TerrariaSession.AcceptClientTalkNpc` consumes and validates the packet shape but
explicitly documents that Dome has no NPC conversation authority.

Disposition: do not treat packet acceptance as an interaction success and do not infer town-NPC
definitions from absent tables. Defer the predicate until NPC definition and conversation/session
ownership are source-backed.
