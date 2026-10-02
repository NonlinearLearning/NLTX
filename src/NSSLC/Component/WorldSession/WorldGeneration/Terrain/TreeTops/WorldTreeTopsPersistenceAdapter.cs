using System.IO;

namespace Terraria.WorldGeneration.Terrain.TreeTops;

public static class WorldTreeTopsPersistenceAdapter
{
  private const int FirstLoadVersionWithSavedTreeTops = 211;

  public static void Save(
    BinaryWriter writer,
    WorldTreeTopsStateComponent state)
  {
    writer.Write(state.AreaCount);
    for (int areaId = 0; areaId < state.AreaCount; areaId++)
    {
      writer.Write(state.GetTreeStyle(areaId));
    }
  }

  public static void Load(
    BinaryReader reader,
    int loadVersion,
    WorldTreeTopsStateComponent state)
  {
    if (loadVersion < FirstLoadVersionWithSavedTreeTops)
    {
      return;
    }

    int variationCount = reader.ReadInt32();
    for (int areaId = 0;
         areaId < variationCount && areaId < state.AreaCount;
         areaId++)
    {
      state.SetTreeStyle(areaId, reader.ReadInt32());
    }
  }
}
