namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct BestiaryModulePacket(
  BestiaryUnlockType UnlockType,
  short NpcNetId,
  int? KillCount);
