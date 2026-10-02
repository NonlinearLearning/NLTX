using System.Net;

namespace Terraria.Network.Transport;

public readonly record struct RemoteEndpointValue
{
  public RemoteEndpointValue(RemoteEndpointKind type, string address, int port)
  {
    if (string.IsNullOrWhiteSpace(address))
    {
      throw new ArgumentException("Endpoint address is required.", nameof(address));
    }

    if (port is < 1 or > 65535)
    {
      throw new ArgumentOutOfRangeException(nameof(port));
    }

    Type = type;
    Address = address.Trim();
    Port = port;
  }

  public RemoteEndpointKind Type { get; }

  public string Address { get; }

  public int Port { get; }

  public bool IsLocalHost =>
    string.Equals(Address, "localhost", StringComparison.OrdinalIgnoreCase) ||
    (IPAddress.TryParse(Address, out IPAddress? parsed) && IPAddress.IsLoopback(parsed));

  public string GetIdentifier()
  {
    return $"{Type.ToString().ToLowerInvariant()}://{FormatAddress()}:{Port}";
  }

  public string GetFriendlyName()
  {
    return $"{FormatAddress()}:{Port}";
  }

  private string FormatAddress()
  {
    return Address.Contains(':', StringComparison.Ordinal) &&
      !Address.StartsWith("[", StringComparison.Ordinal)
      ? $"[{Address}]"
      : Address;
  }
}
