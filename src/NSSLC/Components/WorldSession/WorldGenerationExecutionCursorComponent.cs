using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成执行流程当前所在的步骤编号。
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成当前步骤的标识。</para>
/// <para>拆分来源：Terraria.WorldBuilding.WorldGenerator。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerator.cs。</para>
/// <para>主要源成员：_currentPass（第 274 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 411 行。</para>
/// <para>
/// 用途：执行器提交当前步骤，调度、进度展示和控制流程读取执行位置。
/// </para>
/// <para>
/// 当前实现：ReplaceState 允许 null 表示没有当前步骤，非空编号必须包含有效文本；HasCurrentPass 只检查是否非空。实际步骤执行由外部生成器负
/// 责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C188，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
/// </remarks>
public sealed class WorldGenerationExecutionCursorComponent
{
  public WorldGenerationExecutionCursorComponent(string? currentPassId = null)
  {
    ReplaceState(currentPassId);
  }

  /// <summary>
  /// 当前执行步骤编号；未执行任何步骤时可为空。
  /// </summary>
  public string? CurrentPassId { get; private set; }

  /// <summary>
  /// 当前步骤编号非空时返回 true。
  /// </summary>
  public bool HasCurrentPass => CurrentPassId is not null;

  /// <summary>
  /// 校验并替换当前步骤编号。
  /// </summary>
  internal void ReplaceState(string? currentPassId)
  {
    if (currentPassId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(currentPassId);
    }

    CurrentPassId = currentPassId;
  }
}
