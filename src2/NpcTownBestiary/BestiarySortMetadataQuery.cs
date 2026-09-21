namespace Terraria.NpcTownBestiary;

public static class BestiarySortMetadataQuery
{
  public static bool IsHiddenFromOptions(BestiarySortDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    return definition.HiddenFromOptions;
  }
}
