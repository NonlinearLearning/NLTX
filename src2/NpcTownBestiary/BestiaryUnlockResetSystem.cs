namespace Terraria.NpcTownBestiary;

public sealed class BestiaryUnlockResetSystem
{
  public void Reset(
    BestiaryKillCountStateComponent kills,
    BestiarySightDiscoveryStateComponent sights,
    BestiaryChatDiscoveryStateComponent chats,
    BestiarySightScanBuffer? scanBuffer = null)
  {
    ArgumentNullException.ThrowIfNull(kills);
    ArgumentNullException.ThrowIfNull(sights);
    ArgumentNullException.ThrowIfNull(chats);
    kills.Clear();
    sights.Clear();
    chats.Clear();
    scanBuffer?.Reset();
  }
}
