using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyWorldInfectionConversionRule(
  int ConversionType,
  LegacyWorldInfectionConversionChannel Channel,
  LegacyWorldInfectionConversionCategory Category,
  int Priority,
  IReadOnlySet<ushort> SourceTypes,
  ushort TargetType,
  bool IsDeferred = false);
