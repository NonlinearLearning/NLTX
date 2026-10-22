namespace MinimalImplementation.Server.Inventory.Crafting;

/// <summary>Resolves Version4 item IDs and recipe-group IDs without owning inventory state.</summary>
public interface ICraftingMaterialCatalog
{
  bool IsSupportedRequirement(int itemIdOrRecipeGroup);

  bool Matches(int itemIdOrRecipeGroup, int itemType);
}
