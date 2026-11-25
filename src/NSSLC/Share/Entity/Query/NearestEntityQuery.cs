using System;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;

namespace Terraria.Share.Entities;

/// <summary>
/// Decides whether one candidate entity may take part in the nearest-entity search.
/// </summary>
/// <remarks>
/// 判据由调用方给出：阵营比法、存活口径、排除句柄都不同（NPC 找同阵营、投射物找"不是我这边的"
/// 并排除归属者），<see cref="NearestEntityQuery" /> 只负责"候选里挑最近"这件事本身。
/// </remarks>
public delegate bool NearestEntityPredicate(
    ArchEntity entity,
    in EntityFactionComponent faction,
    in LifeComponent life);

/// <summary>One entity chosen by <see cref="NearestEntityQuery" />.</summary>
/// <param name="Entity">The candidate entity.</param>
/// <param name="Position">The candidate's location at selection time.</param>
/// <param name="DistanceSquared">Squared distance from the query origin.</param>
public readonly record struct NearestEntityCandidate(
    ArchEntity Entity,
    Vector2 Position,
    float DistanceSquared);

/// <summary>
/// Picks the nearest entity that carries faction, life and location, using a caller-supplied
/// predicate as the candidate filter.
/// </summary>
/// <remarks>
/// 这是"最近实体"空间选择的唯一实现：NPC 的目标投影与投射物的目标锁定都经由它，
/// 两侧只保留各自的判据与落点，不再各自复制一遍查询、存活过滤与距离平方比较。
/// <para>
/// 距离口径：<paramref name="maximumDistance" /> 为上限，严格小于（不含端点）；
/// 传 0 或负数表示不限制距离。等距候选取先遍历到的那个，因此结果只取决于查询顺序。
/// </para>
/// <para>
/// 本类只读世界状态：不写组件、不改队列，也不做实体创建或销毁。
/// </para>
/// </remarks>
public static class NearestEntityQuery
{
  /// <summary>
  /// Searches <paramref name="candidates" /> for the entity nearest to <paramref name="origin" />.
  /// </summary>
  /// <param name="world">World to query.</param>
  /// <param name="candidates">Query describing candidate entities.</param>
  /// <param name="origin">Origin the distances are measured from.</param>
  /// <param name="maximumDistance">Upper bound on distance; 0 or negative means unlimited.</param>
  /// <param name="predicate">Filter applied to every candidate before distance comparison.</param>
  /// <param name="nearest">The chosen candidate when the method returns <see langword="true" />.</param>
  /// <returns><see langword="true" /> when at least one candidate passed the filter inside the bound.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="world" /> or <paramref name="predicate" /> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="origin" /> or <paramref name="maximumDistance" /> is not finite.
  /// </exception>
  public static bool TryFind(
      ArchWorld world,
      in QueryDescription candidates,
      Vector2 origin,
      float maximumDistance,
      NearestEntityPredicate predicate,
      out NearestEntityCandidate nearest)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(predicate);
    if (!float.IsFinite(origin.X) || !float.IsFinite(origin.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(origin));
    }

    if (!float.IsFinite(maximumDistance))
    {
      throw new ArgumentOutOfRangeException(nameof(maximumDistance));
    }

    float nearestDistance =
        maximumDistance > 0f ? maximumDistance * maximumDistance : float.PositiveInfinity;
    NearestEntityCandidate found = default;
    bool any = false;
    world.Query(
        in candidates,
        (ArchEntity entity,
            ref EntityFactionComponent faction,
            ref LifeComponent life,
            ref LocationComponent location) =>
        {
          if (!predicate(entity, in faction, in life))
          {
            return;
          }

          float distance = Vector2.DistanceSquared(origin, location.Coordinates);
          if (distance >= nearestDistance)
          {
            return;
          }

          nearestDistance = distance;
          found = new NearestEntityCandidate(entity, location.Coordinates, distance);
          any = true;
        });

    nearest = found;
    return any;
  }
}
