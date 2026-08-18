namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct UnbreakableWallScanModulePacket(
  byte PlayerSlot,
  bool IsInsideUnbreakableWalls);
