# MessageBuffer / NetMessage 基线状态

## Source Oracle

The exact source copies are stored under
`src/Terraria.Dome.Protocol.V1456/LegacyReference/` and excluded from SDK compilation.
They are behavior oracles only; no legacy `Terraria.Main`, `Netplay`, XNA, or full entity
object graph is introduced into the new protocol project.

| File | Source bytes | Source lines | SHA-256 | Compile input |
| --- | ---: | ---: | --- | --- |
| `MessageBuffer.cs` | 90,205 | 3,365 | `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE` | No |
| `NetMessage.cs` | 76,936 | 2,721 | `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A` | No |

## Migration Status

| Area | Status | Evidence / next boundary |
| --- | --- | --- |
| Exact source copy and provenance | Complete | `Build/diagnostics/message-buffer-net-message-source-manifest.json` |
| Compile exclusion | Complete | `Terraria.Dome.Protocol.V1456.csproj` removes `LegacyReference/**/*.cs` |
| Protocol isolation contracts | Complete | `Isolation/DomeNetworkIsolation.cs`, immutable envelopes, capacity and sequence checks |
| MessageBuffer adapter | Complete for framed input | `Isolation/MessageBuffer.cs`; length validation, fragmentation and inbound enqueue |
| NetMessage adapter | Complete for framed imperative output | `Isolation/NetMessage.cs`; payload ownership and ordered outbound envelopes |
| DomeServer update bridge | Complete for active mutating commands | `DomeNetworkUpdateBridge` drains at tick start; replication frames flush at tick end |
| Object slices | Complete for initial slice set | Player, NPC, projectile, item, chest, sign, tile-section, world and tile-entity immutable slices |
| Unsupported diagnostics | Complete for isolated bridge | `DomeNetworkIsolation.UnsupportedMessageIds` records explicit `Unsupported` outcomes |
| Broadcast ignore semantics | Complete for isolated outbound path | `NetworkOutboundEnvelope.IgnoreClient` is honored during tick-end flush |
| Initial NPC settlement | Complete for tick-boundary path | `EnsureInitialNpcsCommand` creates NPCs before the initial world stream |
| Full legacy message parity | Partial | Unsupported message IDs remain explicit in the bridge; bootstrap synchronous paths remain direct |

The migration is executable for the isolated framed path, but it is not a claim of complete legacy
message parity. Unsupported branches and synchronous bootstrap responses remain documented residual
work.

## Verification Evidence (2026-08-19)

Commands were run serially from the repository root with `-p:UseSharedCompilation=false`.

| Command | Result |
| --- | --- |
| `dotnet build src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj` | exit 0; 0 warnings; 0 errors |
| `dotnet build src/Terraria.Dome.Protocol.V1456/Terraria.Dome.Protocol.V1456.csproj` | exit 0; 0 warnings; 0 errors |
| `dotnet build src/Terraria.Dome.Server/Terraria.Dome.Server.csproj` | exit 0; 0 warnings; 0 errors |
| `dotnet run Test/Terraria.Dome.NetworkIsolation.Verification` | exit 0; `PASS: 10 network isolation checks` |
| `dotnet run Test/Terraria.Dome.Protocol.Compatibility.Verification` | exit 0; V1456 length contracts pass |
| `dotnet run Test/Terraria.Dome.World.Protocol.Verification` | exit 0; three world protocol checks pass |
| `dotnet run Test/Terraria.Dome.World.Loopback.Verification` | exit 0; authoritative world entry and requested section pass |
| `dotnet run Test/Terraria.Dome.TileInteraction.Verification` | exit 0; seven typed interaction and loopback checks pass |
| `dotnet run Test/Terraria.Dome.WiringLiquidChest.Loopback.Verification` | exit 0; four wiring/liquid/chest loopback checks pass |
| `dotnet run Test/Terraria.Dome.FullClientBootstrap.Verification` | exit 0; `PASS: full-client bootstrap completes without source-player authority replay` |

TileInteraction 在早期串行运行中曾在首个可见 delta 等待处出现 3 秒间歇超时；重新编译依赖
后连续 3 次通过全部 7 项。验证器的 quiet window 只针对 `SyncNPC`，会继续消费计划中的非
NPC 世界变化帧。

The full-client bootstrap verifier is green. `EnsureInitialNpcsCommand` is processed at the Dome tick
boundary before the initial world stream, and `CreateDefaultWorldNpcs` uses the resolved spawn Y
coordinate so the NPCs remain in the player's visible section. `DefaultJoinNpcFrameBudget` bounds
the subsequent settlement projection. Dynamic responses, including Ping, are
queued through `EnqueueOutboundFrame` and the isolated `NetMessage` outbound list; only the explicit
synchronous Hello/NetModules, sign-open, WorldData, initial world/section, and PlayerSpawn completion
frames remain direct session-writer boundaries. Full legacy 162-message parity remains Partial.
There is no independent runtime feature switch for reverting to a legacy session path yet; the
synchronous bootstrap boundary is documented but is not a hot rollback toggle.
