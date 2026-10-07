# T2-B2 implementation slice — session World bootstrap

**Status:** `partial / in progress / compile-only`. This records the first production slice after B2.0; it is not the completed B2.1–B2.4 cutover.
**B2.0 freeze:** commit `68dcce5`.
**Source base:** `68dcce5` plus the working changes listed below.

## Changed in this slice

- `NSSLC.Application.csproj` now directly references `Arch 2.1.0`, because `LoadedWorldSession` creates and owns an `Arch.Core.World`.
- `LoadedWorldSession` creates the Arch World before generating its `EntityRuntimeId`; that same token is passed to the existing runtime during this transition. The token remains a session epoch and is independent of `World.Id`.
- The session creates one Arch singleton Entity containing the existing world state component instances: tile metrics, town housing, descriptor, rules, time/weather, progression, appearance, NPC history, milestones, season policy, load lifecycle, dimension compatibility, and tree-top state. The singleton holds the same component objects exposed by the current `WorldSessionRestoreState` properties; no second copies of those objects are made.
- Guarded `ArchWorld` and `WorldStateEntity` access checks the session owner thread. Dispose releases the storage/runtime and then the Arch World; repeated Dispose is covered by the selected smoke.
- The existing T2 probe now references `NSSLC.Application` and checks the production session's World/token, world singleton Rules mutation, singleton-compatible `IsFresh`, cross-thread rejection, and repeated disposal.

## Remaining cutover work

This slice still has the legacy `EntityRuntime` as the live gameplay-entity authority. The registry still maps `RuntimeEntityHandle`; the singleton's component instances are also reachable through the old `WorldSessionRestoreState` aggregate; and `WorldStorageRoot` still accepts `EntityRuntime`. These are incomplete B2 work, not permanent compatibility APIs. No dual-write of the same component objects was added, but production entity creation/resolution is not yet Arch-only.

Next in the same batch: replace registry live values with `(EntityRuntimeId, Arch.Entity)`, make `LoadedWorldSession` the only session World/identity owner, migrate the actual slot and runtime caller closure without changing T3/T4 rules, route world state reads/writes through the singleton, then remove the custom runtime from the production path. The B2.0 closure inventory remains the per-file index.

The latest acceptance instruction is compile-only: continue source wiring and affected-project incremental builds; do not run load, tick, World cleanup, identity rejection, smoke, `dotnet test`, `dotnet run`, or another runtime verification command. The requested change record `.agent-workplace/changes/CR-2026-10-08-arch-compile-only-acceptance.md` is absent from this worktree, `HEAD`, and `main`; the user's direct compile-only instruction is applied.

Historical only, before the compile-only steering: the temporary T2 probe build/run exercised this slice, including owner thread rejection, singleton Rules mutation, `IsFresh`, and repeated Dispose. Those runtime outputs are retained in the adjacent logs for traceability but are **not a current acceptance gate** and must not be cited as current B2 runtime evidence. The temporary probe project-reference and runtime assertions were then reverted to keep this compile-only slice out of the test source. T1's malformed-target CommandBuffer path remains out of scope.

Runtime areas currently unverified for this batch: the five-case B2 risk sample; candidate failure after registration; old token/foreign World rejection; entity/UUID reuse; actual production loading, tick, unload, World cleanup, rollback/retry; and Simulation one-tick/exit.

## Commands and results recorded before compile-only steering

The following table is historical. Build results can be used only for the source hashes recorded at that point; the DLL produced by the temporary probe reference is stale after reverting that probe wiring. The runtime row is explicitly not a current gate.

SDK: `dotnet --version` → `10.0.400`.

