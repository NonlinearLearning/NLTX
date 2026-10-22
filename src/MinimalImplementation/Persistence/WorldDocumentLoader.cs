using System.Threading;
using System.Threading.Tasks;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Persistence;

public sealed class WorldDocumentLoader
{
  private readonly IMinimalWorldFileStore _fileStore;
  private readonly IWorldPersistenceDocumentDecoder _decoder;
  private readonly IWorldPersistenceDocumentValidator _validator;

  public WorldDocumentLoader(
    IMinimalWorldFileStore fileStore,
    IWorldPersistenceDocumentDecoder decoder,
    IWorldPersistenceDocumentValidator validator)
  {
    _fileStore = fileStore;
    _decoder = decoder;
    _validator = validator;
  }

  public async Task<WorldDocumentLoadResult> ReadAndValidateAsync(
    string path,
    int maximumBytes,
    CancellationToken cancellationToken = default)
  {
    WorldStorageReadResult read = await _fileStore.ReadBoundedAsync(
      path,
      maximumBytes,
      cancellationToken).ConfigureAwait(false);
    if (!read.Succeeded || read.Data is null)
    {
      return WorldDocumentLoadResult.Rejected(read.Failure);
    }

    WorldPersistenceDecodeResult decoded = _decoder.Decode(read.Data);
    if (!decoded.Succeeded || decoded.Document is null)
    {
      return WorldDocumentLoadResult.Rejected(decoded.Failure);
    }

    WorldStorageFailure validationFailure = _validator.Validate(decoded.Document);
    return validationFailure.Kind == WorldStorageFailureKind.None
      ? WorldDocumentLoadResult.Validated(decoded.Document)
      : WorldDocumentLoadResult.Rejected(validationFailure);
  }
}
