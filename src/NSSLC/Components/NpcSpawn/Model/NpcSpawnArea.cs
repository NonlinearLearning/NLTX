using Terraria.World;

namespace Terraria.Npc;

/// <summary>Version4 <c>Spawner.GetSpawnArea</c> outputs for one player.</summary>
/// <remarks>
/// <see cref="Spawn"/> is where the spawner may look for ground, <see cref="Safe"/> is the part it
/// refuses to use, and the two half extents are the ones
/// <c>Spawner.CheckNotSpawningOnScreen</c> later adds around every player's centre. The reference
/// reuses the last computed half extents as pixels there, so they travel with the area instead of
/// being recomputed.
/// </remarks>
public readonly record struct NpcSpawnArea(
    WorldTileArea Spawn,
    WorldTileArea Safe,
    int SafeHalfExtentX,
    int SafeHalfExtentY)
{
  public bool IsValid => Spawn.IsValid && Safe.IsValid &&
      SafeHalfExtentX >= 0 && SafeHalfExtentY >= 0;
}
