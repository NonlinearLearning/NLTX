namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class DebugMessage
{
  private DebugMessage(
    byte author,
    string commandName,
    string arguments,
    GridPoint mousePosition)
  {
    Author = author;
    CommandName = commandName;
    Arguments = arguments;
    MousePosition = mousePosition;
  }

  public byte Author { get; }

  public string CommandName { get; }

  public string Arguments { get; }

  public GridPoint MousePosition { get; }

  public static DebugMessage Parse(
    byte author,
    string rawMessage,
    GridPoint mousePosition)
  {
    ArgumentNullException.ThrowIfNull(rawMessage);
    if (rawMessage.Length == 0 || rawMessage[0] != '/')
    {
      return new DebugMessage(author, string.Empty, string.Empty, mousePosition);
    }

    string payload = rawMessage[1..];
    int separator = payload.IndexOf(' ');
    if (separator < 0)
    {
      return new DebugMessage(
        author,
        payload.ToLowerInvariant(),
        string.Empty,
        mousePosition);
    }

    string commandName = payload[..separator].ToLowerInvariant();
    string arguments = separator + 1 < payload.Length
      ? payload[(separator + 1)..]
      : string.Empty;
    return new DebugMessage(author, commandName, arguments, mousePosition);
  }
}
