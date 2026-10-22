using System;
using System.Threading;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Persistence;

/// <summary>Immutable input supplied while an owner prepares an unpublished world candidate.</summary>
public sealed class WorldLoadContext
{
  public WorldLoadContext(
    long candidateWorldScope,
    WorldPersistenceDocument document,
    CancellationToken cancellationToken)
  {
    if (candidateWorldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(candidateWorldScope));
    }

    ArgumentNullException.ThrowIfNull(document);
    CandidateWorldScope = candidateWorldScope;
    Document = document;
    CancellationToken = cancellationToken;
  }

  public long CandidateWorldScope { get; }

  public WorldPersistenceDocument Document { get; }

  public CancellationToken CancellationToken { get; }
}
