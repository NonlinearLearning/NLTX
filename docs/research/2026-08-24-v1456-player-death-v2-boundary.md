# V1456 message 118 PlayerDeathV2 boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 118
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 118
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `PlayerDeathV2Packet`

The source writes `playerId`, a variable `PlayerDeathReason`, then `Int16`
damage, `Byte` direction, and `Byte` pvp. The reader consumes the same trailer
after `PlayerDeathReason.FromReader`.

## Owned contract

- DTO: `Packets/PlayerDeathV2Packet.cs`
- Message id: `TerrariaMessageId.PlayerDeathV2 = 118`
- Direction: `Bidirectional`
- Support: `Handled`
- Payload: `Byte playerId`, opaque variable `PlayerDeathReason` bytes, then
  `Int16 damage`, `Byte direction`, `Byte pvp`
- The codec preserves reason bytes without claiming their internal semantic
  model; it requires at least one reason byte and exactly three trailer bytes.

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` performs
  round-trip preservation of variable reason bytes and fixed trailer fields and
  checks catalog support.
- `Build/diagnostics/protocol/player-death-v2/20260824-build.log` records the
  serial protocol-project build.
- `Build/diagnostics/protocol/player-death-v2/20260824-focused.log` records the
  focused compatibility verifier.

This closes only the message boundary. `PlayerDeathReason` internal grammar,
death authority, Boss/full AI, and complete 162-message parity remain open.
