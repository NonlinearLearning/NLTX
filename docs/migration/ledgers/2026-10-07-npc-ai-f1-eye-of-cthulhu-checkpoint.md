# NPC AI F1 checkpoint: Eye of Cthulhu

Date: 2026-10-07  
Status: opening, dash-cycle, transformation, and Servant profiles have verifier coverage;
Simulation full verification root11 and Eye one-tick/220-tick host smokes root36 passed.
Current-source Classic 599/600/601 and Expert 209/210/211 host runs confirm the hover boundary
and first dash; all five transformation/Servant host probes pass. Full F1 coverage remains
partial/open because source-output comparison, broader difficulty and secret-seed coverage,
network authority, presentation, and complete target-selection inputs remain open.

## Scope and source facts

This is the first F1 single-Boss slice, limited to Eye of Cthulhu identity `type=4`,
`netId=4`, `aiStyle=4`. The source facts used are from the reference `Terraria/NPC.cs`
opening AI branch (`20136+`) and `Terraria.ID/NPCID.cs:11118`: the opening hover counter
uses 600 ticks normally and 210 in Expert; the first dash begins after that counter; the
life transition uses strict `< 50%` normal and `< 65%` Expert checks. Daytime and target
loss/death exit paths retain the dust roll before upward velocity and despawn encouragement.
The default identity data registered for the finite simulator is 100x110, 2800 life,
15 damage, 12 defense, boss, and value 30000.

Profile-level source closure now includes the first and second transformation boundaries,
Expert Servant summoning, and Servant steering. Current Simulation host evidence now covers
Classic/Expert hover boundaries and the first dash, plus the five existing transformation and
Servant probes. Broader difficulty/secret-seed parity, network authority, presentation, complete
target-selection inputs, and reference-output comparison remain open. No claim is made that the
complete Eye profile or F1 coverage is closed.

## Changes

- Added the pure Eye profile, state/input/result values, attack and exit enums, and random/effect
  ports under `src/NSSLC/Component/Npc/`.
- Added verifier cases for identity gating, hover boundaries, strict life thresholds, target
  exits, dash cycling, opening Servant effects, both transformation boundaries, and
  random/effect ordering in `Test/Terraria.NpcAi.Verification/`.
- Added the finite `aiStyle=5` Servant of Cthulhu steering profile and the Eye transformation
  Servant spawn/effect path. Simulation transformation probes cover Expert summon timing,
  phase transitions, capacity rejection, and slot-order next-tick updates.
- Wired the finite host's exact identity dispatch, no-gravity path, state commit, and dust/dash/
  despawn/network effect adapter in `src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs`;
  registered type 4 in the finite content bootstrap and support manifest; exposed the ordered
  effect trace in runtime reports.
- Attached `NpcStatusFlagsComponent` in `RuntimeNpcEntity.Hydrate` after the first boundary host
  run exposed target selection reading the missing `Confused` status state.
- The root15 source snapshot was compiled to standard and isolated `FixtureHost` outputs. A later
  root17 `FixtureHost` build passed, but its one-tick smoke failed during world publication before
  the first tick. Root28/root30 are retained as historical integration evidence; the later root11
  full Simulation verification and root36 Eye smokes are the current targeted host evidence.

## Verification

