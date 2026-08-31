namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct NebulaLevelupRequestPacket(
  byte PlayerSlot,
  ushort PowerId,
  float TargetX,
  float TargetY);
