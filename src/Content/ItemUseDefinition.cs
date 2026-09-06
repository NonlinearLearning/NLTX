namespace Terraria.Content;

public sealed record ItemUseDefinition(
  int HoldStyle,
  int UseStyle,
  bool Channel,
  int UseAnimationTicks,
  int UseTimeTicks,
  bool AutoReuse,
  bool UseTurn,
  bool NoUseGraphic,
  bool NoMeleeGraphic,
  bool ShootsEveryUse,
  string? UseSoundKey,
  float UseSoundPitch,
  int ReuseDelayTicks = 0);
