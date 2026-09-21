# NPC Interaction Authority Boundary

The existing generic player interaction component covers tiles, chests, signs and items, not NPC
services. `NpcInteractionSystem` therefore emits a separate typed command containing player/NPC
identity, session identity, interaction kind and target position. It accepts only active valid
identities, a non-empty session and a finite position inside the configured range.

The composition verifier covers a valid town talk intent plus range and empty-session rejection.
Shop inventories, dialogue tables, pylons, capture/release and client presentation remain deferred;
the command is an authority boundary, not an empty service implementation.
NPC interaction creation now requires an explicit `NpcFaction.Town` input.
Hostile and neutral NPCs cannot create Talk, Shop, or Home commands even when the session,
activity, and distance checks pass.

Evidence: `Build/diagnostics/npc-complete/task-5-escape/20260824-150000/`.

`NpcHomeSystem` also rejects a negative timeout or town variant if a mutable
Arch component has been corrupted after construction.

Additional evidence: `Build/diagnostics/npc-complete/task-10-interaction/20260824-153000/`.
