# NPC AI Guide conversation tick checkpoint — 2026-10-07

## Source boundary

This batch adds the player-facing conversation tick for Guide states
`ai[0] == 6/7/18/19` from the read-only source range `NPC.cs:55089-55119`.
The exact profile identity is `type=22 / netID=22 / aiStyle=7`.

Reference source:
`D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`

Reference SHA-256:
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`

The read-only excerpt is preserved in
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/source-conversation-tick-excerpt.txt`.
Source golden comparison is **not-run** because no source golden exists.

## Implemented behavior

`NpcGuideConversationTickProfile` preserves:

- `ai[0] == 18` `localAI[3]` clamping into the source `[1, 2]` range;
- horizontal velocity damping by `0.8f`;
- `ai[1]` decrement;
- invalid conversation target expiry on the same tick;
- facing direction toward a valid target and network update when direction
  changes;
- expiry to `ai[0] = 0`, `60 + Next(60)`, `ai[2] = 0`, and
  `30 + Next(60)` in `localAI[3]`;
- no-op behavior for states outside `6/7/18/19`.

Target validity, line-of-sight, distance, and player talkability are supplied by
the caller. The profile does not access player or world state directly.

## Ownership

| Concern | Owner |
| --- | --- |
| player target snapshot and line-of-sight result | caller-owned target/collision adapter |
| deterministic conversation tick | `NpcGuideConversationTickProfile` |
| expiry random values | `INpcGuideConversationTickRandomPort` |
| network update effect | existing owner through the caller's effect boundary |
| conversation entry and task lifecycle | existing Guide conversation/idle owners and `NpcTaskLifecycleSystem` |

No `RuntimeNpcEntity`, `RuntimeNpcStore`, Simulation `Program`,
`ReferenceVerification`, Blue, Mother, Eye, identity store, Housing/Town owner,
or canonical plan/ledger was modified by this batch.

## Verifier and evidence

The independent Guide verifier covers:

1. valid-target damping, timer decrement, and direction correction;
2. `ai[0] == 18` local marker clamp;
3. invalid-target expiry and source random timers;
4. no-op behavior for non-conversation states;
5. all earlier return-home, resting-spot, ForceSitting, walk prediction,
   day movement, danger, attack, dialogue, idle, traversal, ai0=8, and sitting
   scenarios.

Evidence files are in
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`:

- `build-npc-conversation-tick-final.txt`: NPC project build, exit `0`, zero warnings/errors;
- `build-guide-verifier-conversation-tick-final.txt`: verifier project build, exit `0`, zero warnings/errors;
- `run-guide-verifier-conversation-tick-final.txt`: verifier exit `0` with the Guide `PASS` line;
- `final-fingerprint.txt`: current sorted Guide profile and binary SHA-256 values;
- `guide-profile-input-hashes.txt`: per-file hashes for all sorted `NpcGuide*.cs` inputs.

All generated build artifacts remain under `Build/bin/`.

## Remaining boundary and fallback

The generic `ai[0] == 2/11` idle tick, ordinary animation branches, complete
host composition, host runtime integration, and source differential acceptance
remain open. This checkpoint records the verified player-facing conversation
tick and does not claim complete Guide AI migration or runtime parity.

