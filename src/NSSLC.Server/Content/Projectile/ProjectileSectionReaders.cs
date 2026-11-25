using System.Collections.Generic;

namespace NSSLC.Server.Content.Projectile;

/// <summary>内容族读取器的登记处：新增一族只改这里的列表，<see cref="ProjectileCodec" /> 不动。</summary>
/// <remarks>
/// 顺序即 section 在配方里的顺序。顺序本身不承载语义（折叠是两遍、与顺序无关），
/// 但保持稳定可读，便于诊断与快照对照。
/// </remarks>
internal static class ProjectileSectionReaders
{
  public static IReadOnlyList<IProjectileSectionReader> Create() =>
  [
      new ProjectileBasicsReader(),
      new ProjectileGravityReader(),
      new ProjectileLiquidReader(),
      new ProjectilePenetrationReader(),
      new ProjectileBounceReader(),
      new ProjectileHomingReader(),
  ];
}
