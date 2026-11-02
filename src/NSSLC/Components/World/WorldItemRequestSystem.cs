using System;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.World;

/// <summary>Applies the world-item changes other domains requested.</summary>
/// <remarks>
/// Registered twice per tick: once before the network projection, so a request raised while
/// handling an inbound packet in the preparation phase is applied in the same tick; and once after
/// it, so a drop requested by the projection itself is also applied in that same tick. Both passes
/// drain the same queue, and a drained queue is empty, so running it twice is harmless.
/// </remarks>
public sealed class WorldItemRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly WorldItemStore _items;

  public WorldItemRequestSystem(ArchWorld ecsWorld, WorldItemStore items)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _items = items ?? throw new ArgumentNullException(nameof(items));
  }

  public override void Update(in EcsServerTickContext context)
  {
    // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
    WorldItemRequestQueueComponent queue = WorldItemRequestQueue.Require(World);
    IReadOnlyList<WorldItemRequest> requests = queue.Drain();
    for (int index = 0; index < requests.Count; index++)
    {
      Apply(requests[index]);
    }
  }

  private void Apply(in WorldItemRequest request)
  {
    switch (request.Kind)
    {
      case WorldItemRequestKind.Create:
        _items.TryCreate(
            request.ItemType,
            request.Stack,
            request.Prefix,
            request.Position,
            request.Velocity,
            request.ReservedFor,
            out _);
        break;
      case WorldItemRequestKind.Update:
        _items.TryUpdate(
            request.Slot,
            request.Actor,
            request.ItemType,
            request.Stack,
            request.Prefix,
            request.Position,
            request.Velocity,
            out _);
        break;
      case WorldItemRequestKind.Release:
        _items.TryRelease(request.Slot, request.Actor);
        break;
      case WorldItemRequestKind.Retire:
        _items.TryRetire(request.Slot, request.Actor, out _);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(request));
    }
  }
}
