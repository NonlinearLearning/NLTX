# V1456 MassWireOperation Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2869-2881`
  and `NetMessage.cs:1399-1405`; message `109` reads/writes `Int16 startX`, `Int16 startY`,
  `Int16 endX`, `Int16 endY`, and `Byte toolMode` (9-byte payload).
- Current owner: `MassWireOperationPacket` and `TerrariaPacketCodec` own the exact client request
  grammar; catalog direction is `ClientToServer/Handled`. Wiring mutation and authenticated player
  selection remain server-owned.
- State transition: typed DTO -> fixed 9-byte payload -> framed request; decoder rejects wrong IDs,
  non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed coordinates,
  tool mode, catalog state, and truncation rejection; the unsupported wiring set now contains only
  messages `69` and `110`.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 109 grammar and request framing only; wiring execution and complete 162-message
parity remain open.
