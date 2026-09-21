namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public interface IFishingRandomSource
{
  int Next(int exclusiveUpperBound);
}
