# V1456 message 120 Emoji boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 120
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 120
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `EmojiPacket`

The source writes a player byte followed by an emote byte. The reader consumes
the same two fields and applies the emote to the sender's player state.

## Owned contract

- DTO: `Packets/EmojiPacket.cs`
- Message id: `TerrariaMessageId.Emoji = 120`
- Direction: `Bidirectional`
- Support: `Handled`
- Payload: `Byte playerId`, `Byte emoteId`, exactly 2 bytes

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  round-trip, exact payload length through the frame codec, and catalog metadata.
- `Build/diagnostics/protocol/emoji/20260824-build.log` records the serial build.
- `Build/diagnostics/protocol/emoji/20260824-focused.log` records the focused verifier.

This closes only the emoji wire contract; emote gameplay authority and complete
162-message parity remain open.
