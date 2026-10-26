namespace EntityEcs.Components;
public struct OxygenBarComponent
{
  /// <summary>
  /// 当前剩余呼吸量；本文件尚未提供对外写入方法。
  /// </summary>
  public int Breath { get; private set; }

  /// <summary>
  /// 共享呼吸规则提供的上限。
  /// </summary>
  public int BreathMax ;

  /// <summary>
  /// 呼吸消耗节奏计数；是否按 tick 推进及换算方式由后续实际更新流程定义。
  /// </summary>
  public int BreathCounter { get; private set; }//多少tick更新一次氧气条,为什么不直接乘算到氧气条中去这样就可以依赖tick为单位进行更新
}