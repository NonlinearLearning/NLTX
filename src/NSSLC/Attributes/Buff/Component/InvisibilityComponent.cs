using System.Collections.Generic;

namespace Terraria.Attributes;

/// <summary>Marks an entity as invisible while one or more status effects are active.</summary>
public sealed class InvisibilityComponent
{
  private readonly HashSet<long> _sources = [];

  public InvisibilityComponent()
  {
  }

  public InvisibilityComponent(long sourceId)
  {
    Add(sourceId);
  }

  public IReadOnlyCollection<long> SourceIds => _sources;

  public bool IsActive => _sources.Count != 0;

  public void Add(long sourceId)
  {
    if (sourceId == 0)
    {
      throw new System.ArgumentOutOfRangeException(nameof(sourceId));
    }

    _sources.Add(sourceId);
  }

  public bool Remove(long sourceId) => _sources.Remove(sourceId);
}
