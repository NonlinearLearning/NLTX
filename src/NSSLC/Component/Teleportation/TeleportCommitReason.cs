namespace Terraria.Teleportation;

public enum TeleportCommitReason : byte
{
  None,
  EmptyCommand,
  DuplicateCommand,
  InvalidEndpoint,
  EndpointInactive,
  SameEndpoint,
  PositionMismatch,
  UnsupportedSubject,
  SubjectIdentityMismatch,
  SubjectInactive,
  SubjectDead,
  SubjectTeleporting,
  SubjectImmune,
  BlockedByIteration,
  CooldownActive,
  InvalidCooldown,
  InvalidSource,
  StaleRevision,
  EndpointIdentityMismatch,
  CommitPortRejected,
  CommitPortUnknown,
}
