using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores pause intent and the stable pass identity at which a run may pause.
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
/// </remarks>
public sealed class WorldGenerationControlStateComponent
{
  public WorldGenerationControlStateComponent(
    bool paused = false,
    string? pauseAfterPassId = null)
  {
    ReplaceState(paused, pauseAfterPassId);
  }

  public bool Paused { get; private set; }

  public string? PauseAfterPassId { get; private set; }

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
