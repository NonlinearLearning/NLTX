namespace MinimalImplementation.Combat.Npcs;

/// <summary>Contains only explicitly migrated Version4 NPC content facts.</summary>
public sealed class NpcDefinitionCatalog
{
  private readonly Dictionary<short, NpcDefinition> _definitions;

  public NpcDefinitionCatalog(IReadOnlyCollection<NpcDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitions = new Dictionary<short, NpcDefinition>(definitions.Count);
    foreach (NpcDefinition definition in definitions)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!_definitions.TryAdd(definition.Type, definition))
      {
        throw new ArgumentException(
          $"NPC type {definition.Type} has more than one content definition.",
          nameof(definitions));
      }
    }
  }

  public bool TryGet(short type, out NpcDefinition definition)
  {
    return _definitions.TryGetValue(type, out definition!);
  }

  /// <summary>NPC.SetDefaults, v1.4.5.6/319. This is the first migrated content batch.</summary>
  public static NpcDefinitionCatalog CreateVersion4Core() {
    return new NpcDefinitionCatalog([
      new NpcDefinition(1, 1, 24, 18, 25, false, false, false, false,
          damage: 7, defense: 2),
      new NpcDefinition(2, 2, 30, 32, 60, false, false, false, false,
          damage: 18, defense: 2, knockbackResist: 0.8f),
      new NpcDefinition(3, 3, 18, 40, 45, false, false, false, false,
          damage: 14, defense: 6, knockbackResist: 0.5f),
      new NpcDefinition(22, 7, 18, 40, 250, false, true, false, false,
          damage: 10, defense: 30, friendly: true, knockbackResist: 0.5f)
    ]);
  }
}
