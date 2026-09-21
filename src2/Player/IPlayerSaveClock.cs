namespace Terraria.NonAuthoritative.Player;

public interface IPlayerSaveClock
{
  TimeSpan Now { get; }
}
