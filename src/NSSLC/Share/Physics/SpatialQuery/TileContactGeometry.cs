using System;
using System.Numerics;

namespace Terraria.Share.Physics;

/// <summary>一次瓦片碰撞的几何细节：撞到哪块瓦片、接触点在哪、法线朝哪。</summary>
/// <remarks>
/// 只描述几何，不描述后果：反弹、贴壁、销毁都由下游按自己的策略决定。
/// 法线单位为 1，从瓦片指向实体，与入射速度反向。
/// <para>
/// 瓦片索引用两个整数而不是基础设施类型：物理层不依赖 World 域，
/// 也不该为了一个索引把 Infrastructure 的类型拖进来。
/// </para>
/// </remarks>
public readonly record struct TileContactGeometry
{
  public TileContactGeometry(int tileX, int tileY, Vector2 point, Vector2 normal)
  {
    if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) ||
        !float.IsFinite(normal.X) || !float.IsFinite(normal.Y) ||
        normal == Vector2.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(point));
    }

    TileX = tileX;
    TileY = tileY;
    Point = point;
    Normal = normal;
    HasContact = true;
  }

  public int TileX { get; }

  public int TileY { get; }

  public Vector2 Point { get; }

  public Vector2 Normal { get; }

  /// <summary>是否真的发生过接触；默认实例表示没有。</summary>
  public bool HasContact { get; }
}
