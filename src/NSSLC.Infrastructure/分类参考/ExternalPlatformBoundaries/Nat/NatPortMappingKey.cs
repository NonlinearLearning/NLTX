namespace Terraria.ExternalPlatformBoundaries.Nat;

public readonly record struct NatPortMappingKey
{
  public NatPortMappingKey(
    int externalPort,
    int internalPort,
    string protocol,
    string internalClient,
    string description)
  {
    if (externalPort is < 1 or > 65535)
    {
      throw new ArgumentOutOfRangeException(nameof(externalPort));
    }

    if (internalPort is < 1 or > 65535)
    {
      throw new ArgumentOutOfRangeException(nameof(internalPort));
    }

    if (string.IsNullOrWhiteSpace(protocol))
    {
      throw new ArgumentException("A protocol is required.", nameof(protocol));
    }

    if (string.IsNullOrWhiteSpace(internalClient))
    {
      throw new ArgumentException("An internal client is required.", nameof(internalClient));
    }

    ExternalPort = externalPort;
    InternalPort = internalPort;
    Protocol = protocol.Trim().ToUpperInvariant();
    InternalClient = internalClient.Trim();
    Description = description ?? string.Empty;
  }

  public int ExternalPort { get; }

  public int InternalPort { get; }

  public string Protocol { get; }

  public string InternalClient { get; }

  public string Description { get; }
}
