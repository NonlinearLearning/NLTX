using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileFrameImportant184Query
{
  private const ushort TileType = 184;

  public static TileFrameImportant184Result Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    short currentFrameY,
    GenerationRandomState randomState)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    if (!source.IsActive || source.Type != TileType)
    {
      return new TileFrameImportant184Result(
        false,
        source.FrameX,
        currentFrameY,
        false,
        randomState);
    }

    WorldTile up = GetTile(snapshot, x, y - 1);
    WorldTile down = GetTile(snapshot, x, y + 1);
    WorldTile left = GetTile(snapshot, x - 1, y);
    WorldTile right = GetTile(snapshot, x + 1, y);
    (GenerationRandomState nextState, int randomFrame) = randomState.NextExclusive(3);
    short randomFrameY = checked((short)(randomFrame * 18));

    int mossColor = MossColorQuery.GetColor(down.Type);
    if (IsDownMossSupport(down) && mossColor >= 0)
    {
      short frameY = currentFrameY is >= 0 and <= 36
        ? currentFrameY
        : randomFrameY;
      return new TileFrameImportant184Result(
        true,
        checked((short)(22 * mossColor)),
        frameY,
        false,
        nextState);
    }

    mossColor = MossColorQuery.GetColor(up.Type);
    if (IsUpMossSupport(up) && mossColor >= 0)
    {
      short frameY = currentFrameY is >= 54 and <= 90
        ? currentFrameY
        : checked((short)(54 + randomFrameY));
      return new TileFrameImportant184Result(
        true,
        checked((short)(22 * mossColor)),
        frameY,
        false,
        nextState);
    }

    mossColor = MossColorQuery.GetColor(left.Type);
    if (left.IsActive && mossColor >= 0)
    {
      short frameY = currentFrameY is >= 108 and <= 144
        ? currentFrameY
        : checked((short)(108 + randomFrameY));
      return new TileFrameImportant184Result(
        true,
        checked((short)(22 * mossColor)),
        frameY,
        false,
        nextState);
    }

    mossColor = MossColorQuery.GetColor(right.Type);
    if (right.IsActive && mossColor >= 0)
    {
      short frameY = currentFrameY is >= 162 and <= 198
        ? currentFrameY
        : checked((short)(162 + randomFrameY));
      return new TileFrameImportant184Result(
        true,
        checked((short)(22 * mossColor)),
        frameY,
        false,
        nextState);
    }

    return new TileFrameImportant184Result(false, source.FrameX, currentFrameY, true, nextState);
  }

  private static WorldTile GetTile(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y) ? snapshot.GetTile(x, y) : default;
  }

  private static bool IsDownMossSupport(WorldTile tile)
  {
    return tile.IsActive && !tile.IsHalfBrick && tile.Slope is not (1 or 2);
  }

  private static bool IsUpMossSupport(WorldTile tile)
  {
    return tile.IsActive && tile.Slope is not (3 or 4);
  }
}
