using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Evaluates tile, wall, liquid, height, and neighbor conditions over explicit facts.
/// </summary>
public static class WorldGenerationTileWallConditionStateQuery
{
  private const int Wildcard = -1;
  private const int CardinalDirectionCount = 4;

  private static readonly ImmutableArray<TilePosition> DirectionOffsets =
    ImmutableArray.Create(
      new TilePosition(0, -1),
      new TilePosition(1, 0),
      new TilePosition(-1, 0),
      new TilePosition(0, 1),
      new TilePosition(-1, -1),
      new TilePosition(1, -1),
      new TilePosition(-1, 1),
      new TilePosition(1, 1));

  public enum ConditionKind : byte
  {
    OnlyWalls,
    OnlyTiles,
    IsTouching,
    NotTouching,
    IsTouchingAir,
    SkipTiles,
    HasLiquid,
    NoLiquid,
    SkipWalls,
    IsAboveHeight,
    IsBelowHeight,
  }

  public enum ConditionFailureReason : byte
  {
    None,
    CenterMissing,
    TypeMismatch,
    WallMismatch,
    MatchingContact,
    ContactNotFound,
    NoAirContact,
    LiquidMismatch,
    HeightMismatch,
    OutOfWorldNeighbor,
    CompoundConditionFailed,
  }

  public readonly record struct ConditionResult(
    bool Satisfied,
    ConditionFailureReason FailureReason);

  public readonly record struct TileCellSnapshot(
    bool Exists,
    bool IsActive,
    ushort TileType,
    ushort WallType,
    int LiquidType,
    int LiquidLevel);

  /// <summary>
  /// Owns a defensive immutable center and neighbor fact snapshot.
  /// </summary>
  public sealed class TileNeighborhoodSnapshot
  {
    private readonly FrozenDictionary<TilePosition, TileCellSnapshot> _cells;

    public TileNeighborhoodSnapshot(
      WorldGenerationConditionsAndSearchesQuery.TileWorldBounds bounds,
      TilePosition origin,
      TileCellSnapshot center,
      IReadOnlyDictionary<TilePosition, TileCellSnapshot> neighbors)
    {
      ArgumentNullException.ThrowIfNull(neighbors);
      if (!bounds.Contains(origin))
      {
        throw new ArgumentOutOfRangeException(nameof(origin));
      }

      Dictionary<TilePosition, TileCellSnapshot> copy = new()
      {
        [origin] = center,
      };
      foreach (KeyValuePair<TilePosition, TileCellSnapshot> entry in neighbors)
      {
        if (bounds.Contains(entry.Key))
        {
          copy[entry.Key] = entry.Value;
        }
      }

      Bounds = bounds;
      Origin = origin;
      _cells = copy.ToFrozenDictionary();
    }

    public WorldGenerationConditionsAndSearchesQuery.TileWorldBounds Bounds { get; }

    public TilePosition Origin { get; }

    public bool TryRead(
      TilePosition position,
      out TileCellSnapshot cell)
    {
      if (!Bounds.Contains(position))
      {
        cell = default;
        return false;
      }

      return _cells.TryGetValue(position, out cell);
    }

    public bool TryReadNeighbor(
      TilePosition offset,
      out TileCellSnapshot cell)
    {
      long x = (long)Origin.X + offset.X;
      long y = (long)Origin.Y + offset.Y;
      if (x < int.MinValue || x > int.MaxValue ||
        y < int.MinValue || y > int.MaxValue)
      {
        cell = default;
        return false;
      }

      return TryRead(new TilePosition((int)x, (int)y), out cell);
    }
  }

