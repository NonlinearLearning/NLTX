namespace Terraria.WorldSession.Runtime;

public readonly record struct DeferredProcessDrainResult(
  int Executed,
  int Retained,
  int Cancelled,
  int Failed);
