using Terraria.Projectile;

namespace NSSLC.Infrastructure.Network;

/// <summary>The Steam 326 projectile key and its decoded state; the key retains generation.</summary>
public sealed record SteamProjectileSyncPacket(uint Key, ProjectileNetworkApplyCommand State);
