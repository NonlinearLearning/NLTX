# V1456 BugCatching Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2475-2484`,
  legacy message `70` reads `Int16 npcId` and a reported `Byte playerSlot`, then replaces the
  reported owner with the authenticated session slot before capture.
- Current owner: `BugCatchingPacket` and `TerrariaPacketCodec` own the 3-byte grammar. The packet
  preserves the reported slot for validation/audit; it does not grant client ownership.
- State transition: typed DTO -> fixed 3-byte payload -> frame; decode rejects wrong ID or length.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed NPC ID,
  byte slot, round-trip equality, `ClientToServer/Framed` catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 70 grammar only; capture authority and 162-message parity remain open.