| Check | Command / artifact | Result |
| --- | --- | --- |
| NPC component build (root15 snapshot) | `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:minimal -p:BuildProjectReferences=false` | Exit 0; 0 warnings, 0 errors. Artifact: `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`. |
| NPC AI verifier build (root15 snapshot) | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal` | Exit 0; 0 warnings, 0 errors. Log: `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/npc-ai-verifier-build-root15-20261007.log`. Artifact: `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/Terraria.NpcAi.Verification.dll`. |
| NPC AI verifier run (root15 snapshot) | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | Exit 0; PASS for the NPC AI center, target gates, repeatability, phase boundary, lifecycle, and finite C1–D3 profiles. Log: `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/npc-ai-verifier-run-root15-20261007.log`. |
| Current NPC AI verifier build (Eye transformation/Servant slice) | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | Exit 0; 0 warnings, 0 errors. `Terraria.Npc` and verifier outputs are under `Build/bin/`; verifier artifact SHA-256 `D329B9CF7817CF89816EDF707B2C390070D70FCFBBF25384B251D5C96F3CAFDA`. Eye profile source SHA-256 `EC64F85E10EEA924E2CA1B708C74D7F25D5E13DEAC6CF229A570B5250E305EF1`. |
| Current full NPC AI verifier run (Eye transformation/Servant slice) | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | Exit 0; PASS. The first transformation boundary asserts state `(2,0,0.5,0)`, Servant requested/recorded once, 6 Gore, 32 Dust, projectile reflection, and Servant-before-burst effect order. The second boundary asserts state `(3,0,0,0)`, Servant requested/recorded once, no burst/Gore/sound 3, and 12 Dust (`DustRoll` 1 + Servant summon 10 + transformation tail 1). This is profile-level evidence, not a Simulation host run. |
| Simulation project build (root15 snapshot) | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | Exit 0; 0 warnings, 0 errors. Log: `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/simulation-build-root15-20261007.log`. Artifact: `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`. |
| Simulation `FixtureHost` build (root15 snapshot) | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true` | Exit 0; 0 warnings, 0 errors. Log: `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/simulation-fixture-build-root15-20261007.log`. Artifact: `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`. |
| Eye one-tick host smoke (root15 snapshot) | `dotnet Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 1 --players 0 --seed 1313625176 --spawn-npc 4 --report Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/eye-smoke-root15-20261007.json` | Exit 0; `Succeeded=true`, `TickNumber=1`, final-tick NPC update completed, source world unchanged. Report observed `NetId=4`, `SpawnedNpcTypes=[4]`, no-gravity path, and trace `TargetReacquire → Random.Next(5)=0 → ProfileSupported:true → SpawnDust → EncourageDespawn(10)`. |
| Intervening `FixtureHost` build (root16) | Same `FixtureHostBuild=true` command; log `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/simulation-fixture-build-root16-20261007.log` | Exit 1; 0 warnings, 1 error (`PlayerNetworkStateSystem.ApplyTownNpcCount` duplicate). The Player owner removed the duplicate before root28. |
| Intervening `FixtureHost` build (root17) | Same `FixtureHostBuild=true` command; log `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/simulation-fixture-build-root17-20261007.log` | Exit 0; 0 warnings, 0 errors. This binary predates the current session-publication changes. |
| Intervening Eye one-tick smoke (root17 binary) | Same input and arguments; report `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/eye-smoke-root17-20261007.json` | Exit 1; `TickNumber=0`, `FailureKind=InvalidData`, `FailureDetail="Tile mutation tracking requires the active published session."`; cleanup also reported a disposed `LoadedWorldSession`. This failed run did not reach NPC update. |
| Simulation `FixtureHost` build (root28 snapshot) | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true` | Exit 0; 0 warnings, 0 errors. Log: `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/simulation-fixture-build-root28-20261007.log`. Artifact: `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`. |
| Eye one-tick host smoke (root28 snapshot) | `dotnet Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 1 --players 0 --seed 1313625176 --spawn-npc 4 --report Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/eye-smoke-root28-20261007.json` | Exit 0; `Succeeded=true`, `RecoveryPhase=Completed`, `TickNumber=1`, `RuntimeNpcsUpdatedAtFinalTick=true`, `SourceFileUnchanged=true`. The Eye entry has `NetId=4`, `NoGravity=true`, `NoTileCollide=true`, `DustCount=1`, `DespawnEncouragementTicks=10`, and trace `TargetReacquire → Random.Next(5)=0 → ProfileSupported:true → SpawnDust → EncourageDespawn(10)`. The input SHA-256 was `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241` before and after the run. |
| Simulation `FixtureHost` build (current root30) | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true` | Exit 0; 23 warnings, 0 errors. The project graph emitted current `Terraria.Items`, `Terraria.Projectile`, `NSSLC.Infrastructure.WorldStorage`, and Simulation assemblies under `Build/bin/FixtureHost/`. Log: `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/simulation-fixture-build-root30-20261007.log`; exit code: `simulation-fixture-build-root30-20261007.exit-code.txt`. Simulation artifact: `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`. |
| Simulation full verification (current root11) | `Build/diagnostics/NpcAiRedesign/runs/current-simulation-verification-root-11/summary.json` and `build.log` | `Succeeded=true`; Simulation build exit 0, 0 warnings, 0 errors; clock verifier build/run and TileEntity fixture build exit 0; summary contains 37 evidence entries. Artifact: `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`. This runs-directory build log and summary are authoritative; root-level `current-simulation-build-root-11.log` is an older failed attempt and is not this result. |
| Simulation project build after NPC status initialization fix | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | Exit 0; 23 warnings, 0 errors (transitive `Terraria.WorldStorage` and `NSSLC.WorldGeneration` warnings). Artifact: `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`, SHA-256 `A43AA911D2DB811A2D0BEDDDF501486503519C2A8BF98AB1E73E343F4FCE3E10`. |
| Classic/Expert hover and first-dash host boundaries | Six current-source Simulation runs using `Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld`, one player, seed `1313625176`, and `--npc-eye-scenario classic-night` / `expert-night`; reports: `Build/diagnostics/NpcAiRedesign/runs/f1-eye-boundary-current-20261007/{classic-599,classic-600,classic-601,expert-209,expert-210,expert-211}.json` | All six exited 0 with `Succeeded=true`; `DayTime=false`; Eye and Player were alive at each boundary; source world SHA-256 `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241` remained unchanged. Classic: tick 599 `Ai2=599`, tick 600 `Ai1=1`, tick 601 `Ai1=2` and effect trace includes `BeginDash`. Expert: tick 209 `Ai2=209`, tick 210 `Ai1=1`, tick 211 `Ai1=2` and effect trace includes `BeginDash`. At 601/211 the target was reacquired in slot 0. |
| Expert transformation and Servant host probes | Five current-source Simulation runs against `Build/diagnostics/EntityOrganization/B7/parallel-20261007/tile-host-fixture-current.wld`; reports: `Build/diagnostics/NpcAiRedesign/runs/f1-eye-boundary-current-20261007/{expert-summon,phase-one-boundary,phase-two-boundary,capacity-rejection,next-tick}.json` | All five reports have `Succeeded=true`, `NpcEyeTransformationProbe.Passed=true`, and source world SHA-256 `DB9E14472323CCB1965B6E3EAEC4A12FD49DA4A95BDFA6504F245A524BF0F417` remained unchanged. The 20-tick Expert summon created and updated one Servant on tick 1; phase one committed `(2,0,0.5,0)`, one Servant, 6 Gore, and 32 Dust; phase two committed `(3,0,0,0)`, one Servant, and 12 Dust; capacity rejection filled 199 NPC slots and recorded `ServantSpawnRejected:Capacity`; next-tick reused lower slot 0 and updated the Servant on tick 2 (its tick-1 last-update value was 0). The next-tick probe used the existing neutral input script (`frames: []`) and fired zero player shots. An initial default-input attempt exited 1 when an automatic projectile reached an NPC whose Collider component was unavailable; that failed attempt is not counted as passing evidence. |
| Eye one-tick host smoke (current root36) | Simulation DLL from root11; report `Build/diagnostics/NpcAiRedesign/runs/stage-f1-eye-current-root-36/eye-smoke-1-no-player.json` | Exit 0; `Succeeded=true`, 1 tick, 0 players, daytime. Eye `NetId=4`, `NoGravity=true`, `NoTileCollide=true`; trace `TargetReacquire → Random.Next(5)=0 → ProfileSupported:true → SpawnDust → EncourageDespawn(10)`. `SourceFileUnchanged=true`. |
| Eye 220-tick host smoke (current root36) | Same Simulation DLL; report `Build/diagnostics/NpcAiRedesign/runs/stage-f1-eye-current-root-36/eye-smoke-220-one-player.json` | Exit 0; `Succeeded=true`, 220 ticks, 1 player, daytime. Eye `NetId=4`, `NoGravity=true`, `NoTileCollide=true`; position changed from `(33558, 3670)` to `(33988, 2840.6597)`. Trace `Random.Next(5)=3 → ProfileSupported:true → EncourageDespawn(10)`. `SourceFileUnchanged=true`. This is a daytime movement smoke under an exit condition, not evidence of the first-dash path. |
| Main-session current-source acceptance | `Build/diagnostics/NpcAiRedesign/current-acceptance-root/` and `Build/diagnostics/NpcAiRedesign/runs/current-acceptance-root/simulation-verification/summary.json` | The shared-source Simulation build and NPC AI verifier build/run passed with 0 warnings / 0 errors; full Simulation verification is `Succeeded=true` with 37 evidence, and the current coverage/source-identity gates report 128 styles with 124 indexed / 4 mapped / 0 implemented / 0 verified and 6/6 source hash comparisons. These are current host/regression evidence only; F1 remains partial/open. |

