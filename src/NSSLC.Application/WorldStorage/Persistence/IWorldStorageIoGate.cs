using System;
using System.Threading;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Serializes world mutation work with coherent world snapshot and persistence operations.
/// </summary>
/// <remarks>
/// If <see cref="Enter(CancellationToken)"/> throws, it must not leave the gate acquired. A
/// successful call returns an idempotent lease that releases the gate without throwing when
/// disposed according to the implementation's thread-affinity requirements.
/// </remarks>
public interface IWorldStorageIoGate
{
  /// <summary>Acquires the gate or throws before returning a lease.</summary>
  /// <param name="cancellationToken">Cancels waiting before acquisition completes.</param>
  /// <returns>A lease that releases the acquired gate when disposed.</returns>
  IDisposable Enter(CancellationToken cancellationToken);
}
