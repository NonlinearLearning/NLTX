# V1456 QuestsCountSync Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1158-1162`,
  legacy message `76` writes `Byte playerSlot`, `Int32 anglerQuestsFinished`, and
  `Int32 golferScoreAccumulated`.
- Current owner: `QuestsCountSyncPacket` and `TerrariaPacketCodec`; compatibility projection only,
  with no client authority over quest or score state.
- State transition: typed DTO -> fixed 9-byte payload -> frame; decode rejects wrong ID or exact
  length mismatch.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed counters,
  round-trip equality, catalog support, and truncated-frame rejection.
- Diagnostic: compatibility verifier exit `0`; protocol build exit `0`, zero warnings/errors.

This closes message 76 grammar only; quest progression semantics and 162-message parity remain open.
