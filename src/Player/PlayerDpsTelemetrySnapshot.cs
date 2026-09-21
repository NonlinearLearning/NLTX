namespace Terraria.Player;

public readonly record struct PlayerDpsTelemetrySnapshot(
  bool HasWindow,
  bool IsActive,
  DateTimeOffset? StartedAt,
  DateTimeOffset? EndedAt,
  DateTimeOffset? LastHitAt,
  int Damage,
  TimeSpan Duration,
  double DamagePerSecond);
