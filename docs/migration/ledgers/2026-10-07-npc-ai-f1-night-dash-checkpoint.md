# NPC AI F1 checkpoint: Eye of Cthulhu night boundaries and dash cycle

Date: 2026-10-07
Status: the night hover boundary and first-dash host slice passed on a fresh Simulation artifact;
F1 remains partial/open.

## Scope

This checkpoint records real host runs for Eye of Cthulhu `type=4`, `netID=4`, `aiStyle=4` in
Classic night and Expert night. It covers one tick before, at, and after the opening hover
threshold, the first dash launch, and complete three-dash cycles. It does not close the
transformation branch, servant behavior, full target-selection parity, authority/network effects,
or source-output differential.

The scenario helper commits night and difficulty through
`WorldSessionRestoreSystem.ApplyHeader` and `ApplyEnvironment`, then updates `RuntimeMain` as a
compatibility projection. This evidence therefore uses session-owned world state for the simulation
inputs.

## Fixed source and inputs

The read-only reference is `D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`, SHA-256
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`, matching the pinned source
identity. Its opening branch increments `ai[2]` before checking the hover limit (600 Classic, 210
Expert), enters the aim state at the threshold, and launches the first dash on the following tick.
First-dash speed is 6 Classic and 7 Expert; For the Worthy adds 1. The source's three-dash durations
are 150 Classic and 100 Expert ticks.

The immutable world input was
`Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld` (WorldId
`1904902962`, 4200x1200), SHA-256
`022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241` both before and after all
runs. The neutral input script is
`Build/diagnostics/NpcAiRedesign/runs/f1-night-dash-20261007/eye-neutral-input.json`, containing
`{"frames":[]}`, SHA-256
`2AC0A74C84218685193184E7B18963E5FF2B14AC42146D8B70282E7D6E23110F`.

Each host run used one neutral local player, seed `1313625176`, `--world-time-rate 0`, one Eye
spawn, and either `classic-night` or `expert-night`. All reports show `DayTime=false`, frozen
world time, the requested game mode, a published session, and an unchanged source world.

## Build and verifier evidence

The Simulation artifact was built by the main session and reused without another build for this
checkpoint:

| Check | Command / evidence | Result |
| --- | --- | --- |
| Successful Simulation build | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal`; log `Build/diagnostics/NpcAiRedesign/runs/task-reference-host-20261007/simulation-build.log`; result `acceptance-result.json` | Exit 0; 17 warnings, 0 errors. Warnings are from the rebuilt WorldGeneration dependency. Artifact: `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`, SHA-256 `80C0C8180C00E99D58044C26F8EEA023D9AB06FBA6ED17B58A96EAE38134D02A`. |
| Earlier local Simulation build attempt | `Build/diagnostics/NpcAiRedesign/runs/f1-night-dash-20261007/simulation-build.log` and `.exit-code.txt` | Historical exit 1; 23 warnings, 2 errors in `RuntimeWorldLoadRollbackVerification.cs` (`NpcSlot` and `TileCoordinate` ambiguous). This failed build did not produce the artifact used by the host runs. It is kept separate from the successful main-session build above. |
| NPC AI verifier build | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`; log `npc-ai-verifier-build-sourcefix.log` | Exit 0; 6 warnings, 0 errors. Warnings are from WorldStorage. Artifact under `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/`. |
| NPC AI verifier run | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore`; log `npc-ai-verifier-run-sourcefix.log` | Exit 0; PASS, including the 600/210 boundaries, strict phase threshold, first-dash speed, three-dash state transitions, Expert/For the Worthy duration, and sequential velocity damping. |

The main build log contains repeated diagnostic lines in its detailed and summary sections; its
build summary and the main-session acceptance record both report 17 warnings and 0 errors.

## Real host scenarios

The command template was:

```powershell
dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld <ticks> `
  --players 1 --seed 1313625176 `
  --input-script Build/diagnostics/NpcAiRedesign/runs/f1-night-dash-20261007/eye-neutral-input.json `
  --world-time-rate 0 --spawn-npc 4 --npc-eye-scenario <classic-night|expert-night> `
  --report Build/diagnostics/NpcAiRedesign/runs/f1-night-dash-20261007/<scenario>.json
```

