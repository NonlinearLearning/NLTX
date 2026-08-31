namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyWorldInfectionConversionDeferred(
  long Sequence,
  int X,
  int Y,
  int ConversionType,
  LegacyWorldInfectionConversionChannel Channel,
  LegacyWorldInfectionConversionDeferredReason Reason,
  ushort SourceType);
