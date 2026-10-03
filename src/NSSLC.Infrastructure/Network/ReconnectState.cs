namespace NSSLC.Infrastructure.Network;

public enum ReconnectState {
  Connecting,
  Handshaking,
  Active,
  Backoff,
  Stopped
}
