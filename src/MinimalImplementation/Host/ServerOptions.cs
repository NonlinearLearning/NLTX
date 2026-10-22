using System;

namespace MinimalImplementation.Server.Host;

public sealed class ServerOptions
{
  public ServerOptions(
    string listenAddress,
    int listenPort,
    string worldFilePath,
    int maximumWorldFileBytes,
    int commitQueueCapacity)
  {
    ListenAddress = listenAddress;
    ListenPort = listenPort;
    WorldFilePath = worldFilePath;
    MaximumWorldFileBytes = maximumWorldFileBytes;
    CommitQueueCapacity = commitQueueCapacity;
  }

  public string ListenAddress { get; }

  public int ListenPort { get; }

  public string WorldFilePath { get; }

  public int MaximumWorldFileBytes { get; }

  public int CommitQueueCapacity { get; }

  public void Validate()
  {
    if (!System.Net.IPAddress.TryParse(ListenAddress, out _))
    {
      throw new ArgumentException("The listen address must be an IP address.", nameof(ListenAddress));
    }

    if (ListenPort is < 0 or > ushort.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(ListenPort));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(WorldFilePath);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumWorldFileBytes);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(CommitQueueCapacity);
  }
}
