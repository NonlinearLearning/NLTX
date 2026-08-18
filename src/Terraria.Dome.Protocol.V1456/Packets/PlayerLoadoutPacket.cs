namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerLoadoutPacket(
  byte PlayerSlot,
  byte SelectedLoadout,
  ushort AccessoryVisibility);
