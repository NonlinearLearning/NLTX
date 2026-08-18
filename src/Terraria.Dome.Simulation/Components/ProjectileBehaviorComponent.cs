namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileBehaviorState(
  float Primary,
  float Secondary,
  int Phase,
  int TargetId);

public struct ProjectileBehaviorComponent
{
  public ProjectileBehaviorComponent(int behaviorId, ProjectileBehaviorState state)
  {
    BehaviorId = behaviorId;
    State = state;
  }

  public int BehaviorId { get; }
  public ProjectileBehaviorState State { get; set; }
}
