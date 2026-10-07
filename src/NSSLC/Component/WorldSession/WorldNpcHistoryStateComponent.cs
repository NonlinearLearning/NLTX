using System.Collections.ObjectModel;

namespace Terraria.WorldSession.Components;

/// <summary>
/// 保存世界钓鱼任务、旗帜、图鉴和城镇微光交互记录。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>主要源成员：anglerWhoFinishedToday（第 998 行）； anglerQuest（第 1002 行）。</para>
/// <para>拆分依据目录：docs/system-decomposition/reports/。</para>
/// <para>
/// 拆分依据文件：2026-09-18-system-decomposition-authoritative-P16-world-lifecycle-housing-metrics.md。
/// </para>
/// <para>依据位置：第 307 行。</para>
/// </remarks>
public sealed class WorldNpcHistoryStateComponent {
  public IReadOnlyList<string> AnglerWhoFinishedToday { get; internal set; } =
      Array.Empty<string>();
  public int AnglerQuest { get; internal set; }
  public IReadOnlyList<int> BannerKillCounts { get; internal set; } = Array.Empty<int>();
  public IReadOnlyList<ushort> BannerClaimableCounts { get; internal set; } = Array.Empty<ushort>();
  public IReadOnlyDictionary<string, int> BestiaryKillCounts { get; internal set; } =
      new ReadOnlyDictionary<string, int>(new Dictionary<string, int>());
  public IReadOnlyList<string> SeenNpcIds { get; internal set; } = Array.Empty<string>();
  public IReadOnlyList<string> ChattedNpcIds { get; internal set; } = Array.Empty<string>();
  public IReadOnlyList<int> ShimmeredTownNpcIds { get; internal set; } = Array.Empty<int>();
}
