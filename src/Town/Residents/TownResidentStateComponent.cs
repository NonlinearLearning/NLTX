namespace Terraria.Town.Residents;

public sealed class TownResidentStateComponent
{
  public bool IsTownResident { get; private set; }

  public void SetResident(bool isResident)
  {
    IsTownResident = isResident;
  }

  public void Reset()
  {
    IsTownResident = false;
  }
}
