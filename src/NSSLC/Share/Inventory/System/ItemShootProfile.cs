using System;
using System.Numerics;

namespace Terraria.Share.Items;

/// <summary>射击内容参数：使用该物品时生成哪个投射物、以什么速度从哪里出发。</summary>
/// <remarks>
/// 投射物 id 以内容 id 文本表达，而不是 <c>Terraria.Unshared.Projectiles.ProjectileId</c>：
/// 共享层不得依赖非共享领域类型，解析与存在性校验由投射物域在生成接缝处完成。
/// 瞄准方向尚未建模（协议里没有可用的朝向状态），因此速度由内容声明，记录为未闭合项。
/// </remarks>
public readonly record struct ItemShootProfile
{
  public ItemShootProfile(string projectileId, Vector2 velocity, Vector2 offset)
  {
    if (string.IsNullOrWhiteSpace(projectileId))
    {
      throw new ArgumentException("A shoot projectile id is required.", nameof(projectileId));
    }

    if (!float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y) ||
        (velocity.X == 0f && velocity.Y == 0f))
    {
      throw new ArgumentOutOfRangeException(nameof(velocity));
    }

    if (!float.IsFinite(offset.X) || !float.IsFinite(offset.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(offset));
    }

    ProjectileId = projectileId;
    Velocity = velocity;
    Offset = offset;
  }

  /// <summary>投射物内容定义 id 文本。</summary>
  public string ProjectileId { get; }

  /// <summary>出生速度，单位是像素每 tick。</summary>
  public Vector2 Velocity { get; }

  /// <summary>相对持有者位置的出生偏移。</summary>
  public Vector2 Offset { get; }
}
