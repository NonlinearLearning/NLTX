namespace Terraria.NpcTownBestiary;

public static class BestiaryFilterMetadataQuery
{
  public static bool? GetForcedDisplay(BestiaryFilterDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    return definition.ForcedDisplay;
  }
}
