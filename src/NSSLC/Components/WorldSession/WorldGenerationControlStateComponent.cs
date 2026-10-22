using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成暂停状态及完成指定步骤后暂停的请求。
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成暂停和指定步骤后暂停的控制状态。</para>
/// <para>拆分来源：Terraria.WorldBuilding.WorldGenerator.Controller。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerator.cs。</para>
/// <para>主要源成员：Paused（第 60 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 361 行。</para>
/// <para>
/// 用途：生成调度流程读取 Paused 决定是否继续推进，读取 PauseAfterPassId 决定在某一步完成后进入暂停。
/// </para>
/// <para>
/// 状态变化：ReplaceState 要求非空步骤编号不能为空白；提交已暂停状态时会清除 PauseAfterPassId。具体暂停时机和继续执行由外部生成流程控制。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C187，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
/// </remarks>
public sealed class WorldGenerationControlStateComponent
{
  public WorldGenerationControlStateComponent(
    bool paused = false,
    string? pauseAfterPassId = null)
  {
    ReplaceState(paused, pauseAfterPassId);
  }

  /// <summary>
  /// 世界生成当前是否处于暂停状态。
  /// </summary>
  public bool Paused { get; private set; }

  /// <summary>
  /// 请求完成后暂停的生成步骤编号；已暂停时清为空。
  /// </summary>
  public string? PauseAfterPassId { get; private set; }

  /// <summary>
  /// 提交暂停状态和目标步骤，已暂停时清除后续暂停目标。
  /// </summary>
  internal void ReplaceState(bool paused, string? pauseAfterPassId)
  {
    if (pauseAfterPassId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(pauseAfterPassId);
    }

    Paused = paused;
    PauseAfterPassId = paused ? null : pauseAfterPassId;
  }
}
