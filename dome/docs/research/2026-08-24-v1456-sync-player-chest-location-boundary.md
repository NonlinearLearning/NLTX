# V1456 message 125 SyncPlayerChestLocation boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 125
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 125
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `SyncPlayerChestLocationPacket`

The source writes `Byte playerId`, `Int16 x`, `Int16 y`, and `Byte type`; the
reader consumes those same four fields and forwards the update.

## Owned contract

- DTO: `Packets/SyncPlayerChestLocationPacket.cs`
- Message id: `TerrariaMessageId.SyncPlayerChestLocation = 125`
- Direction: `Bidirectional`
- Support: `Handled`
- Payload: exactly 6 bytes, little-endian signed coordinates

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  exact round-trip and catalog metadata.
- `Build/diagnostics/protocol/sync-player-chest-location/20260824-build.log`
  records the serial protocol build.
- `Build/diagnostics/protocol/sync-player-chest-location/20260824-focused.log`
  records the focused verifier.

Chest-location authority and complete 162-message parity remain open.
