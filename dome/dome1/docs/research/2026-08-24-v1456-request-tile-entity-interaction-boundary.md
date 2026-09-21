# V1456 message 122 RequestTileEntityInteraction boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 122
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 122
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `RequestTileEntityInteractionPacket`

The source writes an `Int32 entityId` followed by a `Byte playerId`; the reader
consumes exactly those fields and handles `-1` as anchor clear.

## Owned contract

- DTO: `Packets/RequestTileEntityInteractionPacket.cs`
- Message id: `TerrariaMessageId.RequestTileEntityInteraction = 122`
- Direction: `Bidirectional`
- Support: `Handled`
- Payload: `Int32 entityId + Byte playerId`, exactly 5 bytes, little-endian integer

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  round-trip with the source's `-1` clear sentinel and catalog metadata.
- `Build/diagnostics/protocol/request-tile-entity-interaction/20260824-build.log`
  records the serial protocol build.
- `Build/diagnostics/protocol/request-tile-entity-interaction/20260824-focused.log`
  records the focused verifier.

Tile-entity authority and complete 162-message parity remain open.
