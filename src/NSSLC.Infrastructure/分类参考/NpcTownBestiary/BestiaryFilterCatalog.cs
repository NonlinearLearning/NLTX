using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

public sealed class BestiaryFilterCatalog
{
  private readonly List<BestiaryFilterDefinition> _definitions = new();

  public IReadOnlyList<BestiaryFilterDefinition> Definitions => _definitions.AsReadOnly();

  public BestiaryFilterCatalog()
  {
    Register(new BestiaryFilterDefinition(BestiaryFilterKind.Search, true));
    Register(new BestiaryFilterDefinition(BestiaryFilterKind.UnlockState, true));
    Register(new BestiaryFilterDefinition(BestiaryFilterKind.RareCreature, null));
    Register(new BestiaryFilterDefinition(BestiaryFilterKind.Boss, null));
  }

  public BestiaryFilterDefinition Register(BestiaryFilterDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    _definitions.Add(definition);
    return definition;
  }
}
