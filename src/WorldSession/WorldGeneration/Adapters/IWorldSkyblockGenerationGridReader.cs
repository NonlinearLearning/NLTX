namespace Terraria.WorldGeneration.Adapters;

public interface IWorldSkyblockGenerationGridReader
{
  WorldSkyblockGenerationTileObservation Read(int x, int y);
}
