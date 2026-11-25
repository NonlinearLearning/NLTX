using System;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Share.Items;

/// <summary>Applies the world-item changes other domains requested at the Commit barrier.</summary>
/// <remarks>
/// Projection systems may enqueue intents, but this system is the sole owner that mutates the
/// authoritative <see cref="ItemStore"/>. Requests emitted during Projection are therefore
/// drained on the next tick's Commit pass.
/// </remarks>
public sealed class ItemSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ItemStore _items;

  public ItemSystem(ArchWorld ecsWorld, ItemStore items)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _items = items ?? throw new ArgumentNullException(nameof(items));
  }

  public override void Update(in EcsServerTickContext context)
  {
    // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
    ItemRequestQueueComponent queue = ItemRequestQueue.Require(World);
    IReadOnlyList<ItemRequest> requests = queue.Drain();
    for (int index = 0; index < requests.Count; index++)
    {
      Apply(requests[index]);
    }
  }

  private void Apply(in ItemRequest request)
  {
    switch (request.Kind)
    {
      case ItemRequestKind.Create:
        _items.TryCreate(
            request.ItemType,
            request.Stack,
            request.Prefix,
            request.Position,
            request.Velocity,
            request.ReservedFor,
            request.NoGrabDelay,
            request.Shimmered,
            out _);
        break;
      case ItemRequestKind.Update:
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
      case ItemRequestKind.Release:
        _items.TryRelease(request.Slot, request.Actor);
        break;
      case ItemRequestKind.Retire:
        _items.TryRetire(request.Slot, request.Actor, out _);
        break;
      case ItemRequestKind.Consume:
        _items.TryConsume(request.Slot, request.Actor, request.Stack, out _);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(request));
    }
  }
}
