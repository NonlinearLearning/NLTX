namespace NLTX.PlayerInputGameplay.Doors;

public interface IDoorOpeningHandler
{
  bool CanOpen(DoorOpeningRequest request);
}

public sealed class DoorOpeningHandlerAdapter
{
  private readonly Dictionary<int, IDoorOpeningHandler> _handlers = new();

  public void Register(int tileType, IDoorOpeningHandler handler)
  {
    if (tileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileType));
    }

    ArgumentNullException.ThrowIfNull(handler);
    _handlers[tileType] = handler;
  }

  public bool TryGet(int tileType, out IDoorOpeningHandler? handler)
  {
    return _handlers.TryGetValue(tileType, out handler);
  }
}