  /// <summary>
  /// Represents one immutable source-style condition parameter set.
  /// </summary>
  public readonly record struct ConditionDefinition
  {
    private ConditionDefinition(
      ConditionKind kind,
      ImmutableArray<ushort> types,
      int liquidType,
      int liquidLevel,
      int height,
      bool inclusive,
      bool useDiagonals)
    {
      Kind = kind;
      Types = types;
      LiquidType = liquidType;
      LiquidLevel = liquidLevel;
      Height = height;
      Inclusive = inclusive;
      UseDiagonals = useDiagonals;
    }

    public ConditionKind Kind { get; }

    public ImmutableArray<ushort> Types { get; }

    public int LiquidType { get; }

    public int LiquidLevel { get; }

    public int Height { get; }

    public bool Inclusive { get; }

    public bool UseDiagonals { get; }

    internal static ConditionDefinition CreateTypeCondition(
      ConditionKind kind,
      ushort[] types,
      bool useDiagonals = false)
    {
      ArgumentNullException.ThrowIfNull(types);
      return new ConditionDefinition(
        kind,
        ImmutableArray.CreateRange(types),
        Wildcard,
        Wildcard,
        0,
        false,
        useDiagonals);
    }

    internal static ConditionDefinition CreateLiquidCondition(
      ConditionKind kind,
      int liquidLevel,
      int liquidType)
    {
      ValidateLiquidParameter(liquidType, nameof(liquidType));
      ValidateLiquidParameter(liquidLevel, nameof(liquidLevel));
      return new ConditionDefinition(
        kind,
        ImmutableArray<ushort>.Empty,
        liquidType,
        liquidLevel,
        0,
        false,
        false);
    }

    internal static ConditionDefinition CreateHeightCondition(
      ConditionKind kind,
      int height,
      bool inclusive)
    {
      return new ConditionDefinition(
        kind,
        ImmutableArray<ushort>.Empty,
        Wildcard,
        Wildcard,
        height,
        inclusive,
        false);
    }
  }

  /// <summary>
  /// Holds an immutable, caller-ordered compound condition list.
  /// </summary>
  public sealed class ConditionSet
  {
    public ConditionSet(IEnumerable<ConditionDefinition> conditions)
    {
      ArgumentNullException.ThrowIfNull(conditions);
      ImmutableArray<ConditionDefinition>.Builder copy =
        ImmutableArray.CreateBuilder<ConditionDefinition>();
      foreach (ConditionDefinition condition in conditions)
      {
        ValidateCondition(condition);
        copy.Add(condition);
      }

      Conditions = copy.ToImmutable();
    }

    public ImmutableArray<ConditionDefinition> Conditions { get; }
  }

  public static ImmutableArray<TilePosition> Directions => DirectionOffsets;

  public static ConditionDefinition OnlyWallsCondition(params ushort[] types)
  {
    return ConditionDefinition.CreateTypeCondition(
      ConditionKind.OnlyWalls,
      types);
  }

  public static ConditionDefinition OnlyTilesCondition(params ushort[] types)
  {
    return ConditionDefinition.CreateTypeCondition(
      ConditionKind.OnlyTiles,
      types);
  }

  public static ConditionDefinition IsTouchingCondition(
    bool useDiagonals,
    params ushort[] tileIds)
  {
    return ConditionDefinition.CreateTypeCondition(
      ConditionKind.IsTouching,
      tileIds,
      useDiagonals);
  }

  public static ConditionDefinition NotTouchingCondition(
    bool useDiagonals,
    params ushort[] tileIds)
  {
    return ConditionDefinition.CreateTypeCondition(
      ConditionKind.NotTouching,
      tileIds,
      useDiagonals);
  }

  public static ConditionDefinition IsTouchingAirCondition(bool useDiagonals = false)
  {
    return ConditionDefinition.CreateTypeCondition(
      ConditionKind.IsTouchingAir,
      Array.Empty<ushort>(),
      useDiagonals);
  }

  public static ConditionDefinition SkipTilesCondition(params ushort[] types)
  {
    return ConditionDefinition.CreateTypeCondition(
      ConditionKind.SkipTiles,
      types);
  }

  public static ConditionDefinition HasLiquidCondition(
    int liquidLevel = Wildcard,
    int liquidType = Wildcard)
  {
    return ConditionDefinition.CreateLiquidCondition(
      ConditionKind.HasLiquid,
      liquidLevel,
      liquidType);
  }

