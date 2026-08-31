using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Player.Components;
using ArchWorld = Arch.Core.World;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerSleepSystem
{
  public void Advance(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    ItemDefinitionRegistry itemDefinitions)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(itemDefinitions);
    foreach (KeyValuePair<PlayerHandle, Entity> entry in players)
    {
      Entity playerEntity = entry.Value;
      if (!entry.Key.IsValid ||
          !world.IsAlive(playerEntity) ||
          !world.Has<PlayerTagComponent>(playerEntity) ||
          !world.Has<PlayerIdentityComponent>(playerEntity) ||
          world.Get<PlayerIdentityComponent>(playerEntity).Player != entry.Key ||
          !world.Has<PlayerLifecycleComponent>(playerEntity) ||
          !world.Get<PlayerLifecycleComponent>(playerEntity).IsActive ||
          !world.Has<PlayerSleepComponent>(playerEntity))
      {
        continue;
      }

      ref PlayerSleepComponent sleep = ref world.Get<PlayerSleepComponent>(playerEntity);
      if (!sleep.IsSleeping)
      {
        continue;
      }

      sleep.Advance();
      if (!world.Has<InventoryComponent>(playerEntity) ||
          !world.Has<ItemUseStateComponent>(playerEntity))
      {
        continue;
      }

      InventoryComponent inventory = world.Get<InventoryComponent>(playerEntity);
      ItemStack heldItem = inventory.GetSlot(inventory.SelectedSlot);
      if (heldItem.IsEmpty ||
          !itemDefinitions.TryGet(heldItem.ItemType, out ItemDefinition definition))
      {
        continue;
      }

      ItemUseStateComponent itemUse = world.Get<ItemUseStateComponent>(playerEntity);
      PlayerSleepWakeInput wakeInput = new(
        sleep.IsSleeping,
        itemUse.AnimationTicks,
        definition.Combat?.Damage ?? 0,
        definition.IsNoMelee);
      if (PlayerSleepWakePolicy.ShouldWakeForItemUse(wakeInput))
      {
        sleep.StopSleeping(PlayerSleepWakeReason.MeleeItem);
      }
    }
  }
}
