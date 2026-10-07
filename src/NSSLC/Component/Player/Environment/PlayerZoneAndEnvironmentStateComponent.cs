namespace Terraria.Player.Environment;

// status: implemented
// componentId: PLAYER.COMP.ZONE_AND_ENVIRONMENT_STATE
// source-members: P08-662, P08-665..P08-670
// crossSubsystemOwner: integration-review
public sealed class PlayerZoneAndEnvironmentStateComponent
{
  public int EnvironmentBuffImmunityTimer { get; internal set; }

  // The legacy BitsByte values are kept as bytes until the protocol type is available in NLTX.
  public byte Zone1 { get; internal set; }

  public byte Zone2 { get; internal set; }

  public byte Zone3 { get; internal set; }

  public byte Zone4 { get; internal set; }

  public byte Zone5 { get; internal set; }

  public bool WasInShimmerZone { get; internal set; }

  public void ApplyNetworkZones(byte zone1, byte zone2, byte zone3, byte zone4, byte zone5)
  {
    Zone1 = zone1;
    Zone2 = zone2;
    Zone3 = zone3;
    Zone4 = zone4;
    Zone5 = zone5;
  }

}
