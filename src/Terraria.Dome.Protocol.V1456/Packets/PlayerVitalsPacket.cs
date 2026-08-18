namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerVitalsPacket(
  byte PlayerSlot,
  int Current,
  int Maximum);
