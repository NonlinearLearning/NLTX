# V1456 TeleportPlayerThroughPortal Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2746-2758`
  and `NetMessage.cs:1320-1332`; message `96` reads/writes `Byte playerSlot`, `Int16 portalColor`,
  two position `Single` values, and two velocity `Single` values (19-byte payload).
- Current owner: `TeleportPlayerThroughPortalPacket` and `TerrariaPacketCodec` own the exact wire
  grammar; catalog direction is `Bidirectional/Handled`. Authenticated player selection and actual
  teleportation remain server-owned.
- State transition: typed DTO -> fixed 19-byte payload -> framed relay; decoder rejects wrong IDs,
  non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed portal color,
  four float fields, catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 96 grammar and projection framing only; portal execution and complete 162-message
parity remain open.
