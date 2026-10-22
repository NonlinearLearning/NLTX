namespace EntityEcs.Components;
/// <summary>
/// 保存主体剩余呼吸量及呼吸消耗计数。
/// </summary>
/// <remarks>
/// <para>
/// 用途：为需要呼吸资源的实体提供剩余量、上限和消耗节奏状态，供外部环境流程读取。
/// </para>
/// <para>
/// 当前实现：Breath 和 BreathCounter 只有私有 setter，BreathMax 从 NpcBreathRuleDefinition 读取。本文件尚未
/// 提供构造赋值、消耗、恢复或重置方法，默认结构体的两个计数为零。
/// </para>
/// </remarks>
public struct OxygenBarComponent
{
  /// <summary>
  /// 当前剩余呼吸量；本文件尚未提供对外写入方法。
  /// </summary>
  public int Breath { get; private set; }

  /// <summary>
  /// 共享呼吸规则提供的上限。
  /// </summary>
  public int BreathMax => NpcBreathRuleDefinition.BreathMax;

  /// <summary>
  /// 呼吸消耗节奏计数；是否按 tick 推进及换算方式由后续实际更新流程定义。
  /// </summary>
  public int BreathCounter { get; private set; }//多少tick更新一次氧气条,为什么不直接乘算到氧气条中去这样就可以依赖tick为单位进行更新
}