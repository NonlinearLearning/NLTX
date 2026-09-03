# NPC segment index boundary

`NpcSegmentLifecycleSystem.Validate` now requires `(Root, SegmentIndex)` to be
unique across a segment graph. A reciprocal parent/child graph with duplicate
indexes is ambiguous for deterministic death ordering and persistence restore,
so it fails closed before producing follow-up despawn commands.

This is a typed relationship invariant; it does not reintroduce the legacy
`realLife` numeric field. The focused composition verifier covers valid
child-before-root ordering, worm-only follow-up despawn, non-worm rejection, and
duplicate-index rejection.
