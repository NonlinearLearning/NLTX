# NPC AI Guide sitting tick checkpoint — 2026-10-07

## Source boundary

This batch closes the Guide `ai[0] == 5` sitting continuation from the
read-only source range `NPC.cs:55058-55087` for exact identity
`type=22 / netID=22 / aiStyle=7`.

Reference source:
`D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`

Reference SHA-256:
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`

The read-only excerpt is preserved in
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/source-sitting-tick-excerpt.txt`.
Source golden comparison is **not-run** because no source golden exists.

## Implemented behavior

`NpcGuideSittingProfile` preserves the source sitting tick:

- horizontal velocity damping by `0.8f`;
- `ai[1]` decrement on each sitting tick;
- chair/bench tile validation supplied by the world owner;
- explicit sitting registration for a valid seat;
- immediate `ai[1] = 0` invalid-seat expiry;
- expiry transition to `ai[0] = 0`, `60 + Next(60)`, `ai[2] = 0`, and
  `30 + Next(60)` in `localAI[3]`;
- registration before the expiry network effect when both happen in one tick.

The profile does not read or mutate tiles, the sitting manager, NPC entities,
or network state directly.

## Ownership

| Concern | Owner |
| --- | --- |
| tile type and chair/bench classification | caller-owned world/tile adapter |
| deterministic sitting state transition | `NpcGuideSittingProfile` |
| seat registration and NPC identity | existing owner through `INpcGuideSittingEffectPort` |
| expiry random values | `INpcGuideSittingRandomPort` |
| task lifecycle | existing `NpcTaskLifecycleSystem` / `NpcTaskReference` |

No `RuntimeNpcEntity`, `RuntimeNpcStore`, Simulation `Program`,
`ReferenceVerification`, Blue, Mother, Eye, identity store, Housing/Town owner,
or canonical plan/ledger was modified by this batch.

## Verifier and evidence

The independent Guide verifier covers:

1. valid-seat damping, timer decrement, and sitting registration;
2. invalid-seat expiry and both source random timers;
3. explicit sitting-effect ownership and ordering;
4. all earlier return-home, resting-spot, ForceSitting, walk prediction,
   day movement, conversation, danger, attack, dialogue, idle, traversal, and
   `ai[0] == 8` continuation scenarios.

Evidence files are in
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`:

- `build-npc-sitting-tick-final.txt`: NPC project build, exit `0`, zero warnings/errors;
- `build-guide-verifier-sitting-tick-final.txt`: verifier project build, exit `0`, zero warnings/errors;
- `run-guide-verifier-sitting-tick-final.txt`: verifier exit `0` with the Guide `PASS` line;
- `final-fingerprint.txt`: current sorted Guide profile and binary SHA-256 values;
- `guide-profile-input-hashes.txt`: per-file hashes for all sorted `NpcGuide*.cs` inputs.

All generated build artifacts remain under `Build/bin/`.

## Remaining boundary and fallback

The full `ai[0] == 3/4/16/17` player-facing conversation tick, generic
`ai[0] == 2/11` idle tick, ordinary animation branches, host runtime
integration, and source differential acceptance remain open. This checkpoint
records the verified sitting continuation and does not claim complete Guide AI
migration or runtime parity.

