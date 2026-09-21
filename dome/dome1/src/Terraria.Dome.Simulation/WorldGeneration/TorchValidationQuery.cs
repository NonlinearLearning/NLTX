using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TorchValidationQuery
{
  private const short FrameDirectionWidth = 22;

  public static TorchValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile torch = snapshot.GetTile(x, y);
    short baseFrame = torch.FrameX >= 66 ? (short)66 : (short)0;
    bool canDown = CanAttachBottom(snapshot, tileDefinitions, x, y + 1);
    bool canLeft = CanAttachSide(snapshot, tileDefinitions, x - 1, y, left: true);
    bool canRight = CanAttachSide(snapshot, tileDefinitions, x + 1, y, left: false);
    bool canAttachToWall = torch.WallType > 0;
    short suggestedFrameX = canDown
      ? baseFrame
      : canLeft
        ? (short)(FrameDirectionWidth + baseFrame)
        : canRight
          ? (short)(FrameDirectionWidth * 2 + baseFrame)
          : baseFrame;
    bool valid = canDown || canLeft || canRight || canAttachToWall;
    return new TorchValidationResult(
      valid,
      !valid,
      canDown,
      canLeft,
      canRight,
      canAttachToWall,
      suggestedFrameX);
  }

  private static bool CanAttachBottom(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry definitions,
    int x,
    int y)
  {
    return snapshot.Metadata.IsInside(x, y) &&
      TileStateQuery.CanAttachToTop(snapshot, definitions, x, y);
  }

  private static bool CanAttachSide(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry definitions,
    int x,
    int y,
    bool left)
  {
    if (!snapshot.Metadata.IsInside(x, y))
    {
      return false;
    }

    return left
      ? TileStateQuery.CanAttachToRight(snapshot, definitions, x, y)
      : TileStateQuery.CanAttachToLeft(snapshot, definitions, x, y);
  }
}
