namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct SimulationRateConfigurationCandidate(
  int DayRate,
  int DesiredWorldTilesUpdateRate);
