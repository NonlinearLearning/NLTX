using System;
using NSSLC.Infrastructure.Lvcs;
using Terraria.Unshared.Projectiles;

namespace NSSLC.Server.Content.Projectile;

/// <summary>重力族。关闭时也要产出 section：共享重力系统依赖配置组件存在。</summary>
internal sealed class ProjectileGravityReader : IProjectileSectionReader
{
  public string Family => "gravity";

  public bool Validate(
      ProjectilePrototype prototype,
      LvcsFamilyId familyId,
      string location,
      LvcsDiagnosticSink diagnostics)
  {
    if (prototype.Gravity is ProjectileGravityPrototype gravity &&
        (!float.IsFinite(gravity.Strength) || gravity.Strength < 0f ||
         !float.IsFinite(gravity.MaximumFallSpeed) || gravity.MaximumFallSpeed < 0f))
    {
      diagnostics.Error(
          familyId, prototype.Id, "Gravity",
          "gravity strength and maximumFallSpeed must be finite and non negative.", location);
      return false;
    }

    return true;
  }

  public IProjectileSection Read(ProjectilePrototype prototype)
  {
    // 「没有这一族」与「声明了但关闭」在实体上必须可区分，所以缺省也给出关闭态 section。
    ProjectileGravityPrototype? gravity = prototype.Gravity;
    return gravity is null
        ? new ProjectileGravitySection(enabled: false, strength: 0f, maximumFallSpeed: 0f)
        : new ProjectileGravitySection(
            gravity.Enabled, gravity.Strength, gravity.MaximumFallSpeed);
  }
}
