namespace Terraria.Player;

public static class PlayerSettingsAdapter
{
  public static PlayerDashControlPreference DefaultDashControl =>
    PlayerDashControlPreference.AllowDoubleTap;

  public static PlayerDashControlSettingsSnapshot ReadDashControl(
    in PlayerDashControlSettingsInput input)
  {
    return new PlayerDashControlSettingsSnapshot(
      input.DashControl ?? DefaultDashControl);
  }
}
