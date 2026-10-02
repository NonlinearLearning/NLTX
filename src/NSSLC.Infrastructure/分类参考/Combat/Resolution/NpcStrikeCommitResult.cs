namespace Terraria.Combat.Resolution;

public readonly record struct NpcStrikeCommitResult(
  bool DamageCommitted,
  bool TargetDeadAfterCommit,
  bool Duplicate);
