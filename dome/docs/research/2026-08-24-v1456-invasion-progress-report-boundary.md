# V1456 InvasionProgressReport Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1158-1161`,
  legacy message `78` writes `Int32 number`, `Int32 number2`, `SByte number3`, `SByte number4`.
- Current owner: `InvasionProgressReportPacket` and `TerrariaPacketCodec`; this is a
  `CompatibilityOnly` wire grammar slice and does not grant client authority over
  `WorldProgressionState`.
- State transition: source-shaped DTO -> fixed 10-byte payload -> frame; decode rejects wrong
  message ID or payload length before exposing fields.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed fields,
  round-trip equality, catalog `ServerToClient/Handled`, and truncated-frame rejection.
- Diagnostic: protocol compatibility verifier exit `0`; protocol project build exit `0`, zero
  warnings and zero errors.

This card closes message 78 grammar only. It does not claim complete 162-message parity or
Simulation authority for invasion progression.
