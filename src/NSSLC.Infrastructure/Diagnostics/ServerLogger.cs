using System;
using System.Text.Json;

namespace Terraria.Diagnostics;

/// <summary>
/// Creates structured server diagnostics and forwards them to an explicit external sink.
/// </summary>
public sealed class ServerLogger
{
  private readonly IServerLogSink _sink;
  private readonly TimeProvider _timeProvider;

  public ServerLogger(IServerLogSink sink, TimeProvider? timeProvider = null)
  {
    _sink = sink ?? throw new ArgumentNullException(nameof(sink));
    _timeProvider = timeProvider ?? TimeProvider.System;
  }

  public void Info(string eventName, string details)
  {
    Write(ServerLogLevel.Info, eventName, details);
  }

  public void Warning(string eventName, string details)
  {
    Write(ServerLogLevel.Warning, eventName, details);
  }

  public void Error(string eventName, string details)
  {
    Write(ServerLogLevel.Error, eventName, details);
  }

  public void Error(string eventName, Exception exception)
  {
    ArgumentNullException.ThrowIfNull(exception);
    Error(eventName, $"{exception.GetType().Name}: {exception.Message}");
  }

  public static string Quote(string value)
  {
    ArgumentNullException.ThrowIfNull(value);
    return JsonSerializer.Serialize(value);
  }

  private void Write(ServerLogLevel level, string eventName, string details)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
    ArgumentNullException.ThrowIfNull(details);
    _sink.Write(new ServerLogEntry(_timeProvider.GetLocalNow(), level, eventName, details));
  }
}
