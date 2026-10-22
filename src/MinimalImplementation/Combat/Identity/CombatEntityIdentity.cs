using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Identity;

/// <summary>A value projection of the server's identity handle, never an identity authority.</summary>
public readonly record struct CombatEntityIdentity(
  EntityUuid Id,
  long WorldScope,
  long Generation)
{
  public bool IsValid => Id.IsValid && WorldScope > 0 && Generation > 0;
}
