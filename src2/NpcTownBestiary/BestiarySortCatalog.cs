namespace Terraria.NpcTownBestiary;

public sealed class BestiarySortCatalog
{
  private readonly List<BestiarySortDefinition> _definitions = new();

  public IReadOnlyList<BestiarySortDefinition> Definitions => _definitions.AsReadOnly();

  public BestiarySortCatalog()
  {
    Register(new BestiarySortDefinition(BestiarySortKind.NetId, true));
    Register(new BestiarySortDefinition(BestiarySortKind.UnlockState, true));
    Register(new BestiarySortDefinition(BestiarySortKind.BestiarySortingId, false));
    Register(new BestiarySortDefinition(BestiarySortKind.Rarity, false));
    Register(new BestiarySortDefinition(BestiarySortKind.Alphabetical, false));
    Register(new BestiarySortDefinition(BestiarySortKind.Stat, false));
  }

  public BestiarySortDefinition Register(BestiarySortDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    _definitions.Add(definition);
    return definition;
  }
}
