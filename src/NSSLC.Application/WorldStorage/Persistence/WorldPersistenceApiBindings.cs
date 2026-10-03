using System;
using System.Threading;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

internal sealed class WorldPersistenceApiBindings : WorldLoadApiBindings
{
  private readonly WorldPersistenceDocument _document;
  private readonly WorldLoadApiRuntimeBindings _runtimeBindings;
  private readonly CancellationToken _cancellationToken;

  public WorldPersistenceApiBindings(
    WorldPersistenceDocument document,
    WorldLoadApiRuntimeBindings runtimeBindings,
    CancellationToken cancellationToken)
  {
    _document = document ?? throw new ArgumentNullException(nameof(document));
    _runtimeBindings = runtimeBindings ??
      throw new ArgumentNullException(nameof(runtimeBindings));
    _cancellationToken = cancellationToken;
  }

  public override int FormatVersion => _document.FormatVersion;

  public override CancellationToken CancellationToken => _cancellationToken;

  public override bool TryGetApi<TApi>(out TApi api)
  {
    return _runtimeBindings.TryGetApi(out api);
  }

  public override bool TryGetOwnerContext<TOwnerContext>(
    string ownerId,
    out TOwnerContext ownerContext)
  {
    return _runtimeBindings.TryGetOwnerContext(ownerId, out ownerContext);
  }

  public override bool TryGetSection<TSection>(
    string sectionId,
    out WorldLoadSection<TSection> section)
  {
    return _document.TryGetSection(sectionId, out section);
  }
}
