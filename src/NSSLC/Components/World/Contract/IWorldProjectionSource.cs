using System.Collections.Generic;

namespace Terraria.World;

/// <summary>
/// Narrow feed that lets another domain publish committed world mutations without holding the
/// world owner.
/// </summary>
/// <remarks>
/// This is deliberately not a read-only port: <see cref="DrainPendingMutations"/> consumes the
/// committed mutation queue. It is narrow enough that a consumer cannot break tiles, place tiles,
/// edit liquids or advance the tick — it can only read two scalars and take the already-committed
/// facts. Widening this interface reopens the cross-domain write path the input boundary
/// constraint forbids.
/// </remarks>
public interface IWorldProjectionSource
{
  /// <summary>Edge length of one tile in world units.</summary>
  float TileSize { get; }

  /// <summary>Revision of the last committed world state.</summary>
  long CommitRevision { get; }

  /// <summary>
  /// Takes at most <paramref name="maximum"/> of the mutations committed since the previous drain.
  /// Mutations above the bound stay queued in commit order, so a projection that runs inside the
  /// tick cannot be forced into unbounded serialization work by a single large world change (for
  /// example a wide liquid pass).
  /// </summary>
  IReadOnlyList<WorldMutation> DrainPendingMutations(int maximum);
}
