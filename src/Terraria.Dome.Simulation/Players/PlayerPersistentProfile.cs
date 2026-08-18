namespace Terraria.Dome.Simulation.Players;

public readonly record struct PlayerPersistentProfile(
  string Name,
  byte SkinVariant = 0,
  byte VoiceVariant = 0,
  float VoicePitchOffset = 0.0f,
  byte Hair = 0,
  byte HairDye = 0,
  ushort AccessoryVisibility = 0,
  byte HideMisc = 0,
  PlayerPersistentColor HairColor = default,
  PlayerPersistentColor SkinColor = default,
  PlayerPersistentColor EyeColor = default,
  PlayerPersistentColor ShirtColor = default,
  PlayerPersistentColor UnderShirtColor = default,
  PlayerPersistentColor PantsColor = default,
  PlayerPersistentColor ShoeColor = default,
  byte DifficultyFlags = 0,
  byte BiomeTorchFlags = 0,
  byte ConsumableFlags = 0);
