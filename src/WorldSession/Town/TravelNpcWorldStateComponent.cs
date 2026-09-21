namespace Terraria.WorldSession.Town;

public sealed class TravelNpcWorldStateComponent
{
  public bool IsTravelNpcActive { get; private set; }

  public void SetTravelNpcActive(bool isActive)
  {
    IsTravelNpcActive = isActive;
  }

  public void Reset()
  {
    IsTravelNpcActive = false;
  }
}
