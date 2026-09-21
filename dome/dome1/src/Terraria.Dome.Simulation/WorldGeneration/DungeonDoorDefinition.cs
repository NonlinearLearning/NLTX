namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonDoorDefinition
{
  public DungeonDoorDefinition(
    int positionX,
    int positionY,
    int? overrideBrickTileType,
    int? overrideBrickWallType,
    int? overrideStyle,
    int direction,
    bool inAHallway,
    int? overrideWidthFluff,
    bool skipOtherDoorsCheck,
    bool skipSpaceCheck,
    bool alwaysClearArea)
  {
    PositionX = positionX;
    PositionY = positionY;
    OverrideBrickTileType = overrideBrickTileType;
    OverrideBrickWallType = overrideBrickWallType;
    OverrideStyle = overrideStyle;
    Direction = direction;
    InAHallway = inAHallway;
    OverrideWidthFluff = overrideWidthFluff;
    SkipOtherDoorsCheck = skipOtherDoorsCheck;
    SkipSpaceCheck = skipSpaceCheck;
    AlwaysClearArea = alwaysClearArea;
  }

  public int PositionX { get; }

  public int PositionY { get; }

  public int? OverrideBrickTileType { get; }

  public int? OverrideBrickWallType { get; }

  public int? OverrideStyle { get; }

  public int Direction { get; }

  public bool InAHallway { get; }

  public int? OverrideWidthFluff { get; }

  public bool SkipOtherDoorsCheck { get; }

  public bool SkipSpaceCheck { get; }

  public bool AlwaysClearArea { get; }
}
