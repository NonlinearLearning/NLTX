using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Layout;

public sealed class DungeonControlLineSystem
{
  public DungeonControlLineComponent Create(
    int index,
    DungeonTilePoint start,
    DungeonTilePoint end,
    DungeonStyleId style,
    int progressionStage,
    bool curveLine = false)
  {
    int deltaX = end.X - start.X;
    int deltaY = end.Y - start.Y;
    float lineLength = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
    var controlLine = new DungeonControlLineComponent(index);
    controlLine.SetGeometry(
      start,
      end,
      new DungeonTilePoint(deltaX, deltaY),
      new DungeonTilePoint(deltaX, deltaY),
      new DungeonTilePoint(-deltaY, deltaX),
      new DungeonTilePoint(-deltaY, deltaX),
      new DungeonTilePoint(deltaX, deltaY),
      startRadius: 1f,
      endRadius: 1f,
      normalizedDistanceSafeFromDither: 0f,
      styleTransitionDitherWidth: 0f,
      borderWidth: 1f,
      normalizedLineDirection: lineLength == 0f ? 0f : deltaX / lineLength,
      lineLength,
      style,
      progressionStage,
      curveLine);
    return controlLine;
  }
}
