namespace MinimalImplementation.Server.Player;

/// <summary>
/// Effective pickup inputs from authoritative equipment, inventory, animation and creative
/// power owners. Effective maxima are statLifeMax2/statManaMax2, not uploaded base maxima.
/// JourneyFarPlacementEnabled includes both Journey difficulty and the actual enabled power.
/// </summary>
public sealed record PlayerPickupSourceFacts(
  bool PreventAllItemPickups,
  bool VoidVaultEnabled,
  bool HeldItemIsEmpty,
  int ItemAnimation,
  int? EffectiveMaximumLife,
  int? EffectiveMaximumMana,
  bool GoldRing,
  bool ManaMagnet,
  bool LifeMagnet,
  bool TreasureMagnet,
  bool JourneyFarPlacementEnabled) {
  /// <summary>
  /// False when an ownership capture lacks the selected item or authoritative animation.
  /// HeldItemIsEmpty and ItemAnimation then cannot authorize inventory pickup.
  /// </summary>
  public bool ItemUseStateKnown => ItemAnimation >= 0;

  public bool CanAcceptItem(int type) {
    bool ignoresEncumberingStone = type is 58 or 184 or 1734 or 1735 or 1867 or 1868
      or 3453 or 3454 or 3455 or 4143;
    return ItemUseStateKnown && (!HeldItemIsEmpty || ItemAnimation <= 0)
      && (!PreventAllItemPickups || ignoresEncumberingStone);
  }

  /// <summary>Pull range; immediate inventory credit still requires actual hitbox intersection.</summary>
  public int GetItemGrabRange(int type) {
    int range = 42;
    if (GoldRing && type is >= 71 and <= 74) {
      range += 350;
    }
    if ((ManaMagnet && type is (184 or 1735 or 1868)) || type == 4143) {
      range += 300;
    }
    if (LifeMagnet && type is (58 or 1734 or 1867)) {
      range += 250;
    }
    if (TreasureMagnet) {
      range += 150;
    }
    if (type == 3822) {
      range += 50;
    }
    if (type is 3453 or 3454 or 3455) {
      range += 100;
    }
    if (JourneyFarPlacementEnabled) {
      range += 240;
    }
    return range;
  }
}
