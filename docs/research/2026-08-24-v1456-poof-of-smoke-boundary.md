# V1456 PoofOfSmoke Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1387-1389`;
  message `106` writes `HalfVector2(number, number2).PackedValue`, a raw 32-bit packed payload.
  `MessageBuffer` has a no-op case, so the message is a server-to-client visual projection.
- Current owner: `PoofOfSmokePacket` and `TerrariaPacketCodec` preserve the raw `UInt32` packed
  value; catalog direction is `ServerToClient/Handled`. No unverified half-float conversion is
  introduced at the protocol boundary.
- State transition: typed raw DTO -> fixed 4-byte payload -> framed projection; decoder rejects wrong
  IDs, non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks raw packed value,
  catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 106 framing only; client visual interpretation and complete 162-message parity
remain open.
