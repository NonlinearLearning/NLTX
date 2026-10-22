using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Session;

public sealed class JoinStepResult
{
  private JoinStepResult(
    OwnerCommitResult result,
    IReadOnlyCollection<SessionPacketProjection> projections,
    JoinSynchronizationBatch? batch)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(projections);
    Result = result;
    Projections = Array.AsReadOnly(projections.ToArray());
    Batch = batch;
  }

  public OwnerCommitResult Result { get; }

  public IReadOnlyList<SessionPacketProjection> Projections { get; }

  public JoinSynchronizationBatch? Batch { get; }

  public bool IsApplied => Result.Status == OwnerCommitStatus.Applied;

  internal static JoinStepResult Rejected(OwnerCommitResult result)
  {
    return new JoinStepResult(result, Array.Empty<SessionPacketProjection>(), null);
  }

  internal static JoinStepResult Applied(
    OwnerCommitResult result,
    IReadOnlyCollection<SessionPacketProjection> projections,
    JoinSynchronizationBatch? batch = null)
  {
    return new JoinStepResult(result, projections, batch);
  }
}
