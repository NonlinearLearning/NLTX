namespace NLTX.EconomyCraftingFishingLoot.Crafting;

public sealed class CraftingRequestQueueState
{
  private readonly Queue<CraftingRequestCommand> _pendingRequests = [];

  public bool HasPendingRequests => _pendingRequests.Count > 0;

  public int PendingRequestCount => _pendingRequests.Count;

  public void Enqueue(CraftingRequestCommand request)
  {
    ArgumentNullException.ThrowIfNull(request);
    _pendingRequests.Enqueue(request);
  }

  public bool TryDequeue(out CraftingRequestCommand request)
  {
    return _pendingRequests.TryDequeue(out request!);
  }
}
