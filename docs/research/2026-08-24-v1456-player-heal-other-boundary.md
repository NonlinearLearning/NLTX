# V1456 PlayerHealOther Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2377-2390`
  and `NetMessage.cs:1104-1108`; message `66` reads/writes `Byte playerId` followed by
  `Int16 amount`.
- Current owner: `PlayerHealOtherPacket` and `TerrariaPacketCodec` own the exact 3-byte
  projection grammar. The packet is server-to-client and does not grant client healing authority.
- State transition: typed DTO -> fixed 3-byte payload -> framed projection; decoder rejects wrong
  message IDs and any non-exact payload length.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed amount,
  round-trip equality, `ServerToClient/Handled` catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 66 grammar and projection framing only; full healing simulation and complete
162-message parity remain open.
