using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class DungeonBiomeRoomSizeQuery
{
  private const int InnerSizeBase = 32;
  private const int InnerSizeBaseTemple = 50;
  private const int WallDepth = 8;
  private const int ReferenceWorldWidth = 4200;
  private const int TempleStyleId = 10;

  public static int GetInnerSize(int worldWidth, int styleId)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldWidth);
    DungeonGenerationStyleSelection.ValidateStyleId(styleId);
    int baseSize = styleId == TempleStyleId ? InnerSizeBaseTemple : InnerSizeBase;
    return checked((int)(baseSize * (float)worldWidth / ReferenceWorldWidth));
  }

  public static int GetOuterSize(int worldWidth, int styleId)
  {
    return checked(GetInnerSize(worldWidth, styleId) + WallDepth);
  }
}
