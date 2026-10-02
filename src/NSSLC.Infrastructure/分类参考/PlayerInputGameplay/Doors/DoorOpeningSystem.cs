namespace NLTX.PlayerInputGameplay.Doors;

public sealed class DoorOpeningSystem
{
  public bool TryBeginOpening(
    DoorOpeningStateComponent state,
    DoorOpeningHandlerAdapter handlers,
    DoorOpeningRequest request)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(handlers);
    if (!handlers.TryGet(request.HandlerTileType, out var handler) || handler is null || !handler.CanOpen(request))
    {
      return false;
    }

    state.Add(new DoorOpenCloseTogglingInfo(
      request.Tile,
      request.HandlerTileType,
      request.IntendedOpeningDirection,
      request.PlayerGravityDirection));
    state.SetVelocityWindow(1);
    return true;
  }

  public void Advance(DoorOpeningStateComponent state, int ticks)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    for (var tick = 0; tick < ticks; tick++)
    {
      state.Tick();
    }
  }

  public int CompleteAll(DoorOpeningStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    var count = state.OngoingOpenDoors.Count;
    for (var index = count - 1; index >= 0; index--)
    {
      state.RemoveAt(index);
    }

    return count;
  }
}
