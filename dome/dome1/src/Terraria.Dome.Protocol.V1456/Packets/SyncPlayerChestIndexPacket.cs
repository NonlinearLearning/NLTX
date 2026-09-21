namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct SyncPlayerChestIndexPacket(byte PlayerSlot, short ChestIndex);
