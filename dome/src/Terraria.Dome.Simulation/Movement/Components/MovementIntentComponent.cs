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
}
