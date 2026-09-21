namespace Terraria.NonAuthoritative.WorldSession.Calendar;

public interface IWorldTemporaryEventSink
{
  void Apply(WorldTemporaryEventContext context);
}
