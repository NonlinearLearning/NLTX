# V1456 ChestName Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/NetMessage.cs:1105-1110`,
  legacy message `69` writes `Int16 chestId`, `Int16 tileX`, `Int16 tileY`, then a BinaryWriter
  UTF-8/.NET string; the client branch reads the same coordinates and name context.
- Current owner: `ChestNamePacket` and `TerrariaPacketCodec` own the source-shaped grammar only.
  Catalog remains `ServerToClient/Unsupported` because current Simulation has no chest-name authority.
- State transition: typed DTO -> fixed coordinate prefix + 7-bit string -> frame; decode rejects
  truncation and trailing bytes before exposing the name.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks `chestId=-1`, signed
  coordinates, Unicode name round-trip, catalog isolation, and truncation rejection.
- Diagnostic: compatibility verifier exit `0`; protocol build exit `0`, zero warnings/errors.

This closes message 69 grammar without upgrading it to semantic support or 162-message parity.
