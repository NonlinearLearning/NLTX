using System.Collections.Immutable;

namespace Terraria.WorldProgression.Components;

public sealed class ProgressionCommitStateComponent
{
  public TransitionCommitStatus CommitStatus { get; set; }
  public long CommitSequence { get; set; }
  public int AppliedBatchCount { get; set; }
  public int TotalBatchCount { get; set; }
  public WorldRevision? WorldRevisionBefore { get; set; }
  public WorldRevision? WorldRevisionAfter { get; set; }
  public ImmutableArray<WorldSectionVersion> ChangedSections { get; set; } =
    ImmutableArray<WorldSectionVersion>.Empty;
  public bool IsResultKnown { get; set; }
}
