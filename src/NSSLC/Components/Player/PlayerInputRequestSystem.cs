using System;
using Arch.System;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;
using Terraria.Entity;
using Terraria.Inventory;
using Terraria.Item;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>Applies the player state changes other domains asked for.</summary>
/// <remarks>
/// Registered at the start of the simulation phase, so positions and inventory slots taken from
/// inbound packets in the preparation phase are in place before any movement or attribute system
/// reads them — the same tick ordering packet handling had when it wrote components directly.
/// </remarks>
public sealed class PlayerInputRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeResolver _resolver;
  private readonly InventorySystem _inventories;

  public PlayerInputRequestSystem(
      ArchWorld ecsWorld,
      IEntityRuntimeResolver resolver,
      InventorySystem inventories)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    _inventories = inventories ?? throw new ArgumentNullException(nameof(inventories));
  }

  public override void Update(in EcsServerTickContext context)
  {
    // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
    PlayerInputRequestQueueComponent queue = PlayerInputRequestQueue.Require(World);
    IReadOnlyList<PlayerInputRequest> pending = queue.Drain();
    for (int index = 0; index < pending.Count; index++)
    {
      Apply(pending[index]);
    }
  }

  private void Apply(in PlayerInputRequest request)
  {
    if (!_resolver.TryResolve(request.Player, out ArchEntity player) ||
        player == ArchEntity.Null || !World.IsAlive(player))
    {
      return;
    }

    switch (request.Kind)
    {
      case PlayerInputRequestKind.SetPosition:
        if (World.Has<LocationComponent>(player))
        {
          World.Get<LocationComponent>(player).Coordinates = request.Position;
        }

        break;
      case PlayerInputRequestKind.SetInventorySlot:
        if (World.Has<InventoryComponent>(player))
        {
          ItemStack stack = default;
          // 与迁移前的 PlayerInventorySlot.IsEmpty 对齐：ItemType<=0 或 Stack<=0 都表示空槽。
          if (request.ItemType > 0 &&
              request.Stack > 0 &&
              ItemIdRules.TryFromNetworkType(request.ItemType, out ItemId itemId))
          {
            stack = new ItemStack(itemId, request.Stack, request.Prefix);
          }

          _inventories.TrySetSlotValue(player, request.Slot, stack);
        }

        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(request));
    }
  }
}
