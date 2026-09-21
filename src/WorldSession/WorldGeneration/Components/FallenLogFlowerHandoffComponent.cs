using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Owns the generation-time coordinate handoff from fallen-log placement to Flowers.
/// </summary>
public sealed class FallenLogFlowerHandoffComponent
{
  public FallenLogFlowerHandoffComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    Reset();
  }

  public long GenerationId { get; }

  public int LogX { get; private set; }

  public int LogY { get; private set; }

  public bool HasPendingLog => LogX >= 0;

  public void Publish(TilePosition position)
  {
    LogX = position.X;
    LogY = position.Y;
  }

  public bool TryConsume(out TilePosition position)
  {
    if (!HasPendingLog)
    {
      position = default;
      return false;
    }

    position = new TilePosition(LogX, LogY);
    LogX = -1;
    return true;
  }

  public void Reset()
  {
    LogX = -1;
    LogY = -1;
  }

  public FallenLogFlowerHandoffSnapshot CreateSnapshot()
  {
    return new FallenLogFlowerHandoffSnapshot(
      GenerationId,
      LogX,
      LogY,
      HasPendingLog);
  }
}
