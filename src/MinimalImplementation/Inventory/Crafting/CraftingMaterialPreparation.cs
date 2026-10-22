using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Crafting;

/// <summary>Pure material-check result; no chest state is changed during preparation.</summary>
public sealed record CraftingMaterialPreparation
{
  public CraftingMaterialPreparationStatus Status { get; }
  public IReadOnlyList<ChestItemMutationBatch> ChestMutations { get; }
  public int? RelatedMaterialIdOrRecipeGroup { get; }

  public bool IsPrepared => Status == CraftingMaterialPreparationStatus.Prepared;

  internal CraftingMaterialPreparation(
    CraftingMaterialPreparationStatus status,
    IReadOnlyCollection<ChestItemMutationBatch> chestMutations,
    int? relatedMaterialIdOrRecipeGroup = null)
  {
    ArgumentNullException.ThrowIfNull(chestMutations);
    Status = status;
    ChestMutations = Array.AsReadOnly(chestMutations.ToArray());
    RelatedMaterialIdOrRecipeGroup = relatedMaterialIdOrRecipeGroup;
  }
}
