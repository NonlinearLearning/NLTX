using System;
using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using NSSLC.WorldGeneration;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Content;
using Terraria.Items;
using Terraria.Npc;
using Terraria.Player;
using Terraria.Player.Movement;
using Terraria.Relationships;
using Terraria.SpatialSimulation.Components;
using LegacyVector2 = NSSLC.WorldGeneration.Geometry.Vector2;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimePlayerStore : IDisposable
{
  internal const int PlayerWidth = 20;
  internal const int PlayerHeight = 42;
  private const float DefaultGravity = 0.4f;
  private readonly PlayerTraversalPhysicsSystem _traversalPhysics = new();
  private EntityIdentityRegistry _identityRegistry;
  private EntityRuntime _entityRuntime;
  private bool _ownsEntityRuntime;
  private RuntimePlayerEntity[] _players = Array.Empty<RuntimePlayerEntity>();
  private SimulationInputScript? _inputScript;
  private LoadedWorldSession? _session;
  private RuntimeItemRegistry? _itemRegistry;
  private WeaponComponent _weapon;
  private long _movementSnapshotTickNumber = -1;
  private bool _isDisposed;

  public RuntimePlayerStore(EntityIdentityRegistry? identityRegistry = null)
  {
    _identityRegistry = identityRegistry ?? new EntityIdentityRegistry();
    _entityRuntime = new EntityRuntime(_identityRegistry);
    _ownsEntityRuntime = true;
  }

  public RuntimePlayerStore(
    EntityRuntime entityRuntime,
    EntityIdentityRegistry identityRegistry)
  {
    _entityRuntime = entityRuntime ?? throw new ArgumentNullException(nameof(entityRuntime));
    _identityRegistry = identityRegistry ?? throw new ArgumentNullException(nameof(identityRegistry));
    _ownsEntityRuntime = false;
  }

  public int ActiveCount => _players.Length;

  public int ShotsFired => _players.Sum(static player => player.ShotsFired);

  public int ArrowAmmoRemaining => _players.Sum(static player => player.Inventory.CountItem(40));

  public IReadOnlyList<RuntimePlayerEntity> Players => Array.AsReadOnly(_players);

  public bool HasMagicQuiver(int playerSlot)
  {
    RuntimePlayerEntity? player = FindPlayer(playerSlot);
    if (player is null)
    {
      throw new InvalidOperationException(
        $"Projectile owner slot {playerSlot} is outside runtime player storage.");
    }

    return player.MagicQuiver;
  }

  public bool HasMagicQuiver(EntityReference playerReference)
  {
    return TryResolveEntityReference(playerReference, out RuntimePlayerEntity? player) &&
      player is not null &&
      player.MagicQuiver;
  }

  public bool TryGetEntityReference(int playerSlot, out EntityReference reference)
  {
    RuntimePlayerEntity? player = FindPlayer(playerSlot);
    if (player is not null &&
        _entityRuntime.TryGetReference(
          player.RuntimeHandle,
          EntityReferenceScope.Player,
          out reference))
    {
      return true;
    }

    reference = EntityReference.None;
    return false;
  }

  internal bool IsBoundTo(LoadedWorldSession session) =>
    session is not null &&
    ReferenceEquals(_session, session) &&
    ReferenceEquals(_entityRuntime, session.EntityRuntime);

  public bool TryGetPlayerAtSlot(int playerSlot, out RuntimePlayerEntity? player)
  {
    player = FindPlayer(playerSlot);
    return player is not null;
  }

  public bool TryResolveEntityReference(
    EntityReference reference,
    out RuntimePlayerEntity? player)
  {
    if (reference.Scope == EntityReferenceScope.Player &&
        _entityRuntime.TryResolve(reference, out RuntimeEntityHandle handle))
    {
      foreach (RuntimePlayerEntity candidate in _players)
      {
        if (candidate.RuntimeHandle == handle)
        {
          player = candidate;
          return true;
        }
      }
    }

    player = null;
    return false;
  }

  public bool TryGetOwnerMovementDelta(
    EntityReference playerReference,
    long tickNumber,
    out Vector2 movementDelta)
  {
    if (_movementSnapshotTickNumber != tickNumber ||
        !TryResolveEntityReference(playerReference, out RuntimePlayerEntity? player) ||
        player is null)
    {
      movementDelta = default;
      return false;
    }

    movementDelta = player.MovementDeltaThisTick;
    return true;
  }

  public bool HasAlivePlayerIntersecting(float left, float top, float width, float height)
  {
    foreach (RuntimePlayerEntity player in _players)
    {
      if (player.Lifecycle.Phase != PlayerLifecyclePhase.Alive)
      {
        continue;
      }

      LocationComponent location = player.Location;
      ColliderComponent collider = player.Collider;
      Vector2 colliderPosition = GetColliderPosition(location, collider);
      if (colliderPosition.X < left + width &&
          colliderPosition.X + collider.Width > left &&
          colliderPosition.Y < top + height &&
          colliderPosition.Y + collider.Height > top)
      {
        return true;
      }
    }

    return false;
  }

  public void Initialize(
    LoadedWorldSession session,
    int playerCount,
    ContentCatalog catalog,
    RuntimeItemRegistry itemRegistry,
    SimulationInputScript? inputScript = null,
    EntityIdentityRegistry? identityRegistry = null)
  {
    ObjectDisposedException.ThrowIf(_isDisposed, this);
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(itemRegistry);
    ArgumentOutOfRangeException.ThrowIfNegative(playerCount);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(playerCount, byte.MaxValue);
    if (!session.IsComplete || !session.IsPublished)
    {
      throw new InvalidOperationException("Local players require a published world session.");
    }
    inputScript?.ValidatePlayerCount(playerCount);

    if (!catalog.Items.TryGet(39, out ItemDefinition bow))
    {
      throw new InvalidOperationException("The scripted player bow is missing from the content catalog.");
    }

    EntityIdentityRegistry candidateIdentityRegistry = session.IdentityRegistry;
    if (identityRegistry is not null &&
        !ReferenceEquals(identityRegistry, candidateIdentityRegistry))
    {
      throw new InvalidOperationException(
        "The Player store identity registry must be the owning session registry.");
    }

    EntityRuntime candidateRuntime = session.EntityRuntime;
    if (!_ownsEntityRuntime && !ReferenceEquals(_entityRuntime, candidateRuntime) &&
        _session is not null && !_session.IsDisposed)
    {
      throw new InvalidOperationException(
        "An injected Player store runtime must be the owning session runtime.");
    }

    if (!_ownsEntityRuntime && !ReferenceEquals(_entityRuntime, candidateRuntime))
    {
      _entityRuntime = candidateRuntime;
      _identityRegistry = candidateIdentityRegistry;
    }

    if (_session?.IsDisposed != true)
    {
      EnsurePlayerCleanupReady(_entityRuntime, _players);
    }

    var players = new RuntimePlayerEntity[playerCount];
    var candidateHandles = new List<RuntimeEntityHandle>(playerCount);

    ItemCombatDefinition combat = bow.Combat;
    var weapon = new WeaponComponent(
      combat.Damage,
      combat.KnockBack,
      bow.Use.UseAnimationTicks,
      bow.Use.UseTimeTicks,
      combat.ShootTypeId ?? 0,
      combat.ShootSpeed,
      ammoCategoryId: combat.AmmoTypeId ?? 0,
      consumesAmmo: combat.AmmoTypeId.HasValue || combat.UseAmmoTypeId.HasValue);
    bool candidateCommitted = false;
    try
    {
      for (int slot = 0; slot < players.Length; slot++)
      {
        Vector2 spawnPosition = GetSpawnPosition(session, slot);
        RuntimeEntityHandle handle = candidateRuntime.CreateEntity();
        candidateHandles.Add(handle);
        players[slot] = RuntimePlayerEntity.Hydrate(
          candidateRuntime,
          handle,
          slot,
          spawnPosition,
          spawnPosition,
          new ColliderComponent(PlayerWidth, PlayerHeight),
          weapon,
          itemRegistry,
          magicQuiver: inputScript?.HasMagicQuiver(slot) ?? false);
      }

      if (_session?.IsDisposed != true)
      {
        foreach (RuntimePlayerEntity player in _players)
        {
          player.Inventory.ReleaseAllItems();
        }
      }

      RemoveOwnedPlayerRoots(
        _entityRuntime,
        _players,
        skipBecauseRuntimeDisposed: _session?.IsDisposed == true);
      if (_ownsEntityRuntime && !ReferenceEquals(_entityRuntime, candidateRuntime))
      {
        _entityRuntime.Dispose();
      }

      _identityRegistry = candidateIdentityRegistry;
      _entityRuntime = candidateRuntime;
      _ownsEntityRuntime = false;
      _players = players;
      _inputScript = inputScript;
      _session = session;
      _itemRegistry = itemRegistry;
      _weapon = weapon;
      _movementSnapshotTickNumber = -1;
      candidateCommitted = true;
    }
    finally
    {
      if (!candidateCommitted)
      {
        foreach (RuntimePlayerEntity? player in players)
        {
          player?.Inventory.ReleaseAllItems();
        }

        RemoveOwnedRuntimeEntities(candidateRuntime, candidateHandles);
      }
    }
  }

  public bool TryDestroyPlayer(int playerSlot)
  {
    if (_session?.IsDisposed == true)
    {
      return false;
    }

    int index = Array.FindIndex(_players, player => player.Slot == playerSlot);
    if (index < 0)
    {
      return false;
    }

    RuntimePlayerEntity player = _players[index];
    if (!ArePlayerCleanupRootsReady(_entityRuntime, new[] { player }))
    {
      return false;
    }

    player.Inventory.ReleaseAllItems();
    if (!_entityRuntime.TryBeginTermination(player.RuntimeHandle) ||
        !_entityRuntime.TryRemoveEntity(player.RuntimeHandle))
    {
      throw new InvalidOperationException("The Player entity could not be terminated.");
    }

    _players = _players.Where((_, currentIndex) => currentIndex != index).ToArray();
    _movementSnapshotTickNumber = -1;
    return true;
  }

  public bool TryCreatePlayerAtSlot(int playerSlot)
  {
    ObjectDisposedException.ThrowIf(_isDisposed, this);
    ArgumentOutOfRangeException.ThrowIfNegative(playerSlot);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(playerSlot, byte.MaxValue);
    if (FindPlayer(playerSlot) is not null || _session is null || _session.IsDisposed ||
        _itemRegistry is null)
    {
      return false;
    }

    Vector2 spawnPosition = GetSpawnPosition(_session, playerSlot);
    RuntimeEntityHandle handle = _entityRuntime.CreateEntity();
    RuntimePlayerEntity player;
    try
    {
      player = RuntimePlayerEntity.Hydrate(
        _entityRuntime,
        handle,
        playerSlot,
        spawnPosition,
        spawnPosition,
        new ColliderComponent(PlayerWidth, PlayerHeight),
        _weapon,
        _itemRegistry,
        magicQuiver: _inputScript?.HasMagicQuiver(playerSlot) ?? false);
    }
    catch
    {
      if (_entityRuntime.TryGetStatus(handle, out EntityRuntimeStatus status))
      {
        if (status == EntityRuntimeStatus.Running)
        {
          _entityRuntime.TryBeginTermination(handle);
        }

        _entityRuntime.TryRemoveEntity(handle);
      }

      throw;
    }

    _players = _players.Append(player).OrderBy(static current => current.Slot).ToArray();
    _movementSnapshotTickNumber = -1;
    return true;
  }

  public void Clear()
  {
    ClearCore(replaceOwnedRuntime: true);
  }

  private void ClearCore(bool replaceOwnedRuntime)
  {
    if (_isDisposed)
    {
      return;
    }

    if (_session?.IsDisposed != true)
    {
      EnsurePlayerCleanupReady(_entityRuntime, _players);
      foreach (RuntimePlayerEntity player in _players)
      {
        player.Inventory.ReleaseAllItems();
      }
    }

    RemoveOwnedPlayerRoots(
      _entityRuntime,
      _players,
      skipBecauseRuntimeDisposed: _session?.IsDisposed == true);
    if (_ownsEntityRuntime)
    {
      _entityRuntime.Dispose();
      if (replaceOwnedRuntime)
      {
        _entityRuntime = new EntityRuntime(_identityRegistry);
      }
    }

    _players = Array.Empty<RuntimePlayerEntity>();
    _inputScript = null;
    _session = null;
    _itemRegistry = null;
    _movementSnapshotTickNumber = -1;
  }

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    ClearCore(replaceOwnedRuntime: false);
    if (_ownsEntityRuntime)
    {
      _entityRuntime.Dispose();
    }

    _isDisposed = true;
  }

  private static void RemoveOwnedPlayerRoots(
    EntityRuntime runtime,
    IEnumerable<RuntimePlayerEntity> players,
    bool skipBecauseRuntimeDisposed = false)
  {
    if (skipBecauseRuntimeDisposed)
    {
      return;
    }

    RemoveOwnedRuntimeEntities(runtime, players.Select(static player => player.RuntimeHandle));
  }

  private static void RemoveOwnedRuntimeEntities(
    EntityRuntime runtime,
    IEnumerable<RuntimeEntityHandle> handles)
  {
    RuntimeEntityHandle[] ownedHandles = handles.ToArray();
    foreach (RuntimeEntityHandle handle in ownedHandles)
    {
      if (runtime.TryGetStatus(handle, out _) && !runtime.IsReadyForTermination(handle))
      {
        throw new InvalidOperationException(
          "Every affected Player root must be unborrowed before domain cleanup.");
      }
    }

    foreach (RuntimeEntityHandle handle in ownedHandles)
    {
      if (!runtime.TryGetStatus(handle, out EntityRuntimeStatus status))
      {
        continue;
      }

      if (status == EntityRuntimeStatus.Running && !runtime.TryBeginTermination(handle))
      {
        throw new InvalidOperationException("A Player root could not begin domain cleanup.");
      }

      if (!runtime.TryRemoveEntity(handle))
      {
        throw new InvalidOperationException("A Player root could not be removed during domain cleanup.");
      }
    }
  }

  private static void EnsurePlayerCleanupReady(
    EntityRuntime runtime,
    IEnumerable<RuntimePlayerEntity> players)
  {
    if (!ArePlayerCleanupRootsReady(runtime, players))
    {
      throw new InvalidOperationException(
        "Every affected Player root and inventory item must be available before domain cleanup.");
    }
  }

  private static bool ArePlayerCleanupRootsReady(
    EntityRuntime runtime,
    IEnumerable<RuntimePlayerEntity> players)
  {
    foreach (RuntimePlayerEntity player in players.ToArray())
    {
      if (!runtime.TryGetStatus(player.RuntimeHandle, out _) ||
          !runtime.IsReadyForTermination(player.RuntimeHandle) ||
          !player.Inventory.TryValidateReleaseAllItems())
      {
        return false;
      }
    }

    return true;
  }

  private RuntimePlayerEntity? FindPlayer(int playerSlot)
  {
    foreach (RuntimePlayerEntity player in _players)
    {
      if (player.Slot == playerSlot)
      {
        return player;
      }
    }

    return null;
  }

  private static Vector2 GetSpawnPosition(LoadedWorldSession session, int playerSlot)
  {
    float x = session.World.Descriptor.SpawnTileX * 16f - PlayerWidth * 0.5f + playerSlot * 24f;
    float y = session.World.Descriptor.SpawnTileY * 16f - PlayerHeight;
    return new Vector2(x, y);
  }

  public void UseScriptedWeapons(
    long tickNumber,
    RuntimeNpcStore npcs,
    RuntimeProjectileStore projectiles)
  {
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(projectiles);
    foreach (RuntimePlayerEntity player in _players)
    {
      if (player.Lifecycle.IsDead)
      {
        continue;
      }

      bool usePressed = _inputScript is null
        ? tickNumber % 60 == 1
        : _inputScript.GetInput(tickNumber, player.Slot).UseItem;
      PlayerItemUseIntentInput intentInput = new(usePressed, ControlUseTile: false);
      bool itemUseEdited = player.TryEditItemUse((ref PlayerItemUseState itemUse) =>
      {
        _ = PlayerItemUseExecutionSystem.BeginCheck(isCrowdControlled: false, itemUse);
        if (itemUse.ReuseDelayRemainingTicks > 0)
        {
          itemUse.ReuseDelayRemainingTicks--;
        }

        if (itemUse.UseRemainingTicks > 0)
        {
          itemUse.UseRemainingTicks--;
        }

        PlayerItemUseAnimationStepResult animation =
          PlayerItemUseExecutionSystem.AdvanceAnimationFrame(
            new PlayerItemUseAnimationStepInput(
              itemUse.AnimationRemainingTicks,
              itemUse.ReuseDelayRemainingTicks,
              ControlUseItem: false,
              ReleaseUseItem: false,
              itemUse.HasPendingReuse));
        itemUse.AnimationRemainingTicks = animation.AnimationRemainingTicks;
        itemUse.HasPendingReuse = animation.HasPendingReuse;
      });
      if (!itemUseEdited)
      {
        throw new InvalidOperationException("The Player item-use state could not advance.");
      }

      player.AdvanceItemUseIntent(intentInput);
      PlayerItemUseIntentComponent itemUseIntent = player.ItemUseIntent;
      var startGate = new PlayerItemUseStartGateInput(
        itemUseIntent.ControlUseItem,
        ReleaseUseItem: usePressed,
        player.ItemUseAnimationRemainingTicks,
        ItemUseStyle: 5,
        HasBufferedSelectionChange: false);
      if (!PlayerItemUseExecutionSystem.ShouldEnterStartUseBranch(in startGate) ||
          player.Inventory.CountItem(40) <= 0 || !player.Weapon.CanSpawnProjectile)
      {
        continue;
      }

      Vector2 aim = ResolveAimDirection(player, npcs.CreateContactSnapshots());
      Vector2 center = GetColliderCenter(player.Location, player.Collider);
      if (!projectiles.TrySpawnArrow(
        player.Weapon.ProjectileType,
        player.Slot,
        center,
        aim * player.Weapon.ProjectileSpeed,
        player.Weapon.Damage,
        player.Weapon.Knockback))
      {
        continue;
      }

      if (!player.Inventory.TryConsume(
            commandId: Guid.NewGuid(),
            typeId: 40,
            amount: 1))
      {
        throw new InvalidOperationException("A spawned arrow could not consume its inventory ammo.");
      }

      player.ShotsFired++;
      if (!player.TryEditItemUse((ref PlayerItemUseState itemUse) =>
          {
            itemUse.AnimationRemainingTicks = player.Weapon.UseAnimationTicks;
            itemUse.AnimationDurationTicks = player.Weapon.UseAnimationTicks;
            itemUse.UseRemainingTicks = player.Weapon.UseTimeTicks;
            itemUse.UseDurationTicks = player.Weapon.UseTimeTicks;
            itemUse.ReuseDelayRemainingTicks = player.Weapon.UseTimeTicks;
            itemUse.LastUseAttemptSucceeded = true;
          }))
      {
        throw new InvalidOperationException("The Player item-use result could not be committed.");
      }
    }
  }

  public void Update(long tickNumber)
  {
    foreach (RuntimePlayerEntity player in _players)
    {
      player.AdvanceLifecycle();
      if (player.Lifecycle.IsDead)
      {
        player.MovementDeltaThisTick = Vector2.Zero;
        continue;
      }

      MovementStateComponent movement = player.Movement;
      Vector2 previousPosition = movement.Position;
      LocationComponent location = player.Location;
      ColliderComponent collider = player.Collider;
      Vector2 colliderPosition = GetColliderPosition(location, collider);
      int colliderWidth = GetCollisionWidth(collider);
      int colliderHeight = GetCollisionHeight(collider);
      bool wasGrounded = movement.IsGrounded;
      LegacyVector2 legacyPosition = new(colliderPosition.X, colliderPosition.Y);
      bool wet = Collision.WetCollision(legacyPosition, colliderWidth, colliderHeight);
      bool shimmerWet = Collision.shimmer;
      bool honeyWet = Collision.honey;
      bool lavaWet = Collision.LavaCollision(legacyPosition, colliderWidth, colliderHeight);
      PlayerTraversalPhysicsResult traversal = _traversalPhysics.Resolve(
        new PlayerTraversalPhysicsInput(
          DefaultGravity,
          PortalPhysicsEnabled: false,
          wet,
          IsPerformingJumpDownDash: false,
          shimmerWet,
          Shimmering: false,
          honeyWet,
          Merman: false,
          Trident: false,
          lavaWet,
          ControlUp: false,
          VortexDebuff: false));
      if (!player.TryEditPhysics(
            (ref PlayerMovementPhysicsStateComponent physics) =>
              _traversalPhysics.CommitMovementState(physics, in traversal)))
      {
        throw new InvalidOperationException("The Player movement physics could not be committed.");
      }
      RuntimePlayerEntity.PhysicsSnapshot physicsSnapshot = player.Physics;

      RuntimePlayerInput input = _inputScript?.GetInput(tickNumber, player.Slot) ?? default;
      int direction = _inputScript is null
        ? ResolveHorizontalInput(player.Slot, tickNumber)
        : input.Horizontal;
      if (direction != 0)
      {
        movement.Velocity.X = Math.Clamp(
          movement.Velocity.X + direction * physicsSnapshot.RunAcceleration,
          -physicsSnapshot.MaxRunSpeed,
          physicsSnapshot.MaxRunSpeed);
      }
      else
      {
        movement.Velocity.X = MoveTowardsZero(
          movement.Velocity.X,
          physicsSnapshot.RunSlowdown);
      }

      bool jumpPressed = (_inputScript is null
        ? tickNumber % 180 == 1
        : input.Jump) && movement.IsGrounded;
      if (jumpPressed)
      {
        movement.Velocity.Y = -traversal.JumpSpeed;
        movement.IsGrounded = false;
        player.JumpCount++;
      }

      movement.Velocity.Y = Math.Min(
        movement.Velocity.Y + physicsSnapshot.Gravity,
        physicsSnapshot.MaxFallSpeed);
      LegacyVector2 requestedVelocity = new(movement.Velocity.X, movement.Velocity.Y);
      LegacyVector2 resolvedVelocity = Collision.TileCollision(
        legacyPosition,
        requestedVelocity,
        colliderWidth,
        colliderHeight);
      bool collidedX = MathF.Abs(resolvedVelocity.X - requestedVelocity.X) > 0.001f;
      bool collidedY = MathF.Abs(resolvedVelocity.Y - requestedVelocity.Y) > 0.001f;
      movement.Position += new Vector2(resolvedVelocity.X, resolvedVelocity.Y);
      movement.Velocity = new Vector2(
        collidedX ? 0f : resolvedVelocity.X,
        collidedY ? 0f : resolvedVelocity.Y);
      bool landed = collidedY && requestedVelocity.Y > 0f && Collision.down;
      movement.IsGrounded = landed;
      if (landed && !wasGrounded)
      {
        player.LandingCount++;
      }
      player.Movement = movement;
      player.MovementDeltaThisTick = movement.Position - previousPosition;
    }

    _movementSnapshotTickNumber = tickNumber;
  }

  public void ApplyNpcContactDamage(
    long tickNumber,
    IReadOnlyList<RuntimeNpcContactSnapshot> npcs)
  {
    ArgumentNullException.ThrowIfNull(npcs);
    foreach (RuntimePlayerEntity player in _players)
    {
      LocationComponent location = player.Location;
      ColliderComponent collider = player.Collider;
      Vector2 colliderPosition = GetColliderPosition(location, collider);
      int colliderWidth = GetCollisionWidth(collider);
      int colliderHeight = GetCollisionHeight(collider);
      foreach (RuntimeNpcContactSnapshot npc in npcs)
      {
        if (!npc.IsActive || !npc.IsHostile ||
            !Overlaps(colliderPosition, colliderWidth, colliderHeight,
              npc.Hitbox.Position,
              npc.Hitbox.Width,
              npc.Hitbox.Height))
        {
          continue;
        }

        if (player.TryTakeNpcContactDamage(npc, tickNumber))
        {
          break;
        }
      }
    }
  }

  private static int ResolveHorizontalInput(int playerSlot, long tickNumber)
  {
    long segment = (tickNumber + playerSlot * 60) % 360;
    return segment < 120 ? 1 : segment < 240 ? -1 : 0;
  }

  private static bool Overlaps(
    Vector2 firstPosition,
    int firstWidth,
    int firstHeight,
    Vector2 secondPosition,
    int secondWidth,
    int secondHeight)
  {
    return firstPosition.X < secondPosition.X + secondWidth &&
      firstPosition.X + firstWidth > secondPosition.X &&
      firstPosition.Y < secondPosition.Y + secondHeight &&
      firstPosition.Y + firstHeight > secondPosition.Y;
  }

  private static Vector2 ResolveAimDirection(
    RuntimePlayerEntity player,
    IReadOnlyList<RuntimeNpcContactSnapshot> npcs)
  {
    Vector2 playerCenter = GetColliderCenter(player.Location, player.Collider);
    RuntimeNpcContactSnapshot? nearest = null;
    float nearestDistanceSquared = float.PositiveInfinity;
    foreach (RuntimeNpcContactSnapshot npc in npcs)
    {
      if (!npc.IsHostile || !npc.IsActive)
      {
        continue;
      }

      float distanceSquared = Vector2.DistanceSquared(playerCenter, npc.Hitbox.Center);
      if (distanceSquared < nearestDistanceSquared)
      {
        nearest = npc;
        nearestDistanceSquared = distanceSquared;
      }
    }

    Vector2 direction = nearest is not { } nearestNpc
      ? new Vector2(player.Velocity.X < 0f ? -1f : 1f, 0f)
      : nearestNpc.Hitbox.Center - playerCenter;
    return direction.LengthSquared() <= float.Epsilon
      ? Vector2.UnitX
      : Vector2.Normalize(direction);
  }

  private static Vector2 GetColliderPosition(
    LocationComponent location,
    ColliderComponent collider)
  {
    return new Vector2(
      location.X + collider.OffsetX,
      location.Y + collider.OffsetY);
  }

  private static Vector2 GetColliderCenter(
    LocationComponent location,
    ColliderComponent collider)
  {
    Vector2 position = GetColliderPosition(location, collider);
    return position + new Vector2(collider.Width * 0.5f, collider.Height * 0.5f);
  }

  private static int GetCollisionWidth(ColliderComponent collider)
  {
    return Math.Max(1, (int)collider.Width);
  }

  private static int GetCollisionHeight(ColliderComponent collider)
  {
    return Math.Max(1, (int)collider.Height);
  }

  private static float MoveTowardsZero(float value, float amount)
  {
    if (value > 0f)
    {
      return Math.Max(0f, value - amount);
    }

    return Math.Min(0f, value + amount);
  }
}
