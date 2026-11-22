namespace Terraria.Diagnostics;

/// <summary>Process-wide server logging entry point used by the standalone host.</summary>
public static class ServerLog
{
  private static readonly ServerLogger s_default = new(new ConsoleServerLogSink());

  public static void Info(string eventName, string details)
  {
    s_default.Info(eventName, details);
  }

  public static void Warning(string eventName, string details)
  {
    s_default.Warning(eventName, details);
  }

  public static void Error(string eventName, string details)
  {
    s_default.Error(eventName, details);
  }

  public static void Error(string eventName, System.Exception exception)
  {
    s_default.Error(eventName, exception);
  }

  public static string Quote(string value)
  {
    return ServerLogger.Quote(value);
  }
}
