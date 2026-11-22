using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.Liquid;
using Terraria.Movement;
using Terraria.Physics;

namespace Terraria.World;

/// <summary>
/// Small authoritative world owner used by NSSLC's standalone ECS runtime.
/// </summary>
/// <remarks>
/// The owner combines the world metadata and dense tile store without introducing a second
/// scheduler. It implements the interaction and spatial query ports, so the same revision guards
/// tile requests and movement reads. Persistence, network publication, drops, slopes and
/// multi-tile rules remain explicit adapters outside this minimal owner.
/// </remarks>
public sealed class WorldRuntime :
    IWorldInteractionWorld,
    IWorldTileReader,
    ISpatialWorldQuery,
    ISpatialWorldRevisionSource,
    IWorldTileGrid,
    IWorldProjectionSource
{
  private readonly object _gate = new();
  private readonly Dictionary<WorldTileCoordinate, WorldMutation> _pendingMutations = [];
  private readonly LiquidSimulation _liquid;
  private readonly LiquidTileStorageBinding _liquidStorage;

  public WorldRuntime(
      int width,
      int height,
      int tileSize = WorldRuntimeStateComponent.DefaultTileSize)
      : this(
          new WorldRuntimeStateComponent(width, height, tileSize),
          new WorldTileStorageComponent(width, height))
  {
  }

  public WorldRuntime(
      WorldRuntimeStateComponent state,
      WorldTileStorageComponent tiles)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(tiles);
    if (state.Width != tiles.Width || state.Height != tiles.Height)
    {
      throw new ArgumentException(
          "The world state and tile storage dimensions must match.", nameof(tiles));
    }

    State = state;
    Tiles = tiles;
    _liquid = new LiquidSimulation(tiles.Count);
    _liquidStorage = new LiquidTileStorageBinding(state, tiles);
  }

  public WorldRuntimeStateComponent State { get; }

  /// <summary>Edge length of one tile in world units.</summary>
  public float TileSize => State.TileSize;

  public WorldTileStorageComponent Tiles { get; }

  public ulong WorldEpoch
  {
    get
    {
      lock (_gate)
      {
        return State.WorldEpoch;
      }
    }
  }

  public long CommitRevision
  {
    get
    {
      lock (_gate)
      {
        return State.Revision;
      }
    }
  }

  public long LastTick
  {
    get
    {
      lock (_gate)
      {
        return State.LastTick;
      }
    }
  }

  public SpatialWorldRevision Revision
  {
    get
    {
      lock (_gate)
      {
        return State.SpatialRevision;
      }
    }
  }

  SpatialWorldRevision ISpatialWorldRevisionSource.Current => Revision;

  /// <summary>Advances world time and revision from the host-supplied ECS tick.</summary>
  public WorldTickResult AdvanceTick(long tick, float deltaTime)
  {
    lock (_gate)
    {
      SpatialWorldRevision current = CurrentRevisionUnsafe();
      return State.TryAdvance(tick, deltaTime, out string? failure)
          ? WorldTickResult.Success(tick, CurrentRevisionUnsafe())
          : WorldTickResult.Rejected(tick, current, failure!);
    }
  }

  /// <summary>
  /// Replaces the dense tile payload as one world-load operation and starts a new world epoch.
  /// </summary>
  public WorldInteractionResult Load(IReadOnlyList<WorldTileState> tiles)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    if (tiles.Count != Tiles.Count)
    {
      return WorldInteractionResult.Rejected(
          "The world tile payload does not match the configured dimensions.");
    }

    WorldTileState[] candidate = tiles.ToArray();
    for (int index = 0; index < candidate.Length; index++)
    {
      if (!IsValidTile(candidate[index]))
      {
        return WorldInteractionResult.Rejected(
            $"The world tile payload contains invalid facts at index {index}.");
      }
    }

    lock (_gate)
    {
      if (!State.TryReload(out string? failure))
      {
        return WorldInteractionResult.Rejected(failure!);
      }

      Tiles.Replace(candidate);

      _pendingMutations.Clear();
      _liquid.Reset();
      _liquid.SeedUnsettled(_liquidStorage);

      return WorldInteractionResult.Success();
    }
  }

  /// <summary>Applies a validated setup or generation tile write through the world owner.</summary>
  public WorldInteractionResult TrySetTile(
      WorldTileCoordinate coordinate,
      WorldTileState tile)
  {
    if (!IsValidTile(tile))
    {
      return WorldInteractionResult.Rejected("The supplied tile facts are invalid.");
    }

    lock (_gate)
    {
      if (!Tiles.Contains(coordinate))
      {
        return WorldInteractionResult.Rejected("The tile coordinate is outside the world.");
      }

      if (Tiles.TryRead(coordinate, out WorldTileState current) && current == tile)
      {
        return WorldInteractionResult.Success();
      }

      if (!State.TryAdvanceRevision(out string? failure))
      {
        return WorldInteractionResult.Rejected(failure!);
      }

      Tiles.TryWrite(coordinate, tile);
      RecordMutationUnsafe(coordinate, current, tile);
      EnqueueLiquidNeighborsUnsafe(coordinate, urgent: true);
      return WorldInteractionResult.Success();
    }
  }

  public bool TryReadTile(WorldTileCoordinate coordinate, out WorldTileState tile)
  {
    lock (_gate)
    {
      return Tiles.TryRead(coordinate, out tile);
    }
  }

  public WorldInteractionResult TryBreakTile(WorldTileCoordinate coordinate)
  {
    lock (_gate)
    {
      if (!Tiles.TryRead(coordinate, out WorldTileState current))
      {
        return WorldInteractionResult.Rejected("The tile coordinate is outside the world.");
      }

      if (!WorldTileTransitionRules.CanBreak(current))
      {
        return WorldInteractionResult.Rejected("The target tile is already empty.");
      }

      if (!State.TryAdvanceRevision(out string? failure))
      {
        return WorldInteractionResult.Rejected(failure!);
      }

      WorldTileState next = WorldTileTransitionRules.Broken(current);
      Tiles.TryWrite(coordinate, next);
      int dropItemType = WorldTileDropRules.TryGetDrop(current, out int itemType)
          ? itemType
          : 0;
      RecordMutationUnsafe(coordinate, current, next, dropItemType);
      EnqueueLiquidNeighborsUnsafe(coordinate, urgent: true);
      return WorldInteractionResult.Success();
    }
  }

  public WorldInteractionResult TryPlaceTile(
      WorldTileCoordinate coordinate,
      ushort tileType,
      byte style)
  {
    if (tileType == 0)
    {
      return WorldInteractionResult.Rejected("A placed tile must have a nonzero type.");
    }

    lock (_gate)
    {
      if (!Tiles.TryRead(coordinate, out WorldTileState current))
      {
        return WorldInteractionResult.Rejected("The tile coordinate is outside the world.");
      }

      if (!WorldTileTransitionRules.CanPlace(current))
      {
        return WorldInteractionResult.Rejected("The target tile is occupied.");
      }

      if (!State.TryAdvanceRevision(out string? failure))
      {
        return WorldInteractionResult.Rejected(failure!);
      }

      WorldTileState next = WorldTileTransitionRules.Placed(current, tileType, style);
      Tiles.TryWrite(coordinate, next);
      RecordMutationUnsafe(coordinate, current, next);
      EnqueueLiquidNeighborsUnsafe(coordinate, urgent: true);
      return WorldInteractionResult.Success();
    }
  }

  /// <summary>Applies one bounded liquid fact update through the world owner.</summary>
  public WorldInteractionResult TrySetLiquid(
      WorldTileCoordinate coordinate,
      LiquidContactFlags liquid,
      byte amount)
  {
    if (!LiquidBlockingRules.TryNormalize(liquid, amount, out liquid, out string? failure))
    {
      return WorldInteractionResult.Rejected(failure!);
    }

    lock (_gate)
    {
      if (!Tiles.TryRead(coordinate, out WorldTileState current))
      {
        return WorldInteractionResult.Rejected("The tile coordinate is outside the world.");
      }

      if (current.Liquid == liquid && current.LiquidAmount == amount)
      {
        return WorldInteractionResult.Success();
      }

      if (!State.TryAdvanceRevision(out string? revisionFailure))
      {
        return WorldInteractionResult.Rejected(revisionFailure!);
      }

      WorldTileState next = WorldTileTransitionRules.WithLiquid(current, liquid, amount);
      Tiles.TryWrite(coordinate, next);
      RecordMutationUnsafe(coordinate, current, next);
      EnqueueLiquidNeighborsUnsafe(coordinate, urgent: true);
      return WorldInteractionResult.Success();
    }
  }

  /// <summary>Runs a bounded authoritative liquid step and records its tile changes.</summary>
  public int AdvanceLiquidSimulation(
      int operationBudget = LiquidSimulation.DefaultOperationBudget)
  {
    if (operationBudget <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(operationBudget));
    }

    lock (_gate)
    {
      // Reserve the revision before mutating tiles: the original algorithm refused to write at
      // all when the revision was exhausted, so a failed step must leave tiles untouched.
      if (!CanAdvanceRevision() && HasQueuedLiquid())
      {
        return 0;
      }

      List<LiquidTileChange> changed = [];
      LiquidStepOutcome outcome = _liquid.Step(_liquidStorage, operationBudget, changed);
      if (!outcome.Changed)
      {
        return 0;
      }

      if (!State.TryAdvanceRevision(out _))
      {
        return 0;
      }

      for (int index = 0; index < changed.Count; index++)
      {
        LiquidTileChange change = changed[index];
        RecordMutationUnsafe(change.Coordinate, change.Previous, change.Current);
      }

      return outcome.Operations;
    }
  }

  private bool CanAdvanceRevision() =>
      WorldTimeAdvanceSolver.CanAdvanceRevision(State.Revision);

  private bool HasQueuedLiquid() => _liquid.HasQueuedWork;

  /// <summary>
  /// Returns and clears up to <paramref name="maximum"/> pending mutations. Anything above the
  /// bound stays queued and is merged with later changes to the same coordinate.
  /// </summary>
  public IReadOnlyList<WorldMutation> DrainPendingMutations(int maximum)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximum);
    lock (_gate)
    {
      if (_pendingMutations.Count == 0)
      {
        return Array.Empty<WorldMutation>();
      }

      int take = Math.Min(maximum, _pendingMutations.Count);
      var mutations = new List<WorldMutation>(take);
      foreach (WorldMutation mutation in _pendingMutations.Values)
      {
        mutations.Add(mutation);
        if (mutations.Count == take)
        {
          break;
        }
      }

      for (int index = 0; index < mutations.Count; index++)
      {
        _pendingMutations.Remove(mutations[index].Coordinate);
      }

      return mutations;
    }
  }

  /// <summary>Queues a coordinate and its neighbours for the next liquid pass.</summary>
  private void EnqueueLiquidNeighborsUnsafe(WorldTileCoordinate coordinate, bool urgent = false)
  {
    _liquid.EnqueueWithNeighbors(_liquidStorage, coordinate, urgent);
  }

  private void RecordMutationUnsafe(
      WorldTileCoordinate coordinate,
      WorldTileState previous,
      WorldTileState current,
      int dropItemType = 0)
  {
    if (!WorldMutationRules.IsRecordable(previous, current))
    {
      return;
    }

    if (_pendingMutations.TryGetValue(coordinate, out WorldMutation existing))
    {
      if (WorldMutationRules.Cancels(existing, current))
      {
        _pendingMutations.Remove(coordinate);
        return;
      }

      _pendingMutations[coordinate] = WorldMutationRules.Merge(existing, current, dropItemType);
      return;
    }

    _pendingMutations.Add(
        coordinate,
        WorldMutationRules.Create(coordinate, previous, current, dropItemType));
  }

  public SpatialTileMotionResult Move(
      Vector2 position,
      Vector2 velocity,
      Vector2 colliderSize,
      TileCollisionCapabilityComponent capability,
      GravityDirection gravityDirection)
  {
    lock (_gate)
    {
      return WorldTileMotionSolver.Move(
          this,
          position,
          velocity,
          colliderSize,
          capability,
          gravityDirection);
    }
  }

  public SpatialLiquidContact GetLiquidContact(
      Vector2 position,
      Vector2 colliderSize,
      LiquidInteractionCapabilityComponent capability)
  {
    lock (_gate)
    {
      return LiquidSimulation.GetContact(
          _liquidStorage,
          State.TileSize,
          position,
          colliderSize,
          capability);
    }
  }

  private SpatialWorldRevision CurrentRevisionUnsafe()
  {
    return State.SpatialRevision;
  }

  private static bool IsValidTile(WorldTileState tile)
  {
    return tile.IsValid;
  }

  int IWorldTileGrid.Width => State.Width;

  int IWorldTileGrid.Height => State.Height;

  int IWorldTileGrid.TileSize => State.TileSize;

  WorldTileState IWorldTileGrid.ReadTile(WorldTileCoordinate coordinate)
  {
    return Tiles.ReadUnchecked(coordinate);
  }
}
