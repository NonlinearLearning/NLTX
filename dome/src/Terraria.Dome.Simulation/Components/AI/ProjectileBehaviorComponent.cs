namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileBehaviorState(
  float Primary,
  float Secondary,
  int Phase,
  int TargetId,
  float Tertiary = 0.0f,
  float LocalAi0 = 0.0f,
  float LocalAi1 = 0.0f,
  float LocalAi2 = 0.0f)
{
  public const int MaxAi = 3;

  public float Ai0 => Primary;

  public float Ai1 => Secondary;

  public float Ai2 => Tertiary;
}

public struct ProjectileBehaviorComponent
{
  public ProjectileBehaviorComponent(int behaviorId, ProjectileBehaviorState state)
  {
    BehaviorId = behaviorId;
    State = state;
  }

  public int BehaviorId { get; }
  public ProjectileBehaviorState State { get; set; }

  public float Ai0
  {
    get => State.Ai0;
    set => State = State with { Primary = value };
  }

  public float Ai1
  {
    get => State.Ai1;
    set => State = State with { Secondary = value };
  }

  public float Ai2
  {
    get => State.Ai2;
    set => State = State with { Tertiary = value };
  }

  public float LocalAi0
  {
    get => State.LocalAi0;
    set => State = State with { LocalAi0 = value };
  }

  public float LocalAi1
  {
    get => State.LocalAi1;
    set => State = State with { LocalAi1 = value };
  }

  public float LocalAi2
  {
    get => State.LocalAi2;
    set => State = State with { LocalAi2 = value };
  }
}
