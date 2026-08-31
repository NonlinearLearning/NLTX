using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HardmodeOreTierSelectionPolicy
{
  private const int AlternateCobaltTileType = 221;
  private const int AlternateMythrilTileType = 222;
  private const int AlternateAdamantiteTileType = 223;

  public static HardmodeOreTierSelection Apply(
    AltarBreakProgression progression,
    HardmodeOreTierState state,
    bool isDrunkWorld,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    ValidateState(state);

    int cobalt = state.CobaltTileType;
    int mythril = state.MythrilTileType;
    int adamantite = state.AdamantiteTileType;
    bool wasToggled = false;
    if (isDrunkWorld && progression.CycleIndex == 1)
    {
      (mythril, wasToggled) = Toggle(mythril, 108, AlternateMythrilTileType);
    }

    if (isDrunkWorld && progression.CycleIndex == 2)
    {
      (int toggledCobalt, bool cobaltToggled) =
        Toggle(cobalt, 107, AlternateCobaltTileType);
      cobalt = toggledCobalt;
      wasToggled |= cobaltToggled;
      (int toggledAdamantite, bool adamantiteToggled) =
        Toggle(adamantite, 111, AlternateAdamantiteTileType);
      adamantite = toggledAdamantite;
      wasToggled |= adamantiteToggled;
    }

    bool wasInitialized = false;
    int selectedTileType;
    switch (progression.CycleIndex)
    {
      case 0:
        (cobalt, selectedTileType, wasInitialized) =
          InitializeIfNeeded(cobalt, 107, AlternateCobaltTileType, random);
        break;
      case 1:
        (mythril, selectedTileType, wasInitialized) =
          InitializeIfNeeded(mythril, 108, AlternateMythrilTileType, random);
        break;
      default:
        (adamantite, selectedTileType, wasInitialized) =
          InitializeIfNeeded(adamantite, 111, AlternateAdamantiteTileType, random);
        break;
    }

    return new HardmodeOreTierSelection(
      new HardmodeOreTierState(cobalt, mythril, adamantite),
      selectedTileType,
      wasInitialized,
      wasToggled);
  }

  private static (int Value, bool WasToggled) Toggle(
    int value,
    int defaultValue,
    int alternateValue)
  {
    if (value == defaultValue)
    {
      return (alternateValue, true);
    }

    if (value == alternateValue)
    {
      return (defaultValue, true);
    }

    return (value, false);
  }

  private static (int Value, int Selected, bool WasInitialized) InitializeIfNeeded(
    int value,
    int defaultValue,
    int alternateValue,
    LegacyPassRandomState random)
  {
    if (value != -1)
    {
      return (value, value, false);
    }

    int selected = random.Next(2) == 0 ? alternateValue : defaultValue;
    return (selected, selected, true);
  }

  private static void ValidateState(HardmodeOreTierState state)
  {
    ValidateTier(state.CobaltTileType, 107, AlternateCobaltTileType, nameof(state));
    ValidateTier(state.MythrilTileType, 108, AlternateMythrilTileType, nameof(state));
    ValidateTier(state.AdamantiteTileType, 111, AlternateAdamantiteTileType, nameof(state));
  }

  private static void ValidateTier(
    int value,
    int defaultValue,
    int alternateValue,
    string parameterName)
  {
    if (value != -1 && value != defaultValue && value != alternateValue)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
