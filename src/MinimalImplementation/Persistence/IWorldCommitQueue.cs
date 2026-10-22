using System;
using System.Threading;
using System.Threading.Tasks;

namespace MinimalImplementation.Persistence;

/// <summary>Runs synchronous world work on the host's single-writer commit boundary.</summary>
public interface IWorldCommitQueue
{
  Task ExecuteAsync(Action commit, CancellationToken cancellationToken = default);
}
