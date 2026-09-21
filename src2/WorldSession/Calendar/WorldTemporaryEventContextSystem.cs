namespace Terraria.NonAuthoritative.WorldSession.Calendar;

public sealed class WorldTemporaryEventContextSystem
{
  public WorldTemporaryEventContext Capture(IWorldTemporaryEventSource source)
  {
    ArgumentNullException.ThrowIfNull(source);
    return source.Capture();
  }

  public WorldTemporaryEventRestoreResult Restore(
    WorldTemporaryEventContext context,
    IWorldTemporaryEventSink sink)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(sink);
    try
    {
      sink.Apply(context);
      return WorldTemporaryEventRestoreResult.Success;
    }
    catch (Exception)
    {
      return WorldTemporaryEventRestoreResult.Failed("temporary-event-restore-failed");
    }
  }
}
