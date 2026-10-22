using System;
using System.Collections.Generic;
using System.Numerics;
using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Contracts;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Composes the phase owners used by the built-in runtime. Each phase is backed by an existing
/// domain owner; adapters retain an explicit NotReady result when a bounded capture cannot be
/// produced instead of turning missing world facts into a successful mutation.
/// </summary>
internal static class DefaultServerTickPhaseOwners
{
  public static IReadOnlyCollection<IServerTickPhaseOwner> Create(
    ServerComposition composition,
    WorldTickCoordinator worldTick,
    ServerOutboundDeliveryPump delivery)
  {
    ArgumentNullException.ThrowIfNull(composition);
    ArgumentNullException.ThrowIfNull(worldTick);
    ArgumentNullException.ThrowIfNull(delivery);

    var playerOwner = new PlayerServerTickPhaseOwner(
      new PlayerUpdateService(composition.Players),
      () => new PlayerUpdateContext(
        new PlayerLuckCalculationContext(
          composition.LanternNightState.CaptureSnapshot().LanternsUp,
          HasStinkyDebuff: false),
        composition.IdentityRegistry.IsActive));

    var worldComposite = new WorldTickPhaseCompositeOwner(
      worldTick,
      tick => CaptureWorldInput(composition, tick),
      composition.WorldTickInvasionBridge,
      applyInvasionBridge: false);

    var owners = new List<IServerTickPhaseOwner>
    {
      playerOwner,
      new NpcServerTickPhaseOwner(composition),
      composition.ProjectileTickPhaseOwner,
      new WorldItemServerTickPhaseOwner(composition),
      new LeashedEntityServerTickPhaseOwner(
        new LeashedEntitySectionService(composition.LeashedEntityRegistry),
        composition.LeashedEntityMotion,
        new CapturingLeashedEntityEffects()),
      worldComposite,
      new InvasionServerTickPhaseOwner(
        worldComposite,
        composition.WorldTickInvasionBridge),
      new ModulePeriodicSyncOwner(composition, delivery)
    };
    return owners.AsReadOnly();
  }

