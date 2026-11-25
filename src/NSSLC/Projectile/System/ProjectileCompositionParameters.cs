using System;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;

namespace Terraria.Unshared.Projectiles;

/// <summary>Cross-family values collected before any component is constructed.</summary>
/// <remarks>
/// 这里只放**确实会被他族读取**的值。每个键只能被一个 section 声明，重复声明视为内容自相矛盾；
/// 折叠结束后 <see cref="Seal" /> 关闭写入面，防止 <c>Contribute</c> 期间又改数值。
/// <para>
/// 默认值保证「没有该族的 section」时装配结果仍然确定：转向加速度 0、飞行模式、子步 1。
/// </para>
/// </remarks>
public sealed class ProjectileCompositionParameters
{
  private bool _sealed;
  private bool _movementAccelerationDeclared;
  private bool _movementModeDeclared;
  private bool _substepCountDeclared;
  private bool _tileCollisionDeclared;
  private bool _liquidDeclared;

  /// <summary>转向加速度：由追踪族声明，由运动能力组件读取。</summary>
  public float MovementAcceleration { get; private set; }

  /// <summary>移动模式：由重力族声明（受重力必须 Free，否则 Flight 跳过重力积分）。</summary>
  public MovementMode MovementMode { get; private set; } = MovementMode.Flight;

  /// <summary>子步数：由平铺族按 extraUpdates 声明。</summary>
  public int SubstepCount { get; private set; } = 1;

  /// <summary>是否参与瓦片碰撞：由平铺族声明。</summary>
  public bool TileCollisionEnabled { get; private set; } = true;

  /// <summary>液体交互开关：由液体族声明。</summary>
  public bool LiquidEnabled { get; private set; }

  /// <summary>受影响液体类别：由液体族声明。</summary>
  public LiquidContactFlags AffectedLiquids { get; private set; } = LiquidContactFlags.None;

  public void DeclareMovementAcceleration(float acceleration)
  {
    if (!float.IsFinite(acceleration) || acceleration < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(acceleration));
    }

    EnsureWritable(nameof(MovementAcceleration), _movementAccelerationDeclared);
    _movementAccelerationDeclared = true;
    MovementAcceleration = acceleration;
  }

  public void DeclareMovementMode(MovementMode mode)
  {
    if (!Enum.IsDefined(mode))
    {
      throw new ArgumentOutOfRangeException(nameof(mode));
    }

    EnsureWritable(nameof(MovementMode), _movementModeDeclared);
    _movementModeDeclared = true;
    MovementMode = mode;
  }

  public void DeclareSubstepCount(int count)
  {
    if (count < 1 || count > ProjectileBasicsSection.MaximumSubstepCount)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    EnsureWritable(nameof(SubstepCount), _substepCountDeclared);
    _substepCountDeclared = true;
    SubstepCount = count;
  }

  public void DeclareTileCollision(bool enabled)
  {
    EnsureWritable(nameof(TileCollisionEnabled), _tileCollisionDeclared);
    _tileCollisionDeclared = true;
    TileCollisionEnabled = enabled;
  }

  public void DeclareLiquid(bool enabled, LiquidContactFlags affectedLiquids)
  {
    if (enabled && affectedLiquids == LiquidContactFlags.None)
    {
      throw new ArgumentException(
          "An enabled liquid family must declare at least one affected liquid.",
          nameof(affectedLiquids));
    }

    EnsureWritable(nameof(LiquidEnabled), _liquidDeclared);
    _liquidDeclared = true;
    LiquidEnabled = enabled;
    AffectedLiquids = affectedLiquids;
  }

  /// <summary>关闭写入面；此后再声明任何键都抛异常。</summary>
  public void Seal()
  {
    _sealed = true;
  }

  private void EnsureWritable(string key, bool alreadyDeclared)
  {
    if (_sealed)
    {
      throw new InvalidOperationException(
          string.Concat("Projectile composition parameter '", key, "' cannot be written after Seal()."));
    }

    if (alreadyDeclared)
    {
      throw new InvalidOperationException(
          string.Concat("Projectile composition parameter '", key, "' is declared by more than one section."));
    }
  }
}
