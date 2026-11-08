using System.Collections.Generic;

namespace Terraria.World;

/// <summary>Tick-scoped queue of applied tile entity outcomes.</summary>
public sealed class WorldTileEntityResultQueueComponent
{
  private readonly List<WorldTileEntityResult> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in WorldTileEntityResult result) => _pending.Add(result);

  public IReadOnlyList<WorldTileEntityResult> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    WorldTileEntityResult[] results = _pending.ToArray();
    _pending.Clear();
    return results;
  }

  public void Clear() => _pending.Clear();
}
