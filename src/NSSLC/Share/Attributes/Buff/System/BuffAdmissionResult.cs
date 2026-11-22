namespace Terraria.Attributes;

/// <summary>Outcome of checking whether a Buff may be applied to one entity.</summary>
public enum BuffAdmissionResult : byte
{
  Accepted = 0,
  MissingReceptionCapability = 1,
  ReceptionDisabled = 2,
  NotAllowedByPolicy = 3,
  Immune = 4,
  UnknownDefinition = 5,
  InvalidSource = 6,
  InvalidRequest = 7
}
