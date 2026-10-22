# P01 Protocol and Transport Handoff

Status: implementation source compiles; runtime integration and acceptance are pending.

## Delivered boundary

The P01 source boundary is `src/MinimalImplementation/Protocol/` plus the isolated generator in
`src/MinimalImplementation/Tools/WireCompiler/`. The Protocol project already includes these
files, and `MinimalImplementation.Server.csproj` references that project. The Server project now
also explicitly includes the Player and Session source that its packet handlers and inventory
adapters consume, keeping compile inclusion explicit rather than relying on default globbing.

The transport surface provides:

- `PacketFrameCodec` for Version4's little-endian `UInt16 totalFrameLength`, one-byte message ID,
  and body. Message 82 requires its two-byte module ID at the start of the body.
- `PacketFrameAssembler` for bounded frames split across arbitrary TCP receive chunks.
- `PacketTcpConnection` for ordered inbound frames, a bounded single-writer send queue, receive and
  send deadlines, session epochs, packet sequences, and send certainty receipts.
- `TcpListenerAdapter` for bounded concurrent socket acceptance and connection callback lifetime.
- `Version4WireProfile` for direction-specific decode and encode bindings over the generated
  protocol descriptors. Decode requires complete frame-body consumption; encode returns a
  `Version4EncodedPacket` with message ID and body.
- `Version4PacketConnection` for one accepted server socket. It turns raw frames into decoded
  client-to-server packets before invoking the application callback, checks Packet 82's framed
  module ID against the decoded module, and sends committed `OutboundPacketIntent` values through
  the ordered transport queue. `Version4WireProfile.CreateOutboundIntent` encodes a server packet
  once for one or more target epochs; the connection helper targets its own epoch.
- `Version4ProtocolFactsInput` and `Version4ProtocolFactsFactory` for publishing the real content
  directories and protocol-dependent resolvers once per process. The production factory has no
  default input path; callers must provide nonempty, shape-compatible directories and a SHA-256
  string. The factory checks the hash format and publishes it, while the caller remains responsible
  for deriving that hash from the actual content facts.
- `Version4ModuleCodecs` for all 15 module IDs. Direction-dependent text and crafting payload
  layouts are selected by the explicit server role.

The standalone wire generator constructs schema-only `ProtocolInputs` in its own process to bind
the compiler model. That process only emits C# sources. Generated codecs obtain runtime dependencies
from `ProtocolInputs.Instance`; the generated source does not include the schema process's input
arrays or resolver results.

## Changed source paths

New server-owned files are under:

- `src/MinimalImplementation/Protocol/`
- `src/MinimalImplementation/Tools/WireCompiler/`
- `src/MinimalImplementation/MinimalImplementation.Server.csproj`
- `src/MinimalImplementation/Player/PlayerOwner.cs`
- `src/MinimalImplementation/Player/PlayerRules.cs`
- `src/MinimalImplementation/Player/PlayerTeam.cs`
- `src/MinimalImplementation/Session/SessionAdmissionCoordinator.cs`
- `src/MinimalImplementation/Inventory/Crafting/ICraftingChestAccessPolicy.cs`
- `src/MinimalImplementation/Inventory/Crafting/LeaseAwareCraftingChestAccessPolicy.cs`

The narrow Infrastructure codec changes are:

- `src/NSSLC.Infrastructure/Network/Pocket/DataPacketDefinition/Codecs/Chests/QuickStackChestsRequestPacket.Codec.cs`
- `src/NSSLC.Infrastructure/Network/Pocket/DataPacketDefinition/Codecs/Chests/QuickStackChestsResponsePacket.Codec.cs`
- `src/NSSLC.Infrastructure/Network/Pocket/DataPacketDefinition/Codecs/TileEntities/PacketTileEntityCodecsV4.cs`
- `src/NSSLC.Infrastructure/Network/Pocket/DataPacketDefinition/Codecs/Tiles/AreaTileChangePacket.Codec.cs`
- `src/NSSLC.Infrastructure/Network/Pocket/DataPacketDefinition/Definitions/PacketDefinitions.cs`

Packet 85's request body is `Int32 slotCount`, that many signed `Int16` inventory slot IDs, and a
boolean smart-stack flag. Its response is `Int32 blockedCount` followed by that many `UInt16` chest
IDs. Counts are bounded against the framed body, and readers reject body-length mismatches.

## Root integration contract

The root must build a `Version4ProtocolFactsInput` from actual Version4 content facts and call
`Version4ProtocolFactsFactory.Create` exactly once before constructing a `Version4WireProfile` or
any generated packet codec. It must not use WireCompiler's schema-only inputs in the server process.

