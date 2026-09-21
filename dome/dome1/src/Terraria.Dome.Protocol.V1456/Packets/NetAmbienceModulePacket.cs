namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct NetAmbienceModulePacket(
  byte PlayerSlot,
  int Seed,
  byte SkyEntityType);
