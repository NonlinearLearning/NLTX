namespace Terraria.WorldSession.Runtime;

public sealed record ShutdownRequestCommand
{
  public string Reason { get; }

  public ShutdownRequestCommand(string reason)
  {
    Reason = string.IsNullOrWhiteSpace(reason)
      ? throw new ArgumentException("A shutdown reason is required.", nameof(reason))
      : reason;
  }
}
