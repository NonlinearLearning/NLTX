using Terraria.Dome.Protocol.V1456.Compatibility;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ServerTextModulePacket(
  byte AuthorId,
  LegacyNetworkText Text,
  TerrariaColor Color);
