namespace Terraria.WorldSession.Runtime;

public sealed class ShutdownSignalAdapter
{
  private readonly HostShutdownSystem _shutdownSystem;

  public ShutdownSignalAdapter(HostShutdownSystem shutdownSystem)
  {
    _shutdownSystem = shutdownSystem ?? throw new ArgumentNullException(nameof(shutdownSystem));
  }

  public void Signal(string reason)
  {
    _shutdownSystem.Submit(new ShutdownRequestCommand(reason));
  }
}
