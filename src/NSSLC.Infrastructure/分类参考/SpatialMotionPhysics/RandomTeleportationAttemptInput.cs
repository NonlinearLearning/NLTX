using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public sealed class RandomTeleportationAttemptInput
{
  public RandomTeleportationAttemptInput(
    Vector2 teleporteeSize,
    Vector2 teleporteeVelocity,
    float teleporteeGravityDirection,
    bool mostlySolidFloor,
    bool avoidLava,
    bool avoidAnyLiquid,
    bool avoidHurtTiles,
    bool avoidWalls,
    int attemptsBeforeGivingUp,
    int maximumFallDistanceFromOrignalPoint,
    bool strictRange,
    IReadOnlyList<int> tilesToAvoid,
    int tilesToAvoidRange,
    bool allowSolidTopFloor,
    Func<Vector2, bool>? specializedConditions)
  {
    if (attemptsBeforeGivingUp < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(attemptsBeforeGivingUp));
    }

    TeleporteeSize = teleporteeSize;
    TeleporteeVelocity = teleporteeVelocity;
    TeleporteeGravityDirection = teleporteeGravityDirection;
    MostlySolidFloor = mostlySolidFloor;
    AvoidLava = avoidLava;
    AvoidAnyLiquid = avoidAnyLiquid;
    AvoidHurtTiles = avoidHurtTiles;
    AvoidWalls = avoidWalls;
    AttemptsBeforeGivingUp = attemptsBeforeGivingUp;
    MaximumFallDistanceFromOrignalPoint = maximumFallDistanceFromOrignalPoint;
    StrictRange = strictRange;
    TilesToAvoid = tilesToAvoid?.ToArray()
      ?? throw new ArgumentNullException(nameof(tilesToAvoid));
    TilesToAvoidRange = tilesToAvoidRange;
    AllowSolidTopFloor = allowSolidTopFloor;
    SpecializedConditions = specializedConditions;
  }

  public Vector2 TeleporteeSize { get; }

  public Vector2 TeleporteeVelocity { get; }

  public float TeleporteeGravityDirection { get; }

  public bool MostlySolidFloor { get; }

  public bool AvoidLava { get; }

  public bool AvoidAnyLiquid { get; }

  public bool AvoidHurtTiles { get; }

  public bool AvoidWalls { get; }

  public int AttemptsBeforeGivingUp { get; }

  public int MaximumFallDistanceFromOrignalPoint { get; }

  public bool StrictRange { get; }

  public IReadOnlyList<int> TilesToAvoid { get; }

  public int TilesToAvoidRange { get; }

  public bool AllowSolidTopFloor { get; }

  public Func<Vector2, bool>? SpecializedConditions { get; }
}
