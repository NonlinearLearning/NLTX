namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerVoidVaultStateComponent
{
  public byte FeatureFlags;
  public bool IsEnabled => (FeatureFlags & 1) != 0;

  public void SetEnabled(bool enabled)
  {
    FeatureFlags = enabled ? (byte)(FeatureFlags | 1) : (byte)(FeatureFlags & ~1);
  }
}
