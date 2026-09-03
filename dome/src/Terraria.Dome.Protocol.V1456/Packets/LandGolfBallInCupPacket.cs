namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct LandGolfBallInCupPacket(
  byte PlayerId,
  ushort X,
  ushort Y,
  ushort Value1,
  ushort Value2);
