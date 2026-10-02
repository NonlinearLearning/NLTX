namespace Terraria.LeashedEntity;

public enum LeashedNetworkFrameValidationStatus : byte
{
  Accepted,
  RejectedUnknownDefinition,
  RejectedInvalidSlot,
  RejectedStaleGeneration,
  RejectedMissingEntity,
  RejectedTypeMismatch,
  RejectedSectionMismatch
}

public readonly record struct LeashedNetworkFrameValidation(
  LeashedNetworkFrameValidationStatus Status,
  bool RequiresRegistration)
{
  public LeashedEntityHandle? ExistingHandle { get; init; }

  public bool IsAccepted => Status == LeashedNetworkFrameValidationStatus.Accepted;
}
