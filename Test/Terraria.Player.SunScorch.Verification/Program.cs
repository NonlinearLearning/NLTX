using Terraria.Player;
using Terraria.Player.Combat;
using Terraria.Player.Environment;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

var pressure = new PlayerEnvironmentalPressureComponent();
var armorEffects = new PlayerArmorAndCombatEffectsComponent();
var openSky = CreateOpenSkyInput();

PlayerSunScorchResult result = PlayerSunScorchSystem.UpdateLocal(
  openSky,
  pressure,
  armorEffects);
Assert(result.IsBurningInSunlight && pressure.SunScorchCounter == 1,
  "An eligible local player with an open tile column should enter sunlight exposure.");

for (int tick = 1; tick < 119; tick++)
{
  result = PlayerSunScorchSystem.UpdateLocal(openSky, pressure, armorEffects);
}

Assert(result.CurrentCounter == 119 &&
  result.RequestedEffects == PlayerSunScorchEffectRequest.None,
  "The threshold effects should remain inactive below the scorch threshold.");

result = PlayerSunScorchSystem.UpdateLocal(openSky, pressure, armorEffects);
Assert(result.CurrentCounter == 120 && result.SizzleVolume == 1f,
  "The counter should advance to the scorch threshold and clamp sizzle volume to one.");
Assert(result.RequestedEffects.HasFlag(
    PlayerSunScorchEffectRequest.RefreshArmorFrameAndAchievement) &&
  result.RequestedEffects.HasFlag(PlayerSunScorchEffectRequest.ClearBuffImmunity) &&
  result.RequestedEffects.HasFlag(PlayerSunScorchEffectRequest.SpawnVampireOnFireParticle) &&
  result.RequestedEffects.HasFlag(PlayerSunScorchEffectRequest.AddOnFireBuff) &&
  result.RequestedEffects.HasFlag(PlayerSunScorchEffectRequest.AddCursedInfernoBuff) &&
  result.RequestedEffects.HasFlag(PlayerSunScorchEffectRequest.AddShadowFlameBuff) &&
  result.RequestedEffects.HasFlag(PlayerSunScorchEffectRequest.Dismount) &&
  result.RequestedEffects.HasFlag(PlayerSunScorchEffectRequest.ClearWings) &&
  result.RequestedEffects.HasFlag(PlayerSunScorchEffectRequest.ClearRocketBoots),
  "Crossing the threshold should return the complete reference effect request.");

result = PlayerSunScorchSystem.UpdateLocal(openSky, pressure, armorEffects);
Assert(result.CurrentCounter == 121 &&
  !result.RequestedEffects.HasFlag(
    PlayerSunScorchEffectRequest.RefreshArmorFrameAndAchievement),
  "A sustained scorch should retain repeated effects without repeating the threshold edge.");

PlayerArmorAndCombatEffectsSystem.Rebuild(default, armorEffects);
Assert(armorEffects.VampireBurningInSunlight,
  "Armor effect rebuilding must not clear the sunlight owner's committed flag.");

var blockedPressure = new PlayerEnvironmentalPressureComponent();
PlayerSunScorchInput blockedInput = CreateOpenSkyInput() with
{
  TileFactsFromFeetUp = new[]
  {
    new PlayerSunScorchTileFact(
      Exists: true,
      WallType: 1,
      InvisibleWall: false,
      Solid: true,
      Type: 1,
      InvisibleBlock: false)
  }
};
result = PlayerSunScorchSystem.UpdateLocal(
  blockedInput,
  blockedPressure,
  new PlayerArmorAndCombatEffectsComponent());
Assert(!result.IsBurningInSunlight && blockedPressure.SunScorchCounter == 0,
  "A solid blocking tile should prevent exposure and keep the counter at zero.");

result = PlayerSunScorchSystem.UpdateDead(pressure, armorEffects);
Assert(!result.IsBurningInSunlight && result.CurrentCounter == 119 &&
  result.RequestedEffects == PlayerSunScorchEffectRequest.None,
  "A dead player should clear the sunlight flag and decay the counter by two.");

Console.WriteLine("PASS: player sunlight exposure, scorch threshold and owner isolation");

static PlayerSunScorchInput CreateOpenSkyInput()
{
  return new PlayerSunScorchInput(
    IsLocalPlayer: true,
    VampireSeed: true,
    FeetTileY: 100,
    WorldSurface: 200d,
    DayTime: true,
    Raining: false,
    Eclipse: false,
    ZoneGraveyard: false,
    ZoneGlowshroom: false,
    HasMoonLordSkyIntensity: false,
    MoonLordSkyIntensity: 0f,
    Wet: false,
    SelectedItemType: 0,
    MountActive: true,
    MountType: 0,
    ShouldShowInvisibleBlocksAndWalls: false,
    TileFactsFromFeetUp: new[]
    {
      new PlayerSunScorchTileFact(
        Exists: true,
        WallType: 0,
        InvisibleWall: false,
        Solid: false,
        Type: 0,
        InvisibleBlock: false)
    },
    OnFire: false);
}
