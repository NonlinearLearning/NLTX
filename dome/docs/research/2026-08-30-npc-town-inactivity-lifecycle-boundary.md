# NPC town-inactivity lifecycle boundary (2026-08-30)

## Result

This batch verifies a narrow source-backed owner for the legacy `townNPC` early-return branch in
`NPC.CheckActive`. It is `verified` for the exact static net-id predicate and lifecycle timer gate,
and remains `partial/deferred` for the rest of `CheckActive` and full NPC parity.

## Legacy source evidence

Oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`.

- `NPC.cs:6397` declares the instance `townNPC` field, while `NPC.cs:8218` resets it to `false`
  during `SetDefaults`.
- The only `townNPC = true` writes are 32 static branches that expand to 39 effective net IDs:
  `17, 18, 19, 20, 22, 37, 38, 54, 107, 108, 124, 142, 160, 178, 207, 208, 209, 227,
  228, 229, 353, 368, 369, 441, 550, 588, 633, 637, 638, 656, 663, 670, 678, 679, 680,
  681, 682, 683, 684`. The source branches at lines `16917` and `17304` contain the
  `637 || 638` and `678..684` alternatives.
- `NPC.cs:64442` includes `townNPC` in the first `CheckActive` guard, before player rectangles,
  AI, event, revenge, collision, and worm cleanup branches.
- `NPC.cs:6671-6681` proves that `isLikeATownNPC` is a different getter: type `453` returns
  `true` without writing `townNPC`. The dynamic `townNPCCanSpawn` candidate table is also a
  separate world-generation concern and is not used by this owner.
- `NPC.cs:17388-17403` shows type `690` has a separate `ai[0] == 0f` early-return condition and
  is not part of the static `townNPC` set.

## ECS owner and boundary

`LegacyNpcTownRegistry` in `Npc/Definitions` owns the exact 39 effective static source IDs and validates the
`0..696` legacy net-id domain. `DomeSimulation` reads the typed
`NpcDefinitionComponent.NetId` and passes the pure result to `NpcLifecycleSystem`. The lifecycle
gate skips only inactivity-timer decrement for a positive-health town NPC; lethal health still
transitions to `Killed`, and `isImmortal` retains its earlier priority. No NPC component, snapshot,
protocol field, persistence field, faction inference, or `IsLikeTownNpc` alias was added.

## Verification evidence

All commands ran from the repository root with serial MSBuild settings
`UseSharedCompilation=false`, `MSBuildNodeReuse=false`, and `-m:1`:

- TDD RED verifier build: `Build/diagnostics/npc-complete/task-10-interaction/20260830-town-inactivity-lifecycle-red/verifier-build-red.log`, exit code `1`, with missing registry and lifecycle API errors.
- Corrected GREEN focused verifier build: `.../20260830-town-inactivity-lifecycle-green/verifier-build-corrected.log`, exit code `0`, with 0 warnings and 0 errors.
- Corrected GREEN focused verifier run: `.../20260830-town-inactivity-lifecycle-green/verifier-corrected.log`, exit code `0`, including `PASS: NPC source-backed townNPC registry and lifecycle gate` and all existing NPC checks.
- Corrected source exact-set audit: `.../20260830-town-inactivity-lifecycle-green/source-audit-corrected.log`, exit code `0`; it records 32 assignment/branch sites, 39 effective IDs, 39 registry IDs, and empty missing/extra sets. The earlier `source-audit.log` is superseded because it counted branches without expanding multi-ID conditions.
- Corrected Simulation Release build: `.../20260830-town-inactivity-lifecycle-green/simulation-build-corrected.log`, exit code `0`, with 0 warnings and 0 errors.
- Corrected Server Release build: `.../20260830-town-inactivity-lifecycle-green/server-build-corrected.log`, exit code `0`, with 0 warnings and 0 errors.
- Corrected NPC protocol shape build/run: `.../npc-protocol-build-corrected.log` and `.../npc-protocol-corrected.log`, both exit code `0`; the existing `SyncNPC` shape remains unchanged.
- The original TDD RED build remains `.../town-inactivity-lifecycle-red/verifier-build-red.log`, exit code `1`, with the expected missing-registry/lifecycle API errors. The later `verifier-red-rerun.log` exercised the stale 32-ID expected set and is retained only as historical evidence.
- Final checkpoint parse: `.../checkpoint-json-parse-final-corrected.log` and
  `.../model-context-json-parse-final-corrected.log` both report `status=PASS`; the checkpoint
  contains 32 batch artifacts with `batch_missing_count=0`. Final hygiene:
  `.../diff-check-corrected.log`, exit code `0`; only existing LF-to-CRLF conversion warnings
  were emitted and no whitespace errors were reported.

## Deferred boundary

Player `safeRange`/`activeRange` scans, `Main.player` state, type-specific AI and event/boss
exceptions, type `690` AI behavior, `noSpawnCycle`, revenge caching, `CheckActive_WormSegments`,
collision/recovery, housing/town services, network/persistence projection,
`WorldGen.UnspawnHomelessNPC`, complete static NPC definitions, complete AI families, and legacy
source deletion remain `partial` or `deferred`. `canRemoveLegacyWorldGen` remains `false`.
