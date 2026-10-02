using System;
using Terraria.Player.Combat;

namespace Terraria.Player.Environment;

// Candidate rules from the complete reference project; Version4 behavior remains unconfirmed.
public static class PlayerSunScorchSystem
{
  private const int MaximumTileScanCount = 15;
  private const int SpecialMountType = 56;
  private const int FirstSunUmbrellaItemType = 946;
  private const int SecondSunUmbrellaItemType = 4707;
  private const int SunlightPassThroughWallTypeOne = 21;
  private const int SunlightPassThroughWallTypeTwo = 318;
  private const int SunlightShieldBlockType = 54;
  private const int InvisibleBlockType = 541;
  private const float MoonLordSkyIntensityThreshold = 0.5f;
  private const int ScorchThreshold = 120;
  private const int MaximumScorchCounter = 300;
  private const int NormalCounterDecay = 6;
  private const int DeadCounterDecay = 2;

  public static PlayerSunScorchResult UpdateLocal(
    in PlayerSunScorchInput input,
    PlayerEnvironmentalPressureComponent pressure,
    PlayerArmorAndCombatEffectsComponent armorEffects)
  {
    ArgumentNullException.ThrowIfNull(pressure);
    ArgumentNullException.ThrowIfNull(armorEffects);

    int previousCounter = pressure.SunScorchCounter;
    if (!input.IsLocalPlayer)
    {
      return new PlayerSunScorchResult(
        Updated: false,
        IsBurningInSunlight: armorEffects.VampireBurningInSunlight,
        PreviousCounter: previousCounter,
        CurrentCounter: previousCounter,
        SizzleVolume: 0f,
        RequestedEffects: PlayerSunScorchEffectRequest.None);
    }

    ArgumentNullException.ThrowIfNull(input.TileFactsFromFeetUp);

    bool exposed = IsExposedToSunlight(input);
    bool burning = exposed;
    armorEffects.VampireBurningInSunlight = burning;

    int delta = burning ? 1 : -NormalCounterDecay;
    int currentCounter = AdvanceCounter(previousCounter, delta);
    pressure.SunScorchCounter = currentCounter;

    bool scorching = burning && currentCounter >= ScorchThreshold;
    bool crossedThreshold = scorching && previousCounter < ScorchThreshold;
    PlayerSunScorchEffectRequest effects = PlayerSunScorchEffectRequest.None;
    if (scorching)
    {
      if (crossedThreshold)
      {
        effects |= PlayerSunScorchEffectRequest.RefreshArmorFrameAndAchievement;
      }

      effects |= PlayerSunScorchEffectRequest.ClearBuffImmunity |
        PlayerSunScorchEffectRequest.AddOnFireBuff |
        PlayerSunScorchEffectRequest.AddCursedInfernoBuff |
        PlayerSunScorchEffectRequest.AddShadowFlameBuff |
        PlayerSunScorchEffectRequest.ClearWings |
        PlayerSunScorchEffectRequest.ClearRocketBoots;

      if (!input.OnFire)
      {
        effects |= PlayerSunScorchEffectRequest.SpawnVampireOnFireParticle;
      }

      if (input.MountActive)
      {
        effects |= PlayerSunScorchEffectRequest.Dismount;
      }
    }

    return new PlayerSunScorchResult(
      Updated: true,
      IsBurningInSunlight: burning,
      PreviousCounter: previousCounter,
      CurrentCounter: currentCounter,
      SizzleVolume: Math.Clamp(currentCounter / (float) ScorchThreshold, 0f, 1f),
      RequestedEffects: effects);
  }

  public static PlayerSunScorchResult UpdateDead(
    PlayerEnvironmentalPressureComponent pressure,
    PlayerArmorAndCombatEffectsComponent armorEffects)
  {
    ArgumentNullException.ThrowIfNull(pressure);
    ArgumentNullException.ThrowIfNull(armorEffects);

    int previousCounter = pressure.SunScorchCounter;
    armorEffects.VampireBurningInSunlight = false;
    int currentCounter = AdvanceCounter(previousCounter, -DeadCounterDecay);
    pressure.SunScorchCounter = currentCounter;

    return new PlayerSunScorchResult(
      Updated: true,
      IsBurningInSunlight: false,
      PreviousCounter: previousCounter,
      CurrentCounter: currentCounter,
      SizzleVolume: Math.Clamp(currentCounter / (float) ScorchThreshold, 0f, 1f),
      RequestedEffects: PlayerSunScorchEffectRequest.None);
  }

  private static int AdvanceCounter(int previousCounter, int delta)
  {
    return Math.Clamp(previousCounter + delta, 0, MaximumScorchCounter);
  }

  private static bool IsExposedToSunlight(in PlayerSunScorchInput input)
  {
    if (!input.VampireSeed || input.FeetTileY >= input.WorldSurface || !input.DayTime ||
      input.Raining || input.Eclipse || input.ZoneGraveyard || input.ZoneGlowshroom || input.Wet)
    {
      return false;
    }

    if (input.HasMoonLordSkyIntensity &&
      input.MoonLordSkyIntensity > MoonLordSkyIntensityThreshold)
    {
      return false;
    }

    bool selectedSunUmbrella = input.SelectedItemType == FirstSunUmbrellaItemType ||
      input.SelectedItemType == SecondSunUmbrellaItemType;
    if (selectedSunUmbrella && !(input.MountActive && input.MountType == SpecialMountType))
    {
      return false;
    }

    int scanCount = Math.Min(input.TileFactsFromFeetUp.Count, MaximumTileScanCount);
    for (int index = 0; index < scanCount; index++)
    {
      PlayerSunScorchTileFact tile = input.TileFactsFromFeetUp[index];
      if (!tile.Exists)
      {
        break;
      }

      bool openWall = tile.WallType == 0 || tile.WallType == SunlightPassThroughWallTypeOne ||
        tile.WallType == SunlightPassThroughWallTypeTwo ||
        (!input.ShouldShowInvisibleBlocksAndWalls && tile.InvisibleWall);
      if (openWall)
      {
        return true;
      }

      bool blocksSunlight = tile.Solid && tile.Type != SunlightShieldBlockType &&
        (!tile.InvisibleBlock || input.ShouldShowInvisibleBlocksAndWalls) &&
        (tile.Type != InvisibleBlockType || input.ShouldShowInvisibleBlocksAndWalls);
      if (blocksSunlight)
      {
        break;
      }
    }

    return false;
  }
}
