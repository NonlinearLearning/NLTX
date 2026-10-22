namespace MinimalImplementation.Combat.Identity;

/// <summary>Adapts the server-owned identity registry to the combat assembly boundary.</summary>
public interface ICombatEntityIdentityAuthority
{
  long CurrentWorldScope { get; }

  CombatEntityIdentity Create();

  bool IsActive(CombatEntityIdentity identity);

  bool Retire(CombatEntityIdentity identity);
}
