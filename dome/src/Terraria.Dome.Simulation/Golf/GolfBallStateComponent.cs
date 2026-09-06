using System.Numerics;
using Arch.Core;

namespace Terraria.Dome.Simulation.Golf;

public struct GolfBallStateComponent
{
  public Entity? Owner;
  public int SwingCount;
  public GolfBallMotionState MotionState;
  public long? LastHitTick;
  public Vector2? LastHitLocation;
  public int PhysicsProfileId;

  public bool IsSettled => MotionState == GolfBallMotionState.AtRest;
}