  public static ConditionDefinition NoLiquidCondition(int liquidType = Wildcard)
  {
    return ConditionDefinition.CreateLiquidCondition(
      ConditionKind.NoLiquid,
      Wildcard,
      liquidType);
  }

  public static ConditionDefinition SkipWallsCondition(params ushort[] types)
  {
    return ConditionDefinition.CreateTypeCondition(
      ConditionKind.SkipWalls,
      types);
  }

  public static ConditionDefinition IsAboveHeightCondition(
    int height,
    bool inclusive = false)
  {
    return ConditionDefinition.CreateHeightCondition(
      ConditionKind.IsAboveHeight,
      height,
      inclusive);
  }

  public static ConditionDefinition IsBelowHeightCondition(
    int height,
    bool inclusive = false)
  {
    return ConditionDefinition.CreateHeightCondition(
      ConditionKind.IsBelowHeight,
      height,
      inclusive);
  }

  public static ConditionSet Compose(params ConditionDefinition[] conditions)
  {
    return new ConditionSet(conditions);
  }

  public static bool OnlyWalls(
    TileNeighborhoodSnapshot snapshot,
    params ushort[] types)
  {
    return Evaluate(snapshot, OnlyWallsCondition(types)).Satisfied;
  }

  public static bool OnlyTiles(
    TileNeighborhoodSnapshot snapshot,
    params ushort[] types)
  {
    return Evaluate(snapshot, OnlyTilesCondition(types)).Satisfied;
  }

  public static bool IsTouching(
    TileNeighborhoodSnapshot snapshot,
    bool useDiagonals,
    params ushort[] tileIds)
  {
    return Evaluate(
      snapshot,
      IsTouchingCondition(useDiagonals, tileIds)).Satisfied;
  }

  public static bool NotTouching(
    TileNeighborhoodSnapshot snapshot,
    bool useDiagonals,
    params ushort[] tileIds)
  {
    return Evaluate(
      snapshot,
      NotTouchingCondition(useDiagonals, tileIds)).Satisfied;
  }

  public static bool IsTouchingAir(
    TileNeighborhoodSnapshot snapshot,
    bool useDiagonals = false)
  {
    return Evaluate(
      snapshot,
      IsTouchingAirCondition(useDiagonals)).Satisfied;
  }

  public static bool SkipTiles(
    TileNeighborhoodSnapshot snapshot,
    params ushort[] types)
  {
    return Evaluate(snapshot, SkipTilesCondition(types)).Satisfied;
  }

  public static bool HasLiquid(
    TileNeighborhoodSnapshot snapshot,
    int liquidLevel = Wildcard,
    int liquidType = Wildcard)
  {
    return Evaluate(
      snapshot,
      HasLiquidCondition(liquidLevel, liquidType)).Satisfied;
  }

  public static bool NoLiquid(
    TileNeighborhoodSnapshot snapshot,
    int liquidType = Wildcard)
  {
    return Evaluate(snapshot, NoLiquidCondition(liquidType)).Satisfied;
  }

  public static bool SkipWalls(
    TileNeighborhoodSnapshot snapshot,
    params ushort[] types)
  {
    return Evaluate(snapshot, SkipWallsCondition(types)).Satisfied;
  }

  public static bool IsAboveHeight(
    TileNeighborhoodSnapshot snapshot,
    int height,
    bool inclusive = false)
  {
    return Evaluate(
      snapshot,
      IsAboveHeightCondition(height, inclusive)).Satisfied;
  }

  public static bool IsBelowHeight(
    TileNeighborhoodSnapshot snapshot,
    int height,
    bool inclusive = false)
  {
    return Evaluate(
      snapshot,
      IsBelowHeightCondition(height, inclusive)).Satisfied;
  }

