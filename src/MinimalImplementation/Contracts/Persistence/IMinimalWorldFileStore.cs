using System.Threading;
using System.Threading.Tasks;

namespace Terraria.NonAuthoritative.Persistence;

public interface IMinimalWorldFileStore
{
  Task<WorldStorageReadResult> ReadBoundedAsync(
    string path,
    int maximumBytes,
    CancellationToken cancellationToken);

  Task<WorldFileCommitResult> CommitAtomicallyAsync(
    string path,
    ReadOnlyMemory<byte> data,
    int maximumBytes,
    CancellationToken cancellationToken);
}
