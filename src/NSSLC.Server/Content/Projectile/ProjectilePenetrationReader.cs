using Terraria.Share.Lvcs;
using Terraria.Unshared.Projectiles;

namespace NSSLC.Server.Content.Projectile;

/// <summary>穿透族。上限是必填项：没有它就不知道打几下停。</summary>
internal sealed class ProjectilePenetrationReader : IProjectileSectionReader
{
  public string Family => "penetration";

  public bool Validate(
      ProjectilePrototype prototype,
      LvcsFamilyId familyId,
      string location,
      LvcsDiagnosticSink diagnostics)
  {
    if (prototype.Penetration is not ProjectilePenetrationPrototype penetration ||
        penetration.MaximumHits < ProjectilePenetrationSection.Unlimited)
    {
      diagnostics.Error(
          familyId, prototype.Id, "Penetration",
          "penetration.maximumHits must be -1 or greater.", location);
      return false;
    }

    return true;
  }

  public IProjectileSection Read(ProjectilePrototype prototype)
  {
    ProjectilePenetrationPrototype penetration = prototype.Penetration!;
    return new ProjectilePenetrationSection(
        penetration.MaximumHits, penetration.SuppressesDamage);
  }
}
