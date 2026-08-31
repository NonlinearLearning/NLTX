# V1456 RequestTeleportationByServer Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2447-2465`,
  legacy message `73` reads one selector byte and dispatches values `0..4` to teleport actions.
- Current owner: `RequestTeleportationByServerPacket` and `TerrariaPacketCodec` own selector
  framing and validation; teleport execution remains outside the current Simulation authority.
- State transition: selector DTO -> one-byte payload -> frame; encode/decode reject selectors above
  `4` and any wrong payload length.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks selector `4` round-trip,
  invalid selector rejection, and `ClientToServer/Framed` catalog state.
- Diagnostic: compatibility verifier exit `0`; protocol build exit `0`, zero warnings/errors.

This closes message 73 selector grammar only; teleport behavior and 162-message parity remain open.
