using System.Collections.Immutable;

namespace Terraria.WorldProgression.Components;

public sealed class TransitionPlanStateComponent
{
  public PlanId PlanId { get; set; }
  public TransitionId TransitionId { get; set; }
  public WorldRevision? BaseWorldRevision { get; set; }
  public ulong BaseGenerationRevision { get; set; }
  public ulong RandomCursor { get; set; }
  public ImmutableArray<WorldSectionId> AffectedSections { get; set; } =
    ImmutableArray<WorldSectionId>.Empty;
  public ImmutableArray<TransitionSectionPrecondition> SectionPreconditions { get; set; } =
    ImmutableArray<TransitionSectionPrecondition>.Empty;
  public int ExpectedBatchCount { get; set; }
  public HardmodeOreTierState OreTierCandidate { get; set; } = HardmodeOreTierState.Uninitialized;
  public bool RequiresResync { get; set; }
}
