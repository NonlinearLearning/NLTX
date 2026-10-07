using Terraria.Items;

namespace Terraria.Player;

public readonly record struct PlayerInventoryCommitPlan(
  Guid CommandId,
  PlayerInventoryCommitTarget Target,
  ItemEntityRef IncomingItem,
  ItemEntityRef ExistingItem,
  int AcceptedStack,
  int RemainingStack,
  int ExistingStackBefore,
  int ExistingStackAfter,
  ItemMutationRevision IncomingMutationRevision = default,
  ItemMutationRevision ExistingMutationRevision = default)
{
  public bool AssignsEmptySlot => ExistingItem.IsEmpty;

  public bool StacksIntoExistingItem => !ExistingItem.IsEmpty;
}
