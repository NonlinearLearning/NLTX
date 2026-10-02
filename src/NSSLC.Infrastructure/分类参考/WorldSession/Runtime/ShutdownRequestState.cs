namespace Terraria.WorldSession.Runtime;

public sealed class ShutdownRequestState
{
  public bool IsRequested { get; private set; }

  public string? Reason { get; private set; }

  public void Request(string reason)
  {
    if (string.IsNullOrWhiteSpace(reason))
    {
      throw new ArgumentException("A shutdown reason is required.", nameof(reason));
    }

    IsRequested = true;
    Reason = reason;
  }

  public void Clear()
  {
    IsRequested = false;
    Reason = null;
  }
}
