# NPC AI Guide traversal checkpoint — 2026-10-07

## Source boundary

This batch adds the Guide-specific traversal decisions from the read-only source
range `NPC.cs:54624-55030` for exact identity `type=22 / netID=22 /
aiStyle=7`. The profile keeps world tile reads, collision probing, door/gate
mutation, stepping, and network packet ownership outside the deterministic
calculation.

Reference source:
`D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`

Reference SHA-256:
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`

The read-only excerpt is preserved in
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/source-traversal-excerpt.txt`.
Source golden comparison is **not-run** because no source golden exists.

## Implemented behavior

`NpcGuideTraversalProfile` now preserves the Guide traversal decisions for:

- pending close-door and close-tall-gate requests after leaving the source
  closing range;
- the source `Next(10) == 0 || return-pressure` door-open gate, including
  random consumption even when return pressure is active;
- opening in the current direction, opening against the direction, tall-gate
  fallback, `ai[1] += 80`, and direction reversal when all open attempts fail;
- three-tile, two-tile, and one-tile jump velocities `-6f`, `-5f`, and
  `-4.4f`, with the caller-provided collision facts and source slope/landing
  gate;
- dangerous obstruction transition to `ai[0] = 8`, `ai[1] = 240`, zero
  horizontal velocity, and network update;
- reverse direction and horizontal velocity reversal for unsafe crossings;
- the source keep-walking timer `ai[1] = 90`, suppressed when the obstruction
  branch has taken ownership;
- avoid-falling recovery to `ai[0] = 0`, `50 + Next(50)`, `ai[2] = 0`, and
  the source liquid-marker ordering;
- wet drowning escape impulse from the caller-provided liquid depth, the
  `26f` cap, town-critter/wet jump multipliers, and the source `localAI[3]`
  landing marker;
- liquid timer clamping/decrement and explicit door-effect then network-effect
  ordering.

The profile does not mutate tiles, NPC entities, or network state directly.
Door and network effects are returned through `INpcGuideTraversalEffectPort`.

## Ownership

| Concern | Owner |
| --- | --- |
| door tile type and open/close success | caller-owned world/tile adapter |
| collision, slope, support count, drowning, and walk prediction facts | caller-owned spatial/collision adapter |
| deterministic traversal state transition | `NpcGuideTraversalProfile` |
| source random rolls and recovery timer | `INpcGuideTraversalRandomPort` |
| door mutation and packet effects | existing owner through `INpcGuideTraversalEffectPort` |
| task lifecycle | existing `NpcTaskLifecycleSystem` / `NpcTaskReference` |

No `RuntimeNpcEntity`, `RuntimeNpcStore`, Simulation `Program`,
`ReferenceVerification`, Blue, Mother, Eye, identity store, Housing/Town owner,
or canonical plan/ledger was modified by this batch.

## Verifier and evidence

The independent verifier covers:

1. three-tile jump velocity and landing marker;
2. dangerous obstruction state and keep-walking suppression;
3. ordinary door opening and effect order;
4. close-door request and tall-gate fallback;
5. drowning escape impulse and liquid marker;
6. avoid-falling recovery and source random timer;
7. no door-roll consumption when no door candidate exists;
8. all earlier return-home, resting-spot, ForceSitting, walk prediction,
   day movement, conversation, danger, attack, dialogue, idle, and shared-task
   scenarios.

Evidence files:

- `build-npc-traversal-final.txt`: `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore`, exit `0`, zero warnings/errors;
- `build-guide-verifier-traversal-final.txt`: verifier project build, exit `0`, zero warnings/errors;
- `run-guide-verifier-traversal-final.txt`: verifier exit `0` with the Guide `PASS` line;
- `final-fingerprint.txt`: profile, NPC binary, and verifier binary SHA-256 values with `sourcegolden=not-run`;
- `guide-profile-input-hashes.txt`: per-file hashes for the sorted `NpcGuide*.cs` profile inputs.

All generated build artifacts remain under `Build/bin/`.

## Remaining boundary and fallback

The caller still owns the complete collision/step implementation and the
actual door/gate world mutation. The full `ai[0] == 8` continuation, ordinary
town-NPC animation branches, host runtime integration, and source differential
acceptance remain open. This checkpoint records the verified traversal slice;
it does not claim complete Guide AI migration or runtime parity.

