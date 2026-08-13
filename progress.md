# Project Context

Last updated: 2026-08-13

`Terraria.Dome.sln` is the build entry point for a minimal, authoritative Arch ECS
Dome. It targets `net10.0` and consists of:

- `Terraria.Dome.Simulation`: Arch World, gameplay components, systems, commands
  and immutable snapshots.
- `Terraria.Dome.Transport`: private line-delimited JSON input and snapshot DTOs.
- `Terraria.Dome.Server`: loopback TCP host and session-to-server-owned Player
  mapping.
- `Terraria.Dome.Client`: interactive console client with `a`, `d`, `j`, `f`, and
  `q` commands.
- `Terraria.Dome.Verification`: executable behavior and loopback verification.

The implementation deliberately does not introduce a replacement `Entity.cs`,
`EntityState`, legacy `Main` arrays, `whoAmI`, or untyped `ai[]` into the new
simulation. It also does not claim legacy Terraria protocol compatibility.

Verified from repository root on 2026-08-13:

```text
dotnet restore Terraria.Dome.sln -p:UseSharedCompilation=false
dotnet build Terraria.Dome.sln --no-restore -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -p:UseSharedCompilation=false
```

The build completed with `0` warnings and `0` errors. The verifier proved Player
movement, jump/ground collision, Npc target/chase, Projectile damage/despawn and a
real loopback Client/Server interaction. All generated artifacts are under
`Build/bin`, `Build/obj`, `Build/generated`, or `Build/packages`.
