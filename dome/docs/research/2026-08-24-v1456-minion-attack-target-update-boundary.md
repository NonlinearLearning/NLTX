# V1456 message 115 MinionAttackTargetUpdate boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 115
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 115
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`

The legacy dispatch reads a player slot as a byte and the selected NPC target as
an Int16. No length prefix, branch, or trailing field is present in this case.

## Owned contract

- DTO: `Packets/MinionAttackTargetUpdatePacket.cs`
- Message id: `TerrariaMessageId.MinionAttackTargetUpdate = 115`
- Direction: `ClientToServer`
- Support: `Handled`
- Payload: `Byte playerSlot` followed by `Int16 targetNpcId`, exactly 3 bytes
- Integer encoding: little-endian, matching the V1456 frame codec

`TerrariaPacketCodec.Encode` emits the fixed payload and
`DecodeMinionAttackTargetUpdate` rejects another message id, a truncated frame,
or any payload whose length is not exactly three bytes.

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` performs
  DTO round-trip and catalog direction/support assertions.
- The same verifier passes the truncated-frame rejection check.
- `Build/diagnostics/protocol/minion-attack-target-update/20260824-build.log`
  records the serial protocol-project build.
- `Build/diagnostics/protocol/minion-attack-target-update/20260824-focused.log`
  records the focused compatibility verifier.

This closes the wire grammar and ownership metadata only. Minion target
authority, minion AI behavior, and complete 162-message parity remain open.
