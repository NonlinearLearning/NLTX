using System.Collections.Generic;

namespace Terraria.Items;

public sealed class CraftingComponent
{
  private readonly List<CraftingMaterialReservation> _reservedMaterials;

  public CraftingComponent(
    int activeRecipeId = 0,
    int requestedQuantity = 0,
    long acceptedAtTick = 0,
    long materialInventoryRevision = 0,
    IReadOnlyList<CraftingMaterialReservation>? reservedMaterials = null,
    long craftSequence = 0)
  {
    ActiveRecipeId = activeRecipeId;
    RequestedQuantity = requestedQuantity;
    AcceptedAtTick = acceptedAtTick;
    MaterialInventoryRevision = materialInventoryRevision;
    _reservedMaterials = reservedMaterials is null
      ? []
      : new List<CraftingMaterialReservation>(reservedMaterials);
    CraftSequence = craftSequence;
  }

  public int ActiveRecipeId;
  public int RequestedQuantity;
  public long AcceptedAtTick;
  public long MaterialInventoryRevision;
  public long CraftSequence;

  public IReadOnlyList<CraftingMaterialReservation> ReservedMaterials => _reservedMaterials;
  public bool HasActiveCraft => ActiveRecipeId > 0 && RequestedQuantity > 0;
}
