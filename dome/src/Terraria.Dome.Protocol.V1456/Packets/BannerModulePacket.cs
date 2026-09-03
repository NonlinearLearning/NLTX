namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct BannerModulePacket(
  BannerModuleMessageType MessageType,
  short? BannerId,
  int? KillCount,
  ushort? ClaimCount,
  bool? Granted,
  int[]? KillCounts,
  ushort[]? ClaimableCounts);
