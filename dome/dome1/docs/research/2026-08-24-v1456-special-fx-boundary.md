# V1456 SpecialFX Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1417-1425`;
  message `112` writes `Byte effectType`, `Int32 number2`, `Int32 number3`, `Byte number4`,
  `Int16 number5`, and `Byte number6` (13-byte payload). The raw field names are retained because
  the source dispatch interprets them by effect subtype.
- Current owner: `SpecialFxPacket` and `TerrariaPacketCodec` own the exact server visual projection;
  catalog direction is `ServerToClient/Handled`. Effect interpretation remains client-owned.
- State transition: typed raw DTO -> fixed 13-byte payload -> framed projection; decoder rejects wrong
  IDs, non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks all raw fields, catalog
  state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 112 framing only; visual subtype semantics and complete 162-message parity remain
open.
