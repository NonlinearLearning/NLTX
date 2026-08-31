# V1456 WiredCannonShot Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1390-1398`;
  message `108` writes `Int16 damage`, `Single knockback`, four `Int16` fields (`x`, `y`, `angle`,
  `ammo`), and `Byte playerId` (15-byte payload). The mature packet model names this
  `WiredCannonShot`.
- Current owner: `WiredCannonShotPacket` and `TerrariaPacketCodec` own the exact server projection;
  catalog direction is `ServerToClient/Handled`. Cannon execution and damage authority remain server-owned.
- State transition: typed DTO -> fixed 15-byte payload -> framed projection; decoder rejects wrong
  IDs, non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks all signed fields,
  float knockback, player id, catalog state, and truncation rejection; the legacy unsupported set now
  contains only messages `69`, `109`, and `110`.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 108 grammar and projection framing only; cannon simulation and complete
162-message parity remain open.
