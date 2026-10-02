namespace Terraria.Player;

public readonly record struct PlayerCoinMergePlan(
  Guid CommandId,
  int SourceSlotIndex,
  int DestinationSlotIndex,
  ItemEntityRef SourceItem,
  ItemEntityRef DestinationItem,
  int UpgradedSourceTypeId,
  int DestinationStackBefore,
  int DestinationStackAfter)
{
  public bool HasDestination => DestinationSlotIndex >= 0;
}
