# V1456 message 117 PlayerHurtV2 boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 117
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 117
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `PlayerHurtV2Packet`

The source writes `playerId`, a variable `PlayerDeathReason`, then `Int16`
damage, `Byte` direction, `Byte` flags, and `SByte` cooldown counter. The
reader consumes the same fixed trailer after `PlayerDeathReason.FromReader`.

## Owned contract

- DTO: `Packets/PlayerHurtV2Packet.cs`
- Message id: `TerrariaMessageId.PlayerHurtV2 = 117`
- Direction: `Bidirectional`
- Support: `Handled`
- Payload: `Byte playerId`, opaque variable `PlayerDeathReason` bytes, then
  `Int16 damage`, `Byte direction`, `Byte flags`, `SByte cooldownCounter`
- The codec preserves the reason bytes without claiming their internal semantic
  model; it requires at least one reason byte and exactly five trailer bytes.

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` performs
  round-trip preservation of a variable reason byte sequence and all fixed
  trailer fields, and checks catalog support.
- `Build/diagnostics/protocol/player-hurt-v2/20260824-build.log` records the
  serial protocol-project build.
- `Build/diagnostics/protocol/player-hurt-v2/20260824-focused.log` records the
  focused compatibility verifier.

This closes the source-shaped message boundary only. `PlayerDeathReason`
internal grammar parity, player hurt authority, and complete 162-message parity
remain open.
