namespace Terraria.Player;

public readonly record struct PlayerCommittedDamageEvent(
  Guid EventId,
  int Damage,
  DateTimeOffset CommittedAt,
  long SourceRevision);
