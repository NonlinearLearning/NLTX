# V1456 CrystalInvasionWipe Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2920-2922`
  is a no-op receive case; the mature packet model defines message `114` as an empty
  `CrystalInvasionWipeAllTheThingsss` server projection.
- Current owner: `CrystalInvasionWipePacket` and `TerrariaPacketCodec` own the empty framing;
  catalog direction is `ServerToClient/Handled`. Invasion entity cleanup remains authoritative
  Simulation behavior.
- State transition: typed empty DTO -> empty payload -> framed projection; decoder rejects wrong IDs
  and any non-empty payload.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks empty round-trip,
  catalog state, and non-empty payload rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 114 empty framing only; invasion cleanup execution and complete 162-message parity
remain open.
