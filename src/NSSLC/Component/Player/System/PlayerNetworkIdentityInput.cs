namespace Terraria.Player;

/// <summary>
/// Validated packet-4 state supplied by the network adapter.
/// </summary>
public readonly record struct PlayerNetworkIdentityInput(
  string CharacterName,
  PlayerDifficulty Difficulty,
  byte SkinVariant,
  byte VoiceVariant,
  float VoicePitchOffset,
  byte Hair,
  byte HairDye,
  ushort HiddenAccessories,
  byte HideMiscBits,
  PlayerAppearanceColor HairColor,
  PlayerAppearanceColor SkinColor,
  PlayerAppearanceColor EyeColor,
  PlayerAppearanceColor ShirtColor,
  PlayerAppearanceColor UnderShirtColor,
  PlayerAppearanceColor PantsColor,
  PlayerAppearanceColor ShoeColor,
  byte DifficultyAndAccessoryFlags,
  byte BiomeAndCartFlags,
  byte PermanentUpgradeFlags);
