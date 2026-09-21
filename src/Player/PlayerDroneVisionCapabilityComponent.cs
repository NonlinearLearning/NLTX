namespace Terraria.Player;

public sealed class PlayerDroneVisionCapabilityComponent
{
  public bool RemoteVisionForDrone { get; internal set; }

  internal void ResetEffects()
  {
    RemoteVisionForDrone = false;
  }
}
