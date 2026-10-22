using System;
using System.Globalization;
using System.Text.Json;

namespace MinimalImplementation.Server.Host;

internal static class ServerLog
{
  private static readonly object s_gate = new();

  public static void Info(string eventName, string details)
  {
    Write("INFO", eventName, details);
  }

  public static void Warning(string eventName, string details)
  {
    Write("WARN", eventName, details);
  }

  public static void Error(string eventName, string details)
  {
    Write("ERROR", eventName, details);
  }

  public static void Error(string eventName, Exception exception)
  {
    ArgumentNullException.ThrowIfNull(exception);
    Error(eventName, $"{exception.GetType().Name}: {exception.Message}");
  }

  public static string Quote(string value)
  {
    ArgumentNullException.ThrowIfNull(value);
    return JsonSerializer.Serialize(value);
  }

  private static void Write(string level, string eventName, string details)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
    ArgumentNullException.ThrowIfNull(details);

    string timestamp = DateTimeOffset.Now.ToString(
      "yyyy-MM-dd'T'HH:mm:ss.fffzzz",
      CultureInfo.InvariantCulture);
    string line = $"{timestamp} {level} {eventName} {SingleLine(details)}";
    lock (s_gate)
    {
      TextWriter writer = level == "ERROR" ? Console.Error : Console.Out;
      writer.WriteLine(line);
      writer.Flush();
    }
  }

  private static string SingleLine(string value)
  {
    return value.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
  }
}
