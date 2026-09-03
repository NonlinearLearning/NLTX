namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct MassWireOperationPayPacket(
  short ItemType,
  short Count,
  byte PlayerId);
