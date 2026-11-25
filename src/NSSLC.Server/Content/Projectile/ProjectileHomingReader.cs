using System;
using NSSLC.Infrastructure.Lvcs;
using Terraria.Unshared.Projectiles;

namespace NSSLC.Server.Content.Projectile;

/// <summary>追踪族。族缺席时返回 null；这是唯一跨族读数（转向率进入运动能力）。</summary>
internal sealed class ProjectileHomingReader : IProjectileSectionReader
{
  public string Family => "homing";

  public bool Validate(
      ProjectilePrototype prototype,
      LvcsFamilyId familyId,
      string location,
      LvcsDiagnosticSink diagnostics)
  {
    if (prototype.Homing is ProjectileHomingPrototype homing &&
        (!float.IsFinite(homing.TurnRate) || homing.TurnRate < 0f ||
         !float.IsFinite(homing.Range) || homing.Range < 0f))
    {
      diagnostics.Error(
          familyId, prototype.Id, "Homing",
          "homing turnRate and range must be finite and non negative.", location);
      return false;
    }

    return true;
  }

  public IProjectileSection? Read(ProjectilePrototype prototype)
  {
    ProjectileHomingPrototype? homing = prototype.Homing;
    return homing is null
        ? null
        : new ProjectileHomingSection(homing.TurnRate, homing.Range);
  }
}
