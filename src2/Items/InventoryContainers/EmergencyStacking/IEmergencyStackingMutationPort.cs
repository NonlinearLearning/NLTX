namespace Terraria.Items.InventoryContainers;

public interface IEmergencyStackingMutationPort
{
  EmergencyStackingMutationResult TryTransfer(
    ItemIdentityAndStackComponent source,
    ItemIdentityAndStackComponent destination,
    int amount);
}
