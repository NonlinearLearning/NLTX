using System;
using System.Collections.Generic;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.World;

/// <summary>Applies chest, sign and tile entity intentions against the world table.</summary>
/// <remarks>
/// The system is the only writer of <see cref="WorldTileEntityStore"/> interaction state. It keeps
/// the store's ownership rules — one session per open chest, only the holder closes it, sign text
/// bounded — and always publishes one result per request so the caller learns the outcome without
/// reading the store.
/// </remarks>
public sealed class WorldTileEntityRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly WorldTileEntityStore _store;
  private WorldTileEntityResultQueueComponent? _results;

  public WorldTileEntityRequestSystem(ArchWorld world, WorldTileEntityStore store)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _store = store ?? throw new ArgumentNullException(nameof(store));
  }

  // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
  private WorldTileEntityResultQueueComponent Results
    => _results ??= WorldTileEntityResultQueue.Require(World);

  public override void Update(in EcsServerTickContext context)
  {
    WorldTileEntityRequestQueueComponent queue = WorldTileEntityRequestQueue.Require(World);
    IReadOnlyList<WorldTileEntityRequest> requests = queue.Drain();
    if (requests.Count == 0)
    {
      return;
    }

    WorldTileEntityResultQueueComponent results = Results;
    for (int index = 0; index < requests.Count; index++)
    {
      results.Enqueue(Apply(requests[index]));
    }
  }

  private WorldTileEntityResult Apply(in WorldTileEntityRequest request)
  {
    return request.Kind switch
    {
      WorldTileEntityRequestKind.OpenChest => ApplyOpenChest(request),
      WorldTileEntityRequestKind.CloseChest => ApplyCloseChest(request),
      WorldTileEntityRequestKind.EditSign => ApplyEditSign(request),
      _ => Result(request, WorldTileEntityResultKind.Rejected)
    };
  }

  private WorldTileEntityResult ApplyOpenChest(in WorldTileEntityRequest request)
  {
    if (!_store.TryGetChest(request.Position, out WorldChestState? chest) || chest is null)
    {
      return Result(request, WorldTileEntityResultKind.NotFound);
    }

    return Result(
        request,
        chest.TryOpen(request.Actor)
            ? WorldTileEntityResultKind.Applied
            : WorldTileEntityResultKind.NotOwner);
  }

  private WorldTileEntityResult ApplyCloseChest(in WorldTileEntityRequest request)
  {
    if (!_store.TryGetChest(request.Position, out WorldChestState? chest) || chest is null)
    {
      return Result(request, WorldTileEntityResultKind.NotFound);
    }

    return Result(
        request,
        chest.TryClose(request.Actor)
            ? WorldTileEntityResultKind.Applied
            : WorldTileEntityResultKind.NotOwner);
  }

  private WorldTileEntityResult ApplyEditSign(in WorldTileEntityRequest request)
  {
    if (!_store.TryGetSign(request.Position, out WorldSignState? sign) || sign is null)
    {
      return Result(request, WorldTileEntityResultKind.NotFound);
    }

    return Result(
        request,
        sign.TryEdit(request.Text)
            ? WorldTileEntityResultKind.Applied
            : WorldTileEntityResultKind.InvalidText);
  }

  private static WorldTileEntityResult Result(
      in WorldTileEntityRequest request,
      WorldTileEntityResultKind outcome)
    => new(request.Kind, outcome, request.Position, request.Actor);
}
