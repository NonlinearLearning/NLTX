using System;
using System.Collections.Generic;

namespace Terraria.Combat;

public struct StatusEffectImmunityComponent
{
  public StatusEffectImmunityComponent(
    IReadOnlyList<bool>? immuneByDefinitionId = null,
    int revision = 0)
  {
    _immuneByDefinitionId = immuneByDefinitionId is null
      ? null
      : new List<bool>(immuneByDefinitionId).ToArray();
    Revision = revision;
  }

  private bool[]? _immuneByDefinitionId;

  public int Revision;

  public ReadOnlyMemory<bool> ImmunityByDefinitionId =>
    new(_immuneByDefinitionId ?? Array.Empty<bool>());

  public int DefinitionCapacity => _immuneByDefinitionId?.Length ?? 0;
}
