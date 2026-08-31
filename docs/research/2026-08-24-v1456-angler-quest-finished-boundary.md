# V1456 AnglerQuestFinished Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2467-2473`,
  legacy message `75` consumes no payload and applies the completion side effect for the session.
- Current owner: `AnglerQuestFinishedPacket` and `TerrariaPacketCodec` own the empty-frame grammar;
  quest progression remains outside the current Simulation authority.
- State transition: typed empty DTO -> empty payload frame; decode rejects wrong ID or any payload.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks empty round-trip,
  `ClientToServer/Framed` catalog state, and non-empty payload rejection.
- Diagnostic: compatibility verifier exit `0`; protocol build exit `0`, one pre-existing
  `WorldInvasionTransition.cs` duplicate-using warning and zero errors.

This closes message 75 framing only; it does not claim quest side-effect parity or 162-message parity.
