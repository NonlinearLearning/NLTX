using System;

namespace Terraria.WorldSession.Events.Banners;

public sealed class BannerKillProgressStateComponent
{
  public const int BannerCapacity = 293;

  private readonly int[] _killCounts;

  public BannerKillProgressStateComponent(IReadOnlyList<int>? killCounts = null)
  {
    _killCounts = new int[BannerCapacity];
    if (killCounts is not null)
    {
      if (killCounts.Count != BannerCapacity)
      {
        throw new ArgumentException(
          $"Banner kill progress must contain exactly {BannerCapacity} entries.",
          nameof(killCounts));
      }

      for (int index = 0; index < _killCounts.Length; index++)
      {
        _killCounts[index] = killCounts[index];
      }
    }

    Validate();
  }

  public IReadOnlyList<int> KillCounts => Array.AsReadOnly(_killCounts);

  public void Validate()
  {
    for (int index = 0; index < _killCounts.Length; index++)
    {
      if (_killCounts[index] < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(KillCounts));
      }
    }
  }
}
