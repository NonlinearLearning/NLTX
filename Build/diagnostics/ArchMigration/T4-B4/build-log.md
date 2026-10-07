# T4-B4 compile log

- Acceptance source: user instruction and accepted CR-2026-10-08-arch-compile-only-acceptance.md.
- SDK selection: `global.json`, .NET SDK `10.0.400`; target framework `net10.0`.
- Output policy: repository `Directory.Build.props` routes output to `Build/bin/` and intermediate
  files to `Build/obj/`.

## Incremental build results

| Command | Project | Exit | Warnings / errors | Output |
| --- | --- | ---: | ---: | --- |
| `dotnet build Test/Terraria.Arch.SystemVerification/Terraria.Arch.SystemVerification.csproj --no-restore -v:minimal` | `Terraria.Arch.SystemVerification` | 0 | 0 / 0 | `Build/bin/Terraria.Arch.SystemVerification/Debug/net10.0/Terraria.Arch.SystemVerification.dll` |
| `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj -v:minimal` | `NSSLC.Tools.Simulation` | 0 | 17 / 0 | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| `dotnet build src/NSSLC.Tools.NetworkServer/NSSLC.Tools.NetworkServer.csproj -v:minimal` | `NSSLC.Tools.NetworkServer` | 0 | 0 / 0 | `Build/bin/NSSLC.Tools.NetworkServer/Debug/net10.0/NSSLC.Tools.NetworkServer.dll` |
| `dotnet build src/NSSLC/Component/Items/Terraria.Items.csproj --no-restore -v:minimal` | `Terraria.Items` | 0 | 0 / 0 | `Build/bin/Terraria.Items/Debug/net10.0/Terraria.Items.dll` |
| `dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore -v:minimal` | `NSSLC.Application` | 0 | 0 / 0 | `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` |
| `dotnet build src/NSSLC.Infrastructure/Network/NSSLC.Infrastructure.Network.csproj --no-restore -v:minimal` | `NSSLC.Infrastructure.Network` | 0 | 0 / 0 | `Build/bin/NSSLC.Infrastructure.Network/Debug/net10.0/NSSLC.Infrastructure.Network.dll` |
| `dotnet build Test/Terraria.Items.Verification/Terraria.Items.Verification.csproj --no-restore -v:minimal` | `Terraria.Items.Verification` | 0 | 0 / 0 | `Build/bin/Terraria.Items.Verification/Debug/net10.0/Terraria.Items.Verification.dll` |
| `dotnet build Test/Terraria.Items.NetworkOwner.Verification/Terraria.Items.NetworkOwner.Verification.csproj -v:minimal` | `Terraria.Items.NetworkOwner.Verification` | 0 | 0 / 0 | `Build/bin/Terraria.Items.NetworkOwner.Verification/Debug/net10.0/Terraria.Items.NetworkOwner.Verification.dll` |
| `dotnet build Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-restore -v:minimal` | `NSSLC.Infrastructure.Network.Verification` | 0 | 0 / 0 | `Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/NSSLC.Infrastructure.Network.Verification.dll` |
| `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore -v:minimal` | `Terraria.NpcAi.Verification` | 0 | 0 / 0 | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/Terraria.NpcAi.Verification.dll` |

The `NSSLC.Tools.Simulation` invocation restored four project references and reported 17
warnings, all from `src/NSSLC.Infrastructure/WorldGeneration` sources; it reported 0 errors and
produced the Simulation assembly under `Build/bin/`. The remaining invocations reported 0
warnings and 0 errors. Exact current DLL/PDB and input hashes are in
[source-hashes.md](source-hashes.md).

The main acceptance owner supplied a separate production-closure summary of 0 errors / 6
warnings. Its command and detailed log were not included in this worktree, so that summary is
recorded separately from the 17-warning local Simulation build.

## Historical runtime commands, excluded from current acceptance

These commands ran before the compile-only steering. Their output is kept as history only; they do
not satisfy the current gate, and no runtime command was run after the steering.

| Historical command | Exit | Recorded output summary | Current acceptance use |
| --- | ---: | --- | --- |
| `dotnet run --project Test/Terraria.Arch.SystemVerification/Terraria.Arch.SystemVerification.csproj --no-build --no-restore` | 0 | Probe printed `PASS Arch.System 1.1.0 lifecycle and World-bound query probe`. | Historical only; no runtime claim. |
| `dotnet run --project Test/Terraria.Items.Verification/Terraria.Items.Verification.csproj -- items-domain` | 0 | Item verification printed `items-domain passed`. | Historical only; no item behavior claim. |
| `dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj -- --social-npc-effects` | 0 | Social NPC effect verifier printed `PASS ... world scope`. | Historical only; no network queue claim. |
| `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj` | 0 | NPC AI verifier printed its PASS summary. | Historical only; no tick/RNG claim. |

The earlier static assembly/source scan also predates the acceptance change and is not an
admission or serialization smoke result.
