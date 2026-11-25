using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Unshared.World;

/// <summary>
/// Authorizes entities with WorldInteractionCapabilityComponent before calling the world port.
/// </summary>
/// <remarks>
/// The system is intended for the NSSLC Commit phase. It consumes one pending request per entity
/// through the Arch World query API and writes the result component in place. The tile map remains
/// owned by the world owner; reads use IWorldTileReader and writes use IWorldInteractionWorld.
/// </remarks>
public sealed class WorldInteractionSystem : BaseSystem<ArchWorld, EcsServerTickContext> {
  private readonly IWorldTileReader _tileReader;
  private readonly IWorldInteractionWorld _world;

  public WorldInteractionSystem(
      ArchWorld ecsWorld,
      IWorldTileReader tileReader,
      IWorldInteractionWorld world)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld))) {
    ArgumentNullException.ThrowIfNull(tileReader);
    ArgumentNullException.ThrowIfNull(world);
    _tileReader = tileReader;
    _world = world;
  }

  /// <summary>
  /// Consumes pending requests from entities that already carry a result component.
  /// </summary>
  /// <remarks>
  /// Register this system in <see cref="EcsServerPhase.Commit"/> when the request should be
  /// applied after simulation. No entity or component structure is changed by this method.
  /// </remarks>
  public override void Update(in EcsServerTickContext context) {
    long tick = context.Tick;
    QueryDescription query = new QueryDescription()
        .WithAll<WorldInteractionRequestComponent>()
        .WithAll<WorldInteractionResultComponent>();

    World.Query(
        in query,
        (ArchEntity entity,
            ref WorldInteractionRequestComponent request,
            ref WorldInteractionResultComponent result) => {
          if (!request.IsPending) {
            return;
          }

          result = request.Tick == tick
              ? ProcessRequest(entity, in request)
              : WorldInteractionResultComponent.Rejected(
                  in request,
                  "The world request does not belong to the current server tick.");
          request.MarkConsumed();
        });
  }

  /// <summary>Issues a token limited to permissions currently held by the entity.</summary>
  public bool TryAcquire(
      ArchEntity entity,
      WorldInteractionPermission requested,
      out WorldInteractionAccess access) {
    access = default;
    if (!WorldInteractionPermissionRules.IsKnown(requested) || !TryGetCapability(entity, out
            WorldInteractionCapabilityComponent capability) || !capability.Allows(requested)) {
      return false;
    }

    access = new WorldInteractionAccess(entity, requested);
    return true;
  }

  public bool TryReadTile(
      WorldInteractionAccess access,
      WorldTileCoordinate coordinate,
      out WorldTileState tile) {
    if (!TryValidateAccess(access, WorldInteractionPermission.ReadTile)) {
      tile = default;
      return false;
    }

    return _tileReader.TryReadTile(coordinate, out tile);
  }

  public WorldInteractionResult TryBreakTile(
      WorldInteractionAccess access,
      WorldTileCoordinate coordinate) {
    if (!TryValidateAccess(access, WorldInteractionPermission.BreakTile)) {
      return WorldInteractionResult.Rejected(
          "The entity does not have permission to break tiles.");
    }

    return _world.TryBreakTile(coordinate);
  }

  public WorldInteractionResult TryPlaceTile(
      WorldInteractionAccess access,
      WorldTileCoordinate coordinate,
      ushort tileType,
      byte style = 0) {
    if (!TryValidateAccess(access, WorldInteractionPermission.PlaceTile)) {
      return WorldInteractionResult.Rejected(
          "The entity does not have permission to place tiles.");
    }

    return _world.TryPlaceTile(coordinate, tileType, style);
  }

  private bool TryValidateAccess(
      WorldInteractionAccess access,
      WorldInteractionPermission required) {
    return access.Allows(required) &&
        TryGetCapability(access.Entity, out WorldInteractionCapabilityComponent capability) &&
        capability.Allows(required);
  }

  private bool TryGetCapability(
      ArchEntity entity,
      out WorldInteractionCapabilityComponent capability) {
    capability = default;
    if (entity.WorldId != World.Id || !World.IsAlive(entity) ||
        !World.TryGet(entity, out capability)) {
      return false;
    }

    return WorldInteractionPermissionRules.IsKnown(capability.Permissions);
  }

  private WorldInteractionResultComponent ProcessRequest(
      ArchEntity entity,
      in WorldInteractionRequestComponent request) {
    WorldInteractionPermission permission =
        WorldInteractionPermissionRules.RequiredFor(request.Operation);

    if (permission == WorldInteractionPermission.None ||
        !TryAcquire(entity, permission, out WorldInteractionAccess access)) {
      return WorldInteractionResultComponent.Rejected(
          in request,
          "The entity does not have permission for the requested world operation.");
    }

    switch (request.Operation) {
      case WorldInteractionOperation.ReadTile:
        if (TryReadTile(access, request.Coordinate, out WorldTileState tile)) {
          return WorldInteractionResultComponent.Success(in request, tile);
        }

        return WorldInteractionResultComponent.Rejected(
            in request,
            "The world rejected the tile read request.");
      case WorldInteractionOperation.BreakTile:
        return ToComponentResult(request, TryBreakTile(access, request.Coordinate));
      case WorldInteractionOperation.PlaceTile:
        return ToComponentResult(
            request,
            TryPlaceTile(access, request.Coordinate, request.TileType, request.Style));
      default:
        return WorldInteractionResultComponent.Rejected(
            in request,
            "The requested world operation is not supported.");
    }
  }

  private static WorldInteractionResultComponent ToComponentResult(
      in WorldInteractionRequestComponent request,
      WorldInteractionResult operationResult) {
    return operationResult.Succeeded
        ? WorldInteractionResultComponent.Success(in request)
        : WorldInteractionResultComponent.Rejected(
            in request,
            operationResult.Failure ?? "The world rejected the operation.");
  }
}
