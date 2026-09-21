# V1456 TeleportEntity Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:2303-2365`
  and `NetMessage.cs:1090-1110`; message `65` reads `BitsByte flags`, `Int16 targetId`,
  two `Single` coordinates, `Byte style`, and an optional `Int32 extraInfo` when flag bit 3 is set.
- Current owner: `TeleportEntityPacket` and `TerrariaPacketCodec` own the source-shaped grammar;
  catalog direction is `ServerToClient/Handled`. Position mutation remains server-owned.
- State transition: typed DTO -> fixed 12-byte or 16-byte payload -> framed projection; decoder
  rejects inconsistent optional-field boundaries and wrong/truncated frames.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks catalog direction,
  round-trip with optional field, round-trip without optional field, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 65 grammar and projection framing only; server teleport execution and complete
162-message parity remain open.
