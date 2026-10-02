using System.Numerics;

namespace Terraria.Player;

public static class PlayerInteractionAndSelectionPropertiesQuery
{
  private const int FloatingMountType = 37;

  public static PlayerInteractionAndSelectionPropertiesSnapshot Evaluate(
    in PlayerInteractionAndSelectionPropertiesInput input)
  {
    ItemEntityRef heldItem = GetHeldItem(input.Inventory, input.SelectedItem);
    bool shouldFloatInWater = input.CanFloatInWater &&
      !input.ControlDown &&
      (!input.MountActive || input.MountType?.Value == FloatingMountType);
    bool canBeTalkedTo = input.Active &&
      !input.Dead &&
      !input.ShouldNotDraw &&
      input.Stealth == 1.0f;
    Vector2 reportedCameraPosition = input.NetCameraTarget ?? input.Position;

    return new PlayerInteractionAndSelectionPropertiesSnapshot(
      new Vector2(input.Direction, input.GravityDirection),
      input.SelectedItem,
      heldItem,
      shouldFloatInWater,
      canBeTalkedTo,
      input.IsVoidVaultEnabled,
      reportedCameraPosition,
      input.ControlUp || input.TryKeepingHoveringUp,
      input.ControlDown || input.TryKeepingHoveringDown);
  }

  private static ItemEntityRef GetHeldItem(
    IReadOnlyList<ItemEntityRef> inventory,
    int selectedItem)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    return (uint)selectedItem < (uint)inventory.Count
      ? inventory[selectedItem]
      : ItemEntityRef.None;
  }
}