  public static ConditionResult Evaluate(
    TileNeighborhoodSnapshot snapshot,
    ConditionSet conditions)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(conditions);
    for (int index = 0; index < conditions.Conditions.Length; index++)
    {
      ConditionResult result = Evaluate(snapshot, conditions.Conditions[index]);
      if (!result.Satisfied)
      {
        return new ConditionResult(
          false,
          result.FailureReason == ConditionFailureReason.None
            ? ConditionFailureReason.CompoundConditionFailed
            : result.FailureReason);
      }
    }

    return new ConditionResult(true, ConditionFailureReason.None);
  }

  public static ConditionResult Evaluate(
    TileNeighborhoodSnapshot snapshot,
    ConditionDefinition condition)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ValidateCondition(condition);
    if (!snapshot.TryRead(snapshot.Origin, out TileCellSnapshot center) ||
      !center.Exists)
    {
      return new ConditionResult(false, ConditionFailureReason.CenterMissing);
    }

    return condition.Kind switch
    {
      ConditionKind.OnlyWalls =>
        MatchType(
          center.WallType,
          condition.Types,
          ConditionFailureReason.WallMismatch),
      ConditionKind.OnlyTiles =>
        center.IsActive
          ? MatchType(
            center.TileType,
            condition.Types,
            ConditionFailureReason.TypeMismatch)
          : new ConditionResult(false, ConditionFailureReason.CenterMissing),
      ConditionKind.IsTouching =>
        EvaluateTouching(snapshot, condition, shouldMatch: true),
      ConditionKind.NotTouching =>
        EvaluateTouching(snapshot, condition, shouldMatch: false),
      ConditionKind.IsTouchingAir => EvaluateTouchingAir(snapshot, condition),
      ConditionKind.SkipTiles =>
        !center.IsActive ||
        !Contains(condition.Types, center.TileType)
          ? new ConditionResult(true, ConditionFailureReason.None)
          : new ConditionResult(false, ConditionFailureReason.TypeMismatch),
      ConditionKind.HasLiquid => EvaluateHasLiquid(center, condition),
      ConditionKind.NoLiquid => EvaluateNoLiquid(center, condition),
      ConditionKind.SkipWalls =>
        Contains(condition.Types, center.WallType)
          ? new ConditionResult(false, ConditionFailureReason.WallMismatch)
          : new ConditionResult(true, ConditionFailureReason.None),
      ConditionKind.IsAboveHeight => EvaluateAboveHeight(snapshot, condition),
      ConditionKind.IsBelowHeight => EvaluateBelowHeight(snapshot, condition),
      _ => throw new ArgumentException(
        "The condition kind is not supported.",
        nameof(condition)),
    };
  }

  private static ConditionResult EvaluateTouching(
    TileNeighborhoodSnapshot snapshot,
    ConditionDefinition condition,
    bool shouldMatch)
  {
    bool sawOutOfWorld = false;
    int offsetCount = condition.UseDiagonals
      ? DirectionOffsets.Length
      : CardinalDirectionCount;
    for (int index = 0; index < offsetCount; index++)
    {
      if (!snapshot.TryReadNeighbor(DirectionOffsets[index], out TileCellSnapshot cell))
      {
        sawOutOfWorld = true;
        continue;
      }

      if (cell.Exists && cell.IsActive && Contains(condition.Types, cell.TileType))
      {
        return shouldMatch
          ? new ConditionResult(true, ConditionFailureReason.None)
          : new ConditionResult(false, ConditionFailureReason.MatchingContact);
      }
    }

    if (shouldMatch)
    {
      return new ConditionResult(
        false,
        sawOutOfWorld
          ? ConditionFailureReason.OutOfWorldNeighbor
          : ConditionFailureReason.ContactNotFound);
    }

    return new ConditionResult(true, ConditionFailureReason.None);
  }

  private static ConditionResult EvaluateTouchingAir(
    TileNeighborhoodSnapshot snapshot,
    ConditionDefinition condition)
  {
    int offsetCount = condition.UseDiagonals
      ? DirectionOffsets.Length
      : CardinalDirectionCount;
    for (int index = 0; index < offsetCount; index++)
    {
      if (!snapshot.TryReadNeighbor(DirectionOffsets[index], out TileCellSnapshot cell) ||
        !cell.Exists ||
        !cell.IsActive)
      {
        return new ConditionResult(true, ConditionFailureReason.None);
      }
    }

    return new ConditionResult(false, ConditionFailureReason.NoAirContact);
  }

  private static ConditionResult EvaluateHasLiquid(
    TileCellSnapshot center,
    ConditionDefinition condition)
  {
    bool typeMatches = condition.LiquidType == Wildcard ||
      condition.LiquidType == center.LiquidType;
    bool levelMatches = condition.LiquidLevel == Wildcard
      ? center.LiquidLevel != 0
      : condition.LiquidLevel == center.LiquidLevel;
    return typeMatches && levelMatches
      ? new ConditionResult(true, ConditionFailureReason.None)
      : new ConditionResult(false, ConditionFailureReason.LiquidMismatch);
  }

  private static ConditionResult EvaluateNoLiquid(
    TileCellSnapshot center,
    ConditionDefinition condition)
  {
    bool excluded = center.LiquidLevel > 0 &&
      (condition.LiquidType == Wildcard ||
        condition.LiquidType == center.LiquidType);
    return excluded
      ? new ConditionResult(false, ConditionFailureReason.LiquidMismatch)
      : new ConditionResult(true, ConditionFailureReason.None);
  }

  private static ConditionResult EvaluateAboveHeight(
    TileNeighborhoodSnapshot snapshot,
    ConditionDefinition condition)
  {
    bool satisfied = condition.Inclusive
      ? snapshot.Origin.Y <= condition.Height
      : snapshot.Origin.Y < condition.Height;
    return satisfied
      ? new ConditionResult(true, ConditionFailureReason.None)
      : new ConditionResult(false, ConditionFailureReason.HeightMismatch);
  }

  private static ConditionResult EvaluateBelowHeight(
    TileNeighborhoodSnapshot snapshot,
    ConditionDefinition condition)
  {
    bool satisfied = condition.Inclusive
      ? snapshot.Origin.Y >= condition.Height
      : snapshot.Origin.Y > condition.Height;
    return satisfied
      ? new ConditionResult(true, ConditionFailureReason.None)
      : new ConditionResult(false, ConditionFailureReason.HeightMismatch);
  }

  private static ConditionResult MatchType(
    ushort value,
    ImmutableArray<ushort> types,
    ConditionFailureReason failureReason)
  {
    return Contains(types, value)
      ? new ConditionResult(true, ConditionFailureReason.None)
      : new ConditionResult(false, failureReason);
  }

  private static bool Contains(
    ImmutableArray<ushort> values,
    ushort value)
  {
    for (int index = 0; index < values.Length; index++)
    {
      if (values[index] == value)
      {
        return true;
      }
    }

    return false;
  }

  private static void ValidateCondition(ConditionDefinition condition)
  {
    if (condition.Types.IsDefault)
    {
      throw new ArgumentException(
        "The condition type list must be initialized.",
        nameof(condition));
    }

    switch (condition.Kind)
    {
      case ConditionKind.OnlyWalls:
      case ConditionKind.OnlyTiles:
      case ConditionKind.IsTouching:
      case ConditionKind.NotTouching:
      case ConditionKind.IsTouchingAir:
      case ConditionKind.SkipTiles:
      case ConditionKind.SkipWalls:
        return;
      case ConditionKind.HasLiquid:
      case ConditionKind.NoLiquid:
        ValidateLiquidParameter(condition.LiquidType, nameof(condition.LiquidType));
        ValidateLiquidParameter(condition.LiquidLevel, nameof(condition.LiquidLevel));
        return;
      case ConditionKind.IsAboveHeight:
      case ConditionKind.IsBelowHeight:
        return;
      default:
        throw new ArgumentException(
          "The condition kind is not supported.",
          nameof(condition));
    }
  }

  private static void ValidateLiquidParameter(int value, string parameterName)
  {
    if (value < Wildcard || value > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
