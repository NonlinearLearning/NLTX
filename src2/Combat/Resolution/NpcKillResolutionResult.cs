namespace Terraria.Combat.Resolution;

public readonly record struct NpcKillResolutionResult(
  NpcKillAttemptSnapshot Attempt,
  bool IsKillConfirmed);
