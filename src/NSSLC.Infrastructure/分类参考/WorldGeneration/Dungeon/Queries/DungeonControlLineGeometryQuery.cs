using Terraria.WorldGeneration.Dungeon.Layout;

namespace Terraria.WorldGeneration.Dungeon.Queries;

public static class DungeonControlLineGeometryQuery
{
  public static DungeonControlLineSnapshot Snapshot(
    DungeonControlLineComponent controlLine)
  {
    ArgumentNullException.ThrowIfNull(controlLine);
    return new DungeonControlLineSnapshot(
      controlLine.Index,
      controlLine.Prev,
      controlLine.Next,
      controlLine.Start,
      controlLine.End,
      controlLine.Center,
      controlLine.Style,
      controlLine.ProgressionStage,
      controlLine.LineLength,
      controlLine.CurveLine);
  }
}
