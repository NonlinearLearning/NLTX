using System.Collections.Generic;

namespace Terraria.World;

/// <summary>Tick-scoped queue of chest, sign and tile entity intentions.</summary>
public sealed class WorldTileEntityRequestQueueComponent
{
  private readonly List<WorldTileEntityRequest> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in WorldTileEntityRequest request) => _pending.Add(request);

  public IReadOnlyList<WorldTileEntityRequest> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    WorldTileEntityRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return requests;
  }

  public void Clear() => _pending.Clear();
}
