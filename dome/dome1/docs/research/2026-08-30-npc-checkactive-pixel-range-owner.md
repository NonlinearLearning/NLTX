# NPC `CheckActive` pixel-range owner boundary (2026-08-30)

## Result

This batch adds a typed, pure pixel-space query contract for the two geometric ranges used by
`NPC.CheckActive`. The boundary is `verified` as a geometry owner and remains `partial` as an
NPC lifecycle migration. It does not claim that the complete `CheckActive` method has moved to
Simulation.

Overall status remains:

> **NPC field/property migration: partial.**

## Legacy source contract

The source oracle is `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs` (SHA-256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`). The relevant source
anchors are:

- `NPC.cs:64448` constructs an active rectangle around the NPC centre using
  `activeRangeX`/`activeRangeY`.
- `NPC.cs:64449` constructs a second screen-range rectangle using `sWidth`, `sHeight`, and the
  NPC pixel width/height.
- `NPC.cs:64450-64457` scans active player hitboxes and uses strict rectangle intersection.
- `NPC.cs:6047-6055` derives `activeRangeX = 4032`, `activeRangeY = 2520`, and
  `activeTime = 750` from a `1920 x 1200` screen.
- `NPC.cs:64460-64470` performs per-player `nearbyActiveNPCs` accounting; that mutable accounting
  is deliberately outside this owner.
- `NPC.cs:64476-64480` refreshes `timeLeft`; the timer write is deliberately outside this owner.

The source rectangles and hitboxes are pixel-space. The source casts the computed rectangle
origins to `int`, and edge-only contact is not an intersection. The owner preserves those
observable geometry rules for representable inputs.

## Typed owner

The implementation is in `src/Terraria.Dome.Simulation/Npc/Systems`:

| Type | Responsibility |
| --- | --- |
| `NpcPixelBounds` | Finite pixel-space NPC position with positive integer width and height. |
| `NpcPixelRectangle` | Validated integer pixel rectangle with strict intersection semantics; overlap arithmetic uses `long` intermediates. |
| `NpcActivityRangeDefinition` | Positive range/screen dimensions; `Version1456` freezes `4032 x 2520` and `1920 x 1200`. |
| `NpcPlayerActivity` | Explicit active flag and pixel-space player hitbox input. |
| `NpcActivityRangeDecision` | Immutable `HasActivePlayerInRange` and `ShouldRefreshInactivityTimer` result. |
| `NpcActivityRangePolicy` | Pure query that constructs both rectangles, skips inactive players, and returns the decision without mutating timers or entity state. |

The policy validates finite NPC coordinates, positive dimensions, positive range values, and
active-player hitboxes. Coordinate conversion rejects non-finite or unrepresentable integer
origins instead of allowing an overflowed rectangle to influence lifecycle state. No caller-owned
collection is mutated.

The active rectangle uses the source's integer half-width/half-height and the source cast order.
The screen rectangle uses the source's half-screen expansion and the NPC dimensions. A player
may refresh the screen-range result without satisfying the wider active-range result, matching
the fact that the legacy method computes the two predicates separately.

## Deliberate stop boundary

This owner is not wired into `DomeSimulation`, `NpcLifecycleSystem`, or a protocol/snapshot
projection. In particular, it does not introduce:

- a pixel-to-tile conversion for `TransformComponent`, `WorldGrid`, or collision systems;
- a `Main.player[0..254]` mirror or a replacement for `PlayerStore`/`AssignedSlot` semantics;
- `nearbyActiveNPCs`, `npcSlots`, slime-rain scaling, or the `releaseOwner == 255` rule;
- generic NPC `ai[]` slots, boss/type/day-time exceptions, or the type-690 guard;
- `timeLeft`, `despawnEncouraged`, `noSpawnCycle`, `active`, `life`, SyncNPC, revenge, or worm
  cleanup side effects.

Consequently, full player-range `CheckActive`, timer refresh integration, slot accounting,
deactivation, collision/recovery, unspawn, persistence/network parity, complete AI families, and
legacy WorldGen deletion remain `partial`/`deferred`; `canRemoveLegacyWorldGen` remains `false`.

## Verification

- TDD RED: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-pixel-range-red/`
  records verifier build exit code `1`, with only the missing pixel-range owner types reported.
- TDD GREEN: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-pixel-range-green/`
  records the verifier build, focused run, Simulation Release build, and Server Release build.
  Each completed with exit code `0` and zero warnings/errors.
- The focused verifier output includes `PASS: NPC CheckActive pixel-range policy` and the
  existing NPC lifecycle/definition checks.
- `git diff --check` and the private checkpoint/model-context JSON parses are required final
  hygiene gates for this batch.

The owner therefore establishes a reusable, source-backed coordinate/player-range input contract,
but not an end-to-end lifecycle replacement.
