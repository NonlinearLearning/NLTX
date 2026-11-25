using System;
using Terraria.Share.Lvcs;
using Terraria.Unshared.Projectiles;

namespace NSSLC.Server.Content.Projectile;

/// <summary>反弹族。族缺席时返回 null——「不会反弹」不是一种策略，而是没有这条策略。</summary>
internal sealed class ProjectileBounceReader : IProjectileSectionReader
{
  public string Family => "bounce";

  public bool Validate(
      ProjectilePrototype prototype,
      LvcsFamilyId familyId,
      string location,
      LvcsDiagnosticSink diagnostics)
  {
    if (prototype.Bounce is not ProjectileBouncePrototype bounce)
    {
      return true;
    }

    if (bounce.MaximumBounces < ProjectileBounceSection.Unlimited)
    {
      diagnostics.Error(
          familyId, prototype.Id, "Bounce",
          "bounce.maximumBounces must be -1 or greater.", location);
      return false;
    }

    if (bounce.PenetrationCost < 0)
    {
      diagnostics.Error(
          familyId, prototype.Id, "Bounce",
          "bounce.penetrationCost must not be negative.", location);
      return false;
    }

    if (!float.IsFinite(bounce.SpeedScale) || bounce.SpeedScale < 0f)
    {
      diagnostics.Error(
          familyId, prototype.Id, "Bounce",
          "bounce.speedScale must be finite and non negative.", location);
      return false;
    }

    if (!TryParseConsumeOrder(bounce.ConsumeOrder, out _))
    {
      diagnostics.Error(
          familyId, prototype.Id, "Bounce",
          "Unknown bounce.consumeOrder '" + bounce.ConsumeOrder + "'.", location);
      return false;
    }

    return true;
  }

  public IProjectileSection? Read(ProjectilePrototype prototype)
  {
    if (prototype.Bounce is not ProjectileBouncePrototype bounce)
    {
      return null;
    }

    if (!TryParseConsumeOrder(bounce.ConsumeOrder, out ProjectileBounceConsumeOrder order))
    {
      throw new ArgumentException(
          "Unknown bounce consume order '" + bounce.ConsumeOrder + "'.", nameof(prototype));
    }

    return new ProjectileBounceSection(
        bounce.MaximumBounces, bounce.PenetrationCost, bounce.SpeedScale, order);
  }

  private static bool TryParseConsumeOrder(
      string value,
      out ProjectileBounceConsumeOrder order)
  {
    return Enum.TryParse(value, ignoreCase: false, out order) && Enum.IsDefined(order);
  }
}
