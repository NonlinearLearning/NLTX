namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingAttemptLocationContext
{
  public FishingAttemptLocationContext(int x, int y, int bobberType)
  {
    X = x;
    Y = y;
    BobberType = bobberType;
  }

  public int X { get; }

  public int Y { get; }

  public int BobberType { get; }
}
