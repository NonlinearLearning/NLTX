namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct RequestTileEntityInteractionPacket(int EntityId, byte PlayerId);
