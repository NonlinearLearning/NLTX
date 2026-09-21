namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerHealOtherPacket(byte PlayerId, short Amount);