| Command | Project / purpose | Exit | Warnings / errors | Output |
| --- | --- | ---: | ---: | --- |
| `dotnet restore src/NSSLC.Application/NSSLC.Application.csproj --nologo` | Application direct Arch package restore | 0 | 0 / 0 | `Build/diagnostics/ArchMigration/T2-B2/application-restore.log`; assets under `Build/obj/` |
| `dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --nologo` | First compile attempt; caught a `World` member/type name collision and wrong static member resolution | 1 | 6 / 3 | `application-build.log`; errors were corrected by fully qualifying `Arch.Core.World` |
| `dotnet restore Test/Terraria.ArchMigration.T2Probe/Terraria.ArchMigration.T2Probe.csproj --nologo` | Restore probe plus Application closure | 0 | 0 / 0 | `t2-probe-restore.log`; assets under `Build/obj/` |
| `dotnet build Test/Terraria.ArchMigration.T2Probe/Terraria.ArchMigration.T2Probe.csproj --no-restore --nologo` | Final build of the probe and affected Application closure | 0 | 0 / 0 | `Build/bin/Terraria.ArchMigration.T2Probe/Debug/net10.0/`; `t2-probe-build-final.log` |
| `dotnet Build/bin/Terraria.ArchMigration.T2Probe/Debug/net10.0/Terraria.ArchMigration.T2Probe.dll` | Historical exploratory production session World/singleton smoke; no external world-file input | 0 | 0 / 0 | `t2-probe-run-final.log`; **not a current gate** |

`NSSLC.Application` resolved `Arch 2.1.0` asset `lib/net8.0/Arch.dll` for `net10.0`. Package SHA-256 is `F29C58EC7A884DF0411FB1A54E154A2C1597A9B7F8815A8C29AD0CB0E33A7505`; resolved DLL SHA-256 is `75194E99FB5507995894BED812381C3338F241DF38C69C3E3288856CE1412386`.

## Source, assets, and output hashes

| Input / output | SHA-256 |
| --- | --- |
| `src/NSSLC.Application/NSSLC.Application.csproj` | `246F1709ADB39BEF2C15CDA128C92A19A641EE3A021B2F0DB1368A5E8D98F11A` |
| `src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs` | `23E961D8D2D54B56A53A9EBC255BA47D430509ADD08A5FEF1CD9A7EB7EE5738E` |
| `src/NSSLC/Component/Share/Entity/System/EntityRuntime.cs` | `96AEC6243A894A72A7B56DBE7F80D1FDBB2CD8B28ABE1EBD392BD71547CCC904` |
| `Test/Terraria.ArchMigration.T2Probe/Terraria.ArchMigration.T2Probe.csproj` | `A291317E170418277A79B513A71ADD5015D5AB796D263170D6443CC4DFB47A4E` |
| `Test/Terraria.ArchMigration.T2Probe/Program.cs` | `33248240848EB946C877E5ED6C77CC9D8D237FABD3F78BB95E956581A72A4AFC` |
| `Build/obj/NSSLC.Application/project.assets.json` | `93D5145C007D7EA987A34A91A94882889AF21F61C105756BD3E50C8DEB381902` |
| `Build/obj/Terraria.ArchMigration.T2Probe/project.assets.json` | `49258E7C374D97B84C2D7981B800DDFA74D1AAC6CAB138634F2966A670D40BB2` |
| `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` | `AD0BF4F2F3DDBB7E448F6A294E3ADE7BD69AF71F8982F350DA1979FD82DF311B` |
| `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.pdb` | `9F58D6D4FBA83AE730EFD21409FBE6E0043A0451FD20A61812E0619F37FBA3D2` |
| `Build/bin/Terraria.ArchMigration.T2Probe/Debug/net10.0/Terraria.ArchMigration.T2Probe.dll` | `34297E73B28ABD660B9E6067A174935124BD1F270F0DCD58F63B143ED2EC74E5` |
| `Build/bin/Terraria.ArchMigration.T2Probe/Debug/net10.0/Terraria.ArchMigration.T2Probe.pdb` | `6DE898B60EC02949305E2434820186A770E1AFBA81E9101ACF0633BBFB4914BC` |

## Current disposition

Only the current compile-only builds may satisfy this iteration's gate. The historical runtime output does not. T2 remains `partial / in progress` until the registry, production entity caller closure, world-state access path, slot owners, candidate cleanup, and the revised compile-only handoff requirements are completed and evidenced.
