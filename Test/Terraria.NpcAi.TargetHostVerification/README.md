# NPC target host verification

This independent verifier covers the NPC target selection domain and the
production `RuntimeNpcTargetSelectionAdapter`/`SelectFinitePlayerTarget` path.
It loads an existing `.wld` through the production world loader, attaches real
Player owners, creates an active Projectile through `ProjectileLifecycleSystem`,
commits target results to real NPC owners, and verifies that the input world file
is unchanged.

Run from the repository root after building this project:

```powershell
dotnet run --project Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj --no-build --no-restore -- src/World/科研.wld Build/diagnostics/NpcAiRedesign/runs/b2-target-host-verification/report.json
```

The report records per-scenario evidence, input-world SHA-256, source-file
fingerprints, and the assemblies loaded by the verifier.
