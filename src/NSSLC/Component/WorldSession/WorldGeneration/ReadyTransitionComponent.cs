using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界进入就绪状态所需的生成、液体、住房和保存完成条件。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 WorldGen 在生成、液体稳定、住房和保存完成后发布世界的流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>重组说明：各完成门槛、发布版本和失败原因是就绪切换模型新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-world-generation-and-ecology-component-design.md。</para>
/// <para>依据位置：第 706 行。</para>
/// </remarks>
public readonly record struct ReadyTransitionComponent
{
  public ReadyTransitionComponent(
    long generationId,
    ulong generationRevision,
    bool passesValidated = false,
    bool liquidStable = false,
    bool housingComplete = false,
    bool persistenceCommitted = false,
    ulong? publicationRevision = null,
    string? failureReason = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    GenerationRevision = generationRevision;
    PassesValidated = passesValidated;
    LiquidStable = liquidStable;
    HousingComplete = housingComplete;
    PersistenceCommitted = persistenceCommitted;
    PublicationRevision = publicationRevision;
    FailureReason = failureReason;
  }

  public long GenerationId { get; }

  public ulong GenerationRevision { get; }

  public bool PassesValidated { get; }

  public bool LiquidStable { get; }

  public bool HousingComplete { get; }

  public bool PersistenceCommitted { get; }

  public ulong? PublicationRevision { get; }

  public string? FailureReason { get; }

  public bool IsReadyCandidate =>
    FailureReason is null &&
    PassesValidated &&
    LiquidStable &&
    HousingComplete &&
    PersistenceCommitted &&
    PublicationRevision.HasValue;
}
