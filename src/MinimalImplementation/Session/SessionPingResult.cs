using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Session;

/// <summary>Prepared single-connection response for ordinary packet 154.</summary>
public sealed class SessionPingResult
{
  internal SessionPingResult(
      OwnerCommitResult result,
      IReadOnlyCollection<SessionPacketProjection> projections)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(projections);
    Result = result;
    Projections = Array.AsReadOnly(projections.ToArray());
  }

  public OwnerCommitResult Result { get; }

  public IReadOnlyList<SessionPacketProjection> Projections { get; }
}
