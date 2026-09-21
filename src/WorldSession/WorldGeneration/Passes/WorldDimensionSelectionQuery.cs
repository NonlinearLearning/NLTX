namespace Terraria.WorldGeneration.Passes;

public static class WorldDimensionSelectionQuery
{
  public static WorldSizeProfile Select(int legacySize)
  {
    return WorldSizeCatalogDefinition.FromLegacyIndex(legacySize);
  }

  public static int GetLegacyIndex(int width)
  {
    return WorldSizeCatalogDefinition.GetLegacyIndexForWidth(width);
  }
}
