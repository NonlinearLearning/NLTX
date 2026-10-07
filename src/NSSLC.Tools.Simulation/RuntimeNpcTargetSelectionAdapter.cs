using System.Collections.Immutable;
using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Npc;
using Terraria.Player;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.SpatialSimulation;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimeNpcTargetSelectionAdapter
{
  private const int WallOfFleshNpcType = 113;
  private readonly RuntimeNpcEntity _npc;
  private readonly LoadedWorldSession _session;
  private readonly RuntimePlayerStore _players;
  private readonly ProjectileLifecycleSystem _projectileLifecycle;
  private readonly Func<IReadOnlyList<NpcNpcTargetSnapshot>> _captureNpcs;
  private readonly int _oldDirection;
  private readonly int _oldDirectionY;
  private readonly int _oldTarget;
  private readonly bool _collideX;
  private readonly bool _collideY;

  public RuntimeNpcTargetSelectionAdapter(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    RuntimePlayerStore players,
    Func<IReadOnlyList<NpcNpcTargetSnapshot>> captureNpcs,
    RuntimeNpcEntity.NpcDirectionSnapshot oldDirection,
    int oldTarget,
    bool collideX,
    bool collideY)
  {
    _npc = npc ?? throw new ArgumentNullException(nameof(npc));
    _session = session ?? throw new ArgumentNullException(nameof(session));
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _projectileLifecycle = new ProjectileLifecycleSystem(session.Storage);
    _captureNpcs = captureNpcs ?? throw new ArgumentNullException(nameof(captureNpcs));
    _oldDirection = oldDirection.Sprite;
    _oldDirectionY = oldDirection.Vertical;
    _oldTarget = oldTarget;
    _collideX = collideX;
    _collideY = collideY;
  }

  public NpcTargetSelectionResult SelectAndCommit(
    Vector2 position,
    int width,
    int height,
    NpcTargetSelectionStrategy? strategy = null,
    Vector2? checkPosition = null,
    bool faceTarget = true)
  {
    NpcTargetSelectionStrategy selectedStrategy = strategy ??
      (_npc.Definition.TypeId == WallOfFleshNpcType
        ? NpcTargetSelectionStrategy.WallOfFlesh
        : NpcTargetSelectionStrategy.Normal);
    Vector2 colliderPosition = position + new Vector2(
      _npc.Collider.OffsetX,
      _npc.Collider.OffsetY);
    var source = new NpcTargetGeometrySnapshot(colliderPosition, width, height);
    var players = new List<NpcPlayerTargetSnapshot>(_players.Players.Count);
    foreach (RuntimePlayerEntity player in _players.Players)
    {
      NpcPlayerTargetSnapshot snapshot =
        player.CaptureNpcTargetSelectionSnapshot(_npc.Definition.TypeId);
      int tankPetSlot = player.CaptureTankPetProjectileSlot();
      if (tankPetSlot >= 0 && TryCaptureTankPet(
            tankPetSlot,
            source,
            selectedStrategy,
            out NpcTankPetTargetSnapshot tankPet))
      {
        snapshot = snapshot with { TankPet = tankPet };
      }

      players.Add(snapshot);
    }

    RuntimeNpcEntity.NpcDirectionSnapshot direction = _npc.CaptureDirection();
    (int currentTarget, _) = _npc.CaptureTargetIndices();
    NpcTargetSelectionResult result = NpcTargetSelectionSystem.Select(
      new NpcTargetSelectionInputs(
        selectedStrategy,
        source,
        direction.Sprite,
        direction.Vertical,
        _oldDirection,
        _oldDirectionY,
        _oldTarget,
        _collideX,
        _collideY,
        _npc.CaptureConfused(),
        _npc.Definition.Capabilities.IsBoss,
        FaceTarget: faceTarget,
        players,
        selectedStrategy == NpcTargetSelectionStrategy.Upgraded
          ? _captureNpcs()
          : Array.Empty<NpcNpcTargetSnapshot>())
      {
        CurrentTarget = currentTarget,
        CheckPosition = checkPosition,
      });
    _npc.CommitTargetSelection(in result, requestNetworkUpdate: true);
    return result;
  }

  private bool TryCaptureTankPet(
    int projectileSlot,
    NpcTargetGeometrySnapshot source,
    NpcTargetSelectionStrategy strategy,
    out NpcTankPetTargetSnapshot tankPet)
  {
    if (!_projectileLifecycle.TryGetRuntimeHandleAtSlot(
          projectileSlot,
          out _,
          out RuntimeEntityHandle runtimeHandle) ||
        !_session.Storage.ProjectileRuntime.TryCapture<
          ProjectileIdentityComponent,
          ProjectileIdentityComponent>(
            runtimeHandle,
            static identity => identity,
            out ProjectileIdentityComponent identity) ||
        !_session.Storage.ProjectileRuntime.TryCapture<
          ProjectileLifetimeStateComponent,
          bool>(
            runtimeHandle,
            static lifetime => lifetime.Active,
            out bool active) ||
        !active ||
        !_session.Storage.ProjectileRuntime.TryCapture<
          LocationComponent,
          LocationComponent>(
            runtimeHandle,
            static location => location,
            out LocationComponent location) ||
        !_session.Storage.ProjectileRuntime.TryCapture<
          ColliderComponent,
          ColliderComponent>(
            runtimeHandle,
            static collider => collider,
            out ColliderComponent collider))
    {
      tankPet = default;
      return false;
    }

    var geometry = new NpcTargetGeometrySnapshot(
      new Vector2(location.X + collider.OffsetX, location.Y + collider.OffsetY),
      Math.Max(1, (int)collider.Width),
      Math.Max(1, (int)collider.Height));
    int canHitPointSize = strategy == NpcTargetSelectionStrategy.Upgraded ? 0 : 1;
    bool? canHit = TryCanHit(
      source.Center,
      geometry.Center,
      canHitPointSize);
    tankPet = new NpcTankPetTargetSnapshot(projectileSlot, geometry, canHit)
    {
      OwnerSlot = identity.OwnerSlot,
    };
    return true;
  }

  private bool? TryCanHit(
    Vector2 sourceCenter,
    Vector2 targetCenter,
    int pointSize)
  {
    int maxTilesX = _session.World.Descriptor.SizeX;
    int maxTilesY = _session.World.Descriptor.SizeY;
    int sourceTileX = GetCanHitTileCoordinate(
      sourceCenter.X,
      pointSize,
      maxTilesX,
      isVertical: false);
    int sourceTileY = GetCanHitTileCoordinate(
      sourceCenter.Y,
      pointSize,
      maxTilesY,
      isVertical: true);
    int targetTileX = GetCanHitTileCoordinate(
      targetCenter.X,
      pointSize,
      maxTilesX,
      isVertical: false);
    int targetTileY = GetCanHitTileCoordinate(
      targetCenter.Y,
      pointSize,
      maxTilesY,
      isVertical: true);
    if (sourceTileX == targetTileX && sourceTileY == targetTileY)
    {
      return true;
    }

    int leftTile = Math.Max(0, Math.Min(sourceTileX, targetTileX) - 1);
    int topTile = Math.Max(0, Math.Min(sourceTileY, targetTileY) - 1);
    int rightTile = Math.Min(maxTilesX - 1, Math.Max(sourceTileX, targetTileX) + 1);
    int bottomTile = Math.Min(maxTilesY - 1, Math.Max(sourceTileY, targetTileY) + 1);
    if (rightTile < leftTile || bottomTile < topTile ||
        !LegacySpatialTileSnapshotAdapter.TryCapture(
          leftTile,
          topTile,
          rightTile - leftTile + 1,
          bottomTile - topTile + 1,
          out ImmutableArray<SpatialTileSnapshot> capturedTiles) ||
        !SpatialTileLookupSnapshot.TryCreate(capturedTiles, out SpatialTileLookupSnapshot lookup))
    {
      return null;
    }

    return SpatialCanHitQuery.TryEvaluate(
      sourceCenter,
      pointSize,
      pointSize,
      targetCenter,
      pointSize,
      pointSize,
      lookup,
      maxTilesX,
      maxTilesY,
      out bool canHit)
      ? canHit
      : null;
  }

  private static int GetCanHitTileCoordinate(
    float coordinate,
    int pointSize,
    int maximumTiles,
    bool isVertical)
  {
    int tile = ((int)coordinate + pointSize / 2) / SpatialTileSnapshot.TileSize;
    if (tile <= 1)
    {
      return 1;
    }

    int maximumCoordinate = isVertical ? maximumTiles - 40 : maximumTiles;
    return tile >= maximumCoordinate
      ? isVertical ? maximumCoordinate : maximumCoordinate - 1
      : tile;
  }
}
