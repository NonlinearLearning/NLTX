namespace Terraria.Content;

public sealed record TileLightingDefinition(
  bool Lighted,
  bool BlockLight = false,
  bool NoSunLight = false,
  int Shine = 0,
  bool Shine2 = false,
  short? GlowMaskId = null);
