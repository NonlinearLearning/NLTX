# NPC segment follow boundary

`NpcSegmentFollowSystem` adds the first typed movement slice for non-root
segments. It requires a valid root and parent handle, consumes explicit
segment/parent positions plus `NpcChaseState`, and emits a normalized bounded
velocity with deterministic facing. Invalid handles, non-finite positions, and
invalid speed/stopping distance are rejected before an intent is produced.

The system deliberately accepts parent position as an input contract. The
current Arch world does not expose a verified `NpcHandle` to `Entity` mapping,
so this change does not silently treat a player target as a segment parent.
World query/wiring remains a separate follow-up.

Evidence:

- `Build/diagnostics/npc-complete/task-7-segment-follow/20260824-080000/simulation-build.log`
- `Build/diagnostics/npc-complete/task-7-segment-follow/20260824-080000/npc-focused.log`
- `Build/diagnostics/npc-complete/task-7-segment-follow/20260824-080000/summary.txt`
- `Build/diagnostics/npc-complete/task-7-segment-follow/20260824-083000/simulation-build.log`
- `Build/diagnostics/npc-complete/task-7-segment-follow/20260824-083000/composition-focused.log`
- `Build/diagnostics/npc-complete/task-7-segment-follow/20260824-083000/summary.txt`

Both build and composition verifier exit `0`. Segment constructors and lifecycle
validation now reject undefined life policies. Complete worm movement, shared-life
parity, and parent-position world wiring remain partial/deferred.
