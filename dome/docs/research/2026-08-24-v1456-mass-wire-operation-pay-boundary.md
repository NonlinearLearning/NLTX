# V1456 MassWireOperationPay Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2883-2890`
  and `NetMessage.cs:1406-1410`; message `110` reads/writes `Int16 itemType`, `Int16 count`, and
  `Byte playerId` (5-byte payload).
- Current owner: `MassWireOperationPayPacket` and `TerrariaPacketCodec` own the exact client request
  grammar; catalog direction is `ClientToServer/Handled`. Item consumption and authenticated player
  authority remain server-owned.
- State transition: typed DTO -> fixed 5-byte payload -> framed request; decoder rejects wrong IDs,
  non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed item/count,
  player id, catalog state, and truncation rejection; the unsupported wiring set now contains only
  message `69`.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 110 grammar and request framing only; item mutation and complete 162-message
parity remain open.
