namespace NLTX.EconomyCraftingFishingLoot.Loot;

public readonly record struct DropChainVisibilityPolicy
{
  public DropChainVisibilityPolicy(bool hideLootReport = false)
  {
    HideLootReport = hideLootReport;
  }

  public bool HideLootReport { get; }
}
