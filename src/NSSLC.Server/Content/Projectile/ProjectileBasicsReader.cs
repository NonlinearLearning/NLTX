using System;
using System.Numerics;
using NSSLC.Infrastructure.Lvcs;
using Terraria.Share.Ai;
using Terraria.Share.Combat;
using Terraria.Unshared.Projectiles;

namespace NSSLC.Server.Content.Projectile;

/// <summary>平铺族：行为选择、推进次数、寿命、碰撞体与尺寸、攻击与伤害。</summary>
/// <remarks>
/// 这一族没有可选开关：缺了它内容就没有身份，因此总能读出一个 section。
/// 取整尺寸不在这里算——那是 section 构造函数的职责（单点派生）。
/// </remarks>
internal sealed class ProjectileBasicsReader : IProjectileSectionReader
{
  public string Family => "basics";

  public bool Validate(
      ProjectilePrototype prototype,
      LvcsFamilyId familyId,
      string location,
      LvcsDiagnosticSink diagnostics)
  {
    if (string.IsNullOrWhiteSpace(prototype.BehaviorId))
    {
      return Fail(diagnostics, familyId, prototype, "BehaviorId",
          "Entry declares no behavior id.", location);
    }

    if (string.IsNullOrWhiteSpace(prototype.ProfileId))
    {
      return Fail(diagnostics, familyId, prototype, "ProfileId",
          "Entry declares no behavior profile id.", location);
    }

    if (prototype.ExtraUpdates < 0 ||
        prototype.ExtraUpdates > ProjectileBasicsSection.MaximumExtraUpdates)
    {
      return Fail(diagnostics, familyId, prototype, "ExtraUpdates",
          "extraUpdates must be between 0 and " +
              ProjectileBasicsSection.MaximumExtraUpdates + ".", location);
    }

    if (prototype.LifetimeTicks < 0)
    {
      return Fail(diagnostics, familyId, prototype, "LifetimeTicks",
          "lifetimeTicks must not be negative.", location);
    }

    if (prototype.Collider is not ProjectileColliderPrototype collider ||
        !float.IsFinite(collider.Width) || collider.Width <= 0f ||
        !float.IsFinite(collider.Height) || collider.Height <= 0f)
    {
      return Fail(diagnostics, familyId, prototype, "Collider",
          "collider requires a positive finite width and height.", location);
    }

    if (string.IsNullOrWhiteSpace(prototype.AttackId))
    {
      return Fail(diagnostics, familyId, prototype, "AttackId",
          "Entry declares no attack id.", location);
    }

    if (!TryParseDamageType(prototype.DamageType, out _))
    {
      return Fail(diagnostics, familyId, prototype, "DamageType",
          "Unknown damage type '" + prototype.DamageType + "'.", location);
    }

    if (prototype.BaseDamage < 0)
    {
      return Fail(diagnostics, familyId, prototype, "BaseDamage",
          "baseDamage must not be negative.", location);
    }

    if (prototype.InitialVelocity is ProjectileInitialVelocityPrototype velocity &&
        (!float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y)))
    {
      return Fail(diagnostics, familyId, prototype, "InitialVelocity",
          "initialVelocity requires finite x and y.", location);
    }

    return true;
  }

  public IProjectileSection Read(ProjectilePrototype prototype)
  {
    ProjectileColliderPrototype collider = prototype.Collider!;
    if (!TryParseDamageType(prototype.DamageType, out DamageType damageType))
    {
      throw new ArgumentException(
          "Unknown damage type '" + prototype.DamageType + "'.", nameof(prototype));
    }

    // 省略 initialVelocity 时为零速：生成后完全由 AI 决定运动的弹体不需要它。
    Vector2 initialVelocity = prototype.InitialVelocity is ProjectileInitialVelocityPrototype v
        ? new Vector2(v.X, v.Y)
        : Vector2.Zero;
    return new ProjectileBasicsSection(
        new AiBehaviorId(prototype.BehaviorId),
        new AiBehaviorProfileId(prototype.ProfileId),
        prototype.ExtraUpdates,
        prototype.LifetimeTicks,
        prototype.TileCollide,
        prototype.IgnoreOwner,
        collider.Width,
        collider.Height,
        new CombatAttackId(prototype.AttackId),
        damageType,
        prototype.BaseDamage,
        initialVelocity);
  }

  private static bool TryParseDamageType(string value, out DamageType damageType)
  {
    return Enum.TryParse(value, ignoreCase: false, out damageType) &&
        Enum.IsDefined(damageType);
  }

  private static bool Fail(
      LvcsDiagnosticSink diagnostics,
      LvcsFamilyId familyId,
      ProjectilePrototype prototype,
      string field,
      string message,
      string location)
  {
    diagnostics.Error(familyId, prototype.Id, field, message, location);
    return false;
  }
}
