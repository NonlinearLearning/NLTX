namespace Terraria.WorldSession.Runtime;

public sealed class AutoJoinRequest
{
  public string? ServerAddress { get; private set; }

  public void SetServerAddress(string serverAddress)
  {
    ServerAddress = string.IsNullOrWhiteSpace(serverAddress)
      ? throw new ArgumentException("A server address is required.", nameof(serverAddress))
      : serverAddress;
  }
}
