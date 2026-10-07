namespace Terraria.WorldGeneration.Terrain.TreeTops;

public interface IWorldTreeTopsRandomSource
{
  int Next(int exclusiveUpperBound);
}