For a connection, the root can pass `TcpListenerAdapter.RunAsync` a callback that owns a
`PacketTcpConnection`. The connection callback should decode client-to-server message IDs through
`Version4WireProfile`, route packet 82 through the selected module codec, authenticate the session
before domain access, and deliver committed `OutboundPacketIntent` values through `SendAsync` after
encoding them with the server-to-client direction. The root still owns that registration, the
connection epoch/admission policy, and the application-level handlers.

## P01 core scenario inventory

The P01 core inventory contains twenty scenarios, `SC001`–`SC020`. The global selection includes
none of them; SC001, SC002, and SC007 are high-risk candidates for a future bounded run. This
inventory describes acceptance coverage and does not claim that a listener or application router is
already integrated.

| Scenario | Core assertion | Status |
| --- | --- | --- |
| SC001 | Frame encoding preserves little-endian total length, message ID, and packet 82 module prefix; packet 85 round-trips its body. | Not selected. |
| SC002 | Short, oversized, invalid-length, truncated module, and trailing-byte frames fail with stable framing errors. | Not selected. |
| SC003 | The assembler reconstructs one frame split at every header and body boundary. | Not selected. |
| SC004 | The assembler emits coalesced frames once, in receive order, without merging their bodies. | Not selected. |
| SC005 | The assembler enforces the configured receive budget and never retains an incomplete oversized frame. | Not selected. |
| SC006 | Connection byte accounting releases budget only when complete frames are consumed or outbound sends settle. | Not selected. |
| SC007 | The bounded send queue has one writer, preserves packet order, and reports queue saturation without silent loss. | Not selected. |
| SC008 | Send timeout and transport failure produce the correct delivery-certainty result and do not trigger blind duplicate sends. | Not selected. |
| SC009 | Cancellation and connection disposal settle queued sends and release receive/send resources exactly once. | Not selected. |
| SC010 | The listener enforces its concurrent-connection limit and observes cancellation while accepting. | Not selected. |
| SC011 | Listener bind/accept failure closes owned sockets and reports a stable transport result. | Not selected. |
| SC012 | The wire profile decodes only registered client-to-server packet directions and rejects wrong-direction reads. | Not selected. |
| SC013 | The wire profile encodes a packet only through its registered server-to-client binding and preserves its message ID. | Not selected. |
| SC014 | Generated codecs cannot be constructed before complete Version4 protocol facts are initialized. | Not selected. |
| SC015 | Protocol-facts construction rejects invalid array shapes, missing directories, and malformed fingerprints. | Not selected. |
| SC016 | Protocol facts publish once and reject a conflicting second initialization without replacing the first snapshot. | Not selected. |
| SC017 | Packet 82 decoding preserves module ID boundaries and requires complete consumption of the bounded module body. | Not selected. |
| SC018 | Every Version4 module ID 0–14 has one direction-correct codec and unknown IDs are rejected. | Not selected. |
| SC019 | Wire compiler output is deterministic, fingerprinted, and confined to the configured Build/generated directory. | Not selected. |
| SC020 | A connection retains one epoch and monotonic packet sequence across ordered inbound frames and outbound receipts. | Not selected. |

## Verification and limits

Both builds used the repository SDK selected by `global.json` (`10.0.400`):

| Command | Exit | Warnings | Errors | Output |
| --- | ---: | ---: | ---: | --- |
| `dotnet build src/MinimalImplementation/Protocol/MinimalImplementation.Protocol.csproj --no-restore` | 0 | 0 | 0 | `Build/bin/MinimalImplementation.Protocol/Debug/net10.0/MinimalImplementation.Protocol.dll` |
| `dotnet build src/MinimalImplementation/MinimalImplementation.Server.csproj --no-restore` | 0 | 0 | 0 | `Build/bin/MinimalImplementation.Server/Debug/net10.0/MinimalImplementation.Server.dll` |

The Protocol build generated 166 packet codec source files under
`Build/generated/MinimalImplementation.Protocol/Debug/net10.0/`. Generated packet 85 sources
preserve the request and response field order described above. The builds did not run functional
scenarios, SC001, SC002, or a listener. They do not establish runtime framing, direction binding,
session admission, handler coverage, or complete server readiness.

The WireCompiler cleanup was checked by placing a temporary stale `.g.cs` file in that dedicated
output directory, rebuilding Protocol, and confirming the stale artifact was removed before
compilation. The build exited 0 with 0 warnings and 0 errors. This only verifies generated-output
hygiene; it is not a server packet scenario.

Outstanding root work is to provide real protocol facts, compose the listener and packet connection,
register packet handlers and outbound projections, freeze the 10-percent core scenario set, and run
only that selected set. `Version4PacketConnection` is ready for the listener's connection callback,
but the root composition does not yet construct it or start the listener. This handoff covers
transport and wire-codec infrastructure; it does not claim that all Version4 packets have
authoritative server interactions.
