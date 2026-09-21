using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Server.Validation;

public enum ItemInteractionRejection
{
  None,
  PlayerNotActive,
  InvalidSelectedSlot
}

public readonly record struct ItemInteractionValidation(
  bool IsAccepted,
  ItemInteractionRejection Rejection)
{
  public static ItemInteractionValidation Reject(ItemInteractionRejection rejection)
  {
    return new ItemInteractionValidation(false, rejection);
  }
}

public sealed class ItemInteractionValidator
{
  public ItemInteractionValidation Validate(
    PlayerControlIntent intent,
    PlayerStateSnapshot player,
    InventoryComponent inventory)
  {
    if (!player.IsActive)
    {
      return ItemInteractionValidation.Reject(ItemInteractionRejection.PlayerNotActive);
    }

    if (intent.SelectedItem >= InventoryComponent.HotbarSlotCount)
    {
      return ItemInteractionValidation.Reject(ItemInteractionRejection.InvalidSelectedSlot);
    }

    return new ItemInteractionValidation(true, ItemInteractionRejection.None);
  }
}
