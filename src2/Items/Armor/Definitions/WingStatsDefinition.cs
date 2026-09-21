namespace Terraria.NonAuthoritative.ContentDefinitions;

public sealed class WingStatsDefinition
{
  internal WingStatsDefinition(
    int wingType,
    int flyTime,
    float speedOverride,
    float accelerationMultiplier,
    bool hasDownHoverStats,
    float downHoverSpeedOverride,
    float downHoverAccelerationMultiplier)
  {
    if (wingType < 0 || flyTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(wingType));
    }

    WingType = wingType;
    FlyTime = flyTime;
    AccRunSpeedOverride = speedOverride;
    AccRunAccelerationMult = accelerationMultiplier;
    HasDownHoverStats = hasDownHoverStats;
    DownHoverSpeedOverride = downHoverSpeedOverride;
    DownHoverAccelerationMult = downHoverAccelerationMultiplier;
  }

  public static WingStatsDefinition Default =>
    new(0, 100, -1f, 1f, false, -1f, 1f);

  public int WingType { get; }

  public int FlyTime { get; }

  public float AccRunSpeedOverride { get; }

  public float AccRunAccelerationMult { get; }

  public bool HasDownHoverStats { get; }

  public float DownHoverSpeedOverride { get; }

  public float DownHoverAccelerationMult { get; }
}

public static class WingStatsRegistrationSystem
{
  public static WingStatsDefinition Create(
    int wingType,
    int flyTime,
    float speedOverride,
    float accelerationMultiplier,
    bool hasDownHoverStats,
    float downHoverSpeedOverride,
    float downHoverAccelerationMultiplier)
  {
    return new WingStatsDefinition(
      wingType,
      flyTime,
      speedOverride,
      accelerationMultiplier,
      hasDownHoverStats,
      downHoverSpeedOverride,
      downHoverAccelerationMultiplier);
  }
}

public static class WingStatsQuery
{
  public static float GetDownHoverSpeed(WingStatsDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    return definition.HasDownHoverStats ? definition.DownHoverSpeedOverride : -1f;
  }
}

public interface IWingStatsProjection
{
  void Write(WingStatsDefinition definition);
}
