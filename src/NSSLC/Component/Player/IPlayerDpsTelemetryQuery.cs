namespace Terraria.Player;

public interface IPlayerDpsTelemetryQuery
{
  PlayerDpsTelemetrySnapshot Snapshot(
    PlayerDpsTelemetryComponent component,
    DateTimeOffset observationTime);
}
