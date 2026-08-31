namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct DisplayDollDataSyncPacket(
  byte PlayerId,
  int EntityId,
  byte Slot,
  byte Param,
  byte[] DataPayload);
