# NPC AI Guide ai0=8 continuation checkpoint — 2026-10-07

## Source boundary

This batch closes the Guide-specific continuation of the danger obstruction
state from the read-only source range `NPC.cs:55058-55087`. It applies to the
exact identity `type=22 / netID=22 / aiStyle=7` after the existing danger
profile has entered `ai[0] = 8`.

Reference source:
`D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`

Reference SHA-256:
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`

The read-only excerpt is preserved in
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/source-ai-eight-excerpt.txt`.
Source golden comparison is **not-run** because no source golden exists.

## Implemented behavior

`NpcGuideAiEightProfile` preserves the source continuation for Guide `ai[0] ==
8`:

- horizontal velocity damping by `0.8f`;
- `ai[1]` decrement on every tick;
- danger timer refresh to `180f` when the decremented timer is below `60f` and
  base-range danger remains active;
- expiry transition to `ai[0] = 0`, `ai[1] = 60 + Next(60)`, `ai[2] = 0`,
  and `localAI[3] = 30 + Next(60)`;
- network update requests for timer refresh and expiry;
- no state or velocity change when the caller supplies a non-`ai[0] == 8`
  state.

The profile receives the danger fact as an explicit snapshot. It does not scan
NPCs, access the world, mutate entities, or own the task lifecycle.

## Ownership

| Concern | Owner |
| --- | --- |
| base-range danger fact | existing `NpcGuideDangerScan` caller |
| deterministic ai0=8 timer and velocity transition | `NpcGuideAiEightProfile` |
| `Next(60)` random values | `INpcGuideAiEightRandomPort` |
| network update effect | existing owner through the caller's effect boundary |
| task lifecycle and danger state entry | existing `NpcGuideDangerProfile` / `NpcTaskLifecycleSystem` |

No `RuntimeNpcEntity`, `RuntimeNpcStore`, Simulation `Program`,
`ReferenceVerification`, Blue, Mother, Eye, identity store, Housing/Town owner,
or canonical plan/ledger was modified by this batch.

## Verifier and evidence

The independent Guide verifier covers:

1. velocity damping and danger timer refresh at the `ai[1] < 60` boundary;
2. expiry to idle with both source random timers and state slot writes;
3. no-op behavior for non-`ai[0] == 8` states;
4. all earlier return-home, resting-spot, ForceSitting, walk prediction,
   day movement, conversation, danger, attack, dialogue, idle, traversal, and
   shared-task scenarios.

Evidence files are in
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`:

- `build-npc-ai-eight-final.txt`: NPC project build, exit `0`, zero warnings/errors;
- `build-guide-verifier-ai-eight-final.txt`: verifier project build, exit `0`, zero warnings/errors;
- `run-guide-verifier-ai-eight-final.txt`: verifier exit `0` with the Guide `PASS` line;
- `final-fingerprint.txt`: current sorted Guide profile and binary SHA-256 values;
- `guide-profile-input-hashes.txt`: per-file hashes for all sorted `NpcGuide*.cs` inputs.

All generated build artifacts remain under `Build/bin/`.

## Remaining boundary and fallback

The generic `ai[0] == 5` sitting tick, `ai[0] == 3/4/16/17` player-facing
conversation tick, the full `ai[0] == 8` host composition, ordinary animation
branches, host runtime integration, and source differential acceptance remain
open. This checkpoint records only the verified `ai[0] == 8` continuation and
does not claim complete Guide AI migration or runtime parity.

