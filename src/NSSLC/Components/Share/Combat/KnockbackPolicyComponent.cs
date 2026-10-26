namespace Terraria.Combat;

/// <summary>
/// 保存主体的击退免疫标记和击退响应系数。
/// </summary>
public struct KnockbackComponent
{
  /// <summary>
  /// 是否完全免疫击退。  免疫击退是一种buff
  /// </summary>
  ///public bool IsImmune;
  /// <summary>
  /// 沿用原版 knockBackResist 的击退响应系数；具体倍率公式由外部战斗流程解释。
  /// </summary>
  public float Resistance;

}
