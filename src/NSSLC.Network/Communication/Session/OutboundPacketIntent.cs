using Terraria.Session;
using System;
using System.Collections.Generic;

namespace Terraria.Network;

public sealed record OutboundPacketIntent
{
  public IReadOnlyList<SessionEpoch> TargetEpochs { get; }
  public SessionEpoch? IgnoreEpoch { get; }
  public byte MessageId { get; }
  public ReadOnlyMemory<byte> Payload { get; }
  public CommitVersion CommitVersion { get; }
  public OutboundPacketPriority Priority { get; }

  public OutboundPacketIntent(
    IReadOnlyCollection<SessionEpoch> targetEpochs,
    SessionEpoch? ignoreEpoch,
    byte messageId,
    ReadOnlyMemory<byte> payload,
    CommitVersion commitVersion,
    OutboundPacketPriority priority = OutboundPacketPriority.Normal)
  {
    ArgumentNullException.ThrowIfNull(targetEpochs);
    if (targetEpochs.Count == 0)
    {
      throw new ArgumentException(
        "An outbound intent must have at least one target.",
        nameof(targetEpochs));
    }

    if (!commitVersion.IsValid)
    {
      throw new ArgumentException(
        "An outbound intent must reference a committed version.",
        nameof(commitVersion));
    }

    TargetEpochs = Array.AsReadOnly(targetEpochs.ToArray());
    IgnoreEpoch = ignoreEpoch;
    MessageId = messageId;
    Payload = payload.ToArray();
    CommitVersion = commitVersion;
    if (!Enum.IsDefined(priority))
    {
      throw new ArgumentOutOfRangeException(nameof(priority));
    }

    Priority = priority;

  }
}
