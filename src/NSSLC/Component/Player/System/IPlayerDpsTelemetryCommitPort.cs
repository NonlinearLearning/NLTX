namespace Terraria.Player;

public interface IPlayerDpsTelemetryCommitPort
{
  bool AcceptCommittedDamage(in PlayerCommittedDamageEvent damageEvent);
}
