namespace NLTX.EconomyCraftingFishingLoot.Loot;

public enum DropChainTriggerKind : byte
{
  FailedRandomRoll,
  Succeeded,
  DoesntFillConditions,
  DidNotRunCode,
}
