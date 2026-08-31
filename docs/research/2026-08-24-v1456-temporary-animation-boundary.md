# V1456 TemporaryAnimation Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1164-1169`,
  legacy message `77` writes `Int16 animationType`, `UInt16 tileType`, `Int16 tileX`, and
  `Int16 tileY`; `MessageBuffer.cs:2498-2504` reads the same order.
- Current owner: `TemporaryAnimationPacket` and `TerrariaPacketCodec`; compatibility projection
  only, with animation execution intentionally outside Simulation authority.
- State transition: typed DTO -> fixed 8-byte payload -> frame; decode rejects wrong ID or exact
  length mismatch.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed coordinates,
  unsigned tile type, round-trip equality, catalog direction, and truncated-frame rejection.
- Diagnostic: compatibility verifier exit `0`; protocol build exit `0`, zero warnings/errors.

This closes message 77 grammar only; it does not claim client animation behavior or 162-message parity.
