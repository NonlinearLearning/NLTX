namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ChestTransferIntent(
  byte PlayerSlot,
  int ChestId,
  byte InventorySlot,
  byte ChestSlot,
  bool Withdraw);
