# NPC AI Guide day movement checkpoint — 2026-10-07

## Source boundary

This batch adds the source-shaped multi-tick transitions for Guide ordinary
`ai[0] == 0` and `ai[0] == 1` from
`NPC.cs:54406-54623`. The surrounding door, gate, jump, step, and tile
mutation code continues to be represented as caller-owned input/effect space;
this batch does not pretend to migrate those side effects.

Reference source:
`D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`

Reference SHA-256:
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`

The read-only source excerpt is
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/source-day-movement-excerpt.txt`.
Source golden comparison is **not-run** because no source golden exists.

## Implemented behavior

`NpcGuideDayMovementProfile` preserves the Guide-relevant source transitions:

- default direction normalization from `0` to `1`;
- `localAI[3]` decrement in the idle state;
- weather-return pressure branch for a server-authoritative, non-talking,
  non-critter Guide;
- home-floor horizontal deceleration and ForceSitting request;
- home-facing `ai[0] = 1` entry with `200 + Next(200)`;
- good-resting-spot return from `ai[0] = 1` with `200 + Next(200)` and
  `localAI[3] = 60`;
- drowning timer preservation;
- outside-home-range directional timer acceleration by `5`;
- ordinary timer decrement and `300 + Next(300)` expiry transition;
- Guide horizontal speed `1f`, acceleration `0.07f`, and grounded damping
  `0.8f`;
- explicit network update and ForceSitting effect requests.

The result is keyed to the input state's original `ai[0]`, so a state write
from the `ai[0] == 0` branch does not incorrectly execute the `ai[0] == 1`
branch again in the same tick.

## Ownership

| Concern | Owner |
| --- | --- |
| floor/home/resting facts | Housing/Town caller snapshot |
| drown and tile collision facts | spatial/collision caller snapshot |
| deterministic state transition | `NpcGuideDayMovementProfile` |
| random timer values | `INpcGuideDayMovementRandomPort` |
| sitting and network effects | existing owners through `INpcGuideDayMovementEffectPort` |
| doors, gates, stepping, jumping, and tile writes | existing movement/world owner; still open |
| task lifecycle | existing `NpcTaskLifecycleSystem` / `NpcTaskReference` |

No runtime host, entity store, Simulation `Program`, ReferenceVerification,
Blue, Mother, Eye, Leashed/TileEntity path, or canonical plan/ledger was
modified by this batch.

## Verifier and evidence

The independent Guide verifier covers:

1. `ai[0] == 0` at home floor, damping to zero, and ForceSitting request;
2. home-facing walk entry and `200 + Next(200)` timer;
3. good-resting-spot return to `ai[0] == 0`;
4. timer expiry and `300 + Next(300)` idle timer;
5. drowning timer preservation while horizontal movement continues;
6. all earlier return-home, resting-spot, walk prediction, danger, attack,
   dialogue, idle, conversation, and shared task lifecycle scenarios.

The NPC project and independent verifier build/run evidence remains in
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`. The final
fingerprint records `sourcegolden=not-run`.

## Remaining boundary and fallback

The source door/gate open-close effects, jump/step collision calls, drowning
escape impulse, `ai[0] == 8` obstruction state, and ordinary town-NPC
animation branches remain open. If a later differential changes timer or
movement ordering, retain this checkpoint as the fallback and add a new
source excerpt plus verifier scenario rather than changing the recorded
source boundary silently.
