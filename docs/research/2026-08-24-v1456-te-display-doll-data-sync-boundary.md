# V1456 message 121 TEDisplayDollDataSync boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 121
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 121
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `DisplayDollDataSyncPacket`

The legacy prefix is `Byte playerId`, `Int32 entityId`, `Byte slot`, and
`Byte param`. The remaining bytes are delegated to
`TEDisplayDoll.WriteData/ReadData`, whose item grammar is intentionally kept
opaque in this slice.

## Owned contract

- DTO: `Packets/DisplayDollDataSyncPacket.cs`
- Message id: `TerrariaMessageId.TedisplayDollDataSync = 121`
- Direction: `Bidirectional`
- Support: `Handled`
- Payload: 7-byte fixed prefix followed by non-empty variable tile-entity data

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  prefix and variable-tail round-trip plus catalog support.
- `Build/diagnostics/protocol/te-display-doll-data-sync/20260824-build.log`
  records the serial protocol build.
- `Build/diagnostics/protocol/te-display-doll-data-sync/20260824-focused.log`
  records the focused verifier.

TEDisplayDoll item-field grammar and complete 162-message parity remain open.
