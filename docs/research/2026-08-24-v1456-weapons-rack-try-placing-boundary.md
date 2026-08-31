# V1456 message 123 WeaponsRackTryPlacing boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 123
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 123
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `WeaponsRackTryPlacingPacket`

The source-shaped request is `Int16 x`, `Int16 y`, `Int16 itemType`, `Byte
prefix`, and `Int16 stack`, with no optional fields.

## Owned contract

- DTO: `Packets/WeaponsRackTryPlacingPacket.cs`
- Message id: `TerrariaMessageId.WeaponsRackTryPlacing = 123`
- Direction: `ClientToServer`
- Support: `Handled`
- Payload: exactly 9 bytes, little-endian signed integers

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  exact round-trip and catalog metadata.
- `Build/diagnostics/protocol/weapons-rack-try-placing/20260824-build.log`
  records the serial protocol build.
- `Build/diagnostics/protocol/weapons-rack-try-placing/20260824-focused.log`
  records the focused verifier.

Rack placement authority and complete 162-message parity remain open.
