namespace NLTX.PlayerInputGameplay.Doors;

public sealed class DoorOpeningStateComponent
{
  private readonly List<DoorOpenCloseTogglingInfo> _ongoingOpenDoors = new();

  public int TimeWeCanOpenDoorsUsingVelocityAlone { get; private set; }

  public IReadOnlyList<DoorOpenCloseTogglingInfo> OngoingOpenDoors => _ongoingOpenDoors;

  internal void Add(DoorOpenCloseTogglingInfo info)
  {
    _ongoingOpenDoors.Add(info);
  }

  internal void RemoveAt(int index)
  {
    _ongoingOpenDoors.RemoveAt(index);
  }

  internal void SetVelocityWindow(int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    TimeWeCanOpenDoorsUsingVelocityAlone = ticks;
  }

  internal void Tick()
  {
    if (TimeWeCanOpenDoorsUsingVelocityAlone > 0)
    {
      TimeWeCanOpenDoorsUsingVelocityAlone--;
    }
  }
}
