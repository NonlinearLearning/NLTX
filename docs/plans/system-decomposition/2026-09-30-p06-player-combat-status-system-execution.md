# P06 Player Combat Status System Execution Plan

**Status:** `proposed`  
**Execution state:** `partial-core-slice-verified`  
**Verification:** `not-run`  
**Source modified by this plan:** `true` (core slice, including `PlayerLifeRegenQuery`)

This document is the execution plan plus the execution record for a narrow P06 core slice. It does
not claim a successful migration. The full phases below remain future work, and the existing P06
report, runner state, Version4 baseline, and complete reference project were not modified.

This continuation records the settled claim, CPG query results, Version4 source, and the complete
reference project, then records the isolated core slice and its explicit-closure verifier. It does
not change runner state, the Version4 tree, or the complete reference project. The focused verifier
output below is historical; the latest `Resting` ordering assertion was added afterward and was not
rebuilt or rerun in this documentation pass. Full-P06 verification remains `not-run`.

## Execution Contract

| Item | Value |
|---|---|
| Partition | `P06` / `AUTH-SYS-P06` |
| Settled report session | `791756dc1f954ff3afcd84b5ef3fe296` |
| Authoritative baseline | `D:\TRbackup\Version4` |
| Complete reference supplement | `D:\TRbackup\无任何删减通过编译` |
| Migration source area | `D:\TRbackup\NLTX\src\NSSLC` |
| Design input | `docs/plans/system-decomposition/2026-09-30-p06-player-combat-status-system-design.md` |
| Claim input report | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P06-player-combat-status.md` |
| Runner `outputReport` | `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P06-player-combat-status.md` |
| Settlement | `completed` with the exact claimed `sessionId` above; no second partition was claimed |
| Current verification | Historical core-slice record; full P06 `not-run` |

The runner session ID above is a settlement identity only. It is not a player, entity, world, or
network ID and must not be placed in a runtime command or component.

The read-only runner state at
`D:\TRbackup\NLTX\.agents\skills\version4-partition-session-runner\sessions\version4-authoritative-partition-session\tasks\authoritative-system-decomposition\task-state.json`
matches this contract: P06 is `completed`, `exitCode=0`, `leaseState=released`, and
`outputReportPath` points to the report above. This document did not claim or settle another
partition session.

## Guardrails Before Any Future Change

1. Read the current `AGENTS.md`, `Context/progress.md`, C# style, ECS file organization,
   side-effect isolation, build concurrency, and output verification constraints again at the
   start of the implementation session.
2. Preserve the Version4 and complete-reference hashes recorded in the design. If a file changes,
   stop and create a new evidence record; do not silently reuse the old conclusion.
3. Keep `D:\TRbackup\Version4` as the P06 denominator. Files or members present only in
   `D:\TRbackup\无任何删减通过编译` are `outside-Version4-baseline` and excluded from 254-member
   coverage.
4. Use the read-only CPG API for relationship questions. A selected-range `complete` result is
   not a closed runtime graph; `SourceSnapshotId=null`, dynamic dispatch, aliases, reflection,
   hooks, callee effects, scheduler registration, and zero-hit interpretations remain gaps.
5. Maintain one authority and one commit point for each P06 invariant. Queries, Projections,
   Network adapters, and Persistence adapters cannot write live state.
6. Do not delete the old facade, shrink a project, remove a file, or narrow a `Compile` list to
   manufacture a build result. Deletion requires a separate real behavior gate.
7. Every future phase records `proposed`, `partial`, `unknown`, or
   `full-reference-supplemented` explicitly. `verificationStatus` stays `not-run` until the
   named check actually runs.

## Executed Core Slice (Approximately 10%)

The implemented slice is intentionally bounded to the accepted-hit resolution seam under
`D:\TRbackup\NLTX\src\NSSLC\Component\Player`:

- `PlayerCombatResolutionSystem` owns one synchronous commit boundary for a damage event.
- Eligibility checks cover empty/invalid identity, non-positive damage, general immunity, source
  cooldown, duplicate event IDs, and optional ShadowDodge consumption.
- `PlayerDamageMitigationQuery` is pure and currently covers defense, endurance, critical doubling,
  and minimum damage of one.
- Only an accepted hit writes `PlayerVitalStateComponent.StatLife`; committed proc and DPS facts
  are published after that write. A lethal result is reported but is not a P04 death/respawn
  implementation.
- `PlayerStatusEffectSystem` owns a 44-slot status core: known-definition add, duplicate refresh,
  additive cap, immunity rejection, first known non-debuff eviction, explicit timer tick, expiry
  compaction, and defensive snapshot.
- `PlayerManaRegenSystem` owns the ordinary delay/count/cap path. Nebula resource increments,
  complete status-derived modifiers, and scheduler registration remain outside this slice.
- `PlayerLifeRegenQuery` computes the selected Version4 life-regen counters and emits a pure damage
  or healing request plan. It never writes `PlayerVitalStateComponent.StatLife`; the later
  `HurtLifeRegen` effect remains `unknown`.

The focused verifier is
`D:\TRbackup\NLTX\src\NSSLC\Component\PlayerCombatResolutionVerification\Terraria.PlayerCombatResolutionVerification.csproj`.
It uses an explicit source closure, so the unrelated Mount compile gaps in the full Player project
do not get hidden behind a narrowed production project.

### Read-only relationship recheck

The CPG API was initialized from
`D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite` and queried through
`CpgEvidence.ps1`. `Find-CpgSymbols` resolved `UpdateLifeRegen()` and
`HurtLifeRegen(int)` exactly. `Find-CpgCallSites(HurtLifeRegen)` returned three exact static
call sites in the selected `Player.cs`/`MessageBuffer.cs`/`Projectile.cs` scope, all in
`Player.cs`. `Find-CpgCallSites(UpdateLifeRegen)` returned `partial`/zero with
`NoMatchingFactInScannedScope`, so the direct Version4 source call remains the authority and the
zero result is not interpreted as absence. `Get-CpgMemberUses(statLife)` returned 31 selected-scope
facts with mixed `Write`, `ReadWrite`, `Unknown`, and `partial` classifications. The CPG snapshot
has `SourceSnapshotId=null`; these are bounded static facts, not a closed runtime graph.

The complete reference project was read at the same paths without being treated as a second
denominator: `Terraria/Player.cs:19749-19756` contains the recovered `HurtLifeRegen` side effects,
`Terraria/MessageBuffer.cs:1096-1103` reads life/max and derives `dead`, and
`Terraria/NetMessage.cs:517-518` writes life/max as `Int16`. Its
`TerrariaServer.sln`/`TerrariaServer.csproj` were not compiled in this pass.

### Verification evidence

The historical serial build completed with `0` warnings and `0` errors and produced:

`D:\TRbackup\NLTX\Build\bin\Terraria.PlayerCombatResolutionVerification\Debug\net10.0\Terraria.PlayerCombatResolutionVerification.dll`

The commands below are retained as reproducibility evidence only. They were not executed again
after the latest `Resting` ordering assertion was added.

The successful commands used the repository wrapper and the SDK required by `global.json`:

```powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build',
  './src/NSSLC/Component/PlayerCombatResolutionVerification/Terraria.PlayerCombatResolutionVerification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')

& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--project',
  './src/NSSLC/Component/PlayerCombatResolutionVerification/Terraria.PlayerCombatResolutionVerification.csproj',
  '--no-build', '--no-restore',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
```

The build exited `0` with zero warnings/errors, and the no-build verifier exited `0`.

An earlier core-only wrapper run used `--no-build --no-restore` and returned:

```text
PASS: player combat resolution core ownership, rejection and commit semantics
EXIT_CODE=0
```

The verifier covers the pure mitigation example (`100`, `10`, `0.25` -> `67`), normal accepted
damage, duplicate rejection, general immunity, ShadowDodge consumption, critical lethal damage,
proc/DPS publication, status refresh/cap/immune rejection/full-table eviction/tick expiry, life
regen healing/Poisoned/ordinary-damage/Burned planning, and ordinary mana delay/count/cap
behavior. Its output is:

```text
PASS: player combat, status-slot, life-regen and mana-regen core semantics
EXIT_CODE=0
```

The current verifier source also contains a `Resting` assertion for the Version4 `3600` cap order;
that assertion is not covered by the historical output and remains `not-run`.

This is focused evidence and does not represent 10% coverage of the 254-member P06 inventory.

### Executed-plan mapping

- Phase 2: the slot-lifecycle core was implemented and exercised; catalog-wide effect rebuild,
  replacement groups, and replication remain open.
- Phase 3: the accepted-hit core was implemented and exercised; recursive Paladin behavior and
  P04 handoff remain open.
- Phase 4: the ordinary mana delay/count/cap core was implemented and exercised; Nebula increments,
  same-frame scheduler visibility, and the final life-regeneration commit remain open. The pure
  `PlayerLifeRegenQuery` counter slice is implemented and exercised.
- Phase 5: committed DPS publication is exercised through the accepted-hit path; clock policy and
  complete reset/lifecycle integration remain open.

An additional serial compile probe against
`D:\TRbackup\NLTX\src\NSSLC\Component\Player\Terraria.Player.csproj` failed before producing a
usable full-project artifact. The five reported errors were existing Mount input gaps: missing
`MountDefinition` in `PlayerInteractionAndSelectionPropertiesInput.cs`, `PlayerMountComponent.cs`,
and `PlayerMountState.cs`, plus two missing `MountDrillRuntimeComponent` references in
`Player/Mount/Systems/DrillMountSystem.cs`. Those unrelated files were not changed, and this
failure does not invalidate the explicit-closure verifier artifact above.

### Explicit non-coverage

Paladin recursion, P04 lifecycle handoff, complete status catalog/effects and replacement policy,
life-regeneration commit side effects, Nebula mana increments, network, persistence, runtime registration,
presentation, knockback, sound, particles, scheduler order, and complete Version4 effect closure
remain `unknown`, `partial`, or `not-run` as applicable.

## Phase Plan

### Phase 0: Freeze Evidence And Recover Source Gaps

**Purpose:** establish a reproducible baseline before choosing an implementation owner.

**Inputs:** Version4 `Player.cs`, `MessageBuffer.cs`, `NetMessage.cs`, `Projectile.cs`;
complete reference same-path files; P06 report; selected CPG query output.

**Actions:**

- Recompute SHA-256 for the four Version4 files and the three complete-reference files.
- Read the complete-reference bodies for `HurtLifeRegen`, `AllowShimmerDodge`,
  `InternalSavePlayerFile`, `Serialize`, `Deserialize`, and the related resource packet
  cases.
- Query symbols, selected call sites, member uses, and callable facts through
  `.agents/skills/ecs-system/tools/CpgEvidence.ps1`; record query scope and limits.
- Write a short evidence delta that distinguishes `Version4 confirmed`, `unknown`, and
  `full-reference-supplemented`.

**Exit evidence:** hashes, source line anchors, query scope, and an updated gap list. No source
code is changed in this phase.

### Phase 1: Select The Authority And Stable Contracts

**Purpose:** prevent competing Vital, Buff, or combat writers before implementation begins.

**Actions:**

- Inspect the actual production registration and call path for current `src/NSSLC` types. Existing
  declarations alone do not select an owner.
- Select one Vital representation and one Status representation, or record why a reviewed
  coordination boundary is required.
- Define immutable snapshots and explicit commands for accepted hit, rejected hit, status tick,
  mana regen, life regen result, and committed damage.
- Define identity, scope, error, retry, and idempotency fields. Keep runner `sessionId` out of all
  runtime contracts.
- Record P04/P07/P08/P09/P11/P12/P15/Network/Persistence handoffs as
  `crossSubsystemOwner: integration-review` until those owners are accepted.

**Exit gate:** a read/write table shows one writer per invariant, the accepted-hit commit point,
and the visibility barriers. If this cannot be shown, stop before adding more Systems.

### Phase 2: Implement Status Effect Ownership

**Purpose:** isolate Buff/debuff slot, immunity, replacement, timer, and expiry behavior.

**Actions:**

- Implement or adapt `PlayerStatusEffectSystem` around the selected Status authority.
- Preserve `AddBuff`, `DelBuff`, and `UpdateBuffs` ordering, slot replacement, immunity checks,
  and expiry behavior from the verified source/reference comparison.
- Publish a read-only status snapshot for capability rebuild and regen queries.
- Route network/persistence effects through adapters; do not write Vital directly from a status
  definition or Query.

**Exit evidence:** focused behavior cases for add, replace, reject, full-slot eviction, tick, and
expiry. Runtime registration and full project build are still separate gates.

### Phase 3: Implement Accepted-Hit Resolution

**Purpose:** preserve the synchronous `Hurt` concept without creating multiple resource writers.

**Actions:**

- Map eligibility, shimmer dodge, immunity, mitigation, critical handling, and cooldown checks to
  explicit read-only rules and one `PlayerCombatResolutionSystem` commit.
- Use the complete-reference shimmer conditions only as a versioned supplement; verify source
  identity and configured NPC/projectile tables at integration time.
- Model Paladin or other recursive damage with explicit source/target and idempotency data.
- Emit a committed result only after authoritative state is updated; rejected hits must not emit
  accepted-hit effects.
- Hand lethal results to P04 instead of implementing a second death system in P06.

**Exit evidence:** old/new observation vectors for rejected, immune, normal, critical, lethal, and
recursive hits, including event count and order. A simple `max(1, amount-defense)` resolver is
not evidence of Version4 parity.

### Phase 4: Implement Vital, Life, And Mana Boundaries

**Purpose:** preserve resource caps and make the recovered life-regeneration effect explicit.

**Actions:**

- Implement `PlayerManaRegenSystem` only after the Vital writer is selected. Preserve delay,
  standing/grappling/buff modifiers, count thresholds, and cap behavior.
- The core `PlayerLifeRegenQuery` calculation is implemented and exercised in the focused slice.
  The remaining action is a reviewed commit command for the `HurtLifeRegen` effect. The complete
  reference shows life subtraction, combat text, and spectating, but this remains
  `full-reference-supplemented` until the target composition is exercised.
- Preserve reset -> status -> equipment -> life regen -> mana regen visibility from
  `Player.Update`.
- Add explicit bounds and error results for malformed or stale resource commands.

**Exit evidence:** resource state deltas, cap behavior, status/equipment ordering, DoT/heal effects,
and death handoff. Do not label a life commit complete while the authoritative Version4 body is
empty or the target path is untested.

### Phase 5: Add DPS Telemetry As A Projection

**Purpose:** preserve committed-damage accounting without moving combat authority into metrics.

**Actions:**

- Consume only committed damage results.
- Expose a clock port that can reproduce the source `DateTime.Now` observation before any
  tick-clock substitution is approved.
- Preserve first-hit, last-hit, start/end, accumulation, and reset semantics.
- Keep the projection read-only and scoped per player/world/session.

**Exit evidence:** deterministic clock fixture plus wall-clock compatibility decision, reset cases,
multiple hits, and no telemetry changes for rejected hits.

### Phase 6: Add Network And Persistence Adapters

**Purpose:** close protocol boundaries after live authority is stable.

**Actions:**

- Encode/decode life/max and mana/max with the observed `Int16` widths and packet order.
- Preserve the life<=0 -> `dead` derivation and max-life floor where the protocol requires it.
- Validate authority, player identity, relay behavior, and stale packet handling before applying a
  snapshot.
- Implement a versioned persistence codec based on the complete-reference version-319 structure,
  metadata, backup/cloud behavior, omitted fields, and release-aware loading. Apply restored state
  only after validation.
- Record field ownership across all 254 P06 members; do not claim that the complete-reference
  serializer proves every field is in the new P06 owner.

**Exit evidence:** packet round trips, malformed/unauthorized packet cases, save/load versions,
backup/cloud failures, partial restore behavior, and exact error results.

### Phase 7: Real Integration, Full Builds, And Behavior Gate

**Purpose:** prove the composition in the real target and keep compile evidence honest.

**Actions:**

- Register the selected Systems in the actual runtime and exercise real Player, NPC, Projectile,
  network, persistence, and lifecycle entry points.
- Run the repository's serial build wrapper from `D:\TRbackup\NLTX` against the complete
  target project/solution selected by the repository build policy.
- Separately compile the complete reference project from
  `D:\TRbackup\无任何删减通过编译` using its full `TerrariaServer.sln` or
  `TerrariaServer.csproj`, without removing files or narrowing project inputs.
- Run the required real behavior tests through the new owner and composition. Record command,
  working directory, inputs, output paths, exit code, warnings/errors, and observation results.
- Keep reference build results and target migration results in separate evidence records.

**Future command shape (not executed in this task):**

```powershell
Set-Location 'D:\TRbackup\NLTX'
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build `
  'D:\TRbackup\无任何删减通过编译\TerrariaServer.sln' `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

