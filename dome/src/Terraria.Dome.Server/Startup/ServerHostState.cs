using System;
using System.IO;

namespace Terraria.Dome.Server.Startup;

public sealed class ServerHostState
{
  public const int DefaultMaximumConnections = byte.MaxValue;
  public const int DefaultBackupRetention = 2;
  public const int DefaultSessionTimeoutSeconds = 120;

  public ServerHostState(string worldPath, int port = 0,
    int maximumConnections = DefaultMaximumConnections,
    int backupRetention = DefaultBackupRetention, bool dedicated = true,
    int sessionTimeoutSeconds = DefaultSessionTimeoutSeconds)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(worldPath);
    if (!Path.IsPathFullyQualified(worldPath))
    {
      throw new ArgumentException("The world path must be absolute.", nameof(worldPath));
    }

    if (port is < 0 or > 65535)
    {
      throw new ArgumentOutOfRangeException(nameof(port));
    }

    if (maximumConnections is < 1 or > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumConnections));
    }

    if (backupRetention is < 0 or > 32)
    {
      throw new ArgumentOutOfRangeException(nameof(backupRetention));
    }

    if (sessionTimeoutSeconds is < 1 or > 3600)
    {
      throw new ArgumentOutOfRangeException(nameof(sessionTimeoutSeconds));
    }

    WorldPath = Path.GetFullPath(worldPath);
    Port = port;
    MaximumConnections = maximumConnections;
    BackupRetention = backupRetention;
    Dedicated = dedicated;
    SessionTimeoutSeconds = sessionTimeoutSeconds;
    LastSessionActivityUtc = DateTimeOffset.UtcNow;
  }

  public int ActiveSessions { get; private set; }
  public int BackupRetention { get; private set; }
  public bool Dedicated { get; }
  public bool IsAcceptingConnections { get; private set; }
  public bool IsReady { get; private set; }
  public string? LastRecoveryFailure { get; private set; }
  public int MaximumConnections { get; private set; }
  public int Port { get; private set; }
  public DateTimeOffset LastSessionActivityUtc { get; private set; }
  public int SessionTimeoutSeconds { get; }
  public string WorldPath { get; }

  public bool TryAttachSession()
  {
    if (ActiveSessions >= MaximumConnections)
    {
      return false;
    }

    ActiveSessions++;
    return true;
  }

  public void SetMaximumConnections(int maximumConnections)
  {
    if (maximumConnections is < 1 or > byte.MaxValue || maximumConnections < ActiveSessions)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumConnections));
    }

    MaximumConnections = maximumConnections;
  }

  public void DetachSession() => ActiveSessions = Math.Max(0, ActiveSessions - 1);

  public void MarkReady(int boundPort)
  {
    Port = boundPort;
    IsReady = true;
    IsAcceptingConnections = true;
  }

  public void SetBackupRetention(int backupRetention)
  {
    if (backupRetention is < 0 or > 32)
    {
      throw new ArgumentOutOfRangeException(nameof(backupRetention));
    }

    BackupRetention = backupRetention;
  }

  public void MarkStopped()
  {
    IsReady = false;
    IsAcceptingConnections = false;
  }

  public void RecordRecoveryFailure(string failureReason)
  {
    LastRecoveryFailure = string.IsNullOrWhiteSpace(failureReason)
      ? "Unknown recovery failure."
      : failureReason;
  }

  public void RecordSessionActivity()
  {
    LastSessionActivityUtc = DateTimeOffset.UtcNow;
  }
}
