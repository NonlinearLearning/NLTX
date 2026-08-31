# V1456 message 119 CombatTextString boundary

## Source anchor

- `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs` message 119 catalog slot
- `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs` case 119
- Mature packet reference: `D:\ProjectItem\SourceCode\Net\Networking\NetworkLayer\Packets\NetPackets.cs`, `CombatTextStringPacket`

The mature serializer writes two `Single` coordinates, RGB bytes, and a
variable `NetworkText`. The legacy reader case is a no-op, so this slice owns
the server-to-client wire projection without introducing a client authority.

## Owned contract

- DTO: `Packets/CombatTextStringPacket.cs`
- Message id: `TerrariaMessageId.CombatTextString = 119`
- Direction: `ServerToClient`
- Support: `Handled`
- Payload: `Single x`, `Single y`, RGB bytes, then non-empty opaque
  `NetworkText` payload bytes

The codec preserves the variable text bytes and rejects an empty text payload.

## Acceptance evidence

- `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs` checks
  coordinate/color/text round-trip and catalog direction.
- `Build/diagnostics/protocol/combat-text-string/20260824-build.log` records
  the serial protocol-project build.
- `Build/diagnostics/protocol/combat-text-string/20260824-focused.log` records
  the focused compatibility verifier.

NetworkText internal grammar parity and complete 162-message parity remain open.
