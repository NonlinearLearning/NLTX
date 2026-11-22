using System.Collections.Generic;

namespace Terraria.Attributes;

/// <summary>Stores one or more status effect movement/decision locks.</summary>
public sealed class StunComponent
{
  private readonly HashSet<long> _sources = [];

  public StunComponent()
  {
  }

  public StunComponent(long sourceId)
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