The exact target solution/project and repository wrapper arguments must be read from the current
build policy before execution. The command above is a future reference-project gate, not evidence
that the reference or NLTX target has built in this task.

**Acceptance gate:** only a real target behavior test suite that exercises the new authority and
composition can set `verificationStatus` above `not-run` or support `migration-success`. A
complete reference build can show that the reference project compiles; it cannot show that NLTX
migration succeeded.

## Verification Matrix

| Area | Required check | Evidence state for this plan |
|---|---|---|
| Source and CPG binding | hashes, selected query scope, unresolved relationship list | `partial`; future re-check required |
| System ownership | one writer per Vital/Status/combat invariant and explicit handoffs | `proposed`; not executed |
| Status effects | add/remove/replace/tick/expiry and rejected effects | core slot lifecycle `passed`; complete catalog/effects `not-run` |
| Damage | dodge/immune/mitigation/critical/lethal/recursive observation vector | core slice `passed`; Version4 parity and recursion `not-run` |
| Life regen | counter query, recovered reference effect, and target commit path | core query `passed`; commit effect `full-reference-supplemented`; target integration `not-run` |
| Mana regen | delay/count/cap and same-frame ordering | core delay/count/cap `passed`; Nebula/order `not-run` |
| DPS | clock, accumulation, reset, rejected-hit behavior | `not-run` |
| Network | resource packet widths/order/authority/relay | packet shape `confirmed`; integration `not-run` |
| Persistence | versioned codec, backup/cloud/error/recovery behavior | reference supplement only; target `not-run` |
| Full reference compile | full solution/project without deletions or narrowed inputs | `not-run` |
| Full target compile | repository-approved full project/solution | attempted; failed on existing Mount gaps; acceptance `not-run` |
| Migration success | real required behavior tests through new owner | `not-run`; no success claim |

## Rollback And Failure Handling

- If an ownership conflict appears, stop the phase and revert only the current implementation
  change; preserve the evidence record and return the conflict to `integration-review`.
- If a behavior vector diverges, keep the old entry as the authority, disable the new route behind
  the smallest reviewed switch, and record the first divergent state/event/order observation.
- If a packet or save/load adapter fails, reject the unvalidated snapshot and preserve the last
  committed authority. Do not compensate by writing directly from an adapter.
- If the full target build fails, record the complete command and diagnostics. Do not delete files,
  change the project to a partial compile, or relabel a reference-project build as target success.
- Old API removal is a later deletion task. It requires proof that real callers use the new
  composition, behavior tests cover the observation vector, and rollback is no longer needed.

## Current Execution Statement

The isolated core slice has a historical serial build/run record. The latest verifier edit was not
rebuilt or rerun in this pass, and the unlisted portions of those phases and the remaining phases
above have not been executed. No full target integration, complete-reference compilation, full
behavior suite, publish, or deletion gate was run. The document-level
`verificationStatus` remains `not-run`, and the P06 work must not be described as a successful
migration.
