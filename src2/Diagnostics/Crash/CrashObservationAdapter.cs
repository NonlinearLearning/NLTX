namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class CrashObservationAdapter
{
  private readonly ICrashObservationSink _sink;

  public CrashObservationAdapter(
    CrashObservationSettings settings,
    ICrashObservationSink sink)
  {
    Settings = settings;
    _sink = sink ?? throw new ArgumentNullException(nameof(sink));
  }

  public CrashObservationSettings Settings { get; }

  public void Observe(Exception exception)
  {
    ArgumentNullException.ThrowIfNull(exception);
    if (Settings.LogAllExceptions)
    {
      _sink.Publish(exception);
    }
  }
}
