using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界准备或生成会话的阶段、失败类别和版本。
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
/// <para>
/// 用途：加载、生成和准备流程提交生命周期结果，调用方读取是否正在加载或生成、是否就绪以及是否失败。
/// </para>
/// <para>
/// 当前实现：构造时校验会话编号和枚举，Failed 阶段必须携带失败类别。IsReady 要求阶段为 Ready 且 Failure 为 None；HasFailed 在
/// 失败阶段或携带失败类别时为 true。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C191，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
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

  /// <summary>
  /// 世界生成会话编号，构造时要求非负。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 世界准备或生成生命周期阶段。
  /// </summary>
  public WorldPreparationState Phase { get; }

  /// <summary>
  /// 生命周期失败类别，无失败时为 None。
  /// </summary>
  public WorldGenerationFailure Failure { get; }

  /// <summary>
  /// 生命周期结果版本。
  /// </summary>
  public ulong GenerationRevision { get; }

  /// <summary>
  /// 阶段为 Loading 或 Generating 时返回 true。
  /// </summary>
  public bool IsLoadingOrGenerating =>
    Phase == WorldPreparationState.Loading ||
    Phase == WorldPreparationState.Generating;

  /// <summary>
  /// 阶段为 Ready 且无失败类别时返回 true。
  /// </summary>
  public bool IsPhaseReady =>
    Phase == WorldPreparationState.Ready &&
    Failure == WorldGenerationFailure.None;

  /// <summary>
  /// IsPhaseReady 的同义读取入口。
  /// </summary>
  public bool IsReady => IsPhaseReady;

  /// <summary>
  /// 阶段为 Failed 或失败类别不为 None 时返回 true。
  /// </summary>
  public bool HasFailed =>
    Phase == WorldPreparationState.Failed ||
    Failure != WorldGenerationFailure.None;
}
