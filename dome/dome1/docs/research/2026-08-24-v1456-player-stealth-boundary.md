# V1456 PlayerStealth Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2530-2534`
  and `NetMessage.cs:1195-1201`; message `84` reads/writes `Byte playerSlot` followed by
  `Single stealth`.
- Current owner: `PlayerStealthPacket` and `TerrariaPacketCodec` own the exact five-byte grammar;
  the catalog marks it `Bidirectional/Handled`. Authenticated-slot replacement and stealth
  simulation remain session/Simulation responsibilities.
- State transition: typed DTO -> fixed five-byte payload -> framed relay; decoder rejects wrong IDs,
  non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks finite float
  round-trip, catalog direction/support, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 84 grammar and projection framing only; full player stealth authority and
complete 162-message parity remain open.
