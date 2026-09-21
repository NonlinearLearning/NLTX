using System;

namespace Terraria.WorldSession.Events.Banners;

public sealed class BannerClaimableCountStateComponent
{
  public const int BannerCapacity = 293;

  private readonly ushort[] _counts;

  public BannerClaimableCountStateComponent(IReadOnlyList<ushort>? counts = null)
  {
    _counts = new ushort[BannerCapacity];
    if (counts is not null)
    {
      if (counts.Count != BannerCapacity)
      {
        throw new ArgumentException(
          $"Banner claimable counts must contain exactly {BannerCapacity} entries.",
          nameof(counts));
      }

      for (int index = 0; index < _counts.Length; index++)
      {
        _counts[index] = counts[index];
      }
    }
  }

  public IReadOnlyList<ushort> Counts => Array.AsReadOnly(_counts);
}
