using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWallFrameLookupRegistry
{
  public const int FrameSize = 36;
  public const int MaskCount = 20;
  public const int FrameNumberCount = 4;

  private static readonly int[][] _phlebasFrameNumbers =
  {
    new[] { 2, 4, 2 },
    new[] { 1, 3, 1 },
    new[] { 2, 2, 4 },
    new[] { 1, 1, 3 }
  };

  private static readonly int[][] _lazureFrameNumbers =
  {
    new[] { 1, 3 },
    new[] { 2, 4 }
  };

  private static readonly int[][] _centerWallFrameOffsets =
  {
    new[] { 2, 0, 0 },
    new[] { 0, 1, 4 },
    new[] { 0, 3, 0 }
  };

  private static readonly LegacyWallFrameOffset[][] _wallFrames =
  {
    CreateRow(9, 3, 10, 3, 11, 3, 6, 6),
    CreateRow(6, 3, 7, 3, 8, 3, 4, 6),
    CreateRow(12, 0, 12, 1, 12, 2, 12, 5),
    CreateRow(1, 4, 3, 4, 5, 4, 3, 6),
    CreateRow(9, 0, 9, 1, 9, 2, 9, 5),
    CreateRow(0, 4, 2, 4, 4, 4, 2, 6),
    CreateRow(6, 4, 7, 4, 8, 4, 5, 6),
    CreateRow(1, 2, 2, 2, 3, 2, 3, 5),
    CreateRow(6, 0, 7, 0, 8, 0, 6, 5),
    CreateRow(5, 0, 5, 1, 5, 2, 5, 5),
    CreateRow(1, 3, 3, 3, 5, 3, 1, 6),
    CreateRow(4, 0, 4, 1, 4, 2, 4, 5),
    CreateRow(0, 3, 2, 3, 4, 3, 0, 6),
    CreateRow(0, 0, 0, 1, 0, 2, 0, 5),
    CreateRow(1, 0, 2, 0, 3, 0, 1, 5),
    CreateRow(1, 1, 2, 1, 3, 1, 2, 5),
    CreateRow(6, 1, 7, 1, 8, 1, 7, 5),
    CreateRow(6, 2, 7, 2, 8, 2, 8, 5),
    CreateRow(10, 0, 10, 1, 10, 2, 10, 5),
    CreateRow(11, 0, 11, 1, 11, 2, 11, 5)
  };

  public static int GetPhlebasFrameNumber(int yModulus, int xModulus)
  {
    if (yModulus < 0 || yModulus >= _phlebasFrameNumbers.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(yModulus));
    }

    if (xModulus < 0 || xModulus >= _phlebasFrameNumbers[yModulus].Length)
    {
      throw new ArgumentOutOfRangeException(nameof(xModulus));
    }

    return _phlebasFrameNumbers[yModulus][xModulus] - 1;
  }

  public static int GetLazureFrameNumber(int xModulus, int yModulus)
  {
    if (xModulus < 0 || xModulus >= _lazureFrameNumbers.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(xModulus));
    }

    if (yModulus < 0 || yModulus >= _lazureFrameNumbers[xModulus].Length)
    {
      throw new ArgumentOutOfRangeException(nameof(yModulus));
    }

    return _lazureFrameNumbers[xModulus][yModulus] - 1;
  }

  public static int GetCenterWallFrameOffset(int xModulus, int yModulus)
  {
    if (xModulus < 0 || xModulus >= _centerWallFrameOffsets.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(xModulus));
    }

    if (yModulus < 0 || yModulus >= _centerWallFrameOffsets[xModulus].Length)
    {
      throw new ArgumentOutOfRangeException(nameof(yModulus));
    }

    return _centerWallFrameOffsets[xModulus][yModulus];
  }

  public static LegacyWallFrameOffset GetWallFrame(int mask, int frameNumber)
  {
    if (mask < 0 || mask >= MaskCount)
    {
      throw new ArgumentOutOfRangeException(nameof(mask));
    }

    if (frameNumber < 0 || frameNumber >= FrameNumberCount)
    {
      throw new ArgumentOutOfRangeException(nameof(frameNumber));
    }

    return _wallFrames[mask][frameNumber];
  }

  private static LegacyWallFrameOffset[] CreateRow(
    int frameOneX,
    int frameOneY,
    int frameTwoX,
    int frameTwoY,
    int frameThreeX,
    int frameThreeY,
    int frameFourX,
    int frameFourY)
  {
    return new[]
    {
      CreateOffset(frameOneX, frameOneY),
      CreateOffset(frameTwoX, frameTwoY),
      CreateOffset(frameThreeX, frameThreeY),
      CreateOffset(frameFourX, frameFourY)
    };
  }

  private static LegacyWallFrameOffset CreateOffset(int frameX, int frameY)
  {
    return new LegacyWallFrameOffset(
      checked((short)(frameX * FrameSize)),
      checked((short)(frameY * FrameSize)));
  }
}
