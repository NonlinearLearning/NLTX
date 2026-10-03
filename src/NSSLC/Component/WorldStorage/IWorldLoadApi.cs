namespace Terraria.WorldStorage;

public interface IWorldLoadApi<TOwnerContext, TSection, TPrepared>
  where TOwnerContext : notnull
  where TSection : notnull
  where TPrepared : notnull
{
  /// <summary>
  /// Validates and stages this owner's input without mutating the section, its nested values, the
  /// authoritative state, or the supplied context. Normalize or copy input into prepared data
  /// instead. Any temporary resources not returned as prepared data must be released before
  /// returning a rejection or throwing, because the dispatcher cannot clean an unavailable handle.
  /// </summary>
  WorldLoadPrepareResult<TPrepared> PrepareLoad(
    TOwnerContext ownerContext,
    WorldLoadSection<TSection> section);

  /// <summary>
  /// Takes responsibility for consuming or releasing preparedData before returning or throwing.
  /// </summary>
  WorldLoadCommitResult CommitLoad(
    TOwnerContext ownerContext,
    in TPrepared preparedData);

  /// <summary>
  /// Releases uncommitted prepared data. Repeated calls with the same data must be safe.
  /// </summary>
  void DiscardPrepared(
    TOwnerContext ownerContext,
    in TPrepared preparedData);
}
