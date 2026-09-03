# V1456 BugReleasing Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2509-2517`,
  legacy message `71` reads `Int32 tileX`, `Int32 tileY`, `Int16 npcType`, and `Byte style`.
- Current owner: `BugReleasingPacket` and `TerrariaPacketCodec` own the 11-byte grammar only;
  NPC release authority remains outside the current Simulation.
- State transition: typed DTO -> fixed 11-byte payload -> frame; decode rejects wrong ID or exact
  length mismatch.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed coordinates/type,
  byte style, round-trip equality, `ClientToServer/Framed` catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 71 grammar only; NPC release behavior and 162-message parity remain open.
