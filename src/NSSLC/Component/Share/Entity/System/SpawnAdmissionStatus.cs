namespace EntityEcs.Components;

public enum SpawnAdmissionStatus : byte
{
  NotRequested,
  Pending,
  Admitted,
  Rejected,
  Cancelled,
}
