# NPC AI Guide good-resting and sitting checkpoint — 2026-10-07

## Delta from the return-home slice

This independent checkpoint closes the narrow owner contract around the Guide
home return boundary. It records three acceptance corrections and the
owner-side semantics that can be verified without changing
`RuntimeNpcEntity`, `RuntimeNpcStore`, Simulation `Program`, or the shared
Housing/Town owner.

Source fingerprint remains:

- `D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`
- SHA-256: `ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`
- Source golden: **not-run**; no source golden output is available.

## Acceptance corrections

1. `NPC.cs:56443-56464` always calls `AI_007_TryForcingSitting` after a
   successful home position write and `netUpdate=true`. The source profile now
   always emits `ForceSittingRequested` on success and
   `ApplyEffectsAndObserve` always calls `TryForceSitting` after network sync.
   The old `SittingCandidateAvailable` input is retained only for source-slice
   input compatibility and is ignored. The owner returns whether its attempt
   committed sitting.
2. An eligible return with `CollisionQuery == null` now throws
   `ArgumentNullException`. A missing capability is not converted into
   `NoPath`, `MarkHomeless`, or `QuickFindHome`.
3. `NpcHomeReturnDestinationQuery` now uses the source integer expression
   `width / 2`, and the verifier covers an odd width (`19`) boundary.

## Good resting spot predicate

`NpcGuideGoodRestingSpotQuery` represents the exact Guide-relevant predicate
from `NPC.cs:53525-53545` after the caller has supplied the ideal floor
coordinates. `NpcGuideRestingSpotSearch` also covers the source
`AI_007_FindGoodRestingSpot` loop from `NPC.cs:53546-53628`:

- at night while `ai[0] == 5`, both tile deltas may be at most `7`;
- otherwise the current tile must exactly equal the ideal tile.

- the home floor is descended until the source solid/sloped/platform tile;
- the day and `ai[0] == 5` early-return branches are preserved;
- seat candidates scan `x ± 7`, `y + 2` through `y - 6`, nearest Manhattan
  distance first, with the source frameY filter and chair position correction;
- a currently occupied selected seat returns the floor fallback.

The return profile consumes the resulting `InGoodRestingSpot` fact, so the
caller owns world reads and can re-evaluate occupancy on each tick.

## Force-sitting owner contract

`NpcGuideForceSittingRequest` carries the home floor coordinates. The source
effect port exposes:

```text
bool TryForceSitting(in NpcGuideForceSittingRequest request)
```

The owner is responsible for the real-time helper checks from
`NPC.cs:53630-53668`:

- home coordinates are in world;
- the floor tile is active and is type `15` or `497`;
- type `15` frameY `1080..1098` is rejected;
- current `ai[0]` is not already `5`;
- no active sitting town NPC or sitting player occupies the point at the
  moment of the call.

Only after those checks does the owner consume exactly one
`Next(10800)` value and write, in source order:

```text
ai[0] = 5
ai[1] = 900 + random
direction = tile.frameX != 0 ? 1 : -1
bottom = (homeFloorX * 16 + 8 + 2 * direction, homeFloorY * 16)
velocity = Vector2.Zero
localAI[3] = 0
netUpdate = true
```

The independent verifier uses a narrow owner probe to exercise these rules:
an occupied seat consumes no random value and performs no writes; an available
seat consumes one `Next(10800)` and records all writes; an already sitting
`ai[0] == 5` state also consumes no random value.

## Effect ordering and ownership

Successful source order is now verified as:

```text
home-teleport -> network-sync -> TryForceSitting(owner)
```

The profile does not predict the owner result and does not mutate entity or
Housing state. The owner returns `ForceSittingCommitted`, while the existing
Housing/Town adapter retains responsibility for occupancy, random source,
entity state, and network write.

## Independent verifier and evidence

Verifier project:
`Test/Terraria.NpcAi.GuideProfileVerification/`

It covers:

- `NpcGuideGoodRestingSpotQuery` night radius and daytime exact match;
- `NpcGuideRestingSpotSearch` floor descent, candidate order, frame correction,
  and real-time occupied-seat fallback;
- collision-query contract rejection;
- integer odd-width home geometry;
- unconditional ForceSitting capability call with owner return;
- real-time NPC/player seat occupancy;
- exact `900 + Next(10800)` consumption order;
- `ai[0]`, `ai[1]`, direction, bottom position, zero velocity, `localAI[3]`,
  and `netUpdate` writes;
- shared `GuideReturnHome` task composition.

Evidence directory:
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`

Final fingerprints are recorded in `final-fingerprint.txt`; the Guide source
profile SHA-256 is
`9837DBBF350CB1572D2E9AD02417BF8D4953CD949CD7767FFCB45EF17C3204AC`.

- NPC project build: exit `0`, `0` warnings, `0` errors.
- Guide verifier build: exit `0`, `0` warnings, `0` errors.
- Guide verifier run: exit `0`.
- Final verifier output includes:
  `good-resting-spot predicate and search, narrow force-sitting owner semantics, and shared task composition.`

## Remaining open behavior

This checkpoint does not claim closure for danger selection and attack
transitions, random idle chatter, or the remainder of the `ai[0]` conversation
state machine. The walk and ordinary conversation gate are recorded in the
separate walk/conversation checkpoint; these remaining slices must preserve
their own owners and source evidence.
