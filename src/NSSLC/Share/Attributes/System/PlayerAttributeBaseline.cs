using System;

namespace Terraria.Share.Attributes;

/// <summary>
/// 玩家的属性基值样例，取自参考客户端的默认值。
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><c>Terraria/Player.cs:18441</c> <c>statDefense = 0</c></item>
/// <item><c>Terraria/Player.cs:18462</c> <c>moveSpeed = 1f</c></item>
/// </list>
/// 这两项与 <c>AttributeCalculator.Resolve</c> 的 fallback 语义一致（缺省为 0，乘法从 1 起算），
/// 因此写入它们不改变任何现有结算结果；它们的意义是把"基值从哪来"显式化，
/// 后续药水、装备的 <see cref="AttributeContribution"/> 才有可叠加的基准。
/// </remarks>
public static class PlayerAttributeBaseline {
  /// <summary>客户端 <c>moveSpeed</c> 的默认倍率。</summary>
  public const float DefaultMovementSpeed = 1f;

  /// <summary>客户端 <c>statDefense</c> 的默认值。</summary>
  public const float DefaultDefense = 0f;

  /// <summary>
  /// 写入玩家基值。
  /// </summary>
  public static void Apply(AttributeSetComponent attributes, int? maximumLife = null) {
    ArgumentNullException.ThrowIfNull(attributes);
    attributes.SetBaseValue(
        new AttributeBaseValue(AttributeKind.MovementSpeed, DefaultMovementSpeed));
    attributes.SetBaseValue(new AttributeBaseValue(AttributeKind.Defense, DefaultDefense));
    if (maximumLife.HasValue)
    {
      if (maximumLife.Value <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(maximumLife));
      }

      attributes.SetBaseValue(new AttributeBaseValue(AttributeKind.MaximumLife, maximumLife.Value));
    }
  }
}
