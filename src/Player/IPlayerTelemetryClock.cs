namespace Terraria.Player;

public interface IPlayerTelemetryClock
{
  DateTimeOffset UtcNow { get; }
}
