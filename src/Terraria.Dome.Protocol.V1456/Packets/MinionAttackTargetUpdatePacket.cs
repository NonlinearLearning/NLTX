namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct MinionAttackTargetUpdatePacket(byte PlayerSlot, short TargetId);
