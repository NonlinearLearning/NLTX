namespace Terraria.NpcTownBestiary;

public static class BestiaryFilterQuery
{
  public static bool Matches(
    BestiaryFilterDefinition definition,
    BestiaryFilterContext context)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(context.InfoElementKeys);

    return definition.Kind switch
    {
      BestiaryFilterKind.Search => string.IsNullOrWhiteSpace(context.SearchTerm) ||
        context.Entry.DisplayName.Contains(context.SearchTerm, StringComparison.OrdinalIgnoreCase),
      BestiaryFilterKind.UnlockState =>
        context.UnlockState != BestiaryEntryUnlockState.NotKnownAtAll,
      BestiaryFilterKind.RareCreature => context.Entry.RarityLevel > 0,
      BestiaryFilterKind.Boss => context.Entry.IsBoss,
      BestiaryFilterKind.InfoElement => definition.InfoElementKey is not null &&
        context.InfoElementKeys.Contains(definition.InfoElementKey),
      _ => false
    };
  }
}
