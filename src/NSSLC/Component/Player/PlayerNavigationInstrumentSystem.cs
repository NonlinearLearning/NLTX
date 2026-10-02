namespace Terraria.Player;

public static class PlayerNavigationInstrumentSystem
{
  private const int FirstWatchAccessoryType = 15;
  private const int FirstWatchAliasAccessoryType = 707;
  private const int SecondWatchAccessoryType = 16;
  private const int SecondWatchAliasAccessoryType = 708;
  private const int ThirdWatchAccessoryType = 17;
  private const int ThirdWatchAliasAccessoryType = 709;
  private const int CompassAccessoryType = 393;
  private const int DepthMeterAccessoryType = 18;
  private const int CombinedNavigationAccessoryType = 395;
  private const int WeatherRadioAccessoryType = 3037;
  private const int CalendarAccessoryType = 3096;
  private const int StopwatchAccessoryType = 3099;
  private const int PdaAccessoryType = 3036;
  private const int StopwatchCompositeAccessoryType = 3121;
  private const int CellPhoneAccessoryType = 3123;
  private const int ShellPhoneAccessoryType = 3124;
  private const int ShellPhoneSpawnAccessoryType = 5358;
  private const int ShellPhoneOceanAccessoryType = 5359;
  private const int ShellPhoneUnderworldAccessoryType = 5360;
  private const int ShellPhoneSpawnPointAccessoryType = 5361;

  public static PlayerNavigationInstrumentComponent CreateResetState()
  {
    return new PlayerNavigationInstrumentComponent();
  }

  public static void BeginFrame(
    ref PlayerNavigationInstrumentComponent state)
  {
    state = CreateResetState();
  }

  public static void ApplyAccessoryType(
    in PlayerNavigationInstrumentInput input,
    ref PlayerNavigationInstrumentComponent state)
  {
    switch (input.AccessoryType)
    {
      case FirstWatchAccessoryType:
      case FirstWatchAliasAccessoryType:
        state.WatchLevel = Math.Max(state.WatchLevel, 1);
        break;
      case SecondWatchAccessoryType:
      case SecondWatchAliasAccessoryType:
        state.WatchLevel = Math.Max(state.WatchLevel, 2);
        break;
      case ThirdWatchAccessoryType:
      case ThirdWatchAliasAccessoryType:
        state.WatchLevel = Math.Max(state.WatchLevel, 3);
        break;
      case CompassAccessoryType:
        state.CompassLevel = 1;
        break;
      case DepthMeterAccessoryType:
        state.DepthMeterLevel = 1;
        break;
      case CombinedNavigationAccessoryType:
      case CellPhoneAccessoryType:
      case ShellPhoneAccessoryType:
      case ShellPhoneSpawnAccessoryType:
      case ShellPhoneOceanAccessoryType:
      case ShellPhoneUnderworldAccessoryType:
      case ShellPhoneSpawnPointAccessoryType:
        state.WatchLevel = Math.Max(state.WatchLevel, 3);
        state.DepthMeterLevel = 1;
        state.CompassLevel = 1;
        break;
    }

    if (IsWeatherRadioAccessory(input.AccessoryType))
    {
      state.WeatherRadioEnabled = true;
    }

    if (IsCalendarAccessory(input.AccessoryType))
    {
      state.CalendarEnabled = true;
    }

    if (IsStopwatchAccessory(input.AccessoryType))
    {
      state.StopwatchEnabled = true;
    }
  }

  private static bool IsWeatherRadioAccessory(int accessoryType)
  {
    return accessoryType == WeatherRadioAccessoryType ||
      IsPdaOrNavigationPhoneAccessory(accessoryType);
  }

  private static bool IsCalendarAccessory(int accessoryType)
  {
    return accessoryType == CalendarAccessoryType ||
      IsPdaOrNavigationPhoneAccessory(accessoryType);
  }

  private static bool IsStopwatchAccessory(int accessoryType)
  {
    return accessoryType == StopwatchAccessoryType ||
      accessoryType == StopwatchCompositeAccessoryType ||
      IsPdaOrNavigationPhoneAccessory(accessoryType);
  }

  private static bool IsPdaOrNavigationPhoneAccessory(int accessoryType)
  {
    return accessoryType == PdaAccessoryType ||
      accessoryType == CellPhoneAccessoryType ||
      accessoryType == ShellPhoneAccessoryType ||
      accessoryType == ShellPhoneSpawnAccessoryType ||
      accessoryType == ShellPhoneOceanAccessoryType ||
      accessoryType == ShellPhoneUnderworldAccessoryType ||
      accessoryType == ShellPhoneSpawnPointAccessoryType;
  }
}
