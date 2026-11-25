using System;
using System.Numerics;
using Terraria.Share.Relationships;

namespace Terraria.Share.Emission;

/// <summary>一次发射意图：在哪、朝哪射什么。</summary>
/// <remarks>
/// 三个字段都是自足的：不含速度（出生速度归 AI 行为计算）、不含碰撞体与尺寸
/// （几何属于实体定义，物理系统只消费）、不含寿命与免疫（属内容定义与属性域）。
/// <para>
/// <see cref="ContentId"/> 是内容 id <b>文本</b>而不是投射物域的 <c>ProjectileId</c>：
/// 共享层不得依赖非共享领域类型，解析与存在性校验由投射物域在排空接缝处完成。
/// 这与 <c>ItemShootProfile</c> 的既有口径一致。
/// </para>
/// <para>
/// <see cref="Origin"/> 取<b>绝对世界坐标</b>。发射方的局部偏移由调用方在入队前加上——
/// Emission 不读发射者位置，否则它的输入就不再自足。
/// </para>
/// </remarks>
public readonly record struct EmissionRequest
{
  public EmissionRequest(EntityRuntimeHandle emitter, string contentId, Vector2 origin)
  {
    if (!emitter.IsValid)
    {
      throw new ArgumentException("An emission request requires a valid emitter handle.", nameof(emitter));
    }

    if (string.IsNullOrWhiteSpace(contentId))
    {
      throw new ArgumentException("An emission request requires a content id.", nameof(contentId));
    }

    if (!float.IsFinite(origin.X) || !float.IsFinite(origin.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(origin));
    }

    Emitter = emitter;
    ContentId = contentId;
    Origin = origin;
  }

  /// <summary>谁发射的。同时决定生成请求的归属与实体阵营的推导来源。</summary>
  public EntityRuntimeHandle Emitter { get; }

  /// <summary>发射内容 id 文本，由投射物域在排空时解析。</summary>
  public string ContentId { get; }

  /// <summary>生成位置的绝对世界坐标。</summary>
  public Vector2 Origin { get; }
}
