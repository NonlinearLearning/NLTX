namespace Terraria.WorldSession.Session;

public sealed class SessionPresentationState
{
  public string Motd { get; private set; } = string.Empty;

  public void SetMotd(string motd)
  {
    Motd = motd ?? throw new ArgumentNullException(nameof(motd));
  }
}
