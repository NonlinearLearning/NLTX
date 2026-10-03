namespace Terraria.Network;

public readonly record struct ConnectionIdentity(Guid SessionKey, long Epoch);
