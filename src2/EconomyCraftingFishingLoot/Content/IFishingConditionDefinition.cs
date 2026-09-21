using NLTX.EconomyCraftingFishingLoot.Fishing;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public interface IFishingConditionDefinition
{
  FishingConditionDisplayMetadata DisplayMetadata { get; }

  bool Matches(FishingConditionEvaluationContext context);
}