The verifier and root15 snapshot artifacts, root16/root17 historical failures, and root28/root30
build/smoke artifacts are in `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/`.
Earlier failed Simulation attempts, including Items/Projectile/WorldStorage interface-mismatch logs,
remain preserved in `Build/diagnostics/NpcAiRedesign/runs/stage-f1-host-recovery-20261007/` and
`Build/diagnostics/NpcAiRedesign/runs/stage-f1-eye-of-cthulhu-20261007/`. Root11 rebuilt the current
project graph for the full Simulation verification, and root36 ran both Eye smokes from that fresh
artifact without using an earlier DLL as evidence.

## Checkpoint and follow-up

The profile-level verifier covers transformation and Servant effect counts, including the
second transformation boundary's 12 Dust: one general dust effect, ten Servant summon effects,
and one transformation tail effect. Current-source host evidence now covers ordinary/Expert
night hover boundaries, first dash, Expert transformation boundaries, capacity rejection, and
next-tick Servant ordering. Complete target inputs, broader difficulty/seed coverage, network
authority, presentation, and reference-output comparison remain open, so F1 stays partial/open.

Suggested coverage-ledger entry (not applied):

| Profile | Source identity | Implementation | Call/effect closure | Verification |
| --- | --- | --- | --- | --- |
| Eye of Cthulhu opening / first dash / transformation slice | type=4, netID=4, aiStyle=4 | Eye and Servant profiles plus finite-host source wiring and transformation probes are present | Partial/open; profile verifier and current-source host reports cover opening summon, Classic/Expert night hover boundaries, first dash, both transformation boundaries, capacity rejection, next-tick Servant ordering, and Servant steering. Broader difficulty/seed coverage, complete target-selection inputs, network authority, presentation, and source-output comparison remain open | NPC AI verifier build/run passed with 0 warnings/errors and transformation effect assertions; root11 Simulation full verification passed; current Simulation build passed with 23 transitive warnings and 0 errors; six boundary reports and all five transformation/Servant host probes passed |

Rollback scope is limited to the F1 Eye and Servant profile types, Eye-specific host dispatch,
content registration and trace fields, transformation probes, verifier cases, and this checkpoint.
Preserve all other existing worktree changes; no C1-C4 profile, coverage ledger, or execution-plan
section was edited.
