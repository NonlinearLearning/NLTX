using System.Numerics;

namespace Terraria.Physics;

public struct MovementIntentComponent
{
  public Vector2 DesiredDirection;
  public long? IssuedAtTick;
  public MovementIntentSource Source;
  public uint Sequence;
  public bool WantsDropThroughPlatforms;
  public bool WantsJump;

  public bool HasDirectionalInput => DesiredDirection != Vector2.Zero;

}
