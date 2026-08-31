# V1456 CrystalInvasionStart Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2893-2905`
  and `NetMessage.cs:1428-1431`; message `113` reads/writes `Int16 x` and `Int16 y` (4-byte payload).
- Current owner: `CrystalInvasionStartPacket` and `TerrariaPacketCodec` own the exact client request
  grammar; catalog direction is `ClientToServer/Handled`. Crystal invasion validation and event
  mutation remain server-owned.
- State transition: typed DTO -> fixed 4-byte payload -> framed request; decoder rejects wrong IDs,
  non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed coordinates,
  catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 113 grammar and request framing only; invasion event execution and complete
162-message parity remain open.
