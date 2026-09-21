# V1456 TeleportNpcThroughPortal Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2781-2793`
  and `NetMessage.cs:1340-1348`; message `100` reads/writes `UInt16 npcId`, `Int16 portalColor`,
  and four `Single` position/velocity values (20-byte payload).
- Current owner: `TeleportNpcThroughPortalPacket` and `TerrariaPacketCodec` own the exact server
  projection grammar; catalog direction is `ServerToClient/Handled`. NPC teleport execution remains
  server-owned.
- State transition: typed DTO -> fixed 20-byte payload -> framed projection; decoder rejects wrong
  IDs, non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks unsigned NPC id, signed
  portal color, four float fields, catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 100 grammar and projection framing only; portal execution and complete
162-message parity remain open.
