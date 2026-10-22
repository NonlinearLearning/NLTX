using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Modules.Banners;

public sealed class BannerSnapshot
{
  public const int BannerCount = 293;

  public BannerSnapshot(
    MinimalImplementation.Contracts.CommitVersion version,
    IReadOnlyList<int> killCounts,
    IReadOnlyList<ushort> claimableCounts,
    bool anyNewClaimableBanners,
    MinimalImplementation.Contracts.CommitVersion lastP05CommitVersion = default)
  {
    if (!version.IsValid)
    {
      throw new ArgumentException("A banner snapshot requires a valid commit version.", nameof(version));
    }

    if (lastP05CommitVersion.Value < 0)
    {
      throw new ArgumentException(
        "A banner snapshot cannot contain a negative P05 source version.",
        nameof(lastP05CommitVersion));
    }

    ArgumentNullException.ThrowIfNull(killCounts);
    ArgumentNullException.ThrowIfNull(claimableCounts);
    if (killCounts.Count != BannerCount || claimableCounts.Count != BannerCount)
    {
      throw new ArgumentException($"A banner snapshot must contain exactly {BannerCount} entries in both tables.");
    }

    Version = version;
    LastP05CommitVersion = lastP05CommitVersion;
    KillCounts = Array.AsReadOnly(killCounts.ToArray());
    ClaimableCounts = Array.AsReadOnly(claimableCounts.ToArray());
    AnyNewClaimableBanners = anyNewClaimableBanners;
  }

  public MinimalImplementation.Contracts.CommitVersion Version { get; }
  /// <summary>
  /// The last P05 source revision accepted by this snapshot. Value zero means that the source
  /// revision was not available in the persistence representation.
  /// </summary>
  public MinimalImplementation.Contracts.CommitVersion LastP05CommitVersion { get; }
  public IReadOnlyList<int> KillCounts { get; }
  public IReadOnlyList<ushort> ClaimableCounts { get; }
  public bool AnyNewClaimableBanners { get; }

  public WorldFileBannerSection ToPersistenceSection() =>
    new(KillCounts, ClaimableCounts);
}
