namespace MinimalImplementation.Server.World;

/// <summary>
/// Explicit inputs for the bounded Version4 liquid scheduler.
/// </summary>
public sealed class LiquidSimulationOptions
{
  public LiquidSimulationOptions(
    int underworldStartY,
    int lavaDelaySteps = 5,
    int honeyDelaySteps = 10,
    int panicBufferThreshold = 45_000,
    int panicTriggerSteps = 3_600,
    int stuckTriggerSteps = 10_000)
  {
    if (underworldStartY < 0 || lavaDelaySteps < 0 || honeyDelaySteps < 0 ||
        panicBufferThreshold <= 0 || panicBufferThreshold > LiquidSimulation.MaximumBufferedEntries ||
        panicTriggerSteps <= 0 || stuckTriggerSteps <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(underworldStartY));
    }

    UnderworldStartY = underworldStartY;
    LavaDelaySteps = lavaDelaySteps;
    HoneyDelaySteps = honeyDelaySteps;
    PanicBufferThreshold = panicBufferThreshold;
    PanicTriggerSteps = panicTriggerSteps;
    StuckTriggerSteps = stuckTriggerSteps;
  }

  public int UnderworldStartY { get; }

  public int LavaDelaySteps { get; }

  public int HoneyDelaySteps { get; }

  public int PanicBufferThreshold { get; }

  public int PanicTriggerSteps { get; }

  public int StuckTriggerSteps { get; }

  public static LiquidSimulationOptions ForGeometry(WorldGeometry geometry)
  {
    if (geometry.Width <= 0 || geometry.Height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(geometry));
    }

    return new LiquidSimulationOptions(Math.Max(0, geometry.Height - 200));
  }
}

/// <summary>Observable scheduler state used by recovery diagnostics.</summary>
public readonly record struct LiquidSimulationStatus(
  int PendingCount,
  int ActiveCount,
  int BufferedCount,
  bool QueueCapacityExceeded,
  bool QuickFall,
  bool QuickSettle,
  bool Stuck,
  int StuckCount,
  bool PanicMode,
  int PanicCounter,
  int PanicY);

/// <summary>Records a liquid pair that produced a merge tile during a staged step.</summary>
public readonly record struct LiquidMergeAction(
  TileCoordinate Coordinate,
  ushort ResultTileType,
  LiquidType IncomingType,
  LiquidType ExistingType,
  int ConsumedLiquidAmount);

/// <summary>Maps Version4 liquid pairs to their merge tile and resulting liquid type.</summary>
public static class LiquidMergeRules
{
  public const int MinimumMergeAmount = 24;

  public static bool TryResolve(
    LiquidType sourceType,
    IReadOnlyCollection<LiquidType> conflictingTypes,
    out ushort resultTileType,
    out LiquidType resultLiquidType,
    out LiquidType existingType)
  {
    ArgumentNullException.ThrowIfNull(conflictingTypes);
    resultTileType = 0;
    resultLiquidType = sourceType;
    existingType = sourceType;

    // Liquid.GetLiquidMergeTypes checks water, lava, honey and shimmer in this order;
    // later matches overwrite the earlier result. Preserve that deterministic precedence.
    foreach (LiquidType candidate in new[]
             { LiquidType.Water, LiquidType.Lava, LiquidType.Honey, LiquidType.Shimmer })
    {
      if (candidate == sourceType || !conflictingTypes.Contains(candidate))
      {
        continue;
      }

      if (!TryResolvePair(sourceType, candidate, out ushort tileType))
      {
        continue;
      }

      resultTileType = tileType;
      resultLiquidType = candidate;
      existingType = candidate;
    }

    return resultTileType != 0;
  }

  private static bool TryResolvePair(
    LiquidType first,
    LiquidType second,
    out ushort tileType)
  {
    tileType = (first, second) switch
    {
      (LiquidType.Water, LiquidType.Lava) or
        (LiquidType.Lava, LiquidType.Water) => 56,
      (LiquidType.Water, LiquidType.Honey) or
        (LiquidType.Honey, LiquidType.Water) => 229,
      (LiquidType.Lava, LiquidType.Honey) or
        (LiquidType.Honey, LiquidType.Lava) => 230,
      (LiquidType.Water, LiquidType.Shimmer) or
        (LiquidType.Shimmer, LiquidType.Water) or
        (LiquidType.Lava, LiquidType.Shimmer) or
        (LiquidType.Shimmer, LiquidType.Lava) or
        (LiquidType.Honey, LiquidType.Shimmer) or
        (LiquidType.Shimmer, LiquidType.Honey) => 659,
      _ => (ushort)0
    };
    return tileType != 0;
  }
}
