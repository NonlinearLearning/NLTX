namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record OnTable1x1ValidationResult(
  bool IsSupported,
  bool ShouldKill,
  bool UsedPlatformSupport,
  bool UsedTableSupport,
  bool UsedType78BottomSlope,
  bool HasUnsupportedShape,
  bool LegacyTableRegistryDeferred);
