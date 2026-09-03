# V1456 message 124 TEHatRackItemSync boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 124
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 124
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `HatRackItemSyncPacket`

The fixed prefix is `Byte playerId`, `Int32 entityId`, and an encoded slot byte.
The slot uses `slot + 2` for the dye channel. `TEHatRack.WriteItem` then emits
`Int32 itemType` and `Byte prefix`; the complete source-shaped payload is 11
bytes.

## Owned contract

- DTO: `Packets/HatRackItemSyncPacket.cs`
- Message id: `TerrariaMessageId.TehatRackItemSync = 124`
- Direction: `Bidirectional`
- Support: `Handled`
- Payload: `Byte + Int32 + encoded Byte slot + Int32 + Byte`, exactly 11 bytes

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  slot/dye normalization, round-trip, and catalog metadata.
- `Build/diagnostics/protocol/te-hat-rack-item-sync/20260824-build.log` records
  the serial protocol build.
- `Build/diagnostics/protocol/te-hat-rack-item-sync/20260824-focused.log` records
  the focused verifier.

Hat-rack gameplay authority and complete 162-message parity remain open.
