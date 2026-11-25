using System.Collections.Generic;

namespace Terraria.Share.Attributes;

/// <summary>Marks damage immunity supplied by one or more status effect sources.</summary>
public sealed class DamageImmunityStatusComponent
{
  private readonly HashSet<long> _sources = [];

  public DamageImmunityStatusComponent()
  {
  }

  public DamageImmunityStatusComponent(long sourceId)
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
