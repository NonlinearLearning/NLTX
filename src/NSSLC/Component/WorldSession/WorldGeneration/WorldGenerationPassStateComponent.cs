using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成步骤的阶段、游标、检查点和暂停中止状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 WorldGenerator 的生成步骤执行循环与控制器流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerator.cs。</para>
/// <para>重组说明：分步游标、读取快照、检查点版本和失败状态是显式调度新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-design.md。
/// </para>
/// <para>依据位置：第 185 行。</para>
/// </remarks>
public struct WorldGenerationPassStateComponent
{
  public WorldGenerationPassStateComponent(
    long generationId,
    WorldGenerationStage stage = WorldGenerationStage.Created,
    string? activePassId = null,
    int passVersion = 0,
    long cursor = 0,
    ulong? readSnapshotRevision = null,
    ulong? checkpointRevision = null,
    string? failureReason = null,
    bool pauseRequested = false,
    bool abortRequested = false)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (!Enum.IsDefined(stage))
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }

    if (cursor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursor));
    }

    if (passVersion < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(passVersion));
    }

    if (activePassId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(activePassId);
    }

    if (activePassId is null && passVersion != 0)
    {
      throw new ArgumentException(
        "An inactive pass cannot carry a pass version.",
        nameof(passVersion));
    }

    if (activePassId is not null && passVersion <= 0)
    {
      throw new ArgumentException(
        "An active pass must carry a positive pass version.",
        nameof(passVersion));
    }

    GenerationId = generationId;
    Stage = stage;
    ActivePassId = activePassId;
    PassVersion = passVersion;
    Cursor = cursor;
    ReadSnapshotRevision = readSnapshotRevision;
    CheckpointRevision = checkpointRevision;
    FailureReason = failureReason;
    PauseRequested = pauseRequested;
    AbortRequested = abortRequested;
  }

  public long GenerationId;

  public WorldGenerationStage Stage;

  public string? ActivePassId;

  public int PassVersion;

  public long Cursor;

  public ulong? ReadSnapshotRevision;

  public ulong? CheckpointRevision;

  public string? FailureReason;

  public bool PauseRequested;

  public bool AbortRequested;

  public bool HasActivePass => ActivePassId is not null;

  public bool HasFailure => !string.IsNullOrWhiteSpace(FailureReason);
}
