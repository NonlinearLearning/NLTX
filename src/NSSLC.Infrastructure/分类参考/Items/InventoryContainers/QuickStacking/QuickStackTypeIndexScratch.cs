namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackTypeIndexScratch
{
  private readonly Dictionary<int, List<QuickStackDestinationSnapshot>> _destinationsByType = new();

  public int EntryCount => _destinationsByType.Count;

  public void Reset()
  {
    _destinationsByType.Clear();
  }

  public void Add(QuickStackDestinationSnapshot destination)
  {
    ArgumentNullException.ThrowIfNull(destination);
    if (destination.Item is null)
    {
      return;
    }

    if (!_destinationsByType.TryGetValue(destination.Item.ContentType, out List<QuickStackDestinationSnapshot>? destinations))
    {
      destinations = new List<QuickStackDestinationSnapshot>();
      _destinationsByType.Add(destination.Item.ContentType, destinations);
    }

    destinations.Add(destination);
  }

  public IReadOnlyList<QuickStackDestinationSnapshot> GetDestinations(int contentType)
  {
    if (!_destinationsByType.TryGetValue(contentType, out List<QuickStackDestinationSnapshot>? destinations))
    {
      return Array.Empty<QuickStackDestinationSnapshot>();
    }

    return Array.AsReadOnly(destinations.ToArray());
  }
}
