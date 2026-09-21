namespace Terraria.Player;

public readonly record struct PlayerAppearanceColor(
  byte Red,
  byte Green,
  byte Blue,
  byte Alpha = byte.MaxValue);
