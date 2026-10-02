using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

public sealed class NpcPersonalityCatalog
{
  private readonly Dictionary<NpcTypeId, NpcPersonalityDefinition> _definitions;

  internal NpcPersonalityCatalog(
    IReadOnlyDictionary<NpcTypeId, NpcPersonalityDefinition> definitions)
  {
    _definitions = new Dictionary<NpcTypeId, NpcPersonalityDefinition>(definitions);
  }

  public IReadOnlyDictionary<NpcTypeId, NpcPersonalityDefinition> Definitions =>
    new ReadOnlyDictionary<NpcTypeId, NpcPersonalityDefinition>(_definitions);

  public bool TryGet(NpcTypeId npcType, out NpcPersonalityDefinition definition)
  {
    return _definitions.TryGetValue(npcType, out definition!);
  }

  public bool TryGet(int npcType, out NpcPersonalityDefinition definition)
  {
    return TryGet(new NpcTypeId(npcType), out definition);
  }
}
