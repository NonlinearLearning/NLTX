using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成整体阶段、失败原因和生成版本。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 WorldGen 的世界生成、加载和就绪切换流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>重组说明：GenerationId、Phase、Failure 和 GenerationRevision 是生成生命周期拆分时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-world-generation-and-ecology-component-code-draft.md。
/// </para>
/// <para>依据位置：第 418 行。</para>
/// </remarks>
public readonly record struct WorldGenerationLifecycleComponent
{
  public WorldGenerationLifecycleComponent(
    long generationId,
    WorldPreparationState phase = WorldPreparationState.Uninitialized,
    WorldGenerationFailure failure = WorldGenerationFailure.None,
    ulong generationRevision = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (!Enum.IsDefined(phase))
    {
      throw new ArgumentOutOfRangeException(nameof(phase));
    }

    if (!Enum.IsDefined(failure))
    {
      throw new ArgumentOutOfRangeException(nameof(failure));
    }

    if (phase == WorldPreparationState.Failed &&
        failure == WorldGenerationFailure.None)
    {
      throw new ArgumentException(
        "A failed lifecycle must carry a failure category.",
        nameof(failure));
    }

    GenerationId = generationId;
    Phase = phase;
    Failure = failure;
    GenerationRevision = generationRevision;
  }

  public long GenerationId { get; }

  public WorldPreparationState Phase { get; }

  public WorldGenerationFailure Failure { get; }

  public ulong GenerationRevision { get; }

  public bool IsLoadingOrGenerating =>
    Phase == WorldPreparationState.Loading ||
    Phase == WorldPreparationState.Generating;

  public bool IsPhaseReady =>
    Phase == WorldPreparationState.Ready &&
    Failure == WorldGenerationFailure.None;

  public bool IsReady => IsPhaseReady;

  public bool HasFailed =>
    Phase == WorldPreparationState.Failed ||
    Failure != WorldGenerationFailure.None;
}
