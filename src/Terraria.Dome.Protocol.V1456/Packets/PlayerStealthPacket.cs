namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerStealthPacket(byte PlayerSlot, float Stealth);
