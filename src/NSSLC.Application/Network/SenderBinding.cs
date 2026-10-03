namespace Terraria.Network;

public sealed record SenderBinding(byte PlayerSlot, Guid GameSessionKey);
