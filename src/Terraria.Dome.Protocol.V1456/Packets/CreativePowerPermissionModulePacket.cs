namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct CreativePowerPermissionModulePacket(
  ushort PowerId,
  byte PermissionLevel);
