using Terraria.WorldGeneration.Actions;

namespace Terraria.WorldGeneration.Systems;

public readonly record struct WorldGenerationActionExecutionRecord(
  long Sequence,
  WorldGenerationActionPayloadKind PayloadKind,
  bool Accepted,
  string? RejectionReason);
