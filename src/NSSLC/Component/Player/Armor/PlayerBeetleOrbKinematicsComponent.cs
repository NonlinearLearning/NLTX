using System.Numerics;

namespace Terraria.Player.Armor;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for random sampling and presentation output
public sealed class PlayerBeetleOrbKinematicsComponent
{
  public const int OrbCapacity = 3;

  public Vector2[] Positions { get; } = new Vector2[OrbCapacity];

  public Vector2[] Velocities { get; } = new Vector2[OrbCapacity];
}
