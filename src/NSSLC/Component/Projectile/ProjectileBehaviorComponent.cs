namespace Terraria.Projectile;

public struct ProjectileBehaviorComponent
{
  public ProjectileBehaviorComponent(
    int style,
    float state0,
    float state1,
    float state2,
    float state3,
    float localState0,
    float localState1)
  {
    Style = style;
    State0 = state0;
    State1 = state1;
    State2 = state2;
    State3 = state3;
    LocalState0 = localState0;
    LocalState1 = localState1;
  }

  public int Style;
  public float State0;
  public float State1;
  public float State2;
  public float State3;
  public float LocalState0;
  public float LocalState1;
}
