using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PalmTreeValidationQuery
{
  private const ushort PalmTreeBaseType = 53;

  public static PalmTreeValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile current = snapshot.GetTile(x, y);
    WorldTile above = snapshot.Metadata.IsInside(x, y - 1)
      ? snapshot.GetTile(x, y - 1)
      : default;
    WorldTile below = snapshot.Metadata.IsInside(x, y + 1)
      ? snapshot.GetTile(x, y + 1)
      : default;
    ushort normalizedAboveType = NormalizeGroundType(above);
    bool validAbove = normalizedAboveType == PalmTreeBaseType ||
      normalizedAboveType == current.Type;
    bool specialFrameRequiresPalm = current.FrameX is 66 or 220;
    bool supported = validAbove &&
      (!specialFrameRequiresPalm || normalizedAboveType == PalmTreeBaseType);
    bool connectedBelow = below.IsActive && below.Type == current.Type;
    short suggestedFrameX = current.FrameX;
    bool requiresRandomFrameSelection = false;
    if (!connectedBelow && current.FrameX <= 44)
    {
      requiresRandomFrameSelection = true;
    }
    else if (!connectedBelow && current.FrameX == 66)
    {
      suggestedFrameX = 220;
    }

    return new PalmTreeValidationResult(
      supported,
      !supported,
      current.Type,
      normalizedAboveType,
      current.FrameX,
      suggestedFrameX,
      requiresRandomFrameSelection,
      true,
      true);
  }

  private static ushort NormalizeGroundType(WorldTile tile)
  {
    if (!tile.IsActive)
    {
      return 0;
    }

    return tile.Type is 234 or 116 or 112 ? PalmTreeBaseType : tile.Type;
  }
}
