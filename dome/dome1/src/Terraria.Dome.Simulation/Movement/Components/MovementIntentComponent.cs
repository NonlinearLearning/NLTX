namespace Terraria.Dome.Simulation.Movement.Components;

public struct MovementIntentComponent
{
  public bool FireRequested;
  public bool JumpRequested;
  public int HorizontalDirection;
  public bool UseItemRequested;
  public bool HasNpcIntent;
  public float NpcHorizontalVelocity;
  public float NpcVerticalVelocity;
  public bool WantsDropThroughPlatforms;
  public MovementIntentSource Source;
  public uint Sequence;
  public long? IssuedAtTick;

  public bool HasDirectionalInput => HorizontalDirection != 0 ||
    NpcHorizontalVelocity != 0.0f || NpcVerticalVelocity != 0.0f;
}
