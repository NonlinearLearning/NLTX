using System;

namespace Terraria.Share.Movement;

/// <summary>声明一个实体在一次空间推进里要切成几个子步。</summary>
/// <remarks>
/// 子步不是"多走几次"，而是把同一段位移切成 N 段分别做碰撞：总位移不变，碰撞精度提高。
/// 高速弹体因此不会在一帧里跨过整面墙（隧穿）。默认 1 表示不分子步。
/// <para>
/// 为什么放在 Movement 而不是 Projectile：这是空间推进的通用能力，任何高速实体都可能需要；
/// 把它挂到某个领域会让物理层反向依赖那个领域。
/// </para>
/// </remarks>
public readonly record struct SubstepCapabilityComponent
{
  /// <summary>子步数上限，防止内容写出一个让单帧开销爆炸的值。</summary>
  public const int MaximumCount = 1025;

  public SubstepCapabilityComponent(int count)
  {
    if (count < 1 || count > MaximumCount)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    Count = count;
  }

  /// <summary>一次推进内的子步数量，至少为 1。</summary>
  public int Count { get; }

  public static SubstepCapabilityComponent Single => new(1);
}
