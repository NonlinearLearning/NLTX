using System;
using System.Collections.Generic;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>把投射物的权威 ECS 值投影成行为可读的不可变事实。</summary>
/// <remarks>
/// 本类型自己不持有查询、也不认识任何一种事实：它只在构造时校验 contributor 集合，
/// 运行时只问那些"事实被当前行为声明过"的 contributor。每个事实的来源域住在各自的
/// contributor 里，所以某个域换投影方式时不需要动这里。
/// <para>
/// 行为只看到事实，看不到 <see cref="ArchWorld" />：这让"追踪怎么算"与"数据从哪来"分开，
/// 也让行为可以在没有世界的验证项目里直接跑。
/// </para>
/// <para>
/// 目标事实由 <see cref="ProjectileTargetComponent" />（句柄 + 版本）加目标实体的实时位置合成；
/// 目标丢失、死亡或不可见时不写入该事实，行为端会退化成匀速直线，而不是追击一个幻影。
/// </para>
/// </remarks>
public sealed class ProjectileAiSnapshotProvider : IAiSnapshotProvider<ArchEntity>
{
  private readonly IProjectileAiSnapshotContributor[] _contributors;
  private readonly IReadOnlyList<Type> _factTypes;

  public ProjectileAiSnapshotProvider(ArchWorld world, IEntityRuntimeResolver resolver)
      : this(ProjectileAiSnapshotContributors.CreateDefault(world, resolver))
  {
  }

  public ProjectileAiSnapshotProvider(IReadOnlyList<IProjectileAiSnapshotContributor> contributors)
  {
    ArgumentNullException.ThrowIfNull(contributors);
    List<Type> factTypes = [];
    HashSet<Type> claimed = [];
    _contributors = new IProjectileAiSnapshotContributor[contributors.Count];
    for (int i = 0; i < contributors.Count; i++)
    {
      IProjectileAiSnapshotContributor contributor = contributors[i] ??
          throw new ArgumentException(
              "A snapshot contributor cannot be null.", nameof(contributors));
      _contributors[i] = contributor;
      foreach (Type factType in contributor.FactTypes)
      {
        if (!claimed.Add(factType))
        {
          throw new ArgumentException(
              $"AI fact {factType.FullName} is claimed by more than one snapshot contributor.",
              nameof(contributors));
        }

        factTypes.Add(factType);
      }
    }

    _factTypes = factTypes.AsReadOnly();
  }

  /// <summary>Fact types this provider can project, in contributor order.</summary>
  public IReadOnlyList<Type> FactTypes => _factTypes;

  public AiSnapshot Capture(ArchEntity entity, IReadOnlyList<Type> requiredComponents)
  {
    ArgumentNullException.ThrowIfNull(requiredComponents);
    AiSnapshot snapshot = AiSnapshot.Empty;
    foreach (IProjectileAiSnapshotContributor contributor in _contributors)
    {
      if (!ProjectsAny(contributor, requiredComponents))
      {
        continue;
      }

      snapshot = contributor.Contribute(entity, snapshot);
    }

    return snapshot;
  }

  private static bool ProjectsAny(
      IProjectileAiSnapshotContributor contributor,
      IReadOnlyList<Type> requiredComponents)
  {
    foreach (Type factType in contributor.FactTypes)
    {
      if (requiredComponents.Contains(factType))
      {
        return true;
      }
    }

    return false;
  }
}
