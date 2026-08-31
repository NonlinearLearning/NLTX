namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerDeathV2Packet(
  byte PlayerId,
  byte[] DeathReasonPayload,
  short Damage,
  byte Direction,
  byte Pvp);
