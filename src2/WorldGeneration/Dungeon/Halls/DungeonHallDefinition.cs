using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Halls;

public sealed class DungeonHallDefinition
{
  public DungeonHallDefinition(
    int hallId,
    int hallType,
    int randomSeed,
    DungeonStyleMaterialDefinition styleData)
  {
    if (hallId < 0 || hallType < 0 || randomSeed < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(hallId));
    }

    HallId = hallId;
    HallType = hallType;
    RandomSeed = randomSeed;
    StyleData = styleData ?? throw new ArgumentNullException(nameof(styleData));
  }

  public int HallId { get; }

  public int HallType { get; }

  public int RandomSeed { get; }

  public DungeonStyleMaterialDefinition StyleData { get; }

  public bool CrackedBrick { get; init; }

  public float CrackedBrickChance { get; init; }

  public bool PlaceOverProtectedBricks { get; init; }

  public float ZigzagChance { get; init; }

  public bool ForceStyleForDoorsAndPlatforms { get; init; }

  public bool CarveOnly { get; init; }

  public DungeonTilePoint StartPosition { get; init; }

  public DungeonTilePoint EndPosition { get; init; }

  public DungeonTilePoint StartDirection { get; init; }

  public DungeonTilePoint EndDirection { get; init; }

  public int OverrideInnerBoundsSize { get; init; }

  public int OverrideOuterBoundsSize { get; init; }

  public float Magnitude { get; init; }

  public int Iterations { get; init; }

  public bool FlipSine { get; init; }

  public float OverrideStrength { get; init; }

  public int OverrideSteps { get; init; }

  public bool ForceHorizontal { get; init; }

  public float OverrideInteriorToExteriorRatio { get; init; }

  public float MaxDistFromLine { get; init; }

  public float PointVariance { get; init; }

  public int InnerBoundsSize { get; init; }

  public int OuterBoundsSize { get; init; }

  public float Gradient { get; init; }

  public bool IsEntranceHall { get; init; }
}
