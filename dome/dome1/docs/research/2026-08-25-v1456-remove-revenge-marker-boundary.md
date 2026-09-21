# V1456 message 127 RemoveRevengeMarker boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 127
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 127
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `RemoveRevengeMarkerPacket`

The writer emits one `Int32 uniqueId`; the reader consumes that ID while the
server-side case remains a projection boundary.

## Owned contract

- DTO: `Packets/RemoveRevengeMarkerPacket.cs`
- Message id: `TerrariaMessageId.RemoveRevengeMarker = 127`
- Direction: `ServerToClient`
- Support: `Handled`
- Payload: `Int32 uniqueId`, exactly 4 bytes, little-endian

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  signed ID round-trip and catalog metadata.
- `Build/diagnostics/protocol/remove-revenge-marker/20260825-build.log` records
  the serial protocol build.
- `Build/diagnostics/protocol/remove-revenge-marker/20260825-focused.log` records
  the focused verifier.

Revenge-marker authority and complete 162-message parity remain open.
