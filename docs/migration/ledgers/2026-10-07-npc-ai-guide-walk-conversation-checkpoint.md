# NPC AI Guide walk and conversation checkpoint — 2026-10-07

## Scope and source

This checkpoint adds two pure Guide slices without touching the runtime host:

- `AI_007_TownEntities_GetWalkPrediction` at `NPC.cs:56475-56555`;
- the player conversation gate at `NPC.cs:54096-54116`.

Reference source:
`D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`

Reference SHA-256:
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`

Source golden comparison is **not-run** because no source golden output is
available.

## Walk prediction API

`NpcGuideWalkPrediction` requires `type=22 / netID=22 / aiStyle=7` and accepts
the source tile and collision facts through explicit inputs:

- home X range (`homeFloorX ± 35`);
- town NPC crowd scan and stationary-friendly-NPC overlap;
- current drowning state;
- direction toward or away from home;
- six-tile source scan (`j=-1..4`) with liquid, lava, and solid samples;
- height-based liquid depth and owner-provided landing drown result.

The result preserves `keepwalking`, `avoidFalling`, liquid count, and branch
flags. It covers the source ordering: crowd/drowning keep-walking first,
outside-home-range fall-avoidance release, tile scan, lava/full-liquid risk,
and landing drown risk.

Tile reads and collision results remain owner queries. `CanBreatheUnderWater`
is retained as an explicit source input even though this helper itself only
uses the caller-computed `currentlyDrowning` value, matching the source
helper's signature and read boundary.

## Conversation API

`NpcGuideConversationProfile` preserves the source player-talking gate:

- no talking player leaves state unchanged;
- `ai[0]` states `10`, `12`, `14`, and `15` remain owned by specialized
  conversation branches;
- all other talking states reset `ai[0]=0`, `ai[1]=300`, and `localAI[3]=100`;
- direction faces the talking player using the source left/equal/right rule;
- a nonzero prior `ai[0]` requests network synchronization.

`INpcGuideConversationEffectPort` owns the network effect; the profile only
returns the state transition and ordered request.

## Verifier coverage

The independent project
`Test/Terraria.NpcAi.GuideProfileVerification/` now covers:

- safe solid landing inside home range;
- outside-range movement toward home;
- drowning keep-walking;
- crowded stationary-friendly-NPC keep-walking;
- liquid landing drown risk and lava risk;
- ordinary Guide conversation reset and direction;
- preservation of specialized conversation state.

The same run also covers the earlier return-home, full resting-spot search,
and narrow ForceSitting owner checkpoint scenarios.

Evidence directory:
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`

The final fingerprints are in `final-fingerprint.txt`; the source profile
SHA-256 remains
`9837DBBF350CB1572D2E9AD02417BF8D4953CD949CD7767FFCB45EF17C3204AC`.

- NPC project build: exit `0`, `0` warnings, `0` errors.
- Guide verifier build: exit `0`, `0` warnings, `0` errors.
- Guide verifier run: exit `0`.
- Final output includes `walk prediction, conversation state, and shared task composition`.

## Remaining source slices

Danger selection and attack transitions, random idle chatter, and the rest of
the `ai[0]` conversation/attack state machine remain open. The host still owns
entity mutation and task composition; no host wiring was added in this batch.
