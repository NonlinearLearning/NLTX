# NPC Target Selection Input Boundary

The NPC target-selection route now fails closed before nearest-distance evaluation when the
authoritative input is unusable. A candidate must have a non-default Arch `Entity`, a positive
stable player id, an active state, positive health, a non-ghost state and finite coordinates. The
NPC position must also be finite. Invalid candidates are ignored; an invalid NPC position produces
`NoValidTarget`.

The source-backed gameplay predicate is the player branch of `NPC.HasValidTarget`: active, living,
non-ghost players are valid and inactive, dead or ghost players are rejected. Source:
`D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, lines `6504-6519`.

The accepted ECS behavior preserves nearest-distance selection and stable-id tie breaking after the
boundary. Focused coverage includes a default entity, non-positive identity, non-finite candidate
position, non-finite NPC position, inactive, dead and ghost candidate cases.

Deferred: legacy NPC-target routing (`SupportsNPCTargets`, translated target ids including `300+`),
type-specific targeting, line-of-sight, AI-style target changes, conversation authority and NPC
static/AI tables. This card does not claim complete NPC lifecycle or target parity.
