using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Player;

public sealed record PlayerPacketMutationResult
{
  public PlayerPacketMutationResult(
    OwnerCommitResult result,
    byte? responseMessageId,
    object? responsePacket,
    IReadOnlyCollection<SessionEpoch> targetEpochs,
    SessionEpoch? ignoreEpoch,
    bool triggeredFaelingSpawn = false,
    bool closeAfterResponse = false,
    CommitVersion? responseVersion = null)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(targetEpochs);
    Result = result;
    ResponseMessageId = responseMessageId;
    ResponsePacket = responsePacket;
    TargetEpochs = Array.AsReadOnly(targetEpochs.ToArray());
    IgnoreEpoch = ignoreEpoch;
    TriggeredFaelingSpawn = triggeredFaelingSpawn;
    CloseAfterResponse = closeAfterResponse;
    ResponseVersion = responseVersion ?? result.Version;
  }

  public OwnerCommitResult Result { get; }

  public byte? ResponseMessageId { get; }

  public object? ResponsePacket { get; }

  public IReadOnlyList<SessionEpoch> TargetEpochs { get; }

  public SessionEpoch? IgnoreEpoch { get; }

  public bool TriggeredFaelingSpawn { get; }

  public bool CloseAfterResponse { get; }

  /// <summary>The response version can identify a rejection without committing player state.</summary>
  public CommitVersion? ResponseVersion { get; }
}
