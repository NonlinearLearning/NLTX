namespace Terraria.ExternalPlatformBoundaries.Nat;

public sealed class NatPortMappingLifecycleSystem
{
  private readonly INatPortMappingPort _port;

  public NatPortMappingLifecycleSystem(INatPortMappingPort port)
  {
    _port = port ?? throw new ArgumentNullException(nameof(port));
  }

  public NatPortMappingResult OnListenerReady(NatPortMappingKey key)
  {
    return _port.Ensure(key);
  }

  public NatPortMappingResult OnServerStopping(NatPortMappingKey key)
  {
    return _port.Release(key);
  }
}
