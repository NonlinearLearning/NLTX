namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct CombatTextStringPacket(
  float PositionX,
  float PositionY,
  TerrariaColor Color,
  byte[] TextPayload);
