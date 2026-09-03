namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct GemLockTogglePacket(short TileX, short TileY, bool Locked);
