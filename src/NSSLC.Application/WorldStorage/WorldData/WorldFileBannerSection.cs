using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Persisted banner kill and claim counts from the WorldFile header continuation.
/// </summary>
/// <remarks>
/// The lists are immutable snapshots. The DTO does not apply BannerSystem limits or update a
/// runtime banner registry; an eventual owner API is responsible for those rules.
/// </remarks>
public sealed class WorldFileBannerSection
{
  public const string SectionId = "world.banners";

  public WorldFileBannerSection(
    IReadOnlyList<int> killCounts,
    IReadOnlyList<ushort> claimableCounts)
  {
    KillCounts = Copy(killCounts, nameof(killCounts));
    ClaimableCounts = Copy(claimableCounts, nameof(claimableCounts));
  }

  public IReadOnlyList<int> KillCounts { get; }

  public IReadOnlyList<ushort> ClaimableCounts { get; }

  public static WorldFileBannerSection Empty => new(
    Array.Empty<int>(),
    Array.Empty<ushort>());

  private static IReadOnlyList<T> Copy<T>(
    IReadOnlyList<T> values,
    string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values, parameterName);
    return Array.AsReadOnly(new List<T>(values).ToArray());
  }
}
