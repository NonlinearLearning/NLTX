namespace Terraria.WorldGeneration.Terrain;

public readonly record struct WorldHardmodeTilePolicyState(
  bool IsHardmodeTileUpdateEnabled)
{
  public static WorldHardmodeTilePolicyState Derive(
    bool hardMode,
    bool remixWorld,
    bool goodWorld,
    bool tenthAnniversaryWorld)
  {
    return new(
      hardMode || (remixWorld && goodWorld && !tenthAnniversaryWorld));
  }
}
