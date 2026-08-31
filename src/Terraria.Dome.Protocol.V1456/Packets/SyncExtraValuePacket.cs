namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct SyncExtraValuePacket(
  short NpcId,
  int ExtraValue,
  float PositionX,
  float PositionY);
