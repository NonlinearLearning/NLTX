namespace Terraria.Network.Session;

public sealed class NetworkSessionConfigurationStateComponent
{
  private NetworkSessionConfigurationInput? _input;
  private bool _isFrozen;

  public bool IsFrozen => _isFrozen;

  internal bool TryApply(NetworkSessionConfigurationInput input, out string error)
  {
    if (_isFrozen)
    {
      error = "Configuration is frozen.";
      return false;
    }

    if (input.MaxConnections <= 0)
    {
      error = "MaxConnections must be positive.";
      return false;
    }

    if (input.NetBufferSize <= 0)
    {
      error = "NetBufferSize must be positive.";
      return false;
    }

    if (input.DefaultPort is < 1 or > 65535)
    {
      error = "DefaultPort must be a TCP port.";
      return false;
    }

    if (string.IsNullOrWhiteSpace(input.ServerIp) ||
        string.IsNullOrWhiteSpace(input.ServerIpText) ||
        string.IsNullOrWhiteSpace(input.BanFilePath))
    {
      error = "External endpoint and ban path must be present.";
      return false;
    }

    _input = input;
    error = string.Empty;
    return true;
  }

  internal void Freeze()
  {
    if (_input is null)
    {
      throw new InvalidOperationException("Configuration must be applied before freezing.");
    }

    _isFrozen = true;
  }

  public NetworkSessionConfigurationSnapshot CreateSnapshot()
  {
    if (_input is null)
    {
      throw new InvalidOperationException("Configuration has not been applied.");
    }

    return new NetworkSessionConfigurationSnapshot(
      _input.MaxConnections,
      _input.NetBufferSize,
      _input.DefaultPort,
      _input.BanFilePath,
      _input.ServerIp,
      _input.ServerIpText,
      _input.IsHostAndPlay,
      _input.UseUpnp,
      _input.SaveOnServerExit,
      _input.HandshakeLoggingEnabled,
      _isFrozen,
      ContainsSecrets: false);
  }
}
