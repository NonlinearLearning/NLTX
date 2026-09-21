namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingGroupDefinition
{
  public EmergencyStackingGroupDefinition(
    string groupKey,
    int stackingPriority,
    int distanceStepSize,
    string predicateKey)
  {
    if (string.IsNullOrWhiteSpace(groupKey))
    {
      throw new ArgumentException("A group key is required.", nameof(groupKey));
    }

    if (distanceStepSize <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(distanceStepSize));
    }

    GroupKey = groupKey;
    StackingPriority = stackingPriority;
    DistanceStepSize = distanceStepSize;
    if (!EmergencyStackingPredicateCatalog.IsKnown(predicateKey))
    {
      throw new ArgumentException("The emergency-stack predicate is not registered.", nameof(predicateKey));
    }

    PredicateKey = predicateKey;
  }

  public string GroupKey { get; }
  public int StackingPriority { get; }
  public int DistanceStepSize { get; }
  public string PredicateKey { get; }

  public bool Matches(ItemStackSnapshot item)
  {
    return EmergencyStackingPredicateCatalog.Matches(PredicateKey, item);
  }
}
