using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存世界事件使用的独立随机流和消费位置。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>主要源成员：rand（第 673 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-calendar-and-event-orchestration-component-code-draft.md。
/// </para>
/// <para>依据位置：第 823 行。</para>
/// </remarks>
public sealed class WorldEventRandomStateComponent
{
  public WorldEventRandomStateComponent(
    uint state,
    ulong worldSeed,
    int streamVersion,
    long consumedDrawCount = 0)
  {
    State = state;
    WorldSeed = worldSeed;
    StreamVersion = streamVersion;
    ConsumedDrawCount = consumedDrawCount;
    Validate();
  }

  public uint State;
  public ulong WorldSeed;
  public int StreamVersion;
  public long ConsumedDrawCount;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(StreamVersion);
    ArgumentOutOfRangeException.ThrowIfNegative(ConsumedDrawCount);
  }
}
