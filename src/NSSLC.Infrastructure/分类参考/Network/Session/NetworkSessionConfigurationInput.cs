namespace Terraria.Network.Session;

public sealed record NetworkSessionConfigurationInput(
  int MaxConnections,
  int NetBufferSize,
  int DefaultPort,
  string BanFilePath,
  string ServerPassword,
  string ServerIp,
  string ServerIpText,
  bool IsHostAndPlay,
  string HostToken,
  bool UseUpnp,
  bool SaveOnServerExit,
  bool HandshakeLoggingEnabled);
