using NLTX.EconomyCraftingFishingLoot.Fishing;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class FishingQuestConditionDefinition
  : IFishingConditionDefinition
{
  public FishingQuestConditionDefinition(
    int checkedType,
    bool isRemixVariant,
    FishingConditionDisplayMetadata? displayMetadata = null)
  {
    CheckedType = checkedType;
    IsRemixVariant = isRemixVariant;
    DisplayMetadata = displayMetadata ??
      new FishingConditionDisplayMetadata(canBeSkippedForDisplay: false);
  }

  public int CheckedType { get; }

  public bool IsRemixVariant { get; }

  public FishingConditionDisplayMetadata DisplayMetadata { get; }

  public bool Matches(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.QuestFishType == CheckedType;
  }
}
