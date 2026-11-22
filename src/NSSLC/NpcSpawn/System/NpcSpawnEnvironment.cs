namespace Terraria.Npc;

/// <summary>World flags the Version4 spawner reads while choosing a rate and a population cap.</summary>
/// <remarks>
/// Version4 reads these from <c>Main</c> and from the spawning player's zone flags. The port keeps
/// the facts the server already has: world mode, the three layer boundaries and the day/night,
/// blood-moon and eclipse flags. Biome zones (Jungle, Dungeon, Corruption, ...) are not modelled
/// yet, so the spawn table has no biome predicates.
/// </remarks>
public readonly record struct NpcSpawnEnvironment(
    bool HardMode,
    int WorldSurfaceTileY,
    int RockLayerTileY,
    int UnderworldTileY,
    bool DayTime,
    bool BloodMoon,
    bool Eclipse)
{
  public bool IsValid => WorldSurfaceTileY >= 0 &&
      RockLayerTileY >= WorldSurfaceTileY &&
      UnderworldTileY >= RockLayerTileY;
}
