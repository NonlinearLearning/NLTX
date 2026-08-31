# V1456 GemLockToggle Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2853-2860`
  and `NetMessage.cs:1377-1381`; message `105` reads/writes `Int16 tileX`, `Int16 tileY`, and
  `Boolean locked` (5-byte payload).
- Current owner: `GemLockTogglePacket` and `TerrariaPacketCodec` own the exact request/relay
  grammar; catalog direction is `Bidirectional/Handled`. Gem-lock mutation remains server-owned.
- State transition: typed DTO -> fixed 5-byte payload -> framed request/relay; decoder rejects wrong
  IDs, non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed tile coordinates,
  boolean state, catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 105 grammar and projection framing only; world mutation authority and complete
162-message parity remain open.