All eight commands exited 0. The report JSON and matching `.run.log` / `.exit-code.txt` are in
`Build/diagnostics/NpcAiRedesign/runs/f1-night-dash-20261007/`.

| Scenario | Ticks | Final `(ai[0], ai[1], ai[2], ai[3])` | Target slot | Observed result |
| --- | ---: | --- | ---: | --- |
| Classic night, before threshold | 599 | `(0, 0, 599, 0)` | 0 | Remains in hover. |
| Classic night, threshold | 600 | `(0, 1, 0, 0)` | -1 | Enters aim state and requests a network update. |
| Classic night, first dash | 601 | `(0, 2, 0, 0)` | 0 | Speed 6; trace includes target reacquire, `BeginDash`, and network update. |
| Expert night, before threshold | 209 | `(0, 0, 209, 0)` | 0 | Remains in hover. |
| Expert night, threshold | 210 | `(0, 1, 0, 0)` | -1 | Enters aim state and requests a network update. |
| Expert night, first dash | 211 | `(0, 2, 0, 0)` | 0 | Speed 7; trace includes target reacquire, dust, `BeginDash`, and network update. |
| Classic night, three-dash run | 1053 | `(0, 0, 0, 0)` | -1 | Returns to hover after the third 150-tick dash. |
| Expert night, three-dash run | 513 | `(0, 0, 0, 0)` | -1 | Returns to hover after the third 100-tick dash. |

Machine-checked report summary: `scenario-validation.json` and `scenario-validation.txt`. Artifact,
source, input, and world fingerprints are in `source-fingerprints-before.tsv`,
`source-fingerprints-after.tsv`, `source-fingerprints.json`, and `source-fingerprint-delta.json`.

The reports ran against artifact SHA-256 `80C0C8180C00E99D58044C26F8EEA023D9AB06FBA6ED17B58A96EAE38134D02A`.
The pre-run fingerprint binds that artifact to the compiled source snapshot. `RuntimeNpcStore.cs`
changed during/after report collection: its recorded hash changed from
`49EC554732CDF705639C87F3FD1EDF9265E2C72DAEB3144A4A43C615ED87F841` before runs to
`5807B80CDEEFD486D1327D215D15DE1D1A67A70908507C1BD7DA004245085733` afterward. The Simulation DLL
hash did not change. The reports are evidence for the recorded artifact/source snapshot, not the
later unbuilt `RuntimeNpcStore.cs` contents. The source edit occurred after these eight reports were
written.

The main session is separately running Classic ticks 752/903 and Expert ticks 312/413 on the same
artifact to capture intermediate cycle boundaries. Those reports are not duplicated or counted as
results in this checkpoint until their own outputs are available.

## Remaining F1 work

- The finite profile only handles `ai[0]=0`. The phase threshold can move state to `ai[0]=1`, but
  the transformation branch is not implemented or host-verified.
- Servant of Cthulhu `netID=5` is absent from the Simulation content manifest and has no dedicated
  finite AI handler. Before emitting a real servant spawn, content registration, its own AI/effect
  behavior, spawn failure handling, and the source's same-tick versus next-tick update order for a
  newly allocated NPC slot must be resolved.
- Full target inputs, all difficulty/secret-seed branches, authority/replication, source-output
  differential, and complete F1 closure remain open. This checkpoint does not promote F1 coverage
  or change the canonical execution plan, coverage ledger, or earlier F1 checkpoint.

## Documentation note and rollback boundary

`Context/progress.md` points to `dome/dome1/docs/flowstate/README.md`, but this checkout has no
`dome/` directory and the linked lifecycle document could not be found under `D:/TRbackup`. The
missing reference is recorded without changing navigation. This checkpoint follows the root
`AGENTS.md`, the development/build constraints, and the existing F1 evidence format.

This checkpoint adds no production code. To roll back only this evidence slice, remove this
checkpoint and the `f1-night-dash-20261007` scenario reports, summaries, and fingerprints. Preserve
the existing F1 profile/host wiring, the successful main-session build artifact, historical failed
build logs, and all other owners' changes.
