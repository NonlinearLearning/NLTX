using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the stable identity of the pass currently being executed.
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
/// </remarks>
public sealed class WorldGenerationExecutionCursorComponent
{
  public WorldGenerationExecutionCursorComponent(string? currentPassId = null)
  {
    ReplaceState(currentPassId);
  }

  public string? CurrentPassId { get; private set; }

  public bool HasCurrentPass => CurrentPassId is not null;

  internal void ReplaceState(string? currentPassId)
  {
    if (currentPassId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(currentPassId);
    }

    CurrentPassId = currentPassId;
  }
}
