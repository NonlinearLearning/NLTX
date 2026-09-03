# V1456 message 126 SyncRevengeMarker boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 126
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 126
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `SyncRevengeMarkerPacket`

The legacy writer delegates to `RevengeMarker.WriteSelfTo`; the mature packet
contract identifies the serialized marker ID as one `Int16`. The legacy reader
case is intentionally empty because this is a server-to-client projection.

## Owned contract

- DTO: `Packets/SyncRevengeMarkerPacket.cs`
- Message id: `TerrariaMessageId.SyncRevengeMarker = 126`
- Direction: `ServerToClient`
- Support: `Handled`
- Payload: `Int16 id`, exactly 2 bytes, little-endian

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  signed ID round-trip and catalog metadata.
- `Build/diagnostics/protocol/sync-revenge-marker/20260825-build.log` records
  the serial protocol build.
- `Build/diagnostics/protocol/sync-revenge-marker/20260825-focused.log` records
  the focused verifier.

Revenge-marker gameplay authority and complete 162-message parity remain open.
