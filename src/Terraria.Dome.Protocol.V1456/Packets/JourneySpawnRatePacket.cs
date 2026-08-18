namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct JourneySpawnRatePacket(
  byte PlayerSlot,
  float SliderValue);
