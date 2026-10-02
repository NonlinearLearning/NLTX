using System.Numerics;

namespace Terraria.Player.Armor;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Spatial and presentation output
public sealed class PlayerSolarShieldKinematicsComponent
{
  public const int ShieldCapacity = 3;

  public Vector2[] Positions { get; } = new Vector2[ShieldCapacity];

  public Vector2[] Velocities { get; } = new Vector2[ShieldCapacity];
}
