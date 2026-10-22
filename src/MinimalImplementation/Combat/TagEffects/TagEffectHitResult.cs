using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Committed TagEffect hit arithmetic and deferred secondary-effect intent.</summary>
public sealed record TagEffectHitResult(
    OwnerCommitResult Result,
    CombatEntityIdentity Npc,
    int Damage,
    bool Critical,
    bool Tagged,
    bool Proc,
    bool ProcCleared,
    TagEffectTaggedAction TaggedAction,
    TagEffectProcAction ProcAction)
{
  public bool IsApplied => Result.Status == OwnerCommitStatus.Applied;
}
