namespace EntityEcs.Components;

public enum SpawnRejectionReason : byte
{
  None,
  CapacityReached,
  InvalidDefinition,
  InvalidPosition,
  DuplicateAdmission,
  AuthorityDenied,
  Cancelled,
}
