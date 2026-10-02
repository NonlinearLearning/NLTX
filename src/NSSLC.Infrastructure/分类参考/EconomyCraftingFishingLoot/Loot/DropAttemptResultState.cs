namespace NLTX.EconomyCraftingFishingLoot.Loot;

public enum DropAttemptResultState : byte
{
  Success,
  DoesntFillConditions,
  FailedRandomRoll,
  DidNotRunCode,
}
