using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyDungeonEntranceAnchorPolicy
{
  private const int InitialOffsetMinimum = -200;
  private const int InitialOffsetMaximum = 200;
  private const int InitialSolidProbeLength = 10;
  private const int UpperScanOffset = 200;
  private const int SolidClearance = 60;

  public static int CalculateAnchorY(
    int dungeonX,
    int worldSurfaceY,
    int rockLayerY,
    LegacyPassRandomState random,
    Func<int, int, bool> isSolid,
    bool drunkWorldGen = false,
    bool noSurface = false)
  {
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(isSolid);
    if (dungeonX < 0 || worldSurfaceY < 0 || rockLayerY < worldSurfaceY)
    {
      throw new ArgumentOutOfRangeException(nameof(dungeonX));
    }

    int anchorY = (worldSurfaceY + rockLayerY) / 2 +
      random.Next(InitialOffsetMinimum, InitialOffsetMaximum);
    int upperBound = (worldSurfaceY + rockLayerY) / 2 + UpperScanOffset;
    bool foundSolid = false;
    for (int offset = 0; offset < InitialSolidProbeLength; offset++)
    {
      if (isSolid(dungeonX, anchorY + offset))
      {
        foundSolid = true;
        break;
      }
    }

    if (!foundSolid)
    {
      for (; anchorY < upperBound && !isSolid(dungeonX, anchorY + InitialSolidProbeLength); anchorY++)
      {
      }
    }
    else
    {
      int solidDepth = 0;
      while (solidDepth < SolidClearance && isSolid(dungeonX, anchorY - solidDepth))
      {
        solidDepth++;
      }

      if (solidDepth < SolidClearance)
      {
        anchorY += SolidClearance - solidDepth;
      }
    }

    if (drunkWorldGen && !noSurface)
    {
      anchorY = worldSurfaceY + 70;
    }

    return anchorY;
  }
}
