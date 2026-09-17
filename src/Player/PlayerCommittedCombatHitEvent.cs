namespace Terraria.Player;

public readonly record struct PlayerCommittedCombatHitEvent(
  Guid EventId,
  Guid SourceId,
  int Damage,
  long SourceRevision,
  bool IsCommitted,
  PlayerCombatProcHitEffects Effects);
