using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成随机流的状态值、算法版本、所属步骤和游标。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：genRand（第 4370 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-execution.md。
/// </para>
/// <para>依据位置：第 375 行。</para>
/// <para>
/// 用途：生成流程在继续执行或恢复时读取随机状态，结合流版本和游标重建对应随机取样进度。
/// </para>
/// <para>
/// 当前实现：只保存状态，构造时要求会话与游标非负、流版本为正数，非空步骤编号不能为空白。随机数生成和状态推进由外部随机流程完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C196，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
/// </remarks>
public readonly record struct WorldGenerationRandomStateComponent
{
  public WorldGenerationRandomStateComponent(
    long generationId,
    uint state,
    int streamVersion,
    string? passId = null,
    long cursor = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (streamVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(streamVersion));
    }

    if (cursor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursor));
    }

    if (passId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    }

    GenerationId = generationId;
    State = state;
    StreamVersion = streamVersion;
    PassId = passId;
    Cursor = cursor;
  }

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 随机流当前保存的 uint 状态值。
  /// </summary>
  public uint State { get; }

  /// <summary>
  /// 随机流算法或状态格式的正整数版本。
  /// </summary>
  public int StreamVersion { get; }

  /// <summary>
  /// 此随机流关联的生成步骤编号，可为空。
  /// </summary>
  public string? PassId { get; }

  /// <summary>
  /// 此随机流的进度游标，具体推进含义由随机流程定义。
  /// </summary>
  public long Cursor { get; }
}
