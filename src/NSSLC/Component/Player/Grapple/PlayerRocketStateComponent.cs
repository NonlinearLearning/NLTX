namespace Terraria.Player.Grapple;

// status: implemented-isolated-core
// crossSubsystemOwner: Equipment, Jump, Flight, Carpet, Spatial, effects, and persistence
public sealed class PlayerRocketStateComponent
{
  public int RocketTime { get; set; }

  public int RocketTimeMax { get; set; } = 7;

  public int RocketDelay { get; set; }

  public int RocketEffectDelay { get; set; }

  public bool RocketRelease { get; set; }

  public bool CanRocket { get; set; }
}
