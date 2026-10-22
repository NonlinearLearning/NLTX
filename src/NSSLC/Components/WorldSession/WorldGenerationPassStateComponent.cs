using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成步骤的执行阶段、游标、快照版本和控制请求。
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
/// <para>
/// 用途：执行器记录当前步骤及处理位置，暂停或恢复流程读取游标和检查点版本，控制流程登记暂停与终止请求。
/// </para>
/// <para>
/// 当前实现：构造时校验会话、阶段和非负计数；无活动步骤时版本必须为 0，有活动步骤时版本必须为正数。之后字段可直接修改，执行和控制请求消费由外部流程负责。
/// </para>
/// <para>
/// 读取含义：HasActivePass 只检查步骤编号非空，HasFailure 只检查失败原因包含非空白文本。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C194，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
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

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId;

  /// <summary>
  /// 生成步骤当前执行阶段。
  /// </summary>
  public WorldGenerationStage Stage;

  /// <summary>
  /// 活动生成步骤编号，无活动步骤时为空。
  /// </summary>
  public string? ActivePassId;

  /// <summary>
  /// 活动步骤的实现版本；无活动步骤时为 0。
  /// </summary>
  public int PassVersion;

  /// <summary>
  /// 活动步骤内部的执行游标。
  /// </summary>
  public long Cursor;

  /// <summary>
  /// 此步骤读取的世界快照版本，可为空。
  /// </summary>
  public ulong? ReadSnapshotRevision;

  /// <summary>
  /// 此步骤最近检查点的版本，可为空。
  /// </summary>
  public ulong? CheckpointRevision;

  /// <summary>
  /// 步骤执行失败原因，可为空。
  /// </summary>
  public string? FailureReason;

  /// <summary>
  /// 是否登记了暂停请求。
  /// </summary>
  public bool PauseRequested;

  /// <summary>
  /// 是否登记了终止请求。
  /// </summary>
  public bool AbortRequested;

  /// <summary>
  /// 活动步骤编号非空时返回 true。
  /// </summary>
  public bool HasActivePass => ActivePassId is not null;

  /// <summary>
  /// 失败原因包含非空白文本时返回 true。
  /// </summary>
  public bool HasFailure => !string.IsNullOrWhiteSpace(FailureReason);
}
