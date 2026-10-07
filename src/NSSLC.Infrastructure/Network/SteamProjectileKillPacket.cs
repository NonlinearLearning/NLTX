using System.Numerics;

namespace NSSLC.Infrastructure.Network;

public sealed record SteamProjectileKillPacket(uint Key, Vector2 Position);
