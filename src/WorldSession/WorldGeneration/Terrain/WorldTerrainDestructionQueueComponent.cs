using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Terrain;

/// <summary>
/// Stores terrain positions waiting for a later frame/network effect flush.
/// </summary>
public sealed class WorldTerrainDestructionQueueComponent
{
  private readonly Queue<TilePosition> _pendingPositions = new();

  public int Count => _pendingPositions.Count;

  public bool IsEmpty => _pendingPositions.Count == 0;

  public void Enqueue(TilePosition position)
  {
    _pendingPositions.Enqueue(position);
  }

  public bool TryDequeue(out TilePosition position)
  {
    return _pendingPositions.TryDequeue(out position);
  }

  public void Clear()
  {
    _pendingPositions.Clear();
  }
}
