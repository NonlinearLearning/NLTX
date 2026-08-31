# V1456 AnglerQuest Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1150-1154`,
  legacy message `74` writes `Byte anglerQuest` and `Boolean finishedToday`.
- Current owner: `AnglerQuestPacket` and `TerrariaPacketCodec`; compatibility projection only,
  with no client authority over quest progression.
- State transition: typed DTO -> fixed 2-byte payload -> frame; decode rejects wrong ID, wrong
  length, and non-boolean completion bytes.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks both default join
  projection and typed `questId/finishedToday` round-trip, catalog support, and truncation rejection.
- Diagnostic: compatibility verifier exit `0`; protocol build exit `0`, zero warnings/errors.

This closes message 74 grammar only; quest progression semantics and 162-message parity remain open.
