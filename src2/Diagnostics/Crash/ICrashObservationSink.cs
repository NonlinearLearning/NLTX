namespace Terraria.NonAuthoritative.Diagnostics;

public interface ICrashObservationSink
{
  void Publish(Exception exception);
}
