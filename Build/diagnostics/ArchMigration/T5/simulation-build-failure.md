# Simulation fixture host build failure

Command: `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj -p:FixtureHostBuild=true --nologo`

- Exit code: 1
- Build summary: 23 warnings, 1 error
- First and only error: `src/NSSLC.Infrastructure/Network/NSSLC.Infrastructure.Network.csproj(24,5): error MSB3073`; generator process exited 1.
- Exact failing command in the full log: `dotnet "<repo>\Build\bin\NetworkCodecGenerator\Debug\net10.0\NetworkCodecGenerator.dll" "<repo>\Build\generated\Network\Debug\net10.0"`.
- The same restore/build log says `NetworkCodecGenerator.dll` was built to `Build/bin/FixtureHost/NetworkCodecGenerator/Debug/net10.0/NetworkCodecGenerator.dll`, while `NSSLC.Infrastructure.Network.csproj` line 22 constructs a non-fixture `Build/bin/NetworkCodecGenerator/...` path at line 24. That expected file was absent.
- Root cause: repository fixture output override and a hard-coded generator invocation path disagree. SDK preflight independently returned `10.0.400`; this is a project output-path mismatch, not an environment diagnosis.
- Warning/error totals are read from the MSBuild summary at the end of the preserved 107-line log. Source hash: `D22E54FEBE9AD29ECF8DB40E872A77D0A7195ECA3174C1625265B23527FC0CFE`.
- No target Simulation DLL/PDB was produced. Generator fixture artifacts and project asset hash, when present, are listed in `command-log.jsonl`.
- Direction change: run the same affected project under the repository's normal output configuration, which aligns the project reference's generator output with the invocation target. No source or project file is modified.
