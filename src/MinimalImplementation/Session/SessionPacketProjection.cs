using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Session;

/// <summary>Typed server output captured before P01 encodes and sends a packet.</summary>
public sealed record SessionPacketProjection
{
  public SessionPacketProjection(
    byte messageId,
    object packet,
    IReadOnlyCollection<SessionEpoch> targetEpochs,
    SessionEpoch? ignoreEpoch,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(targetEpochs);
    if (targetEpochs.Count == 0)
    {
      throw new ArgumentException(
        "A session packet projection must have at least one target.",
        nameof(targetEpochs));
    }

    if (!commitVersion.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(commitVersion));
    }

    MessageId = messageId;
    Packet = packet;
    TargetEpochs = Array.AsReadOnly(targetEpochs.ToArray());
    IgnoreEpoch = ignoreEpoch;
    CommitVersion = commitVersion;
  }

  public byte MessageId { get; }

  public object Packet { get; }

  public IReadOnlyList<SessionEpoch> TargetEpochs { get; }

  public SessionEpoch? IgnoreEpoch { get; }

  public CommitVersion CommitVersion { get; }
}
