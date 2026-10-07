using Terraria.WorldStorage;

namespace Terraria.Projectile;

/// <summary>
/// Player facts read by projectile PVP qualification for one player slot.
/// The fields map active, dead, immune, hostile, and team state.
/// </summary>
public readonly record struct ProjectilePvpTargetSnapshot(
  PlayerSlot Slot,
  bool Active,
  bool Dead,
  bool Immune,
  bool Hostile,
  int Team);
