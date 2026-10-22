using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Rules;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Prepares one version-bound 60 Hz world-item state transition.</summary>
public sealed class WorldItemTickCoordinator
{
  private const int ItemWidth = 16;
  private const int ItemHeight = 16;
  private const int MaximumAge = 2147483547;
  private const int WorldMargin = 20;

  private readonly WorldItemStore _items;
  private readonly WorldItemReservationStore _reservations;
  private readonly WorldItemTransactionCoordinator _transactions;
  private readonly WorldOwner _world;

  public WorldItemTickCoordinator(
    WorldItemStore items, WorldItemTransactionCoordinator transactions, WorldOwner world)
  {
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(transactions);
    ArgumentNullException.ThrowIfNull(world);
    if (!ReferenceEquals(items, transactions.Items))
    {
      throw new ArgumentException("The tick and transaction must use the same item store.");
    }
    _items = items;
    _reservations = transactions.Reservations;
    _transactions = transactions;
    _world = world;
  }

  /// <summary>
  /// Prepares every item in one world scope from the Host-supplied detached captures.
  /// The caller commits the returned transaction on the exclusive ServerLoop.
  /// </summary>
  public OwnerCommitResult PrepareTick(
    WorldScopeId scope,
    ulong worldEpoch,
    WorldHeaderCapture header,
    WorldSpatialSnapshot spatial,
    out WorldItemTransactionCoordinator.PreparedTransaction? transaction)
  {
    ArgumentNullException.ThrowIfNull(header);
    ArgumentNullException.ThrowIfNull(spatial);
    transaction = null;
    if (!scope.IsValid || worldEpoch == 0)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Forbidden, "A world-item tick requires a valid scope and world epoch.");
    }

    if (header.WorldEpoch != worldEpoch
      || spatial.WorldEpoch != worldEpoch
      || spatial.Version != header.Version
      || !ReferenceEquals(spatial.Header, header.Header)
      || spatial.Header.Geometry != header.Header.Geometry
      || !ReferenceEquals(spatial.Facts, header.Facts))
    {
      return Conflict("The detached world-item captures do not share one world revision.");
    }

    lock (_items.CommitGate.SyncRoot)
    {
      if (!_world.IsReady)
      {
        return Unavailable("The authoritative world is not ready for item simulation.");
      }

      if (!_world.IsCurrentWorldRevision(header.WorldEpoch, header.Version))
      {
        return Conflict("The supplied world capture is no longer current.");
      }

      WorldItemSnapshot[] before = _items.CaptureWorld(scope).ToArray();
      WorldItemReservation[] claims = _reservations.CaptureWorld(scope).ToArray();
      if (before.Length == 0)
      {
        return OwnerCommitResult.Applied(header.Version);
      }

      WorldGeometry geometry = header.Header.Geometry;
      if (geometry.Width <= WorldMargin * 2 || geometry.Height <= WorldMargin * 2)
      {
        return Unavailable(
          "The world dimensions do not support the captured item collision rules.");
      }

      OwnerCommitResult unsupported = ValidateKnownReadiness(before, geometry, spatial);
      if (unsupported.Status != OwnerCommitStatus.Applied)
      {
        return unsupported;
      }

      foreach (WorldItemSnapshot item in before)
      {
        WorldItemRuntime runtime = item.Runtime ?? new WorldItemRuntime();
        if (runtime.BeingGrabbed)
        {
          continue;
        }

        if (!TryGetCaptureBounds(item, geometry, out WorldSectionBounds bounds))
        {
          return Unavailable("The item collision capture exceeds the bounded spatial snapshot.");
        }

        if (!Covers(spatial.Bounds, bounds))
        {
          return Unavailable("The item collision capture exceeds the bounded spatial snapshot.");
        }
      }

      if (!_world.IsCurrentWorldRevision(header.WorldEpoch, header.Version))
      {
        return Conflict("The world changed before item motion was prepared.");
      }

      var pass = new WorldItemOrderedPass(before, claims);
      var shimmerEffects = new WorldItemMutation?[before.Length];
      for (int index = 0; index < before.Length; index++)
      {
        WorldItemSnapshot? item = pass.Get(index);
        if (item is null)
        {
          continue;
        }

        OwnerCommitResult result;
        WorldItemMutation? mutation;
        try
        {
          result = PrepareMutation(
            index, item, pass, spatial, header, out mutation, out shimmerEffects[index]);
        }
        catch (ArgumentOutOfRangeException)
        {
          return Unavailable("The detached tile snapshot cannot answer the item collision query.");
        }
        catch (InvalidOperationException)
        {
          return Unavailable("The detached tile snapshot cannot answer the item collision query.");
        }

        if (result.Status != OwnerCommitStatus.Applied)
        {
          return result;
        }

        pass.Set(index, mutation);
      }

      var changes = new List<WorldItemChange>(before.Length);
      for (int index = 0; index < before.Length; index++)
      {
        WorldItemSnapshot item = before[index];
        WorldItemSnapshot? after = pass.Get(index);
        WorldItemMutation? mutation = after is null ? null : WorldItemMutation.FromSnapshot(after);
        if (mutation is null || mutation != WorldItemMutation.FromSnapshot(item)
          || shimmerEffects[index] is not null)
        {
          changes.Add(new WorldItemChange(
            item.Handle, item.Revision, mutation, shimmerEffects[index],
            Broadcast: mutation is null || mutation.Item != item.Item ||
              mutation.IsShimmering != item.IsShimmering || shimmerEffects[index] is not null));
        }
      }

      if (changes.Count == 0)
      {
        return OwnerCommitResult.Applied(header.Version);
      }

      var worldGuard = new TickReadGuard(
        _world, header, spatial, _items, _reservations, scope, before, claims);
      return _transactions.Prepare(
        changes, [], [], null, [worldGuard], out transaction);
    }
  }

  /// <summary>
  /// Advances only reservation and grab-delay counters when motion could not be prepared.
  /// The Host must not call this after a successful motion commit in the same frame.
  /// </summary>
  public OwnerCommitResult PrepareOwnershipTimerTick(
      WorldScopeId scope, ulong worldEpoch, WorldHeaderCapture header,
      out WorldItemTransactionCoordinator.PreparedTransaction? transaction) {
    ArgumentNullException.ThrowIfNull(header);
    transaction = null;
    if (!scope.IsValid || worldEpoch == 0 || header.WorldEpoch != worldEpoch) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "Ownership timers require a valid scope and matching world epoch.");
    }

    lock (_items.CommitGate.SyncRoot) {
      if (!_world.IsReady) {
        return Unavailable("The authoritative world is not ready for ownership timers.");
      }
      if (!_world.IsCurrentWorldRevision(header.WorldEpoch, header.Version)) {
        return Conflict("The ownership-timer world capture is no longer current.");
      }

      WorldItemSnapshot[] before = _items.CaptureWorld(scope).ToArray();
      WorldItemReservation[] claims = _reservations.CaptureWorld(scope).ToArray();
      var changes = new List<WorldItemChange>();
      foreach (WorldItemSnapshot item in before) {
        WorldItemRuntime previous = item.Runtime ?? new WorldItemRuntime();
        WorldItemRuntime runtime = previous.AdvanceOwnershipTimers() with {
          NoGrabDelay = Math.Max(0, previous.NoGrabDelay - 1)
        };
        if (runtime != previous) {
          WorldItemMutation mutation = WorldItemMutation.FromSnapshot(item) with {
            Runtime = runtime
          };
          changes.Add(new WorldItemChange(item.Handle, item.Revision, mutation, Broadcast: false));
        }
      }
      if (changes.Count == 0) {
        return OwnerCommitResult.Applied(header.Version);
      }

      var worldGuard = new TickReadGuard(
        _world, header, null, _items, _reservations, scope, before, claims);
      return _transactions.Prepare(changes, [], [], null, [worldGuard], out transaction);
    }
  }

  /// <summary>Rejects the legacy path because a tick must use one detached world capture.</summary>
  [Obsolete("Supply the Host-captured WorldHeaderCapture and WorldSpatialSnapshot.")]
  public OwnerCommitResult PrepareTick(
    WorldScopeId scope,
    ulong worldEpoch,
    out WorldItemTransactionCoordinator.PreparedTransaction? transaction)
  {
    transaction = null;
    return Unavailable(
      "A world-item tick requires Host-supplied detached world captures.");
  }

  public OwnerCommitResult ValidateTick(
    WorldItemTransactionCoordinator.PreparedTransaction transaction)
  {
    ArgumentNullException.ThrowIfNull(transaction);
    return transaction.Validate();
  }

  public WorldItemTransactionResult CommitTick(
    WorldItemTransactionCoordinator.PreparedTransaction transaction)
  {
    ArgumentNullException.ThrowIfNull(transaction);
    return transaction.Commit();
  }

  private static OwnerCommitResult ValidateKnownReadiness(
    IReadOnlyList<WorldItemSnapshot> items,
    WorldGeometry geometry,
    WorldSpatialSnapshot spatial)
  {
    foreach (WorldItemSnapshot item in items)
    {
      WorldItemRuntime runtime = item.Runtime ?? new WorldItemRuntime();
      if (runtime.BeingGrabbed)
      {
        continue;
      }

      if (!TryGetCaptureBounds(item, geometry, out _))
      {
        return Unavailable("The item collision capture exceeds the bounded spatial snapshot.");
      }

      if (!HasQueryCoverage(spatial, item.Position))
      {
        return Unavailable("The detached tile snapshot does not cover the item collision query.");
      }
    }

    return OwnerCommitResult.Applied(CommitVersion.Initial);
  }

  private static OwnerCommitResult PrepareMutation(
    int index,
    WorldItemSnapshot item,
    WorldItemOrderedPass pass,
    WorldSpatialSnapshot? tiles,
    WorldHeaderCapture header,
    out WorldItemMutation? mutation,
    out WorldItemMutation? shimmerEffect)
  {
    mutation = null;
    shimmerEffect = null;
    WorldItemRuntime previous = item.Runtime ?? new WorldItemRuntime();
    WorldItemRuntime ownershipRuntime = previous.AdvanceOwnershipTimers();
    Vector2 position = item.Position;
    Vector2 velocity = item.Velocity;
    bool wet = previous.Wet;
    bool lavaWet = previous.LavaWet;
    bool honeyWet = previous.HoneyWet;
    bool shimmerWet = previous.ShimmerWet;
    int enemyPickupDelay = item.EnemyPickupDelay;
    float shimmerTime = item.ShimmerTime;

    if (!previous.BeingGrabbed)
    {
      if (tiles is null)
      {
        return Unavailable("The detached item collision snapshot is missing.");
      }

      bool serverOwned = !pass.IsReserved(item.Handle);
      if (item.Item.Type == 205 && serverOwned)
      {
        return Unavailable(
          "The rain, surface-wall and random facts for item 205 are not captured.");
      }

      float gravity = 0.1f;
      float maximumFallSpeed = 7f;
      float wetDisplacement = 0.5f;
      if (previous.ShimmerWet)
      {
        gravity = 0.065f;
        maximumFallSpeed = 4f;
        wetDisplacement = 0.375f;
      }
      else if (previous.HoneyWet)
      {
        gravity = 0.05f;
        maximumFallSpeed = 3f;
        wetDisplacement = 0.25f;
      }
      else if (previous.Wet)
      {
        gravity = 0.08f;
        maximumFallSpeed = 5f;
      }

      Vector2 wetVelocity = item.Velocity * wetDisplacement;
      if (item.IsShimmering)
      {
        gravity = 0f;
        if (!TryGetShimmerLiquidNearCenter(tiles, position, out bool shimmerNearCenter))
        {
          return Unavailable("The detached tile snapshot does not cover the shimmer motion query.");
        }

        if (previous.ShimmerWet || shimmerNearCenter)
        {
          if (velocity.Y > -4f)
          {
            velocity.Y -= 0.05f;
          }
        }
        else
        {
          velocity.Y *= 0.9f;
        }
      }

      if (previous.ShimmerWet && !item.IsShimmering)
      {
        var coordinate = new TileCoordinate(
          (int)((position.X + ItemWidth / 2f) / 16f), (int)(position.Y / 16f - 1f));
        bool immersed = false;
        if (tiles.Header.Geometry.Contains(coordinate))
        {
          if (!tiles.Bounds.Contains(coordinate))
          {
            return Unavailable("The detached tile snapshot does not cover shimmer immersion.");
          }

          WorldTile tile = tiles.GetTile(coordinate);
          immersed = tile.LiquidAmount > 0 && tile.LiquidType == LiquidType.Shimmer;
        }

        if (immersed)
        {
          if (!WorldItemShimmerFacts.TryGetTransform(
            item.Item.Type, header.Header.MoonPhase, out int targetType))
          {
            return Unavailable(
              "The shimmer path requires captured progression, recipe, coin or NPC facts.");
          }

          shimmerTime += 0.01f;
          if (!serverOwned)
          {
            shimmerTime = Math.Min(shimmerTime, 1f);
          }
          else if (shimmerTime > 0.9f)
          {
            // SetDefaults resets prefix/favorite and lavaWet, preserving WorldItem age,
            // noGrabDelay and honeyWet. Common GetShimmered effects then set wet flags.
            item = item with {
              Item = new InventoryItemState(targetType, item.Item.Stack, 0, false),
              IsShimmering = true
            };
            shimmerTime = 1f;
            wet = true;
            lavaWet = false;
            shimmerWet = true;
            velocity *= 0.1f;
            shimmerEffect = new WorldItemMutation(
              item.Item, position, velocity, item.TimeLeft, true, enemyPickupDelay,
              shimmerTime, ownershipRuntime with {
                Wet = true, LavaWet = false, ShimmerWet = true
              });
          }
        }
        else
        {
          shimmerTime = Math.Max(0f, shimmerTime - 0.01f);
        }
      }
      else if (shimmerTime > 0f)
      {
        shimmerTime = Math.Max(0f, shimmerTime - 0.01f);
      }

      if (shimmerTime == 0f)
      {
        WorldItemSnapshot stackingInput = item with { Position = position, Velocity = velocity };
        if (!pass.TryCombine(index, stackingInput, out WorldItemSnapshot combined))
        {
          return Unavailable("Passive world-item stacking requires verified item content facts.");
        }
        item = combined;
        position = combined.Position;
        velocity = combined.Velocity;
      }

      if (enemyPickupDelay > 0)
      {
        enemyPickupDelay--;
      }

      if (enemyPickupDelay == 0 && serverOwned)
      {
        if (item.Item.Type is 89 or 3507)
        {
          return Unavailable(
            "Copper-slime pickup requires captured unlock and Combat/NPC facts.");
        }
        if (ItemContentFacts.IsCoin(item.Item.Type))
        {
          return Unavailable(
            "Monster coin pickup requires effective expert-mode and Combat/NPC facts.");
        }
      }

      if (!item.IsShimmering && IsNoGravityItem(item.Item.Type))
      {
        velocity = ApplyNoGravityDrag(velocity);
      }
      else
      {
        bool repelled = item.IsShimmering
          && pass.ApplyShimmerRepulsion(index, position, ref velocity);
        velocity.Y = Math.Min(velocity.Y + gravity, maximumFallSpeed);
        velocity.X *= 0.95f;
        if (Math.Abs(velocity.X) < 0.1f)
        {
          velocity.X = 0f;
        }
        if (repelled)
        {
          velocity.X *= 0.8f;
        }
      }

      var collision = new WorldItemCollision(tiles);
      if (!HasQueryCoverage(tiles, position))
      {
        return Unavailable("The detached tile snapshot does not cover the conveyor query.");
      }
      _ = collision.ApplyConveyor(ref position, ref velocity, ItemWidth, ItemHeight);
      if (!HasQueryCoverage(tiles, position))
      {
        return Unavailable("The detached tile snapshot does not cover the conveyor query.");
      }

      WorldLiquidContact liquid = tiles.GetLiquidContact(position, ItemWidth, ItemHeight);
      if (liquid.Lava)
      {
        lavaWet = true;
      }
      if (liquid.Honey)
      {
        honeyWet = true;
      }
      if (liquid.Shimmer)
      {
        shimmerWet = true;
      }

      if (liquid.Wet)
      {
        wet = true;
      }
      else if (wet)
      {
        wet = false;
      }

      if (!wet)
      {
        lavaWet = false;
        honeyWet = false;
        shimmerWet = false;
      }

      Vector2 velocityBeforeCollision = velocity;
      if (!HasQueryCoverage(tiles, position))
      {
        return Unavailable("The detached tile snapshot does not cover tile collision.");
      }

      velocity = collision.TileCollision(
        position, velocity, ItemWidth, ItemHeight,
        fallThrough: false, fall2: false, gravDir: 1,
        ignoreAetheriumPlatforms: true, hoik: true);
      if (wet)
      {
        if (velocity.X != velocityBeforeCollision.X)
        {
          wetVelocity.X = velocity.X;
        }
        if (velocity.Y != velocityBeforeCollision.Y)
        {
          wetVelocity.Y = velocity.Y;
        }
      }

      position += wet ? wetVelocity : velocity;
      if (!IsFinite(position) || !IsFinite(velocity)
        || !HasQueryCoverage(tiles, position))
      {
        return Unavailable("The detached tile snapshot does not cover slope correction.");
      }

      Vector4 slope = collision.SlopeCollision(
        position, velocity, ItemWidth, ItemHeight,
        fall: false, ignoreAetheriumPlatforms: true);
      position = new Vector2(slope.X, slope.Y);
      velocity = new Vector2(slope.Z, slope.W);
      if (!IsFinite(position) || !IsFinite(velocity))
      {
        return Unavailable("World-item collision produced a non-finite state.");
      }

      if (lavaWet)
      {
        if (item.Item.Type == 267)
        {
          return Unavailable("Item 267 lava effects require the Combat/NPC owner facts.");
        }
        if (serverOwned && !IsLavaImmuneRegardlessOfRarity(item.Item.Type))
        {
          if (item.Item.Prefix != 0
            || !ItemContentFacts.TryGetUnprefixedRarity(item.Item.Type, out int rarity))
          {
            return Unavailable("Rarity-dependent lava destruction facts are not captured.");
          }

          if (rarity is 0 or -1)
          {
            return OwnerCommitResult.Applied(item.Revision);
          }
        }
      }

      if (!IsInsideWorldMargin(position, header.Header.Geometry))
      {
        if (IsRecoverableImportantItem(item.Item.Type))
        {
          position = new Vector2(
            header.Header.Spawn.X * 16f, header.Header.Spawn.Y * 16f);
          velocity = Vector2.Zero;
        }
        else
        {
          return OwnerCommitResult.Applied(item.Revision);
        }
      }

      if (item.Item.Type == 75 && header.Header.DayTime
        && !header.Header.Facts.RemixWorld && !item.IsShimmering)
      {
        return OwnerCommitResult.Applied(item.Revision);
      }

      if (item.Item.Type == 4143 && previous.Age > 300)
      {
        return OwnerCommitResult.Applied(item.Revision);
      }

      if (item.Item.Type == 3822)
      {
        return Unavailable("The DD2 event state required by item 3822 expiry is not captured.");
      }

      if (item.Item.Type == 74 && serverOwned && item.Item.Stack == 1 && wet
        && (ownershipRuntime.OwnIgnore != 1 || previous.NoGrabDelay > 0))
      {
        return Unavailable("The make-a-wish pickup requires captured player and desert facts.");
      }

    }
    else
    {
      wet = false;
      lavaWet = false;
      honeyWet = false;
      shimmerWet = false;
      position += velocity;
      if (!IsFinite(position))
      {
        return Unavailable("Grabbed-item movement produced a non-finite position.");
      }
    }

    int age = Math.Min(previous.Age, MaximumAge);
    if (age < MaximumAge)
    {
      age++;
    }
    int noGrabDelay = Math.Max(0, previous.NoGrabDelay - 1);
    WorldItemRuntime runtime = ownershipRuntime with {
      Age = age,
      NoGrabDelay = noGrabDelay,
      Wet = wet,
      LavaWet = lavaWet,
      HoneyWet = honeyWet,
      ShimmerWet = shimmerWet,
      BeingGrabbed = false
    };
    mutation = new WorldItemMutation(
      item.Item, position, velocity, item.TimeLeft, item.IsShimmering,
      enemyPickupDelay, shimmerTime, runtime);
    return OwnerCommitResult.Applied(item.Revision);
  }

  public static bool TryGetCaptureBounds(
    WorldItemSnapshot item, WorldGeometry geometry, out WorldSectionBounds bounds)
  {
    bounds = default;
    Vector2 velocity = item.Velocity;
    float extentX = Math.Abs(velocity.X) + 32f;
    float extentY = Math.Abs(velocity.Y) + 48f;
    if (!float.IsFinite(extentX) || !float.IsFinite(extentY))
    {
      return false;
    }

    var positions = new[]
    {
      item.Position,
      item.Position + new Vector2(-extentX, -extentY),
      item.Position + new Vector2(-extentX, extentY),
      item.Position + new Vector2(extentX, -extentY),
      item.Position + new Vector2(extentX, extentY)
    };

    int left = geometry.Width;
    int top = geometry.Height;
    int right = 0;
    int bottom = 0;
    foreach (Vector2 position in positions)
    {
      if (!IsFinite(position))
      {
        return false;
      }

      WorldSectionBounds query;
      try
      {
        query = WorldSpatialSnapshot.GetQueryBounds(
          geometry, position, ItemWidth, ItemHeight);
      }
      catch (ArgumentOutOfRangeException)
      {
        return false;
      }

      left = Math.Min(left, query.OriginX);
      top = Math.Min(top, query.OriginY);
      right = Math.Max(right, query.OriginX + query.Width);
      bottom = Math.Max(bottom, query.OriginY + query.Height);
    }

    long width = (long)right - left;
    long height = (long)bottom - top;
    if (width <= 0 || height <= 0
      || width * height > WorldSpatialSnapshot.MaximumCaptureTiles)
    {
      return false;
    }

    bounds = new WorldSectionBounds(left, top, (int)width, (int)height);
    return true;
  }

  private static bool HasQueryCoverage(WorldSpatialSnapshot tiles, Vector2 position)
  {
    if (!IsFinite(position))
    {
      return false;
    }

    WorldSectionBounds query;
    try
    {
      query = WorldSpatialSnapshot.GetQueryBounds(
        tiles.Header.Geometry, position, ItemWidth, ItemHeight);
    }
    catch (ArgumentOutOfRangeException)
    {
      return false;
    }

    long queryRight = (long)query.OriginX + query.Width;
    long queryBottom = (long)query.OriginY + query.Height;
    long captureRight = (long)tiles.Bounds.OriginX + tiles.Bounds.Width;
    long captureBottom = (long)tiles.Bounds.OriginY + tiles.Bounds.Height;
    return query.OriginX >= tiles.Bounds.OriginX && query.OriginY >= tiles.Bounds.OriginY
      && queryRight <= captureRight && queryBottom <= captureBottom;
  }

  private static bool Covers(WorldSectionBounds outer, WorldSectionBounds inner)
  {
    return inner.OriginX >= outer.OriginX && inner.OriginY >= outer.OriginY
      && (long)inner.OriginX + inner.Width <= (long)outer.OriginX + outer.Width
      && (long)inner.OriginY + inner.Height <= (long)outer.OriginY + outer.Height;
  }

  private static bool TryGetShimmerLiquidNearCenter(
    WorldSpatialSnapshot tiles, Vector2 position, out bool shimmer)
  {
    shimmer = false;
    WorldGeometry geometry = tiles.Header.Geometry;
    int x = (int)((position.X + ItemWidth / 2f) / 16f);
    int y = (int)((position.Y + ItemHeight / 2f) / 16f);
    for (int tileY = y; tileY < y + 2; tileY++)
    {
      var coordinate = new TileCoordinate(x, tileY);
      if (geometry.Contains(coordinate) && !tiles.Bounds.Contains(coordinate))
      {
        return false;
      }

      if (geometry.Contains(coordinate))
      {
        WorldTile tile = tiles.GetTile(coordinate);
        if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidType.Shimmer)
        {
          shimmer = true;
        }
      }
    }

    return true;
  }

  private static Vector2 ApplyNoGravityDrag(Vector2 velocity)
  {
    velocity *= 0.95f;
    if (Math.Abs(velocity.X) < 0.1f)
    {
      velocity.X = 0f;
    }
    if (Math.Abs(velocity.Y) < 0.1f)
    {
      velocity.Y = 0f;
    }

    return velocity;
  }

  private static bool IsNoGravityItem(int type) => type is
    520 or 521 or 575 or 549 or 548 or 547
    or >= 3453 and <= 3459
    or 3580 or 3581 or 4143;

  private static bool IsLavaImmuneRegardlessOfRarity(int type) => type is
    318 or 312 or 173 or 174 or 175 or 4422 or 2701 or 205 or 206 or 207
    or 1128 or 2340 or 2739 or 2492 or 1127 or 85 or 581 or 582;

  private static bool IsRecoverableImportantItem(int type) => type is
    >= 3318 and <= 3332
    or >= 3860 and <= 3862
    or 4782 or 4957 or 5111;

  private static bool IsInsideWorldMargin(Vector2 position, WorldGeometry geometry)
  {
    int tileX = (int)(position.X / 16f);
    int tileY = (int)(position.Y / 16f);
    return tileX >= WorldMargin && tileY >= WorldMargin
      && tileX < geometry.Width - WorldMargin
      && tileY < geometry.Height - WorldMargin;
  }

  private static bool IsFinite(Vector2 value) =>
    float.IsFinite(value.X) && float.IsFinite(value.Y);

  private static OwnerCommitResult Conflict(string detail) => new(
    OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict, detail);

  private static OwnerCommitResult Unavailable(string detail) => new(
    OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);

  private sealed class TickReadGuard(
    WorldOwner world,
    WorldHeaderCapture header,
    WorldSpatialSnapshot? spatial,
    WorldItemStore items,
    WorldItemReservationStore reservations,
    WorldScopeId scope,
    WorldItemSnapshot[] before,
    WorldItemReservation[] claims) : IWorldItemTransactionParticipant
  {
    public OwnerCommitResult Validate()
    {
      if (!world.IsCurrentWorldRevision(header.WorldEpoch, header.Version)
        || spatial is not null && (spatial.WorldEpoch != header.WorldEpoch
          || spatial.Version != header.Version)
        || !items.CaptureWorld(scope).SequenceEqual(before)
        || !reservations.CaptureWorld(scope).SequenceEqual(claims))
      {
        return Conflict("The world, item set or reservations changed after tick preparation.");
      }

      return OwnerCommitResult.Applied(header.Version);
    }

    public void Commit()
    {
    }
  }
}
