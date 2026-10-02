namespace Terraria.Items.InventoryContainers;

public static class ChestMetadataPolicy
{
  public const int MaxNameLength = 20;

  public static bool IsValidName(string? name)
  {
    return name is null || name.Length <= MaxNameLength;
  }
}
