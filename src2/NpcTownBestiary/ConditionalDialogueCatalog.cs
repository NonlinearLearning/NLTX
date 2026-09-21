namespace Terraria.NpcTownBestiary;

public sealed class ConditionalDialogueCatalog
{
  private readonly Dictionary<NpcTypeId, List<ConditionalDialogueDefinition>> _definitions = new();

  public void Register(ConditionalDialogueDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    if (!_definitions.TryGetValue(definition.NpcType, out List<ConditionalDialogueDefinition>? list))
    {
      list = new List<ConditionalDialogueDefinition>();
      _definitions.Add(definition.NpcType, list);
    }

    if (list.Any(existing => existing.Key == definition.Key))
    {
      throw new InvalidOperationException(
        $"Conditional dialogue key already exists: {definition.Key.Value}");
    }

    list.Add(definition);
  }

  public bool TryGet(
    NpcTypeId npcType,
    ConditionalDialogueKey key,
    out ConditionalDialogueDefinition definition)
  {
    if (_definitions.TryGetValue(npcType, out List<ConditionalDialogueDefinition>? list))
    {
      definition = list.FirstOrDefault(candidate => candidate.Key == key)!;
      return definition is not null;
    }

    definition = null!;
    return false;
  }

  public IReadOnlyList<ConditionalDialogueDefinition> GetForNpc(NpcTypeId npcType)
  {
    return _definitions.TryGetValue(npcType, out List<ConditionalDialogueDefinition>? list)
      ? list.AsReadOnly()
      : Array.Empty<ConditionalDialogueDefinition>();
  }
}
