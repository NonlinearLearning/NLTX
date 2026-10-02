namespace Terraria.NpcTownBestiary;

public sealed class NpcPersonalityCatalogBuilder
{
  private readonly Dictionary<NpcTypeId, NpcPersonalityDefinition> _definitions = new();

  public NpcPersonalityCatalogBuilder Add(
    int npcType,
    NpcPersonalityDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    _definitions[new NpcTypeId(npcType)] = definition;
    return this;
  }

  public NpcPersonalityCatalog Build()
  {
    return new NpcPersonalityCatalog(_definitions);
  }
}
