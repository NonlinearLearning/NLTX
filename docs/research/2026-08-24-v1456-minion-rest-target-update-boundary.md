# V1456 MinionRestTargetUpdate Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2765-2771`
  and `NetMessage.cs:1334-1338`; message `99` reads/writes `Byte playerSlot` and two `Single`
  target-position values (9-byte payload).
- Current owner: `MinionRestTargetUpdatePacket` and `TerrariaPacketCodec` own the exact grammar;
  catalog direction is `Bidirectional/Handled`. Authenticated player selection and minion behavior
  remain server-owned.
- State transition: typed DTO -> fixed 9-byte payload -> framed relay; decoder rejects wrong IDs,
  non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks player slot, float
  coordinates, catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 99 grammar and projection framing only; minion behavior and complete 162-message
parity remain open.
