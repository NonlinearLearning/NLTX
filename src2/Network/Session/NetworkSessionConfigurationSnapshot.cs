namespace Terraria.Network.Session;

public sealed record NetworkSessionConfigurationSnapshot(
  int MaxConnections,
  int NetBufferSize,
  int DefaultPort,
  string BanFilePath,
  string ServerIp,
  string ServerIpText,
  bool IsHostAndPlay,
  bool UseUpnp,
  bool SaveOnServerExit,
  bool HandshakeLoggingEnabled,
  bool IsFrozen,
  bool ContainsSecrets);
