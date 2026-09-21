namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct InvasionProgressReportPacket(
  int InvasionType,
  int InvasionSize,
  sbyte Progress,
  sbyte Wave);
