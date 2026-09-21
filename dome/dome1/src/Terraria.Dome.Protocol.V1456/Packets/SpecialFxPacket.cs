namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct SpecialFxPacket(
  byte EffectType,
  int Number2,
  int Number3,
  byte Number4,
  short Number5,
  byte Number6);
