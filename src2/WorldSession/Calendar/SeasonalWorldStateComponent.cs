namespace Terraria.WorldSession.Calendar;

public sealed class SeasonalWorldStateComponent
{
  public SeasonalWorldStateComponent(
    bool isChristmasActive = false,
    bool isHalloweenActive = false)
  {
    IsChristmasActive = isChristmasActive;
    IsHalloweenActive = isHalloweenActive;
  }

  public bool IsChristmasActive { get; internal set; }

  public bool IsHalloweenActive { get; internal set; }
}
