# V1456 NebulaLevelupRequest Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2795-2849`
  and `NetMessage.cs:1349-1354`; message `102` reads/writes `Byte playerSlot`, `UInt16 powerId`,
  and two `Single` target coordinates (11-byte payload).
- Current owner: `NebulaLevelupRequestPacket` and `TerrariaPacketCodec` own the exact request/relay
  grammar; catalog direction is `Bidirectional/Handled`. Authenticated player selection and Nebula
  level-up effects remain server-owned.
- State transition: typed DTO -> fixed 11-byte payload -> framed request/relay; decoder rejects wrong
  IDs, non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks player slot, unsigned
  power id, float coordinates, catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 102 grammar and projection framing only; Nebula gameplay effects and complete
162-message parity remain open.
