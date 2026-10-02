namespace Terraria.Player;

public readonly record struct PlayerCombatResolutionCommand(
  Guid EventId,
  Guid SourceId,
  long SourceRevision,
  int DamageAmount,
  bool Critical,
  bool Dodgeable,
  bool HasGeneralImmunity,
  bool SourceCooldownActive,
  int CooldownTicks,
  DateTimeOffset CommittedAt);
