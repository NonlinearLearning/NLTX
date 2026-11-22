using System;
using System.IO;

namespace Terraria.Diagnostics;

/// <summary>Writes server diagnostics to the process console using the legacy line format.</summary>
public sealed class ConsoleServerLogSink : IServerLogSink
{
  private readonly object _gate = new();

  public void Write(in ServerLogEntry entry)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(entry.EventName);
    ArgumentNullException.ThrowIfNull(entry.Details);

    string line = string.Create(
        System.Globalization.CultureInfo.InvariantCulture,
        $"{entry.Timestamp:yyyy-MM-dd'T'HH:mm:ss.fffzzz} " +
        $"{FormatLevel(entry.Level)} {entry.EventName} {SingleLine(entry.Details)}");
    lock (_gate)
    {
      TextWriter writer = entry.Level == ServerLogLevel.Error ? Console.Error : Console.Out;
      writer.WriteLine(line);
      writer.Flush();
    }
  }

  private static string FormatLevel(ServerLogLevel level)
  {
    return level switch
    {
      ServerLogLevel.Info => "INFO",
      ServerLogLevel.Warning => "WARN",
      ServerLogLevel.Error => "ERROR",
      _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown log level.")
    };
  }

  private static string SingleLine(string value)
  {
    return value.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
  }
}
