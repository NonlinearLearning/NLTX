using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonBiomeRoomSettings
{
  public DungeonBiomeRoomSettings(int worldWidth, int styleId)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldWidth);
    DungeonGenerationStyleSelection.ValidateStyleId(styleId);
    WorldWidth = worldWidth;
    StyleId = styleId;
  }

  public int WorldWidth { get; }

  public int StyleId { get; }

  public int GetBoundingRadius()
  {
    return DungeonBiomeRoomSizeQuery.GetOuterSize(WorldWidth, StyleId);
  }
}
