# T4-B4 compile-only handoff

| Field | Value |
| --- | --- |
| Track | T4 |
| Batch | B4 |
| Status | partial |
| Overall prerequisite status | blocked-by-prerequisite |
| Acceptance mode | compile-only |
| Baseline | `9bdde4eb7b316e27d78eb6463da68355dafd6fb9` |
| Evidence commit | `9550753cb35344e44d3132bce8cfb6a37e943d10` |
| Branch | `codex/arch-migration-t4` |
| Date | 2026-10-08 |

## Acceptance rule

The user accepted CR-2026-10-08-arch-compile-only-acceptance.md. The current gate permits only
incremental `dotnet build` for affected projects. Do not interpret a successful build as runtime,
behavior, performance, serialization, admission, host, or gameplay evidence.

Behavior commands completed before the compile-only steering are listed in
[build-log.md](build-log.md) as historical records only. They are excluded from this handoff's
acceptance result. No `dotnet run`, `dotnet test`, smoke, packet, item, query, RNG, serialization,
or admission command was run after the rule changed.

## Result

The isolated `Arch.System 1.1.0` / `Arch 2.1.0` probe project compiles for `net10.0`. All 10
locally recorded incremental builds exited 0: nine reported 0 warnings / 0 errors, and the
Simulation closure reported 17 warnings / 0 errors from `NSSLC.WorldGeneration`. The exact
project counts and outputs are listed below and in [build-log.md](build-log.md).

The main acceptance owner separately reported its T4 production closure at 0 errors / 6 warnings.
That log was not present in this worktree, so this handoff preserves both scopes: the local
Simulation command's observed count is 17 warnings; the main acceptance summary is 6 warnings.
No runtime work was done to reconcile those counts.

No production source, project reference, or build policy was changed. T4 production code still
uses the custom `EntityRuntime` path. T2's production World/identity signatures and T3's lifecycle
contracts are not integrated in this checkout, so the probe is not a production migration.

## Build evidence

Exact commands, exit codes, warning/error counts, output paths, and artifact hashes are in
[build-log.md](build-log.md) and [source-hashes.md](source-hashes.md). Outputs are under
`Build/bin/`; intermediates and restored packages are under `Build/obj/` and `Build/packages/`.

Local incremental build matrix:

| Project | Exit | Warnings / errors | Output |
| --- | ---: | ---: | --- |
| `Terraria.Arch.SystemVerification` | 0 | 0 / 0 | `Build/bin/Terraria.Arch.SystemVerification/Debug/net10.0/Terraria.Arch.SystemVerification.dll` |
| `NSSLC.Tools.Simulation` | 0 | 17 / 0 | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| `NSSLC.Tools.NetworkServer` | 0 | 0 / 0 | `Build/bin/NSSLC.Tools.NetworkServer/Debug/net10.0/NSSLC.Tools.NetworkServer.dll` |
| `Terraria.Items` | 0 | 0 / 0 | `Build/bin/Terraria.Items/Debug/net10.0/Terraria.Items.dll` |
| `NSSLC.Application` | 0 | 0 / 0 | `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` |
| `NSSLC.Infrastructure.Network` | 0 | 0 / 0 | `Build/bin/NSSLC.Infrastructure.Network/Debug/net10.0/NSSLC.Infrastructure.Network.dll` |
| `Terraria.Items.Verification` | 0 | 0 / 0 | `Build/bin/Terraria.Items.Verification/Debug/net10.0/Terraria.Items.Verification.dll` |
| `Terraria.Items.NetworkOwner.Verification` | 0 | 0 / 0 | `Build/bin/Terraria.Items.NetworkOwner.Verification/Debug/net10.0/Terraria.Items.NetworkOwner.Verification.dll` |
| `NSSLC.Infrastructure.Network.Verification` | 0 | 0 / 0 | `Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/NSSLC.Infrastructure.Network.Verification.dll` |
| `Terraria.NpcAi.Verification` | 0 | 0 / 0 | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/Terraria.NpcAi.Verification.dll` |

The warning set came from `NSSLC.WorldGeneration` dependency sources. No warning was reported
against the new Arch verification project. See the full console result summarized in
[build-log.md](build-log.md).

## System and phase evidence

[system-concept-map.md](system-concept-map.md) records the NuGet/source package baseline, the
observable Arch.System lifecycle surface, the existing WorldSimulationKernel phase contracts, and
the NPC movement ordering candidate. These are source/package mapping notes; the earlier probe
run is historical and does not count as current behavior acceptance.

## Uncompiled or unverified scope

- No T4 production System was switched to `Arch.System`, `World.Query`, or Arch component access.
- T4 production remains on the custom `EntityRuntime` path; T2 World/identity and T3 lifecycle/
  relationship production contracts are not integrated in this checkout.
- No production Query/ref-to-structural-change boundary is accepted from this build-only gate.
- No queued cross-world/stale request was exercised under the Arch entity model; T2/T3 production
  contracts remain prerequisites.
- No item reservation expiry, duplicate pickup, quantity conservation, or transfer failure was
  run under the current acceptance gate.
- No same-tick/next-tick or RNG path was run under the current acceptance gate. The static NPC
  ordering map is not a runtime proof.
- No DTO serialization or admission smoke was run under the current acceptance gate. Existing
  source and assembly scans are historical static evidence only.
- No full test suite, Simulation host run, world switch, save/load, rollback, performance, or A8
  exit audit was run.

## Changed files and commit

Production changes: none. The evidence commit is
`9550753cb35344e44d3132bce8cfb6a37e943d10`, based on baseline
`9bdde4eb7b316e27d78eb6463da68355dafd6fb9`. Its changed files are:

- `Test/Terraria.Arch.SystemVerification/Terraria.Arch.SystemVerification.csproj`
- `Test/Terraria.Arch.SystemVerification/Program.cs`
- `Build/diagnostics/ArchMigration/T4/handoff.md`
- `Build/diagnostics/ArchMigration/T4-B4/build-log.md`
- `Build/diagnostics/ArchMigration/T4-B4/failure-audit.md`
- `Build/diagnostics/ArchMigration/T4-B4/handoff.md`
- `Build/diagnostics/ArchMigration/T4-B4/source-hashes.md`
- `Build/diagnostics/ArchMigration/T4-B4/system-concept-map.md`

This handoff alignment changes documentation only; it does not change the compiled source inputs,
so no additional build was run. The six untracked user plan files remain untouched and unstaged.
Build output and package caches are not part of the commit.

## Next owner action

Keep T4 production integration blocked until the accepted T2 World/identity contract and T3
lifecycle/relationship contract are available in an integrated checkout. Once the user restores a
runtime acceptance gate, define its test budget and runtime evidence separately; this handoff
contains compile evidence only.
