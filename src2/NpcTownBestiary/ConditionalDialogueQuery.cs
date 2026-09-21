namespace Terraria.NpcTownBestiary;

public sealed class ConditionalDialogueQuery
{
  private readonly ConditionalDialogueCatalog _catalog;

  public ConditionalDialogueQuery(ConditionalDialogueCatalog catalog)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  public ConditionalDialogueEligibilityResult Evaluate(NpcDialogueReadView npc)
  {
    foreach (ConditionalDialogueDefinition definition in _catalog.GetForNpc(npc.NpcType))
    {
      if (definition.Condition.Invoke(npc))
      {
        return new ConditionalDialogueEligibilityResult(
          true,
          definition.Key,
          definition.ShowIndicator);
      }
    }

    return new ConditionalDialogueEligibilityResult(false, null, false);
  }
}
