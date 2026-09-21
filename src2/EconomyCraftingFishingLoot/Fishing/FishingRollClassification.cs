namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingRollClassification
{
  public FishingRollClassification(
    bool common,
    bool uncommon,
    bool rare,
    bool veryRare,
    bool legendary,
    bool crate,
    bool junk)
  {
    Common = common;
    Uncommon = uncommon;
    Rare = rare;
    VeryRare = veryRare;
    Legendary = legendary;
    Crate = crate;
    Junk = junk;
  }

  public bool Common { get; }

  public bool Uncommon { get; }

  public bool Rare { get; }

  public bool VeryRare { get; }

  public bool Legendary { get; }

  public bool Crate { get; }

  public bool Junk { get; }
}
