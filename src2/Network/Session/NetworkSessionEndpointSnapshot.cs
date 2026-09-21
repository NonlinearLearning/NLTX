using System.Collections.ObjectModel;

namespace Terraria.Network.Session;

public sealed class NetworkSessionEndpointSnapshot
{
  public NetworkSessionEndpointSnapshot(
    IReadOnlyList<string?> worlds,
    IReadOnlyList<string?> ipAddresses,
    IReadOnlyList<int> ports)
  {
    Worlds = new ReadOnlyCollection<string?>(worlds.ToArray());
    IpAddresses = new ReadOnlyCollection<string?>(ipAddresses.ToArray());
    Ports = new ReadOnlyCollection<int>(ports.ToArray());
  }

  public IReadOnlyList<string?> Worlds { get; }

  public IReadOnlyList<string?> IpAddresses { get; }

  public IReadOnlyList<int> Ports { get; }
}
