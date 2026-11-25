using System;

namespace Terraria.Share.Movement;

/// <summary>
/// 参考客户端中与重力相关的 Buff 内容样例。
/// </summary>
/// <remarks>
/// 数值取自参考客户端源码，不做推测：
/// <list type="bullet">
/// <item><c>Terraria/Player.cs:10064-10066</c> 羽落（Buff 8）令 <c>slowFall = true</c>，</item>
/// <item><c>Terraria/Player.cs:21822-21826</c> 该标志把下落加速度改为 <c>gravity / 3f</c>，</item>
/// <item><c>Terraria/Player.cs:10112-10114</c> 重力药水（Buff 18）令 <c>gravControl = true</c>，</item>
/// <item><c>Terraria/Item.cs:4910</c> 羽落时长 36000 tick，<c>:5050</c> 重力药水时长 10800 tick。</item>
/// </list>
/// 客户端用 <c>gravity / 3f</c> 直接改写局部变量；这里改写为等价的
/// <see cref="GravityModifierKind.MagnitudeMultiplier"/> 来源贡献，因此同一份内容可以被任意
/// 重力提供者（药水、饰品、区域）复用，并随 <c>remainingFrames</c> 自然过期。
/// </remarks>
public static class GravityContent {
  /// <summary>羽落的下落加速度系数：客户端 <c>gravity / 3f</c>。</summary>
  public const float FeatherfallMagnitudeMultiplier = 1f / 3f;

  /// <summary>羽落时长，单位帧（60 帧/秒）：36000 / 60 = 600 秒 = 10 分钟。</summary>
  public const int FeatherfallDurationFrames = 36000;

  /// <summary>重力药水时长，单位帧：10800 / 60 = 180 秒 = 3 分钟。</summary>
  public const int GravitationDurationFrames = 10800;

  /// <summary>
  /// 羽落：把重力加速度乘到三分之一。
  /// </summary>
  public static GravityModifier Featherfall(
      long sourceId,
      int remainingFrames = FeatherfallDurationFrames) {
    return new GravityModifier(
        sourceId,
        GravityModifierKind.MagnitudeMultiplier,
        FeatherfallMagnitudeMultiplier,
        remainingFrames: remainingFrames);
  }

  /// <summary>
  /// 重力药水：允许该实体切换重力方向。
  /// </summary>
  public static GravityModifier Gravitation(
      long sourceId,
      int remainingFrames = GravitationDurationFrames) {
    return new GravityModifier(
        sourceId,
        GravityModifierKind.ToggleDirectionCapability,
        1f,
        remainingFrames: remainingFrames);
  }
}
