namespace NLTX.EconomyCraftingFishingLoot.Loot;

public readonly record struct DropSourceAttribution(
  DropSourceKind Kind,
  string SourceEntityKey)
{
  public bool OwnsEntity => false;

  public static DropSourceAttribution BossSpawn(string sourceEntityKey)
  {
    return Create(DropSourceKind.BossSpawn, sourceEntityKey);
  }

  public static DropSourceAttribution DropAsItem(string sourceEntityKey)
  {
    return Create(DropSourceKind.DropAsItem, sourceEntityKey);
  }

  public static DropSourceAttribution Loot(string sourceEntityKey)
  {
    return Create(DropSourceKind.Loot, sourceEntityKey);
  }

  public static DropSourceAttribution FishedOut(string sourceEntityKey)
  {
    return Create(DropSourceKind.FishedOut, sourceEntityKey);
  }

  private static DropSourceAttribution Create(
    DropSourceKind kind,
    string sourceEntityKey)
  {
    if (string.IsNullOrWhiteSpace(sourceEntityKey))
    {
      throw new ArgumentException(
        "A drop source key must contain non-whitespace characters.",
        nameof(sourceEntityKey));
    }

    return new DropSourceAttribution(kind, sourceEntityKey);
  }
}
