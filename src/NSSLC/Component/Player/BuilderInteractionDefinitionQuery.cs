namespace Terraria.Player;

public static class BuilderInteractionDefinitionQuery
{
  public static bool IsKnownToggleId(int toggleId)
  {
    return toggleId >= 0 && toggleId < PlayerBuilderInteractionCatalog.Count;
  }
}
