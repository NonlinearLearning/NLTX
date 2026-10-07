# NPC AI coverage and registration gate checkpoint

文档 ID：DOC-2026-10-07-NPC-AI-COVERAGE-GATE  
逻辑域：system-decomposition  
产物类型：evidence  
状态：active  
范围：NPC AI 执行计划 G 阶段的独立 coverage/registration gate 基础设施  
证据入口：[128-style reference index](2026-10-05-npc-ai-reference-index.md)、[profile coverage ledger](../migration/ledgers/2026-10-06-npc-ai-profile-coverage-ledger.md)、[coverage verifier](../../Test/Terraria.NpcAi.CoverageVerification/Program.cs)  
canonical 路径：docs/system-decomposition/2026-10-07-npc-ai-coverage-gate.md

This checkpoint adds an executable gate beside the active execution plan and coverage ledger.
Those shared files were not rewritten in this session; the final status row below is intended for the
main session to merge.

## Coverage model

The verifier reads the style rows in both the reference index and the coverage ledger. It requires
each document to contain exactly the ordered range `0–127`, and requires both inventories to match.
This proves the style entry inventory is present; it does not promote a style or concrete profile to
source implementation or behavioral verification.

Concrete registrations retain their full `(type, netId, aiStyle)` identity. Negative `netId` values
are variant identities and are never normalized to positive IDs. Sharing an `aiStyle` does not
register another profile. A tuple that reuses a mapped `(type, netId)` with another style, or a mapped
`(netId, aiStyle)` with another type, is an identity conflict. A changed positive `netId` is also a
conflict when the `(type, aiStyle)` pair has one known identity; negative variant IDs remain explicit
registrations. Other unknown tuples are rejected as unregistered profiles.

The current baseline separates source coverage from finite runtime handler presence:

| Source profile | Identity | Stage | Exact finite handler | Open |
| --- | --- | --- | --- | --- |
| Blue Slime | type=1, netId=1, aiStyle=1 | mapped | yes | yes |
| Demon Eye | type=2, netId=2, aiStyle=2 | mapped | yes | yes |
| Zombie | type=3, netId=3, aiStyle=3 | mapped | yes | yes |
| Mother Slime | type=16, netId=16, aiStyle=1 | mapped | yes | yes |
| Eye of Cthulhu | type=4, netId=4, aiStyle=4 | mapped | yes | yes |
| Lava Slime | type=59, netId=59, aiStyle=1 | mapped | no | yes |
| Guide | type=22, netId=22, aiStyle=7 | mapped | no | yes |
| Old Man | type=37, netId=37, aiStyle=7 | mapped | no | yes |
| Training Dummy | type=488, netId=488, aiStyle=92 | mapped | no | yes |

`mapped` records resolved source identities. The five finite handlers implement bounded slices and
remain open, so none is promoted to `implemented` or `verified`. The finite simulator's Green Slime
`netId=16`, Guide/Old Man `aiStyle=0`, and Training Dummy `aiStyle=0` are not aliases for the source
profiles above. Their legacy fallback or finite behavior can continue to support its declared host
scenario; fallback does not count as a source profile handler or completed coverage. Mother Slime's
exact handler now counts as a finite state slice while its catalog geometry/stats discrepancy remains
open. Neither that handler nor the Eye handler is a verified full profile.

The design records Green Slime's reference `netId=-3`, but the canonical ledger does not yet bind
that variant to a complete `(type, netId, aiStyle)` tuple. The baseline therefore leaves it
unregistered until that mapping is recorded. The focused verifier uses a synthetic negative-ID
fixture to prove that explicitly registered negative values remain distinct from positive IDs.

The API exposes three checks:

- `RequireRegistered(identity)` accepts only a known exact tuple and clearly distinguishes
  unsupported styles, tuple conflicts, and missing profiles.
- `RequireFiniteHandler(identity)` also requires an exact finite profile handler registration.
- `RequireVerified(identity)` requires the stage to be `Verified` and no open work to remain.

