namespace Terraria.WorldStorage;

public sealed class PylonRegistryState
{
  public List<PylonRegistryEntry> CurrentPylons = new();
  public List<PylonRegistryEntry> PreviousPylons = new();
  public int Count => CurrentPylons.Count;
  public bool HasPendingRefresh => RefreshCooldownTicksRemaining == 0;
  public int RefreshCooldownTicksRemaining;
  public uint Revision;
}
