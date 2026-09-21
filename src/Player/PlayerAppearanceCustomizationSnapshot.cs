namespace Terraria.Player;

public readonly record struct PlayerAppearanceCustomizationSnapshot(
  byte HairDye,
  int SkinDyePacked,
  PlayerAppearanceColor HairColor,
  PlayerAppearanceColor SkinColor,
  PlayerAppearanceColor EyeColor,
  PlayerAppearanceColor ShirtColor,
  PlayerAppearanceColor UnderShirtColor,
  PlayerAppearanceColor PantsColor,
  PlayerAppearanceColor ShoeColor,
  int Hair);
