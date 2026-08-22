# World Item Motion Integrity Slice

`WorldItemMotionSystem.TryMove` now rejects non-finite target X/Y values before changing position,
section or revision. The existing active-item and expected-revision checks remain authoritative.
This card covers only numeric integrity for world-item movement; range, ownership, persistence and
network visibility remain separate contracts.
