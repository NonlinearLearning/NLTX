namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingPolicyDefinition
{
  private EmergencyStackingPolicyDefinition(
    int playerViewWidth,
    int playerViewHeight,
    int itemsToStackEachTime,
    IReadOnlyList<EmergencyStackingGroupDefinition> preservationOrder)
  {
    PlayerViewWidth = playerViewWidth;
    PlayerViewHeight = playerViewHeight;
    ItemsToStackEachTime = itemsToStackEachTime;
    PreservationOrder = Array.AsReadOnly(preservationOrder.ToArray());
  }

  public int PlayerViewWidth { get; }
  public int PlayerViewHeight { get; }
  public int ItemsToStackEachTime { get; }
  public IReadOnlyList<EmergencyStackingGroupDefinition> PreservationOrder { get; }

  public static EmergencyStackingPolicyDefinition CreateDefault()
  {
    const int defaultDistance = 160;
    EmergencyStackingGroupDefinition rareCurrency = new(
      "RareCurrency",
      stackingPriority: 4,
      defaultDistance / 4,
      EmergencyStackingPredicateCatalog.RareCurrency);
    EmergencyStackingGroupDefinition equipment = new(
      "Equipment",
      stackingPriority: 3,
      defaultDistance,
      EmergencyStackingPredicateCatalog.Equipment);
    EmergencyStackingGroupDefinition silverCoins = new(
      "SilverCoins",
      stackingPriority: 2,
      defaultDistance,
      EmergencyStackingPredicateCatalog.SilverCoins);
    EmergencyStackingGroupDefinition copperCoins = new(
      "CopperCoins",
      stackingPriority: 1,
      defaultDistance,
      EmergencyStackingPredicateCatalog.CopperCoins);
    EmergencyStackingGroupDefinition fallenStars = new(
      "FallenStars",
      stackingPriority: 0,
      defaultDistance * 4,
      EmergencyStackingPredicateCatalog.FallenStars);
    EmergencyStackingGroupDefinition defaultGroup = new(
      "Default",
      stackingPriority: -1,
      defaultDistance,
      EmergencyStackingPredicateCatalog.Default);

    return new EmergencyStackingPolicyDefinition(
      playerViewWidth: 2320,
      playerViewHeight: 1600,
      itemsToStackEachTime: 20,
      new[]
      {
        rareCurrency,
        equipment,
        silverCoins,
        copperCoins,
        fallenStars,
        defaultGroup
      });
  }

  public int GetPreservationIndex(ItemStackSnapshot item)
  {
    ArgumentNullException.ThrowIfNull(item);
    for (int index = 0; index < PreservationOrder.Count; index++)
    {
      if (PreservationOrder[index].Matches(item))
      {
        return index;
      }
    }

    return PreservationOrder.Count;
  }
}
