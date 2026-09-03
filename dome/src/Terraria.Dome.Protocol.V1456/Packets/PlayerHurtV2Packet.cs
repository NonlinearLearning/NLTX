namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerHurtV2Packet(
  byte PlayerId,
  byte[] DeathReasonPayload,
  short Damage,
  byte Direction,
  byte Flags,
  sbyte CooldownCounter);
