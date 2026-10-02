namespace Terraria.Network.Session;

public sealed class NetworkSessionEndpointState
{
  private readonly string?[] _worlds;
  private readonly string?[] _ipAddresses;
  private readonly int[] _ports;

  public NetworkSessionEndpointState(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity), "Endpoint capacity must be positive.");
    }

    MaxEntries = capacity;
    _worlds = new string?[capacity];
    _ipAddresses = new string?[capacity];
    _ports = new int[capacity];
  }

  public int MaxEntries { get; }

  public bool Apply(RecentServerEndpointCommand command)
  {
    if (command.Slot < 0 || command.Slot >= MaxEntries ||
        command.Port is < 0 or > 65535)
    {
      return false;
    }

    _worlds[command.Slot] = command.World;
    _ipAddresses[command.Slot] = command.IpAddress;
    _ports[command.Slot] = command.Port;
    return true;
  }

  public void Clear()
  {
    Array.Clear(_worlds);
    Array.Clear(_ipAddresses);
    Array.Clear(_ports);
  }

  public NetworkSessionEndpointSnapshot CreateSnapshot()
  {
    return new NetworkSessionEndpointSnapshot(_worlds, _ipAddresses, _ports);
  }
}
