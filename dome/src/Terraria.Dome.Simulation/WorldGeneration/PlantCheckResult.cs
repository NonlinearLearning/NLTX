namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct PlantCheckResult(
  bool ShouldDestroy,
  bool ShouldConvert,
  PlantTypeConversionResult Conversion);
