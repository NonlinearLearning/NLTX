namespace MinimalImplementation.Server.Inventory.Protocol;

public sealed record CraftingMaterialRequirement(int ItemIdOrRecipeGroup, int Stack);

public sealed record CraftingModule11Request
{
  public IReadOnlyList<CraftingMaterialRequirement> Materials { get; }
  public IReadOnlyList<int> ChestIds { get; }

  public CraftingModule11Request(
    IReadOnlyCollection<CraftingMaterialRequirement> materials,
    IReadOnlyCollection<int> chestIds)
  {
    ArgumentNullException.ThrowIfNull(materials);
    ArgumentNullException.ThrowIfNull(chestIds);
    Materials = Array.AsReadOnly(materials.ToArray());
    ChestIds = Array.AsReadOnly(chestIds.ToArray());
  }
}

public sealed record CraftingModule11Response(bool Approved);