  private static WorldTickPhaseInput? CaptureWorldInput(
    ServerComposition composition,
    long gameTickIndex)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);
    try
    {
      WorldTickCapture capture = composition.WorldOwner.CaptureWorldTick();
      return new WorldTickPhaseInput(
        capture.WorldEpoch,
        ElapsedGameTicks: 1,
        DayRate: 1,
        LiquidOperationBudget: 8);
    }
    catch (InvalidOperationException)
    {
      return null;
    }
  }

  private sealed class NpcServerTickPhaseOwner : IServerTickPhaseOwner
  {
    private readonly ServerComposition _composition;
    private readonly NpcTickBatchOwner _batch;

    public NpcServerTickPhaseOwner(ServerComposition composition)
    {
      _composition = composition ?? throw new ArgumentNullException(nameof(composition));
      _batch = new NpcTickBatchOwner(composition.NpcManager);
    }

    public ServerTickPhase Phase => ServerTickPhase.Npc;

    public object TransactionIdentity => this;

    public NpcTickBatchResult? LastResult { get; private set; }

    public string? LastRejection { get; private set; }

    public void Advance(long gameTickIndex)
    {
      ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);
      IReadOnlyList<NpcView> npcs = _composition.NpcManager.Capture();
      var players = new List<NpcTargetPlayer>();
      CommitVersion playerVersion = CommitVersion.Initial;
      foreach (var binding in _composition.SessionBindings.CaptureBindings()) {
        if (!_composition.SessionAdmission.TryGetStage(binding.Epoch, out var stage) ||
            stage != Session.SessionStage.Active ||
            !_composition.Players.TryGetForBinding(binding, out var player) || player is null) {
          continue;
        }
        players.Add(new NpcTargetPlayer(binding.PlayerSlot,
            new Vector2(player.Movement.Position.X, player.Movement.Position.Y),
            player.RuntimeSpatial.Width, player.RuntimeSpatial.Height,
            true, player.IsDead, false, 0, false, player.RuntimeSpatial.ItemAnimation > 0, false));
        if (player.Version.Value > playerVersion.Value) {
          playerVersion = player.Version;
        }
      }
      // Preserve the diagnostic pair until a player is available; no invented target position.
      if (npcs.Count == 0 || !players.Any(player => !player.Dead)) {
        LastResult = null;
        LastRejection = null;
        return;
      }
      if (players.All(player => player.Slot != 0)) {
        players.Add(new NpcTargetPlayer(0, Vector2.Zero, 20, 42,
            false, false, false, 0, false, false, false));
      }
      AdvanceCaptured(gameTickIndex, players, playerVersion);
    }

    public void AdvanceCaptured(
        long gameTickIndex, IReadOnlyList<NpcTargetPlayer> players, CommitVersion playerVersion) {
      IReadOnlyList<NpcView> npcs = _composition.NpcManager.Capture();
      WorldHeaderCapture header = _composition.WorldOwner.CaptureWorldHeader();
      WorldGeometry geometry = header.Header.Geometry;
      int left = geometry.Width;
      int top = geometry.Height;
      int right = 0;
      int bottom = 0;
      foreach (NpcView npc in npcs) {
        int x = (int)Math.Clamp(npc.Position.X / 16, 0, geometry.Width - 1);
        int y = (int)Math.Clamp(npc.Position.Y / 16, 0, geometry.Height - 1);
        left = Math.Min(left, Math.Max(0, x - 12));
        top = Math.Min(top, Math.Max(0, y - 12));
        right = Math.Max(right, Math.Min(geometry.Width, x + npc.Width / 16 + 14));
        bottom = Math.Max(bottom, Math.Min(geometry.Height, y + npc.Height / 16 + 14));
      }
      if (right <= left || bottom <= top ||
          (long)(right - left) * (bottom - top) > WorldSpatialSnapshot.MaximumCaptureTiles) {
        LastRejection = "NPC collision capture exceeds the bounded tile budget.";
        return;
      }
      WorldSpatialSnapshot spatial = _composition.WorldOwner.CaptureSpatialSnapshot(
          new WorldSectionBounds(left, top, right - left, bottom - top));
      var motion = new NpcMotionSnapshot(spatial);
      var frame = new NpcTickFrameContext(
          header.WorldEpoch, header.Version, playerVersion, players);
      var slimes = new List<NpcSlimeTickInput>();
      var eyes = new List<NpcEyeTickInput>();
      var zombies = new List<NpcZombieTickInput>();
      var guides = new List<NpcGuideTickInput>();
      foreach (NpcView npc in npcs.Where(npc => npc.Life > 0)) {
        WorldLiquidContact liquid = spatial.GetLiquidContact(npc.Position, npc.Width, npc.Height);
        var facts = header.Header.Facts;
        bool special = facts.GetGoodWorld || facts.ZenithWorld || facts.RemixWorld ||
            facts.DontStarveWorld;
        switch (npc.Type) {
          case 1:
            slimes.Add(new NpcSlimeTickInput(npc.Entity, npc.Revision, header.WorldEpoch,
                header.Version, 0,
                new NpcSlimeEnvironment(header.Header.DayTime, facts.WorldSurface,
                    geometry.Width, npc.Position.Y / 16 > facts.RockLayer,
                    false, false, special, liquid.Wet, liquid.Honey, liquid.Shimmer), false));
            break;
          case 2:
            eyes.Add(new NpcEyeTickInput(npc.Entity, npc.Revision, header.WorldEpoch,
                header.Version, header.Header.DayTime, facts.WorldSurface, false));
            break;
          case 3:
            zombies.Add(new NpcZombieTickInput(npc.Entity, npc.Revision, header.WorldEpoch,
                header.Version, new NpcZombieEnvironment(header.Header.DayTime, facts.Eclipse,
                    facts.GameMode is 1 or 2, facts.WorldSurface, geometry.Width, geometry.Height,
                    special, liquid.Wet, liquid.Honey, liquid.Shimmer), false));
            break;
          case 22:
            NpcView? threat = npcs.Where(other => other.Type is 1 or 2 or 3 && other.Life > 0)
                .OrderBy(other => Vector2.DistanceSquared(other.Position, npc.Position))
                .FirstOrDefault(other => Vector2.DistanceSquared(other.Position, npc.Position) <
                    300 * 300);
            guides.Add(new NpcGuideTickInput(npc.Entity, npc.Revision, header.WorldEpoch,
                header.Version, threat?.Position));
            break;
        }
      }
      LastResult = _batch.Execute(new NpcTickBatchRequest(frame, motion,
          new WorldRevisionGuard(_composition.WorldOwner), new DeterministicNpcRandom(gameTickIndex),
          slimes, eyes, zombies, guides));
      string? rejection = LastResult.Result.Status == OwnerCommitStatus.Applied
          ? null : LastResult.Result.Detail;
      if (rejection != LastRejection && rejection is not null) {
        ServerLog.Warning("server.npc.tick_rejected", rejection);
      }
      LastRejection = rejection;
      if (gameTickIndex % 120 == 0) {
        foreach (NpcView npc in LastResult.Packet23Snapshots) {
          ServerLog.Info("server.npc.tick",
              $"tick={gameTickIndex} slot={npc.Slot} type={npc.Type} life={npc.Life} " +
              $"position={npc.Position} velocity={npc.Velocity} ai={string.Join(",", npc.Ai)}");
        }
      }
    }
  }

  private sealed class WorldItemServerTickPhaseOwner : IServerTickPhaseOwner
  {
    private readonly ServerComposition _composition;
    private readonly WorldItemTickCoordinator _coordinator;

    public WorldItemServerTickPhaseOwner(ServerComposition composition)
    {
      _composition = composition ?? throw new ArgumentNullException(nameof(composition));
      _coordinator = new WorldItemTickCoordinator(
        composition.WorldItemStore,
        composition.WorldItemTransactions,
        composition.WorldOwner);
    }

    public ServerTickPhase Phase => ServerTickPhase.WorldItem;

    public object TransactionIdentity => this;

    public string? LastRejection { get; private set; }

    public void Advance(long gameTickIndex)
    {
      ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);
      WorldScopeId scope = new(_composition.IdentityRegistry.CurrentWorldScope);
      OwnerCommitResult ownership = _composition.ClientWorldItems.Advance();
      if (ownership.Status != OwnerCommitStatus.Applied) {
        LastRejection = ownership.Detail;
        return;
      }
      IReadOnlyList<WorldItemSnapshot> items = _composition.WorldItemStore.CaptureWorld(scope);
      if (items.Count == 0)
      {
        LastRejection = null;
        return;
      }

      WorldHeaderCapture header = _composition.WorldOwner.CaptureWorldHeader();
      if (!TryGetCaptureBounds(header.Header.Geometry, items, out WorldSectionBounds bounds))
      {
        AdvanceOwnershipTimers(scope, header,
          "World-item collision capture is outside the bounded spatial budget.");
        return;
      }

      WorldSpatialSnapshot spatial = _composition.WorldOwner.CaptureSpatialSnapshot(bounds);
      OwnerCommitResult prepared = _coordinator.PrepareTick(
        scope,
        header.WorldEpoch,
        header,
        spatial,
        out WorldItemTransactionCoordinator.PreparedTransaction? transaction);
      if (prepared.Status != OwnerCommitStatus.Applied || transaction is null)
      {
        if (prepared.Status == OwnerCommitStatus.Unavailable) {
          AdvanceOwnershipTimers(scope, header, prepared.Detail ?? "Item motion is unavailable.");
        } else {
          LastRejection = prepared.Detail;
        }
        return;
      }

      OwnerCommitResult validation = _coordinator.ValidateTick(transaction);
      if (validation.Status != OwnerCommitStatus.Applied)
      {
        LastRejection = validation.Detail;
        return;
      }

      WorldItemTransactionResult committed = _coordinator.CommitTick(transaction);
      LastRejection = committed.Result.Status == OwnerCommitStatus.Applied
        ? null
        : committed.Result.Detail;
    }

    private void AdvanceOwnershipTimers(WorldScopeId scope, WorldHeaderCapture header,
        string motionRejection) {
      OwnerCommitResult result = _coordinator.PrepareOwnershipTimerTick(
        scope, header.WorldEpoch, header, out var transaction);
      if (result.Status == OwnerCommitStatus.Applied && transaction is not null) {
        result = _coordinator.CommitTick(transaction).Result;
      }
      string detail = result.Status == OwnerCommitStatus.Applied
        ? motionRejection
        : $"{motionRejection} Ownership timers: {result.Detail}";
      if (detail != LastRejection) {
        ServerLog.Warning("server.item.motion.deferred", ServerLog.Quote(detail));
      }
      LastRejection = detail;
    }

    private static bool TryGetCaptureBounds(
      WorldGeometry geometry,
      IReadOnlyList<WorldItemSnapshot> items,
      out WorldSectionBounds bounds)
    {
      int left = geometry.Width;
      int top = geometry.Height;
      int right = 0;
      int bottom = 0;
      foreach (WorldItemSnapshot item in items)
      {
        WorldSectionBounds query;
        try
        {
          if (!WorldItemTickCoordinator.TryGetCaptureBounds(item, geometry, out query)) {
            bounds = default;
            return false;
          }
        }
        catch (ArgumentOutOfRangeException)
        {
          bounds = default;
          return false;
        }

        left = Math.Min(left, query.OriginX);
        top = Math.Min(top, query.OriginY);
        right = Math.Max(right, query.OriginX + query.Width);
        bottom = Math.Max(bottom, query.OriginY + query.Height);
      }

      long width = (long)right - left;
      long height = (long)bottom - top;
      if (width <= 0 || height <= 0 || width * height > WorldSpatialSnapshot.MaximumCaptureTiles)
      {
        bounds = default;
        return false;
      }

      bounds = new WorldSectionBounds(left, top, (int)width, (int)height);
      return true;
    }
  }

  private sealed class InvasionServerTickPhaseOwner : IServerTickPhaseOwner
  {
    private readonly WorldTickPhaseCompositeOwner _world;
    private readonly WorldTickInvasionBridge _bridge;

    public InvasionServerTickPhaseOwner(
      WorldTickPhaseCompositeOwner world,
      WorldTickInvasionBridge bridge)
    {
      _world = world ?? throw new ArgumentNullException(nameof(world));
      _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    public ServerTickPhase Phase => ServerTickPhase.Invasion;

    public object TransactionIdentity => this;

    public string? LastRejection { get; private set; }

    public void Advance(long gameTickIndex)
    {
      ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);
      WorldTickResult? result = _world.LastResult;
      if (result is null || result.Tick != gameTickIndex)
      {
        LastRejection = "The invasion phase has no matching committed world tick.";
        return;
      }

      if (!result.Applied)
      {
        LastRejection = result.Rejection ??
          "The invasion phase cannot consume a rejected world tick.";
        return;
      }

      _bridge.Consume(result);
      LastRejection = null;
    }
  }

  private sealed class CapturingLeashedEntityEffects : ILeashedEntityTickEffectPort
  {
    public LeashedEntitySectionService.SectionTickResult? LastResult { get; private set; }

    public void Publish(LeashedEntitySectionService.SectionTickResult result)
    {
      LastResult = result ?? throw new ArgumentNullException(nameof(result));
    }
  }

  private sealed class WorldRevisionGuard : INpcTickWorldRevisionGuard
  {
    private readonly WorldOwner _world;

    public WorldRevisionGuard(WorldOwner world)
    {
      _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    public bool IsCurrentWorldRevision(ulong worldEpoch, CommitVersion version)
    {
      return _world.IsCurrentWorldRevision(worldEpoch, version);
    }
  }

  private sealed class NpcMotionSnapshot : INpcTickMotionSnapshot
  {
    private readonly WorldSpatialQueries _queries;
    private readonly WorldSpatialSnapshot _snapshot;

    public NpcMotionSnapshot(WorldSpatialSnapshot snapshot)
    {
      _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
      _queries = new WorldSpatialQueries(snapshot, Array.Empty<NpcView>());
    }

    public ulong WorldEpoch => _snapshot.WorldEpoch;

    public CommitVersion WorldCommitVersion => _snapshot.Version;

    public bool IsWet(NpcView npc) => _queries.IsWet(npc);

    public NpcMotionResult Move(NpcView npc, Vector2 desiredVelocity) {
      return Move(npc, npc.Position, desiredVelocity, false);
    }

    public bool HasSolidCollision(NpcView npc, Vector2 position)
    {
      return _snapshot.HasSolidTileCollision(position, npc.Width, npc.Height);
    }

    public NpcMotionResult Move(
      NpcView npc,
      Vector2 position,
      Vector2 desiredVelocity,
      bool fallThroughPlatforms)
    {
      WorldMotionResult result = _snapshot.Move(
        position,
        desiredVelocity,
        npc.Width,
        npc.Height,
        fallThroughPlatforms);
      return new NpcMotionResult(
        result.Position,
        result.Velocity,
        result.CollideX,
        result.CollideY,
        position,
        desiredVelocity);
    }

    public NpcFighterTile GetTile(int x, int y)
    {
      WorldTile tile = _snapshot.GetTile(new TileCoordinate(x, y));
      bool active = tile.Active && !tile.Inactive;
      bool solid = active && _snapshot.Facts.Solid[tile.Type];
      bool solidTop = active && _snapshot.Facts.SolidTop[tile.Type];
      bool platform = tile.Type is 19 or 427 or 435 or 436 or 437 or 438 or 439;
      return new NpcFighterTile(
        active,
        solid,
        solidTop,
        tile.HalfBlock,
        tile.Slope is 1 or 2 or 3 or 4,
        platform,
        tile.FrameY == 0,
        tile.Type is 10 or 11 or 388 or 389);
    }

    public bool CanHitTarget(NpcView npc, Vector2 position, NpcTargetPlayer target)
    {
      return _snapshot.HasSolidTileCollision(position, npc.Width, npc.Height) == false;
    }
  }

  private sealed class DeterministicNpcRandom : INpcAiRandom
  {
    private uint _state;

    public DeterministicNpcRandom(long seed)
    {
      _state = unchecked((uint)seed) + 0x9E3779B9u;
    }

    public int Next(int minimumInclusive, int maximumExclusive)
    {
      if (minimumInclusive >= maximumExclusive)
      {
        throw new ArgumentOutOfRangeException(nameof(maximumExclusive));
      }

      _state ^= _state << 13;
      _state ^= _state >> 17;
      _state ^= _state << 5;
      uint range = checked((uint)(maximumExclusive - minimumInclusive));
      return minimumInclusive + (int)(_state % range);
    }
  }
}
