using System;
using Terraria.Share.Combat;

namespace Terraria.Unshared.Projectiles;

/// <summary>References immutable attack content without attaching a payload to the Projectile.</summary>
public readonly record struct ProjectileAttackDefinitionReferenceComponent
{
  public ProjectileAttackDefinitionReferenceComponent(CombatAttackId definitionId)
  {
    if (!definitionId.IsValid)
    {
      throw new ArgumentException("A Projectile attack definition id is required.", nameof(definitionId));
    }

    DefinitionId = definitionId;
  }

  public CombatAttackId DefinitionId { get; }
}
