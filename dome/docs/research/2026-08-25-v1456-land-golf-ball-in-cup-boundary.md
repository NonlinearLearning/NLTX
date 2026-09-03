# V1456 message 128 LandGolfBallInCup boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` case 128
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 128
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `LandGolfBallInCupPacket`

The source writes `Byte playerId` followed by four `UInt16` values. The reader
consumes those same five fields and forwards the event.

## Owned contract

- DTO: `Packets/LandGolfBallInCupPacket.cs`
- Message id: `TerrariaMessageId.LandGolfBallInCup = 128`
- Direction: `Bidirectional`
- Support: `Handled`
- Payload: exactly 9 bytes, little-endian unsigned coordinates/values

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  exact round-trip and catalog metadata.
- `Build/diagnostics/protocol/land-golf-ball-in-cup/20260825-build.log` records
  the serial protocol build.
- `Build/diagnostics/protocol/land-golf-ball-in-cup/20260825-focused.log` records
  the focused verifier.

Golf gameplay authority and complete 162-message parity remain open.
