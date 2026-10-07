namespace Terraria.Items;

public sealed class WorldItemComponent
{
  public WorldItemComponent(
    long spawnedAtTick,
    long? despawnAtTick = null,
    bool isInstanced = false,
    bool isBeingGrabbed = false,
    bool isOnConveyor = false,
    long revision = 0,
    bool isShimmered = false,
    float shimmerTime = 0.0f,
    byte enemyGrabDelayTime = 0)
  {
    if (spawnedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(spawnedAtTick));
    }

    if (despawnAtTick.HasValue && despawnAtTick.Value < spawnedAtTick)
    {
      throw new ArgumentOutOfRangeException(nameof(despawnAtTick));
    }

    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    if (!float.IsFinite(shimmerTime) || shimmerTime < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(shimmerTime));
    }

    SpawnedAtTick = spawnedAtTick;
    DespawnAtTick = despawnAtTick;
    IsInstanced = isInstanced;
    IsBeingGrabbed = isBeingGrabbed;
    IsOnConveyor = isOnConveyor;
    Revision = revision;
    IsShimmered = isShimmered;
    ShimmerTime = shimmerTime;
    EnemyGrabDelayTime = enemyGrabDelayTime;
  }

  public long SpawnedAtTick;
  public long? DespawnAtTick;
  public bool IsInstanced;
  public bool IsBeingGrabbed;
  public bool IsOnConveyor;
  public long Revision;
  public bool IsShimmered;
  public float ShimmerTime;
  public byte EnemyGrabDelayTime;

  public bool IsExpiredAt(long currentTick) =>
    DespawnAtTick.HasValue && currentTick >= DespawnAtTick.Value;

  public long RemainingTicksAt(long currentTick) =>
    DespawnAtTick.HasValue
      ? Math.Max(0, DespawnAtTick.Value - currentTick)
      : long.MaxValue;
}
