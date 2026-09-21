using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

public sealed class BestiaryCollectionProviderDefinitions
{
  private readonly Dictionary<BestiaryEntryKey, BestiaryCollectionProviderDefinition> _definitions = new();

  public IReadOnlyDictionary<BestiaryEntryKey, BestiaryCollectionProviderDefinition> Definitions =>
    new ReadOnlyDictionary<BestiaryEntryKey, BestiaryCollectionProviderDefinition>(_definitions);

  public void Register(
    BestiaryEntryKey entryKey,
    BestiaryCollectionProviderDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    _definitions[entryKey] = definition;
  }

  public bool TryGet(
    BestiaryEntryKey entryKey,
    out BestiaryCollectionProviderDefinition definition)
  {
    return _definitions.TryGetValue(entryKey, out definition!);
  }
}
