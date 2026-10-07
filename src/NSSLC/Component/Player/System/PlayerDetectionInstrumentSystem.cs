namespace Terraria.Player;

public static class PlayerDetectionInstrumentSystem
{
  private const int FishFinderAccessoryType = 3120;
  private const int WeatherAndInfoPdaAccessoryType = 3036;
  private const int ThirdEyeAccessoryType = 3084;
  private const int JarOfSoulsAccessoryType = 3095;
  private const int CritterGuideAccessoryType = 3118;
  private const int OreFinderAccessoryType = 3102;
  private const int DreamCatcherAccessoryType = 3119;
  private const int DetectionCompositeAccessoryType = 3122;
  private const int StopwatchCompositeAccessoryType = 3121;
  private const int CellPhoneAccessoryType = 3123;
  private const int ShellPhoneAccessoryType = 3124;
  private const int ShellPhoneSpawnAccessoryType = 5358;
  private const int ShellPhoneOceanAccessoryType = 5359;
  private const int ShellPhoneUnderworldAccessoryType = 5360;
  private const int ShellPhoneSpawnPointAccessoryType = 5361;

  public static PlayerDetectionInstrumentComponent CreateResetState()
  {
    return new PlayerDetectionInstrumentComponent();
  }

  public static void BeginFrame(
    ref PlayerDetectionInstrumentComponent state)
  {
    state = CreateResetState();
  }

  public static void ApplyAccessoryType(
    in PlayerDetectionInstrumentInput input,
    ref PlayerDetectionInstrumentComponent state)
  {
    if (IsFishFinderAccessory(input.AccessoryType))
    {
      state.FishFinderEnabled = true;
    }

    if (IsThirdEyeAccessory(input.AccessoryType))
    {
      state.ThirdEyeEnabled = true;
    }

    if (IsJarOfSoulsAccessory(input.AccessoryType))
    {
      state.JarOfSoulsEnabled = true;
    }

    if (IsCritterGuideAccessory(input.AccessoryType))
    {
      state.CritterGuideEnabled = true;
    }

    if (IsOreFinderAccessory(input.AccessoryType))
    {
      state.OreFinderEnabled = true;
    }

    if (IsDreamCatcherAccessory(input.AccessoryType))
    {
      state.DreamCatcherEnabled = true;
    }
  }

  private static bool IsFishFinderAccessory(int accessoryType)
  {
    return accessoryType == FishFinderAccessoryType ||
      accessoryType == WeatherAndInfoPdaAccessoryType ||
      IsPhoneAccessory(accessoryType);
  }

  private static bool IsThirdEyeAccessory(int accessoryType)
  {
    return accessoryType == ThirdEyeAccessoryType ||
      accessoryType == DetectionCompositeAccessoryType ||
      IsPhoneAccessory(accessoryType);
  }

  private static bool IsJarOfSoulsAccessory(int accessoryType)
  {
    return accessoryType == JarOfSoulsAccessoryType ||
      accessoryType == DetectionCompositeAccessoryType ||
      IsPhoneAccessory(accessoryType);
  }

  private static bool IsCritterGuideAccessory(int accessoryType)
  {
    return accessoryType == CritterGuideAccessoryType ||
      accessoryType == DetectionCompositeAccessoryType ||
      IsPhoneAccessory(accessoryType);
  }

  private static bool IsOreFinderAccessory(int accessoryType)
  {
    return accessoryType == OreFinderAccessoryType ||
      accessoryType == StopwatchCompositeAccessoryType ||
      IsPhoneAccessory(accessoryType);
  }

  private static bool IsDreamCatcherAccessory(int accessoryType)
  {
    return accessoryType == DreamCatcherAccessoryType ||
      accessoryType == StopwatchCompositeAccessoryType ||
      IsPhoneAccessory(accessoryType);
  }

  private static bool IsPhoneAccessory(int accessoryType)
  {
    return accessoryType == CellPhoneAccessoryType ||
      accessoryType == ShellPhoneAccessoryType ||
      accessoryType == ShellPhoneSpawnAccessoryType ||
      accessoryType == ShellPhoneOceanAccessoryType ||
      accessoryType == ShellPhoneUnderworldAccessoryType ||
      accessoryType == ShellPhoneSpawnPointAccessoryType;
  }
}
