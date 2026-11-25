using System;
using System.Numerics;
using Terraria.Share.Relationships;

namespace Terraria.Share.Physics;

/// <summary>本 tick 内某个实体撞到实心瓦片的一次接触事实。</summary>
/// <remarks>
/// 与 <see cref="PhysicsContactFact" /> 同类：只描述几何（撞到哪块瓦片、法线朝哪、以什么速度撞上来），
/// 不提交状态，也不判断"该不该反弹/该不该死"。瓦片不是实体、没有句柄，所以单独成类型，
/// 免得下游把"撞墙"和"撞到某个实体"混为一谈。
/// </remarks>
public readonly record struct TileContactFact
{
  public TileContactFact(
      EntityRuntimeHandle entity,
      TileContactGeometry geometry,
      Vector2 incomingVelocity,
      long tick,
      SpatialWorldRevision revision,
      int substep = 0)
  {
    if (!entity.IsValid)
    {
      throw new ArgumentException("A tile contact requires a valid entity handle.", nameof(entity));
    }

    if (!geometry.HasContact)
    {
      throw new ArgumentException("A tile contact requires contact geometry.", nameof(geometry));
    }

    if (tick < 0 || !revision.IsValid || substep < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (!float.IsFinite(incomingVelocity.X) || !float.IsFinite(incomingVelocity.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(incomingVelocity));
    }

    Entity = entity;
    Geometry = geometry;
    IncomingVelocity = incomingVelocity;
    Tick = tick;
    Revision = revision;
    Substep = substep;
  }

  public EntityRuntimeHandle Entity { get; }

  public TileContactGeometry Geometry { get; }

  /// <summary>碰撞前的速度，供反射与伤害计算使用。</summary>
  public Vector2 IncomingVelocity { get; }

  public long Tick { get; }

  public SpatialWorldRevision Revision { get; }

  /// <summary>本次 tick 内发生在第几个子步。</summary>
  public int Substep { get; }
}
