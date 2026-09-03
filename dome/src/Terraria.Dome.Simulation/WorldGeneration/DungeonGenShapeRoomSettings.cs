using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonGenShapeRoomSettings
{
  public DungeonGenShapeRoomSettings(DungeonGenShapeType shapeType, int boundingRadius)
  {
    if (!Enum.IsDefined(shapeType))
    {
      throw new ArgumentOutOfRangeException(nameof(shapeType));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(boundingRadius);
    ShapeType = shapeType;
    BoundingRadius = boundingRadius;
  }

  public DungeonGenShapeType ShapeType { get; }

  public int BoundingRadius { get; }

  public int GetBoundingRadius()
  {
    return BoundingRadius;
  }
}
