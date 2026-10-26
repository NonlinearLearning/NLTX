using System;
using Terraria.Combat;

namespace Terraria.StatusEffects;

/// <summary>Checks shared Buff reception rules without mutating entity state.</summary>
public static class BuffAdmissionSystem
{
  public static BuffAdmissionResult Check(
      BuffReceptionCapabilityComponent? capability,
      BuffImmunityComponent? immunity,
      BuffDefinition? definition,
      long sourceId)
  {
    if (definition is null)
    {
      return BuffAdmissionResult.UnknownDefinition;
    }

    if (sourceId == 0)
    {
      return BuffAdmissionResult.InvalidSource;
    }

    if (!capability.HasValue)
    {
      return BuffAdmissionResult.MissingReceptionCapability;
    }

    if (capability.Value.Mode == BuffReceptionMode.Disabled)
    {
      return BuffAdmissionResult.ReceptionDisabled;
    }

    if (!capability.Value.Allows(definition.ImmunityKey))
    {
      return BuffAdmissionResult.NotAllowedByPolicy;
    }

    return immunity.HasValue && immunity.Value.Contains(definition.ImmunityKey)
        ? BuffAdmissionResult.Immune
        : BuffAdmissionResult.Accepted;
  }
}
