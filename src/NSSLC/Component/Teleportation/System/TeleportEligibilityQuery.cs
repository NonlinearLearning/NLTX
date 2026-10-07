using Terraria.Relationships;

namespace Terraria.Teleportation;

public static class TeleportEligibilityQuery
{
  public static TeleportEligibility Evaluate(
    in TeleportTransitionRequest request,
    in TeleportTransitionSnapshot snapshot)
  {
    if (request.SourceEndpoint.IsEmpty ||
      request.DestinationEndpoint.IsEmpty ||
      !snapshot.SourceEndpoint.IsValid ||
      !snapshot.DestinationEndpoint.IsValid)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.InvalidEndpoint);
    }

    if (request.SourceEndpoint != snapshot.SourceEndpoint.Endpoint ||
      request.DestinationEndpoint != snapshot.DestinationEndpoint.Endpoint)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.EndpointIdentityMismatch);
    }

    if (request.SourceEndpoint == request.DestinationEndpoint)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.SameEndpoint);
    }

    if (!snapshot.SourceEndpoint.IsActive ||
      !snapshot.DestinationEndpoint.IsActive)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.EndpointInactive);
    }

    if (request.SourcePosition != snapshot.SourceEndpoint.Position ||
      request.DestinationPosition != snapshot.DestinationEndpoint.Position)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.PositionMismatch);
    }

    if (request.Subject != snapshot.Subject.Subject)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.SubjectIdentityMismatch);
    }

    if (!snapshot.Subject.IsValid ||
      request.SubjectKind != snapshot.Subject.Kind)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.UnsupportedSubject);
    }

    if (!snapshot.Subject.IsActive)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.SubjectInactive);
    }

    if (snapshot.Subject.IsDead)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.SubjectDead);
    }

    if (snapshot.Subject.IsTeleporting)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.SubjectTeleporting);
    }

    if (snapshot.Subject.IsTeleportationImmune)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.SubjectImmune);
    }

    if (request.BlockPlayerTeleportation &&
      snapshot.Subject.Kind == PortalSubjectKind.Player)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.BlockedByIteration);
    }

    if (request.CooldownTicks < 0 || snapshot.Cooldown.RemainingTicks < 0)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.InvalidCooldown);
    }

    if (snapshot.Cooldown.IsOnCooldown)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.CooldownActive);
    }

    if (request.Source == TeleportSource.None)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.InvalidSource);
    }

    if (request.ExpectedSourceRevision != snapshot.SourceEndpoint.Revision ||
      request.ExpectedDestinationRevision !=
      snapshot.DestinationEndpoint.Revision ||
      request.ExpectedSubjectRevision != snapshot.Subject.Revision)
    {
      return TeleportEligibility.Rejected(
        TeleportCommitReason.StaleRevision);
    }

    return TeleportEligibility.Eligible;
  }

  public static bool IsSupportedSubject(EntityReference subject)
  {
    return subject.Scope is EntityReferenceScope.Player or
      EntityReferenceScope.Npc;
  }
}
