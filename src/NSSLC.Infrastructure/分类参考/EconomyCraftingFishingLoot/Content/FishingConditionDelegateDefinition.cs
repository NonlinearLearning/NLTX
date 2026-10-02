using NLTX.EconomyCraftingFishingLoot.Fishing;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class FishingConditionDelegateDefinition
  : IFishingConditionDefinition
{
  private readonly Func<FishingConditionEvaluationContext, bool> _match;

  public FishingConditionDelegateDefinition(
    string name,
    Func<FishingConditionEvaluationContext, bool> match,
    FishingConditionDisplayMetadata displayMetadata)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentNullException.ThrowIfNull(match);
    ArgumentNullException.ThrowIfNull(displayMetadata);

    Name = name;
    _match = match;
    DisplayMetadata = displayMetadata;
  }

  public string Name { get; }

  public FishingConditionDisplayMetadata DisplayMetadata { get; }

  public bool Matches(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return _match.Invoke(context);
  }
}
