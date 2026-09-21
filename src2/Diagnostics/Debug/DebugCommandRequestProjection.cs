namespace Terraria.NonAuthoritative.Diagnostics;

public static class DebugCommandRequestProjection
{
  public static DebugMessage Parse(
    byte author,
    string message,
    GridPoint mousePosition)
  {
    return DebugMessage.Parse(author, message, mousePosition);
  }
}