The coverage registry is pure domain data. Source-document I/O and JSON report writing stay in the
focused verifier, so content registration may call the same validation API without making the NPC
domain depend on Markdown or filesystem infrastructure.

## Focused verifier

`Test/Terraria.NpcAi.CoverageVerification` checks both inventories, all eight mapped source profile
records, the three exact finite handler registrations, negative variant identity, independent
type/netId/aiStyle conflicts, unregistered profiles, style 128 rejection, duplicate registration,
conflicting registration, handler absence, and rejection of `RequireVerified` while source work
remains open. It also confirms that fallback-named registrations are absent.

At this checkpoint the ledger has 128 style rows: 125 are `indexed`, three are partial/mapped, all
128 remain `open`; the profile registry has eight `mapped` identities, three exact finite handlers,
zero `implemented` or `verified` profiles, and eight open profiles.

Build and run commands for this checkpoint:

```powershell
dotnet build Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-restore --nologo -v:minimal
dotnet run --project Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-build --no-restore
```

The verifier writes its machine-readable report to
`Build/diagnostics/NpcAiCoverage/coverage-gate-report.json` by default. Use `--repo-root <path>` or
`--report <path>` when invoking it outside the repository root or when a separate report path is
needed.

## C/F profile wiring

For each new ordinary enemy or Boss profile:

1. Resolve the source `type`, `netId` (including a negative variant when applicable), `aiStyle`, and
   activation conditions from the fixed reference source. Register a separate tuple even when an
   existing style is shared.
2. Add the profile to `NpcAiProfileCoverageBaseline` at `Mapped` with `HasOpenWork=true`. Set
   `HasFiniteHandler=true` only after an exact profile handler is connected; do not use a generic
   or fallback behavior as that handler.
3. Keep the profile's `CanHandle` predicate exact on type, netId, and aiStyle. Add positive and
   one-field-at-a-time mismatch cases to the focused verifier.
4. Advance to `Implemented` only when the declared profile is connected through its complete runtime
   path. Advance to `Verified` only after source-based behavior and effect evidence passes and all
   declared open work is closed.
5. Rerun the focused gate and merge its profile status row into the canonical coverage ledger.

## Rollback

Rollback this infrastructure slice by removing the new NPC coverage registry and baseline files,
`Test/Terraria.NpcAi.CoverageVerification/`, and this checkpoint document. Remove only the generated
`Build/diagnostics/NpcAiCoverage/coverage-gate-report.json` if the report itself is not needed as
evidence. Do not remove C1–C4, D, E, or Boss profile behavior. If a future C/F batch adds a profile
row, remove that exact row only when its matching handler is rolled back; retain historical evidence
and keep the source profile marked open in the canonical ledger.

The gate does not close network authority/replication, save/load state, unload cleanup, or source
golden comparisons. Those remain G-stage work.

## Status row for the main session

The G1 initial snapshot was merged into the G-stage execution plan and canonical ledger on 2026-10-07:

| G coverage/registration gate | complete for its declared infrastructure slice | 128/128 style rows match across the reference index and ledger (125 indexed, 3 partial/mapped, 128 open); 8 mapped source profiles; 3 exact finite handlers; 0 implemented/verified profiles; all 8 profiles remain open | focused verifier command and `Build/diagnostics/NpcAiCoverage/coverage-gate-report.json` | Network authority, source golden, save/load, and unload cleanup remain open |

## G2 source identity preflight

G2 adds a focused executable preflight for the source baseline used by future behavior goldens. It
reads the three unique SHA-256 rows from the reference index and coverage ledger, reads the
read-only reference root declared by the ledger (or accepts `--reference-root <path>`), recomputes
the three file hashes, and writes a machine-readable report. The project has no `ProjectReference`
to Simulation or the NPC runtime.

The check passed on 2026-10-07:

