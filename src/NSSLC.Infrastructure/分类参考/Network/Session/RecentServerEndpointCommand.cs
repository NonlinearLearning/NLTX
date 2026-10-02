namespace Terraria.Network.Session;

public readonly record struct RecentServerEndpointCommand(
  int Slot,
  string? World,
  string? IpAddress,
  int Port);
