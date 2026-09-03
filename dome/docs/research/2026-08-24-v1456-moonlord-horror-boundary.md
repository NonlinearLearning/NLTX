# V1456 MoonlordHorror Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1370-1373`;
  message `103` writes `Int32 maximumCountdown` and `Int32 countdown` (8-byte payload). The
  corresponding `MessageBuffer` case is a no-op, so this is a server-to-client result projection.
- Current owner: `MoonlordHorrorPacket` and `TerrariaPacketCodec` own the exact projection grammar;
  catalog direction is `ServerToClient/Handled`. Boss state and countdown authority remain in the
  server Simulation.
- State transition: typed DTO -> fixed 8-byte payload -> framed projection; decoder rejects wrong
  IDs, non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed countdown fields,
  catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 103 grammar and projection framing only; Boss/full AI and complete 162-message
parity remain open.
