# V1456 SyncPlayerChestIndex Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1173-1175`,
  legacy message `80` writes `Byte playerSlot` and `Int16 chestIndex`.
- Current owner: `SyncPlayerChestIndexPacket` and `TerrariaPacketCodec`; compatibility projection
  only, with no client authority over chest state.
- State transition: typed DTO -> fixed 3-byte payload -> frame; decode rejects wrong message ID or
  truncated/trailing payload before field access.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks signed chest index,
  round-trip equality, `ServerToClient/Handled` catalog state, and truncated-frame rejection.
- Diagnostic: compatibility verifier exit `0`; protocol build exit `0`, zero warnings/errors.

This closes message 80 grammar only; it does not claim complete chest semantics or 162-message parity.
