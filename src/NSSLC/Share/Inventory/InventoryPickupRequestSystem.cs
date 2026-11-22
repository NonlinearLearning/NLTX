using System;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Relationships;
using Terraria.World;

namespace Terraria.Inventory;

/// <summary>Applies the world items the World tick offered to player inventories.</summary>
/// <remarks>
/// The system only reports the quantity it actually committed. The World domain removes exactly
/// that many units through <see cref="WorldItemRequestKind.Consume"/>, so a rejected or partial
/// pickup leaves the remaining world item untouched instead of duplicating or destroying it.
/// </remarks>
public sealed class InventoryPickupRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeResolver _resolver;
  private readonly InventorySystem _inventories;
  private WorldItemRequestQueueComponent? _worldItemRequests;

  public InventoryPickupRequestSystem(
      ArchWorld ecsWorld,
      IEntityRuntimeResolver resolver,
      InventorySystem inventories)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    _inventories = inventories ?? throw new ArgumentNullException(nameof(inventories));
  }

  // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
  private WorldItemRequestQueueComponent WorldItemRequests =>
      _worldItemRequests ??= WorldItemRequestQueue.Require(World);

  public override void Update(in EcsServerTickContext context)
  {
    InventoryPickupRequestQueueComponent queue = InventoryPickupRequestQueue.Require(World);
    IReadOnlyList<InventoryPickupRequest> pending = queue.Drain();
    for (int index = 0; index < pending.Count; index++)
    {
      Apply(pending[index]);
    }
  }

  private void Apply(in InventoryPickupRequest request)
  {
    if (!request.Player.IsValid || request.Stack.IsEmpty ||
        !_resolver.TryResolve(request.Player, out ArchEntity player) ||
        player == ArchEntity.Null || !World.IsAlive(player))
    {
      return;
    }

    InventoryTransferResult result = _inventories.TryInsert(player, request.Stack);
    if (!result.Succeeded || result.Quantity <= 0)
    {
      return;
    }

    WorldItemRequests.Enqueue(new WorldItemRequest(
        WorldItemRequestKind.Consume,
        Slot: request.Slot,
        Actor: request.Actor,
        Stack: result.Quantity));
  }
}
