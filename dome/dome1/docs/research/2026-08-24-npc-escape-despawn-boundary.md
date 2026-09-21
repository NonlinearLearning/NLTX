# NPC Escape and Despawn Boundary

Source oracle: Version4 `Terraria/NPC.cs`, `EncourageDespawn` at lines `7143-7154` and AI call
sites including `AI_002_FloatingEye` around `42447`. `NpcEscapeSystem` turns the supported
server-side slice into typed output: finite identity/positions are required; an NPC inside the
configured radius receives outward movement intent, while radius overflow emits
`DespawnNpcReason.OutOfRange` and zero `timeLeft` emits `TimedOut`.

The system does not mutate lifecycle state, teleport, or bypass the existing despawn commit. Full
per-type despawn encouragement, player-distance policy, collision/LOS, and client presentation
remain deferred.
Escape/despawn authority is now hostile-faction-only. The faction is a required input (there is no
default), so callers cannot silently bypass the boundary. Town and neutral NPCs are rejected before
out-of-range or timed-out escape commands can be emitted; their town/passive lifecycle remains
owned by the corresponding home/lifecycle systems.

Evidence: `Build/diagnostics/npc-complete/task-5-escape/20260824-150000/`.
