namespace Terraria.WorldGeneration.Adapters;

public interface ISkyblockGenerationEffectsPort
{
  void ClearDungeonCoordinates();

  void NotifyLowTilesChanged(bool lowTiles);
}
