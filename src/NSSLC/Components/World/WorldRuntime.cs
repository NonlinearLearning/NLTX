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
    ISpatialWorldQuery,
    ISpatialWorldRevisionSource
{
  private const int CollisionSearchIterations = 12;
  private readonly object _gate = new();

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
  }

  public WorldRuntimeStateComponent State { get; }

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
        return new SpatialWorldRevision(State.WorldEpoch, State.Revision);
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

      if (!current.Active)
      {
        return WorldInteractionResult.Rejected("The target tile is already empty.");
      }

      if (!State.TryAdvanceRevision(out string? failure))
      {
        return WorldInteractionResult.Rejected(failure!);
      }

      Tiles.TryWrite(
          coordinate,
          current with { Active = false, Type = 0, FrameX = 0, FrameY = 0 });
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

      if (current.Active)
      {
        return WorldInteractionResult.Rejected("The target tile is occupied.");
      }

      if (!State.TryAdvanceRevision(out string? failure))
      {
        return WorldInteractionResult.Rejected(failure!);
      }

      Tiles.TryWrite(
          coordinate,
          current with { Active = true, Type = tileType, FrameX = 0, FrameY = 0, Style = style });
      return WorldInteractionResult.Success();
    }
  }

  /// <summary>Applies one bounded liquid fact update through the world owner.</summary>
  public WorldInteractionResult TrySetLiquid(
      WorldTileCoordinate coordinate,
      LiquidContactFlags liquid,
      byte amount)
  {
    const LiquidContactFlags knownLiquids =
        LiquidContactFlags.Water |
        LiquidContactFlags.Honey |
        LiquidContactFlags.Lava |
        LiquidContactFlags.Shimmer;
    byte liquidValue = (byte)liquid;
    if ((liquid & ~knownLiquids) != LiquidContactFlags.None ||
        (liquidValue != 0 && (liquidValue & (liquidValue - 1)) != 0))
    {
      return WorldInteractionResult.Rejected("The supplied liquid kind is not supported.");
    }

    if (liquid == LiquidContactFlags.None && amount > 0)
    {
      return WorldInteractionResult.Rejected("A nonzero liquid amount requires a liquid kind.");
    }

    if (amount == 0)
    {
      liquid = LiquidContactFlags.None;
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

      if (!State.TryAdvanceRevision(out string? failure))
      {
        return WorldInteractionResult.Rejected(failure!);
      }

      Tiles.TryWrite(coordinate, current with { Liquid = liquid, LiquidAmount = amount });
      return WorldInteractionResult.Success();
    }
  }

  public SpatialTileMotionResult Move(
      Vector2 position,
      Vector2 velocity,
      Vector2 colliderSize,
      TileCollisionCapabilityComponent capability,
      GravityDirection gravityDirection)
  {
    if (!IsFinite(position) || !IsFinite(velocity) || !IsFinite(colliderSize) ||
        colliderSize.X <= 0f || colliderSize.Y <= 0f)
    {
      return new SpatialTileMotionResult(position, Vector2.Zero, false, false);
    }

    if (!capability.Enabled)
    {
      return new SpatialTileMotionResult(position + velocity, velocity, false, false);
    }

    lock (_gate)
    {
      Vector2 current = ClampToWorld(position, colliderSize);
      bool collidedX = false;
      bool collidedY = false;

      if (velocity.X != 0f)
      {
        float resolvedX = MoveAxis(
            current,
            velocity.X,
            colliderSize,
            horizontal: true,
            capability,
            out collidedX);
        current.X = resolvedX;
      }

      if (velocity.Y != 0f)
      {
        float resolvedY = MoveAxis(
            current,
            velocity.Y,
            colliderSize,
            horizontal: false,
            capability,
            out collidedY);
        current.Y = resolvedY;
      }

      Vector2 resultingVelocity = new(
          collidedX ? 0f : velocity.X,
          collidedY ? 0f : velocity.Y);
      return new SpatialTileMotionResult(current, resultingVelocity, collidedX, collidedY);
    }
  }

  public SpatialLiquidContact GetLiquidContact(
      Vector2 position,
      Vector2 colliderSize,
      LiquidInteractionCapabilityComponent capability)
  {
    if (!capability.Enabled || capability.AffectedLiquids == LiquidContactFlags.None ||
        !IsFinite(position) || !IsFinite(colliderSize) ||
        colliderSize.X <= 0f || colliderSize.Y <= 0f)
    {
      return new SpatialLiquidContact(LiquidContactFlags.None, 0);
    }

    double worldWidth = (double)State.Width * State.TileSize;
    double worldHeight = (double)State.Height * State.TileSize;
    if ((double)position.X + colliderSize.X <= 0d ||
        (double)position.Y + colliderSize.Y <= 0d ||
        position.X >= worldWidth || position.Y >= worldHeight)
    {
      return new SpatialLiquidContact(LiquidContactFlags.None, 0);
    }

    lock (_gate)
    {
      TileRange range = GetTileRange(position, colliderSize);
      LiquidContactFlags flags = LiquidContactFlags.None;
      byte amount = 0;
      for (int y = range.MinY; y <= range.MaxY; y++)
      {
        for (int x = range.MinX; x <= range.MaxX; x++)
        {
          WorldTileState tile = Tiles.ReadUnchecked(new WorldTileCoordinate(x, y));
          if (!tile.HasLiquid)
          {
            continue;
          }

          float tileTop = y * State.TileSize;
          float liquidTop = tileTop + State.TileSize -
              State.TileSize * tile.LiquidAmount / byte.MaxValue;
          bool overlapsLiquid = position.Y < tileTop + State.TileSize &&
              position.Y + colliderSize.Y > liquidTop &&
              position.X < (x + 1) * State.TileSize &&
              position.X + colliderSize.X > x * State.TileSize;
          if (!overlapsLiquid)
          {
            continue;
          }

          LiquidContactFlags accepted = tile.Liquid & capability.AffectedLiquids;
          flags |= accepted;
          if (accepted != LiquidContactFlags.None)
          {
            amount = Math.Max(amount, tile.LiquidAmount);
          }
        }
      }

      return new SpatialLiquidContact(flags, amount);
    }
  }

  private float MoveAxis(
      Vector2 current,
      float delta,
      Vector2 colliderSize,
      bool horizontal,
      TileCollisionCapabilityComponent capability,
      out bool collided)
  {
    float value = horizontal ? current.X : current.Y;
    float remaining = delta;
    float maximumStep = Math.Max(1f, State.TileSize);
    collided = false;
    while (MathF.Abs(remaining) > 0.0001f)
    {
      float step = MathF.CopySign(MathF.Min(MathF.Abs(remaining), maximumStep), remaining);
      float target = value + step;
      Vector2 position = current;
      if (horizontal)
      {
        position.X = value;
      }
      else
      {
        position.Y = value;
      }

      float resolved = ResolveAxis(position, target, colliderSize, horizontal, capability);
      if (MathF.Abs(resolved - target) > 0.0001f)
      {
        value = resolved;
        collided = true;
        break;
      }

      value = resolved;
      remaining -= step;
      if (horizontal)
      {
        current.X = value;
      }
      else
      {
        current.Y = value;
      }
    }

    return value;
  }

  private float ResolveAxis(
      Vector2 current,
      float target,
      Vector2 colliderSize,
      bool horizontal,
      TileCollisionCapabilityComponent capability)
  {
    Vector2 currentPosition = current;
    if (horizontal)
    {
      currentPosition.X = current.X;
    }
    else
    {
      currentPosition.Y = current.Y;
    }

    if (Collides(currentPosition, colliderSize, capability))
    {
      return horizontal ? current.X : current.Y;
    }

    Vector2 targetPosition = currentPosition;
    if (horizontal)
    {
      targetPosition.X = target;
    }
    else
    {
      targetPosition.Y = target;
    }

    if (!Collides(targetPosition, colliderSize, capability))
    {
      return target;
    }

    float safe = horizontal ? current.X : current.Y;
    float blocked = target;
    for (int iteration = 0; iteration < CollisionSearchIterations; iteration++)
    {
      float middle = safe + (blocked - safe) / 2f;
      Vector2 probe = currentPosition;
      if (horizontal)
      {
        probe.X = middle;
      }
      else
      {
        probe.Y = middle;
      }

      if (Collides(probe, colliderSize, capability))
      {
        blocked = middle;
      }
      else
      {
        safe = middle;
      }
    }

    return safe;
  }

  private bool Collides(
      Vector2 position,
      Vector2 colliderSize,
      TileCollisionCapabilityComponent capability)
  {
    float worldWidth = (float)((double)State.Width * State.TileSize);
    float worldHeight = (float)((double)State.Height * State.TileSize);
    if (position.X < 0f || position.Y < 0f ||
        position.X + colliderSize.X > worldWidth ||
        position.Y + colliderSize.Y > worldHeight)
    {
      return true;
    }

    TileRange range = GetTileRange(position, colliderSize);
    for (int y = range.MinY; y <= range.MaxY; y++)
    {
      for (int x = range.MinX; x <= range.MaxX; x++)
      {
        WorldTileState tile = Tiles.ReadUnchecked(new WorldTileCoordinate(x, y));
        if (tile.Active)
        {
          return true;
        }
      }
    }

    return false;
  }

  private Vector2 ClampToWorld(Vector2 position, Vector2 colliderSize)
  {
    float worldWidth = (float)((double)State.Width * State.TileSize);
    float worldHeight = (float)((double)State.Height * State.TileSize);
    return new Vector2(
        Math.Clamp(position.X, 0f, Math.Max(0f, worldWidth - colliderSize.X)),
        Math.Clamp(position.Y, 0f, Math.Max(0f, worldHeight - colliderSize.Y)));
  }

  private TileRange GetTileRange(Vector2 position, Vector2 colliderSize)
  {
    int minX = (int)Math.Clamp(
        Math.Floor((double)position.X / State.TileSize), 0, State.Width - 1);
    int minY = (int)Math.Clamp(
        Math.Floor((double)position.Y / State.TileSize), 0, State.Height - 1);
    int maxX = (int)Math.Clamp(
        Math.Floor(((double)position.X + colliderSize.X - 0.0001d) / State.TileSize),
        0,
        State.Width - 1);
    int maxY = (int)Math.Clamp(
        Math.Floor(((double)position.Y + colliderSize.Y - 0.0001d) / State.TileSize),
        0,
        State.Height - 1);
    return new TileRange(minX, maxX, minY, maxY);
  }

  private SpatialWorldRevision CurrentRevisionUnsafe()
  {
    return new SpatialWorldRevision(State.WorldEpoch, State.Revision);
  }

  private static bool IsValidTile(WorldTileState tile)
  {
    return tile.IsValid;
  }

  private static bool IsFinite(Vector2 value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }

  private readonly record struct TileRange(int MinX, int MaxX, int MinY, int MaxY);
}
