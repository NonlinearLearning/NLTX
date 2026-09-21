namespace Terraria.WorldSession.Calendar;

public sealed class SeasonalOverridePolicyComponent
{
  public SeasonalOverridePolicyComponent(
    bool forceChristmasToday = false,
    bool forceHalloweenToday = false,
    bool forceChristmasForever = false,
    bool forceHalloweenForever = false)
  {
    ForceChristmasToday = forceChristmasToday;
    ForceHalloweenToday = forceHalloweenToday;
    ForceChristmasForever = forceChristmasForever;
    ForceHalloweenForever = forceHalloweenForever;
  }

  public bool ForceChristmasToday { get; internal set; }

  public bool ForceHalloweenToday { get; internal set; }

  public bool ForceChristmasForever { get; internal set; }

  public bool ForceHalloweenForever { get; internal set; }
}
