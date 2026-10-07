using System.Collections.ObjectModel;

namespace Terraria.WorldSession.Components;

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
