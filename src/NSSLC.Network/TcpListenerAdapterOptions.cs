using Terraria.Communication;

namespace Terraria.Network;

public sealed record TcpListenerAdapterOptions
{
  public int Backlog { get; init; } = 128;

  public int MaximumConcurrentConnections { get; init; } = 256;

  public bool NoDelay { get; init; } = true;

  public void Validate()
  {
    if (Backlog <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Backlog));
    }

    if (MaximumConcurrentConnections <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(MaximumConcurrentConnections));
    }
  }
}
