namespace Terraria.LeashedEntity;

/// <summary>
/// Validates network intent without allocating slots or mutating registration authority.
/// </summary>
public sealed class LeashedEntityNetworkAdapter
{
  public bool TryValidateAndCreateCommand(
    LeashedNetworkFrame frame,
    LeashedDefinitionCatalog definitions,
    LeashedEntityRegistrationSystem registrations,
    out LeashedNetworkFrameCommand command,
    out LeashedNetworkFrameValidation validation)
  {
    validation = Validate(frame, definitions, registrations);
    command = default;
    if (!validation.IsAccepted)
    {
      return false;
    }

    command = new(
      frame,
      validation.ExistingHandle,
      validation.RequiresRegistration);
    return true;
  }

  public LeashedNetworkFrameValidation Validate(
    LeashedNetworkFrame frame,
    LeashedDefinitionCatalog definitions,
    LeashedEntityRegistrationSystem registrations)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(registrations);

    if (frame.LegacySlot < 0)
    {
      return new(LeashedNetworkFrameValidationStatus.RejectedInvalidSlot, false);
    }

    if (frame.Kind != LeashedNetworkFrameKind.Remove
      && !definitions.TryGet(frame.DefinitionId, out _))
    {
      return new(LeashedNetworkFrameValidationStatus.RejectedUnknownDefinition, false);
    }

    bool hasCurrent =
      registrations.TryPeekByLegacySlot(frame.LegacySlot, out LeashedEntityRegistrationSnapshot current);

    if (frame.Kind == LeashedNetworkFrameKind.FullSync)
    {
      if (!hasCurrent)
      {
        return new(LeashedNetworkFrameValidationStatus.Accepted, true)
        {
          ExistingHandle = null
        };
      }

      if (frame.SlotGeneration != 0
        && frame.SlotGeneration != current.LegacySlot.SlotGeneration)
      {
        return new(LeashedNetworkFrameValidationStatus.RejectedStaleGeneration, false);
      }

      if (current.State.DefinitionId != frame.DefinitionId)
      {
        return new(LeashedNetworkFrameValidationStatus.RejectedTypeMismatch, false);
      }

      if (current.SectionMembership.Section != frame.Section)
      {
        return new(LeashedNetworkFrameValidationStatus.RejectedSectionMismatch, false);
      }

      return new(LeashedNetworkFrameValidationStatus.Accepted, false)
      {
        ExistingHandle = current.Handle
      };
    }

    if (!hasCurrent)
    {
      return new(LeashedNetworkFrameValidationStatus.RejectedMissingEntity, false);
    }

    if (frame.SlotGeneration == 0
      || frame.SlotGeneration != current.LegacySlot.SlotGeneration)
    {
      return new(LeashedNetworkFrameValidationStatus.RejectedStaleGeneration, false);
    }

    if (frame.Kind == LeashedNetworkFrameKind.PartialSync
      && current.State.DefinitionId != frame.DefinitionId)
    {
      return new(LeashedNetworkFrameValidationStatus.RejectedTypeMismatch, false);
    }

    return new(LeashedNetworkFrameValidationStatus.Accepted, false)
    {
      ExistingHandle = current.Handle
    };
  }
}
