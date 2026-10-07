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
    if (variationCount < 0 || variationCount > state.AreaCount)
    {
      throw new InvalidDataException(
        "The world TreeTops section contains an invalid style count.");
    }

    int[] styles = new int[variationCount];
    for (int areaId = 0; areaId < styles.Length; areaId++)
    {
      styles[areaId] = reader.ReadInt32();
      if (!WorldTreeTopsSystem.IsValidStyle(areaId, styles[areaId]))
      {
        throw new InvalidDataException(
          $"The world TreeTops section contains an invalid style for area {areaId}.");
      }
    }

    WorldTreeTopsSystem.ApplyStyles(state, styles);
  }
}
