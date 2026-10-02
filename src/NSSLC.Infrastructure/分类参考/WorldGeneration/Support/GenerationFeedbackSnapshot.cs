namespace Terraria.WorldGeneration.Support;

public readonly record struct GenerationFeedbackSnapshot(
  bool StopOnFail,
  bool DisplayText,
  bool IsSpreadActive,
  int EventCount,
  GenerationFeedbackReason LastReason,
  int LastX,
  int LastY,
  int LastIteration);
