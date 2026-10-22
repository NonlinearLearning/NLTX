namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>Resolves the Version4 item-sorting category used by smart QuickStack.</summary>
public interface IQuickStackCategoryResolver
{
  bool TryGetCategory(int itemType, out int category);
}
