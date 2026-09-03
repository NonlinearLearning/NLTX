# V1456 ShopOverride Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1378-1386`;
  message `104` writes `Byte slot`, `Int16 type`, `Int16 stack`, `Byte prefix`, `Int32 value`,
  and `Byte flags` (11-byte payload). The source packet model names these fields as
  `SyncNpcShopItem`.
- Current owner: `ShopOverridePacket` and `TerrariaPacketCodec` own the exact server projection;
  catalog direction is `ServerToClient/Handled`. Merchant inventory/price authority remains server-owned.
- State transition: typed DTO -> fixed 11-byte payload -> framed projection; decoder rejects wrong
  IDs, non-exact lengths, and truncation.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks all signed/unsigned
  fields, raw flags, catalog state, and truncation rejection.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes message 104 grammar and projection framing only; merchant authority and complete
162-message parity remain open.
