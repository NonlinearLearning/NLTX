using Terraria.Relationships;

namespace Terraria.Teleportation;

public readonly record struct TeleportSubjectSnapshot(
  EntityReference Subject,
  PortalSubjectKind Kind,
  uint Revision,
  bool IsActive,
  bool IsDead,
  bool IsTeleporting,
  bool IsTeleportationImmune)
{
  public bool IsValid =>
    !Subject.IsEmpty &&
    IsSupportedKind(Kind) &&
    IsMatchingScope(Subject.Scope, Kind);

  private static bool IsSupportedKind(PortalSubjectKind kind)
  {
    return kind is PortalSubjectKind.Player or PortalSubjectKind.Npc;
  }

  private static bool IsMatchingScope(
    EntityReferenceScope scope,
    PortalSubjectKind kind)
  {
    return kind switch
    {
      PortalSubjectKind.Player => scope == EntityReferenceScope.Player,
      PortalSubjectKind.Npc => scope == EntityReferenceScope.Npc,
      _ => false,
    };
  }
}
