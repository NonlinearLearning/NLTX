# V1456 TravelMerchantItems Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1131-1136`,
  legacy message `72` writes exactly `Main.TravelShopMaxSlots` (40) `Int16` item IDs.
- Current owner: `TravelMerchantItemsPacket` and `TerrariaPacketCodec` own a bounded 40-slot
  source-shaped projection. Catalog remains `ServerToClient/Unsupported` because Simulation has no
  travel-merchant inventory authority.
- State transition: fixed 40-item table -> 80-byte payload -> frame; constructor and decoder reject
  any count/length other than 40 slots.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks first/last signed item
  IDs, unsupported catalog isolation, and exact length rejection. The `AcceptNetModule` contract
  now returns `NetModulePacket`; capability negotiation keeps its ack in `ResponseFrame`.
- Diagnostic: protocol project build exit `0`, zero warnings/errors; focused verifier exit `0`.

This closes message 72 grammar in the protocol project only; it does not claim merchant inventory
authority or 162-message parity.
