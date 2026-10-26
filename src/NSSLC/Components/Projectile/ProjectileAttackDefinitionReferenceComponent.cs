using System;
using Terraria.Combat;

namespace Terraria.Projectile;

/// <summary>References immutable attack content without attaching a payload to the Projectile.</summary>
public readonly record struct ProjectileAttackDefinitionReferenceComponent
{
  public ProjectileAttackDefinitionReferenceComponent(CombatAttackDefinitionId definitionId)
  {
    if (!definitionId.IsValid)
    {
      throw new ArgumentException("A Projectile attack definition id is required.", nameof(definitionId));
    }

    DefinitionId = definitionId;
  }

  public CombatAttackDefinitionId DefinitionId { get; }
}
