namespace Terraria.Movement;

/// <summary>
/// 保存重力修饰器求值后的当前帧投影，供移动和碰撞系统读取。
/// </summary>
public readonly record struct GravityStateComponent(
  float GravityMagnitude,
  float MaxFallSpeed,
  GravityDirection Direction,
  bool CanToggleDirection,
  bool HasDirectionOverride);
