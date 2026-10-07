using Terraria.Items;

namespace Terraria.Player;

public readonly record struct PlayerCoinMergePlan(
  Guid CommandId,
  int SourceSlotIndex,
  int DestinationSlotIndex,
  ItemEntityRef SourceItem,
  ItemEntityRef DestinationItem,
  int UpgradedSourceTypeId,
  int DestinationStackBefore,
  int DestinationStackAfter,
  ItemMutationRevision SourceMutationRevision = default,
  ItemMutationRevision DestinationMutationRevision = default)
{
  public bool HasDestination => DestinationSlotIndex >= 0;
}
