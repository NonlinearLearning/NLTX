using System.Collections.ObjectModel;

namespace Terraria.Player.Mount;

public sealed class MountDefinitionCatalog
{
  private readonly IReadOnlyDictionary<int, MountDefinition> _definitions;
  private readonly IReadOnlyCollection<MountDefinition> _definitionValues;

  public MountDefinitionCatalog(IEnumerable<MountDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    Dictionary<int, MountDefinition> registry = [];
    foreach (MountDefinition definition in definitions)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!registry.TryAdd(definition.Id, definition))
      {
        throw new ArgumentException(
          $"Duplicate mount definition id {definition.Id}.",
          nameof(definitions));
      }
    }

    _definitionValues = registry.Values;
    _definitions = new ReadOnlyDictionary<int, MountDefinition>(registry);
  }

  public IReadOnlyCollection<MountDefinition> Definitions => _definitionValues;

  public bool TryGet(ContentId<MountDefinition> mountType, out MountDefinition definition)
  {
    return _definitions.TryGetValue(mountType.Value, out definition!);
  }

  public bool TryGet(int mountType, out MountDefinition definition)
  {
    return _definitions.TryGetValue(mountType, out definition!);
  }

  public MountDefinition GetRequired(ContentId<MountDefinition> mountType)
  {
    return _definitions.TryGetValue(mountType.Value, out MountDefinition? definition)
      ? definition
      : throw new KeyNotFoundException($"Mount definition {mountType.Value} is not registered.");
  }
}
