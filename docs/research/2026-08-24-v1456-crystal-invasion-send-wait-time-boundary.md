# V1456 message 116 CrystalInvasionSendWaitTime boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 116
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 116
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `CrystalInvasionNextWaveWaitPacket`

The writer emits one `Int32 number` and the reader case has no additional field.
The source-shaped payload is therefore a fixed four-byte wait-time value.

## Owned contract

- DTO: `Packets/CrystalInvasionNextWaveWaitPacket.cs`
- Message id: `TerrariaMessageId.CrystalInvasionSendWaitTime = 116`
- Direction: `ServerToClient`
- Support: `Handled`
- Payload: `Int32 timeLeft`, exactly 4 bytes, little-endian

`TerrariaPacketCodec.Encode` emits the fixed payload and
`DecodeCrystalInvasionNextWaveWait` rejects another message id, truncation, or
any payload whose length is not exactly four bytes.

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` performs
  DTO round-trip and catalog direction/support assertions.
- The verifier also checks truncated-frame rejection.
- `Build/diagnostics/protocol/crystal-invasion-send-wait-time/20260824-build.log`
  records the serial protocol-project build.
- `Build/diagnostics/protocol/crystal-invasion-send-wait-time/20260824-focused.log`
  records the focused compatibility verifier.

This closes only message 116 wire grammar. Crystal invasion wave scheduling,
spawn-table completeness, and full invasion/Boss AI remain open.
