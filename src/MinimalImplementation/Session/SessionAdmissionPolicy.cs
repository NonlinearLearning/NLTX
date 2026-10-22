using System;

namespace MinimalImplementation.Server.Session;

public sealed class SessionAdmissionPolicy
{
  public SessionAdmissionPolicy(
    string protocolVersion,
    int maximumPlayerSlots,
    string? password = null,
    string? hostToken = null,
    bool ignoreProtocolVersion = false)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(protocolVersion);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumPlayerSlots);
    if (maximumPlayerSlots > SessionBindingStore.MaximumPlayerSlots)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumPlayerSlots));
    }

    ProtocolVersion = protocolVersion;
    MaximumPlayerSlots = maximumPlayerSlots;
    Password = string.IsNullOrEmpty(password) ? null : password;
    HostToken = string.IsNullOrEmpty(hostToken) ? null : hostToken;
    IgnoreProtocolVersion = ignoreProtocolVersion;
  }

  public string ProtocolVersion { get; }

  public int MaximumPlayerSlots { get; }

  public bool IgnoreProtocolVersion { get; }

  internal string? Password { get; }

  internal string? HostToken { get; }
}
