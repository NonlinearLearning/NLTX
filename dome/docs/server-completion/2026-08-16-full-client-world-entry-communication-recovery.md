# Full Client World Entry Communication Recovery

Date: 2026-08-16

## Scope

This record covers the Dome V1456 startup path exercised by the source-built
Terraria client at `127.0.0.1:7777`. The required outcome was that the client
enters the world with an initial terrain projection and stays connected long
enough to prove that the connection did not immediately fail.

## Confirmed Causes

The initial live trace established that the complete client sends `(-1, -1)`
in both `SpawnTileData` and its first `PlayerSpawn`. These are client sentinel
coordinates, not an authority grant. Dome passed them into simulation player
creation and then projected an invalid position to the client.

The next live trace exposed a separate state-machine error. Dome sent a
server-to-client `PlayerSpawn` back to the initiating connection. The complete
client responded by sending another `PlayerSpawn(-1, -1)` every frame. The
reference server source at
`D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs` sends
message 12 with the initiating client as `ignoreClient`; it broadcasts a
spawn to other clients instead of echoing it to its source.

## Implemented Protocol Rules

- `WorldSectionReplication.ResolvePlayerSpawn` now chooses the shared world
  spawn only when the client sends a negative coordinate. Valid non-negative
  requests remain valid for existing loopback and movement scenarios.
- `TerrariaProtocolSessionHost` creates the initial simulation player from
  that resolved coordinate. Its `PlayerControls` projection therefore uses
  the same server-owned position.
- Initial completion no longer writes `PlayerSpawn` to the initiating session.
  The emitted connection stream begins with `PlayerActive`, `SyncPlayer`, and
  `PlayerControls`, followed by authority frames and
  `FinishedConnectingToServer`.
- `DomeClient` and the affected verification programs now model this
  source-excluding behavior rather than requiring a self-echoed spawn frame.
- The source-built client test runtime now supports `join-stable`. It waits
  for a local active player, then requires the connection, client mode, menu
  state, and local player to remain valid for ten seconds before succeeding.

## Test-First Evidence

The full-client bootstrap verifier was first changed to send the real
`PlayerSpawn(-1, -1)` sequence. Before the server change, it failed with:

```text
The server must replace the client's sentinel spawn with the authoritative world spawn.
```

After adding the authoritative fallback, it passed. A second temporary
expectation that required an active-spawn acknowledgement failed in two
seconds, and its live trace proved that the acknowledgement produced a client
spawn loop. The final regression instead requires that the source session does
not receive `PlayerSpawn`; that test failed against the self-echoing stream
and passed after the echo was removed.

## Automated Verification

All commands below were run from `D:\TRbackup\NLTX` with serial compilation:

```text
dotnet build Terraria.Dome.sln -c Release -m:1 -p:UseSharedCompilation=false --no-restore --nologo
```

Result: exit code `0`, `0` warnings, `0` errors.

The following runtime verifiers passed after the final server change:

```text
Terraria.Dome.Verification
Terraria.Dome.FullClientBootstrap.Verification
Terraria.Dome.SessionReplication.Verification
Terraria.Dome.World.Loopback.Verification
Terraria.Dome.World.Server.Verification
Terraria.Dome.World.Protocol.Verification
Terraria.Dome.PlayerAuthority.Verification
Terraria.Dome.Hardening.Loopback.Verification
```

The source-built client was rebuilt with:

```text
dotnet build Terraria.csproj -c Debug -m:1 -p:UseSharedCompilation=false --nologo
```

Result: exit code `0`, `82` existing compiler warnings, `0` errors.

## Final Complete-Client Evidence

The final server binary was launched on `127.0.0.1:7778`; a transparent local
frame proxy listened on `127.0.0.1:7777`. The real client executable was:

```text
D:\TRbackup\客户端\bin\Debug\net40\Terraria.exe
```

It was run with `-join 127.0.0.1 -port 7777 -testautomation join-stable` and
fresh save, result, and log directories. The final result is stored at:

```text
Build\diagnostics\final-e2e-20260816\client\join-stable.json
```

```json
{
  "scenario": "join-stable",
  "success": true,
  "message": "Client remained connected with an active local player.",
  "elapsedMilliseconds": 24568,
  "playerSlot": 1,
  "netMode": 1
}
```

The final trace is:

```text
Build\diagnostics\final-e2e-20260816\frame-trace.log
```

Its relevant observed counts are:

| Frame or event | Count | Meaning |
| --- | ---: | --- |
| S2C `TileSection` | 20 | Fifteen initial sections plus five later visibility sections |
| S2C `InitialSpawn` | 1 | Initial terrain stream completed once |
| C2S `PlayerSpawn` | 1 | Client issued its expected initial spawn request |
| S2C `PlayerSpawn` | 0 | No source-echo spawn loop |
| S2C `FinishedConnectingToServer` | 1 | Connection completion emitted once |
| EOF or socket error during observation | 0 | No observed transport loss before test completion |

The automated stability window is ten seconds after the client first becomes
active. It proves entry, complete initial section delivery, and absence of an
immediate connection loss; it is not a claim of indefinite-session coverage.
