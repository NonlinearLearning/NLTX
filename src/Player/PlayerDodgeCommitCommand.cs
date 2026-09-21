namespace Terraria.Player;

public readonly record struct PlayerDodgeCommitCommand(
  Guid CommandId,
  Guid SourceId,
  long SourceRevision,
  int DamageAmount,
  bool IsCommitted);
