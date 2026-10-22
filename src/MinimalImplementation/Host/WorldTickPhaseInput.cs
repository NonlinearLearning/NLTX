namespace MinimalImplementation.Server.Host;

/// <summary>
/// Explicit authoritative input required by the Time/World tick composite.
/// </summary>
public sealed record WorldTickPhaseInput(
  ulong WorldEpoch,
  double ElapsedGameTicks,
  int DayRate,
  int LiquidOperationBudget);
