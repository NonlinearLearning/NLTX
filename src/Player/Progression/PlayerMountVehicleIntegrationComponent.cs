using System.Numerics;

namespace Terraria.Player.Progression;

public sealed class PlayerMountVehicleIntegrationComponent
{
  public bool OnWrongGround { get; internal set; }

  public bool OnTrack { get; internal set; }

  public int CartRampTime { get; internal set; }

  public bool CartFlip { get; internal set; }

  public float TrackBoost { get; internal set; }

  public Vector2 LastBoost { get; internal set; }
}
