namespace Terraria.NpcTownBestiary;

public sealed class BestiarySortDefinition
{
  public BestiarySortDefinition(BestiarySortKind kind, bool hiddenFromOptions)
  {
    Kind = kind;
    HiddenFromOptions = hiddenFromOptions;
  }

  public BestiarySortKind Kind { get; }

  public bool HiddenFromOptions { get; }
}
