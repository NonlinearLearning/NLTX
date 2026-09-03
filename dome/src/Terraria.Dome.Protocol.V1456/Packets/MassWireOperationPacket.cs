namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct MassWireOperationPacket(
  short StartX,
  short StartY,
  short EndX,
  short EndY,
  byte ToolMode);
