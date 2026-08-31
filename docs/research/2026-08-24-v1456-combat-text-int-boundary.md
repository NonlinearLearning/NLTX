# V1456 CombatTextInt Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1176-1183`,
  legacy message `81` writes two `Single` positions, RGB bytes, and an `Int32` value.
- Current owner: `CombatTextIntPacket` and `TerrariaPacketCodec`; compatibility projection only,
  with no client authority over combat state.
- State transition: typed DTO -> fixed 15-byte payload -> frame; decode rejects wrong ID or any
  non-exact payload length.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks finite float values,
  RGB bytes, signed integer, round-trip equality, catalog support, and truncated-frame rejection.
- Diagnostic: compatibility verifier exit `0`; protocol build exit `0`, zero warnings/errors.

This closes message 81 grammar only; it does not claim combat-text presentation or 162-message parity.
