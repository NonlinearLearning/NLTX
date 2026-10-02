namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackPlannerScratchState
{
  private readonly List<string> _blockedDestinationKeys = new();

  public QuickStackTypeIndexScratch TypeIndex { get; } = new();

  public IReadOnlyList<string> BlockedDestinationKeys =>
    _blockedDestinationKeys.AsReadOnly();

  public void Reset()
  {
    TypeIndex.Reset();
    _blockedDestinationKeys.Clear();
  }

  public void MarkBlocked(string containerKey)
  {
    if (string.IsNullOrWhiteSpace(containerKey))
    {
      throw new ArgumentException("A blocked destination requires a key.", nameof(containerKey));
    }

    if (!_blockedDestinationKeys.Contains(containerKey, StringComparer.Ordinal))
    {
      _blockedDestinationKeys.Add(containerKey);
    }
  }
}
