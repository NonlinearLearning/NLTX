namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct CombatTextIntPacket(
  float PositionX,
  float PositionY,
  TerrariaColor Color,
  int Text);
