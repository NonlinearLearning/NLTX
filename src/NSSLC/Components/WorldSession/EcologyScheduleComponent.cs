using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生态传播的调度配置、采样位置和修改预算。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：AllowedToSpreadInfections（第 4185 行）。</para>
/// <para>重组说明：GenerationId、ScheduleRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-world-generation-and-ecology-component-code-draft.md。
/// </para>
/// <para>依据位置：第 869 行。</para>
/// <para>
/// 用途：生态更新流程读取地上与地下采样坐标、更新速率和感染传播许可，控制本轮生态修改。
/// </para>
/// <para>
/// 当前实现：GenerationId 在构造时确定且非负，其余配置在初始化时写入。传播启用要求允许感染且更新速率大于零；预算已配置要求预算非空且非负。采样与地形修改由外部
/// 流程执行。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C182，L2（通用可选玩法）。分级理由：生态采样、结构保留区、地层统计、扫描和地形变换适用于可编辑／动态世界，固定小地图可减少。
/// </para>
/// </remarks>
public sealed class EcologyScheduleComponent
{
  public EcologyScheduleComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 是否允许感染传播。
  /// </summary>
  public bool IsInfectionSpreadAllowed { get; init; }

  /// <summary>
  /// 地上生态采样横坐标，可为空。
  /// </summary>
  public int? OvergroundSampleX { get; init; }

  /// <summary>
  /// 地上生态采样纵坐标，可为空。
  /// </summary>
  public int? OvergroundSampleY { get; init; }

  /// <summary>
  /// 地下生态采样横坐标，可为空。
  /// </summary>
  public int? UndergroundSampleX { get; init; }

  /// <summary>
  /// 地下生态采样纵坐标，可为空。
  /// </summary>
  public int? UndergroundSampleY { get; init; }

  /// <summary>
  /// 世界生态更新速率输入，可为空。
  /// </summary>
  public int? WorldUpdateRate { get; init; }

  /// <summary>
  /// 本轮允许的生态修改预算，可为空。
  /// </summary>
  public int? EcologyMutationBudget { get; init; }

  /// <summary>
  /// 生态调度配置版本。
  /// </summary>
  public ulong ScheduleRevision { get; init; }

  /// <summary>
  /// 允许感染传播且更新速率大于零时返回 true。
  /// </summary>
  public bool IsEcologyPropagationEnabled =>
    IsInfectionSpreadAllowed && WorldUpdateRate is > 0;

  /// <summary>
  /// 生态修改预算已提供且非负时返回 true。
  /// </summary>
  public bool HasConfiguredBudget => EcologyMutationBudget is >= 0;
}
