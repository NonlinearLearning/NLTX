namespace Terraria.Combat.Resolution;

public readonly record struct NpcStrikeCommitCommand(
  NpcKillAttemptSnapshot Attempt,
  int Damage,
  string OperationKey);
