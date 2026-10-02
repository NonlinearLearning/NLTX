using System.Collections.ObjectModel;

namespace Terraria.LeashedEntity;

/// <summary>
/// Owns the ordered, process-local definition catalog. Runtime entities only retain DefinitionId.
/// </summary>
public sealed class LeashedDefinitionCatalog
{
  private readonly List<LeashedDefinitionDescriptor?> _definitions = new() { null };
  private readonly Dictionary<string, LeashedDefinitionDescriptor> _definitionsByKey =
    new(StringComparer.Ordinal);
  private readonly Dictionary<int, LeashedDefinitionDescriptor> _definitionsByContentId = new();
  private bool _isFrozen;

  public int Count => _definitions.Count - 1;

  public bool IsFrozen => _isFrozen;

  public LeashedDefinitionDescriptor Register(
    string key,
    LeashedDefinitionKind kind,
    int? contentId = null)
  {
    if (_isFrozen)
    {
      throw new InvalidOperationException("The leashed definition catalog is frozen.");
    }

    if (string.IsNullOrWhiteSpace(key))
    {
      throw new ArgumentException("A definition key is required.", nameof(key));
    }

    if (_definitionsByKey.ContainsKey(key))
    {
      throw new InvalidOperationException($"The leashed definition key is already registered: {key}");
    }

    if (contentId is <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(contentId));
    }

    if (contentId.HasValue && _definitionsByContentId.ContainsKey(contentId.Value))
    {
      throw new InvalidOperationException(
        $"The leashed content id is already registered: {contentId.Value}");
    }

    LeashedDefinitionDescriptor descriptor =
      new(_definitions.Count, key, kind, contentId);
    _definitions.Add(descriptor);
    _definitionsByKey.Add(key, descriptor);
    if (contentId.HasValue)
    {
      _definitionsByContentId.Add(contentId.Value, descriptor);
    }

    return descriptor;
  }

  public void Freeze()
  {
    _isFrozen = true;
  }

  public bool TryGet(int definitionId, out LeashedDefinitionDescriptor descriptor)
  {
    descriptor = null!;
    if (definitionId <= 0 || definitionId >= _definitions.Count)
    {
      return false;
    }

    LeashedDefinitionDescriptor? candidate = _definitions[definitionId];
    if (candidate is null)
    {
      return false;
    }

    descriptor = candidate;
    return true;
  }

  public bool TryGetByKey(string key, out LeashedDefinitionDescriptor descriptor)
  {
    return _definitionsByKey.TryGetValue(key, out descriptor!);
  }

  public bool TryGetByContentId(int contentId, out LeashedDefinitionDescriptor descriptor)
  {
    return _definitionsByContentId.TryGetValue(contentId, out descriptor!);
  }

  public IReadOnlyList<LeashedDefinitionDescriptor> Snapshot()
  {
    List<LeashedDefinitionDescriptor> snapshot = new(Count);
    for (int i = 1; i < _definitions.Count; i++)
    {
      snapshot.Add(_definitions[i]!);
    }

    return new ReadOnlyCollection<LeashedDefinitionDescriptor>(snapshot);
  }
}

