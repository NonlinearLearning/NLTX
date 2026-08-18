namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ChestOpenIntent(byte PlayerSlot, int ChestId, short TileX, short TileY);
