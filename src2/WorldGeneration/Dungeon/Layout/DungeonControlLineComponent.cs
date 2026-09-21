using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Layout;

public sealed class DungeonControlLineComponent
{
  public DungeonControlLineComponent(int index)
  {
    if (index < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    Index = index;
  }

  public int Index { get; }

  public int? Next { get; private set; }

  public int? Prev { get; private set; }

  public DungeonTilePoint Start { get; private set; }

  public DungeonTilePoint End { get; private set; }

  public DungeonTilePoint StartTangent { get; private set; }

  public DungeonTilePoint EndTangent { get; private set; }

  public DungeonTilePoint StartNormal { get; private set; }

  public DungeonTilePoint EndNormal { get; private set; }

  public DungeonTilePoint CrossTangent { get; private set; }

  public float StartRadius { get; private set; }

  public float EndRadius { get; private set; }

  public float NormalizedDistanceSafeFromDither { get; private set; }

  public float StyleTransitionDitherWidth { get; private set; }

  public float BorderWidth { get; private set; }

  public float NormalizedLineDirection { get; private set; }

  public float LineLength { get; private set; }

  public DungeonStyleId Style { get; private set; }

  public int ProgressionStage { get; private set; }

  public bool CurveLine { get; private set; }

  public DungeonTilePoint Center => new(
    Start.X + (End.X - Start.X) / 2,
    Start.Y + (End.Y - Start.Y) / 2);

  public void SetLinks(int? prev, int? next)
  {
    if (prev == Index || next == Index)
    {
      throw new ArgumentException("A control line cannot link to itself.");
    }

    Prev = prev;
    Next = next;
  }

  public void SetGeometry(
    DungeonTilePoint start,
    DungeonTilePoint end,
    DungeonTilePoint startTangent,
    DungeonTilePoint endTangent,
    DungeonTilePoint startNormal,
    DungeonTilePoint endNormal,
    DungeonTilePoint crossTangent,
    float startRadius,
    float endRadius,
    float normalizedDistanceSafeFromDither,
    float styleTransitionDitherWidth,
    float borderWidth,
    float normalizedLineDirection,
    float lineLength,
    DungeonStyleId style,
    int progressionStage,
    bool curveLine)
  {
    float[] values =
    [
      startRadius,
      endRadius,
      normalizedDistanceSafeFromDither,
      styleTransitionDitherWidth,
      borderWidth,
      normalizedLineDirection,
      lineLength
    ];
    if (values.Any(value => !float.IsFinite(value) || value < 0f))
    {
      throw new ArgumentOutOfRangeException(nameof(startRadius));
    }

    if (progressionStage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(progressionStage));
    }

    Start = start;
    End = end;
    StartTangent = startTangent;
    EndTangent = endTangent;
    StartNormal = startNormal;
    EndNormal = endNormal;
    CrossTangent = crossTangent;
    StartRadius = startRadius;
    EndRadius = endRadius;
    NormalizedDistanceSafeFromDither = normalizedDistanceSafeFromDither;
    StyleTransitionDitherWidth = styleTransitionDitherWidth;
    BorderWidth = borderWidth;
    NormalizedLineDirection = normalizedLineDirection;
    LineLength = lineLength;
    Style = style;
    ProgressionStage = progressionStage;
    CurveLine = curveLine;
  }
}
