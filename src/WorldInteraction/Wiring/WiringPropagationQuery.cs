using System;
using System.Collections.Generic;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public static class WiringPropagationQuery
{
  public static IReadOnlyList<byte> WireColorPassOrder =>
    Array.AsReadOnly(new byte[] { 1, 2, 3, 4 });

  public static byte NormalizeCurrentUser(int currentUser)
  {
    return currentUser is < 0 or > byte.MaxValue
      ? byte.MaxValue
      : (byte)currentUser;
  }

  public static bool IsValidWireColor(byte wireColor)
  {
    return wireColor is >= 1 and <= 4;
  }

  public static bool IsWithinBounds(
    TileCoordinate coordinate,
    int worldWidth,
    int worldHeight)
  {
    ValidateWorldDimensions(worldWidth, worldHeight);
    return coordinate.X >= 0 && coordinate.X < worldWidth &&
      coordinate.Y >= 0 && coordinate.Y < worldHeight;
  }

  public static bool IsWithinBounds(
    WiringPropagationCommand command,
    int worldWidth,
    int worldHeight)
  {
    ValidateWorldDimensions(worldWidth, worldHeight);
    if (command.Kind != WiringPropagationCommandKind.BeginTrip)
    {
      return true;
    }

    if (command.Left < 0 || command.Top < 0 ||
        command.Width <= 0 || command.Height <= 0)
    {
      return false;
    }

    long right = (long)command.Left + command.Width;
    long bottom = (long)command.Top + command.Height;
    return right <= worldWidth && bottom <= worldHeight;
  }

  public static bool IsKnownCommand(
    WiringPropagationCommand command)
  {
    return command.Kind != WiringPropagationCommandKind.Invalid;
  }

  private static void ValidateWorldDimensions(
    int worldWidth,
    int worldHeight)
  {
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (worldHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldHeight));
    }
  }
}
