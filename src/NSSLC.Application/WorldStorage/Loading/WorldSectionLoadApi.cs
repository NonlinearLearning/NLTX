using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Common lifecycle for immutable DTOs. Specialized terrain owns its staging buffer.
/// </summary>
public abstract class WorldSectionLoadApi<TSection> where TSection : notnull {
  public WorldLoadPrepareResult<WorldLoadSection<TSection>> PrepareLoad(
      LoadedWorldSession ownerContext, WorldLoadSection<TSection> section) {
    ArgumentNullException.ThrowIfNull(ownerContext);
    if (!ownerContext.IsFresh) {
      return WorldLoadPrepareResult<WorldLoadSection<TSection>>.Rejected(
          WorldLoadApiFailure.Create("TargetNotFresh", "Load into a fresh unpublished session."));
    }
    if (section.IsPresent) {
      Validate(section.Value);
    }
    return WorldLoadPrepareResult<WorldLoadSection<TSection>>.Prepared(section);
  }

  public WorldLoadCommitResult CommitLoad(LoadedWorldSession ownerContext,
      in WorldLoadSection<TSection> preparedData) {
    if (preparedData.IsPresent) {
      Apply(ownerContext, preparedData.Value);
      ownerContext.RecordCommit(SectionId);
    }
    return WorldLoadCommitResult.Committed();
  }

  public void DiscardPrepared(LoadedWorldSession ownerContext,
      in WorldLoadSection<TSection> preparedData) {
    // DTOs are immutable managed values; no leased resources or owner state need releasing.
  }

  protected abstract string SectionId { get; }
  protected virtual void Validate(TSection section) { }
  protected abstract void Apply(LoadedWorldSession owner, TSection section);
}
