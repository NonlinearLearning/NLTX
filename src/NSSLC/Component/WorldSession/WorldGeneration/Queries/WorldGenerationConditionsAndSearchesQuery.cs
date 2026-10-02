using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Evaluates world-generation conditions and searches against an explicit tile snapshot.
/// </summary>
public static class WorldGenerationConditionsAndSearchesQuery
{
  private const byte LavaLiquidType = 1;
  private const int SolidConditionFluff = 10;
  private const ushort MysticSnakeTileType = 504;

  /// <summary>
  /// Value-equivalent replacement for the legacy mutable Point sentinel.
  /// </summary>
  public static TilePosition NotFound { get; } = new(int.MaxValue, int.MaxValue);

  /// <summary>
  /// Compatibility spelling for callers that still use the source declaration name.
  /// </summary>
  public static TilePosition NOT_FOUND => NotFound;

  public enum SearchTerminationReason : byte
  {
    Found,
    NoMatch,
    ReachedWorldBoundary,
  }

  public readonly record struct SearchResult(
    TilePosition Position,
    SearchTerminationReason TerminationReason)
  {
    public bool Found => TerminationReason == SearchTerminationReason.Found;
  }

  /// <summary>
  /// Uses inclusive lower bounds and exclusive upper bounds for tile coordinates.
  /// </summary>
  public readonly record struct TileWorldBounds
  {
    public TileWorldBounds(int width, int height)
    {
      ArgumentOutOfRangeException.ThrowIfNegative(width);
      ArgumentOutOfRangeException.ThrowIfNegative(height);
      Width = width;
      Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public bool Contains(TilePosition position, int fluff = 0)
    {
      return
        (long)position.X >= fluff &&
        (long)position.X < (long)Width - fluff &&
        (long)position.Y >= fluff &&
        (long)position.Y < (long)Height - fluff;
    }
  }

  /// <summary>
  /// Immutable tile facts required by the conditions in this boundary.
  /// </summary>
  public readonly record struct TileSnapshot(
    bool Exists,
    bool IsActive,
    ushort TileType,
    bool IsSolid,
    byte LiquidAmount,
    byte LiquidType,
    bool IsTileCut);

  /// <summary>
  /// Read-only seam for a caller-owned immutable world/tile view.
  /// </summary>
  public interface ITileSnapshotReader
  {
    TileWorldBounds Bounds { get; }

    bool TryRead(TilePosition position, out TileSnapshot snapshot);
  }

  /// <summary>
  /// Defensive, immutable snapshot implementation used by generation callers and tests.
  /// </summary>
  public sealed class TileWorldSnapshot : ITileSnapshotReader
  {
    private readonly FrozenDictionary<TilePosition, TileSnapshot> _tiles;

    public TileWorldSnapshot(
      TileWorldBounds bounds,
      IReadOnlyDictionary<TilePosition, TileSnapshot> tiles)
    {
      ArgumentNullException.ThrowIfNull(tiles);
      Bounds = bounds;
      _tiles = tiles.ToFrozenDictionary();
    }

    public TileWorldBounds Bounds { get; }

    public bool TryRead(TilePosition position, out TileSnapshot snapshot)
    {
      if (!Bounds.Contains(position))
      {
        snapshot = default;
        return false;
      }

      return _tiles.TryGetValue(position, out snapshot);
    }
  }

  public abstract record Condition
  {
    internal abstract bool Evaluate(
      ITileSnapshotReader snapshot,
      TilePosition position);

    public sealed record IsTile : Condition
    {
      public IsTile(params ushort[] types)
      {
        ArgumentNullException.ThrowIfNull(types);
        Types = ImmutableArray.CreateRange(types);
      }

      public ImmutableArray<ushort> Types { get; }

      internal override bool Evaluate(
        ITileSnapshotReader snapshot,
        TilePosition position)
      {
        if (!snapshot.Bounds.Contains(position) ||
          !snapshot.TryRead(position, out TileSnapshot tile) ||
          !tile.Exists ||
          !tile.IsActive)
        {
          return false;
        }

        for (int index = 0; index < Types.Length; index++)
        {
          if (Types[index] == tile.TileType)
          {
            return true;
          }
        }

        return false;
      }
    }

    public sealed record Continue : Condition
    {
      internal override bool Evaluate(
        ITileSnapshotReader snapshot,
        TilePosition position)
      {
        return false;
      }
    }

    public sealed record BoolCheck : Condition
    {
      public BoolCheck(bool theBool)
      {
        TheBool = theBool;
      }

      public bool TheBool { get; }

      internal override bool Evaluate(
        ITileSnapshotReader snapshot,
        TilePosition position)
      {
        return TheBool;
      }
    }

    public sealed record MysticSnake : Condition
    {
      internal override bool Evaluate(
        ITileSnapshotReader snapshot,
        TilePosition position)
      {
        if (!snapshot.Bounds.Contains(position) ||
          !snapshot.TryRead(position, out TileSnapshot tile) ||
          !tile.Exists ||
          !tile.IsActive)
        {
          return false;
        }

        return !tile.IsTileCut && tile.TileType != MysticSnakeTileType;
      }
    }

    public sealed record InWorld : Condition
    {
      public InWorld(int fluff)
      {
        Fluff = fluff;
      }

      public int Fluff { get; }

      internal override bool Evaluate(
        ITileSnapshotReader snapshot,
        TilePosition position)
      {
        return snapshot.Bounds.Contains(position, Fluff);
      }
    }

    public sealed record IsSolid : Condition
    {
      internal override bool Evaluate(
        ITileSnapshotReader snapshot,
        TilePosition position)
      {
        if (!snapshot.Bounds.Contains(position, SolidConditionFluff) ||
          !snapshot.TryRead(position, out TileSnapshot tile))
        {
          return false;
        }

        return tile.Exists && tile.IsActive && tile.IsSolid;
      }
    }

    public sealed record HasLava : Condition
    {
      internal override bool Evaluate(
        ITileSnapshotReader snapshot,
        TilePosition position)
      {
        if (!snapshot.TryRead(position, out TileSnapshot tile))
        {
          return false;
        }

        return tile.Exists &&
          tile.LiquidAmount > 0 &&
          tile.LiquidType == LavaLiquidType;
      }
    }

    public sealed record NotNull : Condition
    {
      internal override bool Evaluate(
        ITileSnapshotReader snapshot,
        TilePosition position)
      {
        return snapshot.TryRead(position, out TileSnapshot tile) && tile.Exists;
      }
    }
  }

  public static class Conditions
  {
    public static Condition IsTile(params ushort[] types)
    {
      return new Condition.IsTile(types);
    }

    public static Condition Continue()
    {
      return new Condition.Continue();
    }

    public static Condition BoolCheck(bool theBool)
    {
      return new Condition.BoolCheck(theBool);
    }

    public static Condition MysticSnake()
    {
      return new Condition.MysticSnake();
    }

    public static Condition InWorld(int fluff)
    {
      return new Condition.InWorld(fluff);
    }

    public static Condition IsSolid()
    {
      return new Condition.IsSolid();
    }

    public static Condition HasLava()
    {
      return new Condition.HasLava();
    }

    public static Condition NotNull()
    {
      return new Condition.NotNull();
    }
  }

  public abstract class SearchDefinition
  {
    protected SearchDefinition(
      ImmutableArray<Condition> conditions,
      bool requiresAll = true)
    {
      if (conditions.IsDefault)
      {
        throw new ArgumentException(
          "Search conditions must be initialized.",
          nameof(conditions));
      }

      Conditions = conditions;
      RequiresAll = requiresAll;
    }

    public ImmutableArray<Condition> Conditions { get; }

    /// <summary>
    /// Gets whether every condition must match. False preserves the legacy
    /// any-condition search mode exposed by GenSearch.RequireAll(false).
    /// </summary>
    public bool RequiresAll { get; }

    public SearchDefinition WithConditions(params Condition[] conditions)
    {
      return WithConditionsCore(CopyConditions(conditions), RequiresAll);
    }

    public SearchDefinition RequireAll(bool mode)
    {
      return WithConditionsCore(Conditions, mode);
    }

    internal abstract SearchDefinition WithConditionsCore(
      ImmutableArray<Condition> conditions,
      bool requiresAll);

    internal abstract SearchResult Execute(
      ITileSnapshotReader snapshot,
      TilePosition origin);
  }

  public static class Searches
  {
    public sealed class Left : SearchDefinition
    {
      public Left(int maxDistance, params Condition[] conditions)
        : this(maxDistance, CopyConditions(conditions), requiresAll: true)
      {
      }

      private Left(
        int maxDistance,
        ImmutableArray<Condition> conditions,
        bool requiresAll)
        : base(conditions, requiresAll)
      {
        ArgumentOutOfRangeException.ThrowIfNegative(maxDistance);
        MaxDistance = maxDistance;
      }

      public int MaxDistance { get; }

      internal override SearchDefinition WithConditionsCore(
        ImmutableArray<Condition> conditions,
        bool requiresAll)
      {
        return new Left(MaxDistance, conditions, requiresAll);
      }

      internal override SearchResult Execute(
        ITileSnapshotReader snapshot,
        TilePosition origin)
      {
        return ScanDirectional(
          snapshot,
          origin,
          MaxDistance,
          horizontalStep: -1,
          verticalStep: 0,
          Conditions,
          RequiresAll);
      }
    }

    public sealed class Right : SearchDefinition
    {
      public Right(int maxDistance, params Condition[] conditions)
        : this(maxDistance, CopyConditions(conditions), requiresAll: true)
      {
      }

      private Right(
        int maxDistance,
        ImmutableArray<Condition> conditions,
        bool requiresAll)
        : base(conditions, requiresAll)
      {
        ArgumentOutOfRangeException.ThrowIfNegative(maxDistance);
        MaxDistance = maxDistance;
      }

      public int MaxDistance { get; }

      internal override SearchDefinition WithConditionsCore(
        ImmutableArray<Condition> conditions,
        bool requiresAll)
      {
        return new Right(MaxDistance, conditions, requiresAll);
      }

      internal override SearchResult Execute(
        ITileSnapshotReader snapshot,
        TilePosition origin)
      {
        return ScanDirectional(
          snapshot,
          origin,
          MaxDistance,
          horizontalStep: 1,
          verticalStep: 0,
          Conditions,
          RequiresAll);
      }
    }

    public sealed class Down : SearchDefinition
    {
      public Down(int maxDistance, params Condition[] conditions)
        : this(maxDistance, CopyConditions(conditions), requiresAll: true)
      {
      }

      private Down(
        int maxDistance,
        ImmutableArray<Condition> conditions,
        bool requiresAll)
        : base(conditions, requiresAll)
      {
        ArgumentOutOfRangeException.ThrowIfNegative(maxDistance);
        MaxDistance = maxDistance;
      }

      public int MaxDistance { get; }

      internal override SearchDefinition WithConditionsCore(
        ImmutableArray<Condition> conditions,
        bool requiresAll)
      {
        return new Down(MaxDistance, conditions, requiresAll);
      }

      internal override SearchResult Execute(
        ITileSnapshotReader snapshot,
        TilePosition origin)
      {
        return ScanDirectional(
          snapshot,
          origin,
          MaxDistance,
          horizontalStep: 0,
          verticalStep: 1,
          Conditions,
          RequiresAll);
      }
    }

    public sealed class Up : SearchDefinition
    {
      public Up(int maxDistance, params Condition[] conditions)
        : this(maxDistance, CopyConditions(conditions), requiresAll: true)
      {
      }

      private Up(
        int maxDistance,
        ImmutableArray<Condition> conditions,
        bool requiresAll)
        : base(conditions, requiresAll)
      {
        ArgumentOutOfRangeException.ThrowIfNegative(maxDistance);
        MaxDistance = maxDistance;
      }

      public int MaxDistance { get; }

      internal override SearchDefinition WithConditionsCore(
        ImmutableArray<Condition> conditions,
        bool requiresAll)
      {
        return new Up(MaxDistance, conditions, requiresAll);
      }

      internal override SearchResult Execute(
        ITileSnapshotReader snapshot,
        TilePosition origin)
      {
        return ScanDirectional(
          snapshot,
          origin,
          MaxDistance,
          horizontalStep: 0,
          verticalStep: -1,
          Conditions,
          RequiresAll);
      }
    }

    public sealed class Rectangle : SearchDefinition
    {
      public Rectangle(
        int width,
        int height,
        params Condition[] conditions)
        : this(width, height, CopyConditions(conditions), requiresAll: true)
      {
      }

      private Rectangle(
        int width,
        int height,
        ImmutableArray<Condition> conditions,
        bool requiresAll)
        : base(conditions, requiresAll)
      {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        Width = width;
        Height = height;
      }

      public int Width { get; }

      public int Height { get; }

      internal override SearchDefinition WithConditionsCore(
        ImmutableArray<Condition> conditions,
        bool requiresAll)
      {
        return new Rectangle(Width, Height, conditions, requiresAll);
      }

      internal override SearchResult Execute(
        ITileSnapshotReader snapshot,
        TilePosition origin)
      {
        return ScanRectangle(
          snapshot,
          origin,
          Width,
          Height,
          Conditions,
          RequiresAll);
      }
    }

    public static SearchDefinition Chain(
      SearchDefinition search,
      params Condition[] conditions)
    {
      ArgumentNullException.ThrowIfNull(search);
      return search.WithConditions(conditions);
    }
  }

  public static bool EvaluateCondition(
    ITileSnapshotReader snapshot,
    TilePosition position,
    Condition condition)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(condition);
    return condition.Evaluate(snapshot, position);
  }

