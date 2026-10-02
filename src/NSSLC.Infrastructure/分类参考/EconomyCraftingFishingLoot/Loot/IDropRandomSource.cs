namespace NLTX.EconomyCraftingFishingLoot.Loot;

public interface IDropRandomSource
{
  int Next(int exclusiveUpperBound);
}