| Evidence | Command/output | Result |
| --- | --- | --- |
| Restore | `dotnet restore Test/Terraria.NpcAi.SourceIdentityVerification/Terraria.NpcAi.SourceIdentityVerification.csproj --nologo -v:minimal` | exit 0; project restored |
| Build | `dotnet build Test/Terraria.NpcAi.SourceIdentityVerification/Terraria.NpcAi.SourceIdentityVerification.csproj --no-restore --nologo -v:minimal` | exit 0; 0 warnings / 0 errors; artifact `Build/bin/Terraria.NpcAi.SourceIdentityVerification/Debug/net10.0/Terraria.NpcAi.SourceIdentityVerification.dll` |
| Run | `dotnet run --project Test/Terraria.NpcAi.SourceIdentityVerification/Terraria.NpcAi.SourceIdentityVerification.csproj --no-build --no-restore` | exit 0; 3 files checked; index/ledger/source comparisons 6/6; report `Build/diagnostics/NpcAiSourceIdentity/source-identity-report.json` |
| Coverage regression after F1 | `dotnet run --project Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-build --no-restore` | exit 0; 128 rows match, 124 indexed / 4 mapped / 0 implemented / 0 verified; 8 mapped profiles, 3 exact finite handlers, 27 assertions; report `Build/diagnostics/NpcAiCoverage/coverage-gate-report.json` |

The current 124/4 style split includes F1's style 4 mapped row. G1's original 125/3 count is retained
above as its earlier snapshot; the rerun is the current ledger baseline.

This is source provenance evidence, not behavioral source golden evidence. It does not run the
reference server or compare any NPC tick/effect output, and it does not promote profile coverage
stages. Source-output differential remains `not-run`; network authority/replication, save/load,
unload cleanup and old-implementation deletion gates remain open.

## G1 coverage completion gate continuation (2026-10-07)

The baseline now records the already-wired Mother Slime and Eye exact finite handlers. It contains
9 mapped profiles and 5 finite handlers; all remain mapped/open. The historical 8/3 reports above
remain snapshots of the earlier registry, and do not describe the current registration list.

The existing verifier now separates inventory validation from mandatory coverage closure:

```powershell
dotnet build Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-restore --nologo -v:minimal
dotnet run --project Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-build --no-restore -- --require-complete --report Build/diagnostics/NpcAiCoverage/completion-report.json
```

The completion mode writes `Completion.Required`, `Completion.IsReady`, `IncompleteStyles` and
`IncompleteProfiles`, then returns exit 2 when coverage is open. The default inventory mode continues
to return exit 0 for a valid incomplete inventory. A completed style requires a verified stage and
`Profiles=verified`, `Closure=closed`, `Verification=passed`; registered profiles independently require
verified stage, a handler, and no open work. The gate is necessary metadata consistency evidence;
actual source behavior, host integration, authority, persistence and deletion still have separate gates.

Acceptance evidence is under `Build/diagnostics/NpcAiRedesign/coverage-completion-20261007/`:

- Build exit 0, 0 warnings / 0 errors, artifact under `Build/bin/Terraria.NpcAi.CoverageVerification/`.
- Default inventory exit 0: 128 styles, 124 indexed / 4 mapped / 0 implemented / 0 verified;
  9 mapped profiles, 5 finite handlers, 31 assertions; `Completion.IsReady=false`.
- Completion mode exit 2: 128 incomplete styles and 9 incomplete profiles, as expected.
- A diagnostics-only fixture promotes all 128 ledger rows to verified/closed/passed while preserving
  runtime registrations. Completion still returns exit 2 with 0 incomplete style rows and 9 incomplete
  profiles. This proves ledger text alone cannot bypass the profile gate; the canonical ledger is unchanged.

Rollback removes the completion-mode additions from the existing verifier and the two finite-handler
registry updates. Retain the diagnostic reports and do not reinterpret prior inventory passes as
complete coverage.
