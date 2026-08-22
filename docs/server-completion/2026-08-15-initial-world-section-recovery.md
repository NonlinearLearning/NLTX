# Initial World Section Recovery

Date: 2026-08-15

## Symptom

A complete Terraria client could complete the Dome handshake and enter the world, but the visible
world contained no tiles.

## Root Cause

The initial `SpawnTileData` request is V1456 message 8. The complete-client reference initializes
`Player.SpawnX` and `Player.SpawnY` to `-1`, then sends those fields in its message-8 request:

- `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:2404-2406`
- `D:\TRbackup\无任何删减通过编译\Terraria\Netplay.cs:623`

The Version4 server always sends the server world-spawn `5 x 3` section neighborhood first. It
only adds a client-position neighborhood when the client coordinate is valid:

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs:411-537`

`WorldSectionReplication` instead clamped `(-1, -1)` to `(0, 0)` and used that as its only initial
section center. Dome world data declares the default spawn as `(2100, 300)`, so the client camera
at that spawn did not receive the sections that contained its terrain.

## Repair

`src/Terraria.Dome.Server/Replication/WorldSectionReplication.cs` now obtains the initial center
from `GetWorldSpawnSectionCoordinates()`. Both initial-section collection entry points use this
server-authoritative spawn section. For the default `4200 x 1200` Dome world, the coordinate is
`(2100, 300)`, matching the world-data bootstrap packet.

This restores the required behavior for a complete client sending the initial `(-1, -1)` request
without altering V1456 tile compression, packet framing, or the later movement-based PVS path.

## Regression Evidence

`Test/Terraria.Dome.World.Loopback.Verification/Program.cs` now sends:

```csharp
new SpawnTileDataRequestPacket(-1, -1, 0)
```

Before the repair, the verifier exited `1` with:

```text
Server did not stream the authoritative WorldGrid tile during world entry.
```

After the repair, these commands exited `0`:

```text
dotnet run --project Test\Terraria.Dome.World.Loopback.Verification\Terraria.Dome.World.Loopback.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.World.Protocol.Verification\Terraria.Dome.World.Protocol.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.SessionReplication.Verification\Terraria.Dome.SessionReplication.Verification.csproj -p:UseSharedCompilation=false
dotnet build Terraria.Dome.sln -p:UseSharedCompilation=false --no-restore
dotnet build src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -c Release -p:UseSharedCompilation=false --no-restore
```

The solution build and final Release server build both reported `0` warnings and `0` errors.

## Current Runtime

The complete-client entry point remains `127.0.0.1:7777`.

```text
127.0.0.1:7777  PID 21408  Terraria frame trace proxy
127.0.0.1:7778  PID 2364   Terraria.Dome.Server Release backend
```

The proxy forwards `7777` to `7778`; it is retained so protocol frames remain available in
`Build/diagnostics/terraria-7777-frame-trace.log`. The server startup log is
`Build/diagnostics/terraria-dome-server-7778.stdout.log`.
