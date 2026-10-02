namespace Terraria.NonAuthoritative.WorldSession.Calendar;

public interface IWorldTemporaryEventSource
{
  WorldTemporaryEventContext Capture();
}
