namespace Terraria.NpcTownBestiary;

public sealed class BestiarySightScanSystem
{
  public BestiarySightScanResult Scan(
    BestiarySightScanBuffer buffer,
    IEnumerable<BestiaryPlayerBounds> playerBounds,
    IEnumerable<BestiarySightCandidate> candidates,
    BestiarySightDiscoveryStateComponent sights,
    BestiaryDiscoverySystem discovery)
  {
    ArgumentNullException.ThrowIfNull(buffer);
    ArgumentNullException.ThrowIfNull(playerBounds);
    ArgumentNullException.ThrowIfNull(candidates);
    ArgumentNullException.ThrowIfNull(sights);
    ArgumentNullException.ThrowIfNull(discovery);

    BestiaryPlayerBounds[] bounds = playerBounds.ToArray();
    buffer.BeginScan(bounds);
    int discoveredCount = 0;
    foreach (BestiarySightCandidate candidate in candidates)
    {
      if (buffer.SeenNpcNetIds.Contains(candidate.NpcNetId) ||
        !bounds.Any(candidate.Bounds.Intersects))
      {
        continue;
      }

      if (buffer.TryMarkSeen(candidate.NpcNetId))
      {
        BestiaryDiscoveryMutationResult result = discovery.RegisterSight(
          sights,
          candidate.CreditId);
        if (result.Changed)
        {
          discoveredCount++;
        }
      }
    }

    return new BestiarySightScanResult(discoveredCount);
  }
}
