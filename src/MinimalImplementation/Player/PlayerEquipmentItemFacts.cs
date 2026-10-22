namespace MinimalImplementation.Server.Player;

/// <summary>Item definition fields used to decide whether an inventory item grants equip effects.</summary>
public sealed record PlayerEquipmentItemFacts(
  bool IsAccessory,
  bool ExpertOnly,
  int HeadSlot,
  int BodySlot,
  int LegSlot);
