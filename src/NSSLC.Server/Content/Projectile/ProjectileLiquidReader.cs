using NSSLC.Infrastructure.Lvcs;
using Terraria.Share.Liquid;
using Terraria.Unshared.Projectiles;

namespace NSSLC.Server.Content.Projectile;

/// <summary>液体族。关闭时也挂能力组件，保持「声明了但关闭」可区分。</summary>
internal sealed class ProjectileLiquidReader : IProjectileSectionReader
{
  public string Family => "liquid";

  public bool Validate(
      ProjectilePrototype prototype,
      LvcsFamilyId familyId,
      string location,
      LvcsDiagnosticSink diagnostics)
  {
    if (prototype.Liquid is ProjectileLiquidPrototype liquid &&
        liquid.Enabled &&
        !liquid.Water && !liquid.Honey && !liquid.Lava && !liquid.Shimmer)
    {
      diagnostics.Error(
          familyId, prototype.Id, "Liquid",
          "An enabled liquid policy must declare at least one affected liquid.", location);
      return false;
    }

    return true;
  }

  public IProjectileSection Read(ProjectilePrototype prototype)
  {
    ProjectileLiquidPrototype? liquid = prototype.Liquid;
    return liquid is null
        ? new ProjectileLiquidSection(enabled: false, LiquidContactFlags.None)
        : new ProjectileLiquidSection(liquid.Enabled, ResolveFlags(liquid));
  }

  private static LiquidContactFlags ResolveFlags(ProjectileLiquidPrototype liquid)
  {
    LiquidContactFlags flags = LiquidContactFlags.None;
    if (liquid.Water)
    {
      flags |= LiquidContactFlags.Water;
    }

    if (liquid.Honey)
    {
      flags |= LiquidContactFlags.Honey;
    }

    if (liquid.Lava)
    {
      flags |= LiquidContactFlags.Lava;
    }

    if (liquid.Shimmer)
    {
      flags |= LiquidContactFlags.Shimmer;
    }

    return flags;
  }
}
