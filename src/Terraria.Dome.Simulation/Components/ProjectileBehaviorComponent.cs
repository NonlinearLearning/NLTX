namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileBehaviorState(
  float Primary,
  float Secondary,
  int Phase,
  int TargetId,
  float Tertiary = 0.0f,
  float LocalAi0 = 0.0f,
  float LocalAi1 = 0.0f,
  float LocalAi2 = 0.0f);

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
