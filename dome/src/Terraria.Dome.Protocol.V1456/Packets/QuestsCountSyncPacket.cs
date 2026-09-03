namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct QuestsCountSyncPacket(
  byte PlayerSlot,
  int AnglerQuestsFinished,
  int GolferScoreAccumulated);
