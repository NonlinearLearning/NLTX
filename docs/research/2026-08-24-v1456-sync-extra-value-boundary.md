# V1456 SyncExtraValue Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2698-2708`
  and `NetMessage.cs:1314-1318`; message `92` reads/writes `Int16 npcId`, `Int32 extraValue`,
  `Single positionX`, and `Single positionY`.
- Current owner: `SyncExtraValuePacket` and `TerrariaPacketCodec` own the exact fourteen-byte
  grammar; catalog direction is `Bidirectional/Handled`. NPC extra-value mutation remains an
  authoritative Simulation concern.
- State transition: typed DTO -> fixed 14-byte payload -> framed relay; decoder rejects wrong IDs,
  non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed NPC id,
  signed extra value, float coordinates, catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 92 grammar and projection framing only; complete NPC extra-value semantics and
162-message parity remain open.