  public static SearchResult Search(
    ITileSnapshotReader snapshot,
    TilePosition origin,
    SearchDefinition search)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(search);
    return search.Execute(snapshot, origin);
  }

  public static SearchResult Find(
    ITileSnapshotReader snapshot,
    TilePosition origin,
    SearchDefinition search)
  {
    return Search(snapshot, origin, search);
  }

  private static ImmutableArray<Condition> CopyConditions(
    Condition[] conditions)
  {
    ArgumentNullException.ThrowIfNull(conditions);
    ImmutableArray<Condition>.Builder copy =
      ImmutableArray.CreateBuilder<Condition>(conditions.Length);
    for (int index = 0; index < conditions.Length; index++)
    {
      copy.Add(conditions[index] ?? throw new ArgumentException(
        "A search condition cannot be null.",
        nameof(conditions)));
    }

    return copy.MoveToImmutable();
  }

  private static SearchResult ScanDirectional(
    ITileSnapshotReader snapshot,
    TilePosition origin,
    int maxDistance,
    int horizontalStep,
    int verticalStep,
    ImmutableArray<Condition> conditions,
    bool requiresAll)
  {
    bool reachedWorldBoundary = false;
    for (int distance = 0; distance < maxDistance; distance++)
    {
      long x = (long)origin.X + (long)horizontalStep * distance;
      long y = (long)origin.Y + (long)verticalStep * distance;
      if (!TryGetInBoundsPosition(snapshot, x, y, out TilePosition candidate))
      {
        reachedWorldBoundary = true;
        continue;
      }

      if (Matches(snapshot, candidate, conditions, requiresAll))
      {
        return new SearchResult(candidate, SearchTerminationReason.Found);
      }
    }

    return new SearchResult(
      NotFound,
      reachedWorldBoundary
        ? SearchTerminationReason.ReachedWorldBoundary
        : SearchTerminationReason.NoMatch);
  }

  private static SearchResult ScanRectangle(
    ITileSnapshotReader snapshot,
    TilePosition origin,
    int width,
    int height,
    ImmutableArray<Condition> conditions,
    bool requiresAll)
  {
    bool reachedWorldBoundary = false;
    for (int xOffset = 0; xOffset < width; xOffset++)
    {
      for (int yOffset = 0; yOffset < height; yOffset++)
      {
        long x = (long)origin.X + xOffset;
        long y = (long)origin.Y + yOffset;
        if (!TryGetInBoundsPosition(snapshot, x, y, out TilePosition candidate))
        {
          reachedWorldBoundary = true;
          continue;
        }

        if (Matches(snapshot, candidate, conditions, requiresAll))
        {
          return new SearchResult(candidate, SearchTerminationReason.Found);
        }
      }
    }

    return new SearchResult(
      NotFound,
      reachedWorldBoundary
        ? SearchTerminationReason.ReachedWorldBoundary
        : SearchTerminationReason.NoMatch);
  }

  private static bool TryGetInBoundsPosition(
    ITileSnapshotReader snapshot,
    long x,
    long y,
    out TilePosition position)
  {
    if (x < int.MinValue || x > int.MaxValue ||
      y < int.MinValue || y > int.MaxValue)
    {
      position = default;
      return false;
    }

    position = new TilePosition((int)x, (int)y);
    return snapshot.Bounds.Contains(position);
  }

  private static bool Matches(
    ITileSnapshotReader snapshot,
    TilePosition position,
    ImmutableArray<Condition> conditions,
    bool requiresAll)
  {
    for (int index = 0; index < conditions.Length; index++)
    {
      bool conditionResult = conditions[index].Evaluate(snapshot, position);
      if (requiresAll != conditionResult)
      {
        return !requiresAll;
      }
    }

    return requiresAll;
  }
}
